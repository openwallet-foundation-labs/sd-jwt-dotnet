namespace SdJwt.Net.AgentTrust.Mcp;

/// <summary>
/// Configuration options for the MCP client trust interceptor.
/// </summary>
public record McpClientTrustOptions
{
    /// <summary>
    /// Agent identifier for token issuance.
    /// </summary>
    public string AgentId { get; set; } = string.Empty;

    /// <summary>
    /// Mapping of tool names to audience URIs.
    /// </summary>
    public IReadOnlyDictionary<string, string> ToolAudienceMapping { get; set; } = new Dictionary<string, string>();

    /// <summary>
    /// Default token lifetime. Defaults to 60 seconds.
    /// </summary>
    public TimeSpan DefaultTokenLifetime { get; set; } = TimeSpan.FromSeconds(60);

    /// <summary>
    /// Token header name for MCP metadata propagation.
    /// </summary>
    public string TokenHeaderName { get; set; } = "X-Agent-Trust-Token";
}
