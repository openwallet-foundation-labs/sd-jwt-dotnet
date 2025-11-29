using SdJwt.Net.Oid4Vp.Client;
using SdJwt.Net.Oid4Vp.Models;
using Xunit;

namespace SdJwt.Net.Oid4Vp.Tests.Client;

public class AuthorizationRequestParserTests
{
    [Fact]
    public void ValidateUri_WithValidDirectRequestUri_ReturnsSuccess()
    {
        // Arrange
        var uri = $"{Oid4VpConstants.AuthorizationRequestScheme}://?request=%7B%22test%22%3A%22value%22%7D";

        // Act
        var result = AuthorizationRequestParser.ValidateUri(uri);

        // Assert
        Assert.True(result.IsValid);
        Assert.False(result.UsesRequestUri);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public void ValidateUri_WithValidRequestUriParameter_ReturnsSuccess()
    {
        // Arrange
        var uri = $"{Oid4VpConstants.AuthorizationRequestScheme}://?request_uri=https%3A%2F%2Fexample.com%2Frequest";

        // Act
        var result = AuthorizationRequestParser.ValidateUri(uri);

        // Assert
        Assert.True(result.IsValid);
        Assert.True(result.UsesRequestUri);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public void ValidateUri_WithInvalidScheme_ReturnsFailed()
    {
        // Arrange
        var uri = "https://example.com/?request=test";

        // Act
        var result = AuthorizationRequestParser.ValidateUri(uri);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Invalid scheme", result.ErrorMessage!);
    }

    [Fact]
    public void ValidateUri_WithNoParameters_ReturnsFailed()
    {
        // Arrange
        var uri = $"{Oid4VpConstants.AuthorizationRequestScheme}://";

        // Act
        var result = AuthorizationRequestParser.ValidateUri(uri);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("must contain either 'request' or 'request_uri'", result.ErrorMessage!);
    }

    [Fact]
    public void ValidateUri_WithBothParameters_ReturnsFailed()
    {
        // Arrange
        var uri = $"{Oid4VpConstants.AuthorizationRequestScheme}://?request=test&request_uri=https%3A%2F%2Fexample.com";

        // Act
        var result = AuthorizationRequestParser.ValidateUri(uri);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("cannot contain both", result.ErrorMessage!);
    }

    [Fact]
    public void HasDirectRequest_WithRequestParameter_ReturnsTrue()
    {
        // Arrange
        var uri = $"{Oid4VpConstants.AuthorizationRequestScheme}://?request=test";

        // Act
        var result = AuthorizationRequestParser.HasDirectRequest(uri);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void HasDirectRequest_WithoutRequestParameter_ReturnsFalse()
    {
        // Arrange
        var uri = $"{Oid4VpConstants.AuthorizationRequestScheme}://?request_uri=https%3A%2F%2Fexample.com";

        // Act
        var result = AuthorizationRequestParser.HasDirectRequest(uri);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void HasRequestUri_WithRequestUriParameter_ReturnsTrue()
    {
        // Arrange
        var uri = $"{Oid4VpConstants.AuthorizationRequestScheme}://?request_uri=https%3A%2F%2Fexample.com";

        // Act
        var result = AuthorizationRequestParser.HasRequestUri(uri);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void HasRequestUri_WithoutRequestUriParameter_ReturnsFalse()
    {
        // Arrange
        var uri = $"{Oid4VpConstants.AuthorizationRequestScheme}://?request=test";

        // Act
        var result = AuthorizationRequestParser.HasRequestUri(uri);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ExtractRequestUri_WithRequestUriParameter_ReturnsUri()
    {
        // Arrange
        var requestUri = "https://example.com/request";
        var uri = $"{Oid4VpConstants.AuthorizationRequestScheme}://?request_uri={Uri.EscapeDataString(requestUri)}";

        // Act
        var result = AuthorizationRequestParser.ExtractRequestUri(uri);

        // Assert
        Assert.Equal(requestUri, result);
    }

    [Fact]
    public void ExtractRequestUri_WithoutRequestUriParameter_ReturnsNull()
    {
        // Arrange
        var uri = $"{Oid4VpConstants.AuthorizationRequestScheme}://?request=test";

        // Act
        var result = AuthorizationRequestParser.ExtractRequestUri(uri);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void CreateUri_WithAuthorizationRequest_CreatesValidUri()
    {
        // Arrange
        var definition = PresentationDefinition.CreateSimple("test-def", "UniversityDegree");
        var request = AuthorizationRequest.CreateCrossDevice(
            "https://verifier.example.com",
            "https://verifier.example.com/response",
            "test-nonce",
            definition);

        // Act
        var uri = AuthorizationRequestParser.CreateUri(request);

        // Assert
        Assert.StartsWith($"{Oid4VpConstants.AuthorizationRequestScheme}://", uri);
        Assert.Contains("request=", uri);
    }

    [Fact]
    public void CreateUriWithRequestUri_WithValidRequestUri_CreatesValidUri()
    {
        // Arrange
        var requestUri = "https://verifier.example.com/requests/123";

        // Act
        var uri = AuthorizationRequestParser.CreateUriWithRequestUri(requestUri);

        // Assert
        Assert.StartsWith($"{Oid4VpConstants.AuthorizationRequestScheme}://", uri);
        Assert.Contains("request_uri=", uri);
        Assert.Contains(Uri.EscapeDataString(requestUri), uri);
    }

    [Fact]
    public void CreateUriWithRequestUri_WithInvalidRequestUri_ThrowsArgumentException()
    {
        // Arrange
        var invalidRequestUri = "not-a-valid-uri";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => AuthorizationRequestParser.CreateUriWithRequestUri(invalidRequestUri));
    }

    [Fact]
    public void Parse_WithInvalidScheme_ThrowsArgumentException()
    {
        // Arrange
        var uri = "https://example.com/?request=test";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => AuthorizationRequestParser.Parse(uri));
    }

    [Fact]
    public void Parse_WithRequestUriParameter_ThrowsInvalidOperationException()
    {
        // Arrange
        var uri = $"{Oid4VpConstants.AuthorizationRequestScheme}://?request_uri=https%3A%2F%2Fexample.com";

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => AuthorizationRequestParser.Parse(uri));
        Assert.Contains("Use ParseFromRequestUriAsync", exception.Message);
    }

    [Fact]
    public void Parse_WithNoParameters_ThrowsInvalidOperationException()
    {
        // Arrange
        var uri = $"{Oid4VpConstants.AuthorizationRequestScheme}://";

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => AuthorizationRequestParser.Parse(uri));
        Assert.Contains("must contain either 'request' or 'request_uri'", exception.Message);
    }
}