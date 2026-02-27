using SdJwt.Net.Oid4Vp.Client;
using SdJwt.Net.Oid4Vp.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
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

    [Fact]
    public void Parse_WithJarRequestObject_Succeeds()
    {
        // Arrange
        using var key = ECDsa.Create();
        var signingKey = new ECDsaSecurityKey(key);
        var requestPayload = new JwtPayload
        {
            ["client_id"] = "https://verifier.example.com",
            ["response_uri"] = "https://verifier.example.com/response",
            ["response_type"] = "vp_token",
            ["response_mode"] = "direct_post",
            ["nonce"] = "n-123",
            ["presentation_definition"] = new Dictionary<string, object>
            {
                ["id"] = "pd-1",
                ["input_descriptors"] = new object[]
                {
                    new Dictionary<string, object>
                    {
                        ["id"] = "id-1",
                        ["constraints"] = new Dictionary<string, object>
                        {
                            ["fields"] = new object[0]
                        }
                    }
                }
            }
        };

        var header = new JwtHeader(new SigningCredentials(signingKey, SecurityAlgorithms.EcdsaSha256))
        {
            ["typ"] = Oid4VpConstants.AuthorizationRequestJwtType
        };
        var jwt = new JwtSecurityToken(header, requestPayload);
        var compact = new JwtSecurityTokenHandler().WriteToken(jwt);
        var uri = $"{Oid4VpConstants.AuthorizationRequestScheme}://?request={Uri.EscapeDataString(compact)}";

        // Act
        var parsed = AuthorizationRequestParser.Parse(uri);

        // Assert
        Assert.Equal("https://verifier.example.com", parsed.ClientId);
        Assert.Equal("vp_token", parsed.ResponseType);
        Assert.Equal("direct_post", parsed.ResponseMode);
        Assert.Equal("n-123", parsed.Nonce);
    }

    [Fact]
    public async Task ParseFromRequestUriAsync_WithPostMethod_UsesPostAndParsesResponse()
    {
        // Arrange
        var requestObject = """
            {
              "client_id": "https://verifier.example.com",
              "response_uri": "https://verifier.example.com/response",
              "response_type": "vp_token",
              "response_mode": "direct_post",
              "nonce": "n-123",
              "presentation_definition": {
                "id": "pd-1",
                "input_descriptors": [
                  { "id": "id-1", "constraints": { "fields": [] } }
                ]
              }
            }
            """;
        var outerUri = $"{Oid4VpConstants.AuthorizationRequestScheme}://?request_uri={Uri.EscapeDataString("https://example.com/request-object")}&request_uri_method=post";

        HttpMethod? observedMethod = null;
        var handler = new StubHttpHandler((request, _) =>
        {
            observedMethod = request.Method;
            if (request.RequestUri!.AbsoluteUri == "https://example.com/request-object")
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(requestObject, System.Text.Encoding.UTF8, "application/json")
                };
            }

            return new HttpResponseMessage(HttpStatusCode.NotFound);
        });
        using var httpClient = new HttpClient(handler);

        // Act
        var parsed = await AuthorizationRequestParser.ParseFromRequestUriAsync(outerUri, httpClient);

        // Assert
        Assert.Equal(HttpMethod.Post, observedMethod);
        Assert.Equal("https://verifier.example.com", parsed.ClientId);
        Assert.Equal("n-123", parsed.Nonce);
    }

    private sealed class StubHttpHandler(Func<HttpRequestMessage, CancellationToken, HttpResponseMessage> callback) : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, CancellationToken, HttpResponseMessage> _callback = callback;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(_callback(request, cancellationToken));
        }
    }
}
