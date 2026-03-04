namespace SdJwt.Net.AgentTrust.Core;

/// <summary>
/// Writes audit receipts.
/// </summary>
public interface IReceiptWriter
{
    /// <summary>
    /// Writes a receipt.
    /// </summary>
    Task WriteAsync(AuditReceipt receipt, CancellationToken cancellationToken = default);
}

