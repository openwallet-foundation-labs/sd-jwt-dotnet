using FluentAssertions;
using SdJwt.Net.Oid4Vp.Models;
using Xunit;

namespace SdJwt.Net.Oid4Vp.Tests.Models;

public class AuthorizationResponseTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateAuthorizationResponse()
    {
        // Arrange
        var vpToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...";
        var presentationSubmission = new PresentationSubmission
        {
            Id = "submission-123",
            DefinitionId = "definition-456",
            DescriptorMap = new[]
            {
                new InputDescriptorMapping
                {
                    Id = "descriptor-1",
                    Format = "vc+sd-jwt",
                    Path = "$"
                }
            }
        };

        // Act
        var response = new AuthorizationResponse
        {
            VpToken = vpToken,
            PresentationSubmission = presentationSubmission
        };

        // Assert
        response.Should().NotBeNull();
        response.VpToken.Should().Be(vpToken);
        response.PresentationSubmission.Should().Be(presentationSubmission);
    }

    [Fact]
    public void Validate_WithValidResponse_ShouldNotThrow()
    {
        // Arrange
        var response = new AuthorizationResponse
        {
            VpToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
            PresentationSubmission = new PresentationSubmission
            {
                Id = "submission-123",
                DefinitionId = "definition-456",
                DescriptorMap = new[]
                {
                    new InputDescriptorMapping
                    {
                        Id = "descriptor-1",
                        Format = "vc+sd-jwt",
                        Path = "$"
                    }
                }
            }
        };

        // Act & Assert
        var act = () => response.Validate();
        act.Should().NotThrow();
    }

    [Fact]
    public void Validate_WithNullPresentationSubmission_ShouldThrow()
    {
        // Arrange
        var response = new AuthorizationResponse
        {
            VpToken = "valid-token",
            PresentationSubmission = null!
        };

        // Act & Assert
        var act = () => response.Validate();
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*VP tokens*");
    }

    [Fact]
    public void Success_ShouldCreateValidResponse()
    {
        // Arrange
        var vpToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...";
        var presentationSubmission = new PresentationSubmission
        {
            Id = "submission-123",
            DefinitionId = "definition-456",
            DescriptorMap = new[]
            {
                new InputDescriptorMapping
                {
                    Id = "descriptor-1",
                    Format = "vc+sd-jwt",
                    Path = "$"
                }
            }
        };

        // Act
        var response = AuthorizationResponse.Success(vpToken, presentationSubmission);

        // Assert
        response.Should().NotBeNull();
        response.VpToken.Should().Be(vpToken);
        response.PresentationSubmission.Should().Be(presentationSubmission);
    }

    [Fact]
    public void CreateError_ShouldCreateErrorResponse()
    {
        // Arrange
        var error = "invalid_request";
        var description = "The request is invalid";
        var errorUri = "https://example.com/errors";
        var state = "state123";

        // Act
        var response = AuthorizationResponse.CreateError(error, description, errorUri, state);

        // Assert
        response.Should().NotBeNull();
        response.Error.Should().Be(error);
        response.ErrorDescription.Should().Be(description);
        response.ErrorUri.Should().Be(errorUri);
        response.State.Should().Be(state);
        response.IsError.Should().BeTrue();
    }

    [Fact]
    public void GetVpTokens_WithSingleToken_ShouldReturnArray()
    {
        // Arrange
        var vpToken = "token123";
        var response = new AuthorizationResponse { VpToken = vpToken };

        // Act
        var tokens = response.GetVpTokens();

        // Assert
        tokens.Should().BeEquivalentTo(new[] { vpToken });
    }

    [Fact]
    public void GetVpTokens_WithTokenArray_ShouldReturnArray()
    {
        // Arrange
        var vpTokens = new[] { "token1", "token2" };
        var response = new AuthorizationResponse { VpToken = vpTokens };

        // Act
        var tokens = response.GetVpTokens();

        // Assert
        tokens.Should().BeEquivalentTo(vpTokens);
    }

    [Fact]
    public void HasVpTokens_WithTokens_ShouldReturnTrue()
    {
        // Arrange
        var response = new AuthorizationResponse { VpToken = "token123" };

        // Act & Assert
        response.HasVpTokens.Should().BeTrue();
    }

    [Fact]
    public void HasVpTokens_WithoutTokens_ShouldReturnFalse()
    {
        // Arrange
        var response = new AuthorizationResponse { VpToken = null };

        // Act & Assert
        response.HasVpTokens.Should().BeFalse();
    }

    [Fact]
    public void HasVpTokens_WithEmptyToken_ShouldReturnTrue()
    {
        // Arrange
        var response = new AuthorizationResponse { VpToken = "" };

        // Act & Assert - Empty string is still considered a token (though invalid)
        response.HasVpTokens.Should().BeTrue();
    }

    [Fact]
    public void HasVpTokens_WithWhitespaceToken_ShouldReturnTrue()
    {
        // Arrange
        var response = new AuthorizationResponse { VpToken = "   " };

        // Act & Assert - Whitespace string is still considered a token (though invalid)  
        response.HasVpTokens.Should().BeTrue();
    }
}
