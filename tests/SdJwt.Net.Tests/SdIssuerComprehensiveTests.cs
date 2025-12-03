using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.Issuer;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json.Nodes;
using Xunit;

namespace SdJwt.Net.Tests;

/// <summary>
/// Comprehensive tests for SdIssuer to reach 95% coverage - Part 2
/// </summary>
public class SdIssuerComprehensiveTests : TestBase
{
    [Fact]
    public void Issue_WithDeeplyNestedDisclosures_CreatesCorrectStructure()
    {
        // Arrange
        var issuer = new SdIssuer(IssuerSigningKey, SecurityAlgorithms.EcdsaSha256);
        var claims = new JwtPayload
        {
            {
                "user", new
                {
                    profile = new
                    {
                        address = new
                        {
                            street = "123 Main St",
                            city = "Anytown",
                            country = "USA"
                        }
                    }
                }
            }
        };
        var options = new SdIssuanceOptions
        {
            DisclosureStructure = new
            {
                user = new
                {
                    profile = new
                    {
                        address = new
                        {
                            city = true,
                            country = true
                        }
                    }
                }
            }
        };

        // Act
        var result = issuer.Issue(claims, options);

        // Assert
        Assert.Equal(2, result.Disclosures.Count);
        Assert.Contains(result.Disclosures, d => d.ClaimName == "city");
        Assert.Contains(result.Disclosures, d => d.ClaimName == "country");
    }

    [Fact]
    public void Issue_WithMultipleArrayElements_CreatesCorrectDisclosures()
    {
        // Arrange
        var issuer = new SdIssuer(IssuerSigningKey, SecurityAlgorithms.EcdsaSha256);
        var claims = new JwtPayload
        {
            { "roles", new[] { "admin", "user", "viewer", "editor" } }
        };
        var options = new SdIssuanceOptions
        {
            DisclosureStructure = new
            {
                roles = new[] { true, false, true, true }
            }
        };

        // Act
        var result = issuer.Issue(claims, options);

        // Assert
        Assert.Equal(3, result.Disclosures.Count);
        Assert.All(result.Disclosures, d => Assert.Equal("...", d.ClaimName));
    }

    [Fact]
    public void Issue_WithMixedNestedAndArrayDisclosures_HandlesCorrectly()
    {
        // Arrange
        var issuer = new SdIssuer(IssuerSigningKey, SecurityAlgorithms.EcdsaSha256);
        var claims = new JwtPayload
        {
            {
                "data", new
                {
                    items = new[] { "item1", "item2", "item3" },
                    metadata = new { version = "1.0", author = "test" }
                }
            }
        };
        var options = new SdIssuanceOptions
        {
            DisclosureStructure = new
            {
                data = new
                {
                    items = new[] { true, false, true },
                    metadata = new { author = true }
                }
            }
        };

        // Act
        var result = issuer.Issue(claims, options);

        // Assert
        Assert.Equal(3, result.Disclosures.Count);
    }

    [Fact]
    public void Issue_WithComplexObjectInArray_ProcessesCorrectly()
    {
        // Arrange
        var issuer = new SdIssuer(IssuerSigningKey, SecurityAlgorithms.EcdsaSha256);
        var claims = new JwtPayload
        {
            {
                "addresses", new[]
                {
                    new { street = "123 Main", city = "NYC" },
                    new { street = "456 Oak", city = "LA" }
                }
            }
        };
        var options = new SdIssuanceOptions
        {
            DisclosureStructure = new
            {
                addresses = new[] { true, false }
            }
        };

        // Act
        var result = issuer.Issue(claims, options);

        // Assert
        Assert.Single(result.Disclosures);
    }

    [Fact]
    public void Issue_WithLargeDecoyCount_AddsAllDecoys()
    {
        // Arrange
        var issuer = new SdIssuer(IssuerSigningKey, SecurityAlgorithms.EcdsaSha256);
        var claims = new JwtPayload { { "sub", "user123" }, { "email", "test@example.com" } };
        var options = new SdIssuanceOptions
        {
            DisclosureStructure = new { email = true },
            DecoyDigests = 10
        };

        // Act
        var result = issuer.Issue(claims, options);

        // Assert
        var jwt = new JwtSecurityToken(result.SdJwt);
        Assert.True(jwt.Payload.TryGetValue("_sd", out var sdValue));
        var sdJson = System.Text.Json.JsonSerializer.Serialize(sdValue);
        var sdArray = JsonNode.Parse(sdJson)!.AsArray();
        Assert.Equal(11, sdArray.Count); // 1 real + 10 decoys
    }

    [Fact]
    public void Issue_WithNestedObjectsAndDecoys_CombinesCorrectly()
    {
        // Arrange
        var issuer = new SdIssuer(IssuerSigningKey, SecurityAlgorithms.EcdsaSha256);
        var claims = new JwtPayload
        {
            { "sub", "user123" },
            {
                "address", new
                {
                    street = "123 Main",
                    city = "NYC"
                }
            }
        };
        var options = new SdIssuanceOptions
        {
            DisclosureStructure = new
            {
                address = new { city = true }
            },
            DecoyDigests = 2
        };

        // Act
        var result = issuer.Issue(claims, options);

        // Assert
        Assert.Single(result.Disclosures);
        var jwt = new JwtSecurityToken(result.SdJwt);
        Assert.True(jwt.Payload.TryGetValue("_sd", out var sdValue));
        var sdJson = System.Text.Json.JsonSerializer.Serialize(sdValue);
        var sdArray = JsonNode.Parse(sdJson)!.AsArray();
        Assert.Equal(2, sdArray.Count); // Only top-level decoys
    }

    [Fact]
    public void Issue_WithSpecialCharactersInClaimNames_HandlesCorrectly()
    {
        // Arrange
        var issuer = new SdIssuer(IssuerSigningKey, SecurityAlgorithms.EcdsaSha256);
        var claims = new JwtPayload
        {
            { "user-name", "test" },
            { "email_address", "test@example.com" },
            { "phone.number", "123-456-7890" }
        };
        var options = new SdIssuanceOptions
        {
            MakeAllClaimsDisclosable = true
        };

        // Act
        var result = issuer.Issue(claims, options);

        // Assert
        Assert.Equal(3, result.Disclosures.Count);
    }

    [Fact]
    public void Issue_WithNumericValues_HandlesCorrectly()
    {
        // Arrange
        var issuer = new SdIssuer(IssuerSigningKey, SecurityAlgorithms.EcdsaSha256);
        var claims = new JwtPayload
        {
            { "age", 30 },
            { "score", 95.5 },
            { "count", 1000L }
        };
        var options = new SdIssuanceOptions
        {
            DisclosureStructure = new { age = true, score = true }
        };

        // Act
        var result = issuer.Issue(claims, options);

        // Assert
        Assert.Equal(2, result.Disclosures.Count);
    }

    [Fact]
    public void Issue_WithBooleanValues_HandlesCorrectly()
    {
        // Arrange
        var issuer = new SdIssuer(IssuerSigningKey, SecurityAlgorithms.EcdsaSha256);
        var claims = new JwtPayload
        {
            { "is_active", true },
            { "is_verified", false }
        };
        var options = new SdIssuanceOptions
        {
            DisclosureStructure = new { is_active = true }
        };

        // Act
        var result = issuer.Issue(claims, options);

        // Assert
        Assert.Single(result.Disclosures);
        Assert.Equal("is_active", result.Disclosures[0].ClaimName);
    }

    [Fact]
    public void Issue_WithDateTimeValues_HandlesCorrectly()
    {
        // Arrange
        var issuer = new SdIssuer(IssuerSigningKey, SecurityAlgorithms.EcdsaSha256);
        var now = DateTime.UtcNow;
        var claims = new JwtPayload
        {
            { "created_at", now },
            { "expires_at", now.AddDays(30) }
        };
        var options = new SdIssuanceOptions
        {
            DisclosureStructure = new { created_at = true }
        };

        // Act
        var result = issuer.Issue(claims, options);

        // Assert
        Assert.Single(result.Disclosures);
    }
}
