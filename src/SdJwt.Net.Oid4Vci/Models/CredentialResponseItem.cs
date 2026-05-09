using System.Text.Json.Serialization;

namespace SdJwt.Net.Oid4Vci.Models;

/// <summary>
/// Represents a single issued credential within a <see cref="CredentialResponse.Credentials"/> array
/// according to OID4VCI 1.0 Section 8.3.
/// </summary>
public class CredentialResponseItem
{
    /// <summary>
    /// Gets or sets the issued credential.
    /// REQUIRED. The credential value. For JWT/SD-JWT formats this is a string; for ldp_vc it is a JSON object.
    /// </summary>
    [JsonPropertyName("credential")]
    public object Credential { get; set; } = string.Empty;
}
