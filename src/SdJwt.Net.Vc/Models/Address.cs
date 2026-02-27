using System.Text.Json.Serialization;

namespace SdJwt.Net.Vc.Models;

/// <summary>
/// Represents an address structure commonly used in credentials.
/// </summary>
public class Address
{
    /// <summary>
    /// Gets or sets the street address.
    /// Optional. Can be selectively disclosed.
    /// </summary>
    [JsonPropertyName("street_address")]
    public string? StreetAddress
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the locality (city).
    /// Optional. Can be selectively disclosed.
    /// </summary>
    [JsonPropertyName("locality")]
    public string? Locality
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the region (state/province).
    /// Optional. Can be selectively disclosed.
    /// </summary>
    [JsonPropertyName("region")]
    public string? Region
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the postal code.
    /// Optional. Can be selectively disclosed.
    /// </summary>
    [JsonPropertyName("postal_code")]
    public string? PostalCode
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the country.
    /// Optional. Can be selectively disclosed.
    /// </summary>
    [JsonPropertyName("country")]
    public string? Country
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets any additional address properties.
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, object>? AdditionalData
    {
        get; set;
    }
}
