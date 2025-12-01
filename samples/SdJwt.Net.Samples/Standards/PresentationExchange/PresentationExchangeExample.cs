using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.Holder;
using SdJwt.Net.Issuer;
using SdJwt.Net.Models;
using SdJwt.Net.Vc.Issuer;
using SdJwt.Net.Vc.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text.Json;
using JsonWebKeyMs = Microsoft.IdentityModel.Tokens.JsonWebKey;

namespace SdJwt.Net.Samples.Standards.PresentationExchange;

/// <summary>
/// Demonstrates DIF Presentation Exchange concepts with working code examples for intelligent credential selection
/// Shows conceptual implementation approaches for intelligent credential selection
/// </summary>
public class PresentationExchangeExample
{
    public static async Task RunExample(IServiceProvider services)
    {
        var logger = services.GetRequiredService<ILogger<PresentationExchangeExample>>();
        
        Console.WriteLine("\n" + new string('=', 65));
        Console.WriteLine("      DIF Presentation Exchange Intelligent Selection   ");
        Console.WriteLine("              (DIF Presentation Exchange 2.1.1)         ");
        Console.WriteLine(new string('=', 65));

        Console.WriteLine("\nDIF Presentation Exchange enables intelligent credential selection");
        Console.WriteLine("for verifiable presentation workflows. This example demonstrates");
        Console.WriteLine("conceptual approaches and implementation patterns.");
        Console.WriteLine();

        // Setup for working demonstrations
        using var issuerEcdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        using var holderEcdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        
        var issuerKey = new ECDsaSecurityKey(issuerEcdsa) { KeyId = "issuer-2024" };
        var holderPrivateKey = new ECDsaSecurityKey(holderEcdsa) { KeyId = "holder-key-1" };
        var holderPublicKey = new ECDsaSecurityKey(holderEcdsa) { KeyId = "holder-key-1" };
        var holderJwk = JsonWebKeyConverter.ConvertFromSecurityKey(holderPublicKey);

        // Create wallet with multiple credentials
        var credentialWallet = await CreateSampleWallet(issuerKey, holderJwk);

        await DemonstrateBasicCredentialSelection(credentialWallet);
        await DemonstrateComplexRequirements(credentialWallet);
        await DemonstrateEducationVerification(credentialWallet);
        await DemonstrateGovernmentServiceAccess(credentialWallet);
        await DemonstrateEmploymentBackground(credentialWallet);

        Console.WriteLine("\n" + new string('=', 65));
        Console.WriteLine("      Presentation Exchange concepts demonstrated!      ");
        Console.WriteLine("                                                         ");
        Console.WriteLine("  [X] Intelligent credential selection                    ");
        Console.WriteLine("  [X] Complex requirement handling                        ");
        Console.WriteLine("  [X] Educational verification                            ");
        Console.WriteLine("  [X] Government service access                           ");
        Console.WriteLine("  [X] Employment background checks                        ");
        Console.WriteLine(new string('=', 65));
        return;
    }

    private static async Task<object[]> CreateSampleWallet(SecurityKey issuerKey, JsonWebKeyMs holderJwk)
    {
        Console.WriteLine("   Setting up sample wallet with multiple credentials...");
        
        var vcIssuer = new SdJwtVcIssuer(issuerKey, SecurityAlgorithms.EcdsaSha256);
        var wallet = new List<object>();

        // Driver's License
        var driverLicense = vcIssuer.Issue(
            "https://credentials.dmv.ca.gov/drivers-license",
            new SdJwtVcPayload
            {
                Issuer = "https://dmv.california.gov",
                Subject = "did:example:person123",
                IssuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                ExpiresAt = DateTimeOffset.UtcNow.AddYears(5).ToUnixTimeSeconds(),
                AdditionalData = new Dictionary<string, object>
                {
                    ["full_name"] = "Alice Johnson",
                    ["birth_date"] = "1990-05-15",
                    ["age_over_18"] = true,
                    ["age_over_21"] = true,
                    ["license_class"] = "Class C"
                }
            },
            new SdIssuanceOptions { DisclosureStructure = new { age_over_21 = true, age_over_18 = true } },
            holderJwk
        );

        // University Degree
        var degree = vcIssuer.Issue(
            "https://credentials.stanford.edu/degree",
            new SdJwtVcPayload
            {
                Issuer = "https://registrar.stanford.edu",
                Subject = "did:example:graduate789",
                IssuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                AdditionalData = new Dictionary<string, object>
                {
                    ["student_name"] = "Alice Johnson",
                    ["degree"] = "Bachelor of Science",
                    ["major"] = "Computer Science",
                    ["graduation_year"] = 2020,
                    ["gpa"] = 3.8
                }
            },
            new SdIssuanceOptions { DisclosureStructure = new { gpa = true, major = true } },
            holderJwk
        );

        // Professional License
        var professionalLicense = vcIssuer.Issue(
            "https://credentials.engineering-board.ca.gov/license",
            new SdJwtVcPayload
            {
                Issuer = "https://engineering-board.california.gov",
                Subject = "did:example:engineer123",
                IssuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                ExpiresAt = DateTimeOffset.UtcNow.AddYears(3).ToUnixTimeSeconds(),
                AdditionalData = new Dictionary<string, object>
                {
                    ["licensee_name"] = "Alice Johnson",
                    ["license_type"] = "Professional Engineer",
                    ["specialization"] = "Software Engineering",
                    ["license_number"] = "PE-12345"
                }
            },
            new SdIssuanceOptions { DisclosureStructure = new { specialization = true } },
            holderJwk
        );

        // Employment Credential
        var employment = vcIssuer.Issue(
            "https://credentials.techcorp.com/employment",
            new SdJwtVcPayload
            {
                Issuer = "https://hr.techcorp.example.com",
                Subject = "did:example:employee456",
                IssuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                ExpiresAt = DateTimeOffset.UtcNow.AddYears(2).ToUnixTimeSeconds(),
                AdditionalData = new Dictionary<string, object>
                {
                    ["employee_name"] = "Alice Johnson",
                    ["position"] = "Senior Software Engineer",
                    ["department"] = "Engineering",
                    ["security_clearance"] = "Secret",
                    ["start_date"] = "2021-01-15"
                }
            },
            new SdIssuanceOptions { DisclosureStructure = new { security_clearance = true, position = true } },
            holderJwk
        );

        // Convert to wallet format (simplified - in real implementation these would be stored/managed properly)
        wallet.Add(ParseCredentialForWallet(driverLicense.Issuance, "drivers-license"));
        wallet.Add(ParseCredentialForWallet(degree.Issuance, "university-degree"));
        wallet.Add(ParseCredentialForWallet(professionalLicense.Issuance, "professional-license"));
        wallet.Add(ParseCredentialForWallet(employment.Issuance, "employment"));

        Console.WriteLine($"   [X] Wallet created with {wallet.Count} credentials");
        Console.WriteLine();

        return wallet.ToArray();
    }

    private static object ParseCredentialForWallet(string sdJwtCredential, string type)
    {
        // In a real implementation, this would properly parse the SD-JWT and create a wallet entry
        // For demonstration purposes, we'll create a simplified object that contains the key information
        
        var parts = sdJwtCredential.Split('~');
        var jwtPart = parts[0];
        
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(jwtPart);
        
        // Create a simplified wallet entry
        var walletEntry = new Dictionary<string, object>
        {
            ["credential_type"] = type,
            ["original_credential"] = sdJwtCredential,
            ["issuer"] = jwt.Claims.FirstOrDefault(c => c.Type == "iss")?.Value ?? "unknown",
            ["vct"] = jwt.Claims.FirstOrDefault(c => c.Type == "vct")?.Value ?? "unknown",
            ["subject"] = jwt.Claims.FirstOrDefault(c => c.Type == "sub")?.Value ?? "unknown"
        };

        // Add other claims for matching
        foreach (var claim in jwt.Claims.Where(c => !new[] { "iss", "sub", "iat", "exp", "vct" }.Contains(c.Type)))
        {
            walletEntry[claim.Type] = claim.Value;
        }

        return walletEntry;
    }

    private static Task DemonstrateBasicCredentialSelection(object[] wallet)
    {
        Console.WriteLine("\n1. BASIC CREDENTIAL SELECTION - CONCEPTUAL IMPLEMENTATION");
        Console.WriteLine("   Age verification for online service");
        Console.WriteLine();

        try
        {
            // Simplified presentation definition structure
            var presentationDefinition = new
            {
                Id = "age_verification",
                Purpose = "Verify you are 21 years or older",
                InputDescriptors = new[]
                {
                    new
                    {
                        Id = "government_id",
                        Constraints = new
                        {
                            Fields = new[]
                            {
                                new
                                {
                                    Path = new[] { "$.age_over_21" },
                                    Filter = new { type = "boolean", @const = true }
                                }
                            }
                        }
                    }
                }
            };

            Console.WriteLine("   Presentation Definition:");
            Console.WriteLine($"   • Purpose: {presentationDefinition.Purpose}");
            Console.WriteLine($"   • Required field: age_over_21 = true");
            Console.WriteLine();

            // Simulate credential selection logic
            Console.WriteLine("   Credential Selection Process:");
            
            var matchingCredentials = new List<object>();
            foreach (var credential in wallet)
            {
                var credDict = credential as Dictionary<string, object>;
                if (credDict != null && credDict.ContainsKey("age_over_21"))
                {
                    var ageValue = credDict["age_over_21"];
                    if (ageValue is bool ageFlag && ageFlag)
                    {
                        matchingCredentials.Add(credential);
                        Console.WriteLine($"   [X] Found matching credential: {credDict["credential_type"]}");
                    }
                }
            }

            if (matchingCredentials.Any())
            {
                var selectedCredential = matchingCredentials.First();
                var selectedDict = selectedCredential as Dictionary<string, object>;
                
                Console.WriteLine();
                Console.WriteLine("   Selection Result:");
                Console.WriteLine($"   [X] Selected: {selectedDict?["credential_type"]}");
                Console.WriteLine($"   [X] Issuer: {selectedDict?["issuer"]}");
                Console.WriteLine("   [X] Age requirement satisfied with minimal disclosure");
                Console.WriteLine("   [X] Other personal information protected");
            }
            else
            {
                Console.WriteLine("   [X] No matching credentials found");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"   [X] Selection error: {ex.Message}");
        }

        return Task.CompletedTask;
    }

    private static Task DemonstrateComplexRequirements(object[] wallet)
    {
        Console.WriteLine("\n2. COMPLEX REQUIREMENTS - MULTI-CREDENTIAL SELECTION");
        Console.WriteLine("   Bank loan requiring ID + Employment + Education");
        Console.WriteLine();

        try
        {
            // Simplified multi-requirement structure
            var presentationDefinition = new
            {
                Id = "bank_loan_verification",
                Purpose = "Comprehensive verification for loan application",
                SubmissionRequirements = new object[]
                {
                    new { Name = "Identity Verification", Rule = "all", Groups = new[] { "identity" } },
                    new { Name = "Financial Verification", Rule = "pick", Count = 1, Groups = new[] { "financial" } },
                    new { Name = "Education Verification", Rule = "pick", Count = 1, Groups = new[] { "education" } }
                },
                InputDescriptors = new object[]
                {
                    new
                    {
                        Id = "government_id",
                        Groups = new[] { "identity" },
                        RequiredFields = new[] { "full_name" }
                    },
                    new
                    {
                        Id = "employment_verification",
                        Groups = new[] { "financial" },
                        RequiredFields = new[] { "position", "start_date" }
                    },
                    new
                    {
                        Id = "education_verification",
                        Groups = new[] { "education" },
                        RequiredFields = new[] { "degree", "graduation_year" }
                    }
                }
            };

            Console.WriteLine("   Multi-Credential Requirements:");
            Console.WriteLine("   Document Identity (government ID)");
            Console.WriteLine("   Briefcase Employment verification");
            Console.WriteLine("   Graduation Cap Education verification");
            Console.WriteLine();

            // Simulate intelligent selection
            var selectedCredentials = new Dictionary<string, object>();

            // Select identity credential
            var identityCredential = wallet.FirstOrDefault(c =>
            {
                var dict = c as Dictionary<string, object>;
                return dict?.ContainsKey("full_name") == true && 
                       dict.ContainsKey("license_class");
            });

            if (identityCredential != null)
            {
                selectedCredentials["identity"] = identityCredential;
                var dict = identityCredential as Dictionary<string, object>;
                Console.WriteLine($"   [X] Identity: {dict?["credential_type"]} selected");
            }

            // Select employment credential
            var employmentCredential = wallet.FirstOrDefault(c =>
            {
                var dict = c as Dictionary<string, object>;
                return dict?.ContainsKey("position") == true && 
                       dict.ContainsKey("start_date");
            });

            if (employmentCredential != null)
            {
                selectedCredentials["employment"] = employmentCredential;
                var dict = employmentCredential as Dictionary<string, object>;
                Console.WriteLine($"   [X] Employment: {dict?["credential_type"]} selected");
            }

            // Select education credential
            var educationCredential = wallet.FirstOrDefault(c =>
            {
                var dict = c as Dictionary<string, object>;
                return dict?.ContainsKey("degree") == true && 
                       dict.ContainsKey("graduation_year");
            });

            if (educationCredential != null)
            {
                selectedCredentials["education"] = educationCredential;
                var dict = educationCredential as Dictionary<string, object>;
                Console.WriteLine($"   [X] Education: {dict?["credential_type"]} selected");
            }

            Console.WriteLine();
            Console.WriteLine("   Selection Summary:");
            Console.WriteLine($"   [X] {selectedCredentials.Count}/3 requirements satisfied");
            Console.WriteLine("   [X] Optimal combination selected automatically");
            Console.WriteLine("   [X] Minimal disclosure strategy applied");
            Console.WriteLine("   [X] User approval required before submission");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"   [X] Complex selection error: {ex.Message}");
        }

        return Task.CompletedTask;
    }

    private static Task DemonstrateEducationVerification(object[] wallet)
    {
        Console.WriteLine("\n3. EDUCATION VERIFICATION - CONSTRAINT MATCHING");
        Console.WriteLine("   Graduate program requiring specific qualifications");
        Console.WriteLine();

        try
        {
            // Simplified constraint structure
            var presentationDefinition = new
            {
                Id = "graduate_admission",
                Purpose = "Verify qualifications for graduate program admission",
                Requirements = new
                {
                    DegreePattern = ".*Bachelor.*",
                    MajorPattern = ".*(Computer Science|Engineering|Mathematics).*",
                    MinimumGpa = 3.0
                }
            };

            Console.WriteLine("   Admission Requirements:");
            Console.WriteLine("   • Degree: Bachelor's degree (required)");
            Console.WriteLine("   • Field: Technical field (CS, Engineering, Math)");
            Console.WriteLine("   • GPA: Minimum 3.0 (optional disclosure)");
            Console.WriteLine();

            // Find matching credentials
            var matchingEducationCredentials = new List<object>();
            
            foreach (var credential in wallet)
            {
                var dict = credential as Dictionary<string, object>;
                if (dict != null)
                {
                    // Check if it has degree information
                    if (dict.ContainsKey("degree") && dict.ContainsKey("major"))
                    {
                        var degree = dict["degree"]?.ToString() ?? "";
                        var major = dict["major"]?.ToString() ?? "";
                        
                        // Check degree requirement
                        if (degree.Contains("Bachelor"))
                        {
                            // Check major requirement
                            if (major.Contains("Computer Science") || 
                                major.Contains("Engineering") || 
                                major.Contains("Mathematics"))
                            {
                                matchingEducationCredentials.Add(credential);
                                Console.WriteLine($"   [X] Qualifying credential found: {dict["credential_type"]}");
                                Console.WriteLine($"     - Degree: {degree}");
                                Console.WriteLine($"     - Major: {major}");
                                
                                // Check GPA if available
                                if (dict.ContainsKey("gpa") && dict["gpa"] is double gpaValue)
                                {
                                    if (gpaValue >= 3.0)
                                    {
                                        Console.WriteLine($"     - GPA: {gpaValue} (exceeds minimum)");
                                    }
                                    else
                                    {
                                        Console.WriteLine($"     - GPA: {gpaValue} (below minimum - but optional)");
                                    }
                                }
                                else
                                {
                                    Console.WriteLine($"     - GPA: Not disclosed (optional)");
                                }
                            }
                        }
                    }
                }
            }

            Console.WriteLine();
            
            if (matchingEducationCredentials.Any())
            {
                Console.WriteLine("   Admission Decision:");
                Console.WriteLine("   Check QUALIFIED for graduate program");
                Console.WriteLine("   [X] Degree requirement satisfied");
                Console.WriteLine("   [X] Technical field requirement satisfied");
                Console.WriteLine("   [X] Student can choose whether to disclose GPA");
            }
            else
            {
                Console.WriteLine("   Admission Decision:");
                Console.WriteLine("   X NOT QUALIFIED");
                Console.WriteLine("   [X] No qualifying educational credentials found");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"   [X] Education verification error: {ex.Message}");
        }

        return Task.CompletedTask;
    }

    private static Task DemonstrateGovernmentServiceAccess(object[] wallet)
    {
        Console.WriteLine("\n4. GOVERNMENT SERVICE ACCESS - PREFERENCE-BASED SELECTION");
        Console.WriteLine("   Multiple acceptable ID types with government preferences");
        Console.WriteLine();

        try
        {
            Console.WriteLine("   Government Preference Order:");
            Console.WriteLine("   1. Driver's License (preferred - state issued)");
            Console.WriteLine("   2. Professional License (alternative - state board)");
            Console.WriteLine("   3. Employment Credential (fallback - if security cleared)");
            Console.WriteLine();

            // Simulate preference-based selection
            object? selectedCredential = null;
            string selectionReason = "";

            // First preference: Driver's License
            selectedCredential = wallet.FirstOrDefault(c =>
            {
                var dict = c as Dictionary<string, object>;
                return dict?["credential_type"]?.ToString() == "drivers-license";
            });

            if (selectedCredential != null)
            {
                selectionReason = "Government-issued driver's license (highest preference)";
            }
            else
            {
                // Second preference: Professional License
                selectedCredential = wallet.FirstOrDefault(c =>
                {
                    var dict = c as Dictionary<string, object>;
                    return dict?["credential_type"]?.ToString() == "professional-license";
                });

                if (selectedCredential != null)
                {
                    selectionReason = "State professional license (second preference)";
                }
                else
                {
                    // Third preference: Employment with security clearance
                    selectedCredential = wallet.FirstOrDefault(c =>
                    {
                        var dict = c as Dictionary<string, object>;
                        return dict?["credential_type"]?.ToString() == "employment" &&
                               dict.ContainsKey("security_clearance");
                    });

                    if (selectedCredential != null)
                    {
                        selectionReason = "Employment credential with security clearance (fallback)";
                    }
                }
            }

            if (selectedCredential != null)
            {
                var dict = selectedCredential as Dictionary<string, object>;
                Console.WriteLine("   Selection Result:");
                Console.WriteLine($"   [X] Selected: {dict?["credential_type"]}");
                Console.WriteLine($"   [X] Reason: {selectionReason}");
                Console.WriteLine($"   [X] Issuer: {dict?["issuer"]}");
                Console.WriteLine("   [X] Government service access: GRANTED");
                Console.WriteLine();
                
                Console.WriteLine("   Benefits:");
                Console.WriteLine("   [X] Automatic preference-based selection");
                Console.WriteLine("   [X] Fallback options for inclusivity");
                Console.WriteLine("   [X] Government trust optimization");
            }
            else
            {
                Console.WriteLine("   Selection Result:");
                Console.WriteLine("   X No acceptable credentials found");
                Console.WriteLine("   [X] Government service access: DENIED");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"   [X] Government service error: {ex.Message}");
        }

        return Task.CompletedTask;
    }

    private static Task DemonstrateEmploymentBackground(object[] wallet)
    {
        Console.WriteLine("\n5. EMPLOYMENT BACKGROUND CHECK - COMPREHENSIVE VERIFICATION");
        Console.WriteLine("   Security clearance job requiring multi-source verification");
        Console.WriteLine();

        try
        {
            // Simplified multi-step verification structure
            var presentationDefinition = new
            {
                Id = "security_clearance_background",
                Purpose = "Comprehensive background check for security clearance position",
                VerificationSteps = new object[]
                {
                    new { Step = "Identity", Required = new[] { "full_name" }, Sources = new[] { "government_id" } },
                    new { Step = "Education", Required = new[] { "degree", "major" }, Sources = new[] { "university_degree" } },
                    new { Step = "Professional", Required = new[] { "security_clearance", "position" }, Sources = new[] { "employment", "professional_license" } }
                }
            };

            Console.WriteLine("   Security Clearance Requirements:");
            Console.WriteLine("   ID Identity verification (government ID)");
            Console.WriteLine("   Graduation Cap Advanced technical degree");
            Console.WriteLine("   Lock Existing security clearance OR professional license");
            Console.WriteLine();

            // Comprehensive verification process
            var verificationResults = new Dictionary<string, bool>();
            var selectedCredentials = new Dictionary<string, object>();

            // Verify identity
            var identityCredential = wallet.FirstOrDefault(c =>
            {
                var dict = c as Dictionary<string, object>;
                return dict?.ContainsKey("full_name") == true && 
                       (dict["credential_type"]?.ToString()?.Contains("license") == true);
            });

            if (identityCredential != null)
            {
                verificationResults["identity"] = true;
                selectedCredentials["identity"] = identityCredential;
                Console.WriteLine("   Check Identity verification: PASSED");
            }
            else
            {
                verificationResults["identity"] = false;
                Console.WriteLine("   X Identity verification: FAILED");
            }

            // Verify education
            var educationCredential = wallet.FirstOrDefault(c =>
            {
                var dict = c as Dictionary<string, object>;
                if (dict?.ContainsKey("degree") == true && dict.ContainsKey("major") == true)
                {
                    var degree = dict["degree"]?.ToString() ?? "";
                    var major = dict["major"]?.ToString() ?? "";
                    return (degree.Contains("Master") || degree.Contains("Bachelor")) &&
                           (major.Contains("Computer Science") || major.Contains("Engineering"));
                }
                return false;
            });

            if (educationCredential != null)
            {
                verificationResults["education"] = true;
                selectedCredentials["education"] = educationCredential;
                Console.WriteLine("   Check Education verification: PASSED");
            }
            else
            {
                verificationResults["education"] = false;
                Console.WriteLine("   X Education verification: FAILED");
            }

            // Verify professional qualifications
            var professionalCredential = wallet.FirstOrDefault(c =>
            {
                var dict = c as Dictionary<string, object>;
                return dict?.ContainsKey("security_clearance") == true || 
                       dict?["credential_type"]?.ToString() == "professional-license";
            });

            if (professionalCredential != null)
            {
                verificationResults["professional"] = true;
                selectedCredentials["professional"] = professionalCredential;
                var dict = professionalCredential as Dictionary<string, object>;
                if (dict?.ContainsKey("security_clearance") == true)
                {
                    Console.WriteLine($"   Check Professional qualification: PASSED (Security Clearance: {dict["security_clearance"]})");
                }
                else
                {
                    Console.WriteLine("   Check Professional qualification: PASSED (Professional License)");
                }
            }
            else
            {
                verificationResults["professional"] = false;
                Console.WriteLine("   X Professional qualification: FAILED");
            }

            Console.WriteLine();

            // Final assessment
            bool allRequirementsMet = verificationResults.Values.All(r => r);
            
            if (allRequirementsMet)
            {
                Console.WriteLine("   Party SECURITY CLEARANCE BACKGROUND CHECK: PASSED");
                Console.WriteLine($"   [X] {selectedCredentials.Count} credentials verified");
                Console.WriteLine("   [X] Multi-source verification completed");
                Console.WriteLine("   [X] Candidate eligible for security-cleared position");
                Console.WriteLine();
                
                Console.WriteLine("   Presentation Exchange Benefits Demonstrated:");
                Console.WriteLine("   Check Complex multi-credential requirements handled automatically");
                Console.WriteLine("   Check Preference-based selection for optimal user privacy");
                Console.WriteLine("   Check Constraint matching with pattern filters");
                Console.WriteLine("   Check Comprehensive verification with fallback options");
                Console.WriteLine("   Check Standards-based interoperability across systems");
            }
            else
            {
                Console.WriteLine("   X SECURITY CLEARANCE BACKGROUND CHECK: FAILED");
                var failedRequirements = verificationResults.Where(kvp => !kvp.Value).Select(kvp => kvp.Key);
                Console.WriteLine($"   [X] Failed requirements: {string.Join(", ", failedRequirements)}");
            }

            Console.WriteLine();
            Console.WriteLine("   IMPLEMENTATION NOTES:");
            Console.WriteLine("   • This example demonstrates conceptual approaches");
            Console.WriteLine("   • Production implementation would use SdJwt.Net.PresentationExchange");
            Console.WriteLine("   • Real systems need comprehensive constraint evaluation");
            Console.WriteLine("   • Security considerations for credential handling");
            Console.WriteLine("   • Integration with wallet and verifier systems");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"   [X] Background check error: {ex.Message}");
        }

        return Task.CompletedTask;
    }
}

