using SdJwt.Net.PresentationExchange.Models;
using Xunit;

namespace SdJwt.Net.PresentationExchange.Tests.Models;

/// <summary>
/// Tests for PredicateFilter functionality.
/// </summary>
public class PredicateFilterTests
{
    [Fact]
    public void CreateAgeOver_ValidAge_ShouldCreateFilter()
    {
        // Act
        var filter = PredicateFilter.CreateAgeOver(21);

        // Assert
        Assert.Equal("predicate", filter.Type);
        Assert.Equal("age_over", filter.Predicate);
        Assert.Equal(21, filter.Threshold);
        Assert.False(filter.ZeroKnowledge);
        Assert.Null(filter.ProofType);
    }

    [Fact]
    public void CreateAgeOver_WithZeroKnowledge_ShouldSetProofType()
    {
        // Act
        var filter = PredicateFilter.CreateAgeOver(18, zeroKnowledge: true);

        // Assert
        Assert.Equal("predicate", filter.Type);
        Assert.Equal("age_over", filter.Predicate);
        Assert.Equal(18, filter.Threshold);
        Assert.True(filter.ZeroKnowledge);
        Assert.Equal("range-proof", filter.ProofType);
    }

    [Fact]
    public void CreateSalaryOver_ValidAmount_ShouldCreateFilter()
    {
        // Act
        var filter = PredicateFilter.CreateSalaryOver(75000);

        // Assert
        Assert.Equal("predicate", filter.Type);
        Assert.Equal("greater_than_or_equal", filter.Predicate);
        Assert.Equal(75000m, filter.Threshold);
        Assert.True(filter.ZeroKnowledge); // Default true for salary
        Assert.Equal("zk-snark", filter.ProofType);
    }

    [Fact]
    public void CreateRange_ValidRange_ShouldCreateFilter()
    {
        // Act
        var filter = PredicateFilter.CreateRange(100, 1000, zeroKnowledge: true);

        // Assert
        Assert.Equal("predicate", filter.Type);
        Assert.Equal("in_range", filter.Predicate);
        Assert.Equal(new object[] { 100, 1000 }, filter.Range);
        Assert.True(filter.ZeroKnowledge);
        Assert.Equal("range-proof", filter.ProofType);
    }

    [Fact]
    public void CreateCitizenshipIn_ValidCountries_ShouldCreateFilter()
    {
        // Act
        var filter = PredicateFilter.CreateCitizenshipIn("US", "CA", "UK");

        // Assert
        Assert.Equal("predicate", filter.Type);
        Assert.Equal("in_set", filter.Predicate);
        Assert.Equal(new object[] { "US", "CA", "UK" }, filter.Enum);
    }

    [Fact]
    public void Validate_ValidAgeOverFilter_ShouldNotThrow()
    {
        // Arrange
        var filter = PredicateFilter.CreateAgeOver(21);

        // Act & Assert
        filter.Validate(); // Should not throw
    }

    [Fact]
    public void Validate_InvalidPredicate_ShouldThrow()
    {
        // Arrange
        var filter = new PredicateFilter
        {
            Type = "predicate",
            Predicate = "invalid_predicate"
        };

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => filter.Validate());
        Assert.Contains("Invalid predicate 'invalid_predicate'", exception.Message);
    }

    [Fact]
    public void Validate_AgeOverWithoutThreshold_ShouldThrow()
    {
        // Arrange
        var filter = new PredicateFilter
        {
            Type = "predicate",
            Predicate = "age_over"
            // Missing Threshold
        };

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => filter.Validate());
        Assert.Contains("Threshold is required for age_over predicate", exception.Message);
    }

    [Fact]
    public void Validate_InRangeWithInvalidRange_ShouldThrow()
    {
        // Arrange
        var filter = new PredicateFilter
        {
            Type = "predicate",
            Predicate = "in_range",
            Range = new object[] { 100 } // Should have 2 values
        };

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => filter.Validate());
        Assert.Contains("Range must contain exactly two values", exception.Message);
    }
}
