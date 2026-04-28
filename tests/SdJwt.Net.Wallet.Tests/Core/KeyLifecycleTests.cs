using FluentAssertions;
using SdJwt.Net.Wallet.Core;
using Xunit;

namespace SdJwt.Net.Wallet.Tests.Core;

/// <summary>
/// Unit tests for <see cref="IKeyLifecycleManager"/> contract and supporting models.
/// </summary>
public class KeyLifecycleTests
{
    [Fact]
    public void KeyRotationOptions_DefaultValues_DecommissionIsFalse()
    {
        // Act
        var options = new KeyRotationOptions();

        // Assert
        options.DecommissionOldKey.Should().BeFalse();
        options.RequireHsmBacking.Should().BeFalse();
        options.NewAlgorithm.Should().BeNull();
        options.Metadata.Should().BeNull();
    }

    [Fact]
    public void KeyRotationOptions_SetProperties_ReturnsCorrectValues()
    {
        // Arrange & Act
        var metadata = new Dictionary<string, object> { ["purpose"] = "rotation" };
        var options = new KeyRotationOptions
        {
            NewAlgorithm = "ES384",
            DecommissionOldKey = true,
            RequireHsmBacking = true,
            Metadata = metadata
        };

        // Assert
        options.NewAlgorithm.Should().Be("ES384");
        options.DecommissionOldKey.Should().BeTrue();
        options.RequireHsmBacking.Should().BeTrue();
        options.Metadata.Should().ContainKey("purpose");
    }

    [Fact]
    public void KeyFilter_DefaultValues_OnlyActiveIsTrue()
    {
        // Act
        var filter = new KeyFilter();

        // Assert
        filter.OnlyActive.Should().BeTrue();
        filter.Algorithm.Should().BeNull();
        filter.KeyType.Should().BeNull();
        filter.IsHardwareBacked.Should().BeNull();
    }

    [Fact]
    public void KeyFilter_SetProperties_ReturnsCorrectValues()
    {
        // Arrange & Act
        var filter = new KeyFilter
        {
            Algorithm = "ES256",
            KeyType = "EC",
            IsHardwareBacked = true,
            OnlyActive = false
        };

        // Assert
        filter.Algorithm.Should().Be("ES256");
        filter.KeyType.Should().Be("EC");
        filter.IsHardwareBacked.Should().BeTrue();
        filter.OnlyActive.Should().BeFalse();
    }

    [Fact]
    public void IKeyLifecycleManager_ExtendsIKeyManager()
    {
        // Assert
        typeof(IKeyLifecycleManager).Should().Implement<IKeyManager>();
    }
}
