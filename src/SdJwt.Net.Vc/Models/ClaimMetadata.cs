using System.Text.Json.Serialization;

namespace SdJwt.Net.Vc.Models;

/// <summary>
/// Represents claim metadata within Type Metadata.
/// </summary>
public class ClaimMetadata
{
    /// <summary>
    /// Gets or sets the path to the claim being described.
    /// Required. Array indicating the claim or claims being addressed.
    /// </summary>
    [JsonPropertyName("path")]
    public object[]? Path
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets display information for the claim.
    /// Optional. Array of display metadata for different locales.
    /// </summary>
    [JsonPropertyName("display")]
    public DisplayMetadata[]? Display
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets whether the claim must be present in issued credentials.
    /// Optional. Default is false if omitted.
    /// </summary>
    [JsonPropertyName("mandatory")]
    public bool? Mandatory
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the selective disclosure requirement for the claim.
    /// Optional. Values: "always", "allowed", "never". Default is "allowed".
    /// </summary>
    [JsonPropertyName("sd")]
    public string? SelectiveDisclosure
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the SVG template ID for this claim.
    /// Optional. Used for SVG rendering templates.
    /// </summary>
    [JsonPropertyName("svg_id")]
    public string? SvgId
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets any additional claim metadata properties.
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, object>? AdditionalData
    {
        get; set;
    }
}
