using System.Text.Json.Serialization;

namespace SdJwt.Net.PresentationExchange.Models;

/// <summary>
/// Represents a predicate-based field filter for zero-knowledge and privacy-preserving constraints.
/// Extends FieldFilter to support predicate operations without revealing actual values.
/// </summary>
public class PredicateFilter : FieldFilter {
        /// <summary>
        /// Gets or sets the predicate operation to perform.
        /// Required. Examples: "age_over", "greater_than", "less_than", "equal_to", "in_range"
        /// </summary>
        [JsonPropertyName("predicate")]
        public string? Predicate { get; set; }

        /// <summary>
        /// Gets or sets the threshold value for comparison predicates.
        /// Optional. Used with predicates like "greater_than", "age_over", etc.
        /// </summary>
        [JsonPropertyName("threshold")]
        public object? Threshold { get; set; }

        /// <summary>
        /// Gets or sets the range values for range predicates.
        /// Optional. Used with "in_range" predicate. Array of [min, max] values.
        /// </summary>
        [JsonPropertyName("range")]
        public object[]? Range { get; set; }

        /// <summary>
        /// Gets or sets the proof type to use for the predicate.
        /// Optional. Examples: "zk-snark", "bbs+", "range-proof", "circuit"
        /// </summary>
        [JsonPropertyName("proof_type")]
        public string? ProofType { get; set; }

        /// <summary>
        /// Gets or sets whether the predicate supports zero-knowledge proofs.
        /// Optional. Default: false
        /// </summary>
        [JsonPropertyName("zero_knowledge")]
        public bool ZeroKnowledge { get; set; } = false;

        /// <summary>
        /// Gets or sets circuit parameters for zero-knowledge proofs.
        /// Optional. Used when proof_type is "circuit" or "zk-snark"
        /// </summary>
        [JsonPropertyName("circuit_params")]
        public Dictionary<string, object>? CircuitParams { get; set; }

        /// <summary>
        /// Validates the predicate filter according to predicate-specific requirements.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the predicate filter is invalid</exception>
        public new void Validate() {
                // Only call base validation if Type is not "predicate"
                // "predicate" is a custom type extension for this library
                if (Type != "predicate") {
                        base.Validate();
                }

                if (string.IsNullOrWhiteSpace(Predicate))
                        throw new InvalidOperationException("Predicate is required for predicate filters");

                var validPredicates = new[] {
            "age_over", "greater_than", "less_than", "greater_than_or_equal",
            "less_than_or_equal", "equal_to", "not_equal_to", "in_range",
            "in_set", "not_in_set", "is_adult", "is_citizen"
        };

                if (!validPredicates.Contains(Predicate)) {
                        throw new InvalidOperationException($"Invalid predicate '{Predicate}'. Must be one of: {string.Join(", ", validPredicates)}");
                }

                ValidatePredicateSpecificConstraints();
        }

        /// <summary>
        /// Validates constraints specific to the predicate type.
        /// </summary>
        private void ValidatePredicateSpecificConstraints() {
                switch (Predicate) {
                        case "age_over":
                                ValidateAgeOverPredicate();
                                break;
                        case "greater_than":
                        case "less_than":
                        case "greater_than_or_equal":
                        case "less_than_or_equal":
                                ValidateComparisonPredicate();
                                break;
                        case "in_range":
                                ValidateRangePredicate();
                                break;
                        case "in_set":
                        case "not_in_set":
                                ValidateSetPredicate();
                                break;
                }
        }

        /// <summary>
        /// Validates age_over predicate constraints.
        /// </summary>
        private void ValidateAgeOverPredicate() {
                if (Threshold == null)
                        throw new InvalidOperationException("Threshold is required for age_over predicate");

                if (!IsNumeric(Threshold) || Convert.ToInt32(Threshold) < 0)
                        throw new InvalidOperationException("Age threshold must be a non-negative number");
        }

        /// <summary>
        /// Validates comparison predicate constraints.
        /// </summary>
        private void ValidateComparisonPredicate() {
                if (Threshold == null)
                        throw new InvalidOperationException($"Threshold is required for {Predicate} predicate");

                if (!IsNumeric(Threshold))
                        throw new InvalidOperationException($"Threshold must be numeric for {Predicate} predicate");
        }

        /// <summary>
        /// Validates range predicate constraints.
        /// </summary>
        private void ValidateRangePredicate() {
                if (Range == null || Range.Length != 2)
                        throw new InvalidOperationException("Range must contain exactly two values [min, max] for in_range predicate");

                if (!IsNumeric(Range[0]) || !IsNumeric(Range[1]))
                        throw new InvalidOperationException("Range values must be numeric for in_range predicate");

                var min = Convert.ToDouble(Range[0]);
                var max = Convert.ToDouble(Range[1]);
                if (min >= max)
                        throw new InvalidOperationException("Range minimum must be less than maximum");
        }

        /// <summary>
        /// Validates set predicate constraints.
        /// </summary>
        private void ValidateSetPredicate() {
                if (Enum == null || Enum.Length == 0)
                        throw new InvalidOperationException($"Enum values are required for {Predicate} predicate");
        }

        /// <summary>
        /// Creates a predicate filter for age verification.
        /// </summary>
        /// <param name="minimumAge">The minimum age to verify</param>
        /// <param name="zeroKnowledge">Whether to use zero-knowledge proofs</param>
        /// <returns>A new PredicateFilter instance</returns>
        public static PredicateFilter CreateAgeOver(int minimumAge, bool zeroKnowledge = false) {
                if (minimumAge < 0)
                        throw new ArgumentException("Minimum age must be non-negative", nameof(minimumAge));

                return new PredicateFilter {
                        Type = "predicate",
                        Predicate = "age_over",
                        Threshold = minimumAge,
                        ZeroKnowledge = zeroKnowledge,
                        ProofType = zeroKnowledge ? "range-proof" : null
                };
        }

        /// <summary>
        /// Creates a predicate filter for salary range verification.
        /// </summary>
        /// <param name="minimumSalary">The minimum salary threshold</param>
        /// <param name="zeroKnowledge">Whether to use zero-knowledge proofs</param>
        /// <returns>A new PredicateFilter instance</returns>
        public static PredicateFilter CreateSalaryOver(decimal minimumSalary, bool zeroKnowledge = true) {
                if (minimumSalary < 0)
                        throw new ArgumentException("Minimum salary must be non-negative", nameof(minimumSalary));

                return new PredicateFilter {
                        Type = "predicate",
                        Predicate = "greater_than_or_equal",
                        Threshold = minimumSalary,
                        ZeroKnowledge = zeroKnowledge,
                        ProofType = zeroKnowledge ? "zk-snark" : null
                };
        }

        /// <summary>
        /// Creates a predicate filter for range verification.
        /// </summary>
        /// <param name="minimum">The minimum value</param>
        /// <param name="maximum">The maximum value</param>
        /// <param name="zeroKnowledge">Whether to use zero-knowledge proofs</param>
        /// <returns>A new PredicateFilter instance</returns>
        public static new PredicateFilter CreateRange(object minimum, object maximum, bool zeroKnowledge = false) {
                if (!IsNumeric(minimum) || !IsNumeric(maximum))
                        throw new ArgumentException("Range values must be numeric");

                var min = Convert.ToDouble(minimum);
                var max = Convert.ToDouble(maximum);
                if (min >= max)
                        throw new ArgumentException("Minimum must be less than maximum");

                return new PredicateFilter {
                        Type = "predicate",
                        Predicate = "in_range",
                        Range = new[] { minimum, maximum },
                        ZeroKnowledge = zeroKnowledge,
                        ProofType = zeroKnowledge ? "range-proof" : null
                };
        }

        /// <summary>
        /// Creates a predicate filter for citizenship verification.
        /// </summary>
        /// <param name="countries">Array of accepted country codes</param>
        /// <returns>A new PredicateFilter instance</returns>
        public static PredicateFilter CreateCitizenshipIn(params string[] countries) {
                if (countries == null || countries.Length == 0)
                        throw new ArgumentException("At least one country is required", nameof(countries));

                return new PredicateFilter {
                        Type = "predicate",
                        Predicate = "in_set",
                        Enum = countries.Cast<object>().ToArray()
                };
        }

        /// <summary>
        /// Checks if a value is numeric.
        /// </summary>
        /// <param name="value">The value to check</param>
        /// <returns>True if the value is numeric</returns>
        private static bool IsNumeric(object value) {
                return value is int or long or float or double or decimal;
        }
}