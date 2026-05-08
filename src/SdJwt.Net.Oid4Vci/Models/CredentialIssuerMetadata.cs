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
    public int BatchSize { get; set; }
}
