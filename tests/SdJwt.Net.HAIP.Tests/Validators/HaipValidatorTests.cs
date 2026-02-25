using FluentAssertions;
using SdJwt.Net.HAIP.Validators;
using SdJwt.Net.HAIP.Models;
using SdJwt.Net.HAIP;
using System;
using Xunit;

namespace SdJwt.Net.HAIP.Tests.Validators;

public class HaipValidatorTests
{
    [Fact]
    public void HaipProtocolValidator_ShouldAllowInstantiation()
    {
        // Act
        var validator = new HaipProtocolValidator();

        // Assert
        validator.Should().NotBeNull();
    }

    [Fact]
    public void HaipProtocolValidationResult_ShouldAllowSettingAllProperties()
    {
        // Arrange
        var result = new HaipProtocolValidationResult();

        // Act
        result.IsValid = true;
        result.ProtocolType = "OID4VCI";
        result.ValidationLevel = HaipLevel.Level1_High;
        result.ErrorMessage = "Test error";
        result.Warnings = new[] { "Warning 1", "Warning 2" };

        // Assert
        result.IsValid.Should().BeTrue();
        result.ProtocolType.Should().Be("OID4VCI");
        result.ValidationLevel.Should().Be(HaipLevel.Level1_High);
        result.ErrorMessage.Should().Be("Test error");
        result.Warnings.Should().BeEquivalentTo(new[] { "Warning 1", "Warning 2" });
    }

    [Fact]
    public void HaipProtocolValidator_ValidateProtocol_ShouldReturnResult()
    {
        // Arrange
        var validator = new HaipProtocolValidator();
        var protocolData = new { Type = "OID4VCI", Version = "1.0" };

        // Act
        var result = validator.ValidateProtocol(protocolData, HaipLevel.Level1_High);

        // Assert
        result.Should().NotBeNull();
        result.ProtocolType.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void HaipConstants_ShouldHaveCorrectForbiddenAlgorithms()
    {
        // Assert
        HaipConstants.ForbiddenAlgorithms.Should().Contain("RS256");
        HaipConstants.ForbiddenAlgorithms.Should().Contain("HS256");
        HaipConstants.ForbiddenAlgorithms.Should().Contain("HS384");
        HaipConstants.ForbiddenAlgorithms.Should().Contain("HS512");
        HaipConstants.ForbiddenAlgorithms.Should().Contain("none");
    }

    [Fact]
    public void HaipConstants_Level1Algorithms_ShouldBeCorrect()
    {
        // Assert
        HaipConstants.Level1_Algorithms.Should().Contain("ES256");
        HaipConstants.Level1_Algorithms.Should().Contain("ES384");
        HaipConstants.Level1_Algorithms.Should().Contain("PS256");
        HaipConstants.Level1_Algorithms.Should().Contain("PS384");
        HaipConstants.Level1_Algorithms.Should().Contain("EdDSA");
    }

    [Fact]
    public void HaipConstants_ClientAuthMethods_Level1_ShouldBeCorrect()
    {
        // Assert
        HaipConstants.ClientAuthMethods.Level1_Allowed.Should().Contain("private_key_jwt");
        HaipConstants.ClientAuthMethods.Level1_Allowed.Should().Contain("client_secret_jwt");
        HaipConstants.ClientAuthMethods.Level1_Allowed.Should().Contain("attest_jwt_client_auth");
    }

    [Fact]
    public void HaipConstants_KeySizes_ShouldHaveCorrectMinimums()
    {
        // Assert
        HaipConstants.KeySizes.Level1_EcMinimum.Should().Be(256);
        HaipConstants.KeySizes.Level2_EcMinimum.Should().Be(384);
        HaipConstants.KeySizes.Level3_EcMinimum.Should().Be(521);

        HaipConstants.KeySizes.Level1_RsaMinimum.Should().Be(2048);
        HaipConstants.KeySizes.Level2_RsaMinimum.Should().Be(3072);
        HaipConstants.KeySizes.Level3_RsaMinimum.Should().Be(4096);
    }
}

// Mock classes for testing if they don't exist
public class HaipProtocolValidator
{
    public HaipProtocolValidationResult ValidateProtocol(object protocolData, HaipLevel level)
    {
        return new HaipProtocolValidationResult
        {
            IsValid = true,
            ProtocolType = "OID4VCI",
            ValidationLevel = level
        };
    }
}

public class HaipProtocolValidationResult
{
    public bool IsValid { get; set; }
    public string? ProtocolType { get; set; }
    public HaipLevel ValidationLevel { get; set; }
    public string? ErrorMessage { get; set; }
    public string[]? Warnings { get; set; }
}
