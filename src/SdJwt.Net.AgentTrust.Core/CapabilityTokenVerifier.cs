using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;

namespace SdJwt.Net.AgentTrust.Core;

/// <summary>
/// Verifies SD-JWT capability tokens.
/// </summary>
public class CapabilityTokenVerifier
{
    private readonly INonceStore _nonceStore;
    private readonly ILogger<CapabilityTokenVerifier> _logger;

    /// <summary>
    /// Initializes a new verifier.
    /// </summary>
    public CapabilityTokenVerifier(INonceStore nonceStore, ILogger<CapabilityTokenVerifier>? logger = null)
    {
        _nonceStore = nonceStore ?? throw new ArgumentNullException(nameof(nonceStore));
        _logger = logger ?? NullLogger<CapabilityTokenVerifier>.Instance;
    }

    /// <summary>
    /// Verifies a capability token.
    /// </summary>
    public async Task<CapabilityVerificationResult> VerifyAsync(
        string token,
        CapabilityVerificationOptions options,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return CapabilityVerificationResult.Failure("Token is required.", "invalid_token");
        }

        if (options == null)
        {
            return CapabilityVerificationResult.Failure("Options are required.", "invalid_options");
        }

        try
        {
            var parsed = SdJwt.Net.Utils.SdJwtParser.ParsePresentation(token);
            var compact = parsed.RawSdJwt;
            var jwt = new JwtSecurityToken(compact);
            var issuer = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Iss)?.Value;

            if (string.IsNullOrWhiteSpace(issuer) || !options.TrustedIssuers.TryGetValue(issuer, out var key))
            {
                return CapabilityVerificationResult.Failure("Issuer is not trusted.", "untrusted_issuer");
            }

            var tvp = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = false,
                RequireExpirationTime = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                RequireSignedTokens = true
            };

            var handler = new JwtSecurityTokenHandler();
            handler.ValidateToken(compact, tvp, out var validatedToken);
            var validatedJwt = (JwtSecurityToken)validatedToken;
            var payload = validatedJwt.Payload;

            if (!payload.TryGetValue(JwtRegisteredClaimNames.Jti, out var tokenIdValue) ||
                !payload.TryGetValue(JwtRegisteredClaimNames.Exp, out var expValue) ||
                !payload.TryGetValue("cap", out var capValue) ||
                !payload.TryGetValue("ctx", out var ctxValue))
            {
                return CapabilityVerificationResult.Failure("claims are missing.", "invalid_claims");
            }

            var tokenId = tokenIdValue?.ToString();
            if (string.IsNullOrWhiteSpace(tokenId))
            {
                return CapabilityVerificationResult.Failure("Token id is invalid.", "invalid_claims");
            }

            if (!payload.TryGetValue(JwtRegisteredClaimNames.Iss, out var issValue) ||
                !string.Equals(issValue?.ToString(), issuer, StringComparison.Ordinal))
            {
                return CapabilityVerificationResult.Failure("Issuer claim mismatch.", "invalid_issuer");
            }

            if (!payload.TryGetValue(JwtRegisteredClaimNames.Aud, out var audValue))
            {
                return CapabilityVerificationResult.Failure("Audience claim missing.", "invalid_audience");
            }

            var audienceMatched = audValue switch
            {
                string aud => string.Equals(aud, options.ExpectedAudience, StringComparison.Ordinal),
                IEnumerable<object> audList => audList.Any(a => string.Equals(a?.ToString(), options.ExpectedAudience, StringComparison.Ordinal)),
                _ => string.Equals(audValue?.ToString(), options.ExpectedAudience, StringComparison.Ordinal)
            };

            if (!audienceMatched)
            {
                return CapabilityVerificationResult.Failure("Audience claim mismatch.", "invalid_audience");
            }

            if (!long.TryParse(expValue?.ToString(), out var expUnix))
            {
                return CapabilityVerificationResult.Failure("Token expiry claim is invalid.", "invalid_claims");
            }

            if (DateTimeOffset.UtcNow > DateTimeOffset.FromUnixTimeSeconds(expUnix).Add(options.ClockSkewTolerance))
            {
                return CapabilityVerificationResult.Failure("Token is expired.", "token_expired");
            }

            if (options.EnforceReplayPrevention)
            {
                var marked = await _nonceStore.TryMarkAsUsedAsync(tokenId, DateTimeOffset.FromUnixTimeSeconds(expUnix), cancellationToken);
                if (!marked)
                {
                    return CapabilityVerificationResult.Failure("Replay detected.", "replay_detected");
                }
            }

            var capability = DeserializeClaim<CapabilityClaim>(capValue);
            var context = DeserializeClaim<CapabilityContext>(ctxValue);
            if (capability == null || context == null)
            {
                return CapabilityVerificationResult.Failure("Capability payload is invalid.", "invalid_claims");
            }

            return CapabilityVerificationResult.Success(capability, context, tokenId, issuer);
        }
        catch (Exception ex) when (ex is SecurityTokenException or JsonException or ArgumentException)
        {
            _logger.LogWarning(ex, "Capability token verification failed.");
            return CapabilityVerificationResult.Failure(ex.Message, "verification_failed");
        }
    }

    private static T? DeserializeClaim<T>(object value)
    {
        if (value is JsonElement element)
        {
            return element.Deserialize<T>();
        }

        var json = JsonSerializer.Serialize(value);
        return JsonSerializer.Deserialize<T>(json);
    }
}

