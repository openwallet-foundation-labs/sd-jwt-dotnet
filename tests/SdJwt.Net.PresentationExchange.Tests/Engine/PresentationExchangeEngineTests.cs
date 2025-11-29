using FluentAssertions;
using SdJwt.Net.PresentationExchange.Engine;
using SdJwt.Net.PresentationExchange.Models;
using SdJwt.Net.PresentationExchange.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace SdJwt.Net.PresentationExchange.Tests.Engine;

/// <summary>
/// Tests for the main PresentationExchangeEngine class.
/// </summary>
public class PresentationExchangeEngineTests
{
    private readonly Mock<ILogger<PresentationExchangeEngine>> _mockLogger;
    private readonly Mock<ILogger<ConstraintEvaluator>> _mockConstraintLogger;
    private readonly Mock<ILogger<JsonPathEvaluator>> _mockJsonPathLogger;
    private readonly Mock<ILogger<FieldFilterEvaluator>> _mockFieldFilterLogger;
    private readonly Mock<ILogger<CredentialFormatDetector>> _mockFormatDetectorLogger;
    private readonly Mock<ILogger<SubmissionRequirementEvaluator>> _mockSubmissionLogger;

    private readonly PresentationExchangeEngine _engine;

    public PresentationExchangeEngineTests()
    {
        _mockLogger = new Mock<ILogger<PresentationExchangeEngine>>();
        _mockConstraintLogger = new Mock<ILogger<ConstraintEvaluator>>();
        _mockJsonPathLogger = new Mock<ILogger<JsonPathEvaluator>>();
        _mockFieldFilterLogger = new Mock<ILogger<FieldFilterEvaluator>>();
        _mockFormatDetectorLogger = new Mock<ILogger<CredentialFormatDetector>>();
        _mockSubmissionLogger = new Mock<ILogger<SubmissionRequirementEvaluator>>();

        var jsonPathEvaluator = new JsonPathEvaluator(_mockJsonPathLogger.Object);
        var fieldFilterEvaluator = new FieldFilterEvaluator(_mockFieldFilterLogger.Object);
        var constraintEvaluator = new ConstraintEvaluator(_mockConstraintLogger.Object, jsonPathEvaluator, fieldFilterEvaluator);
        var formatDetector = new CredentialFormatDetector(_mockFormatDetectorLogger.Object);
        var submissionRequirementEvaluator = new SubmissionRequirementEvaluator(_mockSubmissionLogger.Object);

        _engine = new PresentationExchangeEngine(
            _mockLogger.Object,
            constraintEvaluator,
            submissionRequirementEvaluator,
            formatDetector);
    }

    [Fact]
    public async Task SelectCredentialsAsync_WithValidDefinition_ShouldReturnSuccess()
    {
        // Arrange
        var definition = CreateSimpleDefinition();
        var wallet = CreateSampleWallet();

        // Act
        var result = await _engine.SelectCredentialsAsync(definition, wallet);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccessful.Should().BeTrue();
        result.SelectedCredentials.Should().NotBeEmpty();
        result.PresentationSubmission.Should().NotBeNull();
        result.Metadata.Should().NotBeNull();
    }

    [Fact]
    public async Task SelectCredentialsAsync_WithNoMatchingCredentials_ShouldReturnFailure()
    {
        // Arrange
        var definition = CreateRestrictiveDefinition();
        var wallet = CreateSampleWallet();

        // Act
        var result = await _engine.SelectCredentialsAsync(definition, wallet);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccessful.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        result.SelectedCredentials.Should().BeEmpty();
    }

    [Fact]
    public async Task SelectCredentialsAsync_WithSdJwtCredentials_ShouldSelectCorrectly()
    {
        // Arrange
        var definition = CreateSdJwtDefinition();
        var wallet = CreateSdJwtWallet();

        // Act
        var result = await _engine.SelectCredentialsAsync(definition, wallet);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccessful.Should().BeTrue();
        result.SelectedCredentials.Should().NotBeEmpty();
        result.SelectedCredentials.First().Format.Should().Be(PresentationExchangeConstants.Formats.SdJwtVc);
    }

    [Fact]
    public async Task SelectCredentialsAsync_WithSubmissionRequirements_ShouldRespectConstraints()
    {
        // Arrange
        var definition = CreateDefinitionWithSubmissionRequirements();
        var wallet = CreateLargeWallet();
        var options = new CredentialSelectionOptions { MaxCredentialsToEvaluate = 5 };

        // Act
        var result = await _engine.SelectCredentialsAsync(definition, wallet, options);

        // Assert
        result.Should().NotBeNull();
        result.Metadata?.CredentialsEvaluated.Should().BeLessOrEqualTo(5);
    }

    [Fact]
    public async Task SelectCredentialsAsync_WithCancellation_ShouldThrowOperationCancelledException()
    {
        // Arrange
        var definition = CreateSimpleDefinition();
        var wallet = CreateLargeWallet();
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _engine.SelectCredentialsAsync(definition, wallet, null, cancellationTokenSource.Token));
    }

    [Fact]
    public async Task SelectCredentialsAsync_WithInvalidDefinition_ShouldThrowValidationException()
    {
        // Arrange
        var invalidDefinition = new PresentationDefinition(); // Missing required fields
        var wallet = CreateSampleWallet();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _engine.SelectCredentialsAsync(invalidDefinition, wallet));
    }

    [Fact]
    public async Task SelectCredentialsAsync_WithCustomOptions_ShouldApplyCorrectly()
    {
        // Arrange
        var definition = CreateSimpleDefinition();
        var wallet = CreateSampleWallet();
        var options = CredentialSelectionOptions.CreatePerformanceOptimized();

        // Act
        var result = await _engine.SelectCredentialsAsync(definition, wallet, options);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccessful.Should().BeTrue();
        result.Metadata?.CredentialsEvaluated.Should().BeLessOrEqualTo(options.MaxCredentialsToEvaluate);
    }

    [Theory]
    [InlineData("DriverLicense", true)]
    [InlineData("UniversityDegree", true)]
    [InlineData("NonExistentType", false)]
    public async Task SelectCredentialsAsync_WithDifferentCredentialTypes_ShouldMatchCorrectly(
        string credentialType, bool shouldMatch)
    {
        // Arrange
        var definition = CreateDefinitionForType(credentialType);
        var wallet = CreateSampleWallet();

        // Act
        var result = await _engine.SelectCredentialsAsync(definition, wallet);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccessful.Should().Be(shouldMatch);
        
        if (shouldMatch)
        {
            result.SelectedCredentials.Should().NotBeEmpty();
        }
        else
        {
            result.SelectedCredentials.Should().BeEmpty();
        }
    }

    private static PresentationDefinition CreateSimpleDefinition()
    {
        var descriptor = InputDescriptor.Create("basic-id", "Basic ID Credential");
        descriptor.Format = FormatConstraints.CreateForAllFormats();
        
        return PresentationDefinition.Create(
            "simple-def",
            new[] { descriptor },
            "Simple credential selection");
    }

    private static PresentationDefinition CreateRestrictiveDefinition()
    {
        var constraints = Constraints.CreateForIssuer("https://non-existent-issuer.example.com");
        var descriptor = InputDescriptor.CreateWithConstraints(
            "restrictive-id", 
            constraints,
            "Highly restrictive credential");

        return PresentationDefinition.Create(
            "restrictive-def",
            new[] { descriptor },
            "Restrictive credential selection");
    }

    private static PresentationDefinition CreateSdJwtDefinition()
    {
        var descriptor = InputDescriptor.CreateForSdJwt(
            "sdjwt-id",
            "DriverLicense",
            "SD-JWT Driver License");

        return PresentationDefinition.Create(
            "sdjwt-def",
            new[] { descriptor },
            "SD-JWT credential selection");
    }

    private static PresentationDefinition CreateDefinitionWithSubmissionRequirements()
    {
        var descriptor1 = InputDescriptor.Create("id-1", "First ID");
        var descriptor2 = InputDescriptor.Create("id-2", "Second ID");
        
        var requirement = SubmissionRequirement.CreatePickNested(
            new[]
            {
                SubmissionRequirement.CreateAll("id-1"),
                SubmissionRequirement.CreateAll("id-2")
            },
            1);

        var definition = PresentationDefinition.Create(
            "submission-def",
            new[] { descriptor1, descriptor2 },
            "Definition with submission requirements");
        
        definition.SubmissionRequirements = new[] { requirement };
        return definition;
    }

    private static PresentationDefinition CreateDefinitionForType(string credentialType)
    {
        var constraints = Constraints.CreateForType(credentialType);
        var descriptor = InputDescriptor.CreateWithConstraints(
            "type-specific-id",
            constraints,
            $"Credential of type {credentialType}");

        return PresentationDefinition.Create(
            "type-def",
            new[] { descriptor },
            $"Selection for {credentialType}");
    }

    private static object[] CreateSampleWallet()
    {
        return new object[]
        {
            CreateMockJwtVc("DriverLicense"),
            CreateMockJwtVc("UniversityDegree"),
            CreateMockSdJwt("DriverLicense"),
            CreateMockJsonCredential("IdentityCard")
        };
    }

    private static object[] CreateSdJwtWallet()
    {
        return new object[]
        {
            CreateMockSdJwt("DriverLicense"),
            CreateMockSdJwt("IdentityCard"),
            CreateMockJwtVc("UniversityDegree") // Non-SD-JWT for comparison
        };
    }

    private static object[] CreateLargeWallet()
    {
        var wallet = new List<object>();
        
        for (int i = 0; i < 20; i++)
        {
            wallet.Add(CreateMockJwtVc($"Type{i}"));
            wallet.Add(CreateMockSdJwt($"Type{i}"));
        }

        return wallet.ToArray();
    }

    private static string CreateMockJwtVc(string credentialType)
    {
        var payload = new
        {
            iss = "https://example-issuer.com",
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

        // This is a mock JWT - in real tests you'd use proper JWT encoding
        var payloadJson = System.Text.Json.JsonSerializer.Serialize(payload);
        var base64Payload = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(payloadJson));
        
        return $"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.{base64Payload}.mock-signature";
    }

    private static string CreateMockSdJwt(string vctType)
    {
        var payload = new
        {
            iss = "https://example-issuer.com",
            sub = "did:example:123",
            iat = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            exp = DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds(),
            vct = vctType,
            _sd_alg = "sha-256",
            name = "John Doe",
            _sd = new[] { "mock-hash-1", "mock-hash-2" }
        };

        var payloadJson = System.Text.Json.JsonSerializer.Serialize(payload);
        var base64Payload = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(payloadJson));
        
        // Mock SD-JWT format with disclosures
        return $"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.{base64Payload}.mock-signature~WyJzYWx0IiwgImJpcnRoRGF0ZSIsICIxOTkwLTAxLTAxIl0~";
    }

    private static object CreateMockJsonCredential(string credentialType)
    {
        return new
        {
            context = new[] { "https://www.w3.org/2018/credentials/v1" },
            type = new[] { "VerifiableCredential", credentialType },
            issuer = "https://example-issuer.com",
            issuanceDate = "2023-01-01T00:00:00Z",
            credentialSubject = new
            {
                id = "did:example:subject",
                name = "John Doe",
                birthDate = "1990-01-01"
            }
        };
    }
}