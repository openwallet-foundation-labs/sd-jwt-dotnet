using System.Text.Json.Serialization;

namespace SdJwt.Net.Oid4Vp.Models.Dcql.Formats;

/// <summary>
/// Format-specific metadata for W3C VC credential queries (<c>jwt_vc_json</c>, <c>ldp_vc</c>)
/// per OID4VP 1.0 Section 7.
/// Constrains which W3C VC type arrays are accepted.
/// </summary>
public class W3cVcMeta : IDcqlMeta
{
    /// <summary>
    /// Gets or sets the accepted W3C VC type combinations.
    /// REQUIRED. Each inner array is one acceptable <c>type</c> array the presented credential
    /// may have. A credential matches if its <c>type</c> array contains all expanded IRIs
    /// listed in at least one inner array.
    /// </summary>
    [JsonPropertyName("type_values")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public string[][]? TypeValues
    {
        get; set;
    }
}
