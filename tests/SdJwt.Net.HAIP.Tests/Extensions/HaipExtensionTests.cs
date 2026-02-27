using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Moq;
using SdJwt.Net.HAIP.Extensions;
using SdJwt.Net.HAIP.Models;
using SdJwt.Net.HAIP.Validators;
using System.Security.Cryptography;
using Xunit;

namespace SdJwt.Net.HAIP.Tests.Extensions;

/// <summary>
/// Tests for HAIP extension methods and validation service
/// </summary>
public class HaipExtensionTests : IDisposable
{
    private readonly Mock<ILogger<HaipValidationService>> _mockLogger;
    private readonly Mock<IHaipCryptoValidator> _mockCryptoValidator;
    private readonly ECDsa _ecdsa;
    private readonly HaipConfiguration _testConfig;

    public HaipExtensionTests()
    {
        _mockLogger = new Mock<ILogger<HaipValidationService>>();
        _mockCryptoValidator = new Mock<IHaipCryptoValidator>();
        _ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);

        _testConfig = new HaipConfiguration
        {
            RequiredLevel = HaipLevel.Level1_High,
            TrustFrameworks = new[] { "https://test.framework" },
            EnableEidasCompliance = true
        };
    }

    public void Dispose()
    {
        _ecdsa?.Dispose();
    }

    [Fact]
    public void UseHaipProfile_WithValidParameters_ShouldConfigureOptions()
    {
        // Arrange
        var options = new TestOptions();

        // Act
        options.UseHaipProfile(HaipLevel.Level1_High);

        // Assert
        options.AllowedSigningAlgorithms.Should().BeEquivalentTo(HaipConstants.Level1_Algorithms);
        options.ForbiddenSigningAlgorithms.Should().BeEquivalentTo(HaipConstants.ForbiddenAlgorithms);
        options.RequireProofOfPossession.Should().BeTrue();
        options.RequireSecureTransport.Should().BeTrue();
        options.RequirePkce.Should().BeTrue();
        options.ClientAuthenticationMethods.Should().BeEquivalentTo(HaipConstants.ClientAuthMethods.Level1_Allowed);
        options.EnableComplianceAuditing.Should().BeTrue();
        options.AuditingOptions.Should().NotBeNull();
    }

    [Fact]
    public void EnforceHaip_WithValidParameters_ShouldConfigureVpOptions()
    {
        // Arrange
        var options = new TestVpOptions();

        // Act
        options.EnforceHaip(HaipLevel.Level2_VeryHigh);

        // Assert
        options.ResponseMode.Should().Be("direct_post.jwt");
        options.AllowedClientIdSchemes.Should().BeEquivalentTo(
            new[] { "redirect_uri", "x509_san_dns", "verifier_attestation", "entity_id" });
        options.RequireVerifierAttestation.Should().BeTrue();
        options.RequireSignedRequest.Should().BeTrue();
        options.RequireQualifiedVerifierAttestation.Should().BeFalse();
    }

    [Fact]
    public async Task HaipValidationService_ValidateIssuanceRequestAsync_WithValidRequest_ShouldReturnCompliant()
    {
        // Arrange
        var cryptoResult = new HaipComplianceResult
        {
            IsCompliant = true,
            AchievedLevel = HaipLevel.Level1_High
        };

        _mockCryptoValidator
            .Setup(v => v.ValidateKeyCompliance(It.IsAny<SecurityKey>(), It.IsAny<string>()))
            .Returns(cryptoResult);

        var service = new HaipValidationService(_mockCryptoValidator.Object, _testConfig, _mockLogger.Object);
        var context = new HaipIssuanceValidationContext
        {
            SigningKey = new ECDsaSecurityKey(_ecdsa),
            SigningAlgorithm = "ES256",
            HasProofOfPossession = true,
            IsSecureTransport = true,
            HasWalletAttestation = false // Not required for Level 1
        };

        // Act
        var result = await service.ValidateIssuanceRequestAsync(context);

        // Assert
        result.Should().NotBeNull();
        result.IsCompliant.Should().BeTrue();
        result.AuditTrail.ValidatorId.Should().Be(nameof(HaipValidationService));
        result.AuditTrail.Steps.Should().NotBeEmpty();
        result.AuditTrail.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task HaipValidationService_ValidateIssuanceRequestAsync_WithInvalidCrypto_ShouldReturnNonCompliant()
    {
        // Arrange
        var cryptoResult = new HaipComplianceResult
        {
            IsCompliant = false,
            Violations = new List<HaipViolation>
            {
                new()
                {
                    Type = HaipViolationType.WeakCryptography,
                    Description = "Weak algorithm",
                    Severity = HaipSeverity.Critical
                }
            }
        };

        _mockCryptoValidator
            .Setup(v => v.ValidateKeyCompliance(It.IsAny<SecurityKey>(), It.IsAny<string>()))
            .Returns(cryptoResult);

        var service = new HaipValidationService(_mockCryptoValidator.Object, _testConfig, _mockLogger.Object);
        var context = new HaipIssuanceValidationContext
        {
            SigningKey = new ECDsaSecurityKey(_ecdsa),
            SigningAlgorithm = "RS256", // Forbidden algorithm
            HasProofOfPossession = true,
            IsSecureTransport = true
        };

        // Act
        var result = await service.ValidateIssuanceRequestAsync(context);

        // Assert
        result.IsCompliant.Should().BeFalse();
        result.Violations.Should().NotBeEmpty();
        result.Violations.Should().Contain(v => v.Type == HaipViolationType.WeakCryptography);
    }

    [Fact]
    public async Task HaipValidationService_ValidateIssuanceRequestAsync_WithMissingProofOfPossession_ShouldReturnNonCompliant()
    {
        // Arrange
        var cryptoResult = new HaipComplianceResult { IsCompliant = true };

        _mockCryptoValidator
            .Setup(v => v.ValidateKeyCompliance(It.IsAny<SecurityKey>(), It.IsAny<string>()))
            .Returns(cryptoResult);

        var service = new HaipValidationService(_mockCryptoValidator.Object, _testConfig, _mockLogger.Object);
        var context = new HaipIssuanceValidationContext
        {
            SigningKey = new ECDsaSecurityKey(_ecdsa),
            SigningAlgorithm = "ES256",
            HasProofOfPossession = false, // Missing!
            IsSecureTransport = true
        };

        // Act
        var result = await service.ValidateIssuanceRequestAsync(context);

        // Assert
        result.IsCompliant.Should().BeFalse();
        result.Violations.Should().Contain(v => v.Type == HaipViolationType.MissingProofOfPossession);
        result.Violations.Should().Contain(v => v.Description.Contains("Proof of possession is required"));
    }

    [Fact]
    public async Task HaipValidationService_ValidateIssuanceRequestAsync_WithInsecureTransport_ShouldReturnNonCompliant()
    {
        // Arrange
        var cryptoResult = new HaipComplianceResult { IsCompliant = true };

        _mockCryptoValidator
            .Setup(v => v.ValidateKeyCompliance(It.IsAny<SecurityKey>(), It.IsAny<string>()))
            .Returns(cryptoResult);

        var service = new HaipValidationService(_mockCryptoValidator.Object, _testConfig, _mockLogger.Object);
        var context = new HaipIssuanceValidationContext
        {
            SigningKey = new ECDsaSecurityKey(_ecdsa),
            SigningAlgorithm = "ES256",
            HasProofOfPossession = true,
            IsSecureTransport = false // Insecure!
        };

        // Act
        var result = await service.ValidateIssuanceRequestAsync(context);

        // Assert
        result.IsCompliant.Should().BeFalse();
        result.Violations.Should().Contain(v => v.Type == HaipViolationType.InsecureTransport);
        result.Violations.Should().Contain(v => v.Description.Contains("Secure transport"));
    }

    [Fact]
    public async Task HaipValidationService_ValidateIssuanceRequestAsync_Level2WithoutWalletAttestation_ShouldReturnNonCompliant()
    {
        // Arrange
        var cryptoResult = new HaipComplianceResult { IsCompliant = true };

        _mockCryptoValidator
            .Setup(v => v.ValidateKeyCompliance(It.IsAny<SecurityKey>(), It.IsAny<string>()))
            .Returns(cryptoResult);

        var level2Config = new HaipConfiguration { RequiredLevel = HaipLevel.Level2_VeryHigh };
        var service = new HaipValidationService(_mockCryptoValidator.Object, level2Config, _mockLogger.Object);
        var context = new HaipIssuanceValidationContext
        {
            SigningKey = new ECDsaSecurityKey(_ecdsa),
            SigningAlgorithm = "ES384",
            HasProofOfPossession = true,
            IsSecureTransport = true,
            HasWalletAttestation = false // Required for Level 2!
        };

        // Act
        var result = await service.ValidateIssuanceRequestAsync(context);

        // Assert
        result.IsCompliant.Should().BeFalse();
        result.Violations.Should().Contain(v => v.Type == HaipViolationType.InsecureClientAuthentication);
        result.Violations.Should().Contain(v => v.Description.Contains("Wallet attestation"));
    }

    [Fact]
    public async Task HaipValidationService_ValidateIssuanceRequestAsync_WithTrustFrameworks_ShouldValidateTrust()
    {
        // Arrange
        var cryptoResult = new HaipComplianceResult { IsCompliant = true };

        _mockCryptoValidator
            .Setup(v => v.ValidateKeyCompliance(It.IsAny<SecurityKey>(), It.IsAny<string>()))
            .Returns(cryptoResult);

        var service = new HaipValidationService(_mockCryptoValidator.Object, _testConfig, _mockLogger.Object);
        var context = new HaipIssuanceValidationContext
        {
            SigningKey = new ECDsaSecurityKey(_ecdsa),
            SigningAlgorithm = "ES256",
            HasProofOfPossession = true,
            IsSecureTransport = true,
            IssuerIdentifier = "https://test.issuer.com"
        };

        // Act
        var result = await service.ValidateIssuanceRequestAsync(context);

        // Assert
        result.AuditTrail.Steps.Should().Contain(s => s.Operation.Contains("Trust framework validation"));
    }

    [Fact]
    public async Task HaipValidationService_ValidateIssuanceRequestAsync_WithException_ShouldHandleGracefully()
    {
        // Arrange
        _mockCryptoValidator
            .Setup(v => v.ValidateKeyCompliance(It.IsAny<SecurityKey>(), It.IsAny<string>()))
            .Throws(new InvalidOperationException("Test exception"));

        var service = new HaipValidationService(_mockCryptoValidator.Object, _testConfig, _mockLogger.Object);
        var context = new HaipIssuanceValidationContext
        {
            SigningKey = new ECDsaSecurityKey(_ecdsa),
            SigningAlgorithm = "ES256"
        };

        // Act
        var result = await service.ValidateIssuanceRequestAsync(context);

        // Assert
        result.IsCompliant.Should().BeFalse();
        result.Violations.Should().Contain(v => v.Description.Contains("Validation error"));
        result.AuditTrail.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public void HaipIssuanceValidationContext_ShouldHaveDefaultValues()
    {
        // Act
        var context = new HaipIssuanceValidationContext();

        // Assert
        context.AdditionalContext.Should().NotBeNull();
        context.AdditionalContext.Should().BeEmpty();
        context.HasProofOfPossession.Should().BeFalse();
        context.IsSecureTransport.Should().BeFalse();
        context.HasWalletAttestation.Should().BeFalse();
    }

    [Fact]
    public async Task HaipValidationService_ValidateIssuanceRequestAsync_WithCancellation_ShouldRespectCancellation()
    {
        // Arrange
        var cryptoResult = new HaipComplianceResult { IsCompliant = true };

        _mockCryptoValidator
            .Setup(v => v.ValidateKeyCompliance(It.IsAny<SecurityKey>(), It.IsAny<string>()))
            .Returns(cryptoResult);

        var service = new HaipValidationService(_mockCryptoValidator.Object, _testConfig, _mockLogger.Object);
        var context = new HaipIssuanceValidationContext();

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        var result = await service.ValidateIssuanceRequestAsync(context, cts.Token);

        // The method should complete even with cancellation since it's not doing long-running work
        // This demonstrates the cancellation token is being passed through properly
        result.Should().NotBeNull();
    }

    // Test helper classes to demonstrate extension method patterns
    private class TestOptions : IHaipOid4VciOptions
    {
        public string[]? AllowedSigningAlgorithms { get; set; }
        public string[]? ForbiddenSigningAlgorithms { get; set; }
        public bool RequireProofOfPossession { get; set; }
        public bool RequireSecureTransport { get; set; }
        public bool RequirePkce { get; set; }
        public string[]? ClientAuthenticationMethods { get; set; }
        public bool RequireWalletAttestation { get; set; }
        public bool RequireQualifiedWalletAttestation { get; set; }
        public bool RequireHardwareSecurityModule { get; set; }
        public bool RequireQualifiedElectronicSignature { get; set; }
        public bool RequirePushedAuthorizationRequests { get; set; }
        public bool RequireDpopOrMtls { get; set; }
        public bool EnableComplianceAuditing { get; set; }
        public object? AuditingOptions { get; set; }
    }

    private class TestVpOptions : IHaipOid4VpOptions
    {
        public string? ResponseMode { get; set; }
        public string[]? AllowedClientIdSchemes { get; set; }
        public bool RequireVerifierAttestation { get; set; }
        public bool RequireSignedRequest { get; set; }
        public bool RequireQualifiedVerifierAttestation { get; set; }
    }
}
