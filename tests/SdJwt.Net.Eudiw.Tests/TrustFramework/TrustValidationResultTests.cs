using FluentAssertions;
using SdJwt.Net.Eudiw.TrustFramework;
using Xunit;

namespace SdJwt.Net.Eudiw.Tests.TrustFramework;

/// <summary>
/// Tests for TrustValidationResult model.
/// </summary>
public class TrustValidationResultTests
{
    [Fact]
    public void Trusted_CreatesValidTrustedResult()
    {
        // Arrange
        var provider = new TrustedServiceProvider
        {
            Name = "Test Provider",
            ServiceType = TrustServiceType.PidProvider,
            Status = "granted"
        };

        // Act
        var result = TrustValidationResult.Trusted(provider, "DE");

        // Assert
        result.IsTrusted.Should().BeTrue();
        result.Reason.Should().BeNull();
        result.IssuerInfo.Should().NotBeNull();
        result.IssuerInfo!.Name.Should().Be("Test Provider");
        result.MemberState.Should().Be("DE");
    }

    [Fact]
    public void Untrusted_CreatesInvalidResult()
    {
        // Act
        var result = TrustValidationResult.Untrusted("Issuer not found in trust list");

        // Assert
        result.IsTrusted.Should().BeFalse();
        result.Reason.Should().Be("Issuer not found in trust list");
        result.IssuerInfo.Should().BeNull();
        result.MemberState.Should().BeNull();
    }

    [Fact]
    public void Untrusted_WithEmptyReason_StillCreatesInvalidResult()
    {
        // Act
        var result = TrustValidationResult.Untrusted("");

        // Assert
        result.IsTrusted.Should().BeFalse();
    }

    [Fact]
    public void IsTrusted_DefaultsToFalse()
    {
        // Arrange & Act
        var result = new TrustValidationResult();

        // Assert
        result.IsTrusted.Should().BeFalse();
    }
}
