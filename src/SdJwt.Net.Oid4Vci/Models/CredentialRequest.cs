using System.Text.Json.Serialization;

namespace SdJwt.Net.Oid4Vci.Models;

/// <summary>
/// Represents a Credential Request according to OID4VCI 1.0 Section 7.2.
/// This is the request body sent by the Wallet to the Issuer's /credential endpoint.
/// </summary>
public class CredentialRequest
{
    /// <summary>
    /// Gets or sets the credential format.
    /// REQUIRED. Format of the Credential to be issued.
    /// This implementation supports "vc+sd-jwt" for SD-JWT credentials.
    /// </summary>
    [JsonPropertyName("format")]
    public string Format { get; set; } = Oid4VciConstants.SdJwtVcFormat;

    /// <summary>
    /// Gets or sets the verifiable credential type.
    /// CONDITIONAL. Required when format is "vc+sd-jwt".
    /// String designating the type of the Credential.
    /// </summary>
    [JsonPropertyName("vct")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public string? Vct { get; set; }

    /// <summary>
    /// Gets or sets the credential definition.
    /// CONDITIONAL. Contains the detailed description of the credential type.
    /// Used when the Credential Issuer supports dynamic credential types.
    /// </summary>
    [JsonPropertyName("credential_definition")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public object? CredentialDefinition { get; set; }

    /// <summary>
    /// Gets or sets the credential identifier.
    /// CONDITIONAL. String identifying a Credential Configuration supported by the Credential Issuer.
    /// Must not be present if credential_definition is present.
    /// </summary>
    [JsonPropertyName("credential_identifier")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public string? CredentialIdentifier { get; set; }

    /// <summary>
    /// Gets or sets the proof of possession.
    /// CONDITIONAL. Contains proof of possession of the cryptographic key material.
    /// Required unless the Credential Issuer decided upon credential delivery method other than 
    /// requiring the Wallet to prove possession of a cryptographic key.
    /// </summary>
    [JsonPropertyName("proof")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public CredentialProof? Proof { get; set; }

    /// <summary>
    /// Gets or sets the credential response encryption parameters.
    /// OPTIONAL. Contains information for encrypting the Credential Response.
    /// </summary>
    [JsonPropertyName("credential_response_encryption")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public object? CredentialResponseEncryption { get; set; }

    /// <summary>
    /// Creates a new credential request for SD-JWT credentials with the specified parameters.
    /// </summary>
    /// <param name="vct">The verifiable credential type</param>
    /// <param name="proofJwt">The proof of possession JWT</param>
    /// <returns>A new CredentialRequest instance</returns>
    public static CredentialRequest Create(string vct, string proofJwt)
    {
#if NET6_0_OR_GREATER
        ArgumentException.ThrowIfNullOrWhiteSpace(vct);
        ArgumentException.ThrowIfNullOrWhiteSpace(proofJwt);
#else
        if (string.IsNullOrWhiteSpace(vct))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(vct));
        if (string.IsNullOrWhiteSpace(proofJwt))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(proofJwt));
#endif

        return new CredentialRequest
        {
            Format = Oid4VciConstants.SdJwtVcFormat,
            Vct = vct,
            Proof = new CredentialProof
            {
                ProofType = Oid4VciConstants.ProofTypes.Jwt,
                Jwt = proofJwt
            }
        };
    }

    /// <summary>
    /// Creates a new credential request using a credential identifier.
    /// </summary>
    /// <param name="credentialIdentifier">The credential identifier</param>
    /// <param name="proofJwt">The proof of possession JWT</param>
    /// <returns>A new CredentialRequest instance</returns>
    public static CredentialRequest CreateByIdentifier(string credentialIdentifier, string proofJwt)
    {
#if NET6_0_OR_GREATER
        ArgumentException.ThrowIfNullOrWhiteSpace(credentialIdentifier);
        ArgumentException.ThrowIfNullOrWhiteSpace(proofJwt);
#else
        if (string.IsNullOrWhiteSpace(credentialIdentifier))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(credentialIdentifier));
        if (string.IsNullOrWhiteSpace(proofJwt))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(proofJwt));
#endif

        return new CredentialRequest
        {
            Format = Oid4VciConstants.SdJwtVcFormat,
            CredentialIdentifier = credentialIdentifier,
            Proof = new CredentialProof
            {
                ProofType = Oid4VciConstants.ProofTypes.Jwt,
                Jwt = proofJwt
            }
        };
    }
}