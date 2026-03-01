namespace SdJwt.Net.Oid4Vp.DcApi;

/// <summary>
/// Constants for W3C Digital Credentials API integration with OpenID4VP.
/// </summary>
public static class DcApiConstants
{
    /// <summary>
    /// Protocol identifier for OpenID4VP over Digital Credentials API.
    /// </summary>
    public const string Protocol = "openid4vp";

    /// <summary>
    /// Client ID scheme for web origin binding in DC API flows.
    /// </summary>
    public const string WebOriginScheme = "web-origin";

    /// <summary>
    /// Standard digital credential type for navigator.credentials.get().
    /// </summary>
    public const string CredentialType = "digital";

    /// <summary>
    /// Response modes supported by the Digital Credentials API.
    /// </summary>
    public static class ResponseModes
    {
        /// <summary>
        /// Plain response mode - VP token returned directly in the response.
        /// This is the default mode for non-sensitive credentials.
        /// </summary>
        public const string DcApi = "dc_api";

        /// <summary>
        /// Encrypted response mode - VP token wrapped in JWE for privacy.
        /// Use this mode for credentials containing sensitive PII.
        /// </summary>
        public const string DcApiJwt = "dc_api.jwt";
    }

    /// <summary>
    /// Error codes specific to DC API validation.
    /// </summary>
    public static class ErrorCodes
    {
        /// <summary>
        /// Browser origin does not match expected client_id.
        /// </summary>
        public const string OriginMismatch = "origin_mismatch";

        /// <summary>
        /// Nonce in response does not match expected value.
        /// </summary>
        public const string NonceMismatch = "nonce_mismatch";

        /// <summary>
        /// Presentation is older than maximum allowed age.
        /// </summary>
        public const string PresentationExpired = "presentation_expired";

        /// <summary>
        /// Invalid protocol in response.
        /// </summary>
        public const string InvalidProtocol = "invalid_protocol";

        /// <summary>
        /// Failed to decrypt dc_api.jwt response.
        /// </summary>
        public const string DecryptionFailed = "decryption_failed";
    }
}
