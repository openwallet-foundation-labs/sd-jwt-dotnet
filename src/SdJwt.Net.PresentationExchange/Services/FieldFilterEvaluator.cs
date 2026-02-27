using System.Text.Json;
using System.Text.RegularExpressions;
using System.Globalization;
using Microsoft.Extensions.Logging;
using SdJwt.Net.PresentationExchange.Models;

namespace SdJwt.Net.PresentationExchange.Services;

/// <summary>
/// Evaluates field filters against JSON values according to JSON Schema specification.
/// Provides comprehensive constraint validation for presentation exchange.
/// </summary>
public class FieldFilterEvaluator {
        private readonly ILogger<FieldFilterEvaluator> _logger;

        /// <summary>
        /// Initializes a new instance of the FieldFilterEvaluator class.
        /// </summary>
        /// <param name="logger">The logger instance</param>
        public FieldFilterEvaluator(ILogger<FieldFilterEvaluator> logger) {
                _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Evaluates a field filter against a JSON value.
        /// </summary>
        /// <param name="value">The JSON value to evaluate</param>
        /// <param name="filter">The field filter to apply</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A task that represents the filter evaluation result</returns>
        public async Task<FilterEvaluationResult> EvaluateAsync(
            JsonElement value,
            FieldFilter filter,
            CancellationToken cancellationToken = default) {
                var result = new FilterEvaluationResult();

                try {
                        _logger.LogDebug("Evaluating field filter against value of type: {ValueKind}", value.ValueKind);

                        var predicateFilter = TryGetPredicateFilter(filter);
                        if (predicateFilter != null) {
                                if (!await EvaluatePredicateConstraintAsync(value, predicateFilter, result, cancellationToken))
                                        return result;

                                result.IsSuccessful = true;
                                return result;
                        }

                        // Evaluate type constraint
                        if (!string.IsNullOrEmpty(filter.Type)) {
                                if (!await EvaluateTypeConstraintAsync(value, filter.Type, result, cancellationToken))
                                        return result;
                        }

                        // Evaluate const constraint
                        if (filter.Const != null) {
                                if (!await EvaluateConstConstraintAsync(value, filter.Const, result, cancellationToken))
                                        return result;
                        }

                        // Evaluate enum constraint
                        if (filter.Enum != null && filter.Enum.Length > 0) {
                                if (!await EvaluateEnumConstraintAsync(value, filter.Enum, result, cancellationToken))
                                        return result;
                        }

                        // Evaluate pattern constraint
                        if (!string.IsNullOrEmpty(filter.Pattern)) {
                                if (!await EvaluatePatternConstraintAsync(value, filter.Pattern, result, cancellationToken))
                                        return result;
                        }

                        // Evaluate string format constraints.
                        if (!string.IsNullOrEmpty(filter.Format)) {
                                if (!await EvaluateFormatConstraintAsync(value, filter.Format, result, cancellationToken))
                                        return result;
                        }

                        // Evaluate numeric constraints
                        if (!await EvaluateNumericConstraintsAsync(value, filter, result, cancellationToken))
                                return result;

                        // Evaluate string/array length constraints
                        if (!await EvaluateLengthConstraintsAsync(value, filter, result, cancellationToken))
                                return result;

                        // Evaluate array constraints
                        if (!await EvaluateArrayConstraintsAsync(value, filter, result, cancellationToken))
                                return result;

                        // Evaluate object constraints
                        if (!await EvaluateObjectConstraintsAsync(value, filter, result, cancellationToken))
                                return result;

                        result.IsSuccessful = true;
                        _logger.LogDebug("Field filter evaluation successful");

                        return result;
                }
                catch (OperationCanceledException) {
                        _logger.LogWarning("Field filter evaluation was cancelled");
                        throw;
                }
                catch (Exception ex) {
                        _logger.LogError(ex, "Error during field filter evaluation");
                        result.AddError("FILTER_EVALUATION_ERROR", $"Filter evaluation failed: {ex.Message}");
                        return result;
                }
        }

        /// <summary>
        /// Evaluates type constraint.
        /// </summary>
        private Task<bool> EvaluateTypeConstraintAsync(
            JsonElement value,
            string expectedType,
            FilterEvaluationResult result,
            CancellationToken cancellationToken) {
                var actualType = GetJsonSchemaType(value);
                var normalizedExpected = expectedType.Trim().ToLowerInvariant();

                if (normalizedExpected == "number" && (actualType == "number" || actualType == "integer")) {
                        return Task.FromResult(true);
                }

                if (actualType != normalizedExpected) {
                        result.AddError("TYPE_MISMATCH", $"Expected type '{normalizedExpected}' but got '{actualType}'");
                        return Task.FromResult(false);
                }

                return Task.FromResult(true);
        }

        /// <summary>
        /// Evaluates const constraint.
        /// </summary>
        private Task<bool> EvaluateConstConstraintAsync(
            JsonElement value,
            object expectedValue,
            FilterEvaluationResult result,
            CancellationToken cancellationToken) {
                if (!ValuesEqual(value, expectedValue)) {
                        result.AddError("CONST_MISMATCH", $"Value does not match expected constant");
                        return Task.FromResult(false);
                }

                return Task.FromResult(true);
        }

        /// <summary>
        /// Evaluates enum constraint.
        /// </summary>
        private Task<bool> EvaluateEnumConstraintAsync(
            JsonElement value,
            object[] allowedValues,
            FilterEvaluationResult result,
            CancellationToken cancellationToken) {
                foreach (var allowedValue in allowedValues) {
                        if (ValuesEqual(value, allowedValue))
                                return Task.FromResult(true);
                }

                result.AddError("ENUM_MISMATCH", "Value is not in the allowed enumeration");
                return Task.FromResult(false);
        }

        /// <summary>
        /// Evaluates pattern constraint for string values.
        /// </summary>
        private Task<bool> EvaluatePatternConstraintAsync(
            JsonElement value,
            string pattern,
            FilterEvaluationResult result,
            CancellationToken cancellationToken) {
                if (value.ValueKind != JsonValueKind.String) {
                        result.AddError("PATTERN_TYPE_MISMATCH", "Pattern constraint can only be applied to string values");
                        return Task.FromResult(false);
                }

                var stringValue = value.GetString();
                if (string.IsNullOrEmpty(stringValue)) {
                        result.AddError("PATTERN_NULL_VALUE", "Cannot apply pattern constraint to null or empty string");
                        return Task.FromResult(false);
                }

                try {
                        var regex = new Regex(pattern, RegexOptions.None, TimeSpan.FromSeconds(5));
                        if (!regex.IsMatch(stringValue)) {
                                result.AddError("PATTERN_MISMATCH", $"Value does not match pattern '{pattern}'");
                                return Task.FromResult(false);
                        }
                }
                catch (RegexMatchTimeoutException) {
                        result.AddError("PATTERN_TIMEOUT", "Pattern matching timed out");
                        return Task.FromResult(false);
                }
                catch (ArgumentException ex) {
                        result.AddError("PATTERN_INVALID", $"Invalid regex pattern: {ex.Message}");
                        return Task.FromResult(false);
                }

                return Task.FromResult(true);
        }

        /// <summary>
        /// Evaluates format constraint for string values.
        /// </summary>
        private Task<bool> EvaluateFormatConstraintAsync(
            JsonElement value,
            string format,
            FilterEvaluationResult result,
            CancellationToken cancellationToken) {
                if (value.ValueKind != JsonValueKind.String) {
                        result.AddError("FORMAT_TYPE_MISMATCH", "Format constraint can only be applied to string values");
                        return Task.FromResult(false);
                }

                var text = value.GetString() ?? string.Empty;
                var normalizedFormat = format.Trim().ToLowerInvariant();
                var valid = normalizedFormat switch
                {
                    "date-time" => DateTimeOffset.TryParse(
                        text,
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.RoundtripKind,
                        out _),
                    "date" => DateTime.TryParseExact(
                        text,
                        "yyyy-MM-dd",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out _),
                    "email" => Regex.IsMatch(text, @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.CultureInvariant),
                    "uri" => Uri.TryCreate(text, UriKind.Absolute, out _),
                    "uuid" => Guid.TryParse(text, out _),
                    _ => true
                };

                if (!valid) {
                        result.AddError("FORMAT_MISMATCH", $"Value does not satisfy format '{format}'");
                        return Task.FromResult(false);
                }

                return Task.FromResult(true);
        }

        /// <summary>
        /// Evaluates numeric constraints (minimum, maximum, etc.).
        /// </summary>
        private Task<bool> EvaluateNumericConstraintsAsync(
            JsonElement value,
            FieldFilter filter,
            FilterEvaluationResult result,
            CancellationToken cancellationToken) {
                if (value.ValueKind != JsonValueKind.Number)
                        return Task.FromResult(true); // Numeric constraints only apply to numbers

                var numericValue = value.GetDouble();

                if (string.Equals(filter.Type, "integer", StringComparison.OrdinalIgnoreCase) &&
                    !IsIntegerValue(value)) {
                        result.AddError("INTEGER_CONSTRAINT", "Value is not an integer");
                        return Task.FromResult(false);
                }

                // Minimum constraint
                if (filter.Minimum != null) {
                        if (!TryConvertToDouble(filter.Minimum, out var minimum)) {
                                result.AddError("MINIMUM_CONSTRAINT", "Minimum must be numeric");
                                return Task.FromResult(false);
                        }

                        if (numericValue < minimum) {
                                result.AddError("MINIMUM_CONSTRAINT", $"Value {numericValue} is less than minimum {minimum}");
                                return Task.FromResult(false);
                        }
                }

                // Maximum constraint
                if (filter.Maximum != null) {
                        if (!TryConvertToDouble(filter.Maximum, out var maximum)) {
                                result.AddError("MAXIMUM_CONSTRAINT", "Maximum must be numeric");
                                return Task.FromResult(false);
                        }

                        if (numericValue > maximum) {
                                result.AddError("MAXIMUM_CONSTRAINT", $"Value {numericValue} is greater than maximum {maximum}");
                                return Task.FromResult(false);
                        }
                }

                // Exclusive minimum constraint
                if (filter.ExclusiveMinimum != null) {
                        if (!TryConvertToDouble(filter.ExclusiveMinimum, out var exclusiveMinimum)) {
                                result.AddError("EXCLUSIVE_MINIMUM_CONSTRAINT", "ExclusiveMinimum must be numeric");
                                return Task.FromResult(false);
                        }

                        if (numericValue <= exclusiveMinimum) {
                                result.AddError("EXCLUSIVE_MINIMUM_CONSTRAINT", $"Value {numericValue} is not greater than exclusive minimum {exclusiveMinimum}");
                                return Task.FromResult(false);
                        }
                }

                // Exclusive maximum constraint
                if (filter.ExclusiveMaximum != null) {
                        if (!TryConvertToDouble(filter.ExclusiveMaximum, out var exclusiveMaximum)) {
                                result.AddError("EXCLUSIVE_MAXIMUM_CONSTRAINT", "ExclusiveMaximum must be numeric");
                                return Task.FromResult(false);
                        }

                        if (numericValue >= exclusiveMaximum) {
                                result.AddError("EXCLUSIVE_MAXIMUM_CONSTRAINT", $"Value {numericValue} is not less than exclusive maximum {exclusiveMaximum}");
                                return Task.FromResult(false);
                        }
                }

                if (filter.MultipleOf != null) {
                        if (!TryConvertToDouble(filter.MultipleOf, out var multipleOf) || multipleOf <= 0) {
                                result.AddError("MULTIPLE_OF_CONSTRAINT", "MultipleOf must be a positive number");
                                return Task.FromResult(false);
                        }

                        var remainder = numericValue / multipleOf;
                        if (Math.Abs(remainder - Math.Round(remainder)) > 1e-12) {
                                result.AddError("MULTIPLE_OF_CONSTRAINT", $"Value {numericValue} is not a multiple of {multipleOf}");
                                return Task.FromResult(false);
                        }
                }

                return Task.FromResult(true);
        }

        /// <summary>
        /// Evaluates length constraints for strings and arrays.
        /// </summary>
        private Task<bool> EvaluateLengthConstraintsAsync(
            JsonElement value,
            FieldFilter filter,
            FilterEvaluationResult result,
            CancellationToken cancellationToken) {
                int? length = null;

                if (value.ValueKind == JsonValueKind.String) {
                        length = value.GetString()?.Length ?? 0;
                }
                else if (value.ValueKind == JsonValueKind.Array) {
                        length = value.GetArrayLength();
                }
                else {
                        return Task.FromResult(true); // Length constraints only apply to strings and arrays
                }

                // MinLength constraint
                if (filter.MinLength.HasValue) {
                        if (length < filter.MinLength.Value) {
                                result.AddError("MIN_LENGTH_CONSTRAINT", $"Length {length} is less than minimum length {filter.MinLength.Value}");
                                return Task.FromResult(false);
                        }
                }

                // MaxLength constraint
                if (filter.MaxLength.HasValue) {
                        if (length > filter.MaxLength.Value) {
                                result.AddError("MAX_LENGTH_CONSTRAINT", $"Length {length} is greater than maximum length {filter.MaxLength.Value}");
                                return Task.FromResult(false);
                        }
                }

                return Task.FromResult(true);
        }

        /// <summary>
        /// Evaluates array-specific constraints.
        /// </summary>
        private async Task<bool> EvaluateArrayConstraintsAsync(
            JsonElement value,
            FieldFilter filter,
            FilterEvaluationResult result,
            CancellationToken cancellationToken) {
                if (value.ValueKind != JsonValueKind.Array)
                        return true; // Array constraints only apply to arrays

                var arrayLength = value.GetArrayLength();

                // MinItems constraint
                if (filter.MinItems.HasValue) {
                        if (arrayLength < filter.MinItems.Value) {
                                result.AddError("MIN_ITEMS_CONSTRAINT", $"Array has {arrayLength} items, less than minimum {filter.MinItems.Value}");
                                return false;
                        }
                }

                // MaxItems constraint
                if (filter.MaxItems.HasValue) {
                        if (arrayLength > filter.MaxItems.Value) {
                                result.AddError("MAX_ITEMS_CONSTRAINT", $"Array has {arrayLength} items, more than maximum {filter.MaxItems.Value}");
                                return false;
                        }
                }

                // UniqueItems constraint
                if (filter.UniqueItems == true) {
                        var items = value.EnumerateArray().ToArray();
                        var uniqueItems = items.Distinct(new JsonElementComparer()).Count();

                        if (uniqueItems != items.Length) {
                                result.AddError("UNIQUE_ITEMS_CONSTRAINT", "Array contains duplicate items but uniqueItems is required");
                                return false;
                        }
                }

                // Items schema constraint
                if (filter.Items != null && TryConvertToFieldFilter(filter.Items, out var itemFilter)) {
                        foreach (var item in value.EnumerateArray()) {
                                cancellationToken.ThrowIfCancellationRequested();
                                var itemResult = await EvaluateAsync(item, itemFilter!, cancellationToken);
                                if (!itemResult.IsSuccessful) {
                                        result.AddError("ITEMS_CONSTRAINT", "Array item does not satisfy 'items' schema");
                                        return false;
                                }
                        }
                }

                // Contains constraint
                if (filter.Contains != null) {
                        var containsFound = false;
                        var hasContainsSchema = TryConvertToFieldFilter(filter.Contains, out var containsFilter);
                        foreach (var item in value.EnumerateArray()) {
                                cancellationToken.ThrowIfCancellationRequested();

                                if (hasContainsSchema) {
                                        var containsResult = await EvaluateAsync(item, containsFilter!, cancellationToken);
                                        if (containsResult.IsSuccessful) {
                                                containsFound = true;
                                                break;
                                        }
                                }
                                else if (filter.Contains is JsonElement containsElement) {
                                        if (JsonElementComparer.Default.Equals(item, containsElement)) {
                                                containsFound = true;
                                                break;
                                        }
                                }
                                else if (ValuesEqual(item, filter.Contains)) {
                                        containsFound = true;
                                        break;
                                }
                        }

                        if (!containsFound) {
                                result.AddError("CONTAINS_CONSTRAINT", "Array does not contain required item");
                                return false;
                        }
                }

                return true;
        }

        /// <summary>
        /// Evaluates object-specific constraints.
        /// </summary>
        private async Task<bool> EvaluateObjectConstraintsAsync(
            JsonElement value,
            FieldFilter filter,
            FilterEvaluationResult result,
            CancellationToken cancellationToken) {
                if (value.ValueKind != JsonValueKind.Object)
                        return true; // Object constraints only apply to objects

                // Required properties constraint
                if (filter.Required != null && filter.Required.Length > 0) {
                        foreach (var requiredProperty in filter.Required) {
                                if (!value.TryGetProperty(requiredProperty, out _)) {
                                        result.AddError("REQUIRED_PROPERTY_MISSING", $"Required property '{requiredProperty}' is missing");
                                        return false;
                                }
                        }
                }

                var definedProperties = new HashSet<string>(StringComparer.Ordinal);

                // Properties constraint
                if (filter.Properties != null) {
                        foreach (var expectedProperty in filter.Properties) {
                                definedProperties.Add(expectedProperty.Key);
                                if (!value.TryGetProperty(expectedProperty.Key, out _)) {
                                        // Property is missing - check if it's required
                                        if (filter.Required?.Contains(expectedProperty.Key) == true) {
                                                result.AddError("PROPERTY_SCHEMA_VIOLATION", $"Property '{expectedProperty.Key}' violates schema");
                                                return false;
                                        }
                                        continue;
                                }

                                if (!TryConvertToFieldFilter(expectedProperty.Value, out var propertyFilter)) {
                                        continue;
                                }

                                var propertyValue = value.GetProperty(expectedProperty.Key);
                                var propertyResult = await EvaluateAsync(propertyValue, propertyFilter!, cancellationToken);
                                if (!propertyResult.IsSuccessful) {
                                        result.AddError("PROPERTY_SCHEMA_VIOLATION", $"Property '{expectedProperty.Key}' does not satisfy schema");
                                        return false;
                                }
                        }
                }

                if (filter.AdditionalProperties == false && filter.Properties != null) {
                        foreach (var property in value.EnumerateObject()) {
                                if (!definedProperties.Contains(property.Name)) {
                                        result.AddError("ADDITIONAL_PROPERTIES_CONSTRAINT", $"Additional property '{property.Name}' is not allowed");
                                        return false;
                                }
                        }
                }

                return true;
        }

        /// <summary>
        /// Gets the JSON Schema type for a JsonElement.
        /// </summary>
        private string GetJsonSchemaType(JsonElement element) {
                return element.ValueKind switch {
                        JsonValueKind.String => "string",
                        JsonValueKind.Number => IsIntegerValue(element) ? "integer" : "number",
                        JsonValueKind.True or JsonValueKind.False => "boolean",
                        JsonValueKind.Array => "array",
                        JsonValueKind.Object => "object",
                        JsonValueKind.Null => "null",
                        _ => "unknown"
                };
        }

        /// <summary>
        /// Compares a JsonElement with an object value for equality.
        /// </summary>
        private bool ValuesEqual(JsonElement jsonElement, object otherValue) {
                try {
                        if (otherValue is JsonElement otherElement) {
                                return JsonElementComparer.Default.Equals(jsonElement, otherElement);
                        }

                        if (otherValue is JsonDocument document) {
                                return JsonElementComparer.Default.Equals(jsonElement, document.RootElement);
                        }

                        return jsonElement.ValueKind switch {
                                JsonValueKind.String => jsonElement.GetString() == otherValue?.ToString(),
                                JsonValueKind.Number when otherValue is int intVal => jsonElement.GetInt32() == intVal,
                                JsonValueKind.Number when otherValue is long longVal => jsonElement.GetInt64() == longVal,
                                JsonValueKind.Number when otherValue is double doubleVal => Math.Abs(jsonElement.GetDouble() - doubleVal) < 1e-15,
                                JsonValueKind.Number when otherValue is float floatVal => Math.Abs(jsonElement.GetSingle() - floatVal) < 1e-7,
                                JsonValueKind.Number when otherValue is decimal decimalVal => Math.Abs(jsonElement.GetDouble() - (double)decimalVal) < 1e-15,
                                JsonValueKind.Number when TryConvertToDouble(otherValue, out var converted) => Math.Abs(jsonElement.GetDouble() - converted) < 1e-15,
                                JsonValueKind.Number => Math.Abs(jsonElement.GetDouble() - Convert.ToDouble(otherValue)) < 1e-15,
                                JsonValueKind.True => otherValue is true,
                                JsonValueKind.False => otherValue is false,
                                JsonValueKind.Array when otherValue is IEnumerable<object> objectArray =>
                                    JsonElementComparer.Default.Equals(
                                        jsonElement,
                                        JsonDocument.Parse(JsonSerializer.Serialize(objectArray)).RootElement),
                                JsonValueKind.Object when otherValue is IDictionary<string, object> objectMap =>
                                    JsonElementComparer.Default.Equals(
                                        jsonElement,
                                        JsonDocument.Parse(JsonSerializer.Serialize(objectMap)).RootElement),
                                JsonValueKind.Null => otherValue == null,
                                _ => false
                        };
                }
                catch {
                        return false;
                }
        }

        private async Task<bool> EvaluatePredicateConstraintAsync(
            JsonElement value,
            PredicateFilter predicateFilter,
            FilterEvaluationResult result,
            CancellationToken cancellationToken) {
                var predicate = predicateFilter.Predicate?.Trim().ToLowerInvariant();
                if (string.IsNullOrWhiteSpace(predicate)) {
                        result.AddError("PREDICATE_MISSING", "Predicate value is required");
                        return false;
                }

                switch (predicate) {
                        case "age_over":
                        case "greater_than":
                                if (!TryGetComparableNumber(value, out var gtValue) ||
                                    !TryConvertToDouble(predicateFilter.Threshold, out var gtThreshold) ||
                                    gtValue <= gtThreshold) {
                                        result.AddError("PREDICATE_FAILED", $"Predicate '{predicate}' failed");
                                        return false;
                                }
                                return true;
                        case "greater_than_or_equal":
                        case "is_adult":
                                var gteThreshold = predicate == "is_adult" ? 18d : (TryConvertToDouble(predicateFilter.Threshold, out var resolved) ? resolved : double.NaN);
                                if (!TryGetComparableNumber(value, out var gteValue) || double.IsNaN(gteThreshold) || gteValue < gteThreshold) {
                                        result.AddError("PREDICATE_FAILED", $"Predicate '{predicate}' failed");
                                        return false;
                                }
                                return true;
                        case "less_than":
                                if (!TryGetComparableNumber(value, out var ltValue) ||
                                    !TryConvertToDouble(predicateFilter.Threshold, out var ltThreshold) ||
                                    ltValue >= ltThreshold) {
                                        result.AddError("PREDICATE_FAILED", $"Predicate '{predicate}' failed");
                                        return false;
                                }
                                return true;
                        case "less_than_or_equal":
                                if (!TryGetComparableNumber(value, out var lteValue) ||
                                    !TryConvertToDouble(predicateFilter.Threshold, out var lteThreshold) ||
                                    lteValue > lteThreshold) {
                                        result.AddError("PREDICATE_FAILED", $"Predicate '{predicate}' failed");
                                        return false;
                                }
                                return true;
                        case "equal_to":
                                if (predicateFilter.Threshold == null || !ValuesEqual(value, predicateFilter.Threshold)) {
                                        result.AddError("PREDICATE_FAILED", "Predicate 'equal_to' failed");
                                        return false;
                                }
                                return true;
                        case "not_equal_to":
                                if (predicateFilter.Threshold == null || ValuesEqual(value, predicateFilter.Threshold)) {
                                        result.AddError("PREDICATE_FAILED", "Predicate 'not_equal_to' failed");
                                        return false;
                                }
                                return true;
                        case "in_range":
                                if (predicateFilter.Range == null ||
                                    predicateFilter.Range.Length != 2 ||
                                    !TryGetComparableNumber(value, out var rangeValue) ||
                                    !TryConvertToDouble(predicateFilter.Range[0], out var rangeMin) ||
                                    !TryConvertToDouble(predicateFilter.Range[1], out var rangeMax) ||
                                    rangeValue < rangeMin ||
                                    rangeValue > rangeMax) {
                                        result.AddError("PREDICATE_FAILED", "Predicate 'in_range' failed");
                                        return false;
                                }
                                return true;
                        case "in_set":
                                if (predicateFilter.Enum == null || !predicateFilter.Enum.Any(item => ValuesEqual(value, item))) {
                                        result.AddError("PREDICATE_FAILED", "Predicate 'in_set' failed");
                                        return false;
                                }
                                return true;
                        case "not_in_set":
                                if (predicateFilter.Enum == null || predicateFilter.Enum.Any(item => ValuesEqual(value, item))) {
                                        result.AddError("PREDICATE_FAILED", "Predicate 'not_in_set' failed");
                                        return false;
                                }
                                return true;
                        case "is_citizen":
                                if (value.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(value.GetString())) {
                                        return true;
                                }
                                if (value.ValueKind == JsonValueKind.True) {
                                        return true;
                                }
                                result.AddError("PREDICATE_FAILED", "Predicate 'is_citizen' failed");
                                return false;
                        default:
                                result.AddError("PREDICATE_UNSUPPORTED", $"Unsupported predicate '{predicate}'");
                                return false;
                }
        }

        private static PredicateFilter? TryGetPredicateFilter(FieldFilter filter) {
                if (filter is PredicateFilter typedFilter) {
                        return typedFilter;
                }

                string? predicate = null;
                object? threshold = null;
                object[]? range = null;
                bool zeroKnowledge = false;
                string? proofType = null;

                if (filter.ExtensionData != null) {
                        if (filter.ExtensionData.TryGetValue("predicate", out var predicateObj)) {
                                predicate = ConvertToString(predicateObj);
                        }

                        if (filter.ExtensionData.TryGetValue("threshold", out var thresholdObj)) {
                                threshold = ConvertToObject(thresholdObj);
                        }

                        if (filter.ExtensionData.TryGetValue("range", out var rangeObj)) {
                                range = ConvertToObjectArray(rangeObj);
                        }

                        if (filter.ExtensionData.TryGetValue("proof_type", out var proofTypeObj)) {
                                proofType = ConvertToString(proofTypeObj);
                        }

                        if (filter.ExtensionData.TryGetValue("zero_knowledge", out var zkObj) &&
                            TryConvertToBool(zkObj, out var zk)) {
                                zeroKnowledge = zk;
                        }
                }

                var isPredicateType = string.Equals(filter.Type, "predicate", StringComparison.OrdinalIgnoreCase);
                if (!isPredicateType && string.IsNullOrWhiteSpace(predicate)) {
                        return null;
                }

                return new PredicateFilter {
                        Type = filter.Type,
                        Predicate = predicate,
                        Threshold = threshold,
                        Range = range,
                        Enum = filter.Enum,
                        ProofType = proofType,
                        ZeroKnowledge = zeroKnowledge
                };
        }

        private static bool TryConvertToFieldFilter(object value, out FieldFilter? filter) {
                filter = null;

                if (value is FieldFilter fieldFilter) {
                        filter = fieldFilter;
                        return true;
                }

                try {
                        var json = value switch
                        {
                            JsonElement element => element.GetRawText(),
                            _ => JsonSerializer.Serialize(value)
                        };

                        filter = JsonSerializer.Deserialize<FieldFilter>(json);
                        if (filter == null) {
                                return false;
                        }

                        var predicate = JsonSerializer.Deserialize<PredicateFilter>(json);
                        if (!string.IsNullOrWhiteSpace(predicate?.Predicate) ||
                            string.Equals(predicate?.Type, "predicate", StringComparison.OrdinalIgnoreCase)) {
                                filter = predicate;
                        }

                        return true;
                }
                catch {
                        return false;
                }
        }

        private static bool IsIntegerValue(JsonElement value) {
                if (value.ValueKind != JsonValueKind.Number) {
                        return false;
                }

                return value.TryGetInt32(out _) || value.TryGetInt64(out _);
        }

        private static bool TryGetComparableNumber(JsonElement value, out double number) {
                number = 0;

                if (value.ValueKind == JsonValueKind.Number) {
                        number = value.GetDouble();
                        return true;
                }

                if (value.ValueKind == JsonValueKind.String &&
                    DateTime.TryParseExact(
                        value.GetString(),
                        "yyyy-MM-dd",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out var date)) {
                        var today = DateTime.UtcNow.Date;
                        number = today.Year - date.Year;
                        if (today < date.AddYears((int)number).Date) {
                                number--;
                        }
                        return true;
                }

                if (value.ValueKind == JsonValueKind.String &&
                    double.TryParse(value.GetString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed)) {
                        number = parsed;
                        return true;
                }

                return false;
        }

        private static bool TryConvertToDouble(object? value, out double number) {
                number = 0;

                switch (value) {
                        case null:
                                return false;
                        case double d:
                                number = d;
                                return true;
                        case float f:
                                number = f;
                                return true;
                        case decimal m:
                                number = (double)m;
                                return true;
                        case int i:
                                number = i;
                                return true;
                        case long l:
                                number = l;
                                return true;
                        case JsonElement element when element.ValueKind == JsonValueKind.Number:
                                number = element.GetDouble();
                                return true;
                        case JsonElement element when element.ValueKind == JsonValueKind.String:
                                return double.TryParse(element.GetString(), NumberStyles.Float, CultureInfo.InvariantCulture, out number);
                        default:
                                return double.TryParse(value.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out number);
                }
        }

        private static bool TryConvertToBool(object value, out bool result) {
                result = false;

                return value switch
                {
                    bool b => (result = b) || true,
                    string s when bool.TryParse(s, out var parsed) => (result = parsed) || true,
                    JsonElement trueElement when trueElement.ValueKind == JsonValueKind.True => (result = true) || true,
                    JsonElement falseElement when falseElement.ValueKind == JsonValueKind.False => true,
                    JsonElement textElement when textElement.ValueKind == JsonValueKind.String &&
                                                  bool.TryParse(textElement.GetString(), out var parsedText) => (result = parsedText) || true,
                    _ => false
                };
        }

        private static string? ConvertToString(object? value) {
                return value switch
                {
                    null => null,
                    string text => text,
                    JsonElement element when element.ValueKind == JsonValueKind.String => element.GetString(),
                    _ => value.ToString()
                };
        }

        private static object? ConvertToObject(object? value) {
                return value switch
                {
                    null => null,
                    JsonElement element => ConvertJsonElementToObject(element),
                    _ => value
                };
        }

        private static object[]? ConvertToObjectArray(object? value) {
                return value switch
                {
                    null => null,
                    object[] array => array,
                    JsonElement element when element.ValueKind == JsonValueKind.Array =>
                        element
                            .EnumerateArray()
                            .Select(ConvertJsonElementToObject)
                            .Select(item => item ?? string.Empty)
                            .Cast<object>()
                            .ToArray(),
                    _ => null
                };
        }

        private static object? ConvertJsonElementToObject(JsonElement element) {
                return element.ValueKind switch
                {
                    JsonValueKind.String => element.GetString(),
                    JsonValueKind.Number => element.TryGetInt64(out var l) ? l : element.GetDouble(),
                    JsonValueKind.True => true,
                    JsonValueKind.False => false,
                    JsonValueKind.Null => null,
                    JsonValueKind.Array => element.EnumerateArray().Select(ConvertJsonElementToObject).ToArray(),
                    JsonValueKind.Object => element.EnumerateObject().ToDictionary(p => p.Name, p => ConvertJsonElementToObject(p.Value)),
                    _ => element.GetRawText()
                };
        }
}

/// <summary>
/// Comparer for JsonElement values.
/// </summary>
public class JsonElementComparer : IEqualityComparer<JsonElement> {
        /// <summary>
        /// Default instance of the JsonElementComparer.
        /// </summary>
        public static readonly JsonElementComparer Default = new();

        /// <summary>
        /// Determines whether the specified JsonElement objects are equal.
        /// </summary>
        /// <param name="x">The first object to compare.</param>
        /// <param name="y">The second object to compare.</param>
        /// <returns>true if the specified objects are equal; otherwise, false.</returns>
        public bool Equals(JsonElement x, JsonElement y) {
                if (x.ValueKind != y.ValueKind)
                        return false;

                return x.ValueKind switch {
                        JsonValueKind.String => x.GetString() == y.GetString(),
                        JsonValueKind.Number => Math.Abs(x.GetDouble() - y.GetDouble()) < 1e-15,
                        JsonValueKind.True or JsonValueKind.False => true, // Same kind means same value
                        JsonValueKind.Null => true,
                        JsonValueKind.Array => ArraysEqual(x, y),
                        JsonValueKind.Object => ObjectsEqual(x, y),
                        _ => false
                };
        }

        private bool ArraysEqual(JsonElement x, JsonElement y) {
                if (x.GetArrayLength() != y.GetArrayLength())
                        return false;

                var xArray = x.EnumerateArray().ToArray();
                var yArray = y.EnumerateArray().ToArray();

                for (int i = 0; i < xArray.Length; i++) {
                        if (!Equals(xArray[i], yArray[i]))
                                return false;
                }

                return true;
        }

        private bool ObjectsEqual(JsonElement x, JsonElement y) {
                var xProperties = x.EnumerateObject().ToDictionary(p => p.Name, p => p.Value);
                var yProperties = y.EnumerateObject().ToDictionary(p => p.Name, p => p.Value);

                if (xProperties.Count != yProperties.Count)
                        return false;

                foreach (var kvp in xProperties) {
                        if (!yProperties.TryGetValue(kvp.Key, out var yValue) || !Equals(kvp.Value, yValue))
                                return false;
                }

                return true;
        }

        /// <summary>
        /// Returns a hash code for the specified JsonElement.
        /// </summary>
        /// <param name="obj">The JsonElement for which a hash code is to be returned.</param>
        /// <returns>A hash code for the specified object.</returns>
        public int GetHashCode(JsonElement obj) {
                // Simple hash code implementation
                return obj.ValueKind.GetHashCode();
        }
}

/// <summary>
/// Represents the result of field filter evaluation.
/// </summary>
public class FilterEvaluationResult {
        /// <summary>
        /// Gets or sets whether the filter evaluation was successful.
        /// </summary>
        public bool IsSuccessful { get; set; }

        /// <summary>
        /// Gets or sets any errors that occurred during evaluation.
        /// </summary>
        public List<string> Errors { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets additional evaluation details.
        /// </summary>
        public Dictionary<string, object> Details { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Adds an error to the result.
        /// </summary>
        /// <param name="errorCode">The error code</param>
        /// <param name="message">The error message</param>
        public void AddError(string errorCode, string message) {
                Errors.Add($"{errorCode}: {message}");
                IsSuccessful = false;
        }
}
