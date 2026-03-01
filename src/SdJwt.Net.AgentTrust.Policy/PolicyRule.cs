namespace SdJwt.Net.AgentTrust.Policy;

/// <summary>
/// Rule definition.
/// </summary>
public record PolicyRule
{
    /// <summary>
    /// Rule name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Agent wildcard pattern.
    /// </summary>
    public string AgentPattern { get; set; } = string.Empty;

    /// <summary>
    /// Tool wildcard pattern.
    /// </summary>
    public string ToolPattern { get; set; } = string.Empty;

    /// <summary>
    /// Action wildcard pattern.
    /// </summary>
    public string ActionPattern { get; set; } = string.Empty;

    /// <summary>
    /// Optional resource wildcard pattern.
    /// </summary>
    public string? ResourcePattern
    {
        get; set;
    }

    /// <summary>
    /// Rule effect.
    /// </summary>
    public PolicyEffect Effect
    {
        get; set;
    }

    /// <summary>
    /// Constraints for allow rules.
    /// </summary>
    public PolicyConstraints? Constraints
    {
        get; set;
    }

    /// <summary>
    /// Rule priority (higher first).
    /// </summary>
    public int Priority
    {
        get; set;
    }
}

