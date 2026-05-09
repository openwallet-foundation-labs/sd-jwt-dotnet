using System.Text.Json;
using Microsoft.Extensions.Logging;
using SdJwt.Net.PresentationExchange.Models;

namespace SdJwt.Net.PresentationExchange.Services;

/// <summary>
/// Validates a presentation submission against a presentation definition and submitted presentation envelope.
/// </summary>
public sealed class PresentationSubmissionValidator
{
    private readonly ILogger<PresentationSubmissionValidator> _logger;
    private readonly JsonPathEvaluator _jsonPathEvaluator;
    private readonly ConstraintEvaluator _constraintEvaluator;

    /// <summary>
    /// Initializes a new instance of the <see cref="PresentationSubmissionValidator"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="jsonPathEvaluator">The JSONPath evaluator.</param>
    /// <param name="constraintEvaluator">The constraint evaluator.</param>
    public PresentationSubmissionValidator(
        ILogger<PresentationSubmissionValidator> logger,
        JsonPathEvaluator jsonPathEvaluator,
        ConstraintEvaluator constraintEvaluator)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _jsonPathEvaluator = jsonPathEvaluator ?? throw new ArgumentNullException(nameof(jsonPathEvaluator));
        _constraintEvaluator = constraintEvaluator ?? throw new ArgumentNullException(nameof(constraintEvaluator));
    }

    /// <summary>
    /// Validates the submitted envelope against a presentation definition and presentation submission.
    /// </summary>
    /// <param name="definition">The expected presentation definition.</param>
    /// <param name="submission">The presentation submission to validate.</param>
    /// <param name="presentationEnvelope">The submitted presentation envelope or VP token collection.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The validation result.</returns>
    public async Task<PresentationSubmissionValidationResult> ValidateAsync(
        PresentationDefinition definition,
        PresentationSubmission submission,
        object presentationEnvelope,
        CancellationToken cancellationToken = default)
    {
        if (definition == null)
            throw new ArgumentNullException(nameof(definition));
        if (submission == null)
            throw new ArgumentNullException(nameof(submission));
        if (presentationEnvelope == null)
            throw new ArgumentNullException(nameof(presentationEnvelope));

        var result = new PresentationSubmissionValidationResult();

        try
        {
            definition.Validate();
            submission.Validate();
        }
        catch (Exception ex)
        {
            result.AddError("invalid_structure", ex.Message);
            return result;
        }

        if (!string.Equals(definition.Id, submission.DefinitionId, StringComparison.Ordinal))
        {
            result.AddError("definition_id_mismatch", "presentation_submission.definition_id does not match the presentation definition id");
            return result;
        }

        using var envelopeJson = ToJsonDocument(presentationEnvelope);
        if (envelopeJson == null)
        {
            result.AddError("invalid_presentation_envelope", "Presentation envelope could not be converted to JSON");
            return result;
        }

        var submittedDescriptorIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (var mapping in submission.DescriptorMap)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var descriptor = definition.GetInputDescriptor(mapping.Id);
            if (descriptor == null)
            {
                result.AddError("unknown_descriptor", $"descriptor_map id '{mapping.Id}' does not match an input descriptor");
                continue;
            }

            submittedDescriptorIds.Add(mapping.Id);

            if (!descriptor.GetAcceptedFormats().Contains(mapping.Format, StringComparer.Ordinal))
            {
                result.AddError("format_mismatch", $"descriptor_map format '{mapping.Format}' is not accepted by input descriptor '{mapping.Id}'");
                continue;
            }

            var pathResult = await _jsonPathEvaluator.EvaluateAsync(envelopeJson, mapping.Path, cancellationToken);
            if (!pathResult.IsSuccessful || pathResult.Values.Length == 0)
            {
                result.AddError("path_not_found", $"descriptor_map path '{mapping.Path}' did not resolve to a submitted claim");
                continue;
            }

            foreach (var resolvedClaim in pathResult.Values)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var claimObject = JsonElementToObject(resolvedClaim);
                if (claimObject == null)
                {
                    result.AddError("invalid_claim", $"descriptor_map path '{mapping.Path}' resolved to an unsupported value");
                    continue;
                }

                var constraintResult = await _constraintEvaluator.EvaluateAsync(
                    claimObject,
                    descriptor.Constraints!,
                    cancellationToken);

                if (!constraintResult.IsSuccessful)
                {
                    result.AddError(
                        "constraints_not_satisfied",
                        $"Submitted claim for descriptor '{mapping.Id}' does not satisfy constraints");
                }
            }
        }

        ValidateExpectedDescriptors(definition, submittedDescriptorIds, result);

        result.IsValid = result.Errors.Count == 0;
        _logger.LogDebug("Presentation submission validation completed. Valid: {Valid}", result.IsValid);
        return result;
    }

    private static void ValidateExpectedDescriptors(
        PresentationDefinition definition,
        HashSet<string> submittedDescriptorIds,
        PresentationSubmissionValidationResult result)
    {
        var requiredDescriptorIds = definition.GetReferencedDescriptorIds();
        foreach (var descriptorId in requiredDescriptorIds)
        {
            if (!submittedDescriptorIds.Contains(descriptorId))
            {
                result.AddError("missing_descriptor", $"descriptor_map is missing required input descriptor '{descriptorId}'");
            }
        }
    }

    private static JsonDocument? ToJsonDocument(object value)
    {
        try
        {
            if (value is JsonDocument document)
            {
                return JsonDocument.Parse(document.RootElement.GetRawText());
            }

            if (value is string text)
            {
                try
                {
                    return JsonDocument.Parse(text);
                }
                catch
                {
                    return JsonDocument.Parse(JsonSerializer.Serialize(text));
                }
            }

            return JsonDocument.Parse(JsonSerializer.Serialize(value));
        }
        catch
        {
            return null;
        }
    }

    private static object? JsonElementToObject(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => element.TryGetInt64(out var l) ? l : element.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            JsonValueKind.Array => element.EnumerateArray().Select(JsonElementToObject).ToArray(),
            JsonValueKind.Object => element.EnumerateObject().ToDictionary(p => p.Name, p => JsonElementToObject(p.Value)),
            _ => element.GetRawText()
        };
    }
}

/// <summary>
/// Result of presentation submission validation.
/// </summary>
public sealed class PresentationSubmissionValidationResult
{
    /// <summary>
    /// Gets or sets whether validation succeeded.
    /// </summary>
    public bool IsValid
    {
        get; set;
    }

    /// <summary>
    /// Gets validation errors.
    /// </summary>
    public List<PresentationSubmissionValidationError> Errors { get; } = new();

    /// <summary>
    /// Adds a validation error.
    /// </summary>
    /// <param name="code">The error code.</param>
    /// <param name="message">The error message.</param>
    public void AddError(string code, string message)
    {
        Errors.Add(new PresentationSubmissionValidationError
        {
            Code = code,
            Message = message
        });
        IsValid = false;
    }
}

/// <summary>
/// Presentation submission validation error details.
/// </summary>
public sealed class PresentationSubmissionValidationError
{
    /// <summary>
    /// Gets or sets the error code.
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the error message.
    /// </summary>
    public string Message { get; set; } = string.Empty;
}
