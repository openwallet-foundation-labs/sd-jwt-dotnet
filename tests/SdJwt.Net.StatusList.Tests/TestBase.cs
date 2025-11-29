using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

namespace SdJwt.Net.StatusList.Tests;

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

        // Create a single ECDsa instance for the holder that holds both private and public keys.
        var holderEcdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);

        // FIX: Both the private key (for signing) and the public key (for verification)
        // should wrap the same underlying ECDsa object.
        HolderPrivateKey = new ECDsaSecurityKey(holderEcdsa) { KeyId = "holder-key-1" };
        HolderPublicKey = new ECDsaSecurityKey(holderEcdsa) { KeyId = "holder-key-1" };

        HolderPublicJwk = JsonWebKeyConverter.ConvertFromSecurityKey(HolderPublicKey);
    }
}