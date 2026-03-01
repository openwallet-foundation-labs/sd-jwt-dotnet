using FluentAssertions;
using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.StatusList.Introspection;
using SdJwt.Net.StatusList.Models;
using SdJwt.Net.StatusList.Verifier;
using System.Net;
using System.Security.Cryptography;
using Xunit;

namespace SdJwt.Net.StatusList.Tests.Verifier;

/// <summary>
/// Tests for HybridStatusChecker combining Status List and Token Introspection.
/// </summary>
public class HybridStatusCheckerTests
{
    private readonly SecurityKey _issuerKey;
    private const string IntrospectionEndpoint = "https://issuer.example.com/introspect";
    private const string StatusListUri = "https://issuer.example.com/status/1";

    public HybridStatusCheckerTests()
    {
        var ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        _issuerKey = new ECDsaSecurityKey(ecdsa) { KeyId = "test-key-1" };
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidOptions_CreatesInstance()
    {
        // Arrange & Act
        var options = new HybridStatusOptions
        {
            Strategy = HybridStrategy.PreferStatusList,
            IntrospectionEndpoint = IntrospectionEndpoint
        };
        var checker = new HybridStatusChecker(options);

        // Assert
        checker.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithNullOptions_ThrowsArgumentNullException()
    {
        // Arrange & Act
        var act = () => new HybridStatusChecker(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("options");
    }

    [Fact]
    public void Constructor_WithIntrospectionOnlyButNoEndpoint_ThrowsArgumentException()
    {
        // Arrange
        var options = new HybridStatusOptions
        {
            Strategy = HybridStrategy.IntrospectionOnly,
            IntrospectionEndpoint = null
        };

        // Act
        var act = () => new HybridStatusChecker(options);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*IntrospectionEndpoint*");
    }

    #endregion

    #region Strategy Tests

    [Fact]
    public async Task CheckStatusAsync_WithStatusListOnlyStrategy_FailsWithInvalidUri()
    {
        // Arrange
        var options = new HybridStatusOptions
        {
            Strategy = HybridStrategy.StatusListOnly,
            StatusListOptions = new StatusListOptions
            {
                EnableStatusChecking = true,
                FailOnStatusCheckError = true
            }
        };

        var statusClaim = new StatusClaim
        {
            StatusList = new StatusListReference
            {
                Uri = "https://invalid.example.com/nonexistent",  // Will fail
                Index = 0
            }
        };

        var checker = new HybridStatusChecker(options);

        // Act & Assert - should throw because status list fetch fails
        await Assert.ThrowsAnyAsync<Exception>(async () =>
            await checker.CheckStatusAsync(statusClaim, async _ => _issuerKey));
    }

    [Fact]
    public async Task CheckStatusAsync_WithIntrospectionOnlyStrategy_UsesIntrospection()
    {
        // Arrange
        var introspectionResponse = """{ "active": true }""";
        var httpClient = CreateMockHttpClient(introspectionResponse, HttpStatusCode.OK);

        var options = new HybridStatusOptions
        {
            Strategy = HybridStrategy.IntrospectionOnly,
            IntrospectionEndpoint = IntrospectionEndpoint
        };

        var checker = new HybridStatusChecker(options, httpClient: httpClient);

        // Act
        var result = await checker.CheckStatusAsync("test-token");

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(StatusType.Valid);
    }

    [Fact]
    public async Task CheckStatusAsync_WithIntrospectionOnly_InactiveToken_ReturnsInvalid()
    {
        // Arrange
        var introspectionResponse = """{ "active": false, "status": "revoked" }""";
        var httpClient = CreateMockHttpClient(introspectionResponse, HttpStatusCode.OK);

        var options = new HybridStatusOptions
        {
            Strategy = HybridStrategy.IntrospectionOnly,
            IntrospectionEndpoint = IntrospectionEndpoint
        };

        var checker = new HybridStatusChecker(options, httpClient: httpClient);

        // Act
        var result = await checker.CheckStatusAsync("test-token");

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(StatusType.Invalid);
    }

    [Fact]
    public async Task CheckStatusAsync_WithPreferStatusListAndFallback_FallsBackWhenStatusListUnavailable()
    {
        // Arrange
        // Status list URI that doesn't exist will cause status list check to fail,
        // fallback to introspection should work
        var introspectionResponse = """{ "active": true }""";
        var httpClient = CreateMockHttpClient(introspectionResponse, HttpStatusCode.OK);

        var options = new HybridStatusOptions
        {
            Strategy = HybridStrategy.PreferStatusList,
            IntrospectionEndpoint = IntrospectionEndpoint,
            FallbackOnError = true,
            StatusListOptions = new StatusListOptions
            {
                EnableStatusChecking = true,
                FailOnStatusCheckError = true  // This causes exception to be thrown
            }
        };

        var statusClaim = new StatusClaim
        {
            StatusList = new StatusListReference
            {
                Uri = "https://invalid.example.com/nonexistent",  // Will fail
                Index = 0
            }
        };

        var checker = new HybridStatusChecker(options, httpClient: httpClient);

        // Act
        var result = await checker.CheckStatusAsync(statusClaim, async _ => _issuerKey, "test-token");

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(StatusType.Valid);
        result.UsedFallback.Should().BeTrue();
        result.Method.Should().Be(StatusCheckMethod.TokenIntrospection);
    }

    [Fact]
    public async Task CheckStatusAsync_WithParallelStrategy_ReturnsIntrospectionWhenAvailable()
    {
        // Arrange - introspection will succeed, status list will fail (no real server)
        var introspectionResponse = """{ "active": true }""";
        var httpClient = CreateMockHttpClient(introspectionResponse, HttpStatusCode.OK);

        var options = new HybridStatusOptions
        {
            Strategy = HybridStrategy.Parallel,
            IntrospectionEndpoint = IntrospectionEndpoint,
            ParallelTimeout = TimeSpan.FromSeconds(5),
            StatusListOptions = new StatusListOptions
            {
                EnableStatusChecking = true,
                FailOnStatusCheckError = true
            }
        };

        var statusClaim = new StatusClaim
        {
            StatusList = new StatusListReference
            {
                // This will fail as there's no actual status list server
                Uri = "https://invalid.example.com/status",
                Index = 0
            }
        };

        var checker = new HybridStatusChecker(options, httpClient: httpClient);

        // Act - parallel runs both, introspection should succeed
        var result = await checker.CheckStatusAsync(statusClaim, async _ => _issuerKey, "test-token");

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(StatusType.Valid);
    }

    #endregion

    #region Caching Tests

    [Fact]
    public async Task CheckStatusAsync_WithCachingEnabled_CachesIntrospectionResult()
    {
        // Arrange
        var callCount = 0;
        var introspectionResponse = """{ "active": true }""";
        var httpClient = CreateMockHttpClient(introspectionResponse, HttpStatusCode.OK, _ => callCount++);

        var options = new HybridStatusOptions
        {
            Strategy = HybridStrategy.IntrospectionOnly,
            IntrospectionEndpoint = IntrospectionEndpoint,
            EnableCaching = true,
            CacheDuration = TimeSpan.FromMinutes(5)
        };

        var checker = new HybridStatusChecker(options, httpClient: httpClient);

        // Act
        await checker.CheckStatusAsync("test-token");
        await checker.CheckStatusAsync("test-token"); // Should use cache

        // Assert
        callCount.Should().Be(1);
    }

    [Fact]
    public async Task CheckStatusAsync_WithCachingDisabled_AlwaysFetchesFresh()
    {
        // Arrange
        var callCount = 0;
        var introspectionResponse = """{ "active": true }""";
        var httpClient = CreateMockHttpClient(introspectionResponse, HttpStatusCode.OK, _ => callCount++);

        var options = new HybridStatusOptions
        {
            Strategy = HybridStrategy.IntrospectionOnly,
            IntrospectionEndpoint = IntrospectionEndpoint,
            EnableCaching = false
        };

        var checker = new HybridStatusChecker(options, httpClient: httpClient);

        // Act
        await checker.CheckStatusAsync("test-token");
        await checker.CheckStatusAsync("test-token");

        // Assert
        callCount.Should().Be(2);
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task CheckStatusAsync_WithNoFallback_ThrowsOnError()
    {
        // Arrange
        var options = new HybridStatusOptions
        {
            Strategy = HybridStrategy.StatusListOnly,  // No fallback possible
            FallbackOnError = false,
            StatusListOptions = new StatusListOptions
            {
                EnableStatusChecking = true,
                FailOnStatusCheckError = true
            }
        };

        var statusClaim = new StatusClaim
        {
            StatusList = new StatusListReference
            {
                // This will fail as there's no actual status list server
                Uri = "https://invalid.example.com/status",
                Index = 0
            }
        };

        var checker = new HybridStatusChecker(options);

        // Act
        var act = () => checker.CheckStatusAsync(statusClaim, async _ => _issuerKey);

        // Assert - should throw because status list fetch fails and no fallback
        await act.Should().ThrowAsync<Exception>();
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

    private class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly string _responseContent;
        private readonly HttpStatusCode _statusCode;
        private readonly Action<HttpRequestMessage>? _requestCallback;

        public MockHttpMessageHandler(
            string responseContent,
            HttpStatusCode statusCode,
            Action<HttpRequestMessage>? requestCallback = null)
        {
            _responseContent = responseContent;
            _statusCode = statusCode;
            _requestCallback = requestCallback;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            _requestCallback?.Invoke(request);

            return new HttpResponseMessage(_statusCode)
            {
                Content = new StringContent(_responseContent, System.Text.Encoding.UTF8, "application/json")
            };
        }
    }

    private class MockStatusListVerifier : StatusListVerifier
    {
        private readonly StatusType? _returnStatus;
        private readonly Exception? _throwException;
        private readonly int _delayMs;

        public bool WasCalled
        {
            get; private set;
        }

        public MockStatusListVerifier(StatusType returnStatus, int delayMs = 0)
            : base()
        {
            _returnStatus = returnStatus;
            _delayMs = delayMs;
        }

        public MockStatusListVerifier(Exception throwException)
            : base()
        {
            _throwException = throwException;
        }

        public new async Task<StatusCheckResult> CheckStatusAsync(
            StatusClaim statusClaim,
            Func<string, Task<SecurityKey>> issuerKeyProvider,
            StatusListOptions? options = null)
        {
            WasCalled = true;

            if (_delayMs > 0)
            {
                await Task.Delay(_delayMs);
            }

            if (_throwException != null)
            {
                throw _throwException;
            }

            return new StatusCheckResult
            {
                Status = _returnStatus!.Value,
                StatusValue = (int)_returnStatus.Value,
                RetrievedAt = DateTime.UtcNow
            };
        }
    }

    #endregion
}
