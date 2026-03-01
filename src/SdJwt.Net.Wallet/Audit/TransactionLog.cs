namespace SdJwt.Net.Wallet.Audit;

/// <summary>
/// Transaction log entry for wallet operations.
/// </summary>
public class TransactionLog
{
    /// <summary>
    /// Timestamp in UTC when the entry was created.
    /// </summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Transaction category.
    /// </summary>
    public TransactionType Type
    {
        get; set;
    }

    /// <summary>
    /// Transaction status.
    /// </summary>
    public TransactionStatus Status
    {
        get; set;
    }

    /// <summary>
    /// Operation identifier (for example, AcceptCredentialOffer).
    /// </summary>
    public string Operation { get; set; } = string.Empty;

    /// <summary>
    /// Optional human-readable error information.
    /// </summary>
    public string? Error
    {
        get; set;
    }

    /// <summary>
    /// Optional transaction metadata.
    /// </summary>
    public IDictionary<string, object>? Metadata
    {
        get; set;
    }
}
