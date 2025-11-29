using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using SdJwt.Net.PresentationExchange.Models;

namespace SdJwt.Net.PresentationExchange.Services;

/// <summary>
/// Evaluates field filters against JSON values according to JSON Schema specification.
/// Provides comprehensive constraint validation for presentation exchange.
/// </summary>
public class FieldFilterEvaluator
{
    private readonly ILogger<FieldFilterEvaluator> _logger;

    /// <summary>
    /// Initializes a new instance of the FieldFilterEvaluator class.
    /// </summary>
    /// <param name="logger">The logger instance</param>
    public FieldFilterEvaluator(ILogger<FieldFilterEvaluator> logger)
    {
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
        CancellationToken cancellationToken = default)
    {
        var result = new FilterEvaluationResult();

        try
        {
            _logger.LogDebug("Evaluating field filter against value of type: {ValueKind}", value.ValueKind);

            // Evaluate type constraint
            if (!string.IsNullOrEmpty(filter.Type))
            {
                if (!await EvaluateTypeConstraintAsync(value, filter.Type, result, cancellationToken))
                    return result;
            }

            // Evaluate const constraint
            if (filter.Const != null)
            {
                if (!await EvaluateConstConstraintAsync(value, filter.Const, result, cancellationToken))
                    return result;
            }

            // Evaluate enum constraint
            if (filter.Enum != null && filter.Enum.Length > 0)
            {
                if (!await EvaluateEnumConstraintAsync(value, filter.Enum, result, cancellationToken))
                    return result;
            }

            // Evaluate pattern constraint
            if (!string.IsNullOrEmpty(filter.Pattern))
            {
                if (!await EvaluatePatternConstraintAsync(value, filter.Pattern, result, cancellationToken))
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
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Field filter evaluation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during field filter evaluation");
            result.AddError("FILTER_EVALUATION_ERROR", $"Filter evaluation failed: {ex.Message}");
            return result;
        }
    }

    /// <summary>
    /// Evaluates type constraint.
    /// </summary>
    private async Task<bool> EvaluateTypeConstraintAsync(
        JsonElement value,
        string expectedType,
        FilterEvaluationResult result,
        CancellationToken cancellationToken)
    {
        var actualType = GetJsonSchemaType(value);

        if (actualType != expectedType)
        {
            result.AddError("TYPE_MISMATCH", $"Expected type '{expectedType}' but got '{actualType}'");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Evaluates const constraint.
    /// </summary>
    private async Task<bool> EvaluateConstConstraintAsync(
        JsonElement value,
        object expectedValue,
        FilterEvaluationResult result,
        CancellationToken cancellationToken)
    {
        if (!ValuesEqual(value, expectedValue))
        {
            result.AddError("CONST_MISMATCH", $"Value does not match expected constant");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Evaluates enum constraint.
    /// </summary>
    private async Task<bool> EvaluateEnumConstraintAsync(
        JsonElement value,
        object[] allowedValues,
        FilterEvaluationResult result,
        CancellationToken cancellationToken)
    {
        foreach (var allowedValue in allowedValues)
        {
            if (ValuesEqual(value, allowedValue))
                return true;
        }

        result.AddError("ENUM_MISMATCH", "Value is not in the allowed enumeration");
        return false;
    }

    /// <summary>
    /// Evaluates pattern constraint for string values.
    /// </summary>
    private async Task<bool> EvaluatePatternConstraintAsync(
        JsonElement value,
        string pattern,
        FilterEvaluationResult result,
        CancellationToken cancellationToken)
    {
        if (value.ValueKind != JsonValueKind.String)
        {
            result.AddError("PATTERN_TYPE_MISMATCH", "Pattern constraint can only be applied to string values");
            return false;
        }

        var stringValue = value.GetString();
        if (string.IsNullOrEmpty(stringValue))
        {
            result.AddError("PATTERN_NULL_VALUE", "Cannot apply pattern constraint to null or empty string");
            return false;
        }

        try
        {
            var regex = new Regex(pattern, RegexOptions.None, TimeSpan.FromSeconds(5));
            if (!regex.IsMatch(stringValue))
            {
                result.AddError("PATTERN_MISMATCH", $"Value does not match pattern '{pattern}'");
                return false;
            }
        }
        catch (RegexMatchTimeoutException)
        {
            result.AddError("PATTERN_TIMEOUT", "Pattern matching timed out");
            return false;
        }
        catch (ArgumentException ex)
        {
            result.AddError("PATTERN_INVALID", $"Invalid regex pattern: {ex.Message}");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Evaluates numeric constraints (minimum, maximum, etc.).
    /// </summary>
    private async Task<bool> EvaluateNumericConstraintsAsync(
        JsonElement value,
        FieldFilter filter,
        FilterEvaluationResult result,
        CancellationToken cancellationToken)
    {
        if (value.ValueKind != JsonValueKind.Number)
            return true; // Numeric constraints only apply to numbers

        var numericValue = value.GetDouble();

        // Minimum constraint
        if (filter.Minimum != null)
        {
            var minimum = Convert.ToDouble(filter.Minimum);
            if (numericValue < minimum)
            {
                result.AddError("MINIMUM_CONSTRAINT", $"Value {numericValue} is less than minimum {minimum}");
                return false;
            }
        }

        // Maximum constraint
        if (filter.Maximum != null)
        {
            var maximum = Convert.ToDouble(filter.Maximum);
            if (numericValue > maximum)
            {
                result.AddError("MAXIMUM_CONSTRAINT", $"Value {numericValue} is greater than maximum {maximum}");
                return false;
            }
        }

        // Exclusive minimum constraint
        if (filter.ExclusiveMinimum != null)
        {
            var exclusiveMinimum = Convert.ToDouble(filter.ExclusiveMinimum);
            if (numericValue <= exclusiveMinimum)
            {
                result.AddError("EXCLUSIVE_MINIMUM_CONSTRAINT", $"Value {numericValue} is not greater than exclusive minimum {exclusiveMinimum}");
                return false;
            }
        }

        // Exclusive maximum constraint
        if (filter.ExclusiveMaximum != null)
        {
            var exclusiveMaximum = Convert.ToDouble(filter.ExclusiveMaximum);
            if (numericValue >= exclusiveMaximum)
            {
                result.AddError("EXCLUSIVE_MAXIMUM_CONSTRAINT", $"Value {numericValue} is not less than exclusive maximum {exclusiveMaximum}");
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Evaluates length constraints for strings and arrays.
    /// </summary>
    private async Task<bool> EvaluateLengthConstraintsAsync(
        JsonElement value,
        FieldFilter filter,
        FilterEvaluationResult result,
        CancellationToken cancellationToken)
    {
        int? length = null;

        if (value.ValueKind == JsonValueKind.String)
        {
            length = value.GetString()?.Length ?? 0;
        }
        else if (value.ValueKind == JsonValueKind.Array)
        {
            length = value.GetArrayLength();
        }
        else
        {
            return true; // Length constraints only apply to strings and arrays
        }

        // MinLength constraint
        if (filter.MinLength.HasValue)
        {
            if (length < filter.MinLength.Value)
            {
                result.AddError("MIN_LENGTH_CONSTRAINT", $"Length {length} is less than minimum length {filter.MinLength.Value}");
                return false;
            }
        }

        // MaxLength constraint
        if (filter.MaxLength.HasValue)
        {
            if (length > filter.MaxLength.Value)
            {
                result.AddError("MAX_LENGTH_CONSTRAINT", $"Length {length} is greater than maximum length {filter.MaxLength.Value}");
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Evaluates array-specific constraints.
    /// </summary>
    private async Task<bool> EvaluateArrayConstraintsAsync(
        JsonElement value,
        FieldFilter filter,
        FilterEvaluationResult result,
        CancellationToken cancellationToken)
    {
        if (value.ValueKind != JsonValueKind.Array)
            return true; // Array constraints only apply to arrays

        var arrayLength = value.GetArrayLength();

        // MinItems constraint
        if (filter.MinItems.HasValue)
        {
            if (arrayLength < filter.MinItems.Value)
            {
                result.AddError("MIN_ITEMS_CONSTRAINT", $"Array has {arrayLength} items, less than minimum {filter.MinItems.Value}");
                return false;
            }
        }

        // MaxItems constraint
        if (filter.MaxItems.HasValue)
        {
            if (arrayLength > filter.MaxItems.Value)
            {
                result.AddError("MAX_ITEMS_CONSTRAINT", $"Array has {arrayLength} items, more than maximum {filter.MaxItems.Value}");
                return false;
            }
        }

        // UniqueItems constraint
        if (filter.UniqueItems == true)
        {
            var items = value.EnumerateArray().ToArray();
            var uniqueItems = items.Distinct(new JsonElementComparer()).Count();
            
            if (uniqueItems != items.Length)
            {
                result.AddError("UNIQUE_ITEMS_CONSTRAINT", "Array contains duplicate items but uniqueItems is required");
                return false;
            }
        }

        // Contains constraint
        if (filter.Contains != null)
        {
            var containsFound = false;
            foreach (var item in value.EnumerateArray())
            {
                // Simple contains check - would need more sophisticated matching for complex schemas
                if (filter.Contains is JsonElement containsElement)
                {
                    if (JsonElementComparer.Default.Equals(item, containsElement))
                    {
                        containsFound = true;
                        break;
                    }
                }
                else if (ValuesEqual(item, filter.Contains))
                {
                    containsFound = true;
                    break;
                }
            }

            if (!containsFound)
            {
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
        CancellationToken cancellationToken)
    {
        if (value.ValueKind != JsonValueKind.Object)
            return true; // Object constraints only apply to objects

        // Required properties constraint
        if (filter.Required != null && filter.Required.Length > 0)
        {
            foreach (var requiredProperty in filter.Required)
            {
                if (!value.TryGetProperty(requiredProperty, out _))
                {
                    result.AddError("REQUIRED_PROPERTY_MISSING", $"Required property '{requiredProperty}' is missing");
                    return false;
                }
            }
        }

        // Properties constraint (simplified - full implementation would require recursive schema validation)
        if (filter.Properties != null)
        {
            // This is a simplified check - full implementation would validate each property against its schema
            foreach (var expectedProperty in filter.Properties)
            {
                if (!value.TryGetProperty(expectedProperty.Key, out _))
                {
                    // Property is missing - check if it's required
                    if (filter.Required?.Contains(expectedProperty.Key) == true)
                    {
                        result.AddError("PROPERTY_SCHEMA_VIOLATION", $"Property '{expectedProperty.Key}' violates schema");
                        return false;
                    }
                }
            }
        }

        return true;
    }

    /// <summary>
    /// Gets the JSON Schema type for a JsonElement.
    /// </summary>
    private string GetJsonSchemaType(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => "string",
            JsonValueKind.Number => "number",
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
    private bool ValuesEqual(JsonElement jsonElement, object otherValue)
    {
        try
        {
            return jsonElement.ValueKind switch
            {
                JsonValueKind.String => jsonElement.GetString() == otherValue?.ToString(),
                JsonValueKind.Number when otherValue is int intVal => jsonElement.GetInt32() == intVal,
                JsonValueKind.Number when otherValue is long longVal => jsonElement.GetInt64() == longVal,
                JsonValueKind.Number when otherValue is double doubleVal => Math.Abs(jsonElement.GetDouble() - doubleVal) < 1e-15,
                JsonValueKind.Number when otherValue is float floatVal => Math.Abs(jsonElement.GetSingle() - floatVal) < 1e-7,
                JsonValueKind.Number => Math.Abs(jsonElement.GetDouble() - Convert.ToDouble(otherValue)) < 1e-15,
                JsonValueKind.True => otherValue is true,
                JsonValueKind.False => otherValue is false,
                JsonValueKind.Null => otherValue == null,
                _ => false
            };
        }
        catch
        {
            return false;
        }
    }
}

/// <summary>
/// Comparer for JsonElement values.
/// </summary>
public class JsonElementComparer : IEqualityComparer<JsonElement>
{
    public static readonly JsonElementComparer Default = new();

    public bool Equals(JsonElement x, JsonElement y)
    {
        if (x.ValueKind != y.ValueKind)
            return false;

        return x.ValueKind switch
        {
            JsonValueKind.String => x.GetString() == y.GetString(),
            JsonValueKind.Number => Math.Abs(x.GetDouble() - y.GetDouble()) < 1e-15,
            JsonValueKind.True or JsonValueKind.False => true, // Same kind means same value
            JsonValueKind.Null => true,
            JsonValueKind.Array => ArraysEqual(x, y),
            JsonValueKind.Object => ObjectsEqual(x, y),
            _ => false
        };
    }

    private bool ArraysEqual(JsonElement x, JsonElement y)
    {
        if (x.GetArrayLength() != y.GetArrayLength())
            return false;

        var xArray = x.EnumerateArray().ToArray();
        var yArray = y.EnumerateArray().ToArray();

        for (int i = 0; i < xArray.Length; i++)
        {
            if (!Equals(xArray[i], yArray[i]))
                return false;
        }

        return true;
    }

    private bool ObjectsEqual(JsonElement x, JsonElement y)
    {
        var xProperties = x.EnumerateObject().ToDictionary(p => p.Name, p => p.Value);
        var yProperties = y.EnumerateObject().ToDictionary(p => p.Name, p => p.Value);

        if (xProperties.Count != yProperties.Count)
            return false;

        foreach (var kvp in xProperties)
        {
            if (!yProperties.TryGetValue(kvp.Key, out var yValue) || !Equals(kvp.Value, yValue))
                return false;
        }

        return true;
    }

    public int GetHashCode(JsonElement obj)
    {
        // Simple hash code implementation
        return obj.ValueKind.GetHashCode();
    }
}

/// <summary>
/// Represents the result of field filter evaluation.
/// </summary>
public class FilterEvaluationResult
{
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
    public void AddError(string errorCode, string message)
    {
        Errors.Add($"{errorCode}: {message}");
        IsSuccessful = false;
    }
}