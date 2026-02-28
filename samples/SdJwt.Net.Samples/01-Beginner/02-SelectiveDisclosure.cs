using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.Issuer;
using SdJwt.Net.Models;
using SdJwt.Net.Utils;
using SdJwt.Net.Samples.Shared;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace SdJwt.Net.Samples.Beginner;

/// <summary>
/// Tutorial 02: Selective Disclosure Deep Dive
///
/// LEARNING OBJECTIVES:
/// - Understand how _sd digests work
/// - See the disclosure structure [salt, name, value]
/// - Learn how claims are hidden and revealed
/// - Explore nested selective disclosure
///
/// TIME: ~10 minutes
/// </summary>
public static class SelectiveDisclosure
{
    public static Task Run()
    {
        ConsoleHelpers.PrintHeader("Tutorial 02: Selective Disclosure Deep Dive");

        Console.WriteLine("In this tutorial, you'll learn exactly how SD-JWT hides and");
        Console.WriteLine("reveals claims using cryptographic digests.");
        Console.WriteLine();

        // Setup
        using var ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var signingKey = new ECDsaSecurityKey(ecdsa) { KeyId = "tutorial-key" };

        // =====================================================================
        // STEP 1: Create SD-JWT with multiple disclosure levels
        // =====================================================================
        ConsoleHelpers.PrintStep(1, "Create SD-JWT with various claim types");

        var claims = new JwtPayload
        {
            [JwtRegisteredClaimNames.Iss] = "https://university.example.edu",
            [JwtRegisteredClaimNames.Sub] = "student_12345",
            [JwtRegisteredClaimNames.Iat] = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            ["student_name"] = "Alice Johnson",
            ["student_id"] = "STU-2024-001",
            ["degree"] = "Computer Science",
            ["gpa"] = 3.85,
            ["graduation_year"] = 2024,
            ["address"] = new Dictionary<string, object>
            {
                ["city"] = "San Francisco",
                ["state"] = "CA",
                ["country"] = "US"
            }
        };

        var issuer = new SdIssuer(signingKey, SecurityAlgorithms.EcdsaSha256);

        var options = new SdIssuanceOptions
        {
            DisclosureStructure = new
            {
                student_name = true,
                student_id = true,
                gpa = true,              // Sensitive - often hidden
                graduation_year = true,
                address = new            // Nested disclosure
                {
                    city = true,
                    state = true
                    // country is NOT selectively disclosable - always visible in address
                }
            }
        };

        var result = issuer.Issue(claims, options);

        ConsoleHelpers.PrintSuccess($"Created SD-JWT with {result.Disclosures.Count} disclosures");

        // =====================================================================
        // STEP 2: Examine the JWT payload
        // =====================================================================
        ConsoleHelpers.PrintStep(2, "Examine the JWT payload with _sd digests");

        var parsed = SdJwtParser.ParseIssuance(result.Issuance);
        var payload = parsed.UnverifiedSdJwt.Payload;

        Console.WriteLine("The JWT payload contains:");
        Console.WriteLine();
        Console.WriteLine("  ALWAYS VISIBLE claims (not in _sd):");
        Console.WriteLine($"    iss: {payload[JwtRegisteredClaimNames.Iss]}");
        Console.WriteLine($"    sub: {payload[JwtRegisteredClaimNames.Sub]}");
        Console.WriteLine($"    iat: {payload[JwtRegisteredClaimNames.Iat]}");
        Console.WriteLine($"    degree: {payload["degree"]}");

        Console.WriteLine();
        Console.WriteLine("  HIDDEN claims (replaced with digests in _sd array):");

        // Get the _sd array from payload
        if (payload.TryGetValue("_sd", out var sdArray) && sdArray is List<object> sdList)
        {
            Console.WriteLine($"    _sd array has {sdList.Count} digests:");
            var i = 1;
            foreach (var digest in sdList.Take(3))
            {
                Console.WriteLine($"      [{i++}] {digest?.ToString()?[..40]}...");
            }
            if (sdList.Count > 3)
            {
                Console.WriteLine($"      ... and {sdList.Count - 3} more");
            }
        }

        // =====================================================================
        // STEP 3: Understand disclosure structure
        // =====================================================================
        ConsoleHelpers.PrintStep(3, "Understand disclosure structure");

        Console.WriteLine("Each disclosure is a Base64URL-encoded JSON array:");
        Console.WriteLine("  [salt, claim_name, claim_value]");
        Console.WriteLine();

        Console.WriteLine("Let's decode one:");
        var firstDisclosure = result.Disclosures.First();

        Console.WriteLine($"  Claim: {firstDisclosure.ClaimName}");
        Console.WriteLine($"  Value: {JsonSerializer.Serialize(firstDisclosure.ClaimValue)}");
        Console.WriteLine($"  Salt:  {firstDisclosure.Salt}  (random, prevents guessing)");
        Console.WriteLine();

        Console.WriteLine("The DIGEST is computed as:");
        Console.WriteLine("  digest = BASE64URL(SHA256([salt, name, value]))");
        Console.WriteLine();
        Console.WriteLine("This digest is stored in _sd. The verifier can:");
        Console.WriteLine("  1. Receive the disclosure");
        Console.WriteLine("  2. Compute the same digest");
        Console.WriteLine("  3. Find it in _sd array");
        Console.WriteLine("  4. Know the claim is authentic (from the signed JWT)");

        // =====================================================================
        // STEP 4: Demonstrate how hiding works
        // =====================================================================
        ConsoleHelpers.PrintStep(4, "How hiding works - the verifier's view");

        Console.WriteLine("WITHOUT disclosure, verifier sees:");
        Console.WriteLine("  {");
        Console.WriteLine("    \"iss\": \"https://university.example.edu\",");
        Console.WriteLine("    \"sub\": \"student_12345\",");
        Console.WriteLine("    \"degree\": \"Computer Science\",");
        Console.WriteLine("    \"_sd\": [\"Abc...\", \"Def...\", ...]  // Opaque digests!");
        Console.WriteLine("  }");
        Console.WriteLine();
        Console.WriteLine("The verifier CANNOT determine:");
        Console.WriteLine("  - What claims are hidden (names are hashed too)");
        Console.WriteLine("  - What values those claims have");
        Console.WriteLine("  - How many hidden claims exist per digest");

        Console.WriteLine();
        Console.WriteLine("WITH disclosure of 'student_name', verifier sees:");
        Console.WriteLine("  {");
        Console.WriteLine("    \"iss\": \"https://university.example.edu\",");
        Console.WriteLine("    \"sub\": \"student_12345\",");
        Console.WriteLine("    \"degree\": \"Computer Science\",");
        Console.WriteLine("    \"student_name\": \"Alice Johnson\",  // REVEALED!");
        Console.WriteLine("    \"_sd\": [...]  // Other claims still hidden");
        Console.WriteLine("  }");

        // =====================================================================
        // STEP 5: Nested selective disclosure
        // =====================================================================
        ConsoleHelpers.PrintStep(5, "Nested selective disclosure (address example)");

        Console.WriteLine("The 'address' claim demonstrates nested disclosure:");
        Console.WriteLine();
        Console.WriteLine("  address: {");
        Console.WriteLine("    city: [SELECTIVELY DISCLOSABLE]");
        Console.WriteLine("    state: [SELECTIVELY DISCLOSABLE]");
        Console.WriteLine("    country: \"US\"  // Always visible within address");
        Console.WriteLine("    _sd: [digest_city, digest_state]");
        Console.WriteLine("  }");
        Console.WriteLine();
        Console.WriteLine("Holder can reveal:");
        Console.WriteLine("  - Just city (recruiter checking location)");
        Console.WriteLine("  - Just state (age verification)");
        Console.WriteLine("  - Both city and state (full address needed)");
        Console.WriteLine("  - Neither (address not relevant to request)");

        // =====================================================================
        // STEP 6: List all disclosures
        // =====================================================================
        ConsoleHelpers.PrintStep(6, "All available disclosures");

        Console.WriteLine("The holder received these disclosures from the issuer:");
        Console.WriteLine();

        foreach (var disclosure in result.Disclosures)
        {
            var valueStr = disclosure.ClaimValue switch
            {
                string s => $"\"{s}\"",
                int i => i.ToString(),
                double d => d.ToString("F2"),
                _ => JsonSerializer.Serialize(disclosure.ClaimValue)
            };
            Console.WriteLine($"  {disclosure.ClaimName,-20} = {valueStr}");
        }

        Console.WriteLine();
        Console.WriteLine("The holder keeps these disclosures secret and reveals");
        Console.WriteLine("only the ones needed for each presentation.");

        // =====================================================================
        // COMPLETION
        // =====================================================================
        ConsoleHelpers.PrintCompletion("Tutorial 02: Selective Disclosure", new[]
        {
            "Created SD-JWT with multiple claim types",
            "Examined _sd digest array in payload",
            "Understood disclosure structure [salt, name, value]",
            "Learned how hiding prevents inference",
            "Explored nested selective disclosure"
        });

        Console.WriteLine();
        Console.WriteLine("NEXT: Tutorial 03 - Holder key binding for proof of possession");

        return Task.CompletedTask;
    }
}
