using System.Text.Json.Serialization;

namespace SdJwt.Net.PresentationExchange.Models;

/// <summary>
/// Represents a Submission Requirement as defined in DIF Presentation Exchange 2.1.1.
/// Defines how input descriptors should be grouped and satisfied in a presentation.
/// </summary>
public class SubmissionRequirement {
        /// <summary>
        /// Gets or sets the rule that determines how this requirement should be satisfied.
        /// Required. Must be either "all" or "pick".
        /// </summary>
        [JsonPropertyName("rule")]
        public string Rule { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the exact number of input descriptors that must be satisfied.
        /// Optional. Used with "pick" rule to specify an exact count.
        /// </summary>
        [JsonPropertyName("count")]
        public int? Count { get; set; }

        /// <summary>
        /// Gets or sets the minimum number of input descriptors that must be satisfied.
        /// Optional. Used with "pick" rule to specify a minimum count.
        /// </summary>
        [JsonPropertyName("min")]
        public int? Min { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of input descriptors that may be satisfied.
        /// Optional. Used with "pick" rule to specify a maximum count.
        /// </summary>
        [JsonPropertyName("max")]
        public int? Max { get; set; }

        /// <summary>
        /// Gets or sets the name of this submission requirement.
        /// Optional. Used for human-readable identification.
        /// </summary>
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the purpose of this submission requirement.
        /// Optional. Explains why this grouping is needed.
        /// </summary>
        [JsonPropertyName("purpose")]
        public string? Purpose { get; set; }

        /// <summary>
        /// Gets or sets the descriptor group identifier that this requirement references.
        /// Optional. Per DIF Presentation Exchange, this typically references an Input Descriptor group.
        /// For backward compatibility, direct descriptor ID references are also accepted.
        /// </summary>
        [JsonPropertyName("from")]
        public string? From { get; set; }

        /// <summary>
        /// Gets or sets nested submission requirements.
        /// Optional. Used for hierarchical requirement structures.
        /// </summary>
        [JsonPropertyName("from_nested")]
        public SubmissionRequirement[]? FromNested { get; set; }

        /// <summary>
        /// Validates the submission requirement according to DIF PEX 2.1.1 requirements.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the requirement is invalid</exception>
        public void Validate() {
                if (string.IsNullOrWhiteSpace(Rule))
                        throw new InvalidOperationException("Submission requirement rule is required");

                var validRules = new[] { PresentationExchangeConstants.SubmissionRules.All, PresentationExchangeConstants.SubmissionRules.Pick };
                if (!validRules.Contains(Rule))
                        throw new InvalidOperationException($"Rule must be one of: {string.Join(", ", validRules)}");

                // Validate count constraints
                ValidateCountConstraints();

                // Ensure either 'from' or 'from_nested' is specified, but not both
                if (string.IsNullOrWhiteSpace(From) && (FromNested == null || FromNested.Length == 0))
                        throw new InvalidOperationException("Submission requirement must specify either 'from' or 'from_nested'");

                if (!string.IsNullOrWhiteSpace(From) && FromNested != null && FromNested.Length > 0)
                        throw new InvalidOperationException("Submission requirement cannot specify both 'from' and 'from_nested'");

                // Validate nested requirements
                if (FromNested != null) {
                        foreach (var nested in FromNested) {
                                nested?.Validate();
                        }
                }

                // Validate rule-specific constraints
                ValidateRuleSpecificConstraints();
        }

        /// <summary>
        /// Validates count-related constraints.
        /// </summary>
        private void ValidateCountConstraints() {
                if (Count.HasValue && Count < 0)
                        throw new InvalidOperationException("Count must be non-negative");

                if (Min.HasValue && Min < 0)
                        throw new InvalidOperationException("Min must be non-negative");

                if (Max.HasValue && Max < 0)
                        throw new InvalidOperationException("Max must be non-negative");

                if (Min.HasValue && Max.HasValue && Min > Max)
                        throw new InvalidOperationException("Min cannot be greater than Max");

                if (Count.HasValue && (Min.HasValue || Max.HasValue))
                        throw new InvalidOperationException("Count cannot be used with Min or Max");
        }

        /// <summary>
        /// Validates constraints specific to the rule type.
        /// </summary>
        private void ValidateRuleSpecificConstraints() {
                switch (Rule) {
                        case PresentationExchangeConstants.SubmissionRules.All:
                                ValidateAllRuleConstraints();
                                break;
                        case PresentationExchangeConstants.SubmissionRules.Pick:
                                ValidatePickRuleConstraints();
                                break;
                }
        }

        /// <summary>
        /// Validates constraints for "all" rule.
        /// </summary>
        private void ValidateAllRuleConstraints() {
                if (Count.HasValue || Min.HasValue || Max.HasValue) {
                        throw new InvalidOperationException("Count, Min, and Max are not applicable for 'all' rule");
                }
        }

        /// <summary>
        /// Validates constraints for "pick" rule.
        /// </summary>
        private void ValidatePickRuleConstraints() {
                if (!Count.HasValue && !Min.HasValue && !Max.HasValue) {
                        throw new InvalidOperationException("Pick rule requires at least one of Count, Min, or Max");
                }
        }

        /// <summary>
        /// Creates a submission requirement that requires all descriptors from the referenced group.
        /// </summary>
        /// <param name="from">The group identifier (or descriptor ID for backward compatibility)</param>
        /// <param name="name">Optional name for the requirement</param>
        /// <param name="purpose">Optional purpose description</param>
        /// <returns>A new SubmissionRequirement instance</returns>
        public static SubmissionRequirement CreateAll(string from, string? name = null, string? purpose = null) {
                if (string.IsNullOrWhiteSpace(from))
                        throw new ArgumentException("From reference cannot be null or empty", nameof(from));

                return new SubmissionRequirement {
                        Rule = PresentationExchangeConstants.SubmissionRules.All,
                        From = from,
                        Name = name,
                        Purpose = purpose
                };
        }

        /// <summary>
        /// Creates a submission requirement that requires all nested requirements.
        /// </summary>
        /// <param name="nestedRequirements">The nested submission requirements</param>
        /// <param name="name">Optional name for the requirement</param>
        /// <param name="purpose">Optional purpose description</param>
        /// <returns>A new SubmissionRequirement instance</returns>
        public static SubmissionRequirement CreateAllNested(SubmissionRequirement[] nestedRequirements, string? name = null, string? purpose = null) {
                if (nestedRequirements == null || nestedRequirements.Length == 0)
                        throw new ArgumentException("At least one nested requirement is required", nameof(nestedRequirements));

                return new SubmissionRequirement {
                        Rule = PresentationExchangeConstants.SubmissionRules.All,
                        FromNested = nestedRequirements,
                        Name = name,
                        Purpose = purpose
                };
        }

        /// <summary>
        /// Creates a submission requirement that requires picking an exact number of descriptors from the referenced group.
        /// </summary>
        /// <param name="from">The group identifier (or descriptor ID for backward compatibility)</param>
        /// <param name="count">The exact number to pick</param>
        /// <param name="name">Optional name for the requirement</param>
        /// <param name="purpose">Optional purpose description</param>
        /// <returns>A new SubmissionRequirement instance</returns>
        public static SubmissionRequirement CreatePick(string from, int count, string? name = null, string? purpose = null) {
                if (string.IsNullOrWhiteSpace(from))
                        throw new ArgumentException("From reference cannot be null or empty", nameof(from));
                if (count < 0)
                        throw new ArgumentException("Count must be non-negative", nameof(count));

                return new SubmissionRequirement {
                        Rule = PresentationExchangeConstants.SubmissionRules.Pick,
                        From = from,
                        Count = count,
                        Name = name,
                        Purpose = purpose
                };
        }

        /// <summary>
        /// Creates a submission requirement that requires picking within a range from the referenced group.
        /// </summary>
        /// <param name="from">The group identifier (or descriptor ID for backward compatibility)</param>
        /// <param name="min">The minimum number to pick</param>
        /// <param name="max">The maximum number to pick</param>
        /// <param name="name">Optional name for the requirement</param>
        /// <param name="purpose">Optional purpose description</param>
        /// <returns>A new SubmissionRequirement instance</returns>
        public static SubmissionRequirement CreatePickRange(string from, int? min = null, int? max = null, string? name = null, string? purpose = null) {
                if (string.IsNullOrWhiteSpace(from))
                        throw new ArgumentException("From reference cannot be null or empty", nameof(from));
                if (min.HasValue && min < 0)
                        throw new ArgumentException("Min must be non-negative", nameof(min));
                if (max.HasValue && max < 0)
                        throw new ArgumentException("Max must be non-negative", nameof(max));
                if (min.HasValue && max.HasValue && min > max)
                        throw new ArgumentException("Min cannot be greater than Max");

                if (!min.HasValue && !max.HasValue)
                        throw new ArgumentException("At least one of min or max must be specified");

                return new SubmissionRequirement {
                        Rule = PresentationExchangeConstants.SubmissionRules.Pick,
                        From = from,
                        Min = min,
                        Max = max,
                        Name = name,
                        Purpose = purpose
                };
        }

        /// <summary>
        /// Creates a submission requirement that picks from nested requirements.
        /// </summary>
        /// <param name="nestedRequirements">The nested submission requirements</param>
        /// <param name="count">The exact number to pick from nested requirements</param>
        /// <param name="name">Optional name for the requirement</param>
        /// <param name="purpose">Optional purpose description</param>
        /// <returns>A new SubmissionRequirement instance</returns>
        public static SubmissionRequirement CreatePickNested(SubmissionRequirement[] nestedRequirements, int count, string? name = null, string? purpose = null) {
                if (nestedRequirements == null || nestedRequirements.Length == 0)
                        throw new ArgumentException("At least one nested requirement is required", nameof(nestedRequirements));
                if (count < 0)
                        throw new ArgumentException("Count must be non-negative", nameof(count));
                if (count > nestedRequirements.Length)
                        throw new ArgumentException("Count cannot exceed the number of nested requirements", nameof(count));

                return new SubmissionRequirement {
                        Rule = PresentationExchangeConstants.SubmissionRules.Pick,
                        FromNested = nestedRequirements,
                        Count = count,
                        Name = name,
                        Purpose = purpose
                };
        }

        /// <summary>
        /// Gets the effective minimum count for this requirement.
        /// </summary>
        /// <returns>The minimum number of items that must be satisfied</returns>
        public int GetEffectiveMinCount() {
                if (Rule == PresentationExchangeConstants.SubmissionRules.All) {
                        if (!string.IsNullOrWhiteSpace(From))
                                return 1;

                        if (FromNested != null)
                                return FromNested.Length;
                }

                if (Count.HasValue)
                        return Count.Value;

                if (Min.HasValue)
                        return Min.Value;

                return 0;
        }

        /// <summary>
        /// Gets the effective maximum count for this requirement.
        /// </summary>
        /// <returns>The maximum number of items that may be satisfied, or null if unlimited</returns>
        public int? GetEffectiveMaxCount() {
                if (Rule == PresentationExchangeConstants.SubmissionRules.All) {
                        if (!string.IsNullOrWhiteSpace(From))
                                return 1;

                        if (FromNested != null)
                                return FromNested.Length;
                }

                if (Count.HasValue)
                        return Count.Value;

                return Max;
        }

        /// <summary>
        /// Checks if this requirement is satisfied by the given count.
        /// </summary>
        /// <param name="satisfiedCount">The number of satisfied items</param>
        /// <returns>True if the requirement is satisfied</returns>
        public bool IsSatisfiedBy(int satisfiedCount) {
                var minCount = GetEffectiveMinCount();
                var maxCount = GetEffectiveMaxCount();

                if (satisfiedCount < minCount)
                        return false;

                if (maxCount.HasValue && satisfiedCount > maxCount.Value)
                        return false;

                return true;
        }

        /// <summary>
        /// Gets all input descriptor IDs directly referenced by this requirement.
        /// </summary>
        /// <returns>Array of input descriptor IDs</returns>
        public string[] GetDirectReferences() {
                if (!string.IsNullOrWhiteSpace(From))
                        return new[] { From };

                return Array.Empty<string>();
        }

        /// <summary>
        /// Gets all input descriptor IDs referenced by this requirement and its nested requirements.
        /// </summary>
        /// <returns>Array of all referenced input descriptor IDs</returns>
        public string[] GetAllReferences() {
                var references = new HashSet<string>();
                CollectAllReferences(references);
                return references.ToArray();
        }

        /// <summary>
        /// Recursively collects all referenced input descriptor IDs.
        /// </summary>
        /// <param name="references">Set to collect references</param>
        private void CollectAllReferences(HashSet<string> references) {
                if (!string.IsNullOrWhiteSpace(From)) {
                        references.Add(From);
                }

                if (FromNested != null) {
                        foreach (var nested in FromNested) {
                                nested.CollectAllReferences(references);
                        }
                }
        }
}
