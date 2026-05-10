namespace SdJwt.Net.AgentTrust.Core;

/// <summary>
/// Context for chained audit receipts, linking a receipt to its predecessor.
/// </summary>
public sealed record ReceiptChainContext
{
    /// <summary>
    /// Receipt ID of the previous receipt in the chain.
    /// </summary>
    public required string PreviousReceiptId
    {
        get; init;
    }

    /// <summary>
    /// SHA-256 hash of the previous receipt's signed payload.
    /// </summary>
    public required string PreviousReceiptHash
    {
        get; init;
    }

    /// <summary>
    /// Sequence number within the chain (1-based).
    /// </summary>
    public int SequenceNumber
    {
        get; init;
    }
}
