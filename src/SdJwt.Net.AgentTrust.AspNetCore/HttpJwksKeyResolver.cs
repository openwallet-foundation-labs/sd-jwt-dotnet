using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.AgentTrust.Core;

namespace SdJwt.Net.AgentTrust.AspNetCore;

/// <summary>
/// Resolves signing keys from a remote JWKS endpoint.
/// Caches keys per issuer for the configured duration.
/// </summary>
public class HttpJwksKeyResolver : IJwksKeyResolver
{
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _cacheDuration;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IReadOnlyDictionary<string, string> _issuerJwksEndpoints;
    private readonly ILogger<HttpJwksKeyResolver> _logger;

    /// <summary>
    /// Initializes a new HTTP JWKS resolver.
    /// </summary>
    /// <param name="issuerJwksEndpoints">Mapping of issuer identifier to JWKS endpoint URL.</param>
    /// <param name="httpClientFactory">HTTP client factory.</param>
    /// <param name="cache">Memory cache for key caching.</param>
    /// <param name="cacheDuration">Duration to cache keys. Defaults to 1 hour.</param>
    /// <param name="logger">Optional logger.</param>
    public HttpJwksKeyResolver(
        IReadOnlyDictionary<string, string> issuerJwksEndpoints,
        IHttpClientFactory httpClientFactory,
        IMemoryCache cache,
        TimeSpan? cacheDuration = null,
        ILogger<HttpJwksKeyResolver>? logger = null)
    {
        _issuerJwksEndpoints = issuerJwksEndpoints ?? throw new ArgumentNullException(nameof(issuerJwksEndpoints));
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _cacheDuration = cacheDuration ?? TimeSpan.FromHours(1);
        _logger = logger ?? NullLogger<HttpJwksKeyResolver>.Instance;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<SecurityKey>> ResolveKeysAsync(
        string issuer,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(issuer))
        {
            return Array.Empty<SecurityKey>();
        }

        var cacheKey = $"jwks:{issuer}";
        if (_cache.TryGetValue<IReadOnlyList<SecurityKey>>(cacheKey, out var cached) && cached != null)
        {
            return cached;
        }

        if (!_issuerJwksEndpoints.TryGetValue(issuer, out var jwksUri))
        {
            _logger.LogWarning("No JWKS endpoint configured for issuer {Issuer}", issuer);
            return Array.Empty<SecurityKey>();
        }

        try
        {
            var configManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                jwksUri,
                new OpenIdConnectConfigurationRetriever(),
                new HttpDocumentRetriever(_httpClientFactory.CreateClient()));

            var config = await configManager.GetConfigurationAsync(cancellationToken);
            var keys = config.SigningKeys.ToList().AsReadOnly();

            _cache.Set(cacheKey, (IReadOnlyList<SecurityKey>)keys, _cacheDuration);
            _logger.LogDebug("Cached {KeyCount} keys for issuer {Issuer}", keys.Count, issuer);

            return keys;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve JWKS for issuer {Issuer} from {JwksUri}", issuer, jwksUri);
            return Array.Empty<SecurityKey>();
        }
    }

    /// <inheritdoc/>
    public async Task<SecurityKey?> ResolveKeyAsync(
        string issuer,
        string keyId,
        CancellationToken cancellationToken = default)
    {
        var keys = await ResolveKeysAsync(issuer, cancellationToken);
        return keys.FirstOrDefault(k => string.Equals(k.KeyId, keyId, StringComparison.Ordinal));
    }
}
