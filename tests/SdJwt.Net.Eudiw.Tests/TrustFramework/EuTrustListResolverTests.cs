using FluentAssertions;
using SdJwt.Net.Eudiw.TrustFramework;
using Xunit;

namespace SdJwt.Net.Eudiw.Tests.TrustFramework;

/// <summary>
/// Tests for EU Trust List Resolver.
/// </summary>
public class EuTrustListResolverTests
{
    private readonly EuTrustListResolver _resolver;

    public EuTrustListResolverTests()
    {
        _resolver = new EuTrustListResolver();
    }

    #region LOTL URL Configuration

    [Fact]
    public void LotlUrl_ReturnsConfiguredUrl()
    {
        // Assert
        _resolver.LotlUrl.Should().Be(EudiwConstants.TrustList.LotlUrl);
    }

    [Fact]
    public void LotlJsonUrl_ReturnsConfiguredUrl()
    {
        // Assert
        _resolver.LotlJsonUrl.Should().Be(EudiwConstants.TrustList.LotlJsonUrl);
    }

    #endregion

    #region Member State Validation

    [Theory]
    [InlineData("DE", true)]
    [InlineData("FR", true)]
    [InlineData("XX", false)]
    [InlineData("US", false)]
    public void IsValidMemberState_ReturnsExpectedResult(string code, bool expected)
    {
        // Act
        var result = _resolver.IsValidMemberState(code);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void GetSupportedMemberStates_ReturnsAll27States()
    {
        // Act
        var states = _resolver.GetSupportedMemberStates();

        // Assert
        states.Should().HaveCount(27);
    }

    #endregion

    #region Trust List Pointer

    [Fact]
    public void TrustedListPointer_HasCorrectProperties()
    {
        // Arrange
        var pointer = new TrustedListPointer
        {
            Territory = "DE",
            TslLocation = "https://example.de/tl.xml"
        };

        // Assert
        pointer.Territory.Should().Be("DE");
        pointer.TslLocation.Should().Be("https://example.de/tl.xml");
    }

    [Fact]
    public void TrustedListPointer_DefaultsToEmptyStrings()
    {
        // Arrange & Act
        var pointer = new TrustedListPointer();

        // Assert
        pointer.Territory.Should().BeEmpty();
        pointer.TslLocation.Should().BeEmpty();
    }

    #endregion

    #region List of Trusted Lists

    [Fact]
    public void ListOfTrustedLists_HasCorrectProperties()
    {
        // Arrange
        var lotl = new ListOfTrustedLists
        {
            SequenceNumber = 42,
            IssueDate = DateTimeOffset.UtcNow,
            NextUpdate = DateTimeOffset.UtcNow.AddMonths(6),
            TrustedLists = new[]
            {
                new TrustedListPointer { Territory = "DE", TslLocation = "https://de.example/tl" }
            }
        };

        // Assert
        lotl.SequenceNumber.Should().Be(42);
        lotl.TrustedLists.Should().HaveCount(1);
    }

    [Fact]
    public void ListOfTrustedLists_TrustedLists_DefaultsToEmpty()
    {
        // Arrange & Act
        var lotl = new ListOfTrustedLists();

        // Assert
        lotl.TrustedLists.Should().BeEmpty();
    }

    #endregion

    #region Trusted Service Provider

    [Fact]
    public void TrustedServiceProvider_HasCorrectProperties()
    {
        // Arrange
        var provider = new TrustedServiceProvider
        {
            Name = "Test Provider GmbH",
            ServiceType = TrustServiceType.PidProvider,
            Status = "http://uri.etsi.org/TrstSvc/TrustedList/Svcstatus/granted",
            ServiceEndpoint = "https://provider.example.de/api"
        };

        // Assert
        provider.Name.Should().Be("Test Provider GmbH");
        provider.ServiceType.Should().Be(TrustServiceType.PidProvider);
        provider.Status.Should().Contain("granted");
    }

    [Fact]
    public void TrustedServiceProvider_DefaultsToEmptyValues()
    {
        // Arrange & Act
        var provider = new TrustedServiceProvider();

        // Assert
        provider.Name.Should().BeEmpty();
        provider.Status.Should().BeEmpty();
        provider.ServiceEndpoint.Should().BeNull();
        provider.Certificates.Should().BeEmpty();
    }

    #endregion

    #region Cache Behavior

    [Fact]
    public void CacheTimeout_HasReasonableDefault()
    {
        // Assert - Cache should refresh periodically but not too frequently
        _resolver.CacheTimeout.Should().BeGreaterOrEqualTo(TimeSpan.FromHours(1));
        _resolver.CacheTimeout.Should().BeLessOrEqualTo(TimeSpan.FromDays(1));
    }

    [Fact]
    public void ClearCache_ResetsCache()
    {
        // Act
        _resolver.ClearCache();

        // Assert - Should not throw and cache should be empty
        _resolver.IsCacheEmpty.Should().BeTrue();
    }

    #endregion
}
