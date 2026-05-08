using System.Text.Json.Serialization;

namespace SdJwt.Net.Oid4Vp.Models.Dcql;

/// <summary>
/// Specifies a trusted authority constraint within a <see cref="DcqlCredentialQuery"/>.
/// Per OID4VP 1.0 Section 7, restricts accepted credentials to those issued under the
/// specified trust root.
/// </summary>
public class TrustedAuthority
{
    /// <summary>
    /// Gets or sets the authority type.
    /// REQUIRED. One of <c>aki</c>, <c>etsi_tl</c>, or <c>openid_federation</c>.
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the authority values.
    /// REQUIRED. Authority identifiers whose meaning depends on <see cref="Type"/>:
    /// <c>aki</c> = base64url-encoded Authority Key Identifiers;
    /// <c>etsi_tl</c> = ETSI trust list URLs;
    /// <c>openid_federation</c> = entity identifiers.
    /// </summary>
    [JsonPropertyName("values")]
    public string[] Values { get; set; } = Array.Empty<string>();
}
