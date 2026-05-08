using System.Text.Json.Serialization;

namespace SdJwt.Net.Oid4Vci.Models.Formats;

/// <summary>
/// Credential configuration for the <c>dc+sd-jwt</c> format per OID4VCI 1.0 Section 14.4.
/// </summary>
public class SdJwtVcCredentialConfiguration : CredentialConfiguration
{
    /// <summary>
    /// Initializes a new instance with <c>format</c> set to <c>dc+sd-jwt</c>.
    /// </summary>
    public SdJwtVcCredentialConfiguration()
    {
        Format = Oid4VciConstants.SdJwtVcFormat;
    }

    /// <summary>
    /// Gets or sets the verifiable credential type URL.
    /// REQUIRED. Identifies the SD-JWT VC type (e.g., <c>https://credentials.example.com/identity_credential</c>).
    /// </summary>
    [JsonPropertyName("vct")]
    public string Vct { get; set; } = string.Empty;
}
