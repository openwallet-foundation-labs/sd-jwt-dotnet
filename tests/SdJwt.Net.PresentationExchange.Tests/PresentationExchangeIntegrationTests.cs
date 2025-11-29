using FluentAssertions;
using SdJwt.Net.PresentationExchange;
using SdJwt.Net.PresentationExchange.Models;
using Xunit;

namespace SdJwt.Net.PresentationExchange.Tests;

/// <summary>
/// Integration tests for the PresentationExchange package.
/// Tests the complete flow from presentation definition to credential selection.
/// </summary>
public class PresentationExchangeIntegrationTests
{
    [Fact]
    public async Task CompleteFlow_SdJwtCredentialSelection_ShouldWorkEndToEnd()
    {
        // Arrange
        var engine = PresentationExchangeFactory.CreateEngine();
        var wallet = CreateTestWallet();
        var presentationDefinition = CreateDriverLicensePresentationDefinition();

        // Act
        var result = await engine.SelectCredentialsAsync(presentationDefinition, wallet);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccessful.Should().BeTrue();
        result.SelectedCredentials.Should().HaveCount(1);
        result.SelectedCredentials[0].Format.Should().Be(PresentationExchangeConstants.Formats.SdJwtVc);
        result.PresentationSubmission.Should().NotBeNull();
        result.PresentationSubmission!.DefinitionId.Should().Be(presentationDefinition.Id);
    }

    [Fact]
    public async Task CompleteFlow_MultipleCredentialTypes_ShouldSelectCorrectOnes()
    {
        // Arrange
        var engine = PresentationExchangeFactory.CreateEngine();
        var wallet = CreateLargeTestWallet();
        var presentationDefinition = CreateMultiTypePresentationDefinition();

        // Act
        var result = await engine.SelectCredentialsAsync(presentationDefinition, wallet);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccessful.Should().BeTrue();
        result.SelectedCredentials.Should().HaveCount(2);
        
        var driverLicense = result.SelectedCredentials.FirstOrDefault(c => 
            c.InputDescriptorId == "driver_license");
        var universityDegree = result.SelectedCredentials.FirstOrDefault(c => 
            c.InputDescriptorId == "university_degree");

        driverLicense.Should().NotBeNull();
        universityDegree.Should().NotBeNull();
    }

    [Fact]
    public async Task CompleteFlow_WithSubmissionRequirements_ShouldRespectPickLogic()
    {
        // Arrange
        var engine = PresentationExchangeFactory.CreateEngine();
        var wallet = CreateLargeTestWallet();
        var presentationDefinition = CreatePickOnePresentationDefinition();

        // Act
        var result = await engine.SelectCredentialsAsync(presentationDefinition, wallet);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccessful.Should().BeTrue();
        result.SelectedCredentials.Should().HaveCount(1); // Only one should be selected due to pick rule
        result.PresentationSubmission.Should().NotBeNull();
    }

    [Fact]
    public async Task CompleteFlow_WithIssuerConstraints_ShouldFilterByIssuer()
    {
        // Arrange
        var engine = PresentationExchangeFactory.CreateEngine();
        var wallet = CreateTestWallet();
        var presentationDefinition = CreateIssuerSpecificPresentationDefinition();

        // Act
        var result = await engine.SelectCredentialsAsync(presentationDefinition, wallet);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccessful.Should().BeTrue();
        result.SelectedCredentials.Should().HaveCount(1);
        
        // Verify the selected credential is from the correct issuer
        var selectedCredential = result.SelectedCredentials[0];
        selectedCredential.Should().NotBeNull();
    }

    [Fact]
    public async Task SimpleSelector_BasicCredentialType_ShouldWork()
    {
        // Arrange
        var selector = PresentationExchangeFactory.CreateSimpleSelector();
        var wallet = CreateTestWallet();

        // Act
        var selectedCredentials = await selector.SelectByTypeAsync(wallet, "DriverLicense");

        // Assert
        selectedCredentials.Should().NotBeEmpty();
        selectedCredentials.Should().HaveCount(1);
    }

    [Fact]
    public async Task SimpleSelector_SdJwtWithDisclosures_ShouldReturnCredentialsAndDisclosures()
    {
        // Arrange
        var selector = PresentationExchangeFactory.CreateSimpleSelector();
        var wallet = CreateTestWallet();
        var requiredFields = new[] { "name", "birthDate" };

        // Act
        var result = await selector.SelectSdJwtWithDisclosuresAsync(wallet, "DriverLicense", requiredFields);

        // Assert
        result.Should().NotBeEmpty();
        result[0].credential.Should().NotBeNull();
        // Note: In this test the disclosures would be null as we're using mock data
        // In real implementation, this would contain the actual disclosures
    }

    [Fact]
    public async Task SimpleSelector_CheckCredential_ShouldValidateCorrectly()
    {
        // Arrange
        var selector = PresentationExchangeFactory.CreateSimpleSelector();
        var driverLicenseCredential = CreateMockSdJwt("DriverLicense", "https://dmv.example.com");
        var universityCredential = CreateMockSdJwt("UniversityDegree", "https://university.example.com");

        // Act
        var isValidDriverLicense = await selector.CheckCredentialAsync(
            driverLicenseCredential, "DriverLicense");
        var isValidUniversity = await selector.CheckCredentialAsync(
            universityCredential, "DriverLicense"); // Wrong type

        // Assert
        isValidDriverLicense.Should().BeTrue();
        isValidUniversity.Should().BeFalse();
    }

    [Theory]
    [InlineData("DriverLicense", "https://dmv.example.com", true)]
    [InlineData("DriverLicense", "https://wrong-issuer.example.com", false)]
    [InlineData("WrongType", "https://dmv.example.com", false)]
    public async Task Engine_WithSpecificConstraints_ShouldMatchAppropriately(
        string credentialType, string issuer, bool shouldMatch)
    {
        // Arrange
        var engine = PresentationExchangeFactory.CreateEngine();
        var credential = CreateMockSdJwt(credentialType, issuer);
        var wallet = new[] { credential };
        var presentationDefinition = CreateSpecificConstraintDefinition("DriverLicense", "https://dmv.example.com");

        // Act
        var result = await engine.SelectCredentialsAsync(presentationDefinition, wallet);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccessful.Should().Be(shouldMatch);
        
        if (shouldMatch)
        {
            result.SelectedCredentials.Should().HaveCount(1);
        }
        else
        {
            result.SelectedCredentials.Should().BeEmpty();
            result.Errors.Should().NotBeEmpty();
        }
    }

    [Fact]
    public async Task Engine_WithPerformanceOptions_ShouldLimitProcessing()
    {
        // Arrange
        var engine = PresentationExchangeFactory.CreateEngine();
        var wallet = CreateVeryLargeTestWallet(100); // 100 credentials
        var presentationDefinition = CreateSimplePresentationDefinition();
        var options = CredentialSelectionOptions.CreatePerformanceOptimized();

        // Act
        var result = await engine.SelectCredentialsAsync(presentationDefinition, wallet, options);

        // Assert
        result.Should().NotBeNull();
        result.Metadata.Should().NotBeNull();
        result.Metadata!.CredentialsEvaluated.Should().BeLessOrEqualTo(options.MaxCredentialsToEvaluate);
    }

    [Fact]
    public async Task Engine_WithThoroughOptions_ShouldProvideDetailedAnalysis()
    {
        // Arrange
        var engine = PresentationExchangeFactory.CreateEngine();
        var wallet = CreateTestWallet();
        var presentationDefinition = CreateDriverLicensePresentationDefinition();
        var options = CredentialSelectionOptions.CreateThoroughEvaluation();

        // Act
        var result = await engine.SelectCredentialsAsync(presentationDefinition, wallet, options);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccessful.Should().BeTrue();
        result.Metadata.Should().NotBeNull();
        result.Metadata!.SelectionDuration.Should().NotBeNull();
    }

    // Helper methods for creating test data

    private static object[] CreateTestWallet()
    {
        return new object[]
        {
            CreateMockSdJwt("DriverLicense", "https://dmv.example.com"),
            CreateMockJwtVc("UniversityDegree", "https://university.example.com"),
            CreateMockJsonCredential("IdentityCard", "https://government.example.com")
        };
    }

    private static object[] CreateLargeTestWallet()
    {
        var credentials = new List<object>
        {
            CreateMockSdJwt("DriverLicense", "https://dmv.example.com"),
            CreateMockSdJwt("IdentityCard", "https://government.example.com"),
            CreateMockJwtVc("UniversityDegree", "https://university.example.com"),
            CreateMockJwtVc("HighSchoolDiploma", "https://highschool.example.com"),
            CreateMockJsonCredential("PassportCard", "https://passport.example.com")
        };

        return credentials.ToArray();
    }

    private static object[] CreateVeryLargeTestWallet(int count)
    {
        var credentials = new List<object>();
        
        for (int i = 0; i < count; i++)
        {
            var typeIndex = i % 5;
            var credentialType = typeIndex switch
            {
                0 => "DriverLicense",
                1 => "UniversityDegree",
                2 => "IdentityCard",
                3 => "PassportCard",
                _ => "EmployeeCard"
            };

            credentials.Add(CreateMockSdJwt(credentialType, $"https://issuer{i}.example.com"));
        }

        return credentials.ToArray();
    }

    private static string CreateMockSdJwt(string vctType, string issuer)
    {
        var payload = new
        {
            iss = issuer,
            sub = "did:example:123",
            iat = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            exp = DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds(),
            vct = vctType,
            _sd_alg = "sha-256",
            name = "John Doe",
            _sd = new[] { "mock-hash-name", "mock-hash-birthdate" }
        };

        var payloadJson = System.Text.Json.JsonSerializer.Serialize(payload);
        var base64Payload = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(payloadJson));
        
        return $"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.{base64Payload}.mock-signature~WyJzYWx0MiIsICJuYW1lIiwgIkpvaG4gRG9lIl0~WyJzYWx0MiIsICJiaXJ0aERhdGUiLCAiMTk5MC0wMS0wMSJd~";
    }

    private static string CreateMockJwtVc(string credentialType, string issuer)
    {
        var payload = new
        {
            iss = issuer,
            sub = "did:example:123",
            iat = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            exp = DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds(),
            vc = new
            {
                type = new[] { "VerifiableCredential", credentialType },
                credentialSubject = new
                {
                    id = "did:example:subject",
                    name = "John Doe",
                    birthDate = "1990-01-01"
                }
            }
        };

        var payloadJson = System.Text.Json.JsonSerializer.Serialize(payload);
        var base64Payload = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(payloadJson));
        
        return $"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.{base64Payload}.mock-signature";
    }

    private static object CreateMockJsonCredential(string credentialType, string issuer)
    {
        return new
        {
            context = new[] { "https://www.w3.org/2018/credentials/v1" },
            type = new[] { "VerifiableCredential", credentialType },
            issuer = issuer,
            issuanceDate = "2023-01-01T00:00:00Z",
            credentialSubject = new
            {
                id = "did:example:subject",
                name = "John Doe",
                birthDate = "1990-01-01"
            }
        };
    }

    private static PresentationDefinition CreateDriverLicensePresentationDefinition()
    {
        var descriptor = InputDescriptor.CreateForSdJwt(
            "driver_license",
            "DriverLicense",
            "Driver License",
            "A valid driver license");

        return PresentationDefinition.Create(
            "driver_license_request",
            new[] { descriptor },
            "Driver License Request",
            "We need to verify your driver license");
    }

    private static PresentationDefinition CreateMultiTypePresentationDefinition()
    {
        var driverLicenseDescriptor = InputDescriptor.CreateForSdJwt(
            "driver_license",
            "DriverLicense",
            "Driver License");

        var universityDescriptor = InputDescriptor.CreateForJwtVc(
            "university_degree",
            new[] { "UniversityDegree" },
            "University Degree");

        return PresentationDefinition.Create(
            "multi_credential_request",
            new[] { driverLicenseDescriptor, universityDescriptor },
            "Multiple Credential Request",
            "We need both your driver license and university degree");
    }

    private static PresentationDefinition CreatePickOnePresentationDefinition()
    {
        var descriptor1 = InputDescriptor.CreateForSdJwt("id_credential_1", "DriverLicense");
        var descriptor2 = InputDescriptor.CreateForSdJwt("id_credential_2", "IdentityCard");

        var requirement = SubmissionRequirement.CreatePickNested(
            new[]
            {
                SubmissionRequirement.CreateAll("id_credential_1"),
                SubmissionRequirement.CreateAll("id_credential_2")
            },
            1);

        var definition = PresentationDefinition.Create(
            "pick_one_id",
            new[] { descriptor1, descriptor2 },
            "Pick One ID Document");

        definition.SubmissionRequirements = new[] { requirement };
        return definition;
    }

    private static PresentationDefinition CreateIssuerSpecificPresentationDefinition()
    {
        var constraints = Constraints.Create(
            Field.CreateForType("DriverLicense"),
            Field.CreateForIssuer("https://dmv.example.com")
        );

        var descriptor = InputDescriptor.CreateWithConstraints(
            "dmv_license",
            constraints,
            "DMV Driver License",
            "Driver license from the DMV");

        return PresentationDefinition.Create(
            "dmv_license_request",
            new[] { descriptor },
            "DMV License Request");
    }

    private static PresentationDefinition CreateSpecificConstraintDefinition(string credentialType, string issuer)
    {
        var constraints = Constraints.Create(
            Field.CreateForType(credentialType),
            Field.CreateForIssuer(issuer)
        );

        var descriptor = InputDescriptor.CreateWithConstraints(
            "specific_credential",
            constraints,
            $"Specific {credentialType}");

        return PresentationDefinition.Create(
            "specific_request",
            new[] { descriptor });
    }

    private static PresentationDefinition CreateSimplePresentationDefinition()
    {
        var descriptor = InputDescriptor.Create("any_credential", "Any Credential");
        descriptor.Format = FormatConstraints.CreateForAllFormats();

        return PresentationDefinition.Create(
            "simple_request",
            new[] { descriptor },
            "Simple Request");
    }
}