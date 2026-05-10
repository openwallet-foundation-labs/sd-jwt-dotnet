namespace SdJwt.Net.AgentTrust.Core;

/// <summary>
/// Evidence of a delegation chain attached to a capability mint request.
/// Distinct from <see cref="DelegationBinding"/> which is the in-token claim binding;
/// this record carries the evidence the Trust Broker needs to validate delegation authority.
/// </summary>
public sealed record DelegationEvidence
{
    /// <summary>
    /// Token ID of the parent capability token.
    /// </summary>
    public required string ParentTokenId
    {
        get; init;
    }

    /// <summary>
    /// SHA-256 hash of the parent capability token.
    /// </summary>
    public required string ParentTokenHash
    {
        get; init;
    }

    /// <summary>
    /// Current delegation depth.
    /// </summary>
    public int Depth
    {
        get; init;
    }

    /// <summary>
    /// Maximum allowed delegation depth.
    /// </summary>
    public int MaxDepth { get; init; } = 3;

    /// <summary>
    /// Issuer of the root capability token in the chain.
    /// </summary>
    public string? RootIssuer
    {
        get; init;
    }

    /// <summary>
    /// Receipt ID from the parent token's mint decision.
    /// </summary>
    public string? ParentReceiptId
    {
        get; init;
    }
}
