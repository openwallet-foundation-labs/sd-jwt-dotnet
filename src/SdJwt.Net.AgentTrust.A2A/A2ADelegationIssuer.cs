using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SdJwt.Net.AgentTrust.Core;
using SdJwt.Net.AgentTrust.Policy;

namespace SdJwt.Net.AgentTrust.A2A;

/// <summary>
/// Mints delegation tokens for agent-to-agent capability delegation.
/// Wraps the capability token issuer with delegation-specific semantics.
/// </summary>
public class A2ADelegationIssuer
{
    private readonly CapabilityTokenIssuer _issuer;
    private readonly IPolicyEngine _policyEngine;
    private readonly ILogger<A2ADelegationIssuer> _logger;

    /// <summary>
    /// Initializes a new A2A delegation issuer.
    /// </summary>
    /// <param name="issuer">Underlying capability token issuer.</param>
    /// <param name="policyEngine">Policy engine for delegation authorization.</param>
    /// <param name="logger">Optional logger.</param>
    public A2ADelegationIssuer(
        CapabilityTokenIssuer issuer,
        IPolicyEngine policyEngine,
        ILogger<A2ADelegationIssuer>? logger = null)
    {
        _issuer = issuer ?? throw new ArgumentNullException(nameof(issuer));
        _policyEngine = policyEngine ?? throw new ArgumentNullException(nameof(policyEngine));
        _logger = logger ?? NullLogger<A2ADelegationIssuer>.Instance;
    }

    /// <summary>
    /// Delegates a capability to another agent after policy evaluation.
    /// </summary>
    /// <param name="options">Delegation options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The minted delegation token result.</returns>
    /// <exception cref="InvalidOperationException">When delegation is denied by policy.</exception>
    public async Task<CapabilityTokenResult> DelegateAsync(
        A2ADelegationOptions options,
        CancellationToken cancellationToken = default)
    {
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        if (options.Delegation.Depth >= options.Delegation.MaxDepth)
        {
            throw new InvalidOperationException(
                $"Delegation depth {options.Delegation.Depth} already at maximum {options.Delegation.MaxDepth}.");
        }

        var policyResult = await _policyEngine.EvaluateAsync(new PolicyRequest
        {
            AgentId = options.Issuer,
            Tool = options.Capability.Tool,
            Action = options.Capability.Action,
            Context = options.Context,
            DelegationChain = options.Delegation
        }, cancellationToken);

        if (!policyResult.IsPermitted)
        {
            throw new InvalidOperationException(
                $"Delegation denied: {policyResult.DenialReason}");
        }

        var result = _issuer.Mint(new CapabilityTokenOptions
        {
            Issuer = options.Issuer,
            Audience = options.Audience,
            Capability = options.Capability,
            Context = options.Context,
            Lifetime = options.Lifetime,
            Delegation = new DelegationBinding
            {
                ParentTokenId = options.Delegation.ParentTokenId ?? string.Empty,
                Depth = options.Delegation.Depth + 1,
                MaxDepth = options.Delegation.MaxDepth,
                RootIssuer = options.Delegation.DelegatedBy
            }
        });

        _logger.LogDebug("Delegated {Tool}/{Action} from {Issuer} to {Audience} at depth {Depth}",
            options.Capability.Tool,
            options.Capability.Action,
            options.Issuer,
            options.Audience,
            options.Delegation.Depth + 1);

        return result;
    }
}
