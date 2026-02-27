using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.Holder;
using SdJwt.Net.Issuer;
using SdJwt.Net.Vc.Issuer;
using SdJwt.Net.Vc.Models;
using SdJwt.Net.Verifier;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;

namespace SdJwt.Net.Samples.RealWorld.Advanced;

/// <summary>
/// Demonstrates an executable policy-first compliance workflow:
/// intent to policy resolution, minimum-claim disclosure, and cryptographic verification.
/// </summary>
public static class AutomatedComplianceScenario
{
    public static async Task RunScenario()
    {
        Console.WriteLine("\n=======================================================");
        Console.WriteLine("    AUTOMATED COMPLIANCE: POLICY-FIRST MINIMIZATION    ");
        Console.WriteLine("=======================================================\n");

        using var issuerEcdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        using var holderEcdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);

        var issuerKey = new ECDsaSecurityKey(issuerEcdsa) { KeyId = "hospital-research-issuer-2026" };
        var holderPrivateKey = new ECDsaSecurityKey(holderEcdsa) { KeyId = "patient-wallet-key-1" };
        var holderPublicKey = new ECDsaSecurityKey(holderEcdsa) { KeyId = "patient-wallet-key-1" };
        var holderJwk = JsonWebKeyConverter.ConvertFromSecurityKey(holderPublicKey);

        var vcIssuer = new SdJwtVcIssuer(issuerKey, SecurityAlgorithms.EcdsaSha256);

        var medicalPayload = new SdJwtVcPayload
        {
            Issuer = "https://clinical-hospital.example.com",
            Subject = "did:example:patient-204",
            IssuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(30).ToUnixTimeSeconds(),
            AdditionalData = new Dictionary<string, object>
            {
                ["age"] = 46,
                ["blood_type"] = "O+",
                ["medications"] = new[] { "Tamoxifen", "Metformin" },
                ["home_address"] = "84 Ridgeway Ave, Springfield",
                ["patient_internal_id"] = "P-204-88-71"
            }
        };

        var disclosureOptions = new SdIssuanceOptions
        {
            DisclosureStructure = new
            {
                age = true,
                blood_type = true,
                medications = true,
                home_address = true,
                patient_internal_id = true
            }
        };

        var issued = vcIssuer.Issue(
            "https://credentials.clinical-hospital.example.com/oncology-summary",
            medicalPayload,
            disclosureOptions,
            holderJwk);

        Console.WriteLine("Input intent: ANON_ONCOLOGY_RESEARCH");
        Console.WriteLine("Requested claims (raw): age, blood_type, medications, home_address");

        var policy = CompliancePolicy.Resolve("ANON_ONCOLOGY_RESEARCH");
        var requestedClaims = new[] { "age", "blood_type", "medications", "home_address" };
        var approvedClaims = requestedClaims.Where(policy.AllowedClaims.Contains).ToArray();
        var blockedClaims = requestedClaims.Except(approvedClaims).ToArray();

        Console.WriteLine($"Policy version: {policy.Version}");
        Console.WriteLine($"Approved claims: {string.Join(", ", approvedClaims)}");
        Console.WriteLine($"Blocked claims: {string.Join(", ", blockedClaims)}");

        var holder = new SdJwtHolder(issued.Issuance);
        var nonce = "compliance-run-2026-001";
        var verifierAudience = "https://research-verifier.example.com";

        var presentation = holder.CreatePresentation(
            disclosure => approvedClaims.Contains(disclosure.ClaimName, StringComparer.Ordinal),
            new JwtPayload
            {
                [JwtRegisteredClaimNames.Aud] = verifierAudience,
                [JwtRegisteredClaimNames.Nonce] = nonce,
                [JwtRegisteredClaimNames.Iat] = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            },
            holderPrivateKey,
            SecurityAlgorithms.EcdsaSha256);

        var verifier = new SdVerifier(jwt => Task.FromResult<SecurityKey>(issuerKey));

        var tokenValidation = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuers = new[] { "https://clinical-hospital.example.com" },
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(5)
        };

        var kbValidation = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = true,
            ValidAudience = verifierAudience,
            ValidateLifetime = false,
            IssuerSigningKey = holderPublicKey,
            ClockSkew = TimeSpan.FromMinutes(5)
        };

        var verification = await verifier.VerifyAsync(presentation, tokenValidation, kbValidation, nonce);

        Console.WriteLine($"\nVerification passed: {verification.KeyBindingVerified}");
        Console.WriteLine("Disclosed claims observed by verifier:");
        PrintIfPresent(verification, "age");
        PrintIfPresent(verification, "blood_type");
        PrintIfPresent(verification, "medications");

        var homeAddressVisible = verification.ClaimsPrincipal.FindFirst("home_address") != null;
        Console.WriteLine($"home_address visible to verifier: {homeAddressVisible}");
        Console.WriteLine("\nResult: policy enforcement blocked over-disclosure and verification used real SD-JWT proofs.");
    }

    private static void PrintIfPresent(VerificationResult verification, string claimType)
    {
        var claim = verification.ClaimsPrincipal.FindFirst(claimType);
        if (claim != null)
        {
            Console.WriteLine($"- {claimType}: {claim.Value}");
        }
    }

    private sealed class CompliancePolicy
    {
        public string Version { get; init; } = string.Empty;
        public HashSet<string> AllowedClaims { get; init; } = new(StringComparer.Ordinal);

        public static CompliancePolicy Resolve(string intentCode)
        {
            return intentCode switch
            {
                "ANON_ONCOLOGY_RESEARCH" => new CompliancePolicy
                {
                    Version = "compliance-policy-v2026.02.1",
                    AllowedClaims = new HashSet<string>(StringComparer.Ordinal)
                    {
                        "age",
                        "blood_type",
                        "medications"
                    }
                },
                _ => new CompliancePolicy
                {
                    Version = "default-v1",
                    AllowedClaims = new HashSet<string>(StringComparer.Ordinal)
                }
            };
        }
    }
}
