using System.Text.Json.Serialization;

namespace SdJwt.Net.VcDm.Models;

/// <summary>
/// Represents a W3C VCDM 2.0 <c>evidence</c> entry documenting how the issuer
/// verified the subject's claims. MUST have a <c>type</c>; additional properties
/// are evidence-method specific.
/// </summary>
public class Evidence
{
    /// <summary>
    /// Optional URI identifying this specific evidence document.
    /// </summary>
    [JsonPropertyName("id")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public string? Id { get; set; }

    /// <summary>
    /// The evidence type identifier(s). REQUIRED. Array or single string.
    /// </summary>
    [JsonPropertyName("type")]
    public string[] Type { get; set; } = [];

    /// <summary>
    /// Additional evidence-method-specific properties.
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, object>? AdditionalProperties { get; set; }
}
