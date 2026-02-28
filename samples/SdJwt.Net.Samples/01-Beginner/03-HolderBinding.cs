using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.Holder;
using SdJwt.Net.Issuer;
using SdJwt.Net.Models;
using SdJwt.Net.Samples.Shared;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;

namespace SdJwt.Net.Samples.Beginner;

/// <summary>
/// Tutorial 03: Holder Binding (Key Binding)
///
/// LEARNING OBJECTIVES:
/// - Understand why key binding matters
/// - Create holder key pairs
/// - Embed holder public key in SD-JWT (cnf claim)
/// - Sign presentations with holder private key
///
/// TIME: ~10 minutes
/// </summary>
public static class HolderBinding
{
    public static Task Run()
    {
        ConsoleHelpers.PrintHeader("Tutorial 03: Holder Binding (Key Binding)");

        Console.WriteLine("In this tutorial, you'll learn how to bind an SD-JWT to a");
        Console.WriteLine("specific holder, preventing theft and unauthorized use.");
        Console.WriteLine();

        // =====================================================================
        // STEP 1: The problem - credential theft
        // =====================================================================
        ConsoleHelpers.PrintStep(1, "The problem: credential theft");

        Console.WriteLine("Without key binding, an SD-JWT is like a bearer token:");
        Console.WriteLine();
        Console.WriteLine("  Scenario: Alice receives her university degree SD-JWT");
        Console.WriteLine("  Problem:  Mallory steals Alice's SD-JWT");
        Console.WriteLine("  Risk:     Mallory can present Alice's degree as her own!");
        Console.WriteLine();
        Console.WriteLine("KEY BINDING solves this by:");
        Console.WriteLine("  1. Issuer embeds Alice's PUBLIC key in the SD-JWT");
        Console.WriteLine("  2. When presenting, Alice must sign with her PRIVATE key");
        Console.WriteLine("  3. Verifier checks that signature matches the embedded key");
        Console.WriteLine("  4. Mallory cannot sign (doesn't have Alice's private key)");

        // =====================================================================
        // STEP 2: Generate holder key pair
        // =====================================================================
        ConsoleHelpers.PrintStep(2, "Generate holder key pair");

        Console.WriteLine("First, the holder (Alice) generates a key pair:");
        Console.WriteLine("  - PRIVATE key: kept secret, used for signing presentations");
        Console.WriteLine("  - PUBLIC key: shared with issuer, embedded in SD-JWT");

        using var holderEcdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var holderPrivateKey = new ECDsaSecurityKey(holderEcdsa) { KeyId = "alice-key-2024" };
        var holderPublicKey = new ECDsaSecurityKey(holderEcdsa) { KeyId = "alice-key-2024" };

        // Convert to JWK format for embedding in SD-JWT
        var holderJwk = JsonWebKeyConverter.ConvertFromSecurityKey(holderPublicKey);

        ConsoleHelpers.PrintSuccess("Generated holder key pair");
        ConsoleHelpers.PrintKeyValue("Key ID", holderPrivateKey.KeyId);
        ConsoleHelpers.PrintKeyValue("Algorithm", "ES256 (ECDSA P-256)");
        ConsoleHelpers.PrintKeyValue("JWK kty", holderJwk.Kty);
        ConsoleHelpers.PrintKeyValue("JWK crv", holderJwk.Crv);

        // =====================================================================
        // STEP 3: Issue SD-JWT with holder binding
        // =====================================================================
        ConsoleHelpers.PrintStep(3, "Issue SD-JWT with holder binding");

        Console.WriteLine("The issuer creates the SD-JWT and embeds Alice's public key:");

        using var issuerEcdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var issuerKey = new ECDsaSecurityKey(issuerEcdsa) { KeyId = "university-2024" };

        var claims = new JwtPayload
        {
            [JwtRegisteredClaimNames.Iss] = "https://university.example.edu",
            [JwtRegisteredClaimNames.Sub] = "alice_student_id",
            [JwtRegisteredClaimNames.Iat] = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            [JwtRegisteredClaimNames.Exp] = DateTimeOffset.UtcNow.AddYears(10).ToUnixTimeSeconds(),
            ["degree"] = "Bachelor of Science",
            ["major"] = "Computer Science",
            ["gpa"] = 3.85
        };

        var options = new SdIssuanceOptions
        {
            DisclosureStructure = new { gpa = true }  // GPA is selectively disclosable
        };

        var issuer = new SdIssuer(issuerKey, SecurityAlgorithms.EcdsaSha256);

        // Pass holder's JWK to embed in the SD-JWT
        var result = issuer.Issue(claims, options, holderJwk);

        ConsoleHelpers.PrintSuccess("SD-JWT issued with holder binding");
        Console.WriteLine();
        Console.WriteLine("  The SD-JWT now contains a 'cnf' (confirmation) claim:");
        Console.WriteLine("  {");
        Console.WriteLine("    \"cnf\": {");
        Console.WriteLine("      \"jwk\": {");
        Console.WriteLine($"        \"kty\": \"{holderJwk.Kty}\",");
        Console.WriteLine($"        \"crv\": \"{holderJwk.Crv}\",");
        Console.WriteLine("        \"x\": \"...\",  // Public key X coordinate");
        Console.WriteLine("        \"y\": \"...\"   // Public key Y coordinate");
        Console.WriteLine("      }");
        Console.WriteLine("    }");
        Console.WriteLine("  }");

        // =====================================================================
        // STEP 4: Create presentation with key binding proof
        // =====================================================================
        ConsoleHelpers.PrintStep(4, "Create presentation with Key Binding JWT (KB-JWT)");

        Console.WriteLine("When Alice presents her credential, she must prove possession");
        Console.WriteLine("of the private key by creating a Key Binding JWT (KB-JWT).");

        var holder = new SdJwtHolder(result.Issuance);

        // Create presentation with key binding
        var presentation = holder.CreatePresentation(
            disclosure => disclosure.ClaimName == "gpa",  // Reveal GPA
            kbJwtPayload: new JwtPayload
            {
                [JwtRegisteredClaimNames.Aud] = "https://employer.example.com",
                [JwtRegisteredClaimNames.Iat] = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                ["nonce"] = "job_application_xyz"
            },
            kbJwtSigningKey: holderPrivateKey,
            kbJwtSigningAlgorithm: SecurityAlgorithms.EcdsaSha256
        );

        ConsoleHelpers.PrintSuccess("Presentation created with KB-JWT");
        Console.WriteLine();
        Console.WriteLine("  Presentation format:");
        Console.WriteLine("  <SD-JWT>~<disclosure1>~...~<KB-JWT>");
        Console.WriteLine();
        Console.WriteLine("  The KB-JWT contains:");
        Console.WriteLine("  {");
        Console.WriteLine("    \"aud\": \"https://employer.example.com\",");
        Console.WriteLine("    \"iat\": <current_time>,");
        Console.WriteLine("    \"nonce\": \"job_application_xyz\",");
        Console.WriteLine("    \"sd_hash\": \"<hash_of_sd_jwt_and_disclosures>\"");
        Console.WriteLine("  }");
        Console.WriteLine();
        Console.WriteLine("  The KB-JWT is SIGNED with Alice's PRIVATE key.");

        // =====================================================================
        // STEP 5: Verification process
        // =====================================================================
        ConsoleHelpers.PrintStep(5, "How the verifier validates key binding");

        Console.WriteLine("The verifier performs these checks:");
        Console.WriteLine();
        Console.WriteLine("  1. Verify SD-JWT signature (issuer's key)");
        Console.WriteLine("     --> Ensures credential is authentic from university");
        Console.WriteLine();
        Console.WriteLine("  2. Extract 'cnf.jwk' from SD-JWT payload");
        Console.WriteLine("     --> Gets Alice's public key embedded by issuer");
        Console.WriteLine();
        Console.WriteLine("  3. Verify KB-JWT signature with extracted public key");
        Console.WriteLine("     --> Proves presenter controls the bound private key");
        Console.WriteLine();
        Console.WriteLine("  4. Validate KB-JWT claims:");
        Console.WriteLine("     - 'aud' matches verifier's identifier");
        Console.WriteLine("     - 'iat' is recent (prevents replay)");
        Console.WriteLine("     - 'nonce' matches what verifier sent (prevents replay)");
        Console.WriteLine("     - 'sd_hash' matches actual SD-JWT content");
        Console.WriteLine();
        Console.WriteLine("  If ANY check fails, the presentation is REJECTED.");

        // =====================================================================
        // STEP 6: Why Mallory cannot use stolen SD-JWT
        // =====================================================================
        ConsoleHelpers.PrintStep(6, "Why stolen credentials are useless");

        Console.WriteLine("If Mallory steals Alice's SD-JWT:");
        Console.WriteLine();
        Console.WriteLine("  Mallory HAS:");
        Console.WriteLine("    [X] The SD-JWT (signed by university)");
        Console.WriteLine("    [X] The disclosures (can see hidden claims)");
        Console.WriteLine();
        Console.WriteLine("  Mallory DOES NOT HAVE:");
        Console.WriteLine("    [ ] Alice's private key");
        Console.WriteLine();
        Console.WriteLine("  When Mallory tries to present:");
        Console.WriteLine("    1. Mallory creates KB-JWT signed with her own key");
        Console.WriteLine("    2. Verifier extracts Alice's public key from cnf.jwk");
        Console.WriteLine("    3. Verifier tries to verify KB-JWT with Alice's key");
        Console.WriteLine("    4. SIGNATURE VERIFICATION FAILS!");
        Console.WriteLine("    5. Presentation REJECTED");
        Console.WriteLine();
        Console.WriteLine("  The credential is BOUND to Alice's key - theft is useless!");

        // =====================================================================
        // COMPLETION
        // =====================================================================
        ConsoleHelpers.PrintCompletion("Tutorial 03: Holder Binding", new[]
        {
            "Understood the credential theft problem",
            "Generated holder key pair",
            "Issued SD-JWT with embedded public key (cnf)",
            "Created presentation with Key Binding JWT",
            "Learned verification process for key binding"
        });

        Console.WriteLine();
        Console.WriteLine("NEXT: Tutorial 04 - Complete verification flow");

        return Task.CompletedTask;
    }
}
