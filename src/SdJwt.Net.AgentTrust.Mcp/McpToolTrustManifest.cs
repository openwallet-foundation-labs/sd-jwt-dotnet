using System.Text.Json.Serialization;

namespace SdJwt.Net.AgentTrust.Mcp;

/// <summary>
/// Describes trust metadata for an MCP tool, conforming to the
/// MCP tool manifest extension for agent trust.
/// </summary>
public record McpToolTrustManifest
{
    /// <summary>
    /// Tool name as registered in the MCP server.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Audience URI for capability token binding.
    /// </summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// Permitted actions for this tool.
    /// </summary>
    public IReadOnlyList<string> Actions { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Whether this tool requires a capability token.
    /// </summary>
    public bool RequiresCapabilityToken { get; set; } = true;

    /// <summary>
    /// Maximum token lifetime accepted by this tool.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public TimeSpan? MaxTokenLifetime
    {
        get; set;
    }
}
