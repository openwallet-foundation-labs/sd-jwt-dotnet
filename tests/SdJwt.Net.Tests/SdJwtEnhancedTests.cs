using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.Issuer;
using SdJwt.Net.Verifier;
using SdJwt.Net.Utils;
using SdJwt.Net.Models;
using System.IdentityModel.Tokens.Jwt;
using Xunit;

namespace SdJwt.Net.Tests;

public class SdJwtEnhancedTests : TestBase
{
    [Fact]
    public void SdIssuer_IssueWithComplexNestedStructure_ShouldHandleAllLevels()
    {
        // Arrange
        var issuer = new SdIssuer(IssuerSigningKey, IssuerSigningAlgorithm);
        var claims = new JwtPayload
        {
            {
                "user", new
                {
                    profile = new
                    {
                        name = "John Doe",
                        age = 30,
                        address = new
                        {
                            street = "123 Main St",
                            city = "Anytown",
                            country = "USA"
                        }
                    },
                    permissions = new[] { "read", "write", "admin" }
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
                        age = true,
                        address = new
                        {
                            city = true,
                            country = true
                        }
                    },
                    permissions = new[] { false, true, true }
                }
            }
        };

        // Act
        var output = issuer.Issue(claims, options);

        // Assert
        // Should have disclosures: age, city, country, write, admin (5 total)
        Assert.Equal(5, output.Disclosures.Count);

        var ageDisclosure = output.Disclosures.FirstOrDefault(d => d.ClaimName == "age");
        Assert.NotNull(ageDisclosure);
        // Compare the actual values based on how they're stored/parsed
        var ageValue = ageDisclosure.ClaimValue;
        Assert.True((ageValue is int intVal && intVal == 30) || (ageValue.ToString() == "30"));

        var cityDisclosure = output.Disclosures.FirstOrDefault(d => d.ClaimName == "city");
        Assert.NotNull(cityDisclosure);
        // Use ToString() for consistent comparison
        Assert.Equal("Anytown", cityDisclosure.ClaimValue.ToString());
    }

    [Fact]
    public void SdIssuer_IssueWithValidHashAlgorithms_ShouldSucceed()
    {
        // Arrange - Test with supported hash algorithms
        var validHashAlgorithms = new[] { "sha-256", "sha-384", "sha-512" };

        foreach (var hashAlg in validHashAlgorithms)
        {
            var issuer = new SdIssuer(IssuerSigningKey, IssuerSigningAlgorithm, hashAlg);
            var claims = new JwtPayload { { "sub", "user123" }, { "email", "test@example.com" } };
            var options = new SdIssuanceOptions
            {
                DisclosureStructure = new { email = true }
            };

            // Act
            var output = issuer.Issue(claims, options);

            // Assert
            Assert.Single(output.Disclosures);

            var jwt = new JwtSecurityToken(output.SdJwt);
            Assert.Equal(hashAlg, jwt.Payload[SdJwtConstants.SdAlgorithmClaim]);
        }
    }

    [Fact]
    public void SdJwtParser_ParseInvalidSdJwt_ShouldThrow()
    {
        // Act & Assert - The exact exception type depends on what invalid.jwt.token triggers
        var act = () => SdJwtParser.ParseIssuance("invalid.jwt.token~disclosure~");
        Assert.ThrowsAny<Exception>(act);
    }

    [Fact]
    public void SdJwtParser_ParseValidSdJwt_ShouldDecodeCorrectly()
    {
        // Arrange
        var issuer = new SdIssuer(IssuerSigningKey, IssuerSigningAlgorithm);
        var claims = new JwtPayload { { "sub", "user123" }, { "email", "test@example.com" } };
        var options = new SdIssuanceOptions { DisclosureStructure = new { email = true } };
        var output = issuer.Issue(claims, options);

        var fullSdJwt = $"{output.SdJwt}~{string.Join("~", output.Disclosures.Select(d => d.EncodedValue))}~";

        // Act
        var parsed = SdJwtParser.ParseIssuance(fullSdJwt);

        // Assert
        Assert.NotNull(parsed);
        Assert.NotNull(parsed.UnverifiedSdJwt);
        Assert.Single(parsed.Disclosures);
        Assert.Equal("email", parsed.Disclosures[0].ClaimName);
        Assert.Equal("test@example.com", parsed.Disclosures[0].ClaimValue);
    }

    [Fact]
    public void SdIssuer_IssueWithEmptyDisclosureStructure_ShouldCreateNoDisclosures()
    {
        // Arrange
        var issuer = new SdIssuer(IssuerSigningKey, IssuerSigningAlgorithm);
        var claims = new JwtPayload { { "sub", "user123" }, { "email", "test@example.com" } };
        var options = new SdIssuanceOptions { DisclosureStructure = new { } };

        // Act
        var output = issuer.Issue(claims, options);

        // Assert
        Assert.Empty(output.Disclosures);
        var jwt = new JwtSecurityToken(output.SdJwt);
        Assert.False(jwt.Payload.ContainsKey(SdJwtConstants.SdClaim));
        Assert.True(jwt.Payload.ContainsKey("email"));
    }

    [Fact]
    public void SdDisclosure_EncodedProperty_ShouldBeBase64UrlEncoded()
    {
        // Arrange
        var issuer = new SdIssuer(IssuerSigningKey, IssuerSigningAlgorithm);
        var claims = new JwtPayload { { "email", "test@example.com" } };
        var options = new SdIssuanceOptions { DisclosureStructure = new { email = true } };

        // Act
        var output = issuer.Issue(claims, options);
        var disclosure = output.Disclosures[0];

        // Assert
        Assert.NotNull(disclosure.EncodedValue);
        Assert.DoesNotContain('+', disclosure.EncodedValue); // base64url doesn't use +
        Assert.DoesNotContain('/', disclosure.EncodedValue); // base64url doesn't use /
        Assert.DoesNotContain('=', disclosure.EncodedValue); // base64url padding is typically removed
    }

    [Fact]
    public void SdJwtConstants_ShouldContainExpectedValues()
    {
        // Assert
        Assert.Equal("_sd", SdJwtConstants.SdClaim);
        Assert.Equal("_sd_alg", SdJwtConstants.SdAlgorithmClaim);
        Assert.Equal("~", SdJwtConstants.DisclosureSeparator);
        Assert.Equal("sha-256", SdJwtConstants.DefaultHashAlgorithm);
    }

    [Fact]
    public void SdIssuer_IssueWithNullClaims_ShouldThrow()
    {
        // Arrange
        var issuer = new SdIssuer(IssuerSigningKey, IssuerSigningAlgorithm);
        var options = new SdIssuanceOptions();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => issuer.Issue(null!, options));
    }

    [Fact]
    public void SdIssuer_IssueWithNullOptions_ShouldThrow()
    {
        // Arrange
        var issuer = new SdIssuer(IssuerSigningKey, IssuerSigningAlgorithm);
        var claims = new JwtPayload { { "sub", "user123" } };

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => issuer.Issue(claims, null!));
    }

    [Fact]
    public void Disclosure_Parse_WithInvalidJson_ShouldThrow()
    {
        // Arrange
        var invalidBase64 = "invalid-base64-content";

        // Act & Assert
        Assert.Throws<FormatException>(() => Disclosure.Parse(invalidBase64));
    }

    [Fact]
    public void Disclosure_Parse_WithValidDisclosure_ShouldSucceed()
    {
        // Arrange
        var disclosure = new Disclosure("salt123", "name", "value");
        var encoded = disclosure.EncodedValue;

        // Act
        var parsed = Disclosure.Parse(encoded);

        // Assert
        Assert.Equal("salt123", parsed.Salt);
        Assert.Equal("name", parsed.ClaimName);
        Assert.Equal("value", parsed.ClaimValue);
        Assert.Equal(encoded, parsed.EncodedValue);
    }

    [Fact]
    public void SdJwtParser_ParsePresentation_WithValidData_ShouldSucceed()
    {
        // Arrange
        var issuer = new SdIssuer(IssuerSigningKey, IssuerSigningAlgorithm);
        var claims = new JwtPayload { { "sub", "user123" }, { "email", "test@example.com" } };
        var options = new SdIssuanceOptions { DisclosureStructure = new { email = true } };
        var output = issuer.Issue(claims, options);

        var presentation = $"{output.SdJwt}~{output.Disclosures[0].EncodedValue}~";

        // Act
        var parsed = SdJwtParser.ParsePresentation(presentation);

        // Assert
        Assert.NotNull(parsed);
        Assert.Equal(output.SdJwt, parsed.RawSdJwt);
        Assert.Single(parsed.Disclosures);
        Assert.Null(parsed.RawKeyBindingJwt);
    }

    [Fact]
    public void SdJwtParser_ParsePresentation_WithEmptyString_ShouldThrow()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => SdJwtParser.ParsePresentation(""));
    }

    [Fact]
    public void SdJwtParser_ParseJson_WithValidJson_ShouldSucceed()
    {
        // Arrange
        var json = """{"test": "value"}""";

        // Act
        var result = SdJwtParser.ParseJson<Dictionary<string, string>>(json);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("value", result["test"]);
    }

    [Fact]
    public void SdIssuanceOptions_WithDecoyDigests_ShouldGenerateDecoys()
    {
        // Arrange
        var issuer = new SdIssuer(IssuerSigningKey, IssuerSigningAlgorithm);
        var claims = new JwtPayload { { "sub", "user123" }, { "email", "test@example.com" } };
        var options = new SdIssuanceOptions
        {
            DisclosureStructure = new { email = true },
            DecoyDigests = 3
        };

        // Act
        var output = issuer.Issue(claims, options);

        // Assert
        var jwt = new JwtSecurityToken(output.SdJwt);
        Assert.True(jwt.Payload.ContainsKey(SdJwtConstants.SdClaim));

        // The _sd array should contain both real digests and decoys
        var sdClaim = jwt.Payload[SdJwtConstants.SdClaim];
        Assert.NotNull(sdClaim);

        // Convert to string array to check count
        var sdArray = System.Text.Json.JsonSerializer.Deserialize<string[]>(sdClaim.ToString()!);
        Assert.NotNull(sdArray);
        Assert.True(sdArray.Length > 1); // Should have real digest plus decoys
    }

    [Fact]
    public void SdIssuer_IssueWithMakeAllClaimsDisclosable_ShouldDisclosureAllClaims()
    {
        // Arrange
        var issuer = new SdIssuer(IssuerSigningKey, IssuerSigningAlgorithm);
        var claims = new JwtPayload
        {
            { "sub", "user123" },
            { "email", "test@example.com" },
            { "name", "John Doe" }
        };
        var options = new SdIssuanceOptions { MakeAllClaimsDisclosable = true };

        // Act
        var output = issuer.Issue(claims, options);

        // Assert
        Assert.Equal(3, output.Disclosures.Count);

        var jwt = new JwtSecurityToken(output.SdJwt);
        Assert.True(jwt.Payload.ContainsKey(SdJwtConstants.SdClaim));

        // None of the original claims should be in the JWT payload anymore
        Assert.False(jwt.Payload.ContainsKey("sub"));
        Assert.False(jwt.Payload.ContainsKey("email"));
        Assert.False(jwt.Payload.ContainsKey("name"));
    }

    [Fact]
    public void SdIssuer_ConstructorWithInvalidParameters_ShouldThrow()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new SdIssuer(IssuerSigningKey, ""));
        Assert.Throws<ArgumentException>(() => new SdIssuer(IssuerSigningKey, IssuerSigningAlgorithm, ""));
        Assert.Throws<ArgumentNullException>(() => new SdIssuer(null!, IssuerSigningAlgorithm));
    }
}
