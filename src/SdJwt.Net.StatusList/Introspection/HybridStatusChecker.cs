using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.StatusList.Models;
using SdJwt.Net.StatusList.Verifier;

namespace SdJwt.Net.StatusList.Introspection;

/// <summary>
/// Hybrid status checker that combines Status List and Token Introspection methods.
/// Provides configurable strategies for real-time and batch status verification.
/// </summary>
public class HybridStatusChecker : IDisposable
{
    private readonly HybridStatusOptions _options;
    private readonly StatusListVerifier? _statusListVerifier;
    private readonly ITokenIntrospectionClient? _introspectionClient;
    private readonly IMemoryCache? _cache;
    private readonly ILogger _logger;
    private readonly bool _ownsVerifier;
    private readonly bool _ownsClient;
    private readonly bool _ownsCache;
    private bool _disposed;

    /// <summary>
    /// Creates a new HybridStatusChecker with the specified options.
    /// </summary>
    /// <param name="options">Configuration options.</param>
    /// <param name="statusListVerifier">Optional Status List verifier.</param>
    /// <param name="httpClient">Optional HTTP client for introspection.</param>
    /// <param name="cache">Optional memory cache.</param>
    /// <param name="logger">Optional logger.</param>
    /// <exception cref="ArgumentNullException">Thrown when options is null.</exception>
    /// <exception cref="ArgumentException">Thrown when required options are missing.</exception>
    public HybridStatusChecker(
        HybridStatusOptions options,
        StatusListVerifier? statusListVerifier = null,
        HttpClient? httpClient = null,
        IMemoryCache? cache = null,
        ILogger<HybridStatusChecker>? logger = null)
    {
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        _options = options;
        _logger = logger ?? NullLogger<HybridStatusChecker>.Instance;

        // Validate options based on strategy
        // Only IntrospectionOnly strictly requires the endpoint
        // PreferStatusList/PreferIntrospection/Parallel can work without introspection if fallback is disabled
        if (_options.Strategy == HybridStrategy.IntrospectionOnly && string.IsNullOrEmpty(_options.IntrospectionEndpoint))
        {
            throw new ArgumentException(
                $"IntrospectionEndpoint is required for strategy {_options.Strategy}",
                nameof(options));
        }

        // Initialize Status List verifier if needed
        if (RequiresStatusList(_options.Strategy))
        {
            if (statusListVerifier != null)
            {
                _statusListVerifier = statusListVerifier;
                _ownsVerifier = false;
            }
            else
            {
                _statusListVerifier = new StatusListVerifier();
                _ownsVerifier = true;
            }
        }

        // Initialize introspection client if needed
        if (RequiresIntrospection(_options.Strategy) && !string.IsNullOrEmpty(_options.IntrospectionEndpoint))
        {
            _introspectionClient = new TokenIntrospectionClient(
                _options.IntrospectionEndpoint,
                httpClient,
                _options.IntrospectionOptions);
            _ownsClient = httpClient == null;
        }

        // Initialize cache if needed
        if (_options.EnableCaching)
        {
            if (cache != null)
            {
                _cache = cache;
                _ownsCache = false;
            }
            else
            {
                _cache = new MemoryCache(new MemoryCacheOptions());
                _ownsCache = true;
            }
        }
    }

    /// <summary>
    /// Checks the status of a token using the configured hybrid strategy.
    /// </summary>
    /// <param name="statusClaim">Status claim from the credential (for Status List).</param>
    /// <param name="issuerKeyProvider">Key resolver for Status List verification.</param>
    /// <param name="token">Optional raw token for introspection.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The hybrid status check result.</returns>
    public async Task<HybridStatusResult> CheckStatusAsync(
        StatusClaim statusClaim,
        Func<string, Task<SecurityKey>> issuerKeyProvider,
        string? token = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Checking status with strategy {Strategy}", _options.Strategy);

        return _options.Strategy switch
        {
            HybridStrategy.StatusListOnly => await CheckStatusListOnlyAsync(
                statusClaim, issuerKeyProvider, cancellationToken),

            HybridStrategy.IntrospectionOnly => await CheckIntrospectionOnlyAsync(
                token!, cancellationToken),

            HybridStrategy.PreferStatusList => await CheckPreferStatusListAsync(
                statusClaim, issuerKeyProvider, token, cancellationToken),

            HybridStrategy.PreferIntrospection => await CheckPreferIntrospectionAsync(
                statusClaim, issuerKeyProvider, token!, cancellationToken),

            HybridStrategy.Parallel => await CheckParallelAsync(
                statusClaim, issuerKeyProvider, token, cancellationToken),

            _ => throw new InvalidOperationException($"Unknown strategy: {_options.Strategy}")
        };
    }

    /// <summary>
    /// Checks the status using token introspection only.
    /// </summary>
    /// <param name="token">The token to introspect.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The hybrid status check result.</returns>
    public async Task<HybridStatusResult> CheckStatusAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        if (_options.Strategy != HybridStrategy.IntrospectionOnly)
        {
            throw new InvalidOperationException(
                "Use CheckStatusAsync with StatusClaim for strategies other than IntrospectionOnly");
        }

        return await CheckIntrospectionOnlyAsync(token, cancellationToken);
    }

    private async Task<HybridStatusResult> CheckStatusListOnlyAsync(
        StatusClaim statusClaim,
        Func<string, Task<SecurityKey>> issuerKeyProvider,
        CancellationToken cancellationToken)
    {
        var result = await _statusListVerifier!.CheckStatusAsync(
            statusClaim,
            issuerKeyProvider,
            _options.StatusListOptions);

        return HybridStatusResult.FromStatusListResult(result);
    }

    private async Task<HybridStatusResult> CheckIntrospectionOnlyAsync(
        string token,
        CancellationToken cancellationToken)
    {
        var cacheKey = $"introspection:{ComputeTokenHash(token)}";

        if (_options.EnableCaching && _cache != null &&
            _cache.TryGetValue(cacheKey, out HybridStatusResult? cachedResult))
        {
            _logger.LogDebug("Using cached introspection result");
            return cachedResult!;
        }

        var introspectionResult = await _introspectionClient!.IntrospectAsync(
            token,
            cancellationToken: cancellationToken);

        var result = HybridStatusResult.FromIntrospectionResult(introspectionResult);

        if (_options.EnableCaching && _cache != null)
        {
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(_options.CacheDuration);
            _cache.Set(cacheKey, result, cacheOptions);
        }

        return result;
    }

    private async Task<HybridStatusResult> CheckPreferStatusListAsync(
        StatusClaim statusClaim,
        Func<string, Task<SecurityKey>> issuerKeyProvider,
        string? token,
        CancellationToken cancellationToken)
    {
        try
        {
            return await CheckStatusListOnlyAsync(statusClaim, issuerKeyProvider, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Status List check failed, attempting fallback to introspection");

            if (!_options.FallbackOnError)
            {
                throw;
            }

            if (string.IsNullOrEmpty(token) || _introspectionClient == null)
            {
                throw new InvalidOperationException(
                    "Cannot fall back to introspection: token or endpoint not configured", ex);
            }

            var introspectionResult = await _introspectionClient.IntrospectAsync(
                token,
                cancellationToken: cancellationToken);

            return HybridStatusResult.FromIntrospectionResult(
                introspectionResult,
                usedFallback: true,
                primaryError: ex.Message);
        }
    }

    private async Task<HybridStatusResult> CheckPreferIntrospectionAsync(
        StatusClaim statusClaim,
        Func<string, Task<SecurityKey>> issuerKeyProvider,
        string token,
        CancellationToken cancellationToken)
    {
        try
        {
            return await CheckIntrospectionOnlyAsync(token, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Introspection check failed, attempting fallback to Status List");

            if (!_options.FallbackOnError)
            {
                throw;
            }

            if (_statusListVerifier == null)
            {
                throw new InvalidOperationException(
                    "Cannot fall back to Status List: verifier not configured", ex);
            }

            var statusListResult = await _statusListVerifier.CheckStatusAsync(
                statusClaim,
                issuerKeyProvider,
                _options.StatusListOptions);

            return HybridStatusResult.FromStatusListResult(
                statusListResult,
                usedFallback: true,
                primaryError: ex.Message);
        }
    }

    private async Task<HybridStatusResult> CheckParallelAsync(
        StatusClaim statusClaim,
        Func<string, Task<SecurityKey>> issuerKeyProvider,
        string? token,
        CancellationToken cancellationToken)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(_options.ParallelTimeout);

        var tasks = new List<Task<HybridStatusResult>>();

        if (_statusListVerifier != null)
        {
            tasks.Add(Task.Run(async () =>
            {
                var result = await _statusListVerifier.CheckStatusAsync(
                    statusClaim,
                    issuerKeyProvider,
                    _options.StatusListOptions);
                return HybridStatusResult.FromStatusListResult(result);
            }, cts.Token));
        }

        if (_introspectionClient != null && !string.IsNullOrEmpty(token))
        {
            tasks.Add(Task.Run(async () =>
            {
                var result = await _introspectionClient.IntrospectAsync(
                    token,
                    cancellationToken: cts.Token);
                return HybridStatusResult.FromIntrospectionResult(result);
            }, cts.Token));
        }

        // Return first successful result
        while (tasks.Count > 0)
        {
            var completedTask = await Task.WhenAny(tasks);
            tasks.Remove(completedTask);

            try
            {
                var result = await completedTask;
                if (result.Status == StatusType.Valid)
                {
                    return result;
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Parallel check task failed");
            }
        }

        // If no tasks succeeded, throw
        throw new InvalidOperationException("All parallel status check methods failed");
    }

    private static string ComputeTokenHash(string token)
    {
        // Use a simple hash for caching - not cryptographic
        return token.GetHashCode().ToString("X8");
    }

    private static bool RequiresStatusList(HybridStrategy strategy) =>
        strategy is HybridStrategy.StatusListOnly
            or HybridStrategy.PreferStatusList
            or HybridStrategy.PreferIntrospection
            or HybridStrategy.Parallel;

    private static bool RequiresIntrospection(HybridStrategy strategy) =>
        strategy is HybridStrategy.IntrospectionOnly
            or HybridStrategy.PreferIntrospection
            or HybridStrategy.PreferStatusList
            or HybridStrategy.Parallel;

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes of resources.
    /// </summary>
    /// <param name="disposing">True if disposing managed resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            if (_ownsVerifier && _statusListVerifier != null)
            {
                _statusListVerifier.Dispose();
            }

            if (_ownsClient && _introspectionClient is IDisposable disposableClient)
            {
                disposableClient.Dispose();
            }

            if (_ownsCache && _cache is IDisposable disposableCache)
            {
                disposableCache.Dispose();
            }
        }

        _disposed = true;
    }
}
