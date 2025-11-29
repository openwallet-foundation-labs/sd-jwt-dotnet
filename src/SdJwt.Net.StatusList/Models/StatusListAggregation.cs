using System.Text.Json.Serialization;

namespace SdJwt.Net.StatusList.Models;

/// <summary>
/// Represents the Status List Aggregation structure.
/// Contains a list of Status List Token URIs published by an Issuer.
/// </summary>
public class StatusListAggregation
{
    /// <summary>
    /// Gets or sets the array of Status List Token URIs.
    /// Required. JSON array of strings that contains URIs linking to Status List Tokens.
    /// </summary>
    [JsonPropertyName("status_lists")]
    public string[] StatusLists { get; set; } = Array.Empty<string>();
}