using ReleaseSupport.Shared;
using SdJwt.Net.AgentTrust.A2A;
using SdJwt.Net.AgentTrust.Core;
using SdJwt.Net.AgentTrust.Policy;

namespace ReleaseSupport.Coordinator;

/// <summary>
/// Mints scoped delegation tokens for the release investigation tool.
/// Encodes repository, package, and version as a composite resource URI.
/// </summary>
public class DelegatedCapabilityIssuer
{
    private readonly A2ADelegationIssuer _delegationIssuer;

    /// <summary>
    /// Initializes the capability issuer.
    /// </summary>
    public DelegatedCapabilityIssuer(A2ADelegationIssuer delegationIssuer)
    {
        _delegationIssuer = delegationIssuer ?? throw new ArgumentNullException(nameof(delegationIssuer));
    }

    /// <summary>
    /// Mints a delegation token scoped to a specific release investigation.
    /// Resource is encoded as <c>owner/repo|packageId|version</c>.
    /// </summary>
    public async Task<CapabilityTokenResult> MintAsync(
        string coordinatorId,
        string investigatorId,
        string repository,
        string packageId,
        string version,
        string action,
        CancellationToken ct = default)
    {
        var options = new A2ADelegationOptions
        {
            Issuer = coordinatorId,
            Audience = investigatorId,
            Capability = new CapabilityClaim
            {
                Tool = Constants.Tools.ReleaseInvestigation,
                Action = action,
                Resource = $"{repository}|{packageId}|{version}",
                Purpose = "agent-delegation"
            },
            Context = new CapabilityContext
            {
                CorrelationId = Guid.NewGuid().ToString("N"),
                WorkflowId = "release-support",
                TenantId = Constants.Agents.DevTenantId
            },
            Delegation = new DelegationChain
            {
                DelegatedBy = coordinatorId,
                Depth = 0,
                MaxDepth = 1,
                AllowedActions = [Constants.Tools.InvestigateAction]
            },
            Lifetime = Constants.DefaultCapabilityLifetime
        };

        return await _delegationIssuer.DelegateAsync(options, ct);
    }
}
