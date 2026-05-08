using System.Text.Json.Serialization;

namespace SdJwt.Net.Oid4Vci.Models;

/// <summary>
/// Represents the OID4VCI <c>proofs</c> container for multiple proof values keyed by proof type,
/// per OID4VCI 1.0 Section 13. Each property is an array of proof values of the corresponding type.
/// </summary>
public class CredentialProofs
{
    /// <summary>
    /// Gets or sets JWT proofs (<c>openid4vci-proof+jwt</c>).
    /// Each element is a compact JWT string.
    /// </summary>
    [JsonPropertyName("jwt")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public string[]? Jwt
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets Data Integrity VP proofs.
    /// Each element is a W3C Verifiable Presentation with a <c>DataIntegrityProof</c>
    /// where <c>challenge</c> = <c>c_nonce</c> and <c>domain</c> = Credential Issuer identifier.
    /// </summary>
    [JsonPropertyName("di_vp")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public object[]? DiVp
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets key attestation proofs.
    /// Each element is a Key Attestation JWT (<c>typ: key-attestation+jwt</c>).
    /// Used to attest hardware security properties of the key without a separate possession proof.
    /// </summary>
    [JsonPropertyName("attestation")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public string[]? Attestation
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets CWT proofs.
    /// </summary>
    [JsonPropertyName("cwt")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public string[]? Cwt
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets Linked Data proof presentations.
    /// </summary>
    [JsonPropertyName("ldp_vp")]
    [Obsolete("Use DiVp. The ldp_vp proof type is superseded by di_vp in OID4VCI 1.0 final.")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public object[]? LdpVp
    {
        get; set;
    }

    /// <summary>
    /// Validates the proofs container.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the container is invalid.</exception>
    public void Validate()
    {
        var hasJwt = Jwt != null && Jwt.Length > 0;
        var hasDiVp = DiVp != null && DiVp.Length > 0;
        var hasAttestation = Attestation != null && Attestation.Length > 0;
        var hasCwt = Cwt != null && Cwt.Length > 0;
#pragma warning disable CS0618
        var hasLdpVp = LdpVp != null && LdpVp.Length > 0;
#pragma warning restore CS0618

        if (!hasJwt && !hasDiVp && !hasAttestation && !hasCwt && !hasLdpVp)
            throw new InvalidOperationException("At least one proof entry is required in proofs.");

        if (hasJwt && Jwt!.Any(string.IsNullOrWhiteSpace))
            throw new InvalidOperationException("JWT proofs must not contain empty entries.");

        if (hasCwt && Cwt!.Any(string.IsNullOrWhiteSpace))
            throw new InvalidOperationException("CWT proofs must not contain empty entries.");

        if (hasAttestation && Attestation!.Any(string.IsNullOrWhiteSpace))
            throw new InvalidOperationException("Attestation proofs must not contain empty entries.");

        if (hasDiVp && DiVp!.Any(v => v == null))
            throw new InvalidOperationException("di_vp proofs must not contain null entries.");

#pragma warning disable CS0618
        if (hasLdpVp && LdpVp!.Any(v => v == null))
            throw new InvalidOperationException("LDP VP proofs must not contain null entries.");
#pragma warning restore CS0618
    }
}
