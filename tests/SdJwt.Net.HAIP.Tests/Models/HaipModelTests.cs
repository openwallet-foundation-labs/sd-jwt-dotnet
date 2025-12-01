using FluentAssertions;
using SdJwt.Net.HAIP.Models;
using Xunit;

namespace SdJwt.Net.HAIP.Tests.Models;

/// <summary>
/// Tests for HAIP model classes
/// </summary>
public class HaipModelTests
{
    [Fact]
    public void HaipViolation_ShouldSetTimestampOnCreation()
    {
        // Act
        var violation = new HaipViolation
        {
            Type = HaipViolationType.WeakCryptography,
            Description = "Test violation",
            Severity = HaipSeverity.Critical
        };

        // Assert
        violation.DetectedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void HaipComplianceResult_AddViolation_ShouldAddViolationAndUpdateCompliance()
    {
        // Arrange
        var result = new HaipComplianceResult { IsCompliant = true };

        // Act
        result.AddViolation(
            "Test critical violation",
            HaipViolationType.WeakCryptography,
            HaipSeverity.Critical,
            "Fix this issue"
        );

        // Assert
        result.IsCompliant.Should().BeFalse();
        result.Violations.Should().HaveCount(1);
        
        var violation = result.Violations.First();
        violation.Description.Should().Be("Test critical violation");
        violation.Type.Should().Be(HaipViolationType.WeakCryptography);
        violation.Severity.Should().Be(HaipSeverity.Critical);
        violation.RecommendedAction.Should().Be("Fix this issue");
    }

    [Fact]
    public void HaipComplianceResult_AddViolation_WithWarning_ShouldNotAffectCompliance()
    {
        // Arrange
        var result = new HaipComplianceResult { IsCompliant = true };

        // Act
        result.AddViolation(
            "Test warning",
            HaipViolationType.WeakCryptography,
            HaipSeverity.Warning
        );

        // Assert
        result.IsCompliant.Should().BeTrue(); // Warnings don't affect compliance
        result.Violations.Should().HaveCount(1);
        result.Violations.First().Severity.Should().Be(HaipSeverity.Warning);
    }

    [Fact]
    public void HaipComplianceResult_AddViolation_WithoutRecommendedAction_ShouldUseDefault()
    {
        // Arrange
        var result = new HaipComplianceResult();

        // Act
        result.AddViolation(
            "Missing proof of possession",
            HaipViolationType.MissingProofOfPossession,
            HaipSeverity.Critical
        );

        // Assert
        result.Violations.Should().HaveCount(1);
        result.Violations.First().RecommendedAction.Should().Be("Enable proof of possession requirement");
    }

    [Theory]
    [InlineData(HaipViolationType.WeakCryptography, "Use ES256, ES384, ES512, PS256, PS384, PS512, or EdDSA")]
    [InlineData(HaipViolationType.MissingProofOfPossession, "Enable proof of possession requirement")]
    [InlineData(HaipViolationType.InsecureClientAuthentication, "Use attest_jwt_client_auth or private_key_jwt")]
    [InlineData(HaipViolationType.UntrustedIssuer, "Ensure issuer is part of trusted federation")]
    [InlineData(HaipViolationType.ExpiredCertificate, "Renew expired certificates")]
    [InlineData(HaipViolationType.InsufficientAssuranceLevel, "Upgrade to higher assurance level")]
    [InlineData(HaipViolationType.InsecureTransport, "Use HTTPS with TLS 1.2 or higher")]
    [InlineData(HaipViolationType.WeakKeyStrength, "Use stronger cryptographic keys")]
    public void HaipComplianceResult_AddViolation_ShouldProvideCorrectDefaultRecommendations(
        HaipViolationType violationType, string expectedRecommendation)
    {
        // Arrange
        var result = new HaipComplianceResult();

        // Act
        result.AddViolation("Test violation", violationType);

        // Assert
        result.Violations.Should().HaveCount(1);
        result.Violations.First().RecommendedAction.Should().Be(expectedRecommendation);
    }

    [Fact]
    public void HaipAuditTrail_AddStep_ShouldAddStepWithTimestamp()
    {
        // Arrange
        var auditTrail = new HaipAuditTrail();

        // Act
        auditTrail.AddStep("Test operation", true, "Test details");

        // Assert
        auditTrail.Steps.Should().HaveCount(1);
        
        var step = auditTrail.Steps.First();
        step.Operation.Should().Be("Test operation");
        step.Success.Should().BeTrue();
        step.Details.Should().Be("Test details");
        step.Timestamp.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void HaipAuditTrail_Complete_ShouldSetCompletedTimestamp()
    {
        // Arrange
        var auditTrail = new HaipAuditTrail();

        // Act
        auditTrail.Complete();

        // Assert
        auditTrail.CompletedAt.Should().NotBeNull();
        auditTrail.CompletedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void HaipAuditStep_ShouldSetTimestampOnCreation()
    {
        // Act
        var step = new HaipAuditStep
        {
            Operation = "Test operation",
            Success = true
        };

        // Assert
        step.Timestamp.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData(HaipLevel.Level1_High)]
    [InlineData(HaipLevel.Level2_VeryHigh)]
    [InlineData(HaipLevel.Level3_Sovereign)]
    public void HaipConfiguration_GetDefault_ShouldReturnValidConfiguration(HaipLevel level)
    {
        // Act
        var config = HaipConfiguration.GetDefault(level);

        // Assert
        config.Should().NotBeNull();
        config.RequiredLevel.Should().Be(level);
        config.AuditingOptions.Should().NotBeNull();
        config.ExtensionParameters.Should().NotBeNull();

        // Verify level-specific settings
        switch (level)
        {
            case HaipLevel.Level1_High:
                config.EnableEidasCompliance.Should().BeTrue();
                config.TrustFrameworks.Should().Contain("https://trust.eudi.europa.eu");
                break;
                
            case HaipLevel.Level2_VeryHigh:
                config.EnableEidasCompliance.Should().BeTrue();
                config.AuditingOptions.DetailedLogging.Should().BeTrue();
                config.TrustFrameworks.Length.Should().BeGreaterThan(1);
                break;
                
            case HaipLevel.Level3_Sovereign:
                config.EnableSovereignCompliance.Should().BeTrue();
                config.AuditingOptions.DetailedLogging.Should().BeTrue();
                config.AuditingOptions.RequireDigitalSignature.Should().BeTrue();
                break;
        }
    }

    [Fact]
    public void HaipAuditingOptions_ShouldHaveDefaultValues()
    {
        // Act
        var options = new HaipAuditingOptions();

        // Assert
        options.DetailedLogging.Should().BeFalse();
        options.RequireDigitalSignature.Should().BeFalse();
        options.PersistentStorage.Should().BeFalse();
        options.CacheTimeout.Should().Be(TimeSpan.FromMinutes(30));
    }
}

/// <summary>
/// Tests for HAIP type enums and constants
/// </summary>
public class HaipTypesTests
{
    [Theory]
    [InlineData(HaipLevel.Level1_High)]
    [InlineData(HaipLevel.Level2_VeryHigh)]
    [InlineData(HaipLevel.Level3_Sovereign)]
    public void HaipLevel_ShouldHaveValidValues(HaipLevel level)
    {
        // Assert
        Enum.IsDefined(typeof(HaipLevel), level).Should().BeTrue();
    }

    [Theory]
    [InlineData(HaipSeverity.Info)]
    [InlineData(HaipSeverity.Warning)]
    [InlineData(HaipSeverity.Critical)]
    public void HaipSeverity_ShouldHaveValidValues(HaipSeverity severity)
    {
        // Assert
        Enum.IsDefined(typeof(HaipSeverity), severity).Should().BeTrue();
    }

    [Fact]
    public void HaipConstants_Level1_Algorithms_ShouldContainExpectedValues()
    {
        // Assert
        HaipConstants.Level1_Algorithms.Should().Contain(new[] { "ES256", "ES384", "PS256", "PS384", "EdDSA" });
        HaipConstants.Level1_Algorithms.Should().NotContain("RS256");
        HaipConstants.Level1_Algorithms.Should().NotContain("HS256");
    }

    [Fact]
    public void HaipConstants_Level2_Algorithms_ShouldBeMoreRestrictive()
    {
        // Assert
        HaipConstants.Level2_Algorithms.Should().Contain(new[] { "ES384", "ES512", "PS384", "PS512", "EdDSA" });
        HaipConstants.Level2_Algorithms.Should().NotContain("ES256"); // Not allowed in Level 2
    }

    [Fact]
    public void HaipConstants_Level3_Algorithms_ShouldBeMostRestrictive()
    {
        // Assert
        HaipConstants.Level3_Algorithms.Should().Contain(new[] { "ES512", "PS512", "EdDSA" });
        HaipConstants.Level3_Algorithms.Should().NotContain("ES256");
        HaipConstants.Level3_Algorithms.Should().NotContain("ES384");
        HaipConstants.Level3_Algorithms.Should().BeSubsetOf(HaipConstants.Level2_Algorithms);
    }

    [Fact]
    public void HaipConstants_ForbiddenAlgorithms_ShouldContainWeakAlgorithms()
    {
        // Assert
        HaipConstants.ForbiddenAlgorithms.Should().Contain(new[] { "RS256", "HS256", "HS384", "HS512", "none" });
    }

    [Fact]
    public void HaipConstants_ClientAuthMethods_ShouldHaveProgressive_Restrictions()
    {
        // Assert
        // Level 1 should be most permissive
        HaipConstants.ClientAuthMethods.Level1_Allowed.Should().Contain("private_key_jwt");
        HaipConstants.ClientAuthMethods.Level1_Allowed.Should().Contain("client_secret_jwt");
        HaipConstants.ClientAuthMethods.Level1_Allowed.Should().Contain("attest_jwt_client_auth");
        
        // Level 2 should be more restrictive
        HaipConstants.ClientAuthMethods.Level2_Required.Should().NotContain("client_secret_jwt");
        HaipConstants.ClientAuthMethods.Level2_Required.Should().Contain("attest_jwt_client_auth");
        
        // Level 3 should be most restrictive
        HaipConstants.ClientAuthMethods.Level3_Required.Should().HaveCount(1);
        HaipConstants.ClientAuthMethods.Level3_Required.Should().Contain("attest_jwt_client_auth");
    }

    [Theory]
    [InlineData(HaipConstants.KeySizes.Level1_EcMinimum, 256)]
    [InlineData(HaipConstants.KeySizes.Level2_EcMinimum, 384)]
    [InlineData(HaipConstants.KeySizes.Level3_EcMinimum, 521)]
    [InlineData(HaipConstants.KeySizes.Level1_RsaMinimum, 2048)]
    [InlineData(HaipConstants.KeySizes.Level2_RsaMinimum, 3072)]
    [InlineData(HaipConstants.KeySizes.Level3_RsaMinimum, 4096)]
    public void HaipConstants_KeySizes_ShouldHaveCorrectMinimums(int actualSize, int expectedSize)
    {
        // Assert
        actualSize.Should().Be(expectedSize);
    }

    [Fact]
    public void HaipConstants_KeySizes_ShouldBeProgressive()
    {
        // Assert - EC key sizes should increase with levels
        HaipConstants.KeySizes.Level2_EcMinimum.Should().BeGreaterThan(HaipConstants.KeySizes.Level1_EcMinimum);
        HaipConstants.KeySizes.Level3_EcMinimum.Should().BeGreaterThan(HaipConstants.KeySizes.Level2_EcMinimum);
        
        // Assert - RSA key sizes should increase with levels
        HaipConstants.KeySizes.Level2_RsaMinimum.Should().BeGreaterThan(HaipConstants.KeySizes.Level1_RsaMinimum);
        HaipConstants.KeySizes.Level3_RsaMinimum.Should().BeGreaterThan(HaipConstants.KeySizes.Level2_RsaMinimum);
    }
}
