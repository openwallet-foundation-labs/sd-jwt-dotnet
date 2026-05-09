using System.Text.Json.Serialization;

namespace SdJwt.Net.VcDm.Models;

/// <summary>
/// Represents a W3C VCDM 2.0 <c>termsOfUse</c> entry.
/// Specifies usage conditions imposed by the issuer or holder.
/// All objects MUST have a <c>type</c>; additional properties are format-specific.
/// </summary>
public class TermsOfUse
{
    /// <summary>
    /// The terms-of-use policy type identifier. REQUIRED.
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Optional URI identifying this specific policy instance.
    /// </summary>
    [JsonPropertyName("id")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public string? Id
    {
        get; set;
    }

    /// <summary>
    /// Additional type-specific properties.
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, object>? AdditionalProperties
    {
        get; set;
    }
}
