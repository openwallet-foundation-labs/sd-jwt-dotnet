using System.Text.Json;

namespace SdJwt.Net.PresentationExchange.Services;

/// <summary>
/// Represents the result of constraint evaluation against a credential.
/// </summary>
public class ConstraintEvaluationResult {
        /// <summary>
        /// Gets or sets whether the constraint evaluation was successful.
        /// </summary>
        public bool IsSuccessful { get; set; }

        /// <summary>
        /// Gets or sets the results of individual field constraint evaluations.
        /// Key is the field path, value is the evaluation result.
        /// </summary>
        public Dictionary<string, FieldEvaluationResult> FieldResults { get; set; } = new();

        /// <summary>
        /// Gets or sets any errors that occurred during evaluation.
        /// </summary>
        public List<ConstraintEvaluationError> Errors { get; set; } = new();

        /// <summary>
        /// Gets or sets any warnings generated during evaluation.
        /// </summary>
        public List<ConstraintEvaluationWarning> Warnings { get; set; } = new();

        /// <summary>
        /// Gets or sets the result of limit disclosure evaluation.
        /// </summary>
        public DisclosureEvaluationResult? DisclosureResult { get; set; }

        /// <summary>
        /// Gets or sets the result of subject evaluation.
        /// </summary>
        public SubjectEvaluationResult? SubjectResult { get; set; }

        /// <summary>
        /// Gets or sets additional evaluation metadata.
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new();

        /// <summary>
        /// Adds an error to the evaluation result.
        /// </summary>
        /// <param name="errorCode">The error code</param>
        /// <param name="message">The error message</param>
        /// <param name="fieldPath">Optional field path associated with the error</param>
        public void AddError(string errorCode, string message, string? fieldPath = null) {
                var error = new ConstraintEvaluationError {
                        Code = errorCode,
                        Message = message,
                        FieldPath = fieldPath
                };

                Errors.Add(error);
                IsSuccessful = false;
        }

        /// <summary>
        /// Adds a warning to the evaluation result.
        /// </summary>
        /// <param name="warningCode">The warning code</param>
        /// <param name="message">The warning message</param>
        /// <param name="fieldPath">Optional field path associated with the warning</param>
        public void AddWarning(string warningCode, string message, string? fieldPath = null) {
                var warning = new ConstraintEvaluationWarning {
                        Code = warningCode,
                        Message = message,
                        FieldPath = fieldPath
                };

                Warnings.Add(warning);
        }

        /// <summary>
        /// Merges a field evaluation result into this constraint evaluation result.
        /// </summary>
        /// <param name="fieldPath">The field path</param>
        /// <param name="fieldResult">The field evaluation result</param>
        public void MergeFieldResult(string fieldPath, FieldEvaluationResult fieldResult) {
                FieldResults[fieldPath] = fieldResult;

                if (!fieldResult.IsSuccessful) {
                        foreach (var error in fieldResult.Errors) {
                                AddError(error.Code, error.Message, fieldPath);
                        }
                }

                foreach (var warning in fieldResult.Warnings) {
                        AddWarning(warning.Code, warning.Message, fieldPath);
                }
        }

        /// <summary>
        /// Merges a disclosure evaluation result into this constraint evaluation result.
        /// </summary>
        /// <param name="disclosureResult">The disclosure evaluation result</param>
        public void MergeDisclosureResult(DisclosureEvaluationResult disclosureResult) {
                DisclosureResult = disclosureResult;

                foreach (var error in disclosureResult.Errors) {
                        AddError(error.Code, error.Message);
                }

                foreach (var warning in disclosureResult.Warnings) {
                        AddWarning(warning.Code, warning.Message);
                }
        }

        /// <summary>
        /// Merges a subject evaluation result into this constraint evaluation result.
        /// </summary>
        /// <param name="subjectResult">The subject evaluation result</param>
        public void MergeSubjectResult(SubjectEvaluationResult subjectResult) {
                SubjectResult = subjectResult;

                foreach (var error in subjectResult.Errors) {
                        AddError(error.Code, error.Message);
                }

                foreach (var warning in subjectResult.Warnings) {
                        AddWarning(warning.Code, warning.Message);
                }
        }

        /// <summary>
        /// Gets all satisfied field paths.
        /// </summary>
        /// <returns>Array of field paths that were successfully evaluated</returns>
        public string[] GetSatisfiedFieldPaths() {
                return FieldResults
                    .Where(kvp => kvp.Value.IsSuccessful)
                    .Select(kvp => kvp.Key)
                    .ToArray();
        }

        /// <summary>
        /// Gets all unsatisfied field paths.
        /// </summary>
        /// <returns>Array of field paths that failed evaluation</returns>
        public string[] GetUnsatisfiedFieldPaths() {
                return FieldResults
                    .Where(kvp => !kvp.Value.IsSuccessful)
                    .Select(kvp => kvp.Key)
                    .ToArray();
        }

        /// <summary>
        /// Checks if all required fields were satisfied.
        /// </summary>
        /// <returns>True if all required fields were satisfied</returns>
        public bool AreAllRequiredFieldsSatisfied() {
                return FieldResults
                    .Where(kvp => !kvp.Value.IsOptional)
                    .All(kvp => kvp.Value.IsSuccessful);
        }
}

/// <summary>
/// Represents the result of evaluating a single field constraint.
/// </summary>
public class FieldEvaluationResult {
        /// <summary>
        /// Gets or sets the field identifier.
        /// </summary>
        public string? FieldId { get; set; }

        /// <summary>
        /// Gets or sets whether the field evaluation was successful.
        /// </summary>
        public bool IsSuccessful { get; set; }

        /// <summary>
        /// Gets or sets whether the field is optional.
        /// </summary>
        public bool IsOptional { get; set; }

        /// <summary>
        /// Gets or sets the path that matched the field requirement.
        /// </summary>
        public string? MatchedPath { get; set; }

        /// <summary>
        /// Gets or sets the field value that was found.
        /// </summary>
        public JsonElement? FieldValue { get; set; }

        /// <summary>
        /// Gets or sets the result of filter evaluation if applicable.
        /// </summary>
        public FilterEvaluationResult? FilterDetails { get; set; }

        /// <summary>
        /// Gets or sets any errors that occurred during field evaluation.
        /// </summary>
        public List<FieldEvaluationError> Errors { get; set; } = new();

        /// <summary>
        /// Gets or sets any warnings generated during field evaluation.
        /// </summary>
        public List<FieldEvaluationWarning> Warnings { get; set; } = new();

        /// <summary>
        /// Adds an error to the field evaluation result.
        /// </summary>
        /// <param name="errorCode">The error code</param>
        /// <param name="message">The error message</param>
        public void AddError(string errorCode, string message) {
                var error = new FieldEvaluationError {
                        Code = errorCode,
                        Message = message
                };

                Errors.Add(error);
                IsSuccessful = false;
        }

        /// <summary>
        /// Adds a warning to the field evaluation result.
        /// </summary>
        /// <param name="warningCode">The warning code</param>
        /// <param name="message">The warning message</param>
        public void AddWarning(string warningCode, string message) {
                var warning = new FieldEvaluationWarning {
                        Code = warningCode,
                        Message = message
                };

                Warnings.Add(warning);
        }

        /// <summary>
        /// Gets the field value as a string if possible.
        /// </summary>
        /// <returns>The string value or null if not a string or not available</returns>
        public string? GetStringValue() {
                if (FieldValue?.ValueKind == JsonValueKind.String)
                        return FieldValue.Value.GetString();

                return null;
        }

        /// <summary>
        /// Gets the field value as a number if possible.
        /// </summary>
        /// <returns>The numeric value or null if not a number or not available</returns>
        public double? GetNumericValue() {
                if (FieldValue?.ValueKind == JsonValueKind.Number)
                        return FieldValue.Value.GetDouble();

                return null;
        }
}

/// <summary>
/// Represents the result of evaluating disclosure requirements.
/// </summary>
public class DisclosureEvaluationResult {
        /// <summary>
        /// Gets or sets whether the disclosure evaluation was successful.
        /// </summary>
        public bool IsSuccessful { get; set; } = true;

        /// <summary>
        /// Gets or sets whether the credential supports selective disclosure.
        /// </summary>
        public bool SupportsSelectiveDisclosure { get; set; }

        /// <summary>
        /// Gets or sets any errors that occurred during disclosure evaluation.
        /// </summary>
        public List<DisclosureEvaluationError> Errors { get; set; } = new();

        /// <summary>
        /// Gets or sets any warnings generated during disclosure evaluation.
        /// </summary>
        public List<DisclosureEvaluationWarning> Warnings { get; set; } = new();

        /// <summary>
        /// Adds an error to the disclosure evaluation result.
        /// </summary>
        /// <param name="errorCode">The error code</param>
        /// <param name="message">The error message</param>
        public void AddError(string errorCode, string message) {
                var error = new DisclosureEvaluationError {
                        Code = errorCode,
                        Message = message
                };

                Errors.Add(error);
                IsSuccessful = false;
        }

        /// <summary>
        /// Adds a warning to the disclosure evaluation result.
        /// </summary>
        /// <param name="warningCode">The warning code</param>
        /// <param name="message">The warning message</param>
        public void AddWarning(string warningCode, string message) {
                var warning = new DisclosureEvaluationWarning {
                        Code = warningCode,
                        Message = message
                };

                Warnings.Add(warning);
        }
}

/// <summary>
/// Represents the result of evaluating subject requirements.
/// </summary>
public class SubjectEvaluationResult {
        /// <summary>
        /// Gets or sets whether the subject evaluation was successful.
        /// </summary>
        public bool IsSuccessful { get; set; } = true;

        /// <summary>
        /// Gets or sets whether the subject equals the issuer.
        /// </summary>
        public bool? SubjectEqualsIssuer { get; set; }

        /// <summary>
        /// Gets or sets any errors that occurred during subject evaluation.
        /// </summary>
        public List<SubjectEvaluationError> Errors { get; set; } = new();

        /// <summary>
        /// Gets or sets any warnings generated during subject evaluation.
        /// </summary>
        public List<SubjectEvaluationWarning> Warnings { get; set; } = new();

        /// <summary>
        /// Adds an error to the subject evaluation result.
        /// </summary>
        /// <param name="errorCode">The error code</param>
        /// <param name="message">The error message</param>
        public void AddError(string errorCode, string message) {
                var error = new SubjectEvaluationError {
                        Code = errorCode,
                        Message = message
                };

                Errors.Add(error);
                IsSuccessful = false;
        }

        /// <summary>
        /// Adds a warning to the subject evaluation result.
        /// </summary>
        /// <param name="warningCode">The warning code</param>
        /// <param name="message">The warning message</param>
        public void AddWarning(string warningCode, string message) {
                var warning = new SubjectEvaluationWarning {
                        Code = warningCode,
                        Message = message
                };

                Warnings.Add(warning);
        }
}

/// <summary>
/// Represents an error that occurred during constraint evaluation.
/// </summary>
public class ConstraintEvaluationError {
        /// <summary>
        /// Gets or sets the error code.
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the field path where the error occurred.
        /// </summary>
        public string? FieldPath { get; set; }
}

/// <summary>
/// Represents a warning that occurred during constraint evaluation.
/// </summary>
public class ConstraintEvaluationWarning {
        /// <summary>
        /// Gets or sets the warning code.
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the warning message.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the field path where the warning occurred.
        /// </summary>
        public string? FieldPath { get; set; }
}

/// <summary>
/// Represents an error that occurred during field evaluation.
/// </summary>
public class FieldEvaluationError {
        /// <summary>
        /// Gets or sets the error code.
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Represents a warning that occurred during field evaluation.
/// </summary>
public class FieldEvaluationWarning {
        /// <summary>
        /// Gets or sets the warning code.
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the warning message.
        /// </summary>
        public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Represents an error that occurred during disclosure evaluation.
/// </summary>
public class DisclosureEvaluationError {
        /// <summary>
        /// Gets or sets the error code.
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Represents a warning that occurred during disclosure evaluation.
/// </summary>
public class DisclosureEvaluationWarning {
        /// <summary>
        /// Gets or sets the warning code.
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the warning message.
        /// </summary>
        public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Represents an error that occurred during subject evaluation.
/// </summary>
public class SubjectEvaluationError {
        /// <summary>
        /// Gets or sets the error code.
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Represents a warning that occurred during subject evaluation.
/// </summary>
public class SubjectEvaluationWarning {
        /// <summary>
        /// Gets or sets the warning code.
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the warning message.
        /// </summary>
        public string Message { get; set; } = string.Empty;
}