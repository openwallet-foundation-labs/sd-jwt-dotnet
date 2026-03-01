using FluentAssertions;
using SdJwt.Net.Eudiw.Arf;
using Xunit;

namespace SdJwt.Net.Eudiw.Tests.Arf;

/// <summary>
/// Tests for ARF Profile Validator ensuring eIDAS 2.0 ARF compliance.
/// </summary>
public class ArfProfileValidatorTests
{
    private readonly ArfProfileValidator _validator;

    public ArfProfileValidatorTests()
    {
        _validator = new ArfProfileValidator();
    }

    #region Algorithm Validation

    [Theory]
    [InlineData("ES256", true)]
    [InlineData("ES384", true)]
    [InlineData("ES512", true)]
    public void ValidateAlgorithm_WithSupportedAlgorithm_ReturnsTrue(string algorithm, bool expected)
    {
        // Act
        var result = _validator.ValidateAlgorithm(algorithm);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("RS256")]
    [InlineData("RS384")]
    [InlineData("RS512")]
    [InlineData("PS256")]
    [InlineData("EdDSA")]
    public void ValidateAlgorithm_WithUnsupportedAlgorithm_ReturnsFalse(string algorithm)
    {
        // Act - ARF mandates ECDSA algorithms only
        var result = _validator.ValidateAlgorithm(algorithm);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("MD5")]
    [InlineData("SHA1")]
    [InlineData("SHA-1")]
    [InlineData("HS256")]
    [InlineData("HS384")]
    [InlineData("HS512")]
    public void ValidateAlgorithm_WithWeakOrSymmetricAlgorithm_ReturnsFalse(string algorithm)
    {
        // Act - Weak algorithms must never be allowed
        var result = _validator.ValidateAlgorithm(algorithm);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void ValidateAlgorithm_WithNullOrEmptyAlgorithm_ReturnsFalse(string? algorithm)
    {
        // Act
        var result = _validator.ValidateAlgorithm(algorithm);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Credential Type Validation

    [Fact]
    public void ValidateCredentialType_WithPidDocType_ReturnsValidResult()
    {
        // Act
        var result = _validator.ValidateCredentialType("eu.europa.ec.eudi.pid.1");

        // Assert
        result.IsValid.Should().BeTrue();
        result.CredentialType.Should().Be(ArfCredentialType.Pid);
    }

    [Fact]
    public void ValidateCredentialType_WithMdlDocType_ReturnsValidResult()
    {
        // Act
        var result = _validator.ValidateCredentialType("org.iso.18013.5.1.mDL");

        // Assert
        result.IsValid.Should().BeTrue();
        result.CredentialType.Should().Be(ArfCredentialType.Mdl);
    }

    [Theory]
    [InlineData("unknown.doctype")]
    [InlineData("invalid")]
    [InlineData("")]
    public void ValidateCredentialType_WithInvalidDocType_ReturnsInvalidResult(string docType)
    {
        // Act
        var result = _validator.ValidateCredentialType(docType);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Error.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void ValidateCredentialType_WithQeaaVct_ReturnsValidResult()
    {
        // Act
        var result = _validator.ValidateCredentialType("urn:eu:europa:ec:eudi:qeaa:diploma");

        // Assert
        result.IsValid.Should().BeTrue();
        result.CredentialType.Should().Be(ArfCredentialType.Qeaa);
    }

    [Fact]
    public void ValidateCredentialType_WithEaaVct_ReturnsValidResult()
    {
        // Act
        var result = _validator.ValidateCredentialType("urn:eu:europa:ec:eudi:eaa:membership");

        // Assert
        result.IsValid.Should().BeTrue();
        result.CredentialType.Should().Be(ArfCredentialType.Eaa);
    }

    #endregion

    #region PID Mandatory Claims Validation

    [Fact]
    public void ValidatePidClaims_WithAllMandatoryClaims_ReturnsValid()
    {
        // Arrange
        var claims = new Dictionary<string, object>
        {
            ["family_name"] = "Doe",
            ["given_name"] = "John",
            ["birth_date"] = "1990-01-15",
            ["issuance_date"] = "2024-01-01",
            ["expiry_date"] = "2029-01-01",
            ["issuing_authority"] = "Test Authority",
            ["issuing_country"] = "DE"
        };

        // Act
        var result = _validator.ValidatePidClaims(claims);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("family_name")]
    [InlineData("given_name")]
    [InlineData("birth_date")]
    [InlineData("issuance_date")]
    [InlineData("expiry_date")]
    [InlineData("issuing_authority")]
    [InlineData("issuing_country")]
    public void ValidatePidClaims_WithMissingMandatoryClaim_ReturnsInvalid(string missingClaim)
    {
        // Arrange
        var claims = new Dictionary<string, object>
        {
            ["family_name"] = "Doe",
            ["given_name"] = "John",
            ["birth_date"] = "1990-01-15",
            ["issuance_date"] = "2024-01-01",
            ["expiry_date"] = "2029-01-01",
            ["issuing_authority"] = "Test Authority",
            ["issuing_country"] = "DE"
        };
        claims.Remove(missingClaim);

        // Act
        var result = _validator.ValidatePidClaims(claims);

        // Assert
        result.IsValid.Should().BeFalse();
        result.MissingClaims.Should().Contain(missingClaim);
    }

    [Fact]
    public void ValidatePidClaims_WithNullClaims_ReturnsInvalid()
    {
        // Act
        var result = _validator.ValidatePidClaims(null);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Error.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void ValidatePidClaims_WithEmptyClaims_ReturnsInvalid()
    {
        // Act
        var result = _validator.ValidatePidClaims(new Dictionary<string, object>());

        // Assert
        result.IsValid.Should().BeFalse();
        result.MissingClaims.Should().NotBeEmpty();
    }

    #endregion

    #region Validity Period Validation

    [Fact]
    public void ValidateValidityPeriod_WithValidPeriod_ReturnsValid()
    {
        // Arrange
        var issuanceDate = DateTimeOffset.UtcNow.AddDays(-30);
        var expiryDate = DateTimeOffset.UtcNow.AddYears(5);

        // Act
        var result = _validator.ValidateValidityPeriod(issuanceDate, expiryDate);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ValidateValidityPeriod_WithExpiredCredential_ReturnsInvalid()
    {
        // Arrange
        var issuanceDate = DateTimeOffset.UtcNow.AddYears(-6);
        var expiryDate = DateTimeOffset.UtcNow.AddDays(-1);

        // Act
        var result = _validator.ValidateValidityPeriod(issuanceDate, expiryDate);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Error.Should().Contain("expired");
    }

    [Fact]
    public void ValidateValidityPeriod_WithFutureIssuanceDate_ReturnsInvalid()
    {
        // Arrange
        var issuanceDate = DateTimeOffset.UtcNow.AddDays(1);
        var expiryDate = DateTimeOffset.UtcNow.AddYears(5);

        // Act
        var result = _validator.ValidateValidityPeriod(issuanceDate, expiryDate);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Error.Should().Contain("future");
    }

    [Fact]
    public void ValidateValidityPeriod_WithExpiryBeforeIssuance_ReturnsInvalid()
    {
        // Arrange
        var issuanceDate = DateTimeOffset.UtcNow;
        var expiryDate = DateTimeOffset.UtcNow.AddDays(-1);

        // Act
        var result = _validator.ValidateValidityPeriod(issuanceDate, expiryDate);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    #endregion

    #region Member State Validation

    [Theory]
    [InlineData("DE", true)]
    [InlineData("FR", true)]
    [InlineData("IT", true)]
    [InlineData("US", false)]
    [InlineData("GB", false)]
    [InlineData("CN", false)]
    [InlineData("XX", false)]
    public void ValidateMemberState_ReturnsExpectedResult(string countryCode, bool expectedValid)
    {
        // Act
        var result = _validator.ValidateMemberState(countryCode);

        // Assert
        result.Should().Be(expectedValid);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ValidateMemberState_WithNullOrEmpty_ReturnsFalse(string? countryCode)
    {
        // Act
        var result = _validator.ValidateMemberState(countryCode);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("de")]
    [InlineData("De")]
    [InlineData("dE")]
    public void ValidateMemberState_IsCaseInsensitive(string countryCode)
    {
        // Act
        var result = _validator.ValidateMemberState(countryCode);

        // Assert
        result.Should().BeTrue();
    }

    #endregion
}
