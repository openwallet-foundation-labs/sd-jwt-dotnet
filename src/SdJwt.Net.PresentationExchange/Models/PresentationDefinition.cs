using System.Text.Json.Serialization;

namespace SdJwt.Net.PresentationExchange.Models;

/// <summary>
/// Represents a Presentation Definition as defined in DIF Presentation Exchange 2.1.1.
/// Used by verifiers to request specific credentials from holders.
/// </summary>
public class PresentationDefinition
{
    /// <summary>
    /// Gets or sets the unique identifier for this presentation definition.
    /// Required. Must be unique within the scope of the requesting party.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the human-readable name for this presentation definition.
    /// Optional. Should be descriptive for user interfaces.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the purpose or reason for requesting these credentials.
    /// Optional. Helps users understand why credentials are being requested.
    /// </summary>
    [JsonPropertyName("purpose")]
    public string? Purpose { get; set; }

    /// <summary>
    /// Gets or sets the format constraints for acceptable credentials.
    /// Optional. Specifies which credential formats are acceptable (e.g., SD-JWT, JWT-VC).
    /// </summary>
    [JsonPropertyName("format")]
    public FormatConstraints? Format { get; set; }

    /// <summary>
    /// Gets or sets the submission requirements that define how input descriptors should be satisfied.
    /// Optional. If not specified, all input descriptors must be satisfied.
    /// </summary>
    [JsonPropertyName("submission_requirements")]
    public SubmissionRequirement[]? SubmissionRequirements { get; set; }

    /// <summary>
    /// Gets or sets the input descriptors that specify the required credential characteristics.
    /// Required. Must contain at least one input descriptor.
    /// </summary>
    [JsonPropertyName("input_descriptors")]
    public InputDescriptor[] InputDescriptors { get; set; } = Array.Empty<InputDescriptor>();

    /// <summary>
    /// Validates the presentation definition according to DIF PEX 2.1.1 requirements.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the definition is invalid</exception>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Id))
            throw new InvalidOperationException("Presentation definition ID is required");

        if (InputDescriptors == null || InputDescriptors.Length == 0)
            throw new InvalidOperationException("At least one input descriptor is required");

        // Validate all input descriptors
        foreach (var descriptor in InputDescriptors)
        {
            descriptor?.Validate();
        }

        // Validate submission requirements if present
        if (SubmissionRequirements != null)
        {
            foreach (var requirement in SubmissionRequirements)
            {
                requirement?.Validate();
            }

            // Check that all referenced input descriptors exist
            ValidateSubmissionRequirementReferences();
        }

        // Validate format constraints if present
        Format?.Validate();
    }

    /// <summary>
    /// Validates that submission requirements reference existing input descriptors.
    /// </summary>
    private void ValidateSubmissionRequirementReferences()
    {
        var descriptorIds = InputDescriptors.Select(d => d.Id).ToHashSet();

        foreach (var requirement in SubmissionRequirements!)
        {
            ValidateSubmissionRequirement(requirement, descriptorIds);
        }
    }

    /// <summary>
    /// Recursively validates submission requirement references.
    /// </summary>
    /// <param name="requirement">The requirement to validate</param>
    /// <param name="descriptorIds">Available input descriptor IDs</param>
    private void ValidateSubmissionRequirement(SubmissionRequirement requirement, HashSet<string> descriptorIds)
    {
        if (requirement.From != null)
        {
            if (!descriptorIds.Contains(requirement.From))
                throw new InvalidOperationException($"Submission requirement references unknown input descriptor: {requirement.From}");
        }

        if (requirement.FromNested != null)
        {
            foreach (var nested in requirement.FromNested)
            {
                ValidateSubmissionRequirement(nested, descriptorIds);
            }
        }
    }

    /// <summary>
    /// Gets all input descriptor IDs referenced by submission requirements.
    /// </summary>
    /// <returns>Set of input descriptor IDs that are referenced</returns>
    public HashSet<string> GetReferencedDescriptorIds()
    {
        var referenced = new HashSet<string>();

        if (SubmissionRequirements != null)
        {
            foreach (var requirement in SubmissionRequirements)
            {
                CollectReferencedIds(requirement, referenced);
            }
        }
        else
        {
            // If no submission requirements, all descriptors are referenced
            foreach (var descriptor in InputDescriptors)
            {
                referenced.Add(descriptor.Id);
            }
        }

        return referenced;
    }

    /// <summary>
    /// Recursively collects referenced input descriptor IDs from submission requirements.
    /// </summary>
    /// <param name="requirement">The requirement to process</param>
    /// <param name="referenced">Set to collect referenced IDs</param>
    private void CollectReferencedIds(SubmissionRequirement requirement, HashSet<string> referenced)
    {
        if (requirement.From != null)
        {
            referenced.Add(requirement.From);
        }

        if (requirement.FromNested != null)
        {
            foreach (var nested in requirement.FromNested)
            {
                CollectReferencedIds(nested, referenced);
            }
        }
    }

    /// <summary>
    /// Creates a basic presentation definition with required fields.
    /// </summary>
    /// <param name="id">The unique identifier for the definition</param>
    /// <param name="inputDescriptors">The input descriptors specifying requirements</param>
    /// <param name="name">Optional human-readable name</param>
    /// <param name="purpose">Optional purpose description</param>
    /// <returns>A new PresentationDefinition instance</returns>
    public static PresentationDefinition Create(
        string id, 
        InputDescriptor[] inputDescriptors, 
        string? name = null, 
        string? purpose = null)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Presentation definition ID cannot be null or empty", nameof(id));

        if (inputDescriptors == null || inputDescriptors.Length == 0)
            throw new ArgumentException("At least one input descriptor is required", nameof(inputDescriptors));

        return new PresentationDefinition
        {
            Id = id,
            Name = name,
            Purpose = purpose,
            InputDescriptors = inputDescriptors
        };
    }

    /// <summary>
    /// Gets input descriptor by ID.
    /// </summary>
    /// <param name="descriptorId">The input descriptor ID</param>
    /// <returns>The input descriptor or null if not found</returns>
    public InputDescriptor? GetInputDescriptor(string descriptorId)
    {
        return InputDescriptors.FirstOrDefault(d => d.Id == descriptorId);
    }

    /// <summary>
    /// Checks if the presentation definition requires all input descriptors to be satisfied.
    /// </summary>
    /// <returns>True if all descriptors must be satisfied</returns>
    public bool RequiresAllDescriptors()
    {
        if (SubmissionRequirements == null || SubmissionRequirements.Length == 0)
            return true;

        // If there's only one submission requirement with rule "all" and no count, then all are required
        if (SubmissionRequirements.Length == 1)
        {
            var requirement = SubmissionRequirements[0];
            return requirement.Rule == PresentationExchangeConstants.SubmissionRules.All && 
                   requirement.Count == null &&
                   requirement.Min == null &&
                   requirement.Max == null;
        }

        return false;
    }
}