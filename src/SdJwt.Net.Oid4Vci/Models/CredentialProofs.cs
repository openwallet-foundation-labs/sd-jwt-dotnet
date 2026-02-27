using System.Text.Json.Serialization;

namespace SdJwt.Net.Oid4Vci.Models;

/// <summary>
/// Represents the OID4VCI <c>proofs</c> container for multiple proof values by proof type.
/// </summary>
public class CredentialProofs {
        /// <summary>
        /// Gets or sets JWT proofs.
        /// </summary>
        [JsonPropertyName("jwt")]
#if NET6_0_OR_GREATER
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
        public string[]? Jwt { get; set; }

        /// <summary>
        /// Gets or sets CWT proofs.
        /// </summary>
        [JsonPropertyName("cwt")]
#if NET6_0_OR_GREATER
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
        public string[]? Cwt { get; set; }

        /// <summary>
        /// Gets or sets Linked Data proof presentations.
        /// </summary>
        [JsonPropertyName("ldp_vp")]
#if NET6_0_OR_GREATER
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
        public object[]? LdpVp { get; set; }

        /// <summary>
        /// Validates the proofs container.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the container is invalid.</exception>
        public void Validate() {
                var hasJwt = Jwt != null && Jwt.Length > 0;
                var hasCwt = Cwt != null && Cwt.Length > 0;
                var hasLdpVp = LdpVp != null && LdpVp.Length > 0;

                if (!hasJwt && !hasCwt && !hasLdpVp) {
                        throw new InvalidOperationException("At least one proof entry is required in proofs.");
                }

                if (hasJwt && Jwt!.Any(string.IsNullOrWhiteSpace)) {
                        throw new InvalidOperationException("JWT proofs must not contain empty entries.");
                }

                if (hasCwt && Cwt!.Any(string.IsNullOrWhiteSpace)) {
                        throw new InvalidOperationException("CWT proofs must not contain empty entries.");
                }

                if (hasLdpVp && LdpVp!.Any(v => v == null)) {
                        throw new InvalidOperationException("LDP VP proofs must not contain null entries.");
                }
        }
}
