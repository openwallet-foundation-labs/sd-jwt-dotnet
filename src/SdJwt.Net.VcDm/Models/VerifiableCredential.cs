using System.Text.Json.Serialization;
using SdJwt.Net.VcDm.Serialization;

namespace SdJwt.Net.VcDm.Models;

/// <summary>
/// W3C Verifiable Credentials Data Model 2.0 credential document.
/// Represents either an unsecured credential (plain JSON-LD document) or the
/// inner payload of a secured verifiable credential (<c>jwt_vc_json</c> / <c>ldp_vc</c>).
///
/// Spec: https://www.w3.org/TR/vc-data-model-2.0/
/// </summary>
public class VerifiableCredential
{
    /// <summary>
    /// JSON-LD context. REQUIRED. First entry MUST be <see cref="VcDmContexts.V2"/>.
    /// Additional contexts may follow to define domain-specific terms.
    /// </summary>
    [JsonPropertyName("@context")]
    public string[] Context { get; set; } = [VcDmContexts.V2];

    /// <summary>
    /// Credential type array. REQUIRED. MUST contain "VerifiableCredential".
    /// Additional type strings identify the specific credential profile.
    /// </summary>
    [JsonPropertyName("type")]
    public string[] Type { get; set; } = ["VerifiableCredential"];

    /// <summary>
    /// Optional URL uniquely identifying this credential instance.
    /// </summary>
    [JsonPropertyName("id")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public string? Id { get; set; }

    /// <summary>
    /// The entity that issued this credential. REQUIRED.
    /// May be a plain URL string or an object with <c>id</c>, <c>name</c>, and <c>description</c>.
    /// </summary>
    [JsonPropertyName("issuer")]
    public Issuer Issuer { get; set; } = null!;

    /// <summary>
    /// The date-time from which the credential is valid (replaces VCDM 1.1 <c>issuanceDate</c>).
    /// Serialized as ISO 8601 string.
    /// </summary>
    [JsonPropertyName("validFrom")]
    [JsonConverter(typeof(Iso8601DateTimeOffsetConverter))]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public DateTimeOffset? ValidFrom { get; set; }

    /// <summary>
    /// The date-time after which the credential is no longer valid (replaces VCDM 1.1 <c>expirationDate</c>).
    /// </summary>
    [JsonPropertyName("validUntil")]
    [JsonConverter(typeof(Iso8601DateTimeOffsetConverter))]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public DateTimeOffset? ValidUntil { get; set; }

    /// <summary>
    /// The claim(s) about the subject(s) of the credential. REQUIRED.
    /// May be a single object or an array; stored as array here for uniformity.
    /// </summary>
    [JsonPropertyName("credentialSubject")]
    [JsonConverter(typeof(SingleOrArrayConverter<CredentialSubject>))]
    public CredentialSubject[] CredentialSubject { get; set; } = [];

    /// <summary>
    /// Credential status information (e.g., revocation, suspension).
    /// May be a single entry or an array.
    /// </summary>
    [JsonPropertyName("credentialStatus")]
    [JsonConverter(typeof(SingleOrArrayConverter<CredentialStatus>))]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public CredentialStatus[]? CredentialStatus { get; set; }

    /// <summary>
    /// Schema reference(s) for structural validation of this credential.
    /// </summary>
    [JsonPropertyName("credentialSchema")]
    [JsonConverter(typeof(SingleOrArrayConverter<CredentialSchema>))]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public CredentialSchema[]? CredentialSchema { get; set; }

    /// <summary>
    /// Usage restriction(s) imposed by the issuer on this credential.
    /// </summary>
    [JsonPropertyName("termsOfUse")]
    [JsonConverter(typeof(SingleOrArrayConverter<TermsOfUse>))]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public TermsOfUse[]? TermsOfUse { get; set; }

    /// <summary>
    /// Evidence documenting how the issuer verified the subject's claims.
    /// </summary>
    [JsonPropertyName("evidence")]
    [JsonConverter(typeof(SingleOrArrayConverter<Evidence>))]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public Evidence[]? Evidence { get; set; }

    /// <summary>
    /// Service endpoint that can issue a refreshed credential.
    /// </summary>
    [JsonPropertyName("refreshService")]
    [JsonConverter(typeof(SingleOrArrayConverter<RefreshService>))]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public RefreshService[]? RefreshService { get; set; }

    /// <summary>
    /// Rendering hint(s) for wallet / verifier UI display.
    /// </summary>
    [JsonPropertyName("renderMethod")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public object[]? RenderMethod { get; set; }

    /// <summary>
    /// Optional human-readable name for this credential.
    /// </summary>
    [JsonPropertyName("name")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public string? Name { get; set; }

    /// <summary>
    /// Optional human-readable description of this credential.
    /// </summary>
    [JsonPropertyName("description")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public string? Description { get; set; }

    // ------------------------------------------------------------------
    // Data Integrity proof (ldp_vc securing mechanism)
    // ------------------------------------------------------------------

    /// <summary>
    /// Embedded Data Integrity proof(s) — present only in <c>ldp_vc</c> secured credentials.
    /// Cryptographic verification requires a separate Data Integrity suite library.
    /// </summary>
    [JsonPropertyName("proof")]
    [JsonConverter(typeof(SingleOrArrayConverter<DataIntegrityProof>))]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public DataIntegrityProof[]? Proof { get; set; }

    // ------------------------------------------------------------------
    // VCDM 1.1 backward-compatibility read-only properties
    // ------------------------------------------------------------------

    /// <summary>
    /// VCDM 1.1 <c>issuanceDate</c> — deprecated; accepted when reading older credentials.
    /// Maps to <see cref="ValidFrom"/> during deserialization.
    /// </summary>
    [Obsolete("issuanceDate is deprecated in VCDM 2.0. Use ValidFrom.")]
    [JsonPropertyName("issuanceDate")]
    [JsonConverter(typeof(Iso8601DateTimeOffsetConverter))]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public DateTimeOffset? IssuanceDate
    {
        get => null; // never write
        set { if (ValidFrom is null) ValidFrom = value; }
    }

    /// <summary>
    /// VCDM 1.1 <c>expirationDate</c> — deprecated; accepted when reading older credentials.
    /// Maps to <see cref="ValidUntil"/> during deserialization.
    /// </summary>
    [Obsolete("expirationDate is deprecated in VCDM 2.0. Use ValidUntil.")]
    [JsonPropertyName("expirationDate")]
    [JsonConverter(typeof(Iso8601DateTimeOffsetConverter))]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public DateTimeOffset? ExpirationDate
    {
        get => null; // never write
        set { if (ValidUntil is null) ValidUntil = value; }
    }

    // ------------------------------------------------------------------
    // Helpers
    // ------------------------------------------------------------------

    /// <summary>
    /// Returns the first (or only) credential subject.
    /// </summary>
    public CredentialSubject? GetPrimarySubject() =>
        CredentialSubject.Length > 0 ? CredentialSubject[0] : null;

    /// <summary>
    /// Returns true when the credential contains a VCDM 2.0 base context.
    /// </summary>
    public bool HasV2Context() =>
        Context.Any(c => c == VcDmContexts.V2);

    /// <summary>
    /// Returns true when the credential type array contains "VerifiableCredential".
    /// </summary>
    public bool IsVerifiableCredential() =>
        Type.Contains("VerifiableCredential");
}
