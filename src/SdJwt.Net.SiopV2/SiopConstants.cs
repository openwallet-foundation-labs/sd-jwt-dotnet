namespace SdJwt.Net.SiopV2;

/// <summary>
/// Constants used by Self-Issued OpenID Provider v2 processing.
/// </summary>
public static class SiopConstants
{
    /// <summary>
    /// The SIOPv2 custom URL scheme.
    /// </summary>
    public const string SiopV2Scheme = "siopv2";

    /// <summary>
    /// The OpenID custom URL scheme used by combined SIOP and OpenID4VP flows.
    /// </summary>
    public const string OpenIdScheme = "openid";

    /// <summary>
    /// SIOPv2 response type values.
    /// </summary>
    public static class ResponseTypes
    {
        /// <summary>
        /// Self-Issued ID Token response type.
        /// </summary>
        public const string IdToken = "id_token";

        /// <summary>
        /// OpenID4VP VP Token response type.
        /// </summary>
        public const string VpToken = "vp_token";

        /// <summary>
        /// Combined OpenID4VP and SIOPv2 response type.
        /// </summary>
        public const string VpTokenIdToken = "vp_token id_token";
    }

    /// <summary>
    /// SIOPv2 scope values.
    /// </summary>
    public static class Scopes
    {
        /// <summary>
        /// OpenID Connect scope.
        /// </summary>
        public const string OpenId = "openid";
    }

    /// <summary>
    /// SIOPv2 subject syntax type identifiers.
    /// </summary>
    public static class SubjectSyntaxTypes
    {
        /// <summary>
        /// JWK thumbprint subject syntax type.
        /// </summary>
        public const string JwkThumbprint = "urn:ietf:params:oauth:jwk-thumbprint";

        /// <summary>
        /// Prefix used by SIOPv2 examples for SHA-256 JWK thumbprint subject identifiers.
        /// </summary>
        public const string JwkThumbprintSha256Prefix = "urn:ietf:params:oauth:jwk-thumbprint:sha-256:";

        /// <summary>
        /// Decentralized Identifier subject syntax type.
        /// </summary>
        public const string DecentralizedIdentifier = "did";
    }

    /// <summary>
    /// SIOPv2 ID Token type values.
    /// </summary>
    public static class IdTokenTypes
    {
        /// <summary>
        /// Subject-signed ID Token.
        /// </summary>
        public const string SubjectSigned = "subject_signed_id_token";
    }

    /// <summary>
    /// SIOPv2-specific claim names.
    /// </summary>
    public static class Claims
    {
        /// <summary>
        /// Claim carrying the subject public JWK.
        /// </summary>
        public const string SubJwk = "sub_jwk";

        /// <summary>
        /// Request parameter identifying the ID Token type.
        /// </summary>
        public const string IdTokenType = "id_token_type";
    }

    /// <summary>
    /// SIOPv2-specific error codes.
    /// </summary>
    public static class ErrorCodes
    {
        /// <summary>
        /// End-user cancelled the authorization request.
        /// </summary>
        public const string UserCancelled = "user_cancelled";

        /// <summary>
        /// A supplied client metadata value is not supported.
        /// </summary>
        public const string ClientMetadataValueNotSupported = "client_metadata_value_not_supported";

        /// <summary>
        /// No mutually supported subject syntax type is available.
        /// </summary>
        public const string SubjectSyntaxTypesNotSupported = "subject_syntax_types_not_supported";

        /// <summary>
        /// Client metadata URI could not be fetched or was invalid.
        /// </summary>
        public const string InvalidClientMetadataUri = "invalid_client_metadata_uri";

        /// <summary>
        /// Client metadata object was invalid.
        /// </summary>
        public const string InvalidClientMetadataObject = "invalid_client_metadata_object";
    }
}
