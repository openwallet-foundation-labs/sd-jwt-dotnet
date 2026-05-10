namespace SdJwt.Net.AgentTrust.Core;

/// <summary>
/// Request to introspect a capability token's current status and metadata.
/// Used by <see cref="ICapabilityAuthority.IntrospectAsync"/>.
/// </summary>
public sealed record CapabilityIntrospectionRequest
{
    /// <summary>
    /// The capability token to introspect.
    /// </summary>
    public required string Token
    {
        get; init;
    }

    /// <summary>
    /// Whether to include detailed claims in the introspection response.
    /// </summary>
    public bool IncludeClaims
    {
        get; init;
    }
}
