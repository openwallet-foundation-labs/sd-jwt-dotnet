using System.Text.Json.Serialization;

namespace SdJwt.Net.VcDm.Models;

/// <summary>
/// Represents the W3C VCDM 2.0 <c>credentialSubject</c> property.
/// Contains an optional subject identifier and arbitrary credential claims.
/// </summary>
public class CredentialSubject
{
    /// <summary>
    /// Optional URI identifying the subject. May be a DID, URL, or other URI.
    /// </summary>
    [JsonPropertyName("id")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public string? Id { get; set; }

    /// <summary>
    /// Domain-specific credential claims (e.g., degree, name, jobTitle).
    /// Serialized alongside <see cref="Id"/> as flat JSON properties.
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, object>? AdditionalClaims { get; set; }
}
