using SdJwt.Net.AgentTrust.Core;

namespace SdJwt.Net.AgentTrust.Policy;

/// <summary>
/// Builder for policy rules.
/// </summary>
public class PolicyBuilder
{
    private readonly List<PolicyRule> _rules = [];

    /// <summary>
    /// Adds allow rule.
    /// </summary>
    public PolicyBuilder Allow(
        string agentPattern,
        string toolPattern,
        string actionPattern,
        Action<PolicyConstraintsBuilder>? constraints = null)
    {
        var builder = new PolicyConstraintsBuilder();
        constraints?.Invoke(builder);

        _rules.Add(new PolicyRule
        {
            Name = $"allow:{agentPattern}:{toolPattern}:{actionPattern}",
            AgentPattern = agentPattern,
            ToolPattern = toolPattern,
            ActionPattern = actionPattern,
            Effect = PolicyEffect.Allow,
            Constraints = builder.Build(),
            Priority = 0
        });

        return this;
    }

    /// <summary>
    /// Adds deny rule.
    /// </summary>
    public PolicyBuilder Deny(string agentPattern, string toolPattern, string actionPattern)
    {
        _rules.Add(new PolicyRule
        {
            Name = $"deny:{agentPattern}:{toolPattern}:{actionPattern}",
            AgentPattern = agentPattern,
            ToolPattern = toolPattern,
            ActionPattern = actionPattern,
            Effect = PolicyEffect.Deny,
            Priority = 100
        });

        return this;
    }

    /// <summary>
    /// Adds delegation allow rule.
    /// </summary>
    public PolicyBuilder AllowDelegation(
        string fromAgent,
        string toAgent,
        int maxDepth = 1,
        Action<DelegationConstraintsBuilder>? constraints = null)
    {
        var delegationBuilder = new DelegationConstraintsBuilder(maxDepth);
        constraints?.Invoke(delegationBuilder);
        var allowedActions = delegationBuilder.AllowedActions;

        _rules.Add(new PolicyRule
        {
            Name = $"allow-delegation:{fromAgent}:{toAgent}",
            AgentPattern = toAgent,
            ToolPattern = "*",
            ActionPattern = "*",
            Effect = PolicyEffect.Allow,
            Priority = 5,
            Constraints = new PolicyConstraints
            {
                RequiredDisclosures = ["ctx.correlationId"],
                Limits = null,
                MaxTokenLifetime = TimeSpan.FromSeconds(60)
            }
        });

        if (allowedActions.Count > 0)
        {
            _rules.Add(new PolicyRule
            {
                Name = $"deny-delegation-action:{fromAgent}:{toAgent}",
                AgentPattern = toAgent,
                ToolPattern = "*",
                ActionPattern = "*",
                Effect = PolicyEffect.Deny,
                Priority = -1
            });
        }

        return this;
    }

    /// <summary>
    /// Builds rules.
    /// </summary>
    public IReadOnlyList<PolicyRule> Build()
    {
        return _rules
            .OrderByDescending(r => r.Priority)
            .ToArray();
    }
}


