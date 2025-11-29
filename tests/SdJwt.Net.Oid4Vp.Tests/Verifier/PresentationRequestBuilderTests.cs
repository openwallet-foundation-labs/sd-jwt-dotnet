using SdJwt.Net.Oid4Vp.Verifier;
using SdJwt.Net.Oid4Vp.Models;
using Xunit;

namespace SdJwt.Net.Oid4Vp.Tests.Verifier;

public class PresentationRequestBuilderTests
{
    [Fact]
    public void Create_WithValidParameters_CreatesBuilder()
    {
        // Arrange
        var clientId = "https://verifier.example.com";
        var responseUri = "https://verifier.example.com/response";

        // Act
        var builder = PresentationRequestBuilder.Create(clientId, responseUri);

        // Assert
        Assert.NotNull(builder);
    }

    [Fact]
    public void RequestCredential_AddsInputDescriptor()
    {
        // Arrange
        var builder = PresentationRequestBuilder.Create(
            "https://verifier.example.com",
            "https://verifier.example.com/response");

        // Act
        builder.RequestCredential("UniversityDegree", "We need to verify your degree");

        // Assert
        var request = builder.Build();
        Assert.Single(request.PresentationDefinition!.InputDescriptors);
        Assert.Equal("input_1", request.PresentationDefinition.InputDescriptors[0].Id);
    }

    [Fact]
    public void RequestCredentialFromIssuer_AddsCorrectConstraints()
    {
        // Arrange
        var builder = PresentationRequestBuilder.Create(
            "https://verifier.example.com",
            "https://verifier.example.com/response");

        // Act
        builder.RequestCredentialFromIssuer(
            "UniversityDegree", 
            "https://university.example.com",
            "We need a degree from this specific university");

        // Assert
        var request = builder.Build();
        var descriptor = request.PresentationDefinition!.InputDescriptors[0];
        Assert.NotNull(descriptor.Constraints?.Fields);
        Assert.Equal(2, descriptor.Constraints.Fields.Length); // credential type + issuer
    }

    [Fact]
    public void WithName_SetsDefinitionName()
    {
        // Arrange
        var builder = PresentationRequestBuilder.Create(
            "https://verifier.example.com",
            "https://verifier.example.com/response");
        var name = "University Verification";

        // Act
        builder.WithName(name).RequestCredential("UniversityDegree");

        // Assert
        var request = builder.Build();
        Assert.Equal(name, request.PresentationDefinition!.Name);
    }

    [Fact]
    public void WithPurpose_SetsDefinitionPurpose()
    {
        // Arrange
        var builder = PresentationRequestBuilder.Create(
            "https://verifier.example.com",
            "https://verifier.example.com/response");
        var purpose = "We need to verify your educational qualifications";

        // Act
        builder.WithPurpose(purpose).RequestCredential("UniversityDegree");

        // Assert
        var request = builder.Build();
        Assert.Equal(purpose, request.PresentationDefinition!.Purpose);
    }

    [Fact]
    public void WithNonce_SetsCustomNonce()
    {
        // Arrange
        var builder = PresentationRequestBuilder.Create(
            "https://verifier.example.com",
            "https://verifier.example.com/response");
        var customNonce = "custom-nonce-12345";

        // Act
        builder.WithNonce(customNonce).RequestCredential("UniversityDegree");

        // Assert
        var request = builder.Build();
        Assert.Equal(customNonce, request.Nonce);
    }

    [Fact]
    public void WithState_SetsStateParameter()
    {
        // Arrange
        var builder = PresentationRequestBuilder.Create(
            "https://verifier.example.com",
            "https://verifier.example.com/response");
        var state = "state-12345";

        // Act
        builder.WithState(state).RequestCredential("UniversityDegree");

        // Assert
        var request = builder.Build();
        Assert.Equal(state, request.State);
    }

    [Fact]
    public void RequireAll_AddsSubmissionRequirement()
    {
        // Arrange
        var builder = PresentationRequestBuilder.Create(
            "https://verifier.example.com",
            "https://verifier.example.com/response");

        // Act
        builder
            .RequestCredential("UniversityDegree")
            .RequestCredential("EmploymentCertificate")
            .RequireAll("All Credentials", "We need both credentials");

        // Assert
        var request = builder.Build();
        Assert.NotNull(request.PresentationDefinition!.SubmissionRequirements);
        Assert.Single(request.PresentationDefinition.SubmissionRequirements);
        Assert.Equal(Oid4VpConstants.PresentationExchange.SubmissionRequirementRules.All,
            request.PresentationDefinition.SubmissionRequirements[0].Rule);
    }

    [Fact]
    public void RequirePick_AddsCorrectSubmissionRequirement()
    {
        // Arrange
        var builder = PresentationRequestBuilder.Create(
            "https://verifier.example.com",
            "https://verifier.example.com/response");

        // Act
        builder
            .RequestCredential("UniversityDegree")
            .RequestCredential("HighSchoolDiploma")
            .RequestCredential("TrainingCertificate")
            .RequirePick(2, "Pick Two", "Choose any two educational credentials");

        // Assert
        var request = builder.Build();
        var requirement = request.PresentationDefinition!.SubmissionRequirements![0];
        Assert.Equal(Oid4VpConstants.PresentationExchange.SubmissionRequirementRules.Pick, requirement.Rule);
        Assert.Equal(2, requirement.Count);
    }

    [Fact]
    public void BuildUri_CreatesValidOid4VpUri()
    {
        // Arrange
        var builder = PresentationRequestBuilder.Create(
            "https://verifier.example.com",
            "https://verifier.example.com/response");

        // Act
        builder.RequestCredential("UniversityDegree");
        var uri = builder.BuildUri();

        // Assert
        Assert.StartsWith($"{Oid4VpConstants.AuthorizationRequestScheme}://", uri);
        Assert.Contains("request=", uri);
    }

    [Fact]
    public void BuildUriWithRequestUri_CreatesValidUri()
    {
        // Arrange
        var builder = PresentationRequestBuilder.Create(
            "https://verifier.example.com",
            "https://verifier.example.com/response");
        var requestUri = "https://verifier.example.com/requests/123";

        // Act
        var uri = builder.BuildUriWithRequestUri(requestUri);

        // Assert
        Assert.StartsWith($"{Oid4VpConstants.AuthorizationRequestScheme}://", uri);
        Assert.Contains("request_uri=", uri);
        Assert.Contains(Uri.EscapeDataString(requestUri), uri);
    }

    [Fact]
    public void Build_WithNoInputDescriptors_ThrowsInvalidOperationException()
    {
        // Arrange
        var builder = PresentationRequestBuilder.Create(
            "https://verifier.example.com",
            "https://verifier.example.com/response");

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => builder.Build());
    }

    [Fact]
    public void WithField_AddsFieldToLastDescriptor()
    {
        // Arrange
        var builder = PresentationRequestBuilder.Create(
            "https://verifier.example.com",
            "https://verifier.example.com/response");
        var customField = Field.CreateForPath("$.custom_field", "Custom Field");

        // Act
        builder
            .RequestCredential("UniversityDegree")
            .WithField(customField);

        // Assert
        var request = builder.Build();
        var descriptor = request.PresentationDefinition!.InputDescriptors[0];
        Assert.Contains(descriptor.Constraints!.Fields!, f => f.Path.Contains("$.custom_field"));
    }

    [Fact]
    public void WithField_WithNoInputDescriptors_ThrowsInvalidOperationException()
    {
        // Arrange
        var builder = PresentationRequestBuilder.Create(
            "https://verifier.example.com",
            "https://verifier.example.com/response");
        var customField = Field.CreateForPath("$.custom_field");

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => builder.WithField(customField));
    }

    [Fact]
    public void GetNonce_ReturnsCurrentNonce()
    {
        // Arrange
        var builder = PresentationRequestBuilder.Create(
            "https://verifier.example.com",
            "https://verifier.example.com/response");

        // Act
        var nonce = builder.GetNonce();

        // Assert
        Assert.NotNull(nonce);
        Assert.NotEmpty(nonce);
    }

    [Fact]
    public void GetState_WithSetState_ReturnsState()
    {
        // Arrange
        var builder = PresentationRequestBuilder.Create(
            "https://verifier.example.com",
            "https://verifier.example.com/response");
        var expectedState = "test-state";

        // Act
        builder.WithState(expectedState);
        var actualState = builder.GetState();

        // Assert
        Assert.Equal(expectedState, actualState);
    }

    [Fact]
    public void GetState_WithoutSetState_ReturnsNull()
    {
        // Arrange
        var builder = PresentationRequestBuilder.Create(
            "https://verifier.example.com",
            "https://verifier.example.com/response");

        // Act
        var state = builder.GetState();

        // Assert
        Assert.Null(state);
    }
}