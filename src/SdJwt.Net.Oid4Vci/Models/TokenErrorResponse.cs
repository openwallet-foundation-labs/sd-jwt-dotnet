using System.Text.Json.Serialization;

namespace SdJwt.Net.Oid4Vci.Models;

/// <summary>
/// Represents a Token Error Response according to OID4VCI 1.0 Section 6.3.
/// </summary>
public class TokenErrorResponse {
        /// <summary>
        /// Gets or sets the error code.
        /// REQUIRED. A single ASCII error code.
        /// </summary>
        [JsonPropertyName("error")]
        public string Error { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the error description.
        /// OPTIONAL. Human-readable ASCII text providing additional information.
        /// </summary>
        [JsonPropertyName("error_description")]
#if NET6_0_OR_GREATER
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
        public string? ErrorDescription { get; set; }

        /// <summary>
        /// Gets or sets the error URI.
        /// OPTIONAL. A URI identifying a human-readable web page with information about the error.
        /// </summary>
        [JsonPropertyName("error_uri")]
#if NET6_0_OR_GREATER
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
        public string? ErrorUri { get; set; }

        /// <summary>
        /// Creates a token error response with the specified error code.
        /// </summary>
        /// <param name="error">The error code</param>
        /// <param name="description">Optional error description</param>
        /// <param name="errorUri">Optional error URI</param>
        /// <returns>A new TokenErrorResponse instance</returns>
        public static TokenErrorResponse Create(string error, string? description = null, string? errorUri = null) {
#if NET6_0_OR_GREATER
                ArgumentException.ThrowIfNullOrWhiteSpace(error);
#else
        if (string.IsNullOrWhiteSpace(error))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(error));
#endif

                return new TokenErrorResponse {
                        Error = error,
                        ErrorDescription = description,
                        ErrorUri = errorUri
                };
        }
}