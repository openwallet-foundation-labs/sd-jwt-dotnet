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

namespace SdJwt.Net.Samples.UseCases.Telecom;

/// <summary>
/// eSIM Transfer Between Carriers
///
/// SCENARIO: Customer transfers eSIM profile to new carrier
/// - New carrier verifies identity and number ownership
/// - Porting authorization without exposing account details
/// - Device identity bound to legitimate owner
///
/// CLAIMS REVEALED: phone_number, carrier_name, account_status, port_eligible
/// CLAIMS HIDDEN: account_number, ssn, billing_address, usage_history
/// </summary>
public static class EsimTransfer
{
    public static async Task Run()
    {
        ConsoleHelpers.PrintHeader("Use Case: eSIM Transfer Between Carriers");

        Console.WriteLine("SCENARIO: Frank switches from Verizon to T-Mobile.");
        Console.WriteLine("He transfers his phone number using verifiable credentials");
        Console.WriteLine("without exposing his full Verizon account details.");
        Console.WriteLine();

        // =========================================================================
        // SETUP: Carrier Infrastructure
        // =========================================================================
        using var verizonEcdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        using var holderEcdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);

        var verizonKey = new ECDsaSecurityKey(verizonEcdsa) { KeyId = "verizon-esim-2024" };
        var holderPrivateKey = new ECDsaSecurityKey(holderEcdsa) { KeyId = "frank-device" };
        var holderJwk = JsonWebKeyConverter.ConvertFromSecurityKey(holderPrivateKey);

        // =========================================================================
        // PHASE 1: Verizon Issues Device Identity Credential
        // =========================================================================
        Console.WriteLine("PHASE 1: Current Carrier Issues Identity Credential");
        Console.WriteLine(new string('-', 50));

        var issuer = new SdJwtVcIssuer(verizonKey, SecurityAlgorithms.EcdsaSha256);

        var credentialPayload = new SdJwtVcPayload
        {
            Issuer = "https://api.verizon.example/identity",
            Subject = "did:device:EID-89001234567890123456",
            IssuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(30).ToUnixTimeSeconds(),
            AdditionalData = new Dictionary<string, object>
            {
                // Device identity (always visible)
                ["eid"] = "89001234567890123456",
                ["device_type"] = "Smartphone",
                ["device_manufacturer"] = "Apple",
                ["device_model"] = "iPhone 15 Pro",
                ["imei"] = "353456789012345",

                // Line/number information (visible for porting)
                ["phone_number"] = "+1-415-555-0199",
                ["carrier_name"] = "Verizon Wireless",
                ["line_type"] = "Postpaid",
                ["account_status"] = "Active - Good Standing",
                ["port_eligible"] = true,
                ["number_active_since"] = "2019-06-15",

                // Porting authorization (visible)
                ["port_authorization"] = new Dictionary<string, object>
                {
                    ["authorized"] = true,
                    ["authorization_date"] = DateTimeOffset.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                    ["valid_until"] = DateTimeOffset.UtcNow.AddDays(30).ToString("yyyy-MM-ddTHH:mm:ssZ"),
                    ["authorization_code"] = "PORT-2024-ALPHA-9876"
                },

                // Account holder identity (visible)
                ["account_holder_name"] = "Frank Chen",

                // Account details (selectively disclosable)
                ["account_number"] = "VZW-9876543210",
                ["account_pin"] = "1234",
                ["social_security_last_four"] = "5678",

                // Billing information (selectively disclosable)
                ["billing_address"] = new Dictionary<string, object>
                {
                    ["street"] = "321 Maple Drive",
                    ["city"] = "San Jose",
                    ["state"] = "CA",
                    ["zip"] = "95112"
                },
                ["monthly_charge"] = 95.00,
                ["autopay_enabled"] = true,

                // Usage data (selectively disclosable)
                ["usage_summary"] = new Dictionary<string, object>
                {
                    ["data_used_gb"] = 45.2,
                    ["minutes_used"] = 320,
                    ["texts_sent"] = 1250,
                    ["international_calls"] = 5
                },

                // Contract status (selectively disclosable)
                ["contract_details"] = new Dictionary<string, object>
                {
                    ["contract_type"] = "No Contract",
                    ["device_payment_remaining"] = 0.00,
                    ["early_termination_fee"] = 0.00
                }
            }
        };

        var sdOptions = new SdIssuanceOptions
        {
            DisclosureStructure = new
            {
                account_number = true,
                account_pin = true,
                social_security_last_four = true,
                billing_address = true,
                monthly_charge = true,
                autopay_enabled = true,
                usage_summary = true,
                contract_details = true
            }
        };

        var credential = issuer.Issue(
            "https://credentials.verizon.example/MobileIdentity",
            credentialPayload, sdOptions, holderJwk);

        Console.WriteLine("Verizon issued mobile identity credential:");
        Console.WriteLine($"  Phone: {credentialPayload.AdditionalData["phone_number"]}");
        Console.WriteLine($"  Device: {credentialPayload.AdditionalData["device_model"]}");
        Console.WriteLine($"  Account Status: {credentialPayload.AdditionalData["account_status"]}");
        Console.WriteLine($"  Port Eligible: {credentialPayload.AdditionalData["port_eligible"]}");
        Console.WriteLine();

        // =========================================================================
        // PHASE 2: Frank Initiates Port to T-Mobile
        // =========================================================================
        Console.WriteLine("PHASE 2: Frank Starts Port-In at T-Mobile");
        Console.WriteLine(new string('-', 50));

        Console.WriteLine("T-Mobile needs to verify:");
        Console.WriteLine("  - Frank owns this phone number");
        Console.WriteLine("  - Number is eligible for porting");
        Console.WriteLine("  - Current carrier authorized the port");
        Console.WriteLine("  - Device is eSIM capable");
        Console.WriteLine();
        Console.WriteLine("T-Mobile does NOT need:");
        Console.WriteLine("  - Frank's Verizon account number");
        Console.WriteLine("  - His account PIN");
        Console.WriteLine("  - SSN digits");
        Console.WriteLine("  - Billing address or usage history");
        Console.WriteLine();

        // =========================================================================
        // PHASE 3: Frank Creates Porting Presentation
        // =========================================================================
        Console.WriteLine("PHASE 3: Frank Creates Porting Presentation");
        Console.WriteLine(new string('-', 50));

        var holder = new SdJwtHolder(credential.Issuance);

        var nonce = $"port_{Guid.NewGuid():N}"[..24];
        var audience = "https://portin.t-mobile.example";

        // Don't disclose account details
        var presentation = holder.CreatePresentation(
            disclosure => false,
            kbJwtPayload: new JwtPayload
            {
                [JwtRegisteredClaimNames.Aud] = audience,
                [JwtRegisteredClaimNames.Iat] = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                ["nonce"] = nonce
            },
            kbJwtSigningKey: holderPrivateKey,
            kbJwtSigningAlgorithm: SecurityAlgorithms.EcdsaSha256
        );

        Console.WriteLine("Frank's porting presentation shows:");
        Console.WriteLine("  [REVEALED] phone_number: +1-415-555-0199");
        Console.WriteLine("  [REVEALED] carrier_name: Verizon Wireless");
        Console.WriteLine("  [REVEALED] account_status: Active - Good Standing");
        Console.WriteLine("  [REVEALED] port_eligible: true");
        Console.WriteLine("  [REVEALED] port_authorization.authorized: true");
        Console.WriteLine("  [REVEALED] eid: 89001234567890123456");
        Console.WriteLine("  [REVEALED] account_holder_name: Frank Chen");
        Console.WriteLine();
        Console.WriteLine("  [HIDDEN] account_number: ***");
        Console.WriteLine("  [HIDDEN] account_pin: ***");
        Console.WriteLine("  [HIDDEN] social_security_last_four: ***");
        Console.WriteLine("  [HIDDEN] billing_address: ***");
        Console.WriteLine("  [HIDDEN] monthly_charge: ***");
        Console.WriteLine("  [HIDDEN] usage_summary: ***");
        Console.WriteLine();

        // =========================================================================
        // PHASE 4: T-Mobile Verifies and Initiates Port
        // =========================================================================
        Console.WriteLine("PHASE 4: T-Mobile Processes Port Request");
        Console.WriteLine(new string('-', 50));

        // T-Mobile would verify Verizon's key via industry trust network
        var verifier = new SdVerifier(_ => Task.FromResult<SecurityKey>(verizonKey));
        var validationParams = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuers = new[]
            {
                "https://api.verizon.example/identity",
                "https://api.att.example/identity",
                "https://api.t-mobile.example/identity"
            },
            ValidateAudience = false,
            ValidateLifetime = true
        };

        var result = await verifier.VerifyAsync(presentation, validationParams);

        if (result.ClaimsPrincipal != null)
        {
            ConsoleHelpers.PrintSuccess("PORT-IN VERIFICATION SUCCESSFUL");
            Console.WriteLine();

            var verifiedClaims = result.ClaimsPrincipal.Claims.ToDictionary(c => c.Type, c => c.Value);
            Console.WriteLine("  Identity verification:");
            Console.WriteLine($"    Account Holder: {verifiedClaims.GetValueOrDefault("account_holder_name")}");
            Console.WriteLine($"    Phone Number: {verifiedClaims.GetValueOrDefault("phone_number")}");
            Console.WriteLine($"    Current Carrier: {verifiedClaims.GetValueOrDefault("carrier_name")}");
            Console.WriteLine();

            Console.WriteLine("  Porting eligibility:");
            Console.WriteLine($"    Account Status: {verifiedClaims.GetValueOrDefault("account_status")}");
            Console.WriteLine($"    Port Eligible: {verifiedClaims.GetValueOrDefault("port_eligible")}");
            Console.WriteLine("    Authorization Code: PORT-2024-ALPHA-9876");
            Console.WriteLine();

            Console.WriteLine("  Device binding:");
            Console.WriteLine($"    EID: {verifiedClaims.GetValueOrDefault("eid")}");
            Console.WriteLine($"    IMEI: {verifiedClaims.GetValueOrDefault("imei")}");
            Console.WriteLine($"    Device: {verifiedClaims.GetValueOrDefault("device_model")}");
            Console.WriteLine();

            Console.WriteLine("  Port-In process initiated:");
            Console.WriteLine("    Status: SUBMITTED TO NPAC");
            Console.WriteLine("    Estimated completion: 2-4 hours");
            Console.WriteLine("    eSIM profile: Ready for download");
        }

        // =========================================================================
        // SECURITY BENEFITS
        // =========================================================================
        Console.WriteLine();
        Console.WriteLine("SECURITY ANALYSIS");
        Console.WriteLine(new string('-', 50));
        Console.WriteLine("This approach prevents common porting fraud:");
        Console.WriteLine();
        Console.WriteLine("  [X] SIM swap fraud - Device EID cryptographically bound");
        Console.WriteLine("  [X] Identity theft - No PII exposed during porting");
        Console.WriteLine("  [X] Account takeover - Authorization from current carrier");
        Console.WriteLine("  [X] Social engineering - No account number/PIN needed");
        Console.WriteLine();
        Console.WriteLine("Privacy protected:");
        Console.WriteLine("  [X] Account credentials never leave current carrier");
        Console.WriteLine("  [X] Billing details stay private");
        Console.WriteLine("  [X] Usage patterns not disclosed");

        // =========================================================================
        // SUMMARY
        // =========================================================================
        Console.WriteLine();
        Console.WriteLine("USE CASE COMPLETE");
        Console.WriteLine(new string('=', 50));
        Console.WriteLine();
        Console.WriteLine("Key achievements:");
        Console.WriteLine("  1. Number ported without sharing account credentials");
        Console.WriteLine("  2. Device identity cryptographically verified (anti-SIM-swap)");
        Console.WriteLine("  3. Current carrier authorized port in credential");
        Console.WriteLine("  4. New carrier didn't need Verizon account number/PIN");
        Console.WriteLine("  5. Faster, more secure than traditional porting");
        Console.WriteLine("  6. eSIM profile bound to verified device identity");
    }
}
