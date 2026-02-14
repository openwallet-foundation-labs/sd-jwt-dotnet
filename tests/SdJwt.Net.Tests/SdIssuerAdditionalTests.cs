using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.Issuer;
using SdJwt.Net.Models;
using SdJwt.Net.Serialization;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json.Nodes;
using Xunit;

namespace SdJwt.Net.Tests;

/// <summary>
/// Additional tests for SdIssuer to improve coverage
/// </summary>
public class SdIssuerAdditionalTests : TestBase
{
    [Fact]
    public void IssueAsJsonSerialization_WithValidClaims_ReturnsCorrectFormat()
    {
        // Arrange
        var issuer = new SdIssuer(IssuerSigningKey, SecurityAlgorithms.EcdsaSha256);
        var claims = new JwtPayload { { "sub", "user123" }, { "email", "test@example.com" } };
        var options = new SdIssuanceOptions { DisclosureStructure = new { email = true } };

        // Act
        var result = issuer.IssueAsJsonSerialization(claims, options);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Protected);
        Assert.NotNull(result.Payload);
        Assert.NotNull(result.Signature);
        Assert.Single(result.Header.Disclosures);
    }

    [Fact]
    public void IssueAsJsonSerialization_WithHolderPublicKey_IncludesCnfClaim()
    {
        // Arrange
        var issuer = new SdIssuer(IssuerSigningKey, SecurityAlgorithms.EcdsaSha256);
        var claims = new JwtPayload { { "sub", "user123" } };
        var options = new SdIssuanceOptions();

        // Act
        var result = issuer.IssueAsJsonSerialization(claims, options, HolderPublicJwk);

        // Assert
        Assert.NotNull(result);
        var compactForm = SdJwtJsonSerializer.FromFlattenedJsonSerialization(result);
        var jwt = new JwtSecurityToken(compactForm.Split('~')[0]);
        Assert.True(jwt.Payload.ContainsKey("cnf"));
    }

    [Fact]
    public void IssueAsGeneralJsonSerialization_WithValidClaims_ReturnsCorrectFormat()
    {
        // Arrange
        var issuer = new SdIssuer(IssuerSigningKey, SecurityAlgorithms.EcdsaSha256);
        var claims = new JwtPayload { { "sub", "user123" }, { "email", "test@example.com" } };
        var options = new SdIssuanceOptions { DisclosureStructure = new { email = true } };

        // Act
        var result = issuer.IssueAsGeneralJsonSerialization(claims, options);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Payload);
        Assert.Single(result.Signatures);
        Assert.NotNull(result.Signatures[0].Protected);
        Assert.NotNull(result.Signatures[0].Signature);
        Assert.Single(result.Signatures[0].Header.Disclosures);
    }

    [Fact]
    public void IssueAsGeneralJsonSerialization_WithAdditionalSignatures_IncludesAllSignatures()
    {
        // Arrange
        var issuer = new SdIssuer(IssuerSigningKey, SecurityAlgorithms.EcdsaSha256);
        var claims = new JwtPayload { { "sub", "user123" } };
        var options = new SdIssuanceOptions();

        var additionalSignature = new SdJwtSignature
        {
            Protected = "eyJhbGciOiJFUzI1NiJ9",
            Signature = "additional-signature",
            Header = new SdJwtUnprotectedHeader()
        };

        // Act
        var result = issuer.IssueAsGeneralJsonSerialization(claims, options, null, new[] { additionalSignature });

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Signatures.Length); // Original + additional
    }

    [Fact]
    public void Issue_WithDecoyDigests_AddsDecoys()
    {
        // Arrange
        var issuer = new SdIssuer(IssuerSigningKey, SecurityAlgorithms.EcdsaSha256);
        var claims = new JwtPayload { { "sub", "user123" }, { "email", "test@example.com" } };
        var options = new SdIssuanceOptions
        {
            DisclosureStructure = new { email = true },
            DecoyDigests = 3
        };

        // Act
        var result = issuer.Issue(claims, options);

        // Assert
        var jwt = new JwtSecurityToken(result.SdJwt);
        Assert.True(jwt.Payload.TryGetValue("_sd", out var sdValue));
        var sdJson = System.Text.Json.JsonSerializer.Serialize(sdValue);
        var sdArray = JsonNode.Parse(sdJson)!.AsArray();
        Assert.Equal(4, sdArray.Count); // 1 real + 3 decoys
    }

    [Fact]
    public void Issue_WithEmptyDisclosureStructure_CreatesNoDisclosures()
    {
        // Arrange
        var issuer = new SdIssuer(IssuerSigningKey, SecurityAlgorithms.EcdsaSha256);
        var claims = new JwtPayload { { "sub", "user123" }, { "email", "test@example.com" } };
        var options = new SdIssuanceOptions { DisclosureStructure = new { } };

        // Act
        var result = issuer.Issue(claims, options);

        // Assert
        Assert.Empty(result.Disclosures);
        var jwt = new JwtSecurityToken(result.SdJwt);
        Assert.True(jwt.Payload.ContainsKey("email"));
        Assert.False(jwt.Payload.ContainsKey("_sd"));
    }
}
