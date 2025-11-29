using System.Text.Json.Serialization;

namespace SdJwt.Net.Oid4Vp.Models;

/// <summary>
/// Represents an Input Descriptor according to DIF Presentation Exchange v2.0.0.
/// Describes a set of requirements that must be satisfied by the Credential.
/// </summary>
public class InputDescriptor
{
    /// <summary>
    /// Gets or sets the unique identifier for this Input Descriptor.
    /// REQUIRED. Must be a string that uniquely identifies this descriptor within the Presentation Definition.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of this Input Descriptor.
    /// OPTIONAL. Human-friendly name that describes this Input Descriptor.
    /// </summary>
    [JsonPropertyName("name")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the purpose of this Input Descriptor.
    /// OPTIONAL. Describes the purpose for which this Input Descriptor exists.
    /// </summary>
    [JsonPropertyName("purpose")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public string? Purpose { get; set; }

    /// <summary>
    /// Gets or sets the format restrictions for this Input Descriptor.
    /// OPTIONAL. Objects with requirements a Verifier has for the format of the Credential.
    /// </summary>
    [JsonPropertyName("format")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public Dictionary<string, object>? Format { get; set; }

    /// <summary>
    /// Gets or sets the constraints for this Input Descriptor.
    /// OPTIONAL. Object containing requirements the Verifier has for the Credential.
    /// </summary>
    [JsonPropertyName("constraints")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public Constraints? Constraints { get; set; }

    /// <summary>
    /// Creates an Input Descriptor for a specific credential type.
    /// </summary>
    /// <param name="id">Unique identifier for this descriptor</param>
    /// <param name="credentialType">The credential type being requested</param>
    /// <param name="name">Optional human-friendly name</param>
    /// <param name="purpose">Optional purpose description</param>
    /// <returns>A new InputDescriptor instance</returns>
    public static InputDescriptor CreateForCredentialType(
        string id,
        string credentialType,
        string? name = null,
        string? purpose = null)
    {
#if NET6_0_OR_GREATER
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(credentialType);
#else
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(id));
        if (string.IsNullOrWhiteSpace(credentialType))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(credentialType));
#endif

        return new InputDescriptor
        {
            Id = id,
            Name = name ?? $"{credentialType} Credential",
            Purpose = purpose,
            Constraints = new Constraints
            {
                Fields = new[]
                {
                    Field.CreateForCredentialType(credentialType)
                }
            }
        };
    }

    /// <summary>
    /// Adds format restrictions for SD-JWT credentials.
    /// </summary>
    /// <param name="algorithms">Optional array of allowed signing algorithms</param>
    /// <returns>This InputDescriptor for method chaining</returns>
    public InputDescriptor WithSdJwtFormat(string[]? algorithms = null)
    {
        Format ??= new Dictionary<string, object>();
        
        var formatConstraints = new Dictionary<string, object>();
        if (algorithms != null && algorithms.Length > 0)
        {
            formatConstraints["alg"] = algorithms;
        }

        Format[Oid4VpConstants.SdJwtVcFormat] = formatConstraints;
        return this;
    }

    /// <summary>
    /// Adds a constraint field to this input descriptor.
    /// </summary>
    /// <param name="field">The field constraint to add</param>
    /// <returns>This InputDescriptor for method chaining</returns>
    public InputDescriptor WithField(Field field)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(field);
#else
        if (field == null)
            throw new ArgumentNullException(nameof(field));
#endif

        Constraints ??= new Constraints();
        
        var currentFields = Constraints.Fields?.ToList() ?? new List<Field>();
        currentFields.Add(field);
        Constraints.Fields = currentFields.ToArray();
        
        return this;
    }

    /// <summary>
    /// Adds multiple constraint fields to this input descriptor.
    /// </summary>
    /// <param name="fields">The field constraints to add</param>
    /// <returns>This InputDescriptor for method chaining</returns>
    public InputDescriptor WithFields(params Field[] fields)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(fields);
#else
        if (fields == null)
            throw new ArgumentNullException(nameof(fields));
#endif

        foreach (var field in fields)
        {
            WithField(field);
        }
        
        return this;
    }

    /// <summary>
    /// Validates this input descriptor according to DIF Presentation Exchange requirements.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the descriptor is invalid</exception>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Id))
        {
            throw new InvalidOperationException("Input descriptor id is required");
        }

        // Validate constraints if present
        Constraints?.Validate();
    }
}