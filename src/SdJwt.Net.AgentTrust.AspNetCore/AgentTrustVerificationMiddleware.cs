using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using SdJwt.Net.AgentTrust.Core;
using SdJwt.Net.AgentTrust.Policy;
using System.Diagnostics;

namespace SdJwt.Net.AgentTrust.AspNetCore;

/// <summary>
/// ASP.NET Core middleware for inbound capability token verification.
/// </summary>
public class AgentTrustVerificationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly CapabilityTokenVerifier _verifier;
    private readonly IPolicyEngine _policyEngine;
    private readonly IReceiptWriter _receiptWriter;
    private readonly AgentTrustVerificationOptions _options;
    private readonly ILogger<AgentTrustVerificationMiddleware> _logger;

    /// <summary>
    /// Initializes middleware.
    /// </summary>
    public AgentTrustVerificationMiddleware(
        RequestDelegate next,
        CapabilityTokenVerifier verifier,
        IPolicyEngine policyEngine,
        IReceiptWriter receiptWriter,
        IOptions<AgentTrustVerificationOptions> options,
        ILogger<AgentTrustVerificationMiddleware>? logger = null)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _verifier = verifier ?? throw new ArgumentNullException(nameof(verifier));
        _policyEngine = policyEngine ?? throw new ArgumentNullException(nameof(policyEngine));
        _receiptWriter = receiptWriter ?? throw new ArgumentNullException(nameof(receiptWriter));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? NullLogger<AgentTrustVerificationMiddleware>.Instance;
    }

    /// <summary>
    /// Middleware execution.
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        if (IsExcluded(context.Request.Path))
        {
            await _next(context);
            return;
        }

        var started = Stopwatch.GetTimestamp();
        var token = ExtractToken(context);
        if (string.IsNullOrWhiteSpace(token))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        var verificationContext = new AgentTrustVerificationContext
        {
            ExpectedAudience = _options.Audience,
            TrustedIssuers = _options.TrustedIssuers,
            SecurityMode = _options.SecurityMode,
            MaxTokenLifetime = _options.MaxTokenLifetime,
            RequireToolManifestBinding = _options.RequireToolManifestBinding,
            EnforceReplayPrevention = _options.EnforceReplayPrevention
        };

        if (_options.AllowedAlgorithms is not null)
        {
            verificationContext = verificationContext with
            {
                AllowedAlgorithms = _options.AllowedAlgorithms
            };
        }

        var verified = await _verifier.VerifyAsync(token, verificationContext, context.RequestAborted);

        if (!verified.IsValid || verified.Capability == null || verified.Context == null || string.IsNullOrWhiteSpace(verified.Issuer))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return;
        }

        var policy = await _policyEngine.EvaluateAsync(new PolicyRequest
        {
            AgentId = verified.Issuer,
            Tool = verified.Capability.Tool,
            Action = verified.Capability.Action,
            Resource = verified.Capability.Resource,
            Context = verified.Context
        }, context.RequestAborted);

        if (!policy.IsPermitted)
        {
            await WriteReceiptAsync(verified, ReceiptDecision.Deny, policy.DenialCode, started, context.RequestAborted);
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return;
        }

        context.Items[AgentTrustContextKeys.VerificationResult] = verified;
        context.Items[AgentTrustContextKeys.Capability] = verified.Capability;
        context.Items[AgentTrustContextKeys.Context] = verified.Context;
        context.Items[AgentTrustContextKeys.Issuer] = verified.Issuer;

        await _next(context);

        await WriteReceiptAsync(verified, ReceiptDecision.Allow, null, started, context.RequestAborted);
    }

    private async Task WriteReceiptAsync(
        CapabilityVerificationResult verified,
        ReceiptDecision decision,
        string? reason,
        long started,
        CancellationToken cancellationToken)
    {
        if (!_options.EmitReceipts || verified.Capability == null || verified.Context == null || string.IsNullOrWhiteSpace(verified.TokenId))
        {
            return;
        }

        var elapsed = (long)((Stopwatch.GetTimestamp() - started) * 1000d / Stopwatch.Frequency);

        await _receiptWriter.WriteAsync(new AuditReceipt
        {
            TokenId = verified.TokenId,
            Timestamp = DateTimeOffset.UtcNow,
            Decision = decision,
            Tool = verified.Capability.Tool,
            Action = verified.Capability.Action,
            CorrelationId = verified.Context.CorrelationId,
            DenyReason = reason,
            DurationMs = elapsed
        }, cancellationToken);
    }

    private string? ExtractToken(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue(_options.TokenHeaderName, out var value))
        {
            return null;
        }

        var raw = value.ToString();
        if (raw.StartsWith(_options.TokenHeaderPrefix + " ", StringComparison.OrdinalIgnoreCase))
        {
            return raw[(_options.TokenHeaderPrefix.Length + 1)..].Trim();
        }

        return raw;
    }

    private bool IsExcluded(PathString path)
    {
        var candidate = path.Value ?? string.Empty;
        foreach (var pattern in _options.ExcludedPaths)
        {
            if (pattern.EndsWith("*", StringComparison.Ordinal))
            {
                var prefix = pattern[..^1];
                if (candidate.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            else if (string.Equals(candidate, pattern, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
