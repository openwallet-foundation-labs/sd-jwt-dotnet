using FluentAssertions;
using Moq;
using Moq.Protected;
using SdJwt.Net.StatusList.Verifier;
using System.Net;
using Xunit;

namespace SdJwt.Net.StatusList.Tests.Verifier;

/// <summary>
/// Tests for HttpStatusListFetcher focusing on critical paths.
/// </summary>
public class HttpStatusListFetcherTests : IDisposable
{
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly HttpClient _httpClient;
    private readonly HttpStatusListFetcher _fetcher;

    public HttpStatusListFetcherTests()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        _fetcher = new HttpStatusListFetcher(_httpClient);
    }

    [Fact]
    public void Constructor_WithNullHttpClient_ShouldCreateDefaultClient()
    {
        // Act
        using var fetcher = new HttpStatusListFetcher(null);

        // Assert
        fetcher.Should().NotBeNull();
    }

    [Fact]
    public async Task FetchStatusListAsync_WithSuccessfulResponse_ShouldReturnToken()
    {
        // Arrange
        var expectedToken = "test.jwt.token";
        var statusListUri = "https://example.com/status-list";

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(expectedToken)
            });

        // Act
        var result = await _fetcher.FetchStatusListAsync(statusListUri);

        // Assert
        result.Should().Be(expectedToken);
    }

    [Fact]
    public async Task FetchStatusListAsync_WithHttpError_ShouldThrowStatusListFetchException()
    {
        // Arrange
        var statusListUri = "https://example.com/status-list";

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound
            });

        // Act
        var act = async () => await _fetcher.FetchStatusListAsync(statusListUri);

        // Assert
        await act.Should().ThrowAsync<StatusListFetchException>();
    }

    [Fact]
    public void Dispose_ShouldDisposeResources()
    {
        // Arrange
        using var httpClient = new HttpClient();
        var fetcher = new HttpStatusListFetcher(httpClient);

        // Act
        fetcher.Dispose();

        // Assert - Should not throw
        fetcher.Should().NotBeNull();
    }

    public void Dispose()
    {
        _fetcher?.Dispose();
        _httpClient?.Dispose();
    }
}
