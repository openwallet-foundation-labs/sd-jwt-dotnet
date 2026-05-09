using System.Text.Json.Serialization;

namespace SdJwt.Net.Oid4Vci.Models.Formats;

/// <summary>
/// Credential configuration for the <c>jwt_vc_json</c> format per OID4VCI 1.0 Section 14.1.1.
/// Represents a W3C Verifiable Credential encoded as a JWT without JSON-LD processing.
/// </summary>
public class JwtVcJsonCredentialConfiguration : CredentialConfiguration
{
    /// <summary>
    /// Initializes a new instance with <c>format</c> set to <c>jwt_vc_json</c>.
    /// </summary>
    public JwtVcJsonCredentialConfiguration()
    {
        Format = Oid4VciConstants.JwtVcJsonFormat;
    }

    /// <summary>
    /// Gets or sets the credential definition specifying the W3C VC type array.
    /// REQUIRED. Contains the <c>type</c> array that identifies the credential type.
    /// </summary>
    [JsonPropertyName("credential_definition")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public JwtVcJsonCredentialDefinition? CredentialDefinition
    {
        get; set;
    }
}

/// <summary>
/// Credential definition for <c>jwt_vc_json</c> format containing the VC type array.
/// </summary>
public class JwtVcJsonCredentialDefinition
{
    /// <summary>
    /// Gets or sets the W3C VC type values.
    /// REQUIRED. Array of type strings (e.g., <c>["VerifiableCredential", "UniversityDegreeCredential"]</c>).
    /// </summary>
    [JsonPropertyName("type")]
    public string[] Type { get; set; } = Array.Empty<string>();
}
