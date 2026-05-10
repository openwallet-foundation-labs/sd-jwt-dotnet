using SdJwt.Net.AgentTrust.Core;

namespace SdJwt.Net.AgentTrust.Mcp;

/// <summary>
/// Represents an MCP tool call that needs trust token propagation.
/// </summary>
public record McpToolCall
{
    /// <summary>
    /// Tool name being called.
    /// </summary>
    public string ToolName { get; set; } = string.Empty;

    /// <summary>
    /// Action to perform (derived from arguments or method).
    /// </summary>
    public string Action { get; set; } = "Read";

    /// <summary>
    /// Tool call arguments.
    /// </summary>
    public IReadOnlyDictionary<string, object>? Arguments
    {
        get; set;
    }

    /// <summary>
    /// Capability context for correlation and tracing.
    /// </summary>
    public CapabilityContext? Context
    {
        get; set;
    }

    /// <summary>
    /// Optional request binding to bind the token to this specific tool invocation.
    /// </summary>
    public RequestBinding? RequestBinding
    {
        get; set;
    }
}
