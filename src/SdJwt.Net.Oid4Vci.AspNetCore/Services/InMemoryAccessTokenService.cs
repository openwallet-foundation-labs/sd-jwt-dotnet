using Microsoft.Extensions.Logging;
using SdJwt.Net.Oid4Vci.Issuer;
using SdJwt.Net.Oid4Vci.Models;
using System.Collections.Concurrent;
using System.Security.Cryptography;

namespace SdJwt.Net.Oid4Vci.AspNetCore.Services;

/// <summary>
/// In-memory implementation of <see cref="IAccessTokenService"/>.
/// Suitable for single-node development and test scenarios.
/// For production distributed deployments, use <see cref="DistributedCacheAccessTokenService"/>.
/// </summary>
public sealed class InMemoryAccessTokenService : IAccessTokenService
{
    private sealed record PreAuthorizedCodeEntry(
        string PreAuthorizedCode,
        string? TransactionCode,
        IReadOnlyList<string> AuthorizedConfigurationIds,
        bool IsUsed);

    private sealed record ActiveToken(
        string Token,
        string CNonce,
        DateTimeOffset ExpiresAt,
        DateTimeOffset CNonceExpiresAt,
        IReadOnlyList<string> AuthorizedConfigurationIds);

    private readonly ConcurrentDictionary<string, PreAuthorizedCodeEntry> _codes = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, ActiveToken> _tokens = new(StringComparer.Ordinal);
    private readonly int _accessTokenLifetimeSeconds;
    private readonly int _cNonceLifetimeSeconds;
    private readonly ILogger<InMemoryAccessTokenService> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="InMemoryAccessTokenService"/>.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="accessTokenLifetimeSeconds">Access token lifetime in seconds.</param>
    /// <param name="cNonceLifetimeSeconds">c_nonce lifetime in seconds.</param>
    public InMemoryAccessTokenService(
        ILogger<InMemoryAccessTokenService> logger,
        int accessTokenLifetimeSeconds = 300,
        int cNonceLifetimeSeconds = 300)
    {
        ArgumentNullException.ThrowIfNull(logger);

        _logger = logger;
        _accessTokenLifetimeSeconds = accessTokenLifetimeSeconds;
        _cNonceLifetimeSeconds = cNonceLifetimeSeconds;
    }

    /// <summary>
    /// Convenience constructor for unit testing. Uses <see cref="Microsoft.Extensions.Logging.Abstractions.NullLogger{T}"/>.
    /// </summary>
    /// <param name="accessTokenLifetimeSeconds">Access token lifetime in seconds.</param>
    /// <param name="cNonceLifetimeSeconds">c_nonce lifetime in seconds.</param>
    public InMemoryAccessTokenService(
        int accessTokenLifetimeSeconds = 300,
        int cNonceLifetimeSeconds = 300)
        : this(Microsoft.Extensions.Logging.Abstractions.NullLogger<InMemoryAccessTokenService>.Instance,
               accessTokenLifetimeSeconds,
               cNonceLifetimeSeconds)
    {
    }

    /// <summary>
    /// Registers a pre-authorized code so it may be exchanged for an access token.
    /// </summary>
    /// <param name="code">The pre-authorized code.</param>
    /// <param name="transactionCode">Optional transaction code (PIN) required at exchange time.</param>
    /// <param name="authorizedConfigurationIds">Credential configuration IDs this code authorizes.</param>
    public void RegisterPreAuthorizedCode(
        string code,
        string? transactionCode,
        IReadOnlyList<string> authorizedConfigurationIds)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentNullException.ThrowIfNull(authorizedConfigurationIds);

        _codes[code] = new PreAuthorizedCodeEntry(code, transactionCode, authorizedConfigurationIds, IsUsed: false);

        _logger.LogInformation(
            "Registered pre-authorized code. Configurations={Configs} RequiresPin={RequiresPin}",
            string.Join(",", authorizedConfigurationIds),
            transactionCode != null);
    }

    /// <inheritdoc/>
    public Task<IssuedAccessToken?> IssueForPreAuthorizedCodeAsync(
        string preAuthorizedCode,
        string? transactionCode,
        CancellationToken cancellationToken = default)
    {
        if (!_codes.TryGetValue(preAuthorizedCode, out var entry) || entry.IsUsed)
        {
            _logger.LogWarning("Pre-authorized code not found or already used. Code={CodePrefix}", Truncate(preAuthorizedCode));
            return Task.FromResult<IssuedAccessToken?>(null);
        }

        if (entry.TransactionCode != null &&
            !string.Equals(entry.TransactionCode, transactionCode, StringComparison.Ordinal))
        {
            _logger.LogWarning("Invalid transaction code supplied. Code={CodePrefix}", Truncate(preAuthorizedCode));
            return Task.FromResult<IssuedAccessToken?>(null);
        }

        _codes[preAuthorizedCode] = entry with { IsUsed = true };

        var token = GenerateSecureToken();
        var cNonce = CNonceValidator.GenerateNonce();
        var now = DateTimeOffset.UtcNow;

        var activeToken = new ActiveToken(
            token,
            cNonce,
            now.AddSeconds(_accessTokenLifetimeSeconds),
            now.AddSeconds(_cNonceLifetimeSeconds),
            entry.AuthorizedConfigurationIds);

        _tokens[token] = activeToken;

        _logger.LogInformation(
            "Issued access token for pre-authorized code. Configurations={Configs} ExpiresIn={ExpiresIn}s",
            string.Join(",", entry.AuthorizedConfigurationIds),
            _accessTokenLifetimeSeconds);

        return Task.FromResult<IssuedAccessToken?>(new IssuedAccessToken(
            token,
            cNonce,
            _accessTokenLifetimeSeconds,
            _cNonceLifetimeSeconds,
            entry.AuthorizedConfigurationIds));
    }

    /// <inheritdoc/>
    public Task<IssuedAccessToken?> ValidateAsync(
        string accessToken,
        CancellationToken cancellationToken = default)
    {
        if (!_tokens.TryGetValue(accessToken, out var token))
        {
            _logger.LogDebug("Access token not found in store.");
            return Task.FromResult<IssuedAccessToken?>(null);
        }

        var now = DateTimeOffset.UtcNow;
        if (token.ExpiresAt < now)
        {
            _tokens.TryRemove(accessToken, out _);
            _logger.LogWarning("Rejected expired access token. ExpiredAt={ExpiredAt:u}", token.ExpiresAt);
            return Task.FromResult<IssuedAccessToken?>(null);
        }

        var remainingTokenSeconds = (int)(token.ExpiresAt - now).TotalSeconds;
        var remainingNonceSeconds = token.CNonceExpiresAt < now
            ? 0
            : (int)(token.CNonceExpiresAt - now).TotalSeconds;

        return Task.FromResult<IssuedAccessToken?>(new IssuedAccessToken(
            token.Token,
            token.CNonce,
            remainingTokenSeconds,
            remainingNonceSeconds,
            token.AuthorizedConfigurationIds));
    }

    /// <summary>
    /// Purges all expired access tokens from the in-memory store.
    /// Called by <see cref="InMemoryTokenCleanupService"/> on a configurable interval.
    /// </summary>
    /// <returns>The number of entries purged.</returns>
    internal int PurgeExpired()
    {
        var now = DateTimeOffset.UtcNow;
        var removed = 0;

        foreach (var kvp in _tokens)
        {
            if (kvp.Value.ExpiresAt < now && _tokens.TryRemove(kvp.Key, out _))
            {
                removed++;
            }
        }

        return removed;
    }

    private static string GenerateSecureToken()
    {
        var bytes = new byte[32];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    private static string Truncate(string value) =>
        value.Length > 8 ? value[..8] + "..." : value;
}
