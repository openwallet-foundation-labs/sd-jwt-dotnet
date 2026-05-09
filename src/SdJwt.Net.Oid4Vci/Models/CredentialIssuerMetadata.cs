using System.Text.Json.Serialization;

namespace SdJwt.Net.Oid4Vci.Models;

/// <summary>
/// Represents the Credential Issuer Metadata document served at
/// <c>/.well-known/openid-credential-issuer</c> per OID4VCI 1.0 Section 11.2.
/// </summary>
public class CredentialIssuerMetadata
{
    /// <summary>
    /// Gets or sets the credential issuer identifier URL.
    /// REQUIRED. Must match the URL used to fetch this metadata.
    /// </summary>
    [JsonPropertyName("credential_issuer")]
    public string CredentialIssuer { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the authorization server URLs.
    /// OPTIONAL. Defaults to the credential issuer URL when absent.
    /// </summary>
    [JsonPropertyName("authorization_servers")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public string[]? AuthorizationServers
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the credential endpoint URL.
    /// REQUIRED. URL of the Credential Endpoint where wallets request credentials.
    /// </summary>
    [JsonPropertyName("credential_endpoint")]
    public string CredentialEndpoint { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the nonce endpoint URL.
    /// OPTIONAL. URL of the Nonce Endpoint for obtaining fresh <c>c_nonce</c> values.
    /// When present, proofs MUST include a <c>nonce</c> claim.
    /// </summary>
    [JsonPropertyName("nonce_endpoint")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public string? NonceEndpoint
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the deferred credential endpoint URL.
    /// OPTIONAL. URL of the Deferred Credential Endpoint for retrieving deferred credentials.
    /// </summary>
    [JsonPropertyName("deferred_credential_endpoint")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public string? DeferredCredentialEndpoint
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the notification endpoint URL.
    /// OPTIONAL. URL of the Notification Endpoint where wallets send credential lifecycle events.
    /// </summary>
    [JsonPropertyName("notification_endpoint")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public string? NotificationEndpoint
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the batch credential issuance configuration.
    /// OPTIONAL. When present, indicates that the issuer supports batch issuance.
    /// </summary>
    [JsonPropertyName("batch_credential_issuance")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public BatchCredentialIssuance? BatchCredentialIssuance
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the supported Credential Request encryption configuration.
    /// OPTIONAL. When present, describes application-layer JWE encryption accepted by the issuer.
    /// </summary>
    [JsonPropertyName("credential_request_encryption")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public CredentialRequestEncryption? CredentialRequestEncryption
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the supported Credential Response encryption configuration.
    /// OPTIONAL. When present, describes application-layer JWE encryption supported by the issuer.
    /// </summary>
    [JsonPropertyName("credential_response_encryption")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public CredentialResponseEncryption? CredentialResponseEncryption
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the supported credential configurations keyed by configuration ID.
    /// REQUIRED. Object where each key is a <c>credential_configuration_id</c> string
    /// and each value is a <see cref="CredentialConfiguration"/> object.
    /// </summary>
    [JsonPropertyName("credential_configurations_supported")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public Dictionary<string, CredentialConfiguration>? CredentialConfigurationsSupported
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the issuer display properties.
    /// OPTIONAL. Array of localized display information about the issuer.
    /// </summary>
    [JsonPropertyName("display")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public CredentialDisplayProperties[]? Display
    {
        get; set;
    }
}

/// <summary>
/// Configuration for batch credential issuance per OID4VCI 1.0 Section 3.3.
/// </summary>
public class BatchCredentialIssuance
{
    /// <summary>
    /// Gets or sets the maximum number of proofs the issuer accepts in a single request.
    /// REQUIRED. Must be &gt;= 2.
    /// </summary>
    [JsonPropertyName("batch_size")]
    public int BatchSize
    {
        get; set;
    }
}

/// <summary>
/// Describes Credential Request encryption support in Credential Issuer Metadata.
/// </summary>
public class CredentialRequestEncryption
{
    /// <summary>
    /// Gets or sets the issuer encryption public keys as a JWK Set object.
    /// REQUIRED when request encryption is advertised.
    /// </summary>
    [JsonPropertyName("jwks")]
    public object? Jwks
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets supported JWE content encryption algorithms.
    /// REQUIRED when request encryption is advertised.
    /// </summary>
    [JsonPropertyName("enc_values_supported")]
    public string[] EncValuesSupported { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets supported JWE compression algorithms.
    /// OPTIONAL.
    /// </summary>
    [JsonPropertyName("zip_values_supported")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public string[]? ZipValuesSupported
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets whether request encryption is required.
    /// REQUIRED when request encryption is advertised.
    /// </summary>
    [JsonPropertyName("encryption_required")]
    public bool EncryptionRequired
    {
        get; set;
    }
}

/// <summary>
/// Describes Credential Response encryption support in Credential Issuer Metadata.
/// </summary>
public class CredentialResponseEncryption
{
    /// <summary>
    /// Gets or sets supported JWE key management algorithms.
    /// REQUIRED when response encryption is advertised.
    /// </summary>
    [JsonPropertyName("alg_values_supported")]
    public string[] AlgValuesSupported { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets supported JWE content encryption algorithms.
    /// REQUIRED when response encryption is advertised.
    /// </summary>
    [JsonPropertyName("enc_values_supported")]
    public string[] EncValuesSupported { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets supported JWE compression algorithms.
    /// OPTIONAL.
    /// </summary>
    [JsonPropertyName("zip_values_supported")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public string[]? ZipValuesSupported
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets whether response encryption is required.
    /// REQUIRED when response encryption is advertised.
    /// </summary>
    [JsonPropertyName("encryption_required")]
    public bool EncryptionRequired
    {
        get; set;
    }
}
