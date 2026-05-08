using System.Text.Json.Serialization;

namespace SdJwt.Net.Oid4Vci.Models.Formats;

/// <summary>
/// Credential configuration for the <c>ldp_vc</c> format per OID4VCI 1.0 Section 14.1.2.
/// Represents a W3C Verifiable Credential with Data Integrity proof and JSON-LD context.
/// </summary>
public class LdpVcCredentialConfiguration : CredentialConfiguration
{
    /// <summary>
    /// Initializes a new instance with <c>format</c> set to <c>ldp_vc</c>.
    /// </summary>
    public LdpVcCredentialConfiguration()
    {
        Format = Oid4VciConstants.LdpVcFormat;
    }

    /// <summary>
    /// Gets or sets the credential definition specifying the JSON-LD context and type array.
    /// REQUIRED. Contains both <c>@context</c> and <c>type</c> arrays processed using JSON-LD rules.
    /// </summary>
    [JsonPropertyName("credential_definition")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public LdpVcCredentialDefinition? CredentialDefinition
    {
        get; set;
    }
}

/// <summary>
/// Credential definition for <c>ldp_vc</c> format containing the JSON-LD context and type array.
/// </summary>
public class LdpVcCredentialDefinition
{
    /// <summary>
    /// Gets or sets the JSON-LD context URLs.
    /// REQUIRED. Array of context URLs (e.g., <c>["https://www.w3.org/2018/credentials/v1", ...]</c>).
    /// </summary>
    [JsonPropertyName("@context")]
    public string[] Context { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets the W3C VC type values.
    /// REQUIRED. Array of type strings (e.g., <c>["VerifiableCredential", "UniversityDegreeCredential"]</c>).
    /// </summary>
    [JsonPropertyName("type")]
    public string[] Type { get; set; } = Array.Empty<string>();
}
