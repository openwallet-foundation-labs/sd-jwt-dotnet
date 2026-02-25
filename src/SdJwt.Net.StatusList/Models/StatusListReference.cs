using System.Text.Json.Serialization;

namespace SdJwt.Net.StatusList.Models;

/// <summary>
/// Represents the status_list reference object within the status claim.
/// </summary>
public class StatusListReference {
        /// <summary>
        /// Gets or sets the index of this referenced token within the status list.
        /// Required. Must be a non-negative integer that represents the index
        /// to check for status information in the Status List for the current Referenced Token.
        /// </summary>
        [JsonPropertyName("idx")]
        public int Index { get; set; }

        /// <summary>
        /// Gets or sets the URI of the Status List Token.
        /// Required. String value that identifies the Status List Token 
        /// containing the status information for the Referenced Token.
        /// The value must be a URI conforming to RFC3986.
        /// </summary>
        [JsonPropertyName("uri")]
        public string Uri { get; set; } = string.Empty;

        /// <summary>
        /// Validates the status list reference according to draft-ietf-oauth-status-list-13.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when validation fails</exception>
        public void Validate() {
                if (string.IsNullOrWhiteSpace(Uri))
                        throw new InvalidOperationException("Status list URI is required");

                if (!System.Uri.TryCreate(Uri, UriKind.Absolute, out _))
                        throw new InvalidOperationException("Status list URI must be a valid absolute URI");

                if (Index < 0)
                        throw new InvalidOperationException("Status list index must be non-negative");
        }
}