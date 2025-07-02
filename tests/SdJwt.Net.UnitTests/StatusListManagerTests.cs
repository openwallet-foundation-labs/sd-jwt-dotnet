using System.Collections;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.Issuer;
using Xunit;

namespace SdJwt.Net.Tests;

public class StatusListManagerTests : TestBase
{
    [Fact]
    public void Constructor_WithNulls_ThrowsException()
    {
        Assert.Throws<ArgumentNullException>(() => new StatusListManager(null!, IssuerSigningAlgorithm));
        Assert.Throws<ArgumentException>(() => new StatusListManager(IssuerSigningKey, " "));
    }

    [Fact]
    public void CreateStatusListCredential_WithNulls_ThrowsException()
    {
        var manager = new StatusListManager(IssuerSigningKey, IssuerSigningAlgorithm);
        Assert.Throws<ArgumentException>(() => manager.CreateStatusListCredential(" ", new BitArray(1)));
        Assert.Throws<ArgumentNullException>(() => manager.CreateStatusListCredential(TrustedIssuer, null!));
    }

    [Fact]
    public void CreateStatusListCredential_GeneratesCorrectlyStructuredJwt()
    {
        // Arrange
        var manager = new StatusListManager(IssuerSigningKey, IssuerSigningAlgorithm);
        var statusBits = new BitArray(1000, false);
        statusBits[123] = true;
        statusBits[456] = true;

        // Act
        var statusListJwt = manager.CreateStatusListCredential(TrustedIssuer, statusBits, "jwt_id_1");
        var token = new JwtSecurityToken(statusListJwt);

        // Assert
        Assert.Equal("statuslist+jwt", token.Header.Typ);
        Assert.Equal(TrustedIssuer, token.Issuer);
        Assert.NotNull(token.Payload.Sub);
        Assert.Equal("jwt_id_1", token.Payload.Jti);

        var statusListClaim = token.Payload.Claims.FirstOrDefault(c => c.Type == "status_list");
        Assert.NotNull(statusListClaim);
        Assert.Equal(JsonClaimValueTypes.Json, statusListClaim.ValueType);

        var statusListObject = JsonDocument.Parse(statusListClaim.Value).RootElement;
        Assert.Equal(1000, statusListObject.GetProperty("len").GetInt32());
        Assert.Equal(1, statusListObject.GetProperty("bits").GetInt32());
    }
}