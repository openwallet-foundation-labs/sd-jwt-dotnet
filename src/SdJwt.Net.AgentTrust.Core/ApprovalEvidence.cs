namespace SdJwt.Net.AgentTrust.Core;

/// <summary>
/// Evidence of human-in-the-loop approval attached to a capability mint request.
/// Maps to the <c>approval</c> claim in the capability token.
/// </summary>
public sealed record ApprovalEvidence
{
    /// <summary>
    /// Unique identifier for the approval decision.
    /// </summary>
    public required string ApprovalId
    {
        get; init;
    }

    /// <summary>
    /// Identity of the approver (e.g., "user://manager-456").
    /// </summary>
    public required string ApprovedBy
    {
        get; init;
    }

    /// <summary>
    /// Timestamp when approval was granted.
    /// </summary>
    public required DateTimeOffset ApprovedAt
    {
        get; init;
    }

    /// <summary>
    /// Approval level (e.g., "human-confirmed", "auto-approved").
    /// </summary>
    public string? ApprovalLevel
    {
        get; init;
    }

    /// <summary>
    /// Expiry of the approval grant.
    /// </summary>
    public DateTimeOffset? ExpiresAt
    {
        get; init;
    }

    /// <summary>
    /// SHA-256 hash of the JCS-canonicalized approval request payload.
    /// </summary>
    public string? ApprovedActionHash
    {
        get; init;
    }
}
