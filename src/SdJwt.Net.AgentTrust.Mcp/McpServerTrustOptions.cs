using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.AgentTrust.Core;

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

    /// <summary>
    /// Security mode for verification strictness.
    /// </summary>
    public AgentTrustSecurityMode SecurityMode { get; set; } = AgentTrustSecurityMode.Pilot;

    /// <summary>
    /// Maximum allowed token lifetime. Tokens with longer exp-iat are rejected.
    /// </summary>
    public TimeSpan MaxTokenLifetime { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Algorithms allowed for token signatures. Null allows any.
    /// </summary>
    public IReadOnlyCollection<string>? AllowedAlgorithms
    {
        get; set;
    }

    /// <summary>
    /// Whether to require tool manifest binding on tokens.
    /// </summary>
    public bool RequireToolManifestBinding
    {
        get; set;
    }

    /// <summary>
    /// Whether to enforce replay prevention.
    /// </summary>
    public bool EnforceReplayPrevention { get; set; } = true;
}
