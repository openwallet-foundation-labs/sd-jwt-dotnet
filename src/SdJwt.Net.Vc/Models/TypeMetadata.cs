using System.Text.Json.Serialization;

namespace SdJwt.Net.Vc.Models;

/// <summary>
/// Represents the structure for Type Metadata as defined in draft-ietf-oauth-sd-jwt-vc-14.
/// Type Metadata defines information about the credential type and how credentials are displayed.
/// </summary>
public class TypeMetadata
{
    /// <summary>
    /// Gets or sets the verifiable credential type described by this metadata document.
    /// Required. Must match the vct value in the SD-JWT VC.
    /// </summary>
    [JsonPropertyName("vct")]
    public string? Vct
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets a human-readable name for the type.
    /// Optional. Intended for developers reading the JSON document.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets a human-readable description for the type.
    /// Optional. Intended for developers reading the JSON document.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the URI of another type that this type extends.
    /// Optional. Used for type inheritance and extension.
    /// </summary>
    [JsonPropertyName("extends")]
    public string? Extends
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the integrity hash of the extended type metadata.
    /// Optional. Used when extends is present for integrity protection.
    /// </summary>
    [JsonPropertyName("extends#integrity")]
    public string? ExtendsIntegrity
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets display information for the credential type.
    /// Optional. Array of objects containing display information for different locales.
    /// </summary>
    [JsonPropertyName("display")]
    public DisplayMetadata[]? Display
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets claim information for the credential type.
    /// Optional. Array of objects containing claim metadata.
    /// </summary>
    [JsonPropertyName("claims")]
    public ClaimMetadata[]? Claims
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets any additional properties in the Type Metadata document.
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, object>? AdditionalData
    {
        get; set;
    }
}
