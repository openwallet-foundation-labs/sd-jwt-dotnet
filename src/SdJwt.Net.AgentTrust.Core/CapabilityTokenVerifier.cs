using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text.Json;

namespace SdJwt.Net.AgentTrust.Core;

/// <summary>
/// Verifies SD-JWT capability tokens.
/// </summary>
public class CapabilityTokenVerifier
{
    private readonly INonceStore _nonceStore;
    private readonly ILogger<CapabilityTokenVerifier> _logger;
    private readonly IDpopProofValidator _dpopProofValidator;
    private readonly ISdJwtKeyBindingValidator _keyBindingValidator;

    /// <summary>
    /// Initializes a new verifier.
    /// </summary>
    /// <param name="nonceStore">Replay-prevention nonce store.</param>
    /// <param name="logger">Optional logger.</param>
    /// <param name="dpopProofValidator">Optional DPoP proof validator. Defaults to <see cref="DpopProofValidator"/>.</param>
    /// <param name="keyBindingValidator">Optional SD-JWT key-binding validator. Defaults to <see cref="SdJwtKeyBindingValidator"/>.</param>
    public CapabilityTokenVerifier(
        INonceStore nonceStore,
        ILogger<CapabilityTokenVerifier>? logger = null,
        IDpopProofValidator? dpopProofValidator = null,
        ISdJwtKeyBindingValidator? keyBindingValidator = null)
    {
        _nonceStore = nonceStore ?? throw new ArgumentNullException(nameof(nonceStore));
        _logger = logger ?? NullLogger<CapabilityTokenVerifier>.Instance;
        _dpopProofValidator = dpopProofValidator ?? new DpopProofValidator();
        _keyBindingValidator = keyBindingValidator ?? new SdJwtKeyBindingValidator();
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

        // Sender-constraint (PoP) and request-binding enforcement. These are only evaluated
        // when explicitly required by the context; with the flags unset behaviour is unchanged.
        if (context.RequireRequestBinding || context.RequireProofOfPossession)
        {
            JwtPayload bindingPayload;
            try
            {
                var parsed = SdJwt.Net.Utils.SdJwtParser.ParsePresentation(token);
                bindingPayload = new JwtSecurityToken(parsed.RawSdJwt).Payload;
            }
            catch (Exception ex)
            {
                return CapabilityVerificationResult.Failure(
                    $"Could not read token claims for binding enforcement: {ex.Message}", "binding_parse_error");
            }

            if (context.RequireRequestBinding)
            {
                var rbFailure = EnforceRequestBinding(bindingPayload, context);
                if (rbFailure != null)
                {
                    return rbFailure;
                }
            }

            if (context.RequireProofOfPossession)
            {
                var (popFailure, proofType) = await EnforceProofOfPossessionAsync(bindingPayload, context, cancellationToken);
                if (popFailure != null)
                {
                    return popFailure;
                }

                result = result with
                {
                    ProofType = proofType
                };
            }
        }

        return result;
    }

    private static CapabilityVerificationResult? EnforceRequestBinding(
        JwtPayload payload,
        AgentTrustVerificationContext context)
    {
        if (!payload.TryGetValue("req_bind", out var rbValue) || rbValue == null)
        {
            return CapabilityVerificationResult.Failure(
                "Request binding is required but the token has no req_bind claim.", "missing_request_binding");
        }

        var reqBind = DeserializeClaim<RequestBinding>(rbValue);
        if (reqBind == null)
        {
            return CapabilityVerificationResult.Failure("Token req_bind claim is invalid.", "invalid_request_binding");
        }

        if (context.ActualRequest == null)
        {
            return CapabilityVerificationResult.Failure(
                "Request binding is required but no actual request was supplied.", "missing_actual_request");
        }

        if (!string.Equals(reqBind.Method, context.ActualRequest.Method, StringComparison.OrdinalIgnoreCase))
        {
            return CapabilityVerificationResult.Failure(
                "Request method does not match the bound method.", "request_binding_mismatch");
        }

        if (!UriMatches(reqBind.Uri, context.ActualRequest.Uri))
        {
            return CapabilityVerificationResult.Failure(
                "Request URI does not match the bound URI.", "request_binding_mismatch");
        }

        if (!string.IsNullOrEmpty(reqBind.BodyHash))
        {
            var actualHash = RequestBinding.ComputeHash(context.ActualRequest.Body ?? Array.Empty<byte>());
            if (!FixedTimeEquals(actualHash, reqBind.BodyHash))
            {
                return CapabilityVerificationResult.Failure(
                    "Request body hash does not match the bound hash.", "request_binding_mismatch");
            }
        }

        return null;
    }

    private async Task<(CapabilityVerificationResult? Failure, string? ProofType)> EnforceProofOfPossessionAsync(
        JwtPayload payload,
        AgentTrustVerificationContext context,
        CancellationToken cancellationToken)
    {
        if (!payload.TryGetValue("cnf", out var cnfValue) || cnfValue == null)
        {
            return (CapabilityVerificationResult.Failure(
                "Proof of possession is required but the token has no cnf claim.", "missing_cnf"), null);
        }

        var cnf = DeserializeClaim<Dictionary<string, string>>(cnfValue);
        var jkt = cnf != null && cnf.TryGetValue("jkt", out var j) ? j : null;
        var x5t = cnf != null && cnf.TryGetValue("x5t#S256", out var x) ? x : null;

        if (string.IsNullOrEmpty(jkt) && string.IsNullOrEmpty(x5t))
        {
            return (CapabilityVerificationResult.Failure(
                "Token cnf claim does not contain a usable sender constraint.", "invalid_cnf"), null);
        }

        var proof = context.ProofMaterial;
        if (proof == null)
        {
            return (CapabilityVerificationResult.Failure(
                "Proof of possession is required but no proof material was supplied.", "missing_proof"), null);
        }

        ProofValidationResult? popResult;
        if (!string.IsNullOrEmpty(jkt) && !string.IsNullOrEmpty(proof.DpopProof))
        {
            var ath = string.IsNullOrEmpty(proof.OAuthAccessToken)
                ? null
                : DpopProofValidator.ComputeAccessTokenHash(proof.OAuthAccessToken);
            popResult = await _dpopProofValidator.ValidateAsync(
                proof.DpopProof,
                jkt,
                context.ActualRequest?.Method ?? string.Empty,
                context.ActualRequest?.Uri ?? string.Empty,
                ath,
                cancellationToken);
        }
        else if (!string.IsNullOrEmpty(jkt) && !string.IsNullOrEmpty(proof.SdJwtKeyBinding))
        {
            popResult = await _keyBindingValidator.ValidateAsync(
                proof.SdJwtKeyBinding,
                jkt,
                context.ExpectedAudience,
                cancellationToken: cancellationToken);
        }
        else if (!string.IsNullOrEmpty(x5t) && !string.IsNullOrEmpty(proof.MtlsCertificateThumbprint))
        {
            popResult = FixedTimeEquals(proof.MtlsCertificateThumbprint, x5t)
                ? ProofValidationResult.Success("mtls", null)
                : ProofValidationResult.Failure(
                    "mTLS certificate thumbprint does not match the token cnf binding.", "mtls_thumbprint_mismatch", "mtls");
        }
        else
        {
            return (CapabilityVerificationResult.Failure(
                "No proof material matching the token's cnf sender constraint was supplied.", "missing_proof"), null);
        }

        if (popResult == null || !popResult.IsValid)
        {
            return (CapabilityVerificationResult.Failure(
                popResult?.Error ?? "Proof of possession validation failed.",
                popResult?.ErrorCode ?? "proof_invalid"), null);
        }

        return (null, popResult.ProofType);
    }

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

        var leftBytes = System.Text.Encoding.UTF8.GetBytes(left);
        var rightBytes = System.Text.Encoding.UTF8.GetBytes(right);
        return leftBytes.Length == rightBytes.Length &&
            CryptographicOperations.FixedTimeEquals(leftBytes, rightBytes);
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

