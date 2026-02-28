using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.Holder;
using SdJwt.Net.Issuer;
using SdJwt.Net.Models;
using SdJwt.Net.Verifier;
using SdJwt.Net.Samples.Shared;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text.Json;

namespace SdJwt.Net.Samples.Beginner;

/// <summary>
/// Tutorial 04: Complete Verification Flow
///
/// LEARNING OBJECTIVES:
/// - Implement full issuer -> holder -> verifier flow
/// - Use SdVerifier for cryptographic validation
/// - Handle verification results and errors
/// - Understand what the verifier sees
///
/// TIME: ~15 minutes
/// </summary>
public static class VerificationFlow
{
    public static async Task Run()
    {
        ConsoleHelpers.PrintHeader("Tutorial 04: Complete Verification Flow");

        Console.WriteLine("In this tutorial, you'll implement the complete SD-JWT flow:");
        Console.WriteLine("  1. Issuer creates credential");
        Console.WriteLine("  2. Holder stores and selectively presents");
        Console.WriteLine("  3. Verifier validates and extracts claims");
        Console.WriteLine();

        // =====================================================================
        // SETUP: Create keys for all parties
        // =====================================================================
        ConsoleHelpers.PrintStep(0, "Setup: Create keys for all parties");

        using var issuerEcdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        using var holderEcdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);

        var issuerKey = new ECDsaSecurityKey(issuerEcdsa) { KeyId = "employer-hr-2024" };
        var holderPrivateKey = new ECDsaSecurityKey(holderEcdsa) { KeyId = "alice-key" };
        var holderPublicKey = new ECDsaSecurityKey(holderEcdsa) { KeyId = "alice-key" };
        var holderJwk = JsonWebKeyConverter.ConvertFromSecurityKey(holderPublicKey);

        ConsoleHelpers.PrintSuccess("Keys created for issuer, holder");

        // =====================================================================
        // STEP 1: ISSUER - Create employment credential
        // =====================================================================
        ConsoleHelpers.PrintStep(1, "ISSUER: Create employment verification credential");

        Console.WriteLine("Scenario: TechCorp HR issues employment verification for Alice");
        Console.WriteLine();

        var claims = new JwtPayload
        {
            [JwtRegisteredClaimNames.Iss] = "https://hr.techcorp.example.com",
            [JwtRegisteredClaimNames.Sub] = "emp_alice_12345",
            [JwtRegisteredClaimNames.Iat] = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            [JwtRegisteredClaimNames.Exp] = DateTimeOffset.UtcNow.AddMonths(6).ToUnixTimeSeconds(),
            // Employment data
            ["employee_name"] = "Alice Johnson",
            ["employee_id"] = "EMP-2024-001",
            ["job_title"] = "Senior Software Engineer",
            ["department"] = "Engineering",
            ["start_date"] = "2022-01-15",
            ["salary_band"] = "L5",
            ["manager_name"] = "Bob Smith",
            ["employment_status"] = "Active"
        };

        var issuer = new SdIssuer(issuerKey, SecurityAlgorithms.EcdsaSha256);

        var options = new SdIssuanceOptions
        {
            DisclosureStructure = new
            {
                employee_name = true,
                employee_id = true,
                salary_band = true,      // Highly sensitive
                manager_name = true,
                start_date = true
            }
        };

        var issuanceResult = issuer.Issue(claims, options, holderJwk);

        ConsoleHelpers.PrintSuccess("Employment credential issued");
        ConsoleHelpers.PrintKeyValue("Disclosures created", issuanceResult.Disclosures.Count);
        Console.WriteLine();
        Console.WriteLine("  Always visible claims:");
        Console.WriteLine("    - job_title: Senior Software Engineer");
        Console.WriteLine("    - department: Engineering");
        Console.WriteLine("    - employment_status: Active");
        Console.WriteLine();
        Console.WriteLine("  Selectively disclosable claims:");
        foreach (var d in issuanceResult.Disclosures)
        {
            Console.WriteLine($"    - {d.ClaimName}");
        }

        // =====================================================================
        // STEP 2: HOLDER - Store and prepare presentation
        // =====================================================================
        ConsoleHelpers.PrintStep(2, "HOLDER: Store credential and prepare presentation");

        Console.WriteLine("Scenario: Alice applies for a mortgage, bank needs employment proof");
        Console.WriteLine();
        Console.WriteLine("Bank's requirements (what they asked for):");
        Console.WriteLine("  - Proof of current employment");
        Console.WriteLine("  - Job title");
        Console.WriteLine("  - Employment start date (tenure check)");
        Console.WriteLine();
        Console.WriteLine("Bank does NOT need:");
        Console.WriteLine("  - Employee ID");
        Console.WriteLine("  - Salary band (separate income verification)");
        Console.WriteLine("  - Manager name");

        var holder = new SdJwtHolder(issuanceResult.Issuance);

        // Alice selects which claims to reveal
        var presentation = holder.CreatePresentation(
            disclosure => disclosure.ClaimName == "employee_name" ||
                         disclosure.ClaimName == "start_date",
            kbJwtPayload: new JwtPayload
            {
                [JwtRegisteredClaimNames.Aud] = "https://mortgage.bank.example.com",
                [JwtRegisteredClaimNames.Iat] = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                ["nonce"] = "mortgage_app_2024_xyz"
            },
            kbJwtSigningKey: holderPrivateKey,
            kbJwtSigningAlgorithm: SecurityAlgorithms.EcdsaSha256
        );

        ConsoleHelpers.PrintSuccess("Presentation created with selective disclosure");
        ConsoleHelpers.PrintKeyValue("Claims revealed", "employee_name, start_date");
        ConsoleHelpers.PrintKeyValue("Claims hidden", "employee_id, salary_band, manager_name");

        // =====================================================================
        // STEP 3: VERIFIER - Validate and extract claims
        // =====================================================================
        ConsoleHelpers.PrintStep(3, "VERIFIER: Validate presentation and extract claims");

        Console.WriteLine("The bank's verification system performs these checks:");
        Console.WriteLine();

        // Create verifier with key resolver
        var verifier = new SdVerifier(issuerClaim =>
        {
            // In production, this would look up the issuer's public key
            // from a trusted registry or federation
            Console.WriteLine($"    Looking up public key for: {issuerClaim}");
            return Task.FromResult<SecurityKey>(issuerKey);
        });

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuers = new[] { "https://hr.techcorp.example.com" },
            ValidateAudience = false,  // Audience is in KB-JWT, not SD-JWT
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(5)
        };

        Console.WriteLine("  1. Resolving issuer's public key...");
        Console.WriteLine("  2. Validating SD-JWT signature...");
        Console.WriteLine("  3. Checking expiration...");
        Console.WriteLine("  4. Processing disclosures...");
        Console.WriteLine("  5. Validating key binding (KB-JWT)...");
        Console.WriteLine();

        var verificationResult = await verifier.VerifyAsync(
            presentation,
            validationParameters);

        if (verificationResult.ClaimsPrincipal != null)
        {
            ConsoleHelpers.PrintSuccess("Presentation VERIFIED successfully!");
            Console.WriteLine();

            // Extract the verified claims
            Console.WriteLine("  Verified claims available to bank:");
            Console.WriteLine("  {");

            var verifiedClaims = verificationResult.ClaimsPrincipal.Claims.ToDictionary(c => c.Type, c => c.Value);
            var relevantClaims = new[] { "iss", "job_title", "department", "employment_status", "employee_name", "start_date" };

            foreach (var claimName in relevantClaims)
            {
                if (verifiedClaims.TryGetValue(claimName, out var value))
                {
                    var displayValue = long.TryParse(value, out var l)
                        ? DateTimeOffset.FromUnixTimeSeconds(l).ToString("yyyy-MM-dd HH:mm:ss")
                        : value ?? "null";
                    Console.WriteLine($"    \"{claimName}\": \"{displayValue}\",");
                }
            }
            Console.WriteLine("  }");

            Console.WriteLine();
            Console.WriteLine("  Claims NOT available (not disclosed):");
            Console.WriteLine("    - employee_id: [HIDDEN]");
            Console.WriteLine("    - salary_band: [HIDDEN]");
            Console.WriteLine("    - manager_name: [HIDDEN]");
        }
        else
        {
            ConsoleHelpers.PrintError("Verification FAILED!");
            Console.WriteLine("  Reason: Verification returned null ClaimsPrincipal");
        }

        // =====================================================================
        // STEP 4: Understanding the security guarantees
        // =====================================================================
        ConsoleHelpers.PrintStep(4, "Security guarantees from verification");

        Console.WriteLine("After successful verification, the bank can trust:");
        Console.WriteLine();
        Console.WriteLine("  AUTHENTICITY:");
        Console.WriteLine("    The credential was issued by TechCorp HR");
        Console.WriteLine("    (verified by issuer's digital signature)");
        Console.WriteLine();
        Console.WriteLine("  INTEGRITY:");
        Console.WriteLine("    The claims have not been modified since issuance");
        Console.WriteLine("    (any tampering would break the signature)");
        Console.WriteLine();
        Console.WriteLine("  BINDING:");
        Console.WriteLine("    The presenter (Alice) is the legitimate holder");
        Console.WriteLine("    (verified by KB-JWT signed with her private key)");
        Console.WriteLine();
        Console.WriteLine("  FRESHNESS:");
        Console.WriteLine("    The presentation was created for this specific request");
        Console.WriteLine("    (nonce and iat in KB-JWT prevent replay attacks)");
        Console.WriteLine();
        Console.WriteLine("  MINIMIZATION:");
        Console.WriteLine("    Only requested claims were disclosed");
        Console.WriteLine("    (bank cannot see salary_band, manager_name, etc.)");

        // =====================================================================
        // STEP 5: Error handling examples
        // =====================================================================
        ConsoleHelpers.PrintStep(5, "Common verification errors");

        Console.WriteLine("Here are common reasons verification might fail:");
        Console.WriteLine();
        Console.WriteLine("  EXPIRED_TOKEN:");
        Console.WriteLine("    The SD-JWT has passed its exp (expiration) time");
        Console.WriteLine("    Solution: Request fresh credential from issuer");
        Console.WriteLine();
        Console.WriteLine("  INVALID_SIGNATURE:");
        Console.WriteLine("    SD-JWT signature doesn't match issuer's public key");
        Console.WriteLine("    Cause: Wrong key, tampering, or corrupt token");
        Console.WriteLine();
        Console.WriteLine("  INVALID_KEY_BINDING:");
        Console.WriteLine("    KB-JWT signature doesn't match cnf.jwk");
        Console.WriteLine("    Cause: Wrong holder key or stolen credential attempt");
        Console.WriteLine();
        Console.WriteLine("  UNKNOWN_ISSUER:");
        Console.WriteLine("    Issuer not in verifier's trusted list");
        Console.WriteLine("    Solution: Add issuer to ValidIssuers or check federation");
        Console.WriteLine();
        Console.WriteLine("  REPLAY_DETECTED:");
        Console.WriteLine("    Nonce already used or iat too old");
        Console.WriteLine("    Cause: Replay attack or slow presentation");

        // =====================================================================
        // COMPLETION
        // =====================================================================
        ConsoleHelpers.PrintCompletion("Tutorial 04: Complete Verification Flow", new[]
        {
            "Issued SD-JWT with selective disclosure",
            "Created holder presentation with KB-JWT",
            "Verified presentation with SdVerifier",
            "Extracted verified claims",
            "Understood security guarantees and errors"
        });

        Console.WriteLine();
        Console.WriteLine("CONGRATULATIONS! You've completed the Beginner Tutorials!");
        Console.WriteLine();
        Console.WriteLine("NEXT STEPS:");
        Console.WriteLine("  - 02-Intermediate: Verifiable Credentials and protocols");
        Console.WriteLine("  - 03-Advanced: Federation and HAIP compliance");
        Console.WriteLine("  - 04-UseCases: Real-world production patterns");
    }
}
