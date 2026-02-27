using System.Text.Json.Serialization;

namespace SdJwt.Net.Vc.Models;

/// <summary>
/// Represents a JSON Web Key Set.
/// Contains an array of JWK objects.
/// </summary>
public class JwkSet
{
    /// <summary>
    /// Gets or sets the array of JWK objects.
    /// Required. Array containing the public keys.
    /// </summary>
    [JsonPropertyName("keys")]
    public JsonWebKey[]? Keys
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets any additional JWK Set properties.
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, object>? AdditionalData
    {
        get; set;
    }
}
