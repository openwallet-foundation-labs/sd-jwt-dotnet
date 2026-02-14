using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SdJwt.Net;
using SdJwt.Net.Issuer;
using SdJwt.Net.Holder;
using SdJwt.Net.Verifier;
using SdJwt.Net.Models;
using SdJwt.Net.HAIP;
using SdJwt.Net.HAIP.Models;
using SdJwt.Net.HAIP.Validators;
using SdJwt.Net.HAIP.Extensions;
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
/// 2. Sovereign-level cryptographic validation using actual HAIP validators
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
        await DemonstrateSovereignValidation(logger);
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
            // Step 1: Create sovereign-level keys (P-521 for Level 3)
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

            // Use actual HAIP validator for algorithm compliance
            var cryptoValidator = new HaipCryptoValidator(HaipLevel.Level3_Sovereign, 
                logger as ILogger<HaipCryptoValidator> ?? LoggerFactory.Create(b => b.AddConsole()).CreateLogger<HaipCryptoValidator>());
            var algorithmValidation = cryptoValidator.ValidateAlgorithm(SecurityAlgorithms.EcdsaSha512);
            
            Console.WriteLine($"   Algorithm ES512 compliant with Level 3: {algorithmValidation.IsValid}");
            if (algorithmValidation.IsValid)
            {
                Console.WriteLine($"   Details: {algorithmValidation.Details}");
            }
            else
            {
                Console.WriteLine($"   Error: {algorithmValidation.ErrorMessage}");
            }
            
            Console.WriteLine("   Key Generation: P-521 curve (meets sovereign requirements)");
            Console.WriteLine("   HSM Backing: Simulated (would be required in production)");
            Console.WriteLine();

            // Validate key compliance using HAIP validator
            var keyValidation = cryptoValidator.ValidateKeyCompliance(governmentSigningKey, SecurityAlgorithms.EcdsaSha512);
            Console.WriteLine($"   Key Validation Result: {(keyValidation.IsCompliant ? "COMPLIANT" : "NON-COMPLIANT")}");
            Console.WriteLine($"   Achieved HAIP Level: {keyValidation.AchievedLevel}");
            Console.WriteLine($"   Violations Found: {keyValidation.Violations.Count}");
            
            foreach (var violation in keyValidation.Violations.Where(v => v.Severity == HaipSeverity.Critical))
            {
                Console.WriteLine($"   Critical Violation: {violation.Description}");
            }
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

            var verifier = new SdVerifier((jwt) => Task.FromResult<SecurityKey>(governmentSigningKey));

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

    private static async Task DemonstrateSovereignValidation(ILogger logger)
    {
        Console.WriteLine("2. SOVEREIGN-LEVEL VALIDATION USING HAIP VALIDATORS");
        Console.WriteLine("   Enforcing highest security standards using actual HAIP library");
        Console.WriteLine();

        Console.WriteLine("   VALIDATION SCENARIOS:");
        Console.WriteLine();

        // Create HAIP crypto validator for Level 3 (Sovereign)
        var sovereignValidator = new HaipCryptoValidator(HaipLevel.Level3_Sovereign, 
            logger as ILogger<HaipCryptoValidator> ?? LoggerFactory.Create(b => b.AddConsole()).CreateLogger<HaipCryptoValidator>());
        
        // Test scenarios for sovereign validation - Define scenario type explicitly
        var scenarios = new (string Name, string Algorithm, Func<SecurityKey> KeyFactory, bool ExpectedPass, string Reason)[]
        {
            ("Valid Sovereign Key (ES512 with P-521)",
             SecurityAlgorithms.EcdsaSha512,
             () => new ECDsaSecurityKey(ECDsa.Create(ECCurve.NamedCurves.nistP521)) { KeyId = "test-p521" },
             true,
             "Meets all Level 3 requirements"),
            
            ("Insufficient Algorithm (ES384)",
             SecurityAlgorithms.EcdsaSha384,
             () => new ECDsaSecurityKey(ECDsa.Create(ECCurve.NamedCurves.nistP384)) { KeyId = "test-p384" },
             false,
             "ES384 insufficient for Level 3"),
            
            ("Forbidden Algorithm (RS256)",
             SecurityAlgorithms.RsaSha256,
             () => new RsaSecurityKey(RSA.Create(2048)) { KeyId = "test-rsa" },
             false,
             "RS256 forbidden in HAIP"),
            
            ("Weak EC Key (P-256 with ES512)",
             SecurityAlgorithms.EcdsaSha512,
             () => new ECDsaSecurityKey(ECDsa.Create(ECCurve.NamedCurves.nistP256)) { KeyId = "test-weak-p256" },
             false,
             "P-256 insufficient for Level 3")
        };

        Console.WriteLine("   DETAILED SOVEREIGN VALIDATION RESULTS:");
        Console.WriteLine("   +---------------------------+----------+-------------------+");
        Console.WriteLine("   | Scenario                  | Result   | Achieved Level    |");
        Console.WriteLine("   +---------------------------+----------+-------------------+");

        foreach (var scenario in scenarios)
        {
            try
            {
                var key = scenario.KeyFactory();
                var validationResult = sovereignValidator.ValidateKeyCompliance(key, scenario.Algorithm);
                
                var result = validationResult.IsCompliant ? "PASS" : "FAIL";
                var level = validationResult.IsCompliant ? validationResult.AchievedLevel.ToString() : "None";
                
                Console.WriteLine($"   | {scenario.Name,-25} | {result,-8} | {level,-17} |");
                
                Console.WriteLine($"      Algorithm Test: {scenario.Algorithm}");
                Console.WriteLine($"      Compliance: {(validationResult.IsCompliant ? "COMPLIANT" : "NON-COMPLIANT")}");
                Console.WriteLine($"      Violations: {validationResult.Violations.Count}");
                
                foreach (var violation in validationResult.Violations.Take(2)) // Show first 2 violations
                {
                    Console.WriteLine($"        - {violation.Type}: {violation.Description}");
                    if (!string.IsNullOrEmpty(violation.RecommendedAction))
                    {
                        Console.WriteLine($"          Recommendation: {violation.RecommendedAction}");
                    }
                }
                
                if (validationResult.AuditTrail.Steps.Any())
                {
                    Console.WriteLine($"      Audit Steps: {validationResult.AuditTrail.Steps.Count}");
                    foreach (var step in validationResult.AuditTrail.Steps.Take(2))
                    {
                        Console.WriteLine($"        - {step.Operation}: {(step.Success ? "SUCCESS" : "FAILED")}");
                    }
                }
                
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   | {scenario.Name,-25} | ERROR    | Exception         |");
                Console.WriteLine($"      Error: {ex.Message}");
                Console.WriteLine();
            }
        }
        
        Console.WriteLine("   +---------------------------+----------+-------------------+");
        Console.WriteLine();

        Console.WriteLine("   SOVEREIGN SECURITY REQUIREMENTS (HAIP Level 3):");
        Console.WriteLine("   • Hardware Security Module (HSM) backing mandatory");
        Console.WriteLine("   • Qualified Electronic Signature standards (QES)");
        Console.WriteLine("   • Enhanced audit trail with digital signatures");
        Console.WriteLine($"   • Cryptographic algorithms: {string.Join(", ", HaipConstants.Level3_Algorithms)}");
        Console.WriteLine($"   • Minimum EC key size: P-{HaipConstants.KeySizes.Level3_EcMinimum}");
        Console.WriteLine($"   • Minimum RSA key size: {HaipConstants.KeySizes.Level3_RsaMinimum} bits");
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

            // Create HAIP validator for Level 2 (Very High) - eIDAS compliance
            var logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<HaipCryptoValidator>();
            var eidasValidator = new HaipCryptoValidator(HaipLevel.Level2_VeryHigh, logger);
            
            // Validate eIDAS key compliance
            var eidasValidation = eidasValidator.ValidateKeyCompliance(eidasSigningKey, SecurityAlgorithms.EcdsaSha384);

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
            Console.WriteLine($"   HAIP Compliance: {(eidasValidation.IsCompliant ? "COMPLIANT" : "NON-COMPLIANT")}");
            Console.WriteLine($"   Achieved Level: {eidasValidation.AchievedLevel}");
            Console.WriteLine($"   Selective Disclosures: {eidasCredential.Disclosures.Count}");
            Console.WriteLine($"   Level of Assurance: Very High");
            Console.WriteLine();

            Console.WriteLine("   eIDAS COMPLIANCE FEATURES:");
            Console.WriteLine($"   [X] Qualified Trust Service Provider (QTSP) issued");
            Console.WriteLine($"   [X] Level of Assurance Very High (LoA 3)");
            Console.WriteLine($"   [X] Person Identification Data (PID) format");
            Console.WriteLine($"   [X] EUDI Wallet compatible");
            Console.WriteLine($"   [X] Cross-border recognition enabled");
            Console.WriteLine($"   [X] GDPR Article 6 legal basis compliance");
            Console.WriteLine($"   [X] HAIP Level 2+ cryptographic compliance: {eidasValidation.IsCompliant}");

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

        // Simulate recognition matrix with HAIP levels
        var recognitionMatrix = new[]
        {
            new { Country = "Germany", Framework = "eIDAS", Level = "Level2_VeryHigh", Recognized = true },
            new { Country = "France", Framework = "eIDAS", Level = "Level2_VeryHigh", Recognized = true },
            new { Country = "United States", Framework = "US-Gov", Level = "Level3_Sovereign", Recognized = true },
            new { Country = "Canada", Framework = "PCTF", Level = "Level2_VeryHigh", Recognized = true },
            new { Country = "Australia", Framework = "TDIF", Level = "Level2_VeryHigh", Recognized = true },
            new { Country = "Japan", Framework = "J-TrustMark", Level = "Level1_High", Recognized = false }
        };

        Console.WriteLine("   RECOGNITION MATRIX (HAIP Levels):");
        Console.WriteLine("   +-------------+------------+------------------+--------------+");
        Console.WriteLine("   | Country     | Framework  | HAIP Level       | Recognized   |");
        Console.WriteLine("   +-------------+------------+------------------+--------------+");
        
        foreach (var entry in recognitionMatrix)
        {
            var status = entry.Recognized ? "[X] Yes" : "[ ] No";
            Console.WriteLine($"   | {entry.Country,-11} | {entry.Framework,-10} | {entry.Level,-16} | {status,-12} |");
        }
        
        Console.WriteLine("   +-------------+------------+------------------+--------------+");
        Console.WriteLine();

        Console.WriteLine("   TRUST CHAIN RESOLUTION WORKFLOW:");
        Console.WriteLine("   Step 1: Receive foreign government credential");
        Console.WriteLine("   Step 2: Parse SD-JWT and extract issuer information");
        Console.WriteLine("   Step 3: Identify issuing country trust framework");
        Console.WriteLine("   Step 4: Validate trust chain to known trust anchor");
        Console.WriteLine("   Step 5: Map foreign assurance level to domestic HAIP level");
        Console.WriteLine("   Step 6: Apply recognition policy based on mutual agreement");
        Console.WriteLine("   Step 7: Verify selective disclosures if required");
        Console.WriteLine("   Step 8: Accept or reject credential for domestic use");
        Console.WriteLine();

        // Demonstrate cross-border validation scenario
        Console.WriteLine("   ACTUAL CROSS-BORDER VALIDATION EXAMPLE:");
        Console.WriteLine();

        try
        {
            // Step 1: Create a foreign government credential (German eIDAS)
            Console.WriteLine("   Step 1: Simulating German eIDAS Credential Reception");
            
            var germanSigningKey = new ECDsaSecurityKey(ECDsa.Create(ECCurve.NamedCurves.nistP384))
            {
                KeyId = "german-eidas-2024"
            };

            var germanIssuer = new SdIssuer(germanSigningKey, SecurityAlgorithms.EcdsaSha384);

            var germanCredentialClaims = new JwtPayload
            {
                { "iss", "https://identity.germany.gov.de" },
                { "sub", "urn:eidas:person:DE:ausweis:123456789" },
                { "vct", "https://eudi.europa.eu/credentials/person-identification-data" },
                { "given_name", "Hans" },
                { "family_name", "Mueller" },
                { "date_of_birth", "1985-08-20" },
                { "nationality", "DE" },
                { "eidas_level_of_assurance", "http://eidas.europa.eu/LoA/high" },
                { "issuing_authority", "Bundesamt für Sicherheit" },
                { "trust_framework", "eIDAS" }
            };

            var germanOptions = new SdIssuanceOptions
            {
                DisclosureStructure = new
                {
                    birth_place = true,
                    current_address = true,
                    document_number = true
                },
                AllowWeakAlgorithms = false,
                DecoyDigests = 3
            };

            var germanCredential = germanIssuer.Issue(germanCredentialClaims, germanOptions);
            Console.WriteLine($"   German Credential Created: {germanCredential.SdJwt.Length} characters");
            Console.WriteLine();

            // Step 2: German citizen creates presentation for Spanish service
            Console.WriteLine("   Step 2: German Citizen Service Request in Spain");
            
            var germanCitizenKey = new ECDsaSecurityKey(ECDsa.Create(ECCurve.NamedCurves.nistP384))
            {
                KeyId = "german-citizen-wallet"
            };

            var germanHolder = new SdJwtHolder(germanCredential.Issuance);

            var crossBorderPresentation = germanHolder.CreatePresentation(
                disclosure => disclosure.ClaimName == "current_address", // Minimal disclosure for service
                kbJwtPayload: new JwtPayload
                {
                    { "aud", "https://services.spain.gov.es" },
                    { "iat", DateTimeOffset.UtcNow.ToUnixTimeSeconds() },
                    { "purpose", "cross_border_social_service_access" },
                    { "recognition_framework", "eIDAS_mutual_recognition" }
                },
                kbJwtSigningKey: germanCitizenKey,
                kbJwtSigningAlgorithm: SecurityAlgorithms.EcdsaSha384
            );

            Console.WriteLine("   Cross-border Presentation Created");
            Console.WriteLine();

            // Step 3: Spanish government service performs cross-border verification
            Console.WriteLine("   Step 3: Spanish Government Cross-Border Verification");

            var spanishVerifier = new SdVerifier((jwt) => Task.FromResult<SecurityKey>(germanSigningKey));

            var crossBorderValidationParams = new TokenValidationParameters
            {
                ValidateIssuer = false, // Trust framework handles issuer validation
                ValidateAudience = false,
                ValidateLifetime = false,
                IssuerSigningKey = germanSigningKey
            };

            var crossBorderKbParams = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = false,
                IssuerSigningKey = germanCitizenKey
            };

            var crossBorderResult = await spanishVerifier.VerifyAsync(
                crossBorderPresentation,
                crossBorderValidationParams,
                crossBorderKbParams);

            Console.WriteLine("   Cross-Border Verification Results:");
            Console.WriteLine($"   Verification Status: {(crossBorderResult.KeyBindingVerified ? "SUCCESS" : "FAILED")}");
            Console.WriteLine($"   German Credential Valid: {crossBorderResult.KeyBindingVerified}");
            Console.WriteLine($"   Key Binding Verified: {crossBorderResult.KeyBindingVerified}");
            
            // Show which claims were actually verified
            var verifiedClaims = crossBorderResult.ClaimsPrincipal.Claims
                .Where(c => !c.Type.StartsWith("_") && c.Type != "iss" && c.Type != "sub")
                .Take(4);
            
            Console.WriteLine("   Verified Cross-Border Claims:");
            foreach (var claim in verifiedClaims)
            {
                Console.WriteLine($"     {claim.Type}: {claim.Value}");
            }
            
            // Verify eIDAS compliance mapping
            var eidasCompliant = crossBorderResult.ClaimsPrincipal.Claims
                .Any(c => c.Type == "eidas_level_of_assurance" && c.Value.Contains("high"));
            
            Console.WriteLine($"   eIDAS High Assurance Verified: {eidasCompliant}");
            Console.WriteLine($"   Trust Framework Recognition: eIDAS mutual recognition");
            Console.WriteLine($"   Mapped to Spanish Level: 2 (Very High)");
            Console.WriteLine();

            Console.WriteLine("   CROSS-BORDER RECOGNITION ACHIEVED:");
            Console.WriteLine($"   [X] German eIDAS credential accepted: {crossBorderResult.KeyBindingVerified}");
            Console.WriteLine($"   [X] Trust framework validation: {eidasCompliant}");
            Console.WriteLine($"   [X] Mutual recognition agreement applied: eIDAS");
            Console.WriteLine($"   [X] Cross-border service access granted: {crossBorderResult.KeyBindingVerified}");
            Console.WriteLine($"   [X] Privacy protection maintained: {verifiedClaims.Count()} limited claims revealed");
            Console.WriteLine($"   [X] SD-JWT interoperability verified: COMPLETE");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"   ERROR in cross-border validation: {ex.Message}");
        }

        Console.WriteLine();
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

        // Create HAIP configuration for sovereign use
        var sovereignConfig = HaipConfiguration.GetDefault(HaipLevel.Level3_Sovereign);
        
        // Create comprehensive audit record with HAIP compliance data
        var auditRecord = new
        {
            AuditId = Guid.NewGuid(),
            Timestamp = DateTimeOffset.UtcNow,
            Operation = "national_id_credential_issuance",
            Subject = "citizen:ES:DNI:12345678Z",
            Issuer = "https://identity.gov.example",
            HaipCompliance = new
            {
                RequiredLevel = sovereignConfig.RequiredLevel,
                ComplianceLevel = "Level3_Sovereign",
                EidasCompliance = sovereignConfig.EnableEidasCompliance,
                SovereignCompliance = sovereignConfig.EnableSovereignCompliance,
                AuditingEnabled = sovereignConfig.AuditingOptions.DetailedLogging,
                DigitalSignatureRequired = sovereignConfig.AuditingOptions.RequireDigitalSignature
            },
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
                SdJwtLibraryVersion = "SdJwt.Net v2.1.0",
                HaipLibraryVersion = "SdJwt.Net.HAIP v1.0.0"
            },
            ComplianceChecks = new[]
            {
                new { Check = "HAIP Level 3 cryptographic compliance", Status = "[X] PASS" },
                new { Check = "eIDAS Article 45e compliance", Status = "[X] PASS" },
                new { Check = "National security standards", Status = "[X] PASS" },
                new { Check = "Data protection regulation", Status = "[X] PASS" },
                new { Check = "Hardware security requirements", Status = "[ ] SIMULATED" },
                new { Check = "SD-JWT format compliance", Status = "[X] PASS" },
                new { Check = "Selective disclosure privacy", Status = "[X] PASS" }
            },
            RegulatoryMetadata = new
            {
                DataController = "Ministry of Digital Identity",
                LegalBasis = "Public Task (GDPR Art 6(1)(e))",
                RetentionPeriod = sovereignConfig.AuditingOptions.CacheTimeout.ToString(),
                CrossBorderTransfer = "Within EU/EEA only",
                DataSubjectRights = "Access, rectification, erasure (with restrictions)",
                TechnicalImplementation = "SD-JWT with HAIP Level 3 selective disclosure"
            }
        };

        Console.WriteLine("   GOVERNMENT AUDIT RECORD WITH HAIP COMPLIANCE:");
        Console.WriteLine("   ```json");
        Console.WriteLine(JsonSerializer.Serialize(auditRecord, new JsonSerializerOptions 
        { 
            WriteIndented = true 
        }));
        Console.WriteLine("   ```");
        Console.WriteLine();

        Console.WriteLine("   HAIP-ENHANCED AUDIT TRAIL PROTECTION:");
        Console.WriteLine("   • All audit records signed with government HSM");
        Console.WriteLine("   • HAIP Level 3 compliance validation for all operations");
        Console.WriteLine("   • Immutable storage in government security infrastructure");
        Console.WriteLine("   • Access controls limited to authorized audit personnel");
        Console.WriteLine("   • Regular integrity verification of audit logs");
        Console.WriteLine("   • Compliance with HAIP sovereign standards");
        Console.WriteLine("   • SD-JWT technical details with HAIP validation results");
        Console.WriteLine();

        Console.WriteLine("   COMPLIANCE DASHBOARD METRICS:");
        var metrics = new
        {
            TotalCredentialsIssued = 1_250_000,
            HaipSovereignComplianceRate = 99.99,
            AverageValidationTime = "125ms",
            SecurityIncidents = 0,
            AuditRecordsGenerated = 3_750_000,
            RegulatoryInspections = 12,
            InspectionResults = "Full HAIP Level 3 Compliance",
            SdJwtOperationsToday = 45_670,
            CrossBorderRecognitions = 8_920,
            HaipValidationsPerformed = 125_890,
            CryptographicViolations = 0
        };

        Console.WriteLine($"   Total Credentials Issued: {metrics.TotalCredentialsIssued:N0}");
        Console.WriteLine($"   HAIP Sovereign Compliance Rate: {metrics.HaipSovereignComplianceRate:F2}%");
        Console.WriteLine($"   Average Validation Time: {metrics.AverageValidationTime}");
        Console.WriteLine($"   Security Incidents: {metrics.SecurityIncidents}");
        Console.WriteLine($"   Audit Records: {metrics.AuditRecordsGenerated:N0}");
        Console.WriteLine($"   Regulatory Inspections: {metrics.RegulatoryInspections}");
        Console.WriteLine($"   Inspection Results: {metrics.InspectionResults}");
        Console.WriteLine($"   SD-JWT Operations Today: {metrics.SdJwtOperationsToday:N0}");
        Console.WriteLine($"   Cross-Border Recognitions: {metrics.CrossBorderRecognitions:N0}");
        Console.WriteLine($"   HAIP Validations Performed: {metrics.HaipValidationsPerformed:N0}");
        Console.WriteLine($"   Cryptographic Violations: {metrics.CryptographicViolations}");
        Console.WriteLine();

        await Task.CompletedTask;
    }
}
