using System.Text.Json.Serialization;

namespace SdJwt.Net.Oid4Vci.Models;

/// <summary>
/// Represents a transaction code configuration for pre-authorized flows.
/// According to OID4VCI 1.0 Section 4.1.1.
/// </summary>
public class TransactionCode {
        /// <summary>
        /// Gets or sets the length of the transaction code.
        /// OPTIONAL. Length of the PIN in characters.
        /// </summary>
        [JsonPropertyName("length")]
#if NET6_0_OR_GREATER
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
        public int? Length { get; set; }

        /// <summary>
        /// Gets or sets the input mode for the transaction code.
        /// OPTIONAL. Defines the type of characters the wallet is expected to enter.
        /// Possible values: "numeric", "text"
        /// </summary>
        [JsonPropertyName("input_mode")]
#if NET6_0_OR_GREATER
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
        public string? InputMode { get; set; }

        /// <summary>
        /// Gets or sets a human-readable description for the transaction code.
        /// OPTIONAL. Description of the transaction code.
        /// </summary>
        [JsonPropertyName("description")]
#if NET6_0_OR_GREATER
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
        public string? Description { get; set; }
}