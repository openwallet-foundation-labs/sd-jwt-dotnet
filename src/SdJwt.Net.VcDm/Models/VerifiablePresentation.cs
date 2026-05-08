using System.Text.Json.Serialization;
using SdJwt.Net.VcDm.Serialization;

namespace SdJwt.Net.VcDm.Models;

/// <summary>
/// W3C Verifiable Credentials Data Model 2.0 presentation document.
/// Used in OID4VP to wrap one or more verifiable credentials addressed to a verifier.
///
/// Note: <c>holder</c> is a role concept in VCDM 2.0, not a JSON property.
/// Holder identity is expressed via the <c>proof</c> object (challenge/domain binding).
///
/// Spec: https://www.w3.org/TR/vc-data-model-2.0/#presentations
/// </summary>
public class VerifiablePresentation
{
    /// <summary>
    /// JSON-LD context. REQUIRED. First entry MUST be <see cref="VcDmContexts.V2"/>.
    /// </summary>
    [JsonPropertyName("@context")]
    public string[] Context { get; set; } = [VcDmContexts.V2];

    /// <summary>
    /// Presentation type array. REQUIRED. MUST contain "VerifiablePresentation".
    /// </summary>
    [JsonPropertyName("type")]
    public string[] Type { get; set; } = ["VerifiablePresentation"];

    /// <summary>
    /// Optional URL uniquely identifying this presentation instance.
    /// </summary>
    [JsonPropertyName("id")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public string? Id { get; set; }

    /// <summary>
    /// The verifiable credentials included in this presentation.
    /// Each element is the secured credential string (JWT) or object (ldp_vc).
    /// May be omitted for holder-only proof presentations.
    /// </summary>
    [JsonPropertyName("verifiableCredential")]
    [JsonConverter(typeof(SingleOrArrayConverter<object>))]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public object[]? VerifiableCredential { get; set; }

    /// <summary>
    /// Embedded Data Integrity proof(s) binding the presentation to the verifier.
    /// For <c>ldp_vc</c> flow; in JWT flow the proof is in the JWT signature.
    /// The <c>challenge</c> MUST equal the OID4VP request <c>nonce</c>;
    /// <c>domain</c> MUST equal <c>client_id</c>.
    /// </summary>
    [JsonPropertyName("proof")]
    [JsonConverter(typeof(SingleOrArrayConverter<DataIntegrityProof>))]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public DataIntegrityProof[]? Proof { get; set; }

    /// <summary>
    /// Usage restrictions imposed by the holder on this presentation.
    /// </summary>
    [JsonPropertyName("termsOfUse")]
    [JsonConverter(typeof(SingleOrArrayConverter<TermsOfUse>))]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public TermsOfUse[]? TermsOfUse { get; set; }

    /// <summary>
    /// Returns true when the presentation type array contains "VerifiablePresentation".
    /// </summary>
    public bool IsVerifiablePresentation() =>
        Type.Contains("VerifiablePresentation");

    /// <summary>
    /// Returns true when the presentation contains a VCDM 2.0 base context.
    /// </summary>
    public bool HasV2Context() =>
        Context.Any(c => c == VcDmContexts.V2);
}
