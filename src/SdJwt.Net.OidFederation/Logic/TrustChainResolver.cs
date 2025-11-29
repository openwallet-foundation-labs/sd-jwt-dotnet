using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.OidFederation.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Security.Claims;
using System.Text.Json;

namespace SdJwt.Net.OidFederation.Logic;

/// <summary>
/// Resolves and validates trust chains according to OpenID Federation 1.0.
/// Provides the client-side logic for validating federation entities.
/// </summary>
public class TrustChainResolver
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<TrustChainResolver>? _logger;
    private readonly JwtSecurityTokenHandler _tokenHandler;
    private readonly Dictionary<string, SecurityKey> _trustAnchors;
    private readonly TrustChainResolverOptions _options;

    /// <summary>
    /// Initializes a new instance of the TrustChainResolver.
    /// </summary>
    /// <param name="httpClient">HTTP client for federation requests</param>
    /// <param name="trustAnchors">Dictionary of trust anchor URLs to their public keys</param>
    /// <param name="options">Optional configuration options</param>
    /// <param name="logger">Optional logger for diagnostics</param>
    public TrustChainResolver(
        HttpClient httpClient, 
        Dictionary<string, SecurityKey> trustAnchors,
        TrustChainResolverOptions? options = null,
        ILogger<TrustChainResolver>? logger = null)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _trustAnchors = trustAnchors ?? throw new ArgumentNullException(nameof(trustAnchors));
        _options = options ?? new TrustChainResolverOptions();
        _logger = logger;
        _tokenHandler = new JwtSecurityTokenHandler();

        ConfigureHttpClient();
    }

    /// <summary>
    /// Resolves and validates the trust chain for a target entity.
    /// </summary>
    /// <param name="targetEntityUrl">The URL of the entity to validate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A TrustChainResult containing the validation outcome</returns>
    public async Task<TrustChainResult> ResolveAsync(string targetEntityUrl, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(targetEntityUrl))
            throw new ArgumentException("Target entity URL cannot be null or empty", nameof(targetEntityUrl));

        if (!Uri.TryCreate(targetEntityUrl, UriKind.Absolute, out var targetUri) || targetUri.Scheme != "https")
            throw new ArgumentException("Target entity URL must be a valid HTTPS URL", nameof(targetEntityUrl));

        _logger?.LogInformation("Starting trust chain resolution for {TargetEntity}", targetEntityUrl);

        try
        {
            var result = await ResolveInternalAsync(targetEntityUrl, new List<string>(), cancellationToken);
            
            _logger?.LogInformation("Trust chain resolution completed for {TargetEntity} with result: {IsValid}", 
                targetEntityUrl, result.IsValid);
            
            return result;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Trust chain resolution failed for {TargetEntity}", targetEntityUrl);
            return TrustChainResult.Failed($"Trust chain resolution failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Internal recursive method for resolving trust chains.
    /// </summary>
    /// <param name="entityUrl">Current entity URL</param>
    /// <param name="visitedEntities">List of visited entities to prevent loops</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Trust chain result</returns>
    private async Task<TrustChainResult> ResolveInternalAsync(
        string entityUrl, 
        List<string> visitedEntities, 
        CancellationToken cancellationToken)
    {
        // Check for cycles
        if (visitedEntities.Contains(entityUrl))
        {
            return TrustChainResult.Failed($"Circular reference detected in trust chain: {entityUrl}");
        }

        // Check path length limit
        if (visitedEntities.Count >= _options.MaxPathLength)
        {
            return TrustChainResult.Failed($"Trust chain exceeds maximum path length of {_options.MaxPathLength}");
        }

        visitedEntities.Add(entityUrl);

        // Step 1: Fetch entity configuration
        var entityConfig = await FetchEntityConfigurationAsync(entityUrl, cancellationToken);
        if (entityConfig == null)
        {
            return TrustChainResult.Failed($"Failed to fetch entity configuration for {entityUrl}");
        }

        // Step 2: Validate entity configuration signature (self-signed)
        var configValidation = await ValidateEntityConfigurationAsync(entityConfig, entityUrl);
        if (!configValidation.IsValid)
        {
            return TrustChainResult.Failed($"Entity configuration validation failed: {configValidation.ErrorMessage}");
        }

        // Step 3: Check if this is a trust anchor
        if (_trustAnchors.ContainsKey(entityUrl))
        {
            _logger?.LogDebug("Reached trust anchor: {EntityUrl}", entityUrl);
            return TrustChainResult.Success(entityUrl, configValidation.Configuration!, new List<EntityStatement>());
        }

        // Step 4: Check authority hints
        if (configValidation.Configuration!.AuthorityHints == null || configValidation.Configuration.AuthorityHints.Length == 0)
        {
            return TrustChainResult.Failed($"No authority hints found for entity {entityUrl} and it's not a trust anchor");
        }

        // Step 5: Try each authority hint
        foreach (var authorityUrl in configValidation.Configuration.AuthorityHints)
        {
            _logger?.LogDebug("Trying authority: {AuthorityUrl} for entity: {EntityUrl}", authorityUrl, entityUrl);

            try
            {
                // Fetch entity statement from authority about this entity
                var entityStatement = await FetchEntityStatementAsync(authorityUrl, entityUrl, cancellationToken);
                if (entityStatement == null)
                {
                    _logger?.LogWarning("Failed to fetch entity statement from {Authority} about {Entity}", authorityUrl, entityUrl);
                    continue;
                }

                // Validate entity statement
                var statementValidation = await ValidateEntityStatementAsync(entityStatement, authorityUrl, entityUrl);
                if (!statementValidation.IsValid)
                {
                    _logger?.LogWarning("Entity statement validation failed: {Error}", statementValidation.ErrorMessage);
                    continue;
                }

                // Recursively resolve the authority
                var authorityResult = await ResolveInternalAsync(authorityUrl, new List<string>(visitedEntities), cancellationToken);
                if (authorityResult.IsValid)
                {
                    // Success! Build the complete chain
                    var chain = new List<EntityStatement>(authorityResult.TrustChain) { statementValidation.Statement! };
                    return TrustChainResult.Success(authorityResult.TrustAnchor!, configValidation.Configuration, chain);
                }

                _logger?.LogDebug("Authority resolution failed for {AuthorityUrl}: {Error}", authorityUrl, authorityResult.ErrorMessage);
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Error processing authority {AuthorityUrl} for entity {EntityUrl}", authorityUrl, entityUrl);
            }
        }

        return TrustChainResult.Failed($"Failed to establish trust chain for entity {entityUrl} through any authority");
    }

    /// <summary>
    /// Fetches the entity configuration from the well-known endpoint.
    /// </summary>
    /// <param name="entityUrl">The entity URL</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The entity configuration JWT or null if failed</returns>
    private async Task<string?> FetchEntityConfigurationAsync(string entityUrl, CancellationToken cancellationToken)
    {
        try
        {
            var configurationUrl = entityUrl.TrimEnd('/') + OidFederationConstants.WellKnownEndpoints.EntityConfiguration;
            _logger?.LogDebug("Fetching entity configuration from: {ConfigurationUrl}", configurationUrl);

            var response = await _httpClient.GetAsync(configurationUrl, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger?.LogWarning("Failed to fetch entity configuration. Status: {StatusCode}", response.StatusCode);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            
            // Content should be a JWT directly, not JSON
            if (string.IsNullOrWhiteSpace(content))
            {
                _logger?.LogWarning("Empty entity configuration response");
                return null;
            }

            return content.Trim();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Exception while fetching entity configuration for {EntityUrl}", entityUrl);
            return null;
        }
    }

    /// <summary>
    /// Fetches an entity statement from a superior entity about a subordinate.
    /// </summary>
    /// <param name="authorityUrl">The authority entity URL</param>
    /// <param name="subjectUrl">The subject entity URL</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The entity statement JWT or null if failed</returns>
    private async Task<string?> FetchEntityStatementAsync(string authorityUrl, string subjectUrl, CancellationToken cancellationToken)
    {
        try
        {
            // First, get the authority's entity configuration to find the federation_fetch_endpoint
            var authorityConfig = await FetchEntityConfigurationAsync(authorityUrl, cancellationToken);
            if (authorityConfig == null)
                return null;

            var configValidation = await ValidateEntityConfigurationAsync(authorityConfig, authorityUrl);
            if (!configValidation.IsValid)
                return null;

            var fetchEndpoint = configValidation.Configuration?.Metadata?.FederationEntity?.FederationFetchEndpoint;
            if (string.IsNullOrWhiteSpace(fetchEndpoint))
            {
                // Fallback: try standard fetch endpoint pattern
                fetchEndpoint = authorityUrl.TrimEnd('/') + "/federation_fetch";
            }

            var requestUrl = $"{fetchEndpoint}?{OidFederationConstants.QueryParameters.Subject}={Uri.EscapeDataString(subjectUrl)}";
            _logger?.LogDebug("Fetching entity statement from: {RequestUrl}", requestUrl);

            var response = await _httpClient.GetAsync(requestUrl, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger?.LogWarning("Failed to fetch entity statement. Status: {StatusCode}", response.StatusCode);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            return string.IsNullOrWhiteSpace(content) ? null : content.Trim();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Exception while fetching entity statement from {Authority} about {Subject}", authorityUrl, subjectUrl);
            return null;
        }
    }

    /// <summary>
    /// Validates an entity configuration JWT.
    /// </summary>
    /// <param name="jwt">The entity configuration JWT</param>
    /// <param name="expectedEntityUrl">The expected entity URL</param>
    /// <returns>Validation result</returns>
    private async Task<EntityConfigurationValidationResult> ValidateEntityConfigurationAsync(string jwt, string expectedEntityUrl)
    {
        try
        {
            // Parse without validation first to extract the public key
            var token = _tokenHandler.ReadJwtToken(jwt);

            // Verify this is an entity configuration
            if (token.Header.Typ != OidFederationConstants.JwtHeaders.EntityConfigurationType)
            {
                return EntityConfigurationValidationResult.Failed("Invalid JWT type for entity configuration");
            }

            // Extract issuer and subject
            var issuer = token.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Iss)?.Value;
            var subject = token.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;

            if (issuer != subject)
            {
                return EntityConfigurationValidationResult.Failed("Issuer and subject must be the same for entity configurations");
            }

            if (issuer != expectedEntityUrl)
            {
                return EntityConfigurationValidationResult.Failed($"Entity URL mismatch. Expected: {expectedEntityUrl}, Found: {issuer}");
            }

            // Extract JWKS and find the signing key
            var jwksClaim = token.Claims.FirstOrDefault(c => c.Type == "jwks")?.Value;
            if (string.IsNullOrWhiteSpace(jwksClaim))
            {
                return EntityConfigurationValidationResult.Failed("No JWKS found in entity configuration");
            }

            // Parse JWKS and extract signing key
            var signingKey = await ExtractSigningKeyFromJwksAsync(jwksClaim, token.Header.Kid);
            if (signingKey == null)
            {
                return EntityConfigurationValidationResult.Failed("Could not extract signing key from JWKS");
            }

            // Validate signature
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = expectedEntityUrl,
                ValidateAudience = false, // Entity configurations don't have audience
                IssuerSigningKey = signingKey,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(_options.ClockSkewMinutes)
            };

            var principal = _tokenHandler.ValidateToken(jwt, validationParameters, out var validatedToken);

            // Parse the configuration
            var configuration = ParseEntityConfiguration(token);
            configuration.Validate();

            return EntityConfigurationValidationResult.Success(configuration);
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Entity configuration validation failed");
            return EntityConfigurationValidationResult.Failed($"Validation failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Validates an entity statement JWT.
    /// </summary>
    /// <param name="jwt">The entity statement JWT</param>
    /// <param name="expectedIssuer">The expected issuer URL</param>
    /// <param name="expectedSubject">The expected subject URL</param>
    /// <returns>Validation result</returns>
    private async Task<EntityStatementValidationResult> ValidateEntityStatementAsync(string jwt, string expectedIssuer, string expectedSubject)
    {
        try
        {
            var token = _tokenHandler.ReadJwtToken(jwt);

            // Verify this is an entity statement
            if (token.Header.Typ != OidFederationConstants.JwtHeaders.EntityStatementType)
            {
                return EntityStatementValidationResult.Failed("Invalid JWT type for entity statement");
            }

            var issuer = token.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Iss)?.Value;
            var subject = token.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;

            if (issuer != expectedIssuer)
            {
                return EntityStatementValidationResult.Failed($"Issuer mismatch. Expected: {expectedIssuer}, Found: {issuer}");
            }

            if (subject != expectedSubject)
            {
                return EntityStatementValidationResult.Failed($"Subject mismatch. Expected: {expectedSubject}, Found: {subject}");
            }

            // Get the issuer's public key for validation
            var issuerKey = await GetIssuerSigningKeyAsync(expectedIssuer);
            if (issuerKey == null)
            {
                return EntityStatementValidationResult.Failed($"Could not obtain signing key for issuer {expectedIssuer}");
            }

            // Validate signature
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = expectedIssuer,
                ValidateAudience = false, // Entity statements don't have audience
                IssuerSigningKey = issuerKey,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(_options.ClockSkewMinutes)
            };

            var principal = _tokenHandler.ValidateToken(jwt, validationParameters, out var validatedToken);

            // Parse the statement
            var statement = ParseEntityStatement(token);
            statement.Validate();

            return EntityStatementValidationResult.Success(statement);
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Entity statement validation failed");
            return EntityStatementValidationResult.Failed($"Validation failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Extracts a signing key from a JWKS.
    /// </summary>
    /// <param name="jwksJson">The JWKS JSON string</param>
    /// <param name="keyId">Optional key ID to select specific key</param>
    /// <returns>The signing key or null if not found</returns>
    private Task<SecurityKey?> ExtractSigningKeyFromJwksAsync(string jwksJson, string? keyId)
    {
        try
        {
            var jwks = JsonSerializer.Deserialize<JsonWebKeySet>(jwksJson);
            if (jwks?.Keys == null || jwks.Keys.Count == 0)
                return Task.FromResult<SecurityKey?>(null);

            // If key ID is specified, find that specific key
            if (!string.IsNullOrWhiteSpace(keyId))
            {
                var keyWithId = jwks.Keys.FirstOrDefault(k => k.Kid == keyId);
                return Task.FromResult<SecurityKey?>(keyWithId);
            }

            // Otherwise, return the first suitable signing key
            var signingKey = jwks.Keys.FirstOrDefault(k => k.Use == "sig" || string.IsNullOrEmpty(k.Use));
            return Task.FromResult<SecurityKey?>(signingKey);
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Failed to extract signing key from JWKS");
            return Task.FromResult<SecurityKey?>(null);
        }
    }

    /// <summary>
    /// Gets the signing key for an issuer entity.
    /// </summary>
    /// <param name="issuerUrl">The issuer URL</param>
    /// <returns>The signing key or null if not found</returns>
    private async Task<SecurityKey?> GetIssuerSigningKeyAsync(string issuerUrl)
    {
        // Check if this is a trust anchor
        if (_trustAnchors.TryGetValue(issuerUrl, out var trustAnchorKey))
        {
            return trustAnchorKey;
        }

        // Otherwise, fetch the issuer's entity configuration
        var configJwt = await FetchEntityConfigurationAsync(issuerUrl, CancellationToken.None);
        if (configJwt == null)
            return null;

        var token = _tokenHandler.ReadJwtToken(configJwt);
        var jwksClaim = token.Claims.FirstOrDefault(c => c.Type == "jwks")?.Value;
        
        return jwksClaim != null 
            ? await ExtractSigningKeyFromJwksAsync(jwksClaim, token.Header.Kid) 
            : null;
    }

    /// <summary>
    /// Parses an entity configuration from a JWT token.
    /// </summary>
    /// <param name="token">The JWT token</param>
    /// <returns>The parsed EntityConfiguration</returns>
    private EntityConfiguration ParseEntityConfiguration(JwtSecurityToken token)
    {
        var config = new EntityConfiguration();
        
        foreach (var claim in token.Claims)
        {
            switch (claim.Type)
            {
                case JwtRegisteredClaimNames.Iss:
                    config.Issuer = claim.Value;
                    break;
                case JwtRegisteredClaimNames.Sub:
                    config.Subject = claim.Value;
                    break;
                case JwtRegisteredClaimNames.Iat:
                    if (long.TryParse(claim.Value, out var iat))
                        config.IssuedAt = iat;
                    break;
                case JwtRegisteredClaimNames.Exp:
                    if (long.TryParse(claim.Value, out var exp))
                        config.ExpiresAt = exp;
                    break;
                case "jwks":
                    config.JwkSet = JsonSerializer.Deserialize<object>(claim.Value);
                    break;
                case "metadata":
                    config.Metadata = JsonSerializer.Deserialize<EntityMetadata>(claim.Value);
                    break;
                case "authority_hints":
                    config.AuthorityHints = JsonSerializer.Deserialize<string[]>(claim.Value);
                    break;
                case "constraints":
                    config.Constraints = JsonSerializer.Deserialize<EntityConstraints>(claim.Value);
                    break;
                case "trust_marks":
                    config.TrustMarks = JsonSerializer.Deserialize<TrustMark[]>(claim.Value);
                    break;
            }
        }

        return config;
    }

    /// <summary>
    /// Parses an entity statement from a JWT token.
    /// </summary>
    /// <param name="token">The JWT token</param>
    /// <returns>The parsed EntityStatement</returns>
    private EntityStatement ParseEntityStatement(JwtSecurityToken token)
    {
        var statement = new EntityStatement();
        
        foreach (var claim in token.Claims)
        {
            switch (claim.Type)
            {
                case JwtRegisteredClaimNames.Iss:
                    statement.Issuer = claim.Value;
                    break;
                case JwtRegisteredClaimNames.Sub:
                    statement.Subject = claim.Value;
                    break;
                case JwtRegisteredClaimNames.Iat:
                    if (long.TryParse(claim.Value, out var iat))
                        statement.IssuedAt = iat;
                    break;
                case JwtRegisteredClaimNames.Exp:
                    if (long.TryParse(claim.Value, out var exp))
                        statement.ExpiresAt = exp;
                    break;
                case "jwks":
                    statement.JwkSet = JsonSerializer.Deserialize<object>(claim.Value);
                    break;
                case "metadata_policy":
                    statement.MetadataPolicy = JsonSerializer.Deserialize<MetadataPolicy>(claim.Value);
                    break;
                case "constraints":
                    statement.Constraints = JsonSerializer.Deserialize<EntityConstraints>(claim.Value);
                    break;
                case "trust_marks":
                    statement.TrustMarks = JsonSerializer.Deserialize<TrustMark[]>(claim.Value);
                    break;
                case "authority_hints":
                    statement.AuthorityHints = JsonSerializer.Deserialize<string[]>(claim.Value);
                    break;
                case "source_endpoint":
                    statement.SourceEndpoint = claim.Value;
                    break;
            }
        }

        return statement;
    }

    /// <summary>
    /// Configures the HTTP client with appropriate settings.
    /// </summary>
    private void ConfigureHttpClient()
    {
        _httpClient.Timeout = TimeSpan.FromSeconds(_options.HttpTimeoutSeconds);
        
        // Set appropriate headers
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "SdJwt.Net.OidFederation/1.0.0");
        _httpClient.DefaultRequestHeaders.Add("Accept", OidFederationConstants.ContentTypes.EntityConfiguration);
    }
}

/// <summary>
/// Configuration options for the TrustChainResolver.
/// </summary>
public class TrustChainResolverOptions
{
    /// <summary>
    /// Gets or sets the maximum path length for trust chains.
    /// Default: 10.
    /// </summary>
    public int MaxPathLength { get; set; } = OidFederationConstants.Defaults.MaxPathLength;

    /// <summary>
    /// Gets or sets the HTTP timeout in seconds.
    /// Default: 30.
    /// </summary>
    public int HttpTimeoutSeconds { get; set; } = OidFederationConstants.Defaults.HttpTimeoutSeconds;

    /// <summary>
    /// Gets or sets the clock skew tolerance in minutes.
    /// Default: 5.
    /// </summary>
    public int ClockSkewMinutes { get; set; } = 5;

    /// <summary>
    /// Gets or sets whether to enable caching of entity configurations and statements.
    /// Default: true.
    /// </summary>
    public bool EnableCaching { get; set; } = true;

    /// <summary>
    /// Gets or sets the cache duration in minutes.
    /// Default: 60.
    /// </summary>
    public int CacheDurationMinutes { get; set; } = OidFederationConstants.Cache.DefaultCacheDurationMinutes;
}
