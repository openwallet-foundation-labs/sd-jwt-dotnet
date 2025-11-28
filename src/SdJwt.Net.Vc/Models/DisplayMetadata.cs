using System.Text.Json.Serialization;

namespace SdJwt.Net.Vc.Models;

/// <summary>
/// Represents display metadata for a credential type or claim.
/// </summary>
public class DisplayMetadata
{
    /// <summary>
    /// Gets or sets the language tag for this display metadata.
    /// Required. Must be a valid language tag as defined in RFC5646.
    /// </summary>
    [JsonPropertyName("locale")]
    public string? Locale { get; set; }

    /// <summary>
    /// Gets or sets the human-readable name.
    /// Required for credential display, optional for claim display.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the human-readable label (for claims).
    /// Required for claim display metadata.
    /// </summary>
    [JsonPropertyName("label")]
    public string? Label { get; set; }

    /// <summary>
    /// Gets or sets the human-readable description.
    /// Optional. Intended for end users.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets rendering information for the credential type.
    /// Optional. Contains rendering-specific metadata.
    /// </summary>
    [JsonPropertyName("rendering")]
    public object? Rendering { get; set; }

    /// <summary>
    /// Gets or sets any additional display properties.
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, object>? AdditionalData { get; set; }
}