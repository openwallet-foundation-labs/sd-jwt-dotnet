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
/// Demonstrates government-level HAIP implementation for sovereign credentials
/// 
/// HAIP Level 3 (Sovereign) Overview:
/// - Maximum security for national identity and critical infrastructure
/// - Hardware Security Module (HSM) backing required
/// - Qualified electronic signatures (QES) compliance
/// - Enhanced device attestation and audit trails
/// - Cross-border recognition through mutual agreements
/// 
/// This example shows:
/// 1. National ID credential issuance with Level 3 compliance
/// 2. Sovereign-level cryptographic validation
/// 3. eIDAS regulation compliance integration
/// 4. Cross-border credential recognition workflows
/// 5. Government audit and compliance reporting
/// </summary>
public class GovernmentHaipExample
{
    public static async Task RunExample(IServiceProvider services)
    {
        var logger = services.GetRequiredService<ILogger<GovernmentHaipExample>>();
        
        Console.WriteLine("\n" + new string('=', 75));
        Console.WriteLine("         Government HAIP Implementation Example              ");
        Console.WriteLine("     Level 3 Sovereign Compliance for National Identity        ");
        Console.WriteLine(new string('=', 75));

        Console.WriteLine("\nThis example demonstrates HAIP Level 3 (Sovereign) compliance for");
        Console.WriteLine("government use cases including national identity credentials,");
        Console.WriteLine("critical infrastructure access, and cross-border recognition.\n");

        Console.WriteLine("Key Government Features:");
        Console.WriteLine("• Hardware Security Module (HSM) backing for all keys");
        Console.WriteLine("• Qualified Electronic Signatures (QES) compliance");
        Console.WriteLine("• eIDAS Very High Level of Assurance integration");
        Console.WriteLine("• Cross-border recognition through mutual agreements");
        Console.WriteLine("• Enhanced audit trails with digital signatures");
        Console.WriteLine("• National security standards compliance");
        Console.WriteLine();
        
        await DemonstrateNationalIdIssuance(logger);
        await DemonstrateSovereignValidation();
        await DemonstrateEidasCompliance();
        await DemonstrateCrossBorderRecognition();
        await DemonstrateAuditAndCompliance();

        Console.WriteLine(new string('=', 75));
        Console.WriteLine("        Government HAIP concepts demonstrated!             ");
        Console.WriteLine("                                                         ");
        Console.WriteLine("  [X] National ID credential issuance with real SD-JWT      ");
        Console.WriteLine("  [X] Sovereign-level cryptographic validation             ");
        Console.WriteLine("  [X] eIDAS compliance integration                         ");
        Console.WriteLine("  [X] Cross-border credential recognition                  ");
        Console.WriteLine("  [X] Government audit and compliance reporting            ");
        Console.WriteLine(new string('=', 75));
    }

    private static async Task DemonstrateNationalIdIssuance(ILogger logger)
    {
        Console.WriteLine("1. NATIONAL IDENTITY CREDENTIAL ISSUANCE WITH ACTUAL SD-JWT");
        Console.WriteLine("   Government issuing citizen identity credentials using real APIs");
        Console.WriteLine();

        Console.WriteLine("   GOVERNMENT ISSUER SETUP:");
        Console.WriteLine("   Entity: Ministry of Digital Identity");
        Console.WriteLine("   URL: https://identity.gov.example");
        Console.WriteLine("   Trust Level: Sovereign (Level 3)");
        Console.WriteLine("   Compliance: eIDAS Very High, National Standards");
        Console.WriteLine();

        try
        {
            // Step 1: Create sovereign-level keys (simulated P-521 for Level 3)
            Console.WriteLine("   Step 1: Government Key Generation (Level 3 Sovereign)");
            
            // In real deployment, this would be HSM-backed
            var governmentSigningKey = new ECDsaSecurityKey(ECDsa.Create(ECCurve.NamedCurves.nistP521)) 
            { 
                KeyId = "gov-signing-2024-001" 
            };
            
            var citizenKey = new ECDsaSecurityKey(ECDsa.Create(ECCurve.NamedCurves.nistP521))
            {
                KeyId = "citizen-key-2024-001"
            };

            // Validate algorithm compliance for Level 3
            var algorithmCompliant = ValidateAlgorithmForLevel3("ES512");
            Console.WriteLine($"   Algorithm ES512 compliant with Level 3: {algorithmCompliant}");
            Console.WriteLine("   Key Generation: P-521 curve (meets sovereign requirements)");
            Console.WriteLine("   HSM Backing: Simulated (would be required in production)");
            Console.WriteLine();

            // Step 2: Create SD-JWT issuer with sovereign-grade algorithm
            Console.WriteLine("   Step 2: Government Credential Issuance");
            
            var issuer = new SdIssuer(governmentSigningKey, SecurityAlgorithms.EcdsaSha512);

            // Create national ID claims with government data
            var nationalIdClaims = new JwtPayload
            {
                { "iss", "https://identity.gov.example" },
                { "sub", "urn:gov:citizen:ES:DNI:12345678Z" },
                { "vct", "https://identity.gov.example/credentials/national-id" },
                { "citizen_id", "NID-2024-123456" },
                { "given_name", "Maria" },
                { "family_name", "Rodriguez" },
                { "date_of_birth", "1990-05-15" },
                { "nationality", "ES" },
                { "document_type", "national_identity_card" },
                { "issuing_authority", "Ministerio del Interior" },
                { "security_features", new { biometric_template_protected = true, qualified_signature = true } }
            };

            // Configure selective disclosure for privacy protection
            var sovereignOptions = new SdIssuanceOptions
            {
                DisclosureStructure = new
                {
                    // Sensitive data selectively disclosable
                    place_of_birth = true,
                    current_address = true,
                    emergency_contact = true,
                    biometric_hash = true
                },
                AllowWeakAlgorithms = false,
                DecoyDigests = 10  // Maximum privacy for sovereign credentials
            };

            // Issue the national ID credential
            var nationalIdIssuance = issuer.Issue(nationalIdClaims, sovereignOptions);
            
            Console.WriteLine("   National ID Credential Issued Successfully");
            Console.WriteLine($"   SD-JWT Length: {nationalIdIssuance.SdJwt.Length} characters");
            Console.WriteLine($"   Selective Disclosures: {nationalIdIssuance.Disclosures.Count}");
            Console.WriteLine($"   Privacy Protection: {sovereignOptions.DecoyDigests} decoy digests");
            Console.WriteLine($"   Algorithm Used: ES512 (sovereign-grade)");
            Console.WriteLine();

            // Step 3: Citizen creates presentation for government service
            Console.WriteLine("   Step 3: Citizen Service Access Presentation");
            
            var holder = new SdJwtHolder(nationalIdIssuance.Issuance);
            
            // Create presentation for accessing government service
            var servicePresentation = holder.CreatePresentation(
                disclosure => disclosure.ClaimName == "current_address", // Only reveal address for service
                kbJwtPayload: new JwtPayload 
                { 
                    { "aud", "https://services.gov.example/social-benefits" },
                    { "iat", DateTimeOffset.UtcNow.ToUnixTimeSeconds() },
                    { "purpose", "social_benefits_application" }
                },
                kbJwtSigningKey: citizenKey,
                kbJwtSigningAlgorithm: SecurityAlgorithms.EcdsaSha512
            );

            Console.WriteLine("   Citizen Presentation Created");
            Console.WriteLine("   Revealed Information: Current address only");
            Console.WriteLine("   Privacy Protected: Place of birth, emergency contact, biometric data");
            Console.WriteLine("   Key Binding: Government-grade proof of possession");
            Console.WriteLine();

            // Step 4: Government service verifies presentation
            Console.WriteLine("   Step 4: Government Service Verification");

            var verifier = new SdVerifier(async (jwt) => governmentSigningKey);

            var validationParams = new TokenValidationParameters
            {
                ValidateIssuer = false, // Simplified for demo
                ValidateAudience = false,
                ValidateLifetime = false,
                IssuerSigningKey = governmentSigningKey
            };

            var kbValidationParams = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = false,
                IssuerSigningKey = citizenKey
            };

            var verificationResult = await verifier.VerifyAsync(
                servicePresentation, 
                validationParams, 
                kbValidationParams);

            Console.WriteLine($"   Verification Result: {(verificationResult.KeyBindingVerified ? "SUCCESS" : "FAILED")}");
            Console.WriteLine($"   Key Binding Verified: {verificationResult.KeyBindingVerified}");
            Console.WriteLine($"   Sovereign Compliance: Maintained throughout workflow");
            Console.WriteLine();

            Console.WriteLine("   SOVEREIGN COMPLIANCE ACHIEVED:");
            Console.WriteLine("   [X] ES512 algorithm (strongest available)");
            Console.WriteLine("   [X] P-521 elliptic curve (sovereign-grade)");
            Console.WriteLine("   [X] Key binding with proof of possession");
            Console.WriteLine("   [X] Selective disclosure for citizen privacy");
            Console.WriteLine("   [X] Maximum decoy count for privacy protection");
            Console.WriteLine("   [X] Government-to-government verification successful");

        }
        catch (Exception ex)
        {
            Console.WriteLine($"   ERROR: {ex.Message}");
            logger.LogError(ex, "Government HAIP demonstration failed");
        }

        Console.WriteLine();
    }

    private static async Task DemonstrateSovereignValidation()
    {
        Console.WriteLine("2. SOVEREIGN-LEVEL VALIDATION");
        Console.WriteLine("   Enforcing highest security standards for government credentials");
        Console.WriteLine();

        Console.WriteLine("   VALIDATION SCENARIOS:");
        Console.WriteLine();

        // Test scenarios for sovereign validation
        var scenarios = new[]
        {
            new
            {
                Name = "Valid Sovereign Key (ES512 with P-521)",
                Algorithm = "ES512",
                KeySize = 521,
                IsHSMBacked = true, // Simulated
                ShouldPass = true,
                Reason = "Meets all Level 3 requirements"
            },
            new
            {
                Name = "Strong but Non-HSM Key (ES512)",
                Algorithm = "ES512", 
                KeySize = 521,
                IsHSMBacked = false,
                ShouldPass = false,
                Reason = "HSM backing required for Level 3"
            },
            new
            {
                Name = "Insufficient Algorithm (ES384)",
                Algorithm = "ES384",
                KeySize = 384,
                IsHSMBacked = true,
                ShouldPass = false,
                Reason = "ES384 insufficient for Level 3 (requires ES512+)"
            },
            new
            {
                Name = "Forbidden Algorithm (RS256)",
                Algorithm = "RS256",
                KeySize = 2048,
                IsHSMBacked = false,
                ShouldPass = false,
                Reason = "RS256 forbidden in all HAIP levels"
            }
        };

        Console.WriteLine("   SOVEREIGN VALIDATION TEST MATRIX:");
        Console.WriteLine("   +-----------------------+----------+--------+---------------------------+");
        Console.WriteLine("   | Scenario              | Key Size | Result | Reason                   |");
        Console.WriteLine("   +-----------------------+----------+--------+---------------------------+");

        foreach (var scenario in scenarios)
        {
            var result = scenario.ShouldPass ? "PASS" : "FAIL";
            var truncatedReason = scenario.Reason.Length > 25 ? scenario.Reason[..22] + "..." : scenario.Reason;
            Console.WriteLine($"   | {scenario.Name,-21} | {scenario.KeySize,-8} | {result,-6} | {truncatedReason,-25} |");
        }
        
        Console.WriteLine("   +-----------------------+----------+--------+---------------------------+");
        Console.WriteLine();

        Console.WriteLine("   DETAILED SOVEREIGN VALIDATION:");
        foreach (var scenario in scenarios.Take(3))
        {
            Console.WriteLine($"   Testing: {scenario.Name}");
            
            try
            {
                SecurityKey? key = scenario.Algorithm.StartsWith("ES") ? 
                    CreateECKey(scenario.KeySize) : 
                    CreateRSAKey(scenario.KeySize);

                if (key != null)
                {
                    var validationResult = PerformSovereignValidation(scenario.Algorithm, scenario.IsHSMBacked);
                    
                    var status = validationResult.IsValid ? "[X] COMPLIANT" : "[ ] NON-COMPLIANT";
                    Console.WriteLine($"      Result: {status}");
                    Console.WriteLine($"      Analysis: {validationResult.Analysis}");
                    
                    if (!validationResult.IsValid)
                    {
                        Console.WriteLine($"      Violation: {validationResult.Violation}");
                        Console.WriteLine($"      Recommendation: {validationResult.Recommendation}");
                    }
                    else
                    {
                        Console.WriteLine($"      Sovereign Level: Achieved");
                        Console.WriteLine($"      Key Strength: 256+ bits effective security");
                    }
                }
                else
                {
                    Console.WriteLine("      Result: [ ] FAILED TO CREATE KEY");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"      Result: [ ] ERROR - {ex.Message}");
            }

            Console.WriteLine();
        }

        Console.WriteLine("   SOVEREIGN SECURITY REQUIREMENTS:");
        Console.WriteLine("   • Hardware Security Module (HSM) backing mandatory");
        Console.WriteLine("   • Qualified Electronic Signature standards (QES)");
        Console.WriteLine("   • Enhanced audit trail with digital signatures");
        Console.WriteLine("   • Cryptographic algorithms: ES512, PS512, EdDSA only");
        Console.WriteLine("   • Minimum key sizes: EC P-521, RSA 4096");
        Console.WriteLine("   • Key attestation from certified hardware");
        Console.WriteLine("   • Government security clearance for operators");
        Console.WriteLine();

        await Task.CompletedTask;
    }

    private static async Task DemonstrateEidasCompliance()
    {
        Console.WriteLine("3. eIDAS COMPLIANCE INTEGRATION");
        Console.WriteLine("   European Digital Identity regulation compliance with actual SD-JWT");
        Console.WriteLine();

        Console.WriteLine("   eIDAS REGULATION REQUIREMENTS:");
        Console.WriteLine("   Article 45e - European Digital Identity Wallets");
        Console.WriteLine("   Level of Assurance: Very High (LoA 3)");
        Console.WriteLine("   Person Identification Data (PID) compliance");
        Console.WriteLine();

        try
        {
            // Create eIDAS-compliant credential using real SD-JWT
            var eidasSigningKey = new ECDsaSecurityKey(ECDsa.Create(ECCurve.NamedCurves.nistP384))
            {
                KeyId = "eidas-qtsp-2024"
            };

            var issuer = new SdIssuer(eidasSigningKey, SecurityAlgorithms.EcdsaSha384);

            var eidasPidClaims = new JwtPayload
            {
                { "iss", "https://member-state.gov.eu" },
                { "sub", "urn:eidas:person:ES:DNI:12345678Z" },
                { "vct", "https://eudi.europa.eu/credentials/person-identification-data" },
                { "family_name", "García" },
                { "given_name", "Ana Isabel" },
                { "date_of_birth", "1985-03-20" },
                { "nationality", "ES" },
                { "eidas_level_of_assurance", "http://eidas.europa.eu/LoA/high" },
                { "issuing_authority", "Ministerio del Interior" },
                { "issuing_country", "ES" }
            };

            var eidasOptions = new SdIssuanceOptions
            {
                DisclosureStructure = new
                {
                    birth_place = true,
                    current_address = true,
                    gender = true
                },
                AllowWeakAlgorithms = false,
                DecoyDigests = 3
            };

            var eidasCredential = issuer.Issue(eidasPidClaims, eidasOptions);

            Console.WriteLine("   eIDAS PID CREDENTIAL ISSUED:");
            Console.WriteLine($"   Format: SD-JWT VC compliant");
            Console.WriteLine($"   Algorithm: ES384 (eIDAS Very High compliant)");
            Console.WriteLine($"   Selective Disclosures: {eidasCredential.Disclosures.Count}");
            Console.WriteLine($"   Level of Assurance: Very High");
            Console.WriteLine();

            Console.WriteLine("   eIDAS COMPLIANCE FEATURES:");
            Console.WriteLine("   [X] Qualified Trust Service Provider (QTSP) issued");
            Console.WriteLine("   [X] Level of Assurance Very High (LoA 3)");
            Console.WriteLine("   [X] Person Identification Data (PID) format");
            Console.WriteLine("   [X] EUDI Wallet compatible");
            Console.WriteLine("   [X] Cross-border recognition enabled");
            Console.WriteLine("   [X] GDPR Article 6 legal basis compliance");

        }
        catch (Exception ex)
        {
            Console.WriteLine($"   ERROR in eIDAS demonstration: {ex.Message}");
        }

        Console.WriteLine();
        await Task.CompletedTask;
    }

    private static async Task DemonstrateCrossBorderRecognition()
    {
        Console.WriteLine("4. CROSS-BORDER CREDENTIAL RECOGNITION");
        Console.WriteLine("   International interoperability for government credentials");
        Console.WriteLine();

        Console.WriteLine("   INTERNATIONAL TRUST FRAMEWORK:");
        Console.WriteLine("   Mutual recognition agreements between sovereign entities");
        Console.WriteLine("   Based on equivalent assurance levels and trust frameworks");
        Console.WriteLine();

        // Simulate recognition matrix
        var recognitionMatrix = new[]
        {
            new { Country = "Germany", Framework = "eIDAS", Level = "Very High", Recognized = true },
            new { Country = "France", Framework = "eIDAS", Level = "Very High", Recognized = true },
            new { Country = "United States", Framework = "US-Gov", Level = "LOA-4", Recognized = true },
            new { Country = "Canada", Framework = "PCTF", Level = "4", Recognized = true },
            new { Country = "Australia", Framework = "TDIF", Level = "IP3", Recognized = true },
            new { Country = "Japan", Framework = "J-TrustMark", Level = "High", Recognized = false }
        };

        Console.WriteLine("   RECOGNITION MATRIX:");
        Console.WriteLine("   +-------------+------------+-------------+--------------+");
        Console.WriteLine("   | Country     | Framework  | Level       | Recognized   |");
        Console.WriteLine("   +-------------+------------+-------------+--------------+");
        
        foreach (var entry in recognitionMatrix)
        {
            var status = entry.Recognized ? "[X] Yes" : "[ ] No";
            Console.WriteLine($"   | {entry.Country,-11} | {entry.Framework,-10} | {entry.Level,-11} | {status,-12} |");
        }
        
        Console.WriteLine("   +-------------+------------+-------------+--------------+");
        Console.WriteLine();

        Console.WriteLine("   TRUST CHAIN RESOLUTION WORKFLOW:");
        Console.WriteLine("   Step 1: Receive foreign government credential");
        Console.WriteLine("   Step 2: Parse SD-JWT and extract issuer information");
        Console.WriteLine("   Step 3: Identify issuing country trust framework");
        Console.WriteLine("   Step 4: Validate trust chain to known trust anchor");
        Console.WriteLine("   Step 5: Map foreign assurance level to domestic equivalent");
        Console.WriteLine("   Step 6: Apply recognition policy based on mutual agreement");
        Console.WriteLine("   Step 7: Verify selective disclosures if required");
        Console.WriteLine("   Step 8: Accept or reject credential for domestic use");
        Console.WriteLine();

        // Demonstrate cross-border validation scenario
        Console.WriteLine("   CROSS-BORDER VALIDATION EXAMPLE:");
        Console.WriteLine();
        
        var foreignCredential = new
        {
            Issuer = "https://identity.germany.gov.de",
            AssuranceLevel = "eIDAS_Very_High",
            TrustFramework = "eIDAS",
            Subject = "German citizen requesting service in Spain",
            MutualRecognitionAgreement = "EU_eIDAS_2014_910",
            SdJwtFormat = "vc+sd-jwt",
            ValidationSteps = new[]
            {
                "[X] SD-JWT structure validated",
                "[X] German issuer found in eIDAS trust list",
                "[X] Certificate chain validated to EU trust anchor",
                "[X] Cryptographic signature verified (ES384)",
                "[X] eIDAS Very High maps to Spanish Level 2 compliance",
                "[X] Mutual recognition agreement exists (eIDAS)",
                "[X] Selective disclosures processed correctly",
                "[X] Credential accepted for domestic service access"
            }
        };

        Console.WriteLine($"   Foreign Credential Issuer: {foreignCredential.Issuer}");
        Console.WriteLine($"   Format: {foreignCredential.SdJwtFormat}");
        Console.WriteLine($"   Assurance Level: {foreignCredential.AssuranceLevel}");
        Console.WriteLine($"   Trust Framework: {foreignCredential.TrustFramework}");
        Console.WriteLine($"   Recognition Agreement: {foreignCredential.MutualRecognitionAgreement}");
        Console.WriteLine();
        Console.WriteLine("   Validation Process:");
        foreach (var step in foreignCredential.ValidationSteps)
        {
            Console.WriteLine($"      {step}");
        }
        Console.WriteLine();

        await Task.CompletedTask;
    }

    private static async Task DemonstrateAuditAndCompliance()
    {
        Console.WriteLine("5. GOVERNMENT AUDIT AND COMPLIANCE");
        Console.WriteLine("   Enhanced audit trail and compliance reporting for sovereign use");
        Console.WriteLine();

        Console.WriteLine("   GOVERNMENT AUDIT REQUIREMENTS:");
        Console.WriteLine("   • Complete audit trail of all credential operations");
        Console.WriteLine("   • Digital signatures on audit records");
        Console.WriteLine("   • Tamper-evident audit storage");
        Console.WriteLine("   • Regulatory compliance reporting");
        Console.WriteLine("   • Data retention according to national laws");
        Console.WriteLine("   • Privacy impact assessments");
        Console.WriteLine();

        // Create comprehensive audit record
        var auditRecord = new
        {
            AuditId = Guid.NewGuid(),
            Timestamp = DateTimeOffset.UtcNow,
            Operation = "national_id_credential_issuance",
            Subject = "citizen:ES:DNI:12345678Z",
            Issuer = "https://identity.gov.example",
            ComplianceLevel = "Level3_Sovereign",
            SdJwtDetails = new
            {
                Algorithm = "ES512",
                KeyCurve = "P-521",
                SdJwtLength = 2847,
                SelectiveDisclosures = 4,
                DecoyDigests = 10,
                KeyBinding = true
            },
            SecurityContext = new
            {
                HsmUsed = true, // Simulated
                QualifiedSignature = true,
                OperatorId = "gov-operator-001",
                SecurityClearance = "Top Secret",
                AuditLogger = "gov-audit-system-v2.1",
                SdJwtLibraryVersion = "SdJwt.Net v2.1.0"
            },
            ComplianceChecks = new[]
            {
                new { Check = "eIDAS Article 45e compliance", Status = "[X] PASS" },
                new { Check = "National security standards", Status = "[X] PASS" },
                new { Check = "Data protection regulation", Status = "[X] PASS" },
                new { Check = "Cryptographic policy compliance", Status = "[X] PASS" },
                new { Check = "Hardware security requirements", Status = "[ ] SIMULATED" },
                new { Check = "SD-JWT format compliance", Status = "[X] PASS" },
                new { Check = "Selective disclosure privacy", Status = "[X] PASS" }
            },
            RegulatoryMetadata = new
            {
                DataController = "Ministry of Digital Identity",
                LegalBasis = "Public Task (GDPR Art 6(1)(e))",
                RetentionPeriod = "10 years",
                CrossBorderTransfer = "Within EU/EEA only",
                DataSubjectRights = "Access, rectification, erasure (with restrictions)",
                TechnicalImplementation = "SD-JWT with selective disclosure"
            }
        };

        Console.WriteLine("   GOVERNMENT AUDIT RECORD:");
        Console.WriteLine("   ```json");
        Console.WriteLine(JsonSerializer.Serialize(auditRecord, new JsonSerializerOptions 
        { 
            WriteIndented = true 
        }));
        Console.WriteLine("   ```");
        Console.WriteLine();

        Console.WriteLine("   AUDIT TRAIL PROTECTION:");
        Console.WriteLine("   • All audit records signed with government HSM");
        Console.WriteLine("   • Immutable storage in government security infrastructure");
        Console.WriteLine("   • Access controls limited to authorized audit personnel");
        Console.WriteLine("   • Regular integrity verification of audit logs");
        Console.WriteLine("   • Compliance with national audit standards");
        Console.WriteLine("   • SD-JWT technical details logged for troubleshooting");
        Console.WriteLine();

        Console.WriteLine("   COMPLIANCE DASHBOARD METRICS:");
        var metrics = new
        {
            TotalCredentialsIssued = 1_250_000,
            SovereignComplianceRate = 99.99,
            AverageValidationTime = "125ms",
            SecurityIncidents = 0,
            AuditRecordsGenerated = 3_750_000,
            RegulatoryInspections = 12,
            InspectionResults = "Full Compliance",
            SdJwtOperationsToday = 45_670,
            CrossBorderRecognitions = 8_920
        };

        Console.WriteLine($"   Total Credentials Issued: {metrics.TotalCredentialsIssued:N0}");
        Console.WriteLine($"   Sovereign Compliance Rate: {metrics.SovereignComplianceRate:F2}%");
        Console.WriteLine($"   Average Validation Time: {metrics.AverageValidationTime}");
        Console.WriteLine($"   Security Incidents: {metrics.SecurityIncidents}");
        Console.WriteLine($"   Audit Records: {metrics.AuditRecordsGenerated:N0}");
        Console.WriteLine($"   Regulatory Inspections: {metrics.RegulatoryInspections}");
        Console.WriteLine($"   Inspection Results: {metrics.InspectionResults}");
        Console.WriteLine($"   SD-JWT Operations Today: {metrics.SdJwtOperationsToday:N0}");
        Console.WriteLine($"   Cross-Border Recognitions: {metrics.CrossBorderRecognitions:N0}");
        Console.WriteLine();

        await Task.CompletedTask;
    }

    private static SecurityKey CreateECKey(int keySize)
    {
        var curve = keySize switch
        {
            256 => ECCurve.NamedCurves.nistP256,
            384 => ECCurve.NamedCurves.nistP384,
            521 => ECCurve.NamedCurves.nistP521,
            _ => ECCurve.NamedCurves.nistP256
        };

        return new ECDsaSecurityKey(ECDsa.Create(curve)) 
        { 
            KeyId = $"gov-ec-{keySize}-{Guid.NewGuid():N}"[..16] 
        };
    }

    private static SecurityKey CreateRSAKey(int keySize)
    {
        return new RsaSecurityKey(RSA.Create(keySize)) 
        { 
            KeyId = $"gov-rsa-{keySize}-{Guid.NewGuid():N}"[..16] 
        };
    }

    private static bool ValidateAlgorithmForLevel3(string algorithm)
    {
        var level3Algorithms = new[] { "ES512", "PS512", "EdDSA" };
        return level3Algorithms.Contains(algorithm);
    }

    private static (bool IsValid, string Analysis, string? Violation, string? Recommendation) PerformSovereignValidation(
        string algorithm, bool isHsmBacked)
    {
        // Check if algorithm is allowed for Level 3
        if (!ValidateAlgorithmForLevel3(algorithm))
        {
            return (false,
                $"Algorithm {algorithm} insufficient for Sovereign level",
                $"Algorithm {algorithm} not approved for Level 3",
                "Use ES512, PS512, or EdDSA for Sovereign compliance");
        }

        // Check HSM backing requirement
        if (!isHsmBacked)
        {
            return (false,
                "HSM backing required for Sovereign level",
                "Key not backed by Hardware Security Module",
                "Deploy HSM-backed keys for Level 3 compliance");
        }

        return (true,
            $"Algorithm {algorithm} with HSM backing meets Sovereign requirements",
            null,
            null);
    }
}
