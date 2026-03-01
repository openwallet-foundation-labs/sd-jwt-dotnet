namespace SdJwt.Net.Wallet.Audit;

/// <summary>
/// Transaction execution status.
/// </summary>
public enum TransactionStatus
{
    /// <summary>
    /// Transaction completed successfully.
    /// </summary>
    Completed,

    /// <summary>
    /// Transaction completed with errors.
    /// </summary>
    Error
}
