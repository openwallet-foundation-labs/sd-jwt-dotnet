using SdJwt.Net.Oid4Vp.Models.Dcql;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json.Serialization;

namespace SdJwt.Net.Oid4Vp.Models;

/// <summary>
/// Represents an Authorization Request according to OID4VP 1.0.
/// This is the QR code payload that initiates the presentation flow.
/// </summary>
public class AuthorizationRequest
{
    /// <summary>
    /// Gets or sets the client identifier.
    /// REQUIRED. Identifier of the Verifier. Depending on the Client Identifier Scheme, 
    /// this can be a URL, DID, or other identifier.
    /// </summary>
    [JsonPropertyName("client_id")]
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the client identifier scheme.
    /// OPTIONAL. Indicates the scheme of the client_id parameter value.
    /// </summary>
    [JsonPropertyName("client_id_scheme")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public string? ClientIdScheme
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the response type.
    /// REQUIRED. Must be "vp_token" or "vp_token id_token".
    /// </summary>
    [JsonPropertyName("response_type")]
    public string ResponseType { get; set; } = Oid4VpConstants.ResponseTypes.VpToken;

    /// <summary>
    /// Gets or sets the response mode.
    /// OPTIONAL. Indicates how the authorization response is to be returned.
    /// For cross-device flow, this should be "direct_post".
    /// </summary>
    [JsonPropertyName("response_mode")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public string? ResponseMode
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the response URI.
    /// CONDITIONAL. The URI to which the wallet should POST the authorization response.
    /// Required when response_mode is "direct_post" or "direct_post.jwt".
    /// </summary>
    [JsonPropertyName("response_uri")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public string? ResponseUri
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the redirect URI.
    /// OPTIONAL. The URI to which the user agent is redirected after authorization.
    /// </summary>
    [JsonPropertyName("redirect_uri")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public string? RedirectUri
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the scope.
    /// OPTIONAL. Scope values that the authorization server is requesting.
    /// </summary>
    [JsonPropertyName("scope")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public string? Scope
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the requested ID Token type for combined OID4VP and SIOPv2 responses.
    /// OPTIONAL. When present, this library supports <c>subject_signed_id_token</c>.
    /// </summary>
    [JsonPropertyName("id_token_type")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public string? IdTokenType
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the nonce.
    /// REQUIRED. String value used to associate a client session with an ID Token.
    /// </summary>
    [JsonPropertyName("nonce")]
    public string Nonce { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the state.
    /// OPTIONAL. Opaque value used to maintain state between the request and callback.
    /// </summary>
    [JsonPropertyName("state")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public string? State
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the presentation definition.
    /// CONDITIONAL. The presentation definition object containing requirements.
    /// Must be present if presentation_definition_uri is not provided.
    /// </summary>
    [JsonPropertyName("presentation_definition")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public PresentationDefinition? PresentationDefinition
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the presentation definition URI.
    /// CONDITIONAL. URI where the presentation definition can be retrieved.
    /// Must be present if presentation_definition is not provided.
    /// </summary>
    [JsonPropertyName("presentation_definition_uri")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public string? PresentationDefinitionUri
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the Digital Credentials Query Language (DCQL) query.
    /// CONDITIONAL. Specifies which credentials the verifier is requesting using DCQL.
    /// Per OID4VP 1.0 Section 5.1, either <see cref="DcqlQuery" /> or
    /// <see cref="PresentationDefinition" />/<see cref="PresentationDefinitionUri" /> MUST be
    /// present, but not both simultaneously.
    /// </summary>
    [JsonPropertyName("dcql_query")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public DcqlQuery? DcqlQuery
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the HTTP method for requesting the Authorization Request Object via request_uri.
    /// OPTIONAL. Per OID4VP 1.0 Section 5.10, the value MUST be "get" or "post".
    /// Defaults to "get" when absent.
    /// See <see cref="Oid4VpConstants.RequestUriMethods" /> for available values.
    /// </summary>
    [JsonPropertyName("request_uri_method")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public string? RequestUriMethod
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the transaction data.
    /// OPTIONAL. Per OID4VP 1.0, each value is base64url-encoded transaction data
    /// that should be bound to the presented credentials.
    /// </summary>
    [JsonPropertyName("transaction_data")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public string[]? TransactionData
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the wallet nonce.
    /// OPTIONAL. Per OID4VP 1.0 Section 5, a nonce value provided by the wallet
    /// to the verifier in a prior interaction, used to prevent replay attacks.
    /// </summary>
    [JsonPropertyName("wallet_nonce")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public string? WalletNonce
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the verifier information entries.
    /// OPTIONAL. Per OID4VP 1.0, each entry conveys verifier attestation information
    /// and may be scoped to specific DCQL credential query identifiers.
    /// </summary>
    [JsonPropertyName("verifier_info")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public VerifierInfo[]? VerifierInfo
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the client metadata.
    /// OPTIONAL. The Client Metadata as defined in RFC 7591.
    /// </summary>
    [JsonPropertyName("client_metadata")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public Dictionary<string, object>? ClientMetadata
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the client metadata URI.
    /// OPTIONAL. URI where the client metadata can be retrieved.
    /// </summary>
    [JsonPropertyName("client_metadata_uri")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public string? ClientMetadataUri
    {
        get; set;
    }

    /// <summary>
    /// Creates an authorization request for cross-device flow with direct post.
    /// </summary>
    /// <param name="clientId">The verifier's identifier</param>
    /// <param name="responseUri">The URI where the wallet should POST the response</param>
    /// <param name="nonce">Security nonce</param>
    /// <param name="presentationDefinition">The presentation definition</param>
    /// <param name="state">Optional state parameter</param>
    /// <returns>A new AuthorizationRequest instance</returns>
    public static AuthorizationRequest CreateCrossDevice(
        string clientId,
        string responseUri,
        string nonce,
        PresentationDefinition presentationDefinition,
        string? state = null)
    {
#if NET6_0_OR_GREATER
        ArgumentException.ThrowIfNullOrWhiteSpace(clientId);
        ArgumentException.ThrowIfNullOrWhiteSpace(responseUri);
        ArgumentException.ThrowIfNullOrWhiteSpace(nonce);
        ArgumentNullException.ThrowIfNull(presentationDefinition);
#else
        if (string.IsNullOrWhiteSpace(clientId))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(clientId));
        if (string.IsNullOrWhiteSpace(responseUri))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(responseUri));
        if (string.IsNullOrWhiteSpace(nonce))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(nonce));
        if (presentationDefinition == null)
            throw new ArgumentNullException(nameof(presentationDefinition));
#endif

        return new AuthorizationRequest
        {
            ClientId = clientId,
            ResponseType = Oid4VpConstants.ResponseTypes.VpToken,
            ResponseMode = Oid4VpConstants.ResponseModes.DirectPost,
            ResponseUri = responseUri,
            Nonce = nonce,
            State = state,
            PresentationDefinition = presentationDefinition
        };
    }

    /// <summary>
    /// Creates an authorization request for cross-device flow using DCQL instead of Presentation Exchange.
    /// Per OID4VP 1.0 Section 6, DCQL is the native credential query mechanism in OID4VP 1.0.
    /// </summary>
    /// <param name="clientId">The verifier's identifier</param>
    /// <param name="responseUri">The URI where the wallet should POST the response</param>
    /// <param name="nonce">Security nonce</param>
    /// <param name="dcqlQuery">The DCQL query specifying requested credentials</param>
    /// <param name="state">Optional state parameter</param>
    /// <returns>A new AuthorizationRequest instance using DCQL</returns>
    public static AuthorizationRequest CreateCrossDeviceWithDcql(
        string clientId,
        string responseUri,
        string nonce,
        DcqlQuery dcqlQuery,
        string? state = null)
    {
#if NET6_0_OR_GREATER
        ArgumentException.ThrowIfNullOrWhiteSpace(clientId);
        ArgumentException.ThrowIfNullOrWhiteSpace(responseUri);
        ArgumentException.ThrowIfNullOrWhiteSpace(nonce);
        ArgumentNullException.ThrowIfNull(dcqlQuery);
#else
        if (string.IsNullOrWhiteSpace(clientId))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(clientId));
        if (string.IsNullOrWhiteSpace(responseUri))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(responseUri));
        if (string.IsNullOrWhiteSpace(nonce))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(nonce));
        if (dcqlQuery == null)
            throw new ArgumentNullException(nameof(dcqlQuery));
#endif

        return new AuthorizationRequest
        {
            ClientId = clientId,
            ResponseType = Oid4VpConstants.ResponseTypes.VpToken,
            ResponseMode = Oid4VpConstants.ResponseModes.DirectPost,
            ResponseUri = responseUri,
            Nonce = nonce,
            State = state,
            DcqlQuery = dcqlQuery
        };
    }

    /// <summary>
    /// Creates an authorization request using a presentation definition URI.
    /// </summary>
    /// <param name="clientId">The verifier's identifier</param>
    /// <param name="responseUri">The URI where the wallet should POST the response</param>
    /// <param name="nonce">Security nonce</param>
    /// <param name="presentationDefinitionUri">URI to the presentation definition</param>
    /// <param name="state">Optional state parameter</param>
    /// <returns>A new AuthorizationRequest instance</returns>
    public static AuthorizationRequest CreateWithDefinitionUri(
        string clientId,
        string responseUri,
        string nonce,
        string presentationDefinitionUri,
        string? state = null)
    {
#if NET6_0_OR_GREATER
        ArgumentException.ThrowIfNullOrWhiteSpace(clientId);
        ArgumentException.ThrowIfNullOrWhiteSpace(responseUri);
        ArgumentException.ThrowIfNullOrWhiteSpace(nonce);
        ArgumentException.ThrowIfNullOrWhiteSpace(presentationDefinitionUri);
#else
        if (string.IsNullOrWhiteSpace(clientId))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(clientId));
        if (string.IsNullOrWhiteSpace(responseUri))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(responseUri));
        if (string.IsNullOrWhiteSpace(nonce))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(nonce));
        if (string.IsNullOrWhiteSpace(presentationDefinitionUri))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(presentationDefinitionUri));
#endif

        return new AuthorizationRequest
        {
            ClientId = clientId,
            ResponseType = Oid4VpConstants.ResponseTypes.VpToken,
            ResponseMode = Oid4VpConstants.ResponseModes.DirectPost,
            ResponseUri = responseUri,
            Nonce = nonce,
            State = state,
            PresentationDefinitionUri = presentationDefinitionUri
        };
    }

    /// <summary>
    /// Validates this authorization request according to OID4VP 1.0 requirements.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the request is invalid</exception>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(ClientId))
        {
            throw new InvalidOperationException("client_id is required");
        }

        if (string.IsNullOrWhiteSpace(ResponseType))
        {
            throw new InvalidOperationException("response_type is required");
        }

        if (ResponseType != Oid4VpConstants.ResponseTypes.VpToken &&
            ResponseType != Oid4VpConstants.ResponseTypes.VpTokenIdToken)
        {
            throw new InvalidOperationException(
                $"response_type must be '{Oid4VpConstants.ResponseTypes.VpToken}' or '{Oid4VpConstants.ResponseTypes.VpTokenIdToken}'");
        }

        if (ResponseType == Oid4VpConstants.ResponseTypes.VpTokenIdToken)
        {
            if (!ContainsScope(Scope, "openid"))
            {
                throw new InvalidOperationException("scope must include 'openid' when response_type includes 'id_token'.");
            }

            if (!string.IsNullOrWhiteSpace(IdTokenType) &&
                IdTokenType != Oid4VpConstants.IdTokenTypes.SubjectSigned)
            {
                throw new InvalidOperationException(
                    $"id_token_type must be '{Oid4VpConstants.IdTokenTypes.SubjectSigned}' when provided.");
            }
        }

        if (string.IsNullOrWhiteSpace(Nonce))
        {
            throw new InvalidOperationException("nonce is required");
        }

        // Validate response mode requirements
        if (ResponseMode is Oid4VpConstants.ResponseModes.DirectPost or Oid4VpConstants.ResponseModes.DirectPostJwt)
        {
            if (string.IsNullOrWhiteSpace(ResponseUri))
            {
                throw new InvalidOperationException($"response_uri is required when response_mode is '{ResponseMode}'");
            }
            if (!Uri.TryCreate(ResponseUri, UriKind.Absolute, out var responseUri) ||
                !string.Equals(responseUri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("response_uri must be an absolute HTTPS URL when using direct_post or direct_post.jwt.");
            }
        }

        // Validate response mode requirements
        // Exactly one credential query mechanism must be supplied:
        // either dcql_query OR (presentation_definition / presentation_definition_uri) — per OID4VP 1.0 Section 5.1.
        var hasDcql = DcqlQuery != null;
        var hasPresentationDef = PresentationDefinition != null || !string.IsNullOrWhiteSpace(PresentationDefinitionUri);

        if (!hasDcql && !hasPresentationDef)
        {
            throw new InvalidOperationException(
                    "Either 'dcql_query' or 'presentation_definition'/'presentation_definition_uri' must be provided.");
        }

        if (hasDcql && hasPresentationDef)
        {
            throw new InvalidOperationException(
                    "'dcql_query' and 'presentation_definition'/'presentation_definition_uri' cannot both be provided.");
        }

        if (PresentationDefinition != null && !string.IsNullOrWhiteSpace(PresentationDefinitionUri))
        {
            throw new InvalidOperationException(
                    "'presentation_definition' and 'presentation_definition_uri' cannot both be provided.");
        }

        // Validate request_uri_method when provided.
        if (!string.IsNullOrWhiteSpace(RequestUriMethod) &&
            RequestUriMethod != Oid4VpConstants.RequestUriMethods.Get &&
            RequestUriMethod != Oid4VpConstants.RequestUriMethods.Post)
        {
            throw new InvalidOperationException("request_uri_method must be 'get' or 'post' when provided.");
        }

        // Validate optional transaction_data entries as base64url.
        if (TransactionData != null)
        {
            if (TransactionData.Length == 0)
            {
                throw new InvalidOperationException("transaction_data must not be empty when provided.");
            }

            foreach (var entry in TransactionData)
            {
                if (string.IsNullOrWhiteSpace(entry))
                {
                    throw new InvalidOperationException("transaction_data entries must not be empty.");
                }

                try
                {
                    _ = Microsoft.IdentityModel.Tokens.Base64UrlEncoder.DecodeBytes(entry);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"transaction_data entries must be valid base64url: {ex.Message}");
                }
            }
        }

        if (VerifierInfo != null)
        {
            if (VerifierInfo.Length == 0)
            {
                throw new InvalidOperationException("verifier_info must not be empty when provided.");
            }

            foreach (var verifierInfo in VerifierInfo)
            {
                verifierInfo.Validate();
            }
        }

        // Validate optional wallet_nonce format.
        if (WalletNonce != null && string.IsNullOrWhiteSpace(WalletNonce))
        {
            throw new InvalidOperationException("wallet_nonce must not be empty when provided.");
        }

        // Validate nested objects
        DcqlQuery?.Validate();
        PresentationDefinition?.Validate();
    }

    private static bool LooksLikeCompactJwt(string value)
    {
        var parts = value.Split('.');
        return parts.Length == 3 && parts.All(p => !string.IsNullOrWhiteSpace(p));
    }

    private static bool ContainsScope(string? scope, string requiredScope)
    {
        if (string.IsNullOrWhiteSpace(scope))
        {
            return false;
        }

        return scope.Split([' '], StringSplitOptions.RemoveEmptyEntries)
            .Any(value => string.Equals(value, requiredScope, StringComparison.Ordinal));
    }
}

/// <summary>
/// Verifier attestation information supplied in an OID4VP Authorization Request.
/// </summary>
public class VerifierInfo
{
    /// <summary>
    /// Gets or sets the verifier information format, for example <c>jwt</c>.
    /// REQUIRED.
    /// </summary>
    [JsonPropertyName("format")]
    public string Format { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the verifier attestation data.
    /// REQUIRED. The structure is format-specific.
    /// </summary>
    [JsonPropertyName("data")]
    public object? Data
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the credential query identifiers this attestation applies to.
    /// OPTIONAL. If omitted, the entry applies to all requested credentials.
    /// </summary>
    [JsonPropertyName("credential_ids")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public string[]? CredentialIds
    {
        get; set;
    }

    /// <summary>
    /// Validates the verifier information entry.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the entry is malformed.</exception>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Format))
        {
            throw new InvalidOperationException("verifier_info.format is required.");
        }

        if (Data == null)
        {
            throw new InvalidOperationException("verifier_info.data is required.");
        }

        if (CredentialIds != null &&
            (CredentialIds.Length == 0 || CredentialIds.Any(string.IsNullOrWhiteSpace)))
        {
            throw new InvalidOperationException("verifier_info.credential_ids must not contain empty entries.");
        }

        if (Data is string data && Format == "jwt" && LooksLikeCompactJwt(data))
        {
            try
            {
                _ = new JwtSecurityTokenHandler().ReadJwtToken(data);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"verifier_info.data is not a valid compact JWT: {ex.Message}");
            }
        }
    }

    private static bool LooksLikeCompactJwt(string value)
    {
        var parts = value.Split('.');
        return parts.Length == 3 && parts.All(p => !string.IsNullOrWhiteSpace(p));
    }
}
