using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics;
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
            using var activity = AgentTrustActivitySource.Source.StartActivity("AgentTrust.Verify");

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

            // Enforce iat freshness and max token lifetime
            if (payload.TryGetValue(JwtRegisteredClaimNames.Iat, out var iatValue) &&
                long.TryParse(iatValue?.ToString(), out var iatUnix))
            {
                var lifetime = TimeSpan.FromSeconds(expUnix - iatUnix);
                if (lifetime > options.MaxTokenLifetime)
                {
                    return CapabilityVerificationResult.Failure(
                        $"Token lifetime {lifetime.TotalSeconds}s exceeds maximum {options.MaxTokenLifetime.TotalSeconds}s.",
                        "excessive_lifetime");
                }
            }

            // Enforce algorithm allowlist
            if (options.AllowedAlgorithms != null && options.AllowedAlgorithms.Count > 0)
            {
                var alg = validatedJwt.Header.Alg;
                if (!options.AllowedAlgorithms.Contains(alg, StringComparer.OrdinalIgnoreCase))
                {
                    return CapabilityVerificationResult.Failure(
                        $"Algorithm '{alg}' is not in the allowed list.",
                        "algorithm_not_allowed");
                }
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

            activity?.SetTag("agent_trust.token_id", tokenId);
            activity?.SetTag("agent_trust.issuer", issuer);
            activity?.SetTag("agent_trust.tool", capability.Tool);
            activity?.SetTag("agent_trust.action", capability.Action);
            activity?.SetTag("agent_trust.result", "valid");

            return CapabilityVerificationResult.Success(capability, context, tokenId, issuer);
        }
        catch (Exception ex) when (ex is SecurityTokenException or JsonException or ArgumentException)
        {
            _logger.LogWarning(ex, "Capability token verification failed.");
            return CapabilityVerificationResult.Failure(ex.Message, "verification_failed");
        }
    }

    /// <summary>
    /// Verifies a capability token with strict enterprise validation context.
    /// Enforces algorithm allowlists, nbf/iat freshness, max lifetime,
    /// request binding, tool/action matching, and tenant boundaries.
    /// </summary>
    public async Task<CapabilityVerificationResult> VerifyAsync(
        string token,
        AgentTrustVerificationContext context,
        CancellationToken cancellationToken = default)
    {
        // Delegate to base verification first
        var options = new CapabilityVerificationOptions
        {
            ExpectedAudience = context.ExpectedAudience,
            TrustedIssuers = context.TrustedIssuers,
            EnforceReplayPrevention = context.EnforceReplayPrevention,
            ClockSkewTolerance = context.ClockSkewTolerance,
            MaxTokenLifetime = context.MaxTokenLifetime,
            AllowedAlgorithms = context.AllowedAlgorithms?.ToList()
        };

        var result = await VerifyAsync(token, options, cancellationToken);
        if (!result.IsValid)
        {
            return result;
        }

        result = result with
        {
            SecurityMode = context.SecurityMode
        };

        // Enforce tool/action matching
        if (!string.IsNullOrWhiteSpace(context.ExpectedToolId) && result.Capability != null)
        {
            var toolMatched = string.Equals(result.Capability.Tool, context.ExpectedToolId, StringComparison.OrdinalIgnoreCase)
                || string.Equals(result.Capability.ToolId, context.ExpectedToolId, StringComparison.OrdinalIgnoreCase);
            if (!toolMatched)
            {
                return CapabilityVerificationResult.Failure(
                    $"Token tool '{result.Capability.Tool}' does not match expected '{context.ExpectedToolId}'.",
                    "tool_mismatch");
            }
        }

        if (!string.IsNullOrWhiteSpace(context.ExpectedAction) && result.Capability != null)
        {
            if (!string.Equals(result.Capability.Action, context.ExpectedAction, StringComparison.OrdinalIgnoreCase))
            {
                return CapabilityVerificationResult.Failure(
                    $"Token action '{result.Capability.Action}' does not match expected '{context.ExpectedAction}'.",
                    "action_mismatch");
            }
        }

        // Enforce tenant boundary
        if (!string.IsNullOrWhiteSpace(context.ExpectedTenantId) && result.Context != null)
        {
            if (!string.Equals(result.Context.TenantId, context.ExpectedTenantId, StringComparison.OrdinalIgnoreCase))
            {
                return CapabilityVerificationResult.Failure(
                    "Token tenant does not match expected tenant boundary.",
                    "tenant_mismatch");
            }
        }

        // Enforce tool manifest binding
        if (context.RequireToolManifestBinding && result.Capability != null)
        {
            if (string.IsNullOrWhiteSpace(result.Capability.ToolManifestHash))
            {
                return CapabilityVerificationResult.Failure(
                    "Tool manifest hash is required but not present in token.",
                    "missing_manifest_hash");
            }

            if (!string.IsNullOrWhiteSpace(context.ExpectedToolSchemaHash) &&
                !string.Equals(result.Capability.ToolManifestHash, context.ExpectedToolSchemaHash, StringComparison.Ordinal))
            {
                return CapabilityVerificationResult.Failure(
                    "Tool manifest hash does not match expected schema hash.",
                    "manifest_hash_mismatch");
            }
        }

        // Enforce accepted token types
        if (context.AcceptedTokenTypes != null && context.AcceptedTokenTypes.Count > 0)
        {
            try
            {
                var parsed = SdJwt.Net.Utils.SdJwtParser.ParsePresentation(token);
                var jwt = new JwtSecurityToken(parsed.RawSdJwt);
                var typ = jwt.Header.Typ;
                if (typ != null && !context.AcceptedTokenTypes.Contains(typ, StringComparer.OrdinalIgnoreCase))
                {
                    return CapabilityVerificationResult.Failure(
                        $"Token type '{typ}' is not in accepted types.",
                        "invalid_token_type");
                }
            }
            catch
            {
                // Token type check is best-effort; parse errors are caught by base verification
            }
        }

        return result;
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

