namespace SdJwt.Net.AgentTrust.Core;

/// <summary>
/// Request to delegate a capability token to a downstream agent.
/// Used by <see cref="ICapabilityAuthority.DelegateAsync"/>.
/// </summary>
public sealed record CapabilityDelegationRequest
{
    /// <summary>
    /// The parent capability token being delegated.
    /// </summary>
    public required string ParentToken
    {
        get; init;
    }

    /// <summary>
    /// Target agent receiving the delegated capability.
    /// </summary>
    public required string DelegateToAgentId
    {
        get; init;
    }

    /// <summary>
    /// Attenuated actions the delegate may perform (must be subset of parent).
    /// </summary>
    public IReadOnlyList<string> AllowedActions { get; init; } = [];

    /// <summary>
    /// Attenuated tools the delegate may access (must be subset of parent).
    /// </summary>
    public IReadOnlyList<string> AllowedTools { get; init; } = [];

    /// <summary>
    /// Optional further-reduced lifetime for the delegated token.
    /// </summary>
    public TimeSpan? MaxLifetime
    {
        get; init;
    }

    /// <summary>
    /// Tenant scope for the delegation.
    /// </summary>
    public string? TenantId
    {
        get; init;
    }
}
