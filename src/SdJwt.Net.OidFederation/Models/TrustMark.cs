using System.Text.Json.Serialization;

namespace SdJwt.Net.OidFederation.Models;

/// <summary>
/// Represents a Trust Mark as defined in OpenID Federation 1.0.
/// Trust Marks are assertions about an entity's compliance with specific policies or standards.
/// </summary>
public class TrustMark {
        /// <summary>
        /// Gets or sets the trust mark identifier.
        /// Required. Unique identifier for the trust mark type.
        /// </summary>
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        /// <summary>
        /// Gets or sets the trust mark value or JWT.
        /// Required. Either a simple value or a signed JWT containing the trust mark.
        /// </summary>
        [JsonPropertyName("trust_mark")]
        public string? TrustMarkValue { get; set; }

        /// <summary>
        /// Gets or sets when the trust mark was issued.
        /// Optional. Unix timestamp when the trust mark was granted.
        /// </summary>
        [JsonPropertyName("iat")]
        public long? IssuedAt { get; set; }

        /// <summary>
        /// Gets or sets when the trust mark expires.
        /// Optional. Unix timestamp when the trust mark expires.
        /// </summary>
        [JsonPropertyName("exp")]
        public long? ExpiresAt { get; set; }

        /// <summary>
        /// Gets or sets the issuer of the trust mark.
        /// Optional. Entity that issued this trust mark.
        /// </summary>
        [JsonPropertyName("iss")]
        public string? Issuer { get; set; }

        /// <summary>
        /// Gets or sets the issuer of the trust mark.
        /// Alias for Issuer property for API compatibility.
        /// </summary>
        [JsonIgnore]
        public string? TrustMarkIssuer {
                get => Issuer;
                set => Issuer = value;
        }

        /// <summary>
        /// Gets or sets the subject of the trust mark.
        /// Optional. Entity that this trust mark applies to.
        /// </summary>
        [JsonPropertyName("sub")]
        public string? Subject { get; set; }

        /// <summary>
        /// Gets or sets additional trust mark attributes.
        /// Optional. Custom attributes specific to the trust mark type.
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, object>? AdditionalAttributes { get; set; }

        /// <summary>
        /// Validates the trust mark according to OpenID Federation 1.0 requirements.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the trust mark is invalid</exception>
        public void Validate() {
                if (string.IsNullOrWhiteSpace(Id))
                        throw new InvalidOperationException("Trust mark ID is required");

                if (string.IsNullOrWhiteSpace(TrustMarkValue))
                        throw new InvalidOperationException("Trust mark value is required");

                if (!string.IsNullOrWhiteSpace(Issuer)) {
                        if (!Uri.TryCreate(Issuer, UriKind.Absolute, out var issuerUri) || issuerUri.Scheme != "https")
                                throw new InvalidOperationException("Trust mark issuer must be a valid HTTPS URL");
                }

                if (!string.IsNullOrWhiteSpace(Subject)) {
                        if (!Uri.TryCreate(Subject, UriKind.Absolute, out var subjectUri) || subjectUri.Scheme != "https")
                                throw new InvalidOperationException("Trust mark subject must be a valid HTTPS URL");
                }

                if (IssuedAt.HasValue && ExpiresAt.HasValue && IssuedAt.Value >= ExpiresAt.Value)
                        throw new InvalidOperationException("ExpiresAt must be after IssuedAt");

                // Only check expiration if IssuedAt is present (complete timestamp info)
                if (IssuedAt.HasValue && ExpiresAt.HasValue && ExpiresAt.Value <= DateTimeOffset.UtcNow.ToUnixTimeSeconds())
                        throw new InvalidOperationException("Trust mark has expired");
        }

        /// <summary>
        /// Checks if the trust mark is currently valid (not expired).
        /// </summary>
        /// <returns>True if the trust mark is valid</returns>
        public bool IsValid() {
                try {
                        Validate();
                        return true;
                }
                catch {
                        return false;
                }
        }

        /// <summary>
        /// Checks if the trust mark has expired.
        /// </summary>
        /// <returns>True if the trust mark has expired</returns>
        public bool IsExpired() {
                if (!ExpiresAt.HasValue)
                        return false;

                return ExpiresAt.Value <= DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }

        /// <summary>
        /// Checks if the trust mark has expired, accounting for clock skew.
        /// </summary>
        /// <param name="clockSkew">The allowed clock skew</param>
        /// <returns>True if the trust mark has expired</returns>
        public bool IsExpired(TimeSpan clockSkew) {
                if (!ExpiresAt.HasValue)
                        return false;

                var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                var adjustedCurrentTime = currentTime - (long)clockSkew.TotalSeconds;

                return ExpiresAt.Value <= adjustedCurrentTime;
        }

        /// <summary>
        /// Creates a simple trust mark with basic information.
        /// </summary>
        /// <param name="id">The trust mark identifier</param>
        /// <param name="value">The trust mark value</param>
        /// <param name="issuer">Optional issuer URL</param>
        /// <param name="validityHours">Validity period in hours (default: no expiration)</param>
        /// <returns>A new TrustMark instance</returns>
        public static TrustMark Create(string id, string value, string? issuer = null, int? validityHours = null) {
                if (string.IsNullOrWhiteSpace(id))
                        throw new ArgumentException("Trust mark id cannot be null or empty", nameof(id));

                if (string.IsNullOrWhiteSpace(value))
                        throw new ArgumentException("Trust mark value cannot be null or empty", nameof(value));

                // Normalize empty/whitespace issuer to null
                if (string.IsNullOrWhiteSpace(issuer))
                        issuer = null;

                if (issuer != null) {
                        if (!Uri.TryCreate(issuer, UriKind.Absolute, out var issuerUri) || issuerUri.Scheme != "https")
                                throw new ArgumentException("Trust mark issuer must be a valid HTTPS URL", nameof(issuer));
                }

                var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                return new TrustMark {
                        Id = id,
                        TrustMarkValue = value,
                        Issuer = issuer,
                        IssuedAt = now,
                        ExpiresAt = validityHours.HasValue ? now + (validityHours.Value * 3600) : now + (24 * 3600) // Default 24 hours if not specified
                };
        }

        /// <summary>
        /// Returns a string representation of this trust mark.
        /// </summary>
        /// <returns>String representation</returns>
        public override string ToString() {
                return $"TrustMark(Id={Id}, Issuer={Issuer})";
        }
}

/// <summary>
/// Common trust mark identifiers used in OpenID Federation ecosystems.
/// </summary>
public static class CommonTrustMarks {
        /// <summary>
        /// Trust mark for entities that comply with eIDAS regulations.
        /// </summary>
        public const string EidasCompliant = "https://eidas.europa.eu/trustmark/compliant";

        /// <summary>
        /// Trust mark for entities that have undergone security certification.
        /// </summary>
        public const string SecurityCertified = "https://security.gov/trustmark/certified";

        /// <summary>
        /// Trust mark for educational institutions.
        /// </summary>
        public const string EducationalInstitution = "https://education.gov/trustmark/institution";

        /// <summary>
        /// Trust mark for financial institutions.
        /// </summary>
        public const string FinancialInstitution = "https://finance.gov/trustmark/institution";

        /// <summary>
        /// Trust mark for healthcare providers.
        /// </summary>
        public const string HealthcareProvider = "https://healthcare.gov/trustmark/provider";

        /// <summary>
        /// Trust mark for government agencies.
        /// </summary>
        public const string GovernmentAgency = "https://government.gov/trustmark/agency";
}