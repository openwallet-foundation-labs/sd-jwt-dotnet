using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.Issuer;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json.Nodes;
using Xunit;

namespace SdJwt.Net.Core.Tests;

public class SdIssuerTests : TestBase
{
    [Fact]
    public void Constructor_WithNulls_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new SdIssuer(null!, SecurityAlgorithms.EcdsaSha256));
        Assert.Throws<ArgumentException>(() => new SdIssuer(IssuerSigningKey, " "));
        Assert.Throws<ArgumentException>(() => new SdIssuer(IssuerSigningKey, SecurityAlgorithms.EcdsaSha256, " "));
    }

    [Fact]
    public void Issue_WithNulls_ThrowsArgumentNullException()
    {
        var issuer = new SdIssuer(IssuerSigningKey, SecurityAlgorithms.EcdsaSha256);
        Assert.Throws<ArgumentNullException>(() => issuer.Issue(null!, new SdIssuanceOptions()));
        Assert.Throws<ArgumentNullException>(() => issuer.Issue([], null!));
    }

    [Fact]
    public void Issue_WithFlatClaims_CreatesValidIssuance()
    {
        // Arrange
        var issuer = new SdIssuer(IssuerSigningKey, SecurityAlgorithms.EcdsaSha256);
        var claims = new JwtPayload { { "sub", "user123" }, { "email", "test@example.com" } };
        var options = new SdIssuanceOptions { DisclosureStructure = new { email = true } };

        // Act
        var output = issuer.Issue(claims, options);

        // Assert
        Assert.Single(output.Disclosures);
        Assert.Equal("email", output.Disclosures[0].ClaimName);

        var jwt = new JwtSecurityToken(output.SdJwt);
        Assert.True(jwt.Payload.ContainsKey("_sd"));
        Assert.False(jwt.Payload.ContainsKey("email"));
    }

    [Fact]
    public void Issue_WithNestedClaims_CreatesRecursiveDisclosures()
    {
        // Arrange
        var issuer = new SdIssuer(IssuerSigningKey, SecurityAlgorithms.EcdsaSha256);
        var claims = new JwtPayload
        {
            { "address", new { street = "123 Main St", city = "Anytown" } }
        };
        var options = new SdIssuanceOptions { DisclosureStructure = new { address = new { city = true } } };

        // Act
        var output = issuer.Issue(claims, options);

        // Assert
        Assert.Single(output.Disclosures);
        Assert.Equal("city", output.Disclosures[0].ClaimName);

        var jwt = new JwtSecurityToken(output.SdJwt);

        Assert.True(jwt.Payload.TryGetValue("address", out var addressValue));
        Assert.NotNull(addressValue);

        // The value from JwtPayload is an object. We need to serialize and re-parse it as a JsonNode
        // to perform the detailed assertions.
        var addressJson = System.Text.Json.JsonSerializer.Serialize(addressValue);
        var addressClaim = JsonNode.Parse(addressJson)!.AsObject();

        Assert.True(addressClaim.ContainsKey("street"));
        Assert.True(addressClaim.ContainsKey("_sd"));
        Assert.False(addressClaim.ContainsKey("city"));
    }
    private static readonly string[] value = new[] { "admin", "user" };

    [Fact]
    public void Issue_WithArrayClaims_CreatesArrayDisclosures()
    {
        // Arrange
        var issuer = new SdIssuer(IssuerSigningKey, SecurityAlgorithms.EcdsaSha256);
        var claims = new JwtPayload { { "roles", value } };
        var options = new SdIssuanceOptions { DisclosureStructure = new { roles = new[] { false, true } } };

        // Act
        var output = issuer.Issue(claims, options);

        // Assert
        Assert.Single(output.Disclosures);
        var disclosure = output.Disclosures[0];
        Assert.Equal("...", disclosure.ClaimName);
        Assert.Contains("user", disclosure.ClaimValue.ToString()!);

        var jwt = new JwtSecurityToken(output.SdJwt);

        Assert.True(jwt.Payload.TryGetValue("roles", out var rolesValue));
        Assert.NotNull(rolesValue);
        var rolesJson = System.Text.Json.JsonSerializer.Serialize(rolesValue);
        var rolesArray = JsonNode.Parse(rolesJson)!.AsArray();

        Assert.True(rolesArray[1]!.AsObject().ContainsKey("..."));
    }

    [Fact]
    public void Issue_WithMakeAllClaimsDisclosable_DisclosesAll()
    {
        // Arrange
        var issuer = new SdIssuer(IssuerSigningKey, SecurityAlgorithms.EcdsaSha256);
        var claims = new JwtPayload { { "sub", "user123" }, { "name", "test" } };
        var options = new SdIssuanceOptions { MakeAllClaimsDisclosable = true };

        // Act
        var output = issuer.Issue(claims, options);

        // Assert
        Assert.Equal(2, output.Disclosures.Count);
        var jwt = new JwtSecurityToken(output.SdJwt);
        Assert.False(jwt.Payload.ContainsKey("sub"));
        Assert.False(jwt.Payload.ContainsKey("name"));
        Assert.True(jwt.Payload.ContainsKey("_sd"));

        Assert.True(jwt.Payload.TryGetValue("_sd", out var sdValue));
        Assert.NotNull(sdValue);
        var sdJson = System.Text.Json.JsonSerializer.Serialize(sdValue);
        var sdArray = JsonNode.Parse(sdJson)!.AsArray();

        Assert.Equal(2, sdArray.Count);
    }

    [Fact]
    public void Issue_WithWeakAlgorithmAndPolicy_ThrowsException()
    {
        // Arrange
        var issuer = new SdIssuer(IssuerSigningKey, SecurityAlgorithms.EcdsaSha256, "sha-1"); // Weak hash alg
        var claims = new JwtPayload();
        var options = new SdIssuanceOptions { AllowWeakAlgorithms = false }; // Default policy

        // Act & Assert
        var ex = Assert.Throws<NotSupportedException>(() => issuer.Issue(claims, options));
        Assert.Contains("weak and not allowed by default", ex.Message);
    }
}