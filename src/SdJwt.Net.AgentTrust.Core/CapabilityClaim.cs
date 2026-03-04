namespace SdJwt.Net.AgentTrust.Core;

/// <summary>
/// Represents the capability scope of an agent trust token.
/// </summary>
public record CapabilityClaim
{
    /// <summary>
    /// Target tool identifier.
    /// </summary>
    public string Tool { get; set; } = string.Empty;

    /// <summary>
    /// Specific action within the tool.
    /// </summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// Optional resource scope.
    /// </summary>
    public string? Resource
    {
        get; set;
    }

    /// <summary>
    /// Optional operation limits.
    /// </summary>
    public CapabilityLimits? Limits
    {
        get; set;
    }

    /// <summary>
    /// Optional purpose.
    /// </summary>
    public string? Purpose
    {
        get; set;
    }
}

