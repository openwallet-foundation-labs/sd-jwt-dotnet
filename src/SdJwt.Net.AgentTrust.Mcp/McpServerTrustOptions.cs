using Microsoft.IdentityModel.Tokens;

namespace SdJwt.Net.AgentTrust.Mcp;

/// <summary>
/// Configuration options for the MCP server trust guard.
/// </summary>
public record McpServerTrustOptions
{
    /// <summary>
    /// Audience URI that this MCP server expects in tokens.
    /// </summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// Trusted issuer-to-key mappings for token verification.
    /// </summary>
    public IReadOnlyDictionary<string, SecurityKey> TrustedIssuers { get; set; } = new Dictionary<string, SecurityKey>();
}
