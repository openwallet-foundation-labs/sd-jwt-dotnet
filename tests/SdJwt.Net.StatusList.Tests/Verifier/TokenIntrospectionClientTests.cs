using FluentAssertions;
using SdJwt.Net.StatusList.Introspection;
using System.Net;
using System.Text.Json;
using Xunit;

namespace SdJwt.Net.StatusList.Tests.Verifier;

/// <summary>
/// Tests for TokenIntrospectionClient per RFC 7662.
/// </summary>
public class TokenIntrospectionClientTests
{
    private const string IntrospectionEndpoint = "https://issuer.example.com/introspect";
    private const string ValidToken = "eyJhbGciOiJFUzI1NiJ9.test.signature";

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullHttpClient_CreatesOwnClient()
    {
        // Arrange & Act
        var client = new TokenIntrospectionClient(IntrospectionEndpoint);

        // Assert
        client.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithEmptyEndpoint_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => new TokenIntrospectionClient("");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("introspectionEndpoint");
    }

    [Fact]
    public void Constructor_WithNullEndpoint_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => new TokenIntrospectionClient(null!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("introspectionEndpoint");
    }

    [Fact]
    public void Constructor_WithInvalidUri_ThrowsArgumentException()
    {
        // Arrange & Act
        var act = () => new TokenIntrospectionClient("not-a-valid-uri");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("introspectionEndpoint");
    }

    #endregion

    #region IntrospectAsync Tests

    [Fact]
    public async Task IntrospectAsync_WithActiveToken_ReturnsActiveResult()
    {
        // Arrange
        var responseJson = """
        {
            "active": true,
            "iss": "https://issuer.example.com",
            "sub": "user-123",
            "aud": "client-456",
            "exp": 1735689600,
            "iat": 1735603200,
            "scope": "read write",
            "client_id": "client-456",
            "token_type": "access_token"
        }
        """;

        var httpClient = CreateMockHttpClient(responseJson, HttpStatusCode.OK);
        var client = new TokenIntrospectionClient(IntrospectionEndpoint, httpClient);

        // Act
        var result = await client.IntrospectAsync(ValidToken);

        // Assert
        result.Should().NotBeNull();
        result.IsActive.Should().BeTrue();
        result.Issuer.Should().Be("https://issuer.example.com");
        result.Subject.Should().Be("user-123");
        result.Audience.Should().Be("client-456");
        result.ExpiresAt.Should().NotBeNull();
        result.IssuedAt.Should().NotBeNull();
        result.Scope.Should().Be("read write");
        result.ClientId.Should().Be("client-456");
        result.TokenType.Should().Be("access_token");
    }

    [Fact]
    public async Task IntrospectAsync_WithInactiveToken_ReturnsInactiveResult()
    {
        // Arrange
        var responseJson = """
        {
            "active": false
        }
        """;

        var httpClient = CreateMockHttpClient(responseJson, HttpStatusCode.OK);
        var client = new TokenIntrospectionClient(IntrospectionEndpoint, httpClient);

        // Act
        var result = await client.IntrospectAsync(ValidToken);

        // Assert
        result.Should().NotBeNull();
        result.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task IntrospectAsync_WithRevokedStatusClaim_ReturnsStatusValue()
    {
        // Arrange - RFC 7662 extension for status claim
        var responseJson = """
        {
            "active": false,
            "status": "revoked"
        }
        """;

        var httpClient = CreateMockHttpClient(responseJson, HttpStatusCode.OK);
        var client = new TokenIntrospectionClient(IntrospectionEndpoint, httpClient);

        // Act
        var result = await client.IntrospectAsync(ValidToken);

        // Assert
        result.Should().NotBeNull();
        result.IsActive.Should().BeFalse();
        result.Status.Should().Be("revoked");
    }

    [Fact]
    public async Task IntrospectAsync_WithNullToken_ThrowsArgumentNullException()
    {
        // Arrange
        var client = new TokenIntrospectionClient(IntrospectionEndpoint, CreateMockHttpClient("{}", HttpStatusCode.OK));

        // Act
        var act = () => client.IntrospectAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("token");
    }

    [Fact]
    public async Task IntrospectAsync_WithEmptyToken_ThrowsArgumentException()
    {
        // Arrange
        var client = new TokenIntrospectionClient(IntrospectionEndpoint, CreateMockHttpClient("{}", HttpStatusCode.OK));

        // Act
        var act = () => client.IntrospectAsync("");

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("token");
    }

    [Fact]
    public async Task IntrospectAsync_WithClientCredentials_SendsAuthorizationHeader()
    {
        // Arrange
        var responseJson = """{ "active": true }""";
        var capturedRequest = default(HttpRequestMessage);
        var httpClient = CreateMockHttpClient(responseJson, HttpStatusCode.OK, req => capturedRequest = req);

        var options = new TokenIntrospectionOptions
        {
            ClientId = "my-client-id",
            ClientSecret = "my-client-secret"
        };
        var client = new TokenIntrospectionClient(IntrospectionEndpoint, httpClient, options);

        // Act
        await client.IntrospectAsync(ValidToken);

        // Assert
        capturedRequest.Should().NotBeNull();
        capturedRequest!.Headers.Authorization.Should().NotBeNull();
        capturedRequest.Headers.Authorization!.Scheme.Should().Be("Basic");
    }

    [Fact]
    public async Task IntrospectAsync_WithTypeHint_IncludesTokenTypeHint()
    {
        // Arrange
        var responseJson = """{ "active": true }""";
        var capturedContent = default(string);
        var httpClient = CreateMockHttpClient(responseJson, HttpStatusCode.OK, async req =>
        {
            capturedContent = await req.Content!.ReadAsStringAsync();
        });
        var client = new TokenIntrospectionClient(IntrospectionEndpoint, httpClient);

        // Act
        await client.IntrospectAsync(ValidToken, "access_token");

        // Assert
        capturedContent.Should().Contain("token_type_hint=access_token");
    }

    [Fact]
    public async Task IntrospectAsync_WithServerError_ThrowsIntrospectionException()
    {
        // Arrange
        var httpClient = CreateMockHttpClient("Internal Server Error", HttpStatusCode.InternalServerError);
        var client = new TokenIntrospectionClient(IntrospectionEndpoint, httpClient);

        // Act
        var act = () => client.IntrospectAsync(ValidToken);

        // Assert
        await act.Should().ThrowAsync<TokenIntrospectionException>()
            .WithMessage("*500*");
    }

    [Fact]
    public async Task IntrospectAsync_WithUnauthorized_ThrowsIntrospectionException()
    {
        // Arrange
        var httpClient = CreateMockHttpClient("Unauthorized", HttpStatusCode.Unauthorized);
        var client = new TokenIntrospectionClient(IntrospectionEndpoint, httpClient);

        // Act
        var act = () => client.IntrospectAsync(ValidToken);

        // Assert
        await act.Should().ThrowAsync<TokenIntrospectionException>()
            .WithMessage("*401*");
    }

    [Fact]
    public async Task IntrospectAsync_WithMalformedJson_ThrowsIntrospectionException()
    {
        // Arrange
        var httpClient = CreateMockHttpClient("not valid json", HttpStatusCode.OK);
        var client = new TokenIntrospectionClient(IntrospectionEndpoint, httpClient);

        // Act
        var act = () => client.IntrospectAsync(ValidToken);

        // Assert
        await act.Should().ThrowAsync<TokenIntrospectionException>()
            .WithMessage("*parse*");
    }

    [Fact]
    public async Task IntrospectAsync_WithCancellationToken_PropagatesCancellation()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel();
        var httpClient = CreateMockHttpClient("""{ "active": true }""", HttpStatusCode.OK);
        var client = new TokenIntrospectionClient(IntrospectionEndpoint, httpClient);

        // Act
        var act = () => client.IntrospectAsync(ValidToken, cancellationToken: cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    #endregion

    #region Additional Response Fields

    [Fact]
    public async Task IntrospectAsync_WithExtensionFields_ParsesAdditionalClaims()
    {
        // Arrange
        var responseJson = """
        {
            "active": true,
            "custom_claim": "custom_value",
            "nested": { "key": "value" }
        }
        """;

        var httpClient = CreateMockHttpClient(responseJson, HttpStatusCode.OK);
        var client = new TokenIntrospectionClient(IntrospectionEndpoint, httpClient);

        // Act
        var result = await client.IntrospectAsync(ValidToken);

        // Assert
        result.Should().NotBeNull();
        result.IsActive.Should().BeTrue();
        result.AdditionalClaims.Should().ContainKey("custom_claim");
        result.AdditionalClaims["custom_claim"].Should().BeEquivalentTo("custom_value");
    }

    [Fact]
    public async Task IntrospectAsync_WithJti_ParsesJwtId()
    {
        // Arrange
        var responseJson = """
        {
            "active": true,
            "jti": "unique-token-id-123"
        }
        """;

        var httpClient = CreateMockHttpClient(responseJson, HttpStatusCode.OK);
        var client = new TokenIntrospectionClient(IntrospectionEndpoint, httpClient);

        // Act
        var result = await client.IntrospectAsync(ValidToken);

        // Assert
        result.Should().NotBeNull();
        result.JwtId.Should().Be("unique-token-id-123");
    }

    #endregion

    #region Helper Methods

    private static HttpClient CreateMockHttpClient(
        string responseContent,
        HttpStatusCode statusCode,
        Action<HttpRequestMessage>? requestCallback = null)
    {
        var handler = new MockHttpMessageHandler(responseContent, statusCode, requestCallback);
        return new HttpClient(handler);
    }

    private static HttpClient CreateMockHttpClient(
        string responseContent,
        HttpStatusCode statusCode,
        Func<HttpRequestMessage, Task>? asyncRequestCallback)
    {
        var handler = new MockHttpMessageHandler(responseContent, statusCode, asyncRequestCallback);
        return new HttpClient(handler);
    }

    private class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly string _responseContent;
        private readonly HttpStatusCode _statusCode;
        private readonly Action<HttpRequestMessage>? _requestCallback;
        private readonly Func<HttpRequestMessage, Task>? _asyncRequestCallback;

        public MockHttpMessageHandler(
            string responseContent,
            HttpStatusCode statusCode,
            Action<HttpRequestMessage>? requestCallback = null)
        {
            _responseContent = responseContent;
            _statusCode = statusCode;
            _requestCallback = requestCallback;
        }

        public MockHttpMessageHandler(
            string responseContent,
            HttpStatusCode statusCode,
            Func<HttpRequestMessage, Task>? asyncRequestCallback)
        {
            _responseContent = responseContent;
            _statusCode = statusCode;
            _asyncRequestCallback = asyncRequestCallback;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            _requestCallback?.Invoke(request);

            if (_asyncRequestCallback != null)
            {
                await _asyncRequestCallback(request);
            }

            return new HttpResponseMessage(_statusCode)
            {
                Content = new StringContent(_responseContent, System.Text.Encoding.UTF8, "application/json")
            };
        }
    }

    #endregion
}
