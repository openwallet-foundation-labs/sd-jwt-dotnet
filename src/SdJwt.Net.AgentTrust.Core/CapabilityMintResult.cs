namespace SdJwt.Net.AgentTrust.Core;

/// <summary>
/// Result from the Capability Authority after minting a capability token.
/// </summary>
public record CapabilityMintResult
{
    /// <summary>
    /// The minted SD-JWT capability token.
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Token identifier (jti claim).
    /// </summary>
    public string TokenId { get; set; } = string.Empty;

    /// <summary>
    /// Token expiry.
    /// </summary>
    public DateTimeOffset ExpiresAt
    {
        get; set;
    }

    /// <summary>
    /// Policy decision identifier that authorized the minting.
    /// </summary>
    public required string DecisionId
    {
        get; init;
    }

    /// <summary>
    /// Signed decision receipt for audit purposes.
    /// </summary>
    public required SignedAuditReceipt DecisionReceipt
    {
        get; init;
    }

    /// <summary>
    /// Policy obligations that MUST be enforced during execution.
    /// </summary>
    public IReadOnlyList<PolicyObligation> Obligations { get; init; } = [];
}
