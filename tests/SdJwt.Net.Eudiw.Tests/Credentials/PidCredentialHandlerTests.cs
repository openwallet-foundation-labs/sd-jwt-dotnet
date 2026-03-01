using FluentAssertions;
using SdJwt.Net.Eudiw.Credentials;
using Xunit;

namespace SdJwt.Net.Eudiw.Tests.Credentials;

/// <summary>
/// Tests for PID Credential Handler per EUDIW ARF requirements.
/// </summary>
public class PidCredentialHandlerTests
{
    private readonly PidCredentialHandler _handler;

    public PidCredentialHandlerTests()
    {
        _handler = new PidCredentialHandler();
    }

    #region Claim Extraction

    [Fact]
    public void ExtractFamilyName_WithValidClaims_ReturnsValue()
    {
        // Arrange
        var claims = CreateValidPidClaims();

        // Act
        var result = _handler.ExtractFamilyName(claims);

        // Assert
        result.Should().Be("Mustermann");
    }

    [Fact]
    public void ExtractGivenName_WithValidClaims_ReturnsValue()
    {
        // Arrange
        var claims = CreateValidPidClaims();

        // Act
        var result = _handler.ExtractGivenName(claims);

        // Assert
        result.Should().Be("Erika");
    }

    [Fact]
    public void ExtractBirthDate_WithValidClaims_ReturnsValue()
    {
        // Arrange
        var claims = CreateValidPidClaims();

        // Act
        var result = _handler.ExtractBirthDate(claims);

        // Assert
        result.Should().Be(new DateTime(1964, 8, 12));
    }

    [Fact]
    public void ExtractIssuingCountry_WithValidClaims_ReturnsValue()
    {
        // Arrange
        var claims = CreateValidPidClaims();

        // Act
        var result = _handler.ExtractIssuingCountry(claims);

        // Assert
        result.Should().Be("DE");
    }

    [Fact]
    public void ExtractAgeOver18_WhenPresent_ReturnsTrue()
    {
        // Arrange
        var claims = CreateValidPidClaims();
        claims["age_over_18"] = true;

        // Act
        var result = _handler.ExtractAgeOver18(claims);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ExtractAgeOver18_WhenAbsent_ReturnsNull()
    {
        // Arrange
        var claims = CreateValidPidClaims();

        // Act
        var result = _handler.ExtractAgeOver18(claims);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region Validation

    [Fact]
    public void Validate_WithValidClaims_ReturnsSuccess()
    {
        // Arrange
        var claims = CreateValidPidClaims();

        // Act
        var result = _handler.Validate(claims);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("family_name")]
    [InlineData("given_name")]
    [InlineData("birth_date")]
    [InlineData("issuance_date")]
    [InlineData("expiry_date")]
    [InlineData("issuing_authority")]
    [InlineData("issuing_country")]
    public void Validate_WithMissingMandatoryClaim_ReturnsFailure(string missingClaim)
    {
        // Arrange
        var claims = CreateValidPidClaims();
        claims.Remove(missingClaim);

        // Act
        var result = _handler.Validate(claims);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Contains(missingClaim));
    }

    [Fact]
    public void Validate_WithNullClaims_ReturnsFailure()
    {
        // Act
        var result = _handler.Validate(null);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_WithExpiredCredential_ReturnsFailure()
    {
        // Arrange
        var claims = CreateValidPidClaims();
        claims["expiry_date"] = "2020-01-01";

        // Act
        var result = _handler.Validate(claims);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("expired"));
    }

    [Fact]
    public void Validate_WithInvalidIssuingCountry_ReturnsFailure()
    {
        // Arrange
        var claims = CreateValidPidClaims();
        claims["issuing_country"] = "XX"; // Not an EU member state

        // Act
        var result = _handler.Validate(claims);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("member state"));
    }

    #endregion

    #region PID Credential Model

    [Fact]
    public void ToPidCredential_WithValidClaims_CreatesModel()
    {
        // Arrange
        var claims = CreateValidPidClaims();

        // Act
        var credential = _handler.ToPidCredential(claims);

        // Assert
        credential.Should().NotBeNull();
        credential.FamilyName.Should().Be("Mustermann");
        credential.GivenName.Should().Be("Erika");
        credential.BirthDate.Should().Be(new DateTime(1964, 8, 12));
        credential.IssuingCountry.Should().Be("DE");
    }

    [Fact]
    public void ToPidCredential_WithOptionalClaims_IncludesOptionalFields()
    {
        // Arrange
        var claims = CreateValidPidClaims();
        claims["age_over_18"] = true;
        claims["nationality"] = "DE";
        claims["resident_address"] = "Berlin, Germany";

        // Act
        var credential = _handler.ToPidCredential(claims);

        // Assert
        credential.AgeOver18.Should().BeTrue();
        credential.Nationality.Should().Be("DE");
        credential.ResidentAddress.Should().Be("Berlin, Germany");
    }

    [Fact]
    public void ToPidCredential_WithInvalidClaims_ThrowsException()
    {
        // Arrange
        var claims = new Dictionary<string, object>(); // Empty, missing mandatory claims

        // Act
        var act = () => _handler.ToPidCredential(claims);

        // Assert
        act.Should().Throw<PidValidationException>();
    }

    #endregion

    #region Helper Methods

    private static Dictionary<string, object> CreateValidPidClaims()
    {
        return new Dictionary<string, object>
        {
            ["family_name"] = "Mustermann",
            ["given_name"] = "Erika",
            ["birth_date"] = "1964-08-12",
            ["issuance_date"] = "2024-01-01",
            ["expiry_date"] = "2029-01-01",
            ["issuing_authority"] = "Bundesdruckerei",
            ["issuing_country"] = "DE"
        };
    }

    #endregion
}
