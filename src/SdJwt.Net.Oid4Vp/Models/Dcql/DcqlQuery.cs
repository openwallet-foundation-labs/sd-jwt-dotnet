using System.Text.Json.Serialization;

namespace SdJwt.Net.Oid4Vp.Models.Dcql;

/// <summary>
/// Represents the root DCQL query object as defined in OID4VP 1.0 Section 6.
/// The DCQL (Digital Credentials Query Language) query is used in the <c>dcql_query</c>
/// parameter of an Authorization Request to specify which credentials the verifier is requesting.
/// </summary>
public class DcqlQuery {
        /// <summary>
        /// Gets or sets the list of credential queries.
        /// REQUIRED. At least one credential query must be provided.
        /// Each entry specifies a type of credential the verifier is requesting.
        /// </summary>
        [JsonPropertyName("credentials")]
        public DcqlCredentialQuery[] Credentials { get; set; } = Array.Empty<DcqlCredentialQuery>();

        /// <summary>
        /// Gets or sets the credential set queries.
        /// OPTIONAL. Defines logical groupings and alternative combinations of
        /// the credential queries in <see cref="Credentials"/>.
        /// If absent, all credential queries are considered independently required.
        /// </summary>
        [JsonPropertyName("credential_sets")]
#if NET6_0_OR_GREATER
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
        public DcqlCredentialSetQuery[]? CredentialSets { get; set; }

        /// <summary>
        /// Validates this DCQL query according to OID4VP 1.0 Section 6.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the query is invalid.</exception>
        public void Validate() {
                if (Credentials == null || Credentials.Length == 0)
                        throw new InvalidOperationException("DCQL query 'credentials' must contain at least one credential query.");

                foreach (var credential in Credentials)
                        credential.Validate();

                if (CredentialSets != null) {
                        foreach (var set in CredentialSets)
                                set.Validate();
                }
        }
}
