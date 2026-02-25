using System.Text.Json.Serialization;

namespace SdJwt.Net.Oid4Vp.Models;

/// <summary>
/// Represents a Submission Requirement according to DIF Presentation Exchange v2.0.0.
/// Describes requirements for how inputs must be submitted via a Presentation Submission.
/// </summary>
public class SubmissionRequirement {
        /// <summary>
        /// Gets or sets the name of this submission requirement.
        /// OPTIONAL. Human-friendly name that describes this submission requirement.
        /// </summary>
        [JsonPropertyName("name")]
#if NET6_0_OR_GREATER
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the purpose of this submission requirement.
        /// OPTIONAL. Describes the purpose for which this submission requirement exists.
        /// </summary>
        [JsonPropertyName("purpose")]
#if NET6_0_OR_GREATER
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
        public string? Purpose { get; set; }

        /// <summary>
        /// Gets or sets the rule for this submission requirement.
        /// REQUIRED. Must be "all" or "pick".
        /// </summary>
        [JsonPropertyName("rule")]
        public string Rule { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the count for pick rules.
        /// CONDITIONAL. Required when rule is "pick". Specifies how many inputs to pick.
        /// </summary>
        [JsonPropertyName("count")]
#if NET6_0_OR_GREATER
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
        public int? Count { get; set; }

        /// <summary>
        /// Gets or sets the minimum count for pick rules.
        /// OPTIONAL. Used with "pick" rule to specify minimum number of inputs.
        /// </summary>
        [JsonPropertyName("min")]
#if NET6_0_OR_GREATER
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
        public int? Min { get; set; }

        /// <summary>
        /// Gets or sets the maximum count for pick rules.
        /// OPTIONAL. Used with "pick" rule to specify maximum number of inputs.
        /// </summary>
        [JsonPropertyName("max")]
#if NET6_0_OR_GREATER
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
        public int? Max { get; set; }

        /// <summary>
        /// Gets or sets the input descriptors referenced by this submission requirement.
        /// CONDITIONAL. Array of strings referencing Input Descriptor IDs.
        /// Must not be present if nested submission requirements are present.
        /// </summary>
        [JsonPropertyName("from")]
#if NET6_0_OR_GREATER
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
        public string[]? From { get; set; }

        /// <summary>
        /// Gets or sets nested submission requirements.
        /// CONDITIONAL. Array of nested Submission Requirement objects.
        /// Must not be present if 'from' is present.
        /// </summary>
        [JsonPropertyName("from_nested")]
#if NET6_0_OR_GREATER
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
        public SubmissionRequirement[]? FromNested { get; set; }

        /// <summary>
        /// Creates a submission requirement that requires all specified input descriptors.
        /// </summary>
        /// <param name="inputDescriptorIds">IDs of the input descriptors to require</param>
        /// <param name="name">Optional human-friendly name</param>
        /// <param name="purpose">Optional purpose description</param>
        /// <returns>A new SubmissionRequirement instance</returns>
        public static SubmissionRequirement RequireAll(
            string[] inputDescriptorIds,
            string? name = null,
            string? purpose = null) {
#if NET6_0_OR_GREATER
                ArgumentNullException.ThrowIfNull(inputDescriptorIds);
#else
        if (inputDescriptorIds == null)
            throw new ArgumentNullException(nameof(inputDescriptorIds));
#endif

                if (inputDescriptorIds.Length == 0)
                        throw new ArgumentException("At least one input descriptor ID is required", nameof(inputDescriptorIds));

                return new SubmissionRequirement {
                        Name = name,
                        Purpose = purpose,
                        Rule = Oid4VpConstants.PresentationExchange.SubmissionRequirementRules.All,
                        From = inputDescriptorIds
                };
        }

        /// <summary>
        /// Creates a submission requirement that allows picking from specified input descriptors.
        /// </summary>
        /// <param name="inputDescriptorIds">IDs of the input descriptors to pick from</param>
        /// <param name="count">Number of inputs to pick</param>
        /// <param name="name">Optional human-friendly name</param>
        /// <param name="purpose">Optional purpose description</param>
        /// <returns>A new SubmissionRequirement instance</returns>
        public static SubmissionRequirement RequirePick(
            string[] inputDescriptorIds,
            int count,
            string? name = null,
            string? purpose = null) {
#if NET6_0_OR_GREATER
                ArgumentNullException.ThrowIfNull(inputDescriptorIds);
#else
        if (inputDescriptorIds == null)
            throw new ArgumentNullException(nameof(inputDescriptorIds));
#endif

                if (inputDescriptorIds.Length == 0)
                        throw new ArgumentException("At least one input descriptor ID is required", nameof(inputDescriptorIds));

                if (count <= 0)
                        throw new ArgumentException("Count must be positive", nameof(count));

                if (count > inputDescriptorIds.Length)
                        throw new ArgumentException("Count cannot exceed number of input descriptors", nameof(count));

                return new SubmissionRequirement {
                        Name = name,
                        Purpose = purpose,
                        Rule = Oid4VpConstants.PresentationExchange.SubmissionRequirementRules.Pick,
                        Count = count,
                        From = inputDescriptorIds
                };
        }

        /// <summary>
        /// Creates a submission requirement with min/max range for picking.
        /// </summary>
        /// <param name="inputDescriptorIds">IDs of the input descriptors to pick from</param>
        /// <param name="min">Minimum number of inputs to pick</param>
        /// <param name="max">Maximum number of inputs to pick</param>
        /// <param name="name">Optional human-friendly name</param>
        /// <param name="purpose">Optional purpose description</param>
        /// <returns>A new SubmissionRequirement instance</returns>
        public static SubmissionRequirement RequirePickRange(
            string[] inputDescriptorIds,
            int min,
            int max,
            string? name = null,
            string? purpose = null) {
#if NET6_0_OR_GREATER
                ArgumentNullException.ThrowIfNull(inputDescriptorIds);
#else
        if (inputDescriptorIds == null)
            throw new ArgumentNullException(nameof(inputDescriptorIds));
#endif

                if (inputDescriptorIds.Length == 0)
                        throw new ArgumentException("At least one input descriptor ID is required", nameof(inputDescriptorIds));

                if (min <= 0)
                        throw new ArgumentException("Min must be positive", nameof(min));

                if (max < min)
                        throw new ArgumentException("Max must be greater than or equal to min", nameof(max));

                if (max > inputDescriptorIds.Length)
                        throw new ArgumentException("Max cannot exceed number of input descriptors", nameof(max));

                return new SubmissionRequirement {
                        Name = name,
                        Purpose = purpose,
                        Rule = Oid4VpConstants.PresentationExchange.SubmissionRequirementRules.Pick,
                        Min = min,
                        Max = max,
                        From = inputDescriptorIds
                };
        }

        /// <summary>
        /// Validates this submission requirement according to DIF Presentation Exchange requirements.
        /// </summary>
        /// <param name="inputDescriptors">The input descriptors to validate against</param>
        /// <exception cref="InvalidOperationException">Thrown when the requirement is invalid</exception>
        public void Validate(InputDescriptor[] inputDescriptors) {
#if NET6_0_OR_GREATER
                ArgumentNullException.ThrowIfNull(inputDescriptors);
#else
        if (inputDescriptors == null)
            throw new ArgumentNullException(nameof(inputDescriptors));
#endif

                if (string.IsNullOrWhiteSpace(Rule)) {
                        throw new InvalidOperationException("Submission requirement rule is required");
                }

                if (Rule != Oid4VpConstants.PresentationExchange.SubmissionRequirementRules.All &&
                    Rule != Oid4VpConstants.PresentationExchange.SubmissionRequirementRules.Pick) {
                        throw new InvalidOperationException($"Submission requirement rule must be 'all' or 'pick', got '{Rule}'");
                }

                // Validate mutual exclusivity of 'from' and 'from_nested'
                if (From != null && FromNested != null) {
                        throw new InvalidOperationException("Submission requirement cannot have both 'from' and 'from_nested'");
                }

                if (From == null && FromNested == null) {
                        throw new InvalidOperationException("Submission requirement must have either 'from' or 'from_nested'");
                }

                // Validate 'from' references
                if (From != null) {
                        var inputDescriptorIds = inputDescriptors.Select(d => d.Id).ToHashSet();
                        foreach (var id in From) {
                                if (!inputDescriptorIds.Contains(id)) {
                                        throw new InvalidOperationException($"Submission requirement references unknown input descriptor ID: {id}");
                                }
                        }
                }

                // Validate pick rule constraints
                if (Rule == Oid4VpConstants.PresentationExchange.SubmissionRequirementRules.Pick) {
                        var availableCount = From?.Length ?? 0;

                        if (Count.HasValue) {
                                if (Count.Value <= 0) {
                                        throw new InvalidOperationException("Pick rule count must be positive");
                                }

                                if (Count.Value > availableCount) {
                                        throw new InvalidOperationException("Pick rule count cannot exceed number of available descriptors");
                                }
                        }
                        else if (Min.HasValue || Max.HasValue) {
                                if (Min.HasValue && Min.Value <= 0) {
                                        throw new InvalidOperationException("Pick rule min must be positive");
                                }

                                if (Max.HasValue && Min.HasValue && Max.Value < Min.Value) {
                                        throw new InvalidOperationException("Pick rule max must be greater than or equal to min");
                                }

                                if (Max.HasValue && Max.Value > availableCount) {
                                        throw new InvalidOperationException("Pick rule max cannot exceed number of available descriptors");
                                }
                        }
                        else {
                                throw new InvalidOperationException("Pick rule must specify either 'count' or 'min'/'max'");
                        }
                }

                // Validate nested requirements
                if (FromNested != null) {
                        foreach (var nested in FromNested) {
                                nested?.Validate(inputDescriptors);
                        }
                }
        }
}