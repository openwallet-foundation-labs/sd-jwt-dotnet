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

namespace SdJwt.Net.Samples.Advanced;

/// <summary>
/// Tutorial 04: Key Rotation and Lifecycle Management
///
/// LEARNING OBJECTIVES:
/// - Plan and execute key rotation
/// - Handle credentials during rotation period
/// - Implement graceful key deprecation
/// - Manage key discovery and publication
///
/// TIME: ~15 minutes
/// </summary>
public static class KeyRotation
{
    public static async Task Run()
    {
        ConsoleHelpers.PrintHeader("Tutorial 04: Key Rotation and Lifecycle Management");

        Console.WriteLine("Cryptographic keys have lifecycles. This tutorial covers");
        Console.WriteLine("rotating issuer keys while maintaining credential validity");
        Console.WriteLine("and verifier compatibility.");
        Console.WriteLine();

        // =====================================================================
        // STEP 1: Why rotate keys?
        // =====================================================================
        ConsoleHelpers.PrintStep(1, "Why rotate keys?");

        Console.WriteLine("Key rotation is essential for:");
        Console.WriteLine();
        Console.WriteLine("  1. SECURITY - Limit exposure if key is compromised");
        Console.WriteLine("     Regular rotation bounds the window of vulnerability");
        Console.WriteLine();
        Console.WriteLine("  2. COMPLIANCE - Many standards mandate rotation schedules");
        Console.WriteLine("     PCI-DSS: Annual, NIST: 1-3 years depending on level");
        Console.WriteLine();
        Console.WriteLine("  3. ALGORITHM MIGRATION - Move to stronger algorithms");
        Console.WriteLine("     Example: P-256 to P-384 for higher assurance");
        Console.WriteLine();
        Console.WriteLine("  4. CRYPTOGRAPHIC HYGIENE - Best practice");
        Console.WriteLine("     Limits signing volume with any single key");

        // =====================================================================
        // STEP 2: Key lifecycle phases
        // =====================================================================
        ConsoleHelpers.PrintStep(2, "Key lifecycle phases");

        Console.WriteLine("A well-managed key goes through these phases:");
        Console.WriteLine();
        Console.WriteLine("  [GENERATION]");
        Console.WriteLine("      |");
        Console.WriteLine("      | Key created but not yet in use");
        Console.WriteLine("      v");
        Console.WriteLine("  [ACTIVATION]  <-- Start signing credentials");
        Console.WriteLine("      |");
        Console.WriteLine("      | Primary key for new credentials");
        Console.WriteLine("      v");
        Console.WriteLine("  [ROTATION]    <-- New key takes over");
        Console.WriteLine("      |");
        Console.WriteLine("      | Old key stops signing, still validates");
        Console.WriteLine("      v");
        Console.WriteLine("  [DEPRECATION] <-- Old credentials expire");
        Console.WriteLine("      |");
        Console.WriteLine("      | Stop accepting old key");
        Console.WriteLine("      v");
        Console.WriteLine("  [DESTRUCTION] <-- Secure key deletion");

        // =====================================================================
        // STEP 3: Setup - Two generations of keys
        // =====================================================================
        ConsoleHelpers.PrintStep(3, "Setup: Two key generations");

        // Old key (still in use, about to rotate)
        using var oldEcdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var oldKey = new ECDsaSecurityKey(oldEcdsa) { KeyId = "issuer-key-2023" };

        // New key (taking over)
        using var newEcdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var newKey = new ECDsaSecurityKey(newEcdsa) { KeyId = "issuer-key-2024" };

        using var holderEcdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var holderPrivateKey = new ECDsaSecurityKey(holderEcdsa) { KeyId = "holder" };
        var holderJwk = JsonWebKeyConverter.ConvertFromSecurityKey(holderPrivateKey);

        Console.WriteLine("Key inventory:");
        Console.WriteLine($"  OLD: {oldKey.KeyId} (P-256, ES256)");
        Console.WriteLine("       Status: Active, scheduled for rotation");
        Console.WriteLine();
        Console.WriteLine($"  NEW: {newKey.KeyId} (P-256, ES256)");
        Console.WriteLine("       Status: Generated, ready for activation");

        // =====================================================================
        // STEP 4: Issue credential with old key
        // =====================================================================
        ConsoleHelpers.PrintStep(4, "Credential issued with old key");

        var oldIssuer = new SdJwtVcIssuer(oldKey, SecurityAlgorithms.EcdsaSha256);
        var oldCredentialPayload = new SdJwtVcPayload
        {
            Issuer = "https://credentials.example.com",
            Subject = "did:example:alice",
            IssuedAt = DateTimeOffset.UtcNow.AddMonths(-6).ToUnixTimeSeconds(),
            ExpiresAt = DateTimeOffset.UtcNow.AddYears(4).ToUnixTimeSeconds(),
            AdditionalData = new Dictionary<string, object>
            {
                ["name"] = "Alice Johnson",
                ["credential_type"] = "EmployeeID",
                ["employee_id"] = "EMP-12345"
            }
        };
        var oldCredential = oldIssuer.Issue(
            "https://credentials.example.com/EmployeeID",
            oldCredentialPayload,
            new SdIssuanceOptions { DisclosureStructure = new { employee_id = true } },
            holderJwk);

        Console.WriteLine("Existing credential (issued 6 months ago):");
        Console.WriteLine($"  Signed with: {oldKey.KeyId}");
        Console.WriteLine($"  Expires: {DateTimeOffset.FromUnixTimeSeconds((long)oldCredentialPayload.ExpiresAt!):yyyy-MM-dd}");
        ConsoleHelpers.PrintPreview("  SD-JWT", oldCredential.Issuance, 50);

        // =====================================================================
        // STEP 5: Publish both keys during rotation
        // =====================================================================
        ConsoleHelpers.PrintStep(5, "Publish both keys during rotation window");

        Console.WriteLine("JWKS endpoint must include BOTH keys during rotation:");
        Console.WriteLine();
        Console.WriteLine("  GET https://credentials.example.com/.well-known/jwks.json");
        Console.WriteLine();
        Console.WriteLine("  {");
        Console.WriteLine("    \"keys\": [");
        Console.WriteLine("      {");
        Console.WriteLine($"        \"kid\": \"{oldKey.KeyId}\",");
        Console.WriteLine("        \"kty\": \"EC\",");
        Console.WriteLine("        \"crv\": \"P-256\",");
        Console.WriteLine("        \"use\": \"sig\"   // Still valid for verification");
        Console.WriteLine("      },");
        Console.WriteLine("      {");
        Console.WriteLine($"        \"kid\": \"{newKey.KeyId}\",");
        Console.WriteLine("        \"kty\": \"EC\",");
        Console.WriteLine("        \"crv\": \"P-256\",");
        Console.WriteLine("        \"use\": \"sig\"   // Primary for new credentials");
        Console.WriteLine("      }");
        Console.WriteLine("    ]");
        Console.WriteLine("  }");
        Console.WriteLine();
        Console.WriteLine("Key: Verifiers use 'kid' from JWT header to select key.");

        // =====================================================================
        // STEP 6: Issue new credential with new key
        // =====================================================================
        ConsoleHelpers.PrintStep(6, "Issue credential with new key");

        var newIssuer = new SdJwtVcIssuer(newKey, SecurityAlgorithms.EcdsaSha256);
        var newCredentialPayload = new SdJwtVcPayload
        {
            Issuer = "https://credentials.example.com",
            Subject = "did:example:bob",
            IssuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            ExpiresAt = DateTimeOffset.UtcNow.AddYears(5).ToUnixTimeSeconds(),
            AdditionalData = new Dictionary<string, object>
            {
                ["name"] = "Bob Smith",
                ["credential_type"] = "EmployeeID",
                ["employee_id"] = "EMP-67890"
            }
        };
        var newCredential = newIssuer.Issue(
            "https://credentials.example.com/EmployeeID",
            newCredentialPayload,
            new SdIssuanceOptions { DisclosureStructure = new { employee_id = true } },
            holderJwk);

        Console.WriteLine("New credential (issued today):");
        Console.WriteLine($"  Signed with: {newKey.KeyId}");
        Console.WriteLine($"  Expires: {DateTimeOffset.FromUnixTimeSeconds((long)newCredentialPayload.ExpiresAt!):yyyy-MM-dd}");
        ConsoleHelpers.PrintPreview("  SD-JWT", newCredential.Issuance, 50);

        // =====================================================================
        // STEP 7: Verifier accepts both during rotation
        // =====================================================================
        ConsoleHelpers.PrintStep(7, "Verifier accepts both keys");

        // Key resolver that fetches from JWKS (simulated)
        var keyInventory = new Dictionary<string, SecurityKey>
        {
            [oldKey.KeyId!] = oldKey,
            [newKey.KeyId!] = newKey
        };

        Task<SecurityKey> RotationAwareKeyResolver(JwtSecurityToken token)
        {
            // In production: fetch JWKS, parse kid from JWT header, return matching key
            // Here we simulate by returning based on token's key ID or issuer
            var kid = token.Header.Kid;
            if (!string.IsNullOrEmpty(kid) && keyInventory.TryGetValue(kid, out var key))
                return Task.FromResult(key);
            return Task.FromResult<SecurityKey>(oldKey);  // Fallback for demo
        }

        var verifier = new SdVerifier(RotationAwareKeyResolver);
        var validationParams = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuers = new[] { "https://credentials.example.com" },
            ValidateAudience = false,
            ValidateLifetime = true,
            // In real implementation, use IssuerSigningKeyResolver for multi-key support
            IssuerSigningKeys = new[] { oldKey, newKey }
        };

        Console.WriteLine("Verifier configured to accept both keys:");
        Console.WriteLine();

        // Create presentations for both credentials
        var oldHolder = new SdJwtHolder(oldCredential.Issuance);
        var oldPresentation = oldHolder.CreatePresentation(_ => false);

        var newHolder = new SdJwtHolder(newCredential.Issuance);
        var newPresentation = newHolder.CreatePresentation(_ => false);

        Console.WriteLine("  Verifying credential signed with OLD key...");
        var oldResult = await verifier.VerifyAsync(oldPresentation, validationParams);
        if (oldResult.ClaimsPrincipal != null)
            ConsoleHelpers.PrintSuccess($"    {oldKey.KeyId}: VALID");
        else
            Console.WriteLine($"    {oldKey.KeyId}: FAILED");

        Console.WriteLine();
        Console.WriteLine("  Verifying credential signed with NEW key...");
        var newResult = await verifier.VerifyAsync(newPresentation, validationParams);
        if (newResult.ClaimsPrincipal != null)
            ConsoleHelpers.PrintSuccess($"    {newKey.KeyId}: VALID");
        else
            Console.WriteLine($"    {newKey.KeyId}: FAILED");

        // =====================================================================
        // STEP 8: Rotation timeline
        // =====================================================================
        ConsoleHelpers.PrintStep(8, "Rotation timeline best practices");

        Console.WriteLine("Recommended rotation schedule:");
        Console.WriteLine();
        Console.WriteLine("  T-30 days:  Generate new key");
        Console.WriteLine("              Begin security review");
        Console.WriteLine();
        Console.WriteLine("  T-14 days:  Add new key to JWKS");
        Console.WriteLine("              Allows verifier caches to update");
        Console.WriteLine();
        Console.WriteLine("  T-0 (Rotation Day):");
        Console.WriteLine("              Switch to new key for all new credentials");
        Console.WriteLine("              Old key remains for verification only");
        Console.WriteLine();
        Console.WriteLine("  T+1 year:   Typical deprecation (or max credential lifetime)");
        Console.WriteLine("              Remove old key from JWKS");
        Console.WriteLine();
        Console.WriteLine("  T+1 year + 30 days:");
        Console.WriteLine("              Secure destruction of old key material");

        // =====================================================================
        // STEP 9: Emergency rotation
        // =====================================================================
        ConsoleHelpers.PrintStep(9, "Emergency key rotation");

        Console.WriteLine("If key compromise is suspected:");
        Console.WriteLine();
        Console.WriteLine("  1. IMMEDIATELY generate replacement key");
        Console.WriteLine();
        Console.WriteLine("  2. Remove compromised key from JWKS");
        Console.WriteLine("     This invalidates ALL credentials signed with it");
        Console.WriteLine();
        Console.WriteLine("  3. Notify affected parties");
        Console.WriteLine("     - Credential holders (reissue needed)");
        Console.WriteLine("     - Verifiers (update caches)");
        Console.WriteLine("     - Trust anchor (if in federation)");
        Console.WriteLine();
        Console.WriteLine("  4. Consider revoking credentials via Status List");
        Console.WriteLine("     Set revocation bit for all affected credentials");
        Console.WriteLine();
        Console.WriteLine("  5. Conduct incident review");
        Console.WriteLine("     Document timeline and remediation steps");

        // =====================================================================
        // COMPLETION
        // =====================================================================
        ConsoleHelpers.PrintCompletion("Tutorial 04: Key Rotation", new[]
        {
            "Understood key lifecycle phases",
            "Implemented rotation with overlap period",
            "Published both keys in JWKS during rotation",
            "Verified credentials from both key generations",
            "Learned emergency rotation procedures"
        });

        Console.WriteLine();
        Console.WriteLine("CONGRATULATIONS! You've completed all Advanced Tutorials!");
        Console.WriteLine();
        Console.WriteLine("NEXT STEPS:");
        Console.WriteLine("  - 04-UseCases: Real-world implementation patterns");
        Console.WriteLine("  - Review docs/architecture-design.md for internals");
    }
}
