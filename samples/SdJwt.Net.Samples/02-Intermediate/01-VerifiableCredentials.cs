using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.Holder;
using SdJwt.Net.Issuer;
using SdJwt.Net.Models;
using SdJwt.Net.Samples.Shared;
using SdJwt.Net.Vc.Issuer;
using SdJwt.Net.Vc.Models;
using SdJwt.Net.Verifier;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text.Json;

namespace SdJwt.Net.Samples.Intermediate;

/// <summary>
/// Tutorial 01: Verifiable Credentials (SD-JWT VC)
///
/// LEARNING OBJECTIVES:
/// - Understand SD-JWT VC format vs core SD-JWT
/// - Use SdJwtVcIssuer for standardized credentials
/// - Work with vct (Verifiable Credential Type)
/// - Create interoperable credentials
///
/// TIME: ~15 minutes
/// </summary>
public static class VerifiableCredentials
{
    public static async Task Run()
    {
        ConsoleHelpers.PrintHeader("Tutorial 01: Verifiable Credentials (SD-JWT VC)");

        Console.WriteLine("SD-JWT VC extends core SD-JWT with standardized structure for");
        Console.WriteLine("interoperability across different identity systems.");
        Console.WriteLine();

        // Setup keys
        using var issuerEcdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        using var holderEcdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);

        var issuerKey = new ECDsaSecurityKey(issuerEcdsa) { KeyId = "university-2024" };
        var holderPrivateKey = new ECDsaSecurityKey(holderEcdsa) { KeyId = "alice-key" };
        var holderJwk = JsonWebKeyConverter.ConvertFromSecurityKey(
            new ECDsaSecurityKey(holderEcdsa) { KeyId = "alice-key" });

        // =====================================================================
        // STEP 1: SD-JWT VC vs Core SD-JWT
        // =====================================================================
        ConsoleHelpers.PrintStep(1, "SD-JWT VC vs Core SD-JWT");

        Console.WriteLine("CORE SD-JWT (RFC 9901):");
        Console.WriteLine("  - Generic selective disclosure for any JWT");
        Console.WriteLine("  - No standardized claim structure");
        Console.WriteLine("  - Flexible but less interoperable");
        Console.WriteLine();
        Console.WriteLine("SD-JWT VC (draft-15):");
        Console.WriteLine("  - Specialized for Verifiable Credentials");
        Console.WriteLine("  - Standardized claims: vct, iss, sub, iat, exp");
        Console.WriteLine("  - Type system via 'vct' claim");
        Console.WriteLine("  - Interoperable with EUDI Wallet, OpenID4VC ecosystem");

        // =====================================================================
        // STEP 2: Create credential with SdJwtVcIssuer
        // =====================================================================
        ConsoleHelpers.PrintStep(2, "Create credential with SdJwtVcIssuer");

        Console.WriteLine("Using SdJwtVcIssuer ensures proper VC structure.");

        var vcIssuer = new SdJwtVcIssuer(issuerKey, SecurityAlgorithms.EcdsaSha256);

        // Define the credential payload using SdJwtVcPayload
        var vcPayload = new SdJwtVcPayload
        {
            // Standard VC claims (handled automatically)
            Issuer = "https://university.example.edu",
            Subject = "did:example:alice123",
            IssuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            ExpiresAt = DateTimeOffset.UtcNow.AddYears(4).ToUnixTimeSeconds(),

            // Custom claims for this credential type
            AdditionalData = new Dictionary<string, object>
            {
                ["student_name"] = "Alice Johnson",
                ["student_id"] = "STU-2024-001",
                ["degree"] = "Bachelor of Science",
                ["major"] = "Computer Science",
                ["graduation_date"] = "2024-06-15",
                ["gpa"] = 3.85,
                ["honors"] = "magna cum laude"
            }
        };

        // Define selective disclosure structure
        var sdOptions = new SdIssuanceOptions
        {
            DisclosureStructure = new
            {
                student_id = true,
                gpa = true,
                honors = true
            }
        };

        // Issue with vct (Verifiable Credential Type)
        var vctUri = "https://credentials.university.example.edu/UniversityDegree";
        var vcResult = vcIssuer.Issue(vctUri, vcPayload, sdOptions, holderJwk);

        ConsoleHelpers.PrintSuccess("SD-JWT VC issued");
        ConsoleHelpers.PrintKeyValue("vct", vctUri);
        ConsoleHelpers.PrintKeyValue("Disclosures", vcResult.Disclosures.Count);

        // =====================================================================
        // STEP 3: Understand the vct claim
        // =====================================================================
        ConsoleHelpers.PrintStep(3, "Understanding the vct (Verifiable Credential Type)");

        Console.WriteLine("The 'vct' claim identifies the credential type:");
        Console.WriteLine();
        Console.WriteLine($"  vct: \"{vctUri}\"");
        Console.WriteLine();
        Console.WriteLine("This enables:");
        Console.WriteLine("  - Schema discovery (what claims should be present)");
        Console.WriteLine("  - Credential categorization in wallets");
        Console.WriteLine("  - Verifier policy matching");
        Console.WriteLine("  - UI rendering hints");
        Console.WriteLine();
        Console.WriteLine("Common vct patterns:");
        Console.WriteLine("  - https://credentials.example.com/EmploymentCredential");
        Console.WriteLine("  - urn:eu.europa.ec.eudi:pid:1");
        Console.WriteLine("  - https://example.org/credentials/DriverLicense");

        // =====================================================================
        // STEP 4: Examine the VC structure
        // =====================================================================
        ConsoleHelpers.PrintStep(4, "Examine the SD-JWT VC structure");

        var parts = vcResult.Issuance.Split('~');
        var jwtPart = parts[0];
        var payloadBase64 = jwtPart.Split('.')[1];

        // Decode payload for display
        var payloadBytes = Base64UrlEncoder.DecodeBytes(payloadBase64);
        var payloadJson = JsonDocument.Parse(payloadBytes);

        Console.WriteLine("SD-JWT VC payload structure:");
        Console.WriteLine("  {");
        Console.WriteLine($"    \"vct\": \"{vctUri}\",");
        Console.WriteLine("    \"iss\": \"https://university.example.edu\",");
        Console.WriteLine("    \"sub\": \"did:example:alice123\",");
        Console.WriteLine("    \"iat\": <timestamp>,");
        Console.WriteLine("    \"exp\": <timestamp>,");
        Console.WriteLine("    \"cnf\": { \"jwk\": { ... } },  // Holder binding");
        Console.WriteLine("    ");
        Console.WriteLine("    // Always visible claims");
        Console.WriteLine("    \"student_name\": \"Alice Johnson\",");
        Console.WriteLine("    \"degree\": \"Bachelor of Science\",");
        Console.WriteLine("    \"major\": \"Computer Science\",");
        Console.WriteLine("    \"graduation_date\": \"2024-06-15\",");
        Console.WriteLine("    ");
        Console.WriteLine("    // Selectively disclosable (digests in _sd)");
        Console.WriteLine("    \"_sd\": [\"<hash_student_id>\", \"<hash_gpa>\", \"<hash_honors>\"]");
        Console.WriteLine("  }");

        // =====================================================================
        // STEP 5: Create presentation
        // =====================================================================
        ConsoleHelpers.PrintStep(5, "Create selective presentation");

        Console.WriteLine("Scenario: Employer wants to verify degree (not GPA)");

        var holder = new SdJwtHolder(vcResult.Issuance);

        // Reveal only what's needed for degree verification
        var presentation = holder.CreatePresentation(
            disclosure => disclosure.ClaimName == "honors",  // Show honors, hide GPA
            kbJwtPayload: new JwtPayload
            {
                [JwtRegisteredClaimNames.Aud] = "https://employer.example.com",
                [JwtRegisteredClaimNames.Iat] = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                ["nonce"] = "job_app_12345"
            },
            kbJwtSigningKey: holderPrivateKey,
            kbJwtSigningAlgorithm: SecurityAlgorithms.EcdsaSha256
        );

        ConsoleHelpers.PrintSuccess("Presentation created");
        Console.WriteLine("  Revealed: student_name, degree, major, graduation_date, honors");
        Console.WriteLine("  Hidden: student_id, gpa");

        // =====================================================================
        // STEP 6: Verify and extract vct
        // =====================================================================
        ConsoleHelpers.PrintStep(6, "Verify and check credential type");

        var verifier = new SdVerifier(_ => Task.FromResult<SecurityKey>(issuerKey));

        var validationParams = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuers = new[] { "https://university.example.edu" },
            ValidateAudience = false,
            ValidateLifetime = true
        };

        var result = await verifier.VerifyAsync(presentation, validationParams);

        if (result.ClaimsPrincipal != null)
        {
            ConsoleHelpers.PrintSuccess("Credential verified");

            var verifiedClaims = result.ClaimsPrincipal.Claims.ToDictionary(c => c.Type, c => c.Value);

            // Check vct to ensure it's the expected credential type
            if (verifiedClaims.TryGetValue("vct", out var vct))
            {
                Console.WriteLine($"  Credential type: {vct}");

                // Verifier can make decisions based on vct
                if (vct == vctUri)
                {
                    Console.WriteLine("  [OK] Expected credential type");
                }
            }

            Console.WriteLine();
            Console.WriteLine("  Verified claims:");
            var displayClaims = new[] { "student_name", "degree", "major", "graduation_date", "honors" };
            foreach (var claim in displayClaims)
            {
                if (verifiedClaims.TryGetValue(claim, out var value))
                {
                    Console.WriteLine($"    {claim}: {value}");
                }
            }
        }

        // =====================================================================
        // STEP 7: Multiple credential types
        // =====================================================================
        ConsoleHelpers.PrintStep(7, "Working with multiple credential types");

        Console.WriteLine("A wallet may hold many credential types:");
        Console.WriteLine();
        Console.WriteLine("  Credential 1:");
        Console.WriteLine("    vct: https://credentials.university.example.edu/UniversityDegree");
        Console.WriteLine("    claims: degree, major, graduation_date, gpa");
        Console.WriteLine();
        Console.WriteLine("  Credential 2:");
        Console.WriteLine("    vct: https://credentials.techcorp.example.com/Employment");
        Console.WriteLine("    claims: job_title, department, start_date, salary_band");
        Console.WriteLine();
        Console.WriteLine("  Credential 3:");
        Console.WriteLine("    vct: urn:eu.europa.ec.eudi:pid:1");
        Console.WriteLine("    claims: given_name, family_name, birth_date, nationality");
        Console.WriteLine();
        Console.WriteLine("Presentation Exchange (Tutorial 05) helps verifiers request");
        Console.WriteLine("specific credential types with required claims.");

        // =====================================================================
        // COMPLETION
        // =====================================================================
        ConsoleHelpers.PrintCompletion("Tutorial 01: Verifiable Credentials", new[]
        {
            "Understood SD-JWT VC vs core SD-JWT",
            "Used SdJwtVcIssuer for standardized credentials",
            "Learned vct (Verifiable Credential Type)",
            "Created and verified SD-JWT VC presentation",
            "Explored multi-credential scenarios"
        });

        Console.WriteLine();
        Console.WriteLine("NEXT: Tutorial 02 - Status Lists for revocation");
    }
}
