namespace SdJwt.Net.AgentTrust.Policy;

/// <summary>
/// Policy constraints for permitted actions.
/// </summary>
public record PolicyConstraints
{
    /// <summary>
    /// Maximum token lifetime.
    /// </summary>
    public TimeSpan? MaxTokenLifetime
    {
        get; set;
    }

    /// <summary>
    /// Embedded capability limits.
    /// </summary>
    public SdJwt.Net.AgentTrust.Core.CapabilityLimits? Limits
    {
        get; set;
    }

    /// <summary>
    /// disclosure claim names.
    /// </summary>
    public IReadOnlyList<string>? RequiredDisclosures
    {
        get; set;
    }
}

