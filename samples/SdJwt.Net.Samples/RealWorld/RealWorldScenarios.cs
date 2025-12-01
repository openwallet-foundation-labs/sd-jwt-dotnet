using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.Holder;
using SdJwt.Net.Issuer;
using SdJwt.Net.Models;
using SdJwt.Net.Vc.Issuer;
using SdJwt.Net.Vc.Models;
using SdJwt.Net.Verifier;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;

namespace SdJwt.Net.Samples.RealWorld;

/// <summary>
/// Real-world scenarios demonstrating the complete SD-JWT ecosystem
/// Shows end-to-end workflows combining multiple packages with actual implementation
/// </summary>
public class RealWorldScenarios
{
    public static async Task RunExample(IServiceProvider services)
    {
        var logger = services.GetRequiredService<ILogger<RealWorldScenarios>>();
        
        Console.WriteLine("\n" + new string('=', 65));
        Console.WriteLine("              Real-World Scenarios Demonstration        ");
        Console.WriteLine("           (Complete SD-JWT Ecosystem Workflows)        ");
        Console.WriteLine(new string('=', 65));

        // Run complete end-to-end scenarios with actual implementations
        await DemonstrateUniversityToBankLoanAsync(services);
        await DemonstrateJobApplicationWorkflowAsync(services);
        await DemonstrateMedicalRecordSharingAsync(services);
        await DemonstrateGovernmentServiceAccessAsync(services);

        Console.WriteLine("\n" + new string('=', 65));
        Console.WriteLine("         Real-World Scenarios completed!                ");
        Console.WriteLine("                                                         ");
        Console.WriteLine("  [X] University to bank loan workflow                    ");
        Console.WriteLine("  [X] Job application with background check               ");
        Console.WriteLine("  [X] Medical record sharing with consent                 ");
        Console.WriteLine("  [X] Government service access verification              ");
        Console.WriteLine(new string('=', 65));
    }

    private static async Task DemonstrateUniversityToBankLoanAsync(IServiceProvider services)
    {
        var logger = services.GetRequiredService<ILogger<RealWorldScenarios>>();
        
        Console.WriteLine("\n" + new string('=', 64));
        Console.WriteLine("SCENARIO 1: University Graduate Applying for Home Loan");
        Console.WriteLine(new string('=', 64));
        Console.WriteLine("Story: Sarah graduated from Stanford with a Computer Science degree");
        Console.WriteLine("       and is now applying for a home loan at her local bank.");
        Console.WriteLine("       The bank needs to verify her education and employment status.");

        // Setup: Create keys for all parties
        using var universityEcdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        using var employerEcdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        using var sarahEcdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        using var bankEcdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);

        var universityKey = new ECDsaSecurityKey(universityEcdsa) { KeyId = "stanford-registrar-2024" };
        var employerKey = new ECDsaSecurityKey(employerEcdsa) { KeyId = "techcorp-hr-2024" };
        var sarahPrivateKey = new ECDsaSecurityKey(sarahEcdsa) { KeyId = "sarah-key-1" };
        var sarahPublicKey = new ECDsaSecurityKey(sarahEcdsa) { KeyId = "sarah-key-1" };
        var bankKey = new ECDsaSecurityKey(bankEcdsa) { KeyId = "bank-verifier-2024" };
        var sarahJwk = JsonWebKeyConverter.ConvertFromSecurityKey(sarahPublicKey);

        // Step 1: University issues degree credential
        Console.WriteLine("\n--- Step 1: University Issues Degree Credential ---");
        var universityIssuer = new SdJwtVcIssuer(universityKey, SecurityAlgorithms.EcdsaSha256);

        var degreePayload = new SdJwtVcPayload
        {
            Issuer = "https://registrar.stanford.edu",
            Subject = "did:example:sarah123",
            IssuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            AdditionalData = new Dictionary<string, object>
            {
                ["student_name"] = "Sarah Johnson",
                ["student_id"] = "STAN2024789",
                ["degree"] = "Bachelor of Science",
                ["major"] = "Computer Science",
                ["gpa"] = 3.7,
                ["graduation_date"] = "2024-06-15",
                ["honors"] = "magna cum laude",
                ["credits_earned"] = 120,
                ["thesis_title"] = "Machine Learning Applications in Healthcare"
            }
        };

        var degreeOptions = new SdIssuanceOptions
        {
            DisclosureStructure = new
            {
                gpa = true,           // Can be selectively disclosed
                honors = true,        // Can be selectively disclosed
                thesis_title = true,  // Can be selectively disclosed
                credits_earned = true // Can be selectively disclosed
            }
        };

        var degreeCredential = universityIssuer.Issue(
            "https://credentials.stanford.edu/bachelor-degree",
            degreePayload,
            degreeOptions,
            sarahJwk
        );

        Console.WriteLine("[X] Stanford University issued degree credential to Sarah");
        Console.WriteLine($"  - Degree: {degreePayload.AdditionalData["degree"]}");
        Console.WriteLine($"  - Major: {degreePayload.AdditionalData["major"]}");
        Console.WriteLine($"  - Selective disclosure available for: GPA, honors, thesis title");

        // Step 2: Employer issues employment credential
        Console.WriteLine("\n--- Step 2: Employer Issues Employment Credential ---");
        var employerIssuer = new SdJwtVcIssuer(employerKey, SecurityAlgorithms.EcdsaSha256);

        var employmentPayload = new SdJwtVcPayload
        {
            Issuer = "https://hr.techcorp.example.com",
            Subject = "did:example:sarah123",
            IssuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            AdditionalData = new Dictionary<string, object>
            {
                ["employee_name"] = "Sarah Johnson",
                ["employee_id"] = "TC789456",
                ["position"] = "Software Engineer II",
                ["department"] = "Product Development",
                ["start_date"] = "2024-07-15",
                ["employment_type"] = "Full-time",
                ["annual_salary"] = 95000,
                ["benefits"] = new[] { "Health", "Dental", "401k", "Stock Options" },
                ["manager"] = "Alex Chen",
                ["office_location"] = "San Francisco, CA"
            }
        };

        var employmentOptions = new SdIssuanceOptions
        {
            DisclosureStructure = new
            {
                annual_salary = true,     // Sensitive salary information
                benefits = true,          // Benefit details
                manager = true,           // Manager information
                office_location = true    // Location details
            }
        };

        var employmentCredential = employerIssuer.Issue(
            "https://credentials.techcorp.com/employment",
            employmentPayload,
            employmentOptions,
            sarahJwk
        );

        Console.WriteLine("[X] TechCorp issued employment credential to Sarah");
        Console.WriteLine($"  - Position: {employmentPayload.AdditionalData["position"]}");
        Console.WriteLine($"  - Department: {employmentPayload.AdditionalData["department"]}");
        Console.WriteLine($"  - Selective disclosure available for: Salary, benefits, manager details");

        // Step 3: Sarah creates selective presentations for bank
        Console.WriteLine("\n--- Step 3: Sarah Creates Selective Presentations for Bank ---");
        
        var loanApplicationNonce = "home-loan-application-2024-12345";
        var bankAudience = "https://bank.example.com";

        Console.WriteLine("Bank requirements for home loan:");
        Console.WriteLine("  - Educational qualification (degree verification)");
        Console.WriteLine("  - Employment verification (position and start date)");
        Console.WriteLine("  - Privacy protection (no GPA or salary details needed)");

        // Education presentation - show degree and honors but not GPA or thesis details
        var degreeHolder = new SdJwtHolder(degreeCredential.Issuance);
        var educationPresentation = degreeHolder.CreatePresentation(
            disclosure => disclosure.ClaimName == "honors", // Only show honors, not GPA or thesis
            new JwtPayload
            {
                [JwtRegisteredClaimNames.Aud] = bankAudience,
                [JwtRegisteredClaimNames.Iat] = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                ["nonce"] = loanApplicationNonce,
                ["purpose"] = "education_verification"
            },
            sarahPrivateKey,
            SecurityAlgorithms.EcdsaSha256
        );

        // Employment presentation - show position but not salary details
        var employmentHolder = new SdJwtHolder(employmentCredential.Issuance);
        var employmentPresentation = employmentHolder.CreatePresentation(
            disclosure => false, // Don't disclose sensitive salary details
            new JwtPayload
            {
                [JwtRegisteredClaimNames.Aud] = bankAudience,
                [JwtRegisteredClaimNames.Iat] = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                ["nonce"] = loanApplicationNonce,
                ["purpose"] = "employment_verification"
            },
            sarahPrivateKey,
            SecurityAlgorithms.EcdsaSha256
        );

        Console.WriteLine("[X] Sarah created selective presentations:");
        Console.WriteLine("  - Education: Disclosed honors only (protected GPA and thesis)");
        Console.WriteLine("  - Employment: Disclosed position and dates (protected salary details)");

        // Step 4: Bank verifies the presentations
        Console.WriteLine("\n--- Step 4: Bank Verifies the Presentations ---");
        
        var bankVerifier = new SdVerifier(jwt =>
        {
            var issuerFromJwt = jwt.Payload.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Iss)?.Value ?? "";
            logger.LogInformation("Bank resolving issuer key for: {Issuer}", issuerFromJwt);
            return Task.FromResult<SecurityKey>(issuerFromJwt switch
            {
                "https://registrar.stanford.edu" => universityKey,
                "https://hr.techcorp.example.com" => employerKey,
                _ => throw new InvalidOperationException($"Unknown issuer: {issuerFromJwt}")
            });
        });

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuers = new[] { "https://registrar.stanford.edu", "https://hr.techcorp.example.com" },
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(5)
        };

        var keyBindingValidation = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = true,
            ValidAudience = bankAudience,
            ValidateLifetime = false,
            IssuerSigningKey = sarahPublicKey,
            ClockSkew = TimeSpan.FromMinutes(5)
        };

        // Verify education credential
        var educationResult = await bankVerifier.VerifyAsync(educationPresentation, validationParameters, keyBindingValidation);
        Console.WriteLine("[X] Education credential verification successful");
        Console.WriteLine($"  - Issuer: {educationResult.ClaimsPrincipal.FindFirst(JwtRegisteredClaimNames.Iss)?.Value}");
        Console.WriteLine($"  - Degree: Computer Science (from always-visible claims)");
        Console.WriteLine($"  - Honors: magna cum laude (selectively disclosed)");

        // Verify employment credential
        var employmentResult = await bankVerifier.VerifyAsync(employmentPresentation, validationParameters, keyBindingValidation);
        Console.WriteLine("[X] Employment credential verification successful");
        Console.WriteLine($"  - Issuer: {employmentResult.ClaimsPrincipal.FindFirst(JwtRegisteredClaimNames.Iss)?.Value}");
        Console.WriteLine($"  - Position: Software Engineer II (from always-visible claims)");
        Console.WriteLine($"  - Start Date: 2024-07-15 (employment stability confirmed)");

        // Step 5: Bank makes loan decision
        Console.WriteLine("\n--- Step 5: Bank Makes Loan Decision ---");
        Console.WriteLine("Bank automated underwriting system:");
        Console.WriteLine("[X] Educational qualification: Verified Stanford CS degree");
        Console.WriteLine("[X] Employment stability: Full-time software engineer");
        Console.WriteLine("[X] Privacy protection: No sensitive data accessed");
        Console.WriteLine("[X] Cryptographic verification: All signatures valid");
        Console.WriteLine();
        Console.WriteLine("LOAN APPLICATION APPROVED");
        Console.WriteLine("   Terms: Pre-approved for home loan up to $500,000");
        Console.WriteLine("   Rate: 6.5% APR (qualified professional rate)");

        Console.WriteLine("\nScenario 1 completed successfully!");
        Console.WriteLine("Key benefits demonstrated:");
        Console.WriteLine("• Privacy-preserving verification (GPA and salary protected)");
        Console.WriteLine("• Cryptographic proof of authenticity");
        Console.WriteLine("• User control over data disclosure");
        Console.WriteLine("• Efficient multi-credential workflows");
    }

    private static async Task DemonstrateJobApplicationWorkflowAsync(IServiceProvider services)
    {
        var logger = services.GetRequiredService<ILogger<RealWorldScenarios>>();
        
        Console.WriteLine("\n" + new string('=', 64));
        Console.WriteLine("SCENARIO 2: Job Application with Background Verification");
        Console.WriteLine(new string('=', 64));
        Console.WriteLine("Story: Michael is applying for a senior engineering position at a");
        Console.WriteLine("       defense contractor. The role requires security clearance");
        Console.WriteLine("       verification and comprehensive background checks.");

        // Setup keys for all parties
        using var govEcdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        using var universityEcdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        using var michaelEcdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);

        var govKey = new ECDsaSecurityKey(govEcdsa) { KeyId = "gov-security-2024" };
        var universityKey = new ECDsaSecurityKey(universityEcdsa) { KeyId = "mit-registrar-2024" };
        var michaelPrivateKey = new ECDsaSecurityKey(michaelEcdsa) { KeyId = "michael-key-1" };
        var michaelPublicKey = new ECDsaSecurityKey(michaelEcdsa) { KeyId = "michael-key-1" };
        var michaelJwk = JsonWebKeyConverter.ConvertFromSecurityKey(michaelPublicKey);

        // Step 1: Government issues security clearance credential
        Console.WriteLine("\n--- Step 1: Government Issues Security Clearance Credential ---");
        var govIssuer = new SdJwtVcIssuer(govKey, SecurityAlgorithms.EcdsaSha256);

        var securityClearancePayload = new SdJwtVcPayload
        {
            Issuer = "https://security.gov",
            Subject = "did:example:michael456",
            IssuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            ExpiresAt = DateTimeOffset.UtcNow.AddYears(5).ToUnixTimeSeconds(),
            AdditionalData = new Dictionary<string, object>
            {
                ["holder_name"] = "Michael Rodriguez",
                ["clearance_level"] = "Secret",
                ["investigation_type"] = "SSBI",
                ["adjudication_date"] = "2023-08-15",
                ["security_office"] = "Defense Security Service",
                ["background_check_complete"] = true,
                ["polygraph_required"] = false,
                ["foreign_contacts_disclosed"] = true
            }
        };

        var securityOptions = new SdIssuanceOptions
        {
            DisclosureStructure = new
            {
                investigation_type = true,
                adjudication_date = true,
                polygraph_required = true,
                foreign_contacts_disclosed = true
            }
        };

        var securityCredential = govIssuer.Issue(
            "https://credentials.security.gov/clearance",
            securityClearancePayload,
            securityOptions,
            michaelJwk
        );

        Console.WriteLine("[X] Government issued security clearance credential");
        Console.WriteLine($"  - Clearance Level: {securityClearancePayload.AdditionalData["clearance_level"]}");
        Console.WriteLine($"  - Investigation: {securityClearancePayload.AdditionalData["investigation_type"]}");

        // Step 2: University issues degree credential
        Console.WriteLine("\n--- Step 2: University Issues Degree Credential ---");
        var universityIssuer = new SdJwtVcIssuer(universityKey, SecurityAlgorithms.EcdsaSha256);

        var degreePayload = new SdJwtVcPayload
        {
            Issuer = "https://registrar.mit.edu",
            Subject = "did:example:michael456",
            IssuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            AdditionalData = new Dictionary<string, object>
            {
                ["graduate_name"] = "Michael Rodriguez",
                ["degree"] = "Master of Science",
                ["major"] = "Aerospace Engineering",
                ["graduation_date"] = "2018-05-15",
                ["gpa"] = 3.9,
                ["thesis_title"] = "Advanced Propulsion Systems for Satellite Applications",
                ["advisor"] = "Dr. Sarah Chen",
                ["honors"] = "summa cum laude"
            }
        };

        var degreeOptions = new SdIssuanceOptions
        {
            DisclosureStructure = new
            {
                gpa = true,
                thesis_title = true,
                advisor = true,
                honors = true
            }
        };

        var degreeCredential = universityIssuer.Issue(
            "https://credentials.mit.edu/masters-degree",
            degreePayload,
            degreeOptions,
            michaelJwk
        );

        Console.WriteLine("[X] MIT issued masters degree credential");
        Console.WriteLine($"  - Degree: {degreePayload.AdditionalData["degree"]}");
        Console.WriteLine($"  - Major: {degreePayload.AdditionalData["major"]}");

        // Step 3: Create presentations and verify
        Console.WriteLine("\n--- Step 3: Michael Creates Multi-Credential Presentation ---");
        var applicationNonce = "defense-job-application-2024-67890";
        var defenseAudience = "https://defense-contractor.example.com";

        var securityHolder = new SdJwtHolder(securityCredential.Issuance);
        var securityPresentation = securityHolder.CreatePresentation(
            disclosure => disclosure.ClaimName == "investigation_type",
            new JwtPayload
            {
                [JwtRegisteredClaimNames.Aud] = defenseAudience,
                [JwtRegisteredClaimNames.Iat] = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                ["nonce"] = applicationNonce,
                ["purpose"] = "security_clearance_verification"
            },
            michaelPrivateKey,
            SecurityAlgorithms.EcdsaSha256
        );

        var degreeHolder = new SdJwtHolder(degreeCredential.Issuance);
        var degreePresentation = degreeHolder.CreatePresentation(
            disclosure => disclosure.ClaimName == "honors",
            new JwtPayload
            {
                [JwtRegisteredClaimNames.Aud] = defenseAudience,
                [JwtRegisteredClaimNames.Iat] = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                ["nonce"] = applicationNonce,
                ["purpose"] = "education_verification"
            },
            michaelPrivateKey,
            SecurityAlgorithms.EcdsaSha256
        );

        Console.WriteLine("[X] Michael created comprehensive presentation with selective disclosure:");
        Console.WriteLine("  - Security clearance: Disclosed investigation type");
        Console.WriteLine("  - Education: Disclosed academic honors only");

        // Step 4: Defense contractor verifies credentials
        Console.WriteLine("\n--- Step 4: Defense Contractor Verifies All Credentials ---");
        
        var defenseVerifier = new SdVerifier(jwt =>
        {
            var issuerFromJwt = jwt.Payload.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Iss)?.Value ?? "";
            logger.LogInformation("Defense contractor resolving issuer key for: {Issuer}", issuerFromJwt);
            return Task.FromResult<SecurityKey>(issuerFromJwt switch
            {
                "https://security.gov" => govKey,
                "https://registrar.mit.edu" => universityKey,
                _ => throw new InvalidOperationException($"Unknown issuer: {issuerFromJwt}")
            });
        });

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuers = new[] { "https://security.gov", "https://registrar.mit.edu" },
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(5)
        };

        var keyBindingValidation = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = true,
            ValidAudience = defenseAudience,
            ValidateLifetime = false,
            IssuerSigningKey = michaelPublicKey,
            ClockSkew = TimeSpan.FromMinutes(5)
        };

        var securityResult = await defenseVerifier.VerifyAsync(securityPresentation, validationParameters, keyBindingValidation);
        Console.WriteLine("[X] Security clearance verified: Secret level with SSBI investigation");

        var degreeResult = await defenseVerifier.VerifyAsync(degreePresentation, validationParameters, keyBindingValidation);
        Console.WriteLine("[X] Education verified: MIT MS Aerospace Engineering with honors");
        
        Console.WriteLine("\n--- Step 5: Hiring Decision ---");
        Console.WriteLine("Defense contractor automated hiring system:");
        Console.WriteLine("[X] Security clearance: SECRET level confirmed");
        Console.WriteLine("[X] Education: Advanced degree in relevant field");
        Console.WriteLine("[X] Cryptographic verification: All credentials authentic");
        Console.WriteLine();
        Console.WriteLine("JOB APPLICATION APPROVED - Senior Defense Systems Engineer");
        Console.WriteLine("   Security Level: SECRET projects authorized");
        Console.WriteLine("   Start Date: Available immediately");

        Console.WriteLine("\nScenario 2 completed successfully!");
        Console.WriteLine("Key features demonstrated:");
        Console.WriteLine("• Multi-credential background verification");
        Console.WriteLine("• Security clearance validation with selective disclosure");
        Console.WriteLine("• Educational qualification verification");
        Console.WriteLine("• Privacy protection for sensitive investigation details");
    }

    private static async Task DemonstrateMedicalRecordSharingAsync(IServiceProvider services)
    {
        var logger = services.GetRequiredService<ILogger<RealWorldScenarios>>();
        
        Console.WriteLine("\n" + new string('=', 64));
        Console.WriteLine("SCENARIO 3: Medical Record Sharing with Patient Consent");
        Console.WriteLine(new string('=', 64));
        Console.WriteLine("Story: Dr. Johnson needs to share patient Alice's medical records");
        Console.WriteLine("       with a specialist. Privacy regulations require explicit");
        Console.WriteLine("       consent and minimal data disclosure.");

        // Setup keys
        using var hospitalEcdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        using var aliceEcdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);

        var hospitalKey = new ECDsaSecurityKey(hospitalEcdsa) { KeyId = "general-hospital-2024" };
        var alicePrivateKey = new ECDsaSecurityKey(aliceEcdsa) { KeyId = "alice-patient-key" };
        var alicePublicKey = new ECDsaSecurityKey(aliceEcdsa) { KeyId = "alice-patient-key" };
        var aliceJwk = JsonWebKeyConverter.ConvertFromSecurityKey(alicePublicKey);

        // Hospital creates medical summary credential
        Console.WriteLine("\n--- Step 1: Dr. Johnson Creates Medical Summary Credential ---");
        var hospitalIssuer = new SdJwtVcIssuer(hospitalKey, SecurityAlgorithms.EcdsaSha256);

        var medicalSummaryPayload = new SdJwtVcPayload
        {
            Issuer = "https://general-hospital.example.com",
            Subject = "did:example:alice-patient",
            IssuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            ExpiresAt = DateTimeOffset.UtcNow.AddMonths(6).ToUnixTimeSeconds(),
            AdditionalData = new Dictionary<string, object>
            {
                ["patient_name"] = "Alice Chen",
                ["patient_id"] = "PAT-12345",
                ["primary_complaint"] = "Chronic fatigue and joint pain",
                ["diagnosis"] = "Autoimmune condition - requires specialist evaluation",
                ["allergies"] = new[] { "Penicillin", "Shellfish" },
                ["current_medications"] = new[] { "Ibuprofen 400mg", "Vitamin D 1000IU" },
                ["test_results"] = new { inflammatory_markers = "high", rheumatoid_factor = "positive" },
                ["social_security_number"] = "123-45-6789",
                ["insurance_policy"] = "Premium Health Plan - Policy INS-456789"
            }
        };

        var medicalOptions = new SdIssuanceOptions
        {
            DisclosureStructure = new
            {
                allergies = true,
                current_medications = true,
                test_results = true,
                social_security_number = true,
                insurance_policy = true
            }
        };

        var medicalCredential = hospitalIssuer.Issue(
            "https://credentials.general-hospital.com/medical-summary",
            medicalSummaryPayload,
            medicalOptions,
            aliceJwk
        );

        Console.WriteLine("[X] Dr. Johnson created medical summary credential for Alice");
        Console.WriteLine($"  - Primary complaint: {medicalSummaryPayload.AdditionalData["primary_complaint"]}");
        Console.WriteLine($"  - Diagnosis: {medicalSummaryPayload.AdditionalData["diagnosis"]}");
        Console.WriteLine("  - Sensitive data protected by selective disclosure");

        // Alice creates selective presentation for specialist
        Console.WriteLine("\n--- Step 2: Alice Provides Consent for Specialist Sharing ---");
        var specialistAudience = "https://rheumatology-specialist.example.com";
        var consultNonce = "specialist-consultation-2024";

        Console.WriteLine("Alice's consent choices:");
        Console.WriteLine("  SHARE: Primary complaint, diagnosis, allergies, medications, test results");
        Console.WriteLine("  PROTECT: SSN, insurance details, patient ID");

        var medicalHolder = new SdJwtHolder(medicalCredential.Issuance);
        var medicalPresentation = medicalHolder.CreatePresentation(
            disclosure => disclosure.ClaimName == "allergies" || 
                         disclosure.ClaimName == "current_medications" || 
                         disclosure.ClaimName == "test_results",
            new JwtPayload
            {
                [JwtRegisteredClaimNames.Aud] = specialistAudience,
                [JwtRegisteredClaimNames.Iat] = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                ["nonce"] = consultNonce,
                ["purpose"] = "specialist_consultation",
                ["patient_consent"] = "explicit_consent_for_rheumatology"
            },
            alicePrivateKey,
            SecurityAlgorithms.EcdsaSha256
        );

        Console.WriteLine("[X] Alice created selective medical presentation with explicit consent");

        // Specialist verification
        Console.WriteLine("\n--- Step 3: Specialist Verifies Medical Data with Privacy Protection ---");
        
        var specialistVerifier = new SdVerifier(jwt =>
        {
            var issuerFromJwt = jwt.Payload.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Iss)?.Value ?? "";
            logger.LogInformation("Specialist resolving issuer key for: {Issuer}", issuerFromJwt);
            return Task.FromResult<SecurityKey>(issuerFromJwt switch
            {
                "https://general-hospital.example.com" => hospitalKey,
                _ => throw new InvalidOperationException($"Unknown issuer: {issuerFromJwt}")
            });
        });

        var validationParams = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuers = new[] { "https://general-hospital.example.com" },
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(5)
        };

        var keyBindingParams = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = true,
            ValidAudience = specialistAudience,
            ValidateLifetime = false,
            IssuerSigningKey = alicePublicKey,
            ClockSkew = TimeSpan.FromMinutes(5)
        };

        var medicalResult = await specialistVerifier.VerifyAsync(medicalPresentation, validationParams, keyBindingParams);

        Console.WriteLine("[X] Specialist verification completed successfully:");
        Console.WriteLine($"  - Patient: {medicalResult.ClaimsPrincipal.FindFirst("patient_name")?.Value}");
        Console.WriteLine($"  - Primary complaint: {medicalResult.ClaimsPrincipal.FindFirst("primary_complaint")?.Value}");
        Console.WriteLine($"  - Diagnosis: {medicalResult.ClaimsPrincipal.FindFirst("diagnosis")?.Value}");
        Console.WriteLine("  - Shared with consent: Allergies, medications, test results");
        Console.WriteLine("  - Protected data: SSN and insurance details not disclosed");
        Console.WriteLine($"  - Cryptographic verification: Key binding verified = {medicalResult.KeyBindingVerified}");

        // Audit trail for compliance
        Console.WriteLine("\n--- Step 4: HIPAA Compliance Audit Trail ---");
        var auditRecord = new
        {
            Timestamp = DateTimeOffset.UtcNow,
            Patient = "Alice Chen (ID: PAT-12345)",
            RequestingProvider = "Dr. Emily Johnson, General Hospital",
            ReceivingProvider = "Rheumatology Specialist",
            Purpose = "Specialist consultation for autoimmune evaluation",
            DataShared = new[] { "Primary complaint", "Diagnosis", "Allergies", "Medications", "Test results" },
            DataProtected = new[] { "SSN", "Insurance details", "Patient ID" },
            PatientConsent = "Explicit consent provided for rheumatology consultation",
            VerificationMethod = "Cryptographic SD-JWT verification with key binding",
            ComplianceFramework = "HIPAA Privacy Rule - Minimum Necessary Standard"
        };

        Console.WriteLine("[X] HIPAA compliance audit record created:");
        Console.WriteLine($"  - Access time: {auditRecord.Timestamp:yyyy-MM-dd HH:mm:ss UTC}");
        Console.WriteLine($"  - Patient consent: {auditRecord.PatientConsent}");
        Console.WriteLine($"  - Data shared: {auditRecord.DataShared.Length} medical fields");
        Console.WriteLine($"  - Data protected: {auditRecord.DataProtected.Length} sensitive fields");
        Console.WriteLine($"  - Verification: {auditRecord.VerificationMethod}");
        Console.WriteLine($"  - Compliance: {auditRecord.ComplianceFramework}");

        Console.WriteLine("\nScenario 3 completed successfully!");
        Console.WriteLine("Key features demonstrated:");
        Console.WriteLine("• Patient-controlled selective medical data sharing");
        Console.WriteLine("• Healthcare provider credential verification");
        Console.WriteLine("• HIPAA-compliant audit trails and consent management");
        Console.WriteLine("• Cryptographic verification with key binding");
        Console.WriteLine("• Minimal data disclosure with explicit patient consent");
    }

    private static async Task DemonstrateGovernmentServiceAccessAsync(IServiceProvider services)
    {
        var logger = services.GetRequiredService<ILogger<RealWorldScenarios>>();
        
        Console.WriteLine("\n" + new string('=', 64));
        Console.WriteLine("SCENARIO 4: Digital Government Service Access");
        Console.WriteLine(new string('=', 64));
        Console.WriteLine("Story: Citizens use digital identity credentials to access");
        Console.WriteLine("       various government services with selective disclosure.");

        // Setup keys
        using var dmvEcdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        using var johnEcdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);

        var dmvKey = new ECDsaSecurityKey(dmvEcdsa) { KeyId = "state-dmv-2024" };
        var johnPrivateKey = new ECDsaSecurityKey(johnEcdsa) { KeyId = "john-citizen-key" };
        var johnPublicKey = new ECDsaSecurityKey(johnEcdsa) { KeyId = "john-citizen-key" };
        var johnJwk = JsonWebKeyConverter.ConvertFromSecurityKey(johnPublicKey);

        // DMV issues digital driver's license
        Console.WriteLine("\n--- Step 1: DMV Issues Digital Driver's License ---");
        var dmvIssuer = new SdJwtVcIssuer(dmvKey, SecurityAlgorithms.EcdsaSha256);

        var driversLicensePayload = new SdJwtVcPayload
        {
            Issuer = "https://dmv.state.gov",
            Subject = "did:example:john-citizen",
            IssuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            ExpiresAt = DateTimeOffset.UtcNow.AddYears(8).ToUnixTimeSeconds(),
            AdditionalData = new Dictionary<string, object>
            {
                ["full_name"] = "John William Smith",
                ["license_number"] = "DL-9876543210",
                ["date_of_birth"] = "1980-07-15",
                ["address"] = "123 Main Street, Springfield, CA 90210",
                ["license_class"] = "Class C",
                ["age_over_18"] = true,
                ["age_over_21"] = true,
                ["age_over_65"] = false,
                ["veteran_status"] = true
            }
        };

        var driversLicenseOptions = new SdIssuanceOptions
        {
            DisclosureStructure = new
            {
                license_number = true,
                date_of_birth = true,
                address = true,
                age_over_18 = true,
                age_over_21 = true,
                age_over_65 = true,
                veteran_status = true
            }
        };

        var driversLicenseCredential = dmvIssuer.Issue(
            "https://credentials.dmv.state.gov/drivers-license",
            driversLicensePayload,
            driversLicenseOptions,
            johnJwk
        );

        Console.WriteLine("[X] DMV issued digital driver's license to John");
        Console.WriteLine($"  - License holder: {driversLicensePayload.AdditionalData["full_name"]}");
        Console.WriteLine($"  - License class: {driversLicensePayload.AdditionalData["license_class"]}");
        Console.WriteLine("  - Selective disclosure available for age verification and veteran status");

        // Demonstrate service access with age verification
        Console.WriteLine("\n--- Step 2: Age Verification for Online Government Services ---");
        var ageVerificationAudience = "https://restricted-service.gov";
        var ageVerificationNonce = "age-check-2024";

        Console.WriteLine("Government service requirements:");
        Console.WriteLine("  - Age verification (21 or older) for restricted service access");
        Console.WriteLine("  - Privacy protection (exact birth date not needed)");

        var driversLicenseHolder = new SdJwtHolder(driversLicenseCredential.Issuance);
        var ageVerificationPresentation = driversLicenseHolder.CreatePresentation(
            disclosure => disclosure.ClaimName == "age_over_21",
            new JwtPayload
            {
                [JwtRegisteredClaimNames.Aud] = ageVerificationAudience,
                [JwtRegisteredClaimNames.Iat] = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                ["nonce"] = ageVerificationNonce,
                ["purpose"] = "age_verification"
            },
            johnPrivateKey,
            SecurityAlgorithms.EcdsaSha256
        );

        var serviceVerifier = new SdVerifier(jwt =>
        {
            var issuerFromJwt = jwt.Payload.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Iss)?.Value ?? "";
            logger.LogInformation("Service resolving issuer key for: {Issuer}", issuerFromJwt);
            return Task.FromResult<SecurityKey>(issuerFromJwt switch
            {
                "https://dmv.state.gov" => dmvKey,
                _ => throw new InvalidOperationException($"Unknown issuer: {issuerFromJwt}")
            });
        });

        var serviceValidationParams = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuers = new[] { "https://dmv.state.gov" },
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(5)
        };

        var serviceKeyBindingParams = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = true,
            ValidAudience = ageVerificationAudience,
            ValidateLifetime = false,
            IssuerSigningKey = johnPublicKey,
            ClockSkew = TimeSpan.FromMinutes(5)
        };

        var ageResult = await serviceVerifier.VerifyAsync(ageVerificationPresentation, serviceValidationParams, serviceKeyBindingParams);

        Console.WriteLine("[X] Age verification completed successfully:");
        Console.WriteLine($"  - Citizen: {ageResult.ClaimsPrincipal.FindFirst("full_name")?.Value}");
        Console.WriteLine("  - Age verification: Over 21 confirmed (selectively disclosed)");
        Console.WriteLine("  - Privacy protected: Exact birth date not disclosed");
        Console.WriteLine("  - Service access: GRANTED to age-restricted government service");

        // Demonstrate veteran services access
        Console.WriteLine("\n--- Step 3: Veteran Services Access ---");
        var veteranAudience = "https://va.gov";
        var veteranNonce = "veteran-services-2024";

        Console.WriteLine("VA service requirements:");
        Console.WriteLine("  - Veteran status verification for benefits access");
        Console.WriteLine("  - Identity verification with privacy protection");

        var veteranPresentation = driversLicenseHolder.CreatePresentation(
            disclosure => disclosure.ClaimName == "veteran_status",
            new JwtPayload
            {
                [JwtRegisteredClaimNames.Aud] = veteranAudience,
                [JwtRegisteredClaimNames.Iat] = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                ["nonce"] = veteranNonce,
                ["purpose"] = "veteran_status_verification"
            },
            johnPrivateKey,
            SecurityAlgorithms.EcdsaSha256
        );

        var vaValidationParams = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuers = new[] { "https://dmv.state.gov" },
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(5)
        };

        var vaKeyBindingParams = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = true,
            ValidAudience = veteranAudience,
            ValidateLifetime = false,
            IssuerSigningKey = johnPublicKey,
            ClockSkew = TimeSpan.FromMinutes(5)
        };

        var veteranResult = await serviceVerifier.VerifyAsync(veteranPresentation, vaValidationParams, vaKeyBindingParams);

        Console.WriteLine("[X] Veteran services access completed:");
        Console.WriteLine($"  - Citizen: {veteranResult.ClaimsPrincipal.FindFirst("full_name")?.Value}");
        Console.WriteLine("  - Veteran status: CONFIRMED (selectively disclosed)");
        Console.WriteLine("  - Full VA services portfolio: AVAILABLE");
        Console.WriteLine("  - Privacy protected: Address and license number not shared");

        // Cross-service federation summary
        Console.WriteLine("\n--- Step 4: Cross-Service Digital Identity Summary ---");
        Console.WriteLine("[X] Single digital identity credential used across multiple services:");
        Console.WriteLine("    - Age-restricted service: Age verification only");
        Console.WriteLine("    - VA services: Veteran status confirmation");
        Console.WriteLine();
        Console.WriteLine("[X] Privacy protection maintained across all interactions:");
        Console.WriteLine("    - Exact birth date: PROTECTED (only age assertions shared)");
        Console.WriteLine("    - Home address: PROTECTED (not shared with any service)");
        Console.WriteLine("    - License number: PROTECTED (not needed for service access)");
        Console.WriteLine();
        Console.WriteLine("[X] Enhanced security and user experience:");
        Console.WriteLine("    - Single sign-on with government services");
        Console.WriteLine("    - Cryptographic proof of identity");
        Console.WriteLine("    - User-controlled data sharing");

        Console.WriteLine("\nScenario 4 completed successfully!");
        Console.WriteLine("Key features demonstrated:");
        Console.WriteLine("• Single digital identity for multiple government services");
        Console.WriteLine("• Selective disclosure based on specific service requirements");
        Console.WriteLine("• Privacy-preserving age and status verification");
        Console.WriteLine("• Interoperable credential system across agencies");
        Console.WriteLine("• Enhanced citizen experience with privacy protection");

        Console.WriteLine("\n" + new string('=', 63));
        Console.WriteLine("ALL SCENARIOS COMPLETED SUCCESSFULLY!");
        Console.WriteLine(new string('=', 63));
        Console.WriteLine();
        Console.WriteLine("The SD-JWT .NET ecosystem provides comprehensive support for:");
        Console.WriteLine();
        Console.WriteLine("EDUCATION & CREDENTIALS");
        Console.WriteLine("   • University degree verification with selective disclosure");
        Console.WriteLine("   • Professional license management and validation");
        Console.WriteLine("   • Academic achievement privacy protection");
        Console.WriteLine();
        Console.WriteLine("EMPLOYMENT & FINANCE");
        Console.WriteLine("   • Employment verification for loan applications");
        Console.WriteLine("   • Background checks for security clearance positions");
        Console.WriteLine("   • Salary and performance data privacy protection");
        Console.WriteLine();
        Console.WriteLine("HEALTHCARE & PRIVACY");
        Console.WriteLine("   • Patient-controlled medical data sharing");
        Console.WriteLine("   • Healthcare provider credential verification");
        Console.WriteLine("   • HIPAA-compliant audit trails and consent management");
        Console.WriteLine();
        Console.WriteLine("GOVERNMENT & IDENTITY");
        Console.WriteLine("   • Digital identity for seamless government service access");
        Console.WriteLine("   • Cross-agency credential interoperability");
        Console.WriteLine("   • Privacy-preserving age and status verification");
        Console.WriteLine();
        Console.WriteLine("SECURITY & TRUST");
        Console.WriteLine("   • End-to-end cryptographic verification");
        Console.WriteLine("   • Tamper-resistant credential integrity");
        Console.WriteLine("   • User-controlled selective disclosure");
        Console.WriteLine("   • Production-ready security implementations");
    }
}
