using FluentAssertions;
using SdJwt.Net.Eudiw.Credentials;
using Xunit;

namespace SdJwt.Net.Eudiw.Tests.Credentials;

/// <summary>
/// Tests for QEAA (Qualified Electronic Attestation of Attributes) Handler.
/// </summary>
public class QeaaHandlerTests
{
    private readonly QeaaHandler _handler;

    public QeaaHandlerTests()
    {
        _handler = new QeaaHandler();
    }

    #region VCT Validation

    [Fact]
    public void IsQeaaCredential_WithValidQeaaVct_ReturnsTrue()
    {
        // Act
        var result = _handler.IsQeaaCredential("urn:eu:europa:ec:eudi:qeaa:diploma:bachelor");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsQeaaCredential_WithEaaVct_ReturnsFalse()
    {
        // Act
        var result = _handler.IsQeaaCredential("urn:eu:europa:ec:eudi:eaa:loyalty");

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("some.random.vct")]
    public void IsQeaaCredential_WithInvalidVct_ReturnsFalse(string? vct)
    {
        // Act
        var result = _handler.IsQeaaCredential(vct);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Credential Validation

    [Fact]
    public void Validate_WithValidQeaaClaims_ReturnsSuccess()
    {
        // Arrange
        var claims = CreateValidQeaaClaims();

        // Act
        var result = _handler.Validate(claims);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("iss")]
    [InlineData("iat")]
    [InlineData("exp")]
    [InlineData("vct")]
    public void Validate_WithMissingMandatoryClaim_ReturnsFailure(string missingClaim)
    {
        // Arrange
        var claims = CreateValidQeaaClaims();
        claims.Remove(missingClaim);

        // Act
        var result = _handler.Validate(claims);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains(missingClaim));
    }

    [Fact]
    public void Validate_WithExpiredCredential_ReturnsFailure()
    {
        // Arrange
        var claims = CreateValidQeaaClaims();
        claims["exp"] = DateTimeOffset.UtcNow.AddDays(-1).ToUnixTimeSeconds();

        // Act
        var result = _handler.Validate(claims);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("expired"));
    }

    [Fact]
    public void Validate_WithNullClaims_ReturnsFailure()
    {
        // Act
        var result = _handler.Validate(null);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    #endregion

    #region Issuer Trust Requirement

    [Fact]
    public void RequiresQualifiedIssuer_ReturnsTrue()
    {
        // Assert - QEAA must be issued by qualified trust service providers
        _handler.RequiresQualifiedIssuer.Should().BeTrue();
    }

    #endregion

    #region Credential Types

    [Theory]
    [InlineData("urn:eu:europa:ec:eudi:qeaa:diploma:bachelor", "diploma")]
    [InlineData("urn:eu:europa:ec:eudi:qeaa:professional:engineer", "professional")]
    [InlineData("urn:eu:europa:ec:eudi:qeaa:health:vaccination", "health")]
    public void ExtractCredentialCategory_ReturnsCorrectCategory(string vct, string expectedCategory)
    {
        // Act
        var category = _handler.ExtractCredentialCategory(vct);

        // Assert
        category.Should().Be(expectedCategory);
    }

    #endregion

    #region Helper Methods

    private static Dictionary<string, object> CreateValidQeaaClaims()
    {
        return new Dictionary<string, object>
        {
            ["iss"] = "https://issuer.eudiw.example.eu",
            ["iat"] = DateTimeOffset.UtcNow.AddDays(-30).ToUnixTimeSeconds(),
            ["exp"] = DateTimeOffset.UtcNow.AddYears(1).ToUnixTimeSeconds(),
            ["vct"] = "urn:eu:europa:ec:eudi:qeaa:diploma:bachelor",
            ["sub"] = "user123",
            ["cnf"] = new Dictionary<string, object>
            {
                ["jwk"] = new Dictionary<string, object>
                {
                    ["kty"] = "EC",
                    ["crv"] = "P-256"
                }
            }
        };
    }

    #endregion
}
