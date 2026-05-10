namespace SdJwt.Net.AgentTrust.Core;

/// <summary>
/// Token type constants for the Agent Trust capability profile.
/// </summary>
public static class AgentTrustTokenTypes
{
    /// <summary>
    /// SD-JWT capability token type for runtime agent trust tokens.
    /// </summary>
    public const string CapabilitySdJwt = "agent-cap+sd-jwt";

    /// <summary>
    /// Content type for agent capability payloads.
    /// </summary>
    public const string CapabilityContentType = "application/agent-capability+json";

    /// <summary>
    /// Legacy SD-JWT VC type retained for backward compatibility with existing tokens.
    /// New tokens should use <see cref="CapabilitySdJwt"/>.
    /// </summary>
    public const string LegacySdJwtVc = "vc+sd-jwt";

    /// <summary>
    /// Demo capability token type with relaxed validation. Only valid in Demo mode.
    /// </summary>
    public const string DemoCapabilitySdJwt = "agent-cap+sd-jwt+demo";

    /// <summary>
    /// Capability template token type for pre-approved token patterns.
    /// </summary>
    public const string CapabilityTemplate = "agent-cap-template+jwt";
}
