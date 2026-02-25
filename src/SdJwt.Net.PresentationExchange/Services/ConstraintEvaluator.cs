using System.Text.Json;
using Microsoft.Extensions.Logging;
using SdJwt.Net.PresentationExchange.Models;
using SdJwt.Net.Utils;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;

namespace SdJwt.Net.PresentationExchange.Services;

/// <summary>
/// Evaluates field constraints against credential data according to DIF Presentation Exchange 2.1.1.
/// Supports JSON Schema-based validation and JSON path evaluation.
/// </summary>
public class ConstraintEvaluator {
        private readonly ILogger<ConstraintEvaluator> _logger;
        private readonly JsonPathEvaluator _jsonPathEvaluator;
        private readonly FieldFilterEvaluator _fieldFilterEvaluator;
        private readonly JwtSecurityTokenHandler _jwtHandler;

        /// <summary>
        /// Initializes a new instance of the ConstraintEvaluator class.
        /// </summary>
        /// <param name="logger">The logger instance</param>
        /// <param name="jsonPathEvaluator">The JSON path evaluator</param>
        /// <param name="fieldFilterEvaluator">The field filter evaluator</param>
        public ConstraintEvaluator(
            ILogger<ConstraintEvaluator> logger,
            JsonPathEvaluator jsonPathEvaluator,
            FieldFilterEvaluator fieldFilterEvaluator) {
                _logger = logger ?? throw new ArgumentNullException(nameof(logger));
                _jsonPathEvaluator = jsonPathEvaluator ?? throw new ArgumentNullException(nameof(jsonPathEvaluator));
                _fieldFilterEvaluator = fieldFilterEvaluator ?? throw new ArgumentNullException(nameof(fieldFilterEvaluator));
                _jwtHandler = new JwtSecurityTokenHandler();
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
            CancellationToken cancellationToken = default) {
                try {
                        _logger.LogDebug("Starting constraint evaluation for credential");

                        var result = new ConstraintEvaluationResult();

                        // Convert credential to JsonDocument for evaluation
                        var credentialJson = ConvertToJsonDocument(credential);
                        if (credentialJson == null) {
                                result.AddError("INVALID_CREDENTIAL_FORMAT", "Credential could not be converted to JSON format");
                                return result;
                        }

                        // Evaluate field constraints if present
                        if (constraints.Fields != null && constraints.Fields.Length > 0) {
                                foreach (var field in constraints.Fields) {
                                        cancellationToken.ThrowIfCancellationRequested();

                                        var fieldResult = await EvaluateFieldConstraintAsync(credentialJson, field, cancellationToken);
                                        result.MergeFieldResult(field.GetPrimaryPath() ?? "unknown", fieldResult);
                                }
                        }

                        // Evaluate limit disclosure requirements
                        if (!string.IsNullOrEmpty(constraints.LimitDisclosure)) {
                                var limitDisclosureResult = EvaluateLimitDisclosure(credential, constraints.LimitDisclosure);
                                result.MergeDisclosureResult(limitDisclosureResult);
                        }

                        // Evaluate subject requirements
                        if (!string.IsNullOrEmpty(constraints.SubjectIsIssuer)) {
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
                catch (OperationCanceledException) {
                        _logger.LogWarning("Constraint evaluation was cancelled");
                        throw;
                }
                catch (Exception ex) {
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
            CancellationToken cancellationToken = default) {
                var result = new FieldEvaluationResult {
                        FieldId = field.Id ?? field.GetPrimaryPath(),
                        IsOptional = field.Optional
                };

                try {
                        // Evaluate each path until one succeeds or all fail
                        var pathFound = false;
                        JsonElement? fieldValue = null;
                        string? successfulPath = null;

                        if (field.Path != null) {
                                foreach (var path in field.Path) {
                                        cancellationToken.ThrowIfCancellationRequested();

                                        var pathResult = await _jsonPathEvaluator.EvaluateAsync(credentialJson, path, cancellationToken);
                                        if (pathResult.IsSuccessful && pathResult.Values.Any()) {
                                                pathFound = true;
                                                fieldValue = pathResult.Values.First();
                                                successfulPath = path;
                                                result.MatchedPath = path;
                                                break;
                                        }
                                }
                        }

                        // If field is required but not found, that's an error
                        if (!pathFound && !field.Optional) {
                                result.AddError("FIELD_NOT_FOUND", $"Required field not found at any specified path");
                                return result;
                        }

                        // If field is optional and not found, that's okay
                        if (!pathFound && field.Optional) {
                                result.IsSuccessful = true;
                                return result;
                        }

                        // If we have a filter, evaluate it
                        if (field.Filter != null && fieldValue.HasValue) {
                                var filterResult = await _fieldFilterEvaluator.EvaluateAsync(fieldValue.Value, field.Filter, cancellationToken);
                                if (!filterResult.IsSuccessful) {
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
                catch (Exception ex) {
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
        private DisclosureEvaluationResult EvaluateLimitDisclosure(object credential, string limitDisclosure) {
                var result = new DisclosureEvaluationResult();

                try {
                        // Check if the credential supports selective disclosure
                        var supportsSelectiveDisclosure = CheckSelectiveDisclosureSupport(credential);

                        if (limitDisclosure == "required" && !supportsSelectiveDisclosure) {
                                result.AddError("SELECTIVE_DISCLOSURE_REQUIRED", "Credential must support selective disclosure but doesn't");
                                return result;
                        }

                        if (limitDisclosure == "preferred" && !supportsSelectiveDisclosure) {
                                result.AddWarning("SELECTIVE_DISCLOSURE_PREFERRED", "Selective disclosure is preferred but not supported");
                        }

                        result.IsSuccessful = true;
                        result.SupportsSelectiveDisclosure = supportsSelectiveDisclosure;

                        return result;
                }
                catch (Exception ex) {
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
            CancellationToken cancellationToken = default) {
                var result = new SubjectEvaluationResult();

                try {
                        // Get issuer value
                        var issuerResult = await _jsonPathEvaluator.EvaluateAsync(credentialJson, "$.iss", cancellationToken);
                        if (!issuerResult.IsSuccessful || !issuerResult.Values.Any()) {
                                if (subjectIsIssuer == "required") {
                                        result.AddError("ISSUER_NOT_FOUND", "Issuer field is required but not found");
                                        return result;
                                }
                        }

                        // Get subject value
                        var subjectResult = await _jsonPathEvaluator.EvaluateAsync(credentialJson, "$.sub", cancellationToken);
                        if (!subjectResult.IsSuccessful || !subjectResult.Values.Any()) {
                                if (subjectIsIssuer == "required") {
                                        result.AddError("SUBJECT_NOT_FOUND", "Subject field is required but not found");
                                        return result;
                                }
                        }

                        // Compare issuer and subject
                        if (issuerResult.IsSuccessful && subjectResult.IsSuccessful) {
                                var issuerValue = issuerResult.Values.First().GetString();
                                var subjectValue = subjectResult.Values.First().GetString();

                                var areEqual = string.Equals(issuerValue, subjectValue, StringComparison.Ordinal);

                                if (subjectIsIssuer == "required" && !areEqual) {
                                        result.AddError("SUBJECT_ISSUER_MISMATCH", "Subject must equal issuer but they differ");
                                        return result;
                                }

                                if (subjectIsIssuer == "preferred" && !areEqual) {
                                        result.AddWarning("SUBJECT_ISSUER_PREFERRED", "Subject and issuer should be equal but they differ");
                                }

                                result.SubjectEqualsIssuer = areEqual;
                        }

                        result.IsSuccessful = true;
                        return result;
                }
                catch (Exception ex) {
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
        private JsonDocument? ConvertToJsonDocument(object credential) {
                try {
                        if (credential is JsonDocument jsonDoc)
                                return jsonDoc;

                        if (credential is string credentialString) {
                                _logger.LogDebug("Processing string credential of length: {Length}", credentialString.Length);

                                // Check if this is an SD-JWT or JWT
                                if (IsJwtFormat(credentialString)) {
                                        _logger.LogDebug("Detected JWT format credential");
                                        return ExtractPayloadFromJwt(credentialString);
                                }

                                _logger.LogDebug("Attempting to parse credential string as direct JSON");
                                // Try to parse as direct JSON
                                return JsonDocument.Parse(credentialString);
                        }

                        _logger.LogDebug("Serializing credential object of type: {Type}", credential.GetType().Name);
                        // Serialize object to JSON and parse back
                        var json = JsonSerializer.Serialize(credential);
                        return JsonDocument.Parse(json);
                }
                catch (Exception ex) {
                        _logger.LogError(ex, "Failed to convert credential to JsonDocument");
                        return null;
                }
        }

        /// <summary>
        /// Checks if a string is in JWT format.
        /// </summary>
        /// <param name="input">The string to check</param>
        /// <returns>True if the string appears to be a JWT</returns>
        private bool IsJwtFormat(string input) {
                if (string.IsNullOrWhiteSpace(input))
                        return false;

                // Extract the JWT part (before any ~ for SD-JWT)
                var jwtPart = input.Split('~')[0];

                // JWT format has exactly 2 dots (3 parts: header.payload.signature)
                var parts = jwtPart.Split('.');
                return parts.Length == 3 && parts.All(part => !string.IsNullOrWhiteSpace(part));
        }

        /// <summary>
        /// Extracts the payload from a JWT or SD-JWT string and returns it as JsonDocument.
        /// </summary>
        /// <param name="jwt">The JWT or SD-JWT string</param>
        /// <returns>JsonDocument of the payload or null if extraction fails</returns>
        private JsonDocument? ExtractPayloadFromJwt(string jwt) {
                try {
                        // Extract the JWT part (before any ~ for SD-JWT)
                        var jwtPart = jwt.Split('~')[0];

                        // Use JwtSecurityTokenHandler to read the token
                        var token = _jwtHandler.ReadJwtToken(jwtPart);

                        // Convert claims to a dictionary for JSON serialization
                        // Use the standard JWT claim names as keys
                        var payload = new Dictionary<string, object>();

                        foreach (var claim in token.Claims) {
                                var claimType = claim.Type;
                                var claimValue = claim.Value;

                                // Handle standard JWT claims that might be URIs
                                if (claimType.Contains("/")) {
                                        claimType = claimType.Split('/').Last();
                                }

                                if (payload.ContainsKey(claimType)) {
                                        // Handle multiple values for the same claim type
                                        if (payload[claimType] is List<object> list) {
                                                list.Add(GetClaimValue(claimValue));
                                        }
                                        else {
                                                var existingValue = payload[claimType];
                                                payload[claimType] = new List<object> { existingValue, GetClaimValue(claimValue) };
                                        }
                                }
                                else {
                                        payload[claimType] = GetClaimValue(claimValue);
                                }
                        }

                        // Convert to JSON and create JsonDocument
                        var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions {
                                PropertyNamingPolicy = null // Keep original property names
                        });
                        return JsonDocument.Parse(json);
                }
                catch (Exception ex) {
                        _logger.LogError(ex, "Failed to extract payload from JWT");
                        return null;
                }
        }

        /// <summary>
        /// Converts a claim value to the appropriate object type.
        /// </summary>
        /// <param name="claimValue">The claim value as string</param>
        /// <returns>The parsed claim value</returns>
        private object GetClaimValue(string claimValue) {
                // Try to parse as JSON for complex values
                if (TryParseAsJson(claimValue, out var jsonValue) && jsonValue != null) {
                        return jsonValue;
                }

                // Try to parse as number
                if (long.TryParse(claimValue, out var longValue)) {
                        return longValue;
                }

                if (double.TryParse(claimValue, out var doubleValue)) {
                        return doubleValue;
                }

                // Try to parse as boolean
                if (bool.TryParse(claimValue, out var boolValue)) {
                        return boolValue;
                }

                // Return as string
                return claimValue;
        }

        /// <summary>
        /// Tries to parse a string as JSON.
        /// </summary>
        /// <param name="input">The string to parse</param>
        /// <param name="result">The parsed JSON object</param>
        /// <returns>True if parsing succeeded</returns>
        private bool TryParseAsJson(string input, out object? result) {
                result = null;
                try {
                        using var jsonDoc = JsonDocument.Parse(input);
                        result = JsonElementToObject(jsonDoc.RootElement);
                        return true;
                }
                catch {
                        return false;
                }
        }

        /// <summary>
        /// Converts a JsonElement to a CLR object.
        /// </summary>
        /// <param name="element">The JsonElement to convert</param>
        /// <returns>The CLR object representation</returns>
        private object? JsonElementToObject(JsonElement element) {
                return element.ValueKind switch {
                        JsonValueKind.String => element.GetString(),
                        JsonValueKind.Number => element.TryGetInt64(out var longVal) ? longVal : element.GetDouble(),
                        JsonValueKind.True => true,
                        JsonValueKind.False => false,
                        JsonValueKind.Null => null,
                        JsonValueKind.Array => element.EnumerateArray().Select(JsonElementToObject).ToArray(),
                        JsonValueKind.Object => element.EnumerateObject().ToDictionary(p => p.Name, p => JsonElementToObject(p.Value)),
                        _ => element.GetRawText()
                };
        }

        /// <summary>
        /// Checks if a credential supports selective disclosure.
        /// </summary>
        /// <param name="credential">The credential to check</param>
        /// <returns>True if the credential supports selective disclosure</returns>
        private bool CheckSelectiveDisclosureSupport(object credential) {
                try {
                        // For SD-JWT credentials, check if they contain disclosure information
                        if (credential is string credentialString) {
                                // SD-JWT format contains tildes (~) separating disclosure parts
                                return credentialString.Contains("~");
                        }

                        // For structured credentials, check for SD-JWT specific fields
                        var json = JsonSerializer.Serialize(credential);
                        return json.Contains("_sd") || json.Contains("_sd_alg");
                }
                catch (Exception ex) {
                        _logger.LogError(ex, "Error checking selective disclosure support");
                        return false;
                }
        }
}