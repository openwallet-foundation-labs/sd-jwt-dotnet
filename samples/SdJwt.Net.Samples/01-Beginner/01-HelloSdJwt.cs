using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.Issuer;
using SdJwt.Net.Models;
using SdJwt.Net.Samples.Shared;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text.Json;

namespace SdJwt.Net.Samples.Beginner;

/// <summary>
/// Tutorial 01: Hello SD-JWT
///
/// LEARNING OBJECTIVES:
/// - Create your first SD-JWT
/// - Understand the basic structure
/// - See the difference between regular JWT and SD-JWT
///
/// TIME: ~5 minutes
/// </summary>
public static class HelloSdJwt
{
    public static Task Run()
    {
        ConsoleHelpers.PrintHeader("Tutorial 01: Hello SD-JWT");

        Console.WriteLine("In this tutorial, you'll create your first SD-JWT and understand");
        Console.WriteLine("how it differs from a regular JWT.");
        Console.WriteLine();

        // =====================================================================
        // STEP 1: Create a signing key
        // =====================================================================
        ConsoleHelpers.PrintStep(1, "Create a signing key");

        Console.WriteLine("Every SD-JWT needs a cryptographic key for signing.");
        Console.WriteLine("We use ECDSA P-256 (ES256) - the most common algorithm.");

        using var ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var signingKey = new ECDsaSecurityKey(ecdsa) { KeyId = "my-first-key" };

        ConsoleHelpers.PrintSuccess("Created ECDSA P-256 signing key");
        ConsoleHelpers.PrintKeyValue("Key ID", signingKey.KeyId);
        ConsoleHelpers.PrintKeyValue("Algorithm", "ES256");

        // =====================================================================
        // STEP 2: Define claims to include
        // =====================================================================
        ConsoleHelpers.PrintStep(2, "Define claims to include");

        Console.WriteLine("Claims are the data inside the JWT.");
        Console.WriteLine("Let's create a simple credential with name and email.");

        var claims = new JwtPayload
        {
            [JwtRegisteredClaimNames.Iss] = "https://example.com",
            [JwtRegisteredClaimNames.Sub] = "user123",
            [JwtRegisteredClaimNames.Iat] = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            ["given_name"] = "Alice",
            ["family_name"] = "Johnson",
            ["email"] = "alice@example.com"
        };

        ConsoleHelpers.PrintSuccess("Defined claims");
        Console.WriteLine("  Claims:");
        Console.WriteLine($"    - iss: {claims[JwtRegisteredClaimNames.Iss]}");
        Console.WriteLine($"    - sub: {claims[JwtRegisteredClaimNames.Sub]}");
        Console.WriteLine($"    - given_name: {claims["given_name"]}");
        Console.WriteLine($"    - family_name: {claims["family_name"]}");
        Console.WriteLine($"    - email: {claims["email"]}");

        // =====================================================================
        // STEP 3: Create the SD-JWT
        // =====================================================================
        ConsoleHelpers.PrintStep(3, "Create the SD-JWT with selective disclosure");

        Console.WriteLine("Now we specify which claims should be selectively disclosable.");
        Console.WriteLine("The issuer, subject, and issued-at are always visible.");
        Console.WriteLine("But given_name, family_name, and email can be hidden by the holder.");

        var issuer = new SdIssuer(signingKey, SecurityAlgorithms.EcdsaSha256);

        // Mark personal claims as selectively disclosable
        var options = new SdIssuanceOptions
        {
            DisclosureStructure = new
            {
                given_name = true,   // Can be selectively disclosed
                family_name = true,  // Can be selectively disclosed
                email = true         // Can be selectively disclosed
            }
        };

        var result = issuer.Issue(claims, options);

        ConsoleHelpers.PrintSuccess("SD-JWT created!");
        ConsoleHelpers.PrintKeyValue("Total length", $"{result.Issuance.Length} characters");
        ConsoleHelpers.PrintKeyValue("Number of disclosures", result.Disclosures.Count);

        // =====================================================================
        // STEP 4: Examine the structure
        // =====================================================================
        ConsoleHelpers.PrintStep(4, "Examine the SD-JWT structure");

        Console.WriteLine("An SD-JWT has this format:");
        Console.WriteLine("  <JWT>~<disclosure1>~<disclosure2>~...");
        Console.WriteLine();

        var parts = result.Issuance.Split('~');
        Console.WriteLine($"  JWT part: {parts[0][..60]}...");
        Console.WriteLine($"  Disclosures: {parts.Length - 1} items");

        Console.WriteLine();
        Console.WriteLine("Each disclosure is Base64-encoded [salt, name, value]:");
        foreach (var disclosure in result.Disclosures)
        {
            Console.WriteLine($"    {disclosure.ClaimName} = {JsonSerializer.Serialize(disclosure.ClaimValue)}");
        }

        // =====================================================================
        // STEP 5: Compare with regular JWT
        // =====================================================================
        ConsoleHelpers.PrintStep(5, "Compare: Regular JWT vs SD-JWT");

        Console.WriteLine("REGULAR JWT:");
        Console.WriteLine("  - All claims visible to anyone with the token");
        Console.WriteLine("  - No way to hide sensitive data");
        Console.WriteLine();
        Console.WriteLine("SD-JWT:");
        Console.WriteLine("  - Issuer decides WHICH claims CAN be hidden");
        Console.WriteLine("  - Holder decides WHICH claims to REVEAL");
        Console.WriteLine("  - Verifier sees ONLY what holder chooses to show");
        Console.WriteLine();
        Console.WriteLine("This is SELECTIVE DISCLOSURE - the core concept!");

        // =====================================================================
        // COMPLETION
        // =====================================================================
        ConsoleHelpers.PrintCompletion("Tutorial 01: Hello SD-JWT", new[]
        {
            "Created ECDSA signing key",
            "Defined JWT claims",
            "Created SD-JWT with selective disclosure",
            "Understood SD-JWT structure",
            "Learned difference from regular JWT"
        });

        Console.WriteLine();
        Console.WriteLine("NEXT: Tutorial 02 - Deep dive into selective disclosure mechanics");

        return Task.CompletedTask;
    }
}
