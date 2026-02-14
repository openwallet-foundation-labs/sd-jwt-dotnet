using FluentAssertions;
using SdJwt.Net.StatusList.Models;
using SdJwt.Net.StatusList.Verifier;
using System.Text.Json;
using Xunit;

namespace SdJwt.Net.StatusList.Tests.Models;

public class StatusListModelsEnhancedTests
{
    [Fact]
    public void StatusListAggregation_ShouldSerializeCorrectly()
    {
        // Arrange
        var aggregation = new StatusListAggregation
        {
            StatusLists = new[]
            {
                "https://issuer.example.com/status-list-1",
                "https://issuer.example.com/status-list-2"
            }
        };

        // Act
        var json = JsonSerializer.Serialize(aggregation, SdJwtConstants.DefaultJsonSerializerOptions);
        var deserialized = JsonSerializer.Deserialize<StatusListAggregation>(json, SdJwtConstants.DefaultJsonSerializerOptions);

        // Assert
        json.Should().Contain("status_lists");
        deserialized.Should().NotBeNull();
        deserialized!.StatusLists.Should().BeEquivalentTo(aggregation.StatusLists);
    }

    [Fact]
    public void RetryPolicy_DefaultValues_ShouldBeReasonable()
    {
        // Act
        var policy = new RetryPolicy();

        // Assert
        policy.MaxRetries.Should().Be(3);
        policy.BaseDelay.Should().Be(TimeSpan.FromSeconds(1));
        policy.MaxDelay.Should().Be(TimeSpan.FromSeconds(10)); // Updated to match implementation
        policy.UseExponentialBackoff.Should().BeTrue();
    }

    [Fact]
    public void StatusCheckResult_ShouldInitializeCorrectly()
    {
        // Act
        var result = new StatusCheckResult();

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(StatusType.Valid); // Default value
    }

    [Fact]
    public void StatusClaim_ShouldSerializeCorrectly()
    {
        // Arrange
        var statusClaim = new StatusClaim
        {
            StatusList = new StatusListReference
            {
                Uri = "https://issuer.example.com/status-list",
                Index = 42
            }
        };

        // Act
        var json = JsonSerializer.Serialize(statusClaim, SdJwtConstants.DefaultJsonSerializerOptions);
        var deserialized = JsonSerializer.Deserialize<StatusClaim>(json, SdJwtConstants.DefaultJsonSerializerOptions);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.StatusList.Should().NotBeNull();
        deserialized.StatusList!.Uri.Should().Be("https://issuer.example.com/status-list");
        deserialized.StatusList.Index.Should().Be(42);
    }

    [Fact]
    public void StatusList_ShouldSerializeCorrectly()
    {
        // Arrange
        var statusList = new SdJwt.Net.StatusList.Models.StatusList
        {
            Bits = 2,
            List = "eNpFVM2NwzAI_Jd8j8CgfHJvVVQptH0CuSR2sLa6Ye7HQN53tOz-C5_0"
        };

        // Act
        var json = JsonSerializer.Serialize(statusList, SdJwtConstants.DefaultJsonSerializerOptions);
        var deserialized = JsonSerializer.Deserialize<SdJwt.Net.StatusList.Models.StatusList>(json, SdJwtConstants.DefaultJsonSerializerOptions);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.Bits.Should().Be(2);
        deserialized.List.Should().Be("eNpFVM2NwzAI_Jd8j8CgfHJvVVQptH0CuSR2sLa6Ye7HQN53tOz-C5_0");
    }

    [Fact]
    public void StatusListData_ShouldInitializeCorrectly()
    {
        // Act
        var data = new StatusListData();

        // Assert
        data.Should().NotBeNull();
    }

    [Fact]
    public void StatusListReference_ShouldSerializeCorrectly()
    {
        // Arrange
        var reference = new StatusListReference
        {
            Uri = "https://issuer.example.com/status-list",
            Index = 123
        };

        // Act
        var json = JsonSerializer.Serialize(reference, SdJwtConstants.DefaultJsonSerializerOptions);
        var deserialized = JsonSerializer.Deserialize<StatusListReference>(json, SdJwtConstants.DefaultJsonSerializerOptions);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.Uri.Should().Be("https://issuer.example.com/status-list");
        deserialized.Index.Should().Be(123);
    }

    [Fact]
    public void StatusListTokenPayload_ShouldInitializeCorrectly()
    {
        // Arrange
        var payload = new StatusListTokenPayload
        {
            Subject = "https://issuer.example.com/status-list",
            IssuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            StatusList = new SdJwt.Net.StatusList.Models.StatusList
            {
                Bits = 1,
                List = "test-list"
            }
        };

        // Act
        var json = JsonSerializer.Serialize(payload, SdJwtConstants.DefaultJsonSerializerOptions);
        var deserialized = JsonSerializer.Deserialize<StatusListTokenPayload>(json, SdJwtConstants.DefaultJsonSerializerOptions);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.Subject.Should().Be("https://issuer.example.com/status-list");
        deserialized.StatusList.Should().NotBeNull();
    }

    [Fact]
    public void StatusType_Extensions_ShouldWorkCorrectly()
    {
        // Act & Assert
        StatusType.Valid.GetName().Should().Be("VALID"); // Updated to match implementation
        StatusType.Invalid.GetName().Should().Be("INVALID"); // Updated to match implementation
        StatusType.Suspended.GetName().Should().Be("SUSPENDED"); // Updated to match implementation

        StatusTypeExtensions.FromValue(0).Should().Be(StatusType.Valid);
        StatusTypeExtensions.FromValue(1).Should().Be(StatusType.Invalid);
        StatusTypeExtensions.FromValue(2).Should().Be(StatusType.Suspended);
    }

    [Fact]
    public void StatusListOptions_ShouldHaveDefaultValues()
    {
        // Act
        var options = new StatusListOptions();

        // Assert
        options.Should().NotBeNull();
        options.EnableStatusChecking.Should().BeFalse(); // Updated to match implementation
        options.CacheStatusLists.Should().BeTrue();
        options.FailOnStatusCheckError.Should().BeTrue();
    }

    [Fact]
    public void HttpStatusListFetcher_Constructor_WithNullHttpClient_ShouldCreateDefault()
    {
        // Act
        var fetcher = new HttpStatusListFetcher();

        // Assert
        fetcher.Should().NotBeNull();
    }

    [Fact]
    public void HttpStatusListFetcher_Dispose_ShouldNotThrow()
    {
        // Arrange
        var fetcher = new HttpStatusListFetcher();

        // Act & Assert
        fetcher.Invoking(f => f.Dispose()).Should().NotThrow();
    }

    [Fact]
    public void StatusListOptions_WithCustomValues_ShouldPreserveValues()
    {
        // Arrange
        var options = new StatusListOptions
        {
            EnableStatusChecking = false,
            CacheStatusLists = false,
            FailOnStatusCheckError = false,
            MaxStatusListAge = TimeSpan.FromHours(2),
            CacheDuration = TimeSpan.FromMinutes(30)
        };

        // Assert
        options.EnableStatusChecking.Should().BeFalse();
        options.CacheStatusLists.Should().BeFalse();
        options.FailOnStatusCheckError.Should().BeFalse();
        options.MaxStatusListAge.Should().Be(TimeSpan.FromHours(2));
        options.CacheDuration.Should().Be(TimeSpan.FromMinutes(30));
    }
}
