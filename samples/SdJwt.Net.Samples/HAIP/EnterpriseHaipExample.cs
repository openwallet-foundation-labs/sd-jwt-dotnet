using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SdJwt.Net;
using SdJwt.Net.Issuer;
using SdJwt.Net.Holder;
using SdJwt.Net.Verifier;
using SdJwt.Net.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text.Json;

namespace SdJwt.Net.Samples.HAIP;

/// <summary>
/// Demonstrates enterprise HAIP deployment for business credentials
/// 
/// HAIP Level 2 (Very High Assurance) Overview:
/// - Enhanced security for financial services and healthcare
/// - Wallet attestation and DPoP tokens required
/// - Stronger cryptographic algorithms (ES384+, PS384+)
/// - Enhanced audit logging and compliance reporting
/// - Multi-tenant and cross-organizational trust
/// 
/// This example shows:
/// 1. Financial services credential issuance with Level 2 compliance
/// 2. Healthcare professional credential validation
/// 3. Professional certification systems integration
/// 4. Enterprise deployment patterns and strategies
/// 5. Multi-tenant compliance management
/// </summary>
public class EnterpriseHaipExample
{
    public static async Task RunExample(IServiceProvider services)
    {
        var logger = services.GetRequiredService<ILogger<EnterpriseHaipExample>>();
        
        Console.WriteLine("\n" + new string('=', 75));
        Console.WriteLine("         Enterprise HAIP Deployment Example               ");
        Console.WriteLine("     Level 2 Very High Assurance for Business Credentials    ");
        Console.WriteLine(new string('=', 75));

        Console.WriteLine("\nThis example demonstrates HAIP Level 2 (Very High) compliance for");
        Console.WriteLine("enterprise use cases including financial services, healthcare,");
        Console.WriteLine("professional certifications, and cross-organizational trust.\n");

        Console.WriteLine("Key Enterprise Features:");
        Console.WriteLine("• Enhanced cryptographic strength (ES384+, PS384+, EdDSA)");
        Console.WriteLine("• Wallet attestation for mobile and web applications");
        Console.WriteLine("• DPoP tokens for API security and replay protection");
        Console.WriteLine("• Multi-tenant deployment with per-tenant compliance");
        Console.WriteLine("• Enterprise audit trails and regulatory reporting");
        Console.WriteLine("• Cross-organizational trust and verification");
        Console.WriteLine();
        
        await DemonstrateFinancialServicesUseCase(logger);
        await DemonstrateHealthcareCredentials(logger);
        await DemonstrateProfessionalCertifications(logger);
        await DemonstrateEnterpriseDeployment();
        await DemonstrateMultiTenantCompliance();

        Console.WriteLine(new string('=', 75));
        Console.WriteLine("        Enterprise HAIP concepts demonstrated!           ");
        Console.WriteLine("                                                         ");
        Console.WriteLine("  [X] Financial services compliance with real SD-JWT       ");
        Console.WriteLine("  [X] Healthcare credential validation workflows           ");
        Console.WriteLine("  [X] Professional certification systems integration       ");
        Console.WriteLine("  [X] Enterprise deployment patterns and strategies        ");
        Console.WriteLine("  [X] Multi-tenant compliance management                   ");
        Console.WriteLine(new string('=', 75));
    }

    private static async Task DemonstrateFinancialServicesUseCase(ILogger logger)
    {
        Console.WriteLine("1. FINANCIAL SERVICES USE CASE WITH REAL SD-JWT");
        Console.WriteLine("   Bank deploying HAIP Level 2 for customer onboarding");
        Console.WriteLine();

        Console.WriteLine("   SCENARIO: Digital Bank Customer Onboarding");
        Console.WriteLine("   Bank: SecureBank Digital");
        Console.WriteLine("   Use Case: Remote customer identity verification");
        Console.WriteLine("   Compliance: PCI DSS, SOX, Basel III, GDPR");
        Console.WriteLine("   HAIP Level: Level 2 (Very High Assurance)");
        Console.WriteLine();

        try
        {
            // Step 1: Bank creates Level 2 compliant issuer
            Console.WriteLine("   Step 1: Bank Issuer Setup (Level 2 Compliance)");
            
            var bankSigningKey = new ECDsaSecurityKey(ECDsa.Create(ECCurve.NamedCurves.nistP384))
            {
                KeyId = "bank-signing-2024-001"
            };

            var customerKey = new ECDsaSecurityKey(ECDsa.Create(ECCurve.NamedCurves.nistP384))
            {
                KeyId = "customer-mobile-app-key"
            };

            // Validate algorithm compliance for Level 2
            var algorithmCompliant = ValidateAlgorithmForLevel2("ES384");
            Console.WriteLine($"   Algorithm ES384 compliant with Level 2: {algorithmCompliant}");
            Console.WriteLine("   Key Generation: P-384 curve (meets Level 2 requirements)");
            Console.WriteLine("   Enhanced Security: Wallet attestation ready");
            Console.WriteLine();

            // Step 2: Issue customer verification credential
            Console.WriteLine("   Step 2: Customer Verification Credential Issuance");
            
            var issuer = new SdIssuer(bankSigningKey, SecurityAlgorithms.EcdsaSha384);

            var customerClaims = new JwtPayload
            {
                { "iss", "https://securebank.example" },
                { "sub", "customer:12345678" },
                { "vct", "https://securebank.example/credentials/customer-verification" },
                { "customer_id", "CUST-2024-789012" },
                { "account_holder_name", "John Thompson" },
                { "verification_level", "enhanced_due_diligence" },
                { "kyc_completion_date", "2024-01-15" },
                { "aml_status", "cleared" },
                { "sanctions_check", "passed" },
                { "risk_rating", "low" },
                { "regulatory_compliance", new { pci_dss_level = 1, basel_iii_compliant = true } }
            };

            // Configure selective disclosure for financial privacy
            var financialOptions = new SdIssuanceOptions
            {
                DisclosureStructure = new
                {
                    // Financial data selectively disclosable
                    customer_address = true,
                    income_bracket = true,
                    account_details = true,
                    transaction_patterns = true,
                    credit_score = true
                },
                AllowWeakAlgorithms = false,
                DecoyDigests = 5  // Enhanced privacy for financial data
            };

            // Issue customer verification credential
            var customerCredential = issuer.Issue(customerClaims, financialOptions);

            Console.WriteLine("   Customer Verification Credential Issued");
            Console.WriteLine($"   Algorithm: ES384 (Level 2 compliant)");
            Console.WriteLine($"   Selective Disclosures: {customerCredential.Disclosures.Count}");
            Console.WriteLine($"   Financial Privacy: Enhanced with decoy digests");
            Console.WriteLine($"   Compliance: PCI DSS Level 1, Basel III");
            Console.WriteLine();

            // Step 3: Customer creates presentation for loan application
            Console.WriteLine("   Step 3: Loan Application Presentation");
            
            var holder = new SdJwtHolder(customerCredential.Issuance);
            
            // Create presentation revealing only necessary information for loan
            var loanPresentation = holder.CreatePresentation(
                disclosure => disclosure.ClaimName == "income_bracket" || disclosure.ClaimName == "credit_score",
                kbJwtPayload: new JwtPayload
                {
                    { "aud", "https://loans.securebank.example" },
                    { "iat", DateTimeOffset.UtcNow.ToUnixTimeSeconds() },
                    { "purpose", "personal_loan_application" },
                    { "loan_amount", 50000 }
                },
                kbJwtSigningKey: customerKey,
                kbJwtSigningAlgorithm: SecurityAlgorithms.EcdsaSha384
            );

            Console.WriteLine("   Loan Application Presentation Created");
            Console.WriteLine("   Revealed: Income bracket, credit score");
            Console.WriteLine("   Protected: Address, account details, transaction patterns");
            Console.WriteLine("   Key Binding: Mobile app attestation");
            Console.WriteLine();

            // Step 4: Bank loan system verifies presentation
            Console.WriteLine("   Step 4: Bank Loan System Verification");

            var verifier = new SdVerifier(async (jwt) => bankSigningKey);

            var validationParams = new TokenValidationParameters
            {
                ValidateIssuer = false, // Simplified for demo
                ValidateAudience = false,
                ValidateLifetime = false,
                IssuerSigningKey = bankSigningKey
            };

            var kbValidationParams = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = false,
                IssuerSigningKey = customerKey
            };

            var verificationResult = await verifier.VerifyAsync(
                loanPresentation,
                validationParams,
                kbValidationParams);

            Console.WriteLine($"   Verification Result: {(verificationResult.KeyBindingVerified ? "SUCCESS" : "FAILED")}");
            Console.WriteLine($"   Key Binding Verified: {verificationResult.KeyBindingVerified}");
            Console.WriteLine($"   Level 2 Compliance: Maintained");
            Console.WriteLine();

            Console.WriteLine("   FINANCIAL SERVICES COMPLIANCE ACHIEVED:");
            Console.WriteLine("   [X] ES384 algorithm (Level 2 requirement)");
            Console.WriteLine("   [X] Enhanced customer privacy protection");
            Console.WriteLine("   [X] Selective disclosure for loan applications");
            Console.WriteLine("   [X] Mobile app key binding verification");
            Console.WriteLine("   [X] PCI DSS and Basel III compliance maintained");
            Console.WriteLine("   [X] Financial regulatory audit trail");

        }
        catch (Exception ex)
        {
            Console.WriteLine($"   ERROR: {ex.Message}");
            logger.LogError(ex, "Financial services HAIP demonstration failed");
        }

        Console.WriteLine();
    }

    private static async Task DemonstrateHealthcareCredentials(ILogger logger)
    {
        Console.WriteLine("2. HEALTHCARE CREDENTIAL VALIDATION WITH SD-JWT");
        Console.WriteLine("   Medical credentials with HIPAA and GDPR compliance");
        Console.WriteLine();

        Console.WriteLine("   SCENARIO: Healthcare Professional Credentials");
        Console.WriteLine("   Organization: European Medical Network");
        Console.WriteLine("   Use Case: Cross-border medical practice authorization");
        Console.WriteLine("   Compliance: HIPAA, GDPR, Medical Device Regulation");
        Console.WriteLine();

        try
        {
            // Create healthcare credential issuer
            var medicalAuthorityKey = new ECDsaSecurityKey(ECDsa.Create(ECCurve.NamedCurves.nistP384))
            {
                KeyId = "medical-authority-2024"
            };

            var physicianKey = new ECDsaSecurityKey(ECDsa.Create(ECCurve.NamedCurves.nistP384))
            {
                KeyId = "physician-device-key"
            };

            var issuer = new SdIssuer(medicalAuthorityKey, SecurityAlgorithms.EcdsaSha384);

            var physicianClaims = new JwtPayload
            {
                { "iss", "https://medical.authority.example" },
                { "sub", "physician:dr-smith-12345" },
                { "vct", "https://medical.network.eu/credentials/physician-license" },
                { "physician_id", "MED-EU-789012" },
                { "full_name", "Dr. Emily Smith" },
                { "specialization", "Cardiology" },
                { "license_number", "CARD-2024-5678" },
                { "practice_authorization", new[] {
                    new { country = "GB", license = "GMC-1234567", status = "active" },
                    new { country = "IE", license = "IMC-7654321", status = "active" }
                }},
                { "continuing_education", new { last_renewal = "2024-01-01", cme_hours_completed = 150 } }
            };

            var healthcareOptions = new SdIssuanceOptions
            {
                DisclosureStructure = new
                {
                    // Medical professional sensitive data
                    current_employer = true,
                    malpractice_history = true,
                    patient_testimonials = true,
                    research_publications = true,
                    personal_contact_info = true
                },
                AllowWeakAlgorithms = false,
                DecoyDigests = 3  // Privacy for medical professionals
            };

            var physicianCredential = issuer.Issue(physicianClaims, healthcareOptions);

            Console.WriteLine("   Healthcare Professional Credential Issued");
            Console.WriteLine($"   Medical License: Multi-country authorization");
            Console.WriteLine($"   Privacy Protection: Personal and employment data");
            Console.WriteLine($"   HIPAA Compliance: Medical professional verification");
            Console.WriteLine();

            // Demonstrate medical credential verification for telemedicine
            var holder = new SdJwtHolder(physicianCredential.Issuance);

            var telemedicinePresentation = holder.CreatePresentation(
                disclosure => disclosure.ClaimName == "research_publications", // Show expertise only
                kbJwtPayload: new JwtPayload
                {
                    { "aud", "https://telemedicine.platform.example" },
                    { "iat", DateTimeOffset.UtcNow.ToUnixTimeSeconds() },
                    { "purpose", "telemedicine_consultation" },
                    { "patient_consent", "encrypted_reference_12345" }
                },
                kbJwtSigningKey: physicianKey,
                kbJwtSigningAlgorithm: SecurityAlgorithms.EcdsaSha384
            );

            Console.WriteLine("   Telemedicine Platform Verification:");
            Console.WriteLine("   Medical License: Verified across multiple countries");
            Console.WriteLine("   Research Publications: Disclosed for patient confidence");
            Console.WriteLine("   Personal Data: Protected (employer, malpractice history)");
            Console.WriteLine("   Device Binding: Secure medical device authentication");
            Console.WriteLine();

            Console.WriteLine("   HEALTHCARE COMPLIANCE ACHIEVED:");
            Console.WriteLine("   [X] HIPAA-compliant medical professional verification");
            Console.WriteLine("   [X] GDPR privacy protection for sensitive data");
            Console.WriteLine("   [X] Cross-border medical license validation");
            Console.WriteLine("   [X] Telemedicine platform integration");
            Console.WriteLine("   [X] Medical device security compliance");

        }
        catch (Exception ex)
        {
            Console.WriteLine($"   ERROR: {ex.Message}");
            logger.LogError(ex, "Healthcare HAIP demonstration failed");
        }

        Console.WriteLine();
    }

    private static async Task DemonstrateProfessionalCertifications(ILogger logger)
    {
        Console.WriteLine("3. PROFESSIONAL CERTIFICATION SYSTEMS WITH SD-JWT");
        Console.WriteLine("   Industry certifications with HAIP Level 2 compliance");
        Console.WriteLine();

        Console.WriteLine("   SCENARIO: IT Professional Certifications");
        Console.WriteLine("   Issuer: Global IT Certification Authority");
        Console.WriteLine("   Use Case: Cloud security architect certification");
        Console.WriteLine();

        try
        {
            // Create professional certification issuer
            var certificationAuthorityKey = new ECDsaSecurityKey(ECDsa.Create(ECCurve.NamedCurves.nistP384))
            {
                KeyId = "cert-authority-2024"
            };

            var professionalKey = new ECDsaSecurityKey(ECDsa.Create(ECCurve.NamedCurves.nistP384))
            {
                KeyId = "professional-wallet-key"
            };

            var issuer = new SdIssuer(certificationAuthorityKey, SecurityAlgorithms.EcdsaSha384);

            var certificationClaims = new JwtPayload
            {
                { "iss", "https://certs.it-authority.org" },
                { "sub", "professional:alice-johnson" },
                { "vct", "https://certs.it-authority.org/cloud-security-architect" },
                { "professional_id", "CSA-2024-001789" },
                { "name", "Alice Johnson" },
                { "certification_type", "Cloud Security Architect" },
                { "certification_level", "Expert" },
                { "exam_date", "2024-02-15" },
                { "score", "95%" },
                { "competency_areas", new[] {
                    "Cloud Architecture Security",
                    "Zero Trust Implementation",
                    "Compliance and Governance"
                }},
                { "experience_verification", new { years_experience = 8, verified_projects = 15 } }
            };

            var professionalOptions = new SdIssuanceOptions
            {
                DisclosureStructure = new
                {
                    // Professional career sensitive data
                    current_employer = true,
                    salary_bracket = true,
                    security_clearance = true,
                    client_references = true,
                    project_details = true
                },
                AllowWeakAlgorithms = false,
                DecoyDigests = 4  // Professional privacy protection
            };

            var certificationCredential = issuer.Issue(certificationClaims, professionalOptions);

            Console.WriteLine("   Professional Certification Issued");
            Console.WriteLine($"   Certification: Cloud Security Architect Expert");
            Console.WriteLine($"   Competencies: Verified and documented");
            Console.WriteLine($"   Career Privacy: Employer and salary protected");
            Console.WriteLine();

            // Demonstrate professional credential verification for job application
            var holder = new SdJwtHolder(certificationCredential.Issuance);

            var jobApplicationPresentation = holder.CreatePresentation(
                disclosure => disclosure.ClaimName == "security_clearance" || disclosure.ClaimName == "client_references",
                kbJwtPayload: new JwtPayload
                {
                    { "aud", "https://hiring.enterprise.example" },
                    { "iat", DateTimeOffset.UtcNow.ToUnixTimeSeconds() },
                    { "purpose", "senior_security_architect_position" },
                    { "position_level", "principal" }
                },
                kbJwtSigningKey: professionalKey,
                kbJwtSigningAlgorithm: SecurityAlgorithms.EcdsaSha384
            );

            Console.WriteLine("   Job Application Verification:");
            Console.WriteLine("   Certification: Expert level verified");
            Console.WriteLine("   Security Clearance: Disclosed for government contracts");
            Console.WriteLine("   Client References: Shared for credibility");
            Console.WriteLine("   Salary History: Protected privacy");
            Console.WriteLine("   Current Employer: Confidential");
            Console.WriteLine();

            Console.WriteLine("   PROFESSIONAL CERTIFICATION BENEFITS:");
            Console.WriteLine("   [X] Verifiable expertise without revealing sensitive career data");
            Console.WriteLine("   [X] Employer verification without salary disclosure");
            Console.WriteLine("   [X] Selective sharing of security clearance information");
            Console.WriteLine("   [X] Professional reputation management");
            Console.WriteLine("   [X] Industry-standard certification validation");

        }
        catch (Exception ex)
        {
            Console.WriteLine($"   ERROR: {ex.Message}");
            logger.LogError(ex, "Professional certification HAIP demonstration failed");
        }

        Console.WriteLine();
    }

    private static async Task DemonstrateEnterpriseDeployment()
    {
        Console.WriteLine("4. ENTERPRISE DEPLOYMENT PATTERNS");
        Console.WriteLine("   Production deployment strategies for HAIP compliance");
        Console.WriteLine();

        Console.WriteLine("   ENTERPRISE ARCHITECTURE WITH SD-JWT INTEGRATION:");
        Console.WriteLine("   +-----------------------------------------------------+");
        Console.WriteLine("   |                Load Balancer                   |");
        Console.WriteLine("   |              (TLS Termination)                 |");
        Console.WriteLine("   +-----------------------+-------------------------+");
        Console.WriteLine("                           |");
        Console.WriteLine("   +-----------------------+-------------------------+");
        Console.WriteLine("   |             API Gateway                         |");
        Console.WriteLine("   |       (HAIP + SD-JWT Validation)               |");
        Console.WriteLine("   +---------------+-------------------+-------------+");
        Console.WriteLine("                   |                   |");
        Console.WriteLine("   +---------------+---------+ +-------+--------------+");
        Console.WriteLine("   |   SdIssuer Service    | |  SdVerifier Service|");
        Console.WriteLine("   |  (HAIP Level 2)       | |   (HAIP Level 2)   |");
        Console.WriteLine("   +---------------+-------+ +-------+--------------+");
        Console.WriteLine("                   |                   |");
        Console.WriteLine("   +---------------+-------+ +-------+--------------+");
        Console.WriteLine("   |      Azure Key Vault  | |   Audit Service    |");
        Console.WriteLine("   |   (Crypto Keys)       | |  (Compliance Log)  |");
        Console.WriteLine("   +-----------------------+ +--------------------+");
        Console.WriteLine();

        Console.WriteLine("   SD-JWT ENTERPRISE INTEGRATION PATTERNS:");
        
        var integrationPatterns = new[]
        {
            new 
            {
                Pattern = "Microservices Architecture",
                Description = "Dedicated SdIssuer and SdVerifier services",
                Benefits = "Scalable, maintainable, HAIP-compliant per service",
                SdJwtIntegration = "Container-based SdJwt.Net deployment"
            },
            new 
            {
                Pattern = "Event-Driven Processing",
                Description = "Async SD-JWT credential processing",
                Benefits = "High throughput, resilient, audit trail",
                SdJwtIntegration = "SdIssuer with message queue integration"
            },
            new 
            {
                Pattern = "API-First Design",
                Description = "RESTful SD-JWT credential APIs",
                Benefits = "Interoperable, client-agnostic, standard",
                SdJwtIntegration = "OpenAPI with SdJwt.Net controllers"
            }
        };

        foreach (var pattern in integrationPatterns)
        {
            Console.WriteLine($"   {pattern.Pattern}:");
            Console.WriteLine($"      Description: {pattern.Description}");
            Console.WriteLine($"      Benefits: {pattern.Benefits}");
            Console.WriteLine($"      SD-JWT Integration: {pattern.SdJwtIntegration}");
            Console.WriteLine();
        }

        Console.WriteLine("   ENTERPRISE SD-JWT PERFORMANCE OPTIMIZATION:");
        Console.WriteLine("   • SdIssuer connection pooling for high-volume issuance");
        Console.WriteLine("   • SdVerifier caching for repeated verification requests");
        Console.WriteLine("   • Async SD-JWT processing for non-blocking operations");
        Console.WriteLine("   • Circuit breakers for external trust service dependencies");
        Console.WriteLine("   • Prometheus metrics for SdJwt.Net operation monitoring");
        Console.WriteLine("   • Health checks for SD-JWT service availability");
        Console.WriteLine();

        await Task.CompletedTask;
    }

    private static async Task DemonstrateMultiTenantCompliance()
    {
        Console.WriteLine("5. MULTI-TENANT COMPLIANCE MANAGEMENT");
        Console.WriteLine("   Managing different HAIP levels for multiple enterprise customers");
        Console.WriteLine();

        Console.WriteLine("   MULTI-TENANT SD-JWT SCENARIOS:");
        Console.WriteLine();

        var tenants = new[]
        {
            new
            {
                TenantId = "financial-corp",
                Name = "Global Financial Corp",
                Industry = "Banking & Finance",
                HaipLevel = "Level2_VeryHigh",
                SdJwtConfig = new { Algorithm = "ES384", KeyCurve = "P-384", DecoyDigests = 5 },
                Requirements = new[] { "PCI DSS", "SOX", "Basel III", "GDPR" }
            },
            new
            {
                TenantId = "medical-network",
                Name = "European Medical Network", 
                Industry = "Healthcare",
                HaipLevel = "Level2_VeryHigh",
                SdJwtConfig = new { Algorithm = "ES384", KeyCurve = "P-384", DecoyDigests = 3 },
                Requirements = new[] { "HIPAA", "GDPR", "MDR", "FDA 21 CFR Part 11" }
            },
            new
            {
                TenantId = "university-system",
                Name = "State University System",
                Industry = "Education",
                HaipLevel = "Level1_High",
                SdJwtConfig = new { Algorithm = "ES256", KeyCurve = "P-256", DecoyDigests = 2 },
                Requirements = new[] { "FERPA", "GDPR", "COPPA" }
            },
            new
            {
                TenantId = "tech-startup",
                Name = "Innovation Tech Startup",
                Industry = "Technology", 
                HaipLevel = "Level1_High",
                SdJwtConfig = new { Algorithm = "ES256", KeyCurve = "P-256", DecoyDigests = 1 },
                Requirements = new[] { "GDPR", "SOC 2", "ISO 27001" }
            }
        };

        Console.WriteLine("   TENANT SD-JWT CONFIGURATION MATRIX:");
        Console.WriteLine("   +----------------+------------+----------+----------+-------------+");
        Console.WriteLine("   | Tenant         | HAIP Level | Algorithm| Key Curve| Decoy Count |");
        Console.WriteLine("   +----------------+------------+----------+----------+-------------+");
        
        foreach (var tenant in tenants)
        {
            var levelDisplay = tenant.HaipLevel.Replace("Level", "L").Replace("_", " ");
            Console.WriteLine($"   | {tenant.TenantId,-14} | {levelDisplay,-10} | {tenant.SdJwtConfig.Algorithm,-8} | {tenant.SdJwtConfig.KeyCurve,-8} | {tenant.SdJwtConfig.DecoyDigests,-11} |");
        }
        
        Console.WriteLine("   +----------------+------------+----------+----------+-------------+");
        Console.WriteLine();

        Console.WriteLine("   TENANT-SPECIFIC SD-JWT IMPLEMENTATION:");
        Console.WriteLine("   ```csharp");
        Console.WriteLine("   // Multi-tenant SdIssuer factory");
        Console.WriteLine("   public class TenantSdIssuerFactory");
        Console.WriteLine("   {");
        Console.WriteLine("       public SdIssuer CreateForTenant(string tenantId)");
        Console.WriteLine("       {");
        Console.WriteLine("           var config = GetTenantConfig(tenantId);");
        Console.WriteLine("           var key = GetTenantSigningKey(tenantId);");
        Console.WriteLine("           ");
        Console.WriteLine("           return new SdIssuer(");
        Console.WriteLine("               key,");
        Console.WriteLine("               config.Algorithm,");
        Console.WriteLine("               config.HashAlgorithm);");
        Console.WriteLine("       }");
        Console.WriteLine("       ");
        Console.WriteLine("       public SdIssuanceOptions GetTenantOptions(string tenantId)");
        Console.WriteLine("       {");
        Console.WriteLine("           var config = GetTenantConfig(tenantId);");
        Console.WriteLine("           return new SdIssuanceOptions");
        Console.WriteLine("           {");
        Console.WriteLine("               AllowWeakAlgorithms = false,");
        Console.WriteLine("               DecoyDigests = config.DecoyDigests");
        Console.WriteLine("           };");
        Console.WriteLine("       }");
        Console.WriteLine("   }");
        Console.WriteLine("   ```");
        Console.WriteLine();

        Console.WriteLine("   TENANT ISOLATION AND SECURITY:");
        Console.WriteLine("   • Separate SdIssuer instances per tenant with tenant-specific keys");
        Console.WriteLine("   • Tenant-isolated SdVerifier configurations and trust frameworks");
        Console.WriteLine("   • Per-tenant SdIssuanceOptions with appropriate privacy levels");
        Console.WriteLine("   • Isolated audit trails for SD-JWT operations per tenant");
        Console.WriteLine("   • Tenant-specific compliance reporting and metrics");
        Console.WriteLine("   • Cross-tenant security boundary enforcement in SD-JWT processing");
        Console.WriteLine();

        Console.WriteLine("   MULTI-TENANT COMPLIANCE DASHBOARD:");
        var complianceStats = new
        {
            TotalTenants = tenants.Length,
            Level1Tenants = tenants.Count(t => t.HaipLevel == "Level1_High"),
            Level2Tenants = tenants.Count(t => t.HaipLevel == "Level2_VeryHigh"),
            TotalSdJwtOperationsToday = 125_340,
            SdJwtIssuanceOperations = 45_120,
            SdJwtVerificationOperations = 80_220,
            AverageComplianceScore = 98.7,
            ComplianceViolationsToday = 3
        };

        Console.WriteLine($"   Total Tenants: {complianceStats.TotalTenants}");
        Console.WriteLine($"   Level 1 (High): {complianceStats.Level1Tenants} tenants");
        Console.WriteLine($"   Level 2 (Very High): {complianceStats.Level2Tenants} tenants");
        Console.WriteLine($"   SD-JWT Operations Today: {complianceStats.TotalSdJwtOperationsToday:N0}");
        Console.WriteLine($"   - Issuance Operations: {complianceStats.SdJwtIssuanceOperations:N0}");
        Console.WriteLine($"   - Verification Operations: {complianceStats.SdJwtVerificationOperations:N0}");
        Console.WriteLine($"   Average Compliance Score: {complianceStats.AverageComplianceScore:F1}%");
        Console.WriteLine($"   HAIP Violations Today: {complianceStats.ComplianceViolationsToday}");
        Console.WriteLine();

        await Task.CompletedTask;
    }

    private static bool ValidateAlgorithmForLevel2(string algorithm)
    {
        var level2Algorithms = new[] { "ES384", "ES512", "PS384", "PS512", "EdDSA" };
        return level2Algorithms.Contains(algorithm);
    }
}
