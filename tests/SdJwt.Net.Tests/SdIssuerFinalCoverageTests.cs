using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.Issuer;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;
using Xunit;

namespace SdJwt.Net.Tests;

public class SdIssuerFinalCoverageTests : TestBase
{
    [Fact]
    public void Issue_WithUndisclosedArrayContainingNestedDisclosures_CoversProcessNodeBranch()
    {
        // Arrange
        var claims = new JwtPayload
        {
            { "my_array", new[] { new { sub_claim = "secret" }, new { sub_claim = "secret2" } } }
        };

        // Disclosure structure: my_array[0].sub_claim is disclosed
        // my_array itself is NOT disclosed
        var disclosureStructure = new Dictionary<string, object>
        {
            { "my_array", new[] { 
                new Dictionary<string, object> { { "sub_claim", true } },
                new Dictionary<string, object> { { "sub_claim", true } }
            } }
        };

        var options = new SdIssuanceOptions
        {
            DisclosureStructure = disclosureStructure
        };

        var issuer = new SdIssuer(IssuerSigningKey, SecurityAlgorithms.EcdsaSha256);

        // Act
        var output = issuer.Issue(claims, options);

        // Assert
        // Verify that the array is still there, but the item inside is processed
        var payload = output.SdJwt.Split('.')[1];
        var json = Base64UrlEncoder.Decode(payload);
        var jsonNode = JsonNode.Parse(json);
        
        Assert.NotNull(jsonNode!["my_array"]);
        Assert.IsType<JsonArray>(jsonNode["my_array"]);
        
        // The item inside should be an object with _sd
        var array = jsonNode["my_array"]!.AsArray();
        var item = array[0]!.AsObject();
        Assert.NotNull(item[SdJwtConstants.SdClaim]);

        var item2 = array[1]!.AsObject();
        Assert.NotNull(item2[SdJwtConstants.SdClaim]);
        
        // And we should have 2 disclosures (one for each item)
        Assert.Equal(2, output.Disclosures.Count);
    }

    [Fact]
    public void Issue_WithArrayContainingUndisclosedItems_CoversArrayLoopBranch()
    {
        // Arrange
        var claims = new JwtPayload
        {
            { "mixed_array", new[] { "disclose_me", "keep_me" } }
        };

        // Disclosure structure: index 0 is true, index 1 is false (or missing)
        var disclosureStructure = new Dictionary<string, object>
        {
            { "mixed_array", new object[] { true, false } }
        };

        var options = new SdIssuanceOptions
        {
            DisclosureStructure = disclosureStructure
        };

        var issuer = new SdIssuer(IssuerSigningKey, SecurityAlgorithms.EcdsaSha256);

        // Act
        var output = issuer.Issue(claims, options);

        // Assert
        var payload = output.SdJwt.Split('.')[1];
        var json = Base64UrlEncoder.Decode(payload);
        var jsonNode = JsonNode.Parse(json);
        
        var array = jsonNode!["mixed_array"]!.AsArray();
        
        // Index 0 should be replaced by object with "..."
        Assert.IsType<JsonObject>(array[0]);
        Assert.True(array[0]!.AsObject().ContainsKey("..."));
        
        // Index 1 should remain as string "keep_me"
        Assert.IsAssignableFrom<JsonValue>(array[1]);
        Assert.Equal("keep_me", array[1]!.GetValue<string>());
        
        Assert.Single(output.Disclosures);
    }
}
