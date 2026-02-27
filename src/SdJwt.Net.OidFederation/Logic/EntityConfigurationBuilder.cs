using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.OidFederation.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;

namespace SdJwt.Net.OidFederation.Logic;

/// <summary>
/// Builder for creating Entity Configuration JWTs according to OpenID Federation 1.0.
/// Provides a fluent API for building self-signed entity configurations.
/// </summary>
public class EntityConfigurationBuilder
{
    private readonly string _entityUrl;
    private readonly EntityConfiguration _configuration;
    private SecurityKey? _signingKey;
    private string _signingAlgorithm = OidFederationConstants.SigningAlgorithms.ES256;
    private readonly ILogger? _logger;

    private EntityConfigurationBuilder(string entityUrl, ILogger? logger = null)
    {
        if (string.IsNullOrWhiteSpace(entityUrl))
            throw new ArgumentException("Entity URL cannot be null or empty", nameof(entityUrl));

        if (!Uri.TryCreate(entityUrl, UriKind.Absolute, out var uri) || uri.Scheme != "https")
            throw new ArgumentException("Entity URL must be a valid HTTPS URL", nameof(entityUrl));

        _entityUrl = entityUrl;
        _logger = logger;
        _configuration = new EntityConfiguration
        {
            Issuer = entityUrl,
            Subject = entityUrl,
            IssuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(1).ToUnixTimeSeconds()
        };
    }

    /// <summary>
    /// Creates a new entity configuration builder for the specified entity.
    /// </summary>
    /// <param name="entityUrl">The entity URL (must be HTTPS)</param>
    /// <param name="logger">Optional logger for diagnostics</param>
    /// <returns>A new EntityConfigurationBuilder instance</returns>
    public static EntityConfigurationBuilder Create(string entityUrl, ILogger? logger = null)
    {
        return new EntityConfigurationBuilder(entityUrl, logger);
    }

    /// <summary>
    /// Sets the signing key and algorithm for the entity configuration.
    /// </summary>
    /// <param name="signingKey">The private key used to sign the JWT</param>
    /// <param name="algorithm">The signing algorithm (default: ES256)</param>
    /// <returns>This builder for method chaining</returns>
    public EntityConfigurationBuilder WithSigningKey(SecurityKey signingKey, string algorithm = OidFederationConstants.SigningAlgorithms.ES256)
    {
        _signingKey = signingKey ?? throw new ArgumentNullException(nameof(signingKey));

        if (!OidFederationConstants.SigningAlgorithms.All.Contains(algorithm))
            throw new ArgumentException($"Unsupported signing algorithm: {algorithm}", nameof(algorithm));

        _signingAlgorithm = algorithm;
        _logger?.LogDebug("Signing key configured with algorithm {Algorithm}", algorithm);
        return this;
    }

    /// <summary>
    /// Sets the JSON Web Key Set for the entity.
    /// </summary>
    /// <param name="jwkSet">The entity's public key set</param>
    /// <returns>This builder for method chaining</returns>
    public EntityConfigurationBuilder WithJwkSet(object jwkSet)
    {
        _configuration.JwkSet = jwkSet ?? throw new ArgumentNullException(nameof(jwkSet));
        _logger?.LogDebug("JWK Set configured for entity");
        return this;
    }

    /// <summary>
    /// Sets the entity metadata containing protocol-specific configuration.
    /// </summary>
    /// <param name="metadata">The entity metadata</param>
    /// <returns>This builder for method chaining</returns>
    public EntityConfigurationBuilder WithMetadata(EntityMetadata metadata)
    {
        _configuration.Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
        _logger?.LogDebug("Entity metadata configured");
        return this;
    }

    /// <summary>
    /// Adds an authority hint pointing to a superior entity.
    /// </summary>
    /// <param name="authorityUrl">The URL of the authority entity</param>
    /// <returns>This builder for method chaining</returns>
    public EntityConfigurationBuilder AddAuthorityHint(string authorityUrl)
    {
        if (string.IsNullOrWhiteSpace(authorityUrl))
            throw new ArgumentException("Authority URL cannot be null or empty", nameof(authorityUrl));

        if (!Uri.TryCreate(authorityUrl, UriKind.Absolute, out var uri) || uri.Scheme != "https")
            throw new ArgumentException("Authority URL must be a valid HTTPS URL", nameof(authorityUrl));

        var hints = new List<string>();
        if (_configuration.AuthorityHints != null)
            hints.AddRange(_configuration.AuthorityHints);

        if (!hints.Contains(authorityUrl))
        {
            hints.Add(authorityUrl);
            _configuration.AuthorityHints = hints.ToArray();
            _logger?.LogDebug("Added authority hint: {AuthorityUrl}", authorityUrl);
        }

        return this;
    }

    /// <summary>
    /// Sets multiple authority hints at once.
    /// </summary>
    /// <param name="authorityUrls">Array of authority URLs</param>
    /// <returns>This builder for method chaining</returns>
    public EntityConfigurationBuilder WithAuthorityHints(params string[] authorityUrls)
    {
        if (authorityUrls == null)
            throw new ArgumentNullException(nameof(authorityUrls));

        foreach (var url in authorityUrls)
        {
            AddAuthorityHint(url);
        }

        return this;
    }

    /// <summary>
    /// Sets entity constraints.
    /// </summary>
    /// <param name="constraints">The entity constraints</param>
    /// <returns>This builder for method chaining</returns>
    public EntityConfigurationBuilder WithConstraints(EntityConstraints constraints)
    {
        _configuration.Constraints = constraints ?? throw new ArgumentNullException(nameof(constraints));
        _logger?.LogDebug("Entity constraints configured");
        return this;
    }

    /// <summary>
    /// Adds a trust mark to the entity configuration.
    /// </summary>
    /// <param name="trustMark">The trust mark to add</param>
    /// <returns>This builder for method chaining</returns>
    public EntityConfigurationBuilder AddTrustMark(TrustMark trustMark)
    {
        if (trustMark == null)
            throw new ArgumentNullException(nameof(trustMark));

        var marks = new List<TrustMark>();
        if (_configuration.TrustMarks != null)
            marks.AddRange(_configuration.TrustMarks);

        marks.Add(trustMark);
        _configuration.TrustMarks = marks.ToArray();
        _logger?.LogDebug("Added trust mark: {TrustMarkId}", trustMark.Id);
        return this;
    }

    /// <summary>
    /// Sets the validity period for the entity configuration.
    /// </summary>
    /// <param name="validityHours">Validity period in hours</param>
    /// <returns>This builder for method chaining</returns>
    public EntityConfigurationBuilder WithValidity(int validityHours)
    {
        if (validityHours <= 0)
            throw new ArgumentException("Validity hours must be positive", nameof(validityHours));

        if (validityHours > OidFederationConstants.Defaults.MaxValidityHours)
            throw new ArgumentException($"Validity hours cannot exceed {OidFederationConstants.Defaults.MaxValidityHours}", nameof(validityHours));

        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        _configuration.IssuedAt = now;
        _configuration.ExpiresAt = now + (validityHours * 3600);

        _logger?.LogDebug("Validity period set to {ValidityHours} hours", validityHours);
        return this;
    }

    /// <summary>
    /// Sets custom issued at and expires at times.
    /// </summary>
    /// <param name="issuedAt">The issued at time</param>
    /// <param name="expiresAt">The expiration time</param>
    /// <returns>This builder for method chaining</returns>
    public EntityConfigurationBuilder WithTiming(DateTimeOffset issuedAt, DateTimeOffset expiresAt)
    {
        if (expiresAt <= issuedAt)
            throw new ArgumentException("Expires at must be after issued at", nameof(expiresAt));

        _configuration.IssuedAt = issuedAt.ToUnixTimeSeconds();
        _configuration.ExpiresAt = expiresAt.ToUnixTimeSeconds();

        _logger?.LogDebug("Custom timing configured: issued={IssuedAt}, expires={ExpiresAt}", issuedAt, expiresAt);
        return this;
    }

    /// <summary>
    /// Builds and signs the entity configuration JWT.
    /// </summary>
    /// <returns>A signed JWT string representing the entity configuration</returns>
    /// <exception cref="InvalidOperationException">Thrown when required configuration is missing</exception>
    public string Build()
    {
        _logger?.LogDebug("Building entity configuration for {EntityUrl}", _entityUrl);

        // Validate required components
        if (_signingKey == null)
            throw new InvalidOperationException("Signing key is required. Call WithSigningKey() first.");

        if (_configuration.JwkSet == null)
            throw new InvalidOperationException("JWK Set is required. Call WithJwkSet() first.");

        // Validate the configuration
        try
        {
            _configuration.Validate();
        }
        catch (InvalidOperationException ex)
        {
            _logger?.LogError(ex, "Entity configuration validation failed");
            throw;
        }

        // Create JWT header
        var signingCredentials = new SigningCredentials(_signingKey, _signingAlgorithm);
        var header = new JwtHeader(signingCredentials);
        header[JwtHeaderParameterNames.Typ] = OidFederationConstants.JwtHeaders.EntityConfigurationType;

        // Create JWT payload from configuration
        var payload = CreateJwtPayload();

        // Sign and return the JWT
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = new JwtSecurityToken(header, payload);
        var jwt = tokenHandler.WriteToken(token);

        _logger?.LogInformation("Entity configuration built successfully for {EntityUrl}", _entityUrl);
        return jwt;
    }

    /// <summary>
    /// Builds the configuration as an EntityConfiguration object without signing.
    /// Useful for testing or when you want to inspect the configuration before signing.
    /// </summary>
    /// <returns>The EntityConfiguration object</returns>
    public EntityConfiguration BuildConfiguration()
    {
        _configuration.Validate();
        return _configuration;
    }

    /// <summary>
    /// Creates the JWT payload from the entity configuration.
    /// </summary>
    /// <returns>A JwtPayload containing the configuration claims</returns>
    private JwtPayload CreateJwtPayload()
    {
        var payload = new JwtPayload
        {
            [JwtRegisteredClaimNames.Iss] = _configuration.Issuer,
            [JwtRegisteredClaimNames.Sub] = _configuration.Subject,
            [JwtRegisteredClaimNames.Iat] = _configuration.IssuedAt,
            [JwtRegisteredClaimNames.Exp] = _configuration.ExpiresAt
        };

        // Serialize complex objects to JSON strings to avoid serialization issues
        if (_configuration.JwkSet != null)
        {
            payload["jwks"] = JsonSerializer.Serialize(_configuration.JwkSet);
        }

        if (_configuration.Metadata != null)
        {
            payload["metadata"] = JsonSerializer.Serialize(_configuration.Metadata);
        }

        // Authority hints are simple string arrays, so keep them as arrays
        if (_configuration.AuthorityHints != null && _configuration.AuthorityHints.Length > 0)
        {
            payload["authority_hints"] = _configuration.AuthorityHints;
        }

        if (_configuration.Constraints != null)
        {
            payload["constraints"] = JsonSerializer.Serialize(_configuration.Constraints);
        }

        if (_configuration.TrustMarks != null && _configuration.TrustMarks.Length > 0)
        {
            payload["trust_marks"] = JsonSerializer.Serialize(_configuration.TrustMarks);
        }

        return payload;
    }
}

/// <summary>
/// Extension methods for EntityConfigurationBuilder to support common scenarios.
/// </summary>
public static class EntityConfigurationBuilderExtensions
{
    /// <summary>
    /// Configures the entity as an OID4VCI Credential Issuer.
    /// </summary>
    /// <param name="builder">The builder instance</param>
    /// <param name="credentialIssuerMetadata">The credential issuer metadata</param>
    /// <returns>The builder for method chaining</returns>
    public static EntityConfigurationBuilder AsCredentialIssuer(this EntityConfigurationBuilder builder, object credentialIssuerMetadata)
    {
        var metadata = new EntityMetadata
        {
            OpenIdCredentialIssuer = credentialIssuerMetadata
        };

        return builder.WithMetadata(metadata);
    }

    /// <summary>
    /// Configures the entity as an OID4VP Verifier.
    /// </summary>
    /// <param name="builder">The builder instance</param>
    /// <param name="verifierMetadata">The verifier metadata</param>
    /// <returns>The builder for method chaining</returns>
    public static EntityConfigurationBuilder AsVerifier(this EntityConfigurationBuilder builder, object verifierMetadata)
    {
        var metadata = new EntityMetadata
        {
            OpenIdRelyingPartyVerifier = verifierMetadata
        };

        return builder.WithMetadata(metadata);
    }

    /// <summary>
    /// Configures the entity as an OpenID Connect Provider.
    /// </summary>
    /// <param name="builder">The builder instance</param>
    /// <param name="providerMetadata">The OpenID Connect provider metadata</param>
    /// <returns>The builder for method chaining</returns>
    public static EntityConfigurationBuilder AsOpenIdProvider(this EntityConfigurationBuilder builder, object providerMetadata)
    {
        var metadata = new EntityMetadata
        {
            OpenIdProvider = providerMetadata
        };

        return builder.WithMetadata(metadata);
    }

    /// <summary>
    /// Adds multiple protocol metadata at once.
    /// </summary>
    /// <param name="builder">The builder instance</param>
    /// <param name="protocolMetadata">Dictionary of protocol identifiers to metadata objects</param>
    /// <returns>The builder for method chaining</returns>
    public static EntityConfigurationBuilder WithMultipleProtocols(this EntityConfigurationBuilder builder,
        Dictionary<string, object> protocolMetadata)
    {
        var metadata = new EntityMetadata
        {
            AdditionalMetadata = protocolMetadata
        };

        return builder.WithMetadata(metadata);
    }
}
