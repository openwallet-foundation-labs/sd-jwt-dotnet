using SdJwt.Net.Oid4Vp.Models.Dcql;
using System.Text.Json.Serialization;

namespace SdJwt.Net.Oid4Vp.Models;

/// <summary>
/// Represents an Authorization Request according to OID4VP 1.0.
/// This is the QR code payload that initiates the presentation flow.
/// </summary>
public class AuthorizationRequest {
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
        public string? ClientIdScheme { get; set; }

        /// <summary>
        /// Gets or sets the response type.
        /// REQUIRED. Must be "vp_token".
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
        public string? ResponseMode { get; set; }

        /// <summary>
        /// Gets or sets the response URI.
        /// CONDITIONAL. The URI to which the wallet should POST the authorization response.
        /// Required when response_mode is "direct_post" or "direct_post.jwt".
        /// </summary>
        [JsonPropertyName("response_uri")]
#if NET6_0_OR_GREATER
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
        public string? ResponseUri { get; set; }

        /// <summary>
        /// Gets or sets the redirect URI.
        /// OPTIONAL. The URI to which the user agent is redirected after authorization.
        /// </summary>
        [JsonPropertyName("redirect_uri")]
#if NET6_0_OR_GREATER
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
        public string? RedirectUri { get; set; }

        /// <summary>
        /// Gets or sets the scope.
        /// OPTIONAL. Scope values that the authorization server is requesting.
        /// </summary>
        [JsonPropertyName("scope")]
#if NET6_0_OR_GREATER
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
        public string? Scope { get; set; }

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
        public string? State { get; set; }

        /// <summary>
        /// Gets or sets the presentation definition.
        /// CONDITIONAL. The presentation definition object containing requirements.
        /// Must be present if presentation_definition_uri is not provided.
        /// </summary>
        [JsonPropertyName("presentation_definition")]
#if NET6_0_OR_GREATER
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
        public PresentationDefinition? PresentationDefinition { get; set; }

        /// <summary>
        /// Gets or sets the presentation definition URI.
        /// CONDITIONAL. URI where the presentation definition can be retrieved.
        /// Must be present if presentation_definition is not provided.
        /// </summary>
        [JsonPropertyName("presentation_definition_uri")]
#if NET6_0_OR_GREATER
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
        public string? PresentationDefinitionUri { get; set; }

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
        public DcqlQuery? DcqlQuery { get; set; }

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
        public string? RequestUriMethod { get; set; }

        /// <summary>
        /// Gets or sets the transaction data.
        /// OPTIONAL. Per OID4VP 1.0 Section 8.4, base64url-encoded transaction data
        /// that should be bound to the presented credentials.
        /// </summary>
        [JsonPropertyName("transaction_data")]
#if NET6_0_OR_GREATER
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
        public string? TransactionData { get; set; }

        /// <summary>
        /// Gets or sets the wallet nonce.
        /// OPTIONAL. Per OID4VP 1.0 Section 5, a nonce value provided by the wallet
        /// to the verifier in a prior interaction, used to prevent replay attacks.
        /// </summary>
        [JsonPropertyName("wallet_nonce")]
#if NET6_0_OR_GREATER
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
        public string? WalletNonce { get; set; }

        /// <summary>
        /// Gets or sets the verifier information.
        /// OPTIONAL. Per OID4VP 1.0 Section 5.11, a JWT or string conveying the verifier's
        /// identity and proof of possession, enabling trust establishment with the wallet.
        /// </summary>
        [JsonPropertyName("verifier_info")]
#if NET6_0_OR_GREATER
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
        public string? VerifierInfo { get; set; }

        /// <summary>
        /// Gets or sets the client metadata.
        /// OPTIONAL. The Client Metadata as defined in RFC 7591.
        /// </summary>
        [JsonPropertyName("client_metadata")]
#if NET6_0_OR_GREATER
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
        public Dictionary<string, object>? ClientMetadata { get; set; }

        /// <summary>
        /// Gets or sets the client metadata URI.
        /// OPTIONAL. URI where the client metadata can be retrieved.
        /// </summary>
        [JsonPropertyName("client_metadata_uri")]
#if NET6_0_OR_GREATER
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
        public string? ClientMetadataUri { get; set; }

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
            string? state = null) {
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

                return new AuthorizationRequest {
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
            string? state = null) {
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

                return new AuthorizationRequest {
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
            string? state = null) {
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

                return new AuthorizationRequest {
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
        public void Validate() {
                if (string.IsNullOrWhiteSpace(ClientId)) {
                        throw new InvalidOperationException("client_id is required");
                }

                if (string.IsNullOrWhiteSpace(ResponseType)) {
                        throw new InvalidOperationException("response_type is required");
                }

                if (ResponseType != Oid4VpConstants.ResponseTypes.VpToken) {
                        throw new InvalidOperationException($"response_type must be '{Oid4VpConstants.ResponseTypes.VpToken}'");
                }

                if (string.IsNullOrWhiteSpace(Nonce)) {
                        throw new InvalidOperationException("nonce is required");
                }

                // Validate response mode requirements
                if (ResponseMode is Oid4VpConstants.ResponseModes.DirectPost or Oid4VpConstants.ResponseModes.DirectPostJwt) {
                        if (string.IsNullOrWhiteSpace(ResponseUri)) {
                                throw new InvalidOperationException($"response_uri is required when response_mode is '{ResponseMode}'");
                        }
                }

                // Validate response mode requirements
                // Exactly one credential query mechanism must be supplied:
                // either dcql_query OR (presentation_definition / presentation_definition_uri) — per OID4VP 1.0 Section 5.1.
                var hasDcql = DcqlQuery != null;
                var hasPresentationDef = PresentationDefinition != null || !string.IsNullOrWhiteSpace(PresentationDefinitionUri);

                if (!hasDcql && !hasPresentationDef) {
                        throw new InvalidOperationException(
                                "Either 'dcql_query' or 'presentation_definition'/'presentation_definition_uri' must be provided.");
                }

                if (hasDcql && hasPresentationDef) {
                        throw new InvalidOperationException(
                                "'dcql_query' and 'presentation_definition'/'presentation_definition_uri' cannot both be provided.");
                }

                if (PresentationDefinition != null && !string.IsNullOrWhiteSpace(PresentationDefinitionUri)) {
                        throw new InvalidOperationException(
                                "'presentation_definition' and 'presentation_definition_uri' cannot both be provided.");
                }

                // Validate nested objects
                DcqlQuery?.Validate();
                PresentationDefinition?.Validate();
        }
}