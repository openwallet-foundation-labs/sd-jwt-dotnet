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

namespace SdJwt.Net.Samples.UseCases.Education;

/// <summary>
/// University Degree Credential
///
/// SCENARIO: Graduate presents degree for job application
/// - Employer verifies degree type and field without seeing GPA
/// - Verification happens without contacting university
/// - Graduate controls what academic details are revealed
///
/// CLAIMS REVEALED: degree_type, field_of_study, institution_name, graduation_date
/// CLAIMS HIDDEN: gpa, honors, student_id, courses_completed
/// </summary>
public static class UniversityDegree
{
    public static async Task Run()
    {
        ConsoleHelpers.PrintHeader("Use Case: University Degree Verification");

        Console.WriteLine("SCENARIO: Alice applies for a software engineering position.");
        Console.WriteLine("The employer needs to verify her CS degree without accessing");
        Console.WriteLine("her detailed academic record (GPA, specific courses).");
        Console.WriteLine();

        // =========================================================================
        // SETUP: University and Employer Infrastructure
        // =========================================================================
        using var universityEcdsa = ECDsa.Create(ECCurve.NamedCurves.nistP384);
        using var holderEcdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);

        var universityKey = new ECDsaSecurityKey(universityEcdsa) { KeyId = "mit-registrar-2024" };
        var holderPrivateKey = new ECDsaSecurityKey(holderEcdsa) { KeyId = "alice-wallet" };
        var holderJwk = JsonWebKeyConverter.ConvertFromSecurityKey(holderPrivateKey);

        // =========================================================================
        // PHASE 1: University Issues Credential to Graduate
        // =========================================================================
        Console.WriteLine("PHASE 1: Credential Issuance");
        Console.WriteLine(new string('-', 50));

        var issuer = new SdJwtVcIssuer(universityKey, SecurityAlgorithms.EcdsaSha384);

        // Full academic credential with sensitive and non-sensitive claims
        var credentialPayload = new SdJwtVcPayload
        {
            Issuer = "https://registrar.mit.edu",
            Subject = "did:alumni:alice-johnson-2024",
            IssuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            ExpiresAt = DateTimeOffset.UtcNow.AddYears(50).ToUnixTimeSeconds(),
            AdditionalData = new Dictionary<string, object>
            {
                // Always visible claims
                ["institution_name"] = "Massachusetts Institute of Technology",
                ["degree_type"] = "Bachelor of Science",
                ["field_of_study"] = "Computer Science and Engineering",
                ["graduation_date"] = "2024-06-07",
                ["credential_number"] = "MIT-2024-CS-1234",

                // Sensitive claims (selectively disclosable)
                ["gpa"] = 3.92,
                ["honors"] = "Cum Laude",
                ["student_id"] = "MIT-900123456",
                ["thesis_title"] = "Efficient Zero-Knowledge Proofs for Credential Systems",
                ["advisor_name"] = "Prof. Shafi Goldwasser",
                ["courses_completed"] = new[]
                {
                    new { code = "6.857", name = "Network and Computer Security", grade = "A" },
                    new { code = "6.858", name = "Computer Systems Security", grade = "A" },
                    new { code = "6.046", name = "Design and Analysis of Algorithms", grade = "A-" }
                }
            }
        };

        // Define which claims are selectively disclosable
        var sdOptions = new SdIssuanceOptions
        {
            DisclosureStructure = new
            {
                gpa = true,
                honors = true,
                student_id = true,
                thesis_title = true,
                advisor_name = true,
                courses_completed = true
            }
        };

        var credential = issuer.Issue(
            "https://credentials.mit.edu/UniversityDegree",
            credentialPayload, sdOptions, holderJwk);

        Console.WriteLine("University issued degree credential to Alice:");
        Console.WriteLine($"  Issuer: {credentialPayload.Issuer}");
        Console.WriteLine($"  Degree: {credentialPayload.AdditionalData["degree_type"]}");
        Console.WriteLine($"  Field: {credentialPayload.AdditionalData["field_of_study"]}");
        Console.WriteLine("  Selectively disclosable claims: gpa, honors, student_id,");
        Console.WriteLine("    thesis_title, advisor_name, courses_completed");
        Console.WriteLine();

        // =========================================================================
        // PHASE 2: Employer Creates Verification Request
        // =========================================================================
        Console.WriteLine("PHASE 2: Employer Creates Request");
        Console.WriteLine(new string('-', 50));

        var employerDefinition = new PresentationDefinition
        {
            Id = "software-engineer-education-verification",
            Name = "Education Verification",
            Purpose = "Verify CS degree for Senior Software Engineer position",
            InputDescriptors = new[]
            {
                new InputDescriptor
                {
                    Id = "cs_degree",
                    Name = "Computer Science Degree",
                    Purpose = "Confirm relevant educational background",
                    Constraints = new Constraints
                    {
                        LimitDisclosure = "required",  // Respect minimal disclosure
                        Fields = new[]
                        {
                            new Field
                            {
                                Path = new[] { "$.vct" },
                                Filter = new FieldFilter { Pattern = ".*UniversityDegree.*" }
                            },
                            new Field
                            {
                                Path = new[] { "$.degree_type" },
                                Filter = new FieldFilter { Type = "string" }
                            },
                            new Field
                            {
                                Path = new[] { "$.field_of_study" },
                                Filter = new FieldFilter { Pattern = ".*(Computer Science|Software Engineering).*" }
                            },
                            new Field
                            {
                                Path = new[] { "$.graduation_date" }
                            }
                        }
                    }
                }
            }
        };

        Console.WriteLine($"Employer: {employerDefinition.Name}");
        Console.WriteLine($"Purpose: {employerDefinition.Purpose}");
        Console.WriteLine("Required fields: degree_type, field_of_study, graduation_date");
        Console.WriteLine("NOT required: gpa, honors, student_id, courses");
        Console.WriteLine();

        // =========================================================================
        // PHASE 3: Graduate Creates Minimal Presentation
        // =========================================================================
        Console.WriteLine("PHASE 3: Alice Creates Presentation");
        Console.WriteLine(new string('-', 50));

        var holder = new SdJwtHolder(credential.Issuance);

        var nonce = $"job_app_{Guid.NewGuid():N}"[..24];
        var audience = "https://careers.techcorp.example";

        // Create presentation with NO optional disclosures
        var presentation = holder.CreatePresentation(
            disclosure => false,  // Don't disclose any optional claims
            kbJwtPayload: new JwtPayload
            {
                [JwtRegisteredClaimNames.Aud] = audience,
                [JwtRegisteredClaimNames.Iat] = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                ["nonce"] = nonce
            },
            kbJwtSigningKey: holderPrivateKey,
            kbJwtSigningAlgorithm: SecurityAlgorithms.EcdsaSha256
        );

        Console.WriteLine("Alice's presentation includes:");
        Console.WriteLine("  [REVEALED] institution_name: Massachusetts Institute of Technology");
        Console.WriteLine("  [REVEALED] degree_type: Bachelor of Science");
        Console.WriteLine("  [REVEALED] field_of_study: Computer Science and Engineering");
        Console.WriteLine("  [REVEALED] graduation_date: 2024-06-07");
        Console.WriteLine();
        Console.WriteLine("  [HIDDEN] gpa: ***");
        Console.WriteLine("  [HIDDEN] honors: ***");
        Console.WriteLine("  [HIDDEN] student_id: ***");
        Console.WriteLine("  [HIDDEN] thesis_title: ***");
        Console.WriteLine("  [HIDDEN] courses_completed: ***");
        Console.WriteLine();

        // =========================================================================
        // PHASE 4: Employer Verifies Credential
        // =========================================================================
        Console.WriteLine("PHASE 4: Employer Verifies Presentation");
        Console.WriteLine(new string('-', 50));

        var verifier = new SdVerifier(_ => Task.FromResult<SecurityKey>(universityKey));
        var validationParams = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuers = new[]
            {
                "https://registrar.mit.edu",
                "https://registrar.stanford.edu",
                "https://registrar.berkeley.edu"
            },
            ValidateAudience = false,
            ValidateLifetime = true
        };

        var result = await verifier.VerifyAsync(presentation, validationParams);

        if (result.ClaimsPrincipal != null)
        {
            ConsoleHelpers.PrintSuccess("CREDENTIAL VERIFIED");
            Console.WriteLine();
            Console.WriteLine("  Verified claims:");
            var verifiedClaims = result.ClaimsPrincipal.Claims.ToDictionary(c => c.Type, c => c.Value);
            Console.WriteLine($"    Institution: {verifiedClaims.GetValueOrDefault("institution_name")}");
            Console.WriteLine($"    Degree: {verifiedClaims.GetValueOrDefault("degree_type")}");
            Console.WriteLine($"    Major: {verifiedClaims.GetValueOrDefault("field_of_study")}");
            if (verifiedClaims.TryGetValue("graduation_date", out var graduationDate))
                Console.WriteLine($"    Graduated: {graduationDate}");
            Console.WriteLine();
            Console.WriteLine("  Privacy preserved:");
            Console.WriteLine("    GPA, honors, student ID NOT accessible to employer");
        }
        else
        {
            Console.WriteLine("Verification failed");
        }

        // =========================================================================
        // SUMMARY
        // =========================================================================
        Console.WriteLine();
        Console.WriteLine("USE CASE COMPLETE");
        Console.WriteLine(new string('=', 50));
        Console.WriteLine();
        Console.WriteLine("Key achievements:");
        Console.WriteLine("  1. Employer verified degree without seeing GPA");
        Console.WriteLine("  2. No direct contact with university required");
        Console.WriteLine("  3. Alice controlled exactly what was disclosed");
        Console.WriteLine("  4. Cryptographic proof of authenticity");
        Console.WriteLine("  5. Credential remains valid for 50 years");
    }
}
