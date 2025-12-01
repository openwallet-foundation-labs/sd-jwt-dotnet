using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Moq;
using SdJwt.Net.HAIP.Models;
using SdJwt.Net.HAIP.Validators;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using Xunit;

namespace SdJwt.Net.HAIP.Tests.Validators;

/// <summary>
/// Tests for HAIP cryptographic validator
/// </summary>
public class HaipCryptoValidatorTests : IDisposable
{
    private readonly Mock<ILogger<HaipCryptoValidator>> _mockLogger;
    private readonly ECDsa _ecdsaP256;
    private readonly ECDsa _ecdsaP384;
    private readonly ECDsa _ecdsaP521;
    private readonly RSA _rsa2048;
    private readonly RSA _rsa4096;

    public HaipCryptoValidatorTests()
    {
        _mockLogger = new Mock<ILogger<HaipCryptoValidator>>();
        
        // Create test keys
        _ecdsaP256 = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        _ecdsaP384 = ECDsa.Create(ECCurve.NamedCurves.nistP384);
        _ecdsaP521 = ECDsa.Create(ECCurve.NamedCurves.nistP521);
        _rsa2048 = RSA.Create(2048);
        _rsa4096 = RSA.Create(4096);
    }

    public void Dispose()
    {
        _ecdsaP256?.Dispose();
        _ecdsaP384?.Dispose();
        _ecdsaP521?.Dispose();
        _rsa2048?.Dispose();
        _rsa4096?.Dispose();
    }

    [Theory]
    [InlineData(HaipLevel.Level1_High, "ES256", true)]
    [InlineData(HaipLevel.Level1_High, "ES384", true)]
    [InlineData(HaipLevel.Level1_High, "PS256", true)]
    [InlineData(HaipLevel.Level1_High, "EdDSA", true)]
    [InlineData(HaipLevel.Level1_High, "RS256", false)]
    [InlineData(HaipLevel.Level1_High, "HS256", false)]
    [InlineData(HaipLevel.Level1_High, "none", false)]
    [InlineData(HaipLevel.Level2_VeryHigh, "ES256", false)]
    [InlineData(HaipLevel.Level2_VeryHigh, "ES384", true)]
    [InlineData(HaipLevel.Level2_VeryHigh, "ES512", true)]
    [InlineData(HaipLevel.Level3_Sovereign, "ES384", false)]
    [InlineData(HaipLevel.Level3_Sovereign, "ES512", true)]
    [InlineData(HaipLevel.Level3_Sovereign, "PS512", true)]
    public void ValidateAlgorithm_ShouldReturnExpectedResult(HaipLevel level, string algorithm, bool expectedValid)
    {
        // Arrange
        var validator = new HaipCryptoValidator(level, _mockLogger.Object);

        // Act
        var result = validator.ValidateAlgorithm(algorithm);

        // Assert
        result.IsValid.Should().Be(expectedValid);
        
        if (!expectedValid)
        {
            result.ErrorMessage.Should().NotBeNullOrEmpty();
        }
        else
        {
            result.Details.Should().NotBeNullOrEmpty();
        }
    }

    [Fact]
    public void ValidateAlgorithm_WithNullOrEmpty_ShouldReturnInvalid()
    {
        // Arrange
        var validator = new HaipCryptoValidator(HaipLevel.Level1_High, _mockLogger.Object);

        // Act & Assert
        validator.ValidateAlgorithm(null!).IsValid.Should().BeFalse();
        validator.ValidateAlgorithm("").IsValid.Should().BeFalse();
        validator.ValidateAlgorithm("   ").IsValid.Should().BeFalse();
    }

    [Fact]
    public void ValidateKeyCompliance_Level1_WithValidECKey_ShouldBeCompliant()
    {
        // Arrange
        var validator = new HaipCryptoValidator(HaipLevel.Level1_High, _mockLogger.Object);
        var key = new ECDsaSecurityKey(_ecdsaP256) { KeyId = "test-key" };

        // Act
        var result = validator.ValidateKeyCompliance(key, "ES256");

        // Assert
        result.IsCompliant.Should().BeTrue();
        result.Violations.Should().BeEmpty();
        result.AchievedLevel.Should().Be(HaipLevel.Level1_High);
        result.AuditTrail.Steps.Should().NotBeEmpty();
    }

    [Fact]
    public void ValidateKeyCompliance_Level2_WithP256Key_ShouldNotBeCompliant()
    {
        // Arrange
        var validator = new HaipCryptoValidator(HaipLevel.Level2_VeryHigh, _mockLogger.Object);
        var key = new ECDsaSecurityKey(_ecdsaP256) { KeyId = "test-key" };

        // Act
        var result = validator.ValidateKeyCompliance(key, "ES384");

        // Assert
        result.IsCompliant.Should().BeFalse();
        result.Violations.Should().ContainSingle(v => v.Type == HaipViolationType.WeakKeyStrength);
    }

    [Fact]
    public void ValidateKeyCompliance_Level2_WithP384Key_ShouldBeCompliant()
    {
        // Arrange
        var validator = new HaipCryptoValidator(HaipLevel.Level2_VeryHigh, _mockLogger.Object);
        var key = new ECDsaSecurityKey(_ecdsaP384) { KeyId = "test-key" };

        // Act
        var result = validator.ValidateKeyCompliance(key, "ES384");

        // Assert
        result.IsCompliant.Should().BeTrue();
        result.Violations.Should().BeEmpty();
    }

    [Fact]
    public void ValidateKeyCompliance_Level3_WithoutHSM_ShouldNotBeCompliant()
    {
        // Arrange
        var validator = new HaipCryptoValidator(HaipLevel.Level3_Sovereign, _mockLogger.Object);
        var key = new ECDsaSecurityKey(_ecdsaP521) { KeyId = "test-key" };

        // Act
        var result = validator.ValidateKeyCompliance(key, "ES512");

        // Assert
        result.IsCompliant.Should().BeFalse();
        result.Violations.Should().ContainSingle(v => v.Type == HaipViolationType.InsufficientAssuranceLevel);
        result.Violations.First().Description.Should().Contain("Hardware Security Module");
    }

    [Fact]
    public void ValidateKeyCompliance_WithForbiddenAlgorithm_ShouldNotBeCompliant()
    {
        // Arrange
        var validator = new HaipCryptoValidator(HaipLevel.Level1_High, _mockLogger.Object);
        var key = new ECDsaSecurityKey(_ecdsaP256) { KeyId = "test-key" };

        // Act
        var result = validator.ValidateKeyCompliance(key, "RS256");

        // Assert
        result.IsCompliant.Should().BeFalse();
        result.Violations.Should().ContainSingle(v => v.Type == HaipViolationType.WeakCryptography);
        result.Violations.First().Description.Should().Contain("RS256");
        result.Violations.First().RecommendedAction.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void ValidateKeyCompliance_WithRSAKey_ShouldValidateKeySize()
    {
        // Arrange
        var validator = new HaipCryptoValidator(HaipLevel.Level1_High, _mockLogger.Object);
        var key = new RsaSecurityKey(_rsa2048) { KeyId = "test-key" };

        // Act
        var result = validator.ValidateKeyCompliance(key, "PS256");

        // Assert
        result.IsCompliant.Should().BeTrue();
        result.AuditTrail.Steps.Should().Contain(s => s.Operation.Contains("Key strength validation"));
    }

    [Fact]
    public void ValidateKeyCompliance_WithWeakRSAKey_ShouldNotBeCompliant()
    {
        // Arrange
        var validator = new HaipCryptoValidator(HaipLevel.Level2_VeryHigh, _mockLogger.Object);
        var key = new RsaSecurityKey(_rsa2048) { KeyId = "test-key" }; // Too weak for Level 2

        // Act
        var result = validator.ValidateKeyCompliance(key, "PS384");

        // Assert
        result.IsCompliant.Should().BeFalse();
        result.Violations.Should().ContainSingle(v => v.Type == HaipViolationType.WeakKeyStrength);
    }

    [Fact]
    public void ValidateKeyCompliance_WithSymmetricKey_ShouldNotBeCompliant()
    {
        // Arrange
        var validator = new HaipCryptoValidator(HaipLevel.Level1_High, _mockLogger.Object);
        var key = new SymmetricSecurityKey(new byte[32]) { KeyId = "test-key" };

        // Act
        var result = validator.ValidateKeyCompliance(key, "HS256");

        // Assert
        result.IsCompliant.Should().BeFalse();
        result.Violations.Should().ContainSingle(v => v.Type == HaipViolationType.WeakKeyStrength);
        result.Violations.First().Description.Should().Contain("Symmetric keys are not allowed");
    }

    [Fact]
    public void ValidateJwtHeader_WithValidHeader_ShouldBeCompliant()
    {
        // Arrange
        var validator = new HaipCryptoValidator(HaipLevel.Level1_High, _mockLogger.Object);
        var key = new ECDsaSecurityKey(_ecdsaP256) { KeyId = "test-key" };
        var header = new JwtHeader(new SigningCredentials(key, SecurityAlgorithms.EcdsaSha256))
        {
            ["kid"] = "test-key"
        };

        // Act
        var result = validator.ValidateJwtHeader(header);

        // Assert
        result.IsCompliant.Should().BeTrue();
    }

    [Fact]
    public void ValidateJwtHeader_WithMissingAlgorithm_ShouldNotBeCompliant()
    {
        // Arrange
        var validator = new HaipCryptoValidator(HaipLevel.Level1_High, _mockLogger.Object);
        var header = new JwtHeader();

        // Act
        var result = validator.ValidateJwtHeader(header);

        // Assert
        result.IsCompliant.Should().BeFalse();
        result.Violations.Should().ContainSingle(v => v.Description.Contains("missing algorithm"));
    }

    [Fact]
    public void ValidateJwtHeader_WithMissingKeyId_ShouldHaveWarning()
    {
        // Arrange
        var validator = new HaipCryptoValidator(HaipLevel.Level1_High, _mockLogger.Object);
        var key = new ECDsaSecurityKey(_ecdsaP256);
        var header = new JwtHeader(new SigningCredentials(key, SecurityAlgorithms.EcdsaSha256));

        // Act
        var result = validator.ValidateJwtHeader(header);

        // Assert
        result.IsCompliant.Should().BeTrue(); // Warnings don't affect compliance
        result.Violations.Should().ContainSingle(v => v.Severity == HaipSeverity.Warning);
        result.Violations.First().Description.Should().Contain("Key ID (kid) should be present");
    }

    [Fact]
    public void ValidateKeyCompliance_ShouldCreateDetailedAuditTrail()
    {
        // Arrange
        var validator = new HaipCryptoValidator(HaipLevel.Level1_High, _mockLogger.Object);
        var key = new ECDsaSecurityKey(_ecdsaP256) { KeyId = "test-key" };

        // Act
        var result = validator.ValidateKeyCompliance(key, "ES256");

        // Assert
        result.AuditTrail.Should().NotBeNull();
        result.AuditTrail.ValidatorId.Should().Be(nameof(HaipCryptoValidator));
        result.AuditTrail.StartedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
        result.AuditTrail.CompletedAt.Should().NotBeNull();
        result.AuditTrail.Steps.Should().NotBeEmpty();
        
        // Should have steps for algorithm and key strength validation
        result.AuditTrail.Steps.Should().Contain(s => s.Operation.Contains("Algorithm validation"));
        result.AuditTrail.Steps.Should().Contain(s => s.Operation.Contains("Key strength validation"));
    }

    [Fact]
    public void ValidateKeyCompliance_WithException_ShouldHandleGracefully()
    {
        // Arrange
        var validator = new HaipCryptoValidator(HaipLevel.Level1_High, _mockLogger.Object);
        
        // Create a mock key that will throw during validation
        var mockKey = new Mock<SecurityKey>();
        mockKey.Setup(k => k.GetType()).Throws(new InvalidOperationException("Test exception"));

        // Act
        var result = validator.ValidateKeyCompliance(mockKey.Object, "ES256");

        // Assert
        result.IsCompliant.Should().BeFalse();
        result.Violations.Should().ContainSingle(v => v.Description.Contains("Validation error"));
        result.AuditTrail.CompletedAt.Should().NotBeNull();
    }

    [Theory]
    [InlineData(HaipLevel.Level1_High)]
    [InlineData(HaipLevel.Level2_VeryHigh)]
    [InlineData(HaipLevel.Level3_Sovereign)]
    public void Constructor_WithValidLevel_ShouldSucceed(HaipLevel level)
    {
        // Act & Assert
        var validator = new HaipCryptoValidator(level, _mockLogger.Object);
        validator.Should().NotBeNull();
    }
}
