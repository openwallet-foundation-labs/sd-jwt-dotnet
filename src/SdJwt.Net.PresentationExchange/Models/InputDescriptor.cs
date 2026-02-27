using System.Text.Json.Serialization;

namespace SdJwt.Net.PresentationExchange.Models;

/// <summary>
/// Represents an Input Descriptor as defined in DIF Presentation Exchange 2.1.1.
/// Describes the characteristics required for a specific credential.
/// </summary>
public class InputDescriptor
{
    /// <summary>
    /// Gets or sets the unique identifier for this input descriptor.
    /// Required. Must be unique within the presentation definition.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the human-readable name for this input descriptor.
    /// Optional. Should be descriptive for user interfaces.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the purpose or reason for requesting this specific credential.
    /// Optional. Helps users understand why this credential is being requested.
    /// </summary>
    [JsonPropertyName("purpose")]
    public string? Purpose
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the group identifier for organizing related input descriptors.
    /// Optional. Used for grouping in submission requirements.
    /// </summary>
    [JsonPropertyName("group")]
    public string[]? Group
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the format constraints for this input descriptor.
    /// Optional. Specifies which credential formats are acceptable.
    /// </summary>
    [JsonPropertyName("format")]
    public FormatConstraints? Format
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the constraints that define the required credential characteristics.
    /// Optional. If not specified, any credential matching the format is acceptable.
    /// </summary>
    [JsonPropertyName("constraints")]
    public Constraints? Constraints
    {
        get; set;
    }

    /// <summary>
    /// Validates the input descriptor according to DIF PEX 2.1.1 requirements.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the descriptor is invalid</exception>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Id))
            throw new InvalidOperationException("Input descriptor ID is required");

        // Validate constraints if present
        Constraints?.Validate();

        // Validate format constraints if present
        Format?.Validate();

        // Validate group identifiers if present
        if (Group != null)
        {
            foreach (var groupId in Group)
            {
                if (string.IsNullOrWhiteSpace(groupId))
                    throw new InvalidOperationException("Group identifier cannot be null or empty");
            }
        }
    }

    /// <summary>
    /// Creates a basic input descriptor with required fields.
    /// </summary>
    /// <param name="id">The unique identifier for the descriptor</param>
    /// <param name="name">Optional human-readable name</param>
    /// <param name="purpose">Optional purpose description</param>
    /// <returns>A new InputDescriptor instance</returns>
    public static InputDescriptor Create(string id, string? name = null, string? purpose = null)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Input descriptor ID cannot be null or empty", nameof(id));

        return new InputDescriptor
        {
            Id = id,
            Name = name,
            Purpose = purpose
        };
    }

    /// <summary>
    /// Creates an input descriptor with format constraints.
    /// </summary>
    /// <param name="id">The unique identifier for the descriptor</param>
    /// <param name="formatConstraints">The format constraints to apply</param>
    /// <param name="name">Optional human-readable name</param>
    /// <param name="purpose">Optional purpose description</param>
    /// <returns>A new InputDescriptor instance</returns>
    public static InputDescriptor CreateWithFormat(
        string id,
        FormatConstraints formatConstraints,
        string? name = null,
        string? purpose = null)
    {
        var descriptor = Create(id, name, purpose);
        descriptor.Format = formatConstraints;
        return descriptor;
    }

    /// <summary>
    /// Creates an input descriptor with constraints.
    /// </summary>
    /// <param name="id">The unique identifier for the descriptor</param>
    /// <param name="constraints">The constraints to apply</param>
    /// <param name="name">Optional human-readable name</param>
    /// <param name="purpose">Optional purpose description</param>
    /// <returns>A new InputDescriptor instance</returns>
    public static InputDescriptor CreateWithConstraints(
        string id,
        Constraints constraints,
        string? name = null,
        string? purpose = null)
    {
        var descriptor = Create(id, name, purpose);
        descriptor.Constraints = constraints;
        return descriptor;
    }

    /// <summary>
    /// Creates an input descriptor for SD-JWT credentials.
    /// </summary>
    /// <param name="id">The unique identifier for the descriptor</param>
    /// <param name="vctType">The verifiable credential type (vct) to require</param>
    /// <param name="name">Optional human-readable name</param>
    /// <param name="purpose">Optional purpose description</param>
    /// <returns>A new InputDescriptor instance configured for SD-JWT</returns>
    public static InputDescriptor CreateForSdJwt(
        string id,
        string vctType,
        string? name = null,
        string? purpose = null)
    {
        if (string.IsNullOrWhiteSpace(vctType))
            throw new ArgumentException("VCT type cannot be null or empty", nameof(vctType));

        var descriptor = Create(id, name, purpose);

        // Set format to SD-JWT VC
        descriptor.Format = new FormatConstraints
        {
            SdJwtVc = new SdJwtFormatConstraints()
        };

        // Add constraint for the VCT type
        descriptor.Constraints = new Constraints
        {
            Fields = new[]
            {
                new Field
                {
                    Path = new[] { PresentationExchangeConstants.CommonJsonPaths.VctType },
                    Filter = new FieldFilter
                    {
                        Type = "string",
                        Const = vctType
                    }
                }
            }
        };

        return descriptor;
    }

    /// <summary>
    /// Creates an input descriptor for JWT VC credentials.
    /// </summary>
    /// <param name="id">The unique identifier for the descriptor</param>
    /// <param name="credentialTypes">The credential types to require</param>
    /// <param name="name">Optional human-readable name</param>
    /// <param name="purpose">Optional purpose description</param>
    /// <returns>A new InputDescriptor instance configured for JWT VC</returns>
    public static InputDescriptor CreateForJwtVc(
        string id,
        string[] credentialTypes,
        string? name = null,
        string? purpose = null)
    {
        if (credentialTypes == null || credentialTypes.Length == 0)
            throw new ArgumentException("At least one credential type is required", nameof(credentialTypes));

        var descriptor = Create(id, name, purpose);

        // Set format to JWT VC
        descriptor.Format = new FormatConstraints
        {
            JwtVc = new JwtFormatConstraints()
        };

        // Add constraint for the credential types
        // For JWT VC, we expect vc.type to be an array containing the credential type
        descriptor.Constraints = new Constraints
        {
            Fields = new[]
            {
                new Field
                {
                    Path = new[] { PresentationExchangeConstants.CommonJsonPaths.VcType },
                    Filter = new FieldFilter
                    {
                        Type = "array",
                        // For a single credential type, just check if the array contains that type
                        Contains = credentialTypes[0] // Use the string directly, not wrapped in an object
                    }
                }
            }
        };

        return descriptor;
    }

    /// <summary>
    /// Checks if this input descriptor belongs to any of the specified groups.
    /// </summary>
    /// <param name="groupIds">The group IDs to check</param>
    /// <returns>True if the descriptor belongs to any of the specified groups</returns>
    public bool BelongsToGroup(params string[] groupIds)
    {
        if (Group == null || groupIds == null || groupIds.Length == 0)
            return false;

        return Group.Any(g => groupIds.Contains(g));
    }

    /// <summary>
    /// Gets all format identifiers accepted by this input descriptor.
    /// </summary>
    /// <returns>Array of accepted format identifiers</returns>
    public string[] GetAcceptedFormats()
    {
        if (Format == null)
            return PresentationExchangeConstants.Formats.All;

        var formats = new List<string>();

        if (Format.Jwt != null)
            formats.Add(PresentationExchangeConstants.Formats.Jwt);
        if (Format.JwtVc != null)
            formats.Add(PresentationExchangeConstants.Formats.JwtVc);
        if (Format.JwtVp != null)
            formats.Add(PresentationExchangeConstants.Formats.JwtVp);
        if (Format.SdJwt != null)
            formats.Add(PresentationExchangeConstants.Formats.SdJwt);
        if (Format.SdJwtVc != null)
            formats.Add(PresentationExchangeConstants.Formats.SdJwtVc);
        if (Format.Ldp != null)
            formats.Add(PresentationExchangeConstants.Formats.Ldp);
        if (Format.LdpVc != null)
            formats.Add(PresentationExchangeConstants.Formats.LdpVc);
        if (Format.LdpVp != null)
            formats.Add(PresentationExchangeConstants.Formats.LdpVp);

        return formats.ToArray();
    }
}
