using System.Text.Json.Serialization;

namespace SdJwt.Net.Oid4Vci.Models;

/// <summary>
/// Represents a Credential Request according to OID4VCI 1.0 Section 8.2.
/// The wallet sends this to the Issuer's credential endpoint to request credential issuance.
/// Either <see cref="CredentialConfigurationId"/> or <see cref="CredentialIdentifier"/> MUST be present,
/// but not both.
/// </summary>
public class CredentialRequest
{
    /// <summary>
    /// Gets or sets the credential configuration identifier.
    /// CONDITIONAL. String that identifies a supported credential configuration in the issuer's metadata.
    /// Required when the wallet is NOT using credential identifiers from an authorization server
    /// (i.e., when <see cref="CredentialIdentifier"/> is absent).
    /// </summary>
    [JsonPropertyName("credential_configuration_id")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public string? CredentialConfigurationId
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the credential identifier.
    /// CONDITIONAL. String that identifies a specific credential authorized via the token endpoint
    /// <c>authorization_details</c>. Required when the wallet received credential identifiers
    /// from the authorization server (i.e., when <see cref="CredentialConfigurationId"/> is absent).
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
    /// Gets or sets the credential definition.
    /// OPTIONAL. Contains format-specific credential details such as claim subset selection.
    /// Used by some formats (e.g., <c>jwt_vc_json</c>) to narrow the requested claims.
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
    /// Gets or sets multiple proofs of possession keyed by proof type.
    /// CONDITIONAL. Object containing one or more proof type arrays (e.g., <c>jwt</c>, <c>di_vp</c>,
    /// <c>attestation</c>). Required unless the issuer allows credential delivery without
    /// cryptographic binding.
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
    /// OPTIONAL. When present, instructs the issuer to return the response as a JWE.
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
    /// Validates the credential request according to OID4VCI 1.0 Section 8.2.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the request is invalid.</exception>
    public void Validate()
    {
        var hasConfigId = !string.IsNullOrWhiteSpace(CredentialConfigurationId);
        var hasIdentifier = !string.IsNullOrWhiteSpace(CredentialIdentifier);

        if (!hasConfigId && !hasIdentifier)
            throw new InvalidOperationException(
                "Either credential_configuration_id or credential_identifier must be present.");

        if (hasConfigId && hasIdentifier)
            throw new InvalidOperationException(
                "Cannot specify both credential_configuration_id and credential_identifier.");

        Proofs?.Validate();
    }

    /// <summary>
    /// Creates a credential request using a credential configuration ID.
    /// Convenience alias for <see cref="CreateByConfigurationId"/>.
    /// </summary>
    /// <param name="credentialConfigurationId">The credential configuration identifier from issuer metadata.</param>
    /// <param name="proofJwt">The proof of possession JWT string.</param>
    /// <returns>A new <see cref="CredentialRequest"/> instance.</returns>
    public static CredentialRequest Create(string credentialConfigurationId, string proofJwt)
        => CreateByConfigurationId(credentialConfigurationId, proofJwt);

    /// <summary>
    /// Creates a credential request using a credential configuration ID.
    /// </summary>
    /// <param name="credentialConfigurationId">The credential configuration identifier from issuer metadata.</param>
    /// <param name="proofJwt">The proof of possession JWT string.</param>
    /// <returns>A new <see cref="CredentialRequest"/> instance.</returns>
    public static CredentialRequest CreateByConfigurationId(string credentialConfigurationId, string proofJwt)
    {
#if NET6_0_OR_GREATER
        ArgumentException.ThrowIfNullOrWhiteSpace(credentialConfigurationId);
        ArgumentException.ThrowIfNullOrWhiteSpace(proofJwt);
#else
        if (string.IsNullOrWhiteSpace(credentialConfigurationId))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(credentialConfigurationId));
        if (string.IsNullOrWhiteSpace(proofJwt))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(proofJwt));
#endif

        return new CredentialRequest
        {
            CredentialConfigurationId = credentialConfigurationId,
            Proofs = new CredentialProofs { Jwt = new[] { proofJwt } }
        };
    }

    /// <summary>
    /// Creates a credential request using a credential identifier from the token response.
    /// </summary>
    /// <param name="credentialIdentifier">The credential identifier from <c>authorization_details</c>.</param>
    /// <param name="proofJwt">The proof of possession JWT string.</param>
    /// <returns>A new <see cref="CredentialRequest"/> instance.</returns>
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
            CredentialIdentifier = credentialIdentifier,
            Proofs = new CredentialProofs { Jwt = new[] { proofJwt } }
        };
    }

    /// <summary>
    /// Creates a batch credential request using a credential configuration ID with multiple JWT proofs.
    /// Per OID4VCI 1.0 Section 3.3, each JWT proof produces one credential bound to a different key.
    /// </summary>
    /// <param name="credentialConfigurationId">The credential configuration identifier from issuer metadata.</param>
    /// <param name="proofJwts">Array of proof JWT strings, one per requested credential.</param>
    /// <returns>A new <see cref="CredentialRequest"/> instance.</returns>
    public static CredentialRequest CreateBatch(string credentialConfigurationId, string[] proofJwts)
    {
#if NET6_0_OR_GREATER
        ArgumentException.ThrowIfNullOrWhiteSpace(credentialConfigurationId);
        ArgumentNullException.ThrowIfNull(proofJwts);
#else
        if (string.IsNullOrWhiteSpace(credentialConfigurationId))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(credentialConfigurationId));
        if (proofJwts == null)
            throw new ArgumentNullException(nameof(proofJwts));
#endif

        if (proofJwts.Length == 0)
            throw new ArgumentException("At least one proof JWT is required.", nameof(proofJwts));

        return new CredentialRequest
        {
            CredentialConfigurationId = credentialConfigurationId,
            Proofs = new CredentialProofs { Jwt = proofJwts }
        };
    }
}
