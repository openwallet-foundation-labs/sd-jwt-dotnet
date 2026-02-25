using System.Text.Json.Serialization;

namespace SdJwt.Net.PresentationExchange.Models;

/// <summary>
/// Represents a field filter for constraining credential field values as defined in DIF Presentation Exchange 2.1.1.
/// Supports JSON Schema-based validation criteria for precise field matching.
/// </summary>
public class FieldFilter {
        /// <summary>
        /// Gets or sets the expected type of the field value.
        /// Optional. Supports JSON Schema types: string, number, integer, boolean, array, object.
        /// </summary>
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        /// <summary>
        /// Gets or sets the exact value that the field must match.
        /// Optional. Used for exact value matching.
        /// </summary>
        [JsonPropertyName("const")]
        public object? Const { get; set; }

        /// <summary>
        /// Gets or sets the list of allowed values for the field.
        /// Optional. The field value must be one of these values.
        /// </summary>
        [JsonPropertyName("enum")]
        public object[]? Enum { get; set; }

        /// <summary>
        /// Gets or sets the pattern that string values must match.
        /// Optional. Used with type "string" for regex pattern matching.
        /// </summary>
        [JsonPropertyName("pattern")]
        public string? Pattern { get; set; }

        /// <summary>
        /// Gets or sets the minimum value for numeric fields.
        /// Optional. Used with type "number" or "integer".
        /// </summary>
        [JsonPropertyName("minimum")]
        public object? Minimum { get; set; }

        /// <summary>
        /// Gets or sets the maximum value for numeric fields.
        /// Optional. Used with type "number" or "integer".
        /// </summary>
        [JsonPropertyName("maximum")]
        public object? Maximum { get; set; }

        /// <summary>
        /// Gets or sets the exclusive minimum value for numeric fields.
        /// Optional. Used with type "number" or "integer".
        /// </summary>
        [JsonPropertyName("exclusiveMinimum")]
        public object? ExclusiveMinimum { get; set; }

        /// <summary>
        /// Gets or sets the exclusive maximum value for numeric fields.
        /// Optional. Used with type "number" or "integer".
        /// </summary>
        [JsonPropertyName("exclusiveMaximum")]
        public object? ExclusiveMaximum { get; set; }

        /// <summary>
        /// Gets or sets the minimum length for string or array fields.
        /// Optional. Used with type "string" or "array".
        /// </summary>
        [JsonPropertyName("minLength")]
        public int? MinLength { get; set; }

        /// <summary>
        /// Gets or sets the maximum length for string or array fields.
        /// Optional. Used with type "string" or "array".
        /// </summary>
        [JsonPropertyName("maxLength")]
        public int? MaxLength { get; set; }

        /// <summary>
        /// Gets or sets the minimum number of items for array fields.
        /// Optional. Used with type "array".
        /// </summary>
        [JsonPropertyName("minItems")]
        public int? MinItems { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of items for array fields.
        /// Optional. Used with type "array".
        /// </summary>
        [JsonPropertyName("maxItems")]
        public int? MaxItems { get; set; }

        /// <summary>
        /// Gets or sets whether all array items must be unique.
        /// Optional. Used with type "array".
        /// </summary>
        [JsonPropertyName("uniqueItems")]
        public bool? UniqueItems { get; set; }

        /// <summary>
        /// Gets or sets the schema that array items must match.
        /// Optional. Used with type "array" for item validation.
        /// </summary>
        [JsonPropertyName("items")]
        public object? Items { get; set; }

        /// <summary>
        /// Gets or sets the schema that at least one array item must match.
        /// Optional. Used with type "array" for contains validation.
        /// </summary>
        [JsonPropertyName("contains")]
        public object? Contains { get; set; }

        /// <summary>
        /// Gets or sets the required properties for object fields.
        /// Optional. Used with type "object".
        /// </summary>
        [JsonPropertyName("required")]
        public string[]? Required { get; set; }

        /// <summary>
        /// Gets or sets the property schemas for object fields.
        /// Optional. Used with type "object".
        /// </summary>
        [JsonPropertyName("properties")]
        public Dictionary<string, object>? Properties { get; set; }

        /// <summary>
        /// Gets or sets whether additional properties are allowed for object fields.
        /// Optional. Used with type "object".
        /// </summary>
        [JsonPropertyName("additionalProperties")]
        public bool? AdditionalProperties { get; set; }

        /// <summary>
        /// Gets or sets the format constraint for string fields.
        /// Optional. Supports formats like "date-time", "email", "uri", etc.
        /// </summary>
        [JsonPropertyName("format")]
        public string? Format { get; set; }

        /// <summary>
        /// Gets or sets additional filter properties not defined in the base specification.
        /// Optional. Allows for extension properties.
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, object>? ExtensionData { get; set; }

        /// <summary>
        /// Validates the field filter according to JSON Schema and DIF PEX 2.1.1 requirements.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the filter is invalid</exception>
        public void Validate() {
                // Validate type consistency
                if (!string.IsNullOrEmpty(Type)) {
                        var validTypes = new[] { "string", "number", "integer", "boolean", "array", "object", "null" };
                        if (!validTypes.Contains(Type)) {
                                throw new InvalidOperationException($"Invalid type '{Type}'. Must be one of: {string.Join(", ", validTypes)}");
                        }

                        ValidateTypeSpecificConstraints();
                }

                // Validate numeric ranges
                ValidateNumericRanges();

                // Validate string/array lengths
                ValidateLengthConstraints();

                // Validate array constraints
                ValidateArrayConstraints();
        }

        /// <summary>
        /// Validates type-specific constraints.
        /// </summary>
        private void ValidateTypeSpecificConstraints() {
                switch (Type) {
                        case "string":
                                ValidateStringConstraints();
                                break;
                        case "number":
                        case "integer":
                                ValidateNumericConstraints();
                                break;
                        case "array":
                                ValidateArrayTypeConstraints();
                                break;
                        case "object":
                                ValidateObjectConstraints();
                                break;
                }
        }

        /// <summary>
        /// Validates string-specific constraints.
        /// </summary>
        private void ValidateStringConstraints() {
                if (MinLength.HasValue && MinLength < 0)
                        throw new InvalidOperationException("MinLength must be non-negative");

                if (MaxLength.HasValue && MaxLength < 0)
                        throw new InvalidOperationException("MaxLength must be non-negative");

                if (MinLength.HasValue && MaxLength.HasValue && MinLength > MaxLength)
                        throw new InvalidOperationException("MinLength cannot be greater than MaxLength");
        }

        /// <summary>
        /// Validates numeric-specific constraints.
        /// </summary>
        private void ValidateNumericConstraints() {
                // Validate that numeric values are actually numeric when type is number/integer
                if (Const != null && !IsNumeric(Const))
                        throw new InvalidOperationException($"Const value must be numeric for type '{Type}'");
        }

        /// <summary>
        /// Validates array-specific constraints.
        /// </summary>
        private void ValidateArrayTypeConstraints() {
                if (MinItems.HasValue && MinItems < 0)
                        throw new InvalidOperationException("MinItems must be non-negative");

                if (MaxItems.HasValue && MaxItems < 0)
                        throw new InvalidOperationException("MaxItems must be non-negative");

                if (MinItems.HasValue && MaxItems.HasValue && MinItems > MaxItems)
                        throw new InvalidOperationException("MinItems cannot be greater than MaxItems");
        }

        /// <summary>
        /// Validates object-specific constraints.
        /// </summary>
        private void ValidateObjectConstraints() {
                if (Required != null && Required.Any(string.IsNullOrWhiteSpace))
                        throw new InvalidOperationException("Required property names cannot be null or empty");
        }

        /// <summary>
        /// Validates numeric range constraints.
        /// </summary>
        private void ValidateNumericRanges() {
                if (Minimum != null && Maximum != null &&
                    IsNumeric(Minimum) && IsNumeric(Maximum)) {
                        var min = Convert.ToDouble(Minimum);
                        var max = Convert.ToDouble(Maximum);
                        if (min > max)
                                throw new InvalidOperationException("Minimum cannot be greater than Maximum");
                }

                if (ExclusiveMinimum != null && ExclusiveMaximum != null &&
                    IsNumeric(ExclusiveMinimum) && IsNumeric(ExclusiveMaximum)) {
                        var min = Convert.ToDouble(ExclusiveMinimum);
                        var max = Convert.ToDouble(ExclusiveMaximum);
                        if (min >= max)
                                throw new InvalidOperationException("ExclusiveMinimum must be less than ExclusiveMaximum");
                }
        }

        /// <summary>
        /// Validates length constraints.
        /// </summary>
        private void ValidateLengthConstraints() {
                if (MinLength.HasValue && MinLength < 0)
                        throw new InvalidOperationException("MinLength must be non-negative");

                if (MaxLength.HasValue && MaxLength < 0)
                        throw new InvalidOperationException("MaxLength must be non-negative");

                if (MinLength.HasValue && MaxLength.HasValue && MinLength > MaxLength)
                        throw new InvalidOperationException("MinLength cannot be greater than MaxLength");
        }

        /// <summary>
        /// Validates array constraints.
        /// </summary>
        private void ValidateArrayConstraints() {
                if (MinItems.HasValue && MinItems < 0)
                        throw new InvalidOperationException("MinItems must be non-negative");

                if (MaxItems.HasValue && MaxItems < 0)
                        throw new InvalidOperationException("MaxItems must be non-negative");

                if (MinItems.HasValue && MaxItems.HasValue && MinItems > MaxItems)
                        throw new InvalidOperationException("MinItems cannot be greater than MaxItems");
        }

        /// <summary>
        /// Checks if a value is numeric.
        /// </summary>
        /// <param name="value">The value to check</param>
        /// <returns>True if the value is numeric</returns>
        private static bool IsNumeric(object value) {
                return value is int or long or float or double or decimal;
        }

        /// <summary>
        /// Creates a filter that requires an exact constant value.
        /// </summary>
        /// <param name="value">The required constant value</param>
        /// <returns>A new FieldFilter instance</returns>
        public static FieldFilter CreateConst(object value) {
                return new FieldFilter {
                        Const = value
                };
        }

        /// <summary>
        /// Creates a filter that allows one of several enumerated values.
        /// </summary>
        /// <param name="values">The allowed values</param>
        /// <returns>A new FieldFilter instance</returns>
        public static FieldFilter CreateEnum(params object[] values) {
                if (values == null || values.Length == 0)
                        throw new ArgumentException("At least one value is required for enum filter", nameof(values));

                return new FieldFilter {
                        Enum = values
                };
        }

        /// <summary>
        /// Creates a filter for string pattern matching.
        /// </summary>
        /// <param name="pattern">The regex pattern to match</param>
        /// <returns>A new FieldFilter instance</returns>
        public static FieldFilter CreatePattern(string pattern) {
                if (string.IsNullOrWhiteSpace(pattern))
                        throw new ArgumentException("Pattern cannot be null or empty", nameof(pattern));

                return new FieldFilter {
                        Type = "string",
                        Pattern = pattern
                };
        }

        /// <summary>
        /// Creates a filter for numeric range validation.
        /// </summary>
        /// <param name="minimum">Optional minimum value (inclusive)</param>
        /// <param name="maximum">Optional maximum value (inclusive)</param>
        /// <param name="isInteger">Whether to use integer type instead of number</param>
        /// <returns>A new FieldFilter instance</returns>
        public static FieldFilter CreateRange(object? minimum = null, object? maximum = null, bool isInteger = false) {
                var filter = new FieldFilter {
                        Type = isInteger ? "integer" : "number"
                };

                if (minimum != null)
                        filter.Minimum = minimum;
                if (maximum != null)
                        filter.Maximum = maximum;

                return filter;
        }

        /// <summary>
        /// Creates a filter for array contains validation.
        /// </summary>
        /// <param name="containsValue">The value that the array must contain</param>
        /// <returns>A new FieldFilter instance</returns>
        public static FieldFilter CreateArrayContains(object containsValue) {
                return new FieldFilter {
                        Type = "array",
                        Contains = new { Const = containsValue }
                };
        }

        /// <summary>
        /// Creates a filter for type checking only.
        /// </summary>
        /// <param name="type">The required type</param>
        /// <returns>A new FieldFilter instance</returns>
        public static FieldFilter CreateType(string type) {
                if (string.IsNullOrWhiteSpace(type))
                        throw new ArgumentException("Type cannot be null or empty", nameof(type));

                return new FieldFilter {
                        Type = type
                };
        }
}