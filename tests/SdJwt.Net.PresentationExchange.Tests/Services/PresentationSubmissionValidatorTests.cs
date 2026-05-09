using Microsoft.Extensions.Logging.Abstractions;
using SdJwt.Net.PresentationExchange.Models;
using SdJwt.Net.PresentationExchange.Services;
using Xunit;

namespace SdJwt.Net.PresentationExchange.Tests.Services;

public class PresentationSubmissionValidatorTests
{
    [Fact]
    public async Task ValidateAsync_WithMatchingSubmission_ReturnsValid()
    {
        var validator = CreateValidator();
        var definition = CreateDefinition();
        var submission = PresentationSubmission.Create(
            "submission-1",
            "definition-1",
            new[]
            {
                new InputDescriptorMapping
                {
                    Id = "employee_id",
                    Format = PresentationExchangeConstants.Formats.JwtVc,
                    Path = "$"
                }
            });
        var envelope = """{"type":["VerifiableCredential","EmployeeCredential"],"issuer":"did:example:issuer"}""";

        var result = await validator.ValidateAsync(definition, submission, envelope);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task ValidateAsync_WithUnsatisfiedConstraints_ReturnsInvalid()
    {
        var validator = CreateValidator();
        var definition = CreateDefinition();
        var submission = PresentationSubmission.Create(
            "submission-1",
            "definition-1",
            new[]
            {
                new InputDescriptorMapping
                {
                    Id = "employee_id",
                    Format = PresentationExchangeConstants.Formats.JwtVc,
                    Path = "$"
                }
            });
        var envelope = """{"type":["VerifiableCredential","OtherCredential"],"issuer":"did:example:issuer"}""";

        var result = await validator.ValidateAsync(definition, submission, envelope);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.Code == "constraints_not_satisfied");
    }

    private static PresentationSubmissionValidator CreateValidator()
    {
        var jsonPathEvaluator = new JsonPathEvaluator(NullLogger<JsonPathEvaluator>.Instance);
        var fieldFilterEvaluator = new FieldFilterEvaluator(NullLogger<FieldFilterEvaluator>.Instance);
        var constraintEvaluator = new ConstraintEvaluator(
            NullLogger<ConstraintEvaluator>.Instance,
            jsonPathEvaluator,
            fieldFilterEvaluator);

        return new PresentationSubmissionValidator(
            NullLogger<PresentationSubmissionValidator>.Instance,
            jsonPathEvaluator,
            constraintEvaluator);
    }

    private static PresentationDefinition CreateDefinition()
    {
        return PresentationDefinition.Create(
            "definition-1",
            new[]
            {
                new InputDescriptor
                {
                    Id = "employee_id",
                    Format = new FormatConstraints
                    {
                        JwtVc = new JwtFormatConstraints()
                    },
                    Constraints = Constraints.Create(
                        new Field
                        {
                            Path = new[] { "$.type" },
                            Filter = new FieldFilter
                            {
                                Type = "array",
                                Contains = "EmployeeCredential"
                            }
                        })
                }
            });
    }
}
