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

    /// <summary>
    /// Canonical tool identifier for tool-registry binding.
    /// </summary>
    public string? ToolId
    {
        get; set;
    }

    /// <summary>
    /// Semantic version of the tool for version-pinned grants.
    /// </summary>
    public string? ToolVersion
    {
        get; set;
    }

    /// <summary>
    /// SHA-256 hash of the tool manifest for tamper detection.
    /// </summary>
    public string? ToolManifestHash
    {
        get; set;
    }

    /// <summary>
    /// Resource type classification (e.g., "pii", "financial").
    /// </summary>
    public string? ResourceType
    {
        get; set;
    }

    /// <summary>
    /// Data classification level for governance.
    /// </summary>
    public string? DataClassification
    {
        get; set;
    }
}

