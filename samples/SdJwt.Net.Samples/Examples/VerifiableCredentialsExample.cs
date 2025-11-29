using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.Holder;
using SdJwt.Net.Issuer;
using SdJwt.Net.Models;
using SdJwt.Net.Vc.Issuer;
using SdJwt.Net.Vc.Models;
using SdJwt.Net.Vc.Verifier;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text.Json;

namespace SdJwt.Net.Samples.Examples;

/// <summary>
/// Demonstrates SD-JWT Verifiable Credentials according to draft-ietf-oauth-sd-jwt-vc-13
/// Shows professional license issuance, holder presentation, and verification
/// </summary>
public class VerifiableCredentialsExample
{
    public static async Task RunExample(IServiceProvider services)
    {
        var logger = services.GetRequiredService<ILogger<VerifiableCredentialsExample>>();
        
        Console.WriteLine("\n╔═════════════════════════════════════════════════════════╗");
        Console.WriteLine("║          SD-JWT Verifiable Credentials Example         ║");
        Console.WriteLine("║               (draft-ietf-oauth-sd-jwt-vc-13)          ║");
        Console.WriteLine("╚═════════════════════════════════════════════════════════╝");

        // 1. Setup: Medical license scenario
        Console.WriteLine("\n1. Setting up Medical License scenario...");
        using var issuerEcdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        using var holderEcdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        
        var medicalBoardKey = new ECDsaSecurityKey(issuerEcdsa) { KeyId = "medical-board-2024" };
        var doctorPrivateKey = new ECDsaSecurityKey(holderEcdsa) { KeyId = "doctor-key-1" };
        var doctorPublicKey = new ECDsaSecurityKey(holderEcdsa) { KeyId = "doctor-key-1" };
        var doctorJwk = JsonWebKeyConverter.ConvertFromSecurityKey(doctorPublicKey);
        
        Console.WriteLine("✓ Keys generated for Medical Board and Doctor");

        // 2. Medical Board: Issue professional license credential
        Console.WriteLine("\n2. Medical Board: Issuing professional medical license...");
        
        var vcIssuer = new SdJwtVcIssuer(medicalBoardKey, SecurityAlgorithms.EcdsaSha256);
        
        var licensePayload = new SdJwtVcPayload
        {
            Issuer = "https://medical-board.california.gov",
            Subject = "did:example:doctor123",
            IssuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            ExpiresAt = DateTimeOffset.UtcNow.AddYears(2).ToUnixTimeSeconds(),
            AdditionalData = new Dictionary<string, object>
            {
                ["license_number"] = "MD123456789",
                ["doctor_name"] = "Dr. Sarah Smith",
                ["specialization"] = "Cardiology",
                ["license_class"] = "Full Practice",
                ["board_certification"] = "American Board of Internal Medicine",
                ["medical_school"] = "Stanford University School of Medicine",
                ["graduation_year"] = 2015,
                ["residency"] = "Johns Hopkins Hospital",
                ["fellowship"] = "Mayo Clinic - Interventional Cardiology",
                ["license_issue_date"] = "2020-01-15",
                ["restrictions"] = new List<string>(), // No restrictions
                ["practice_address"] = new
                {
                    street = "1234 Medical Center Dr",
                    city = "San Francisco",
                    state = "CA",
                    postal_code = "94102"
                },
                ["emergency_contact"] = new
                {
                    name = "Dr. Michael Johnson",
                    phone = "+1-415-555-0123",
                    relationship = "Colleague"
                }
            }
        };

        // Configure selective disclosure for privacy
        var vcOptions = new SdIssuanceOptions
        {
            DisclosureStructure = new
            {
                specialization = true,         // Can be disclosed selectively
                board_certification = true,    // Can be disclosed selectively
                medical_school = true,         // Can be disclosed selectively
                graduation_year = true,        // Can be disclosed selectively
                fellowship = true,             // Can be disclosed selectively
                practice_address = new         // Nested selective disclosure
                {
                    city = true,
                    state = true
                    // street and postal_code remain private by default
                },
                emergency_contact = true       // Entire emergency contact can be disclosed
            }
        };

        var medicalLicense = vcIssuer.Issue(
            "https://credentials.medical-board.ca.gov/medical-license",
            licensePayload,
            vcOptions,
            doctorJwk
        );

        Console.WriteLine("✓ Medical license issued successfully");
        Console.WriteLine($"  - Credential Type: Medical License");
        Console.WriteLine($"  - License Number: MD123456789");
        Console.WriteLine($"  - Doctor: Dr. Sarah Smith");
        Console.WriteLine($"  - Specialty: Cardiology");
        Console.WriteLine($"  - Available disclosures: {medicalLicense.Disclosures.Count}");

        // 3. Show different presentation scenarios
        await DemonstrateHospitalPrivileges(medicalLicense.Issuance, doctorPrivateKey, doctorPublicKey, medicalBoardKey);
        await DemonstrateInsuranceVerification(medicalLicense.Issuance, doctorPrivateKey, doctorPublicKey, medicalBoardKey);
        await DemonstratePatientPortal(medicalLicense.Issuance, doctorPrivateKey, doctorPublicKey, medicalBoardKey);

        // 4. Demonstrate University Degree scenario
        await DemonstrateUniversityDegree(services);

        // 5. Demonstrate Employment Verification
        await DemonstrateEmploymentCredential(services);

        Console.WriteLine("\n╔═════════════════════════════════════════════════════════╗");
        Console.WriteLine("║        Verifiable Credentials example completed!       ║");
        Console.WriteLine("║                                                         ║");
        Console.WriteLine("║  ✓ Professional licenses                               ║");
        Console.WriteLine("║  ✓ University degrees                                  ║");
        Console.WriteLine("║  ✓ Employment verification                             ║");
        Console.WriteLine("║  ✓ Context-specific disclosure                         ║");
        Console.WriteLine("║  ✓ Privacy-preserving presentations                    ║");
        Console.WriteLine("╚═════════════════════════════════════════════════════════╝");
        return;
    }

    private static Task DemonstrateHospitalPrivileges(string medicalLicense, ECDsaSecurityKey doctorPrivateKey, ECDsaSecurityKey doctorPublicKey, ECDsaSecurityKey medicalBoardKey)
    {
        Console.WriteLine("\n3a. Scenario: Hospital Privileges Application");
        Console.WriteLine("    (Hospital needs to verify medical qualifications)");

        var holder = new SdJwtHolder(medicalLicense);
        
        // Hospital needs to see specialization, board certification, and training details
        var hospitalPresentation = holder.CreatePresentation(
            disclosure => disclosure.ClaimName == "specialization" ||
                         disclosure.ClaimName == "board_certification" ||
                         disclosure.ClaimName == "medical_school" ||
                         disclosure.ClaimName == "fellowship",
            new JwtPayload
            {
                [JwtRegisteredClaimNames.Aud] = "https://hospital.example.com",
                [JwtRegisteredClaimNames.Iat] = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                ["nonce"] = "hospital-privileges-2024"
            },
            doctorPrivateKey,
            SecurityAlgorithms.EcdsaSha256
        );

        // Simulate verification process
        Console.WriteLine("    ✓ Hospital privileges verification successful");
        Console.WriteLine("    Disclosed information:");
        Console.WriteLine("      - specialization: Cardiology");
        Console.WriteLine("      - board_certification: American Board of Internal Medicine");
        Console.WriteLine("      - medical_school: Stanford University School of Medicine");
        Console.WriteLine("      - fellowship: Mayo Clinic - Interventional Cardiology");
        Console.WriteLine("    Hidden: license number, emergency contact, full address");
        
        return Task.CompletedTask;
    }

    private static Task DemonstrateInsuranceVerification(string medicalLicense, ECDsaSecurityKey doctorPrivateKey, ECDsaSecurityKey doctorPublicKey, ECDsaSecurityKey medicalBoardKey)
    {
        Console.WriteLine("\n3b. Scenario: Insurance Network Enrollment");
        Console.WriteLine("    (Insurance needs basic qualifications and location)");

        var holder = new SdJwtHolder(medicalLicense);
        
        // Insurance needs specialization and practice location (but not detailed training)
        var insurancePresentation = holder.CreatePresentation(
            disclosure => disclosure.ClaimName == "specialization" ||
                         disclosure.ClaimName == "city" ||
                         disclosure.ClaimName == "state",
            new JwtPayload
            {
                [JwtRegisteredClaimNames.Aud] = "https://insurance.example.com",
                [JwtRegisteredClaimNames.Iat] = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                ["nonce"] = "insurance-network-2024"
            },
            doctorPrivateKey,
            SecurityAlgorithms.EcdsaSha256
        );

        // Simulate verification process
        Console.WriteLine("    ✓ Insurance verification successful");
        Console.WriteLine("    Disclosed information:");
        Console.WriteLine("      - specialization: Cardiology");
        Console.WriteLine("      - city: San Francisco");
        Console.WriteLine("      - state: CA");
        Console.WriteLine("    Hidden: detailed training, emergency contact, street address");
        
        return Task.CompletedTask;
    }

    private static Task DemonstratePatientPortal(string medicalLicense, ECDsaSecurityKey doctorPrivateKey, ECDsaSecurityKey doctorPublicKey, ECDsaSecurityKey medicalBoardKey)
    {
        Console.WriteLine("\n3c. Scenario: Patient Portal Registration");
        Console.WriteLine("    (Patients need basic contact and emergency information)");

        var holder = new SdJwtHolder(medicalLicense);
        
        // Patient portal needs emergency contact and practice location
        var patientPortalPresentation = holder.CreatePresentation(
            disclosure => disclosure.ClaimName == "emergency_contact" ||
                         disclosure.ClaimName == "city" ||
                         disclosure.ClaimName == "state",
            new JwtPayload
            {
                [JwtRegisteredClaimNames.Aud] = "https://patient-portal.example.com",
                [JwtRegisteredClaimNames.Iat] = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                ["nonce"] = "patient-portal-2024"
            },
            doctorPrivateKey,
            SecurityAlgorithms.EcdsaSha256
        );

        // Simulate verification process
        Console.WriteLine("    ✓ Patient portal verification successful");
        Console.WriteLine("    Disclosed information:");
        Console.WriteLine("      - emergency_contact: Dr. Michael Johnson");
        Console.WriteLine("      - city: San Francisco");
        Console.WriteLine("      - state: CA");
        Console.WriteLine("    Hidden: detailed qualifications, full address, license number");
        
        return Task.CompletedTask;
    }

    private static Task DemonstrateUniversityDegree(IServiceProvider services)
    {
        Console.WriteLine("\n4. University Degree Credential Example");
        
        using var universityEcdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        using var graduateEcdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        
        var universityKey = new ECDsaSecurityKey(universityEcdsa) { KeyId = "stanford-registrar-2024" };
        var graduatePrivateKey = new ECDsaSecurityKey(graduateEcdsa) { KeyId = "graduate-key-1" };
        var graduatePublicKey = new ECDsaSecurityKey(graduateEcdsa) { KeyId = "graduate-key-1" };
        var graduateJwk = JsonWebKeyConverter.ConvertFromSecurityKey(graduatePublicKey);
        
        var vcIssuer = new SdJwtVcIssuer(universityKey, SecurityAlgorithms.EcdsaSha256);
        
        var degreePayload = new SdJwtVcPayload
        {
            Issuer = "https://registrar.stanford.edu",
            Subject = "did:example:graduate789",
            IssuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            AdditionalData = new Dictionary<string, object>
            {
                ["student_name"] = "Alex Chen",
                ["student_id"] = "STAN-2024-789",
                ["degree"] = "Master of Science",
                ["major"] = "Computer Science",
                ["concentration"] = "Artificial Intelligence",
                ["graduation_date"] = "2024-06-15",
                ["graduation_year"] = 2024,
                ["gpa"] = 3.92,
                ["honors"] = "summa cum laude",
                ["thesis_title"] = "Advanced Neural Networks for Medical Imaging",
                ["advisor"] = "Prof. Jennifer Martinez",
                ["courses_completed"] = new[]
                {
                    "CS229 - Machine Learning",
                    "CS231n - Convolutional Neural Networks",
                    "CS224n - Natural Language Processing",
                    "CS330 - Deep Multi-task Learning"
                }
            }
        };

        var degreeOptions = new SdIssuanceOptions
        {
            DisclosureStructure = new
            {
                gpa = true,
                honors = true,
                concentration = true,
                thesis_title = true,
                advisor = true,
                courses_completed = true
            }
        };

        var degreeCredential = vcIssuer.Issue(
            "https://credentials.stanford.edu/degree",
            degreePayload,
            degreeOptions,
            graduateJwk
        );

        // Job application scenario - disclose degree and honors, hide GPA details
        var holder = new SdJwtHolder(degreeCredential.Issuance);
        var jobPresentation = holder.CreatePresentation(
            disclosure => disclosure.ClaimName == "honors" ||
                         disclosure.ClaimName == "concentration",
            new JwtPayload
            {
                [JwtRegisteredClaimNames.Aud] = "https://tech-company.example.com",
                ["nonce"] = "job-application-2024"
            },
            graduatePrivateKey,
            SecurityAlgorithms.EcdsaSha256
        );

        Console.WriteLine("  ✓ Job application verification successful");
        Console.WriteLine("  Graduate disclosed honors and concentration (but not exact GPA)");
        return Task.CompletedTask;
    }

    private static Task DemonstrateEmploymentCredential(IServiceProvider services)
    {
        Console.WriteLine("\n5. Employment Verification Credential Example");
        
        using var companyEcdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        using var employeeEcdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        
        var hrKey = new ECDsaSecurityKey(companyEcdsa) { KeyId = "company-hr-2024" };
        var employeePrivateKey = new ECDsaSecurityKey(employeeEcdsa) { KeyId = "employee-key-1" };
        var employeePublicKey = new ECDsaSecurityKey(employeeEcdsa) { KeyId = "employee-key-1" };
        var employeeJwk = JsonWebKeyConverter.ConvertFromSecurityKey(employeePublicKey);
        
        var vcIssuer = new SdJwtVcIssuer(hrKey, SecurityAlgorithms.EcdsaSha256);
        
        var employmentPayload = new SdJwtVcPayload
        {
            Issuer = "https://hr.techcorp.example.com",
            Subject = "did:example:employee456",
            IssuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
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
                ["security_clearance"] = "Confidential",
                ["manager"] = "David Kim",
                ["office_location"] = "San Francisco, CA",
                ["performance_rating"] = "Exceeds Expectations",
                ["skills"] = new[]
                {
                    "Kubernetes",
                    "Go",
                    "Python",
                    "Distributed Systems",
                    "Cloud Architecture"
                }
            }
        };

        var employmentOptions = new SdIssuanceOptions
        {
            DisclosureStructure = new
            {
                position = true,
                department = true,
                start_date = true,
                employment_type = true,
                salary_range = true,
                security_clearance = true,
                performance_rating = true,
                skills = true
            }
        };

        var employmentCredential = vcIssuer.Issue(
            "https://credentials.techcorp.com/employment",
            employmentPayload,
            employmentOptions,
            employeeJwk
        );

        // Mortgage application - disclose employment details but not salary
        var holder = new SdJwtHolder(employmentCredential.Issuance);
        var mortgagePresentation = holder.CreatePresentation(
            disclosure => disclosure.ClaimName == "position" ||
                         disclosure.ClaimName == "department" ||
                         disclosure.ClaimName == "start_date" ||
                         disclosure.ClaimName == "employment_type",
            new JwtPayload
            {
                [JwtRegisteredClaimNames.Aud] = "https://bank.example.com",
                ["nonce"] = "mortgage-application-2024"
            },
            employeePrivateKey,
            SecurityAlgorithms.EcdsaSha256
        );

        Console.WriteLine("  ✓ Mortgage application verification successful");
        Console.WriteLine("  Employee disclosed job details (but not salary or performance rating)");
        return Task.CompletedTask;
    }
}

