using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.Holder;
using SdJwt.Net.Issuer;
using SdJwt.Net.Models;
using SdJwt.Net.PresentationExchange;
using SdJwt.Net.PresentationExchange.Models;
using SdJwt.Net.Samples.Shared;
using SdJwt.Net.Vc.Issuer;
using SdJwt.Net.Vc.Models;
using System.Security.Cryptography;
using System.Text.Json;

namespace SdJwt.Net.Samples.Intermediate;

/// <summary>
/// Tutorial 05: Presentation Exchange (DIF PEX)
///
/// LEARNING OBJECTIVES:
/// - Understand presentation definitions
/// - Create input descriptors with constraints
/// - Match credentials to requirements
/// - Generate presentation submissions
///
/// TIME: ~15 minutes
/// </summary>
public static class PresentationExchangeTutorial
{
    public static Task Run()
    {
        ConsoleHelpers.PrintHeader("Tutorial 05: Presentation Exchange (DIF PEX)");

        Console.WriteLine("DIF Presentation Exchange (PEX) defines a declarative format for");
        Console.WriteLine("verifiers to express credential requirements and for wallets to");
        Console.WriteLine("match and present credentials.");
        Console.WriteLine();

        // Setup - create credentials in wallet
        using var issuerEcdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        using var holderEcdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);

        var issuerKey = new ECDsaSecurityKey(issuerEcdsa) { KeyId = "issuer-2024" };
        var holderJwk = JsonWebKeyConverter.ConvertFromSecurityKey(
            new ECDsaSecurityKey(holderEcdsa) { KeyId = "alice-wallet" });

        // =====================================================================
        // STEP 1: Why Presentation Exchange?
        // =====================================================================
        ConsoleHelpers.PrintStep(1, "Why Presentation Exchange?");

        Console.WriteLine("Problem: Verifiers need flexible ways to request credentials.");
        Console.WriteLine();
        Console.WriteLine("  Simple case: \"Show me your driver's license\"");
        Console.WriteLine();
        Console.WriteLine("  Complex cases:");
        Console.WriteLine("    - \"Show driver's license OR passport OR national ID\"");
        Console.WriteLine("    - \"Must include address field\"");
        Console.WriteLine("    - \"From issuer in approved list\"");
        Console.WriteLine("    - \"Not expired before 2025\"");
        Console.WriteLine();
        Console.WriteLine("PEX provides a standard JSON format for these requirements.");

        // =====================================================================
        // STEP 2: Presentation Definition structure
        // =====================================================================
        ConsoleHelpers.PrintStep(2, "Presentation Definition structure");

        Console.WriteLine("A presentation_definition has this structure:");
        Console.WriteLine();
        Console.WriteLine("  {");
        Console.WriteLine("    \"id\": \"unique-request-id\",");
        Console.WriteLine("    \"name\": \"Human readable name\",");
        Console.WriteLine("    \"purpose\": \"Why credentials are needed\",");
        Console.WriteLine("    \"input_descriptors\": [");
        Console.WriteLine("      // List of credential requirements");
        Console.WriteLine("    ]");
        Console.WriteLine("  }");
        Console.WriteLine();
        Console.WriteLine("Each input_descriptor specifies ONE credential requirement.");

        // =====================================================================
        // STEP 3: Create a simple presentation definition
        // =====================================================================
        ConsoleHelpers.PrintStep(3, "Create a simple presentation definition");

        Console.WriteLine("Scenario: Job application requires degree verification.");

        var simpleDefinition = new PresentationDefinition
        {
            Id = "job-application-degree",
            Name = "Degree Verification",
            Purpose = "Verify educational qualifications for Senior Engineer position",
            InputDescriptors = new[]
            {
                new InputDescriptor
                {
                    Id = "degree_credential",
                    Name = "University Degree",
                    Purpose = "Confirm completion of relevant degree program",
                    Constraints = new Constraints
                    {
                        Fields = new[]
                        {
                            new Field
                            {
                                Path = new[] { "$.vct" },
                                Filter = new FieldFilter
                                {
                                    Type = "string",
                                    Pattern = ".*UniversityDegree.*"
                                }
                            },
                            new Field
                            {
                                Path = new[] { "$.degree" },
                                Filter = new FieldFilter { Type = "string" }
                            },
                            new Field
                            {
                                Path = new[] { "$.major" }
                            }
                        }
                    }
                }
            }
        };

        Console.WriteLine();
        Console.WriteLine("Definition created:");
        Console.WriteLine($"  id: {simpleDefinition.Id}");
        Console.WriteLine($"  purpose: {simpleDefinition.Purpose}");
        Console.WriteLine("  input_descriptors[0]:");
        Console.WriteLine("    - Credential type: UniversityDegree");
        Console.WriteLine("    - Required fields: vct, degree, major");

        // =====================================================================
        // STEP 4: Field constraints and filters
        // =====================================================================
        ConsoleHelpers.PrintStep(4, "Field constraints and filters");

        Console.WriteLine("Fields use JSONPath to locate claims, with optional filters:");
        Console.WriteLine();
        Console.WriteLine("  PATH EXAMPLES:");
        Console.WriteLine("    $.degree           - Top-level 'degree' claim");
        Console.WriteLine("    $.address.city     - Nested 'city' in 'address'");
        Console.WriteLine("    $['job-title']     - Claim name with special chars");
        Console.WriteLine();
        Console.WriteLine("  FILTER TYPES:");
        Console.WriteLine("    type: string       - Must be a string");
        Console.WriteLine("    type: number       - Must be a number");
        Console.WriteLine("    pattern: regex     - Must match regex pattern");
        Console.WriteLine("    minimum: 18        - Number must be >= 18");
        Console.WriteLine("    enum: [\"A\",\"B\"]    - Must be one of values");
        Console.WriteLine("    const: \"specific\"  - Must equal exact value");

        // =====================================================================
        // STEP 5: Complex definition with alternatives
        // =====================================================================
        ConsoleHelpers.PrintStep(5, "Complex definition with alternatives");

        Console.WriteLine("Use 'submission_requirements' for OR logic:");

        var complexDefinition = new PresentationDefinition
        {
            Id = "age-verification",
            Name = "Age Verification",
            Purpose = "Verify user is 21 or older",
            InputDescriptors = new[]
            {
                new InputDescriptor
                {
                    Id = "drivers_license",
                    Name = "Driver's License",
                    Group = new[] { "identity_docs" },
                    Constraints = new Constraints
                    {
                        Fields = new[]
                        {
                            new Field { Path = new[] { "$.vct" }, Filter = new FieldFilter { Pattern = ".*DriversLicense.*" } },
                            new Field { Path = new[] { "$.date_of_birth" } }
                        }
                    }
                },
                new InputDescriptor
                {
                    Id = "passport",
                    Name = "Passport",
                    Group = new[] { "identity_docs" },
                    Constraints = new Constraints
                    {
                        Fields = new[]
                        {
                            new Field { Path = new[] { "$.vct" }, Filter = new FieldFilter { Pattern = ".*Passport.*" } },
                            new Field { Path = new[] { "$.birthdate" } }
                        }
                    }
                },
                new InputDescriptor
                {
                    Id = "national_id",
                    Name = "National ID",
                    Group = new[] { "identity_docs" },
                    Constraints = new Constraints
                    {
                        Fields = new[]
                        {
                            new Field { Path = new[] { "$.vct" }, Filter = new FieldFilter { Pattern = ".*NationalID.*" } },
                            new Field { Path = new[] { "$.birth_date" } }
                        }
                    }
                }
            },
            SubmissionRequirements = new[]
            {
                new SubmissionRequirement
                {
                    Rule = "pick",
                    Count = 1,
                    From = "identity_docs"
                }
            }
        };

        Console.WriteLine();
        Console.WriteLine("Definition allows ANY ONE of:");
        Console.WriteLine("  - Driver's License with date_of_birth");
        Console.WriteLine("  - Passport with birthdate");
        Console.WriteLine("  - National ID with birth_date");
        Console.WriteLine();
        Console.WriteLine("submission_requirements:");
        Console.WriteLine("  rule: pick");
        Console.WriteLine("  count: 1");
        Console.WriteLine("  from: identity_docs  (group containing all three)");

        // =====================================================================
        // STEP 6: Wallet matches credentials
        // =====================================================================
        ConsoleHelpers.PrintStep(6, "Wallet matches credentials to requirements");

        // Create sample credentials in wallet
        var vcIssuer = new SdJwtVcIssuer(issuerKey, SecurityAlgorithms.EcdsaSha256);

        var degreeCredential = vcIssuer.Issue(
            "https://credentials.example.edu/UniversityDegree",
            new SdJwtVcPayload
            {
                Issuer = "https://university.example.edu",
                Subject = "alice",
                IssuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                AdditionalData = new Dictionary<string, object>
                {
                    ["degree"] = "Bachelor of Science",
                    ["major"] = "Computer Science",
                    ["graduation_date"] = "2024-06-15"
                }
            },
            new SdIssuanceOptions { DisclosureStructure = new { graduation_date = true } },
            holderJwk);

        Console.WriteLine("Wallet contents:");
        Console.WriteLine("  1. University Degree (vct: UniversityDegree)");
        Console.WriteLine("     - degree: Bachelor of Science");
        Console.WriteLine("     - major: Computer Science");
        Console.WriteLine();

        Console.WriteLine("Matching against 'job-application-degree' definition...");
        Console.WriteLine();
        Console.WriteLine("  Checking input_descriptor 'degree_credential':");
        Console.WriteLine("    [X] vct matches pattern .*UniversityDegree.*");
        Console.WriteLine("    [X] $.degree exists and is string");
        Console.WriteLine("    [X] $.major exists");
        Console.WriteLine("  Result: MATCH");

        // =====================================================================
        // STEP 7: Create presentation submission
        // =====================================================================
        ConsoleHelpers.PrintStep(7, "Create presentation submission");

        Console.WriteLine("The wallet creates a presentation_submission mapping:");
        Console.WriteLine();

        var submission = new PresentationSubmission
        {
            Id = $"submission_{Guid.NewGuid():N}"[..24],
            DefinitionId = simpleDefinition.Id,
            DescriptorMap = new[]
            {
                new InputDescriptorMapping
                {
                    Id = "degree_credential",
                    Format = "vc+sd-jwt",
                    Path = "$"
                }
            }
        };

        Console.WriteLine("presentation_submission:");
        Console.WriteLine($"  id: {submission.Id}");
        Console.WriteLine($"  definition_id: {submission.DefinitionId}");
        Console.WriteLine("  descriptor_map:");
        Console.WriteLine("    - id: degree_credential");
        Console.WriteLine("      format: vc+sd-jwt");
        Console.WriteLine("      path: $  (root of vp_token)");
        Console.WriteLine();
        Console.WriteLine("This tells the verifier:");
        Console.WriteLine("  \"The credential at path '$' in vp_token satisfies");
        Console.WriteLine("   the 'degree_credential' input descriptor.\"");

        // =====================================================================
        // STEP 8: Limit disclosure
        // =====================================================================
        ConsoleHelpers.PrintStep(8, "Limit disclosure to required fields");

        Console.WriteLine("PEX 'limit_disclosure' controls what's revealed:");
        Console.WriteLine();
        Console.WriteLine("  constraints: {");
        Console.WriteLine("    limit_disclosure: \"required\",");
        Console.WriteLine("    fields: [...]");
        Console.WriteLine("  }");
        Console.WriteLine();
        Console.WriteLine("  VALUES:");
        Console.WriteLine("    \"required\" - Only disclose fields in constraints");
        Console.WriteLine("    \"preferred\" - Prefer minimal disclosure if supported");
        Console.WriteLine("    (absent)   - Disclose whatever holder wants");
        Console.WriteLine();
        Console.WriteLine("Example: If definition requests only 'degree' and 'major',");
        Console.WriteLine("wallet should NOT disclose graduation_date or GPA.");

        // =====================================================================
        // STEP 9: Using PEX with OID4VP
        // =====================================================================
        ConsoleHelpers.PrintStep(9, "Integration with OpenID4VP");

        Console.WriteLine("PEX is embedded in OID4VP authorization requests:");
        Console.WriteLine();
        Console.WriteLine("  GET /authorize?");
        Console.WriteLine("    response_type=vp_token&");
        Console.WriteLine("    client_id=verifier.example.com&");
        Console.WriteLine("    nonce=xyz123&");
        Console.WriteLine("    presentation_definition={...}&  // PEX definition");
        Console.WriteLine("    ...");
        Console.WriteLine();
        Console.WriteLine("Response includes both vp_token and presentation_submission:");
        Console.WriteLine();
        Console.WriteLine("  {");
        Console.WriteLine("    \"vp_token\": \"<SD-JWT presentation>\",");
        Console.WriteLine("    \"presentation_submission\": {");
        Console.WriteLine("      \"definition_id\": \"job-application-degree\",");
        Console.WriteLine("      \"descriptor_map\": [...]");
        Console.WriteLine("    }");
        Console.WriteLine("  }");

        // =====================================================================
        // COMPLETION
        // =====================================================================
        ConsoleHelpers.PrintCompletion("Tutorial 05: Presentation Exchange", new[]
        {
            "Understood presentation definition structure",
            "Created input descriptors with constraints",
            "Learned field paths and filters",
            "Built complex definitions with alternatives",
            "Integrated PEX with OID4VP flow"
        });

        Console.WriteLine();
        Console.WriteLine("CONGRATULATIONS! You've completed the Intermediate Tutorials!");
        Console.WriteLine();
        Console.WriteLine("NEXT STEPS:");
        Console.WriteLine("  - 03-Advanced: OpenID Federation and HAIP compliance");
        Console.WriteLine("  - 04-UseCases: Production patterns by industry");

        return Task.CompletedTask;
    }
}
