using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace SdJwt.Net.AgentTrust.Core;

/// <summary>
/// Default <see cref="ISdJwtKeyBindingValidator"/> implementation.
/// Validates the Key Binding JWT of an SD-JWT+KB presentation: the holder key declared in the
/// SD-JWT's <c>cnf.jwk</c> must match the expected thumbprint (<c>cnf.jkt</c>) from the capability
/// token, the KB-JWT must be signed by that key, carry <c>typ=kb+jwt</c>, and bind to the
/// expected audience and nonce.
/// </summary>
public sealed class SdJwtKeyBindingValidator : ISdJwtKeyBindingValidator
{
    private const string ProofType = "kb-jwt";
    private const string KbTokenType = "kb+jwt";

    private static readonly string[] AllowedAlgorithms =
    [
        SecurityAlgorithms.EcdsaSha256, SecurityAlgorithms.EcdsaSha384, SecurityAlgorithms.EcdsaSha512,
        SecurityAlgorithms.RsaSsaPssSha256, SecurityAlgorithms.RsaSsaPssSha384, SecurityAlgorithms.RsaSsaPssSha512,
        SecurityAlgorithms.RsaSha256, SecurityAlgorithms.RsaSha384, SecurityAlgorithms.RsaSha512
    ];

    /// <inheritdoc/>
    public Task<ProofValidationResult> ValidateAsync(
        string sdJwtPresentation,
        string expectedJwkThumbprint,
        string expectedAudience,
        string? expectedNonce = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sdJwtPresentation))
        {
            return Fail("SD-JWT key binding presentation is missing.", "missing_kb_presentation");
        }

        if (string.IsNullOrWhiteSpace(expectedJwkThumbprint))
        {
            return Fail("Expected JWK thumbprint (cnf.jkt) is missing.", "missing_cnf_jkt");
        }

        Models.ParsedPresentation parsed;
        try
        {
            parsed = Utils.SdJwtParser.ParsePresentation(sdJwtPresentation);
        }
        catch (Exception ex)
        {
            return Fail($"Could not parse SD-JWT presentation: {ex.Message}", "kb_parse_error");
        }

        if (!parsed.IsKeyBindingPresentation || parsed.RawKeyBindingJwt == null || parsed.UnverifiedKeyBindingJwt == null)
        {
            return Fail("Presentation does not contain a Key Binding JWT.", "missing_kb_jwt");
        }

        // The holder public key is declared in the SD-JWT body's cnf.jwk.
        JsonWebKey holderKey;
        try
        {
            holderKey = ExtractCnfJwk(parsed.UnverifiedSdJwt);
        }
        catch (Exception ex)
        {
            return Fail($"Could not read holder key from cnf.jwk: {ex.Message}", "missing_cnf_jwk");
        }

        // That key must match the thumbprint the capability token was bound to.
        string actualThumbprint;
        try
        {
            actualThumbprint = Base64UrlEncoder.Encode(holderKey.ComputeJwkThumbprint());
        }
        catch (Exception ex)
        {
            return Fail($"Could not compute holder key thumbprint: {ex.Message}", "kb_thumbprint_error");
        }

        if (!FixedTimeEquals(actualThumbprint, expectedJwkThumbprint))
        {
            return Fail("Holder key does not match the token's cnf.jkt binding.", "kb_jkt_mismatch");
        }

        // The KB-JWT must be signed by that holder key.
        try
        {
            var handler = new JwtSecurityTokenHandler();
            if (!string.Equals(parsed.UnverifiedKeyBindingJwt.Header.Typ, KbTokenType, StringComparison.Ordinal))
            {
                return Fail($"Key Binding JWT typ must be '{KbTokenType}'.", "invalid_kb_typ");
            }

            var tvp = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = false,
                RequireExpirationTime = false,
                RequireSignedTokens = true,
                ValidateIssuerSigningKey = false,
                IssuerSigningKey = holderKey,
                ValidAlgorithms = AllowedAlgorithms
            };
            handler.ValidateToken(parsed.RawKeyBindingJwt, tvp, out _);
        }
        catch (Exception ex) when (ex is SecurityTokenException or ArgumentException or JsonException or FormatException)
        {
            return Fail($"Key Binding JWT signature validation failed: {ex.Message}", "kb_signature_invalid");
        }

        var kbPayload = parsed.UnverifiedKeyBindingJwt.Payload;

        // Audience binding.
        if (!string.IsNullOrEmpty(expectedAudience))
        {
            var audMatched = parsed.UnverifiedKeyBindingJwt.Audiences.Contains(expectedAudience, StringComparer.Ordinal);
            if (!audMatched)
            {
                return Fail("Key Binding JWT audience does not match the expected audience.", "kb_aud_mismatch");
            }
        }

        // Nonce binding.
        if (!string.IsNullOrEmpty(expectedNonce))
        {
            var nonce = kbPayload.TryGetValue("nonce", out var nonceValue) ? nonceValue?.ToString() : null;
            if (string.IsNullOrEmpty(nonce) || !FixedTimeEquals(nonce, expectedNonce))
            {
                return Fail("Key Binding JWT nonce does not match the expected nonce.", "kb_nonce_mismatch");
            }
        }

        return Task.FromResult(ProofValidationResult.Success(ProofType, actualThumbprint));
    }

    private static JsonWebKey ExtractCnfJwk(JwtSecurityToken sdJwt)
    {
        if (!sdJwt.Payload.TryGetValue("cnf", out var cnfValue) || cnfValue == null)
        {
            throw new SecurityTokenException("SD-JWT has no cnf claim.");
        }

        var cnfJson = cnfValue is JsonElement element
            ? element.GetRawText()
            : JsonSerializer.Serialize(cnfValue);

        using var doc = JsonDocument.Parse(cnfJson);
        if (!doc.RootElement.TryGetProperty("jwk", out var jwkElement) ||
            jwkElement.ValueKind != JsonValueKind.Object)
        {
            throw new SecurityTokenException("cnf claim does not contain a jwk member.");
        }

        return new JsonWebKey(jwkElement.GetRawText());
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
