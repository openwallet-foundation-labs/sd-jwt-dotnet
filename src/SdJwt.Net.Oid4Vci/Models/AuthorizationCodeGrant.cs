using System.Text.Json.Serialization;

namespace SdJwt.Net.Oid4Vci.Models;

/// <summary>
/// Represents authorization code grant configuration according to OID4VCI 1.0 Section 4.1.1.
/// </summary>
public class AuthorizationCodeGrant {
        /// <summary>
        /// Gets or sets the issuer state parameter.
        /// OPTIONAL. String value created by the Credential Issuer and opaque to the Wallet 
        /// that is used to maintain state between the request and callback.
        /// </summary>
        [JsonPropertyName("issuer_state")]
#if NET6_0_OR_GREATER
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
        public string? IssuerState { get; set; }

        /// <summary>
        /// Gets or sets the authorization server endpoint.
        /// OPTIONAL. String that is a URL identifying the Authorization Server.
        /// </summary>
        [JsonPropertyName("authorization_server")]
#if NET6_0_OR_GREATER
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
        public string? AuthorizationServer { get; set; }
}