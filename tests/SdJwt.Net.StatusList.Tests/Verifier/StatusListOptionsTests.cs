using FluentAssertions;
using SdJwt.Net.StatusList.Verifier;
using Xunit;

namespace SdJwt.Net.StatusList.Tests.Verifier;

/// <summary>
/// Tests for StatusListOptions focusing on critical configuration paths.
/// </summary>
public class StatusListOptionsTests
{
    [Fact]
    public void DefaultConstructor_ShouldInitializeWithDefaultValues()
    {
        // Act
        var options = new StatusListOptions();

        // Assert
        options.EnableStatusChecking.Should().BeFalse();
        options.CacheStatusLists.Should().BeTrue();
        options.CacheDuration.Should().Be(TimeSpan.FromMinutes(5));
        options.MaxCacheSize.Should().Be(100);
        options.FailOnStatusCheckError.Should().BeTrue();
        options.StatusCheckTimeout.Should().Be(TimeSpan.FromSeconds(10));
        options.ValidateStatusListIssuer.Should().BeTrue();
        options.ValidateStatusListTiming.Should().BeTrue();
        options.MaxStatusListAge.Should().Be(TimeSpan.FromHours(24));
        options.UseConditionalRequests.Should().BeTrue();
        options.RetryPolicy.Should().NotBeNull();
    }

    [Fact]
    public void Properties_ShouldBeSettable()
    {
        // Arrange
        var options = new StatusListOptions();
        var customHttpClient = new HttpClient();

        // Act
        options.EnableStatusChecking = true;
        options.CacheStatusLists = false;
        options.CacheDuration = TimeSpan.FromMinutes(10);
        options.MaxCacheSize = 200;
        options.FailOnStatusCheckError = false;
        options.StatusCheckTimeout = TimeSpan.FromSeconds(30);
        options.HttpClient = customHttpClient;
        options.ValidateStatusListIssuer = false;
        options.ValidateStatusListTiming = false;
        options.MaxStatusListAge = TimeSpan.FromHours(48);
        options.UseConditionalRequests = false;

        // Assert
        options.EnableStatusChecking.Should().BeTrue();
        options.CacheStatusLists.Should().BeFalse();
        options.CacheDuration.Should().Be(TimeSpan.FromMinutes(10));
        options.MaxCacheSize.Should().Be(200);
        options.FailOnStatusCheckError.Should().BeFalse();
        options.StatusCheckTimeout.Should().Be(TimeSpan.FromSeconds(30));
        options.HttpClient.Should().Be(customHttpClient);
        options.ValidateStatusListIssuer.Should().BeFalse();
        options.ValidateStatusListTiming.Should().BeFalse();
        options.MaxStatusListAge.Should().Be(TimeSpan.FromHours(48));
        options.UseConditionalRequests.Should().BeFalse();
    }

    [Fact]
    public void AllowedStatusPurposes_ShouldBeModifiable()
    {
        // Arrange
        var options = new StatusListOptions();

        // Act
        options.AllowedStatusPurposes.Add("revocation");
        options.AllowedStatusPurposes.Add("suspension");

        // Assert
        options.AllowedStatusPurposes.Should().Contain("revocation");
        options.AllowedStatusPurposes.Should().Contain("suspension");
        options.AllowedStatusPurposes.Should().HaveCount(2);
    }

    [Fact]
    public void CustomHeaders_ShouldBeModifiable()
    {
        // Arrange
        var options = new StatusListOptions();

        // Act
        options.CustomHeaders["Authorization"] = "Bearer token123";
        options.CustomHeaders["X-Custom-Header"] = "custom-value";

        // Assert
        options.CustomHeaders.Should().ContainKey("Authorization");
        options.CustomHeaders["Authorization"].Should().Be("Bearer token123");
        options.CustomHeaders.Should().ContainKey("X-Custom-Header");
        options.CustomHeaders["X-Custom-Header"].Should().Be("custom-value");
    }

    [Fact]
    public void RetryPolicy_ShouldHaveDefaultValues()
    {
        // Arrange
        var options = new StatusListOptions();

        // Assert
        options.RetryPolicy.MaxRetries.Should().Be(3);
        options.RetryPolicy.BaseDelay.Should().Be(TimeSpan.FromSeconds(1));
        options.RetryPolicy.UseExponentialBackoff.Should().BeTrue();
        options.RetryPolicy.MaxDelay.Should().Be(TimeSpan.FromSeconds(10));
    }

    [Fact]
    public void RetryPolicy_PropertiesShouldBeSettable()
    {
        // Arrange
        var retryPolicy = new RetryPolicy();

        // Act
        retryPolicy.MaxRetries = 5;
        retryPolicy.BaseDelay = TimeSpan.FromSeconds(2);
        retryPolicy.UseExponentialBackoff = false;
        retryPolicy.MaxDelay = TimeSpan.FromSeconds(20);

        // Assert
        retryPolicy.MaxRetries.Should().Be(5);
        retryPolicy.BaseDelay.Should().Be(TimeSpan.FromSeconds(2));
        retryPolicy.UseExponentialBackoff.Should().BeFalse();
        retryPolicy.MaxDelay.Should().Be(TimeSpan.FromSeconds(20));
    }
}
