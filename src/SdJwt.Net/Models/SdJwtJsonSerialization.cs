using System.Text.Json.Serialization;

namespace SdJwt.Net.Models;

/// <summary>
/// Represents an SD-JWT in JWS JSON Serialization format (RFC 9901 Section 8)
/// </summary>
public class SdJwtJsonSerialization
{
    /// <summary>
    /// The JWS payload (base64url-encoded)
    /// </summary>
    [JsonPropertyName("payload")]
    public string Payload { get; set; } = string.Empty;

    /// <summary>
    /// The protected header (base64url-encoded)
    /// </summary>
    [JsonPropertyName("protected")]
    public string Protected { get; set; } = string.Empty;

    /// <summary>
    /// The signature (base64url-encoded)
    /// </summary>
    [JsonPropertyName("signature")]
    public string Signature { get; set; } = string.Empty;

    /// <summary>
    /// Unprotected header containing disclosures and optional key binding JWT
    /// </summary>
    [JsonPropertyName("header")]
    public SdJwtUnprotectedHeader Header { get; set; } = new();
}

/// <summary>
/// Represents the unprotected header for JWS JSON Serialization
/// </summary>
public class SdJwtUnprotectedHeader
{
    /// <summary>
    /// Array of disclosures (base64url-encoded)
    /// </summary>
    [JsonPropertyName("disclosures")]
    public string[] Disclosures { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Optional Key Binding JWT for SD-JWT+KB
    /// </summary>
    [JsonPropertyName("kb_jwt")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? KbJwt { get; set; }

    /// <summary>
    /// Additional header parameters (extensibility)
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, object>? AdditionalData { get; set; }
}

/// <summary>
/// Represents an SD-JWT in JWS JSON General Serialization format
/// </summary>
public class SdJwtGeneralJsonSerialization
{
    /// <summary>
    /// The JWS payload (base64url-encoded)
    /// </summary>
    [JsonPropertyName("payload")]
    public string Payload { get; set; } = string.Empty;

    /// <summary>
    /// Array of signature objects
    /// </summary>
    [JsonPropertyName("signatures")]
    public SdJwtSignature[] Signatures { get; set; } = Array.Empty<SdJwtSignature>();
}

/// <summary>
/// Represents a signature object in General JSON Serialization
/// </summary>
public class SdJwtSignature
{
    /// <summary>
    /// The protected header (base64url-encoded)
    /// </summary>
    [JsonPropertyName("protected")]
    public string Protected { get; set; } = string.Empty;

    /// <summary>
    /// The signature (base64url-encoded)
    /// </summary>
    [JsonPropertyName("signature")]
    public string Signature { get; set; } = string.Empty;

    /// <summary>
    /// Unprotected header (disclosures and kb_jwt only in first signature)
    /// </summary>
    [JsonPropertyName("header")]
    public SdJwtUnprotectedHeader Header { get; set; } = new();
}