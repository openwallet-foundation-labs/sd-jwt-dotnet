# SdJwt.Net.HAIP - High Assurance Interoperability Profile

[![NuGet Version](https://img.shields.io/nuget/v/SdJwt.Net.HAIP.svg)](https://www.nuget.org/packages/SdJwt.Net.HAIP)
[![License](https://img.shields.io/badge/license-Apache%202.0-blue.svg)](LICENSE)
[![Build Status](https://img.shields.io/github/workflow/status/thomas-tran/sd-jwt-dotnet/CI)](https://github.com/openwallet-foundation-labs/sd-jwt-dotnet/actions)

The High Assurance Interoperability Profile (HAIP) package provides policy-based compliance validation for OpenID4VC implementations requiring high security guarantees. HAIP defines three progressive compliance levels suitable for government, enterprise, and regulated industry use cases.

## Overview

HAIP addresses the need for standardized security requirements across verifiable credential ecosystems. Rather than each implementation defining its own security policies, HAIP provides:

-   **Standardized Security Levels**: Three progressive compliance tiers
-   **Policy-Driven Enforcement**: Automatic validation of cryptographic and protocol requirements
-   **Non-Intrusive Integration**: Works with existing SD-JWT implementations
-   **Comprehensive Audit Trails**: Detailed compliance reporting for regulatory requirements
-   **Trust Framework Integration**: Leverages trust chains for large-scale deployments

### HAIP Compliance Levels

| Level       | Name                | Use Cases                                     | Security Requirements                                 |
| ----------- | ------------------- | --------------------------------------------- | ----------------------------------------------------- |
| **Level 1** | High Assurance      | Education, standard business, consumer apps   | ES256+, PS256+, proof of possession, secure transport |
| **Level 2** | Very High Assurance | Banking, healthcare, government services      | ES384+, PS384+, wallet attestation, DPoP, PAR         |
| **Level 3** | Sovereign           | National ID, defense, critical infrastructure | ES512+, PS512+, HSM backing, qualified signatures     |

## Installation

```bash
dotnet add package SdJwt.Net.HAIP
```

For complete OpenID4VC functionality, also install:

```bash
dotnet add package SdJwt.Net                 # Core SD-JWT functionality
dotnet add package SdJwt.Net.Vc              # Verifiable Credentials
dotnet add package SdJwt.Net.Oid4Vci         # Credential Issuance
dotnet add package SdJwt.Net.Oid4Vp          # Credential Presentation
dotnet add package SdJwt.Net.OidFederation   # Trust Infrastructure
```

## Quick Start

### Basic HAIP Validation with SD-JWT

```csharp
using SdJwt.Net.HAIP;
using SdJwt.Net.Issuer;
using Microsoft.IdentityModel.Tokens;

// Create HAIP-compliant issuer with Level 1 security
var signingKey = new ECDsaSecurityKey(ECDsa.Create(ECCurve.NamedCurves.nistP256));
var issuer = new SdIssuer(signingKey, SecurityAlgorithms.EcdsaSha256);

// HAIP validation before issuance
var cryptoValidator = new HaipCryptoValidator(HaipLevel.Level1_High, logger);
var validationResult = cryptoValidator.ValidateAlgorithm("ES256");

if (validationResult.IsCompliant)
{
    // Configure HAIP-compliant issuance options
    var haipOptions = new SdIssuanceOptions
    {
        DecoyDigests = 2              // Privacy enhancement
    };

    var credential = issuer.Issue(claims, haipOptions);
    Console.WriteLine($"HAIP Level 1 compliant credential issued");
}
else
{
    Console.WriteLine($"HAIP violation: {validationResult.Violations.First().Description}");
}
```

### Level 2 Financial Services Configuration

```csharp
// Enhanced security for financial services (Level 2)
var bankingKey = new ECDsaSecurityKey(ECDsa.Create(ECCurve.NamedCurves.nistP384));
var bankingIssuer = new SdIssuer(bankingKey, SecurityAlgorithms.EcdsaSha384);

var level2Validator = new HaipCryptoValidator(HaipLevel.Level2_VeryHigh, logger);
var level2Result = level2Validator.ValidateKeyCompliance(bankingKey, "ES384");

if (level2Result.IsCompliant)
{
    var financialOptions = new SdIssuanceOptions
    {
        DecoyDigests = 5,  // Enhanced privacy for financial data
        // Additional Level 2 validation would be applied here
    };

    var bankingCredential = bankingIssuer.Issue(claims, financialOptions);
    Console.WriteLine($"HAIP Level 2 compliant banking credential issued");
}
```

### Government Sovereign Configuration (Level 3)

```csharp
// Maximum security for government credentials (Level 3)
var sovereignKey = new ECDsaSecurityKey(ECDsa.Create(ECCurve.NamedCurves.nistP521));
var governmentIssuer = new SdIssuer(sovereignKey, SecurityAlgorithms.EcdsaSha512);

var level3Validator = new HaipCryptoValidator(HaipLevel.Level3_Sovereign, logger);
var level3Result = level3Validator.ValidateKeyCompliance(sovereignKey, "ES512");

if (level3Result.IsCompliant)
{
    var sovereignOptions = new SdIssuanceOptions
    {
        DecoyDigests = 10,  // Maximum privacy protection
        // HSM validation would be performed here in production
    };

    var nationalIdCredential = governmentIssuer.Issue(claims, sovereignOptions);
    Console.WriteLine($"HAIP Level 3 sovereign credential issued");
}
```

## Core Features

### 1. Cryptographic Policy Enforcement

HAIP validates and enforces cryptographic requirements:

```csharp
// Cryptographic validation example
var validator = new HaipCryptoValidator(HaipLevel.Level2_VeryHigh, logger);

var result = validator.ValidateKeyCompliance(signingKey, "ES384");

if (!result.IsCompliant)
{
    foreach (var violation in result.Violations)
    {
        Console.WriteLine($"Violation: {violation.Description}");
        Console.WriteLine($"Fix: {violation.RecommendedAction}");
    }
}

Console.WriteLine($"Achieved Level: {result.AchievedLevel}");
Console.WriteLine($"Algorithm Approved: {result.IsCompliant}");
```

### 2. Protocol Security Validation

HAIP enforces protocol-level security requirements:

```csharp
public class HaipCompliantCredentialController : ControllerBase
{
    private readonly ISdIssuer _issuer;
    private readonly IHaipProtocolValidator _protocolValidator;

    [HttpPost("credential")]
    public async Task<IActionResult> IssueCredential([FromBody] CredentialRequest request)
    {
        try
        {
            // HAIP protocol validation
            var protocolResult = await _protocolValidator.ValidateRequestAsync(request, HaipLevel.Level2_VeryHigh);

            if (!protocolResult.IsCompliant)
            {
                return BadRequest(new
                {
                    error = "haip_compliance_failure",
                    required_level = "Level2_VeryHigh",
                    violations = protocolResult.Violations.Select(v => new
                    {
                        type = v.Type,
                        description = v.Description,
                        recommendation = v.RecommendedAction
                    })
                });
            }

            // Issue credential with HAIP compliance
            var credential = _issuer.Issue(request.Claims, new SdIssuanceOptions
            {
                DecoyDigests = 5
            });

            return Ok(new { credential = credential.SdJwt, compliance_level = "Level2_VeryHigh" });
        }
        catch (HaipComplianceException ex)
        {
            return BadRequest(new
            {
                error = "compliance_failure",
                violations = ex.ComplianceResult.Violations
            });
        }
    }
}
```

### 3. Comprehensive Audit Trails

HAIP provides detailed compliance reporting:

```csharp
// Generate compliance audit report
public class HaipAuditService
{
    public async Task<HaipComplianceResult> GenerateComplianceReportAsync(
        string operationId,
        HaipLevel requiredLevel)
    {
        var auditTrail = new HaipAuditTrail
        {
            OperationId = operationId,
            RequiredLevel = requiredLevel,
            StartTime = DateTimeOffset.UtcNow
        };

        // Perform compliance validation
        var cryptoValidator = new HaipCryptoValidator(requiredLevel, _logger);
        var protocolValidator = new HaipProtocolValidator(requiredLevel, _logger);

        var result = new HaipComplianceResult
        {
            IsCompliant = true,
            AchievedLevel = requiredLevel,
            AuditTrail = auditTrail,
            Violations = new List<HaipViolation>()
        };

        // Add validation steps to audit trail
        auditTrail.Steps.Add(new HaipValidationStep
        {
            Operation = "Cryptographic validation",
            Success = result.IsCompliant,
            Timestamp = DateTimeOffset.UtcNow
        });

        return result;
    }
}
```

## Integration with SD-JWT Ecosystem

### Complete Workflow Example

```csharp
using SdJwt.Net.Issuer;
using SdJwt.Net.Holder;
using SdJwt.Net.Verifier;
using SdJwt.Net.HAIP;

public class HaipCompliantWorkflow
{
    public async Task DemonstrateCompleteWorkflowAsync()
    {
        // Step 1: HAIP-compliant credential issuance
        var issuerKey = new ECDsaSecurityKey(ECDsa.Create(ECCurve.NamedCurves.nistP384));
        var issuer = new SdIssuer(issuerKey, SecurityAlgorithms.EcdsaSha384);

        // Validate HAIP compliance
        var cryptoValidator = new HaipCryptoValidator(HaipLevel.Level2_VeryHigh, _logger);
        var validationResult = cryptoValidator.ValidateKeyCompliance(issuerKey, "ES384");

        if (!validationResult.IsCompliant)
        {
            throw new HaipComplianceException("Key does not meet Level 2 requirements");
        }

        var claims = new JwtPayload
        {
            { "iss", "https://bank.example.com" },
            { "sub", "customer:12345" },
            { "customer_verification", "enhanced_due_diligence" }
        };

        var haipOptions = new SdIssuanceOptions
        {
            DisclosureStructure = new { customer_address = true, income_level = true },
            DecoyDigests = 5
        };

        var issuanceResult = issuer.Issue(claims, haipOptions);

        // Step 2: Holder creates presentation
        var holderKey = new ECDsaSecurityKey(ECDsa.Create(ECCurve.NamedCurves.nistP384));
        var holder = new SdJwtHolder(issuanceResult.Issuance);

        var presentation = holder.CreatePresentation(
            disclosure => disclosure.ClaimName == "customer_address",
            kbJwtPayload: new JwtPayload { { "aud", "verifier.example.com" } },
            kbJwtSigningKey: holderKey,
            kbJwtSigningAlgorithm: SecurityAlgorithms.EcdsaSha384
        );

        // Step 3: HAIP-compliant verification
        var verifier = new SdVerifier(async (jwt) => issuerKey);

        var validationParams = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = false,
            IssuerSigningKey = issuerKey
        };

        var kbValidationParams = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = false,
            IssuerSigningKey = holderKey
        };

        var verificationResult = await verifier.VerifyAsync(
            presentation, validationParams, kbValidationParams);

        Console.WriteLine($"HAIP Level 2 workflow completed successfully");
        Console.WriteLine($"Key binding verified: {verificationResult.KeyBindingVerified}");
    }
}
```

## Real-World Examples

### University Degree Credential (Level 1)

```csharp
public class UniversityCredentialIssuer
{
    private readonly SdIssuer _issuer;
    private readonly HaipCryptoValidator _validator;

    public UniversityCredentialIssuer()
    {
        var signingKey = new ECDsaSecurityKey(ECDsa.Create(ECCurve.NamedCurves.nistP256));
        _issuer = new SdIssuer(signingKey, SecurityAlgorithms.EcdsaSha256);
        _validator = new HaipCryptoValidator(HaipLevel.Level1_High, logger);
    }

    public async Task<string> IssueDegreeCredentialAsync(DegreeInfo degree)
    {
        // Validate HAIP Level 1 compliance
        var validationResult = _validator.ValidateAlgorithm("ES256");
        if (!validationResult.IsCompliant)
        {
            throw new InvalidOperationException("Configuration does not meet HAIP Level 1 requirements");
        }

        var claims = new JwtPayload
        {
            { "iss", "https://university.example.edu" },
            { "sub", $"student:{degree.StudentId}" },
            { "vct", "https://university.example.edu/credentials/degree" },
            { "degree_type", degree.Type },
            { "graduation_date", degree.GraduationDate },
            { "institution", "Example University" }
        };

        var options = new SdIssuanceOptions
        {
            DisclosureStructure = new
            {
                gpa = true,           // Student can choose to reveal
                honors = true,
                course_grades = true
            },
            DecoyDigests = 2
        };

        var credential = _issuer.Issue(claims, options);
        return credential.SdJwt;
    }
}
```

### Banking KYC Credential (Level 2)

```csharp
public class BankKycIssuer
{
    private readonly SdIssuer _issuer;
    private readonly HaipCryptoValidator _cryptoValidator;
    private readonly HaipProtocolValidator _protocolValidator;

    public BankKycIssuer()
    {
        var bankingKey = new ECDsaSecurityKey(ECDsa.Create(ECCurve.NamedCurves.nistP384));
        _issuer = new SdIssuer(bankingKey, SecurityAlgorithms.EcdsaSha384);
        _cryptoValidator = new HaipCryptoValidator(HaipLevel.Level2_VeryHigh, logger);
        _protocolValidator = new HaipProtocolValidator(HaipLevel.Level2_VeryHigh, logger);
    }

    public async Task<BankingCredentialResult> IssueKycCredentialAsync(KycData kyc)
    {
        // Validate HAIP Level 2 compliance
        var cryptoResult = _cryptoValidator.ValidateAlgorithm("ES384");
        if (!cryptoResult.IsCompliant)
        {
            throw new HaipComplianceException("Cryptographic configuration insufficient for Level 2");
        }

        var claims = new JwtPayload
        {
            { "iss", "https://securebank.example" },
            { "sub", $"customer:{kyc.CustomerId}" },
            { "vct", "https://securebank.example/credentials/kyc" },
            { "verification_level", "enhanced_due_diligence" },
            { "kyc_completion_date", kyc.CompletionDate },
            { "aml_status", "cleared" },
            { "risk_rating", kyc.RiskRating }
        };

        var options = new SdIssuanceOptions
        {
            DisclosureStructure = new
            {
                personal_info = true,        // Name, DOB selectively disclosable
                address = true,
                income_bracket = true,
                transaction_patterns = true
            },
            DecoyDigests = 5                 // Enhanced privacy for financial data
        };

        var credential = _issuer.Issue(claims, options);

        return new BankingCredentialResult
        {
            Credential = credential.SdJwt,
            ComplianceLevel = HaipLevel.Level2_VeryHigh,
            SelectiveDisclosureClaims = new[] { "personal_info", "address", "income_bracket" }
        };
    }
}
```

### Government Identity Credential (Level 3)

```csharp
public class GovernmentIdentityIssuer
{
    private readonly SdIssuer _issuer;
    private readonly HaipCryptoValidator _validator;

    public GovernmentIdentityIssuer()
    {
        // Level 3 requires P-521 curve and ES512 algorithm
        var sovereignKey = new ECDsaSecurityKey(ECDsa.Create(ECCurve.NamedCurves.nistP521));
        _issuer = new SdIssuer(sovereignKey, SecurityAlgorithms.EcdsaSha512);
        _validator = new HaipCryptoValidator(HaipLevel.Level3_Sovereign, logger);
    }

    public async Task<NationalIdResult> IssueNationalIdAsync(CitizenData citizen)
    {
        // Validate HAIP Level 3 (Sovereign) compliance
        var validationResult = _validator.ValidateAlgorithm("ES512");
        if (!validationResult.IsCompliant)
        {
            throw new HaipComplianceException("Configuration does not meet Sovereign level requirements");
        }

        // In production, additional HSM validation would be required
        var claims = new JwtPayload
        {
            { "iss", "https://identity.gov.example" },
            { "sub", $"urn:gov:citizen:{citizen.CitizenId}" },
            { "vct", "https://identity.gov.example/credentials/national-id" },
            { "citizen_id", citizen.CitizenId },
            { "nationality", citizen.Nationality },
            { "document_type", "national_identity_card" },
            { "issuing_authority", "Ministry of Interior" }
        };

        var options = new SdIssuanceOptions
        {
            DisclosureStructure = new
            {
                personal_info = true,        // Name, DOB selectively disclosable
                address = true,
                place_of_birth = true,
                biometric_template = true    // Privacy-preserving biometric reference
            },
            DecoyDigests = 10                // Maximum privacy for citizens
        };

        var credential = _issuer.Issue(claims, options);

        return new NationalIdResult
        {
            Credential = credential.SdJwt,
            ComplianceLevel = HaipLevel.Level3_Sovereign,
            QualifiedSignature = true,  // Government credentials require QES
            ValidityPeriod = TimeSpan.FromYears(10)
        };
    }
}
```

## Algorithm Restrictions

HAIP enforces strict algorithm policies:

```csharp
// HAIP algorithm validation
public static class HaipAlgorithmPolicy
{
    public static readonly string[] Level1_Algorithms = { "ES256", "ES384", "PS256", "PS384", "EdDSA" };
    public static readonly string[] Level2_Algorithms = { "ES384", "ES512", "PS384", "PS512", "EdDSA" };
    public static readonly string[] Level3_Algorithms = { "ES512", "PS512", "EdDSA" };

    // These algorithms are FORBIDDEN at ALL HAIP levels
    public static readonly string[] ForbiddenAlgorithms = { "RS256", "HS256", "HS384", "HS512", "none" };
}

// Usage example
var validator = new HaipCryptoValidator(HaipLevel.Level2_VeryHigh, logger);
var result = validator.ValidateAlgorithm("RS256");  // Returns non-compliant

Console.WriteLine($"Algorithm: RS256");
Console.WriteLine($"Compliant: {result.IsCompliant}");  // False
Console.WriteLine($"Reason: {result.Violations.First().Description}");  // "Algorithm RS256 is forbidden"
```

## Multi-Tenant Deployment

HAIP supports multi-tenant architectures with tenant-specific compliance:

```csharp
// Tenant-specific HAIP factory
public class TenantHaipFactory
{
    public SdIssuer CreateIssuerForTenant(string tenantId)
    {
        var config = GetTenantConfiguration(tenantId);
        var key = CreateKeyForLevel(config.RequiredLevel);
        var algorithm = GetAlgorithmForLevel(config.RequiredLevel);

        return new SdIssuer(key, algorithm);
    }

    public SdIssuanceOptions CreateOptionsForTenant(string tenantId)
    {
        var config = GetTenantConfiguration(tenantId);

        return new SdIssuanceOptions
        {
            DecoyDigests = config.RequiredLevel switch
            {
                HaipLevel.Level1_High => 2,
                HaipLevel.Level2_VeryHigh => 5,
                HaipLevel.Level3_Sovereign => 10,
                _ => 1
            }
        };
    }

    private SecurityKey CreateKeyForLevel(HaipLevel level)
    {
        return level switch
        {
            HaipLevel.Level1_High => new ECDsaSecurityKey(ECDsa.Create(ECCurve.NamedCurves.nistP256)),
            HaipLevel.Level2_VeryHigh => new ECDsaSecurityKey(ECDsa.Create(ECCurve.NamedCurves.nistP384)),
            HaipLevel.Level3_Sovereign => new ECDsaSecurityKey(ECDsa.Create(ECCurve.NamedCurves.nistP521)),
            _ => throw new ArgumentException("Invalid HAIP level")
        };
    }

    private string GetAlgorithmForLevel(HaipLevel level)
    {
        return level switch
        {
            HaipLevel.Level1_High => SecurityAlgorithms.EcdsaSha256,
            HaipLevel.Level2_VeryHigh => SecurityAlgorithms.EcdsaSha384,
            HaipLevel.Level3_Sovereign => SecurityAlgorithms.EcdsaSha512,
            _ => throw new ArgumentException("Invalid HAIP level")
        };
    }
}
```

## Testing

### Unit Test Examples

```csharp
[TestClass]
public class HaipComplianceTests
{
    [TestMethod]
    public void Level1_ES256_ShouldPass()
    {
        // Arrange
        var validator = new HaipCryptoValidator(HaipLevel.Level1_High, _logger);

        // Act
        var result = validator.ValidateAlgorithm("ES256");

        // Assert
        Assert.IsTrue(result.IsCompliant);
        Assert.AreEqual(HaipLevel.Level1_High, result.AchievedLevel);
    }

    [TestMethod]
    public void Level2_ES256_ShouldFail()
    {
        // Arrange
        var validator = new HaipCryptoValidator(HaipLevel.Level2_VeryHigh, _logger);

        // Act
        var result = validator.ValidateAlgorithm("ES256");

        // Assert
        Assert.IsFalse(result.IsCompliant);
        Assert.IsTrue(result.Violations.Any(v => v.Type == HaipViolationType.WeakCryptography));
    }

    [TestMethod]
    public void ForbiddenAlgorithm_RS256_ShouldAlwaysFail()
    {
        // Arrange
        var validator = new HaipCryptoValidator(HaipLevel.Level1_High, _logger);

        // Act
        var result = validator.ValidateAlgorithm("RS256");

        // Assert
        Assert.IsFalse(result.IsCompliant);
        Assert.IsTrue(result.Violations.Any(v => v.Description.Contains("RS256 is forbidden")));
    }
}
```
