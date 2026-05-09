namespace SdJwt.Net.Oid4Vp.DcApi;

/// <summary>
/// Constants for W3C Digital Credentials API integration with OpenID4VP.
/// </summary>
public static class DcApiConstants
{
    /// <summary>
    /// Default protocol identifier for OpenID4VP over Digital Credentials API.
    /// </summary>
    public const string Protocol = Protocols.OpenId4VpV1Unsigned;

    /// <summary>
    /// Protocol identifiers registered for Digital Credentials API requests.
    /// </summary>
    public static class Protocols
    {
        /// <summary>
        /// OpenID4VP 1.0 unsigned request protocol identifier.
        /// </summary>
        public const string OpenId4VpV1Unsigned = "openid4vp-v1-unsigned";

        /// <summary>
        /// OpenID4VP 1.0 signed request protocol identifier.
        /// </summary>
        public const string OpenId4VpV1Signed = "openid4vp-v1-signed";

        /// <summary>
        /// OpenID4VP 1.0 multisigned request protocol identifier.
        /// </summary>
        public const string OpenId4VpV1Multisigned = "openid4vp-v1-multisigned";

        /// <summary>
        /// ISO mdoc direct presentation protocol identifier.
        /// </summary>
        public const string IsoMdoc = "org-iso-mdoc";

        /// <summary>
        /// OpenID4VCI 1.0 issuance protocol identifier.
        /// </summary>
        public const string OpenId4VciV1 = "openid4vci-v1";
    }

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
