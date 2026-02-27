using System.Text.Json.Serialization;

namespace SdJwt.Net.Oid4Vp.Models;

/// <summary>
/// Represents a Presentation Submission according to DIF Presentation Exchange v2.0.0.
/// Maps the credentials provided to the requirements in the Presentation Definition.
/// </summary>
public class PresentationSubmission
{
    /// <summary>
    /// Gets or sets the unique identifier for this submission.
    /// REQUIRED. Must match the id of the Presentation Definition.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the definition ID.
    /// REQUIRED. Must match the id of the Presentation Definition that this submission responds to.
    /// </summary>
    [JsonPropertyName("definition_id")]
    public string DefinitionId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the descriptor map.
    /// REQUIRED. Array of Input Descriptor Mapping Objects.
    /// </summary>
    [JsonPropertyName("descriptor_map")]
    public InputDescriptorMapping[] DescriptorMap { get; set; } = Array.Empty<InputDescriptorMapping>();

    /// <summary>
    /// Creates a presentation submission with a single credential mapping.
    /// </summary>
    /// <param name="id">Unique identifier for this submission</param>
    /// <param name="definitionId">ID of the presentation definition</param>
    /// <param name="inputDescriptorId">ID of the input descriptor</param>
    /// <param name="format">Format of the credential (e.g., "vc+sd-jwt")</param>
    /// <param name="path">Path to the credential in the VP token array</param>
    /// <returns>A new PresentationSubmission instance</returns>
    public static PresentationSubmission CreateSingle(
        string id,
        string definitionId,
        string inputDescriptorId,
        string format,
        string path = "$")
    {
#if NET6_0_OR_GREATER
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(definitionId);
        ArgumentException.ThrowIfNullOrWhiteSpace(inputDescriptorId);
        ArgumentException.ThrowIfNullOrWhiteSpace(format);
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
#else
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(id));
        if (string.IsNullOrWhiteSpace(definitionId))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(definitionId));
        if (string.IsNullOrWhiteSpace(inputDescriptorId))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(inputDescriptorId));
        if (string.IsNullOrWhiteSpace(format))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(format));
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(path));
#endif

        return new PresentationSubmission
        {
            Id = id,
            DefinitionId = definitionId,
            DescriptorMap = new[]
            {
                InputDescriptorMapping.Create(inputDescriptorId, format, path)
            }
        };
    }

    /// <summary>
    /// Creates a presentation submission for multiple credentials.
    /// </summary>
    /// <param name="id">Unique identifier for this submission</param>
    /// <param name="definitionId">ID of the presentation definition</param>
    /// <param name="mappings">Array of input descriptor mappings</param>
    /// <returns>A new PresentationSubmission instance</returns>
    public static PresentationSubmission CreateMultiple(
        string id,
        string definitionId,
        params InputDescriptorMapping[] mappings)
    {
#if NET6_0_OR_GREATER
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(definitionId);
        ArgumentNullException.ThrowIfNull(mappings);
#else
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(id));
        if (string.IsNullOrWhiteSpace(definitionId))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(definitionId));
        if (mappings == null)
            throw new ArgumentNullException(nameof(mappings));
#endif

        if (mappings.Length == 0)
            throw new ArgumentException("At least one mapping is required", nameof(mappings));

        return new PresentationSubmission
        {
            Id = id,
            DefinitionId = definitionId,
            DescriptorMap = mappings
        };
    }

    /// <summary>
    /// Adds an input descriptor mapping to this submission.
    /// </summary>
    /// <param name="mapping">The mapping to add</param>
    /// <returns>This PresentationSubmission for method chaining</returns>
    public PresentationSubmission WithMapping(InputDescriptorMapping mapping)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(mapping);
#else
        if (mapping == null)
            throw new ArgumentNullException(nameof(mapping));
#endif

        var currentMappings = DescriptorMap?.ToList() ?? new List<InputDescriptorMapping>();
        currentMappings.Add(mapping);
        DescriptorMap = currentMappings.ToArray();
        return this;
    }

    /// <summary>
    /// Validates this presentation submission according to DIF Presentation Exchange requirements.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the submission is invalid</exception>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Id))
        {
            throw new InvalidOperationException("Presentation submission id is required");
        }

        if (string.IsNullOrWhiteSpace(DefinitionId))
        {
            throw new InvalidOperationException("Presentation submission definition_id is required");
        }

        if (DescriptorMap == null || DescriptorMap.Length == 0)
        {
            throw new InvalidOperationException("At least one descriptor mapping is required");
        }

        // Validate each mapping
        foreach (var mapping in DescriptorMap)
        {
            mapping?.Validate();
        }

        // Check for duplicate input descriptor IDs
        var ids = DescriptorMap.Select(m => m.Id).ToList();
        var duplicates = ids.GroupBy(id => id).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
        if (duplicates.Count > 0)
        {
            throw new InvalidOperationException($"Duplicate input descriptor IDs found: {string.Join(", ", duplicates)}");
        }
    }
}
