using System.Text.Json.Serialization;

namespace SdJwt.Net.Oid4Vp.Models.Dcql;

/// <summary>
/// Represents a credential set query within a DCQL query as defined in OID4VP 1.0 Section 6.2.
/// Groups credential queries into alternative sets, allowing the verifier to express
/// that one of several combinations of credentials is acceptable.
/// </summary>
public class DcqlCredentialSetQuery {
        /// <summary>
        /// Gets or sets the alternative options for satisfying this credential set.
        /// REQUIRED. Each inner array is an alternative set of credential query IDs
        /// (references to <see cref="DcqlCredentialQuery.Id"/>) that, if all satisfied,
        /// fulfil this credential set requirement.
        /// At least one element is required.
        /// </summary>
        [JsonPropertyName("options")]
        public string[][] Options { get; set; } = Array.Empty<string[]>();

        /// <summary>
        /// Gets or sets whether this credential set is required.
        /// OPTIONAL. When <c>true</c> or absent (null), the wallet MUST satisfy at least
        /// one of the <see cref="Options"/>. When <c>false</c>, the credential set is
        /// optional and the wallet MAY skip it. Defaults to <c>true</c> per the spec.
        /// </summary>
        [JsonPropertyName("required")]
#if NET6_0_OR_GREATER
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
        public bool? Required { get; set; }

        /// <summary>
        /// Gets or sets a human-readable purpose for this credential set.
        /// OPTIONAL. Intended to be shown to the end user to explain why these
        /// credentials are being requested.
        /// </summary>
        [JsonPropertyName("purpose")]
#if NET6_0_OR_GREATER
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
        public object? Purpose { get; set; }

        /// <summary>
        /// Validates this credential set query according to OID4VP 1.0 Section 6.2.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the query is invalid.</exception>
        public void Validate() {
                if (Options == null || Options.Length == 0)
                        throw new InvalidOperationException("DCQL credential set query 'options' must contain at least one alternative.");
        }
}