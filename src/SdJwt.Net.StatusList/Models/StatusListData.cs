using System.Text.Json.Serialization;

namespace SdJwt.Net.StatusList.Models;

/// <summary>
/// Represents the Status List data structure within the Status List Token.
/// Contains status information of many Referenced Tokens represented by one or multiple bits.
/// </summary>
public class StatusList
{
    /// <summary>
    /// Gets or sets the number of bits per Referenced Token in the compressed byte array.
    /// Required. The allowed values for bits are 1, 2, 4 and 8.
    /// </summary>
    [JsonPropertyName("bits")]
    public int Bits { get; set; }

    /// <summary>
    /// Gets or sets the status values for all Referenced Tokens.
    /// Required. Base64url-encoded compressed byte array as specified in the specification.
    /// </summary>
    [JsonPropertyName("lst")]
    public string List { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the aggregation URI.
    /// Optional. URI to retrieve the Status List Aggregation for this type of Referenced Token or Issuer.
    /// </summary>
    [JsonPropertyName("aggregation_uri")]
    public string? AggregationUri { get; set; }
}