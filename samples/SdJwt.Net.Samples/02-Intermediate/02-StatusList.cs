using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.Issuer;
using SdJwt.Net.Models;
using SdJwt.Net.Samples.Shared;
using SdJwt.Net.StatusList.Issuer;
using SdJwt.Net.StatusList.Models;
using SdJwt.Net.Vc.Issuer;
using SdJwt.Net.Vc.Models;
using System.Security.Cryptography;

namespace SdJwt.Net.Samples.Intermediate;

/// <summary>
/// Tutorial 02: Status Lists (Revocation)
///
/// LEARNING OBJECTIVES:
/// - Understand why credentials need revocation
/// - Create and manage status lists
/// - Embed status references in credentials
/// - Check credential status during verification
///
/// TIME: ~15 minutes
/// </summary>
public static class StatusListTutorial
{
    public static async Task Run()
    {
        ConsoleHelpers.PrintHeader("Tutorial 02: Status Lists (Credential Revocation)");

        Console.WriteLine("Credentials may need to be invalidated before expiration.");
        Console.WriteLine("Status Lists provide efficient, privacy-preserving revocation.");
        Console.WriteLine();

        // Setup
        using var issuerEcdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        using var holderEcdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);

        var issuerKey = new ECDsaSecurityKey(issuerEcdsa) { KeyId = "employer-hr-2024" };
        var holderJwk = JsonWebKeyConverter.ConvertFromSecurityKey(
            new ECDsaSecurityKey(holderEcdsa) { KeyId = "alice-key" });

        // =====================================================================
        // STEP 1: Why revocation matters
        // =====================================================================
        ConsoleHelpers.PrintStep(1, "Why credentials need revocation");

        Console.WriteLine("Scenarios requiring revocation:");
        Console.WriteLine();
        Console.WriteLine("  EMPLOYMENT CREDENTIAL:");
        Console.WriteLine("    - Employee terminated");
        Console.WriteLine("    - Role changed");
        Console.WriteLine("    - Company acquired");
        Console.WriteLine();
        Console.WriteLine("  PROFESSIONAL LICENSE:");
        Console.WriteLine("    - License suspended");
        Console.WriteLine("    - Disciplinary action");
        Console.WriteLine("    - Failed continuing education");
        Console.WriteLine();
        Console.WriteLine("  ACCESS CREDENTIAL:");
        Console.WriteLine("    - Key compromised");
        Console.WriteLine("    - Policy violation");
        Console.WriteLine("    - Temporary ban");
        Console.WriteLine();
        Console.WriteLine("Without revocation, credentials remain valid until exp!");

        // =====================================================================
        // STEP 2: Status List concept
        // =====================================================================
        ConsoleHelpers.PrintStep(2, "How Status Lists work");

        Console.WriteLine("A Status List is a compressed bit array:");
        Console.WriteLine();
        Console.WriteLine("  Index:  0  1  2  3  4  5  6  7  8  9  ...");
        Console.WriteLine("  Bits:   0  0  1  0  0  0  1  0  0  0  ...");
        Console.WriteLine("          |     |           |");
        Console.WriteLine("          |     |           +-- Credential 6: REVOKED");
        Console.WriteLine("          |     +-------------- Credential 2: REVOKED");
        Console.WriteLine("          +-------------------- Credential 0: Valid");
        Console.WriteLine();
        Console.WriteLine("Benefits:");
        Console.WriteLine("  - Compact: millions of statuses in kilobytes");
        Console.WriteLine("  - Privacy: verifier learns nothing about other credentials");
        Console.WriteLine("  - Efficient: single fetch checks many credentials");
        Console.WriteLine("  - Cacheable: status list can be cached");

        // =====================================================================
        // STEP 3: Create a status list
        // =====================================================================
        ConsoleHelpers.PrintStep(3, "Create a status list");

        // Create status list with 1000 entries (each entry is 1 bit)
        // Status values: 0 = Valid, 1 = Invalid/Revoked
        var statusValues = new byte[125]; // 1000 bits = 125 bytes
        Array.Fill(statusValues, (byte)0); // All valid initially

        ConsoleHelpers.PrintSuccess("Status list created");
        ConsoleHelpers.PrintKeyValue("Size", "1000 entries (125 bytes)");
        ConsoleHelpers.PrintKeyValue("Initial state", "All valid (0)");

        // =====================================================================
        // STEP 4: Issue credential with status reference
        // =====================================================================
        ConsoleHelpers.PrintStep(4, "Issue credential with status reference");

        Console.WriteLine("Each credential gets a unique index in the status list.");

        var vcIssuer = new SdJwtVcIssuer(issuerKey, SecurityAlgorithms.EcdsaSha256);

        // Assign index 42 to Alice's credential
        var aliceIndex = 42;

        var vcPayload = new SdJwtVcPayload
        {
            Issuer = "https://hr.techcorp.example.com",
            Subject = "emp_alice_12345",
            IssuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            ExpiresAt = DateTimeOffset.UtcNow.AddYears(1).ToUnixTimeSeconds(),
            AdditionalData = new Dictionary<string, object>
            {
                ["employee_name"] = "Alice Johnson",
                ["job_title"] = "Senior Engineer",
                ["department"] = "Engineering",

                // Status reference embedded in credential
                ["status"] = new Dictionary<string, object>
                {
                    ["status_list"] = new Dictionary<string, object>
                    {
                        ["uri"] = "https://hr.techcorp.example.com/status/employees",
                        ["idx"] = aliceIndex
                    }
                }
            }
        };

        var sdOptions = new SdIssuanceOptions
        {
            DisclosureStructure = new { employee_name = true }
        };

        var vcResult = vcIssuer.Issue(
            "https://credentials.techcorp.example.com/Employment",
            vcPayload,
            sdOptions,
            holderJwk);

        ConsoleHelpers.PrintSuccess("Credential issued with status reference");
        ConsoleHelpers.PrintKeyValue("Status list URI", "https://hr.techcorp.example.com/status/employees");
        ConsoleHelpers.PrintKeyValue("Credential index", aliceIndex);

        // Issue another credential for Bob at index 43
        var bobIndex = 43;
        Console.WriteLine();
        Console.WriteLine($"  Also issued: Bob's credential at index {bobIndex}");

        // =====================================================================
        // STEP 5: Revoke a credential
        // =====================================================================
        ConsoleHelpers.PrintStep(5, "Revoke a credential");

        Console.WriteLine("Scenario: Bob has been terminated, revoke his credential.");

        // Revoke Bob's credential (index 43) by setting bit to 1
        // Each byte holds 8 status bits: byteIndex = idx / 8, bitIndex = idx % 8
        var byteIndex = bobIndex / 8;
        var bitIndex = bobIndex % 8;
        statusValues[byteIndex] |= (byte)(1 << bitIndex);

        ConsoleHelpers.PrintSuccess($"Credential at index {bobIndex} REVOKED");
        Console.WriteLine();
        Console.WriteLine("Status list now:");
        Console.WriteLine($"  Index {aliceIndex} (Alice): Valid");
        Console.WriteLine($"  Index {bobIndex} (Bob):   REVOKED");

        // =====================================================================
        // STEP 6: Check credential status
        // =====================================================================
        ConsoleHelpers.PrintStep(6, "Verify credential status");

        Console.WriteLine("Verifier checks status during credential verification:");
        Console.WriteLine();

        // Helper function to check status at an index
        bool GetStatusBit(byte[] values, int idx)
        {
            var bIdx = idx / 8;
            var bitIdx = idx % 8;
            return (values[bIdx] & (1 << bitIdx)) != 0;
        }

        // Check Alice's status
        var aliceRevoked = GetStatusBit(statusValues, aliceIndex);
        Console.WriteLine($"  Checking Alice (index {aliceIndex})...");
        Console.WriteLine($"    Status: {(aliceRevoked ? "REVOKED" : "VALID")}");

        // Check Bob's status
        var bobRevoked = GetStatusBit(statusValues, bobIndex);
        Console.WriteLine();
        Console.WriteLine($"  Checking Bob (index {bobIndex})...");
        Console.WriteLine($"    Status: {(bobRevoked ? "REVOKED" : "VALID")}");

        Console.WriteLine();
        Console.WriteLine("Verification decision:");
        Console.WriteLine($"  Alice: {(aliceRevoked ? "REJECT" : "ACCEPT")} credential");
        Console.WriteLine($"  Bob:   {(bobRevoked ? "REJECT" : "ACCEPT")} credential");

        // =====================================================================
        // STEP 7: Status types
        // =====================================================================
        ConsoleHelpers.PrintStep(7, "Status types: Revocation vs Suspension");

        Console.WriteLine("Two primary status types:");
        Console.WriteLine();
        Console.WriteLine("  REVOCATION (permanent):");
        Console.WriteLine("    - Credential permanently invalid");
        Console.WriteLine("    - Cannot be undone");
        Console.WriteLine("    - Use for: termination, fraud, key compromise");
        Console.WriteLine();
        Console.WriteLine("  SUSPENSION (temporary):");
        Console.WriteLine("    - Credential temporarily invalid");
        Console.WriteLine("    - Can be lifted later");
        Console.WriteLine("    - Use for: investigation, payment lapse, temp leave");
        Console.WriteLine();

        // Demonstrate suspension (using separate status list for suspension)
        var charlieIndex = 44;
        var suspensionValues = new byte[125]; // Separate list for suspension status
        var charlieByteIdx = charlieIndex / 8;
        var charlieBitIdx = charlieIndex % 8;
        suspensionValues[charlieByteIdx] |= (byte)(1 << charlieBitIdx);
        Console.WriteLine($"Example: Charlie (index {charlieIndex}) SUSPENDED");
        Console.WriteLine("  Charlie's credential is temporarily invalid.");
        Console.WriteLine("  Can be restored when investigation completes.");

        // =====================================================================
        // STEP 8: Publishing status lists
        // =====================================================================
        ConsoleHelpers.PrintStep(8, "Publishing status lists");

        Console.WriteLine("The issuer publishes the status list as a signed JWT:");
        Console.WriteLine();
        Console.WriteLine("  1. Compress the bit array (DEFLATE)");
        Console.WriteLine("  2. Base64URL encode");
        Console.WriteLine("  3. Wrap in JWT with metadata");
        Console.WriteLine("  4. Sign with issuer key");
        Console.WriteLine("  5. Host at the URI referenced in credentials");
        Console.WriteLine();

        // Create the status list token using StatusListManager
        var statusListManager = new StatusListManager(issuerKey, SecurityAlgorithms.EcdsaSha256);
        var statusListToken = await statusListManager.CreateStatusListTokenAsync(
            subject: "https://hr.techcorp.example.com/status/employees",
            statusValues: statusValues,
            bits: 1,
            validUntil: DateTime.UtcNow.AddDays(7),
            timeToLive: 3600);

        ConsoleHelpers.PrintSuccess("Status list token created");
        ConsoleHelpers.PrintPreview("Token", statusListToken, 60);

        Console.WriteLine();
        Console.WriteLine("Verifiers fetch this token to check credential status.");
        Console.WriteLine("Caching headers control how long status can be cached.");

        // =====================================================================
        // STEP 9: Verification workflow
        // =====================================================================
        ConsoleHelpers.PrintStep(9, "Complete verification workflow");

        Console.WriteLine("When verifying a credential with status:");
        Console.WriteLine();
        Console.WriteLine("  1. Verify SD-JWT signature");
        Console.WriteLine("  2. Extract status reference from credential");
        Console.WriteLine("     {");
        Console.WriteLine("       \"status\": {");
        Console.WriteLine("         \"status_list\": {");
        Console.WriteLine("           \"uri\": \"https://hr.techcorp.example.com/status/employees\",");
        Console.WriteLine($"           \"idx\": {aliceIndex}");
        Console.WriteLine("         }");
        Console.WriteLine("       }");
        Console.WriteLine("     }");
        Console.WriteLine("  3. Fetch status list from URI (with caching)");
        Console.WriteLine("  4. Verify status list JWT signature");
        Console.WriteLine("  5. Decompress and check bit at index");
        Console.WriteLine("  6. Accept or reject based on status");

        // =====================================================================
        // COMPLETION
        // =====================================================================
        ConsoleHelpers.PrintCompletion("Tutorial 02: Status Lists", new[]
        {
            "Understood why revocation matters",
            "Created a status list",
            "Issued credential with status reference",
            "Revoked and suspended credentials",
            "Learned verification workflow"
        });

        Console.WriteLine();
        Console.WriteLine("NEXT: Tutorial 03 - OpenID4VCI credential issuance");
    }
}
