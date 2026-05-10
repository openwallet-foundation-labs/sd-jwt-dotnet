using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SdJwt.Net.AgentTrust.Core;
using SdJwt.Net.AgentTrust.Policy;

namespace SdJwt.Net.AgentTrust.Maf;

/// <summary>
/// Adapter for MCP tool calls.
/// </summary>
public class McpTrustAdapter
{
    private readonly CapabilityTokenIssuer _issuer;
    private readonly IPolicyEngine _policyEngine;
    private readonly ILogger<McpTrustAdapter> _logger;
    private readonly string _agentId;
    private readonly IReadOnlyDictionary<string, string> _toolAudienceMapping;

    /// <summary>
    /// Initializes a new adapter.
    /// </summary>
    public McpTrustAdapter(
        CapabilityTokenIssuer issuer,
        IPolicyEngine policyEngine,
        string agentId,
        IReadOnlyDictionary<string, string> toolAudienceMapping,
        ILogger<McpTrustAdapter>? logger = null)
    {
        _issuer = issuer;
        _policyEngine = policyEngine;
        _agentId = agentId;
        _toolAudienceMapping = toolAudienceMapping;
        _logger = logger ?? NullLogger<McpTrustAdapter>.Instance;
    }

    /// <summary>
    /// Mints a capability token for a tool call.
    /// </summary>
    public async Task<CapabilityTokenResult> MintForToolCallAsync(
        string toolName,
        IDictionary<string, object>? arguments,
        CapabilityContext context,
        RequestBinding? requestBinding = null,
        CancellationToken cancellationToken = default)
    {
        if (!_toolAudienceMapping.TryGetValue(toolName, out var audience))
        {
            throw new InvalidOperationException($"No audience mapping found for tool '{toolName}'.");
        }

        var action = arguments?.TryGetValue("action", out var actionObj) == true
            ? actionObj?.ToString() ?? "Invoke"
            : "Read";

        var policy = await _policyEngine.EvaluateAsync(new PolicyRequest
        {
            AgentId = _agentId,
            Tool = toolName,
            Action = action,
            Context = context
        }, cancellationToken);

        if (!policy.IsPermitted)
        {
            throw new InvalidOperationException(policy.DenialReason ?? "Policy denied request.");
        }

        var result = _issuer.Mint(new CapabilityTokenOptions
        {
            Issuer = _agentId,
            Audience = audience,
            Capability = new CapabilityClaim
            {
                Tool = toolName,
                Action = action,
                Limits = policy.Constraints?.Limits
            },
            Context = context,
            Lifetime = policy.Constraints?.MaxTokenLifetime ?? TimeSpan.FromSeconds(60),
            DisclosableClaims = policy.Constraints?.RequiredDisclosures,
            RequestBinding = requestBinding
        });

        _logger.LogDebug("Minted MCP capability token for tool {ToolName}", toolName);
        return result;
    }
}
