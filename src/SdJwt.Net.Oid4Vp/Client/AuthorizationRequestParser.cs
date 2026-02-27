using System.Text.Json;
using System.Web;
using SdJwt.Net.Oid4Vp.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Text;

namespace SdJwt.Net.Oid4Vp.Client;

/// <summary>
/// Utility for parsing OID4VP authorization request URIs.
/// Handles both direct request objects and request_uri references.
/// </summary>
public static class AuthorizationRequestParser {
        private const int MaxRequestObjectBytes = 256 * 1024;
        /// <summary>
        /// Parses an OID4VP authorization request URI.
        /// </summary>
        /// <param name="uri">The authorization request URI (e.g., from QR code)</param>
        /// <returns>The parsed authorization request</returns>
        /// <exception cref="ArgumentException">Thrown when the URI is invalid</exception>
        /// <exception cref="InvalidOperationException">Thrown when the request cannot be parsed</exception>
        public static AuthorizationRequest Parse(string uri) {
#if NET6_0_OR_GREATER
                ArgumentException.ThrowIfNullOrWhiteSpace(uri);
#else
        if (string.IsNullOrWhiteSpace(uri))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(uri));
#endif

                if (!Uri.TryCreate(uri, UriKind.Absolute, out var parsedUri)) {
                        throw new ArgumentException("Invalid URI format", nameof(uri));
                }

                if (parsedUri.Scheme != Oid4VpConstants.AuthorizationRequestScheme) {
                        throw new ArgumentException($"Expected scheme '{Oid4VpConstants.AuthorizationRequestScheme}', got '{parsedUri.Scheme}'", nameof(uri));
                }

                var query = HttpUtility.ParseQueryString(parsedUri.Query);

                // Check for direct request object
                var requestParam = query["request"];
                if (!string.IsNullOrEmpty(requestParam)) {
                        return ParseRequestObject(requestParam);
                }

                // Check for request_uri parameter
                var requestUriParam = query["request_uri"];
                if (!string.IsNullOrEmpty(requestUriParam)) {
                        throw new InvalidOperationException($"request_uri parameter found. Use ParseFromRequestUriAsync method to fetch from: {requestUriParam}");
                }

                throw new InvalidOperationException("Authorization request URI must contain either 'request' or 'request_uri' parameter");
        }

        /// <summary>
        /// Fetches and parses an authorization request from a request_uri.
        /// </summary>
        /// <param name="uri">The authorization request URI containing request_uri parameter</param>
        /// <param name="httpClient">HTTP client for fetching the request object</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The parsed authorization request</returns>
        public static async Task<AuthorizationRequest> ParseFromRequestUriAsync(
            string uri,
            HttpClient httpClient,
            CancellationToken cancellationToken = default) {
#if NET6_0_OR_GREATER
                ArgumentException.ThrowIfNullOrWhiteSpace(uri);
                ArgumentNullException.ThrowIfNull(httpClient);
#else
        if (string.IsNullOrWhiteSpace(uri))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(uri));
        if (httpClient == null)
            throw new ArgumentNullException(nameof(httpClient));
#endif

                if (!Uri.TryCreate(uri, UriKind.Absolute, out var parsedUri)) {
                        throw new ArgumentException("Invalid URI format", nameof(uri));
                }

                if (parsedUri.Scheme != Oid4VpConstants.AuthorizationRequestScheme) {
                        throw new ArgumentException($"Expected scheme '{Oid4VpConstants.AuthorizationRequestScheme}', got '{parsedUri.Scheme}'", nameof(uri));
                }

                var query = HttpUtility.ParseQueryString(parsedUri.Query);
                var requestUriParam = query["request_uri"];
                var requestUriMethod = query["request_uri_method"];

                if (string.IsNullOrEmpty(requestUriParam)) {
                        throw new InvalidOperationException("request_uri parameter not found. Use Parse method for direct request objects.");
                }

                try {
                        if (!Uri.TryCreate(requestUriParam, UriKind.Absolute, out var requestUri) ||
                            !string.Equals(requestUri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase) ||
                            !string.IsNullOrEmpty(requestUri.Fragment)) {
                                throw new InvalidOperationException("request_uri must be an absolute HTTPS URI without fragment.");
                        }

                        var method = string.IsNullOrWhiteSpace(requestUriMethod)
                            ? Oid4VpConstants.RequestUriMethods.Get
                            : requestUriMethod;
                        if (!string.Equals(method, Oid4VpConstants.RequestUriMethods.Get, StringComparison.OrdinalIgnoreCase) &&
                            !string.Equals(method, Oid4VpConstants.RequestUriMethods.Post, StringComparison.OrdinalIgnoreCase)) {
                                throw new InvalidOperationException("request_uri_method must be either 'get' or 'post'.");
                        }

                        using var request = new HttpRequestMessage(
                            string.Equals(method, Oid4VpConstants.RequestUriMethods.Post, StringComparison.OrdinalIgnoreCase)
                                ? HttpMethod.Post
                                : HttpMethod.Get,
                            requestUri);

                        if (request.Method == HttpMethod.Post) {
                                // Wallet capabilities payload placeholder; callers may send richer data via custom flow.
                                request.Content = new StringContent("{}", Encoding.UTF8, "application/json");
                        }

                        using var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
                        if (response.StatusCode != HttpStatusCode.OK) {
                                throw new InvalidOperationException($"Failed to fetch request object: HTTP {(int)response.StatusCode}.");
                        }

                        var payload = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                        if (payload.Length > MaxRequestObjectBytes) {
                                throw new InvalidOperationException("Fetched request object exceeds size limit.");
                        }

                        var requestObject = Encoding.UTF8.GetString(payload);
                        return ParseRequestObject(requestObject);
                }
                catch (HttpRequestException ex) {
                        throw new InvalidOperationException($"Failed to fetch request object from {requestUriParam}: {ex.Message}", ex);
                }
        }

        /// <summary>
        /// Parses a request object JSON string.
        /// </summary>
        /// <param name="requestObject">The JSON request object (URL decoded)</param>
        /// <returns>The parsed authorization request</returns>
        private static AuthorizationRequest ParseRequestObject(string requestObject) {
                try {
                        if (IsCompactJwt(requestObject)) {
                                return ParseJarRequestObject(requestObject);
                        }

                        var options = new JsonSerializerOptions {
                                PropertyNameCaseInsensitive = true,
                                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                        };

                        var request = JsonSerializer.Deserialize<AuthorizationRequest>(requestObject, options);
                        if (request == null) {
                                throw new InvalidOperationException("Failed to deserialize authorization request");
                        }

                        // Validate the parsed request
                        request.Validate();

                        return request;
                }
                catch (JsonException ex) {
                        throw new InvalidOperationException($"Invalid JSON in request object: {ex.Message}", ex);
                }
        }

        private static AuthorizationRequest ParseJarRequestObject(string compactJwt) {
                var handler = new JwtSecurityTokenHandler();
                JwtSecurityToken token;
                try {
                        token = handler.ReadJwtToken(compactJwt);
                }
                catch (Exception ex) {
                        throw new InvalidOperationException($"Invalid JAR request object: {ex.Message}", ex);
                }

                if (!string.Equals(token.Header.Typ, Oid4VpConstants.AuthorizationRequestJwtType, StringComparison.Ordinal)) {
                        throw new InvalidOperationException($"Invalid JAR typ header. Expected '{Oid4VpConstants.AuthorizationRequestJwtType}'.");
                }

                var payloadJson = token.Payload.SerializeToJson();
                var options = new JsonSerializerOptions {
                        PropertyNameCaseInsensitive = true,
                        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                };

                var request = JsonSerializer.Deserialize<AuthorizationRequest>(payloadJson, options);
                if (request == null) {
                        throw new InvalidOperationException("Failed to deserialize authorization request payload from JAR.");
                }

                request.Validate();
                return request;
        }

        private static bool IsCompactJwt(string input) {
                if (string.IsNullOrWhiteSpace(input)) {
                        return false;
                }

                var parts = input.Split('.');
                return parts.Length == 3 &&
                       parts.All(p => !string.IsNullOrWhiteSpace(p));
        }

        /// <summary>
        /// Creates an authorization request URI from an AuthorizationRequest object.
        /// </summary>
        /// <param name="request">The authorization request</param>
        /// <returns>The OID4VP URI</returns>
        public static string CreateUri(AuthorizationRequest request) {
#if NET6_0_OR_GREATER
                ArgumentNullException.ThrowIfNull(request);
#else
        if (request == null)
            throw new ArgumentNullException(nameof(request));
#endif

                request.Validate();

                var options = new JsonSerializerOptions {
                        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
                        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                };

                var json = JsonSerializer.Serialize(request, options);
                var encodedRequest = Uri.EscapeDataString(json);

                return $"{Oid4VpConstants.AuthorizationRequestScheme}://?request={encodedRequest}";
        }

        /// <summary>
        /// Creates an authorization request URI using request_uri parameter.
        /// </summary>
        /// <param name="requestUri">The URI where the request object can be fetched</param>
        /// <returns>The OID4VP URI</returns>
        public static string CreateUriWithRequestUri(string requestUri) {
#if NET6_0_OR_GREATER
                ArgumentException.ThrowIfNullOrWhiteSpace(requestUri);
#else
        if (string.IsNullOrWhiteSpace(requestUri))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(requestUri));
#endif

                if (!Uri.TryCreate(requestUri, UriKind.Absolute, out _)) {
                        throw new ArgumentException("Invalid request URI format", nameof(requestUri));
                }

                var encodedRequestUri = Uri.EscapeDataString(requestUri);
                return $"{Oid4VpConstants.AuthorizationRequestScheme}://?request_uri={encodedRequestUri}";
        }

        /// <summary>
        /// Extracts the request_uri parameter from an authorization request URI.
        /// </summary>
        /// <param name="uri">The authorization request URI</param>
        /// <returns>The request_uri value, or null if not present</returns>
        public static string? ExtractRequestUri(string uri) {
#if NET6_0_OR_GREATER
                ArgumentException.ThrowIfNullOrWhiteSpace(uri);
#else
        if (string.IsNullOrWhiteSpace(uri))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(uri));
#endif

                if (!Uri.TryCreate(uri, UriKind.Absolute, out var parsedUri)) {
                        return null;
                }

                if (parsedUri.Scheme != Oid4VpConstants.AuthorizationRequestScheme) {
                        return null;
                }

                var query = HttpUtility.ParseQueryString(parsedUri.Query);
                return query["request_uri"];
        }

        /// <summary>
        /// Checks if an authorization request URI contains a direct request object.
        /// </summary>
        /// <param name="uri">The authorization request URI</param>
        /// <returns>True if the URI contains a request parameter</returns>
        public static bool HasDirectRequest(string uri) {
#if NET6_0_OR_GREATER
                ArgumentException.ThrowIfNullOrWhiteSpace(uri);
#else
        if (string.IsNullOrWhiteSpace(uri))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(uri));
#endif

                if (!Uri.TryCreate(uri, UriKind.Absolute, out var parsedUri)) {
                        return false;
                }

                if (parsedUri.Scheme != Oid4VpConstants.AuthorizationRequestScheme) {
                        return false;
                }

                var query = HttpUtility.ParseQueryString(parsedUri.Query);
                return !string.IsNullOrEmpty(query["request"]);
        }

        /// <summary>
        /// Checks if an authorization request URI uses request_uri parameter.
        /// </summary>
        /// <param name="uri">The authorization request URI</param>
        /// <returns>True if the URI contains a request_uri parameter</returns>
        public static bool HasRequestUri(string uri) {
#if NET6_0_OR_GREATER
                ArgumentException.ThrowIfNullOrWhiteSpace(uri);
#else
        if (string.IsNullOrWhiteSpace(uri))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(uri));
#endif

                if (!Uri.TryCreate(uri, UriKind.Absolute, out var parsedUri)) {
                        return false;
                }

                if (parsedUri.Scheme != Oid4VpConstants.AuthorizationRequestScheme) {
                        return false;
                }

                var query = HttpUtility.ParseQueryString(parsedUri.Query);
                return !string.IsNullOrEmpty(query["request_uri"]);
        }

        /// <summary>
        /// Validates an OID4VP authorization request URI format.
        /// </summary>
        /// <param name="uri">The URI to validate</param>
        /// <returns>Validation result with details</returns>
        public static AuthorizationUriValidationResult ValidateUri(string uri) {
                if (string.IsNullOrWhiteSpace(uri)) {
                        return AuthorizationUriValidationResult.Failed("URI cannot be null or empty");
                }

                if (!Uri.TryCreate(uri, UriKind.Absolute, out var parsedUri)) {
                        return AuthorizationUriValidationResult.Failed("Invalid URI format");
                }

                if (parsedUri.Scheme != Oid4VpConstants.AuthorizationRequestScheme) {
                        return AuthorizationUriValidationResult.Failed($"Invalid scheme. Expected '{Oid4VpConstants.AuthorizationRequestScheme}', got '{parsedUri.Scheme}'");
                }

                var query = HttpUtility.ParseQueryString(parsedUri.Query);
                var hasRequest = !string.IsNullOrEmpty(query["request"]);
                var hasRequestUri = !string.IsNullOrEmpty(query["request_uri"]);

                if (!hasRequest && !hasRequestUri) {
                        return AuthorizationUriValidationResult.Failed("URI must contain either 'request' or 'request_uri' parameter");
                }

                if (hasRequest && hasRequestUri) {
                        return AuthorizationUriValidationResult.Failed("URI cannot contain both 'request' and 'request_uri' parameters");
                }

                return AuthorizationUriValidationResult.Success(hasRequestUri);
        }
}

/// <summary>
/// Result of authorization URI validation.
/// </summary>
public class AuthorizationUriValidationResult {
        /// <summary>
        /// Gets or sets whether the URI is valid.
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// Gets or sets the error message if validation failed.
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets whether the URI uses request_uri parameter.
        /// </summary>
        public bool UsesRequestUri { get; set; }

        /// <summary>
        /// Creates a successful validation result.
        /// </summary>
        /// <param name="usesRequestUri">Whether the URI uses request_uri</param>
        /// <returns>A successful validation result</returns>
        public static AuthorizationUriValidationResult Success(bool usesRequestUri) => new() {
                IsValid = true,
                UsesRequestUri = usesRequestUri
        };

        /// <summary>
        /// Creates a failed validation result.
        /// </summary>
        /// <param name="errorMessage">The error message</param>
        /// <returns>A failed validation result</returns>
        public static AuthorizationUriValidationResult Failed(string errorMessage) => new() {
                IsValid = false,
                ErrorMessage = errorMessage
        };
}
