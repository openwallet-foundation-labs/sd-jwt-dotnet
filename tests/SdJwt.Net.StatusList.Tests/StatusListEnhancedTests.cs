using SdJwt.Net.StatusList.Issuer;
using SdJwt.Net.StatusList.Models;
using System.Collections;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using Xunit;

namespace SdJwt.Net.StatusList.Tests;

public class StatusListEnhancedTests : TestBase
{
    [Fact]
    public async Task StatusListManager_CreateStatusListFromMultipleSizes_ShouldHandleCorrectly()
    {
        var manager = new StatusListManager(IssuerSigningKey, IssuerSigningAlgorithm);

        // Test different sizes
        var sizes = new[] { 100, 1000, 10000 };

        foreach (var size in sizes)
        {
            var statusBits = new BitArray(size, false);
            // Set some random bits
            statusBits[size / 4] = true;
            statusBits[size / 2] = true;
            statusBits[size - 1] = true;

            var statusListJwt = await manager.CreateStatusListTokenFromBitArrayAsync(TrustedIssuer, statusBits, 1);

            var token = new JwtSecurityToken(statusListJwt);
            Assert.Equal("statuslist+jwt", token.Header.Typ);
            Assert.Equal(TrustedIssuer, token.Payload.Claims.FirstOrDefault(c => c.Type == "sub")?.Value);
        }
    }

    [Fact]
    public void StatusTypeExtensions_AllEnumValues_ShouldConvertCorrectly()
    {
        // Test Valid status
        Assert.Equal("valid", StatusType.Valid.ToStringValue());

        // Test Invalid status
        Assert.Equal("invalid", StatusType.Invalid.ToStringValue());

        // Test Suspended status
        Assert.Equal("suspended", StatusType.Suspended.ToStringValue());

        // Test UnderInvestigation status
        Assert.Equal("under_investigation", StatusType.UnderInvestigation.ToStringValue());
    }

    [Fact]
    public void StatusTypeExtensions_FromString_ShouldParseAllValidValues()
    {
        // Test parsing valid strings
        Assert.Equal(StatusType.Valid, StatusTypeExtensions.FromString("valid"));
        Assert.Equal(StatusType.Invalid, StatusTypeExtensions.FromString("invalid"));
        Assert.Equal(StatusType.Suspended, StatusTypeExtensions.FromString("suspended"));
        Assert.Equal(StatusType.UnderInvestigation, StatusTypeExtensions.FromString("under_investigation"));

        // Test case insensitivity
        Assert.Equal(StatusType.Valid, StatusTypeExtensions.FromString("VALID"));
        Assert.Equal(StatusType.Invalid, StatusTypeExtensions.FromString("Invalid"));
    }

    [Fact]
    public void StatusListReference_Validation_ShouldEnforceRequiredFields()
    {
        var reference = new StatusListReference
        {
            Uri = "https://issuer.example.com/status/123",
            Index = 42
        };

        // Should validate successfully
        reference.Validate();

        // Test invalid URI
        reference.Uri = "not-a-url";
        Assert.Throws<InvalidOperationException>(() => reference.Validate());

        // Test negative index
        reference.Uri = "https://issuer.example.com/status/123";
        reference.Index = -1;
        Assert.Throws<InvalidOperationException>(() => reference.Validate());
    }

    [Fact]
    public void StatusClaim_Validation_ShouldEnforceRequiredStatusList()
    {
        var statusClaim = new StatusClaim
        {
            StatusList = new StatusListReference
            {
                Uri = "https://issuer.example.com/status/123",
                Index = 42
            }
        };

        // Should validate successfully
        statusClaim.Validate();

        // Test null status list
        statusClaim.StatusList = null;
        Assert.Throws<InvalidOperationException>(() => statusClaim.Validate());
    }

    [Fact]
    public void StatusCheckResult_FactoryMethods_ShouldCreateCorrectResults()
    {
        // Test success result
        var successResult = StatusCheckResult.Success();
        Assert.True(successResult.IsValid);
        Assert.True(successResult.IsActive);
        Assert.Null(successResult.ErrorMessage);

        // Test revoked result
        var revokedResult = StatusCheckResult.Revoked();
        Assert.False(revokedResult.IsValid); // Invalid status means not valid
        Assert.False(revokedResult.IsActive);
        Assert.Null(revokedResult.ErrorMessage);

        // Test suspended result
        var suspendedResult = StatusCheckResult.Suspended();
        Assert.False(suspendedResult.IsValid); // Suspended is not valid
        Assert.False(suspendedResult.IsActive);
        Assert.Null(suspendedResult.ErrorMessage);

        // Test failed result
        var failedResult = StatusCheckResult.Failed("Test error");
        Assert.False(failedResult.IsValid);
        Assert.False(failedResult.IsActive);
        Assert.Equal("Test error", failedResult.ErrorMessage);
    }

    [Fact]
    public void StatusListData_Validation_ShouldEnforceConstraints()
    {
        var data = new StatusListData
        {
            Bits = 1,
            Data = new byte[] { 0xAB, 0xCD }
        };

        // Should validate successfully
        data.Validate();

        // Test invalid bits
        data.Bits = 0;
        Assert.Throws<InvalidOperationException>(() => data.Validate());

        data.Bits = 9; // Assuming 8 is max
        Assert.Throws<InvalidOperationException>(() => data.Validate());

        // Test null data
        data.Bits = 1;
        data.Data = null!;
        Assert.Throws<InvalidOperationException>(() => data.Validate());

        // Test empty data
        data.Data = Array.Empty<byte>();
        Assert.Throws<InvalidOperationException>(() => data.Validate());
    }

    [Fact]
    public async Task StatusListManager_CreateTokenWithDifferentBitSizes_ShouldWork()
    {
        var manager = new StatusListManager(IssuerSigningKey, IssuerSigningAlgorithm);

        // Test with 1-bit per entry
        var statusBits1 = new BitArray(1000, false);
        statusBits1[100] = true;

        var jwt1 = await manager.CreateStatusListTokenFromBitArrayAsync(TrustedIssuer, statusBits1, 1);
        var token1 = new JwtSecurityToken(jwt1);

        var statusListClaim1 = token1.Payload.Claims.FirstOrDefault(c => c.Type == "status_list");
        Assert.NotNull(statusListClaim1);

        var statusListObject1 = JsonDocument.Parse(statusListClaim1.Value).RootElement;
        Assert.Equal(1, statusListObject1.GetProperty("bits").GetInt32());

        // Test with 2-bit per entry  
        var jwt2 = await manager.CreateStatusListTokenFromBitArrayAsync(TrustedIssuer, statusBits1, 2);
        var token2 = new JwtSecurityToken(jwt2);

        var statusListClaim2 = token2.Payload.Claims.FirstOrDefault(c => c.Type == "status_list");
        Assert.NotNull(statusListClaim2);

        var statusListObject2 = JsonDocument.Parse(statusListClaim2.Value).RootElement;
        Assert.Equal(2, statusListObject2.GetProperty("bits").GetInt32());
    }
}
