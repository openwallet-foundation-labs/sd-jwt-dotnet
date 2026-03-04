using Microsoft.Extensions.Logging;
using SdJwt.Net.Oid4Vci.Models;
using System.Collections.Concurrent;

namespace SdJwt.Net.Oid4Vci.AspNetCore.Services;

/// <summary>
/// In-memory implementation of <see cref="IDeferredCredentialStore"/>.
/// Suitable for single-node development scenarios.
/// For production, use <see cref="DistributedCacheDeferredCredentialStore"/>.
/// </summary>
public sealed class InMemoryDeferredCredentialStore : IDeferredCredentialStore
{
    private sealed record DeferredEntry(CredentialRequest Request, string AccessToken);

    private readonly ConcurrentDictionary<string, DeferredEntry> _entries = new(StringComparer.Ordinal);
    private readonly ILogger<InMemoryDeferredCredentialStore> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="InMemoryDeferredCredentialStore"/>.
    /// </summary>
    /// <param name="logger">The logger.</param>
    public InMemoryDeferredCredentialStore(ILogger<InMemoryDeferredCredentialStore> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;
    }

    /// <summary>
    /// Convenience constructor for unit testing. Uses <see cref="Microsoft.Extensions.Logging.Abstractions.NullLogger{T}"/>.
    /// </summary>
    public InMemoryDeferredCredentialStore()
        : this(Microsoft.Extensions.Logging.Abstractions.NullLogger<InMemoryDeferredCredentialStore>.Instance)
    {
    }

    /// <inheritdoc/>
    public Task SaveAsync(
        string transactionId,
        CredentialRequest request,
        string accessToken,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(transactionId);
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(accessToken);

        _entries[transactionId] = new DeferredEntry(request, accessToken);

        _logger.LogInformation("Stored deferred credential transaction. TransactionId={TransactionId}", Truncate(transactionId));

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task<(CredentialRequest Request, string AccessToken)?> RetrieveAsync(
        string transactionId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(transactionId);

        if (_entries.TryRemove(transactionId, out var entry))
        {
            _logger.LogInformation("Retrieved and removed deferred credential transaction. TransactionId={TransactionId}", Truncate(transactionId));
            return Task.FromResult<(CredentialRequest, string)?>((entry.Request, entry.AccessToken));
        }

        _logger.LogWarning("Deferred credential transaction not found or already redeemed. TransactionId={TransactionId}", Truncate(transactionId));
        return Task.FromResult<(CredentialRequest, string)?>(null);
    }

    private static string Truncate(string value) =>
        value.Length > 8 ? value[..8] + "..." : value;
}
