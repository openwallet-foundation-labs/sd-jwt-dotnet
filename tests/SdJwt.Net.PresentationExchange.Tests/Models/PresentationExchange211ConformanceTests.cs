using FluentAssertions;
using SdJwt.Net.PresentationExchange.Models;
using Xunit;

namespace SdJwt.Net.PresentationExchange.Tests.Models;

public class PresentationExchange211ConformanceTests
{
    [Fact]
    public void InputDescriptor_WithoutConstraints_ThrowsInvalidOperationException()
    {
        var descriptor = new InputDescriptor
        {
            Id = "identity"
        };

        var act = () => descriptor.Validate();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*constraints are required*");
    }

    [Fact]
    public void PresentationDefinition_WithDuplicateNestedIds_ThrowsInvalidOperationException()
    {
        var definition = new PresentationDefinition
        {
            Id = "definition",
            InputDescriptors = new[]
            {
                new InputDescriptor
                {
                    Id = "definition",
                    Constraints = Constraints.Create(Field.CreateForExistence("$"))
                }
            }
        };

        var act = () => definition.Validate();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Duplicate input descriptor id*");
    }

    [Fact]
    public void Field_WithSpecPredicateAndNoFilter_ThrowsInvalidOperationException()
    {
        var field = new Field
        {
            Path = new[] { "$.age" },
            Predicate = "required"
        };

        var act = () => field.Validate();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*filter is required*");
    }

    [Fact]
    public void Constraints_WithRelationalAndStatusFeatures_Validates()
    {
        var constraints = new Constraints
        {
            IsHolder = new object[]
            {
                new HolderDirective
                {
                    FieldId = new[] { "subject_id" },
                    Directive = "required"
                }
            },
            SameSubject = new object[]
            {
                new HolderDirective
                {
                    FieldId = new[] { "subject_id", "account_id" },
                    Directive = "preferred"
                }
            },
            Statuses = new StatusConstraints
            {
                Active = new StatusDirective
                {
                    Directive = "required",
                    Type = new[] { "StatusList2021Entry" }
                },
                Revoked = new StatusDirective
                {
                    Directive = "disallowed"
                }
            }
        };

        var act = () => constraints.Validate();

        act.Should().NotThrow();
    }

    [Fact]
    public void SubmissionRequirement_WithZeroCount_ThrowsInvalidOperationException()
    {
        var requirement = new SubmissionRequirement
        {
            Rule = global::SdJwt.Net.PresentationExchange.Models.PresentationExchangeConstants.SubmissionRules.Pick,
            From = "group-a",
            Count = 0
        };

        var act = () => requirement.Validate();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*greater than zero*");
    }
}
