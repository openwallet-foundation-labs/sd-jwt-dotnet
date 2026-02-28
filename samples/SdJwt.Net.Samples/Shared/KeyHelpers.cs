using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

namespace SdJwt.Net.Samples.Shared;

/// <summary>
/// Shared cryptographic key utilities for all samples.
/// Provides consistent key generation and management across tutorials.
/// </summary>
public static class KeyHelpers
{
    /// <summary>
    /// Creates an ECDSA P-256 key pair for use as an issuer signing key.
    /// </summary>
    /// <param name="keyId">Optional key identifier (defaults to generated GUID)</param>
    /// <returns>Tuple of (privateKey, publicKey) as ECDsaSecurityKey</returns>
    public static (ECDsaSecurityKey PrivateKey, ECDsaSecurityKey PublicKey) CreateIssuerKeyPair(string? keyId = null)
    {
        var ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        keyId ??= $"issuer-{Guid.NewGuid():N}"[..20];

        var privateKey = new ECDsaSecurityKey(ecdsa) { KeyId = keyId };
        var publicKey = new ECDsaSecurityKey(ecdsa) { KeyId = keyId };

        return (privateKey, publicKey);
    }

    /// <summary>
    /// Creates an ECDSA P-256 key pair for use as a holder binding key.
    /// </summary>
    /// <param name="keyId">Optional key identifier (defaults to generated GUID)</param>
    /// <returns>Tuple of (privateKey, publicKey, jwk) for holder operations</returns>
    public static (ECDsaSecurityKey PrivateKey, ECDsaSecurityKey PublicKey, JsonWebKey Jwk) CreateHolderKeyPair(string? keyId = null)
    {
        var ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        keyId ??= $"holder-{Guid.NewGuid():N}"[..20];

        var privateKey = new ECDsaSecurityKey(ecdsa) { KeyId = keyId };
        var publicKey = new ECDsaSecurityKey(ecdsa) { KeyId = keyId };
        var jwk = JsonWebKeyConverter.ConvertFromSecurityKey(publicKey);

        return (privateKey, publicKey, jwk);
    }

    /// <summary>
    /// Creates an ECDSA P-384 key pair for higher security requirements (HAIP Level 2+).
    /// </summary>
    public static (ECDsaSecurityKey PrivateKey, ECDsaSecurityKey PublicKey) CreateP384KeyPair(string? keyId = null)
    {
        var ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP384);
        keyId ??= $"key-p384-{Guid.NewGuid():N}"[..24];

        var privateKey = new ECDsaSecurityKey(ecdsa) { KeyId = keyId };
        var publicKey = new ECDsaSecurityKey(ecdsa) { KeyId = keyId };

        return (privateKey, publicKey);
    }

    /// <summary>
    /// Creates a verifier key resolver function for simple single-issuer scenarios.
    /// </summary>
    public static Func<string, Task<SecurityKey>> CreateSimpleKeyResolver(SecurityKey issuerPublicKey)
    {
        return _ => Task.FromResult(issuerPublicKey);
    }

    /// <summary>
    /// Creates a verifier key resolver that maps issuer identifiers to their public keys.
    /// </summary>
    public static Func<string, Task<SecurityKey>> CreateMultiIssuerKeyResolver(
        Dictionary<string, SecurityKey> issuerKeys)
    {
        return issuer =>
        {
            if (issuerKeys.TryGetValue(issuer, out var key))
            {
                return Task.FromResult(key);
            }
            throw new SecurityTokenException($"Unknown issuer: {issuer}");
        };
    }
}
