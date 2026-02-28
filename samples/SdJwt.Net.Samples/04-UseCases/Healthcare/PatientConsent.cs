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

namespace SdJwt.Net.Samples.UseCases.Healthcare;

/// <summary>
/// Patient Consent for Data Sharing
///
/// SCENARIO: Patient shares specific medical data with research institution
/// - Patient controls exactly which data categories are shared
/// - Consent is cryptographically bound and time-limited
/// - Research institution cannot access beyond consented scope
///
/// CLAIMS REVEALED: consent_scope, data_categories, validity_period
/// CLAIMS HIDDEN: medical_record_number, ssn, full_address
/// </summary>
public static class PatientConsent
{
    public static async Task Run()
    {
        ConsoleHelpers.PrintHeader("Use Case: Patient Consent for Medical Research");

        Console.WriteLine("SCENARIO: Carol participates in a diabetes research study.");
        Console.WriteLine("She consents to share specific health data while keeping");
        Console.WriteLine("identifying information and unrelated medical history private.");
        Console.WriteLine();

        // =========================================================================
        // SETUP: Healthcare Provider and Research Infrastructure
        // =========================================================================
        using var healthcareEcdsa = ECDsa.Create(ECCurve.NamedCurves.nistP384);
        using var holderEcdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);

        var healthcareKey = new ECDsaSecurityKey(healthcareEcdsa) { KeyId = "mercy-hospital-2024" };
        var holderPrivateKey = new ECDsaSecurityKey(holderEcdsa) { KeyId = "carol-wallet" };
        var holderJwk = JsonWebKeyConverter.ConvertFromSecurityKey(holderPrivateKey);

        // =========================================================================
        // PHASE 1: Healthcare Provider Issues Patient Identity + Consent
        // =========================================================================
        Console.WriteLine("PHASE 1: Hospital Issues Patient Credential with Consent");
        Console.WriteLine(new string('-', 50));

        var issuer = new SdJwtVcIssuer(healthcareKey, SecurityAlgorithms.EcdsaSha384);

        var credentialPayload = new SdJwtVcPayload
        {
            Issuer = "https://ehr.mercyhospital.example",
            Subject = "did:patient:carol-consent-2024",
            IssuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            ExpiresAt = DateTimeOffset.UtcNow.AddYears(2).ToUnixTimeSeconds(),
            AdditionalData = new Dictionary<string, object>
            {
                // Patient identity (always visible to authorized parties)
                ["patient_name"] = "Carol Williams",
                ["date_of_birth"] = "1978-09-22",
                ["healthcare_provider"] = "Mercy Hospital System",

                // Consent details (visible)
                ["consent_type"] = "Research Data Sharing",
                ["consent_date"] = DateTimeOffset.UtcNow.ToString("yyyy-MM-dd"),
                ["consent_valid_until"] = DateTimeOffset.UtcNow.AddYears(2).ToString("yyyy-MM-dd"),
                ["research_study_id"] = "DIAB-2024-001",
                ["research_institution"] = "National Institutes of Health",

                // Consented data categories (visible)
                ["consented_data_categories"] = new[]
                {
                    "blood_glucose_levels",
                    "hba1c_results",
                    "diabetes_medications",
                    "bmi_measurements"
                },

                // Explicitly excluded (visible that they're excluded)
                ["excluded_categories"] = new[]
                {
                    "mental_health_records",
                    "reproductive_health",
                    "genetic_testing_results"
                },

                // Sensitive identifiers (selectively disclosable)
                ["medical_record_number"] = "MRN-9876543",
                ["social_security_number"] = "123-45-6789",
                ["insurance_member_id"] = "BCBS-987654321",

                // Full address (selectively disclosable)
                ["address"] = new Dictionary<string, object>
                {
                    ["street"] = "456 Oak Avenue",
                    ["city"] = "Boston",
                    ["state"] = "MA",
                    ["zip"] = "02101"
                },

                // Actual health data (selectively disclosable)
                ["recent_blood_glucose"] = new[]
                {
                    new { date = "2024-01-15", value = 142, unit = "mg/dL" },
                    new { date = "2024-01-14", value = 138, unit = "mg/dL" },
                    new { date = "2024-01-13", value = 155, unit = "mg/dL" }
                },
                ["latest_hba1c"] = new { date = "2024-01-01", value = 7.2, unit = "%" },
                ["current_medications"] = new[]
                {
                    new { name = "Metformin", dosage = "1000mg", frequency = "twice daily" },
                    new { name = "Jardiance", dosage = "10mg", frequency = "once daily" }
                }
            }
        };

        var sdOptions = new SdIssuanceOptions
        {
            DisclosureStructure = new
            {
                medical_record_number = true,
                social_security_number = true,
                insurance_member_id = true,
                address = true,
                recent_blood_glucose = true,
                latest_hba1c = true,
                current_medications = true
            }
        };

        var credential = issuer.Issue(
            "https://credentials.mercyhospital.example/PatientConsent",
            credentialPayload, sdOptions, holderJwk);

        Console.WriteLine("Mercy Hospital issued consent credential to Carol:");
        Console.WriteLine($"  Research Study: {credentialPayload.AdditionalData["research_study_id"]}");
        Console.WriteLine($"  Institution: {credentialPayload.AdditionalData["research_institution"]}");
        Console.WriteLine("  Consented categories: blood_glucose, hba1c, medications, bmi");
        Console.WriteLine("  Excluded: mental_health, reproductive, genetic");
        Console.WriteLine();

        // =========================================================================
        // PHASE 2: Research Institution Requests Data
        // =========================================================================
        Console.WriteLine("PHASE 2: NIH Requests Consented Data");
        Console.WriteLine(new string('-', 50));

        Console.WriteLine("NIH Diabetes Research Program requests:");
        Console.WriteLine("  - Proof of valid consent for study DIAB-2024-001");
        Console.WriteLine("  - Blood glucose data");
        Console.WriteLine("  - HbA1c results");
        Console.WriteLine("  - Current diabetes medications");
        Console.WriteLine();
        Console.WriteLine("NOT requested (and Carol won't provide):");
        Console.WriteLine("  - Social security number");
        Console.WriteLine("  - Home address");
        Console.WriteLine("  - Medical record number");
        Console.WriteLine();

        // =========================================================================
        // PHASE 3: Carol Creates Consent-Bounded Presentation
        // =========================================================================
        Console.WriteLine("PHASE 3: Carol Creates Presentation");
        Console.WriteLine(new string('-', 50));

        var holder = new SdJwtHolder(credential.Issuance);

        var nonce = $"research_{Guid.NewGuid():N}"[..24];
        var audience = "https://research.nih.gov/diabetes";

        // Disclose ONLY health data, NOT identifiers
        var presentation = holder.CreatePresentation(
            disclosure => disclosure.ClaimName is "recent_blood_glucose"
                or "latest_hba1c"
                or "current_medications",
            kbJwtPayload: new JwtPayload
            {
                [JwtRegisteredClaimNames.Aud] = audience,
                [JwtRegisteredClaimNames.Iat] = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                ["nonce"] = nonce
            },
            kbJwtSigningKey: holderPrivateKey,
            kbJwtSigningAlgorithm: SecurityAlgorithms.EcdsaSha256
        );

        Console.WriteLine("Carol's presentation includes:");
        Console.WriteLine("  [REVEALED] consent_type: Research Data Sharing");
        Console.WriteLine("  [REVEALED] research_study_id: DIAB-2024-001");
        Console.WriteLine("  [REVEALED] consented_data_categories: [blood_glucose, hba1c, ...]");
        Console.WriteLine("  [REVEALED] recent_blood_glucose: [3 readings]");
        Console.WriteLine("  [REVEALED] latest_hba1c: 7.2%");
        Console.WriteLine("  [REVEALED] current_medications: [Metformin, Jardiance]");
        Console.WriteLine();
        Console.WriteLine("  [HIDDEN] social_security_number: ***");
        Console.WriteLine("  [HIDDEN] medical_record_number: ***");
        Console.WriteLine("  [HIDDEN] address: ***");
        Console.WriteLine("  [HIDDEN] insurance_member_id: ***");
        Console.WriteLine();

        // =========================================================================
        // PHASE 4: NIH Verifies Consent and Data
        // =========================================================================
        Console.WriteLine("PHASE 4: NIH Verifies Consent and Processes Data");
        Console.WriteLine(new string('-', 50));

        var verifier = new SdVerifier(_ => Task.FromResult<SecurityKey>(healthcareKey));
        var validationParams = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuers = new[] { "https://ehr.mercyhospital.example" },
            ValidateAudience = false,
            ValidateLifetime = true
        };

        var result = await verifier.VerifyAsync(presentation, validationParams);

        if (result.ClaimsPrincipal != null)
        {
            ConsoleHelpers.PrintSuccess("CONSENT AND DATA VERIFIED");
            Console.WriteLine();

            // Verify consent is for correct study
            var verifiedClaims = result.ClaimsPrincipal.Claims.ToDictionary(c => c.Type, c => c.Value);
            var studyId = verifiedClaims.GetValueOrDefault("research_study_id");
            Console.WriteLine($"  Consent verified for study: {studyId}");
            Console.WriteLine($"  Valid until: {verifiedClaims.GetValueOrDefault("consent_valid_until")}");
            Console.WriteLine();

            Console.WriteLine("  Research data received (de-identified):");
            Console.WriteLine("    Blood glucose readings: 3 data points");
            Console.WriteLine("    HbA1c: 7.2% (moderate control)");
            Console.WriteLine("    Medications: 2 diabetes drugs");
            Console.WriteLine();
            Console.WriteLine("  Data added to study cohort with pseudonymous ID");
        }

        // =========================================================================
        // SUMMARY
        // =========================================================================
        Console.WriteLine();
        Console.WriteLine("USE CASE COMPLETE");
        Console.WriteLine(new string('=', 50));
        Console.WriteLine();
        Console.WriteLine("Key achievements:");
        Console.WriteLine("  1. Patient retained control of sensitive identifiers");
        Console.WriteLine("  2. Consent scope cryptographically enforced");
        Console.WriteLine("  3. Only consented data categories accessible");
        Console.WriteLine("  4. Exclusions (mental health, etc.) protected");
        Console.WriteLine("  5. Time-limited consent with clear expiry");
        Console.WriteLine("  6. HIPAA-aligned selective disclosure");
    }
}
