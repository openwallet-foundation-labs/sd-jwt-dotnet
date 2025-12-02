using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Moq;
using SdJwt.Net.HAIP;
using SdJwt.Net.HAIP.Models;
using SdJwt.Net.HAIP.Validators;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using Xunit;

namespace SdJwt.Net.HAIP.Tests.Enhanced;

public class HaipEnhancedTests
{
    private readonly Mock<ILogger<HaipCryptoValidator>> _loggerMock;
    private readonly HaipCryptoValidator _validator;

    public HaipEnhancedTests()
    {
        _loggerMock = new Mock<ILogger<HaipCryptoValidator>>();
        _validator = new HaipCryptoValidator(HaipLevel.Level1_High, _loggerMock.Object);
    }

    [Fact]
    public void ValidateJwtHeader_WithWeakCryptographicAlgorithm_ShouldDetectViolation()
    {
        // Arrange
        var weakKey = new RsaSecurityKey(RSA.Create(1024)); // Weak key size
        var credentials = new SigningCredentials(weakKey, "HS256"); // Use weak algorithm directly
        var header = new JwtHeader(credentials);
        // Don't add alg manually as it's already set by the credentials

        // Act
        var result = _validator.ValidateJwtHeader(header);

        // Assert
        result.Should().NotBeNull();
        result.IsCompliant.Should().BeFalse();
        result.Violations.Should().NotBeEmpty();
        result.Violations.Should().Contain(v => v.Type == HaipViolationType.WeakCryptography);
    }

    [Fact]
    public void ValidateJwtHeader_WithMissingAlgorithm_ShouldDetectViolation()
    {
        // Arrange
        var header = new JwtHeader();
        // Missing algorithm header

        // Act
        var result = _validator.ValidateJwtHeader(header);

        // Assert
        result.Should().NotBeNull();
        result.IsCompliant.Should().BeFalse();
        result.Violations.Should().NotBeEmpty();
        result.Violations.Should().Contain(v => v.Description.Contains("missing algorithm"));
    }

    [Fact]
    public void ValidateKeyCompliance_WithValidKey_ShouldPass()
    {
        // Arrange
        using var ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var key = new ECDsaSecurityKey(ecdsa);
        var algorithm = "ES256";

        // Act
        var result = _validator.ValidateKeyCompliance(key, algorithm);

        // Assert
        result.Should().NotBeNull();
        result.IsCompliant.Should().BeTrue();
        result.Violations.Should().BeEmpty();
        result.AchievedLevel.Should().Be(HaipLevel.Level1_High);
    }

    [Fact]
    public void ValidateKeyCompliance_WithWeakKey_ShouldDetectViolation()
    {
        // Arrange
        var rsa = RSA.Create(1024); // Weak key
        var key = new RsaSecurityKey(rsa);
        var algorithm = "PS256";

        // Act
        var result = _validator.ValidateKeyCompliance(key, algorithm);

        // Assert
        result.Should().NotBeNull();
        result.IsCompliant.Should().BeFalse();
        result.Violations.Should().NotBeEmpty();
        result.Violations.Should().Contain(v => v.Type == HaipViolationType.WeakKeyStrength);
    }

    [Fact]
    public void ValidateAlgorithm_WithAllowedAlgorithm_ShouldPass()
    {
        // Arrange
        var algorithm = "ES256";

        // Act
        var result = _validator.ValidateAlgorithm(algorithm);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeTrue();
        result.ErrorMessage.Should().BeNullOrEmpty();
    }

    [Fact]
    public void ValidateAlgorithm_WithForbiddenAlgorithm_ShouldFail()
    {
        // Arrange
        var algorithm = "HS256"; // Forbidden

        // Act
        var result = _validator.ValidateAlgorithm(algorithm);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNullOrEmpty();
        result.ErrorMessage.Should().Contain("forbidden");
    }

    [Fact]
    public void ValidateAlgorithm_WithEmptyAlgorithm_ShouldFail()
    {
        // Arrange
        var algorithm = string.Empty;

        // Act
        var result = _validator.ValidateAlgorithm(algorithm);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateValidator()
    {
        // Arrange & Act
        var validator = new HaipCryptoValidator(HaipLevel.Level2_VeryHigh, _loggerMock.Object);

        // Assert
        validator.Should().NotBeNull();
    }

    [Theory]
    [InlineData(HaipLevel.Level1_High, "ES256", true)]
    [InlineData(HaipLevel.Level1_High, "HS256", false)]
    [InlineData(HaipLevel.Level2_VeryHigh, "ES256", false)]
    [InlineData(HaipLevel.Level2_VeryHigh, "ES384", true)]
    [InlineData(HaipLevel.Level3_Sovereign, "ES256", false)]
    [InlineData(HaipLevel.Level3_Sovereign, "ES512", true)]
    public void ValidateAlgorithm_WithDifferentLevels_ShouldValidateCorrectly(
        HaipLevel level, string algorithm, bool expectedValid)
    {
        // Arrange
        var validator = new HaipCryptoValidator(level, _loggerMock.Object);

        // Act
        var result = validator.ValidateAlgorithm(algorithm);

        // Assert
        result.IsValid.Should().Be(expectedValid);
    }
}
