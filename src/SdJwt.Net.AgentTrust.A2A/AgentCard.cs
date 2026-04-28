namespace SdJwt.Net.AgentTrust.A2A;

/// <summary>
/// Describes an agent's identity and capabilities for discovery by other agents.
/// Modeled after the A2A Agent Card specification.
/// </summary>
public record AgentCard
{
    /// <summary>
    /// Unique agent identifier (e.g., <c>agent://alpha</c>).
    /// </summary>
    public string AgentId { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable agent name.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Agent description.
    /// </summary>
    public string? Description
    {
        get; set;
    }

    /// <summary>
    /// Agent version.
    /// </summary>
    public string? Version
    {
        get; set;
    }

    /// <summary>
    /// Capability URIs this agent can perform (e.g., <c>Weather.Read</c>).
    /// </summary>
    public IReadOnlyList<string> Capabilities { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Endpoint for trust metadata and JWKS discovery.
    /// </summary>
    public string? TrustEndpoint
    {
        get; set;
    }

    /// <summary>
    /// JWKS URI for verifying tokens issued by this agent.
    /// </summary>
    public string? JwksUri
    {
        get; set;
    }

    /// <summary>
    /// Maximum delegation depth this agent accepts.
    /// </summary>
    public int MaxDelegationDepth { get; set; } = 3;

    /// <summary>
    /// Whether this agent supports receiving delegation tokens.
    /// </summary>
    public bool SupportsDelegation { get; set; } = true;

    /// <summary>
    /// Tenant identifier for multi-tenant environments.
    /// </summary>
    public string? TenantId
    {
        get; set;
    }
}
