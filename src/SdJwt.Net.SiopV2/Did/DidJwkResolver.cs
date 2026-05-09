using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json;

namespace SdJwt.Net.SiopV2.Did;

/// <summary>
/// Resolves verification keys from did:jwk DIDs (SIOPv2 draft-13 Section 6.2.1).
/// A did:jwk DID encodes the public JWK directly in the DID string as a base64url value,
/// e.g. did:jwk:eyJrdHkiOiJFQyIsImNydiI6IlAtMjU2IiwieCI6Ii4uLiIsInkiOiIuLi4ifQ
/// </summary>
public sealed class DidJwkResolver : IDidKeyResolver
{
    /// <inheritdoc />
    public Task<SecurityKey> ResolveKeyAsync(string did, string? keyId, CancellationToken cancellationToken = default)
    {
        const string prefix = "did:jwk:";
        if (!did.StartsWith(prefix, StringComparison.Ordinal))
        {
            throw new ArgumentException($"Not a did:jwk DID: '{did}'", nameof(did));
        }

        var encoded = did.Substring(prefix.Length);

        byte[] jwkBytes;
        try
        {
            jwkBytes = Base64UrlEncoder.DecodeBytes(encoded);
        }
        catch (Exception ex)
        {
            throw new SecurityTokenException($"did:jwk base64url decoding failed for '{did}'.", ex);
        }

        string jwkJson;
        try
        {
            jwkJson = Encoding.UTF8.GetString(jwkBytes);
            // Validate it parses as JSON before handing to JsonWebKey
            using var doc = JsonDocument.Parse(jwkJson);
        }
        catch (Exception ex)
        {
            throw new SecurityTokenException($"did:jwk payload is not valid JSON for '{did}'.", ex);
        }

        JsonWebKey jwk;
        try
        {
            jwk = new JsonWebKey(jwkJson);
        }
        catch (Exception ex)
        {
            throw new SecurityTokenException($"did:jwk payload is not a valid JWK for '{did}'.", ex);
        }

        return Task.FromResult<SecurityKey>(jwk);
    }
}
