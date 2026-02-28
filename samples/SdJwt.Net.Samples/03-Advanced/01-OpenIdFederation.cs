using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.OidFederation;
using SdJwt.Net.OidFederation.Models;
using SdJwt.Net.Samples.Shared;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text.Json;

namespace SdJwt.Net.Samples.Advanced;

/// <summary>
/// Tutorial 01: OpenID Federation
///
/// LEARNING OBJECTIVES:
/// - Understand trust chains vs static trust lists
/// - Create and validate entity statements
/// - Build trust chain resolution
/// - Work with federation metadata
///
/// TIME: ~20 minutes
/// </summary>
public static class OpenIdFederation
{
    public static async Task Run()
    {
        ConsoleHelpers.PrintHeader("Tutorial 01: OpenID Federation");

        Console.WriteLine("OpenID Federation enables dynamic trust through cryptographic chains");
        Console.WriteLine("rather than pre-configured trust lists. This is essential for");
        Console.WriteLine("large-scale credential ecosystems.");
        Console.WriteLine();

        // Setup - create keys for trust hierarchy
        using var trustAnchorEcdsa = ECDsa.Create(ECCurve.NamedCurves.nistP384);
        using var intermediateEcdsa = ECDsa.Create(ECCurve.NamedCurves.nistP384);
        using var issuerEcdsa = ECDsa.Create(ECCurve.NamedCurves.nistP384);

        var trustAnchorKey = new ECDsaSecurityKey(trustAnchorEcdsa) { KeyId = "ta-2024" };
        var intermediateKey = new ECDsaSecurityKey(intermediateEcdsa) { KeyId = "int-2024" };
        var issuerKey = new ECDsaSecurityKey(issuerEcdsa) { KeyId = "issuer-2024" };

        // =====================================================================
        // STEP 1: The trust problem
        // =====================================================================
        ConsoleHelpers.PrintStep(1, "The trust problem");

        Console.WriteLine("TRADITIONAL TRUST LISTS:");
        Console.WriteLine("  - Verifier maintains list of trusted issuers");
        Console.WriteLine("  - Manual updates when issuers join/leave");
        Console.WriteLine("  - Doesn't scale beyond hundreds of issuers");
        Console.WriteLine();
        Console.WriteLine("FEDERATED TRUST:");
        Console.WriteLine("  - Verifier trusts ONE trust anchor");
        Console.WriteLine("  - Trust anchor certifies intermediates");
        Console.WriteLine("  - Intermediates certify issuers");
        Console.WriteLine("  - Automatic discovery, dynamic updates");
        Console.WriteLine();
        Console.WriteLine("Example: EU Digital Wallet ecosystem");
        Console.WriteLine("  - Trust anchor: EU Commission");
        Console.WriteLine("  - Intermediates: National governments");
        Console.WriteLine("  - Issuers: Thousands of universities, DMVs, etc.");

        // =====================================================================
        // STEP 2: Entity statements
        // =====================================================================
        ConsoleHelpers.PrintStep(2, "Entity statements");

        Console.WriteLine("Every federation participant publishes an entity statement:");
        Console.WriteLine();
        Console.WriteLine("  GET https://issuer.example.edu/.well-known/openid-federation");
        Console.WriteLine("  Returns: signed JWT with entity metadata");
        Console.WriteLine();

        // Create a self-signed entity configuration (trust anchor)
        var trustAnchorJwk = JsonWebKeyConverter.ConvertFromSecurityKey(trustAnchorKey);
        var trustAnchorConfig = new EntityConfiguration
        {
            Issuer = "https://federation.example.eu",
            Subject = "https://federation.example.eu",  // Self-signed
            IssuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(365).ToUnixTimeSeconds(),
            JwkSet = new JsonWebKeySet { Keys = { trustAnchorJwk } },
            Metadata = new EntityMetadata
            {
                FederationEntity = new FederationEntityMetadata
                {
                    Name = "EU Trust Services"
                }
            }
        };

        Console.WriteLine("Trust Anchor entity configuration:");
        Console.WriteLine($"  iss: {trustAnchorConfig.Issuer}");
        Console.WriteLine($"  sub: {trustAnchorConfig.Subject}");
        Console.WriteLine("  jwks: [public key for verification]");
        Console.WriteLine("  metadata.federation_entity:");
        Console.WriteLine($"    name: {trustAnchorConfig.Metadata.FederationEntity?.Name}");
        Console.WriteLine();
        Console.WriteLine("This is a SELF-SIGNED configuration (iss == sub).");
        Console.WriteLine("Trust anchors are the root of trust - must be pre-configured.");

        // =====================================================================
        // STEP 3: Subordinate statements
        // =====================================================================
        ConsoleHelpers.PrintStep(3, "Subordinate statements");

        Console.WriteLine("Trust anchors issue statements ABOUT subordinates:");
        Console.WriteLine();

        var intermediateJwk = JsonWebKeyConverter.ConvertFromSecurityKey(intermediateKey);
        var subordinateStatement = new EntityStatement
        {
            Issuer = "https://federation.example.eu",  // Trust anchor signs
            Subject = "https://education.gov.example",  // About intermediate
            IssuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(365).ToUnixTimeSeconds(),
            JwkSet = new JsonWebKeySet { Keys = { intermediateJwk } }
        };

        Console.WriteLine($"  iss: {subordinateStatement.Issuer} (trust anchor)");
        Console.WriteLine($"  sub: {subordinateStatement.Subject} (subordinate)");
        Console.WriteLine("  jwks: [subordinate's public key]");
        Console.WriteLine();
        Console.WriteLine("This says: \"I (trust anchor) certify that education.gov.example");
        Console.WriteLine("is authorized to act in this federation.\"");
        Console.WriteLine();
        Console.WriteLine("Subordinate statements are fetched via:");
        Console.WriteLine($"  GET {trustAnchorConfig.Issuer}/fetch?sub={Uri.EscapeDataString(subordinateStatement.Subject)}");

        // =====================================================================
        // STEP 4: Trust chains
        // =====================================================================
        ConsoleHelpers.PrintStep(4, "Trust chains");

        Console.WriteLine("A trust chain is a path from leaf to trust anchor:");
        Console.WriteLine();
        Console.WriteLine("  [Issuer Self-Statement]");
        Console.WriteLine("         |");
        Console.WriteLine("         | signed by issuer's key");
        Console.WriteLine("         v");
        Console.WriteLine("  [Intermediate -> Issuer Statement]");
        Console.WriteLine("         |");
        Console.WriteLine("         | signed by intermediate's key");
        Console.WriteLine("         v");
        Console.WriteLine("  [Trust Anchor -> Intermediate Statement]");
        Console.WriteLine("         |");
        Console.WriteLine("         | signed by trust anchor's key");
        Console.WriteLine("         v");
        Console.WriteLine("  [Trust Anchor Self-Statement]");
        Console.WriteLine("         |");
        Console.WriteLine("         | (pre-configured trust)");
        Console.WriteLine();
        Console.WriteLine("Chain validation:");
        Console.WriteLine("  1. Parse all statements");
        Console.WriteLine("  2. Verify each signature with issuer's key from previous statement");
        Console.WriteLine("  3. Check all statements are not expired");
        Console.WriteLine("  4. Verify chain ends at known trust anchor");

        // =====================================================================
        // STEP 5: Entity configuration
        // =====================================================================
        ConsoleHelpers.PrintStep(5, "Entity configuration endpoint");

        Console.WriteLine("Every entity MUST host their self-signed statement at:");
        Console.WriteLine("  {entity_id}/.well-known/openid-federation");
        Console.WriteLine();
        Console.WriteLine("Example for a credential issuer:");

        var issuerJwk = JsonWebKeyConverter.ConvertFromSecurityKey(issuerKey);
        var issuerConfig = new EntityConfiguration
        {
            Issuer = "https://university.example.edu",
            Subject = "https://university.example.edu",
            IssuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(30).ToUnixTimeSeconds(),
            JwkSet = new JsonWebKeySet { Keys = { issuerJwk } },
            Metadata = new EntityMetadata
            {
                FederationEntity = new FederationEntityMetadata
                {
                    Name = "Example University"
                },
                OpenIdCredentialIssuer = new
                {
                    credential_issuer = "https://university.example.edu",
                    credential_endpoint = "https://university.example.edu/credentials",
                    credential_configurations_supported = new Dictionary<string, object>
                    {
                        ["UniversityDegree"] = new
                        {
                            format = "vc+sd-jwt",
                            scope = "openid_credential:UniversityDegree",
                            cryptographic_binding_methods_supported = new[] { "jwk" },
                            credential_signing_alg_values_supported = new[] { "ES256", "ES384" }
                        }
                    }
                }
            },
            AuthorityHints = new[] { "https://education.gov.example" }
        };

        Console.WriteLine();
        Console.WriteLine($"GET {issuerConfig.Issuer}/.well-known/openid-federation");
        Console.WriteLine();
        Console.WriteLine("Returns entity configuration with:");
        Console.WriteLine($"  iss/sub: {issuerConfig.Issuer}");
        Console.WriteLine("  jwks: [issuer's public keys]");
        Console.WriteLine("  metadata:");
        Console.WriteLine("    federation_entity: organization info");
        Console.WriteLine("    openid_credential_issuer: credential capabilities");
        Console.WriteLine("  authority_hints: [\"https://education.gov.example\"]");
        Console.WriteLine();
        Console.WriteLine("authority_hints tells resolvers where to look for subordinate statements.");

        // =====================================================================
        // STEP 6: Trust chain resolution
        // =====================================================================
        ConsoleHelpers.PrintStep(6, "Trust chain resolution algorithm");

        Console.WriteLine("Verifier resolves trust chain automatically:");
        Console.WriteLine();
        Console.WriteLine("  1. Fetch issuer's entity configuration");
        Console.WriteLine("     GET https://university.example.edu/.well-known/openid-federation");
        Console.WriteLine();
        Console.WriteLine("  2. Follow authority_hints upward");
        Console.WriteLine("     GET https://education.gov.example/fetch?sub=university.example.edu");
        Console.WriteLine("     GET https://education.gov.example/.well-known/openid-federation");
        Console.WriteLine();
        Console.WriteLine("  3. Continue until reaching trust anchor");
        Console.WriteLine("     GET https://federation.example.eu/fetch?sub=education.gov.example");
        Console.WriteLine("     GET https://federation.example.eu/.well-known/openid-federation");
        Console.WriteLine();
        Console.WriteLine("  4. Validate signatures top-down from trust anchor");
        Console.WriteLine();
        Console.WriteLine("The resolver caches results but refreshes before expiry.");

        // =====================================================================
        // STEP 7: Metadata policies
        // =====================================================================
        ConsoleHelpers.PrintStep(7, "Metadata policies");

        Console.WriteLine("Superiors can constrain subordinate metadata:");
        Console.WriteLine();
        Console.WriteLine("Example: Trust anchor requires specific algorithms");
        Console.WriteLine();
        Console.WriteLine("  \"metadata_policy\": {");
        Console.WriteLine("    \"openid_credential_issuer\": {");
        Console.WriteLine("      \"credential_signing_alg_values_supported\": {");
        Console.WriteLine("        \"subset_of\": [\"ES256\", \"ES384\", \"ES512\"],");
        Console.WriteLine("        \"superset_of\": [\"ES256\"]");
        Console.WriteLine("      }");
        Console.WriteLine("    }");
        Console.WriteLine("  }");
        Console.WriteLine();
        Console.WriteLine("Policy operators:");
        Console.WriteLine("  subset_of:   Value must be subset of specified array");
        Console.WriteLine("  superset_of: Value must be superset of specified array");
        Console.WriteLine("  add:         Add values to subordinate's array");
        Console.WriteLine("  default:     Use value if subordinate doesn't specify");
        Console.WriteLine("  essential:   Field is required (boolean)");

        // =====================================================================
        // STEP 8: Practical usage
        // =====================================================================
        ConsoleHelpers.PrintStep(8, "Practical usage in verification");

        Console.WriteLine("When verifying a credential, the verifier:");
        Console.WriteLine();
        Console.WriteLine("  1. Extract issuer from SD-JWT (iss claim)");
        Console.WriteLine("  2. Resolve trust chain to known trust anchor");
        Console.WriteLine("  3. Apply metadata policies down the chain");
        Console.WriteLine("  4. Use resolved metadata to validate credential");
        Console.WriteLine("  5. Verify SD-JWT signature with issuer's key");
        Console.WriteLine();
        Console.WriteLine("Benefits:");
        Console.WriteLine("  - No manual trust list management");
        Console.WriteLine("  - Automatic key rotation (just update entity statement)");
        Console.WriteLine("  - Hierarchical governance");
        Console.WriteLine("  - Revocation by removing subordinate statements");

        // =====================================================================
        // COMPLETION
        // =====================================================================
        ConsoleHelpers.PrintCompletion("Tutorial 01: OpenID Federation", new[]
        {
            "Understood federated trust vs static lists",
            "Created entity statements (self-signed and subordinate)",
            "Learned trust chain structure and resolution",
            "Explored metadata policies",
            "Saw integration with credential verification"
        });

        Console.WriteLine();
        Console.WriteLine("NEXT: Tutorial 02 - HAIP Compliance for high-security requirements");
    }
}
