using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.Holder;
using SdJwt.Net.Issuer;
using SdJwt.Net.Models;
using SdJwt.Net.PresentationExchange.Models;
using SdJwt.Net.Samples.Shared;
using SdJwt.Net.Vc.Issuer;
using SdJwt.Net.Vc.Models;
using SdJwt.Net.Verifier;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;

namespace SdJwt.Net.Samples.Advanced;

/// <summary>
/// Tutorial 03: Multi-Credential Flows
///
/// LEARNING OBJECTIVES:
/// - Present multiple credentials in one request
/// - Handle different credential types together
/// - Manage wallet inventory for complex requests
/// - Build combined verification workflows
///
/// TIME: ~20 minutes
/// </summary>
public static class MultiCredentialFlow
{
    public static async Task Run()
    {
        ConsoleHelpers.PrintHeader("Tutorial 03: Multi-Credential Flows");

        Console.WriteLine("Real-world verifications often require multiple credentials.");
        Console.WriteLine("This tutorial shows how to handle complex presentations with");
        Console.WriteLine("credentials from different issuers for different purposes.");
        Console.WriteLine();

        // Setup keys for different issuers
        using var universityEcdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        using var employerEcdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        using var govEcdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        using var holderEcdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);

        var universityKey = new ECDsaSecurityKey(universityEcdsa) { KeyId = "uni-2024" };
        var employerKey = new ECDsaSecurityKey(employerEcdsa) { KeyId = "emp-2024" };
        var govKey = new ECDsaSecurityKey(govEcdsa) { KeyId = "gov-2024" };
        var holderPrivateKey = new ECDsaSecurityKey(holderEcdsa) { KeyId = "alice-wallet" };
        var holderJwk = JsonWebKeyConverter.ConvertFromSecurityKey(holderPrivateKey);

        // =====================================================================
        // STEP 1: Use case - Immigration application
        // =====================================================================
        ConsoleHelpers.PrintStep(1, "Use case: Immigration application");

        Console.WriteLine("Scenario: Alice applies for a visa requiring:");
        Console.WriteLine();
        Console.WriteLine("  1. Proof of identity (Government ID)");
        Console.WriteLine("  2. Proof of education (University degree)");
        Console.WriteLine("  3. Proof of employment (Employment letter)");
        Console.WriteLine();
        Console.WriteLine("Each credential comes from a different issuer and");
        Console.WriteLine("contains different selectively disclosable claims.");

        // =====================================================================
        // STEP 2: Issue multiple credentials to wallet
        // =====================================================================
        ConsoleHelpers.PrintStep(2, "Populate wallet with credentials");

        // Issue Government ID
        var govIssuer = new SdJwtVcIssuer(govKey, SecurityAlgorithms.EcdsaSha256);
        var govIdPayload = new SdJwtVcPayload
        {
            Issuer = "https://id.gov.example",
            Subject = "did:example:alice",
            IssuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            ExpiresAt = DateTimeOffset.UtcNow.AddYears(10).ToUnixTimeSeconds(),
            AdditionalData = new Dictionary<string, object>
            {
                ["given_name"] = "Alice",
                ["family_name"] = "Johnson",
                ["birthdate"] = "1992-03-15",
                ["nationality"] = "United States",
                ["document_number"] = "US-123456789"
            }
        };
        var govIdResult = govIssuer.Issue(
            "https://id.gov.example/NationalID",
            govIdPayload,
            new SdIssuanceOptions
            {
                DisclosureStructure = new
                {
                    given_name = true,
                    family_name = true,
                    birthdate = true,
                    document_number = true
                }
            },
            holderJwk);

        // Issue University Degree
        var uniIssuer = new SdJwtVcIssuer(universityKey, SecurityAlgorithms.EcdsaSha256);
        var degreePayload = new SdJwtVcPayload
        {
            Issuer = "https://degree.university.example",
            Subject = "did:example:alice",
            IssuedAt = DateTimeOffset.UtcNow.AddYears(-2).ToUnixTimeSeconds(),
            ExpiresAt = DateTimeOffset.UtcNow.AddYears(50).ToUnixTimeSeconds(),
            AdditionalData = new Dictionary<string, object>
            {
                ["degree_type"] = "Master of Science",
                ["field_of_study"] = "Computer Science",
                ["graduation_date"] = "2021-05-20",
                ["gpa"] = 3.92,
                ["honors"] = "Cum Laude"
            }
        };
        var degreeResult = uniIssuer.Issue(
            "https://credentials.university.example/UniversityDegree",
            degreePayload,
            new SdIssuanceOptions
            {
                DisclosureStructure = new
                {
                    gpa = true,
                    honors = true
                }
            },
            holderJwk);

        // Issue Employment Letter
        var empIssuer = new SdJwtVcIssuer(employerKey, SecurityAlgorithms.EcdsaSha256);
        var employmentPayload = new SdJwtVcPayload
        {
            Issuer = "https://hr.techcorp.example",
            Subject = "did:example:alice",
            IssuedAt = DateTimeOffset.UtcNow.AddMonths(-1).ToUnixTimeSeconds(),
            ExpiresAt = DateTimeOffset.UtcNow.AddYears(1).ToUnixTimeSeconds(),
            AdditionalData = new Dictionary<string, object>
            {
                ["job_title"] = "Senior Software Engineer",
                ["department"] = "Platform Engineering",
                ["start_date"] = "2021-06-01",
                ["annual_salary"] = 180000,
                ["employment_type"] = "Full-time"
            }
        };
        var employmentResult = empIssuer.Issue(
            "https://credentials.techcorp.example/EmploymentLetter",
            employmentPayload,
            new SdIssuanceOptions
            {
                DisclosureStructure = new
                {
                    department = true,
                    annual_salary = true
                }
            },
            holderJwk);

        Console.WriteLine("Wallet now contains:");
        Console.WriteLine($"  [1] National ID from {govIdPayload.Issuer}");
        Console.WriteLine($"  [2] University Degree from {degreePayload.Issuer}");
        Console.WriteLine($"  [3] Employment Letter from {employmentPayload.Issuer}");

        // =====================================================================
        // STEP 3: Verifier creates multi-credential request
        // =====================================================================
        ConsoleHelpers.PrintStep(3, "Verifier creates presentation request");

        Console.WriteLine("Immigration office requests multiple credentials:");
        Console.WriteLine();

        var multiDefinition = new PresentationDefinition
        {
            Id = "visa-application-2024",
            Name = "Visa Application Requirements",
            Purpose = "Verify identity, education, and employment for visa application",
            InputDescriptors = new[]
            {
                new InputDescriptor
                {
                    Id = "identity_proof",
                    Name = "Government-issued ID",
                    Purpose = "Verify applicant identity",
                    Constraints = new Constraints
                    {
                        Fields = new[]
                        {
                            new Field { Path = new[] { "$.vct" }, Filter = new FieldFilter { Pattern = ".*NationalID.*" } },
                            new Field { Path = new[] { "$.given_name" } },
                            new Field { Path = new[] { "$.family_name" } },
                            new Field { Path = new[] { "$.nationality" } }
                        }
                    }
                },
                new InputDescriptor
                {
                    Id = "education_proof",
                    Name = "Educational Qualification",
                    Purpose = "Verify educational background",
                    Constraints = new Constraints
                    {
                        Fields = new[]
                        {
                            new Field { Path = new[] { "$.vct" }, Filter = new FieldFilter { Pattern = ".*Degree.*" } },
                            new Field { Path = new[] { "$.degree_type" } },
                            new Field { Path = new[] { "$.field_of_study" } }
                        }
                    }
                },
                new InputDescriptor
                {
                    Id = "employment_proof",
                    Name = "Employment Verification",
                    Purpose = "Verify current employment",
                    Constraints = new Constraints
                    {
                        Fields = new[]
                        {
                            new Field { Path = new[] { "$.vct" }, Filter = new FieldFilter { Pattern = ".*Employment.*" } },
                            new Field { Path = new[] { "$.job_title" } },
                            new Field { Path = new[] { "$.employment_type" } }
                        }
                    }
                }
            }
        };

        Console.WriteLine("  presentation_definition:");
        Console.WriteLine($"    id: {multiDefinition.Id}");
        Console.WriteLine("    input_descriptors:");
        foreach (var desc in multiDefinition.InputDescriptors)
        {
            Console.WriteLine($"      - {desc.Id}: {desc.Name}");
        }

        // =====================================================================
        // STEP 4: Wallet matches and prepares presentations
        // =====================================================================
        ConsoleHelpers.PrintStep(4, "Wallet prepares multiple presentations");

        Console.WriteLine("Wallet matches credentials to each input descriptor:");
        Console.WriteLine();

        var nonce = $"visa_app_{Guid.NewGuid():N}"[..24];
        var audience = "https://immigration.gov.example";

        // Create holders for each credential
        var govIdHolder = new SdJwtHolder(govIdResult.Issuance);
        var degreeHolder = new SdJwtHolder(degreeResult.Issuance);
        var employmentHolder = new SdJwtHolder(employmentResult.Issuance);

        // Create KB-JWT payload (shared across all presentations in this session)
        var kbPayload = new JwtPayload
        {
            [JwtRegisteredClaimNames.Aud] = audience,
            [JwtRegisteredClaimNames.Iat] = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            ["nonce"] = nonce
        };

        // Create presentations (selective disclosure)
        var govIdPresentation = govIdHolder.CreatePresentation(
            d => d.ClaimName is "given_name" or "family_name",  // Disclose required names
            kbPayload, holderPrivateKey, SecurityAlgorithms.EcdsaSha256);

        var degreePresentation = degreeHolder.CreatePresentation(
            _ => false,  // Don't disclose optional claims (GPA, honors)
            kbPayload, holderPrivateKey, SecurityAlgorithms.EcdsaSha256);

        var employmentPresentation = employmentHolder.CreatePresentation(
            _ => false,  // Don't disclose optional claims (salary, department)
            kbPayload, holderPrivateKey, SecurityAlgorithms.EcdsaSha256);

        Console.WriteLine("  [Match] identity_proof <- National ID");
        Console.WriteLine("          Disclosing: given_name, family_name, nationality");
        Console.WriteLine("          Hidden: birthdate, document_number");
        Console.WriteLine();
        Console.WriteLine("  [Match] education_proof <- University Degree");
        Console.WriteLine("          Disclosing: degree_type, field_of_study, graduation_date");
        Console.WriteLine("          Hidden: gpa, honors");
        Console.WriteLine();
        Console.WriteLine("  [Match] employment_proof <- Employment Letter");
        Console.WriteLine("          Disclosing: job_title, employment_type, start_date");
        Console.WriteLine("          Hidden: department, annual_salary");

        // =====================================================================
        // STEP 5: Build multi-credential response
        // =====================================================================
        ConsoleHelpers.PrintStep(5, "Build multi-credential response");

        Console.WriteLine("VP tokens sent as array when multiple credentials:");
        Console.WriteLine();
        Console.WriteLine("  {");
        Console.WriteLine("    \"vp_token\": [");
        Console.WriteLine("      \"<National ID presentation>\",");
        Console.WriteLine("      \"<University Degree presentation>\",");
        Console.WriteLine("      \"<Employment Letter presentation>\"");
        Console.WriteLine("    ],");
        Console.WriteLine("    \"presentation_submission\": {");
        Console.WriteLine("      \"definition_id\": \"visa-application-2024\",");
        Console.WriteLine("      \"descriptor_map\": [");
        Console.WriteLine("        { \"id\": \"identity_proof\", \"path\": \"$[0]\" },");
        Console.WriteLine("        { \"id\": \"education_proof\", \"path\": \"$[1]\" },");
        Console.WriteLine("        { \"id\": \"employment_proof\", \"path\": \"$[2]\" }");
        Console.WriteLine("      ]");
        Console.WriteLine("    }");
        Console.WriteLine("  }");

        var vpTokens = new[] { govIdPresentation, degreePresentation, employmentPresentation };

        // =====================================================================
        // STEP 6: Verifier validates all credentials
        // =====================================================================
        ConsoleHelpers.PrintStep(6, "Verifier validates all credentials");

        // Create key resolver that handles multiple issuers
        Task<SecurityKey> MultiIssuerKeyResolver(JwtSecurityToken token)
        {
            var issuer = token.Issuer;
            return issuer switch
            {
                "https://id.gov.example" => Task.FromResult<SecurityKey>(govKey),
                "https://degree.university.example" => Task.FromResult<SecurityKey>(universityKey),
                "https://hr.techcorp.example" => Task.FromResult<SecurityKey>(employerKey),
                _ => throw new InvalidOperationException($"Unknown issuer: {issuer}")
            };
        }

        var verifier = new SdVerifier(MultiIssuerKeyResolver);
        var validationParams = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuers = new[]
            {
                "https://id.gov.example",
                "https://degree.university.example",
                "https://hr.techcorp.example"
            },
            ValidateAudience = false,
            ValidateLifetime = true
        };

        Console.WriteLine("Verifying each credential in the response:");
        Console.WriteLine();

        var allValid = true;
        var credentialNames = new[] { "National ID", "University Degree", "Employment Letter" };

        for (int i = 0; i < vpTokens.Length; i++)
        {
            var result = await verifier.VerifyAsync(vpTokens[i], validationParams);
            if (result.ClaimsPrincipal != null)
            {
                ConsoleHelpers.PrintSuccess($"  [{i + 1}] {credentialNames[i]}: VERIFIED");
            }
            else
            {
                Console.WriteLine($"  [{i + 1}] {credentialNames[i]}: FAILED");
                allValid = false;
            }
        }

        Console.WriteLine();
        if (allValid)
        {
            Console.WriteLine("  All credentials verified - visa application can proceed.");
        }

        // =====================================================================
        // STEP 7: Cross-credential analysis
        // =====================================================================
        ConsoleHelpers.PrintStep(7, "Cross-credential consistency checks");

        Console.WriteLine("Advanced verification may check consistency across credentials:");
        Console.WriteLine();
        Console.WriteLine("  Checks the verifier might perform:");
        Console.WriteLine("    - Subject (sub) matches across all credentials");
        Console.WriteLine("    - Names match between ID and other credentials");
        Console.WriteLine("    - Timeline consistency (graduation before employment start)");
        Console.WriteLine("    - No credential expired");
        Console.WriteLine();
        Console.WriteLine("  // Example consistency check");
        Console.WriteLine("  if (govIdSubject != degreeSubject)");
        Console.WriteLine("      throw new Exception(\"Subject mismatch detected\");");

        // =====================================================================
        // COMPLETION
        // =====================================================================
        ConsoleHelpers.PrintCompletion("Tutorial 03: Multi-Credential Flows", new[]
        {
            "Populated wallet with multiple credentials from different issuers",
            "Created presentation definition requiring multiple credentials",
            "Built selective presentations for each credential",
            "Structured multi-credential VP response",
            "Verified all credentials with multi-issuer key resolution"
        });

        Console.WriteLine();
        Console.WriteLine("NEXT: Tutorial 04 - Key Rotation and Lifecycle Management");
    }
}
