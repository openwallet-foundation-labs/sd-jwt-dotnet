using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.Issuer;
using SdJwt.Net.Models;
using SdJwt.Net.Oid4Vci.Models;
using SdJwt.Net.Samples.Shared;
using SdJwt.Net.Vc.Issuer;
using SdJwt.Net.Vc.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text.Json;

namespace SdJwt.Net.Samples.Intermediate;

/// <summary>
/// Tutorial 03: OpenID4VCI (Credential Issuance)
///
/// LEARNING OBJECTIVES:
/// - Understand credential issuance protocol
/// - Work with issuer metadata
/// - Implement pre-authorized and authorization code flows
/// - Handle proof of possession in credential requests
///
/// TIME: ~15 minutes
/// </summary>
public static class OpenId4Vci
{
    public static Task Run()
    {
        ConsoleHelpers.PrintHeader("Tutorial 03: OpenID4VCI (Credential Issuance)");

        Console.WriteLine("OpenID for Verifiable Credential Issuance (OID4VCI) standardizes");
        Console.WriteLine("how wallets request and receive credentials from issuers.");
        Console.WriteLine();

        // Setup
        using var issuerEcdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        using var holderEcdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);

        var issuerKey = new ECDsaSecurityKey(issuerEcdsa) { KeyId = "university-2024" };
        var holderKey = new ECDsaSecurityKey(holderEcdsa) { KeyId = "wallet-key" };
        var holderJwk = JsonWebKeyConverter.ConvertFromSecurityKey(holderKey);

        // =====================================================================
        // STEP 1: Protocol overview
        // =====================================================================
        ConsoleHelpers.PrintStep(1, "OID4VCI protocol overview");

        Console.WriteLine("Two main flows for obtaining credentials:");
        Console.WriteLine();
        Console.WriteLine("  PRE-AUTHORIZED CODE FLOW:");
        Console.WriteLine("    - Issuer provides a code to user (email, QR, etc.)");
        Console.WriteLine("    - User enters code in wallet");
        Console.WriteLine("    - Wallet exchanges code for credential");
        Console.WriteLine("    - Use case: University emails graduation code");
        Console.WriteLine();
        Console.WriteLine("  AUTHORIZATION CODE FLOW:");
        Console.WriteLine("    - Wallet initiates OAuth-style authorization");
        Console.WriteLine("    - User authenticates with issuer");
        Console.WriteLine("    - Issuer returns authorization code");
        Console.WriteLine("    - Wallet exchanges for access token, then credential");
        Console.WriteLine("    - Use case: Employee requests from HR portal");

        // =====================================================================
        // STEP 2: Issuer metadata
        // =====================================================================
        ConsoleHelpers.PrintStep(2, "Issuer metadata discovery");

        Console.WriteLine("Wallets discover issuer capabilities via metadata:");
        Console.WriteLine();
        Console.WriteLine("  GET /.well-known/openid-credential-issuer");
        Console.WriteLine();

        var issuerMetadata = new
        {
            credential_issuer = "https://university.example.edu",
            credential_endpoint = "https://university.example.edu/credentials",
            credential_configurations_supported = new Dictionary<string, object>
            {
                ["UniversityDegree_SD-JWT"] = new
                {
                    format = Oid4VciConstants.SdJwtVcFormat,
                    vct = "https://credentials.university.example.edu/UniversityDegree",
                    cryptographic_binding_methods_supported = new[] { "jwk" },
                    credential_signing_alg_values_supported = new[] { "ES256" },
                    proof_types_supported = new Dictionary<string, object>
                    {
                        ["jwt"] = new
                        {
                            proof_signing_alg_values_supported = new[] { "ES256" }
                        }
                    },
                    claims = new Dictionary<string, object>
                    {
                        ["student_name"] = new { mandatory = true },
                        ["degree"] = new { mandatory = true },
                        ["graduation_date"] = new { mandatory = true },
                        ["gpa"] = new { mandatory = false }
                    }
                }
            }
        };

        Console.WriteLine("Metadata structure:");
        Console.WriteLine($"  credential_issuer: {issuerMetadata.credential_issuer}");
        Console.WriteLine($"  credential_endpoint: {issuerMetadata.credential_endpoint}");
        Console.WriteLine("  credential_configurations_supported:");
        Console.WriteLine("    UniversityDegree_SD-JWT:");
        Console.WriteLine($"      format: {Oid4VciConstants.SdJwtVcFormat}");
        Console.WriteLine("      vct: https://credentials.university.example.edu/UniversityDegree");
        Console.WriteLine("      proof_types_supported: jwt");
        Console.WriteLine("      claims: student_name, degree, graduation_date, gpa");

        // =====================================================================
        // STEP 3: Pre-authorized code flow
        // =====================================================================
        ConsoleHelpers.PrintStep(3, "Pre-authorized code flow");

        Console.WriteLine("Scenario: Alice graduates, receives code via email");
        Console.WriteLine();

        // Simulate credential offer from issuer
        var preAuthCode = "graduation_code_alice_2024";
        var credentialOffer = new
        {
            credential_issuer = "https://university.example.edu",
            credential_configuration_ids = new[] { "UniversityDegree_SD-JWT" },
            grants = new
            {
                urn_ietf_params_oauth_grant_type_pre_authorized_code = new
                {
                    pre_authorized_code = preAuthCode,
                    tx_code = new
                    {
                        input_mode = "numeric",
                        length = 6
                    }
                }
            }
        };

        Console.WriteLine("1. Alice receives credential offer (email/QR):");
        Console.WriteLine($"   pre_authorized_code: {preAuthCode}");
        Console.WriteLine("   tx_code required: 6-digit PIN");
        Console.WriteLine();

        // User enters PIN
        var userPin = "123456";
        Console.WriteLine($"2. Alice enters PIN: {userPin}");
        Console.WriteLine();

        // Token request
        Console.WriteLine("3. Wallet requests access token:");
        Console.WriteLine("   POST /token");
        Console.WriteLine("   {");
        Console.WriteLine($"     \"grant_type\": \"{Oid4VciConstants.GrantTypes.PreAuthorizedCode}\",");
        Console.WriteLine($"     \"pre-authorized_code\": \"{preAuthCode}\",");
        Console.WriteLine($"     \"tx_code\": \"{userPin}\"");
        Console.WriteLine("   }");
        Console.WriteLine();

        // Access token response (simulated)
        var accessToken = "eyJ...access_token...";
        Console.WriteLine("4. Issuer returns access token:");
        Console.WriteLine($"   access_token: {accessToken[..20]}...");
        Console.WriteLine("   c_nonce: Wq...  (for proof of possession)");

        // =====================================================================
        // STEP 4: Credential request with proof
        // =====================================================================
        ConsoleHelpers.PrintStep(4, "Credential request with proof of possession");

        Console.WriteLine("Wallet proves control of holder key in credential request:");
        Console.WriteLine();

        // Create proof JWT
        var cNonce = "university_nonce_12345";
        var proofPayload = new JwtPayload
        {
            [JwtRegisteredClaimNames.Iss] = "https://alice-wallet.example.com",
            [JwtRegisteredClaimNames.Aud] = "https://university.example.edu",
            [JwtRegisteredClaimNames.Iat] = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            ["nonce"] = cNonce
        };

        var proofHeader = new JwtHeader(
            new SigningCredentials(holderKey, SecurityAlgorithms.EcdsaSha256))
        {
            ["typ"] = "openid4vci-proof+jwt"
        };
        proofHeader["jwk"] = holderJwk;

        var proofJwt = new JwtSecurityTokenHandler().WriteToken(
            new JwtSecurityToken(proofHeader, proofPayload));

        Console.WriteLine("Proof JWT structure:");
        Console.WriteLine("  Header:");
        Console.WriteLine("    typ: openid4vci-proof+jwt");
        Console.WriteLine("    alg: ES256");
        Console.WriteLine("    jwk: { holder's public key }");
        Console.WriteLine("  Payload:");
        Console.WriteLine($"    iss: https://alice-wallet.example.com");
        Console.WriteLine($"    aud: https://university.example.edu");
        Console.WriteLine($"    nonce: {cNonce}");
        Console.WriteLine();

        Console.WriteLine("Credential request:");
        Console.WriteLine("  POST /credentials");
        Console.WriteLine("  Authorization: Bearer <access_token>");
        Console.WriteLine("  {");
        Console.WriteLine($"    \"format\": \"{Oid4VciConstants.SdJwtVcFormat}\",");
        Console.WriteLine("    \"credential_configuration_id\": \"UniversityDegree_SD-JWT\",");
        Console.WriteLine("    \"proof\": {");
        Console.WriteLine("      \"proof_type\": \"jwt\",");
        Console.WriteLine($"      \"jwt\": \"{proofJwt[..40]}...\"");
        Console.WriteLine("    }");
        Console.WriteLine("  }");

        // =====================================================================
        // STEP 5: Issue credential
        // =====================================================================
        ConsoleHelpers.PrintStep(5, "Issuer creates and returns credential");

        var vcIssuer = new SdJwtVcIssuer(issuerKey, SecurityAlgorithms.EcdsaSha256);

        var vcPayload = new SdJwtVcPayload
        {
            Issuer = "https://university.example.edu",
            Subject = "did:example:alice123",
            IssuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            ExpiresAt = DateTimeOffset.UtcNow.AddYears(10).ToUnixTimeSeconds(),
            AdditionalData = new Dictionary<string, object>
            {
                ["student_name"] = "Alice Johnson",
                ["degree"] = "Bachelor of Science in Computer Science",
                ["graduation_date"] = "2024-06-15",
                ["gpa"] = 3.85
            }
        };

        var sdOptions = new SdIssuanceOptions
        {
            DisclosureStructure = new { gpa = true }
        };

        var credential = vcIssuer.Issue(
            "https://credentials.university.example.edu/UniversityDegree",
            vcPayload,
            sdOptions,
            holderJwk);

        Console.WriteLine("Credential response:");
        Console.WriteLine("  {");
        Console.WriteLine($"    \"format\": \"{Oid4VciConstants.SdJwtVcFormat}\",");
        ConsoleHelpers.PrintPreview("    \"credential\"", credential.Issuance, 50);
        Console.WriteLine("  }");
        Console.WriteLine();
        ConsoleHelpers.PrintSuccess("Credential issued and bound to holder key");

        // =====================================================================
        // STEP 6: Authorization code flow
        // =====================================================================
        ConsoleHelpers.PrintStep(6, "Authorization code flow (overview)");

        Console.WriteLine("For user-initiated credential requests:");
        Console.WriteLine();
        Console.WriteLine("  1. Wallet discovers issuer metadata");
        Console.WriteLine("  2. Wallet opens authorization endpoint in browser");
        Console.WriteLine("     GET /authorize?");
        Console.WriteLine("       response_type=code&");
        Console.WriteLine("       client_id=wallet&");
        Console.WriteLine("       authorization_details=[{\"type\":\"openid_credential\",...}]&");
        Console.WriteLine("       redirect_uri=wallet://callback");
        Console.WriteLine();
        Console.WriteLine("  3. User authenticates with issuer (login page)");
        Console.WriteLine("  4. Issuer redirects back with authorization code");
        Console.WriteLine("     wallet://callback?code=auth_code_xyz");
        Console.WriteLine();
        Console.WriteLine("  5. Wallet exchanges code for access token");
        Console.WriteLine("  6. Wallet requests credential (same as pre-auth flow)");

        // =====================================================================
        // STEP 7: Batch and deferred issuance
        // =====================================================================
        ConsoleHelpers.PrintStep(7, "Advanced: batch and deferred issuance");

        Console.WriteLine("BATCH ISSUANCE:");
        Console.WriteLine("  Request multiple credentials in one call.");
        Console.WriteLine("  Use case: Graduation ceremony - degree + transcript");
        Console.WriteLine();
        Console.WriteLine("DEFERRED ISSUANCE:");
        Console.WriteLine("  Issuer returns acceptance_token instead of credential.");
        Console.WriteLine("  Wallet polls later to retrieve credential.");
        Console.WriteLine("  Use case: Background check takes 24 hours.");
        Console.WriteLine();
        Console.WriteLine("NOTIFICATION ENDPOINT:");
        Console.WriteLine("  Wallet notifies issuer when credential is successfully stored.");
        Console.WriteLine("  Helps with audit logging and delivery confirmation.");

        // =====================================================================
        // COMPLETION
        // =====================================================================
        ConsoleHelpers.PrintCompletion("Tutorial 03: OpenID4VCI", new[]
        {
            "Understood OID4VCI protocol flows",
            "Explored issuer metadata structure",
            "Implemented pre-authorized code flow",
            "Created proof of possession JWT",
            "Learned batch and deferred issuance"
        });

        Console.WriteLine();
        Console.WriteLine("NEXT: Tutorial 04 - OpenID4VP presentations");

        return Task.CompletedTask;
    }
}
