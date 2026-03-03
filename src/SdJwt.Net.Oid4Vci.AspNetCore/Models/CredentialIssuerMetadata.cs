using System.Text.Json;
using System.Text.Json.Serialization;

namespace SdJwt.Net.Oid4Vci.AspNetCore.Models;

/// <summary>
/// Represents the OpenID Credential Issuer Metadata document
/// served at <c>/.well-known/openid-credential-issuer</c>.
/// Conforms to OID4VCI 1.0 Section 11.2.
/// </summary>
public sealed class CredentialIssuerMetadata
{
    /// <summary>
    /// Gets or sets the credential issuer URL.
    /// REQUIRED. The URL of the Credential Issuer.
    /// </summary>
    [JsonPropertyName("credential_issuer")]
    public string CredentialIssuer { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the list of authorization server URLs.
    /// OPTIONAL. Identifies the authorization server(s) that the issuer relies on.
    /// </summary>
    [JsonPropertyName("authorization_servers")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string[]? AuthorizationServers { get; set; }

    /// <summary>
    /// Gets or sets the credential endpoint URL.
    /// REQUIRED. URL of the Credential Endpoint.
    /// </summary>
    [JsonPropertyName("credential_endpoint")]
    public string CredentialEndpoint { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the deferred credential endpoint URL.
    /// OPTIONAL. URL of the Deferred Credential Endpoint.
    /// </summary>
    [JsonPropertyName("deferred_credential_endpoint")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? DeferredCredentialEndpoint { get; set; }

    /// <summary>
    /// Gets or sets the notification endpoint URL.
    /// OPTIONAL. URL of the Notification Endpoint.
    /// </summary>
    [JsonPropertyName("notification_endpoint")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? NotificationEndpoint { get; set; }

    /// <summary>
    /// Gets or sets the supported credential configurations keyed by configuration identifier.
    /// REQUIRED. An object containing a list of the Credential Configurations supported.
    /// </summary>
    [JsonPropertyName("credential_configurations_supported")]
    public Dictionary<string, JsonElement> CredentialConfigurationsSupported { get; set; } = new();

    /// <summary>
    /// Gets or sets the display information for the issuer.
    /// OPTIONAL. An array of objects containing display properties of the Issuer.
    /// </summary>
    [JsonPropertyName("display")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object[]? Display { get; set; }
}
