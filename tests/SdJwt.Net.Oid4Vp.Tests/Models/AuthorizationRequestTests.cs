using SdJwt.Net.Oid4Vp.Models;
using Xunit;

namespace SdJwt.Net.Oid4Vp.Tests.Models;

public class AuthorizationRequestTests
{
    [Fact]
    public void CreateCrossDevice_WithValidParameters_CreatesCorrectRequest()
    {
        // Arrange
        var clientId = "https://verifier.example.com";
        var responseUri = "https://verifier.example.com/response";
        var nonce = "test-nonce";
        var definition = PresentationDefinition.CreateSimple("test-def", "UniversityDegree");

        // Act
        var request = AuthorizationRequest.CreateCrossDevice(clientId, responseUri, nonce, definition);

        // Assert
        Assert.Equal(clientId, request.ClientId);
        Assert.Equal(responseUri, request.ResponseUri);
        Assert.Equal(nonce, request.Nonce);
        Assert.Equal(Oid4VpConstants.ResponseTypes.VpToken, request.ResponseType);
        Assert.Equal(Oid4VpConstants.ResponseModes.DirectPost, request.ResponseMode);
        Assert.Equal(definition, request.PresentationDefinition);
    }

    [Fact]
    public void CreateWithDefinitionUri_WithValidParameters_CreatesCorrectRequest()
    {
        // Arrange
        var clientId = "https://verifier.example.com";
        var responseUri = "https://verifier.example.com/response";
        var nonce = "test-nonce";
        var definitionUri = "https://verifier.example.com/definition/123";

        // Act
        var request = AuthorizationRequest.CreateWithDefinitionUri(clientId, responseUri, nonce, definitionUri);

        // Assert
        Assert.Equal(clientId, request.ClientId);
        Assert.Equal(responseUri, request.ResponseUri);
        Assert.Equal(nonce, request.Nonce);
        Assert.Equal(definitionUri, request.PresentationDefinitionUri);
        Assert.Null(request.PresentationDefinition);
    }

    [Fact]
    public void Validate_WithValidCrossDeviceRequest_DoesNotThrow()
    {
        // Arrange
        var definition = PresentationDefinition.CreateSimple("test-def", "UniversityDegree");
        var request = AuthorizationRequest.CreateCrossDevice(
            "https://verifier.example.com",
            "https://verifier.example.com/response",
            "test-nonce",
            definition);

        // Act & Assert
        var exception = Record.Exception(() => request.Validate());
        Assert.Null(exception);
    }

    [Fact]
    public void Validate_WithMissingClientId_ThrowsInvalidOperationException()
    {
        // Arrange
        var request = new AuthorizationRequest
        {
            ClientId = "",
            ResponseType = Oid4VpConstants.ResponseTypes.VpToken,
            Nonce = "test-nonce",
            PresentationDefinition = PresentationDefinition.CreateSimple("test", "TestCredential")
        };

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => request.Validate());
    }

    [Fact]
    public void Validate_WithInvalidResponseType_ThrowsInvalidOperationException()
    {
        // Arrange
        var request = new AuthorizationRequest
        {
            ClientId = "https://verifier.example.com",
            ResponseType = "invalid_response_type",
            Nonce = "test-nonce",
            PresentationDefinition = PresentationDefinition.CreateSimple("test", "TestCredential")
        };

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => request.Validate());
    }

    [Fact]
    public void Validate_WithDirectPostModeButNoResponseUri_ThrowsInvalidOperationException()
    {
        // Arrange
        var request = new AuthorizationRequest
        {
            ClientId = "https://verifier.example.com",
            ResponseType = Oid4VpConstants.ResponseTypes.VpToken,
            ResponseMode = Oid4VpConstants.ResponseModes.DirectPost,
            Nonce = "test-nonce",
            PresentationDefinition = PresentationDefinition.CreateSimple("test", "TestCredential")
            // ResponseUri is missing
        };

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => request.Validate());
    }

    [Fact]
    public void Validate_WithBothDefinitionAndDefinitionUri_ThrowsInvalidOperationException()
    {
        // Arrange
        var request = new AuthorizationRequest
        {
            ClientId = "https://verifier.example.com",
            ResponseType = Oid4VpConstants.ResponseTypes.VpToken,
            Nonce = "test-nonce",
            PresentationDefinition = PresentationDefinition.CreateSimple("test", "TestCredential"),
            PresentationDefinitionUri = "https://example.com/definition"
        };

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => request.Validate());
    }

    [Fact]
    public void Validate_WithNeitherDefinitionNorDefinitionUri_ThrowsInvalidOperationException()
    {
        // Arrange
        var request = new AuthorizationRequest
        {
            ClientId = "https://verifier.example.com",
            ResponseType = Oid4VpConstants.ResponseTypes.VpToken,
            Nonce = "test-nonce"
            // Neither PresentationDefinition nor PresentationDefinitionUri is set
        };

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => request.Validate());
    }
}
