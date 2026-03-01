namespace SdJwt.Net.AgentTrust.Core;

/// <summary>
/// Store for token IDs to prevent replay.
/// </summary>
public interface INonceStore
{
    /// <summary>
    /// Marks token ID as used.
    /// </summary>
    Task<bool> TryMarkAsUsedAsync(string tokenId, DateTimeOffset expiry, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if token ID is used.
    /// </summary>
    Task<bool> IsUsedAsync(string tokenId, CancellationToken cancellationToken = default);
}

