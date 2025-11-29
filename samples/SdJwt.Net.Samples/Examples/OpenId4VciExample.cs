using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace SdJwt.Net.Samples.Examples;

/// <summary>
/// Demonstrates OpenID4VCI concepts for credential issuance workflows
/// Shows the protocol concepts and message flows
/// </summary>
public class OpenId4VciExample
{
    public static async Task RunExample(IServiceProvider services)
    {
        var logger = services.GetRequiredService<ILogger<OpenId4VciExample>>();
        
        Console.WriteLine("\n╔═════════════════════════════════════════════════════════╗");
        Console.WriteLine("║            OpenID4VCI Credential Issuance Example      ║");
        Console.WriteLine("║                     (OID4VCI 1.0 Final)                ║");
        Console.WriteLine("╚═════════════════════════════════════════════════════════╝");

        Console.WriteLine("\nOpenID for Verifiable Credential Issuance (OID4VCI) enables");
        Console.WriteLine("standardized workflows for issuing verifiable credentials to wallets.");
        Console.WriteLine();

        await DemonstratePreAuthorizedFlow();
        await DemonstrateAuthorizationCodeFlow();
        await DemonstrateBatchIssuance();
        await DemonstrateDeferredIssuance();
        await DemonstrateCredentialFormats();

        Console.WriteLine("\n╔═════════════════════════════════════════════════════════╗");
        Console.WriteLine("║          OpenID4VCI concepts demonstrated!             ║");
        Console.WriteLine("║                                                         ║");
        Console.WriteLine("║  ✓ Pre-authorized code flow                            ║");
        Console.WriteLine("║  ✓ Authorization code flow                             ║");
        Console.WriteLine("║  ✓ Batch credential issuance                           ║");
        Console.WriteLine("║  ✓ Deferred credential issuance                        ║");
        Console.WriteLine("║  ✓ Multiple credential formats                         ║");
        Console.WriteLine("╚═════════════════════════════════════════════════════════╝");
        return;
    }

    private static Task DemonstratePreAuthorizedFlow()
    {
        Console.WriteLine("\n1. PRE-AUTHORIZED CODE FLOW");
        Console.WriteLine("   Scenario: University pre-approves degree issuance for graduate");
        Console.WriteLine();

        Console.WriteLine("   Step 1: University creates credential offer with pre-authorized code");
        Console.WriteLine("   {");
        Console.WriteLine("     \"credential_issuer\": \"https://university.example.edu\",");
        Console.WriteLine("     \"credential_configuration_ids\": [\"UniversityDegree_SDJWT\"],");
        Console.WriteLine("     \"grants\": {");
        Console.WriteLine("       \"urn:ietf:params:oauth:grant-type:pre-authorized_code\": {");
        Console.WriteLine("         \"pre-authorized_code\": \"eyJhbGciOiJFUzI1NiJ9...\",");
        Console.WriteLine("         \"user_pin_required\": true,");
        Console.WriteLine("         \"interval\": 5,");
        Console.WriteLine("         \"expires_in\": 86400");
        Console.WriteLine("       }");
        Console.WriteLine("     }");
        Console.WriteLine("   }");
        Console.WriteLine();

        Console.WriteLine("   Step 2: Offer converted to URI for QR code or deep link");
        Console.WriteLine("   openid-credential-offer://?credential_offer_uri=https://university.example.edu/offers/abc123");
        Console.WriteLine();

        Console.WriteLine("   Step 3: Wallet scans QR code and parses offer");
        Console.WriteLine("   ✓ Wallet discovers: University credential offer");
        Console.WriteLine("   ✓ Pre-authorized: No additional OAuth flow needed");
        Console.WriteLine("   ✓ PIN required: Student enters graduation PIN");
        Console.WriteLine();

        Console.WriteLine("   Step 4: Wallet creates proof of possession");
        Console.WriteLine("   - Wallet generates key pair for credential binding");
        Console.WriteLine("   - Creates JWT proof signed with private key");
        Console.WriteLine("   - Includes c_nonce from credential issuer");
        Console.WriteLine();

        Console.WriteLine("   Step 5: Credential request to issuer");
        Console.WriteLine("   POST /credentials HTTP/1.1");
        Console.WriteLine("   Content-Type: application/json");
        Console.WriteLine("   Authorization: Bearer <access_token>");
        Console.WriteLine();
        Console.WriteLine("   {");
        Console.WriteLine("     \"format\": \"vc+sd-jwt\",");
        Console.WriteLine("     \"credential_configuration_id\": \"UniversityDegree_SDJWT\",");
        Console.WriteLine("     \"proof\": {");
        Console.WriteLine("       \"proof_type\": \"jwt\",");
        Console.WriteLine("       \"jwt\": \"eyJ0eXAiOiJvcGVuaWQ4dmNpLXByb29mK2p3dC...\"");
        Console.WriteLine("     }");
        Console.WriteLine("   }");
        Console.WriteLine();

        Console.WriteLine("   Step 6: Issuer validates and returns credential");
        Console.WriteLine("   ✓ Validates pre-authorized code");
        Console.WriteLine("   ✓ Verifies proof of possession");
        Console.WriteLine("   ✓ Issues SD-JWT VC with selective disclosure");
        Console.WriteLine("   ✓ Binds credential to wallet's public key");
        return Task.CompletedTask;
    }

    private static Task DemonstrateAuthorizationCodeFlow()
    {
        Console.WriteLine("\n2. AUTHORIZATION CODE FLOW");
        Console.WriteLine("   Scenario: User-initiated credential request with authorization");
        Console.WriteLine();

        Console.WriteLine("   Step 1: Credential offer for authorization flow");
        Console.WriteLine("   {");
        Console.WriteLine("     \"credential_issuer\": \"https://government.example.gov\",");
        Console.WriteLine("     \"credential_configuration_ids\": [\"DigitalID_SDJWT\"],");
        Console.WriteLine("     \"grants\": {");
        Console.WriteLine("       \"authorization_code\": {");
        Console.WriteLine("         \"issuer_state\": \"id_application_state_789\",");
        Console.WriteLine("         \"authorization_server\": \"https://auth.government.example.gov\"");
        Console.WriteLine("       }");
        Console.WriteLine("     }");
        Console.WriteLine("   }");
        Console.WriteLine();

        Console.WriteLine("   Step 2: Wallet initiates OAuth 2.0 authorization");
        Console.WriteLine("   https://auth.government.example.gov/oauth2/authorize");
        Console.WriteLine("     ?response_type=code");
        Console.WriteLine("     &client_id=wallet_app");
        Console.WriteLine("     &scope=credential_issuance");
        Console.WriteLine("     &state=id_application_state_789");
        Console.WriteLine();

        Console.WriteLine("   Step 3: User completes authentication and authorization");
        Console.WriteLine("   ✓ Government identity verification (biometrics, etc.)");
        Console.WriteLine("   ✓ User consents to credential issuance");
        Console.WriteLine("   ✓ Authorization server issues authorization code");
        Console.WriteLine();

        Console.WriteLine("   Step 4: Wallet exchanges code for access token");
        Console.WriteLine("   POST /oauth2/token");
        Console.WriteLine("   {");
        Console.WriteLine("     \"grant_type\": \"authorization_code\",");
        Console.WriteLine("     \"code\": \"auth_code_xyz_789\",");
        Console.WriteLine("     \"client_id\": \"wallet_app\"");
        Console.WriteLine("   }");
        Console.WriteLine();

        Console.WriteLine("   Step 5: Access token used for credential issuance");
        Console.WriteLine("   ✓ Same credential request flow as pre-authorized");
        Console.WriteLine("   ✓ Access token provides authorization context");
        Console.WriteLine("   ✓ Government issues digital ID credential");
        return Task.CompletedTask;
    }

    private static Task DemonstrateBatchIssuance()
    {
        Console.WriteLine("\n3. BATCH CREDENTIAL ISSUANCE");
        Console.WriteLine("   Scenario: Multiple credentials issued in single transaction");
        Console.WriteLine();

        Console.WriteLine("   Use Case: Corporate onboarding package");
        Console.WriteLine("   - Employee ID credential");
        Console.WriteLine("   - Security badge credential");
        Console.WriteLine("   - Parking permit credential");
        Console.WriteLine("   - Health insurance credential");
        Console.WriteLine();

        Console.WriteLine("   Batch Credential Request:");
        Console.WriteLine("   POST /batch-credentials HTTP/1.1");
        Console.WriteLine("   {");
        Console.WriteLine("     \"credential_requests\": [");
        Console.WriteLine("       {");
        Console.WriteLine("         \"format\": \"vc+sd-jwt\",");
        Console.WriteLine("         \"credential_configuration_id\": \"EmployeeID_SDJWT\",");
        Console.WriteLine("         \"proof\": { /* proof of possession */ }");
        Console.WriteLine("       },");
        Console.WriteLine("       {");
        Console.WriteLine("         \"format\": \"vc+sd-jwt\",");
        Console.WriteLine("         \"credential_configuration_id\": \"SecurityBadge_SDJWT\",");
        Console.WriteLine("         \"proof\": { /* proof of possession */ }");
        Console.WriteLine("       }");
        Console.WriteLine("     ]");
        Console.WriteLine("   }");
        Console.WriteLine();

        Console.WriteLine("   Batch Response:");
        Console.WriteLine("   {");
        Console.WriteLine("     \"credential_responses\": [");
        Console.WriteLine("       { \"credential\": \"eyJ0eXAiOiJ2YytzZC1qd3Q...\" },");
        Console.WriteLine("       { \"credential\": \"eyJ0eXAiOiJ2YytzZC1qd3Q...\" }");
        Console.WriteLine("     ],");
        Console.WriteLine("     \"c_nonce\": \"new_nonce_for_next_request\",");
        Console.WriteLine("     \"c_nonce_expires_in\": 300");
        Console.WriteLine("   }");
        Console.WriteLine();

        Console.WriteLine("   Benefits:");
        Console.WriteLine("   ✓ Reduces round trips for multiple credentials");
        Console.WriteLine("   ✓ Atomic transaction - all or none");
        Console.WriteLine("   ✓ Consistent nonce management");
        Console.WriteLine("   ✓ Efficient for corporate/institutional use cases");
        return Task.CompletedTask;
    }

    private static Task DemonstrateDeferredIssuance()
    {
        Console.WriteLine("\n4. DEFERRED CREDENTIAL ISSUANCE");
        Console.WriteLine("   Scenario: Credential requires manual approval before issuance");
        Console.WriteLine();

        Console.WriteLine("   Use Case: Professional license requiring board review");
        Console.WriteLine();

        Console.WriteLine("   Step 1: Initial credential request");
        Console.WriteLine("   POST /credentials");
        Console.WriteLine("   {");
        Console.WriteLine("     \"format\": \"vc+sd-jwt\",");
        Console.WriteLine("     \"credential_configuration_id\": \"MedicalLicense_SDJWT\",");
        Console.WriteLine("     \"proof\": { /* proof of possession */ }");
        Console.WriteLine("   }");
        Console.WriteLine();

        Console.WriteLine("   Step 2: Issuer returns acceptance token (not credential)");
        Console.WriteLine("   {");
        Console.WriteLine("     \"acceptance_token\": \"urn:ietf:params:oauth:token-type:access_token:acceptance_token_123\",");
        Console.WriteLine("     \"c_nonce\": \"deferred_nonce_456\",");
        Console.WriteLine("     \"c_nonce_expires_in\": 300");
        Console.WriteLine("   }");
        Console.WriteLine();

        Console.WriteLine("   Step 3: Manual approval process");
        Console.WriteLine("   ✓ Medical board reviews application");
        Console.WriteLine("   ✓ Background check completed");
        Console.WriteLine("   ✓ Professional qualifications verified");
        Console.WriteLine("   ✓ License approved and ready for issuance");
        Console.WriteLine();

        Console.WriteLine("   Step 4: Wallet polls for credential using acceptance token");
        Console.WriteLine("   POST /deferred-credentials");
        Console.WriteLine("   {");
        Console.WriteLine("     \"acceptance_token\": \"acceptance_token_123\"");
        Console.WriteLine("   }");
        Console.WriteLine();

        Console.WriteLine("   Step 5: Approved credential returned");
        Console.WriteLine("   {");
        Console.WriteLine("     \"credential\": \"eyJ0eXAiOiJ2YytzZC1qd3Q...\"},");
        Console.WriteLine("     \"c_nonce\": \"final_nonce_789\",");
        Console.WriteLine("     \"c_nonce_expires_in\": 300");
        Console.WriteLine("   }");
        Console.WriteLine();

        Console.WriteLine("   Benefits:");
        Console.WriteLine("   ✓ Supports manual approval workflows");
        Console.WriteLine("   ✓ Asynchronous credential delivery");
        Console.WriteLine("   ✓ Maintains security through acceptance tokens");
        Console.WriteLine("   ✓ Essential for regulated credential types");
        return Task.CompletedTask;
    }

    private static Task DemonstrateCredentialFormats()
    {
        Console.WriteLine("\n5. CREDENTIAL FORMATS");
        Console.WriteLine("   OID4VCI supports multiple credential formats:");
        Console.WriteLine();

        Console.WriteLine("   SD-JWT VC Format (Recommended):");
        Console.WriteLine("   {");
        Console.WriteLine("     \"format\": \"vc+sd-jwt\",");
        Console.WriteLine("     \"vct\": \"https://credentials.example.com/identity\",");
        Console.WriteLine("     \"cryptographic_binding_methods_supported\": [\"jwk\", \"did:key\"]," );
        Console.WriteLine("     \"cryptographic_suites_supported\": [\"ES256\", \"ES384\"],");
        Console.WriteLine("     \"proof_types_supported\": {");
        Console.WriteLine("       \"jwt\": {");
        Console.WriteLine("         \"proof_signing_alg_values_supported\": [\"ES256\", \"ES384\"]");
        Console.WriteLine("       }");
        Console.WriteLine("     }");
        Console.WriteLine("   }");
        Console.WriteLine();

        Console.WriteLine("   Features:");
        Console.WriteLine("   ✓ Selective disclosure capabilities");
        Console.WriteLine("   ✓ Cryptographic binding to holder keys");
        Console.WriteLine("   ✓ JSON-based credential format");
        Console.WriteLine("   ✓ Standardized verification process");
        Console.WriteLine();

        Console.WriteLine("   Credential Configuration Example:");
        Console.WriteLine("   {");
        Console.WriteLine("     \"credential_configurations_supported\": {");
        Console.WriteLine("       \"UniversityDegree_SDJWT\": {");
        Console.WriteLine("         \"format\": \"vc+sd-jwt\",");
        Console.WriteLine("         \"vct\": \"https://credentials.university.example.edu/degree\",");
        Console.WriteLine("         \"display\": [{");
        Console.WriteLine("           \"name\": \"University Degree\",");
        Console.WriteLine("           \"locale\": \"en-US\",");
        Console.WriteLine("           \"background_color\": \"#003366\",");
        Console.WriteLine("           \"text_color\": \"#FFFFFF\"");
        Console.WriteLine("         }],");
        Console.WriteLine("         \"claims\": {");
        Console.WriteLine("           \"given_name\": {");
        Console.WriteLine("             \"mandatory\": true,");
        Console.WriteLine("             \"display\": [{ \"name\": \"First Name\", \"locale\": \"en-US\" }]");
        Console.WriteLine("           },");
        Console.WriteLine("           \"degree\": {");
        Console.WriteLine("             \"mandatory\": true,");
        Console.WriteLine("             \"display\": [{ \"name\": \"Degree Type\", \"locale\": \"en-US\" }]");
        Console.WriteLine("           },");
        Console.WriteLine("           \"gpa\": {");
        Console.WriteLine("             \"mandatory\": false,");
        Console.WriteLine("             \"display\": [{ \"name\": \"GPA\", \"locale\": \"en-US\" }]");
        Console.WriteLine("           }");
        Console.WriteLine("         }");
        Console.WriteLine("       }");
        Console.WriteLine("     }");
        Console.WriteLine("   }");
        Console.WriteLine();

        Console.WriteLine("OID4VCI PROTOCOL BENEFITS:");
        Console.WriteLine("✓ Standardized credential issuance across wallets and issuers");
        Console.WriteLine("✓ Support for multiple flows (pre-authorized, OAuth 2.0)");
        Console.WriteLine("✓ Batch issuance for efficiency");
        Console.WriteLine("✓ Deferred issuance for approval workflows");
        Console.WriteLine("✓ Rich metadata for wallet display");
        Console.WriteLine("✓ Cryptographic binding for security");
        Console.WriteLine("✓ QR code and deep link integration");
        Console.WriteLine();

        Console.WriteLine("IMPLEMENTATION WORKFLOW:");
        Console.WriteLine("1. Configure credential issuer metadata");
        Console.WriteLine("2. Implement credential configuration endpoints");
        Console.WriteLine("3. Handle credential offers (QR codes/deep links)");
        Console.WriteLine("4. Support proof of possession validation");
        Console.WriteLine("5. Issue SD-JWT VCs with proper binding");
        Console.WriteLine("6. Provide batch and deferred issuance as needed");
        Console.WriteLine();

        Console.WriteLine("Note: This demonstrates OID4VCI protocol concepts and flows.");
        Console.WriteLine("For production implementation, use the SdJwt.Net.Oid4Vci package");
        Console.WriteLine("with proper protocol message handling and security validation.");
        return Task.CompletedTask;
    }
}

