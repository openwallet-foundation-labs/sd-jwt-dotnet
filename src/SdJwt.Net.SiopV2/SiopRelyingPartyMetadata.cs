using System.Text.Json.Serialization;

namespace SdJwt.Net.SiopV2;

/// <summary>
/// Relying Party metadata values relevant to SIOPv2.
/// </summary>
public class SiopRelyingPartyMetadata
{
    /// <summary>
    /// Gets or sets supported subject syntax types.
    /// </summary>
    [JsonPropertyName("subject_syntax_types_supported")]
    public string[] SubjectSyntaxTypesSupported { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets supported ID Token signing algorithms.
    /// </summary>
    [JsonPropertyName("id_token_signing_alg_values_supported")]
    public string[] IdTokenSigningAlgValuesSupported { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets supported ID Token types.
    /// </summary>
    [JsonPropertyName("id_token_types_supported")]
    public string[] IdTokenTypesSupported { get; set; } = Array.Empty<string>();
}
