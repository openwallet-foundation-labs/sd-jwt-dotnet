using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Text.RegularExpressions;

namespace SdJwt.Net.AgentTrust.Policy;

/// <summary>
/// Default rule-based policy engine.
/// </summary>
public class DefaultPolicyEngine : IPolicyEngine
{
    private readonly IReadOnlyList<PolicyRule> _rules;
    private readonly ILogger<DefaultPolicyEngine> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultPolicyEngine"/> class.
    /// </summary>
    public DefaultPolicyEngine(IReadOnlyList<PolicyRule> rules, ILogger<DefaultPolicyEngine>? logger = null)
    {
        _rules = rules?.OrderByDescending(r => r.Priority).ToArray()
            ?? throw new ArgumentNullException(nameof(rules));
        _logger = logger ?? NullLogger<DefaultPolicyEngine>.Instance;
    }

    /// <inheritdoc/>
    public Task<PolicyDecision> EvaluateAsync(PolicyRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (request.DelegationChain != null)
        {
            if (request.DelegationChain.Depth > request.DelegationChain.MaxDepth)
            {
                return Task.FromResult(PolicyDecision.Deny("Delegation depth exceeded.", "delegation_depth_exceeded"));
            }

            if (request.DelegationChain.AllowedActions != null && request.DelegationChain.AllowedActions.Count > 0)
            {
                var allowed = request.DelegationChain.AllowedActions.Any(a => string.Equals(a, request.Action, StringComparison.OrdinalIgnoreCase));
                if (!allowed)
                {
                    return Task.FromResult(PolicyDecision.Deny("Action not allowed by delegation.", "delegation_action_denied"));
                }
            }
        }

        foreach (var rule in _rules)
        {
            if (Matches(rule.AgentPattern, request.AgentId) &&
                Matches(rule.ToolPattern, request.Tool) &&
                Matches(rule.ActionPattern, request.Action) &&
                (rule.ResourcePattern == null || Matches(rule.ResourcePattern, request.Resource ?? string.Empty)))
            {
                _logger.LogDebug("Policy rule matched: {RuleName}", rule.Name);

                if (rule.Effect == PolicyEffect.Deny)
                {
                    return Task.FromResult(PolicyDecision.Deny($"Denied by rule '{rule.Name}'.", "policy_denied"));
                }

                return Task.FromResult(PolicyDecision.Permit(rule.Constraints));
            }
        }

        return Task.FromResult(PolicyDecision.Deny("No matching allow rule.", "policy_no_match"));
    }

    private static bool Matches(string pattern, string value)
    {
        if (string.Equals(pattern, "*", StringComparison.Ordinal))
        {
            return true;
        }

        var regex = "^" + Regex.Escape(pattern).Replace("\\*", ".*") + "$";
        return Regex.IsMatch(value, regex, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
    }
}

