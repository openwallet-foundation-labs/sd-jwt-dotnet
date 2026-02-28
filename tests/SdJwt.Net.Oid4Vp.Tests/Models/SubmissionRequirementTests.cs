using FluentAssertions;
using SdJwt.Net.Oid4Vp.Models;
using Xunit;

namespace SdJwt.Net.Oid4Vp.Tests.Coverage;

/// <summary>
/// Tests for <see cref="SubmissionRequirement"/> class, covering factory methods,
/// validation rules, and nested requirement handling.
/// </summary>
public class SubmissionRequirementTests
{
    /// <summary>
    /// Tests that RequireAll() creates valid submission requirement.
    /// </summary>
    [Fact]
    public void SubmissionRequirement_RequireAll_WithValidParams_ReturnsInstance()
    {
        // Act
        var result = SubmissionRequirement.RequireAll(
            new[] { "desc-1", "desc-2" },
            "Identity Verification",
            "To verify identity");

        // Assert
        result.Rule.Should().Be(Oid4VpConstants.PresentationExchange.SubmissionRequirementRules.All);
        result.From.Should().ContainInOrder("desc-1", "desc-2");
        result.Name.Should().Be("Identity Verification");
        result.Purpose.Should().Be("To verify identity");
    }

    /// <summary>
    /// Tests that RequireAll() throws ArgumentNullException when inputDescriptorIds is null.
    /// </summary>
    [Fact]
    public void SubmissionRequirement_RequireAll_WithNullIds_ThrowsArgumentNullException()
    {
        // Act & Assert
        var act = () => SubmissionRequirement.RequireAll(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    /// <summary>
    /// Tests that RequireAll() throws ArgumentException when inputDescriptorIds is empty.
    /// </summary>
    [Fact]
    public void SubmissionRequirement_RequireAll_WithEmptyIds_ThrowsArgumentException()
    {
        // Act & Assert
        var act = () => SubmissionRequirement.RequireAll(Array.Empty<string>());
        act.Should().Throw<ArgumentException>().WithMessage("*At least one*");
    }

    /// <summary>
    /// Tests that RequirePick() creates valid submission requirement.
    /// </summary>
    [Fact]
    public void SubmissionRequirement_RequirePick_WithValidParams_ReturnsInstance()
    {
        // Act
        var result = SubmissionRequirement.RequirePick(
            new[] { "desc-1", "desc-2", "desc-3" },
            2,
            "Pick Two",
            "Pick any two credentials");

        // Assert
        result.Rule.Should().Be(Oid4VpConstants.PresentationExchange.SubmissionRequirementRules.Pick);
        result.Count.Should().Be(2);
        result.From.Should().HaveCount(3);
        result.Name.Should().Be("Pick Two");
        result.Purpose.Should().Be("Pick any two credentials");
    }

    /// <summary>
    /// Tests that RequirePick() throws ArgumentNullException when inputDescriptorIds is null.
    /// </summary>
    [Fact]
    public void SubmissionRequirement_RequirePick_WithNullIds_ThrowsArgumentNullException()
    {
        // Act & Assert
        var act = () => SubmissionRequirement.RequirePick(null!, 1);
        act.Should().Throw<ArgumentNullException>();
    }

    /// <summary>
    /// Tests that RequirePick() throws ArgumentException when inputDescriptorIds is empty.
    /// </summary>
    [Fact]
    public void SubmissionRequirement_RequirePick_WithEmptyIds_ThrowsArgumentException()
    {
        // Act & Assert
        var act = () => SubmissionRequirement.RequirePick(Array.Empty<string>(), 1);
        act.Should().Throw<ArgumentException>().WithMessage("*At least one*");
    }

    /// <summary>
    /// Tests that RequirePick() throws ArgumentException when count is zero.
    /// </summary>
    [Fact]
    public void SubmissionRequirement_RequirePick_WithZeroCount_ThrowsArgumentException()
    {
        // Act & Assert
        var act = () => SubmissionRequirement.RequirePick(new[] { "desc-1" }, 0);
        act.Should().Throw<ArgumentException>().WithMessage("*positive*");
    }

    /// <summary>
    /// Tests that RequirePick() throws ArgumentException when count is negative.
    /// </summary>
    [Fact]
    public void SubmissionRequirement_RequirePick_WithNegativeCount_ThrowsArgumentException()
    {
        // Act & Assert
        var act = () => SubmissionRequirement.RequirePick(new[] { "desc-1" }, -1);
        act.Should().Throw<ArgumentException>().WithMessage("*positive*");
    }

    /// <summary>
    /// Tests that RequirePick() throws ArgumentException when count exceeds descriptors.
    /// </summary>
    [Fact]
    public void SubmissionRequirement_RequirePick_WithCountExceedingDescriptors_ThrowsArgumentException()
    {
        // Act & Assert
        var act = () => SubmissionRequirement.RequirePick(new[] { "desc-1", "desc-2" }, 5);
        act.Should().Throw<ArgumentException>().WithMessage("*exceed*");
    }

    /// <summary>
    /// Tests that RequirePickRange() creates valid submission requirement.
    /// </summary>
    [Fact]
    public void SubmissionRequirement_RequirePickRange_WithValidParams_ReturnsInstance()
    {
        // Act
        var result = SubmissionRequirement.RequirePickRange(
            new[] { "desc-1", "desc-2", "desc-3" },
            1, 2,
            "Pick Range",
            "Pick between 1 and 2");

        // Assert
        result.Rule.Should().Be(Oid4VpConstants.PresentationExchange.SubmissionRequirementRules.Pick);
        result.Min.Should().Be(1);
        result.Max.Should().Be(2);
        result.From.Should().HaveCount(3);
    }

    /// <summary>
    /// Tests that RequirePickRange() throws ArgumentNullException when inputDescriptorIds is null.
    /// </summary>
    [Fact]
    public void SubmissionRequirement_RequirePickRange_WithNullIds_ThrowsArgumentNullException()
    {
        // Act & Assert
        var act = () => SubmissionRequirement.RequirePickRange(null!, 1, 2);
        act.Should().Throw<ArgumentNullException>();
    }

    /// <summary>
    /// Tests that RequirePickRange() throws ArgumentException when inputDescriptorIds is empty.
    /// </summary>
    [Fact]
    public void SubmissionRequirement_RequirePickRange_WithEmptyIds_ThrowsArgumentException()
    {
        // Act & Assert
        var act = () => SubmissionRequirement.RequirePickRange(Array.Empty<string>(), 1, 2);
        act.Should().Throw<ArgumentException>().WithMessage("*At least one*");
    }

    /// <summary>
    /// Tests that RequirePickRange() throws ArgumentException when min is zero.
    /// </summary>
    [Fact]
    public void SubmissionRequirement_RequirePickRange_WithZeroMin_ThrowsArgumentException()
    {
        // Act & Assert
        var act = () => SubmissionRequirement.RequirePickRange(new[] { "desc-1" }, 0, 1);
        act.Should().Throw<ArgumentException>().WithMessage("*positive*");
    }

    /// <summary>
    /// Tests that RequirePickRange() throws ArgumentException when max is less than min.
    /// </summary>
    [Fact]
    public void SubmissionRequirement_RequirePickRange_WithMaxLessThanMin_ThrowsArgumentException()
    {
        // Act & Assert
        var act = () => SubmissionRequirement.RequirePickRange(new[] { "desc-1", "desc-2" }, 2, 1);
        act.Should().Throw<ArgumentException>().WithMessage("*greater than or equal*");
    }

    /// <summary>
    /// Tests that RequirePickRange() throws ArgumentException when max exceeds descriptors.
    /// </summary>
    [Fact]
    public void SubmissionRequirement_RequirePickRange_WithMaxExceedingDescriptors_ThrowsArgumentException()
    {
        // Act & Assert
        var act = () => SubmissionRequirement.RequirePickRange(new[] { "desc-1" }, 1, 5);
        act.Should().Throw<ArgumentException>().WithMessage("*exceed*");
    }

    /// <summary>
    /// Tests that Validate() throws InvalidOperationException when rule is empty.
    /// </summary>
    [Fact]
    public void SubmissionRequirement_Validate_WithEmptyRule_ThrowsInvalidOperationException()
    {
        // Arrange
        var requirement = new SubmissionRequirement { Rule = "" };
        var inputDescriptors = new[] { new InputDescriptor { Id = "desc-1" } };

        // Act & Assert
        var act = () => requirement.Validate(inputDescriptors);
        act.Should().Throw<InvalidOperationException>().WithMessage("*rule*required*");
    }

    /// <summary>
    /// Tests that Validate() throws InvalidOperationException when rule is invalid.
    /// </summary>
    [Fact]
    public void SubmissionRequirement_Validate_WithInvalidRule_ThrowsInvalidOperationException()
    {
        // Arrange
        var requirement = new SubmissionRequirement { Rule = "invalid" };
        var inputDescriptors = new[] { new InputDescriptor { Id = "desc-1" } };

        // Act & Assert
        var act = () => requirement.Validate(inputDescriptors);
        act.Should().Throw<InvalidOperationException>().WithMessage("*'all' or 'pick'*");
    }

    /// <summary>
    /// Tests that Validate() throws InvalidOperationException when both from and from_nested present.
    /// </summary>
    [Fact]
    public void SubmissionRequirement_Validate_WithBothFromAndFromNested_ThrowsInvalidOperationException()
    {
        // Arrange
        var requirement = new SubmissionRequirement
        {
            Rule = "all",
            From = new[] { "desc-1" },
            FromNested = new[] { new SubmissionRequirement() }
        };
        var inputDescriptors = new[] { new InputDescriptor { Id = "desc-1" } };

        // Act & Assert
        var act = () => requirement.Validate(inputDescriptors);
        act.Should().Throw<InvalidOperationException>().WithMessage("*both*");
    }

    /// <summary>
    /// Tests that Validate() throws InvalidOperationException when neither from nor from_nested present.
    /// </summary>
    [Fact]
    public void SubmissionRequirement_Validate_WithNeitherFromNorFromNested_ThrowsInvalidOperationException()
    {
        // Arrange
        var requirement = new SubmissionRequirement { Rule = "all" };
        var inputDescriptors = new[] { new InputDescriptor { Id = "desc-1" } };

        // Act & Assert
        var act = () => requirement.Validate(inputDescriptors);
        act.Should().Throw<InvalidOperationException>().WithMessage("*either*");
    }

    /// <summary>
    /// Tests that Validate() throws InvalidOperationException when from references unknown descriptor.
    /// </summary>
    [Fact]
    public void SubmissionRequirement_Validate_WithUnknownDescriptorId_ThrowsInvalidOperationException()
    {
        // Arrange
        var requirement = SubmissionRequirement.RequireAll(new[] { "unknown-id" });
        var inputDescriptors = new[] { new InputDescriptor { Id = "desc-1" } };

        // Act & Assert
        var act = () => requirement.Validate(inputDescriptors);
        act.Should().Throw<InvalidOperationException>().WithMessage("*unknown*");
    }

    /// <summary>
    /// Tests that Validate() succeeds with valid all rule.
    /// </summary>
    [Fact]
    public void SubmissionRequirement_Validate_WithValidAllRule_DoesNotThrow()
    {
        // Arrange
        var requirement = SubmissionRequirement.RequireAll(new[] { "desc-1", "desc-2" });
        var inputDescriptors = new[]
        {
            new InputDescriptor { Id = "desc-1" },
            new InputDescriptor { Id = "desc-2" }
        };

        // Act & Assert
        var act = () => requirement.Validate(inputDescriptors);
        act.Should().NotThrow();
    }

    /// <summary>
    /// Tests that Validate() succeeds with valid pick rule using count.
    /// </summary>
    [Fact]
    public void SubmissionRequirement_Validate_WithValidPickRuleCount_DoesNotThrow()
    {
        // Arrange
        var requirement = SubmissionRequirement.RequirePick(new[] { "desc-1", "desc-2" }, 1);
        var inputDescriptors = new[]
        {
            new InputDescriptor { Id = "desc-1" },
            new InputDescriptor { Id = "desc-2" }
        };

        // Act & Assert
        var act = () => requirement.Validate(inputDescriptors);
        act.Should().NotThrow();
    }

    /// <summary>
    /// Tests that Validate() succeeds with valid pick rule using min/max.
    /// </summary>
    [Fact]
    public void SubmissionRequirement_Validate_WithValidPickRuleRange_DoesNotThrow()
    {
        // Arrange
        var requirement = SubmissionRequirement.RequirePickRange(new[] { "desc-1", "desc-2" }, 1, 2);
        var inputDescriptors = new[]
        {
            new InputDescriptor { Id = "desc-1" },
            new InputDescriptor { Id = "desc-2" }
        };

        // Act & Assert
        var act = () => requirement.Validate(inputDescriptors);
        act.Should().NotThrow();
    }

    /// <summary>
    /// Tests that Validate() throws when pick rule has no count or min/max.
    /// </summary>
    [Fact]
    public void SubmissionRequirement_Validate_PickRuleWithoutCountOrRange_ThrowsInvalidOperationException()
    {
        // Arrange
        var requirement = new SubmissionRequirement
        {
            Rule = "pick",
            From = new[] { "desc-1" }
        };
        var inputDescriptors = new[] { new InputDescriptor { Id = "desc-1" } };

        // Act & Assert
        var act = () => requirement.Validate(inputDescriptors);
        act.Should().Throw<InvalidOperationException>().WithMessage("*'count' or 'min'/'max'*");
    }

    /// <summary>
    /// Tests that Validate() cascades validation to nested requirements.
    /// </summary>
    [Fact]
    public void SubmissionRequirement_Validate_CascadesToNestedRequirements()
    {
        // Arrange
        var requirement = new SubmissionRequirement
        {
            Rule = "all",
            FromNested = new[]
            {
                new SubmissionRequirement { Rule = "" }  // Invalid nested
            }
        };
        var inputDescriptors = new[] { new InputDescriptor { Id = "desc-1" } };

        // Act & Assert
        var act = () => requirement.Validate(inputDescriptors);
        act.Should().Throw<InvalidOperationException>();
    }
}
