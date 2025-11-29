using System.Text.Json.Serialization;

namespace SdJwt.Net.Oid4Vci.Models;

/// <summary>
/// Represents the proof object in a credential request according to OID4VCI 1.0 Section 7.2.1.
/// </summary>
public class CredentialProof
{
    /// <summary>
    /// Gets or sets the proof type.
    /// REQUIRED. Denotes the key proof(s) that the Credential Issuer requires the Wallet to provide 
    /// with the Credential Request. Supported value: "jwt".
    /// </summary>
    [JsonPropertyName("proof_type")]
    public string ProofType { get; set; } = "jwt";

    /// <summary>
    /// Gets or sets the JWT proof string.
    /// REQUIRED when proof_type is "jwt". String containing a JWT as defined in Section 7.2.1.1.
    /// </summary>
    [JsonPropertyName("jwt")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public string? Jwt { get; set; }

    /// <summary>
    /// Gets or sets the CWT proof string.
    /// REQUIRED when proof_type is "cwt". String containing a CWT.
    /// </summary>
    [JsonPropertyName("cwt")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public string? Cwt { get; set; }

    /// <summary>
    /// Gets or sets the LDAP proof string.
    /// REQUIRED when proof_type is "ldp_vp". String containing a W3C Verifiable Presentation.
    /// </summary>
    [JsonPropertyName("ldp_vp")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public object? LdpVp { get; set; }
}