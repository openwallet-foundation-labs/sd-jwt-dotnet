using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using SdJwt.Net.Oid4Vci.Issuer;
using System.Security.Cryptography;
using System.Text.Json;

namespace SdJwt.Net.Oid4Vci.AspNetCore.Services;

/// <summary>
/// <see cref="IAccessTokenService"/> implementation backed by <see cref="IDistributedCache"/>.
/// Suitable for multi-node production deployments (Redis, SQL Server, Azure Cache for Redis, etc.).
/// Token entries are stored as JSON with sliding expiry driven by <c>AccessTokenLifetimeSeconds</c>.
/// </summary>
public sealed class DistributedCacheAccessTokenService : IAccessTokenService
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly IDistributedCache _cache;
    private readonly ILogger<DistributedCacheAccessTokenService> _logger;
    private readonly int _accessTokenLifetimeSeconds;
    private readonly int _cNonceLifetimeSeconds;

    /// <summary>
    /// Initializes a new instance of <see cref="DistributedCacheAccessTokenService"/>.
    /// </summary>
    public DistributedCacheAccessTokenService(
        IDistributedCache cache,
        ILogger<DistributedCacheAccessTokenService> logger,
        int accessTokenLifetimeSeconds = 300,
        int cNonceLifetimeSeconds = 300)
    {
        ArgumentNullException.ThrowIfNull(cache);
        ArgumentNullException.ThrowIfNull(logger);

        _cache = cache;
        _logger = logger;
        _accessTokenLifetimeSeconds = accessTokenLifetimeSeconds;
        _cNonceLifetimeSeconds = cNonceLifetimeSeconds;
    }

    /// <inheritdoc/>
    public async Task<IssuedAccessToken?> IssueForPreAuthorizedCodeAsync(
        string preAuthorizedCode,
        string? transactionCode,
        CancellationToken cancellationToken = default)
    {
        var codeKey = CodeCacheKey(preAuthorizedCode);
        var entryBytes = await _cache.GetAsync(codeKey, cancellationToken).ConfigureAwait(false);

        if (entryBytes is null)
        {
            _logger.LogWarning("Pre-authorized code not found or expired. Code={CodePrefix}", Truncate(preAuthorizedCode));
            return null;
        }

        var entry = JsonSerializer.Deserialize<PreAuthorizedCodeEntry>(entryBytes, SerializerOptions);
        if (entry is null || entry.IsUsed)
        {
            _logger.LogWarning("Pre-authorized code already used. Code={CodePrefix}", Truncate(preAuthorizedCode));
            return null;
        }

        if (entry.TransactionCode != null &&
            !string.Equals(entry.TransactionCode, transactionCode, StringComparison.Ordinal))
        {
            _logger.LogWarning("Invalid transaction code supplied for pre-authorized code. Code={CodePrefix}", Truncate(preAuthorizedCode));
            return null;
        }

        // Mark as used — small expiry so the entry disappears quickly after use
        var used = entry with { IsUsed = true };
        await _cache.SetAsync(codeKey, Serialize(used), new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30)
        }, cancellationToken).ConfigureAwait(false);

        var token = GenerateSecureToken();
        var cNonce = CNonceValidator.GenerateNonce();
        var now = DateTimeOffset.UtcNow;

        var activeToken = new ActiveTokenEntry(
            token,
            cNonce,
            now.AddSeconds(_accessTokenLifetimeSeconds),
            now.AddSeconds(_cNonceLifetimeSeconds),
            entry.AuthorizedConfigurationIds);

        await _cache.SetAsync(TokenCacheKey(token), Serialize(activeToken), new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(_accessTokenLifetimeSeconds)
        }, cancellationToken).ConfigureAwait(false);

        _logger.LogInformation(
            "Issued access token for pre-authorized code. Configurations={Configs} ExpiresIn={ExpiresIn}s",
            string.Join(",", entry.AuthorizedConfigurationIds),
            _accessTokenLifetimeSeconds);

        return new IssuedAccessToken(token, cNonce, _accessTokenLifetimeSeconds, _cNonceLifetimeSeconds, entry.AuthorizedConfigurationIds);
    }

    /// <inheritdoc/>
    public async Task<IssuedAccessToken?> ValidateAsync(
        string accessToken,
        CancellationToken cancellationToken = default)
    {
        var bytes = await _cache.GetAsync(TokenCacheKey(accessToken), cancellationToken).ConfigureAwait(false);
        if (bytes is null)
        {
            _logger.LogDebug("Access token not found in distributed cache.");
            return null;
        }

        var entry = JsonSerializer.Deserialize<ActiveTokenEntry>(bytes, SerializerOptions);
        if (entry is null)
        {
            _logger.LogWarning("Failed to deserialize access token entry from cache.");
            return null;
        }

        var now = DateTimeOffset.UtcNow;
        var remainingTokenSeconds = (int)(entry.ExpiresAt - now).TotalSeconds;
        var remainingNonceSeconds = entry.CNonceExpiresAt < now ? 0 : (int)(entry.CNonceExpiresAt - now).TotalSeconds;

        return new IssuedAccessToken(entry.Token, entry.CNonce, remainingTokenSeconds, remainingNonceSeconds, entry.AuthorizedConfigurationIds);
    }

    /// <summary>
    /// Registers a pre-authorized code in the distributed cache.
    /// </summary>
    public async Task RegisterPreAuthorizedCodeAsync(
        string code,
        string? transactionCode,
        IReadOnlyList<string> authorizedConfigurationIds,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentNullException.ThrowIfNull(authorizedConfigurationIds);

        var entry = new PreAuthorizedCodeEntry(code, transactionCode, authorizedConfigurationIds, IsUsed: false);
        await _cache.SetAsync(CodeCacheKey(code), Serialize(entry), new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(_accessTokenLifetimeSeconds * 2)
        }, cancellationToken).ConfigureAwait(false);

        _logger.LogInformation(
            "Registered pre-authorized code. Configurations={Configs} RequiresPin={RequiresPin}",
            string.Join(",", authorizedConfigurationIds),
            transactionCode != null);
    }

    private static string CodeCacheKey(string code) => $"oid4vci:code:{code}";
    private static string TokenCacheKey(string token) => $"oid4vci:token:{token}";

    private static byte[] Serialize<T>(T value) =>
        JsonSerializer.SerializeToUtf8Bytes(value, SerializerOptions);

    private static string Truncate(string value) =>
        value.Length > 8 ? value[..8] + "..." : value;

    private static string GenerateSecureToken()
    {
        var bytes = new byte[32];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    // ── private DTOs for cache serialisation ──────────────────────────────

    private sealed record PreAuthorizedCodeEntry(
        string Code,
        string? TransactionCode,
        IReadOnlyList<string> AuthorizedConfigurationIds,
        bool IsUsed);

    private sealed record ActiveTokenEntry(
        string Token,
        string CNonce,
        DateTimeOffset ExpiresAt,
        DateTimeOffset CNonceExpiresAt,
        IReadOnlyList<string> AuthorizedConfigurationIds);
}
