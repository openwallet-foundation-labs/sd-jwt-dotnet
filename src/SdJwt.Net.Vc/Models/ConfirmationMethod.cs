using System.Text.Json.Serialization;

namespace SdJwt.Net.Vc.Models;

/// <summary>
/// Represents the confirmation method for key binding in SD-JWT VCs.
/// Used in the 'cnf' claim to specify the proof-of-possession key.
/// </summary>
public class ConfirmationMethod
{
    /// <summary>
    /// Gets or sets the JSON Web Key for proof of possession.
    /// Optional. JWK as defined in RFC7800 Section 3.2.
    /// </summary>
    [JsonPropertyName("jwk")]
    public object? Jwk { get; set; }

    /// <summary>
    /// Gets or sets the JWK Set URL.
    /// Optional. URL pointing to a JWK Set containing the key.
    /// </summary>
    [JsonPropertyName("jku")]
    public string? JwkSetUrl { get; set; }

    /// <summary>
    /// Gets or sets the key identifier.
    /// Optional. Key ID that can be used to locate the key.
    /// </summary>
    [JsonPropertyName("kid")]
    public string? KeyId { get; set; }

    /// <summary>
    /// Gets or sets any additional confirmation method properties.
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, object>? AdditionalData { get; set; }
}