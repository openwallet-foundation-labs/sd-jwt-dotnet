using System.Text.Json.Serialization;

namespace SdJwt.Net.VcDm.Models;

/// <summary>
/// Represents a W3C VCDM 2.0 <c>credentialSchema</c> entry.
/// References a machine-readable schema for structural validation of the credential.
/// </summary>
public class CredentialSchema
{
    /// <summary>
    /// URL of the schema document. REQUIRED.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Schema type identifier (e.g., "JsonSchema", "JsonSchemaCredential"). REQUIRED.
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Additional properties present in the schema object.
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, object>? AdditionalProperties { get; set; }
}
