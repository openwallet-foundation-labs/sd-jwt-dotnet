using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Moq.Protected;
using SdJwt.Net.StatusList.Models;
using SdJwt.Net.StatusList.Verifier;
using System.Collections;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Cryptography;
using System.Text.Json;
using System.IO.Compression;
using Xunit;
using StatusListModel = SdJwt.Net.StatusList.Models.StatusList;

namespace SdJwt.Net.StatusList.Tests.Verifier;

public class StatusListVerifierTests : IDisposable
{
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly HttpClient _httpClient;
    private readonly ECDsaSecurityKey _signingKey;
    private readonly Mock<ILogger<StatusListVerifier>> _loggerMock;

    public StatusListVerifierTests()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        
        var ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        _signingKey = new ECDsaSecurityKey(ecdsa);
        
        _loggerMock = new Mock<ILogger<StatusListVerifier>>();
    }

    [Fact]
    public void Constructor_WithNullHttpClient_ShouldCreateDefaultHttpClient()
    {
        // Act & Assert - The constructor should not throw with null httpClient as it creates a default one
        var act = () => new StatusListVerifier(null);
        act.Should().NotThrow();
    }

    [Fact]
    public void Constructor_WithNullOptions_ShouldUseDefaults()
    {
        // Act & Assert
        var act = () => new StatusListVerifier(_httpClient, null);
        act.Should().NotThrow();
    }

    [Fact]
    public void Constructor_WithValidParameters_ShouldSucceed()
    {
        // Act & Assert
        var act = () => new StatusListVerifier(_httpClient, null, _loggerMock.Object);
        act.Should().NotThrow();
    }

    [Fact]
    public async Task CheckStatusAsync_WithNullStatusClaim_ShouldThrow()
    {
        // Arrange
        var verifier = new StatusListVerifier(_httpClient);

        // Act & Assert
        var act = async () => await verifier.CheckStatusAsync(null!, _ => Task.FromResult<SecurityKey>(_signingKey));
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*statusClaim*");
    }

    [Fact]
    public async Task CheckStatusAsync_WithNullKeyProvider_ShouldThrow()
    {
        // Arrange
        var verifier = new StatusListVerifier(_httpClient);
        var statusClaim = CreateValidStatusClaim();

        // Act & Assert
        var act = async () => await verifier.CheckStatusAsync(statusClaim, null!);
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithMessage("*issuerKeyProvider*");
    }

    [Fact]
    public async Task CheckStatusAsync_WithInvalidStatusClaim_ShouldReturnFailed()
    {
        // Arrange
        var verifier = new StatusListVerifier(_httpClient);
        var statusClaim = new StatusClaim
        {
            StatusList = new StatusListReference
            {
                Uri = "invalid-url", // Invalid URI
                Index = 42
            }
        };

        var options = new StatusListOptions 
        { 
            EnableStatusChecking = true,
            FailOnStatusCheckError = false // Don't throw, return failed result
        };

        // Act
        var result = await verifier.CheckStatusAsync(statusClaim, _ => Task.FromResult<SecurityKey>(_signingKey), options);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(StatusType.Invalid);
    }

    [Fact]
    public async Task CheckStatusAsync_WithHttpFailure_ShouldReturnFailed()
    {
        // Arrange
        var statusListUri = "https://issuer.example.com/status/123";
        var statusClaim = CreateValidStatusClaim(statusListUri);
        
        SetupHttpResponse(statusListUri, null, HttpStatusCode.NotFound);
        
        var verifier = new StatusListVerifier(_httpClient, null, _loggerMock.Object);
        var options = new StatusListOptions 
        { 
            EnableStatusChecking = true,
            FailOnStatusCheckError = false // Return failed result instead of throwing
        };

        // Act
        var result = await verifier.CheckStatusAsync(statusClaim, _ => Task.FromResult<SecurityKey>(_signingKey), options);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(StatusType.Invalid);
    }

    [Fact]
    public async Task CheckStatusAsync_WithInvalidJwt_ShouldReturnFailed()
    {
        // Arrange
        var statusListUri = "https://issuer.example.com/status/123";
        var statusClaim = CreateValidStatusClaim(statusListUri);
        
        SetupHttpResponse(statusListUri, "invalid-jwt-content");
        
        var verifier = new StatusListVerifier(_httpClient, null, _loggerMock.Object);
        var options = new StatusListOptions 
        { 
            EnableStatusChecking = true,
            FailOnStatusCheckError = false // Return failed result instead of throwing
        };

        // Act
        var result = await verifier.CheckStatusAsync(statusClaim, _ => Task.FromResult<SecurityKey>(_signingKey), options);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(StatusType.Invalid);
    }

    [Fact]
    public async Task CheckStatusAsync_WithValidActiveStatus_ShouldReturnSuccess()
    {
        // Arrange
        var statusListUri = "https://issuer.example.com/status/123";
        var statusIndex = 42;
        var statusClaim = CreateValidStatusClaim(statusListUri, statusIndex);
        
        // Create a status list with the specified index set to 0 (active)
        var statusListJwt = CreateValidStatusListJwt(statusListUri, statusIndex, StatusType.Valid);
        
        SetupHttpResponse(statusListUri, statusListJwt);
        
        var verifier = new StatusListVerifier(_httpClient, null, _loggerMock.Object);
        var options = new StatusListOptions 
        { 
            EnableStatusChecking = true,
            ValidateStatusListTiming = false // Disable timing validation for tests
        };

        // Act
        var result = await verifier.CheckStatusAsync(statusClaim, _ => Task.FromResult<SecurityKey>(_signingKey), options);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(StatusType.Valid);
    }

    [Fact]
    public async Task CheckStatusAsync_WithValidRevokedStatus_ShouldReturnRevoked()
    {
        // Arrange
        var statusListUri = "https://issuer.example.com/status/123";
        var statusIndex = 42;
        var statusClaim = CreateValidStatusClaim(statusListUri, statusIndex);
        
        // Create a status list with the specified index set to 1 (revoked)
        var statusListJwt = CreateValidStatusListJwt(statusListUri, statusIndex, StatusType.Invalid);
        
        SetupHttpResponse(statusListUri, statusListJwt);
        
        var verifier = new StatusListVerifier(_httpClient, null, _loggerMock.Object);
        var options = new StatusListOptions 
        { 
            EnableStatusChecking = true,
            ValidateStatusListTiming = false
        };

        // Act
        var result = await verifier.CheckStatusAsync(statusClaim, _ => Task.FromResult<SecurityKey>(_signingKey), options);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(StatusType.Invalid);
    }

    [Fact]
    public async Task CheckStatusAsync_WithSuspensionStatus_ShouldHandleCorrectly()
    {
        // Arrange
        var statusListUri = "https://issuer.example.com/status/123";
        var statusIndex = 42;
        var statusClaim = CreateValidStatusClaim(statusListUri, statusIndex);
        
        // Create a status list with the specified index set to 2 (suspended) using 2 bits per status
        var statusListJwt = CreateValidStatusListJwt(statusListUri, statusIndex, StatusType.Suspended, 2);
        
        SetupHttpResponse(statusListUri, statusListJwt);
        
        var verifier = new StatusListVerifier(_httpClient, null, _loggerMock.Object);
        var options = new StatusListOptions 
        { 
            EnableStatusChecking = true,
            ValidateStatusListTiming = false
        };

        // Act
        var result = await verifier.CheckStatusAsync(statusClaim, _ => Task.FromResult<SecurityKey>(_signingKey), options);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(StatusType.Suspended);
    }

    [Fact]
    public async Task CheckStatusAsync_WithIndexOutOfRange_ShouldReturnFailed()
    {
        // Arrange
        var statusListUri = "https://issuer.example.com/status/123";
        var statusIndex = 2000; // Larger than status list size
        var statusClaim = CreateValidStatusClaim(statusListUri, statusIndex);
        
        var statusListJwt = CreateValidStatusListJwt(statusListUri, 10, StatusType.Valid); // Small status list
        
        SetupHttpResponse(statusListUri, statusListJwt);
        
        var verifier = new StatusListVerifier(_httpClient, null, _loggerMock.Object);
        var options = new StatusListOptions 
        { 
            EnableStatusChecking = true,
            ValidateStatusListTiming = false,
            FailOnStatusCheckError = false
        };

        // Act
        var result = await verifier.CheckStatusAsync(statusClaim, _ => Task.FromResult<SecurityKey>(_signingKey), options);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(StatusType.Invalid);
    }

    [Fact]
    public async Task CheckStatusAsync_WithExpiredStatusList_ShouldReturnFailed()
    {
        // Arrange
        var statusListUri = "https://issuer.example.com/status/123";
        var statusClaim = CreateValidStatusClaim(statusListUri);
        
        var expiredTime = DateTimeOffset.UtcNow.AddHours(-1); // Expired 1 hour ago
        var statusListJwt = CreateValidStatusListJwt(statusListUri, 42, StatusType.Valid, 1, expiredTime);
        
        SetupHttpResponse(statusListUri, statusListJwt);
        
        var verifier = new StatusListVerifier(_httpClient, null, _loggerMock.Object);
        var options = new StatusListOptions 
        { 
            EnableStatusChecking = true, 
            ValidateStatusListTiming = true, // Enable timing validation
            FailOnStatusCheckError = false 
        };

        // Act
        var result = await verifier.CheckStatusAsync(statusClaim, _ => Task.FromResult<SecurityKey>(_signingKey), options);

        // Assert - The verifier should handle the expired token gracefully and return a failed result
        result.Should().NotBeNull();
        result.Status.Should().Be(StatusType.Invalid);
    }

    [Fact]
    public async Task CheckStatusAsync_WithHttpException_ShouldReturnFailed()
    {
        // Arrange
        var statusClaim = CreateValidStatusClaim();
        
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ThrowsAsync(new HttpRequestException("Network error"));

        var verifier = new StatusListVerifier(_httpClient, null, _loggerMock.Object);
        var options = new StatusListOptions 
        { 
            EnableStatusChecking = true,
            FailOnStatusCheckError = false
        };

        // Act
        var result = await verifier.CheckStatusAsync(statusClaim, _ => Task.FromResult<SecurityKey>(_signingKey), options);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(StatusType.Invalid);
    }

    private StatusClaim CreateValidStatusClaim(string uri = "https://issuer.example.com/status/123", 
        int index = 42)
    {
        return new StatusClaim
        {
            StatusList = new StatusListReference
            {
                Uri = uri,
                Index = index
            }
        };
    }

    private string CreateValidStatusListJwt(string issuer, int targetIndex, StatusType targetStatus, int bits = 1, DateTimeOffset? expiry = null)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var now = DateTimeOffset.UtcNow;
        var exp = expiry ?? now.AddDays(1);

        // Create a proper status list with compressed data
        var statusCount = Math.Max(targetIndex + 1, 100); // Ensure we have enough space
        var statusData = new byte[statusCount];
        
        // Set all statuses to valid (0) by default
        for (int i = 0; i < statusCount; i++)
        {
            statusData[i] = 0;
        }
        
        // Set the target status
        if (targetIndex < statusCount)
        {
            statusData[targetIndex] = (byte)targetStatus;
        }

        // Convert to bit array and then compress properly
        var bitArray = new BitArray(statusCount * bits);
        for (int i = 0; i < statusCount; i++)
        {
            var statusValue = statusData[i];
            for (int bit = 0; bit < bits; bit++)
            {
                var bitIndex = i * bits + bit;
                if (bitIndex < bitArray.Length)
                {
                    bitArray[bitIndex] = (statusValue & (1 << bit)) != 0;
                }
            }
        }

        // Convert BitArray to bytes
        var bytes = new byte[(bitArray.Length + 7) / 8];
        bitArray.CopyTo(bytes, 0);

        // Compress the data using DEFLATE (as required by the spec)
        var compressedData = CompressData(bytes);

        var statusListData = new StatusListModel
        {
            Bits = bits,
            List = Base64UrlEncoder.Encode(compressedData)
        };

        var claims = new List<System.Security.Claims.Claim>
        {
            new("sub", issuer),
            new("iat", now.ToUnixTimeSeconds().ToString()),
            new("exp", exp.ToUnixTimeSeconds().ToString()),
            new("status_list", JsonSerializer.Serialize(statusListData, SdJwtConstants.DefaultJsonSerializerOptions))
        };

        var credentials = new SigningCredentials(_signingKey, SecurityAlgorithms.EcdsaSha256);
        var header = new JwtHeader(credentials)
        {
            ["typ"] = "statuslist+jwt"
        };

        var jwtPayload = new JwtPayload(claims);
        var token = new JwtSecurityToken(header, jwtPayload);

        return tokenHandler.WriteToken(token);
    }

    private static byte[] CompressData(byte[] data)
    {
        using var output = new MemoryStream();
        using (var deflate = new DeflateStream(output, CompressionMode.Compress, true))
        {
            deflate.Write(data, 0, data.Length);
        }
        return output.ToArray();
    }

    private void SetupHttpResponse(string url, string? content, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        var responseMessage = new HttpResponseMessage(statusCode);
        if (content != null)
        {
            responseMessage.Content = new StringContent(content);
        }

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri!.ToString() == url),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(responseMessage);
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
        _signingKey?.ECDsa?.Dispose();
    }
}
