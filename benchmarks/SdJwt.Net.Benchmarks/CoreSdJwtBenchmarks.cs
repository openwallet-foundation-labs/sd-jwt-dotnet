using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.Holder;
using SdJwt.Net.Issuer;
using SdJwt.Net.Verifier;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;

namespace SdJwt.Net.Benchmarks;

/// <summary>
/// Core SD-JWT performance benchmarks measuring issuance, presentation, and verification.
/// These operations are the critical path for credential workflows.
/// </summary>
[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net90)]
public class CoreSdJwtBenchmarks
{
    private ECDsa? _issuerEcdsa;
    private ECDsaSecurityKey? _issuerKey;
    private SdIssuer? _issuer;
    private JwtPayload? _claims;
    private JwtPayload? _largeClaims;
    private SdIssuanceOptions? _issuanceOptions;
    private SdIssuanceOptions? _largeIssuanceOptions;
    private SdJwtHolder? _holder;
    private SdJwtHolder? _largeHolder;
    private string? _presentation;
    private SdVerifier? _verifier;
    private TokenValidationParameters? _validationParameters;

    [GlobalSetup]
    public void GlobalSetup()
    {
        _issuerEcdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        _issuerKey = new ECDsaSecurityKey(_issuerEcdsa) { KeyId = "bench-issuer-key" };
        _issuer = new SdIssuer(_issuerKey, SecurityAlgorithms.EcdsaSha256);

        // Standard payload (8 claims)
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

        // Large payload (20+ claims for stress testing)
        _largeClaims = new JwtPayload
        {
            { "iss", "https://issuer.example.com" },
            { "sub", "did:example:holder-123" },
            { "name", "Alice Doe" },
            { "email", "alice@example.com" },
            { "given_name", "Alice" },
            { "family_name", "Doe" },
            { "birthdate", "1998-05-12" },
            { "phone_number", "+1-555-123-4567" },
            { "nationality", "US" },
            { "tax_id", "123-45-6789" },
            { "employer", "Contoso Corp" },
            { "job_title", "Software Engineer" },
            { "department", "Engineering" },
            { "hire_date", "2020-01-15" },
            { "salary_band", "L5" },
            { "address", new Dictionary<string, object>
                {
                    ["street"] = "123 Main St",
                    ["city"] = "New York",
                    ["state"] = "NY",
                    ["postal_code"] = "10001",
                    ["country"] = "US"
                }
            },
            { "emergency_contact", new Dictionary<string, object>
                {
                    ["name"] = "Bob Doe",
                    ["relationship"] = "Spouse",
                    ["phone"] = "+1-555-987-6543"
                }
            }
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

        _largeIssuanceOptions = new SdIssuanceOptions
        {
            DisclosureStructure = new
            {
                email = true,
                birthdate = true,
                phone_number = true,
                nationality = true,
                tax_id = true,
                employer = true,
                job_title = true,
                salary_band = true,
                address = new { street = true, city = true, state = true, postal_code = true },
                emergency_contact = new { name = true, phone = true }
            }
        };

        var issuance = _issuer.Issue(_claims, _issuanceOptions).Issuance;
        _holder = new SdJwtHolder(issuance);
        _presentation = _holder.CreatePresentation(d => d.ClaimName is "email" or "birthdate");

        var largeIssuance = _issuer.Issue(_largeClaims, _largeIssuanceOptions).Issuance;
        _largeHolder = new SdJwtHolder(largeIssuance);

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

    /// <summary>
    /// Measures credential issuance with standard payload (~8 claims, 4 selective).
    /// Target: less than 500 microseconds.
    /// </summary>
    [Benchmark(Description = "Issue (8 claims, 4 SD)")]
    public string IssueCredential()
    {
        return _issuer!.Issue(_claims!, _issuanceOptions!).Issuance;
    }

    /// <summary>
    /// Measures credential issuance with large payload (~20 claims, 12 selective).
    /// Stress test for complex credentials like employment or medical records.
    /// </summary>
    [Benchmark(Description = "Issue (20 claims, 12 SD)")]
    public string IssueLargeCredential()
    {
        return _issuer!.Issue(_largeClaims!, _largeIssuanceOptions!).Issuance;
    }

    /// <summary>
    /// Measures presentation creation with selective disclosure.
    /// Target: less than 100 microseconds (no crypto, just string manipulation).
    /// </summary>
    [Benchmark(Description = "Present (2 disclosures)")]
    public string CreatePresentation()
    {
        return _holder!.CreatePresentation(d => d.ClaimName is "email" or "birthdate");
    }

    /// <summary>
    /// Measures presentation creation with many disclosures.
    /// </summary>
    [Benchmark(Description = "Present (8 disclosures)")]
    public string CreateLargePresentation()
    {
        return _largeHolder!.CreatePresentation(d =>
            d.ClaimName is "email" or "birthdate" or "phone_number" or "employer"
                or "job_title" or "salary_band" or "nationality" or "tax_id");
    }

    /// <summary>
    /// Measures presentation verification including signature and disclosure validation.
    /// Target: less than 300 microseconds.
    /// </summary>
    [Benchmark(Description = "Verify presentation")]
    public Task<VerificationResult> VerifyPresentation()
    {
        return _verifier!.VerifyAsync(_presentation!, _validationParameters!);
    }
}
