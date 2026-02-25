using System.Text.Json.Serialization;

namespace SdJwt.Net.Oid4Vp.Models;

/// <summary>
/// Represents a Presentation Definition according to DIF Presentation Exchange v2.1.1.
/// This object communicates the proof requirements to a Holder.
/// </summary>
public class PresentationDefinition {
        /// <summary>
        /// Gets or sets the unique identifier for this Presentation Definition.
        /// REQUIRED. Must be a string that uniquely identifies this definition.
        /// </summary>
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the name of this Presentation Definition.
        /// OPTIONAL. Human-friendly name that describes what the Presentation Definition pertains to.
        /// </summary>
        [JsonPropertyName("name")]
#if NET6_0_OR_GREATER
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the purpose of this Presentation Definition.
        /// OPTIONAL. Describes the purpose for which the Presentation Definition's inputs are being requested.
        /// </summary>
        [JsonPropertyName("purpose")]
#if NET6_0_OR_GREATER
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
        public string? Purpose { get; set; }

        /// <summary>
        /// Gets or sets the format restrictions for this Presentation Definition.
        /// OPTIONAL. Objects with requirements a Verifier has for the format of the Credential.
        /// </summary>
        [JsonPropertyName("format")]
#if NET6_0_OR_GREATER
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
        public Dictionary<string, object>? Format { get; set; }

        /// <summary>
        /// Gets or sets the submission requirements for this Presentation Definition.
        /// OPTIONAL. Value expressing how inputs must be submitted via a Presentation Submission.
        /// </summary>
        [JsonPropertyName("submission_requirements")]
#if NET6_0_OR_GREATER
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
        public SubmissionRequirement[]? SubmissionRequirements { get; set; }

        /// <summary>
        /// Gets or sets the input descriptors for this Presentation Definition.
        /// REQUIRED. Array of Input Descriptor Objects, each representing a discrete set of requirements.
        /// </summary>
        [JsonPropertyName("input_descriptors")]
        public InputDescriptor[] InputDescriptors { get; set; } = Array.Empty<InputDescriptor>();

        /// <summary>
        /// Creates a basic presentation definition with a single input descriptor.
        /// </summary>
        /// <param name="id">Unique identifier for the presentation definition</param>
        /// <param name="credentialType">The credential type being requested</param>
        /// <param name="name">Optional human-friendly name</param>
        /// <param name="purpose">Optional purpose description</param>
        /// <returns>A new PresentationDefinition instance</returns>
        public static PresentationDefinition CreateSimple(
            string id,
            string credentialType,
            string? name = null,
            string? purpose = null) {
#if NET6_0_OR_GREATER
                ArgumentException.ThrowIfNullOrWhiteSpace(id);
                ArgumentException.ThrowIfNullOrWhiteSpace(credentialType);
#else
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(id));
        if (string.IsNullOrWhiteSpace(credentialType))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(credentialType));
#endif

                return new PresentationDefinition {
                        Id = id,
                        Name = name,
                        Purpose = purpose,
                        InputDescriptors = new[]
                    {
                InputDescriptor.CreateForCredentialType($"{id}_input", credentialType)
            }
                };
        }

        /// <summary>
        /// Adds format restrictions for SD-JWT credentials.
        /// </summary>
        /// <param name="algorithms">Optional array of allowed signing algorithms</param>
        /// <returns>This PresentationDefinition for method chaining</returns>
        public PresentationDefinition WithSdJwtFormat(string[]? algorithms = null) {
                Format ??= new Dictionary<string, object>();

                var formatConstraints = new Dictionary<string, object>();
                if (algorithms != null && algorithms.Length > 0) {
                        formatConstraints["alg"] = algorithms;
                }

                Format[Oid4VpConstants.SdJwtVcFormat] = formatConstraints;
                return this;
        }

        /// <summary>
        /// Adds a submission requirement rule.
        /// </summary>
        /// <param name="rule">The submission requirement rule</param>
        /// <returns>This PresentationDefinition for method chaining</returns>
        public PresentationDefinition WithSubmissionRequirement(SubmissionRequirement rule) {
#if NET6_0_OR_GREATER
                ArgumentNullException.ThrowIfNull(rule);
#else
        if (rule == null)
            throw new ArgumentNullException(nameof(rule));
#endif

                var currentRequirements = SubmissionRequirements?.ToList() ?? new List<SubmissionRequirement>();
                currentRequirements.Add(rule);
                SubmissionRequirements = currentRequirements.ToArray();
                return this;
        }

        /// <summary>
        /// Adds an input descriptor to this presentation definition.
        /// </summary>
        /// <param name="descriptor">The input descriptor to add</param>
        /// <returns>This PresentationDefinition for method chaining</returns>
        public PresentationDefinition WithInputDescriptor(InputDescriptor descriptor) {
#if NET6_0_OR_GREATER
                ArgumentNullException.ThrowIfNull(descriptor);
#else
        if (descriptor == null)
            throw new ArgumentNullException(nameof(descriptor));
#endif

                var currentDescriptors = InputDescriptors?.ToList() ?? new List<InputDescriptor>();
                currentDescriptors.Add(descriptor);
                InputDescriptors = currentDescriptors.ToArray();
                return this;
        }

        /// <summary>
        /// Validates this presentation definition according to DIF Presentation Exchange requirements.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the definition is invalid</exception>
        public void Validate() {
                if (string.IsNullOrWhiteSpace(Id)) {
                        throw new InvalidOperationException("Presentation definition id is required");
                }

                if (InputDescriptors == null || InputDescriptors.Length == 0) {
                        throw new InvalidOperationException("At least one input descriptor is required");
                }

                // Validate each input descriptor
                foreach (var descriptor in InputDescriptors) {
                        descriptor?.Validate();
                }

                // Validate submission requirements if present
                if (SubmissionRequirements != null) {
                        foreach (var requirement in SubmissionRequirements) {
                                requirement?.Validate(InputDescriptors);
                        }
                }
        }
}