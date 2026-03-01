using System.Collections.Concurrent;

namespace SdJwt.Net.AgentTrust.Core;

/// <summary>
/// In-memory token replay store.
/// </summary>
public class MemoryNonceStore : INonceStore
{
    private readonly ConcurrentDictionary<string, DateTimeOffset> _entries = new(StringComparer.Ordinal);

    /// <summary>
    /// Initializes store.
    /// </summary>
    public MemoryNonceStore()
    {
    }

    /// <inheritdoc/>
    public Task<bool> TryMarkAsUsedAsync(string tokenId, DateTimeOffset expiry, CancellationToken cancellationToken = default)
    {
        CleanupExpired();
        return Task.FromResult(_entries.TryAdd(tokenId, expiry));
    }

    /// <inheritdoc/>
    public Task<bool> IsUsedAsync(string tokenId, CancellationToken cancellationToken = default)
    {
        CleanupExpired();
        return Task.FromResult(_entries.ContainsKey(tokenId));
    }

    private void CleanupExpired()
    {
        var now = DateTimeOffset.UtcNow;
        foreach (var item in _entries)
        {
            if (item.Value <= now)
            {
                _entries.TryRemove(item.Key, out _);
            }
        }
    }
}

