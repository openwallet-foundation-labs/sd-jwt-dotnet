using System.Text.Json.Serialization;

namespace SdJwt.Net.HAIP.Models;

/// <summary>
/// Represents a HAIP compliance violation
/// </summary>
public class HaipViolation
{
    /// <summary>
    /// Type of violation
    /// </summary>
    public HaipViolationType Type
    {
        get; set;
    }

    /// <summary>
    /// Human-readable description of the violation
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Severity level of the violation
    /// </summary>
    public HaipSeverity Severity
    {
        get; set;
    }

    /// <summary>
    /// Recommended action to fix the violation
    /// </summary>
    public string RecommendedAction { get; set; } = string.Empty;

    /// <summary>
    /// Additional context or details about the violation
    /// </summary>
    public Dictionary<string, object>? Context
    {
        get; set;
    }

    /// <summary>
    /// Timestamp when the violation was detected
    /// </summary>
    public DateTimeOffset DetectedAt { get; set; } = DateTimeOffset.UtcNow;
}

/// <summary>
/// Result of HAIP compliance validation
/// </summary>
public class HaipComplianceResult
{
    /// <summary>
    /// Whether the item being validated is HAIP compliant
    /// </summary>
    public bool IsCompliant
    {
        get; set;
    }

    /// <summary>
    /// Highest HAIP level achieved by the item
    /// </summary>
    public HaipLevel AchievedLevel
    {
        get; set;
    }

    /// <summary>
    /// List of compliance violations found
    /// </summary>
    public List<HaipViolation> Violations { get; set; } = new();

    /// <summary>
    /// Audit trail of the validation process
    /// </summary>
    public HaipAuditTrail AuditTrail { get; set; } = new();

    /// <summary>
    /// Additional metadata about the validation
    /// </summary>
    public Dictionary<string, object>? Metadata
    {
        get; set;
    }

    /// <summary>
    /// Adds a violation to the result
    /// </summary>
    public void AddViolation(string description, HaipViolationType type, HaipSeverity severity = HaipSeverity.Critical,
        string? recommendedAction = null)
    {
        Violations.Add(new HaipViolation
        {
            Description = description,
            Type = type,
            Severity = severity,
            RecommendedAction = recommendedAction ?? GetDefaultRecommendation(type)
        });

        // Update compliance status
        if (severity == HaipSeverity.Critical)
        {
            IsCompliant = false;
        }
    }

    private static string GetDefaultRecommendation(HaipViolationType type)
    {
        return type switch
        {
            HaipViolationType.WeakCryptography => "Use ES256, ES384, ES512, PS256, PS384, PS512, or EdDSA",
            HaipViolationType.MissingProofOfPossession => "Enable proof of possession requirement",
            HaipViolationType.InsecureClientAuthentication => "Use attest_jwt_client_auth or private_key_jwt",
            HaipViolationType.UntrustedIssuer => "Ensure issuer is part of trusted federation",
            HaipViolationType.ExpiredCertificate => "Renew expired certificates",
            HaipViolationType.InsufficientAssuranceLevel => "Upgrade to higher assurance level",
            HaipViolationType.InsecureTransport => "Use HTTPS with TLS 1.2 or higher",
            HaipViolationType.WeakKeyStrength => "Use stronger cryptographic keys",
            _ => "Review HAIP compliance requirements"
        };
    }
}

/// <summary>
/// Audit trail for HAIP compliance validation
/// </summary>
public class HaipAuditTrail
{
    /// <summary>
    /// Validation start timestamp
    /// </summary>
    public DateTimeOffset StartedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Validation completion timestamp
    /// </summary>
    public DateTimeOffset? CompletedAt
    {
        get; set;
    }

    /// <summary>
    /// List of validation steps performed
    /// </summary>
    public List<HaipAuditStep> Steps { get; set; } = new();

    /// <summary>
    /// Validator that performed the validation
    /// </summary>
    public string ValidatorId { get; set; } = string.Empty;

    /// <summary>
    /// Version of HAIP specification used
    /// </summary>
    public string HaipVersion { get; set; } = "1.0";

    /// <summary>
    /// Adds a step to the audit trail
    /// </summary>
    public void AddStep(string operation, bool success, string? details = null)
    {
        Steps.Add(new HaipAuditStep
        {
            Operation = operation,
            Success = success,
            Details = details,
            Timestamp = DateTimeOffset.UtcNow
        });
    }

    /// <summary>
    /// Marks the validation as completed
    /// </summary>
    public void Complete()
    {
        CompletedAt = DateTimeOffset.UtcNow;
    }
}

/// <summary>
/// Individual audit step in HAIP validation
/// </summary>
public class HaipAuditStep
{
    /// <summary>
    /// Operation that was performed
    /// </summary>
    public string Operation { get; set; } = string.Empty;

    /// <summary>
    /// Whether the operation succeeded
    /// </summary>
    public bool Success
    {
        get; set;
    }

    /// <summary>
    /// Additional details about the operation
    /// </summary>
    public string? Details
    {
        get; set;
    }

    /// <summary>
    /// Timestamp when the operation was performed
    /// </summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
}

/// <summary>
/// HAIP configuration options
/// </summary>
public class HaipConfiguration
{
    /// <summary>
    /// Required HAIP compliance level
    /// </summary>
    public HaipLevel RequiredLevel { get; set; } = HaipLevel.Level1_High;

    /// <summary>
    /// Trust frameworks to validate against
    /// </summary>
    public string[] TrustFrameworks { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Whether to enable eIDAS compliance checking
    /// </summary>
    public bool EnableEidasCompliance { get; set; } = true;

    /// <summary>
    /// Whether to enable sovereign/government compliance
    /// </summary>
    public bool EnableSovereignCompliance
    {
        get; set;
    }

    /// <summary>
    /// Auditing configuration
    /// </summary>
    public HaipAuditingOptions AuditingOptions { get; set; } = new();

    /// <summary>
    /// Custom extension parameters
    /// </summary>
    public Dictionary<string, object> ExtensionParameters { get; set; } = new();

    /// <summary>
    /// Gets default configuration for specified HAIP level
    /// </summary>
    public static HaipConfiguration GetDefault(HaipLevel level)
    {
        return level switch
        {
            HaipLevel.Level1_High => new HaipConfiguration
            {
                RequiredLevel = level,
                TrustFrameworks = new[] { "https://trust.eudi.europa.eu" },
                EnableEidasCompliance = true
            },
            HaipLevel.Level2_VeryHigh => new HaipConfiguration
            {
                RequiredLevel = level,
                TrustFrameworks = new[] { "https://trust.eudi.europa.eu", "https://trust.government.example" },
                EnableEidasCompliance = true,
                AuditingOptions = new HaipAuditingOptions { DetailedLogging = true }
            },
            HaipLevel.Level3_Sovereign => new HaipConfiguration
            {
                RequiredLevel = level,
                EnableSovereignCompliance = true,
                EnableEidasCompliance = true,
                AuditingOptions = new HaipAuditingOptions
                {
                    DetailedLogging = true,
                    RequireDigitalSignature = true
                }
            },
            _ => new HaipConfiguration()
        };
    }
}

/// <summary>
/// HAIP auditing options
/// </summary>
public class HaipAuditingOptions
{
    /// <summary>
    /// Enable detailed logging of validation steps
    /// </summary>
    public bool DetailedLogging
    {
        get; set;
    }

    /// <summary>
    /// Require digital signatures on audit records
    /// </summary>
    public bool RequireDigitalSignature
    {
        get; set;
    }

    /// <summary>
    /// Maximum age of cached validation results
    /// </summary>
    public TimeSpan CacheTimeout { get; set; } = TimeSpan.FromMinutes(30);

    /// <summary>
    /// Whether to store audit logs persistently
    /// </summary>
    public bool PersistentStorage
    {
        get; set;
    }
}
