using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.HAIP;
using SdJwt.Net.HAIP.Validators;
using SdJwt.Net.Issuer;
using SdJwt.Net.Models;
using SdJwt.Net.Samples.Shared;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;

namespace SdJwt.Net.Samples.Advanced;

/// <summary>
/// Tutorial 02: HAIP Compliance
///
/// LEARNING OBJECTIVES:
/// - Understand HAIP security levels
/// - Validate algorithm compliance
/// - Enforce key size requirements
/// - Apply HAIP to credential workflows
///
/// TIME: ~15 minutes
/// </summary>
public static class HaipCompliance
{
    public static Task Run()
    {
        ConsoleHelpers.PrintHeader("Tutorial 02: HAIP Compliance (High Assurance Interoperability Profile)");

        // Create logger for HAIP validator
        using var loggerFactory = LoggerFactory.Create(builder => builder.SetMinimumLevel(LogLevel.Warning));
        var logger = loggerFactory.CreateLogger<HaipCryptoValidator>();

        Console.WriteLine("HAIP defines security requirements for high-value credential systems");
        Console.WriteLine("like government IDs and financial credentials. It ensures");
        Console.WriteLine("interoperability while mandating strong cryptography.");
        Console.WriteLine();

        // =====================================================================
        // STEP 1: Why HAIP?
        // =====================================================================
        ConsoleHelpers.PrintStep(1, "Why HAIP?");

        Console.WriteLine("Not all credentials need the same security level:");
        Console.WriteLine();
        Console.WriteLine("  Newsletter subscription: Basic security sufficient");
        Console.WriteLine("  Student discount card:   Standard security");
        Console.WriteLine("  Driver's license:        High assurance required");
        Console.WriteLine("  National ID:             Maximum assurance required");
        Console.WriteLine();
        Console.WriteLine("HAIP provides standardized security levels that:");
        Console.WriteLine("  - Government agencies can mandate");
        Console.WriteLine("  - Wallet vendors can implement");
        Console.WriteLine("  - Verifiers can require and check");

        // =====================================================================
        // STEP 2: Security levels
        // =====================================================================
        ConsoleHelpers.PrintStep(2, "HAIP security levels");

        Console.WriteLine("HAIP defines three security levels:");
        Console.WriteLine();
        Console.WriteLine("LEVEL 1 - Standard Assurance");
        Console.WriteLine("  Key type: P-256 (secp256r1)");
        Console.WriteLine("  Algorithm: ES256");
        Console.WriteLine("  Security: ~128-bit");
        Console.WriteLine("  Use case: General applications, commercial use");
        Console.WriteLine();
        Console.WriteLine("LEVEL 2 - High Assurance");
        Console.WriteLine("  Key type: P-384 (secp384r1)");
        Console.WriteLine("  Algorithm: ES384");
        Console.WriteLine("  Security: ~192-bit");
        Console.WriteLine("  Use case: Government, financial services");
        Console.WriteLine();
        Console.WriteLine("LEVEL 3 - Maximum Assurance");
        Console.WriteLine("  Key type: P-521 (secp521r1)");
        Console.WriteLine("  Algorithm: ES512");
        Console.WriteLine("  Security: ~256-bit");
        Console.WriteLine("  Use case: National security, highest-value assets");

        // =====================================================================
        // STEP 3: Algorithm restrictions
        // =====================================================================
        ConsoleHelpers.PrintStep(3, "Prohibited algorithms");

        Console.WriteLine("HAIP EXPLICITLY PROHIBITS weak or deprecated algorithms:");
        Console.WriteLine();
        Console.WriteLine("  FORBIDDEN:");
        Console.WriteLine("    MD5     - Completely broken, collision attacks trivial");
        Console.WriteLine("    SHA-1   - Deprecated, practical collision attacks exist");
        Console.WriteLine("    RS256   - RSA with SHA-256 (not EC-based)");
        Console.WriteLine();
        Console.WriteLine("  REQUIRED (ECDSA only):");
        Console.WriteLine("    ES256   - ECDSA with P-256 and SHA-256");
        Console.WriteLine("    ES384   - ECDSA with P-384 and SHA-384");
        Console.WriteLine("    ES512   - ECDSA with P-521 and SHA-512");
        Console.WriteLine();
        Console.WriteLine("The library enforces these restrictions at compile and runtime.");

        // =====================================================================
        // STEP 4: Using HaipCryptoValidator
        // =====================================================================
        ConsoleHelpers.PrintStep(4, "Using HaipCryptoValidator");

        Console.WriteLine("The HaipCryptoValidator class enforces compliance:");
        Console.WriteLine();

        // Valid Level 1 key
        using var p256 = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var p256Key = new ECDsaSecurityKey(p256) { KeyId = "key-p256" };

        // Valid Level 2 key
        using var p384 = ECDsa.Create(ECCurve.NamedCurves.nistP384);
        var p384Key = new ECDsaSecurityKey(p384) { KeyId = "key-p384" };

        // Valid Level 3 key
        using var p521 = ECDsa.Create(ECCurve.NamedCurves.nistP521);
        var p521Key = new ECDsaSecurityKey(p521) { KeyId = "key-p521" };

        // Create validators for each level
        var level1Validator = new HaipCryptoValidator(HaipLevel.Level1_High, logger);
        var level2Validator = new HaipCryptoValidator(HaipLevel.Level2_VeryHigh, logger);
        var level3Validator = new HaipCryptoValidator(HaipLevel.Level3_Sovereign, logger);

        Console.WriteLine("Checking algorithm compliance:");
        Console.WriteLine();

        // Check various algorithms using Level 1 validator
        var algorithms = new[] { "ES256", "ES384", "ES512", "RS256", "HS256" };
        foreach (var alg in algorithms)
        {
            var validationResult = level1Validator.ValidateAlgorithm(alg);
            var status = validationResult.IsValid ? "[  OK  ]" : "[REJECT]";
            Console.WriteLine($"  {status} {alg}");
        }

        // =====================================================================
        // STEP 5: Validating keys for security level
        // =====================================================================
        ConsoleHelpers.PrintStep(5, "Validating keys for security level");

        Console.WriteLine("HaipCryptoValidator.ValidateKeyCompliance validates key + algorithm:");
        Console.WriteLine();

        Console.WriteLine("P-256 key (ES256) validation:");
        ValidateAndPrint(level1Validator, p256Key, "ES256", "Level 1 (High)");
        ValidateAndPrint(level2Validator, p256Key, "ES256", "Level 2 (Very High)");
        Console.WriteLine();

        Console.WriteLine("P-384 key (ES384) validation:");
        ValidateAndPrint(level1Validator, p384Key, "ES384", "Level 1 (High)");
        ValidateAndPrint(level2Validator, p384Key, "ES384", "Level 2 (Very High)");
        ValidateAndPrint(level3Validator, p384Key, "ES384", "Level 3 (Sovereign)");
        Console.WriteLine();

        Console.WriteLine("P-521 key (ES512) validation:");
        ValidateAndPrint(level2Validator, p521Key, "ES512", "Level 2 (Very High)");
        ValidateAndPrint(level3Validator, p521Key, "ES512", "Level 3 (Sovereign)");

        // =====================================================================
        // STEP 6: HAIP-compliant issuance
        // =====================================================================
        ConsoleHelpers.PrintStep(6, "HAIP-compliant credential issuance");

        Console.WriteLine("Creating a Level 2 compliant SD-JWT:");
        Console.WriteLine();

        // Use P-384 key for Level 2 compliance
        var issuer = new SdIssuer(p384Key, SecurityAlgorithms.EcdsaSha384);

        var claims = new JwtPayload
        {
            [JwtRegisteredClaimNames.Iss] = "https://gov.example.eu/identity",
            [JwtRegisteredClaimNames.Sub] = "did:example:citizen123",
            [JwtRegisteredClaimNames.Iat] = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            [JwtRegisteredClaimNames.Exp] = DateTimeOffset.UtcNow.AddYears(10).ToUnixTimeSeconds(),
            ["given_name"] = "Alice",
            ["family_name"] = "Government",
            ["birthdate"] = "1990-01-15",
            ["nationality"] = "EU"
        };

        var options = new SdIssuanceOptions
        {
            DisclosureStructure = new
            {
                given_name = true,
                family_name = true,
                birthdate = true,
                nationality = true
            }
        };

        var result = issuer.Issue(claims, options);

        Console.WriteLine($"  Algorithm: ES384 (P-384 curve)");
        Console.WriteLine($"  Security level: Level 2 (Very High Assurance)");
        Console.WriteLine($"  Suitable for: Government ID, regulated credentials");
        ConsoleHelpers.PrintPreview("  SD-JWT", result.Issuance, 50);

        // =====================================================================
        // STEP 7: HAIP requirements for holders
        // =====================================================================
        ConsoleHelpers.PrintStep(7, "Holder key requirements");

        Console.WriteLine("HAIP also governs holder binding keys:");
        Console.WriteLine();
        Console.WriteLine("For Level 2 credentials:");
        Console.WriteLine("  - Holder key MUST be P-384 or stronger");
        Console.WriteLine("  - Key MUST be stored in secure element or TEE");
        Console.WriteLine("  - Key binding JWT MUST use ES384 or ES512");
        Console.WriteLine();
        Console.WriteLine("This ensures end-to-end high assurance:");
        Console.WriteLine("  Issuer (P-384) -> Credential -> Holder (P-384) -> Presentation");

        // =====================================================================
        // STEP 8: Verifier enforcement
        // =====================================================================
        ConsoleHelpers.PrintStep(8, "Verifier enforcement");

        Console.WriteLine("Verifiers can mandate HAIP compliance:");
        Console.WriteLine();
        Console.WriteLine("  // Create validator for required level");
        Console.WriteLine("  var validator = new HaipCryptoValidator(");
        Console.WriteLine("      HaipLevel.Level2_VeryHigh, logger);");
        Console.WriteLine();
        Console.WriteLine("  // Validate key and algorithm");
        Console.WriteLine("  var compliance = validator.ValidateKeyCompliance(key, algorithm);");
        Console.WriteLine();
        Console.WriteLine("  if (!compliance.IsCompliant)");
        Console.WriteLine("  {");
        Console.WriteLine("      foreach (var violation in compliance.Violations)");
        Console.WriteLine("          Console.WriteLine($\"Violation: {violation.Message}\");");
        Console.WriteLine("  }");

        // =====================================================================
        // STEP 9: Migration considerations
        // =====================================================================
        ConsoleHelpers.PrintStep(9, "Migration considerations");

        Console.WriteLine("When upgrading to higher security levels:");
        Console.WriteLine();
        Console.WriteLine("  1. Plan key rotation window");
        Console.WriteLine("     - Issue credentials with new algorithm");
        Console.WriteLine("     - Accept old credentials during transition");
        Console.WriteLine();
        Console.WriteLine("  2. Update all system components");
        Console.WriteLine("     - Issuer key infrastructure");
        Console.WriteLine("     - Wallet key storage");
        Console.WriteLine("     - Verifier validation logic");
        Console.WriteLine();
        Console.WriteLine("  3. Coordinate with federation");
        Console.WriteLine("     - Update entity statement metadata");
        Console.WriteLine("     - Apply metadata policies for new algorithms");
        Console.WriteLine();
        Console.WriteLine("  4. Communicate deprecation timeline");
        Console.WriteLine("     - Notify holders to upgrade credentials");
        Console.WriteLine("     - Set cut-off date for old credentials");

        // =====================================================================
        // COMPLETION
        // =====================================================================
        ConsoleHelpers.PrintCompletion("Tutorial 02: HAIP Compliance", new[]
        {
            "Understood HAIP security levels (Level1_High, Level2_VeryHigh, Level3_Sovereign)",
            "Learned prohibited and required algorithms",
            "Used HaipCryptoValidator for compliance checking",
            "Created Level 2 compliant credentials",
            "Saw enforcement patterns for verifiers"
        });

        Console.WriteLine();
        Console.WriteLine("NEXT: Tutorial 03 - Multi-Credential Flows");

        return Task.CompletedTask;
    }

    private static void ValidateAndPrint(HaipCryptoValidator validator, ECDsaSecurityKey key, string algorithm, string levelName)
    {
        var compliance = validator.ValidateKeyCompliance(key, algorithm);
        var status = compliance.IsCompliant ? "[  OK  ]" : "[REJECT]";
        Console.WriteLine($"  {status} {key.KeyId} with {algorithm} for {levelName}");
    }
}
