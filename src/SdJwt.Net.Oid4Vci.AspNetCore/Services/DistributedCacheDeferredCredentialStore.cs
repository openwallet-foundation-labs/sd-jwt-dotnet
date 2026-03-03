using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using SdJwt.Net.Oid4Vci.Models;
using System.Text.Json;

namespace SdJwt.Net.Oid4Vci.AspNetCore.Services;

/// <summary>
/// <see cref="IDeferredCredentialStore"/> implementation backed by <see cref="IDistributedCache"/>.
/// Suitable for multi-node production deployments.
/// </summary>
public sealed class DistributedCacheDeferredCredentialStore : IDeferredCredentialStore
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly IDistributedCache _cache;
    private readonly ILogger<DistributedCacheDeferredCredentialStore> _logger;
    private readonly int _deferredEntryTtlSeconds;

    /// <summary>
    /// Initializes a new instance of <see cref="DistributedCacheDeferredCredentialStore"/>.
    /// </summary>
    /// <param name="cache">The distributed cache.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="deferredEntryTtlSeconds">
    /// How long (seconds) a deferred entry may remain unredeemed. Defaults to 3600 (1 hour).
    /// </param>
    public DistributedCacheDeferredCredentialStore(
        IDistributedCache cache,
        ILogger<DistributedCacheDeferredCredentialStore> logger,
        int deferredEntryTtlSeconds = 3600)
    {
        ArgumentNullException.ThrowIfNull(cache);
        ArgumentNullException.ThrowIfNull(logger);

        _cache = cache;
        _logger = logger;
        _deferredEntryTtlSeconds = deferredEntryTtlSeconds;
    }

    /// <inheritdoc/>
    public async Task SaveAsync(
        string transactionId,
        CredentialRequest request,
        string accessToken,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(transactionId);
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(accessToken);

        var entry = new DeferredEntry(request, accessToken);
        await _cache.SetAsync(CacheKey(transactionId), Serialize(entry), new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(_deferredEntryTtlSeconds)
        }, cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Stored deferred credential transaction. TransactionId={TransactionId}", Truncate(transactionId));
    }

    /// <inheritdoc/>
    public async Task<(CredentialRequest Request, string AccessToken)?> RetrieveAsync(
        string transactionId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(transactionId);

        var bytes = await _cache.GetAsync(CacheKey(transactionId), cancellationToken).ConfigureAwait(false);
        if (bytes is null)
        {
            _logger.LogWarning("Deferred credential transaction not found or already redeemed. TransactionId={TransactionId}", Truncate(transactionId));
            return null;
        }

        var entry = JsonSerializer.Deserialize<DeferredEntry>(bytes, SerializerOptions);
        if (entry is null)
        {
            _logger.LogError("Failed to deserialize deferred credential entry. TransactionId={TransactionId}", Truncate(transactionId));
            return null;
        }

        // Remove immediately (consume-on-read semantics)
        await _cache.RemoveAsync(CacheKey(transactionId), cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Retrieved and removed deferred credential transaction. TransactionId={TransactionId}", Truncate(transactionId));
        return (entry.Request, entry.AccessToken);
    }

    private static string CacheKey(string transactionId) => $"oid4vci:deferred:{transactionId}";

    private static byte[] Serialize<T>(T value) =>
        JsonSerializer.SerializeToUtf8Bytes(value, SerializerOptions);

    private static string Truncate(string value) =>
        value.Length > 8 ? value[..8] + "..." : value;

    private sealed record DeferredEntry(CredentialRequest Request, string AccessToken);
}
