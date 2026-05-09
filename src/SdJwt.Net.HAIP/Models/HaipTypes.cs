using System.Text.Json.Serialization;

namespace SdJwt.Net.HAIP;

/// <summary>
/// Legacy non-normative assurance levels retained for source compatibility.
/// </summary>
/// <remarks>
/// OpenID4VC HAIP 1.0 Final does not define Level 1, Level 2, or Level 3
/// conformance tiers. New code should use <see cref="HaipFlow"/> and
/// <see cref="HaipCredentialProfile"/> based validation instead.
/// </remarks>
public enum HaipLevel
{
    /// <summary>
    /// Standard HAIP compliance (eIDAS Level High)
    /// - ES256/ES384/PS256 mandatory
    /// - DPoP required for public clients
    /// - Proof of possession mandatory
    /// </summary>
    Level1_High,

    /// <summary>
    /// Very High Assurance (eIDAS Level Very High)
    /// - ES384/ES512/PS384+ mandatory
    /// - Wallet Attestation required
    /// - Multi-signature support
    /// - Enhanced trust chain validation
    /// </summary>
    Level2_VeryHigh,

    /// <summary>
    /// Sovereign/Government level
    /// - Hardware security module required
    /// - Qualified electronic signatures
    /// - National trust framework integration
    /// </summary>
    Level3_Sovereign
}

/// <summary>
/// HAIP severity levels for compliance violations
/// </summary>
public enum HaipSeverity
{
    /// <summary>
    /// Information only - does not affect compliance
    /// </summary>
    Info,

    /// <summary>
    /// Warning - potential security concern but not blocking
    /// </summary>
    Warning,

    /// <summary>
    /// Critical - blocks compliance, must be fixed
    /// </summary>
    Critical
}

/// <summary>
/// Types of HAIP compliance violations
/// </summary>
public enum HaipViolationType
{
    /// <summary>
    /// Weak or forbidden cryptographic algorithms
    /// </summary>
    WeakCryptography,

    /// <summary>
    /// Missing proof of possession requirement
    /// </summary>
    MissingProofOfPossession,

    /// <summary>
    /// Insecure client authentication method
    /// </summary>
    InsecureClientAuthentication,

    /// <summary>
    /// Untrusted issuer or invalid trust chain
    /// </summary>
    UntrustedIssuer,

    /// <summary>
    /// Expired or invalid certificates
    /// </summary>
    ExpiredCertificate,

    /// <summary>
    /// Insufficient assurance level for requirement
    /// </summary>
    InsufficientAssuranceLevel,

    /// <summary>
    /// Missing required transport security
    /// </summary>
    InsecureTransport,

    /// <summary>
    /// Invalid key strength for assurance level
    /// </summary>
    WeakKeyStrength
}

/// <summary>
/// HAIP constants and algorithm definitions
/// </summary>
public static class HaipConstants
{
    /// <summary>
    /// OpenID4VC HAIP 1.0 Final specification version implemented by this package.
    /// </summary>
    public const string FinalSpecificationVersion = "1.0-final";

    /// <summary>
    /// Minimum JOSE algorithm that HAIP Final requires entities to support for validation.
    /// </summary>
    public const string RequiredJoseAlgorithm = "ES256";

    /// <summary>
    /// Minimum COSE algorithms that HAIP Final requires entities to support for validation.
    /// </summary>
    public static readonly int[] RequiredCoseAlgorithms = { -7, -9 };

    /// <summary>
    /// Hash algorithm that HAIP Final requires for SD-JWT VC and ISO mdoc digest validation.
    /// </summary>
    public const string RequiredHashAlgorithm = "SHA-256";

    /// <summary>
    /// SD-JWT VC credential format identifier required by HAIP Final.
    /// </summary>
    public const string SdJwtVcFormat = "dc+sd-jwt";

    /// <summary>
    /// ISO mdoc credential format identifier used by OpenID4VC.
    /// </summary>
    public const string MsoMdocFormat = "mso_mdoc";

    /// <summary>
    /// Legacy algorithms allowed in the old non-normative Level 1 profile.
    /// </summary>
    public static readonly string[] Level1_Algorithms = { "ES256", "ES384", "PS256", "PS384", "EdDSA" };

    /// <summary>
    /// Legacy algorithms allowed in the old non-normative Level 2 profile.
    /// </summary>
    public static readonly string[] Level2_Algorithms = { "ES384", "ES512", "PS384", "PS512", "EdDSA" };

    /// <summary>
    /// Legacy algorithms allowed in the old non-normative Level 3 profile.
    /// </summary>
    public static readonly string[] Level3_Algorithms = { "ES512", "PS512", "EdDSA" };

    /// <summary>
    /// Algorithms explicitly forbidden in HAIP
    /// </summary>
    public static readonly string[] ForbiddenAlgorithms = { "RS256", "HS256", "HS384", "HS512", "none" };

    /// <summary>
    /// Client authentication methods for different HAIP levels
    /// </summary>
    public static class ClientAuthMethods
    {
        /// <summary>
        /// Client authentication methods allowed for HAIP Level 1 (High)
        /// </summary>
        public static readonly string[] Level1_Allowed = { "private_key_jwt", "client_secret_jwt", "attest_jwt_client_auth" };

        /// <summary>
        /// Client authentication methods required for HAIP Level 2 (Very High)
        /// </summary>
        public static readonly string[] Level2_Required = { "attest_jwt_client_auth", "private_key_jwt" };

        /// <summary>
        /// Client authentication methods required for HAIP Level 3 (Sovereign)
        /// </summary>
        public static readonly string[] Level3_Required = { "attest_jwt_client_auth" };
    }

    /// <summary>
    /// Minimum key sizes for different HAIP levels
    /// </summary>
    public static class KeySizes
    {
        /// <summary>
        /// Minimum elliptic curve key size for HAIP Level 1 (P-256)
        /// </summary>
        public const int Level1_EcMinimum = 256;  // P-256

        /// <summary>
        /// Minimum elliptic curve key size for HAIP Level 2 (P-384)
        /// </summary>
        public const int Level2_EcMinimum = 384;  // P-384

        /// <summary>
        /// Minimum elliptic curve key size for HAIP Level 3 (P-521)
        /// </summary>
        public const int Level3_EcMinimum = 521;  // P-521

        /// <summary>
        /// Minimum RSA key size for HAIP Level 1 (2048 bits)
        /// </summary>
        public const int Level1_RsaMinimum = 2048;

        /// <summary>
        /// Minimum RSA key size for HAIP Level 2 (3072 bits)
        /// </summary>
        public const int Level2_RsaMinimum = 3072;

        /// <summary>
        /// Minimum RSA key size for HAIP Level 3 (4096 bits)
        /// </summary>
        public const int Level3_RsaMinimum = 4096;
    }
}

/// <summary>
/// HAIP type constants
/// </summary>
public static class HaipTypes
{
    /// <summary>
    /// HAIP request type identifier
    /// </summary>
    public const string HaipRequestType = "haip_request";

    /// <summary>
    /// HAIP response type identifier
    /// </summary>
    public const string HaipResponseType = "haip_response";

    /// <summary>
    /// Authorization details type for HAIP
    /// </summary>
    public const string AuthorizationDetailsType = "openid_credential";
}

/// <summary>
/// Represents a HAIP request
/// </summary>
public class HaipRequest
{
    /// <summary>
    /// Gets or sets the endpoint URL
    /// </summary>
    [JsonPropertyName("endpoint")]
    public string? Endpoint
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the nonce value
    /// </summary>
    [JsonPropertyName("nonce")]
    public string? Nonce
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the state value
    /// </summary>
    [JsonPropertyName("state")]
    public string? State
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the authorization details
    /// </summary>
    [JsonPropertyName("authorization_details")]
    public AuthorizationDetailsRequest? AuthorizationDetails
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the wallet data
    /// </summary>
    [JsonPropertyName("wallet_data")]
    public WalletData? WalletData
    {
        get; set;
    }
}

/// <summary>
/// Represents a HAIP response
/// </summary>
public class HaipResponse
{
    /// <summary>
    /// Gets or sets the response data
    /// </summary>
    [JsonPropertyName("response_data")]
    public Dictionary<string, object>? ResponseData
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the state value
    /// </summary>
    [JsonPropertyName("state")]
    public string? State
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the error code
    /// </summary>
    [JsonPropertyName("error")]
    public string? Error
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the error description
    /// </summary>
    [JsonPropertyName("error_description")]
    public string? ErrorDescription
    {
        get; set;
    }
}

/// <summary>
/// Represents authorization details in a HAIP request
/// </summary>
public class AuthorizationDetailsRequest
{
    /// <summary>
    /// Gets or sets the type of authorization details
    /// </summary>
    [JsonPropertyName("type")]
    public string? Type
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the locations where the authorization applies
    /// </summary>
    [JsonPropertyName("locations")]
    public string[]? Locations
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the actions that are authorized
    /// </summary>
    [JsonPropertyName("actions")]
    public string[]? Actions
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the data types covered by the authorization
    /// </summary>
    [JsonPropertyName("data_types")]
    public string[]? DataTypes
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the identifier for the authorization
    /// </summary>
    [JsonPropertyName("identifier")]
    public string? Identifier
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the privileges or additional parameters
    /// </summary>
    [JsonPropertyName("privileges")]
    public Dictionary<string, object>? Privileges
    {
        get; set;
    }
}

/// <summary>
/// Represents wallet data in a HAIP request
/// </summary>
public class WalletData
{
    /// <summary>
    /// Gets or sets the wallet identifier
    /// </summary>
    [JsonPropertyName("wallet_id")]
    public string? WalletId
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the wallet name
    /// </summary>
    [JsonPropertyName("wallet_name")]
    public string? WalletName
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the wallet version
    /// </summary>
    [JsonPropertyName("wallet_version")]
    public string? WalletVersion
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the supported credential formats
    /// </summary>
    [JsonPropertyName("supported_formats")]
    public string[]? SupportedFormats
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the supported cryptographic suites
    /// </summary>
    [JsonPropertyName("supported_cryptographic_suites")]
    public string[]? SupportedCryptographicSuites
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the wallet capabilities
    /// </summary>
    [JsonPropertyName("capabilities")]
    public Dictionary<string, object>? Capabilities
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets additional metadata
    /// </summary>
    [JsonPropertyName("metadata")]
    public Dictionary<string, object>? Metadata
    {
        get; set;
    }
}
