using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SdJwt.Net.AgentTrust.Core;
using SdJwt.Net.AgentTrust.Policy;

namespace SdJwt.Net.AgentTrust.Mcp;

/// <summary>
/// Client-side interceptor that mints and attaches capability tokens to outbound MCP tool calls.
/// </summary>
public class McpClientTrustInterceptor
{
    private readonly CapabilityTokenIssuer _issuer;
    private readonly IPolicyEngine _policyEngine;
    private readonly McpClientTrustOptions _options;
    private readonly ILogger<McpClientTrustInterceptor> _logger;

    /// <summary>
    /// Initializes a new MCP client trust interceptor.
    /// </summary>
    /// <param name="issuer">Capability token issuer.</param>
    /// <param name="policyEngine">Policy engine for pre-flight evaluation.</param>
    /// <param name="options">Client trust options.</param>
    /// <param name="logger">Optional logger.</param>
    public McpClientTrustInterceptor(
        CapabilityTokenIssuer issuer,
        IPolicyEngine policyEngine,
        McpClientTrustOptions options,
        ILogger<McpClientTrustInterceptor>? logger = null)
    {
        _issuer = issuer ?? throw new ArgumentNullException(nameof(issuer));
        _policyEngine = policyEngine ?? throw new ArgumentNullException(nameof(policyEngine));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? NullLogger<McpClientTrustInterceptor>.Instance;
    }

    /// <summary>
    /// Evaluates policy and mints a capability token for the given tool call.
    /// </summary>
    /// <param name="toolCall">The MCP tool call to authorize.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The minted token result, or null if the policy denied the call.</returns>
    public async Task<McpTrustResult> BeforeToolCallAsync(
        McpToolCall toolCall,
        CancellationToken cancellationToken = default)
    {
        if (toolCall == null)
        {
            throw new ArgumentNullException(nameof(toolCall));
        }

        if (!_options.ToolAudienceMapping.TryGetValue(toolCall.ToolName, out var audience))
        {
            return McpTrustResult.Failed($"No audience mapping for tool '{toolCall.ToolName}'.");
        }

        var policyResult = await _policyEngine.EvaluateAsync(new PolicyRequest
        {
            AgentId = _options.AgentId,
            Tool = toolCall.ToolName,
            Action = toolCall.Action,
            Context = toolCall.Context
        }, cancellationToken);

        if (!policyResult.IsPermitted)
        {
            _logger.LogWarning("Policy denied MCP tool call {Tool}/{Action}: {Reason}",
                toolCall.ToolName, toolCall.Action, policyResult.DenialReason);
            return McpTrustResult.Denied(policyResult.DenialReason ?? "Policy denied.");
        }

        var tokenResult = _issuer.Mint(new CapabilityTokenOptions
        {
            Issuer = _options.AgentId,
            Audience = audience,
            Capability = new CapabilityClaim
            {
                Tool = toolCall.ToolName,
                Action = toolCall.Action
            },
            Context = toolCall.Context ?? new CapabilityContext { CorrelationId = Guid.NewGuid().ToString("N") },
            Lifetime = _options.DefaultTokenLifetime
        });

        return McpTrustResult.Success(tokenResult.Token, tokenResult.TokenId);
    }
}
