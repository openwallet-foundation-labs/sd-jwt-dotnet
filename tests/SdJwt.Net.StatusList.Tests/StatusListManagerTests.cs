using SdJwt.Net.StatusList.Issuer;
using System.Collections;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using Xunit;

namespace SdJwt.Net.StatusList.Tests;

public class StatusListManagerTests : TestBase
{
    [Fact]
    public void Constructor_WithNulls_ThrowsException()
    {
        Assert.Throws<ArgumentNullException>(() => new StatusListManager(null!, IssuerSigningAlgorithm));
        Assert.Throws<ArgumentException>(() => new StatusListManager(IssuerSigningKey, " "));
    }

    [Fact]
    public async Task CreateStatusListTokenAsync_WithNulls_ThrowsException()
    {
        var manager = new StatusListManager(IssuerSigningKey, IssuerSigningAlgorithm);
        await Assert.ThrowsAsync<ArgumentException>(() => manager.CreateStatusListTokenAsync(" ", new byte[1]));
        await Assert.ThrowsAsync<ArgumentNullException>(() => manager.CreateStatusListTokenAsync(TrustedIssuer, null!));
    }

    [Fact]
    public async Task CreateStatusListTokenFromBitArrayAsync_GeneratesCorrectlyStructuredJwt()
    {
        // Arrange
        var manager = new StatusListManager(IssuerSigningKey, IssuerSigningAlgorithm);
        var statusBits = new BitArray(1000, false);
        statusBits[123] = true;
        statusBits[456] = true;

        // Act
        var statusListJwt = await manager.CreateStatusListTokenFromBitArrayAsync(TrustedIssuer, statusBits, 1);
        var token = new JwtSecurityToken(statusListJwt);

        // Assert
        Assert.Equal("statuslist+jwt", token.Header.Typ);
        Assert.Equal(TrustedIssuer, token.Payload.Claims.FirstOrDefault(c => c.Type == "sub")?.Value);

        var statusListClaim = token.Payload.Claims.FirstOrDefault(c => c.Type == "status_list");
        Assert.NotNull(statusListClaim);
        Assert.Equal(JsonClaimValueTypes.Json, statusListClaim.ValueType);

        var statusListObject = JsonDocument.Parse(statusListClaim.Value).RootElement;
        Assert.Equal(1, statusListObject.GetProperty("bits").GetInt32());
        Assert.True(statusListObject.TryGetProperty("lst", out var lst));
    }
}