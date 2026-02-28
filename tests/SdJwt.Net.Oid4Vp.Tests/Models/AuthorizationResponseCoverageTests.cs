using FluentAssertions;
using SdJwt.Net.Oid4Vp.Models;
using System.Text.Json;
using Xunit;

namespace SdJwt.Net.Oid4Vp.Tests.Coverage;

/// <summary>
/// Tests for <see cref="AuthorizationResponse"/> class, covering success and error
/// response creation, VP token handling, and validation.
/// </summary>
public class AuthorizationResponseCoverageTests
{
    /// <summary>
    /// Tests that Success() with single token creates valid response.
    /// </summary>
    [Fact]
    public void AuthorizationResponse_Success_WithSingleToken_ReturnsInstance()
    {
        // Arrange
        var vpToken = "eyJhbGciOiJFUzI1NiIsInR5cCI6IkpXVCJ9...";
        var submission = PresentationSubmission.CreateSingle("sub-1", "def-1", "desc-1", "vc+sd-jwt");

        // Act
        var result = AuthorizationResponse.Success(vpToken, submission, "state123");

        // Assert
        result.VpToken.Should().Be(vpToken);
        result.PresentationSubmission.Should().BeSameAs(submission);
        result.State.Should().Be("state123");
        result.IsError.Should().BeFalse();
        result.HasVpTokens.Should().BeTrue();
    }

    /// <summary>
    /// Tests that Success() throws ArgumentException when vpToken is null.
    /// </summary>
    [Fact]
    public void AuthorizationResponse_Success_WithNullVpToken_ThrowsArgumentException()
    {
        // Arrange
        var submission = PresentationSubmission.CreateSingle("sub-1", "def-1", "desc-1", "vc+sd-jwt");

        // Act & Assert
        var act = () => AuthorizationResponse.Success((string)null!, submission);
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Tests that Success() throws ArgumentNullException when presentationSubmission is null.
    /// </summary>
    [Fact]
    public void AuthorizationResponse_Success_WithNullSubmission_ThrowsArgumentNullException()
    {
        // Act & Assert
        var act = () => AuthorizationResponse.Success("token", null!);
        act.Should().Throw<ArgumentNullException>();
    }

    /// <summary>
    /// Tests that Success() with multiple tokens creates valid response.
    /// </summary>
    [Fact]
    public void AuthorizationResponse_Success_WithMultipleTokens_ReturnsInstance()
    {
        // Arrange
        var vpTokens = new[] { "token1", "token2", "token3" };
        var submission = PresentationSubmission.CreateSingle("sub-1", "def-1", "desc-1", "vc+sd-jwt");

        // Act
        var result = AuthorizationResponse.Success(vpTokens, submission, "state123");

        // Assert
        result.VpToken.Should().BeEquivalentTo(vpTokens);
        result.HasVpTokens.Should().BeTrue();
        result.GetVpTokens().Should().HaveCount(3);
    }

    /// <summary>
    /// Tests that Success() with multiple tokens throws ArgumentNullException when tokens is null.
    /// </summary>
    [Fact]
    public void AuthorizationResponse_Success_WithNullTokenArray_ThrowsArgumentNullException()
    {
        // Arrange
        var submission = PresentationSubmission.CreateSingle("sub-1", "def-1", "desc-1", "vc+sd-jwt");

        // Act & Assert
        var act = () => AuthorizationResponse.Success((string[])null!, submission);
        act.Should().Throw<ArgumentNullException>();
    }

    /// <summary>
    /// Tests that Success() with multiple tokens throws ArgumentException when tokens is empty.
    /// </summary>
    [Fact]
    public void AuthorizationResponse_Success_WithEmptyTokenArray_ThrowsArgumentException()
    {
        // Arrange
        var submission = PresentationSubmission.CreateSingle("sub-1", "def-1", "desc-1", "vc+sd-jwt");

        // Act & Assert
        var act = () => AuthorizationResponse.Success(Array.Empty<string>(), submission);
        act.Should().Throw<ArgumentException>().WithMessage("*At least one*");
    }

    /// <summary>
    /// Tests that Success() with multiple tokens throws ArgumentException when any token is whitespace.
    /// </summary>
    [Fact]
    public void AuthorizationResponse_Success_WithWhitespaceInTokenArray_ThrowsArgumentException()
    {
        // Arrange
        var vpTokens = new[] { "token1", "   ", "token3" };
        var submission = PresentationSubmission.CreateSingle("sub-1", "def-1", "desc-1", "vc+sd-jwt");

        // Act & Assert
        var act = () => AuthorizationResponse.Success(vpTokens, submission);
        act.Should().Throw<ArgumentException>().WithMessage("*null or whitespace*");
    }

    /// <summary>
    /// Tests that CreateError() creates valid error response.
    /// </summary>
    [Fact]
    public void AuthorizationResponse_CreateError_WithValidParams_ReturnsInstance()
    {
        // Act
        var result = AuthorizationResponse.CreateError(
            Oid4VpConstants.ErrorCodes.InvalidRequest,
            "The request is malformed",
            "https://example.com/errors/invalid",
            "state123");

        // Assert
        result.Error.Should().Be(Oid4VpConstants.ErrorCodes.InvalidRequest);
        result.ErrorDescription.Should().Be("The request is malformed");
        result.ErrorUri.Should().Be("https://example.com/errors/invalid");
        result.State.Should().Be("state123");
        result.IsError.Should().BeTrue();
        result.HasVpTokens.Should().BeFalse();
    }

    /// <summary>
    /// Tests that CreateError() throws ArgumentException when error is null.
    /// </summary>
    [Fact]
    public void AuthorizationResponse_CreateError_WithNullError_ThrowsArgumentException()
    {
        // Act & Assert
        var act = () => AuthorizationResponse.CreateError(null!);
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Tests that GetVpTokens() returns empty array when VpToken is null.
    /// </summary>
    [Fact]
    public void AuthorizationResponse_GetVpTokens_WithNullVpToken_ReturnsEmptyArray()
    {
        // Arrange
        var response = new AuthorizationResponse { VpToken = null };

        // Act
        var result = response.GetVpTokens();

        // Assert
        result.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that GetVpTokens() returns single-element array when VpToken is string.
    /// </summary>
    [Fact]
    public void AuthorizationResponse_GetVpTokens_WithStringVpToken_ReturnsSingleArray()
    {
        // Arrange
        var response = new AuthorizationResponse { VpToken = "token123" };

        // Act
        var result = response.GetVpTokens();

        // Assert
        result.Should().BeEquivalentTo(new[] { "token123" });
    }

    /// <summary>
    /// Tests that GetVpTokens() returns the array when VpToken is string array.
    /// </summary>
    [Fact]
    public void AuthorizationResponse_GetVpTokens_WithArrayVpToken_ReturnsArray()
    {
        // Arrange
        var tokens = new[] { "token1", "token2" };
        var response = new AuthorizationResponse { VpToken = tokens };

        // Act
        var result = response.GetVpTokens();

        // Assert
        result.Should().BeEquivalentTo(tokens);
    }

    /// <summary>
    /// Tests that GetVpTokens() handles JsonElement with string value.
    /// </summary>
    [Fact]
    public void AuthorizationResponse_GetVpTokens_WithJsonElementString_ReturnsArray()
    {
        // Arrange
        var json = "{\"vp_token\":\"token123\"}";
        var response = JsonSerializer.Deserialize<AuthorizationResponse>(json);

        // Act
        var result = response!.GetVpTokens();

        // Assert
        result.Should().BeEquivalentTo(new[] { "token123" });
    }

    /// <summary>
    /// Tests that GetVpTokens() handles JsonElement with array value.
    /// </summary>
    [Fact]
    public void AuthorizationResponse_GetVpTokens_WithJsonElementArray_ReturnsArray()
    {
        // Arrange
        var json = "{\"vp_token\":[\"token1\",\"token2\"]}";
        var response = JsonSerializer.Deserialize<AuthorizationResponse>(json);

        // Act
        var result = response!.GetVpTokens();

        // Assert
        result.Should().BeEquivalentTo(new[] { "token1", "token2" });
    }

    /// <summary>
    /// Tests that Validate() throws InvalidOperationException when neither success nor error.
    /// </summary>
    [Fact]
    public void AuthorizationResponse_Validate_WithNeitherSuccessNorError_ThrowsInvalidOperationException()
    {
        // Arrange
        var response = new AuthorizationResponse();

        // Act & Assert
        var act = () => response.Validate();
        act.Should().Throw<InvalidOperationException>().WithMessage("*VP tokens*or*error*");
    }

    /// <summary>
    /// Tests that Validate() throws InvalidOperationException when both success and error present.
    /// </summary>
    [Fact]
    public void AuthorizationResponse_Validate_WithBothSuccessAndError_ThrowsInvalidOperationException()
    {
        // Arrange
        var response = new AuthorizationResponse
        {
            VpToken = "token",
            PresentationSubmission = PresentationSubmission.CreateSingle("sub-1", "def-1", "desc-1", "vc+sd-jwt"),
            Error = "invalid_request"
        };

        // Act & Assert
        var act = () => response.Validate();
        act.Should().Throw<InvalidOperationException>().WithMessage("*both*");
    }

    /// <summary>
    /// Tests that Validate() succeeds with valid success response.
    /// </summary>
    [Fact]
    public void AuthorizationResponse_Validate_WithValidSuccessResponse_DoesNotThrow()
    {
        // Arrange
        var response = AuthorizationResponse.Success(
            "token",
            PresentationSubmission.CreateSingle("sub-1", "def-1", "desc-1", "vc+sd-jwt"));

        // Act & Assert
        var act = () => response.Validate();
        act.Should().NotThrow();
    }

    /// <summary>
    /// Tests that Validate() succeeds with valid error response.
    /// </summary>
    [Fact]
    public void AuthorizationResponse_Validate_WithValidErrorResponse_DoesNotThrow()
    {
        // Arrange
        var response = AuthorizationResponse.CreateError(Oid4VpConstants.ErrorCodes.AccessDenied);

        // Act & Assert
        var act = () => response.Validate();
        act.Should().NotThrow();
    }

    /// <summary>
    /// Tests that Validate() cascades validation to presentation submission.
    /// </summary>
    [Fact]
    public void AuthorizationResponse_Validate_CascadesToPresentationSubmission()
    {
        // Arrange
        var response = new AuthorizationResponse
        {
            VpToken = "token",
            PresentationSubmission = new PresentationSubmission
            {
                Id = "",  // Invalid
                DefinitionId = "def-1",
                DescriptorMap = new[] { InputDescriptorMapping.Create("desc-1", "vc+sd-jwt") }
            }
        };

        // Act & Assert
        var act = () => response.Validate();
        act.Should().Throw<InvalidOperationException>();
    }
}
