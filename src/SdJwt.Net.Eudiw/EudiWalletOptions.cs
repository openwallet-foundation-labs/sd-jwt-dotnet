using SdJwt.Net.Eudiw.Arf;
using SdJwt.Net.Eudiw.TrustFramework;

namespace SdJwt.Net.Eudiw;

/// <summary>
/// Configuration options for EU Digital Identity Wallet.
/// </summary>
public class EudiWalletOptions
{
    /// <summary>
    /// Wallet instance identifier.
    /// </summary>
    public string? WalletId
    {
        get; set;
    }

    /// <summary>
    /// Display name for the wallet.
    /// </summary>
    public string? DisplayName { get; set; } = "EUDI Wallet";

    /// <summary>
    /// EU Trust List endpoints for trust anchor resolution.
    /// </summary>
    public IReadOnlyList<string> TrustListEndpoints
    {
        get; set;
    } = new[]
    {
        EudiwConstants.TrustList.LotlUrl,
        EudiwConstants.TrustList.LotlJsonUrl
    };

    /// <summary>
    /// Required minimum HAIP compliance level (Level 2 minimum for EUDI).
    /// </summary>
    public int MinimumHaipLevel { get; set; } = 2;

    /// <summary>
    /// Supported credential types for EUDI ecosystem.
    /// </summary>
    public IReadOnlyList<string> SupportedCredentialTypes
    {
        get; set;
    } = new[]
    {
        EudiwConstants.Pid.DocType,           // Person Identification Data
        EudiwConstants.Mdl.DocType,           // Mobile Driving License
        "eu.europa.ec.eudi.loyalty.1",        // Loyalty credentials
        "eu.europa.ec.eudi.health.1"          // Health credentials
    };

    /// <summary>
    /// Whether to enforce ARF (Architecture Reference Framework) requirements.
    /// </summary>
    public bool EnforceArfCompliance { get; set; } = true;

    /// <summary>
    /// Whether to automatically validate credentials on add.
    /// </summary>
    public bool ValidateOnAdd { get; set; } = true;

    /// <summary>
    /// Whether to validate issuer against EU Trust List.
    /// </summary>
    public bool ValidateIssuerTrust { get; set; } = true;

    /// <summary>
    /// Preferred language for error messages and UI (ISO 639-1).
    /// </summary>
    public string PreferredLanguage { get; set; } = "en";

    /// <summary>
    /// Whether to require hardware-backed key storage.
    /// </summary>
    public bool RequireHardwareKeys
    {
        get; set;
    }

    /// <summary>
    /// Trust list cache timeout in hours.
    /// </summary>
    public int TrustListCacheHours { get; set; } = 6;

    /// <summary>
    /// Supported algorithms per ARF.
    /// </summary>
    public IReadOnlyList<string> SupportedAlgorithms
    {
        get; set;
    } =
        EudiwConstants.Algorithms.SupportedAlgorithms.ToList();
}

/// <summary>
/// Result of EU Trust validation.
/// </summary>
public class EuTrustValidationResult
{
    /// <summary>
    /// Whether the issuer is trusted.
    /// </summary>
    public bool IsTrusted
    {
        get; set;
    }

    /// <summary>
    /// Member state of the trusted service provider.
    /// </summary>
    public string? MemberState
    {
        get; set;
    }

    /// <summary>
    /// Service type of the provider.
    /// </summary>
    public TrustServiceType ServiceType
    {
        get; set;
    }

    /// <summary>
    /// Validation errors if not trusted.
    /// </summary>
    public IReadOnlyList<string> Errors { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Creates a trusted result.
    /// </summary>
    /// <param name="memberState">The member state.</param>
    /// <param name="serviceType">The service type.</param>
    /// <returns>A trusted result.</returns>
    public static EuTrustValidationResult Trusted(string memberState, TrustServiceType serviceType = TrustServiceType.QualifiedAttestation)
    {
        return new EuTrustValidationResult
        {
            IsTrusted = true,
            MemberState = memberState,
            ServiceType = serviceType
        };
    }

    /// <summary>
    /// Creates an untrusted result.
    /// </summary>
    /// <param name="errors">The validation errors.</param>
    /// <returns>An untrusted result.</returns>
    public static EuTrustValidationResult Untrusted(params string[] errors)
    {
        return new EuTrustValidationResult
        {
            IsTrusted = false,
            Errors = errors
        };
    }
}

/// <summary>
/// Exception thrown for EUDI Trust validation failures.
/// </summary>
public class EudiTrustException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EudiTrustException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public EudiTrustException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="EudiTrustException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public EudiTrustException(string message, Exception innerException)
        : base(message, innerException) { }
}

/// <summary>
/// Exception thrown for ARF compliance violations.
/// </summary>
public class ArfComplianceException : Exception
{
    /// <summary>
    /// Gets the list of ARF violations.
    /// </summary>
    public IReadOnlyList<string> Violations
    {
        get;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ArfComplianceException"/> class.
    /// </summary>
    /// <param name="violations">The list of ARF compliance violations.</param>
    public ArfComplianceException(IReadOnlyList<string> violations)
        : base($"ARF compliance violations: {string.Join(", ", violations)}")
    {
        Violations = violations;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ArfComplianceException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public ArfComplianceException(string message)
        : base(message)
    {
        Violations = new[] { message };
    }
}

/// <summary>
/// Options for PID credential request.
/// </summary>
public class PidRequestOptions
{
    /// <summary>
    /// Authentication method to use for issuance.
    /// </summary>
    public EudiAuthMethod AuthMethod { get; set; } = EudiAuthMethod.AuthorizationCode;

    /// <summary>
    /// Pre-authorized code if using pre-auth flow.
    /// </summary>
    public string? PreAuthorizedCode
    {
        get; set;
    }

    /// <summary>
    /// User PIN if required.
    /// </summary>
    public string? UserPin
    {
        get; set;
    }

    /// <summary>
    /// Key algorithm for credential binding.
    /// </summary>
    public string KeyAlgorithm { get; set; } = "ES256";

    /// <summary>
    /// Redirect URI for authorization code flow.
    /// </summary>
    public string? RedirectUri
    {
        get; set;
    }
}

/// <summary>
/// Authentication methods for EUDI credential issuance.
/// </summary>
public enum EudiAuthMethod
{
    /// <summary>
    /// OAuth 2.0 Authorization Code flow.
    /// </summary>
    AuthorizationCode,

    /// <summary>
    /// Pre-Authorized Code flow.
    /// </summary>
    PreAuthorized,

    /// <summary>
    /// eIDAS authentication.
    /// </summary>
    Eidas
}
