namespace SdJwt.Net.Oid4Vp.Models;

/// <summary>
/// Common constants used in OID4VP 1.0 protocol.
/// </summary>
public static class Oid4VpConstants
{
    /// <summary>
    /// The scheme used in authorization request URIs.
    /// </summary>
    public const string AuthorizationRequestScheme = "openid4vp";

    /// <summary>
    /// The credential format identifier for SD-JWT credentials.
    /// </summary>
    public const string SdJwtVcFormat = "dc+sd-jwt";

    /// <summary>
    /// Legacy SD-JWT VC credential format identifier accepted for backward compatibility.
    /// </summary>
    public const string SdJwtVcLegacyFormat = "vc+sd-jwt";

    /// <summary>
    /// The JWT type for key binding JWTs in presentations.
    /// </summary>
    public const string KeyBindingJwtType = "kb+jwt";

    /// <summary>
    /// The JWT type for signed authorization request objects (JAR).
    /// </summary>
    public const string AuthorizationRequestJwtType = "oauth-authz-req+jwt";

    /// <summary>
    /// Response modes supported by OID4VP.
    /// </summary>
    public static class ResponseModes
    {
        /// <summary>
        /// Direct post response mode for cross-device flows.
        /// </summary>
        public const string DirectPost = "direct_post";

        /// <summary>
        /// Direct post JWT response mode.
        /// </summary>
        public const string DirectPostJwt = "direct_post.jwt";

        /// <summary>
        /// Fragment response mode.
        /// </summary>
        public const string Fragment = "fragment";

        /// <summary>
        /// Query response mode.
        /// </summary>
        public const string Query = "query";
    }

    /// <summary>
    /// Response types supported by OID4VP.
    /// </summary>
    public static class ResponseTypes
    {
        /// <summary>
        /// VP token response type.
        /// </summary>
        public const string VpToken = "vp_token";
    }

    /// <summary>
    /// Client ID schemes for OID4VP.
    /// </summary>
    public static class ClientIdSchemes
    {
        /// <summary>
        /// Redirect URI scheme.
        /// </summary>
        public const string RedirectUri = "redirect_uri";

        /// <summary>
        /// Entity identifier scheme.
        /// </summary>
        public const string EntityId = "entity_id";

        /// <summary>
        /// DID scheme.
        /// </summary>
        public const string Did = "did";

        /// <summary>
        /// Web scheme.
        /// </summary>
        public const string Web = "web";

        /// <summary>
        /// X509 SAN DNS scheme.
        /// </summary>
        public const string X509SanDns = "x509_san_dns";

        /// <summary>
        /// X509 SAN URI scheme.
        /// </summary>
        public const string X509SanUri = "x509_san_uri";

        /// <summary>
        /// Verifier attestation scheme.
        /// </summary>
        public const string VerifierAttestation = "verifier_attestation";

        /// <summary>
        /// Pre-registered scheme.
        /// </summary>
        public const string PreRegistered = "pre-registered";
    }

    /// <summary>
    /// Presentation Exchange related constants.
    /// </summary>
    public static class PresentationExchange
    {
        /// <summary>
        /// Presentation Exchange version 2.0.0.
        /// </summary>
        public const string Version = "2.0.0";

        /// <summary>
        /// Submission requirement rule types.
        /// </summary>
        public static class SubmissionRequirementRules
        {
            /// <summary>
            /// All rule - requires all listed descriptors.
            /// </summary>
            public const string All = "all";

            /// <summary>
            /// Pick rule - requires selection from listed descriptors.
            /// </summary>
            public const string Pick = "pick";
        }

        /// <summary>
        /// Input descriptor constraints field filter types.
        /// </summary>
        public static class FilterTypes
        {
            /// <summary>
            /// String filter type.
            /// </summary>
            public const string String = "string";

            /// <summary>
            /// Number filter type.
            /// </summary>
            public const string Number = "number";

            /// <summary>
            /// Array filter type.
            /// </summary>
            public const string Array = "array";

            /// <summary>
            /// Object filter type.
            /// </summary>
            public const string Object = "object";

            /// <summary>
            /// Boolean filter type.
            /// </summary>
            public const string Boolean = "boolean";
        }

        /// <summary>
        /// Path nested descriptor properties.
        /// </summary>
        public static class PathNestedProperties
        {
            /// <summary>
            /// Format property for nested descriptors.
            /// </summary>
            public const string Format = "format";

            /// <summary>
            /// Path property for nested descriptors.
            /// </summary>
            public const string Path = "path";

            /// <summary>
            /// Path nested property for nested descriptors.
            /// </summary>
            public const string PathNested = "path_nested";
        }
    }

    /// <summary>
    /// Standard error codes for authorization responses according to OID4VP 1.0.
    /// </summary>
    public static class ErrorCodes
    {
        /// <summary>
        /// The request is missing a required parameter, includes an invalid parameter value, 
        /// includes a parameter more than once, or is otherwise malformed.
        /// </summary>
        public const string InvalidRequest = "invalid_request";

        /// <summary>
        /// The client is not authorized to request a presentation using this method.
        /// </summary>
        public const string UnauthorizedClient = "unauthorized_client";

        /// <summary>
        /// The resource owner or authorization server denied the request.
        /// </summary>
        public const string AccessDenied = "access_denied";

        /// <summary>
        /// The authorization server does not support obtaining a presentation using this method.
        /// </summary>
        public const string UnsupportedResponseType = "unsupported_response_type";

        /// <summary>
        /// The requested scope is invalid, unknown, or malformed.
        /// </summary>
        public const string InvalidScope = "invalid_scope";

        /// <summary>
        /// The authorization server encountered an unexpected condition.
        /// </summary>
        public const string ServerError = "server_error";

        /// <summary>
        /// The authorization server is currently unable to handle the request.
        /// </summary>
        public const string TemporarilyUnavailable = "temporarily_unavailable";

        /// <summary>
        /// The wallet could not find credentials satisfying the presentation definition.
        /// </summary>
        public const string VpFormatsNotSupported = "vp_formats_not_supported";

        /// <summary>
        /// The presentation definition is invalid or malformed.
        /// </summary>
        public const string InvalidPresentationDefinitionUri = "invalid_presentation_definition_uri";

        /// <summary>
        /// The presentation definition object is invalid.
        /// </summary>
        public const string InvalidPresentationDefinitionObject = "invalid_presentation_definition_object";
    }

    /// <summary>
    /// Media types for OID4VP.
    /// </summary>
    public static class MediaTypes
    {
        /// <summary>
        /// SD-JWT application media type.
        /// </summary>
        public const string SdJwt = "application/sd-jwt";

        /// <summary>
        /// Key binding JWT application media type.
        /// </summary>
        public const string KeyBindingJwt = "application/kb+jwt";

        /// <summary>
        /// VP token application media type.
        /// </summary>
        public const string VpToken = "application/vp-token";
    }

    /// <summary>
    /// JSONPath expressions commonly used in presentation exchange.
    /// </summary>
    public static class JsonPaths
    {
        /// <summary>
        /// Path to the credential type.
        /// </summary>
        public const string CredentialType = "$.vct";

        /// <summary>
        /// Path to the credential subject.
        /// </summary>
        public const string CredentialSubject = "$.credentialSubject";

        /// <summary>
        /// Path to the issuer.
        /// </summary>
        public const string Issuer = "$.iss";

        /// <summary>
        /// Path to the subject.
        /// </summary>
        public const string Subject = "$.sub";

        /// <summary>
        /// Path to the expiration time.
        /// </summary>
        public const string ExpirationTime = "$.exp";

        /// <summary>
        /// Path to the issued at time.
        /// </summary>
        public const string IssuedAt = "$.iat";

        /// <summary>
        /// Path to the not before time.
        /// </summary>
        public const string NotBefore = "$.nbf";
    }

    /// <summary>
    /// Values for the <c>request_uri_method</c> parameter (OID4VP 1.0 Section 5.10).
    /// Indicates the HTTP method the wallet SHOULD use to retrieve the request object
    /// from the <c>request_uri</c>.
    /// </summary>
    public static class RequestUriMethods
    {
        /// <summary>
        /// The wallet uses HTTP GET to retrieve the request object.
        /// This is the default behaviour when <c>request_uri_method</c> is absent.
        /// </summary>
        public const string Get = "get";

        /// <summary>
        /// The wallet uses HTTP POST to retrieve the request object,
        /// sending its capabilities in the request body.
        /// </summary>
        public const string Post = "post";
    }
}
