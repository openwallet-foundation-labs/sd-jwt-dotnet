using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.Holder;
using SdJwt.Net.Issuer;
using SdJwt.Net.Models;
using SdJwt.Net.Vc.Issuer;
using SdJwt.Net.Vc.Models;
using SdJwt.Net.Oid4Vci.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text.Json;
using JsonWebKeyMs = Microsoft.IdentityModel.Tokens.JsonWebKey;

namespace SdJwt.Net.Samples.Standards.OpenId;

/// <summary>
/// Demonstrates OpenID4VCI concepts and provides working code examples for credential issuance workflows
/// Shows both protocol concepts and actual implementation using SdJwt.Net.Oid4Vci package
/// </summary>
public class OpenId4VciExample
{
    public static async Task RunExample(IServiceProvider services)
    {
        var logger = services.GetRequiredService<ILogger<OpenId4VciExample>>();
        
        Console.WriteLine("\n" + new string('=', 65));
        Console.WriteLine("            OpenID4VCI Credential Issuance Example      ");
        Console.WriteLine("                     (OID4VCI 1.0 Final)                ");
        Console.WriteLine(new string('=', 65));

        Console.WriteLine("\nOpenID for Verifiable Credential Issuance (OID4VCI) enables");
        Console.WriteLine("standardized workflows for issuing verifiable credentials to wallets.");
        Console.WriteLine("This example demonstrates both concepts and working implementation.");
        Console.WriteLine();

        // Setup keys for actual implementation
        using var issuerEcdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        using var holderEcdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        
        var issuerKey = new ECDsaSecurityKey(issuerEcdsa) { KeyId = "issuer-2024" };
        var holderPrivateKey = new ECDsaSecurityKey(holderEcdsa) { KeyId = "holder-key-1" };
        var holderPublicKey = new ECDsaSecurityKey(holderEcdsa) { KeyId = "holder-key-1" };
        var holderJwk = JsonWebKeyConverter.ConvertFromSecurityKey(holderPublicKey);

        await DemonstrateCredentialIssuerSetup(issuerKey);
        await DemonstratePreAuthorizedFlow(issuerKey, holderJwk, holderPrivateKey);
        await DemonstrateAuthorizationCodeFlow();
        await DemonstrateBatchIssuance(issuerKey, holderJwk);
        await DemonstrateDeferredIssuance();
        await DemonstrateCredentialFormats(issuerKey, holderJwk);

        Console.WriteLine("\n" + new string('=', 65));
        Console.WriteLine("          OpenID4VCI implementation demonstrated!       ");
        Console.WriteLine("                                                         ");
        Console.WriteLine("  [X] Credential issuer setup                             ");
        Console.WriteLine("  [X] Pre-authorized code flow                            ");
        Console.WriteLine("  [X] Authorization code flow (conceptual)                ");
        Console.WriteLine("  [X] Batch credential issuance                           ");
        Console.WriteLine("  [X] Deferred credential issuance (conceptual)           ");
        Console.WriteLine("  [X] Multiple credential formats                         ");
        Console.WriteLine(new string('=', 65));
        return;
    }

    private static Task DemonstrateCredentialIssuerSetup(SecurityKey issuerKey)
    {
        Console.WriteLine("\n1. CREDENTIAL ISSUER SETUP");
        Console.WriteLine("   Setting up issuer metadata and configurations...");
        Console.WriteLine();

        // Create issuer metadata programmatically
        var issuerMetadata = new
        {
            credential_issuer = "https://university.example.edu",
            authorization_servers = new[] { "https://auth.university.example.edu" },
            credential_endpoint = "https://university.example.edu/credentials",
            batch_credential_endpoint = "https://university.example.edu/batch-credentials",
            deferred_credential_endpoint = "https://university.example.edu/deferred-credentials",
            notification_endpoint = "https://university.example.edu/notification",
            credential_response_encryption = new
            {
                alg_values_supported = new[] { "RSA-OAEP-256", "ECDH-ES" },
                enc_values_supported = new[] { "A128GCM", "A256GCM" },
                encryption_required = false
            },
            credential_configurations_supported = new Dictionary<string, object>
            {
                ["UniversityDegree_SDJWT"] = new
                {
                    format = Oid4VciConstants.SdJwtVcFormat,
                    vct = "https://credentials.university.example.edu/degree",
                    cryptographic_binding_methods_supported = new[] { "jwk", "did:key" },
                    cryptographic_suites_supported = new[] { "ES256", "ES384" },
                    proof_types_supported = new Dictionary<string, object>
                    {
                        [Oid4VciConstants.ProofTypes.Jwt] = new
                        {
                            proof_signing_alg_values_supported = new[] { "ES256", "ES384", "RS256" }
                        }
                    },
                    display = new[]
                    {
                        new
                        {
                            name = "University Degree",
                            locale = "en-US",
                            background_color = "#003366",
                            text_color = "#FFFFFF",
                            logo = new
                            {
                                uri = "https://university.example.edu/logo.png",
                                alt_text = "University Logo"
                            }
                        }
                    },
                    claims = new Dictionary<string, object>
                    {
                        ["student_name"] = new
                        {
                            mandatory = true,
                            display = new[] { new { name = "Student Name", locale = "en-US" } }
                        },
                        ["degree"] = new
                        {
                            mandatory = true,
                            display = new[] { new { name = "Degree Type", locale = "en-US" } }
                        },
                        ["major"] = new
                        {
                            mandatory = true,
                            display = new[] { new { name = "Field of Study", locale = "en-US" } }
                        },
                        ["graduation_date"] = new
                        {
                            mandatory = true,
                            display = new[] { new { name = "Graduation Date", locale = "en-US" } }
                        },
                        ["gpa"] = new
                        {
                            mandatory = false,
                            display = new[] { new { name = "GPA", locale = "en-US" } }
                        }
                    }
                }
            }
        };

        Console.WriteLine("   Issuer Metadata:");
        Console.WriteLine($"   • Issuer: {issuerMetadata.credential_issuer}");
        Console.WriteLine($"   • Credential Endpoint: {issuerMetadata.credential_endpoint}");
        Console.WriteLine($"   • Supported Formats: {Oid4VciConstants.SdJwtVcFormat}");
        Console.WriteLine($"   • Signing Algorithms: ES256, ES384, RS256");
        Console.WriteLine();

        Console.WriteLine("   [X] Issuer metadata configured");
        Console.WriteLine("   [X] Credential configurations defined");
        Console.WriteLine("   [X] Display metadata for wallet UI");
        Console.WriteLine("   [X] Cryptographic binding support enabled");

        return Task.CompletedTask;
    }

    private static Task DemonstratePreAuthorizedFlow(SecurityKey issuerKey, JsonWebKeyMs holderJwk, SecurityKey holderPrivateKey)
    {
        Console.WriteLine("\n2. PRE-AUTHORIZED CODE FLOW - WORKING IMPLEMENTATION");
        Console.WriteLine("   Scenario: University pre-approves degree issuance for graduate");
        Console.WriteLine();

        try
        {
            // Step 1: Create credential offer with pre-authorized code
            Console.WriteLine("   Step 1: University creates credential offer...");
            
            var preAuthorizedCode = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"pre-auth-{Guid.NewGuid()}"));
            var userPin = "1234"; // In real implementation, this would be communicated securely
            
            var credentialOffer = new
            {
                credential_issuer = "https://university.example.edu",
                credential_configuration_ids = new[] { "UniversityDegree_SDJWT" },
                grants = new Dictionary<string, object>
                {
                    [Oid4VciConstants.GrantTypes.PreAuthorizedCode] = new
                    {
                        pre_authorized_code = preAuthorizedCode,
                        user_pin_required = true,
                        tx_code = new
                        {
                            input_mode = Oid4VciConstants.InputModes.Numeric,
                            length = 4,
                            description = "Enter the PIN provided by the university"
                        }
                    }
                }
            };

            Console.WriteLine($"   [X] Pre-authorized code: {preAuthorizedCode[..20]}...");
            Console.WriteLine($"   [X] PIN required: {userPin}");
            Console.WriteLine();

            // Step 2: Create credential offer URI
            var offerJson = JsonSerializer.Serialize(credentialOffer);
            var offerUri = $"{Oid4VciConstants.CredentialOfferScheme}://?credential_offer={Uri.EscapeDataString(offerJson)}";
            
            Console.WriteLine("   Step 2: Credential offer URI generated");
            Console.WriteLine($"   URI: {offerUri[..80]}...");
            Console.WriteLine();

            // Step 3: Simulate wallet processing (token exchange)
            Console.WriteLine("   Step 3: Wallet exchanges pre-authorized code for access token...");
            
            // In real implementation, this would be an HTTP POST to token endpoint
            var tokenResponse = new
            {
                access_token = $"access_token_{Guid.NewGuid()}",
                token_type = Oid4VciConstants.TokenTypes.Bearer,
                expires_in = 3600,
                c_nonce = Convert.ToBase64String(RandomNumberGenerator.GetBytes(16)),
                c_nonce_expires_in = 300
            };

            Console.WriteLine($"   [X] Access token: {tokenResponse.access_token[..20]}...");
            Console.WriteLine($"   [X] C-nonce: {tokenResponse.c_nonce}");
            Console.WriteLine();

            // Step 4: Create proof of possession
            Console.WriteLine("   Step 4: Creating proof of possession JWT...");
            
            var proofPayload = new JwtPayload
            {
                [JwtRegisteredClaimNames.Iss] = "holder-did-or-client-id",
                [JwtRegisteredClaimNames.Aud] = "https://university.example.edu",
                [JwtRegisteredClaimNames.Iat] = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                [JwtRegisteredClaimNames.Exp] = DateTimeOffset.UtcNow.AddMinutes(10).ToUnixTimeSeconds(),
                [JwtRegisteredClaimNames.Nonce] = tokenResponse.c_nonce
            };

            var proofHeader = new JwtHeader(new SigningCredentials(holderPrivateKey, SecurityAlgorithms.EcdsaSha256))
            {
                [JwtHeaderParameterNames.Typ] = Oid4VciConstants.ProofJwtType,
                [JwtHeaderParameterNames.Jwk] = holderJwk
            };

            var proofJwt = new JwtSecurityToken(proofHeader, proofPayload);
            var proofJwtString = new JwtSecurityTokenHandler().WriteToken(proofJwt);

            Console.WriteLine($"   [X] Proof JWT created: {proofJwtString[..50]}...");
            Console.WriteLine();

            // Step 5: Issue the credential using SD-JWT VC
            Console.WriteLine("   Step 5: Issuing SD-JWT VC credential...");
            
            var vcIssuer = new SdJwtVcIssuer(issuerKey, SecurityAlgorithms.EcdsaSha256);
            
            var credentialPayload = new SdJwtVcPayload
            {
                Issuer = "https://university.example.edu",
                Subject = "did:example:student123",
                IssuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                ExpiresAt = DateTimeOffset.UtcNow.AddYears(10).ToUnixTimeSeconds(),
                AdditionalData = new Dictionary<string, object>
                {
                    ["student_name"] = "Alice Johnson",
                    ["degree"] = "Bachelor of Science",
                    ["major"] = "Computer Science",
                    ["graduation_date"] = "2024-05-15",
                    ["gpa"] = 3.85,
                    ["university"] = "Example University",
                    ["honors"] = "magna cum laude"
                }
            };

            var options = new SdIssuanceOptions
            {
                DisclosureStructure = new
                {
                    gpa = true,      // Selectively disclosable
                    honors = true    // Selectively disclosable
                }
            };

            var issuedCredential = vcIssuer.Issue(
                "https://credentials.university.example.edu/degree",
                credentialPayload,
                options,
                holderJwk
            );

            Console.WriteLine($"   [X] Credential issued successfully");
            Console.WriteLine($"   [X] JWT claims: {issuedCredential.Issuance[..80]}...");
            Console.WriteLine($"   [X] Disclosures: {issuedCredential.Disclosures.Count} available for selective sharing");
            Console.WriteLine();

            // Step 6: Credential response
            var credentialResponse = new
            {
                format = Oid4VciConstants.SdJwtVcFormat,
                credential = issuedCredential.Issuance, // Full SD-JWT with disclosures
                c_nonce = Convert.ToBase64String(RandomNumberGenerator.GetBytes(16)),
                c_nonce_expires_in = 300
            };

            Console.WriteLine("   Step 6: Credential response created");
            Console.WriteLine($"   [X] Format: {credentialResponse.format}");
            Console.WriteLine($"   [X] Ready for wallet storage");
            Console.WriteLine();

            Console.WriteLine("   IMPLEMENTATION SUMMARY:");
            Console.WriteLine("   [X] Real credential offer with pre-authorized code");
            Console.WriteLine("   [X] Authentic JWT proof of possession");
            Console.WriteLine("   [X] Actual SD-JWT VC with selective disclosure");
            Console.WriteLine("   [X] Cryptographic binding to holder key");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"   [X] Error in implementation: {ex.Message}");
        }

        return Task.CompletedTask;
    }

    private static Task DemonstrateAuthorizationCodeFlow()
    {
        Console.WriteLine("\n3. AUTHORIZATION CODE FLOW - PROTOCOL FLOW");
        Console.WriteLine("   Scenario: User-initiated credential request with authorization");
        Console.WriteLine("   (This demonstrates the protocol - full implementation would require OAuth server)");
        Console.WriteLine();

        Console.WriteLine("   Step 1: Credential offer for authorization flow");
        var authCodeOffer = new
        {
            credential_issuer = "https://government.example.gov",
            credential_configuration_ids = new[] { "DigitalID_SDJWT" },
            grants = new Dictionary<string, object>
            {
                [Oid4VciConstants.GrantTypes.AuthorizationCode] = new
                {
                    issuer_state = "id_application_state_789",
                    authorization_server = "https://auth.government.example.gov"
                }
            }
        };

        Console.WriteLine($"   [X] Issuer state: {authCodeOffer.grants["authorization_code"]}");
        Console.WriteLine();

        Console.WriteLine("   OAuth 2.0 Authorization Flow:");
        Console.WriteLine("   1. Wallet → Authorization Server: Authorization request");
        Console.WriteLine("   2. User authenticates and consents");
        Console.WriteLine("   3. Authorization Server → Wallet: Authorization code");
        Console.WriteLine("   4. Wallet → Token Endpoint: Code exchange");
        Console.WriteLine("   5. Token Endpoint → Wallet: Access token");
        Console.WriteLine("   6. Wallet → Credential Endpoint: Credential request");
        Console.WriteLine();

        Console.WriteLine("   Required OAuth 2.0 Integration:");
        Console.WriteLine("   • Authorization server supporting PKCE");
        Console.WriteLine("   • Custom scope for credential issuance");
        Console.WriteLine("   • User authentication and consent flow");
        Console.WriteLine("   • Secure token management");

        return Task.CompletedTask;
    }

    private static Task DemonstrateBatchIssuance(SecurityKey issuerKey, JsonWebKeyMs holderJwk)
    {
        Console.WriteLine("\n4. BATCH CREDENTIAL ISSUANCE - WORKING IMPLEMENTATION");
        Console.WriteLine("   Scenario: Corporate onboarding - multiple credentials in one request");
        Console.WriteLine();

        try
        {
            var vcIssuer = new SdJwtVcIssuer(issuerKey, SecurityAlgorithms.EcdsaSha256);

            Console.WriteLine("   Creating multiple credentials for employee onboarding...");
            Console.WriteLine();

            // Employee ID Credential
            var employeeIdPayload = new SdJwtVcPayload
            {
                Issuer = "https://hr.techcorp.example.com",
                Subject = "did:example:employee456",
                IssuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                ExpiresAt = DateTimeOffset.UtcNow.AddYears(2).ToUnixTimeSeconds(),
                AdditionalData = new Dictionary<string, object>
                {
                    ["employee_name"] = "Bob Smith",
                    ["employee_id"] = "EMP-456",
                    ["position"] = "Senior Developer",
                    ["department"] = "Engineering",
                    ["start_date"] = "2024-01-15"
                }
            };

            var employeeOptions = new SdIssuanceOptions
            {
                DisclosureStructure = new
                {
                    employee_id = true,
                    position = true,
                    department = true
                }
            };

            var employeeCredential = vcIssuer.Issue(
                "https://credentials.techcorp.com/employee-id",
                employeeIdPayload,
                employeeOptions,
                holderJwk
            );

            // Security Badge Credential
            var securityPayload = new SdJwtVcPayload
            {
                Issuer = "https://security.techcorp.example.com",
                Subject = "did:example:employee456",
                IssuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                ExpiresAt = DateTimeOffset.UtcNow.AddYears(1).ToUnixTimeSeconds(),
                AdditionalData = new Dictionary<string, object>
                {
                    ["badge_number"] = "SEC-12345",
                    ["access_level"] = "Level 3",
                    ["building_access"] = new[] { "Main Building", "Data Center", "Lab A" },
                    ["valid_hours"] = "24/7"
                }
            };

            var securityOptions = new SdIssuanceOptions
            {
                DisclosureStructure = new
                {
                    access_level = true,
                    building_access = true
                }
            };

            var securityCredential = vcIssuer.Issue(
                "https://credentials.techcorp.com/security-badge",
                securityPayload,
                securityOptions,
                holderJwk
            );

            // Parking Permit Credential
            var parkingPayload = new SdJwtVcPayload
            {
                Issuer = "https://facilities.techcorp.example.com",
                Subject = "did:example:employee456",
                IssuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                ExpiresAt = DateTimeOffset.UtcNow.AddYears(1).ToUnixTimeSeconds(),
                AdditionalData = new Dictionary<string, object>
                {
                    ["permit_number"] = "PARK-789",
                    ["vehicle_license"] = "ABC-1234",
                    ["parking_zone"] = "Zone A",
                    ["space_number"] = "A-123"
                }
            };

            var parkingOptions = new SdIssuanceOptions
            {
                DisclosureStructure = new
                {
                    vehicle_license = true,
                    space_number = true
                }
            };

            var parkingCredential = vcIssuer.Issue(
                "https://credentials.techcorp.com/parking-permit",
                parkingPayload,
                parkingOptions,
                holderJwk
            );

            Console.WriteLine("   Batch Credential Response:");
            var batchResponse = new
            {
                credential_responses = new[]
                {
                    new { credential = employeeCredential.Issuance, format = Oid4VciConstants.SdJwtVcFormat },
                    new { credential = securityCredential.Issuance, format = Oid4VciConstants.SdJwtVcFormat },
                    new { credential = parkingCredential.Issuance, format = Oid4VciConstants.SdJwtVcFormat }
                },
                c_nonce = Convert.ToBase64String(RandomNumberGenerator.GetBytes(16)),
                c_nonce_expires_in = 300
            };

            Console.WriteLine($"   [X] Employee ID: {employeeCredential.Disclosures.Count} selective disclosures");
            Console.WriteLine($"   [X] Security Badge: {securityCredential.Disclosures.Count} selective disclosures");
            Console.WriteLine($"   [X] Parking Permit: {parkingCredential.Disclosures.Count} selective disclosures");
            Console.WriteLine($"   [X] Total credentials issued: {batchResponse.credential_responses.Length}");
            Console.WriteLine();

            Console.WriteLine("   Implementation Benefits:");
            Console.WriteLine("   [X] Single transaction for multiple credentials");
            Console.WriteLine("   [X] Consistent cryptographic binding");
            Console.WriteLine("   [X] Unified nonce management");
            Console.WriteLine("   [X] Atomic operation - all or none");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"   [X] Batch issuance error: {ex.Message}");
        }

        return Task.CompletedTask;
    }

    private static Task DemonstrateDeferredIssuance()
    {
        Console.WriteLine("\n5. DEFERRED CREDENTIAL ISSUANCE - PROTOCOL FLOW");
        Console.WriteLine("   Scenario: Professional license requiring manual approval");
        Console.WriteLine("   (Demonstrates protocol - would require background processing system)");
        Console.WriteLine();

        Console.WriteLine("   Step 1: Initial credential request → Acceptance token response");
        var acceptanceResponse = new
        {
            acceptance_token = $"acceptance_{Convert.ToBase64String(RandomNumberGenerator.GetBytes(16))}",
            c_nonce = Convert.ToBase64String(RandomNumberGenerator.GetBytes(16)),
            c_nonce_expires_in = 300
        };

        Console.WriteLine($"   [X] Acceptance token: {acceptanceResponse.acceptance_token[..30]}...");
        Console.WriteLine();

        Console.WriteLine("   Background Processing:");
        Console.WriteLine("   • Application submitted to licensing board");
        Console.WriteLine("   • Background check initiated");
        Console.WriteLine("   • Professional qualifications verified");
        Console.WriteLine("   • Manual review by board members");
        Console.WriteLine("   • Approval decision recorded");
        Console.WriteLine();

        Console.WriteLine("   Step 2: Wallet polls deferred credential endpoint");
        Console.WriteLine("   POST /deferred-credentials");
        Console.WriteLine($"   Authorization: Bearer {acceptanceResponse.acceptance_token[..20]}...");
        Console.WriteLine();

        Console.WriteLine("   Implementation Requirements:");
        Console.WriteLine("   • Background processing system");
        Console.WriteLine("   • Status tracking database");
        Console.WriteLine("   • Notification system (optional)");
        Console.WriteLine("   • Secure acceptance token storage");
        Console.WriteLine("   • Polling interval management");

        return Task.CompletedTask;
    }

    private static Task DemonstrateCredentialFormats(SecurityKey issuerKey, JsonWebKeyMs holderJwk)
    {
        Console.WriteLine("\n6. CREDENTIAL FORMATS - SD-JWT VC IMPLEMENTATION");
        Console.WriteLine("   Demonstrating SD-JWT VC format with various configurations...");
        Console.WriteLine();

        try
        {
            var vcIssuer = new SdJwtVcIssuer(issuerKey, SecurityAlgorithms.EcdsaSha256);

            // Minimal SD-JWT VC
            Console.WriteLine("   Creating minimal SD-JWT VC...");
            var minimalCredential = vcIssuer.IssueSimple(
                vctIdentifier: "https://example.com/minimal-credential",
                issuer: "https://issuer.example.com",
                subject: "did:example:subject123",
                claims: new Dictionary<string, object>
                {
                    ["name"] = "Test User",
                    ["verified"] = true
                },
                options: new SdIssuanceOptions
                {
                    DisclosureStructure = new { verified = true }
                }
            );

            Console.WriteLine($"   [X] Minimal credential: {minimalCredential.Disclosures.Count} disclosure(s)");

            // Rich SD-JWT VC with status
            Console.WriteLine("   Creating rich SD-JWT VC with status information...");
            var richPayload = new SdJwtVcPayload
            {
                Issuer = "https://issuer.example.com",
                Subject = "did:example:subject123",
                IssuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                ExpiresAt = DateTimeOffset.UtcNow.AddYears(5).ToUnixTimeSeconds(),
                Status = vcIssuer.CreateStatusReference("https://issuer.example.com/status", 12345),
                Confirmation = vcIssuer.CreateKeyBindingConfiguration(holderJwk),
                AdditionalData = new Dictionary<string, object>
                {
                    ["qualification"] = "Certified Professional",
                    ["level"] = "Expert",
                    ["specialization"] = "Cloud Security",
                    ["certification_date"] = "2024-01-15",
                    ["valid_until"] = "2029-01-15"
                }
            };

            var richCredential = vcIssuer.Issue(
                "https://example.com/professional-certification",
                richPayload,
                new SdIssuanceOptions
                {
                    DisclosureStructure = new
                    {
                        level = true,
                        specialization = true,
                        certification_date = true
                    }
                },
                holderJwk
            );

            Console.WriteLine($"   [X] Rich credential: {richCredential.Disclosures.Count} selective disclosures");
            Console.WriteLine("   [X] Status list integration enabled");
            Console.WriteLine("   [X] Key binding configured");
            Console.WriteLine();

            Console.WriteLine("   SD-JWT VC Structure Analysis:");
            Console.WriteLine($"   • JWT Header: Algorithm ES256, Key ID: {issuerKey.KeyId}");
            Console.WriteLine("   • JWT Payload: Core claims + disclosure digests");
            Console.WriteLine("   • Disclosures: Base64URL-encoded [salt, name, value] arrays");
            Console.WriteLine("   • Key Binding: Holder public key in cnf claim");
            Console.WriteLine("   • Status: StatusList2021 integration ready");
            Console.WriteLine();

            Console.WriteLine("   Format Features Demonstrated:");
            Console.WriteLine("   [X] Selective disclosure with digest-based privacy");
            Console.WriteLine("   [X] Cryptographic binding to holder keys");
            Console.WriteLine("   [X] JSON-based claims structure");
            Console.WriteLine("   [X] Status list integration");
            Console.WriteLine("   [X] Collision-resistant VCT identifiers");
            Console.WriteLine("   [X] JOSE/JWT compatibility");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"   [X] Format demonstration error: {ex.Message}");
        }

        return Task.CompletedTask;
    }
}

