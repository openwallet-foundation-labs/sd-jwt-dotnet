using System.Text.Json.Serialization;

namespace SdJwt.Net.Oid4Vp.Models.Dcql.Formats;

/// <summary>
/// Format-specific metadata for <c>dc+sd-jwt</c> credential queries per OID4VP 1.0 Section 7.
/// Constrains which SD-JWT VC types are accepted.
/// </summary>
public class SdJwtVcMeta : IDcqlMeta
{
    /// <summary>
    /// Gets or sets the accepted verifiable credential type URLs.
    /// REQUIRED. Array of acceptable <c>vct</c> values. The wallet MUST present a credential
    /// whose <c>vct</c> claim matches one of these values.
    /// </summary>
    [JsonPropertyName("vct_values")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public string[]? VctValues
    {
        get; set;
    }
}
