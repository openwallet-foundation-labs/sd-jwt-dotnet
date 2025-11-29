using System.Text.Json.Serialization;

namespace SdJwt.Net.Oid4Vci.Models;

/// <summary>
/// Common constants used in OID4VCI 1.0 protocol.
/// </summary>
public static class Oid4VciConstants
{
    /// <summary>
    /// The credential format identifier for SD-JWT credentials.
    /// </summary>
    public const string SdJwtVcFormat = "vc+sd-jwt";

    /// <summary>
    /// The JWT type for proof of possession JWTs in OID4VCI.
    /// </summary>
    public const string ProofJwtType = "openid4vci-proof+jwt";

    /// <summary>
    /// The scheme used in credential offer URIs.
    /// </summary>
    public const string CredentialOfferScheme = "openid-credential-offer";

    /// <summary>
    /// Client assertion type for JWT bearer tokens.
    /// </summary>
    public const string JwtBearerClientAssertionType = "urn:ietf:params:oauth:client-assertion-type:jwt-bearer";

    /// <summary>
    /// Grant types supported by OID4VCI.
    /// </summary>
    public static class GrantTypes
    {
        /// <summary>
        /// Pre-authorized code grant type.
        /// </summary>
        public const string PreAuthorizedCode = "urn:ietf:params:oauth:grant-type:pre-authorized_code";

        /// <summary>
        /// Authorization code grant type.
        /// </summary>
        public const string AuthorizationCode = "authorization_code";

        /// <summary>
        /// Refresh token grant type.
        /// </summary>
        public const string RefreshToken = "refresh_token";
    }

    /// <summary>
    /// Proof types supported by OID4VCI.
    /// </summary>
    public static class ProofTypes
    {
        /// <summary>
        /// JWT proof type.
        /// </summary>
        public const string Jwt = "jwt";

        /// <summary>
        /// CWT proof type.
        /// </summary>
        public const string Cwt = "cwt";

        /// <summary>
        /// Linked Data Proof Verifiable Presentation proof type.
        /// </summary>
        public const string LdpVp = "ldp_vp";
    }

    /// <summary>
    /// Standard error codes for token responses according to RFC 6749.
    /// </summary>
    public static class TokenErrorCodes
    {
        /// <summary>
        /// The request is missing a required parameter, includes an unsupported parameter value, 
        /// repeats a parameter, includes multiple credentials, utilizes more than one mechanism 
        /// for authenticating the client, or is otherwise malformed.
        /// </summary>
        public const string InvalidRequest = "invalid_request";

        /// <summary>
        /// Client authentication failed.
        /// </summary>
        public const string InvalidClient = "invalid_client";

        /// <summary>
        /// The provided authorization grant or refresh token is invalid, expired, revoked, 
        /// does not match the redirection URI used in the authorization request, or was issued to another client.
        /// </summary>
        public const string InvalidGrant = "invalid_grant";

        /// <summary>
        /// The client is not authorized to request an authorization code using this method.
        /// </summary>
        public const string UnauthorizedClient = "unauthorized_client";

        /// <summary>
        /// The authorization grant type is not supported by the authorization server.
        /// </summary>
        public const string UnsupportedGrantType = "unsupported_grant_type";

        /// <summary>
        /// The requested scope is invalid, unknown, or malformed.
        /// </summary>
        public const string InvalidScope = "invalid_scope";

        /// <summary>
        /// The request is missing a transaction code or the transaction code is invalid.
        /// </summary>
        public const string InvalidTransactionCode = "invalid_transaction_code";
    }

    /// <summary>
    /// Standard error codes for credential responses according to OID4VCI 1.0.
    /// </summary>
    public static class CredentialErrorCodes
    {
        /// <summary>
        /// The credential request is missing a required parameter, includes an unsupported parameter value, 
        /// repeats a parameter, or is otherwise malformed.
        /// </summary>
        public const string InvalidRequest = "invalid_request";

        /// <summary>
        /// The access token provided is expired, revoked, malformed, or invalid for other reasons.
        /// </summary>
        public const string InvalidToken = "invalid_token";

        /// <summary>
        /// The access token provided is not sufficient for the request.
        /// </summary>
        public const string InsufficientScope = "insufficient_scope";

        /// <summary>
        /// The Credential Issuer does not support the credential format.
        /// </summary>
        public const string UnsupportedCredentialFormat = "unsupported_credential_format";

        /// <summary>
        /// The Credential Issuer does not support the credential type.
        /// </summary>
        public const string UnsupportedCredentialType = "unsupported_credential_type";

        /// <summary>
        /// The proof in the Credential Request is invalid.
        /// </summary>
        public const string InvalidProof = "invalid_proof";

        /// <summary>
        /// The proof is missing from the Credential Request.
        /// </summary>
        public const string InvalidOrMissingProof = "invalid_or_missing_proof";
    }

    /// <summary>
    /// Input modes for transaction codes.
    /// </summary>
    public static class InputModes
    {
        /// <summary>
        /// Numeric input mode (digits 0-9).
        /// </summary>
        public const string Numeric = "numeric";

        /// <summary>
        /// Text input mode (any UTF-8 characters).
        /// </summary>
        public const string Text = "text";
    }

    /// <summary>
    /// Token types.
    /// </summary>
    public static class TokenTypes
    {
        /// <summary>
        /// Bearer token type.
        /// </summary>
        public const string Bearer = "Bearer";

        /// <summary>
        /// DPoP token type.
        /// </summary>
        public const string DPoP = "DPoP";
    }
}