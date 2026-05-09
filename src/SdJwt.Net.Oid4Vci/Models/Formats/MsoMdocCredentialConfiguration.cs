using System.Text.Json.Serialization;

namespace SdJwt.Net.Oid4Vci.Models.Formats;

/// <summary>
/// Credential configuration for the <c>mso_mdoc</c> format per OID4VCI 1.0 Section 14.3.
/// </summary>
public class MsoMdocCredentialConfiguration : CredentialConfiguration
{
    /// <summary>
    /// Initializes a new instance with <c>format</c> set to <c>mso_mdoc</c>.
    /// </summary>
    public MsoMdocCredentialConfiguration()
    {
        Format = Oid4VciConstants.MsoMdocFormat;
    }

    /// <summary>
    /// Gets or sets the mdoc document type.
    /// REQUIRED. Identifies the document type (e.g., <c>org.iso.18013.5.1.mDL</c>).
    /// </summary>
    [JsonPropertyName("doctype")]
    public string Doctype { get; set; } = string.Empty;
}
