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
    /// This implementation supports both <c>dc+sd-jwt</c> and legacy <c>vc+sd-jwt</c> for SD-JWT credentials.
    /// </summary>
    [JsonPropertyName("format")]
    public string? Format
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the verifiable credential type.
    /// CONDITIONAL. Required when format is <c>dc+sd-jwt</c> or legacy <c>vc+sd-jwt</c>.
    /// String designating the type of the Credential.
    /// </summary>
    [JsonPropertyName("vct")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public string? Vct
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the credential definition.
    /// CONDITIONAL. Contains the detailed description of the credential type.
    /// Used when the Credential Issuer supports dynamic credential types.
    /// </summary>
    [JsonPropertyName("credential_definition")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public object? CredentialDefinition
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the credential identifier.
    /// CONDITIONAL. String identifying a Credential Configuration supported by the Credential Issuer.
    /// Must not be present if credential_definition is present.
    /// </summary>
    [JsonPropertyName("credential_identifier")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public string? CredentialIdentifier
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the document type for mDL credentials.
    /// CONDITIONAL. Required when format is "mso_mdoc".
    /// </summary>
    [JsonPropertyName("doctype")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public string? DocType
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the claims for mDL credentials.
    /// CONDITIONAL. Claims to include in mDL credentials.
    /// </summary>
    [JsonPropertyName("claims")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public Dictionary<string, object>? Claims
    {
        get; set;
    }

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
    public CredentialProof? Proof
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets multiple proofs of possession keyed by proof type.
    /// CONDITIONAL. Alternative to <see cref="Proof"/> for sending multiple proofs.
    /// </summary>
    [JsonPropertyName("proofs")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public CredentialProofs? Proofs
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the credential response encryption parameters.
    /// OPTIONAL. Contains information for encrypting the Credential Response.
    /// </summary>
    [JsonPropertyName("credential_response_encryption")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public object? CredentialResponseEncryption
    {
        get; set;
    }

    /// <summary>
    /// Validates the credential request according to OID4VCI 1.0 requirements.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the request is invalid</exception>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Format))
            throw new InvalidOperationException("Format is required");

        if (CredentialDefinition != null && !string.IsNullOrWhiteSpace(CredentialIdentifier))
            throw new InvalidOperationException("Cannot specify both credential_definition and credential_identifier");

        if (IsSdJwtVcFormat(Format))
        {
            if (string.IsNullOrWhiteSpace(Vct) && CredentialDefinition == null && string.IsNullOrWhiteSpace(CredentialIdentifier))
                throw new InvalidOperationException("VCT, credential_definition, or credential_identifier is required for dc+sd-jwt (or legacy vc+sd-jwt) format");
        }

        if (Proof != null && Proofs != null)
        {
            throw new InvalidOperationException("Cannot specify both proof and proofs");
        }

        Proof?.Validate();
        Proofs?.Validate();
    }

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

    private static bool IsSdJwtVcFormat(string? format)
    {
        return string.Equals(format, Oid4VciConstants.SdJwtVcFormat, StringComparison.Ordinal) ||
               string.Equals(format, Oid4VciConstants.SdJwtVcLegacyFormat, StringComparison.Ordinal);
    }
}
