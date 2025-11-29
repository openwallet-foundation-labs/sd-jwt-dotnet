using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace SdJwt.Net.Samples.Examples;

/// <summary>
/// Demonstrates OpenID4VP concepts for verifiable presentations
/// Shows the protocol concepts and verification flows
/// </summary>
public class OpenId4VpExample
{
    public static async Task RunExample(IServiceProvider services)
    {
        var logger = services.GetRequiredService<ILogger<OpenId4VpExample>>();
        
        Console.WriteLine("\n╔═════════════════════════════════════════════════════════╗");
        Console.WriteLine("║        OpenID4VP Presentation Verification Example     ║");
        Console.WriteLine("║                    (OID4VP 1.0 Final)                  ║");
        Console.WriteLine("╚═════════════════════════════════════════════════════════╝");

        Console.WriteLine("\nOpenID for Verifiable Presentations (OID4VP) enables");
        Console.WriteLine("standardized workflows for requesting and verifying credentials.");
        Console.WriteLine();

        await DemonstrateEmploymentVerification();
        await DemonstrateAgeVerification();
        await DemonstrateEducationVerification();
        await DemonstrateCrossDeviceFlow();
        await DemonstrateComplexRequirements();

        Console.WriteLine("\n╔═════════════════════════════════════════════════════════╗");
        Console.WriteLine("║           OpenID4VP concepts demonstrated!             ║");
        Console.WriteLine("║                                                         ║");
        Console.WriteLine("║  ✓ Employment verification                             ║");
        Console.WriteLine("║  ✓ Age verification                                    ║");
        Console.WriteLine("║  ✓ Education verification                              ║");
        Console.WriteLine("║  ✓ Cross-device flows                                  ║");
        Console.WriteLine("║  ✓ Complex presentation requirements                   ║");
        Console.WriteLine("╚═════════════════════════════════════════════════════════╝");
        return;
    }

    private static Task DemonstrateEmploymentVerification()
    {
        Console.WriteLine("\n1. EMPLOYMENT VERIFICATION SCENARIO");
        Console.WriteLine("   Bank needs to verify employment for loan application");
        Console.WriteLine();

        Console.WriteLine("   Step 1: Bank creates presentation request");
        Console.WriteLine("   {");
        Console.WriteLine("     \"client_id\": \"https://bank.example.com\",");
        Console.WriteLine("     \"response_type\": \"vp_token\",");
        Console.WriteLine("     \"response_mode\": \"direct_post\",");
        Console.WriteLine("     \"response_uri\": \"https://bank.example.com/presentations\",");
        Console.WriteLine("     \"nonce\": \"bank_loan_nonce_123\",");
        Console.WriteLine("     \"presentation_definition\": {");
        Console.WriteLine("       \"id\": \"employment_verification\",");
        Console.WriteLine("       \"name\": \"Employment Verification for Loan\",");
        Console.WriteLine("       \"purpose\": \"Verify employment status for loan application\",");
        Console.WriteLine("       \"input_descriptors\": [{");
        Console.WriteLine("         \"id\": \"employment_credential\",");
        Console.WriteLine("         \"constraints\": {");
        Console.WriteLine("           \"fields\": [");
        Console.WriteLine("             { \"path\": [\"$.position\"]},");
        Console.WriteLine("             { \"path\": [\"$.employment_type\"], \"filter\": { \"const\": \"Full-time\" } },");
        Console.WriteLine("             { \"path\": [\"$.start_date\"]}");
        Console.WriteLine("           ]");
        Console.WriteLine("         }");
        Console.WriteLine("       }]");
        Console.WriteLine("     }");
        Console.WriteLine("   }");
        Console.WriteLine();

        Console.WriteLine("   Step 2: Encoded as QR code or deep link");
        Console.WriteLine("   openid4vp://presentation_request?");
        Console.WriteLine("     client_id=https%3A//bank.example.com&");
        Console.WriteLine("     request_uri=https%3A//bank.example.com/requests/abc123");
        Console.WriteLine();

        Console.WriteLine("   Step 3: Wallet processes request and shows user");
        Console.WriteLine("   ✓ Bank wants to verify your employment");
        Console.WriteLine("   ✓ Required: Position, employment type, start date");
        Console.WriteLine("   ✓ Salary information will NOT be shared");
        Console.WriteLine("   ✓ [Accept] [Decline] buttons shown to user");
        Console.WriteLine();

        Console.WriteLine("   Step 4: User approves and wallet creates presentation");
        Console.WriteLine("   - Selects employment credential from wallet");
        Console.WriteLine("   - Creates selective disclosure (only required fields)");
        Console.WriteLine("   - Signs presentation with holder key binding");
        Console.WriteLine();

        Console.WriteLine("   Step 5: Presentation submitted to bank");
        Console.WriteLine("   POST https://bank.example.com/presentations");
        Console.WriteLine("   {");
        Console.WriteLine("     \"vp_token\": \"eyJ0eXAiOiJ2cCtzZC1qd3Q...\",");
        Console.WriteLine("     \"presentation_submission\": {");
        Console.WriteLine("       \"id\": \"submission_123\",");
        Console.WriteLine("       \"definition_id\": \"employment_verification\",");
        Console.WriteLine("       \"descriptor_map\": [{");
        Console.WriteLine("         \"id\": \"employment_credential\",");
        Console.WriteLine("         \"format\": \"vc+sd-jwt\",");
        Console.WriteLine("         \"path\": \"$\"");
        Console.WriteLine("       }]");
        Console.WriteLine("     }");
        Console.WriteLine("   }");
        Console.WriteLine();

        Console.WriteLine("   Results:");
        Console.WriteLine("   ✓ Employment verified: Senior Software Engineer");
        Console.WriteLine("   ✓ Employment type: Full-time (meets requirement)");
        Console.WriteLine("   ✓ Start date: 2023-08-01 (sufficient tenure)");
        Console.WriteLine("   ✓ Salary details protected (not disclosed)");
        return Task.CompletedTask;
    }

    private static Task DemonstrateAgeVerification()
    {
        Console.WriteLine("\n2. AGE VERIFICATION SCENARIO");
        Console.WriteLine("   Online service needs to verify user is over 21");
        Console.WriteLine();

        Console.WriteLine("   Presentation Request:");
        Console.WriteLine("   {");
        Console.WriteLine("     \"presentation_definition\": {");
        Console.WriteLine("       \"id\": \"age_verification_21\",");
        Console.WriteLine("       \"purpose\": \"Verify you are 21 years or older\",");
        Console.WriteLine("       \"input_descriptors\": [{");
        Console.WriteLine("         \"id\": \"government_id\",");
        Console.WriteLine("         \"constraints\": {");
        Console.WriteLine("           \"fields\": [{");
        Console.WriteLine("             \"path\": [\"$.age_over_21\"],");
        Console.WriteLine("             \"filter\": { \"type\": \"boolean\", \"const\": true }");
        Console.WriteLine("           }]");
        Console.WriteLine("         }");
        Console.WriteLine("       }]");
        Console.WriteLine("     }");
        Console.WriteLine("   }");
        Console.WriteLine();

        Console.WriteLine("   Privacy-Preserving Age Verification:");
        Console.WriteLine("   ✓ Driver's license contains 'age_over_21': true claim");
        Console.WriteLine("   ✓ No birth date revealed to verifier");
        Console.WriteLine("   ✓ No name or address disclosed");
        Console.WriteLine("   ✓ Minimal information for age proof");
        Console.WriteLine();

        Console.WriteLine("   Verification Result:");
        Console.WriteLine("   ✓ Age requirement: MET (user is over 21)");
        Console.WriteLine("   ✓ Government-issued credential: VERIFIED");
        Console.WriteLine("   ✓ Issuer trust: California DMV (trusted)");
        Console.WriteLine("   ✓ Access granted to age-restricted service");
        return Task.CompletedTask;
    }

    private static Task DemonstrateEducationVerification()
    {
        Console.WriteLine("\n3. EDUCATION VERIFICATION SCENARIO");
        Console.WriteLine("   Employer verifying degree for job application");
        Console.WriteLine();

        Console.WriteLine("   Employer Requirements:");
        Console.WriteLine("   - Bachelor's degree or higher");
        Console.WriteLine("   - Technical field preferred");
        Console.WriteLine("   - Recent graduation (within 10 years)");
        Console.WriteLine("   - GPA disclosure optional (candidate choice)");
        Console.WriteLine();

        Console.WriteLine("   Presentation Definition:");
        Console.WriteLine("   {");
        Console.WriteLine("     \"input_descriptors\": [{");
        Console.WriteLine("       \"id\": \"university_degree\",");
        Console.WriteLine("       \"constraints\": {");
        Console.WriteLine("         \"fields\": [");
        Console.WriteLine("           {");
        Console.WriteLine("             \"path\": [\"$.degree\"],");
        Console.WriteLine("             \"filter\": { \"pattern\": \".*(Bachelor|Master|Doctor).*\" }");
        Console.WriteLine("           },");
        Console.WriteLine("           {");
        Console.WriteLine("             \"path\": [\"$.major\"],");
        Console.WriteLine("             \"filter\": { \"pattern\": \".*(Computer Science|Engineering).*\" }");
        Console.WriteLine("           },");
        Console.WriteLine("           {");
        Console.WriteLine("             \"path\": [\"$.graduation_date\"]");
        Console.WriteLine("           },");
        Console.WriteLine("           {");
        Console.WriteLine("             \"path\": [\"$.gpa\"],");
        Console.WriteLine("             \"optional\": true");
        Console.WriteLine("           }");
        Console.WriteLine("         ]");
        Console.WriteLine("       }");
        Console.WriteLine("     }]");
        Console.WriteLine("   }");
        Console.WriteLine();

        Console.WriteLine("   Candidate's Disclosure Choice:");
        Console.WriteLine("   ✓ Degree: Master of Science (disclosed)");
        Console.WriteLine("   ✓ Major: Computer Science (disclosed)");
        Console.WriteLine("   ✓ Graduation: 2023-06-15 (disclosed)");
        Console.WriteLine("   ✗ GPA: 3.8 (NOT disclosed - candidate choice)");
        Console.WriteLine("   ✗ Thesis title: (NOT disclosed - not requested)");
        Console.WriteLine();

        Console.WriteLine("   Verification Results:");
        Console.WriteLine("   ✓ Education requirement: SATISFIED");
        Console.WriteLine("   ✓ Technical field: Computer Science (preferred)");
        Console.WriteLine("   ✓ Recent graduate: 2023 (excellent)");
        Console.WriteLine("   ✓ Candidate maintains GPA privacy");
        return Task.CompletedTask;
    }

    private static Task DemonstrateCrossDeviceFlow()
    {
        Console.WriteLine("\n4. CROSS-DEVICE FLOW");
        Console.WriteLine("   User scans QR code to present credential from mobile to desktop");
        Console.WriteLine();

        Console.WriteLine("   Scenario: Airport security check kiosk");
        Console.WriteLine();

        Console.WriteLine("   Step 1: Kiosk generates presentation request QR code");
        Console.WriteLine("   ┌─────────────────────────────────┐");
        Console.WriteLine("   │ ███ ███ ███    ███   ███ ███ ███ │");
        Console.WriteLine("   │ ███ ███ ███ ██ ███ ██ ███ ███ ███ │");
        Console.WriteLine("   │ ███ ███ ███ ██ ███ ██ ███ ███ ███ │");
        Console.WriteLine("   │    ███    ██   ██   ██    ███     │");
        Console.WriteLine("   │ ███ ███ ███ ██ ███ ██ ███ ███ ███ │");
        Console.WriteLine("   │ ███ ███ ███    ███   ███ ███ ███ │");
        Console.WriteLine("   │ ███ ███ ███ ██ ███ ██ ███ ███ ███ │");
        Console.WriteLine("   └─────────────────────────────────┘");
        Console.WriteLine("   \"Scan to verify ID for security screening\"");
        Console.WriteLine();

        Console.WriteLine("   QR Code Contains:");
        Console.WriteLine("   openid4vp://authorize?");
        Console.WriteLine("     client_id=https%3A//kiosk.airport.example.com&");
        Console.WriteLine("     request_uri=https%3A//kiosk.airport.example.com/requests/security123");
        Console.WriteLine();

        Console.WriteLine("   Step 2: Mobile wallet scans QR code");
        Console.WriteLine("   ✓ Wallet app detects OpenID4VP request");
        Console.WriteLine("   ✓ Fetches presentation definition from kiosk");
        Console.WriteLine("   ✓ Shows user: \"Airport security verification\"");
        Console.WriteLine("   ✓ Required: Name and ID number (for security check)");
        Console.WriteLine();

        Console.WriteLine("   Step 3: User approves on mobile device");
        Console.WriteLine("   📱 [Airport Security Verification]");
        Console.WriteLine("      Required information:");
        Console.WriteLine("      • Full name");
        Console.WriteLine("      • ID document number");
        Console.WriteLine("      ");
        Console.WriteLine("      Privacy protected:");
        Console.WriteLine("      • Birth date, address not shared");
        Console.WriteLine("      ");
        Console.WriteLine("      [ Approve ] [ Decline ]");
        Console.WriteLine();

        Console.WriteLine("   Step 4: Mobile submits to kiosk endpoint");
        Console.WriteLine("   - Wallet creates selective presentation");
        Console.WriteLine("   - Signs with holder key binding");
        Console.WriteLine("   - POSTs to kiosk response endpoint");
        Console.WriteLine();

        Console.WriteLine("   Step 5: Desktop kiosk receives verification");
        Console.WriteLine("   🖥️  [Security Check Complete]");
        Console.WriteLine("      ✓ Government ID verified");
        Console.WriteLine("      ✓ Name: Alice Johnson");
        Console.WriteLine("      ✓ ID: CA1234567890");
        Console.WriteLine("      ✓ Proceed to gate B7");
        Console.WriteLine();

        Console.WriteLine("   Cross-Device Benefits:");
        Console.WriteLine("   ✓ Seamless mobile-to-desktop workflow");
        Console.WriteLine("   ✓ No app installation on public terminals");
        Console.WriteLine("   ✓ Secure credential handling on personal device");
        Console.WriteLine("   ✓ QR code simplicity for any environment");
        return Task.CompletedTask;
    }

    private static Task DemonstrateComplexRequirements()
    {
        Console.WriteLine("\n5. COMPLEX REQUIREMENTS SCENARIO");
        Console.WriteLine("   Government contractor requiring multiple credentials");
        Console.WriteLine();

        Console.WriteLine("   Security Clearance Application Requirements:");
        Console.WriteLine("   ┌─────────────────┐ AND ┌─────────────────┐ AND ┌─────────────────┐");
        Console.WriteLine("   │   IDENTITY      │     │   EDUCATION     │     │   EMPLOYMENT    │");
        Console.WriteLine("   │   (Pick 1)      │     │   (Pick 1)      │     │   (Pick 1)      │");
        Console.WriteLine("   │                 │     │                 │     │                 │");
        Console.WriteLine("   │ • Driver's      │     │ • University    │     │ • Current job   │");
        Console.WriteLine("   │   License       │     │   Degree        │     │   w/ clearance  │");
        Console.WriteLine("   │ • Passport      │     │ • Professional  │     │ • Military      │");
        Console.WriteLine("   │ • National ID   │     │   License       │     │   Service       │");
        Console.WriteLine("   └─────────────────┘     └─────────────────┘     └─────────────────┘");
        Console.WriteLine();

        Console.WriteLine("   Submission Requirements Structure:");
        Console.WriteLine("   {");
        Console.WriteLine("     \"submission_requirements\": [");
        Console.WriteLine("       {");
        Console.WriteLine("         \"name\": \"Identity Verification\",");
        Console.WriteLine("         \"rule\": \"pick\",");
        Console.WriteLine("         \"count\": 1,");
        Console.WriteLine("         \"from\": \"Group_A\"");
        Console.WriteLine("       },");
        Console.WriteLine("       {");
        Console.WriteLine("         \"name\": \"Professional Qualifications\",");
        Console.WriteLine("         \"rule\": \"all\",");
        Console.WriteLine("         \"from_nested\": [");
        Console.WriteLine("           { \"rule\": \"pick\", \"count\": 1, \"from\": \"Group_B\" },");
        Console.WriteLine("           { \"rule\": \"pick\", \"count\": 1, \"from\": \"Group_C\" }");
        Console.WriteLine("         ]");
        Console.WriteLine("       }");
        Console.WriteLine("     ]");
        Console.WriteLine("   }");
        Console.WriteLine();

        Console.WriteLine("   Wallet Selection Process:");
        Console.WriteLine("   1. Analyze requirements: Identity + Education + Employment");
        Console.WriteLine("   2. Check available credentials in wallet");
        Console.WriteLine("   3. Present options to user:");
        Console.WriteLine("      ✓ Driver's License (preferred for identity)");
        Console.WriteLine("      ✓ University Degree (satisfies education)");
        Console.WriteLine("      ✓ Employment Credential w/ Security Clearance");
        Console.WriteLine("   4. User approves selected combination");
        Console.WriteLine("   5. Create presentation with all three credentials");
        Console.WriteLine();

        Console.WriteLine("   Multi-Credential Presentation:");
        Console.WriteLine("   {");
        Console.WriteLine("     \"vp_token\": [");
        Console.WriteLine("       \"eyJ0eXAiOiJ2YytzZC1qd3Q...\"  // Driver's License");
        Console.WriteLine("       \"eyJ0eXAiOiJ2YytzZC1qd3Q...\"  // University Degree");
        Console.WriteLine("       \"eyJ0eXAiOiJ2YytzZC1qd3Q...\"  // Employment Credential");
        Console.WriteLine("     ],");
        Console.WriteLine("     \"presentation_submission\": {");
        Console.WriteLine("       \"descriptor_map\": [");
        Console.WriteLine("         { \"id\": \"government_id\", \"format\": \"vc+sd-jwt\", \"path\": \"$[0]\" },");
        Console.WriteLine("         { \"id\": \"degree\", \"format\": \"vc+sd-jwt\", \"path\": \"$[1]\" },");
        Console.WriteLine("         { \"id\": \"employment\", \"format\": \"vc+sd-jwt\", \"path\": \"$[2]\" }");
        Console.WriteLine("       ]");
        Console.WriteLine("     }");
        Console.WriteLine("   }");
        Console.WriteLine();

        Console.WriteLine("   Verification Results:");
        Console.WriteLine("   ✓ Identity verified: Alice Johnson (CA DL)");
        Console.WriteLine("   ✓ Education verified: MS Computer Science, Stanford");
        Console.WriteLine("   ✓ Employment verified: Senior Engineer w/ Secret clearance");
        Console.WriteLine("   ✓ All requirements satisfied");
        Console.WriteLine("   ✓ Security clearance application: APPROVED for review");
        Console.WriteLine();

        Console.WriteLine("OPENID4VP PROTOCOL BENEFITS:");
        Console.WriteLine("✓ Standardized presentation request format");
        Console.WriteLine("✓ Rich requirement expression (Presentation Exchange)");
        Console.WriteLine("✓ Cross-device flow support");
        Console.WriteLine("✓ Selective disclosure optimization");
        Console.WriteLine("✓ Multiple credential combinations");
        Console.WriteLine("✓ QR code and deep link integration");
        Console.WriteLine("✓ Direct post for result delivery");
        Console.WriteLine();

        Console.WriteLine("IMPLEMENTATION CHECKLIST:");
        Console.WriteLine("1. Support presentation definition parsing");
        Console.WriteLine("2. Implement credential selection engine");
        Console.WriteLine("3. Handle cross-device flows with QR codes");
        Console.WriteLine("4. Support multiple VP token formats");
        Console.WriteLine("5. Validate presentation submissions");
        Console.WriteLine("6. Integrate with selective disclosure");
        Console.WriteLine();

        Console.WriteLine("Note: This demonstrates OpenID4VP concepts and flows.");
        Console.WriteLine("For production implementation, use the SdJwt.Net.Oid4Vp package");
        Console.WriteLine("with proper protocol handling and Presentation Exchange integration.");
        return Task.CompletedTask;
    }
}

