namespace SdJwt.Net.Wallet.Audit;

/// <summary>
/// Abstraction for recording wallet transaction activity.
/// </summary>
public interface ITransactionLogger
{
    /// <summary>
    /// Writes a transaction log entry.
    /// </summary>
    /// <param name="entry">The transaction log entry.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that completes when logging finishes.</returns>
    Task LogAsync(
        TransactionLog entry,
        CancellationToken cancellationToken = default);
}
