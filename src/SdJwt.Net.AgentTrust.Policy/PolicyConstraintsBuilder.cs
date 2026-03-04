using SdJwt.Net.AgentTrust.Core;

namespace SdJwt.Net.AgentTrust.Policy;

/// <summary>
/// Builder for policy constraints.
/// </summary>
public class PolicyConstraintsBuilder
{
    private TimeSpan? _maxTokenLifetime;
    private CapabilityLimits? _limits;
    private readonly List<string> _requiredDisclosures = [];

    /// <summary>
    /// Sets max token lifetime.
    /// </summary>
    public PolicyConstraintsBuilder MaxLifetime(TimeSpan lifetime)
    {
        _maxTokenLifetime = lifetime;
        return this;
    }

    /// <summary>
    /// Sets capability limits.
    /// </summary>
    public PolicyConstraintsBuilder Limits(CapabilityLimits limits)
    {
        _limits = limits;
        return this;
    }

    /// <summary>
    /// Adds disclosure.
    /// </summary>
    public PolicyConstraintsBuilder RequireDisclosure(string claim)
    {
        if (!string.IsNullOrWhiteSpace(claim))
        {
            _requiredDisclosures.Add(claim);
        }

        return this;
    }

    internal PolicyConstraints? Build()
    {
        if (_maxTokenLifetime == null && _limits == null && _requiredDisclosures.Count == 0)
        {
            return null;
        }

        return new PolicyConstraints
        {
            MaxTokenLifetime = _maxTokenLifetime,
            Limits = _limits,
            RequiredDisclosures = _requiredDisclosures.Count == 0 ? null : _requiredDisclosures
        };
    }
}
