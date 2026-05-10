namespace SdJwt.Net.AgentTrust.Core;

/// <summary>
/// Request to revoke a token, agent, tool, or other trust entity.
/// </summary>
public sealed record RevocationRequest
{
    /// <summary>
    /// Revocation target check containing the identifiers to revoke.
    /// </summary>
    public required RevocationCheck Target
    {
        get; init;
    }

    /// <summary>
    /// Reason for revocation.
    /// </summary>
    public string? Reason
    {
        get; init;
    }

    /// <summary>
    /// Identity of the entity requesting revocation.
    /// </summary>
    public string? RequestedBy
    {
        get; init;
    }
}
