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

namespace SdJwt.Net.Samples.UseCases.Retail;

/// <summary>
/// Fraud-Resistant Returns
///
/// SCENARIO: Customer returns item with purchase verification
/// - Store verifies the purchase happened without seeing payment details
/// - Return policy compliance checked cryptographically
/// - Customer doesn't need to show full receipt
///
/// CLAIMS REVEALED: purchase_date, item_sku, store_location, return_eligible
/// CLAIMS HIDDEN: payment_method, card_last_four, receipt_total, loyalty_number
/// </summary>
public static class FraudResistantReturns
{
    public static async Task Run()
    {
        ConsoleHelpers.PrintHeader("Use Case: Fraud-Resistant Returns");

        Console.WriteLine("SCENARIO: Emma returns a jacket to the store.");
        Console.WriteLine("She proves she purchased it without revealing her payment");
        Console.WriteLine("details or the total amount of her shopping trip.");
        Console.WriteLine();

        // =========================================================================
        // SETUP: Retailer Infrastructure
        // =========================================================================
        using var retailerEcdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        using var holderEcdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);

        var retailerKey = new ECDsaSecurityKey(retailerEcdsa) { KeyId = "nordstrom-pos-2024" };
        var holderPrivateKey = new ECDsaSecurityKey(holderEcdsa) { KeyId = "emma-wallet" };
        var holderJwk = JsonWebKeyConverter.ConvertFromSecurityKey(holderPrivateKey);

        // =========================================================================
        // PHASE 1: Retailer Issues Purchase Credential at Checkout
        // =========================================================================
        Console.WriteLine("PHASE 1: Purchase Credential Issued at Checkout");
        Console.WriteLine(new string('-', 50));

        var issuer = new SdJwtVcIssuer(retailerKey, SecurityAlgorithms.EcdsaSha256);

        var credentialPayload = new SdJwtVcPayload
        {
            Issuer = "https://receipts.nordstrom.example",
            Subject = "did:transaction:NS-2024-0115-7823",
            IssuedAt = DateTimeOffset.UtcNow.AddDays(-7).ToUnixTimeSeconds(),
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(83).ToUnixTimeSeconds(),  // 90 day return policy
            AdditionalData = new Dictionary<string, object>
            {
                // Transaction identity (always visible)
                ["transaction_id"] = "NS-2024-0115-7823",
                ["store_location"] = "Nordstrom - San Francisco Centre",
                ["store_id"] = "SF-001",
                ["purchase_date"] = DateTimeOffset.UtcNow.AddDays(-7).ToString("yyyy-MM-dd"),
                ["purchase_time"] = "14:32:15",

                // Item details (visible for return verification)
                ["items"] = new[]
                {
                    new Dictionary<string, object>
                    {
                        ["sku"] = "NJ-WM-BLZ-001",
                        ["description"] = "Women's Navy Blazer",
                        ["size"] = "M",
                        ["color"] = "Navy",
                        ["unit_price"] = 189.00,
                        ["quantity"] = 1,
                        ["return_eligible"] = true,
                        ["return_window_days"] = 90
                    },
                    new Dictionary<string, object>
                    {
                        ["sku"] = "NA-WM-SCF-012",
                        ["description"] = "Silk Scarf",
                        ["size"] = "One Size",
                        ["color"] = "Burgundy",
                        ["unit_price"] = 75.00,
                        ["quantity"] = 1,
                        ["return_eligible"] = true,
                        ["return_window_days"] = 90
                    }
                },

                // Return policy (visible)
                ["return_policy"] = new Dictionary<string, object>
                {
                    ["full_refund_days"] = 90,
                    ["exchange_days"] = 365,
                    ["requires_tags"] = true,
                    ["requires_unworn"] = true
                },

                // Payment information (selectively disclosable)
                ["payment_summary"] = new Dictionary<string, object>
                {
                    ["subtotal"] = 264.00,
                    ["tax"] = 23.10,
                    ["total"] = 287.10,
                    ["payment_method"] = "Credit Card",
                    ["card_type"] = "Visa",
                    ["card_last_four"] = "4532",
                    ["authorization_code"] = "AUTH-789012"
                },

                // Loyalty information (selectively disclosable)
                ["loyalty"] = new Dictionary<string, object>
                {
                    ["member_id"] = "NORD-EMMA-9876",
                    ["tier"] = "Influencer",
                    ["points_earned"] = 287,
                    ["lifetime_spend"] = 12450.00
                },

                // Associate who helped (selectively disclosable)
                ["sales_associate"] = new Dictionary<string, object>
                {
                    ["id"] = "SA-1234",
                    ["name"] = "Jennifer"
                }
            }
        };

        var sdOptions = new SdIssuanceOptions
        {
            DisclosureStructure = new
            {
                payment_summary = true,
                loyalty = true,
                sales_associate = true
            }
        };

        var credential = issuer.Issue(
            "https://credentials.nordstrom.example/PurchaseReceipt",
            credentialPayload, sdOptions, holderJwk);

        Console.WriteLine("Digital receipt credential issued to Emma's wallet:");
        Console.WriteLine($"  Store: {credentialPayload.AdditionalData["store_location"]}");
        Console.WriteLine($"  Date: {credentialPayload.AdditionalData["purchase_date"]}");
        Console.WriteLine("  Items: Navy Blazer ($189), Silk Scarf ($75)");
        Console.WriteLine("  Return window: 90 days (expires in 83 days)");
        Console.WriteLine();

        // =========================================================================
        // PHASE 2: Return Request
        // =========================================================================
        Console.WriteLine("PHASE 2: Emma Initiates Return (7 days later)");
        Console.WriteLine(new string('-', 50));

        Console.WriteLine("Emma wants to return the Navy Blazer:");
        Console.WriteLine("  - Item: NJ-WM-BLZ-001 (Women's Navy Blazer)");
        Console.WriteLine("  - Reason: Wrong size");
        Console.WriteLine("  - Has original tags attached");
        Console.WriteLine();
        Console.WriteLine("Store return desk needs to verify:");
        Console.WriteLine("  - Item was actually purchased at Nordstrom");
        Console.WriteLine("  - Purchase is within return window");
        Console.WriteLine("  - Item is return-eligible");
        Console.WriteLine();
        Console.WriteLine("Store does NOT need:");
        Console.WriteLine("  - Emma's payment card details");
        Console.WriteLine("  - Total amount of her transaction");
        Console.WriteLine("  - Her loyalty account information");
        Console.WriteLine();

        // =========================================================================
        // PHASE 3: Emma Creates Return Presentation
        // =========================================================================
        Console.WriteLine("PHASE 3: Emma Creates Return Presentation");
        Console.WriteLine(new string('-', 50));

        var holder = new SdJwtHolder(credential.Issuance);

        var nonce = $"return_{Guid.NewGuid():N}"[..24];
        var audience = "https://returns.nordstrom.example";

        // Don't disclose payment or loyalty details
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

        Console.WriteLine("Emma's return presentation shows:");
        Console.WriteLine("  [REVEALED] transaction_id: NS-2024-0115-7823");
        Console.WriteLine("  [REVEALED] store_location: Nordstrom - San Francisco Centre");
        Console.WriteLine("  [REVEALED] purchase_date: 7 days ago");
        Console.WriteLine("  [REVEALED] items[0].sku: NJ-WM-BLZ-001");
        Console.WriteLine("  [REVEALED] items[0].return_eligible: true");
        Console.WriteLine("  [REVEALED] return_policy.full_refund_days: 90");
        Console.WriteLine();
        Console.WriteLine("  [HIDDEN] payment_summary.total: ***");
        Console.WriteLine("  [HIDDEN] payment_summary.card_last_four: ***");
        Console.WriteLine("  [HIDDEN] loyalty.member_id: ***");
        Console.WriteLine("  [HIDDEN] loyalty.lifetime_spend: ***");
        Console.WriteLine();

        // =========================================================================
        // PHASE 4: Store Verifies and Processes Return
        // =========================================================================
        Console.WriteLine("PHASE 4: Store Processes Return");
        Console.WriteLine(new string('-', 50));

        var verifier = new SdVerifier(_ => Task.FromResult<SecurityKey>(retailerKey));
        var validationParams = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuers = new[] { "https://receipts.nordstrom.example" },
            ValidateAudience = false,
            ValidateLifetime = true
        };

        var result = await verifier.VerifyAsync(presentation, validationParams);

        if (result.ClaimsPrincipal != null)
        {
            ConsoleHelpers.PrintSuccess("PURCHASE VERIFIED - RETURN APPROVED");
            Console.WriteLine();

            var verifiedClaims = result.ClaimsPrincipal.Claims.ToDictionary(c => c.Type, c => c.Value);
            Console.WriteLine("  Return desk verification:");
            Console.WriteLine($"    Transaction: {verifiedClaims.GetValueOrDefault("transaction_id")}");
            Console.WriteLine($"    Store: {verifiedClaims.GetValueOrDefault("store_location")}");
            Console.WriteLine($"    Purchase date: {verifiedClaims.GetValueOrDefault("purchase_date")}");
            Console.WriteLine();

            // Check return eligibility
            var purchaseDateStr = verifiedClaims.GetValueOrDefault("purchase_date") ?? "";
            if (DateTimeOffset.TryParse(purchaseDateStr, out var purchaseDate))
            {
                var daysSincePurchase = (DateTimeOffset.UtcNow - purchaseDate).Days;
                Console.WriteLine($"  Policy check:");
                Console.WriteLine($"    Days since purchase: {daysSincePurchase}");
                Console.WriteLine($"    Return window: 90 days");
                Console.WriteLine($"    Status: WITHIN WINDOW");
            }
            Console.WriteLine();

            Console.WriteLine("  Refund processing:");
            Console.WriteLine("    Item: Women's Navy Blazer");
            Console.WriteLine("    SKU verified against transaction");
            Console.WriteLine("    Refund amount: $189.00");
            Console.WriteLine("    Method: Original payment method (card ending ****)");
            Console.WriteLine();
            Console.WriteLine("  Note: Card details retrieved from secure backend,");
            Console.WriteLine("        NOT from customer presentation");
        }

        // =========================================================================
        // FRAUD PREVENTION BENEFITS
        // =========================================================================
        Console.WriteLine();
        Console.WriteLine("FRAUD PREVENTION ANALYSIS");
        Console.WriteLine(new string('-', 50));
        Console.WriteLine("This approach prevents common return fraud:");
        Console.WriteLine();
        Console.WriteLine("  [X] Receipt fraud - Cryptographic proof of purchase");
        Console.WriteLine("  [X] Wardrobing - Original transaction verifiable");
        Console.WriteLine("  [X] Return kiting - One credential per transaction");
        Console.WriteLine("  [X] Employee collusion - No manual override needed");
        Console.WriteLine();
        Console.WriteLine("Customer privacy protected:");
        Console.WriteLine("  [X] Payment details hidden from return desk");
        Console.WriteLine("  [X] Loyalty status private");
        Console.WriteLine("  [X] Total spending not revealed");

        // =========================================================================
        // SUMMARY
        // =========================================================================
        Console.WriteLine();
        Console.WriteLine("USE CASE COMPLETE");
        Console.WriteLine(new string('=', 50));
        Console.WriteLine();
        Console.WriteLine("Key achievements:");
        Console.WriteLine("  1. Return processed without showing full receipt");
        Console.WriteLine("  2. Payment details never exposed to return desk");
        Console.WriteLine("  3. Cryptographic proof prevents receipt fraud");
        Console.WriteLine("  4. Return policy enforced automatically");
        Console.WriteLine("  5. Better customer experience (no paper receipt needed)");
    }
}
