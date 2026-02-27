using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.Holder;
using SdJwt.Net.Issuer;
using SdJwt.Net.Verifier;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;

namespace SdJwt.Net.Benchmarks;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net90)]
public class CoreSdJwtBenchmarks
{
    private ECDsa? _issuerEcdsa;
    private ECDsaSecurityKey? _issuerKey;
    private SdIssuer? _issuer;
    private JwtPayload? _claims;
    private SdIssuanceOptions? _issuanceOptions;
    private SdJwtHolder? _holder;
    private string? _presentation;
    private SdVerifier? _verifier;
    private TokenValidationParameters? _validationParameters;

    [GlobalSetup]
    public void GlobalSetup()
    {
        _issuerEcdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        _issuerKey = new ECDsaSecurityKey(_issuerEcdsa) { KeyId = "bench-issuer-key" };
        _issuer = new SdIssuer(_issuerKey, SecurityAlgorithms.EcdsaSha256);

        _claims = new JwtPayload
        {
            { "iss", "https://issuer.example.com" },
            { "sub", "did:example:holder-123" },
            { "name", "Alice Doe" },
            { "email", "alice@example.com" },
            { "given_name", "Alice" },
            { "family_name", "Doe" },
            { "birthdate", "1998-05-12" },
            { "address", new Dictionary<string, object> { ["street"] = "123 Main St", ["city"] = "New York", ["country"] = "US" } }
        };

        _issuanceOptions = new SdIssuanceOptions
        {
            DisclosureStructure = new
            {
                email = true,
                birthdate = true,
                address = new { street = true, city = true }
            }
        };

        var issuance = _issuer.Issue(_claims, _issuanceOptions).Issuance;
        _holder = new SdJwtHolder(issuance);
        _presentation = _holder.CreatePresentation(d => d.ClaimName is "email" or "birthdate");

        _verifier = new SdVerifier(jwt => Task.FromResult<SecurityKey>(_issuerKey));
        _validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = false
        };
    }

    [GlobalCleanup]
    public void GlobalCleanup()
    {
        _issuerEcdsa?.Dispose();
    }

    [Benchmark]
    public string IssueCredential()
    {
        return _issuer!.Issue(_claims!, _issuanceOptions!).Issuance;
    }

    [Benchmark]
    public string CreatePresentation()
    {
        return _holder!.CreatePresentation(d => d.ClaimName is "email" or "birthdate");
    }

    [Benchmark]
    public Task<VerificationResult> VerifyPresentation()
    {
        return _verifier!.VerifyAsync(_presentation!, _validationParameters!);
    }
}
