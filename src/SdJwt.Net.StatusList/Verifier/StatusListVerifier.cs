using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.StatusList.Models;
using System.IdentityModel.Tokens.Jwt;
using System.IO.Compression;
using System.Text.Json;
using StatusListModel = SdJwt.Net.StatusList.Models.StatusList;

namespace SdJwt.Net.StatusList.Verifier;

/// <summary>
/// Provides status checking functionality for Referenced Tokens using Status List Tokens
/// according to draft-ietf-oauth-status-list-13.
/// </summary>
public class StatusListVerifier : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger _logger;
    private readonly bool _ownsHttpClient;
    private readonly bool _ownsMemoryCache;

    /// <summary>
    /// Initializes a new instance of the StatusListVerifier.
    /// </summary>
    /// <param name="httpClient">Optional HTTP client for status list retrieval.</param>
    /// <param name="memoryCache">Optional memory cache for status list caching.</param>
    /// <param name="logger">Optional logger for diagnostics.</param>
    public StatusListVerifier(HttpClient? httpClient = null, IMemoryCache? memoryCache = null, ILogger<StatusListVerifier>? logger = null)
    {
        _logger = logger ?? NullLogger<StatusListVerifier>.Instance;

        if (httpClient == null)
        {
            _httpClient = new HttpClient();
            _httpClient.Timeout = TimeSpan.FromSeconds(10);
            _ownsHttpClient = true;
        }
        else
        {
            _httpClient = httpClient;
            _ownsHttpClient = false;
        }

        if (memoryCache == null)
        {
            _memoryCache = new MemoryCache(new MemoryCacheOptions());
            _ownsMemoryCache = true;
        }
        else
        {
            _memoryCache = memoryCache;
            _ownsMemoryCache = false;
        }
    }

    /// <summary>
    /// Checks the status of a Referenced Token using its status claim according to draft-ietf-oauth-status-list-13.
    /// </summary>
    /// <param name="statusClaim">The status claim from the Referenced Token.</param>
    /// <param name="issuerKeyProvider">Function to resolve the Status List issuer's public key.</param>
    /// <param name="options">Options for status checking.</param>
    /// <returns>The result of the status check.</returns>
    public async Task<StatusCheckResult> CheckStatusAsync(
        StatusClaim statusClaim,
        Func<string, Task<SecurityKey>> issuerKeyProvider,
        StatusListOptions? options = null)
    {
        if (statusClaim?.StatusList == null)
            throw new ArgumentException("Status claim with status_list is required", nameof(statusClaim));
        if (issuerKeyProvider == null)
            throw new ArgumentNullException(nameof(issuerKeyProvider));

        options ??= new StatusListOptions();

        if (!options.EnableStatusChecking)
        {
            _logger.LogDebug("Status checking is disabled, returning valid status");
            return new StatusCheckResult { Status = StatusType.Valid, StatusValue = 0 };
        }

        var statusList = statusClaim.StatusList;
        var credentialIndex = statusList.Index;
        var statusListUri = statusList.Uri;

        if (string.IsNullOrEmpty(statusListUri))
            throw new ArgumentException("Status list URI is required", nameof(statusClaim));

        if (credentialIndex < 0)
            throw new ArgumentException("Status list index must be non-negative", nameof(statusClaim));

        _logger.LogInformation("Checking status for Referenced Token index {Index} from URI {Uri}", credentialIndex, statusListUri);

        try
        {
            // Get Status List Token
            var statusListToken = await GetStatusListTokenAsync(statusListUri, options);

            // Parse and validate the token
            var statusListPayload = await ParseAndValidateStatusListTokenAsync(statusListToken, issuerKeyProvider, statusListUri, options);

            // Check the status
            var statusResult = await CheckTokenStatusAsync(statusListPayload, credentialIndex, options);

            _logger.LogInformation("Status check completed for Referenced Token index {Index}: Status={Status} ({StatusValue})",
                credentialIndex, statusResult.Status.GetName(), statusResult.StatusValue);

            return statusResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check status for Referenced Token index {Index} from URI {Uri}", credentialIndex, statusListUri);

            if (options.FailOnStatusCheckError)
                throw;

            // Return conservative result if configured to continue on errors
            return new StatusCheckResult
            {
                Status = StatusType.Invalid,
                StatusValue = -1,
                RetrievedAt = DateTime.UtcNow,
                StatusListUri = statusListUri
            };
        }
    }

    /// <summary>
    /// Retrieves Status List Aggregation from the specified URI.
    /// </summary>
    /// <param name="aggregationUri">The URI of the Status List Aggregation.</param>
    /// <param name="options">Options for the request.</param>
    /// <returns>The Status List Aggregation containing URIs of Status List Tokens.</returns>
    public async Task<StatusListAggregation> GetStatusListAggregationAsync(string aggregationUri, StatusListOptions? options = null)
    {
        if (string.IsNullOrWhiteSpace(aggregationUri))
            throw new ArgumentException("Aggregation URI cannot be null or empty", nameof(aggregationUri));

        options ??= new StatusListOptions();

        _logger.LogDebug("Fetching Status List Aggregation from URI {Uri}", aggregationUri);

        using var request = new HttpRequestMessage(HttpMethod.Get, aggregationUri);
        request.Headers.Add("Accept", "application/json");

        var response = await ExecuteWithRetryAsync(() => _httpClient.SendAsync(request), options.RetryPolicy);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var aggregation = JsonSerializer.Deserialize<StatusListAggregation>(json, SdJwtConstants.DefaultJsonSerializerOptions);

        if (aggregation?.StatusLists == null || aggregation.StatusLists.Length == 0)
            throw new InvalidOperationException("Invalid Status List Aggregation: no status lists found");

        return aggregation;
    }

    /// <summary>
    /// Retrieves a Status List Token from the specified URI with caching support.
    /// </summary>
    private async Task<string> GetStatusListTokenAsync(string uri, StatusListOptions options)
    {
        var cacheKey = $"status_list:{uri}";

        // Check cache first
        if (options.CacheStatusLists && _memoryCache.TryGetValue(cacheKey, out string? cachedToken))
        {
            _logger.LogDebug("Retrieved Status List Token from cache for URI {Uri}", uri);
            return cachedToken!;
        }

        _logger.LogDebug("Fetching Status List Token from URI {Uri}", uri);

        // Prepare HTTP request with appropriate Accept header
        using var request = new HttpRequestMessage(HttpMethod.Get, uri);
        request.Headers.Add("Accept", "application/statuslist+jwt, application/statuslist+cwt");

        // Add custom headers
        foreach (var header in options.CustomHeaders)
        {
            request.Headers.Add(header.Key, header.Value);
        }

        // Use conditional requests if enabled
        if (options.UseConditionalRequests && _memoryCache.TryGetValue($"etag:{uri}", out string? etag))
        {
            request.Headers.Add("If-None-Match", etag);
        }

        // Perform request with retry logic
        var response = await ExecuteWithRetryAsync(() => _httpClient.SendAsync(request), options.RetryPolicy);

        if (response.StatusCode == System.Net.HttpStatusCode.NotModified)
        {
            // Content hasn't changed, return cached version
            if (_memoryCache.TryGetValue(cacheKey, out cachedToken))
                return cachedToken!;
        }

        response.EnsureSuccessStatusCode();

        var statusListToken = await response.Content.ReadAsStringAsync();

        // Cache the result with TTL considerations
        if (options.CacheStatusLists)
        {
            // Try to determine cache duration from token TTL
            var cacheDuration = options.CacheDuration;
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwt = tokenHandler.ReadJwtToken(statusListToken);
                var ttlClaim = jwt.Claims.FirstOrDefault(c => c.Type == "ttl")?.Value;
                if (!string.IsNullOrEmpty(ttlClaim) && int.TryParse(ttlClaim, out var ttl) && ttl > 0)
                {
                    cacheDuration = TimeSpan.FromSeconds(Math.Min(ttl, (int)cacheDuration.TotalSeconds));
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse TTL from Status List Token, using default cache duration");
            }

            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = cacheDuration,
                Size = 1
            };

            _memoryCache.Set(cacheKey, statusListToken, cacheOptions);

            // Cache ETag for conditional requests
            if (response.Headers.ETag != null)
            {
                _memoryCache.Set($"etag:{uri}", response.Headers.ETag.Tag, cacheOptions);
            }

            _logger.LogDebug("Cached Status List Token for URI {Uri} with duration {Duration}", uri, cacheDuration);
        }

        return statusListToken;
    }

    /// <summary>
    /// Parses and validates a Status List Token according to draft-ietf-oauth-status-list-13.
    /// </summary>
    private async Task<StatusListTokenPayload> ParseAndValidateStatusListTokenAsync(
        string statusListToken,
        Func<string, Task<SecurityKey>> issuerKeyProvider,
        string expectedSubject,
        StatusListOptions options)
    {
        var tokenHandler = new JwtSecurityTokenHandler();

        // Read token without validation first to get claims
        var unvalidatedToken = tokenHandler.ReadJwtToken(statusListToken);

        // Validate token type
        if (unvalidatedToken.Header.Typ != "statuslist+jwt")
        {
            throw new SecurityTokenException($"Invalid token type: expected 'statuslist+jwt', found '{unvalidatedToken.Header.Typ}'");
        }

        // Validate subject matches expected URI
        if (unvalidatedToken.Subject != expectedSubject)
        {
            throw new SecurityTokenException($"Subject claim mismatch: expected '{expectedSubject}', found '{unvalidatedToken.Subject}'");
        }

        // Get signing key (note: draft-13 doesn't require issuer claim for key resolution)
        SecurityKey signingKey;
        if (!string.IsNullOrEmpty(unvalidatedToken.Issuer))
        {
            signingKey = await issuerKeyProvider(unvalidatedToken.Issuer);
        }
        else
        {
            // Use subject for key resolution if no issuer
            signingKey = await issuerKeyProvider(unvalidatedToken.Subject);
        }

        // Validate token signature and structure
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false, // Subject validation is more important in draft-13
            ValidateAudience = false,
            ValidateLifetime = options.ValidateStatusListTiming,
            IssuerSigningKey = signingKey,
            ClockSkew = TimeSpan.FromMinutes(5)
        };

        var principal = tokenHandler.ValidateToken(statusListToken, validationParameters, out var validatedToken);
        var jwt = (JwtSecurityToken)validatedToken;

        // Validate required claims
        if (string.IsNullOrEmpty(jwt.Subject))
            throw new SecurityTokenException("Status List Token must have a subject claim");

        var iatClaim = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Iat)?.Value;
        if (string.IsNullOrEmpty(iatClaim) || !long.TryParse(iatClaim, out var iat))
            throw new SecurityTokenException("Status List Token must have a valid iat claim");

        // Check token age
        if (options.MaxStatusListAge > TimeSpan.Zero)
        {
            var tokenAge = DateTime.UtcNow - DateTimeOffset.FromUnixTimeSeconds(iat).DateTime;
            if (tokenAge > options.MaxStatusListAge)
            {
                throw new SecurityTokenException($"Status List Token is too old: {tokenAge.TotalHours:F1} hours");
            }
        }

        // Extract and validate Status List
        var statusListClaim = jwt.Claims.FirstOrDefault(c => c.Type == "status_list")?.Value
            ?? throw new SecurityTokenException("status_list claim not found in token");

        var statusList = JsonSerializer.Deserialize<StatusListModel>(statusListClaim, SdJwtConstants.DefaultJsonSerializerOptions)
            ?? throw new SecurityTokenException("Failed to deserialize status_list claim");

        // Validate status list structure
        if (statusList.Bits != 1 && statusList.Bits != 2 && statusList.Bits != 4 && statusList.Bits != 8)
            throw new SecurityTokenException($"Invalid bits value: {statusList.Bits}. Must be 1, 2, 4, or 8");

        if (string.IsNullOrEmpty(statusList.List))
            throw new SecurityTokenException("Status List lst field is required");

        // Extract optional claims
        var expClaim = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Exp)?.Value;
        long? exp = null;
        if (!string.IsNullOrEmpty(expClaim) && long.TryParse(expClaim, out var expValue))
        {
            exp = expValue;
        }

        var ttlClaim = jwt.Claims.FirstOrDefault(c => c.Type == "ttl")?.Value;
        int? ttl = null;
        if (!string.IsNullOrEmpty(ttlClaim) && int.TryParse(ttlClaim, out var ttlValue))
        {
            ttl = ttlValue;
        }

        return new StatusListTokenPayload
        {
            Subject = jwt.Subject,
            IssuedAt = iat,
            ExpiresAt = exp,
            TimeToLive = ttl,
            StatusList = statusList
        };
    }

    /// <summary>
    /// Checks the status of a specific Referenced Token within the Status List.
    /// </summary>
    private async Task<StatusCheckResult> CheckTokenStatusAsync(
        StatusListTokenPayload payload,
        int tokenIndex,
        StatusListOptions options)
    {
        var statusList = payload.StatusList;

        // Decompress Status List
        var statusValues = await DecompressStatusListAsync(statusList.List, statusList.Bits);

        // Check if token index is within range
        if (tokenIndex >= statusValues.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(tokenIndex),
                $"Token index {tokenIndex} is out of range (max: {statusValues.Length - 1})");
        }

        // Get status value for the token
        var statusValue = statusValues[tokenIndex];
        var statusType = StatusTypeExtensions.FromValue(statusValue);

        var result = new StatusCheckResult
        {
            Status = statusType,
            StatusValue = statusValue,
            RetrievedAt = DateTime.UtcNow,
            FromCache = false, // TODO: Track this properly
            StatusListUri = payload.Subject
        };

        return result;
    }

    /// <summary>
    /// Executes an operation with retry logic.
    /// </summary>
    private async Task<T> ExecuteWithRetryAsync<T>(Func<Task<T>> operation, RetryPolicy retryPolicy)
    {
        var lastException = new Exception();

        for (int attempt = 0; attempt <= retryPolicy.MaxRetries; attempt++)
        {
            try
            {
                return await operation();
            }
            catch (Exception ex)
            {
                lastException = ex;

                if (attempt == retryPolicy.MaxRetries)
                    break;

                var delay = retryPolicy.BaseDelay;
                if (retryPolicy.UseExponentialBackoff)
                {
                    delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * Math.Pow(2, attempt));
                    delay = delay > retryPolicy.MaxDelay ? retryPolicy.MaxDelay : delay;
                }

                _logger.LogWarning("Operation failed on attempt {Attempt}, retrying in {Delay}ms: {Error}",
                    attempt + 1, delay.TotalMilliseconds, ex.Message);

                await Task.Delay(delay);
            }
        }

        throw new Exception($"Operation failed after {retryPolicy.MaxRetries + 1} attempts", lastException);
    }

    /// <summary>
    /// Decompresses a Status List according to draft-ietf-oauth-status-list-13.
    /// Uses DEFLATE with ZLIB data format.
    /// </summary>
    private static async Task<byte[]> DecompressStatusListAsync(string encodedList, int bits)
    {
        var compressedBytes = Base64UrlEncoder.DecodeBytes(encodedList);

        using var input = new MemoryStream(compressedBytes);
        using var deflate = new DeflateStream(input, CompressionMode.Decompress);
        using var output = new MemoryStream();

        await deflate.CopyToAsync(output);
        var decompressedBytes = output.ToArray();

        // Convert decompressed bytes back to status values
        var totalBits = decompressedBytes.Length * 8;
        var statusCount = totalBits / bits;
        var statusValues = new byte[statusCount];

        for (int i = 0; i < statusCount; i++)
        {
            var bitIndex = i * bits;
            byte statusValue = 0;

            // Extract bits for this status value (least significant bit first)
            for (int bit = 0; bit < bits; bit++)
            {
                var globalBitIndex = bitIndex + bit;
                var byteIndex = globalBitIndex / 8;
                var bitInByte = globalBitIndex % 8;

                if (byteIndex < decompressedBytes.Length &&
                    (decompressedBytes[byteIndex] & (1 << bitInByte)) != 0)
                {
                    statusValue |= (byte)(1 << bit);
                }
            }

            statusValues[i] = statusValue;
        }

        return statusValues;
    }

    /// <summary>
    /// Disposes resources used by the StatusListVerifier.
    /// </summary>
    public void Dispose()
    {
        if (_ownsHttpClient)
            _httpClient?.Dispose();

        if (_ownsMemoryCache)
            _memoryCache?.Dispose();
    }
}
