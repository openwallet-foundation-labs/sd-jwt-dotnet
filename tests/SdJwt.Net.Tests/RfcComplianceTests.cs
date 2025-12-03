using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.Internal;
using SdJwt.Net.Issuer;
using SdJwt.Net.Models;
using SdJwt.Net.Serialization;
using SdJwt.Net.Verifier;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Nodes;
using Xunit;

namespace SdJwt.Net.Tests;

public class RfcComplianceTests : TestBase
{
    [Fact]
    public void ArrayElementDisclosure_ShouldHaveTwoElements()
    {
        var issuer = new SdIssuer(IssuerSigningKey, "ES256");
        var claims = new JwtPayload
        {
            { "array_claim", new object[] { "element1" } }
        };

        var options = new SdIssuanceOptions();
        // options.DisclosureStructure.Add("array_claim", new JsonArray { new JsonObject { { "alg", "sha-256" } } }); // Just to trigger disclosure, structure might be simpler
        // Actually, simpler way to force disclosure for array elements:
        var options2 = new SdIssuanceOptions();
        options2.DisclosureStructure = new JsonObject
        {
            { "array_claim", new JsonArray { true } }
        };

        var issuance = issuer.Issue(claims, options2);
        
        // Find the disclosure for the array element
        var disclosureString = issuance.Disclosures.First().EncodedValue;
        var decodedBytes = Base64UrlEncoder.DecodeBytes(disclosureString);
        var jsonArray = JsonSerializer.Deserialize<JsonElement[]>(decodedBytes);

        Assert.NotNull(jsonArray);
        Assert.Equal(2, jsonArray.Length); // MUST be 2 elements: [salt, value]
        Assert.Equal("element1", jsonArray[1].GetString());
    }

    [Fact]
    public async Task VerifyAsync_WithArrayElementDisclosure_ShouldVerifyCorrectly()
    {
        var issuer = new SdIssuer(IssuerSigningKey, "ES256");
        var claims = new JwtPayload
        {
            { "array_claim", new object[] { "element1", "element2" } }
        };

        var options = new SdIssuanceOptions();
        // Disclose both elements
        options.DisclosureStructure = new JsonObject
        {
            { "array_claim", new JsonArray { true, true } }
        };

        var issuance = issuer.Issue(claims, options);

        var verifier = new SdVerifier((_) => Task.FromResult<SecurityKey>(IssuerSigningKey));
        var validationParams = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = false,
            IssuerSigningKey = IssuerSigningKey
        };

        var result = await verifier.VerifyAsync(issuance.Issuance, validationParams);

        Assert.NotNull(result.ClaimsPrincipal);
        var claim = result.ClaimsPrincipal.FindFirst("array_claim");
        Assert.NotNull(claim);
        var rehydratedArray = JsonSerializer.Deserialize<string[]>(claim.Value);
        Assert.Equal(new[] { "element1", "element2" }, rehydratedArray);
    }
}
