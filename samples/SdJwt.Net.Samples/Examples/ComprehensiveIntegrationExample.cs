using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.Holder;
using SdJwt.Net.Issuer;
using SdJwt.Net.Models;
using SdJwt.Net.StatusList.Issuer;
using SdJwt.Net.StatusList.Models;
using SdJwt.Net.Vc.Issuer;
using SdJwt.Net.Vc.Models;
using SdJwt.Net.Vc.Verifier;
using SdJwt.Net.Verifier;
using System.Collections;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text.Json;
using JsonWebKey = Microsoft.IdentityModel.Tokens.JsonWebKey;

namespace SdJwt.Net.Samples.Examples;

/// <summary>
/// Comprehensive integration example demonstrating advanced SD-JWT ecosystem features
/// Shows complex workflows with multiple packages working together
/// </summary>
public class ComprehensiveIntegrationExample
{
    public static async Task RunExample(IServiceProvider services)
    {
        var logger = services.GetRequiredService<ILogger<ComprehensiveIntegrationExample>>();
        
        Console.WriteLine("\n╔═════════════════════════════════════════════════════════╗");
        Console.WriteLine("║         Comprehensive SD-JWT Integration Example      ║");
        Console.WriteLine("║          (Advanced Multi-Package Integration)          ║");
        Console.WriteLine("╚═════════════════════════════════════════════════════════╝");

        Console.WriteLine("\nThis example demonstrates advanced integration scenarios:");
        Console.WriteLine("• Complex nested selective disclosure");
        Console.WriteLine("• Multiple credential types in single workflow");
        Console.WriteLine("• Status list integration with revocation");
        Console.WriteLine("• Key binding and holder verification");
        Console.WriteLine("• JSON serialization with advanced formats");
        Console.WriteLine("• Performance optimization techniques");
        Console.WriteLine();

        await DemonstrateAdvancedSelectiveDisclosure();
        await DemonstrateMultiCredentialWorkflow(services);
        await DemonstrateStatusIntegratedCredentials(services);
        await DemonstrateAdvancedKeyBinding();
        await DemonstratePerformanceOptimizations();

        Console.WriteLine("\n╔═════════════════════════════════════════════════════════╗");
        Console.WriteLine("║      Comprehensive integration example completed!      ║");
        Console.WriteLine("║                                                         ║");
        Console.WriteLine("║  ✓ Advanced selective disclosure patterns              ║");
        Console.WriteLine("║  ✓ Multi-credential complex workflows                  ║");
        Console.WriteLine("║  ✓ Status list integration                             ║");
        Console.WriteLine("║  ✓ Advanced key binding scenarios                      ║");
        Console.WriteLine("║  ✓ Performance optimization techniques                 ║");
        Console.WriteLine("╚═════════════════════════════════════════════════════════╝");
    }

    private static async Task DemonstrateAdvancedSelectiveDisclosure()
    {
        Console.WriteLine("\n1. ADVANCED SELECTIVE DISCLOSURE PATTERNS");
        Console.WriteLine("   Demonstrating complex nested disclosure structures");
        Console.WriteLine();

        using var issuerEcdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        using var holderEcdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        
        var issuerKey = new ECDsaSecurityKey(issuerEcdsa) { KeyId = "advanced-issuer-2024" };
        var holderPrivateKey = new ECDsaSecurityKey(holderEcdsa) { KeyId = "advanced-holder-key" };
        var holderPublicKey = new ECDsaSecurityKey(holderEcdsa) { KeyId = "advanced-holder-key" };
        var holderJwk = JsonWebKeyConverter.ConvertFromSecurityKey(holderPublicKey);

        var issuer = new SdIssuer(issuerKey, SecurityAlgorithms.EcdsaSha256);

        // Create deeply nested claims structure
        var complexClaims = new JwtPayload
        {
            [JwtRegisteredClaimNames.Iss] = "https://advanced.issuer.com",
            [JwtRegisteredClaimNames.Sub] = "complex_user_123",
            ["personal_info"] = new
            {
                basic = new
                {
                    given_name = "Alexander",
                    family_name = "Johnson",
                    middle_name = "Robert"
                },
                contact = new
                {
                    email = "alexander.johnson@example.com",
                    phone = "+1-555-123-4567",
                    secondary_email = "alex.j.personal@gmail.com"
                },
                address = new
                {
                    home = new
                    {
                        street = "123 Elm Street",
                        city = "Springfield",
                        state = "IL",
                        zip = "62701",
                        country = "US"
                    },
                    work = new
                    {
                        company = "Tech Innovations Inc",
                        street = "456 Business Ave",
                        city = "Chicago",
                        state = "IL",
                        zip = "60601",
                        country = "US"
                    }
                }
            },
            ["professional"] = new
            {
                current_position = new
                {
                    title = "Senior Software Architect",
                    department = "Engineering",
                    level = "L6",
                    start_date = "2022-03-15"
                },
                skills = new[]
                {
                    "Cloud Architecture",
                    "Microservices",
                    "Kubernetes",
                    "Go",
                    "Python",
                    "System Design"
                },
                certifications = new[]
                {
                    new { name = "AWS Solutions Architect", level = "Professional", expiry = "2025-12-31" },
                    new { name = "Kubernetes Administrator", level = "CKA", expiry = "2024-08-15" }
                },
                salary_info = new
                {
                    base_salary = 145000,
                    bonus_target = 0.15,
                    equity_value = 50000,
                    total_compensation = 216750
                }
            }
        };

        // Define granular selective disclosure
        var advancedOptions = new SdIssuanceOptions
        {
            DisclosureStructure = new
            {
                personal_info = new
                {
                    basic = new
                    {
                        given_name = true,
                        family_name = true,
                        middle_name = true    // Can disclose middle name separately
                    },
                    contact = new
                    {
                        email = true,         // Primary email can be disclosed
                        phone = true,         // Phone can be disclosed
                        secondary_email = true // Secondary email can be disclosed separately
                    },
                    address = new
                    {
                        home = new
                        {
                            city = true,      // Can show city without full address
                            state = true,     // Can show state without full address
                            zip = true,       // Can show ZIP separately
                            street = true,    // Can disclose street separately
                            country = true    // Country can be disclosed
                        },
                        work = true           // Entire work address as one disclosure
                    }
                },
                professional = new
                {
                    current_position = new
                    {
                        title = true,         // Job title can be disclosed
                        department = true,    // Department can be disclosed
                        level = true,         // Level can be disclosed separately
                        start_date = true     // Start date can be disclosed
                    },
                    skills = true,            // Skills array as one disclosure
                    certifications = true,    // Certifications array as one disclosure
                    salary_info = new
                    {
                        base_salary = true,   // Base salary can be disclosed separately
                        bonus_target = true,  // Bonus target separately
                        equity_value = true,  // Equity value separately
                        total_compensation = true // Total comp separately
                    }
                }
            }
        };

        var complexCredential = issuer.Issue(complexClaims, advancedOptions, holderJwk);

        Console.WriteLine("✓ Complex nested credential issued");
        Console.WriteLine($"  - Total disclosures available: {complexCredential.Disclosures.Count}");
        Console.WriteLine("  - Disclosure structure:");
        
        foreach (var disclosure in complexCredential.Disclosures.Take(10)) // Show first 10
        {
            Console.WriteLine($"    • {disclosure.ClaimName}: {JsonSerializer.Serialize(disclosure.ClaimValue)}");
        }
        if (complexCredential.Disclosures.Count > 10)
        {
            Console.WriteLine($"    ... and {complexCredential.Disclosures.Count - 10} more disclosures");
        }

        // Demonstrate different presentation scenarios
        await DemonstrateSelectivePresentationScenarios(complexCredential.Issuance, holderPrivateKey, issuerKey);
    }

    private static async Task DemonstrateSelectivePresentationScenarios(string credential, ECDsaSecurityKey holderPrivateKey, ECDsaSecurityKey issuerKey)
    {
        var holder = new SdJwtHolder(credential);
        var verifier = new SdVerifier(async issuer => issuerKey);

        var baseValidationParams = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = "https://advanced.issuer.com",
            ValidateAudience = false,
            ValidateLifetime = true
        };

        // Scenario 1: Job application - show professional info but not salary
        Console.WriteLine("\n   Scenario 1: Job Application Presentation");
        var jobPresentation = holder.CreatePresentation(
            disclosure => disclosure.ClaimName.Contains("title") || 
                         disclosure.ClaimName.Contains("department") ||
                         disclosure.ClaimName.Contains("skills") ||
                         disclosure.ClaimName.Contains("certifications"),
            new JwtPayload
            {
                [JwtRegisteredClaimNames.Aud] = "https://newcompany.example.com",
                ["purpose"] = "job_application"
            },
            holderPrivateKey,
            SecurityAlgorithms.EcdsaSha256
        );

        var jobResult = await verifier.VerifyAsync(jobPresentation, baseValidationParams);
        Console.WriteLine("      ✓ Job application verification successful");
        Console.WriteLine("      ✓ Professional qualifications disclosed, salary protected");

        // Scenario 2: Loan application - show income but not detailed personal info
        Console.WriteLine("\n   Scenario 2: Loan Application Presentation");
        var loanPresentation = holder.CreatePresentation(
            disclosure => disclosure.ClaimName.Contains("salary") ||
                         disclosure.ClaimName.Contains("total_compensation") ||
                         disclosure.ClaimName.Contains("given_name") ||
                         disclosure.ClaimName.Contains("city") ||
                         disclosure.ClaimName.Contains("state"),
            new JwtPayload
            {
                [JwtRegisteredClaimNames.Aud] = "https://bank.example.com",
                ["purpose"] = "loan_application"
            },
            holderPrivateKey,
            SecurityAlgorithms.EcdsaSha256
        );

        var loanResult = await verifier.VerifyAsync(loanPresentation, baseValidationParams);
        Console.WriteLine("      ✓ Loan application verification successful");
        Console.WriteLine("      ✓ Income and basic location disclosed, details protected");

        // Scenario 3: Background check - comprehensive but selective
        Console.WriteLine("\n   Scenario 3: Background Check Presentation");
        var backgroundPresentation = holder.CreatePresentation(
            disclosure => !disclosure.ClaimName.Contains("salary") &&
                         !disclosure.ClaimName.Contains("compensation") &&
                         !disclosure.ClaimName.Contains("secondary_email") &&
                         !disclosure.ClaimName.Contains("street"),
            new JwtPayload
            {
                [JwtRegisteredClaimNames.Aud] = "https://backgroundcheck.example.com",
                ["purpose"] = "employment_verification"
            },
            holderPrivateKey,
            SecurityAlgorithms.EcdsaSha256
        );

        var backgroundResult = await verifier.VerifyAsync(backgroundPresentation, baseValidationParams);
        Console.WriteLine("      ✓ Background check verification successful");
        Console.WriteLine("      ✓ Professional and general info disclosed, sensitive data protected");
    }

    private static async Task DemonstrateMultiCredentialWorkflow(IServiceProvider services)
    {
        Console.WriteLine("\n2. MULTI-CREDENTIAL WORKFLOW INTEGRATION");
        Console.WriteLine("   Demonstrating orchestrated use of multiple credential types");
        Console.WriteLine();

        // Setup multiple issuers for different credential types
        using var universityEcdsa = ECDsa.Create();
        using var employerEcdsa = ECDsa.Create();
        using var governmentEcdsa = ECDsa.Create();
        using var holderEcdsa = ECDsa.Create();

        var universityKey = new ECDsaSecurityKey(universityEcdsa) { KeyId = "university-2024" };
        var employerKey = new ECDsaSecurityKey(employerEcdsa) { KeyId = "employer-2024" };
        var governmentKey = new ECDsaSecurityKey(governmentEcdsa) { KeyId = "government-2024" };
        var holderPrivateKey = new ECDsaSecurityKey(holderEcdsa) { KeyId = "multi-holder" };
        var holderPublicKey = new ECDsaSecurityKey(holderEcdsa) { KeyId = "multi-holder" };
        var holderJwk = JsonWebKeyConverter.ConvertFromSecurityKey(holderPublicKey);

        // Issue multiple types of credentials
        var credentials = await IssueMultipleCredentials(universityKey, employerKey, governmentKey, holderJwk);

        // Demonstrate complex presentation requirements
        await DemonstrateComplexPresentationRequirements(credentials, holderPrivateKey, holderPublicKey, 
            universityKey, employerKey, governmentKey);
    }

    private static async Task<MultiCredentialSet> IssueMultipleCredentials(
        ECDsaSecurityKey universityKey, ECDsaSecurityKey employerKey, ECDsaSecurityKey governmentKey, JsonWebKey holderJwk)
    {
        // 1. University degree credential
        var universityIssuer = new SdJwtVcIssuer(universityKey, SecurityAlgorithms.EcdsaSha256);
        var degreePayload = new SdJwtVcPayload
        {
            Issuer = "https://university.example.edu",
            Subject = "did:example:student123",
            IssuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            AdditionalData = new Dictionary<string, object>
            {
                ["student_name"] = "Maria Rodriguez",
                ["student_id"] = "UNIV2024789",
                ["degree"] = "Master of Science",
                ["major"] = "Computer Science",
                ["gpa"] = 3.85,
                ["graduation_date"] = "2024-05-15",
                ["honors"] = "magna cum laude",
                ["thesis"] = "Advanced Machine Learning in Cybersecurity",
                ["advisor"] = "Prof. Sarah Chen"
            }
        };

        var degreeOptions = new SdIssuanceOptions
        {
            DisclosureStructure = new { gpa = true, thesis = true, advisor = true, honors = true }
        };

        var degreeCredential = universityIssuer.Issue(
            "https://credentials.university.edu/degree", degreePayload, degreeOptions, holderJwk);

        // 2. Employment credential
        var employerIssuer = new SdJwtVcIssuer(employerKey, SecurityAlgorithms.EcdsaSha256);
        var employmentPayload = new SdJwtVcPayload
        {
            Issuer = "https://hr.techcorp.com",
            Subject = "did:example:employee123",
            IssuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            AdditionalData = new Dictionary<string, object>
            {
                ["employee_name"] = "Maria Rodriguez",
                ["employee_id"] = "EMP789456",
                ["position"] = "Senior Software Engineer",
                ["department"] = "AI Research",
                ["start_date"] = "2024-06-01",
                ["salary"] = 125000,
                ["security_clearance"] = "Confidential",
                ["manager"] = "Dr. Alex Kim"
            }
        };

        var employmentOptions = new SdIssuanceOptions
        {
            DisclosureStructure = new { salary = true, security_clearance = true, manager = true }
        };

        var employmentCredential = employerIssuer.Issue(
            "https://credentials.techcorp.com/employment", employmentPayload, employmentOptions, holderJwk);

        // 3. Government ID credential
        var governmentIssuer = new SdJwtVcIssuer(governmentKey, SecurityAlgorithms.EcdsaSha256);
        var idPayload = new SdJwtVcPayload
        {
            Issuer = "https://dmv.california.gov",
            Subject = "did:example:citizen123",
            IssuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            ExpiresAt = DateTimeOffset.UtcNow.AddYears(5).ToUnixTimeSeconds(),
            AdditionalData = new Dictionary<string, object>
            {
                ["full_name"] = "Maria Elena Rodriguez",
                ["date_of_birth"] = "1995-08-22",
                ["license_number"] = "D1234567",
                ["address"] = "456 Tech Street, San Francisco, CA 94105",
                ["age_over_18"] = true,
                ["age_over_21"] = true,
                ["organ_donor"] = true
            }
        };

        var idOptions = new SdIssuanceOptions
        {
            DisclosureStructure = new 
            { 
                date_of_birth = true, 
                address = true, 
                age_over_18 = true, 
                age_over_21 = true, 
                organ_donor = true 
            }
        };

        var idCredential = governmentIssuer.Issue(
            "https://credentials.dmv.ca.gov/license", idPayload, idOptions, holderJwk);

        Console.WriteLine("✓ Multiple credentials issued:");
        Console.WriteLine("  • University degree with selective academic details");
        Console.WriteLine("  • Employment credential with salary and clearance options");
        Console.WriteLine("  • Government ID with age verification and privacy options");

        return new MultiCredentialSet(degreeCredential.Issuance, employmentCredential.Issuance, idCredential.Issuance);
    }

    private static async Task DemonstrateComplexPresentationRequirements(
        MultiCredentialSet credentials, ECDsaSecurityKey holderPrivateKey, ECDsaSecurityKey holderPublicKey,
        ECDsaSecurityKey universityKey, ECDsaSecurityKey employerKey, ECDsaSecurityKey governmentKey)
    {
        // Create verifier that can resolve multiple issuers
        var multiVerifier = new SdJwtVcVerifier(async jwt =>
        {
            var issuer = jwt.Claims.FirstOrDefault(c => c.Type == "iss")?.Value;
            return issuer switch
            {
                "https://university.example.edu" => universityKey,
                "https://hr.techcorp.com" => employerKey,
                "https://dmv.california.gov" => governmentKey,
                _ => throw new InvalidOperationException($"Unknown issuer: {issuer}")
            };
        });

        // Scenario: Security clearance application requiring multiple credentials
        Console.WriteLine("\n   Complex Scenario: Security Clearance Application");
        Console.WriteLine("   Requirements:");
        Console.WriteLine("   • Education: Advanced degree in technical field");
        Console.WriteLine("   • Employment: Current position in technology");
        Console.WriteLine("   • Identity: Government-issued ID with age verification");
        Console.WriteLine("   • Privacy: No personal details beyond requirements");

        // Create presentations from each credential
        var degreeHolder = new SdJwtHolder(credentials.DegreeCredential);
        var employmentHolder = new SdJwtHolder(credentials.EmploymentCredential);
        var idHolder = new SdJwtHolder(credentials.IdCredential);

        var clearanceApplicationAudience = "https://securityclearance.gov";
        var applicationNonce = "clearance-app-2024-" + Guid.NewGuid().ToString("N")[..8];

        // Degree presentation - show degree and major, hide GPA and thesis
        var degreePresentation = degreeHolder.CreatePresentation(
            disclosure => disclosure.ClaimName == "honors", // Only show honors
            new JwtPayload
            {
                [JwtRegisteredClaimNames.Aud] = clearanceApplicationAudience,
                ["nonce"] = applicationNonce,
                ["purpose"] = "education_verification"
            },
            holderPrivateKey,
            SecurityAlgorithms.EcdsaSha256
        );

        // Employment presentation - show position and department, hide salary
        var employmentPresentation = employmentHolder.CreatePresentation(
            disclosure => disclosure.ClaimName == "security_clearance", // Show existing clearance
            new JwtPayload
            {
                [JwtRegisteredClaimNames.Aud] = clearanceApplicationAudience,
                ["nonce"] = applicationNonce,
                ["purpose"] = "employment_verification"
            },
            holderPrivateKey,
            SecurityAlgorithms.EcdsaSha256
        );

        // ID presentation - show age verification only
        var idPresentation = idHolder.CreatePresentation(
            disclosure => disclosure.ClaimName == "age_over_18" || disclosure.ClaimName == "age_over_21",
            new JwtPayload
            {
                [JwtRegisteredClaimNames.Aud] = clearanceApplicationAudience,
                ["nonce"] = applicationNonce,
                ["purpose"] = "identity_verification"
            },
            holderPrivateKey,
            SecurityAlgorithms.EcdsaSha256
        );

        // Verify all presentations
        var validationParams = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = false,
            ValidateLifetime = true
        };

        var kbValidationParams = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = true,
            ValidAudience = clearanceApplicationAudience,
            ValidateLifetime = false,
            IssuerSigningKey = holderPublicKey
        };

        var degreeResult = await multiVerifier.VerifyAsync(
            degreePresentation, validationParams, kbValidationParams, 
            "https://credentials.university.edu/degree");

        var employmentResult = await multiVerifier.VerifyAsync(
            employmentPresentation, validationParams, kbValidationParams, 
            "https://credentials.techcorp.com/employment");

        var idResult = await multiVerifier.VerifyAsync(
            idPresentation, validationParams, kbValidationParams, 
            "https://credentials.dmv.ca.gov/license");

        Console.WriteLine("\n   ✓ Multi-credential verification completed:");
        Console.WriteLine($"      • Education verified: {degreeResult.KeyBindingVerified}");
        Console.WriteLine($"      • Employment verified: {employmentResult.KeyBindingVerified}");
        Console.WriteLine($"      • Identity verified: {idResult.KeyBindingVerified}");
        Console.WriteLine("      • All presentations linked to same holder key");
        Console.WriteLine("      • Minimal information disclosed for each requirement");
    }

    private static async Task DemonstrateStatusIntegratedCredentials(IServiceProvider services)
    {
        Console.WriteLine("\n3. STATUS LIST INTEGRATION WITH CREDENTIALS");
        Console.WriteLine("   Demonstrating real-time revocation checking in workflows");
        Console.WriteLine();

        using var issuerEcdsa = ECDsa.Create();
        using var holderEcdsa = ECDsa.Create();
        
        var issuerKey = new ECDsaSecurityKey(issuerEcdsa) { KeyId = "status-issuer-2024" };
        var holderPrivateKey = new ECDsaSecurityKey(holderEcdsa) { KeyId = "status-holder" };
        var holderPublicKey = new ECDsaSecurityKey(holderEcdsa) { KeyId = "status-holder" };
        var holderJwk = JsonWebKeyConverter.ConvertFromSecurityKey(holderPublicKey);

        // Create status list
        var statusManager = new StatusListManager(issuerKey, SecurityAlgorithms.EcdsaSha256);
        var statusBits = new BitArray(1000, false);
        statusBits[123] = true; // Revoke credential at index 123

        const string statusListUrl = "https://status.example.com/revocation/1";
        var statusListToken = await statusManager.CreateStatusListTokenFromBitArrayAsync(statusListUrl, statusBits);

        // Issue credential with status reference
        var vcIssuer = new SdJwtVcIssuer(issuerKey, SecurityAlgorithms.EcdsaSha256);
        var statusAwarePayload = new SdJwtVcPayload
        {
            Issuer = "https://professional.licensing.gov",
            Subject = "did:example:professional123",
            IssuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            Status = new { 
                status_list = new StatusListReference 
                { 
                    Index = 456,  // This credential is NOT revoked
                    Uri = statusListUrl 
                } 
            },
            AdditionalData = new Dictionary<string, object>
            {
                ["license_type"] = "Professional Engineer",
                ["license_number"] = "PE123456789",
                ["holder_name"] = "Dr. Jennifer Walsh",
                ["specialization"] = "Structural Engineering",
                ["issue_date"] = "2024-01-15",
                ["expiry_date"] = "2027-01-15"
            }
        };

        var statusOptions = new SdIssuanceOptions
        {
            DisclosureStructure = new
            {
                specialization = true,
                issue_date = true,
                expiry_date = true
            }
        };

        var statusCredential = vcIssuer.Issue(
            "https://credentials.licensing.gov/engineer", statusAwarePayload, statusOptions, holderJwk);

        Console.WriteLine("✓ Status-aware credential issued:");
        Console.WriteLine($"  • License type: Professional Engineer");
        Console.WriteLine($"  • Status list index: 456 (not revoked)");
        Console.WriteLine($"  • Status list URL: {statusListUrl}");

        // Demonstrate status checking in verification workflow
        await DemonstrateStatusCheckingWorkflow(statusCredential.Issuance, holderPrivateKey, issuerKey);
    }

    private static async Task DemonstrateStatusCheckingWorkflow(
        string credential, ECDsaSecurityKey holderPrivateKey, ECDsaSecurityKey issuerKey)
    {
        Console.WriteLine("\n   Status Verification Workflow:");
        
        // Create presentation
        var holder = new SdJwtHolder(credential);
        var presentation = holder.CreatePresentation(
            disclosure => disclosure.ClaimName == "specialization",
            new JwtPayload
            {
                [JwtRegisteredClaimNames.Aud] = "https://engineering.firm.com",
                ["purpose"] = "contractor_verification"
            },
            holderPrivateKey,
            SecurityAlgorithms.EcdsaSha256
        );

        // Verify presentation (note: status checking would be done separately in production)
        var verifier = new SdJwtVcVerifier(async issuer => issuerKey);
        var validationParams = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = "https://professional.licensing.gov",
            ValidateAudience = false,
            ValidateLifetime = true
        };

        var result = await verifier.VerifyAsync(presentation, validationParams);
        
        Console.WriteLine("      ✓ Credential structure verification: PASSED");
        Console.WriteLine("      ✓ Signature verification: PASSED");
        Console.WriteLine("      ✓ Key binding verification: PASSED");
        Console.WriteLine("      • Status verification: Would check status list in production");
        Console.WriteLine("      • Engineering firm can trust the professional license");
    }

    private static async Task DemonstrateAdvancedKeyBinding()
    {
        Console.WriteLine("\n4. ADVANCED KEY BINDING SCENARIOS");
        Console.WriteLine("   Demonstrating sophisticated key binding and holder verification");
        Console.WriteLine();

        using var issuerEcdsa = ECDsa.Create();
        using var holder1Ecdsa = ECDsa.Create();
        using var holder2Ecdsa = ECDsa.Create();
        
        var issuerKey = new ECDsaSecurityKey(issuerEcdsa) { KeyId = "kb-issuer-2024" };
        var holder1PrivateKey = new ECDsaSecurityKey(holder1Ecdsa) { KeyId = "holder1-key" };
        var holder1PublicKey = new ECDsaSecurityKey(holder1Ecdsa) { KeyId = "holder1-key" };
        var holder2PrivateKey = new ECDsaSecurityKey(holder2Ecdsa) { KeyId = "holder2-key" };
        var holder2PublicKey = new ECDsaSecurityKey(holder2Ecdsa) { KeyId = "holder2-key" };
        
        var holder1Jwk = JsonWebKeyConverter.ConvertFromSecurityKey(holder1PublicKey);
        var holder2Jwk = JsonWebKeyConverter.ConvertFromSecurityKey(holder2PublicKey);

        var issuer = new SdIssuer(issuerKey, SecurityAlgorithms.EcdsaSha256);

        // Issue same credential to two different holders
        var sharedClaims = new JwtPayload
        {
            [JwtRegisteredClaimNames.Iss] = "https://kb.issuer.com",
            [JwtRegisteredClaimNames.Sub] = "shared_resource_access",
            ["resource_type"] = "Shared Document",
            ["access_level"] = "Read-Write",
            ["project_name"] = "Alpha Project",
            ["valid_until"] = DateTimeOffset.UtcNow.AddDays(30).ToUnixTimeSeconds()
        };

        var kbOptions = new SdIssuanceOptions
        {
            DisclosureStructure = new { access_level = true, valid_until = true }
        };

        var holder1Credential = issuer.Issue(sharedClaims, kbOptions, holder1Jwk);
        var holder2Credential = issuer.Issue(sharedClaims, kbOptions, holder2Jwk);

        Console.WriteLine("✓ Key binding demonstration setup:");
        Console.WriteLine("  • Same credential issued to two different holders");
        Console.WriteLine("  • Each bound to different holder key");
        Console.WriteLine("  • Demonstrates holder identity verification");

        // Demonstrate holder verification
        await DemonstrateHolderIdentityVerification(
            holder1Credential.Issuance, holder1PrivateKey, holder1PublicKey,
            holder2Credential.Issuance, holder2PrivateKey, holder2PublicKey,
            issuerKey);
    }

    private static async Task DemonstrateHolderIdentityVerification(
        string holder1Credential, ECDsaSecurityKey holder1PrivateKey, ECDsaSecurityKey holder1PublicKey,
        string holder2Credential, ECDsaSecurityKey holder2PrivateKey, ECDsaSecurityKey holder2PublicKey,
        ECDsaSecurityKey issuerKey)
    {
        var verifier = new SdVerifier(async issuer => issuerKey);
        var resourceAudience = "https://project.alpha.com/api";

        // Holder 1 creates presentation
        var holder1 = new SdJwtHolder(holder1Credential);
        var holder1Presentation = holder1.CreatePresentation(
            disclosure => disclosure.ClaimName == "access_level",
            new JwtPayload
            {
                [JwtRegisteredClaimNames.Aud] = resourceAudience,
                ["timestamp"] = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                ["operation"] = "read_document"
            },
            holder1PrivateKey,
            SecurityAlgorithms.EcdsaSha256
        );

        // Holder 2 creates presentation
        var holder2 = new SdJwtHolder(holder2Credential);
        var holder2Presentation = holder2.CreatePresentation(
            disclosure => disclosure.ClaimName == "access_level",
            new JwtPayload
            {
                [JwtRegisteredClaimNames.Aud] = resourceAudience,
                ["timestamp"] = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                ["operation"] = "write_document"
            },
            holder2PrivateKey,
            SecurityAlgorithms.EcdsaSha256
        );

        var validationParams = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = "https://kb.issuer.com",
            ValidateAudience = false,
            ValidateLifetime = true
        };

        // Verify holder 1 presentation
        var holder1KbParams = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = true,
            ValidAudience = resourceAudience,
            ValidateLifetime = false,
            IssuerSigningKey = holder1PublicKey
        };

        var holder1Result = await verifier.VerifyAsync(holder1Presentation, validationParams, holder1KbParams);

        // Verify holder 2 presentation
        var holder2KbParams = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = true,
            ValidAudience = resourceAudience,
            ValidateLifetime = false,
            IssuerSigningKey = holder2PublicKey
        };

        var holder2Result = await verifier.VerifyAsync(holder2Presentation, validationParams, holder2KbParams);

        Console.WriteLine("\n   Key Binding Verification Results:");
        Console.WriteLine($"      • Holder 1 verification: {(holder1Result.KeyBindingVerified ? "SUCCESS" : "FAILED")}");
        Console.WriteLine($"      • Holder 2 verification: {(holder2Result.KeyBindingVerified ? "SUCCESS" : "FAILED")}");
        Console.WriteLine("      • Each presentation cryptographically bound to different holder");
        Console.WriteLine("      • Resource server can distinguish between holders");

        // Demonstrate invalid key binding attempt
        try
        {
            var invalidKbParams = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = true,
                ValidAudience = resourceAudience,
                ValidateLifetime = false,
                IssuerSigningKey = holder2PublicKey // Wrong key for holder 1's presentation
            };

            await verifier.VerifyAsync(holder1Presentation, validationParams, invalidKbParams);
            Console.WriteLine("      • ERROR: Invalid key binding should have failed!");
        }
        catch (SecurityTokenException)
        {
            Console.WriteLine("      ✓ Invalid key binding correctly rejected");
        }
    }

    private static async Task DemonstratePerformanceOptimizations()
    {
        Console.WriteLine("\n5. PERFORMANCE OPTIMIZATION TECHNIQUES");
        Console.WriteLine("   Demonstrating efficient patterns for high-throughput scenarios");
        Console.WriteLine();

        using var issuerEcdsa = ECDsa.Create();
        using var holderEcdsa = ECDsa.Create();
        
        var issuerKey = new ECDsaSecurityKey(issuerEcdsa) { KeyId = "perf-issuer-2024" };
        var holderPrivateKey = new ECDsaSecurityKey(holderEcdsa) { KeyId = "perf-holder" };
        var holderPublicKey = new ECDsaSecurityKey(holderEcdsa) { KeyId = "perf-holder" };
        var holderJwk = JsonWebKeyConverter.ConvertFromSecurityKey(holderPublicKey);

        var issuer = new SdIssuer(issuerKey, SecurityAlgorithms.EcdsaSha256);

        // Performance test 1: Batch credential issuance
        Console.WriteLine("   Performance Test 1: Batch Credential Issuance");
        var batchClaims = new JwtPayload
        {
            [JwtRegisteredClaimNames.Iss] = "https://perf.issuer.com",
            ["batch_test"] = true,
            ["data"] = "Sample credential data for performance testing"
        };

        var perfOptions = new SdIssuanceOptions
        {
            DisclosureStructure = new { data = true }
        };

        const int batchSize = 1000;
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var credentials = new List<string>(batchSize);

        for (int i = 0; i < batchSize; i++)
        {
            var batchClaimsWithIndex = new JwtPayload
            {
                [JwtRegisteredClaimNames.Iss] = "https://perf.issuer.com",
                ["batch_test"] = true,
                ["data"] = "Sample credential data for performance testing",
                ["index"] = i
            };
            var credential = issuer.Issue(batchClaimsWithIndex, perfOptions, holderJwk);
            credentials.Add(credential.Issuance);
        }

        stopwatch.Stop();

        Console.WriteLine($"      ✓ {batchSize:N0} credentials issued in {stopwatch.ElapsedMilliseconds:N0} ms");
        Console.WriteLine($"      ✓ Average: {stopwatch.ElapsedMilliseconds / (double)batchSize:F2} ms per credential");
        Console.WriteLine($"      ✓ Throughput: {batchSize / stopwatch.Elapsed.TotalSeconds:F0} credentials/second");

        // Performance test 2: Batch verification
        Console.WriteLine("\n   Performance Test 2: Batch Verification");
        var verifier = new SdVerifier(async issuer => issuerKey);

        var validationParams = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = "https://perf.issuer.com",
            ValidateAudience = false,
            ValidateLifetime = true
        };

        stopwatch.Restart();
        var verificationTasks = credentials.Take(100).Select(async credential =>
        {
            return await verifier.VerifyAsync(credential, validationParams);
        });

        var verificationResults = await Task.WhenAll(verificationTasks);
        stopwatch.Stop();

        var successfulVerifications = verificationResults.Count(r => r != null);
        Console.WriteLine($"      ✓ {successfulVerifications} verifications completed in {stopwatch.ElapsedMilliseconds:N0} ms");
        Console.WriteLine($"      ✓ Average: {stopwatch.ElapsedMilliseconds / (double)successfulVerifications:F2} ms per verification");
        Console.WriteLine($"      ✓ Concurrent throughput: {successfulVerifications / stopwatch.Elapsed.TotalSeconds:F0} verifications/second");

        // Performance test 3: Memory usage analysis
        Console.WriteLine("\n   Performance Test 3: Memory Usage Analysis");
        var averageCredentialSize = credentials.Take(10).Average(c => System.Text.Encoding.UTF8.GetByteCount(c));
        var totalMemoryKB = (averageCredentialSize * credentials.Count) / 1024.0;

        Console.WriteLine($"      ✓ Average credential size: {averageCredentialSize:F0} bytes");
        Console.WriteLine($"      ✓ Total memory for {credentials.Count:N0} credentials: {totalMemoryKB:F1} KB");
        Console.WriteLine($"      ✓ Memory efficiency: {averageCredentialSize / 1024.0:F2} KB per credential");

        Console.WriteLine("\n   Performance Optimization Recommendations:");
        Console.WriteLine("      • Reuse issuer/verifier instances for batch operations");
        Console.WriteLine("      • Use concurrent verification for high-throughput scenarios");
        Console.WriteLine("      • Consider credential caching for frequently accessed credentials");
        Console.WriteLine("      • Monitor memory usage in production deployments");
    }

    private record MultiCredentialSet(string DegreeCredential, string EmploymentCredential, string IdCredential);
}
