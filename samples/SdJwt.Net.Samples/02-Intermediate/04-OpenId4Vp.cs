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
/// Tutorial 04: OpenID4VP (Verifiable Presentations)
///
/// LEARNING OBJECTIVES:
/// - Understand presentation request/response flow
/// - Work with presentation_definition
/// - Create VP tokens with selective disclosure
/// - Handle same-device and cross-device flows
///
/// TIME: ~15 minutes
/// </summary>
public static class OpenId4Vp
{
    public static async Task Run()
    {
        ConsoleHelpers.PrintHeader("Tutorial 04: OpenID4VP (Verifiable Presentations)");

        Console.WriteLine("OpenID for Verifiable Presentations (OID4VP) standardizes how");
        Console.WriteLine("verifiers request credentials and holders present them.");
        Console.WriteLine();

        // Setup - create issuer, holder, and verifier keys
        using var issuerEcdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        using var holderEcdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);

        var issuerKey = new ECDsaSecurityKey(issuerEcdsa) { KeyId = "university-2024" };
        var holderPrivateKey = new ECDsaSecurityKey(holderEcdsa) { KeyId = "alice-wallet" };
        var holderJwk = JsonWebKeyConverter.ConvertFromSecurityKey(
            new ECDsaSecurityKey(holderEcdsa) { KeyId = "alice-wallet" });

        // Issue a credential first (holder already has this)
        var vcIssuer = new SdJwtVcIssuer(issuerKey, SecurityAlgorithms.EcdsaSha256);
        var vcPayload = new SdJwtVcPayload
        {
            Issuer = "https://university.example.edu",
            Subject = "did:example:alice",
            IssuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            ExpiresAt = DateTimeOffset.UtcNow.AddYears(4).ToUnixTimeSeconds(),
            AdditionalData = new Dictionary<string, object>
            {
                ["student_name"] = "Alice Johnson",
                ["degree"] = "Bachelor of Science",
                ["major"] = "Computer Science",
                ["graduation_date"] = "2024-06-15",
                ["gpa"] = 3.85
            }
        };
        var sdOptions = new SdIssuanceOptions
        {
            DisclosureStructure = new { gpa = true, student_name = true }
        };
        var credentialResult = vcIssuer.Issue(
            "https://credentials.university.example.edu/UniversityDegree",
            vcPayload, sdOptions, holderJwk);

        // =====================================================================
        // STEP 1: Protocol overview
        // =====================================================================
        ConsoleHelpers.PrintStep(1, "OID4VP protocol overview");

        Console.WriteLine("Verifier requests credentials, wallet presents them:");
        Console.WriteLine();
        Console.WriteLine("  Wallet                                    Verifier");
        Console.WriteLine("    |                                          |");
        Console.WriteLine("    |<-- Authorization Request -----------------|");
        Console.WriteLine("    |    (presentation_definition)              |");
        Console.WriteLine("    |                                          |");
        Console.WriteLine("    |    [User reviews & approves]              |");
        Console.WriteLine("    |                                          |");
        Console.WriteLine("    |-- Authorization Response ----------------->|");
        Console.WriteLine("    |   (vp_token with SD-JWT presentation)     |");
        Console.WriteLine("    |                                          |");

        // =====================================================================
        // STEP 2: Authorization request
        // =====================================================================
        ConsoleHelpers.PrintStep(2, "Verifier creates authorization request");

        Console.WriteLine("Scenario: Employer verifies Alice's degree during hiring.");
        Console.WriteLine();

        var clientId = "https://employer.example.com";
        var nonce = $"job_app_{Guid.NewGuid():N}"[..24];
        var responseUri = "https://employer.example.com/callback";

        var authRequest = new
        {
            response_type = "vp_token",
            client_id = clientId,
            response_mode = "direct_post",
            response_uri = responseUri,
            nonce = nonce,
            presentation_definition = new
            {
                id = "degree-verification",
                input_descriptors = new[]
                {
                    new
                    {
                        id = "university_degree",
                        format = new
                        {
                            vc_sd_jwt = new { alg = new[] { "ES256" } }
                        },
                        constraints = new
                        {
                            fields = new object[]
                            {
                                new { path = new[] { "$.vct" }, filter = new { type = "string", pattern = ".*UniversityDegree" } },
                                new { path = new[] { "$.degree" } },
                                new { path = new[] { "$.graduation_date" } }
                            }
                        }
                    }
                }
            }
        };

        Console.WriteLine("Authorization request parameters:");
        Console.WriteLine($"  response_type: vp_token");
        Console.WriteLine($"  client_id: {clientId}");
        Console.WriteLine($"  response_mode: direct_post");
        Console.WriteLine($"  nonce: {nonce}");
        Console.WriteLine("  presentation_definition:");
        Console.WriteLine("    id: degree-verification");
        Console.WriteLine("    input_descriptors[0]:");
        Console.WriteLine("      - Requires UniversityDegree credential");
        Console.WriteLine("      - Must include: degree, graduation_date");
        Console.WriteLine("      - Format: vc+sd-jwt with ES256");

        // =====================================================================
        // STEP 3: Request delivery methods
        // =====================================================================
        ConsoleHelpers.PrintStep(3, "Request delivery methods");

        Console.WriteLine("SAME-DEVICE FLOW (redirect):");
        Console.WriteLine("  User clicks verify button, browser redirects to wallet app");
        Console.WriteLine("  openid4vp://authorize?client_id=...&request_uri=...");
        Console.WriteLine();
        Console.WriteLine("CROSS-DEVICE FLOW (QR code):");
        Console.WriteLine("  Verifier displays QR code, user scans with wallet");
        Console.WriteLine("  QR contains: openid4vp://authorize?request_uri=...");
        Console.WriteLine();
        Console.WriteLine("REQUEST_URI vs inline:");
        Console.WriteLine("  - request_uri: parameters fetched from URL (recommended)");
        Console.WriteLine("  - inline: all parameters in the initial request");

        // =====================================================================
        // STEP 4: Wallet processes request
        // =====================================================================
        ConsoleHelpers.PrintStep(4, "Wallet processes request");

        Console.WriteLine("Wallet performs these steps:");
        Console.WriteLine();
        Console.WriteLine("  1. Parse presentation_definition");
        Console.WriteLine("  2. Search credentials matching requirements:");

        // Simulate wallet searching credentials
        Console.WriteLine();
        Console.WriteLine("     Searching wallet for matching credentials...");
        Console.WriteLine("     [Match] UniversityDegree credential from https://university.example.edu");
        Console.WriteLine();
        Console.WriteLine("  3. Display consent screen to user:");
        Console.WriteLine("     +-----------------------------------------+");
        Console.WriteLine("     | Employer.example.com requests:          |");
        Console.WriteLine("     |                                         |");
        Console.WriteLine("     | From: University Degree                 |");
        Console.WriteLine("     |   - Degree type                         |");
        Console.WriteLine("     |   - Graduation date                     |");
        Console.WriteLine("     |                                         |");
        Console.WriteLine("     | [Deny]              [Allow]             |");
        Console.WriteLine("     +-----------------------------------------+");
        Console.WriteLine();
        Console.WriteLine("  4. User approves -> Create presentation");

        // =====================================================================
        // STEP 5: Create VP token
        // =====================================================================
        ConsoleHelpers.PrintStep(5, "Create VP token (selective presentation)");

        var holder = new SdJwtHolder(credentialResult.Issuance);

        // Create presentation disclosing only what's requested
        // Note: degree and graduation_date are always visible (not in _sd)
        // We don't disclose gpa or student_name
        var vpToken = holder.CreatePresentation(
            disclosure => false,  // Don't disclose any optional claims
            kbJwtPayload: new JwtPayload
            {
                [JwtRegisteredClaimNames.Aud] = clientId,
                [JwtRegisteredClaimNames.Iat] = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                ["nonce"] = nonce
            },
            kbJwtSigningKey: holderPrivateKey,
            kbJwtSigningAlgorithm: SecurityAlgorithms.EcdsaSha256
        );

        Console.WriteLine("VP token created:");
        Console.WriteLine("  Format: <SD-JWT>~<KB-JWT>");
        Console.WriteLine("  Disclosed claims: degree, major, graduation_date (always visible)");
        Console.WriteLine("  Hidden claims: gpa, student_name (not disclosed)");
        Console.WriteLine();
        Console.WriteLine("  KB-JWT binds presentation to this specific request:");
        Console.WriteLine($"    aud: {clientId}");
        Console.WriteLine($"    nonce: {nonce}");
        ConsoleHelpers.PrintPreview("  VP Token", vpToken, 60);

        // =====================================================================
        // STEP 6: Authorization response
        // =====================================================================
        ConsoleHelpers.PrintStep(6, "Send authorization response");

        Console.WriteLine("Wallet sends response to verifier's response_uri:");
        Console.WriteLine();
        Console.WriteLine($"  POST {responseUri}");
        Console.WriteLine("  Content-Type: application/x-www-form-urlencoded");
        Console.WriteLine();
        Console.WriteLine("  vp_token=<VP_TOKEN>");
        Console.WriteLine("  presentation_submission={");
        Console.WriteLine("    \"id\": \"submission-1\",");
        Console.WriteLine("    \"definition_id\": \"degree-verification\",");
        Console.WriteLine("    \"descriptor_map\": [{");
        Console.WriteLine("      \"id\": \"university_degree\",");
        Console.WriteLine("      \"format\": \"vc+sd-jwt\",");
        Console.WriteLine("      \"path\": \"$\"");
        Console.WriteLine("    }]");
        Console.WriteLine("  }");

        // =====================================================================
        // STEP 7: Verifier validates presentation
        // =====================================================================
        ConsoleHelpers.PrintStep(7, "Verifier validates presentation");

        var verifier = new SdVerifier(_ => Task.FromResult<SecurityKey>(issuerKey));

        var validationParams = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuers = new[] { "https://university.example.edu" },
            ValidateAudience = false,
            ValidateLifetime = true
        };

        Console.WriteLine("Verification checks:");
        Console.WriteLine("  1. Validate SD-JWT signature (issuer authenticity)");
        Console.WriteLine("  2. Check SD-JWT expiration");
        Console.WriteLine("  3. Validate KB-JWT signature (holder binding)");
        Console.WriteLine("  4. Verify KB-JWT audience matches client_id");
        Console.WriteLine("  5. Verify KB-JWT nonce matches request nonce");
        Console.WriteLine("  6. Extract and validate disclosed claims");
        Console.WriteLine();

        var result = await verifier.VerifyAsync(vpToken, validationParams);

        if (result.ClaimsPrincipal != null)
        {
            ConsoleHelpers.PrintSuccess("Presentation VERIFIED");
            Console.WriteLine();
            Console.WriteLine("  Verified claims available to employer:");
            var verifiedClaims = result.ClaimsPrincipal.Claims.ToDictionary(c => c.Type, c => c.Value);
            if (verifiedClaims.TryGetValue("degree", out var degree))
                Console.WriteLine($"    degree: {degree}");
            if (verifiedClaims.TryGetValue("major", out var major))
                Console.WriteLine($"    major: {major}");
            if (verifiedClaims.TryGetValue("graduation_date", out var gradDate))
                Console.WriteLine($"    graduation_date: {gradDate}");
            Console.WriteLine();
            Console.WriteLine("  NOT available (not disclosed):");
            Console.WriteLine("    gpa: [HIDDEN]");
            Console.WriteLine("    student_name: [HIDDEN]");
        }

        // =====================================================================
        // STEP 8: Response modes
        // =====================================================================
        ConsoleHelpers.PrintStep(8, "Response modes");

        Console.WriteLine("OID4VP supports multiple response delivery methods:");
        Console.WriteLine();
        Console.WriteLine("  direct_post (recommended for cross-device):");
        Console.WriteLine("    - Wallet POSTs directly to verifier backend");
        Console.WriteLine("    - Works with QR code flows");
        Console.WriteLine();
        Console.WriteLine("  fragment (same-device):");
        Console.WriteLine("    - Response in URL fragment after redirect");
        Console.WriteLine("    - Good for browser-based wallets");
        Console.WriteLine();
        Console.WriteLine("  direct_post.jwt (encrypted response):");
        Console.WriteLine("    - Response encrypted to verifier's key");
        Console.WriteLine("    - Maximum privacy for sensitive credentials");

        // =====================================================================
        // COMPLETION
        // =====================================================================
        ConsoleHelpers.PrintCompletion("Tutorial 04: OpenID4VP", new[]
        {
            "Understood OID4VP request/response flow",
            "Created authorization request with presentation_definition",
            "Built VP token with selective disclosure",
            "Verified presentation as verifier",
            "Learned response modes and delivery methods"
        });

        Console.WriteLine();
        Console.WriteLine("NEXT: Tutorial 05 - Presentation Exchange for complex queries");
    }
}
