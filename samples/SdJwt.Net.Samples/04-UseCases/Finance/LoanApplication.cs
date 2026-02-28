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

namespace SdJwt.Net.Samples.UseCases.Finance;

/// <summary>
/// Loan Application with Income Verification
///
/// SCENARIO: Customer applies for mortgage loan
/// - Bank needs proof of employment and income range
/// - Exact salary should remain private
/// - Employment verification without employer direct contact
///
/// CLAIMS REVEALED: employer_name, job_title, employment_type, income_range
/// CLAIMS HIDDEN: exact_salary, bonus, stock_options, employee_id
/// </summary>
public static class LoanApplication
{
    public static async Task Run()
    {
        ConsoleHelpers.PrintHeader("Use Case: Mortgage Loan Application");

        Console.WriteLine("SCENARIO: Bob applies for a $500,000 mortgage.");
        Console.WriteLine("The bank needs to verify employment and income level,");
        Console.WriteLine("but Bob wants to keep his exact compensation private.");
        Console.WriteLine();

        // =========================================================================
        // SETUP: Employer and Bank Infrastructure
        // =========================================================================
        using var employerEcdsa = ECDsa.Create(ECCurve.NamedCurves.nistP384);
        using var holderEcdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);

        var employerKey = new ECDsaSecurityKey(employerEcdsa) { KeyId = "techcorp-hr-2024" };
        var holderPrivateKey = new ECDsaSecurityKey(holderEcdsa) { KeyId = "bob-wallet" };
        var holderJwk = JsonWebKeyConverter.ConvertFromSecurityKey(holderPrivateKey);

        // =========================================================================
        // PHASE 1: Employer Issues Income Credential
        // =========================================================================
        Console.WriteLine("PHASE 1: Employer Issues Income Credential");
        Console.WriteLine(new string('-', 50));

        var issuer = new SdJwtVcIssuer(employerKey, SecurityAlgorithms.EcdsaSha384);

        var credentialPayload = new SdJwtVcPayload
        {
            Issuer = "https://hr.techcorp.example",
            Subject = "did:employee:bob-smith-2024",
            IssuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            ExpiresAt = DateTimeOffset.UtcNow.AddYears(1).ToUnixTimeSeconds(),
            AdditionalData = new Dictionary<string, object>
            {
                // Employment verification (non-sensitive)
                ["employer_name"] = "TechCorp Inc.",
                ["employer_ein"] = "12-3456789",
                ["job_title"] = "Principal Software Engineer",
                ["employment_type"] = "Full-time",
                ["employment_start_date"] = "2019-03-15",

                // Income range (can be disclosed)
                ["income_range"] = "200K-300K",
                ["income_currency"] = "USD",

                // Exact compensation (sensitive)
                ["base_salary"] = 245000,
                ["annual_bonus"] = 50000,
                ["stock_options_value"] = 125000,
                ["total_compensation"] = 420000,

                // Internal identifiers (sensitive)
                ["employee_id"] = "TC-789012",
                ["department"] = "Platform Engineering",
                ["cost_center"] = "ENG-PLT-001"
            }
        };

        var sdOptions = new SdIssuanceOptions
        {
            DisclosureStructure = new
            {
                base_salary = true,
                annual_bonus = true,
                stock_options_value = true,
                total_compensation = true,
                employee_id = true,
                department = true,
                cost_center = true
            }
        };

        var credential = issuer.Issue(
            "https://credentials.techcorp.example/IncomeVerification",
            credentialPayload, sdOptions, holderJwk);

        Console.WriteLine("TechCorp HR issued income verification credential to Bob:");
        Console.WriteLine($"  Job: {credentialPayload.AdditionalData["job_title"]}");
        Console.WriteLine($"  Income Range: {credentialPayload.AdditionalData["income_range"]}");
        Console.WriteLine("  Contains exact salary details (selectively disclosable)");
        Console.WriteLine();

        // =========================================================================
        // PHASE 2: Bank Requests Income Verification
        // =========================================================================
        Console.WriteLine("PHASE 2: Mortgage Bank Creates Request");
        Console.WriteLine(new string('-', 50));

        Console.WriteLine("Bank: First National Mortgage");
        Console.WriteLine("Loan Amount: $500,000");
        Console.WriteLine("Required verification:");
        Console.WriteLine("  - Active employment at recognized employer");
        Console.WriteLine("  - Job tenure > 2 years");
        Console.WriteLine("  - Income range supporting loan amount");
        Console.WriteLine();
        Console.WriteLine("NOT required by bank:");
        Console.WriteLine("  - Exact salary figures");
        Console.WriteLine("  - Bonus/stock details");
        Console.WriteLine("  - Internal employee ID");
        Console.WriteLine();

        // =========================================================================
        // PHASE 3: Bob Creates Privacy-Preserving Presentation
        // =========================================================================
        Console.WriteLine("PHASE 3: Bob Creates Presentation");
        Console.WriteLine(new string('-', 50));

        var holder = new SdJwtHolder(credential.Issuance);

        var nonce = $"mortgage_{Guid.NewGuid():N}"[..24];
        var audience = "https://loans.firstnational.example";

        // Disclose ONLY income_range, keep exact figures hidden
        var presentation = holder.CreatePresentation(
            disclosure => false,  // Don't disclose any exact salary claims
            kbJwtPayload: new JwtPayload
            {
                [JwtRegisteredClaimNames.Aud] = audience,
                [JwtRegisteredClaimNames.Iat] = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                ["nonce"] = nonce
            },
            kbJwtSigningKey: holderPrivateKey,
            kbJwtSigningAlgorithm: SecurityAlgorithms.EcdsaSha256
        );

        Console.WriteLine("Bob's presentation discloses:");
        Console.WriteLine("  [REVEALED] employer_name: TechCorp Inc.");
        Console.WriteLine("  [REVEALED] job_title: Principal Software Engineer");
        Console.WriteLine("  [REVEALED] employment_type: Full-time");
        Console.WriteLine("  [REVEALED] employment_start_date: 2019-03-15 (5+ year tenure)");
        Console.WriteLine("  [REVEALED] income_range: 200K-300K");
        Console.WriteLine();
        Console.WriteLine("  [HIDDEN] base_salary: ***");
        Console.WriteLine("  [HIDDEN] annual_bonus: ***");
        Console.WriteLine("  [HIDDEN] stock_options_value: ***");
        Console.WriteLine("  [HIDDEN] total_compensation: ***");
        Console.WriteLine();

        // =========================================================================
        // PHASE 4: Bank Verifies and Makes Decision
        // =========================================================================
        Console.WriteLine("PHASE 4: Bank Verifies and Approves");
        Console.WriteLine(new string('-', 50));

        var verifier = new SdVerifier(_ => Task.FromResult<SecurityKey>(employerKey));
        var validationParams = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuers = new[] { "https://hr.techcorp.example" },
            ValidateAudience = false,
            ValidateLifetime = true
        };

        var result = await verifier.VerifyAsync(presentation, validationParams);

        if (result.ClaimsPrincipal != null)
        {
            ConsoleHelpers.PrintSuccess("INCOME VERIFICATION PASSED");
            Console.WriteLine();

            // Bank's underwriting logic
            var verifiedClaims = result.ClaimsPrincipal.Claims.ToDictionary(c => c.Type, c => c.Value);
            var incomeRange = verifiedClaims.GetValueOrDefault("income_range");
            var employmentStart = verifiedClaims.GetValueOrDefault("employment_start_date");
            var employerName = verifiedClaims.GetValueOrDefault("employer_name");

            Console.WriteLine("  Underwriting assessment:");
            Console.WriteLine($"    Employer: {employerName} (recognized tech employer)");
            Console.WriteLine($"    Tenure: {employmentStart} (5+ years - EXCELLENT)");
            Console.WriteLine($"    Income: {incomeRange} (supports $500K loan)");
            Console.WriteLine();
            Console.WriteLine("  Loan decision: PRE-APPROVED");
            Console.WriteLine("  Debt-to-income ratio: Favorable (based on range)");
        }

        // =========================================================================
        // ALTERNATIVE: Bob Chooses to Disclose More
        // =========================================================================
        Console.WriteLine();
        Console.WriteLine("ALTERNATIVE SCENARIO:");
        Console.WriteLine(new string('-', 50));
        Console.WriteLine("Bob could choose to disclose exact salary for better rate:");
        Console.WriteLine();

        var fullDisclosurePresentation = holder.CreatePresentation(
            disclosure => disclosure.ClaimName is "base_salary" or "total_compensation",
            kbJwtPayload: new JwtPayload
            {
                [JwtRegisteredClaimNames.Aud] = audience,
                [JwtRegisteredClaimNames.Iat] = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                ["nonce"] = $"full_{nonce}"
            },
            kbJwtSigningKey: holderPrivateKey,
            kbJwtSigningAlgorithm: SecurityAlgorithms.EcdsaSha256
        );

        var fullResult = await verifier.VerifyAsync(fullDisclosurePresentation, validationParams);
        if (fullResult.ClaimsPrincipal != null)
        {
            Console.WriteLine("  With full disclosure, bank sees:");
            var fullClaims = fullResult.ClaimsPrincipal.Claims.ToDictionary(c => c.Type, c => c.Value);
            if (fullClaims.TryGetValue("base_salary", out var baseSalary))
                Console.WriteLine($"    Base Salary: ${baseSalary}");
            if (fullClaims.TryGetValue("total_compensation", out var totalComp))
                Console.WriteLine($"    Total Comp: ${totalComp}");
            Console.WriteLine("  Result: Better rate (0.25% reduction)");
        }

        // =========================================================================
        // SUMMARY
        // =========================================================================
        Console.WriteLine();
        Console.WriteLine("USE CASE COMPLETE");
        Console.WriteLine(new string('=', 50));
        Console.WriteLine();
        Console.WriteLine("Key achievements:");
        Console.WriteLine("  1. Bank verified income without seeing exact salary");
        Console.WriteLine("  2. No direct employer contact required");
        Console.WriteLine("  3. Bob chose disclosure level (range vs exact)");
        Console.WriteLine("  4. Underwriting completed with privacy preserved");
        Console.WriteLine("  5. Option to disclose more for better terms");
    }
}
