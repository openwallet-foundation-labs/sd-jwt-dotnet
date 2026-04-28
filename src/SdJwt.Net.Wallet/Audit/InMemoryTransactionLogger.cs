using System.Collections.Concurrent;

namespace SdJwt.Net.Wallet.Audit;

/// <summary>
/// In-memory implementation of <see cref="ITransactionLogger"/> for testing,
/// development, and reference purposes.
/// </summary>
/// <remarks>
/// This implementation stores all log entries in a thread-safe in-memory list.
/// It is not intended for production use where durable storage is required.
/// </remarks>
public class InMemoryTransactionLogger : ITransactionLogger
{
    private readonly ConcurrentBag<TransactionLog> _entries = new();

    /// <inheritdoc/>
    public Task LogAsync(TransactionLog entry, CancellationToken cancellationToken = default)
    {
        if (entry == null)
        {
            throw new ArgumentNullException(nameof(entry));
        }

        _entries.Add(entry);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Gets all recorded log entries.
    /// </summary>
    /// <returns>An unordered snapshot of all entries.</returns>
    public IReadOnlyList<TransactionLog> GetEntries()
    {
        return _entries.ToArray();
    }

    /// <summary>
    /// Gets log entries of type <see cref="CredentialAuditEntry"/>.
    /// </summary>
    /// <returns>Credential audit entries only.</returns>
    public IReadOnlyList<CredentialAuditEntry> GetAuditEntries()
    {
        return _entries.OfType<CredentialAuditEntry>().ToArray();
    }

    /// <summary>
    /// Clears all recorded entries.
    /// </summary>
    public void Clear()
    {
        while (_entries.TryTake(out _))
        {
        }
    }
}
