using System.Text.Json.Serialization;

namespace SdJwt.Net.OidFederation.Models;

/// <summary>
/// Represents an Entity Statement JWT as defined in OpenID Federation 1.0.
/// This is an endorsement signed by a superior entity about a subordinate entity.
/// </summary>
public class EntityStatement
{
    /// <summary>
    /// Gets or sets the issuer identifier (the superior entity making the statement).
    /// Required. Must be a valid HTTPS URL.
    /// </summary>
    [JsonPropertyName("iss")]
    public string Issuer { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the subject identifier (the subordinate entity being described).
    /// Required. Must be a valid HTTPS URL and different from the issuer.
    /// </summary>
    [JsonPropertyName("sub")]
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the time at which the entity statement was issued.
    /// Required. Unix timestamp.
    /// </summary>
    [JsonPropertyName("iat")]
    public long IssuedAt { get; set; }

    /// <summary>
    /// Gets or sets the expiration time of the entity statement.
    /// Required. Unix timestamp. Must be after iat.
    /// </summary>
    [JsonPropertyName("exp")]
    public long ExpiresAt { get; set; }

    /// <summary>
    /// Gets or sets the JSON Web Key Set containing the subject's allowed public keys.
    /// Optional. Constrains which keys the subject is allowed to use.
    /// </summary>
    [JsonPropertyName("jwks")]
    public object? JwkSet { get; set; }

    /// <summary>
    /// Gets or sets the metadata policy that applies to the subject entity.
    /// Optional. Defines constraints and requirements for the subject's metadata.
    /// </summary>
    [JsonPropertyName("metadata_policy")]
    public MetadataPolicy? MetadataPolicy { get; set; }

    /// <summary>
    /// Gets or sets the constraints that apply to the subject entity.
    /// Optional. Defines operational limitations for the subject.
    /// </summary>
    [JsonPropertyName("constraints")]
    public EntityConstraints? Constraints { get; set; }

    /// <summary>
    /// Gets or sets the trust marks issued to the subject entity.
    /// Optional. Trust marks granted by this superior entity.
    /// </summary>
    [JsonPropertyName("trust_marks")]
    public TrustMark[]? TrustMarks { get; set; }

    /// <summary>
    /// Gets or sets additional critical parameters.
    /// Optional. Parameters that must be understood by the recipient.
    /// </summary>
    [JsonPropertyName("crit")]
    public string[]? Critical { get; set; }

    /// <summary>
    /// Gets or sets the authority hints for the subject entity.
    /// Optional. Overrides or supplements the subject's own authority hints.
    /// </summary>
    [JsonPropertyName("authority_hints")]
    public string[]? AuthorityHints { get; set; }

    /// <summary>
    /// Gets or sets the source endpoint where this statement was obtained.
    /// Optional. Used for auditing and verification purposes.
    /// </summary>
    [JsonPropertyName("source_endpoint")]
    public string? SourceEndpoint { get; set; }

    /// <summary>
    /// Validates the entity statement according to OpenID Federation 1.0 requirements.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the statement is invalid</exception>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Issuer))
            throw new InvalidOperationException("Issuer is required");

        if (string.IsNullOrWhiteSpace(Subject))
            throw new InvalidOperationException("Subject is required");

        if (Issuer == Subject)
            throw new InvalidOperationException("Issuer and Subject must be different for entity statements");

        if (!Uri.TryCreate(Issuer, UriKind.Absolute, out var issuerUri) || issuerUri.Scheme != "https")
            throw new InvalidOperationException("Issuer must be a valid HTTPS URL");

        if (!Uri.TryCreate(Subject, UriKind.Absolute, out var subjectUri) || subjectUri.Scheme != "https")
            throw new InvalidOperationException("Subject must be a valid HTTPS URL");

        if (IssuedAt <= 0)
            throw new InvalidOperationException("IssuedAt must be a valid Unix timestamp");

        if (ExpiresAt <= IssuedAt)
            throw new InvalidOperationException("ExpiresAt must be after IssuedAt");

        if (AuthorityHints != null)
        {
            foreach (var hint in AuthorityHints)
            {
                if (!Uri.TryCreate(hint, UriKind.Absolute, out var hintUri) || hintUri.Scheme != "https")
                    throw new InvalidOperationException($"Authority hint '{hint}' must be a valid HTTPS URL");
            }
        }

        if (!string.IsNullOrWhiteSpace(SourceEndpoint))
        {
            if (!Uri.TryCreate(SourceEndpoint, UriKind.Absolute, out var sourceUri) || sourceUri.Scheme != "https")
                throw new InvalidOperationException("SourceEndpoint must be a valid HTTPS URL");
        }

        MetadataPolicy?.Validate();
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
    /// Creates a basic entity statement with required fields.
    /// </summary>
    /// <param name="issuerUrl">The superior entity URL</param>
    /// <param name="subjectUrl">The subordinate entity URL</param>
    /// <param name="validityHours">Validity period in hours (default: 24)</param>
    /// <returns>A new EntityStatement instance</returns>
    public static EntityStatement Create(string issuerUrl, string subjectUrl, int validityHours = 24)
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        
        return new EntityStatement
        {
            Issuer = issuerUrl,
            Subject = subjectUrl,
            IssuedAt = now,
            ExpiresAt = now + (validityHours * 3600)
        };
    }
}