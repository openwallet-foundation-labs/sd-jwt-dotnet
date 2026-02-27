using SdJwt.Net.OidFederation.Models;

namespace SdJwt.Net.OidFederation.Tests.Models;

public class EntityConfigurationTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldCreateEntityConfiguration()
    {
        // Arrange
        var entityUrl = "https://issuer.example.com";
        var jwkSet = new
        {
            keys = new[] { new { kty = "EC", crv = "P-256" } }
        };

        // Act
        var config = EntityConfiguration.Create(entityUrl, jwkSet);

        // Assert
        config.Should().NotBeNull();
        config.Issuer.Should().Be(entityUrl);
        config.Subject.Should().Be(entityUrl);
        config.JwkSet.Should().Be(jwkSet);
        config.IssuedAt.Should().BeGreaterThan(0);
        config.ExpiresAt.Should().BeGreaterThan(config.IssuedAt);
    }

    [Fact]
    public void Create_WithCustomValidityHours_ShouldSetCorrectExpiration()
    {
        // Arrange
        var entityUrl = "https://issuer.example.com";
        var jwkSet = new
        {
            keys = new[] { new { kty = "EC" } }
        };
        var validityHours = 48;

        // Act
        var config = EntityConfiguration.Create(entityUrl, jwkSet, validityHours);

        // Assert
        var expectedExpiry = config.IssuedAt + (validityHours * 3600);
        config.ExpiresAt.Should().Be(expectedExpiry);
    }

    [Fact]
    public void Validate_WithValidConfiguration_ShouldNotThrow()
    {
        // Arrange
        var config = EntityConfiguration.Create("https://issuer.example.com", new
        {
            keys = new object[] { }
        });

        // Act & Assert
        var act = () => config.Validate();
        act.Should().NotThrow();
    }

    [Fact]
    public void Validate_WithNullIssuer_ShouldThrow()
    {
        // Arrange
        var config = EntityConfiguration.Create("https://issuer.example.com", new
        {
            keys = new object[] { }
        });
        config.Issuer = null!;

        // Act & Assert
        var act = () => config.Validate();
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("Issuer is required");
    }

    [Fact]
    public void Validate_WithMismatchedIssuerAndSubject_ShouldThrow()
    {
        // Arrange
        var config = EntityConfiguration.Create("https://issuer.example.com", new
        {
            keys = new object[] { }
        });
        config.Subject = "https://different.example.com";

        // Act & Assert
        var act = () => config.Validate();
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("Issuer and Subject must be the same for entity configurations");
    }

    [Fact]
    public void Validate_WithInvalidHttpUrl_ShouldThrow()
    {
        // Arrange
        var config = EntityConfiguration.Create("https://issuer.example.com", new
        {
            keys = new object[] { }
        });
        config.Issuer = "http://insecure.example.com"; // HTTP instead of HTTPS
        config.Subject = "http://insecure.example.com";

        // Act & Assert
        var act = () => config.Validate();
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("Issuer must be a valid HTTPS URL");
    }

    [Fact]
    public void Validate_WithInvalidTimestamps_ShouldThrow()
    {
        // Arrange
        var config = EntityConfiguration.Create("https://issuer.example.com", new
        {
            keys = new object[] { }
        });
        config.ExpiresAt = config.IssuedAt - 1; // Expired before issued

        // Act & Assert
        var act = () => config.Validate();
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("ExpiresAt must be after IssuedAt");
    }

    [Fact]
    public void Validate_WithInvalidAuthorityHints_ShouldThrow()
    {
        // Arrange
        var config = EntityConfiguration.Create("https://issuer.example.com", new
        {
            keys = new object[] { }
        });
        config.AuthorityHints = new[] { "invalid-url" };

        // Act & Assert
        var act = () => config.Validate();
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("Authority hint 'invalid-url' must be a valid HTTPS URL");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("ftp://example.com")]
    [InlineData("http://example.com")]
    [InlineData("not-a-url")]
    public void Validate_WithInvalidUrls_ShouldThrow(string invalidUrl)
    {
        // Arrange & Act & Assert
        var act = () => EntityConfiguration.Create(invalidUrl, new { keys = new object[] { } });
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Validate_WithValidAuthorityHints_ShouldNotThrow()
    {
        // Arrange
        var config = EntityConfiguration.Create("https://issuer.example.com", new
        {
            keys = new object[] { }
        });
        config.AuthorityHints = new[]
        {
            "https://authority1.example.com",
            "https://authority2.example.com"
        };

        // Act & Assert
        var act = () => config.Validate();
        act.Should().NotThrow();
    }

    [Fact]
    public void Validate_WithValidTrustMarks_ShouldNotThrow()
    {
        // Arrange
        var config = EntityConfiguration.Create("https://issuer.example.com", new
        {
            keys = new object[] { }
        });
        var trustMark = TrustMark.Create("test-trust-mark", "test-value", "https://issuer.example.com");
        config.TrustMarks = new[] { trustMark };

        // Act & Assert
        var act = () => config.Validate();
        act.Should().NotThrow();
    }

    [Fact]
    public void Validate_WithInvalidTrustMarks_ShouldThrow()
    {
        // Arrange
        var config = EntityConfiguration.Create("https://issuer.example.com", new
        {
            keys = new object[] { }
        });
        var invalidTrustMark = new TrustMark { Id = "", TrustMarkValue = "test" }; // Invalid ID
        config.TrustMarks = new[] { invalidTrustMark };

        // Act & Assert
        var act = () => config.Validate();
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("Trust mark ID is required");
    }
}
