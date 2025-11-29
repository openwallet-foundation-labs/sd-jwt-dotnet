using System.Text.Json;
using Microsoft.Extensions.Logging;
using SdJwt.Net.PresentationExchange.Models;

namespace SdJwt.Net.PresentationExchange.Services;

/// <summary>
/// Evaluates field constraints against credential data according to DIF Presentation Exchange 2.1.1.
/// Supports JSON Schema-based validation and JSON path evaluation.
/// </summary>
public class ConstraintEvaluator
{
    private readonly ILogger<ConstraintEvaluator> _logger;
    private readonly JsonPathEvaluator _jsonPathEvaluator;
    private readonly FieldFilterEvaluator _fieldFilterEvaluator;

    /// <summary>
    /// Initializes a new instance of the ConstraintEvaluator class.
    /// </summary>
    /// <param name="logger">The logger instance</param>
    /// <param name="jsonPathEvaluator">The JSON path evaluator</param>
    /// <param name="fieldFilterEvaluator">The field filter evaluator</param>
    public ConstraintEvaluator(
        ILogger<ConstraintEvaluator> logger,
        JsonPathEvaluator jsonPathEvaluator,
        FieldFilterEvaluator fieldFilterEvaluator)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _jsonPathEvaluator = jsonPathEvaluator ?? throw new ArgumentNullException(nameof(jsonPathEvaluator));
        _fieldFilterEvaluator = fieldFilterEvaluator ?? throw new ArgumentNullException(nameof(fieldFilterEvaluator));
    }

    /// <summary>
    /// Evaluates whether a credential satisfies the given constraints.
    /// </summary>
    /// <param name="credential">The credential to evaluate</param>
    /// <param name="constraints">The constraints to check</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task that represents the constraint evaluation result</returns>
    public async Task<ConstraintEvaluationResult> EvaluateAsync(
        object credential,
        Constraints constraints,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Starting constraint evaluation for credential");

            var result = new ConstraintEvaluationResult();

            // Convert credential to JsonDocument for evaluation
            var credentialJson = ConvertToJsonDocument(credential);
            if (credentialJson == null)
            {
                result.AddError("INVALID_CREDENTIAL_FORMAT", "Credential could not be converted to JSON format");
                return result;
            }

            // Evaluate field constraints if present
            if (constraints.Fields != null && constraints.Fields.Length > 0)
            {
                foreach (var field in constraints.Fields)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    
                    var fieldResult = await EvaluateFieldConstraintAsync(credentialJson, field, cancellationToken);
                    result.MergeFieldResult(field.GetPrimaryPath() ?? "unknown", fieldResult);
                }
            }

            // Evaluate limit disclosure requirements
            if (!string.IsNullOrEmpty(constraints.LimitDisclosure))
            {
                var limitDisclosureResult = EvaluateLimitDisclosure(credential, constraints.LimitDisclosure);
                result.MergeDisclosureResult(limitDisclosureResult);
            }

            // Evaluate subject requirements
            if (!string.IsNullOrEmpty(constraints.SubjectIsIssuer))
            {
                var subjectResult = await EvaluateSubjectIsIssuerAsync(credentialJson, constraints.SubjectIsIssuer, cancellationToken);
                result.MergeSubjectResult(subjectResult);
            }

            // Set overall success based on field results
            result.IsSuccessful = result.FieldResults.All(kvp => kvp.Value.IsSuccessful) && 
                                  result.Errors.Count == 0;

            _logger.LogDebug("Constraint evaluation completed. Success: {Success}, Errors: {ErrorCount}", 
                result.IsSuccessful, result.Errors.Count);

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Constraint evaluation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during constraint evaluation");
            var result = new ConstraintEvaluationResult();
            result.AddError("EVALUATION_ERROR", $"Constraint evaluation failed: {ex.Message}");
            return result;
        }
    }

    /// <summary>
    /// Evaluates a single field constraint against the credential.
    /// </summary>
    /// <param name="credentialJson">The credential as JsonDocument</param>
    /// <param name="field">The field constraint to evaluate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task that represents the field evaluation result</returns>
    private async Task<FieldEvaluationResult> EvaluateFieldConstraintAsync(
        JsonDocument credentialJson,
        Field field,
        CancellationToken cancellationToken = default)
    {
        var result = new FieldEvaluationResult
        {
            FieldId = field.Id ?? field.GetPrimaryPath(),
            IsOptional = field.Optional
        };

        try
        {
            // Evaluate each path until one succeeds or all fail
            var pathFound = false;
            JsonElement? fieldValue = null;
            string? successfulPath = null;

            if (field.Path != null)
            {
                foreach (var path in field.Path)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var pathResult = await _jsonPathEvaluator.EvaluateAsync(credentialJson, path, cancellationToken);
                    if (pathResult.IsSuccessful && pathResult.Values.Any())
                    {
                        pathFound = true;
                        fieldValue = pathResult.Values.First();
                        successfulPath = path;
                        result.MatchedPath = path;
                        break;
                    }
                }
            }

            // If field is required but not found, that's an error
            if (!pathFound && !field.Optional)
            {
                result.AddError("FIELD_NOT_FOUND", $"Required field not found at any specified path");
                return result;
            }

            // If field is optional and not found, that's okay
            if (!pathFound && field.Optional)
            {
                result.IsSuccessful = true;
                return result;
            }

            // If we have a filter, evaluate it
            if (field.Filter != null && fieldValue.HasValue)
            {
                var filterResult = await _fieldFilterEvaluator.EvaluateAsync(fieldValue.Value, field.Filter, cancellationToken);
                if (!filterResult.IsSuccessful)
                {
                    result.AddError("FILTER_FAILED", $"Field value does not satisfy filter requirements");
                    result.FilterDetails = filterResult;
                    return result;
                }
            }

            result.IsSuccessful = true;
            result.FieldValue = fieldValue;

            _logger.LogDebug("Field constraint satisfied. Field: {FieldId}, Path: {Path}", 
                result.FieldId, successfulPath);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error evaluating field constraint. Field: {FieldId}", result.FieldId);
            result.AddError("FIELD_EVALUATION_ERROR", $"Field evaluation failed: {ex.Message}");
            return result;
        }
    }

    /// <summary>
    /// Evaluates limit disclosure requirements.
    /// </summary>
    /// <param name="credential">The credential to evaluate</param>
    /// <param name="limitDisclosure">The limit disclosure requirement</param>
    /// <returns>The evaluation result</returns>
    private DisclosureEvaluationResult EvaluateLimitDisclosure(object credential, string limitDisclosure)
    {
        var result = new DisclosureEvaluationResult();

        try
        {
            // Check if the credential supports selective disclosure
            var supportsSelectiveDisclosure = CheckSelectiveDisclosureSupport(credential);

            if (limitDisclosure == "required" && !supportsSelectiveDisclosure)
            {
                result.AddError("SELECTIVE_DISCLOSURE_REQUIRED", "Credential must support selective disclosure but doesn't");
                return result;
            }

            if (limitDisclosure == "preferred" && !supportsSelectiveDisclosure)
            {
                result.AddWarning("SELECTIVE_DISCLOSURE_PREFERRED", "Selective disclosure is preferred but not supported");
            }

            result.IsSuccessful = true;
            result.SupportsSelectiveDisclosure = supportsSelectiveDisclosure;

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error evaluating limit disclosure requirement");
            result.AddError("DISCLOSURE_EVALUATION_ERROR", $"Disclosure evaluation failed: {ex.Message}");
            return result;
        }
    }

    /// <summary>
    /// Evaluates subject is issuer requirements.
    /// </summary>
    /// <param name="credentialJson">The credential as JsonDocument</param>
    /// <param name="subjectIsIssuer">The subject is issuer requirement</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task that represents the subject evaluation result</returns>
    private async Task<SubjectEvaluationResult> EvaluateSubjectIsIssuerAsync(
        JsonDocument credentialJson,
        string subjectIsIssuer,
        CancellationToken cancellationToken = default)
    {
        var result = new SubjectEvaluationResult();

        try
        {
            // Get issuer value
            var issuerResult = await _jsonPathEvaluator.EvaluateAsync(credentialJson, "$.iss", cancellationToken);
            if (!issuerResult.IsSuccessful || !issuerResult.Values.Any())
            {
                if (subjectIsIssuer == "required")
                {
                    result.AddError("ISSUER_NOT_FOUND", "Issuer field is required but not found");
                    return result;
                }
            }

            // Get subject value
            var subjectResult = await _jsonPathEvaluator.EvaluateAsync(credentialJson, "$.sub", cancellationToken);
            if (!subjectResult.IsSuccessful || !subjectResult.Values.Any())
            {
                if (subjectIsIssuer == "required")
                {
                    result.AddError("SUBJECT_NOT_FOUND", "Subject field is required but not found");
                    return result;
                }
            }

            // Compare issuer and subject
            if (issuerResult.IsSuccessful && subjectResult.IsSuccessful)
            {
                var issuerValue = issuerResult.Values.First().GetString();
                var subjectValue = subjectResult.Values.First().GetString();

                var areEqual = string.Equals(issuerValue, subjectValue, StringComparison.Ordinal);

                if (subjectIsIssuer == "required" && !areEqual)
                {
                    result.AddError("SUBJECT_ISSUER_MISMATCH", "Subject must equal issuer but they differ");
                    return result;
                }

                if (subjectIsIssuer == "preferred" && !areEqual)
                {
                    result.AddWarning("SUBJECT_ISSUER_PREFERRED", "Subject and issuer should be equal but they differ");
                }

                result.SubjectEqualsIssuer = areEqual;
            }

            result.IsSuccessful = true;
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error evaluating subject is issuer requirement");
            result.AddError("SUBJECT_EVALUATION_ERROR", $"Subject evaluation failed: {ex.Message}");
            return result;
        }
    }

    /// <summary>
    /// Converts a credential object to JsonDocument for evaluation.
    /// </summary>
    /// <param name="credential">The credential to convert</param>
    /// <returns>JsonDocument or null if conversion fails</returns>
    private JsonDocument? ConvertToJsonDocument(object credential)
    {
        try
        {
            if (credential is JsonDocument jsonDoc)
                return jsonDoc;

            if (credential is string jsonString)
                return JsonDocument.Parse(jsonString);

            // Serialize object to JSON and parse back
            var json = JsonSerializer.Serialize(credential);
            return JsonDocument.Parse(json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to convert credential to JsonDocument");
            return null;
        }
    }

    /// <summary>
    /// Checks if a credential supports selective disclosure.
    /// </summary>
    /// <param name="credential">The credential to check</param>
    /// <returns>True if the credential supports selective disclosure</returns>
    private bool CheckSelectiveDisclosureSupport(object credential)
    {
        try
        {
            // For SD-JWT credentials, check if they contain disclosure information
            if (credential is string credentialString)
            {
                // SD-JWT format contains tildes (~) separating disclosure parts
                return credentialString.Contains("~");
            }

            // For structured credentials, check for SD-JWT specific fields
            var json = JsonSerializer.Serialize(credential);
            return json.Contains("_sd") || json.Contains("_sd_alg");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking selective disclosure support");
            return false;
        }
    }
}