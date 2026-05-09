using System.Text.Json.Serialization;

namespace SdJwt.Net.VcDm.Models;

/// <summary>
/// Represents a W3C Data Integrity <c>proof</c> embedded in an <c>ldp_vc</c> credential.
/// This class models the proof metadata only — cryptographic verification requires
/// a Data Integrity suite library (not included in SdJwt.Net.VcDm).
/// </summary>
public class DataIntegrityProof
{
    /// <summary>
    /// The proof type, e.g. "DataIntegrityProof". REQUIRED.
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = "DataIntegrityProof";

    /// <summary>
    /// The cryptographic suite identifier, e.g. "ecdsa-rdfc-2019", "ecdsa-sd-2023", "bbs-2023".
    /// </summary>
    [JsonPropertyName("cryptosuite")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public string? Cryptosuite
    {
        get; set;
    }

    /// <summary>
    /// ISO 8601 datetime string when the proof was created.
    /// </summary>
    [JsonPropertyName("created")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public string? Created
    {
        get; set;
    }

    /// <summary>
    /// URI of the verification method (key) used to create this proof.
    /// </summary>
    [JsonPropertyName("verificationMethod")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public string? VerificationMethod
    {
        get; set;
    }

    /// <summary>
    /// The purpose of this proof, e.g. "assertionMethod", "authentication".
    /// </summary>
    [JsonPropertyName("proofPurpose")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public string? ProofPurpose
    {
        get; set;
    }

    /// <summary>
    /// The domain value bound into the proof (used in presentations).
    /// </summary>
    [JsonPropertyName("domain")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public string? Domain
    {
        get; set;
    }

    /// <summary>
    /// The challenge value bound into the proof (used in presentations).
    /// </summary>
    [JsonPropertyName("challenge")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public string? Challenge
    {
        get; set;
    }

    /// <summary>
    /// The base64url-encoded proof value.
    /// </summary>
    [JsonPropertyName("proofValue")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public string? ProofValue
    {
        get; set;
    }

    /// <summary>
    /// Additional proof-type-specific properties.
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, object>? AdditionalProperties
    {
        get; set;
    }
}
