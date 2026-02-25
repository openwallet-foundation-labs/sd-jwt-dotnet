using System.Text.Json.Serialization;

namespace SdJwt.Net.Oid4Vci.Models;

/// <summary>
/// Represents a Credential Response according to OID4VCI 1.0 Section 7.3.
/// This is the response body returned by the Issuer after a successful credential request.
/// </summary>
public class CredentialResponse {
        /// <summary>
        /// Gets or sets the issued credential.
        /// CONDITIONAL. The issued Credential. Required unless the acceptance_token parameter is present.
        /// Can be either a string (for JWT/SD-JWT) or an object (for JSON-LD).
        /// </summary>
        [JsonPropertyName("credential")]
#if NET6_0_OR_GREATER
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
        public object? Credential { get; set; }

        /// <summary>
        /// Gets or sets the credential format.
        /// OPTIONAL. The format of the issued credential.
        /// </summary>
        [JsonPropertyName("format")]
#if NET6_0_OR_GREATER
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
        public string? Format { get; set; }

        /// <summary>
        /// Gets or sets the acceptance token for deferred credential issuance.
        /// CONDITIONAL. Used for Deferred Credential Issuance. Required unless the credential parameter is present.
        /// </summary>
        [JsonPropertyName("acceptance_token")]
#if NET6_0_OR_GREATER
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
        public string? AcceptanceToken { get; set; }

        /// <summary>
        /// Gets or sets the credential nonce.
        /// OPTIONAL. A fresh nonce for the next credential request.
        /// </summary>
        [JsonPropertyName("c_nonce")]
#if NET6_0_OR_GREATER
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
        public string? CNonce { get; set; }

        /// <summary>
        /// Gets or sets the credential nonce expiration time.
        /// OPTIONAL. Lifetime of the c_nonce in seconds.
        /// </summary>
        [JsonPropertyName("c_nonce_expires_in")]
#if NET6_0_OR_GREATER
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
        public int? CNonceExpiresIn { get; set; }

        /// <summary>
        /// Gets or sets the notification identifier for the credential.
        /// OPTIONAL. Used for notification of credential acceptance or rejection.
        /// </summary>
        [JsonPropertyName("notification_id")]
#if NET6_0_OR_GREATER
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
        public string? NotificationId { get; set; }

        /// <summary>
        /// Gets or sets the transaction identifier for deferred issuance.
        /// </summary>
        [JsonIgnore]
        public string? TransactionId { get; set; }

        /// <summary>
        /// Validates the credential response according to OID4VCI 1.0 requirements.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the response is invalid</exception>
        public void Validate() {
                bool hasCredential = Credential != null;
                bool hasAcceptanceToken = !string.IsNullOrWhiteSpace(AcceptanceToken);

                if (!hasCredential && !hasAcceptanceToken)
                        throw new InvalidOperationException("Either credential or acceptance_token must be present");

                if (hasCredential && hasAcceptanceToken)
                        throw new InvalidOperationException("Cannot have both credential and acceptance_token");
        }

        /// <summary>
        /// Creates a successful credential response with an issued credential.
        /// </summary>
        /// <param name="credential">The issued SD-JWT credential</param>
        /// <param name="cNonce">Optional new nonce for subsequent requests</param>
        /// <param name="cNonceExpiresIn">Optional nonce expiration time in seconds</param>
        /// <param name="notificationId">Optional notification identifier</param>
        /// <returns>A new CredentialResponse instance</returns>
        public static CredentialResponse Success(object credential, string? cNonce = null, int? cNonceExpiresIn = null, string? notificationId = null) {
#if NET6_0_OR_GREATER
                ArgumentNullException.ThrowIfNull(credential);
#else
        if (credential == null)
            throw new ArgumentNullException(nameof(credential));
#endif

                return new CredentialResponse {
                        Credential = credential,
                        CNonce = cNonce,
                        CNonceExpiresIn = cNonceExpiresIn,
                        NotificationId = notificationId
                };
        }

        /// <summary>
        /// Creates a deferred credential response with an acceptance token.
        /// </summary>
        /// <param name="acceptanceToken">The acceptance token for later retrieval</param>
        /// <param name="cNonce">Optional new nonce for subsequent requests</param>
        /// <param name="cNonceExpiresIn">Optional nonce expiration time in seconds</param>
        /// <returns>A new CredentialResponse instance for deferred issuance</returns>
        public static CredentialResponse Deferred(string acceptanceToken, string? cNonce = null, int? cNonceExpiresIn = null) {
#if NET6_0_OR_GREATER
                ArgumentException.ThrowIfNullOrWhiteSpace(acceptanceToken);
#else
        if (string.IsNullOrWhiteSpace(acceptanceToken))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(acceptanceToken));
#endif

                return new CredentialResponse {
                        AcceptanceToken = acceptanceToken,
                        TransactionId = acceptanceToken, // Set TransactionId for backward compatibility
                        CNonce = cNonce,
                        CNonceExpiresIn = cNonceExpiresIn
                };
        }
}