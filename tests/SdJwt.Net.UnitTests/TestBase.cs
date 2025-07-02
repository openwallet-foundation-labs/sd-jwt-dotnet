using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

namespace SdJwt.Net.Tests;

public abstract class TestBase
{
    protected readonly SecurityKey IssuerSigningKey;
    protected readonly SecurityKey HolderPrivateKey;
    protected readonly SecurityKey HolderPublicKey;
    protected readonly JsonWebKey HolderPublicJwk;
    protected const string IssuerSigningAlgorithm = SecurityAlgorithms.EcdsaSha256;
    protected const string HolderSigningAlgorithm = SecurityAlgorithms.EcdsaSha256;
    protected const string TrustedIssuer = "https://issuer.example.com";

    protected TestBase()
    {
        var issuerEcdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        IssuerSigningKey = new ECDsaSecurityKey(issuerEcdsa) { KeyId = "issuer-key-1" };

        var holderEcdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        HolderPrivateKey = new ECDsaSecurityKey(holderEcdsa) { KeyId = "holder-key-1" };
        HolderPublicKey = new ECDsaSecurityKey(ECDsa.Create(holderEcdsa.ExportParameters(false))) { KeyId = "holder-key-1" };
        HolderPublicJwk = JsonWebKeyConverter.ConvertFromSecurityKey(HolderPublicKey);
    }
}