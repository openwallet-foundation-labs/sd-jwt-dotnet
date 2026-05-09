using System.Text.Json.Serialization;

namespace SdJwt.Net.VcDm.Models;

/// <summary>
/// Represents a W3C VCDM 2.0 <c>refreshService</c> entry.
/// Provides a mechanism by which the subject may refresh the credential.
/// MUST have both <c>id</c> and <c>type</c>.
/// </summary>
public class RefreshService
{
    /// <summary>
    /// URL of the service that can issue a refreshed credential. REQUIRED.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// The refresh service type identifier. REQUIRED.
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Additional type-specific properties.
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, object>? AdditionalProperties
    {
        get; set;
    }
}
