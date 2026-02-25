using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Moq;
using SdJwt.Net.StatusList.Issuer;
using SdJwt.Net.StatusList.Models;
using System.Collections;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text.Json;
using Xunit;
using StatusListModel = SdJwt.Net.StatusList.Models.StatusList;

namespace SdJwt.Net.StatusList.Tests.Issuer;

public class StatusListManagerEnhancedTests : IDisposable
{
    private readonly ECDsaSecurityKey _signingKey;
    private readonly Mock<ILogger<StatusListManager>> _loggerMock;
    private readonly StatusListManager _manager;

    public StatusListManagerEnhancedTests()
    {
        var ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        _signingKey = new ECDsaSecurityKey(ecdsa);
        _loggerMock = new Mock<ILogger<StatusListManager>>();
        _manager = new StatusListManager(_signingKey, SecurityAlgorithms.EcdsaSha256);
    }

    [Fact]
    public async Task CreateStatusListTokenAsync_WithValidInput_ShouldCreateValidToken()
    {
        // Arrange
        var subject = "https://example.com/status/123";
        var statusValues = new byte[] { 0, 1, 0, 1 }; // Valid, Invalid, Valid, Invalid
        var bits = 1;

        // Act
        var token = await _manager.CreateStatusListTokenAsync(subject, statusValues, bits);

        // Assert
        token.Should().NotBeNullOrEmpty();

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        jwt.Subject.Should().Be(subject);
        jwt.Header.Typ.Should().Be("statuslist+jwt");
    }

    [Fact]
    public async Task CreateStatusListTokenFromBitArrayAsync_ShouldWork()
    {
        // Arrange
        var subject = "https://example.com/status/123";
        var statusBits = new BitArray(16);
        var bits = 1;

        // Act
        var token = await _manager.CreateStatusListTokenFromBitArrayAsync(subject, statusBits, bits);

        // Assert
        token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task UpdateStatusAsync_ShouldUpdateStatusCorrectly()
    {
        // Arrange
        var subject = "https://example.com/status/123";
        var statusValues = new byte[] { 0, 0, 0, 0 }; // All valid initially
        var originalToken = await _manager.CreateStatusListTokenAsync(subject, statusValues, 1);

        var updates = new Dictionary<int, StatusType>
        {
            { 1, StatusType.Invalid },
            { 3, StatusType.Suspended }
        };

        // Act
        var updatedToken = await _manager.UpdateStatusAsync(originalToken, updates);

        // Assert
        updatedToken.Should().NotBeNullOrEmpty();
        updatedToken.Should().NotBe(originalToken);
    }

    [Fact]
    public async Task RevokeTokensAsync_ShouldRevokeSpecifiedTokens()
    {
        // Arrange
        var subject = "https://example.com/status/123";
        var statusValues = new byte[] { 0, 0, 0, 0 }; // All valid initially
        var originalToken = await _manager.CreateStatusListTokenAsync(subject, statusValues, 1);

        var indicesToRevoke = new int[] { 1, 3 };

        // Act
        var revokedToken = await _manager.RevokeTokensAsync(originalToken, indicesToRevoke);

        // Assert
        revokedToken.Should().NotBeNullOrEmpty();
        revokedToken.Should().NotBe(originalToken);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(3)]
    [InlineData(5)]
    [InlineData(7)]
    [InlineData(9)]
    public void SetCredentialStatus_WithValidIndex_ShouldSetStatus(int index)
    {
        // Arrange
        var credentialCount = 10;
        var bitsPerCredential = 1;
        var statusBits = _manager.CreateStatusBits(credentialCount, bitsPerCredential);

        // Act
        _manager.SetCredentialStatus(statusBits, index, StatusType.Invalid, bitsPerCredential);
        var retrievedStatus = _manager.GetCredentialStatus(statusBits, index, bitsPerCredential);

        // Assert
        retrievedStatus.Should().Be(StatusType.Invalid);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(4)]
    [InlineData(8)]
    public void CreateStatusBits_WithValidBitsPerCredential_ShouldCreateCorrectBitArray(int bitsPerCredential)
    {
        // Arrange
        var credentialCount = 10;

        // Act
        var statusBits = _manager.CreateStatusBits(credentialCount, bitsPerCredential);

        // Assert
        statusBits.Should().NotBeNull();
        statusBits.Length.Should().Be(credentialCount * bitsPerCredential);
    }

    [Fact]
    public void CreateStatusListAggregation_WithValidUris_ShouldCreateAggregation()
    {
        // Arrange
        var statusListUris = new[]
        {
            "https://example.com/status/list1",
            "https://example.com/status/list2"
        };

        // Act
        var aggregation = _manager.CreateStatusListAggregation(statusListUris);

        // Assert
        aggregation.Should().NotBeNullOrEmpty();

        var parsed = JsonSerializer.Deserialize<StatusListAggregation>(aggregation, SdJwtConstants.DefaultJsonSerializerOptions);
        parsed.Should().NotBeNull();
        parsed!.StatusLists.Should().HaveCount(2);
        parsed.StatusLists.Should().Contain(statusListUris);
    }

    [Fact]
    public async Task GetMultipleCredentialStatuses_ShouldReturnCorrectStatuses()
    {
        // Arrange
        var subject = "https://example.com/status/123";
        var credentialCount = 16;
        var bitsPerCredential = 2; // Support for multiple status types
        var statusBits = _manager.CreateStatusBits(credentialCount, bitsPerCredential);

        // Set some statuses
        _manager.SetCredentialStatus(statusBits, 0, StatusType.Valid, bitsPerCredential);
        _manager.SetCredentialStatus(statusBits, 1, StatusType.Invalid, bitsPerCredential);
        _manager.SetCredentialStatus(statusBits, 2, StatusType.Suspended, bitsPerCredential);

        // Convert BitArray to byte array for token creation
        var statusValues = ConvertBitArrayToStatusValues(statusBits, bitsPerCredential);
        var statusListToken = await _manager.CreateStatusListTokenAsync(subject, statusValues, bitsPerCredential);

        // Parse the token and extract the status list
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(statusListToken);
        var statusListClaim = jwt.Claims.FirstOrDefault(c => c.Type == "status_list")?.Value;
        statusListClaim.Should().NotBeNullOrEmpty();

        // Use StatusListModel instead of StatusList to avoid namespace conflict
        var statusList = JsonSerializer.Deserialize<StatusListModel>(statusListClaim!, SdJwtConstants.DefaultJsonSerializerOptions);
        statusList.Should().NotBeNull();

        // Assert
        var retrievedBits = StatusListManager.GetBitsFromToken(statusListToken);
        var status0 = _manager.GetCredentialStatus(retrievedBits, 0, bitsPerCredential);
        var status1 = _manager.GetCredentialStatus(retrievedBits, 1, bitsPerCredential);
        var status2 = _manager.GetCredentialStatus(retrievedBits, 2, bitsPerCredential);

        status0.Should().Be(StatusType.Valid);
        status1.Should().Be(StatusType.Invalid);
        status2.Should().Be(StatusType.Suspended);

        statusList!.Bits.Should().Be(bitsPerCredential);
    }

    [Fact]
    public void CreateStatusBits_WithZeroCredentials_ShouldThrowArgumentException()
    {
        // Act & Assert
        var action = () => _manager.CreateStatusBits(0, 1);
        action.Should().Throw<ArgumentException>()
            .WithMessage("*Credential count must be positive*");
    }

    [Fact]
    public void CreateStatusBits_WithInvalidBitsPerCredential_ShouldThrowArgumentException()
    {
        // Act & Assert
        var action = () => _manager.CreateStatusBits(10, 3); // 3 is not valid
        action.Should().Throw<ArgumentException>()
            .WithMessage("*Bits per credential must be 1, 2, 4, or 8*");
    }

    [Fact]
    public void SetCredentialStatus_WithNegativeIndex_ShouldThrowArgumentException()
    {
        // Arrange
        var statusBits = _manager.CreateStatusBits(10, 1);

        // Act & Assert
        var action = () => _manager.SetCredentialStatus(statusBits, -1, StatusType.Invalid, 1);
        action.Should().Throw<ArgumentException>()
            .WithMessage("*Credential index must be non-negative*");
    }

    [Fact]
    public void SetCredentialStatus_WithIndexOutOfRange_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        var statusBits = _manager.CreateStatusBits(5, 1);

        // Act & Assert
        var action = () => _manager.SetCredentialStatus(statusBits, 10, StatusType.Invalid, 1); // Index 10 > 5 credentials
        action.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public async Task SuspendTokensAsync_ShouldSuspendSpecifiedTokens()
    {
        // Arrange
        var subject = "https://example.com/status/123";
        var statusValues = new byte[] { 0, 0, 0, 0 }; // All valid initially
        var originalToken = await _manager.CreateStatusListTokenAsync(subject, statusValues, 1);

        var indicesToSuspend = new int[] { 1, 3 };

        // Act
        var suspendedToken = await _manager.SuspendTokensAsync(originalToken, indicesToSuspend);

        // Assert
        suspendedToken.Should().NotBeNullOrEmpty();
        suspendedToken.Should().NotBe(originalToken);
    }

    [Fact]
    public async Task ReinstateTokensAsync_ShouldReinstateSpecifiedTokens()
    {
        // Arrange
        var subject = "https://example.com/status/123";
        var statusValues = new byte[] { 1, 1, 1, 1 }; // All invalid initially
        var originalToken = await _manager.CreateStatusListTokenAsync(subject, statusValues, 1);

        var indicesToReinstate = new int[] { 1, 3 };

        // Act
        var reinstatedToken = await _manager.ReinstateTokensAsync(originalToken, indicesToReinstate);

        // Assert
        reinstatedToken.Should().NotBeNullOrEmpty();
        reinstatedToken.Should().NotBe(originalToken);
    }

    // Helper method to convert BitArray to status values
    private static byte[] ConvertBitArrayToStatusValues(BitArray bitArray, int bits)
    {
        var statusCount = bitArray.Length / bits;
        var statusValues = new byte[statusCount];

        for (int i = 0; i < statusCount; i++)
        {
            byte statusValue = 0;
            for (int bit = 0; bit < bits; bit++)
            {
                var bitIndex = i * bits + bit;
                if (bitIndex < bitArray.Length && bitArray[bitIndex])
                {
                    statusValue |= (byte)(1 << bit);
                }
            }
            statusValues[i] = statusValue;
        }

        return statusValues;
    }

    public void Dispose()
    {
        // ECDsaSecurityKey doesn't implement IDisposable directly, but the underlying ECDsa does
        _signingKey?.ECDsa?.Dispose();
    }
}
