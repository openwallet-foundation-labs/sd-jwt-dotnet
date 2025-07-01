using System.Text.Json.Serialization;

namespace SdJwt.Net.Models;

/// <summary>
/// Represents the payload of a Verifiable Credential as defined in the SD-JWT-VC specification.
/// Standard JWT claims like 'iss', 'sub', 'iat', and 'exp' are expected to be inside this object,
/// not at the top level of the SD-JWT.
/// </summary>
public class VerifiableCredentialPayload
{
    /// <summary>
    /// The issuer of the credential.
    /// </summary>
    [JsonPropertyName("iss")]
    public string? Issuer { get; set; }

    /// <summary>
    /// The subject of the credential.
    /// </summary>
    [JsonPropertyName("sub")]
    public string? Subject { get; set; }

    /// <summary>
    /// The time at which the credential was issued (Unix time).
    /// </summary>
    [JsonPropertyName("iat")]
    public long? IssuedAt { get; set; }

    /// <summary>
    /// The expiration time of the credential (Unix time).
    /// </summary>
    [JsonPropertyName("exp")]
    public long? ExpirationTime { get; set; }

    /// <summary>
    /// The confirmation method, typically containing the Holder's public key (Jwk).
    /// </summary>
    [JsonPropertyName("cnf")]
    public object? Confirmation { get; set; }

    /// <summary>
    /// A link to a Status List for revocation checking.
    /// </summary>
    [JsonPropertyName("status")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public StatusClaim? Status { get; set; }

    /// <summary>
    /// A dictionary of claims about the subject. This is where the main
    /// user-facing claims (e.g., given_name, birthdate) are stored.
    /// </summary>
    [JsonPropertyName("credential_subject")]
    public Dictionary<string, object> CredentialSubject { get; set; } = [];

    /// <summary>
    /// A dictionary to capture any other custom claims within the Verifiable Credential.
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, object> AdditionalData { get; set; } = [];
}