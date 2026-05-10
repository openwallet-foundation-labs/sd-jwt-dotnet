namespace SdJwt.Net.AgentTrust.Core;

/// <summary>
/// Envelope for an MCP tool call, used for canonical hash computation of MCP arguments.
/// </summary>
public sealed record McpToolCallEnvelope
{
    /// <summary>
    /// JSON-RPC method (e.g., "tools/call").
    /// </summary>
    public required string JsonRpcMethod
    {
        get; init;
    }

    /// <summary>
    /// MCP tool identifier.
    /// </summary>
    public required string McpToolId
    {
        get; init;
    }

    /// <summary>
    /// Raw MCP tool call arguments as a JSON string.
    /// </summary>
    public string? ArgumentsJson
    {
        get; init;
    }

    /// <summary>
    /// SHA-256 hash of the MCP tool JSON schema.
    /// </summary>
    public string? McpToolSchemaHash
    {
        get; init;
    }
}
