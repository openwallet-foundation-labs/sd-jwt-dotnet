namespace SdJwt.Net.AgentTrust.Core;

/// <summary>
/// Signs audit receipts for tamper-evident audit trails.
/// </summary>
public interface IReceiptSigner
{
    /// <summary>
    /// Signs an audit receipt and returns the signed envelope.
    /// </summary>
    /// <param name="receipt">The audit receipt to sign.</param>
    /// <param name="previousReceiptHash">Optional hash of the previous receipt for chaining.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The signed audit receipt.</returns>
    Task<SignedAuditReceipt> SignAsync(
        AuditReceipt receipt,
        string? previousReceiptHash = null,
        CancellationToken cancellationToken = default);
}
