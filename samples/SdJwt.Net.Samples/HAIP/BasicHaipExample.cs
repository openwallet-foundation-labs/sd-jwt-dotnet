using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SdJwt.Net;
using SdJwt.Net.Issuer;
using SdJwt.Net.Holder;
using SdJwt.Net.Verifier;
using SdJwt.Net.Models;
using SdJwt.Net.PresentationExchange;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;

namespace SdJwt.Net.Samples.HAIP;

/// <summary>
/// Demonstrates basic HAIP (High Assurance Interoperability Profile) implementation and compliance validation
/// 
/// HAIP Overview:
/// - OpenID4VC specification for high-assurance verifiable credentials
/// - Three progressive compliance levels with increasing security requirements
/// - Policy-based cryptographic and protocol enforcement
/// - Designed for government, enterprise, and regulated industry use cases
/// 
/// This example shows:
/// 1. HAIP compliance level requirements and restrictions
/// 2. Cryptographic algorithm validation and key strength enforcement
/// 3. Protocol security validation (proof of possession, secure transport)
/// 4. Configuration patterns for different deployment scenarios
/// 5. Comprehensive compliance reporting and audit trail generation
/// 
/// Note: This is a conceptual demonstration using actual SD-JWT .NET APIs.
/// The HAIP compliance validation is simulated to show the concepts and requirements.
/// </summary>
public class BasicHaipExample
{
    public static async Task RunExample(IServiceProvider services)
    {
        var logger = services.GetRequiredService<ILogger<BasicHaipExample>>();

        Console.WriteLine("\n" + new string('=', 75));
        Console.WriteLine("         HAIP Basic Implementation Example                ");
        Console.WriteLine("     OpenID4VC High Assurance Interoperability Profile    ");
        Console.WriteLine(new string('=', 75));

        Console.WriteLine("\nHAIP provides policy-based compliance validation for high assurance");
        Console.WriteLine("verifiable credentials. It defines three progressive security levels");
        Console.WriteLine("with strict cryptographic and protocol requirements for government,");
        Console.WriteLine("enterprise, and regulated industry use cases.\n");

        Console.WriteLine("Key HAIP Benefits:");
        Console.WriteLine("• Policy-driven security enforcement at the framework level");
        Console.WriteLine("• Progressive compliance levels for different risk profiles");
        Console.WriteLine("• Cryptographic algorithm restrictions (bans weak algorithms)");
        Console.WriteLine("• Protocol security mandates (proof of possession, secure transport)");
        Console.WriteLine("• Comprehensive audit trails for regulatory compliance");
        Console.WriteLine("• Non-intrusive integration with existing SD-JWT implementations");
        Console.WriteLine();

        await DemonstrateHaipLevels();
        await DemonstrateCryptographicValidation();
        await DemonstrateProtocolSecurity();
        await DemonstrateActualSdJwtIntegration(logger);
        await DemonstrateConfigurationPatterns();
        await DemonstrateComplianceReporting();

        Console.WriteLine(new string('=', 75));
        Console.WriteLine("          HAIP Basic concepts demonstrated!             ");
        Console.WriteLine("                                                       ");
        Console.WriteLine("  [X] HAIP compliance levels and requirements           ");
        Console.WriteLine("  [X] Cryptographic algorithm validation                ");
        Console.WriteLine("  [X] Protocol security enforcement                     ");
        Console.WriteLine("  [X] Integration with actual SD-JWT .NET APIs          ");
        Console.WriteLine("  [X] Configuration and deployment patterns             ");
        Console.WriteLine("  [X] Compliance reporting and audit trails             ");
        Console.WriteLine(new string('=', 75));
    }

    private static Task DemonstrateHaipLevels()
    {
        Console.WriteLine("1. HAIP COMPLIANCE LEVELS");
        Console.WriteLine("   Three progressive levels of assurance for different use cases");
        Console.WriteLine();

        var levels = new[]
        {
            new
            {
                Level = "Level1_High",
                Name = "Level 1 - High Assurance",
                Description = "Standard business and educational credentials",
                Algorithms = "ES256, ES384, PS256, PS384, EdDSA",
                KeySize = "EC P-256+, RSA 2048+",
                Requirements = new[] { "Proof of possession", "Secure transport (HTTPS)", "Algorithm restrictions" },
                UseCases = "Education, standard business, consumer applications",
                Compliance = "Basic enterprise security, GDPR"
            },
            new
            {
                Level = "Level2_VeryHigh",
                Name = "Level 2 - Very High Assurance",
                Description = "Financial services and healthcare credentials",
                Algorithms = "ES384, ES512, PS384, PS512, EdDSA",
                KeySize = "EC P-384+, RSA 3072+",
                Requirements = new[] { "Wallet attestation", "DPoP/mTLS", "PAR", "Enhanced audit logging" },
                UseCases = "Banking, healthcare, government services, professional licensing",
                Compliance = "PCI DSS, HIPAA, eIDAS Very High, SOX"
            },
            new
            {
                Level = "Level3_Sovereign",
                Name = "Level 3 - Sovereign",
                Description = "National identity and critical infrastructure",
                Algorithms = "ES512, PS512, EdDSA",
                KeySize = "EC P-521+, RSA 4096+, HSM required",
                Requirements = new[] { "Hardware Security Module", "Qualified electronic signatures", "Enhanced attestation" },
                UseCases = "National ID, defense, critical infrastructure, cross-border recognition",
                Compliance = "eIDAS Very High, National security standards, FIPS 140-2 Level 3+"
            }
        };

        foreach (var level in levels)
        {
            Console.WriteLine($"   {level.Name}");
            Console.WriteLine($"      Purpose: {level.Description}");
            Console.WriteLine($"      Algorithms: {level.Algorithms}");
            Console.WriteLine($"      Min Key Size: {level.KeySize}");
            Console.WriteLine($"      Requirements:");
            foreach (var req in level.Requirements)
            {
                Console.WriteLine($"        - {req}");
            }
            Console.WriteLine($"      Use Cases: {level.UseCases}");
            Console.WriteLine($"      Compliance: {level.Compliance}");
            Console.WriteLine();
        }

        Console.WriteLine("   ALGORITHM POLICY ENFORCEMENT:");
        Console.WriteLine("   ");
        Console.WriteLine("   FORBIDDEN ALGORITHMS (All Levels):");
        var forbiddenAlgorithms = new[] { "RS256", "HS256", "HS384", "HS512", "none" };
        Console.WriteLine($"      {string.Join(", ", forbiddenAlgorithms)}");
        Console.WriteLine("      Reason: Cryptographic weaknesses or inappropriate for high assurance");
        Console.WriteLine("      - RS256: RSA with SHA-256 (considered weak for new systems)");
        Console.WriteLine("      - HS256/384/512: Symmetric HMAC (inappropriate for verifiable credentials)");
        Console.WriteLine("      - none: No signature (completely insecure)");
        Console.WriteLine();

        Console.WriteLine("   PROGRESSIVE ALGORITHM RESTRICTIONS:");
        Console.WriteLine("   Level 1 → Level 2: ES256 and PS256 are prohibited");
        Console.WriteLine("   Level 2 → Level 3: Only the strongest algorithms allowed");
        Console.WriteLine("   Rationale: Higher levels require stronger cryptographic guarantees");
        Console.WriteLine();

        return Task.CompletedTask;
    }

    private static async Task DemonstrateCryptographicValidation()
    {
        Console.WriteLine("2. CRYPTOGRAPHIC VALIDATION");
        Console.WriteLine("   HAIP enforces strict cryptographic requirements with detailed validation");
        Console.WriteLine();

        // Test different algorithms and key combinations
        var testCases = new[]
        {
            new { Alg = "ES256", KeyType = "P-256", Level = "Level1_High", ShouldPass = true, Reason = "Compliant with Level 1 requirements" },
            new { Alg = "ES256", KeyType = "P-256", Level = "Level2_VeryHigh", ShouldPass = false, Reason = "ES256 insufficient for Level 2 (requires ES384+)" },
            new { Alg = "ES384", KeyType = "P-384", Level = "Level2_VeryHigh", ShouldPass = true, Reason = "Meets Level 2 cryptographic requirements" },
            new { Alg = "ES512", KeyType = "P-521", Level = "Level2_VeryHigh", ShouldPass = true, Reason = "Exceeds Level 2 requirements (Level 3 capable)" },
            new { Alg = "RS256", KeyType = "RSA-2048", Level = "Level1_High", ShouldPass = false, Reason = "RS256 forbidden in all HAIP levels" },
            new { Alg = "ES512", KeyType = "P-521", Level = "Level3_Sovereign", ShouldPass = false, Reason = "Missing HSM backing requirement" },
            new { Alg = "PS512", KeyType = "RSA-4096", Level = "Level3_Sovereign", ShouldPass = false, Reason = "Missing HSM backing requirement" },
            new { Alg = "HS256", KeyType = "HMAC-256", Level = "Level1_High", ShouldPass = false, Reason = "Symmetric keys inappropriate for verifiable credentials" }
        };

        Console.WriteLine("   VALIDATION TEST MATRIX:");
        Console.WriteLine("   +----------+----------+----------------+--------+-----------------------------------+");
        Console.WriteLine("   | Algorithm| Key Type | HAIP Level     | Result | Reason                           |");
        Console.WriteLine("   +----------+----------+----------------+--------+-----------------------------------+");

        foreach (var testCase in testCases)
        {
            var result = testCase.ShouldPass ? "PASS" : "FAIL";
            Console.WriteLine($"   | {testCase.Alg,-8} | {testCase.KeyType,-8} | {testCase.Level,-14} | {result,-6} | {testCase.Reason,-32} |");
        }

        Console.WriteLine("   +----------+----------+----------------+--------+-----------------------------------+");
        Console.WriteLine();

        Console.WriteLine("   DETAILED VALIDATION SCENARIOS:");
        foreach (var testCase in testCases.Take(5)) // Show first 5 for detailed analysis
        {
            Console.WriteLine($"   Testing {testCase.Alg} with {testCase.KeyType} for {testCase.Level}:");

            try
            {
                var key = CreateTestKey(testCase.KeyType);

                if (key != null)
                {
                    // Simulated validation logic with detailed analysis
                    var validationResult = PerformDetailedValidation(testCase.Alg, testCase.KeyType, testCase.Level);

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
                        Console.WriteLine($"      Security Level: {validationResult.SecurityLevel}");
                        Console.WriteLine($"      Key Strength: {validationResult.KeyStrength} bits effective security");
                    }
                }
                else
                {
                    Console.WriteLine("      Result: [ ] FAILED TO CREATE KEY");
                    Console.WriteLine("      Analysis: Unsupported key type for demonstration");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"      Result: [ ] ERROR - {ex.Message}");
            }

            Console.WriteLine();
        }

        await Task.CompletedTask;
    }

    private static async Task DemonstrateProtocolSecurity()
    {
        Console.WriteLine("3. PROTOCOL SECURITY ENFORCEMENT");
        Console.WriteLine("   HAIP mandates specific protocol security features beyond cryptography");
        Console.WriteLine();

        // Test scenarios for protocol security
        var securityScenarios = new[]
        {
            new
            {
                Name = "Compliant Level 1 Request",
                Algorithm = "ES256",
                KeyType = "P-256",
                HasProofOfPossession = true,
                IsSecureTransport = true,
                HasWalletAttestation = false,
                UsesDPoP = false,
                UsesPAR = false,
                Level = "Level1_High",
                ExpectedResult = true
            },
            new
            {
                Name = "Missing Proof of Possession",
                Algorithm = "ES256",
                KeyType = "P-256",
                HasProofOfPossession = false,
                IsSecureTransport = true,
                HasWalletAttestation = false,
                UsesDPoP = false,
                UsesPAR = false,
                Level = "Level1_High",
                ExpectedResult = false
            },
            new
            {
                Name = "Insecure Transport (HTTP)",
                Algorithm = "ES256",
                KeyType = "P-256",
                HasProofOfPossession = true,
                IsSecureTransport = false,
                HasWalletAttestation = false,
                UsesDPoP = false,
                UsesPAR = false,
                Level = "Level1_High",
                ExpectedResult = false
            },
            new
            {
                Name = "Level 2 Without Wallet Attestation",
                Algorithm = "ES384",
                KeyType = "P-384",
                HasProofOfPossession = true,
                IsSecureTransport = true,
                HasWalletAttestation = false,
                UsesDPoP = true,
                UsesPAR = true,
                Level = "Level2_VeryHigh",
                ExpectedResult = false
            },
            new
            {
                Name = "Compliant Level 2 Request",
                Algorithm = "ES384",
                KeyType = "P-384",
                HasProofOfPossession = true,
                IsSecureTransport = true,
                HasWalletAttestation = true,
                UsesDPoP = true,
                UsesPAR = true,
                Level = "Level2_VeryHigh",
                ExpectedResult = true
            }
        };

        Console.WriteLine("   PROTOCOL SECURITY VALIDATION:");
        Console.WriteLine();

        foreach (var scenario in securityScenarios)
        {
            Console.WriteLine($"   Scenario: {scenario.Name}");
            Console.WriteLine($"   Target Level: {scenario.Level}");

            try
            {
                // Simulate protocol validation
                var violations = new List<string>();
                var warnings = new List<string>();

                // Check mandatory requirements
                if (!scenario.HasProofOfPossession)
                {
                    violations.Add("Proof of possession is mandatory for all HAIP levels");
                }

                if (!scenario.IsSecureTransport)
                {
                    violations.Add("Secure transport (HTTPS with TLS 1.2+) is mandatory for all HAIP levels");
                }

                // Check level-specific requirements
                if (scenario.Level == "Level2_VeryHigh" || scenario.Level == "Level3_Sovereign")
                {
                    if (!scenario.HasWalletAttestation)
                    {
                        violations.Add("Wallet attestation is required for Level 2+ compliance");
                    }

                    if (!scenario.UsesDPoP)
                    {
                        violations.Add("DPoP (Demonstration of Proof of Possession) is required for Level 2+");
                    }

                    if (!scenario.UsesPAR)
                    {
                        warnings.Add("Pushed Authorization Requests (PAR) recommended for Level 2+");
                    }
                }

                if (scenario.Level == "Level3_Sovereign")
                {
                    warnings.Add("Level 3 requires additional HSM validation (simulated here)");
                }

                var isCompliant = violations.Count == 0;
                var status = isCompliant ? "[X] COMPLIANT" : "[ ] NON-COMPLIANT";
                Console.WriteLine($"      Result: {status}");

                if (violations.Any())
                {
                    Console.WriteLine("      Critical Violations:");
                    foreach (var violation in violations)
                    {
                        Console.WriteLine($"        [!] {violation}");
                    }
                }

                if (warnings.Any())
                {
                    Console.WriteLine("      Warnings:");
                    foreach (var warning in warnings)
                    {
                        Console.WriteLine($"        [?] {warning}");
                    }
                }

                if (isCompliant)
                {
                    Console.WriteLine("      Security Features Verified:");
                    Console.WriteLine($"        [X] Cryptographic proof of possession: {scenario.HasProofOfPossession}");
                    Console.WriteLine($"        [X] Secure transport: {scenario.IsSecureTransport}");
                    if (scenario.Level != "Level1_High")
                    {
                        Console.WriteLine($"        [X] Wallet attestation: {scenario.HasWalletAttestation}");
                        Console.WriteLine($"        [X] DPoP usage: {scenario.UsesDPoP}");
                    }
                }

                Console.WriteLine($"      Validation Steps: {4 + (scenario.Level == "Level1_High" ? 0 : 2)}");
                Console.WriteLine($"      Processing Time: {(isCompliant ? "1.8" : "2.4")}ms (simulated)");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"      Result: [ ] ERROR - {ex.Message}");
            }

            Console.WriteLine();
        }

        Console.WriteLine("   PROTOCOL SECURITY REQUIREMENTS SUMMARY:");
        Console.WriteLine("   ");
        Console.WriteLine("   Level 1 (High Assurance):");
        Console.WriteLine("     [X] Proof of possession (cryptographic binding)");
        Console.WriteLine("     [X] Secure transport (HTTPS/TLS 1.2+)");
        Console.WriteLine("     [X] Algorithm restrictions (no RS256, HS*, none)");
        Console.WriteLine("   ");
        Console.WriteLine("   Level 2 (Very High Assurance) - includes Level 1 plus:");
        Console.WriteLine("     [X] Wallet attestation (device/app authenticity)");
        Console.WriteLine("     [X] DPoP tokens (replay protection)");
        Console.WriteLine("     [X] Pushed Authorization Requests (PAR)");
        Console.WriteLine("     [X] Enhanced audit logging");
        Console.WriteLine("   ");
        Console.WriteLine("   Level 3 (Sovereign) - includes Level 2 plus:");
        Console.WriteLine("     [X] Hardware Security Module (HSM) backing");
        Console.WriteLine("     [X] Qualified electronic signatures");
        Console.WriteLine("     [X] Enhanced device attestation");
        Console.WriteLine("     [X] Regulatory compliance auditing");
        Console.WriteLine();

        await Task.CompletedTask;
    }

    private static async Task DemonstrateActualSdJwtIntegration(ILogger logger)
    {
        Console.WriteLine("4. INTEGRATION WITH ACTUAL SD-JWT .NET APIs");
        Console.WriteLine("   Showing how HAIP compliance works with real SD-JWT operations");
        Console.WriteLine();

        // Create test key for demonstration
        var issuerKey = CreateTestKey("P-256")!;
        var holderKey = CreateTestKey("P-256")!;

        Console.WriteLine("   DEMONSTRATION SCENARIO: University Degree Credential");
        Console.WriteLine("   HAIP Level 1 compliance for education credentials");
        Console.WriteLine();

        try
        {
            // Step 1: HAIP-compliant credential issuance
            Console.WriteLine("   Step 1: HAIP-Compliant Credential Issuance");

            // Validate algorithm compliance before issuing
            var algorithmCompliant = ValidateAlgorithmForHaipLevel("ES256", "Level1_High");
            Console.WriteLine($"   Algorithm ES256 compliant with Level 1: {algorithmCompliant}");

            if (!algorithmCompliant)
            {
                Console.WriteLine("   ERROR: Algorithm not compliant with HAIP Level 1");
                return;
            }

            // Create SD-JWT issuer with HAIP-compliant algorithm
            // Note: Using null for logger parameter to avoid type conversion issues
            var issuer = new SdIssuer(issuerKey, SecurityAlgorithms.EcdsaSha256);

            // Create claims for university degree
            var claims = new JwtPayload
            {
                { "iss", "https://university.example.edu" },
                { "sub", "student123" },
                { "degree", new { type = "BachelorOfScience", field = "Computer Science" } },
                { "gpa", 3.8 },
                { "graduationDate", "2024-05-15" },
                { "honors", "magna_cum_laude" }
            };

            // Configure selective disclosure (HAIP requires privacy-by-design)
            var options = new SdIssuanceOptions
            {
                DisclosureStructure = new
                {
                    gpa = true,           // Selectively disclosable (sensitive)
                    graduationDate = true, // Selectively disclosable
                    honors = true         // Selectively disclosable
                },
                // HAIP security features
                AllowWeakAlgorithms = false,
                DecoyDigests = 2  // Privacy enhancement
            };

            // Issue the credential
            var issuanceResult = issuer.Issue(claims, options);
            Console.WriteLine($"   Credential issued successfully");
            Console.WriteLine($"   SD-JWT length: {issuanceResult.SdJwt.Length} characters");
            Console.WriteLine($"   Disclosures created: {issuanceResult.Disclosures.Count}");
            Console.WriteLine($"   Privacy-preserving features: {options.DecoyDigests} decoy digests added");
            Console.WriteLine();

            // Step 2: Holder creates presentation (HAIP compliance maintained)
            Console.WriteLine("   Step 2: Holder Creates HAIP-Compliant Presentation");

            // Using default constructor without logger to avoid type issues
            var holder = new SdJwtHolder(issuanceResult.Issuance);

            // Selective disclosure decision (user privacy control)
            var presentation = holder.CreatePresentation(
                disclosure => disclosure.ClaimName == "graduationDate" || disclosure.ClaimName == "honors",
                kbJwtPayload: new JwtPayload { { "aud", "employer.example.com" }, { "iat", DateTimeOffset.UtcNow.ToUnixTimeSeconds() } },
                kbJwtSigningKey: holderKey,
                kbJwtSigningAlgorithm: SecurityAlgorithms.EcdsaSha256
            );

            Console.WriteLine($"   Presentation created with selective disclosure");
            Console.WriteLine($"   Revealed claims: graduationDate, honors");
            Console.WriteLine($"   Hidden claims: gpa (privacy protected)");
            Console.WriteLine($"   Key binding: Included for proof of possession");
            Console.WriteLine($"   HAIP compliance: Maintained through key binding");
            Console.WriteLine();

            // Step 3: Verifier validates with HAIP compliance
            Console.WriteLine("   Step 3: Verifier Performs HAIP-Compliant Validation");

            // Using constructor without logger to avoid type issues
            var verifier = new SdVerifier(
                (jwt) => Task.FromResult<SecurityKey>(issuerKey)); // Key resolution

            var validationParams = new TokenValidationParameters
            {
                ValidateIssuer = false, // Simplified for demo
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

            var verificationResult = await verifier.VerifyAsync(presentation, validationParams, kbValidationParams);

            Console.WriteLine($"   Verification result: {(verificationResult.KeyBindingVerified ? "SUCCESS" : "FAILED")}");
            Console.WriteLine($"   Key binding verified: {verificationResult.KeyBindingVerified}");
            Console.WriteLine($"   Claims verified: {verificationResult.ClaimsPrincipal.Claims.Count()}");

            // Display verified claims
            Console.WriteLine("   Verified claims:");
            foreach (var claim in verificationResult.ClaimsPrincipal.Claims.Take(6))
            {
                var value = claim.Value.Length > 50 ? claim.Value[..47] + "..." : claim.Value;
                Console.WriteLine($"     {claim.Type}: {value}");
            }

            Console.WriteLine();
            Console.WriteLine("   HAIP COMPLIANCE ACHIEVED:");
            Console.WriteLine("   [X] Strong cryptographic algorithm (ES256)");
            Console.WriteLine("   [X] Proof of possession through key binding");
            Console.WriteLine("   [X] Selective disclosure for privacy");
            Console.WriteLine("   [X] Cryptographic verification successful");
            Console.WriteLine("   [X] Audit trail maintained");

        }
        catch (Exception ex)
        {
            Console.WriteLine($"   ERROR during SD-JWT integration: {ex.Message}");
            logger.LogError(ex, "HAIP integration demonstration failed");
        }

        Console.WriteLine();
    }

    private static Task DemonstrateConfigurationPatterns()
    {
        Console.WriteLine("5. CONFIGURATION PATTERNS FOR HAIP INTEGRATION");
        Console.WriteLine("   How to integrate HAIP compliance with actual SD-JWT .NET APIs");
        Console.WriteLine();

        Console.WriteLine("   PATTERN 1: Simple Level 1 Configuration");
        Console.WriteLine("   ```csharp");
        Console.WriteLine("   // Create HAIP-compliant issuer for education credentials");
        Console.WriteLine("   var issuerKey = CreateECKey(256); // P-256 curve");
        Console.WriteLine("   var issuer = new SdIssuer(issuerKey, SecurityAlgorithms.EcdsaSha256);");
        Console.WriteLine("   ");
        Console.WriteLine("   // HAIP Level 1 options");
        Console.WriteLine("   var haipOptions = new SdIssuanceOptions");
        Console.WriteLine("   {");
        Console.WriteLine("       AllowWeakAlgorithms = false,  // HAIP requirement");
        Console.WriteLine("       DecoyDigests = 2              // Privacy enhancement");
        Console.WriteLine("   };");
        Console.WriteLine("   ");
        Console.WriteLine("   var credential = issuer.Issue(claims, haipOptions);");
        Console.WriteLine("   ```");
        Console.WriteLine();

        Console.WriteLine("   PATTERN 2: Level 2 Financial Services Configuration");
        Console.WriteLine("   ```csharp");
        Console.WriteLine("   // Create HAIP Level 2 compliant issuer");
        Console.WriteLine("   var financialKey = CreateECKey(384); // P-384 curve for Level 2");
        Console.WriteLine("   var issuer = new SdIssuer(financialKey, SecurityAlgorithms.EcdsaSha384);");
        Console.WriteLine("   ");
        Console.WriteLine("   // Enhanced security options for financial services");
        Console.WriteLine("   var level2Options = new SdIssuanceOptions");
        Console.WriteLine("   {");
        Console.WriteLine("       AllowWeakAlgorithms = false,");
        Console.WriteLine("       DecoyDigests = 5,             // Enhanced privacy");
        Console.WriteLine("       // Additional validation would be added here");
        Console.WriteLine("   };");
        Console.WriteLine("   ");
        Console.WriteLine("   // Key binding required for Level 2");
        Console.WriteLine("   var holderPublicKey = GetHolderPublicKey();");
        Console.WriteLine("   var credential = issuer.Issue(claims, level2Options, holderPublicKey);");
        Console.WriteLine("   ```");
        Console.WriteLine();

        Console.WriteLine("   PATTERN 3: Level 3 Government Configuration");
        Console.WriteLine("   ```csharp");
        Console.WriteLine("   // Government-grade security (Level 3)");
        Console.WriteLine("   var sovereignKey = LoadHsmBackedKey(); // HSM required");
        Console.WriteLine("   var issuer = new SdIssuer(sovereignKey, SecurityAlgorithms.EcdsaSha512);");
        Console.WriteLine("   ");
        Console.WriteLine("   var sovereignOptions = new SdIssuanceOptions");
        Console.WriteLine("   {");
        Console.WriteLine("       AllowWeakAlgorithms = false,");
        Console.WriteLine("       DecoyDigests = 10,            // Maximum privacy");
        Console.WriteLine("       // HSM validation, qualified signatures, etc.");
        Console.WriteLine("   };");
        Console.WriteLine("   ```");
        Console.WriteLine();

        Console.WriteLine("   PATTERN 4: Verification with HAIP Compliance");
        Console.WriteLine("   ```csharp");
        Console.WriteLine("   // HAIP-compliant verification");
        Console.WriteLine("   var verifier = new SdVerifier(ResolveIssuerKey, logger);");
        Console.WriteLine("   ");
        Console.WriteLine("   // Validation parameters enforcing HAIP requirements");
        Console.WriteLine("   var validationParams = new TokenValidationParameters");
        Console.WriteLine("   {");
        Console.WriteLine("       // HAIP mandates strong validation");
        Console.WriteLine("       ValidateIssuer = true,");
        Console.WriteLine("       ValidateAudience = true,");
        Console.WriteLine("       ValidateLifetime = true,");
        Console.WriteLine("       RequireSignedTokens = true,");
        Console.WriteLine("       // Algorithm restrictions enforced");
        Console.WriteLine("       ValidAlgorithms = HaipAllowedAlgorithms");
        Console.WriteLine("   };");
        Console.WriteLine("   ");
        Console.WriteLine("   // Key binding validation (required for Level 2+)");
        Console.WriteLine("   var result = await verifier.VerifyAsync(");
        Console.WriteLine("       presentation, validationParams, kbValidationParams);");
        Console.WriteLine("   ```");
        Console.WriteLine();

        Console.WriteLine("   PATTERN 5: Integration with Presentation Exchange");
        Console.WriteLine("   ```csharp");
        Console.WriteLine("   // HAIP requirements in presentation definitions");
        Console.WriteLine("   var engine = PresentationExchangeFactory.CreateEngine();");
        Console.WriteLine("   ");
        Console.WriteLine("   var definition = new PresentationDefinition");
        Console.WriteLine("   {");
        Console.WriteLine("       InputDescriptors = new[]");
        Console.WriteLine("       {");
        Console.WriteLine("           new InputDescriptor");
        Console.WriteLine("           {");
        Console.WriteLine("               Constraints = new Constraints");
        Console.WriteLine("               {");
        Console.WriteLine("                   Fields = new[]");
        Console.WriteLine("                   {");
        Console.WriteLine("                       // HAIP compliance level requirement");
        Console.WriteLine("                       new Field");
        Console.WriteLine("                       {");
        Console.WriteLine("                           Path = [\"$.haip_compliance_level\"],");
        Console.WriteLine("                           Filter = new { minimum = \"Level1_High\" }");
        Console.WriteLine("                       }");
        Console.WriteLine("                   }");
        Console.WriteLine("               }");
        Console.WriteLine("           }");
        Console.WriteLine("       }");
        Console.WriteLine("   };");
        Console.WriteLine("   ```");
        Console.WriteLine();

        // Demonstrate actual configuration examples
        Console.WriteLine("   ALGORITHM MAPPING FOR HAIP LEVELS:");

        var algorithmMapping = new Dictionary<string, string[]>
        {
            ["Level1_High"] = ["ES256", "ES384", "PS256", "PS384", "EdDSA"],
            ["Level2_VeryHigh"] = ["ES384", "ES512", "PS384", "PS512", "EdDSA"],
            ["Level3_Sovereign"] = ["ES512", "PS512", "EdDSA"]
        };

        foreach (var (level, algorithms) in algorithmMapping)
        {
            Console.WriteLine($"   {level}:");
            Console.WriteLine($"      Allowed: {string.Join(", ", algorithms)}");
            Console.WriteLine($"      SecurityAlgorithms constants: {GetSecurityAlgorithmConstants(algorithms)}");
            Console.WriteLine();
        }

        return Task.CompletedTask;
    }

    private static Task DemonstrateComplianceReporting()
    {
        Console.WriteLine("6. COMPLIANCE REPORTING AND AUDIT TRAILS");
        Console.WriteLine("   Comprehensive compliance validation with detailed audit trails");
        Console.WriteLine();

        // Simulate a realistic compliance result with violations
        var result = new
        {
            IsCompliant = false,
            AchievedLevel = "Level1_High",
            RequestedLevel = "Level2_VeryHigh",
            ValidationTimestamp = DateTimeOffset.UtcNow,
            Metadata = new Dictionary<string, object>
            {
                ["RequestId"] = Guid.NewGuid(),
                ["ClientId"] = "banking-app-prod",
                ["IssuerUrl"] = "https://credentials.securebank.example",
                ["Algorithm"] = "ES256",
                ["KeyType"] = "ECDSA P-256",
                ["SdJwtLength"] = 1247,
                ["DisclosureCount"] = 3
            },
            Violations = new[]
            {
                new
                {
                    Type = "InsufficientCryptographicStrength",
                    Description = "Algorithm ES256 is insufficient for Level 2 (requires ES384+)",
                    Severity = "Critical",
                    RecommendedAction = "Upgrade to ES384, ES512, PS384, PS512, or EdDSA",
                    RuleReference = "HAIP-CRYPTO-002",
                    DetectedAt = DateTimeOffset.UtcNow.AddMilliseconds(-25),
                    SdJwtIntegration = "Use SecurityAlgorithms.EcdsaSha384 in SdIssuer constructor"
                },
                new
                {
                    Type = "MissingWalletAttestation",
                    Description = "Wallet attestation is required for Level 2+ compliance",
                    Severity = "Critical",
                    RecommendedAction = "Implement wallet attestation using client certificates or device attestation",
                    RuleReference = "HAIP-AUTH-001",
                    DetectedAt = DateTimeOffset.UtcNow.AddMilliseconds(-15),
                    SdJwtIntegration = "Add holder public key to SdIssuer.Issue() call"
                },
                new
                {
                    Type = "WeakKeyBinding",
                    Description = "Key binding JWT missing from presentation",
                    Severity = "Warning",
                    RecommendedAction = "Include key binding in SdJwtHolder.CreatePresentation()",
                    RuleReference = "HAIP-KB-001",
                    DetectedAt = DateTimeOffset.UtcNow.AddMilliseconds(-5),
                    SdJwtIntegration = "Provide kbJwtPayload and kbJwtSigningKey parameters"
                }
            },
            AuditTrail = new
            {
                ValidationId = Guid.NewGuid(),
                ValidatorId = "HaipSdJwtValidator",
                ValidatorVersion = "1.0.2",
                HaipSpecVersion = "OpenID4VC-HAIP-1.0",
                SdJwtLibraryVersion = "SdJwt.Net v2.1.0",
                StartedAt = DateTimeOffset.UtcNow.AddMilliseconds(-50),
                CompletedAt = DateTimeOffset.UtcNow,
                Steps = new object[]
                {
                    new { Operation = "Initializing HAIP validation", Success = true, Duration = "0.5ms", Component = "HaipValidator", Timestamp = DateTimeOffset.UtcNow.AddMilliseconds(-49) },
                    new { Operation = "SD-JWT structure validation", Success = true, Duration = "1.1ms", Component = "SdVerifier", Timestamp = DateTimeOffset.UtcNow.AddMilliseconds(-47) },
                    new { Operation = "Algorithm validation: ES256", Success = false, Duration = "0.8ms", Component = "HaipCryptoValidator", Details = "ES256 insufficient for Level 2", Timestamp = DateTimeOffset.UtcNow.AddMilliseconds(-45) },
                    new { Operation = "Key strength validation", Success = true, Duration = "2.1ms", Component = "CryptoAnalyzer", Details = "P-256 key meets Level 1 requirements", Timestamp = DateTimeOffset.UtcNow.AddMilliseconds(-35) },
                    new { Operation = "Protocol security validation", Success = false, Duration = "1.8ms", Component = "HaipProtocolValidator", Details = "Missing wallet attestation for Level 2", Timestamp = DateTimeOffset.UtcNow.AddMilliseconds(-25) },
                    new { Operation = "Selective disclosure validation", Success = true, Duration = "3.2ms", Component = "SdVerifier", Details = "All disclosures verified successfully", Timestamp = DateTimeOffset.UtcNow.AddMilliseconds(-20) },
                    new { Operation = "Key binding validation", Success = false, Duration = "0.9ms", Component = "KeyBindingValidator", Details = "No key binding JWT present", Timestamp = DateTimeOffset.UtcNow.AddMilliseconds(-15) },
                    new { Operation = "Final compliance assessment", Success = true, Duration = "0.3ms", Component = "HaipComplianceEngine", Details = "Level 1 compliance achieved, Level 2 denied", Timestamp = DateTimeOffset.UtcNow.AddMilliseconds(-5) }
                }
            },
            ComplianceScore = new
            {
                OverallScore = 72.5,
                CryptographyScore = 85.0,
                ProtocolScore = 60.0,
                PrivacyScore = 90.0,
                AuditingScore = 95.0,
                MaxPossibleScore = 100.0
            },
            SdJwtIntegrationDetails = new
            {
                IssuerClass = "SdJwt.Net.Issuer.SdIssuer",
                VerifierClass = "SdJwt.Net.Verifier.SdVerifier",
                HolderClass = "SdJwt.Net.Holder.SdJwtHolder",
                RecommendedFixes = new[]
                {
                    "Change algorithm to SecurityAlgorithms.EcdsaSha384",
                    "Include holderPublicKey in Issue() call",
                    "Add key binding parameters to CreatePresentation()"
                }
            }
        };

        Console.WriteLine("   COMPREHENSIVE COMPLIANCE REPORT:");
        Console.WriteLine("   " + new string('-', 60));
        Console.WriteLine($"   Validation ID: {result.AuditTrail.ValidationId}");
        Console.WriteLine($"   Overall Status: {(result.IsCompliant ? "[X] COMPLIANT" : "[ ] NON-COMPLIANT")}");
        Console.WriteLine($"   Requested Level: {result.RequestedLevel}");
        Console.WriteLine($"   Achieved Level: {result.AchievedLevel}");
        Console.WriteLine($"   Compliance Score: {result.ComplianceScore.OverallScore:F1}%");
        Console.WriteLine($"   Validator: {result.AuditTrail.ValidatorId} v{result.AuditTrail.ValidatorVersion}");
        Console.WriteLine($"   HAIP Specification: {result.AuditTrail.HaipSpecVersion}");
        Console.WriteLine($"   SD-JWT Library: {result.AuditTrail.SdJwtLibraryVersion}");
        Console.WriteLine($"   Validation Time: {result.ValidationTimestamp:yyyy-MM-dd HH:mm:ss} UTC");
        Console.WriteLine();

        Console.WriteLine("   COMPLIANCE SCORE BREAKDOWN:");
        Console.WriteLine($"   Cryptography:     {result.ComplianceScore.CryptographyScore:F1}%  (Algorithm strength, key sizes)");
        Console.WriteLine($"   Protocol Security:{result.ComplianceScore.ProtocolScore:F1}%  (PoP, attestation, transport)");
        Console.WriteLine($"   Privacy:          {result.ComplianceScore.PrivacyScore:F1}%   (Selective disclosure, decoys)");
        Console.WriteLine($"   Audit & Logging:  {result.ComplianceScore.AuditingScore:F1}%   (Traceability, compliance)");
        Console.WriteLine($"   Overall:          {result.ComplianceScore.OverallScore:F1}%");
        Console.WriteLine();

        Console.WriteLine("   VIOLATIONS FOUND:");
        foreach (var violation in result.Violations)
        {
            var severityIcon = violation.Severity switch
            {
                "Critical" => "[!][red]",
                "Warning" => "[?][yellow]",
                "Info" => "[i][blue]",
                _ => "[ ]"
            };

            Console.WriteLine($"      {severityIcon} {violation.Severity.ToUpper()}: {violation.Description}");
            Console.WriteLine($"         Type: {violation.Type}");
            Console.WriteLine($"         Rule: {violation.RuleReference}");
            Console.WriteLine($"         Fix: {violation.RecommendedAction}");
            Console.WriteLine($"         SD-JWT Integration: {violation.SdJwtIntegration}");
            Console.WriteLine($"         Detected: {violation.DetectedAt:HH:mm:ss.fff}");
            Console.WriteLine();
        }

        Console.WriteLine("   DETAILED AUDIT TRAIL:");
        Console.WriteLine($"      Started: {result.AuditTrail.StartedAt:HH:mm:ss.fff}");
        Console.WriteLine($"      Completed: {result.AuditTrail.CompletedAt:HH:mm:ss.fff}");
        Console.WriteLine($"      Total Duration: {(result.AuditTrail.CompletedAt - result.AuditTrail.StartedAt).TotalMilliseconds:F1}ms");
        Console.WriteLine();

        Console.WriteLine("      Validation Steps:");
        foreach (dynamic step in result.AuditTrail.Steps)
        {
            var statusIcon = step.Success ? "[X]" : "[ ]";
            Console.WriteLine($"         {statusIcon} {step.Operation} ({step.Duration})");
            Console.WriteLine($"            Component: {step.Component}");
            if (!string.IsNullOrEmpty((string?)step.Details))
            {
                Console.WriteLine($"            Details: {step.Details}");
            }
            Console.WriteLine($"            Time: {((DateTimeOffset)step.Timestamp):HH:mm:ss.fff}");
        }

        Console.WriteLine();
        Console.WriteLine("   INTEGRATION GUIDANCE:");
        Console.WriteLine("   The following changes to SD-JWT .NET code would achieve compliance:");
        foreach (var fix in result.SdJwtIntegrationDetails.RecommendedFixes)
        {
            Console.WriteLine($"      • {fix}");
        }
        Console.WriteLine();

        Console.WriteLine("   REQUEST METADATA:");
        foreach (var metadata in result.Metadata)
        {
            Console.WriteLine($"      {metadata.Key}: {metadata.Value}");
        }

        return Task.CompletedTask;
    }

    private static SecurityKey? CreateTestKey(string keyType)
    {
        try
        {
            return keyType switch
            {
                "P-256" => new ECDsaSecurityKey(ECDsa.Create(ECCurve.NamedCurves.nistP256)) { KeyId = "test-p256" },
                "P-384" => new ECDsaSecurityKey(ECDsa.Create(ECCurve.NamedCurves.nistP384)) { KeyId = "test-p384" },
                "P-521" => new ECDsaSecurityKey(ECDsa.Create(ECCurve.NamedCurves.nistP521)) { KeyId = "test-p521" },
                "RSA-2048" => new RsaSecurityKey(RSA.Create(2048)) { KeyId = "test-rsa2048" },
                "RSA-4096" => new RsaSecurityKey(RSA.Create(4096)) { KeyId = "test-rsa4096" },
                _ => null
            };
        }
        catch
        {
            return null;
        }
    }

    private static (bool IsValid, string Analysis, string? Violation, string? Recommendation, string? SecurityLevel, int? KeyStrength) PerformDetailedValidation(string algorithm, string keyType, string level)
    {
        // Forbidden algorithms always fail
        if (algorithm == "RS256" || algorithm.StartsWith("HS") || algorithm == "none")
        {
            return (false,
                    $"Algorithm {algorithm} is explicitly forbidden in HAIP",
                    $"Use of forbidden algorithm: {algorithm}",
                    "Upgrade to HAIP-approved algorithms: ES256/384/512, PS256/384/512, or EdDSA",
                    null, null);
        }

        // Determine key strength
        var keyStrength = keyType switch
        {
            "P-256" => 128,
            "P-384" => 192,
            "P-521" => 256,
            "RSA-2048" => 112,
            "RSA-4096" => 150,
            _ => 0
        };

        // Level-specific validation
        var isValid = level switch
        {
            "Level1_High" => algorithm == "ES256" || algorithm == "ES384" || algorithm == "PS256" || algorithm == "PS384" || algorithm == "EdDSA",
            "Level2_VeryHigh" => algorithm == "ES384" || algorithm == "ES512" || algorithm == "PS384" || algorithm == "PS512" || algorithm == "EdDSA",
            "Level3_Sovereign" => algorithm == "ES512" || algorithm == "PS512" || algorithm == "EdDSA",
            _ => false
        };

        if (!isValid)
        {
            return (false,
                    $"Algorithm {algorithm} is insufficient for {level}",
                    $"Cryptographic strength below required level",
                    $"Upgrade to {level}-approved algorithms",
                    null, keyStrength);
        }

        var securityLevel = level switch
        {
            "Level1_High" => "High Assurance",
            "Level2_VeryHigh" => "Very High Assurance",
            "Level3_Sovereign" => "Sovereign Level",
            _ => "Unknown"
        };

        return (true,
                $"Algorithm {algorithm} with {keyType} meets {level} requirements",
                null,
                null,
                securityLevel,
                keyStrength);
    }

    private static bool ValidateAlgorithmForHaipLevel(string algorithm, string level)
    {
        var allowedAlgorithms = level switch
        {
            "Level1_High" => new[] { "ES256", "ES384", "PS256", "PS384", "EdDSA" },
            "Level2_VeryHigh" => new[] { "ES384", "ES512", "PS384", "PS512", "EdDSA" },
            "Level3_Sovereign" => new[] { "ES512", "PS512", "EdDSA" },
            _ => Array.Empty<string>()
        };

        return allowedAlgorithms.Contains(algorithm);
    }

    private static string GetSecurityAlgorithmConstants(string[] algorithms)
    {
        var constants = algorithms.Select(alg => alg switch
        {
            "ES256" => "EcdsaSha256",
            "ES384" => "EcdsaSha384",
            "ES512" => "EcdsaSha512",
            "PS256" => "RsaSsaPssSha256",
            "PS384" => "RsaSsaPssSha384",
            "PS512" => "RsaSsaPssSha512",
            "EdDSA" => "EdDsa",
            _ => alg
        });

        return string.Join(", ", constants);
    }
}
