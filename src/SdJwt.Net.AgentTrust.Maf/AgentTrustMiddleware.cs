using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SdJwt.Net.AgentTrust.Core;
using SdJwt.Net.AgentTrust.Policy;

namespace SdJwt.Net.AgentTrust.Maf;

/// <summary>
/// Mints and propagates capability tokens in tool-call middleware.
/// </summary>
public class AgentTrustMiddleware
{
    private readonly CapabilityTokenIssuer _issuer;
    private readonly IPolicyEngine _policyEngine;
    private readonly IReceiptWriter _receiptWriter;
    private readonly AgentTrustMiddlewareOptions _options;
    private readonly ILogger<AgentTrustMiddleware> _logger;

    /// <summary>
    /// Initializes middleware.
    /// </summary>
    public AgentTrustMiddleware(
        CapabilityTokenIssuer issuer,
        IPolicyEngine policyEngine,
        IReceiptWriter receiptWriter,
        AgentTrustMiddlewareOptions options,
        ILogger<AgentTrustMiddleware>? logger = null)
    {
        _issuer = issuer;
        _policyEngine = policyEngine;
        _receiptWriter = receiptWriter;
        _options = options;
        _logger = logger ?? NullLogger<AgentTrustMiddleware>.Instance;
    }

    /// <summary>
    /// Processes a function call.
    /// </summary>
    public async Task InvokeAsync(FunctionCallContext context, Func<FunctionCallContext, Task> next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

        try
        {
            var policy = await _policyEngine.EvaluateAsync(new PolicyRequest
            {
                AgentId = _options.AgentId,
                Tool = context.ToolName,
                Action = context.ActionName,
                Context = context.Context
            });

            if (!policy.IsPermitted)
            {
                throw new InvalidOperationException(policy.DenialReason ?? "Policy denied tool call.");
            }

            if (!_options.ToolAudienceMapping.TryGetValue(context.ToolName, out var audience))
            {
                throw new InvalidOperationException($"No audience mapping found for tool '{context.ToolName}'.");
            }

            var minted = _issuer.Mint(new CapabilityTokenOptions
            {
                Issuer = _options.AgentId,
                Audience = audience,
                Capability = new CapabilityClaim
                {
                    Tool = context.ToolName,
                    Action = context.ActionName,
                    Limits = policy.Constraints?.Limits
                },
                Context = context.Context,
                Lifetime = policy.Constraints?.MaxTokenLifetime ?? _options.DefaultTokenLifetime,
                DisclosableClaims = policy.Constraints?.RequiredDisclosures,
                RequestBinding = context.RequestBinding
            });

            context.Metadata[_options.TokenHeaderName] = $"{_options.TokenHeaderPrefix} {minted.Token}";
            await next(context);
        }
        catch
        {
            if (_options.FailOnMintError)
            {
                throw;
            }

            _logger.LogWarning("Agent trust middleware could not mint token. Continuing due to fail-open mode.");
            await next(context);
        }
    }
}
