namespace SdJwt.Net.HAIP.Models;

/// <summary>
/// Result of banking credential issuance with HAIP compliance
/// </summary>
public class BankingCredentialResult
{
    /// <summary>
    /// The issued SD-JWT credential
    /// </summary>
    public string Credential { get; set; } = string.Empty;
    
    /// <summary>
    /// HAIP compliance level achieved
    /// </summary>
    public HaipLevel ComplianceLevel { get; set; }
    
    /// <summary>
    /// Claims that are selectively disclosable
    /// </summary>
    public string[] SelectiveDisclosureClaims { get; set; } = Array.Empty<string>();
    
    /// <summary>
    /// Whether revocation is supported
    /// </summary>
    public bool RevocationSupported { get; set; }
    
    /// <summary>
    /// Status list URL for revocation checking
    /// </summary>
    public string? StatusListUrl { get; set; }
}

/// <summary>
/// Result of national ID credential issuance
/// </summary>
public class NationalIdResult
{
    /// <summary>
    /// The issued SD-JWT credential
    /// </summary>
    public string Credential { get; set; } = string.Empty;
    
    /// <summary>
    /// HAIP compliance level achieved
    /// </summary>
    public HaipLevel ComplianceLevel { get; set; }
    
    /// <summary>
    /// Whether the credential includes a qualified electronic signature
    /// </summary>
    public bool QualifiedSignature { get; set; }
    
    /// <summary>
    /// Validity period of the credential
    /// </summary>
    public TimeSpan ValidityPeriod { get; set; }
    
    /// <summary>
    /// Claims that are selectively disclosable
    /// </summary>
    public string[] SelectiveDisclosureClaims { get; set; } = Array.Empty<string>();
}

/// <summary>
/// Input data for degree credential issuance
/// </summary>
public class DegreeInfo
{
    /// <summary>
    /// Student identifier
    /// </summary>
    public string StudentId { get; set; } = string.Empty;
    
    /// <summary>
    /// Type of degree (e.g., "Bachelor of Science")
    /// </summary>
    public string Type { get; set; } = string.Empty;
    
    /// <summary>
    /// Degree name/title
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// School or department
    /// </summary>
    public string School { get; set; } = string.Empty;
    
    /// <summary>
    /// Student's GPA
    /// </summary>
    public double GPA { get; set; }
    
    /// <summary>
    /// Graduation date
    /// </summary>
    public DateTime GraduationDate { get; set; }
    
    /// <summary>
    /// Honors received (if any)
    /// </summary>
    public string? Honors { get; set; }
}

/// <summary>
/// Input data for KYC credential issuance
/// </summary>
public class KycData
{
    /// <summary>
    /// Customer identifier
    /// </summary>
    public string CustomerId { get; set; } = string.Empty;
    
    /// <summary>
    /// Customer's full name
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Date of birth
    /// </summary>
    public DateTime DateOfBirth { get; set; }
    
    /// <summary>
    /// Customer's nationality
    /// </summary>
    public string Nationality { get; set; } = string.Empty;
    
    /// <summary>
    /// Customer's address
    /// </summary>
    public string Address { get; set; } = string.Empty;
    
    /// <summary>
    /// KYC completion date
    /// </summary>
    public DateTime CompletionDate { get; set; }
    
    /// <summary>
    /// Risk rating assigned
    /// </summary>
    public string RiskRating { get; set; } = string.Empty;
    
    /// <summary>
    /// Verifying officer identifier
    /// </summary>
    public string VerifyingOfficer { get; set; } = string.Empty;
    
    /// <summary>
    /// Verification date
    /// </summary>
    public DateTime VerificationDate { get; set; }
}

/// <summary>
/// Input data for citizen identity credential issuance
/// </summary>
public class CitizenData
{
    /// <summary>
    /// Citizen identifier
    /// </summary>
    public string CitizenId { get; set; } = string.Empty;
    
    /// <summary>
    /// Citizen's full name
    /// </summary>
    public string FullName { get; set; } = string.Empty;
    
    /// <summary>
    /// Date of birth
    /// </summary>
    public DateTime DateOfBirth { get; set; }
    
    /// <summary>
    /// Place of birth
    /// </summary>
    public string PlaceOfBirth { get; set; } = string.Empty;
    
    /// <summary>
    /// Nationality
    /// </summary>
    public string Nationality { get; set; } = string.Empty;
    
    /// <summary>
    /// Current address
    /// </summary>
    public string Address { get; set; } = string.Empty;
    
    /// <summary>
    /// Biometric hash for privacy-preserving verification
    /// </summary>
    public string BiometricHash { get; set; } = string.Empty;
}

/// <summary>
/// Generic credential request for HAIP validation
/// </summary>
public class CredentialRequest
{
    /// <summary>
    /// Credential type identifier
    /// </summary>
    public string Type { get; set; } = string.Empty;
    
    /// <summary>
    /// Subject identifier
    /// </summary>
    public string Subject { get; set; } = string.Empty;
    
    /// <summary>
    /// Credential claims
    /// </summary>
    public Dictionary<string, object> Claims { get; set; } = new();
    
    /// <summary>
    /// Claims that should be selectively disclosable
    /// </summary>
    public Dictionary<string, object> SelectiveDisclosureClaims { get; set; } = new();
}

/// <summary>
/// Generic credential response with HAIP compliance information
/// </summary>
public class CredentialResponse
{
    /// <summary>
    /// The issued SD-JWT credential
    /// </summary>
    public string Credential { get; set; } = string.Empty;
    
    /// <summary>
    /// HAIP compliance level achieved
    /// </summary>
    public string ComplianceLevel { get; set; } = string.Empty;
    
    /// <summary>
    /// Claims that are selectively disclosable
    /// </summary>
    public string[] SelectiveDisclosureClaims { get; set; } = Array.Empty<string>();
}