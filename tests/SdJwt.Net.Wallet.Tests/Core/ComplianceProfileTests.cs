using FluentAssertions;
using SdJwt.Net.Wallet.Core;
using Xunit;

namespace SdJwt.Net.Wallet.Tests.Core;

/// <summary>
/// Unit tests for <see cref="IComplianceProfile"/> contract.
/// </summary>
public class ComplianceProfileTests
{
    [Fact]
    public void IComplianceProfile_HasRequiredMembers()
    {
        // Assert interface shape
        var profileType = typeof(IComplianceProfile);
        profileType.GetProperty("ProfileName").Should().NotBeNull();
        profileType.GetMethod("IsAlgorithmAllowed").Should().NotBeNull();
        profileType.GetMethod("IsKeyAllowed").Should().NotBeNull();
    }

    [Fact]
    public void WalletOptions_ComplianceProfile_DefaultsToNull()
    {
        // Act
        var options = new WalletOptions();

        // Assert
        options.ComplianceProfile.Should().BeNull();
    }

    [Fact]
    public void WalletOptions_ComplianceProfile_CanBeSet()
    {
        // Arrange
        var profile = new TestComplianceProfile("HAIP Level 2");

        // Act
        var options = new WalletOptions
        {
            ComplianceProfile = profile
        };

        // Assert
        options.ComplianceProfile.Should().NotBeNull();
        options.ComplianceProfile!.ProfileName.Should().Be("HAIP Level 2");
    }

    [Fact]
    public void TestComplianceProfile_IsAlgorithmAllowed_ReturnsTrueForES256()
    {
        // Arrange
        var profile = new TestComplianceProfile("test");

        // Act & Assert
        profile.IsAlgorithmAllowed("ES256").Should().BeTrue();
        profile.IsAlgorithmAllowed("RS256").Should().BeFalse();
    }

    [Fact]
    public void TestComplianceProfile_IsKeyAllowed_ReturnsTrueForEcP256()
    {
        // Arrange
        var profile = new TestComplianceProfile("test");

        // Act & Assert
        profile.IsKeyAllowed("EC", "P-256").Should().BeTrue();
        profile.IsKeyAllowed("RSA", null).Should().BeFalse();
    }

    /// <summary>
    /// Test implementation of <see cref="IComplianceProfile"/> that allows only ES256/P-256.
    /// </summary>
    private sealed class TestComplianceProfile : IComplianceProfile
    {
        public TestComplianceProfile(string profileName)
        {
            ProfileName = profileName;
        }

        public string ProfileName
        {
            get;
        }

        public bool IsAlgorithmAllowed(string algorithm)
        {
            return string.Equals(algorithm, "ES256", StringComparison.OrdinalIgnoreCase);
        }

        public bool IsKeyAllowed(string keyType, string? curve)
        {
            return string.Equals(keyType, "EC", StringComparison.OrdinalIgnoreCase)
                && string.Equals(curve, "P-256", StringComparison.OrdinalIgnoreCase);
        }
    }
}
