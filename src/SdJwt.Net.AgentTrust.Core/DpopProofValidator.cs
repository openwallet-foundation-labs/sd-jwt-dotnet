using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace SdJwt.Net.AgentTrust.Core;

/// <summary>
/// Default <see cref="IDpopProofValidator"/> implementation per RFC 9449.
/// Verifies a DPoP proof JWT, confirms it is signed by the key whose RFC 7638 thumbprint
/// matches the capability token's <c>cnf.jkt</c>, and binds it to the HTTP method/URI.
/// </summary>
public sealed class DpopProofValidator : IDpopProofValidator
{
    private const string ProofType = "dpop";
    private const string DpopTokenType = "dpop+jwt";

    // Asymmetric signature algorithms only. "none" and HMAC are deliberately excluded:
    // a DPoP proof MUST be signed by the holder's private key.
    private static readonly string[] AllowedAlgorithms =
    [
        SecurityAlgorithms.EcdsaSha256, SecurityAlgorithms.EcdsaSha384, SecurityAlgorithms.EcdsaSha512,
        SecurityAlgorithms.RsaSsaPssSha256, SecurityAlgorithms.RsaSsaPssSha384, SecurityAlgorithms.RsaSsaPssSha512,
        SecurityAlgorithms.RsaSha256, SecurityAlgorithms.RsaSha384, SecurityAlgorithms.RsaSha512
    ];

    private readonly TimeSpan _maxProofAge;

    /// <summary>
    /// Initializes a new instance of the <see cref="DpopProofValidator"/> class.
    /// </summary>
    /// <param name="maxProofAge">Maximum accepted age of the DPoP proof based on its <c>iat</c>. Defaults to 5 minutes.</param>
    public DpopProofValidator(TimeSpan? maxProofAge = null)
    {
        _maxProofAge = maxProofAge ?? TimeSpan.FromMinutes(5);
    }

    /// <inheritdoc/>
    public Task<ProofValidationResult> ValidateAsync(
        string dpopProof,
        string expectedJwkThumbprint,
        string httpMethod,
        string httpUri,
        string? accessTokenHash = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dpopProof))
        {
            return Fail("DPoP proof is missing.", "missing_dpop_proof");
        }

        if (string.IsNullOrWhiteSpace(expectedJwkThumbprint))
        {
            return Fail("Expected JWK thumbprint (cnf.jkt) is missing.", "missing_cnf_jkt");
        }

        JwtSecurityToken proof;
        JsonWebKey jwk;
        try
        {
            var handler = new JwtSecurityTokenHandler();
            proof = handler.ReadJwtToken(dpopProof);

            if (!string.Equals(proof.Header.Typ, DpopTokenType, StringComparison.Ordinal))
            {
                return Fail($"DPoP proof typ must be '{DpopTokenType}'.", "invalid_dpop_typ");
            }

            // The proof's public key is embedded in the protected header as 'jwk'.
            jwk = ExtractHeaderJwk(proof);
            if (!string.IsNullOrEmpty(jwk.D))
            {
                return Fail("DPoP proof must not embed a private key.", "dpop_private_key_present");
            }

            // Verify the proof signature with the embedded public key.
            var tvp = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = false,
                RequireExpirationTime = false,
                RequireSignedTokens = true,
                ValidateIssuerSigningKey = false,
                IssuerSigningKey = jwk,
                ValidAlgorithms = AllowedAlgorithms
            };
            handler.ValidateToken(dpopProof, tvp, out _);
        }
        catch (Exception ex) when (ex is SecurityTokenException or ArgumentException or JsonException or FormatException)
        {
            return Fail($"DPoP proof signature validation failed: {ex.Message}", "dpop_signature_invalid");
        }

        // The proof key must be the key the token was bound to (cnf.jkt).
        string actualThumbprint;
        try
        {
            actualThumbprint = Base64UrlEncoder.Encode(jwk.ComputeJwkThumbprint());
        }
        catch (Exception ex)
        {
            return Fail($"Could not compute DPoP key thumbprint: {ex.Message}", "dpop_thumbprint_error");
        }

        if (!FixedTimeEquals(actualThumbprint, expectedJwkThumbprint))
        {
            return Fail("DPoP proof key does not match the token's cnf.jkt binding.", "dpop_jkt_mismatch");
        }

        // htm: HTTP method binding (case-insensitive per RFC 9449).
        var htm = GetClaim(proof, "htm");
        if (string.IsNullOrEmpty(htm) || !string.Equals(htm, httpMethod, StringComparison.OrdinalIgnoreCase))
        {
            return Fail("DPoP proof htm does not match the request method.", "dpop_htm_mismatch");
        }

        // htu: HTTP URI binding (compared without query/fragment per RFC 9449).
        var htu = GetClaim(proof, "htu");
        if (string.IsNullOrEmpty(htu) || !UriMatches(htu, httpUri))
        {
            return Fail("DPoP proof htu does not match the request URI.", "dpop_htu_mismatch");
        }

        // jti is required (replay scoping is the caller's responsibility).
        if (string.IsNullOrEmpty(GetClaim(proof, JwtRegisteredClaimNames.Jti)))
        {
            return Fail("DPoP proof is missing the jti claim.", "dpop_missing_jti");
        }

        // iat freshness.
        var iat = GetClaim(proof, JwtRegisteredClaimNames.Iat);
        if (string.IsNullOrEmpty(iat) || !long.TryParse(iat, out var iatUnix))
        {
            return Fail("DPoP proof is missing a valid iat claim.", "dpop_missing_iat");
        }
        var age = DateTimeOffset.UtcNow - DateTimeOffset.FromUnixTimeSeconds(iatUnix);
        if (age > _maxProofAge || age < -_maxProofAge)
        {
            return Fail("DPoP proof iat is outside the accepted freshness window.", "dpop_stale");
        }

        // ath: bind to the access token when one is in play (dual binding).
        if (!string.IsNullOrEmpty(accessTokenHash))
        {
            var ath = GetClaim(proof, "ath");
            if (string.IsNullOrEmpty(ath) || !FixedTimeEquals(ath, accessTokenHash))
            {
                return Fail("DPoP proof ath does not match the access token hash.", "dpop_ath_mismatch");
            }
        }

        return Task.FromResult(ProofValidationResult.Success(ProofType, actualThumbprint));
    }

    /// <summary>
    /// Computes the RFC 9449 <c>ath</c> value: base64url(SHA-256(ASCII(access_token))).
    /// </summary>
    public static string ComputeAccessTokenHash(string accessToken)
    {
        if (accessToken == null)
        {
            throw new ArgumentNullException(nameof(accessToken));
        }

        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.ASCII.GetBytes(accessToken));
        return Base64UrlEncoder.Encode(hash);
    }

    private static JsonWebKey ExtractHeaderJwk(JwtSecurityToken proof)
    {
        var headerJson = Base64UrlEncoder.Decode(proof.EncodedHeader);
        using var doc = JsonDocument.Parse(headerJson);
        if (!doc.RootElement.TryGetProperty("jwk", out var jwkElement) ||
            jwkElement.ValueKind != JsonValueKind.Object)
        {
            throw new SecurityTokenException("DPoP proof header is missing the 'jwk' member.");
        }

        return new JsonWebKey(jwkElement.GetRawText());
    }

    private static string? GetClaim(JwtSecurityToken proof, string name)
        => proof.Payload.TryGetValue(name, out var value) ? value?.ToString() : null;

    private static bool UriMatches(string left, string right)
    {
        static string Normalize(string uri)
        {
            if (Uri.TryCreate(uri, UriKind.Absolute, out var parsed))
            {
                return parsed.GetLeftPart(UriPartial.Path).TrimEnd('/');
            }

            var cut = uri.IndexOfAny(['?', '#']);
            var path = cut >= 0 ? uri[..cut] : uri;
            return path.TrimEnd('/');
        }

        return string.Equals(Normalize(left), Normalize(right), StringComparison.OrdinalIgnoreCase);
    }

    private static bool FixedTimeEquals(string? left, string? right)
    {
        if (left == null || right == null)
        {
            return false;
        }

        var leftBytes = Encoding.UTF8.GetBytes(left);
        var rightBytes = Encoding.UTF8.GetBytes(right);
        return leftBytes.Length == rightBytes.Length &&
            CryptographicOperations.FixedTimeEquals(leftBytes, rightBytes);
    }

    private static Task<ProofValidationResult> Fail(string error, string errorCode)
        => Task.FromResult(ProofValidationResult.Failure(error, errorCode, ProofType));
}
