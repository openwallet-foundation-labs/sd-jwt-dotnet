using SdJwt.Net.OidFederation.Models;

namespace SdJwt.Net.OidFederation.Tests.Models;

public class EntityStatementTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldCreateEntityStatement()
    {
        // Arrange
        var issuerUrl = "https://authority.example.com";
        var subjectUrl = "https://entity.example.com";

        // Act
        var statement = EntityStatement.Create(issuerUrl, subjectUrl);

        // Assert
        statement.Should().NotBeNull();
        statement.Issuer.Should().Be(issuerUrl);
        statement.Subject.Should().Be(subjectUrl);
        statement.IssuedAt.Should().BeGreaterThan(0);
        statement.ExpiresAt.Should().BeGreaterThan(statement.IssuedAt);
    }

    [Fact]
    public void Create_WithCustomValidityHours_ShouldSetCorrectExpiration()
    {
        // Arrange
        var issuerUrl = "https://authority.example.com";
        var subjectUrl = "https://entity.example.com";
        var validityHours = 72;

        // Act
        var statement = EntityStatement.Create(issuerUrl, subjectUrl, validityHours);

        // Assert
        var expectedExpiry = statement.IssuedAt + (validityHours * 3600);
        statement.ExpiresAt.Should().Be(expectedExpiry);
    }

    [Fact]
    public void Validate_WithValidStatement_ShouldNotThrow()
    {
        // Arrange
        var statement = EntityStatement.Create("https://authority.example.com", "https://entity.example.com");

        // Act & Assert
        var act = () => statement.Validate();
        act.Should().NotThrow();
    }

    [Fact]
    public void Validate_WithNullIssuer_ShouldThrow()
    {
        // Arrange
        var statement = EntityStatement.Create("https://authority.example.com", "https://entity.example.com");
        statement.Issuer = null!;

        // Act & Assert
        var act = () => statement.Validate();
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("Issuer is required");
    }

    [Fact]
    public void Validate_WithSameIssuerAndSubject_ShouldThrow()
    {
        // Arrange
        var statement = EntityStatement.Create("https://entity.example.com", "https://entity.example.com");

        // Act & Assert
        var act = () => statement.Validate();
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("Issuer and Subject must be different for entity statements");
    }

    [Theory]
    [InlineData("http://insecure.example.com", "https://entity.example.com")]
    [InlineData("https://authority.example.com", "ftp://entity.example.com")]
    [InlineData("not-a-url", "https://entity.example.com")]
    [InlineData("https://authority.example.com", "invalid-url")]
    public void Validate_WithInvalidUrls_ShouldThrow(string issuerUrl, string subjectUrl)
    {
        // Arrange
        var statement = new EntityStatement
        {
            Issuer = issuerUrl,
            Subject = subjectUrl,
            IssuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(1).ToUnixTimeSeconds()
        };

        // Act & Assert
        var act = () => statement.Validate();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Validate_WithInvalidTimestamps_ShouldThrow()
    {
        // Arrange
        var statement = EntityStatement.Create("https://authority.example.com", "https://entity.example.com");
        statement.ExpiresAt = statement.IssuedAt - 1; // Expired before issued

        // Act & Assert
        var act = () => statement.Validate();
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("ExpiresAt must be after IssuedAt");
    }

    [Fact]
    public void Validate_WithInvalidAuthorityHints_ShouldThrow()
    {
        // Arrange
        var statement = EntityStatement.Create("https://authority.example.com", "https://entity.example.com");
        statement.AuthorityHints = new[] { "invalid-authority-url" };

        // Act & Assert
        var act = () => statement.Validate();
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("Authority hint 'invalid-authority-url' must be a valid HTTPS URL");
    }

    [Fact]
    public void Validate_WithInvalidSourceEndpoint_ShouldThrow()
    {
        // Arrange
        var statement = EntityStatement.Create("https://authority.example.com", "https://entity.example.com");
        statement.SourceEndpoint = "not-a-url";

        // Act & Assert
        var act = () => statement.Validate();
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("SourceEndpoint must be a valid HTTPS URL");
    }

    [Fact]
    public void Validate_WithValidAuthorityHints_ShouldNotThrow()
    {
        // Arrange
        var statement = EntityStatement.Create("https://authority.example.com", "https://entity.example.com");
        statement.AuthorityHints = new[] 
        { 
            "https://higher-authority1.example.com",
            "https://higher-authority2.example.com" 
        };

        // Act & Assert
        var act = () => statement.Validate();
        act.Should().NotThrow();
    }

    [Fact]
    public void Validate_WithValidSourceEndpoint_ShouldNotThrow()
    {
        // Arrange
        var statement = EntityStatement.Create("https://authority.example.com", "https://entity.example.com");
        statement.SourceEndpoint = "https://federation.example.com/fetch";

        // Act & Assert
        var act = () => statement.Validate();
        act.Should().NotThrow();
    }

    [Fact]
    public void Validate_WithMetadataPolicy_ShouldValidatePolicy()
    {
        // Arrange
        var statement = EntityStatement.Create("https://authority.example.com", "https://entity.example.com");
        statement.MetadataPolicy = new MetadataPolicy
        {
            OpenIdCredentialIssuer = new MetadataPolicyRules()
        };

        // Act & Assert
        var act = () => statement.Validate();
        act.Should().NotThrow();
    }

    [Fact]
    public void Validate_WithConstraints_ShouldValidateConstraints()
    {
        // Arrange
        var statement = EntityStatement.Create("https://authority.example.com", "https://entity.example.com");
        statement.Constraints = new EntityConstraints
        {
            MaxPathLength = 5
        };

        // Act & Assert
        var act = () => statement.Validate();
        act.Should().NotThrow();
    }

    [Fact]
    public void Validate_WithTrustMarks_ShouldValidateTrustMarks()
    {
        // Arrange
        var statement = EntityStatement.Create("https://authority.example.com", "https://entity.example.com");
        var trustMark = TrustMark.Create("authority-trust-mark", "verified", "https://authority.example.com", "https://entity.example.com");
        statement.TrustMarks = new[] { trustMark };

        // Act & Assert
        var act = () => statement.Validate();
        act.Should().NotThrow();
    }
}