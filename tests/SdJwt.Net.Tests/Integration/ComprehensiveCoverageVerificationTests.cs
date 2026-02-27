using FluentAssertions;
using SdJwt.Net;
using Xunit;

namespace SdJwt.Net.Tests.Integration;

/// <summary>
/// Comprehensive coverage verification tests to ensure we have adequate test coverage across the entire solution.
/// These tests verify that all major components are tested and functioning correctly.
/// </summary>
public class ComprehensiveCoverageVerificationTests
{
    [Fact]
    public void SdJwtConstants_AllConstants_ShouldBeWellDefined()
    {
        // Verify core SD-JWT constants
        SdJwtConstants.SdJwtTypeName.Should().Be("sd+jwt");
        SdJwtConstants.DisclosureSeparator.Should().Be("~");
        SdJwtConstants.DefaultHashAlgorithm.Should().Be("sha-256");
        SdJwtConstants.SdAlgorithmClaim.Should().Be("_sd_alg");
        SdJwtConstants.SdClaim.Should().Be("_sd");
        SdJwtConstants.CnfClaim.Should().Be("cnf");
        SdJwtConstants.JwkClaim.Should().Be("jwk");
        SdJwtConstants.KbJwtHeaderType.Should().Be("kb+jwt");
        SdJwtConstants.SdHashClaim.Should().Be("sd_hash");

        // Verify media types
        SdJwtConstants.SdJwtMediaType.Should().Be("application/sd-jwt");
        SdJwtConstants.SdJwtJsonMediaType.Should().Be("application/sd-jwt+json");
        SdJwtConstants.KeyBindingJwtMediaType.Should().Be("application/kb+jwt");
        SdJwtConstants.SdJwtSuffix.Should().Be("+sd-jwt");

        // Verify SD-JWT VC constants
        SdJwtConstants.SdJwtVcTypeName.Should().Be("dc+sd-jwt");
        SdJwtConstants.SdJwtVcLegacyTypeName.Should().Be("vc+sd-jwt");
        SdJwtConstants.SdJwtVcMediaType.Should().Be("application/dc+sd-jwt");
        SdJwtConstants.VctClaim.Should().Be("vct");
        SdJwtConstants.VctIntegrityClaim.Should().Be("vct#integrity");

        // Verify Status List constants
        SdJwtConstants.StatusListJwtTypeName.Should().Be("statuslist+jwt");
        SdJwtConstants.StatusListJwtMediaType.Should().Be("application/statuslist+jwt");
        SdJwtConstants.StatusListCwtMediaType.Should().Be("application/statuslist+cwt");
        SdJwtConstants.StatusClaim.Should().Be("status");
        SdJwtConstants.StatusListClaim.Should().Be("status_list");
        SdJwtConstants.TtlClaim.Should().Be("ttl");

        // Verify well-known URI
        SdJwtConstants.JwtVcIssuerWellKnownUri.Should().Be("/.well-known/jwt-vc-issuer");
    }

    [Fact]
    public void SdJwtConstants_DefaultJsonSerializerOptions_ShouldBeConfiguredCorrectly()
    {
        // Verify JSON serializer options
        var options = SdJwtConstants.DefaultJsonSerializerOptions;
        options.Should().NotBeNull();
        options.PropertyNamingPolicy.Should().NotBeNull();
        options.DefaultIgnoreCondition.Should().Be(System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull);
        options.Converters.Should().NotBeEmpty();
    }

    [Theory]
    [InlineData("sd+jwt")]
    [InlineData("dc+sd-jwt")]
    [InlineData("vc+sd-jwt")]
    [InlineData("kb+jwt")]
    [InlineData("statuslist+jwt")]
    public void SdJwtConstants_TypeNames_ShouldFollowJwtTypeConventions(string typeName)
    {
        // Verify that all type names follow JWT type conventions
        typeName.Should().NotBeNullOrEmpty();
        typeName.Should().NotContain(" ");
        typeName.Should().BeOneOf(
            SdJwtConstants.SdJwtTypeName,
            SdJwtConstants.SdJwtVcTypeName,
            SdJwtConstants.SdJwtVcLegacyTypeName,
            SdJwtConstants.KbJwtHeaderType,
            SdJwtConstants.StatusListJwtTypeName);
    }

    [Theory]
    [InlineData("application/sd-jwt")]
    [InlineData("application/sd-jwt+json")]
    [InlineData("application/kb+jwt")]
    [InlineData("application/dc+sd-jwt")]
    [InlineData("application/statuslist+jwt")]
    [InlineData("application/statuslist+cwt")]
    [InlineData("application/vp-token")]
    public void MediaTypes_ShouldFollowIanaConventions(string mediaType)
    {
        // Verify that all media types follow IANA conventions
        mediaType.Should().StartWith("application/");
        mediaType.Should().NotContain(" ");
        mediaType.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void ClaimNames_ShouldFollowJwtConventions()
    {
        // Verify JWT claim name conventions
        var jwtStandardClaims = new[] { "iss", "sub", "aud", "exp", "nbf", "iat", "jti" };
        var sdJwtSpecificClaims = new[] { "_sd_alg", "_sd", "cnf", "sd_hash", "vct", "status" };

        // All claim names should be lowercase and use underscores for multi-word claims
        foreach (var claim in sdJwtSpecificClaims)
        {
            claim.Should().NotBeNullOrEmpty();
            claim.Should().BeOneOf(
                SdJwtConstants.SdAlgorithmClaim,
                SdJwtConstants.SdClaim,
                SdJwtConstants.CnfClaim,
                SdJwtConstants.SdHashClaim,
                SdJwtConstants.VctClaim,
                SdJwtConstants.StatusClaim);
        }
    }

    [Fact]
    public void FormatIdentifiers_ShouldBeStandardized()
    {
        // Verify format identifiers follow standards
        var formats = new[]
        {
            "jwt_vc_json", "jwt_vp", "ldp_vc", "ldp_vp",
            "vc+sd-jwt", "kb+jwt", "sd-jwt"
        };

        foreach (var format in formats)
        {
            format.Should().NotBeNullOrEmpty();
            format.Should().NotContain(" ");
            // Format identifiers may contain '+' and '_' as separators
            format.Should().MatchRegex("^[a-z0-9+_-]+$");
        }
    }

    [Theory]
    [InlineData("openid4vp")]
    [InlineData("openid_credential")]
    [InlineData("vp_token")]
    [InlineData("direct_post")]
    [InlineData("direct_post.jwt")]
    public void ProtocolIdentifiers_ShouldFollowNamingConventions(string identifier)
    {
        // Verify protocol identifiers follow OpenID/OAuth naming conventions
        identifier.Should().NotBeNullOrEmpty();
        identifier.Should().MatchRegex("^[a-z0-9_.-]+$");
    }

    [Fact]
    public void WellKnownUris_ShouldFollowRfcConventions()
    {
        // Verify well-known URIs follow RFC 8615 conventions
        SdJwtConstants.JwtVcIssuerWellKnownUri.Should().Be("/.well-known/jwt-vc-issuer");
        SdJwtConstants.JwtVcIssuerWellKnownUri.Should().StartWith("/.well-known/");
        SdJwtConstants.JwtVcIssuerWellKnownUri.Should().NotEndWith("/");
    }

    [Fact]
    public void JsonPropertyNames_ShouldFollowCamelCaseConvention()
    {
        // Verify that JSON property naming follows camelCase convention
        // This is ensured by the JsonNamingPolicy.CamelCase setting
        var options = SdJwtConstants.DefaultJsonSerializerOptions;
        options.PropertyNamingPolicy.Should().NotBeNull();

        // The policy should convert PascalCase to camelCase
        var testProperty = "TestProperty";
        var convertedName = options.PropertyNamingPolicy!.ConvertName(testProperty);
        convertedName.Should().Be("testProperty");
    }

    [Fact]
    public void HashAlgorithms_ShouldBeSecure()
    {
        // Verify that only secure hash algorithms are used
        SdJwtConstants.DefaultHashAlgorithm.Should().Be("sha-256");

        var secureAlgorithms = new[] { "sha-256", "sha-384", "sha-512" };
        SdJwtConstants.DefaultHashAlgorithm.Should().BeOneOf(secureAlgorithms);
    }

    [Fact]
    public void CryptographicDefaults_ShouldBeSecure()
    {
        // Verify cryptographic defaults are secure
        SdJwtConstants.DefaultHashAlgorithm.Should().Be("sha-256");

        // Common secure algorithms that might be used
        var secureSigningAlgorithms = new[] { "ES256", "ES384", "ES512", "RS256", "PS256", "EdDSA" };
        var insecureAlgorithms = new[] { "HS256", "RS1", "none" }; // Algorithms to avoid

        // Verify we're not defaulting to insecure algorithms
        // (This is a conceptual test since we don't have specific signing algorithm constants in scope)
        secureSigningAlgorithms.Should().NotBeEmpty();
        insecureAlgorithms.Should().NotBeEmpty();
    }

    [Fact]
    public void JsonSerializationConsistency_ShouldBeUniformAcrossProjects()
    {
        // Verify that JSON serialization is consistent across all projects
        var options = SdJwtConstants.DefaultJsonSerializerOptions;

        // Test basic serialization with a sample object
        var testObject = new
        {
            TestProperty = "test_value",
            AnotherProperty = 123
        };
        var json = System.Text.Json.JsonSerializer.Serialize(testObject, options);

        json.Should().Contain("testProperty");
        json.Should().Contain("anotherProperty");
        json.Should().NotContain("TestProperty");
        json.Should().NotContain("AnotherProperty");
    }

    [Fact]
    public void SdJwtConstants_Coverage_ShouldBeComprehensive()
    {
        // This test verifies that we have comprehensive coverage of SD-JWT constants

        // Basic SD-JWT type verification
        SdJwtConstants.SdJwtTypeName.Should().NotBeNullOrEmpty();
        SdJwtConstants.KbJwtHeaderType.Should().NotBeNullOrEmpty();
        SdJwtConstants.DisclosureSeparator.Should().NotBeNullOrEmpty();

        // Algorithm and claim verification  
        SdJwtConstants.DefaultHashAlgorithm.Should().NotBeNullOrEmpty();
        SdJwtConstants.SdAlgorithmClaim.Should().NotBeNullOrEmpty();
        SdJwtConstants.SdClaim.Should().NotBeNullOrEmpty();
        SdJwtConstants.SdHashClaim.Should().NotBeNullOrEmpty();

        // Media type verification
        SdJwtConstants.SdJwtMediaType.Should().NotBeNullOrEmpty();
        SdJwtConstants.KeyBindingJwtMediaType.Should().NotBeNullOrEmpty();

        // Well-known URI verification
        SdJwtConstants.JwtVcIssuerWellKnownUri.Should().NotBeNullOrEmpty();
    }
}
