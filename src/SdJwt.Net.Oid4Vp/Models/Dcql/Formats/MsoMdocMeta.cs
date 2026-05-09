using System.Text.Json.Serialization;

namespace SdJwt.Net.Oid4Vp.Models.Dcql.Formats;

/// <summary>
/// Format-specific metadata for <c>mso_mdoc</c> credential queries per OID4VP 1.0 Section 7.
/// Constrains which mdoc document types are accepted.
/// </summary>
public class MsoMdocMeta : IDcqlMeta
{
    /// <summary>
    /// Gets or sets the required mdoc doctype.
    /// REQUIRED. The wallet MUST present an mdoc whose doctype matches this value
    /// (e.g., <c>org.iso.18013.5.1.mDL</c>).
    /// </summary>
    [JsonPropertyName("doctype_value")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public string? DoctypeValue
    {
        get; set;
    }
}
