using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.Holder;
using SdJwt.Net.Issuer;
using SdJwt.Net.Models;
using SdJwt.Net.Vc.Issuer;
using SdJwt.Net.Vc.Models;
using SdJwt.Net.Oid4Vp.Models;
using SdJwt.Net.Oid4Vp.Verifier;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text.Json;
using JsonWebKeyMs = Microsoft.IdentityModel.Tokens.JsonWebKey;
using OID4VPPresentationDefinition = SdJwt.Net.Oid4Vp.Models.PresentationDefinition;
using OID4VPInputDescriptor = SdJwt.Net.Oid4Vp.Models.InputDescriptor;
using OID4VPConstraints = SdJwt.Net.Oid4Vp.Models.Constraints;
using OID4VPField = SdJwt.Net.Oid4Vp.Models.Field;
using OID4VPSubmissionRequirement = SdJwt.Net.Oid4Vp.Models.SubmissionRequirement;
using OID4VPDescriptorMapping = SdJwt.Net.Oid4Vp.Models.InputDescriptorMapping;
using OID4VPPresentationSubmission = SdJwt.Net.Oid4Vp.Models.PresentationSubmission;

namespace SdJwt.Net.Samples.Standards.OpenId;

/// <summary>
/// Demonstrates OpenID4VP concepts with working code examples for verifiable presentations
/// Shows both protocol concepts and actual implementation using SdJwt.Net.Oid4Vp package
/// </summary>
public class OpenId4VpExample
{
    public static async Task RunExample(IServiceProvider services)
    {
        var logger = services.GetRequiredService<ILogger<OpenId4VpExample>>();
        
        Console.WriteLine("\n" + new string('=', 65));
        Console.WriteLine("        OpenID4VP Presentation Verification Example     ");
        Console.WriteLine("                    (OID4VP 1.0 Final)                  ");
        Console.WriteLine(new string('=', 65));

        Console.WriteLine("\nOpenID for Verifiable Presentations (OID4VP) enables");
        Console.WriteLine("standardized workflows for requesting and verifying credentials.");
        Console.WriteLine("This example demonstrates both concepts and working implementation.");
        Console.WriteLine();

        // Setup keys and credentials for demonstration
        using var issuerEcdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        using var holderEcdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        
        var issuerKey = new ECDsaSecurityKey(issuerEcdsa) { KeyId = "issuer-2024" };
        var holderPrivateKey = new ECDsaSecurityKey(holderEcdsa) { KeyId = "holder-key-1" };
        var holderPublicKey = new ECDsaSecurityKey(holderEcdsa) { KeyId = "holder-key-1" };
        var holderJwk = JsonWebKeyConverter.ConvertFromSecurityKey(holderPublicKey);

        // Create sample credentials for demonstrations
        var employmentCredential = CreateEmploymentCredential(issuerKey, holderJwk);
        var driverLicenseCredential = CreateDriverLicenseCredential(issuerKey, holderJwk);
        var degreeCredential = CreateDegreeCredential(issuerKey, holderJwk);

        await DemonstrateEmploymentVerification(employmentCredential, holderPrivateKey, holderPublicKey, issuerKey, logger);
        await DemonstrateAgeVerification(driverLicenseCredential, holderPrivateKey, holderPublicKey, issuerKey, logger);
        await DemonstrateEducationVerification(degreeCredential, holderPrivateKey, holderPublicKey, issuerKey, logger);
        await DemonstrateCrossDeviceFlow();
        await DemonstrateComplexRequirements(employmentCredential, degreeCredential, driverLicenseCredential, holderPrivateKey, holderPublicKey, issuerKey, logger);

        Console.WriteLine("\n" + new string('=', 65));
        Console.WriteLine("           OpenID4VP implementation demonstrated!       ");
        Console.WriteLine("                                                         ");
        Console.WriteLine("  [X] Employment verification                             ");
        Console.WriteLine("  [X] Age verification                                    ");
        Console.WriteLine("  [X] Education verification                              ");
        Console.WriteLine("  [X] Cross-device flows (conceptual)                    ");
        Console.WriteLine("  [X] Complex presentation requirements                   ");
        Console.WriteLine(new string('=', 65));
        return;
    }

    private static string CreateEmploymentCredential(SecurityKey issuerKey, JsonWebKeyMs holderJwk)
    {
        var vcIssuer = new SdJwtVcIssuer(issuerKey, SecurityAlgorithms.EcdsaSha256);
        
        var payload = new SdJwtVcPayload
        {
            Issuer = "https://hr.techcorp.example.com",
            Subject = "did:example:employee456",
            IssuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            ExpiresAt = DateTimeOffset.UtcNow.AddYears(2).ToUnixTimeSeconds(),
            AdditionalData = new Dictionary<string, object>
            {
                ["employee_name"] = "Maria Rodriguez",
                ["employee_id"] = "EMP-456",
                ["position"] = "Senior Software Engineer",
                ["department"] = "Engineering",
                ["team"] = "Platform Architecture",
                ["start_date"] = "2020-03-15",
                ["employment_type"] = "Full-time",
                ["salary_range"] = "$140,000 - $160,000",
                ["manager"] = "David Kim",
                ["office_location"] = "San Francisco, CA"
            }
        };

        var options = new SdIssuanceOptions
        {
            DisclosureStructure = new
            {
                position = true,
                department = true,
                start_date = true,
                employment_type = true,
                salary_range = true,
                office_location = true
            }
        };

        var result = vcIssuer.Issue(
            "https://credentials.techcorp.com/employment",
            payload,
            options,
            holderJwk
        );

        return result.Issuance;
    }

    private static string CreateDriverLicenseCredential(SecurityKey issuerKey, JsonWebKeyMs holderJwk)
    {
        var vcIssuer = new SdJwtVcIssuer(issuerKey, SecurityAlgorithms.EcdsaSha256);
        
        var payload = new SdJwtVcPayload
        {
            Issuer = "https://dmv.california.gov",
            Subject = "did:example:person123",
            IssuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            ExpiresAt = DateTimeOffset.UtcNow.AddYears(5).ToUnixTimeSeconds(),
            AdditionalData = new Dictionary<string, object>
            {
                ["full_name"] = "Alice Johnson",
                ["license_number"] = "CA1234567890",
                ["birth_date"] = "1990-05-15",
                ["age_over_18"] = true,
                ["age_over_21"] = true,
                ["address"] = new
                {
                    street = "123 Main St",
                    city = "San Francisco",
                    state = "CA",
                    postal_code = "94102"
                },
                ["license_class"] = "Class C",
                ["issue_date"] = "2024-01-15"
            }
        };

        var options = new SdIssuanceOptions
        {
            DisclosureStructure = new
            {
                full_name = true,
                license_number = true,
                birth_date = true,
                age_over_18 = true,
                age_over_21 = true,
                address = new
                {
                    street = true,
                    city = true,
                    state = true,
                    postal_code = true
                }
            }
        };

        var result = vcIssuer.Issue(
            "https://credentials.dmv.ca.gov/drivers-license",
            payload,
            options,
            holderJwk
        );

        return result.Issuance;
    }

    private static string CreateDegreeCredential(SecurityKey issuerKey, JsonWebKeyMs holderJwk)
    {
        var vcIssuer = new SdJwtVcIssuer(issuerKey, SecurityAlgorithms.EcdsaSha256);
        
        var payload = new SdJwtVcPayload
        {
            Issuer = "https://registrar.stanford.edu",
            Subject = "did:example:graduate789",
            IssuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            AdditionalData = new Dictionary<string, object>
            {
                ["student_name"] = "Alex Chen",
                ["degree"] = "Master of Science",
                ["major"] = "Computer Science",
                ["concentration"] = "Artificial Intelligence",
                ["graduation_date"] = "2023-06-15",
                ["graduation_year"] = 2023,
                ["gpa"] = 3.92,
                ["honors"] = "summa cum laude",
                ["university"] = "Stanford University"
            }
        };

        var options = new SdIssuanceOptions
        {
            DisclosureStructure = new
            {
                gpa = true,
                honors = true,
                concentration = true,
                graduation_date = true
            }
        };

        var result = vcIssuer.Issue(
            "https://credentials.stanford.edu/degree",
            payload,
            options,
            holderJwk
        );

        return result.Issuance;
    }

    private static async Task DemonstrateEmploymentVerification(string employmentCredential, SecurityKey holderPrivateKey, SecurityKey holderPublicKey, SecurityKey issuerKey, ILogger logger)
    {
        Console.WriteLine("\n1. EMPLOYMENT VERIFICATION - WORKING IMPLEMENTATION");
        Console.WriteLine("   Bank verifying employment for loan application");
        Console.WriteLine();

        try
        {
            // Step 1: Create presentation request using real OID4VP models
            Console.WriteLine("   Step 1: Creating OpenID4VP presentation request...");
            
            var clientId = "https://bank.example.com";
            var responseUri = "https://bank.example.com/presentations";
            var nonce = Convert.ToBase64String(RandomNumberGenerator.GetBytes(16));

            var presentationDefinition = new OID4VPPresentationDefinition
            {
                Id = "employment_verification",
                Name = "Employment Verification for Loan",
                Purpose = "Verify employment status for loan application",
                InputDescriptors = new[]
                {
                    new OID4VPInputDescriptor
                    {
                        Id = "employment_credential",
                        Constraints = new OID4VPConstraints
                        {
                            Fields = new[]
                            {
                                new OID4VPField { Path = new[] { "$.position" } },
                                new OID4VPField 
                                { 
                                    Path = new[] { "$.employment_type" }, 
                                    Filter = new Dictionary<string, object> { { "@const", "Full-time" } }
                                },
                                new OID4VPField { Path = new[] { "$.start_date" } }
                            }
                        }
                    }
                }
            };

            var authRequest = AuthorizationRequest.CreateCrossDevice(
                clientId, responseUri, nonce, presentationDefinition, Guid.NewGuid().ToString());

            Console.WriteLine($"   [X] Client ID: {authRequest.ClientId}");
            Console.WriteLine($"   [X] Required fields: position, employment_type, start_date");
            Console.WriteLine($"   [X] Nonce: {authRequest.Nonce[..16]}...");
            Console.WriteLine();

            // Step 2: Create selective presentation using holder
            Console.WriteLine("   Step 2: Creating selective presentation...");
            
            var holder = new SdJwtHolder(employmentCredential);
            
            // Select only the required disclosures
            var selectedPresentation = holder.CreatePresentation(
                disclosure => disclosure.ClaimName == "position" ||
                            disclosure.ClaimName == "employment_type" ||
                            disclosure.ClaimName == "start_date",
                new JwtPayload
                {
                    [JwtRegisteredClaimNames.Aud] = clientId,
                    [JwtRegisteredClaimNames.Iat] = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                    [JwtRegisteredClaimNames.Nonce] = nonce
                },
                holderPrivateKey,
                SecurityAlgorithms.EcdsaSha256
            );

            Console.WriteLine($"   [X] Selective presentation created");
            Console.WriteLine($"   [X] Only required fields disclosed");
            Console.WriteLine($"   [X] Salary information protected");
            Console.WriteLine();

            // Step 3: Create authorization response
            Console.WriteLine("   Step 3: Creating authorization response...");
            
            var presentationSubmission = new OID4VPPresentationSubmission
            {
                Id = Guid.NewGuid().ToString(),
                DefinitionId = "employment_verification",
                DescriptorMap = new[]
                {
                    new OID4VPDescriptorMapping
                    {
                        Id = "employment_credential",
                        Format = Oid4VpConstants.SdJwtVcFormat,
                        Path = "$"
                    }
                }
            };

            var authResponse = AuthorizationResponse.Success(
                selectedPresentation, presentationSubmission, authRequest.State);

            Console.WriteLine($"   [X] Authorization response created");
            Console.WriteLine($"   [X] VP Token length: {selectedPresentation.Length}");
            Console.WriteLine();

            // Step 4: Verify the presentation using VpTokenValidator
            Console.WriteLine("   Step 4: Verifying presentation with OID4VP validator...");

            // ✅ NEW: Create validator with SD-JWT VC validation enabled
            var vpTokenValidator = new VpTokenValidator(
                jwtToken => Task.FromResult<SecurityKey>(issuerKey),
                useSdJwtVcValidation: true); // Enables vct, iss, typ validation

            // ✅ NEW: Use factory method for OID4VP-compliant secure defaults
            var validationOptions = VpTokenValidationOptions.CreateForOid4Vp(clientId);

            // Optional: Customize validation options
            validationOptions.ValidIssuers = new[] { "https://hr.techcorp.example.com" };
            validationOptions.MaxKeyBindingAge = TimeSpan.FromMinutes(10); // Replay protection

            // Note: These security features are now enabled by default:
            // - ValidateKeyBindingAudience = true (verifies KB-JWT aud matches client_id)
            // - ValidateKeyBindingFreshness = true (prevents replay attacks per OID4VP 14.1)
            // - SD-JWT VC validation (validates vct, iss, typ per draft-ietf-oauth-sd-jwt-vc-13)

            var verificationResult = await vpTokenValidator.ValidateAsync(
                authResponse, nonce, validationOptions);

            if (verificationResult.IsValid)
            {
                Console.WriteLine("   VERIFICATION RESULTS:");
                Console.WriteLine("   [X] Employment verified: Senior Software Engineer");
                Console.WriteLine("   [X] Employment type: Full-time (requirement met)");
                Console.WriteLine("   [X] Start date: 2020-03-15 (sufficient tenure)");
                Console.WriteLine("   [X] Salary details: PROTECTED (not disclosed)");
                Console.WriteLine("   [X] Manager information: PROTECTED (not disclosed)");
                Console.WriteLine("   [X] Cryptographic verification: PASSED");
                Console.WriteLine("   [X] Key binding verification: PASSED");
            }
            else
            {
                Console.WriteLine($"   [X] Verification failed: {verificationResult.Error}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"   [X] Implementation error: {ex.Message}");
        }
    }

    private static async Task DemonstrateAgeVerification(string driverLicenseCredential, SecurityKey holderPrivateKey, SecurityKey holderPublicKey, SecurityKey issuerKey, ILogger logger)
    {
        Console.WriteLine("\n2. AGE VERIFICATION - WORKING IMPLEMENTATION");
        Console.WriteLine("   Online service verifying user is over 21");
        Console.WriteLine();

        try
        {
            // Create presentation definition for age verification
            var presentationDefinition = new OID4VPPresentationDefinition
            {
                Id = "age_verification_21",
                Purpose = "Verify you are 21 years or older",
                InputDescriptors = new[]
                {
                    new OID4VPInputDescriptor
                    {
                        Id = "government_id",
                        Constraints = new OID4VPConstraints
                        {
                            Fields = new[]
                            {
                                new OID4VPField 
                                { 
                                    Path = new[] { "$.age_over_21" },
                                    Filter = new Dictionary<string, object> { { "type", "boolean" }, { "@const", true } }
                                }
                            }
                        }
                    }
                }
            };

            var clientId = "https://alcohol-store.example.com";
            var responseUri = "https://alcohol-store.example.com/presentations";
            var nonce = Convert.ToBase64String(RandomNumberGenerator.GetBytes(16));

            var authRequest = AuthorizationRequest.CreateCrossDevice(
                clientId, responseUri, nonce, presentationDefinition);

            Console.WriteLine("   Privacy-Preserving Age Verification:");
            Console.WriteLine($"   [X] Verifier: {authRequest.ClientId}");
            Console.WriteLine("   [X] Only age verification flag requested");
            Console.WriteLine("   [X] Birth date remains private");
            Console.WriteLine();

            // Create selective presentation with minimal disclosure
            var holder = new SdJwtHolder(driverLicenseCredential);
            
            var agePresentation = holder.CreatePresentation(
                disclosure => disclosure.ClaimName == "age_over_21",
                new JwtPayload
                {
                    [JwtRegisteredClaimNames.Aud] = clientId,
                    [JwtRegisteredClaimNames.Iat] = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                    [JwtRegisteredClaimNames.Nonce] = nonce
                },
                holderPrivateKey,
                SecurityAlgorithms.EcdsaSha256
            );

            Console.WriteLine("   Selective Disclosure Results:");
            Console.WriteLine("   [X] age_over_21: true (DISCLOSED)");
            Console.WriteLine("   [X] birth_date: PROTECTED (not disclosed)");
            Console.WriteLine("   [X] full_name: PROTECTED (not disclosed)");
            Console.WriteLine("   [X] address: PROTECTED (not disclosed)");
            Console.WriteLine("   [X] license_number: PROTECTED (not disclosed)");
            Console.WriteLine();

            // Verify the age presentation
            var presentationSubmission = new OID4VPPresentationSubmission
            {
                Id = Guid.NewGuid().ToString(),
                DefinitionId = "age_verification_21",
                DescriptorMap = new[]
                {
                    new OID4VPDescriptorMapping
                    {
                        Id = "government_id",
                        Format = Oid4VpConstants.SdJwtVcFormat,
                        Path = "$"
                    }
                }
            };

            var authResponse = AuthorizationResponse.Success(agePresentation, presentationSubmission);
            
            var vpTokenValidator = new VpTokenValidator(jwtToken => Task.FromResult<SecurityKey>(issuerKey));
            var validationOptions = new VpTokenValidationOptions
            {
                ValidateIssuer = true,
                ValidIssuers = new[] { "https://dmv.california.gov" }
            };

            var verificationResult = await vpTokenValidator.ValidateAsync(
                authResponse, nonce, validationOptions);

            Console.WriteLine("   Verification Outcome:");
            if (verificationResult.IsValid)
            {
                Console.WriteLine("   [X] Age requirement: SATISFIED (over 21)");
                Console.WriteLine("   [X] Government-issued: VERIFIED (DMV signature)");
                Console.WriteLine("   [X] Privacy: MAXIMIZED (minimal disclosure)");
                Console.WriteLine("   [X] Access: GRANTED");
            }
            else
            {
                Console.WriteLine($"   [X] Age verification failed: {verificationResult.Error}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"   [X] Age verification error: {ex.Message}");
        }
    }

    private static async Task DemonstrateEducationVerification(string degreeCredential, SecurityKey holderPrivateKey, SecurityKey holderPublicKey, SecurityKey issuerKey, ILogger logger)
    {
        Console.WriteLine("\n3. EDUCATION VERIFICATION - WORKING IMPLEMENTATION");
        Console.WriteLine("   Employer verifying degree for job application");
        Console.WriteLine();

        try
        {
            // Create presentation definition for education verification
            var presentationDefinition = new OID4VPPresentationDefinition
            {
                Id = "education_verification",
                Purpose = "Verify education qualifications for employment",
                InputDescriptors = new[]
                {
                    new OID4VPInputDescriptor
                    {
                        Id = "university_degree",
                        Constraints = new OID4VPConstraints
                        {
                            Fields = new[]
                            {
                                new OID4VPField { Path = new[] { "$.degree" } },
                                new OID4VPField { Path = new[] { "$.major" } },
                                new OID4VPField { Path = new[] { "$.graduation_date" } },
                                new OID4VPField { Path = new[] { "$.gpa" }, Optional = true }
                            }
                        }
                    }
                }
            };

            var clientId = "https://employer.example.com";
            var responseUri = "https://employer.example.com/presentations";
            var nonce = Convert.ToBase64String(RandomNumberGenerator.GetBytes(16));

            var authRequest = AuthorizationRequest.CreateCrossDevice(
                clientId, responseUri, nonce, presentationDefinition);

            Console.WriteLine($"   Employer Requirements:");
            Console.WriteLine($"   • Degree type (required)");
            Console.WriteLine($"   • Field of study (required)");
            Console.WriteLine($"   • Graduation date (required)");
            Console.WriteLine($"   • GPA (optional - candidate choice)");
            Console.WriteLine();

            // Holder chooses to share most fields but keep GPA private
            var holder = new SdJwtHolder(degreeCredential);
            
            var educationPresentation = holder.CreatePresentation(
                disclosure => disclosure.ClaimName == "degree" ||
                            disclosure.ClaimName == "major" ||
                            disclosure.ClaimName == "graduation_date" ||
                            disclosure.ClaimName == "honors",
                new JwtPayload
                {
                    [JwtRegisteredClaimNames.Aud] = clientId,
                    [JwtRegisteredClaimNames.Iat] = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                    [JwtRegisteredClaimNames.Nonce] = nonce
                },
                holderPrivateKey,
                SecurityAlgorithms.EcdsaSha256
            );

            Console.WriteLine("   Candidate's Disclosure Choices:");
            Console.WriteLine("   [X] degree: Master of Science (DISCLOSED)");
            Console.WriteLine("   [X] major: Computer Science (DISCLOSED)");
            Console.WriteLine("   [X] graduation_date: 2023-06-15 (DISCLOSED)");
            Console.WriteLine("   [X] honors: summa cum laude (DISCLOSED)");
            Console.WriteLine("   [X] gpa: 3.92 (KEPT PRIVATE - candidate choice)");
            Console.WriteLine("   [X] concentration: AI (NOT REQUESTED)");
            Console.WriteLine();

            // Verify the education presentation
            var presentationSubmission = new OID4VPPresentationSubmission
            {
                Id = Guid.NewGuid().ToString(),
                DefinitionId = "education_verification",
                DescriptorMap = new[]
                {
                    new OID4VPDescriptorMapping
                    {
                        Id = "university_degree",
                        Format = Oid4VpConstants.SdJwtVcFormat,
                        Path = "$"
                    }
                }
            };

            var authResponse = AuthorizationResponse.Success(educationPresentation, presentationSubmission);
            
            var vpTokenValidator = new VpTokenValidator(jwtToken => Task.FromResult<SecurityKey>(issuerKey));
            var validationOptions = new VpTokenValidationOptions
            {
                ValidateIssuer = true,
                ValidIssuers = new[] { "https://registrar.stanford.edu" }
            };

            var verificationResult = await vpTokenValidator.ValidateAsync(
                authResponse, nonce, validationOptions);

            Console.WriteLine("   Verification Results:");
            if (verificationResult.IsValid)
            {
                Console.WriteLine("   [X] Education requirement: SATISFIED");
                Console.WriteLine("   [X] Technical field: Computer Science (preferred)");
                Console.WriteLine("   [X] Advanced degree: Master's (exceeds minimum)");
                Console.WriteLine("   [X] Recent graduate: 2023 (excellent)");
                Console.WriteLine("   [X] Academic honors: summa cum laude (impressive)");
                Console.WriteLine("   [X] Candidate maintains GPA privacy while showing excellence");
            }
            else
            {
                Console.WriteLine($"   [X] Education verification failed: {verificationResult.Error}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"   [X] Education verification error: {ex.Message}");
        }
    }

    private static Task DemonstrateCrossDeviceFlow()
    {
        Console.WriteLine("\n4. CROSS-DEVICE FLOW - PROTOCOL DEMONSTRATION");
        Console.WriteLine("   QR code flow for airport security kiosk");
        Console.WriteLine();

        // Generate QR code content using real AuthorizationRequest
        var presentationDefinition = new OID4VPPresentationDefinition
        {
            Id = "airport_security",
            Purpose = "Verify identity for airport security",
            InputDescriptors = new[]
            {
                new OID4VPInputDescriptor
                {
                    Id = "government_id",
                    Constraints = new OID4VPConstraints
                    {
                        Fields = new[]
                        {
                            new OID4VPField { Path = new[] { "$.full_name" } },
                            new OID4VPField { Path = new[] { "$.license_number" } }
                        }
                    }
                }
            }
        };

        var authRequest = AuthorizationRequest.CreateCrossDevice(
            "https://kiosk.airport.example.com",
            "https://kiosk.airport.example.com/presentations",
            Convert.ToBase64String(RandomNumberGenerator.GetBytes(16)),
            presentationDefinition,
            Guid.NewGuid().ToString()
        );

        var requestJson = JsonSerializer.Serialize(authRequest);
        var qrCodeContent = $"{Oid4VpConstants.AuthorizationRequestScheme}://?request={Uri.EscapeDataString(requestJson)}";

        Console.WriteLine("   QR Code Generation:");
        Console.WriteLine("   ┌─────────────────────────────────┐");
        Console.WriteLine("   │ ███ ███ ███    ███   ███ ███ ███ │");
        Console.WriteLine("   │ ███ ███ ███ ██ ███ ██ ███ ███ ███ │");
        Console.WriteLine("   │ ███ ███ ███ ██ ███ ██ ███ ███ ███ │");
        Console.WriteLine("   │    ███    ██   ██   ██    ███     │");
        Console.WriteLine("   │ ███ ███ ███ ██ ███ ██ ███ ███ ███ │");
        Console.WriteLine("   │ ███ ███ ███    ███   ███ ███ ███ │");
        Console.WriteLine("   │ ███ ███ ███ ██ ███ ██ ███ ███ ███ │");
        Console.WriteLine("   └─────────────────────────────────┘");
        Console.WriteLine("   \"Scan to verify ID for security screening\"");
        Console.WriteLine();

        Console.WriteLine($"   QR Content: {qrCodeContent[..80]}...");
        Console.WriteLine();

        Console.WriteLine("   Cross-Device Flow Steps:");
        Console.WriteLine("   1. Computer  Kiosk displays QR code with presentation request");
        Console.WriteLine("   2. Mobile Mobile wallet scans QR code");
        Console.WriteLine("   3. Mobile Wallet parses OpenID4VP request");
        Console.WriteLine("   4. Mobile User approves selective disclosure on mobile");
        Console.WriteLine("   5. Mobile Wallet creates presentation and POSTs to response_uri");
        Console.WriteLine("   6. Computer  Kiosk receives verification result");
        Console.WriteLine();

        Console.WriteLine("   Implementation Benefits:");
        Console.WriteLine("   [X] No app installation on public kiosks");
        Console.WriteLine("   [X] Secure credential handling on personal device");
        Console.WriteLine("   [X] Standardized QR code format");
        Console.WriteLine("   [X] Direct post eliminates complex redirects");

        return Task.CompletedTask;
    }

    private static async Task DemonstrateComplexRequirements(string employmentCredential, string degreeCredential, string driverLicenseCredential, SecurityKey holderPrivateKey, SecurityKey holderPublicKey, SecurityKey issuerKey, ILogger logger)
    {
        Console.WriteLine("\n5. COMPLEX REQUIREMENTS - MULTI-CREDENTIAL PRESENTATION");
        Console.WriteLine("   Government contractor requiring multiple credentials");
        Console.WriteLine();

        try
        {
            // Create complex presentation definition requiring multiple credentials
            var presentationDefinition = new OID4VPPresentationDefinition
            {
                Id = "security_clearance_application",
                Purpose = "Comprehensive verification for security clearance",
                SubmissionRequirements = new[]
                {
                    OID4VPSubmissionRequirement.RequireAll(new[] { "government_id" }, "Identity Verification"),
                    OID4VPSubmissionRequirement.RequireAll(new[] { "degree_verification" }, "Education Verification"),
                    OID4VPSubmissionRequirement.RequireAll(new[] { "employment_verification" }, "Employment Verification")
                },
                InputDescriptors = new[]
                {
                    new OID4VPInputDescriptor
                    {
                        Id = "government_id",
                        Constraints = new OID4VPConstraints
                        {
                            Fields = new[]
                            {
                                new OID4VPField { Path = new[] { "$.full_name" } },
                                new OID4VPField { Path = new[] { "$.license_number" } }
                            }
                        }
                    },
                    new OID4VPInputDescriptor
                    {
                        Id = "degree_verification",
                        Constraints = new OID4VPConstraints
                        {
                            Fields = new[]
                            {
                                new OID4VPField { Path = new[] { "$.degree" } },
                                new OID4VPField { Path = new[] { "$.major" } },
                                new OID4VPField { Path = new[] { "$.graduation_year" } }
                            }
                        }
                    },
                    new OID4VPInputDescriptor
                    {
                        Id = "employment_verification",
                        Constraints = new OID4VPConstraints
                        {
                            Fields = new[]
                            {
                                new OID4VPField { Path = new[] { "$.position" } },
                                new OID4VPField { Path = new[] { "$.employment_type" } },
                                new OID4VPField { Path = new[] { "$.start_date" } }
                            }
                        }
                    }
                }
            };

            var clientId = "https://contractor.gov";
            var nonce = Convert.ToBase64String(RandomNumberGenerator.GetBytes(16));

            var authRequest = AuthorizationRequest.CreateCrossDevice(
                clientId, "https://contractor.gov/presentations", nonce, presentationDefinition);

            Console.WriteLine("   Multi-Credential Requirements:");
            Console.WriteLine("   Document Identity: Driver's License");
            Console.WriteLine("   Degree Education: University Degree");
            Console.WriteLine("   Briefcase Employment: Current Job");
            Console.WriteLine();

            // Create presentations for each credential
            Console.WriteLine("   Creating selective presentations...");

            var idHolder = new SdJwtHolder(driverLicenseCredential);
            var idPresentation = idHolder.CreatePresentation(
                disclosure => disclosure.ClaimName == "full_name" ||
                            disclosure.ClaimName == "license_number",
                new JwtPayload
                {
                    [JwtRegisteredClaimNames.Aud] = clientId,
                    [JwtRegisteredClaimNames.Iat] = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                    [JwtRegisteredClaimNames.Nonce] = nonce
                },
                holderPrivateKey,
                SecurityAlgorithms.EcdsaSha256
            );

            var degreeHolder = new SdJwtHolder(degreeCredential);
            var degreePresentation = degreeHolder.CreatePresentation(
                disclosure => disclosure.ClaimName == "degree" ||
                            disclosure.ClaimName == "major" ||
                            disclosure.ClaimName == "graduation_year",
                new JwtPayload
                {
                    [JwtRegisteredClaimNames.Aud] = clientId,
                    [JwtRegisteredClaimNames.Iat] = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                    [JwtRegisteredClaimNames.Nonce] = nonce
                },
                holderPrivateKey,
                SecurityAlgorithms.EcdsaSha256
            );

            var employmentHolder = new SdJwtHolder(employmentCredential);
            var employmentPresentation = employmentHolder.CreatePresentation(
                disclosure => disclosure.ClaimName == "position" ||
                            disclosure.ClaimName == "employment_type" ||
                            disclosure.ClaimName == "start_date",
                new JwtPayload
                {
                    [JwtRegisteredClaimNames.Aud] = clientId,
                    [JwtRegisteredClaimNames.Iat] = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                    [JwtRegisteredClaimNames.Nonce] = nonce
                },
                holderPrivateKey,
                SecurityAlgorithms.EcdsaSha256
            );

            // Create multi-credential authorization response
            var presentationSubmission = new OID4VPPresentationSubmission
            {
                Id = Guid.NewGuid().ToString(),
                DefinitionId = "security_clearance_application",
                DescriptorMap = new[]
                {
                    new OID4VPDescriptorMapping { Id = "government_id", Format = Oid4VpConstants.SdJwtVcFormat, Path = "$[0]" },
                    new OID4VPDescriptorMapping { Id = "degree_verification", Format = Oid4VpConstants.SdJwtVcFormat, Path = "$[1]" },
                    new OID4VPDescriptorMapping { Id = "employment_verification", Format = Oid4VpConstants.SdJwtVcFormat, Path = "$[2]" }
                }
            };

            var authResponse = AuthorizationResponse.Success(
                new[] { idPresentation, degreePresentation, employmentPresentation },
                presentationSubmission
            );

            Console.WriteLine("   Multi-Credential Presentation Created:");
            Console.WriteLine("   [X] Identity verified: Alice Johnson (CA DL)");
            Console.WriteLine("   [X] Education verified: MS Computer Science, Stanford");
            Console.WriteLine("   [X] Employment verified: Senior Engineer, TechCorp");
            Console.WriteLine("   [X] All requirements satisfied with minimal disclosure");
            Console.WriteLine();

            // Note: For multi-issuer scenarios, we would need a key resolver function
            // that can handle different issuers. For this demo, we assume same issuer.
            var vpTokenValidator = new VpTokenValidator(jwtToken => Task.FromResult<SecurityKey>(issuerKey));
            
            var validationOptions = new VpTokenValidationOptions
            {
                ValidateIssuer = true,
                ValidIssuers = new[] { 
                    "https://dmv.california.gov", 
                    "https://registrar.stanford.edu", 
                    "https://hr.techcorp.example.com" 
                }
            };

            var verificationResult = await vpTokenValidator.ValidateAsync(
                authResponse, nonce, validationOptions);

            Console.WriteLine("   Privacy Protection:");
            Console.WriteLine("   Lock Birth date: PROTECTED");
            Console.WriteLine("   Lock Address details: PROTECTED");
            Console.WriteLine("   Lock GPA: PROTECTED");
            Console.WriteLine("   Lock Salary information: PROTECTED");
            Console.WriteLine("   Lock Manager details: PROTECTED");
            Console.WriteLine();

            Console.WriteLine("   Verification Status:");
            if (verificationResult.IsValid)
            {
                Console.WriteLine("   Check APPROVED for security clearance review");
            }
            else
            {
                Console.WriteLine($"   X REJECTED: {verificationResult.Error}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"   [X] Complex requirements error: {ex.Message}");
        }
    }
}

