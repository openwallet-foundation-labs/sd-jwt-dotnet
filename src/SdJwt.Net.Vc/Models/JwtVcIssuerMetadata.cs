using System.Text.Json.Serialization;

namespace SdJwt.Net.Vc.Models;

/// <summary>
/// Represents JWT VC Issuer Metadata as defined in draft-ietf-oauth-sd-jwt-vc-13.
/// Used for retrieving issuer's public keys and metadata.
/// </summary>
public class JwtVcIssuerMetadata {
        /// <summary>
        /// Gets or sets the issuer identifier.
        /// Required. Must be identical to the iss value in the JWT.
        /// </summary>
        [JsonPropertyName("issuer")]
        public string? Issuer { get; set; }

        /// <summary>
        /// Gets or sets the JWK Set URI.
        /// Optional. URL referencing the Issuer's JWK Set document.
        /// Must not be present if jwks is present.
        /// </summary>
        [JsonPropertyName("jwks_uri")]
        public string? JwksUri { get; set; }

        /// <summary>
        /// Gets or sets the JWK Set.
        /// Optional. Issuer's JSON Web Key Set document.
        /// Must not be present if jwks_uri is present.
        /// </summary>
        [JsonPropertyName("jwks")]
        public JwkSet? Jwks { get; set; }

        /// <summary>
        /// Gets or sets any additional issuer metadata properties.
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, object>? AdditionalData { get; set; }
}