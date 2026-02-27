using System.Text.Json.Serialization;

namespace SdJwt.Net.Vc.Models;

/// <summary>
/// Represents the payload structure for an SD-JWT VC according to draft-ietf-oauth-sd-jwt-vc-13.
/// This is the structure of claims that can be included in the payload of SD-JWT VCs.
/// Note: This specification does not utilize the W3C Verifiable Credentials Data Model.
/// </summary>
public class SdJwtVcPayload
{
    /// <summary>
    /// Gets or sets the verifiable credential type identifier.
    /// Required. A case-sensitive string serving as an identifier for the type of the SD-JWT VC.
    /// Must be a Collision-Resistant Name as defined in RFC7515 Section 2.
    /// </summary>
    [JsonPropertyName("vct")]
    public string? Vct
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the hash of the Type Metadata document for integrity protection.
    /// Optional. Must be an "integrity metadata" string as defined in W3C Subresource Integrity.
    /// </summary>
    [JsonPropertyName("vct#integrity")]
    public string? VctIntegrity
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the issuer of the SD-JWT VC.
    /// Optional. Explicitly indicates the Issuer of the Verifiable Credential
    /// when it is not conveyed by other means (e.g., the subject of the end-entity certificate of an x5c header).
    /// </summary>
    [JsonPropertyName("iss")]
    public string? Issuer
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the subject identifier of the Verifiable Credential.
    /// Optional. The Issuer MAY use it to provide the Subject identifier known by the Issuer.
    /// Can be selectively disclosed.
    /// </summary>
    [JsonPropertyName("sub")]
    public string? Subject
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the time of issuance of the Verifiable Credential.
    /// Optional. Unix timestamp when the credential was issued.
    /// Can be selectively disclosed.
    /// </summary>
    [JsonPropertyName("iat")]
    public long? IssuedAt
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the time before which the Verifiable Credential must not be accepted.
    /// Optional. Unix timestamp for not-before validation.
    /// Cannot be selectively disclosed.
    /// </summary>
    [JsonPropertyName("nbf")]
    public long? NotBefore
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the expiry time of the Verifiable Credential.
    /// Optional. Unix timestamp after which the credential is no longer valid.
    /// Cannot be selectively disclosed.
    /// </summary>
    [JsonPropertyName("exp")]
    public long? ExpiresAt
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the confirmation method identifying the proof of possession key.
    /// Optional unless cryptographic Key Binding is to be supported, in which case it is required.
    /// Contains the confirmation method as defined in RFC7800.
    /// Cannot be selectively disclosed.
    /// </summary>
    [JsonPropertyName("cnf")]
    public object? Confirmation
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the status information for the Verifiable Credential.
    /// Optional. Information on how to read the status of the Verifiable Credential.
    /// When present and using status_list mechanism, the Status List Token must be in JWT format.
    /// Cannot be selectively disclosed.
    /// </summary>
    [JsonPropertyName("status")]
    public object? Status
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets any additional public and private claims.
    /// These can be selectively disclosed based on the credential type requirements.
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, object>? AdditionalData
    {
        get; set;
    }
}
