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

                // Validate presentation definition requirements
                if (PresentationDefinition == null && string.IsNullOrWhiteSpace(PresentationDefinitionUri)) {
                        throw new InvalidOperationException("Either presentation_definition or presentation_definition_uri must be provided");
                }

                if (PresentationDefinition != null && !string.IsNullOrWhiteSpace(PresentationDefinitionUri)) {
                        throw new InvalidOperationException("presentation_definition and presentation_definition_uri cannot both be provided");
                }

                // Validate presentation definition if provided
                PresentationDefinition?.Validate();
        }
}