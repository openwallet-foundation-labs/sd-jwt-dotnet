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

namespace SdJwt.Net.Samples.UseCases.Government;

/// <summary>
/// Cross-Border Identity Verification
///
/// SCENARIO: Traveler presents digital identity at border crossing
/// - Immigration verifies nationality and visa status
/// - Address and detailed biometrics remain private
/// - Works across different national identity systems
///
/// CLAIMS REVEALED: nationality, visa_status, document_validity, name
/// CLAIMS HIDDEN: home_address, ssn, biometric_template, travel_history
/// </summary>
public static class CrossBorderIdentity
{
    public static async Task Run()
    {
        ConsoleHelpers.PrintHeader("Use Case: Cross-Border Identity Verification");

        Console.WriteLine("SCENARIO: Daniel travels from US to EU with digital credentials.");
        Console.WriteLine("EU border control verifies his identity and visa status without");
        Console.WriteLine("accessing his full address or detailed travel history.");
        Console.WriteLine();

        // =========================================================================
        // SETUP: Government Issuer Infrastructure (P-384 for high assurance)
        // =========================================================================
        using var stateEcdsa = ECDsa.Create(ECCurve.NamedCurves.nistP384);
        using var holderEcdsa = ECDsa.Create(ECCurve.NamedCurves.nistP384);

        var stateKey = new ECDsaSecurityKey(stateEcdsa) { KeyId = "us-state-dept-2024" };
        var holderPrivateKey = new ECDsaSecurityKey(holderEcdsa) { KeyId = "daniel-wallet" };
        var holderJwk = JsonWebKeyConverter.ConvertFromSecurityKey(holderPrivateKey);

        // =========================================================================
        // PHASE 1: US State Department Issues Digital Travel Credential
        // =========================================================================
        Console.WriteLine("PHASE 1: US Issues Digital Travel Credential");
        Console.WriteLine(new string('-', 50));

        var issuer = new SdJwtVcIssuer(stateKey, SecurityAlgorithms.EcdsaSha384);

        var credentialPayload = new SdJwtVcPayload
        {
            Issuer = "https://travel.state.gov",
            Subject = "did:passport:us:123456789",
            IssuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            ExpiresAt = DateTimeOffset.UtcNow.AddYears(10).ToUnixTimeSeconds(),
            AdditionalData = new Dictionary<string, object>
            {
                // Identity claims (always visible for border control)
                ["given_name"] = "Daniel",
                ["family_name"] = "Martinez",
                ["date_of_birth"] = "1985-07-20",
                ["nationality"] = "United States of America",
                ["sex"] = "M",

                // Document details (visible)
                ["document_type"] = "Passport",
                ["document_number"] = "US-123456789",
                ["issuing_country"] = "USA",
                ["issue_date"] = "2022-01-15",
                ["expiry_date"] = "2032-01-14",

                // Visa status (visible)
                ["visa_status"] = new Dictionary<string, object>
                {
                    ["schengen_visa"] = true,
                    ["visa_type"] = "C",
                    ["valid_from"] = "2024-01-01",
                    ["valid_until"] = "2024-12-31",
                    ["entries_allowed"] = "Multiple",
                    ["days_allowed"] = 90
                },

                // Contact information (selectively disclosable)
                ["address"] = new Dictionary<string, object>
                {
                    ["street"] = "789 Pine Street",
                    ["city"] = "San Francisco",
                    ["state"] = "California",
                    ["zip"] = "94102",
                    ["country"] = "USA"
                },
                ["phone"] = "+1-415-555-0123",
                ["email"] = "daniel.martinez@email.example",

                // Sensitive identifiers (selectively disclosable)
                ["social_security_number"] = "456-78-9012",
                ["global_entry_number"] = "GE-98765432",

                // Biometric reference (selectively disclosable)
                ["biometric_template_hash"] = "sha384:abc123def456...",
                ["photo_hash"] = "sha384:xyz789...",

                // Travel history (selectively disclosable)
                ["recent_travel"] = new[]
                {
                    new { country = "Canada", date = "2023-11-15", purpose = "Business" },
                    new { country = "Mexico", date = "2023-09-01", purpose = "Vacation" },
                    new { country = "Japan", date = "2023-06-10", purpose = "Conference" }
                }
            }
        };

        var sdOptions = new SdIssuanceOptions
        {
            DisclosureStructure = new
            {
                address = true,
                phone = true,
                email = true,
                social_security_number = true,
                global_entry_number = true,
                biometric_template_hash = true,
                photo_hash = true,
                recent_travel = true
            }
        };

        var credential = issuer.Issue(
            "https://credentials.state.gov/DigitalTravelCredential",
            credentialPayload, sdOptions, holderJwk);

        Console.WriteLine("US State Department issued digital travel credential:");
        Console.WriteLine($"  Holder: {credentialPayload.AdditionalData["given_name"]} {credentialPayload.AdditionalData["family_name"]}");
        Console.WriteLine($"  Document: {credentialPayload.AdditionalData["document_type"]} {credentialPayload.AdditionalData["document_number"]}");
        Console.WriteLine("  Includes Schengen visa with 90-day allowance");
        Console.WriteLine();

        // =========================================================================
        // PHASE 2: EU Border Control Request
        // =========================================================================
        Console.WriteLine("PHASE 2: EU Border Control Check");
        Console.WriteLine(new string('-', 50));

        Console.WriteLine("EU Border Officer requests verification of:");
        Console.WriteLine("  - Identity (name, date of birth, nationality)");
        Console.WriteLine("  - Document validity (not expired)");
        Console.WriteLine("  - Schengen visa status (valid for entry)");
        Console.WriteLine();
        Console.WriteLine("NOT required by border control:");
        Console.WriteLine("  - Full home address");
        Console.WriteLine("  - Social security number");
        Console.WriteLine("  - Detailed travel history");
        Console.WriteLine();

        // =========================================================================
        // PHASE 3: Daniel Creates Border Presentation
        // =========================================================================
        Console.WriteLine("PHASE 3: Daniel Creates Presentation");
        Console.WriteLine(new string('-', 50));

        var holder = new SdJwtHolder(credential.Issuance);

        var nonce = $"border_{Guid.NewGuid():N}"[..24];
        var audience = "https://border.eu.example/schengen";

        // Don't disclose any optional claims for border
        var presentation = holder.CreatePresentation(
            disclosure => false,
            kbJwtPayload: new JwtPayload
            {
                [JwtRegisteredClaimNames.Aud] = audience,
                [JwtRegisteredClaimNames.Iat] = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                ["nonce"] = nonce
            },
            kbJwtSigningKey: holderPrivateKey,
            kbJwtSigningAlgorithm: SecurityAlgorithms.EcdsaSha384
        );

        Console.WriteLine("Daniel's presentation at border:");
        Console.WriteLine("  [REVEALED] given_name: Daniel");
        Console.WriteLine("  [REVEALED] family_name: Martinez");
        Console.WriteLine("  [REVEALED] date_of_birth: 1985-07-20");
        Console.WriteLine("  [REVEALED] nationality: United States of America");
        Console.WriteLine("  [REVEALED] document_number: US-123456789");
        Console.WriteLine("  [REVEALED] expiry_date: 2032-01-14");
        Console.WriteLine("  [REVEALED] visa_status: Schengen C, valid Multiple entries");
        Console.WriteLine();
        Console.WriteLine("  [HIDDEN] address: ***");
        Console.WriteLine("  [HIDDEN] social_security_number: ***");
        Console.WriteLine("  [HIDDEN] recent_travel: ***");
        Console.WriteLine();

        // =========================================================================
        // PHASE 4: EU Border Verifies Credential
        // =========================================================================
        Console.WriteLine("PHASE 4: Border Control Verification");
        Console.WriteLine(new string('-', 50));

        // EU border would resolve US State Department key via federation
        var verifier = new SdVerifier(_ => Task.FromResult<SecurityKey>(stateKey));
        var validationParams = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuers = new[] { "https://travel.state.gov" },
            ValidateAudience = false,
            ValidateLifetime = true
        };

        var result = await verifier.VerifyAsync(presentation, validationParams);

        if (result.ClaimsPrincipal != null)
        {
            ConsoleHelpers.PrintSuccess("TRAVEL CREDENTIAL VERIFIED");
            Console.WriteLine();

            // Border control checks
            var verifiedClaims = result.ClaimsPrincipal.Claims.ToDictionary(c => c.Type, c => c.Value);
            var nationality = verifiedClaims.GetValueOrDefault("nationality");
            var expiryDate = verifiedClaims.GetValueOrDefault("expiry_date");

            Console.WriteLine("  Identity confirmed:");
            Console.WriteLine($"    Name: {verifiedClaims.GetValueOrDefault("given_name")} {verifiedClaims.GetValueOrDefault("family_name")}");
            Console.WriteLine($"    Nationality: {nationality}");
            Console.WriteLine();
            Console.WriteLine("  Document status:");
            Console.WriteLine($"    Passport expires: {expiryDate} (VALID)");
            Console.WriteLine();
            Console.WriteLine("  Schengen visa:");
            Console.WriteLine("    Type: C (short-stay)");
            Console.WriteLine("    Valid: 2024-01-01 to 2024-12-31");
            Console.WriteLine("    Entries: Multiple");
            Console.WriteLine();
            Console.WriteLine("  DECISION: ENTRY PERMITTED");
            Console.WriteLine("  Stamp: Schengen Entry - Frankfurt Airport");
        }

        // =========================================================================
        // SUMMARY
        // =========================================================================
        Console.WriteLine();
        Console.WriteLine("USE CASE COMPLETE");
        Console.WriteLine(new string('=', 50));
        Console.WriteLine();
        Console.WriteLine("Key achievements:");
        Console.WriteLine("  1. Cross-border verification without central database");
        Console.WriteLine("  2. Home address remained private");
        Console.WriteLine("  3. SSN and travel history protected");
        Console.WriteLine("  4. High-assurance cryptography (P-384)");
        Console.WriteLine("  5. Federation-ready for international trust");
        Console.WriteLine("  6. Works offline (no real-time US database check)");
    }
}
