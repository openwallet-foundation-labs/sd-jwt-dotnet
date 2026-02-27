using System.Text.Json.Serialization;

namespace SdJwt.Net.OidFederation.Models;

/// <summary>
/// Represents an Entity Configuration JWT as defined in OpenID Federation 1.0.
/// This is a self-signed statement that every federation participant must publish at /.well-known/openid-federation.
/// </summary>
public class EntityConfiguration
{
    /// <summary>
    /// Gets or sets the issuer identifier. Must be the same as the subject for entity configurations (self-signed).
    /// Required. Must be a valid HTTPS URL.
    /// </summary>
    [JsonPropertyName("iss")]
    public string Issuer { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the subject identifier. Must be the same as the issuer for entity configurations (self-signed).
    /// Required. Must be a valid HTTPS URL.
    /// </summary>
    [JsonPropertyName("sub")]
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the time at which the entity configuration was issued.
    /// Required. Unix timestamp.
    /// </summary>
    [JsonPropertyName("iat")]
    public long IssuedAt
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the expiration time of the entity configuration.
    /// Required. Unix timestamp. Must be after iat.
    /// </summary>
    [JsonPropertyName("exp")]
    public long ExpiresAt
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the JSON Web Key Set containing the entity's public keys.
    /// Required. Used for signature verification and key rotation.
    /// </summary>
    [JsonPropertyName("jwks")]
    public object? JwkSet
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the entity metadata containing protocol-specific configuration.
    /// Optional. Contains metadata for supported protocols (OID4VCI, OID4VP, OIDC, etc.).
    /// </summary>
    [JsonPropertyName("metadata")]
    public EntityMetadata? Metadata
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the list of authority hints indicating superior entities.
    /// Optional. Array of HTTPS URLs pointing to entities that may issue statements about this entity.
    /// </summary>
    [JsonPropertyName("authority_hints")]
    public string[]? AuthorityHints
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the constraints that apply to this entity.
    /// Optional. Defines limitations on the entity's operations.
    /// </summary>
    [JsonPropertyName("constraints")]
    public EntityConstraints? Constraints
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the trust marks claimed by this entity.
    /// Optional. Array of trust marks that this entity claims to possess.
    /// </summary>
    [JsonPropertyName("trust_marks")]
    public TrustMark[]? TrustMarks
    {
        get; set;
    }

    /// <summary>
    /// Validates the entity configuration according to OpenID Federation 1.0 requirements.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the configuration is invalid</exception>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Issuer))
            throw new InvalidOperationException("Issuer is required");

        if (string.IsNullOrWhiteSpace(Subject))
            throw new InvalidOperationException("Subject is required");

        if (Issuer != Subject)
            throw new InvalidOperationException("Issuer and Subject must be the same for entity configurations");

        if (!Uri.TryCreate(Issuer, UriKind.Absolute, out var issuerUri) || issuerUri.Scheme != "https")
            throw new InvalidOperationException("Issuer must be a valid HTTPS URL");

        if (IssuedAt <= 0)
            throw new InvalidOperationException("IssuedAt must be a valid Unix timestamp");

        if (ExpiresAt <= IssuedAt)
            throw new InvalidOperationException("ExpiresAt must be after IssuedAt");

        if (JwkSet == null)
            throw new InvalidOperationException("JwkSet is required");

        if (AuthorityHints != null)
        {
            foreach (var hint in AuthorityHints)
            {
                if (!Uri.TryCreate(hint, UriKind.Absolute, out var hintUri) || hintUri.Scheme != "https")
                    throw new InvalidOperationException($"Authority hint '{hint}' must be a valid HTTPS URL");
            }
        }

        Metadata?.Validate();
        Constraints?.Validate();

        if (TrustMarks != null)
        {
            foreach (var trustMark in TrustMarks)
            {
                trustMark?.Validate();
            }
        }
    }

    /// <summary>
    /// Creates a basic entity configuration with required fields.
    /// </summary>
    /// <param name="entityUrl">The entity URL (used for both issuer and subject)</param>
    /// <param name="jwkSet">The entity's public key set</param>
    /// <param name="validityHours">Validity period in hours (default: 24)</param>
    /// <returns>A new EntityConfiguration instance</returns>
    public static EntityConfiguration Create(string entityUrl, object jwkSet, int validityHours = 24)
    {
        if (string.IsNullOrWhiteSpace(entityUrl))
            throw new ArgumentException("Entity URL cannot be null or empty", nameof(entityUrl));

        if (!Uri.TryCreate(entityUrl, UriKind.Absolute, out var uri) || uri.Scheme != "https")
            throw new ArgumentException("Entity URL must be a valid HTTPS URL", nameof(entityUrl));

        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        return new EntityConfiguration
        {
            Issuer = entityUrl,
            Subject = entityUrl,
            IssuedAt = now,
            ExpiresAt = now + (validityHours * 3600),
            JwkSet = jwkSet
        };
    }
}
