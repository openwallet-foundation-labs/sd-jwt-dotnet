using System.Text.Json.Serialization;

namespace SdJwt.Net.Vc.Models;

/// <summary>
/// Represents a JSON Web Key for use in confirmation methods.
/// Simplified representation of JWK for common key types.
/// </summary>
public class JsonWebKey
{
    /// <summary>
    /// Gets or sets the key type.
    /// Required. E.g., "EC", "RSA", "oct".
    /// </summary>
    [JsonPropertyName("kty")]
    public string? KeyType { get; set; }

    /// <summary>
    /// Gets or sets the curve for elliptic curve keys.
    /// Required for EC keys. E.g., "P-256", "P-384", "P-521".
    /// </summary>
    [JsonPropertyName("crv")]
    public string? Curve { get; set; }

    /// <summary>
    /// Gets or sets the x coordinate for elliptic curve keys.
    /// Required for EC keys.
    /// </summary>
    [JsonPropertyName("x")]
    public string? X { get; set; }

    /// <summary>
    /// Gets or sets the y coordinate for elliptic curve keys.
    /// Required for EC keys.
    /// </summary>
    [JsonPropertyName("y")]
    public string? Y { get; set; }

    /// <summary>
    /// Gets or sets the key use.
    /// Optional. E.g., "sig", "enc".
    /// </summary>
    [JsonPropertyName("use")]
    public string? Use { get; set; }

    /// <summary>
    /// Gets or sets the key operations.
    /// Optional. Array of key operation values.
    /// </summary>
    [JsonPropertyName("key_ops")]
    public string[]? KeyOperations { get; set; }

    /// <summary>
    /// Gets or sets the algorithm intended for use with the key.
    /// Optional. E.g., "ES256", "RS256".
    /// </summary>
    [JsonPropertyName("alg")]
    public string? Algorithm { get; set; }

    /// <summary>
    /// Gets or sets the key ID.
    /// Optional. Key identifier.
    /// </summary>
    [JsonPropertyName("kid")]
    public string? KeyId { get; set; }

    /// <summary>
    /// Gets or sets any additional JWK properties.
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, object>? AdditionalData { get; set; }
}