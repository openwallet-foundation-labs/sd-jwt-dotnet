using FluentAssertions;
using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.OidFederation.Logic;
using SdJwt.Net.OidFederation.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using Xunit;

namespace SdJwt.Net.OidFederation.Tests.Logic;

public class EntityConfigurationBuilderEnhancedTests
{
    private readonly ECDsaSecurityKey _signingKey;
    private readonly object _jwkSet;

    public EntityConfigurationBuilderEnhancedTests()
    {
        var ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        _signingKey = new ECDsaSecurityKey(ecdsa);
        _jwkSet = new { keys = new[] { new { kty = "EC", crv = "P-256", x = "test", y = "test", use = "sig" } } };
    }

    [Fact]
    public void Create_WithValidEntityUrl_ShouldCreateBuilder()
    {
        // Arrange & Act
        var builder = EntityConfigurationBuilder.Create("https://issuer.example.com");

        // Assert
        builder.Should().NotBeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("http://insecure.example.com")]
    [InlineData("ftp://example.com")]
    [InlineData("not-a-url")]
    public void Create_WithInvalidEntityUrl_ShouldThrow(string invalidUrl)
    {
        // Act & Assert
        var act = () => EntityConfigurationBuilder.Create(invalidUrl);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Build_WithoutSigningKey_ShouldThrow()
    {
        // Arrange
        var builder = EntityConfigurationBuilder.Create("https://issuer.example.com")
            .WithJwkSet(_jwkSet);

        // Act & Assert
        var act = () => builder.Build();
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("Signing key is required. Call WithSigningKey() first.");
    }

    [Fact]
    public void Build_WithoutJwkSet_ShouldThrow()
    {
        // Arrange
        var builder = EntityConfigurationBuilder.Create("https://issuer.example.com")
            .WithSigningKey(_signingKey);

        // Act & Assert
        var act = () => builder.Build();
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("JWK Set is required. Call WithJwkSet() first.");
    }

    [Fact]
    public void Build_WithValidConfiguration_ShouldReturnJwt()
    {
        // Arrange
        var builder = EntityConfigurationBuilder.Create("https://issuer.example.com")
            .WithSigningKey(_signingKey)
            .WithJwkSet(_jwkSet);

        // Act
        var jwt = builder.Build();

        // Assert
        jwt.Should().NotBeNullOrWhiteSpace();
        
        // Verify it's a valid JWT
        var tokenHandler = new JwtSecurityTokenHandler();
        tokenHandler.CanReadToken(jwt).Should().BeTrue();
        
        var token = tokenHandler.ReadJwtToken(jwt);
        token.Header.Typ.Should().Be(OidFederationConstants.JwtHeaders.EntityConfigurationType);
        token.Claims.Should().Contain(c => c.Type == "iss" && c.Value == "https://issuer.example.com");
        token.Claims.Should().Contain(c => c.Type == "sub" && c.Value == "https://issuer.example.com");
    }

    [Fact]
    public void WithSigningKey_WithSupportedAlgorithm_ShouldSucceed()
    {
        // Arrange
        var builder = EntityConfigurationBuilder.Create("https://issuer.example.com");

        // Act & Assert
        var act = () => builder.WithSigningKey(_signingKey, OidFederationConstants.SigningAlgorithms.ES256);
        act.Should().NotThrow();
    }

    [Fact]
    public void WithMetadata_WithValidMetadata_ShouldIncludeInJwt()
    {
        // Arrange
        var metadata = new EntityMetadata
        {
            OpenIdCredentialIssuer = new { credential_issuer = "https://issuer.example.com" }
        };
        
        var builder = EntityConfigurationBuilder.Create("https://issuer.example.com")
            .WithSigningKey(_signingKey)
            .WithJwkSet(_jwkSet)
            .WithMetadata(metadata);

        // Act
        var jwt = builder.Build();

        // Assert
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.ReadJwtToken(jwt);
        token.Claims.Should().Contain(c => c.Type == "metadata");
    }

    [Fact]
    public void AddAuthorityHint_WithValidUrl_ShouldAddToConfiguration()
    {
        // Arrange
        var builder = EntityConfigurationBuilder.Create("https://issuer.example.com")
            .WithSigningKey(_signingKey)
            .WithJwkSet(_jwkSet);

        // Act
        var config = builder
            .AddAuthorityHint("https://authority1.example.com")
            .AddAuthorityHint("https://authority2.example.com")
            .BuildConfiguration();

        // Assert
        config.AuthorityHints.Should().NotBeNull();
        config.AuthorityHints.Should().Contain("https://authority1.example.com");
        config.AuthorityHints.Should().Contain("https://authority2.example.com");
    }

    [Fact]
    public void AddAuthorityHint_WithInvalidUrl_ShouldThrow()
    {
        // Arrange
        var builder = EntityConfigurationBuilder.Create("https://issuer.example.com");

        // Act & Assert
        var act = () => builder.AddAuthorityHint("invalid-url");
        act.Should().Throw<ArgumentException>()
           .WithMessage("Authority URL must be a valid HTTPS URL*");
    }

    [Fact]
    public void WithAuthorityHints_WithValidUrls_ShouldAddAllHints()
    {
        // Arrange
        var builder = EntityConfigurationBuilder.Create("https://issuer.example.com")
            .WithSigningKey(_signingKey)
            .WithJwkSet(_jwkSet);

        // Act
        var config = builder
            .WithAuthorityHints("https://authority1.example.com", "https://authority2.example.com")
            .BuildConfiguration();

        // Assert
        config.AuthorityHints.Should().NotBeNull();
        config.AuthorityHints.Should().HaveCount(2);
        config.AuthorityHints.Should().Contain("https://authority1.example.com");
        config.AuthorityHints.Should().Contain("https://authority2.example.com");
    }

    [Fact]
    public void WithConstraints_WithValidConstraints_ShouldIncludeInJwt()
    {
        // Arrange
        var constraints = new EntityConstraints
        {
            MaxPathLength = 5
        };
        
        var builder = EntityConfigurationBuilder.Create("https://issuer.example.com")
            .WithSigningKey(_signingKey)
            .WithJwkSet(_jwkSet)
            .WithConstraints(constraints);

        // Act
        var jwt = builder.Build();

        // Assert
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.ReadJwtToken(jwt);
        token.Claims.Should().Contain(c => c.Type == "constraints");
    }

    [Fact]
    public void AddTrustMark_WithValidTrustMark_ShouldIncludeInJwt()
    {
        // Arrange
        var trustMark = TrustMark.Create("test-mark", "test-value", "https://issuer.example.com");
        
        var builder = EntityConfigurationBuilder.Create("https://issuer.example.com")
            .WithSigningKey(_signingKey)
            .WithJwkSet(_jwkSet)
            .AddTrustMark(trustMark);

        // Act
        var jwt = builder.Build();

        // Assert
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.ReadJwtToken(jwt);
        token.Claims.Should().Contain(c => c.Type == "trust_marks");
    }

    [Fact]
    public void WithValidity_WithValidHours_ShouldSetCorrectExpiration()
    {
        // Arrange
        var validityHours = 48;
        var builder = EntityConfigurationBuilder.Create("https://issuer.example.com")
            .WithSigningKey(_signingKey)
            .WithJwkSet(_jwkSet)
            .WithValidity(validityHours);

        // Act
        var jwt = builder.Build();

        // Assert
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.ReadJwtToken(jwt);
        
        var iatClaim = token.Claims.FirstOrDefault(c => c.Type == "iat")?.Value;
        var expClaim = token.Claims.FirstOrDefault(c => c.Type == "exp")?.Value;
        
        iatClaim.Should().NotBeNull();
        expClaim.Should().NotBeNull();
        
        var iat = long.Parse(iatClaim!);
        var exp = long.Parse(expClaim!);
        
        var expectedExp = iat + (validityHours * 3600);
        exp.Should().Be(expectedExp);
    }

    [Fact]
    public void WithValidity_WithInvalidHours_ShouldThrow()
    {
        // Arrange
        var builder = EntityConfigurationBuilder.Create("https://issuer.example.com");

        // Act & Assert
        var act = () => builder.WithValidity(0);
        act.Should().Throw<ArgumentException>()
           .WithMessage("Validity hours must be positive*");
    }

    [Fact]
    public void WithValidity_WithExcessiveHours_ShouldThrow()
    {
        // Arrange
        var builder = EntityConfigurationBuilder.Create("https://issuer.example.com");

        // Act & Assert
        var act = () => builder.WithValidity(10000); // Way more than max allowed
        act.Should().Throw<ArgumentException>()
           .WithMessage($"Validity hours cannot exceed {OidFederationConstants.Defaults.MaxValidityHours}*");
    }

    [Fact]
    public void WithTiming_WithValidTimes_ShouldSetCorrectTimestamps()
    {
        // Arrange
        var issuedAt = DateTimeOffset.UtcNow;
        var expiresAt = issuedAt.AddHours(24);
        
        var builder = EntityConfigurationBuilder.Create("https://issuer.example.com")
            .WithSigningKey(_signingKey)
            .WithJwkSet(_jwkSet)
            .WithTiming(issuedAt, expiresAt);

        // Act
        var jwt = builder.Build();

        // Assert
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.ReadJwtToken(jwt);
        
        var iatClaim = token.Claims.FirstOrDefault(c => c.Type == "iat")?.Value;
        var expClaim = token.Claims.FirstOrDefault(c => c.Type == "exp")?.Value;
        
        long.Parse(iatClaim!).Should().Be(issuedAt.ToUnixTimeSeconds());
        long.Parse(expClaim!).Should().Be(expiresAt.ToUnixTimeSeconds());
    }

    [Fact]
    public void WithTiming_WithInvalidTimes_ShouldThrow()
    {
        // Arrange
        var issuedAt = DateTimeOffset.UtcNow;
        var expiresAt = issuedAt.AddHours(-1); // Expires before issue
        
        var builder = EntityConfigurationBuilder.Create("https://issuer.example.com");

        // Act & Assert
        var act = () => builder.WithTiming(issuedAt, expiresAt);
        act.Should().Throw<ArgumentException>()
           .WithMessage("Expires at must be after issued at*");
    }

    [Fact]
    public void BuildConfiguration_WithValidSetup_ShouldReturnEntityConfiguration()
    {
        // Arrange
        var builder = EntityConfigurationBuilder.Create("https://issuer.example.com")
            .WithSigningKey(_signingKey)
            .WithJwkSet(_jwkSet);

        // Act
        var config = builder.BuildConfiguration();

        // Assert
        config.Should().NotBeNull();
        config.Issuer.Should().Be("https://issuer.example.com");
        config.Subject.Should().Be("https://issuer.example.com");
        config.JwkSet.Should().Be(_jwkSet);
    }

    [Fact]
    public void WithSigningKey_WithNullKey_ShouldThrow()
    {
        // Arrange
        var builder = EntityConfigurationBuilder.Create("https://issuer.example.com");

        // Act & Assert
        var act = () => builder.WithSigningKey(null!);
        act.Should().Throw<ArgumentNullException>()
           .WithParameterName("signingKey");
    }

    [Fact]
    public void WithJwkSet_WithNullJwkSet_ShouldThrow()
    {
        // Arrange
        var builder = EntityConfigurationBuilder.Create("https://issuer.example.com");

        // Act & Assert
        var act = () => builder.WithJwkSet(null!);
        act.Should().Throw<ArgumentNullException>()
           .WithParameterName("jwkSet");
    }

    [Fact]
    public void WithMetadata_WithNullMetadata_ShouldThrow()
    {
        // Arrange
        var builder = EntityConfigurationBuilder.Create("https://issuer.example.com");

        // Act & Assert
        var act = () => builder.WithMetadata(null!);
        act.Should().Throw<ArgumentNullException>()
           .WithParameterName("metadata");
    }

    [Fact]
    public void WithConstraints_WithNullConstraints_ShouldThrow()
    {
        // Arrange
        var builder = EntityConfigurationBuilder.Create("https://issuer.example.com");

        // Act & Assert
        var act = () => builder.WithConstraints(null!);
        act.Should().Throw<ArgumentNullException>()
           .WithParameterName("constraints");
    }

    [Fact]
    public void AddTrustMark_WithNullTrustMark_ShouldThrow()
    {
        // Arrange
        var builder = EntityConfigurationBuilder.Create("https://issuer.example.com");

        // Act & Assert
        var act = () => builder.AddTrustMark(null!);
        act.Should().Throw<ArgumentNullException>()
           .WithParameterName("trustMark");
    }

    [Fact]
    public void WithAuthorityHints_WithNullHints_ShouldThrow()
    {
        // Arrange
        var builder = EntityConfigurationBuilder.Create("https://issuer.example.com");

        // Act & Assert
        var act = () => builder.WithAuthorityHints(null!);
        act.Should().Throw<ArgumentNullException>()
           .WithParameterName("authorityUrls");
    }

    [Fact]
    public void Create_WithNullEntityUrl_ShouldThrow()
    {
        // Act & Assert
        var act = () => EntityConfigurationBuilder.Create(null!);
        act.Should().Throw<ArgumentException>()
           .WithMessage("*cannot be null or empty*");
    }
}
