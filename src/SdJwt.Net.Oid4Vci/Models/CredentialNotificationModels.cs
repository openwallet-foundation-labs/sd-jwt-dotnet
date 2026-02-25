using System.Text.Json.Serialization;

namespace SdJwt.Net.Oid4Vci.Models;

/// <summary>
/// Represents a Credential Notification Request according to OID4VCI 1.0 Section 10.
/// This is sent to notify the issuer about credential acceptance or rejection.
/// </summary>
public class CredentialNotificationRequest {
        /// <summary>
        /// Gets or sets the notification identifier.
        /// REQUIRED. The notification ID received in the credential response.
        /// </summary>
        [JsonPropertyName("notification_id")]
        public string NotificationId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the event type.
        /// REQUIRED. The type of event being reported. 
        /// Possible values: "credential_accepted", "credential_failed", "credential_deleted".
        /// </summary>
        [JsonPropertyName("event")]
        public string Event { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the event description.
        /// OPTIONAL. Human-readable description of the event.
        /// </summary>
        [JsonPropertyName("event_description")]
#if NET6_0_OR_GREATER
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
        public string? EventDescription { get; set; }
}

/// <summary>
/// Represents a Credential Notification Response according to OID4VCI 1.0 Section 10.
/// This is returned after processing a notification request.
/// </summary>
public class CredentialNotificationResponse {
        /// <summary>
        /// Creates a successful notification response.
        /// </summary>
        /// <returns>A new CredentialNotificationResponse instance</returns>
        public static CredentialNotificationResponse Success() {
                return new CredentialNotificationResponse();
        }
}

/// <summary>
/// Contains constants for credential notification events.
/// </summary>
public static class CredentialNotificationEvents {
        /// <summary>
        /// Credential was successfully processed and accepted by the Wallet.
        /// </summary>
        public const string CredentialAccepted = "credential_accepted";

        /// <summary>
        /// Credential could not be processed.
        /// </summary>
        public const string CredentialFailed = "credential_failed";

        /// <summary>
        /// Credential was deleted from the Wallet.
        /// </summary>
        public const string CredentialDeleted = "credential_deleted";
}