using System.Text.Json.Serialization;

namespace SdJwt.Net.Vc.Models;

/// <summary>
/// Represents display metadata for a credential type or an individual claim, per draft-ietf-oauth-sd-jwt-vc-16.
/// This class is shared between two distinct contexts defined by the spec:
/// <list type="bullet">
///   <item><description>
///     <b>Type display metadata</b> (Section 4.5): populated by <see cref="TypeMetadata.Display"/>.
///     Relevant properties: <see cref="Locale"/>, <see cref="Name"/>, <see cref="Description"/>, <see cref="Rendering"/>.
///   </description></item>
///   <item><description>
///     <b>Claim display metadata</b> (Section 4.6.2): populated by <see cref="ClaimMetadata.Display"/>.
///     Relevant properties: <see cref="Locale"/>, <see cref="Label"/>, <see cref="Description"/>.
///   </description></item>
/// </list>
/// </summary>
public class DisplayMetadata
{
    /// <summary>
    /// Gets or sets the language tag for this display metadata.
    /// Required in both type and claim display contexts. Must be a valid RFC 5646 language tag.
    /// </summary>
    [JsonPropertyName("locale")]
    public string? Locale
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the human-readable name for the credential type.
    /// Required in type display metadata (Section 4.5). Not used in claim display metadata.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the human-readable label for a claim.
    /// Required in claim display metadata (Section 4.6.2). Not used in type display metadata.
    /// </summary>
    [JsonPropertyName("label")]
    public string? Label
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the human-readable description.
    /// Optional in both type and claim display contexts. Intended for end users.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets rendering information for the credential type.
    /// Optional. Used only in type display metadata (Section 4.5.1). Not applicable to claim display metadata.
    /// </summary>
    [JsonPropertyName("rendering")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public RenderingMetadata? Rendering
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets any additional display properties not covered by this class.
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, object>? AdditionalData
    {
        get; set;
    }
}
