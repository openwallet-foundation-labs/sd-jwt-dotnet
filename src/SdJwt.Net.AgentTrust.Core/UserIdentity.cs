namespace SdJwt.Net.AgentTrust.Core;

/// <summary>
/// Represents the human user on whose behalf an agent is acting.
/// Maps to the <c>act</c> claim in the capability token.
/// </summary>
public sealed record UserIdentity
{
    /// <summary>
    /// User subject identifier (e.g., "user://12345").
    /// </summary>
    public required string SubjectId
    {
        get; init;
    }

    /// <summary>
    /// Tenant the user belongs to.
    /// </summary>
    public string? TenantId
    {
        get; init;
    }
}
