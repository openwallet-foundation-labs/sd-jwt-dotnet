using SdJwt.Net.Models;
using SdJwt.Net.Issuer;
using SdJwt.Net.Internal;
using Xunit;
using System.Text.Json;
using System.IdentityModel.Tokens.Jwt;

namespace SdJwt.Net.Tests.Models;

public class SdJwtConstantsTests
{
    [Fact]
    public void SdJwtConstants_ShouldHaveExpectedClaimValues()
    {
        // Assert
        Assert.Equal("_sd", SdJwtConstants.SdClaim);
        Assert.Equal("_sd_alg", SdJwtConstants.SdAlgorithmClaim);
        Assert.Equal("~", SdJwtConstants.DisclosureSeparator);
        Assert.Equal("sha-256", SdJwtConstants.DefaultHashAlgorithm);
        Assert.Equal("kb+jwt", SdJwtConstants.KbJwtHeaderType);
        Assert.Equal("sd_hash", SdJwtConstants.SdHashClaim);
    }

    [Fact]
    public void SdJwtConstants_ShouldHaveExpectedTypeValues()
    {
        // Assert
        Assert.Equal("sd+jwt", SdJwtConstants.SdJwtTypeName);
        Assert.Equal("dc+sd-jwt", SdJwtConstants.SdJwtVcTypeName);
        Assert.Equal("vc+sd-jwt", SdJwtConstants.SdJwtVcLegacyTypeName);
        Assert.Equal("statuslist+jwt", SdJwtConstants.StatusListJwtTypeName);
    }

    [Fact]
    public void SdJwtConstants_ShouldHaveExpectedMediaTypes()
    {
        // Assert
        Assert.Equal("application/sd-jwt", SdJwtConstants.SdJwtMediaType);
        Assert.Equal("application/sd-jwt+json", SdJwtConstants.SdJwtJsonMediaType);
        Assert.Equal("application/kb+jwt", SdJwtConstants.KeyBindingJwtMediaType);
        Assert.Equal("application/dc+sd-jwt", SdJwtConstants.SdJwtVcMediaType);
        Assert.Equal("application/statuslist+jwt", SdJwtConstants.StatusListJwtMediaType);
        Assert.Equal("application/statuslist+cwt", SdJwtConstants.StatusListCwtMediaType);
    }

    [Fact]
    public void SdJwtConstants_ShouldHaveExpectedVcClaims()
    {
        // Assert
        Assert.Equal("vct", SdJwtConstants.VctClaim);
        Assert.Equal("vct#integrity", SdJwtConstants.VctIntegrityClaim);
        Assert.Equal("cnf", SdJwtConstants.CnfClaim);
        Assert.Equal("jwk", SdJwtConstants.JwkClaim);
    }

    [Fact]
    public void SdJwtConstants_ShouldHaveExpectedStatusClaims()
    {
        // Assert
        Assert.Equal("status", SdJwtConstants.StatusClaim);
        Assert.Equal("status_list", SdJwtConstants.StatusListClaim);
        Assert.Equal("ttl", SdJwtConstants.TtlClaim);
    }

    [Fact]
    public void DefaultJsonSerializerOptions_ShouldBeConfiguredCorrectly()
    {
        // Assert
        Assert.NotNull(SdJwtConstants.DefaultJsonSerializerOptions);
        Assert.Equal(JsonNamingPolicy.CamelCase, SdJwtConstants.DefaultJsonSerializerOptions.PropertyNamingPolicy);
    }
}

public class DisclosureTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateDisclosure()
    {
        // Arrange
        var salt = "test-salt";
        var claimName = "email";
        var claimValue = "test@example.com";

        // Act
        var disclosure = new Disclosure(salt, claimName, claimValue);

        // Assert
        Assert.NotNull(disclosure);
        Assert.Equal(salt, disclosure.Salt);
        Assert.Equal(claimName, disclosure.ClaimName);
        Assert.Equal(claimValue, disclosure.ClaimValue);
        Assert.NotNull(disclosure.EncodedValue);
        Assert.NotEmpty(disclosure.EncodedValue);
    }

    [Fact]
    public void EncodedValue_ShouldBeBase64UrlEncoded()
    {
        // Arrange
        var disclosure = new Disclosure("salt", "name", "value");

        // Act
        var encoded = disclosure.EncodedValue;

        // Assert
        Assert.DoesNotContain('+', encoded); // Base64Url doesn't use +
        Assert.DoesNotContain('/', encoded); // Base64Url doesn't use /
        Assert.DoesNotContain('=', encoded); // Base64Url padding is typically removed
    }

    [Fact]
    public void Parse_WithValidEncodedDisclosure_ShouldDecodeCorrectly()
    {
        // Arrange
        var originalDisclosure = new Disclosure("test-salt", "email", "test@example.com");
        var encoded = originalDisclosure.EncodedValue;

        // Act
        var parsedDisclosure = Disclosure.Parse(encoded);

        // Assert
        Assert.NotNull(parsedDisclosure);
        Assert.Equal(originalDisclosure.Salt, parsedDisclosure.Salt);
        Assert.Equal(originalDisclosure.ClaimName, parsedDisclosure.ClaimName);
        Assert.Equal(originalDisclosure.ClaimValue, parsedDisclosure.ClaimValue);
        Assert.Equal(originalDisclosure.EncodedValue, parsedDisclosure.EncodedValue);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Parse_WithInvalidInput_ShouldThrow(string? invalidInput)
    {
        // Act & Assert
        Assert.ThrowsAny<Exception>(() => Disclosure.Parse(invalidInput!));
    }

    [Fact]
    public void Parse_WithMalformedBase64_ShouldThrow()
    {
        // Arrange
        var malformedBase64 = "not-valid-base64!";

        // Act & Assert
        Assert.ThrowsAny<Exception>(() => Disclosure.Parse(malformedBase64));
    }

    [Fact]
    public void Constructor_WithComplexClaimValue_ShouldHandleCorrectly()
    {
        // Arrange
        var complexValue = new
        {
            name = "John Doe",
            age = 30,
            address = new
            {
                street = "123 Main St",
                city = "Anytown"
            }
        };

        // Act
        var disclosure = new Disclosure("salt", "user_info", complexValue);

        // Assert
        Assert.NotNull(disclosure);
        Assert.Equal("user_info", disclosure.ClaimName);
        Assert.Equal(complexValue, disclosure.ClaimValue);
    }

    [Fact]
    public void Parse_WithComplexClaimValue_ShouldPreserveStructure()
    {
        // Arrange
        var complexValue = new
        {
            name = "John Doe",
            age = 30
        };
        var originalDisclosure = new Disclosure("salt", "user_info", complexValue);
        var encoded = originalDisclosure.EncodedValue;

        // Act
        var parsedDisclosure = Disclosure.Parse(encoded);

        // Assert
        Assert.NotNull(parsedDisclosure);
        Assert.Equal("user_info", parsedDisclosure.ClaimName);

        // For complex objects, we should verify the JSON structure is preserved
        var originalJson = JsonSerializer.Serialize(complexValue);
        var parsedJson = JsonSerializer.Serialize(parsedDisclosure.ClaimValue);
        Assert.Equal(originalJson, parsedJson);
    }

    [Fact]
    public void ToString_ShouldReturnMeaningfulRepresentation()
    {
        // Arrange
        var disclosure = new Disclosure("test-salt", "email", "test@example.com");

        // Act
        var stringRepresentation = disclosure.ToString();

        // Assert
        Assert.Contains("email", stringRepresentation);
        Assert.Contains("test@example.com", stringRepresentation);
    }

    [Fact]
    public void Equals_WithSameValues_ShouldReturnTrue()
    {
        // Arrange
        var disclosure1 = new Disclosure("salt", "name", "value");
        var disclosure2 = new Disclosure("salt", "name", "value");

        // Act & Assert
        Assert.True(disclosure1.Equals(disclosure2));
        Assert.True(disclosure1 == disclosure2);
        Assert.Equal(disclosure1.GetHashCode(), disclosure2.GetHashCode());
    }

    [Fact]
    public void Equals_WithDifferentValues_ShouldReturnFalse()
    {
        // Arrange
        var disclosure1 = new Disclosure("salt1", "name", "value");
        var disclosure2 = new Disclosure("salt2", "name", "value");

        // Act & Assert
        Assert.False(disclosure1.Equals(disclosure2));
        Assert.True(disclosure1 != disclosure2);
    }

    [Fact]
    public void Equals_WithNull_ShouldReturnFalse()
    {
        // Arrange
        var disclosure = new Disclosure("salt", "name", "value");

        // Act & Assert
        Assert.False(disclosure.Equals(null));
        Assert.False(disclosure == null);
    }
}

public class SdIssuanceOptionsTests
{
    [Fact]
    public void Constructor_WithDefaults_ShouldCreateInstance()
    {
        // Act
        var options = new SdIssuanceOptions();

        // Assert
        Assert.NotNull(options);
        Assert.Null(options.DisclosureStructure);
        Assert.Equal(0, options.DecoyDigests);
        Assert.False(options.MakeAllClaimsDisclosable);
        Assert.False(options.AllowWeakAlgorithms);
    }

    [Fact]
    public void DisclosureStructure_WithValidObject_ShouldSetCorrectly()
    {
        // Arrange
        var disclosureStructure = new { email = true, name = false };

        // Act
        var options = new SdIssuanceOptions { DisclosureStructure = disclosureStructure };

        // Assert
        Assert.Equal(disclosureStructure, options.DisclosureStructure);
    }

    [Fact]
    public void DecoyDigests_WithValidValue_ShouldSetCorrectly()
    {
        // Arrange
        var decoyCount = 5;

        // Act
        var options = new SdIssuanceOptions { DecoyDigests = decoyCount };

        // Assert
        Assert.Equal(decoyCount, options.DecoyDigests);
    }

    [Fact]
    public void MakeAllClaimsDisclosable_WithTrue_ShouldSetCorrectly()
    {
        // Act
        var options = new SdIssuanceOptions { MakeAllClaimsDisclosable = true };

        // Assert
        Assert.True(options.MakeAllClaimsDisclosable);
    }

    [Fact]
    public void AllowWeakAlgorithms_WithTrue_ShouldSetCorrectly()
    {
        // Act
        var options = new SdIssuanceOptions { AllowWeakAlgorithms = true };

        // Assert
        Assert.True(options.AllowWeakAlgorithms);
    }

    [Fact]
    public void Constructor_WithComplexDisclosureStructure_ShouldHandle()
    {
        // Arrange
        var complexStructure = new
        {
            user = new
            {
                profile = new
                {
                    name = true,
                    age = false,
                    address = new
                    {
                        street = true,
                        city = false
                    }
                },
                permissions = new[] { true, false, true }
            },
            metadata = new
            {
                created = true,
                modified = false
            }
        };

        // Act
        var options = new SdIssuanceOptions { DisclosureStructure = complexStructure };

        // Assert
        Assert.Equal(complexStructure, options.DisclosureStructure);
    }
}

public class ParsedSdJwtTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Arrange
        var rawSdJwt = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c";
        var unverifiedToken = new JwtSecurityToken(rawSdJwt);
        var disclosures = new List<Disclosure>
        {
            new("salt1", "email", "test@example.com")
        }.AsReadOnly();

        // Act
        var parsed = new ParsedSdJwt(rawSdJwt, unverifiedToken, disclosures);

        // Assert
        Assert.NotNull(parsed);
        Assert.Equal(rawSdJwt, parsed.RawSdJwt);
        Assert.Equal(unverifiedToken, parsed.UnverifiedSdJwt);
        Assert.Equal(disclosures, parsed.Disclosures);
    }

    [Fact]
    public void Deconstruct_ShouldProvideComponents()
    {
        // Arrange
        var rawSdJwt = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c";
        var unverifiedToken = new JwtSecurityToken(rawSdJwt);
        var disclosures = new List<Disclosure>
        {
            new("salt1", "email", "test@example.com")
        }.AsReadOnly();
        var parsed = new ParsedSdJwt(rawSdJwt, unverifiedToken, disclosures);

        // Act
        var (deconstructedRawSdJwt, deconstructedDisclosures) = parsed;

        // Assert
        Assert.Equal(rawSdJwt, deconstructedRawSdJwt);
        Assert.Equal(disclosures, deconstructedDisclosures);
    }
}

public class ParsedPresentationTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateInstance()
    {
        // Arrange
        var rawSdJwt = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c";
        var unverifiedToken = new JwtSecurityToken(rawSdJwt);
        var disclosures = new List<Disclosure>
        {
            new("salt1", "email", "test@example.com")
        }.AsReadOnly();
        var rawKeyBindingJwt = "eyJhbGciOiJIUzI1NiIsInR5cCI6ImtiK2p3dCJ9.eyJhdWQiOiJ0ZXN0IiwiaWF0IjoxNjc1NzY0MjAwLCJub25jZSI6InRlc3QifQ.test";
        var unverifiedKeyBindingToken = new JwtSecurityToken(rawKeyBindingJwt);

        // Act
        var parsed = new ParsedPresentation(rawSdJwt, unverifiedToken, disclosures, rawKeyBindingJwt, unverifiedKeyBindingToken);

        // Assert
        Assert.NotNull(parsed);
        Assert.Equal(rawSdJwt, parsed.RawSdJwt);
        Assert.Equal(unverifiedToken, parsed.UnverifiedSdJwt);
        Assert.Equal(disclosures, parsed.Disclosures);
        Assert.Equal(rawKeyBindingJwt, parsed.RawKeyBindingJwt);
        Assert.Equal(unverifiedKeyBindingToken, parsed.UnverifiedKeyBindingJwt);
    }

    [Fact]
    public void Constructor_WithoutKeyBinding_ShouldCreateInstance()
    {
        // Arrange
        var rawSdJwt = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c";
        var unverifiedToken = new JwtSecurityToken(rawSdJwt);
        var disclosures = new List<Disclosure>
        {
            new("salt1", "email", "test@example.com")
        }.AsReadOnly();

        // Act
        var parsed = new ParsedPresentation(rawSdJwt, unverifiedToken, disclosures, null, null);

        // Assert
        Assert.NotNull(parsed);
        Assert.Equal(rawSdJwt, parsed.RawSdJwt);
        Assert.Equal(unverifiedToken, parsed.UnverifiedSdJwt);
        Assert.Equal(disclosures, parsed.Disclosures);
        Assert.Null(parsed.RawKeyBindingJwt);
        Assert.Null(parsed.UnverifiedKeyBindingJwt);
    }

    [Fact]
    public void Deconstruct_ShouldProvideAllComponents()
    {
        // Arrange
        var rawSdJwt = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c";
        var unverifiedToken = new JwtSecurityToken(rawSdJwt);
        var disclosures = new List<Disclosure>
        {
            new("salt1", "email", "test@example.com")
        }.AsReadOnly();
        var rawKeyBindingJwt = "eyJhbGciOiJSUzI1NiIsInR5cCI6ImtiK2p3dCJ9.eyJhdWQiOiJ0ZXN0IiwiaWF0IjoxNjc1NzY0MjAwLCJub25jZSI6InRlc3QifQ.test";
        var unverifiedKeyBindingToken = new JwtSecurityToken(rawKeyBindingJwt);
        var parsed = new ParsedPresentation(rawSdJwt, unverifiedToken, disclosures, rawKeyBindingJwt, unverifiedKeyBindingToken);

        // Act
        var (deconstructedRawSdJwt, deconstructedDisclosures, deconstructedRawKeyBindingJwt) = parsed;

        // Assert
        Assert.Equal(rawSdJwt, deconstructedRawSdJwt);
        Assert.Equal(disclosures, deconstructedDisclosures);
        Assert.Equal(rawKeyBindingJwt, deconstructedRawKeyBindingJwt);
    }
}

public class SdJwtUtilsTests
{
    [Fact]
    public void GenerateSalt_ShouldReturnValidBase64UrlString()
    {
        // Act
        var salt = SdJwtUtils.GenerateSalt();

        // Assert
        Assert.NotNull(salt);
        Assert.NotEmpty(salt);
        Assert.DoesNotContain('+', salt); // Base64Url doesn't use +
        Assert.DoesNotContain('/', salt); // Base64Url doesn't use /
        Assert.DoesNotContain('=', salt); // Base64Url padding is typically removed
    }

    [Fact]
    public void GenerateSalt_ShouldReturnUniqueValues()
    {
        // Act
        var salt1 = SdJwtUtils.GenerateSalt();
        var salt2 = SdJwtUtils.GenerateSalt();

        // Assert
        Assert.NotEqual(salt1, salt2);
    }

    [Fact]
    public void CreateDigest_WithSha256_ShouldReturnValidHash()
    {
        // Arrange
        var encodedDisclosure = "test-disclosure";

        // Act
        var digest = SdJwtUtils.CreateDigest("sha-256", encodedDisclosure);

        // Assert
        Assert.NotNull(digest);
        Assert.NotEmpty(digest);
    }

    [Fact]
    public void CreateDigest_SameInput_ShouldReturnSameHash()
    {
        // Arrange
        var encodedDisclosure = "test-disclosure";

        // Act
        var digest1 = SdJwtUtils.CreateDigest("sha-256", encodedDisclosure);
        var digest2 = SdJwtUtils.CreateDigest("sha-256", encodedDisclosure);

        // Assert
        Assert.Equal(digest1, digest2);
    }

    [Fact]
    public void CreateDigest_DifferentInput_ShouldReturnDifferentHash()
    {
        // Act
        var digest1 = SdJwtUtils.CreateDigest("sha-256", "disclosure1");
        var digest2 = SdJwtUtils.CreateDigest("sha-256", "disclosure2");

        // Assert
        Assert.NotEqual(digest1, digest2);
    }

    [Fact]
    public void CreateDigest_WithUnsupportedAlgorithm_ShouldThrow()
    {
        // Act & Assert
        var ex = Assert.Throws<NotSupportedException>(() => SdJwtUtils.CreateDigest("md5", "test"));
        Assert.Contains("not supported", ex.Message);
    }

    [Fact]
    public void IsApprovedHashAlgorithm_WithValidAlgorithms_ShouldReturnTrue()
    {
        // Assert
        Assert.True(SdJwtUtils.IsApprovedHashAlgorithm("SHA-256"));
        Assert.True(SdJwtUtils.IsApprovedHashAlgorithm("SHA-384"));
        Assert.True(SdJwtUtils.IsApprovedHashAlgorithm("SHA-512"));
        Assert.True(SdJwtUtils.IsApprovedHashAlgorithm("sha-256")); // Case insensitive
    }

    [Fact]
    public void IsApprovedHashAlgorithm_WithInvalidAlgorithms_ShouldReturnFalse()
    {
        // Assert
        Assert.False(SdJwtUtils.IsApprovedHashAlgorithm("MD5"));
        Assert.False(SdJwtUtils.IsApprovedHashAlgorithm("SHA-1"));
        Assert.False(SdJwtUtils.IsApprovedHashAlgorithm(""));
        Assert.False(SdJwtUtils.IsApprovedHashAlgorithm(null));
        Assert.False(SdJwtUtils.IsApprovedHashAlgorithm("unknown"));
    }

    [Fact]
    public void ConvertJsonElement_WithString_ShouldReturnString()
    {
        // Arrange
        var json = "\"test string\"";
        var element = JsonSerializer.Deserialize<JsonElement>(json);

        // Act
        var result = SdJwtUtils.ConvertJsonElement(element);

        // Assert
        Assert.IsType<string>(result);
        Assert.Equal("test string", result);
    }

    [Fact]
    public void ConvertJsonElement_WithNumber_ShouldReturnDecimal()
    {
        // Arrange
        var json = "42.5";
        var element = JsonSerializer.Deserialize<JsonElement>(json);

        // Act
        var result = SdJwtUtils.ConvertJsonElement(element);

        // Assert
        Assert.IsType<decimal>(result);
        Assert.Equal(42.5m, result);
    }

    [Fact]
    public void ConvertJsonElement_WithBoolean_ShouldReturnBoolean()
    {
        // Arrange
        var json = "true";
        var element = JsonSerializer.Deserialize<JsonElement>(json);

        // Act
        var result = SdJwtUtils.ConvertJsonElement(element);

        // Assert
        Assert.IsType<bool>(result);
        Assert.True((bool)result);
    }

    [Fact]
    public void ConvertJsonElement_WithObject_ShouldReturnDictionary()
    {
        // Arrange
        var json = """{"name": "John", "age": 30}""";
        var element = JsonSerializer.Deserialize<JsonElement>(json);

        // Act
        var result = SdJwtUtils.ConvertJsonElement(element);

        // Assert
        Assert.IsType<Dictionary<string, object>>(result);
        var dict = (Dictionary<string, object>)result;
        Assert.True(dict.ContainsKey("name"));
        Assert.True(dict.ContainsKey("age"));
        // Compare string values directly since the exact type might vary
        Assert.Equal("John", dict["name"].ToString());
        Assert.Equal("30", dict["age"].ToString());
    }

    [Fact]
    public void ConvertJsonElement_WithArray_ShouldReturnList()
    {
        // Arrange
        var json = """["item1", "item2", 42]""";
        var element = JsonSerializer.Deserialize<JsonElement>(json);

        // Act
        var result = SdJwtUtils.ConvertJsonElement(element);

        // Assert
        Assert.IsType<List<object>>(result);
        var list = (List<object>)result;
        Assert.Equal(3, list.Count);
        Assert.Equal("item1", list[0].ToString());
        Assert.Equal("item2", list[1].ToString());
        Assert.Equal("42", list[2].ToString());
    }
}
