using System.Text.Json.Serialization;

namespace SdJwt.Net.HAIP;

/// <summary>
/// HAIP Compliance Levels with specific security requirements
/// </summary>
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
    /// Algorithms allowed in HAIP Level 1 (High)
    /// </summary>
    public static readonly string[] Level1_Algorithms = { "ES256", "ES384", "PS256", "PS384", "EdDSA" };
    
    /// <summary>
    /// Algorithms allowed in HAIP Level 2 (Very High)
    /// </summary>
    public static readonly string[] Level2_Algorithms = { "ES384", "ES512", "PS384", "PS512", "EdDSA" };
    
    /// <summary>
    /// Algorithms allowed in HAIP Level 3 (Sovereign)
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