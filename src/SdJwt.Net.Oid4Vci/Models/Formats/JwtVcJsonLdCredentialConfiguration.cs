using System.Text.Json.Serialization;

namespace SdJwt.Net.Oid4Vci.Models.Formats;

/// <summary>
/// Credential configuration for the <c>jwt_vc_json-ld</c> format per OID4VCI 1.0.
/// Represents a W3C Verifiable Credential encoded as a JWT using JSON-LD context and type data.
/// </summary>
public class JwtVcJsonLdCredentialConfiguration : CredentialConfiguration
{
    /// <summary>
    /// Initializes a new instance with <c>format</c> set to <c>jwt_vc_json-ld</c>.
    /// </summary>
    public JwtVcJsonLdCredentialConfiguration()
    {
        Format = Oid4VciConstants.JwtVcJsonLdFormat;
    }

    /// <summary>
    /// Gets or sets the credential definition specifying the JSON-LD context and type array.
    /// REQUIRED. Contains both <c>@context</c> and <c>type</c> arrays.
    /// </summary>
    [JsonPropertyName("credential_definition")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public JwtVcJsonLdCredentialDefinition? CredentialDefinition
    {
        get; set;
    }
}

/// <summary>
/// Credential definition for <c>jwt_vc_json-ld</c> format containing the JSON-LD context and type array.
/// </summary>
public class JwtVcJsonLdCredentialDefinition
{
    /// <summary>
    /// Gets or sets the JSON-LD context URLs.
    /// REQUIRED. Array of context URLs.
    /// </summary>
    [JsonPropertyName("@context")]
    public string[] Context { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets the W3C VC type values.
    /// REQUIRED. Array of type strings.
    /// </summary>
    [JsonPropertyName("type")]
    public string[] Type { get; set; } = Array.Empty<string>();
}
