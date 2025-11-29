using System.Text.Json.Serialization;

namespace SdJwt.Net.OidFederation.Models;

/// <summary>
/// Represents entity metadata containing protocol-specific configuration.
/// Used within Entity Configuration and Entity Statement JWTs.
/// </summary>
public class EntityMetadata
{
    /// <summary>
    /// Gets or sets the federation entity metadata.
    /// Optional. Contains federation-specific configuration.
    /// </summary>
    [JsonPropertyName("federation_entity")]
    public FederationEntityMetadata? FederationEntity { get; set; }

    /// <summary>
    /// Gets or sets the OpenID Connect Relying Party metadata.
    /// Optional. Present when the entity acts as an OIDC RP.
    /// </summary>
    [JsonPropertyName("openid_relying_party")]
    public object? OpenIdRelyingParty { get; set; }

    /// <summary>
    /// Gets or sets the OpenID Connect Provider metadata.
    /// Optional. Present when the entity acts as an OIDC OP.
    /// </summary>
    [JsonPropertyName("openid_provider")]
    public object? OpenIdProvider { get; set; }

    /// <summary>
    /// Gets or sets the OAuth Authorization Server metadata.
    /// Optional. Present when the entity acts as an OAuth AS.
    /// </summary>
    [JsonPropertyName("oauth_authorization_server")]
    public object? OAuthAuthorizationServer { get; set; }

    /// <summary>
    /// Gets or sets the OAuth Resource Server metadata.
    /// Optional. Present when the entity acts as an OAuth RS.
    /// </summary>
    [JsonPropertyName("oauth_resource")]
    public object? OAuthResource { get; set; }

    /// <summary>
    /// Gets or sets the OID4VCI Credential Issuer metadata.
    /// Optional. Present when the entity issues verifiable credentials.
    /// </summary>
    [JsonPropertyName("openid_credential_issuer")]
    public object? OpenIdCredentialIssuer { get; set; }

    /// <summary>
    /// Gets or sets the OID4VP Verifier metadata.
    /// Optional. Present when the entity verifies presentations.
    /// </summary>
    [JsonPropertyName("openid_relying_party_verifier")]
    public object? OpenIdRelyingPartyVerifier { get; set; }

    /// <summary>
    /// Gets or sets additional metadata for other protocols.
    /// Allows extension for future or custom protocols.
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, object>? AdditionalMetadata { get; set; }

    /// <summary>
    /// Validates the entity metadata.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the metadata is invalid</exception>
    public void Validate()
    {
        FederationEntity?.Validate();
        
        // Additional protocol-specific validation can be added here
        // For now, we accept any valid JSON objects for the protocol metadata
    }

    /// <summary>
    /// Checks if the entity supports a specific protocol.
    /// </summary>
    /// <param name="protocol">The protocol identifier</param>
    /// <returns>True if the protocol is supported</returns>
    public bool SupportsProtocol(string protocol) => protocol switch
    {
        "federation_entity" => FederationEntity != null,
        "openid_relying_party" => OpenIdRelyingParty != null,
        "openid_provider" => OpenIdProvider != null,
        "oauth_authorization_server" => OAuthAuthorizationServer != null,
        "oauth_resource" => OAuthResource != null,
        "openid_credential_issuer" => OpenIdCredentialIssuer != null,
        "openid_relying_party_verifier" => OpenIdRelyingPartyVerifier != null,
        _ => AdditionalMetadata?.ContainsKey(protocol) == true
    };

    /// <summary>
    /// Gets the metadata for a specific protocol.
    /// </summary>
    /// <param name="protocol">The protocol identifier</param>
    /// <returns>The protocol metadata or null if not found</returns>
    public object? GetProtocolMetadata(string protocol) => protocol switch
    {
        "federation_entity" => FederationEntity,
        "openid_relying_party" => OpenIdRelyingParty,
        "openid_provider" => OpenIdProvider,
        "oauth_authorization_server" => OAuthAuthorizationServer,
        "oauth_resource" => OAuthResource,
        "openid_credential_issuer" => OpenIdCredentialIssuer,
        "openid_relying_party_verifier" => OpenIdRelyingPartyVerifier,
        _ => AdditionalMetadata?.GetValueOrDefault(protocol)
    };
}

/// <summary>
/// Represents federation entity specific metadata.
/// </summary>
public class FederationEntityMetadata
{
    /// <summary>
    /// Gets or sets the federation fetch endpoint.
    /// Optional. Endpoint for fetching entity statements about subordinates.
    /// </summary>
    [JsonPropertyName("federation_fetch_endpoint")]
    public string? FederationFetchEndpoint { get; set; }

    /// <summary>
    /// Gets or sets the federation list endpoint.
    /// Optional. Endpoint for listing subordinate entities.
    /// </summary>
    [JsonPropertyName("federation_list_endpoint")]
    public string? FederationListEndpoint { get; set; }

    /// <summary>
    /// Gets or sets the federation resolve endpoint.
    /// Optional. Endpoint for resolving entity metadata through the federation.
    /// </summary>
    [JsonPropertyName("federation_resolve_endpoint")]
    public string? FederationResolveEndpoint { get; set; }

    /// <summary>
    /// Gets or sets the federation trust mark status endpoint.
    /// Optional. Endpoint for checking trust mark status.
    /// </summary>
    [JsonPropertyName("federation_trust_mark_status_endpoint")]
    public string? FederationTrustMarkStatusEndpoint { get; set; }

    /// <summary>
    /// Gets or sets the name of the federation entity.
    /// Optional. Human-readable name for the entity.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets additional contacts for the federation entity.
    /// Optional. Contact information for the entity operators.
    /// </summary>
    [JsonPropertyName("contacts")]
    public string[]? Contacts { get; set; }

    /// <summary>
    /// Validates the federation entity metadata.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the metadata is invalid</exception>
    public void Validate()
    {
        ValidateEndpoint(FederationFetchEndpoint, nameof(FederationFetchEndpoint));
        ValidateEndpoint(FederationListEndpoint, nameof(FederationListEndpoint));
        ValidateEndpoint(FederationResolveEndpoint, nameof(FederationResolveEndpoint));
        ValidateEndpoint(FederationTrustMarkStatusEndpoint, nameof(FederationTrustMarkStatusEndpoint));

        if (Contacts != null)
        {
            foreach (var contact in Contacts)
            {
                if (string.IsNullOrWhiteSpace(contact))
                    throw new InvalidOperationException("Contact cannot be null or empty");
            }
        }
    }

    private static void ValidateEndpoint(string? endpoint, string fieldName)
    {
        if (!string.IsNullOrWhiteSpace(endpoint))
        {
            if (!Uri.TryCreate(endpoint, UriKind.Absolute, out var uri) || uri.Scheme != "https")
                throw new InvalidOperationException($"{fieldName} must be a valid HTTPS URL");
        }
    }
}