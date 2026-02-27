using System.Text.Json.Serialization;

namespace SdJwt.Net.Oid4Vp.Models;

/// <summary>
/// Represents an Input Descriptor Mapping according to DIF Presentation Exchange v2.0.0.
/// Maps a credential to an Input Descriptor requirement.
/// </summary>
public class InputDescriptorMapping
{
    /// <summary>
    /// Gets or sets the input descriptor ID.
    /// REQUIRED. Must match the id of an Input Descriptor in the Presentation Definition.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the format of the credential.
    /// REQUIRED. The format identifier (e.g., "vc+sd-jwt").
    /// </summary>
    [JsonPropertyName("format")]
    public string Format { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the path to the credential.
    /// REQUIRED. JSONPath expression pointing to the credential in the VP token array.
    /// </summary>
    [JsonPropertyName("path")]
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the path nested descriptor.
    /// OPTIONAL. Object for expressing paths nested within the located credential.
    /// </summary>
    [JsonPropertyName("path_nested")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public PathNestedDescriptor? PathNested
    {
        get; set;
    }

    /// <summary>
    /// Creates an input descriptor mapping.
    /// </summary>
    /// <param name="id">The input descriptor ID</param>
    /// <param name="format">The credential format</param>
    /// <param name="path">The JSONPath to the credential</param>
    /// <returns>A new InputDescriptorMapping instance</returns>
    public static InputDescriptorMapping Create(
        string id,
        string format,
        string path = "$")
    {
#if NET6_0_OR_GREATER
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(format);
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
#else
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(id));
        if (string.IsNullOrWhiteSpace(format))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(format));
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(path));
#endif

        return new InputDescriptorMapping
        {
            Id = id,
            Format = format,
            Path = path
        };
    }

    /// <summary>
    /// Creates an input descriptor mapping for a single SD-JWT credential.
    /// </summary>
    /// <param name="inputDescriptorId">The input descriptor ID</param>
    /// <param name="index">Optional index in VP token array (default: 0)</param>
    /// <returns>A new InputDescriptorMapping instance</returns>
    public static InputDescriptorMapping CreateForSdJwt(
        string inputDescriptorId,
        int index = 0)
    {
#if NET6_0_OR_GREATER
        ArgumentException.ThrowIfNullOrWhiteSpace(inputDescriptorId);
#else
        if (string.IsNullOrWhiteSpace(inputDescriptorId))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(inputDescriptorId));
#endif

        if (index < 0)
            throw new ArgumentException("Index must be non-negative", nameof(index));

        var path = index == 0 ? "$" : $"$[{index}]";

        return new InputDescriptorMapping
        {
            Id = inputDescriptorId,
            Format = Oid4VpConstants.SdJwtVcFormat,
            Path = path
        };
    }

    /// <summary>
    /// Adds a path nested descriptor to this mapping.
    /// </summary>
    /// <param name="pathNested">The path nested descriptor</param>
    /// <returns>This InputDescriptorMapping for method chaining</returns>
    public InputDescriptorMapping WithPathNested(PathNestedDescriptor pathNested)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(pathNested);
#else
        if (pathNested == null)
            throw new ArgumentNullException(nameof(pathNested));
#endif

        PathNested = pathNested;
        return this;
    }

    /// <summary>
    /// Validates this input descriptor mapping according to DIF Presentation Exchange requirements.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the mapping is invalid</exception>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Id))
        {
            throw new InvalidOperationException("Input descriptor mapping id is required");
        }

        if (string.IsNullOrWhiteSpace(Format))
        {
            throw new InvalidOperationException("Input descriptor mapping format is required");
        }

        if (string.IsNullOrWhiteSpace(Path))
        {
            throw new InvalidOperationException("Input descriptor mapping path is required");
        }

        // Validate path nested if present
        PathNested?.Validate();
    }
}

/// <summary>
/// Represents a Path Nested Descriptor according to DIF Presentation Exchange v2.0.0.
/// For expressing paths nested within the located credential.
/// </summary>
public class PathNestedDescriptor
{
    /// <summary>
    /// Gets or sets the format of the nested object.
    /// OPTIONAL. The format identifier for the nested object.
    /// </summary>
    [JsonPropertyName("format")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public string? Format
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the path to the nested object.
    /// REQUIRED. JSONPath expression pointing to the nested object.
    /// </summary>
    [JsonPropertyName("path")]
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets further nested path descriptors.
    /// OPTIONAL. Additional nesting levels.
    /// </summary>
    [JsonPropertyName("path_nested")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public PathNestedDescriptor? PathNested
    {
        get; set;
    }

    /// <summary>
    /// Creates a path nested descriptor.
    /// </summary>
    /// <param name="path">The JSONPath to the nested object</param>
    /// <param name="format">Optional format identifier</param>
    /// <returns>A new PathNestedDescriptor instance</returns>
    public static PathNestedDescriptor Create(string path, string? format = null)
    {
#if NET6_0_OR_GREATER
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
#else
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(path));
#endif

        return new PathNestedDescriptor
        {
            Path = path,
            Format = format
        };
    }

    /// <summary>
    /// Adds a further nested path descriptor.
    /// </summary>
    /// <param name="pathNested">The nested descriptor</param>
    /// <returns>This PathNestedDescriptor for method chaining</returns>
    public PathNestedDescriptor WithPathNested(PathNestedDescriptor pathNested)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(pathNested);
#else
        if (pathNested == null)
            throw new ArgumentNullException(nameof(pathNested));
#endif

        PathNested = pathNested;
        return this;
    }

    /// <summary>
    /// Validates this path nested descriptor.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the descriptor is invalid</exception>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Path))
        {
            throw new InvalidOperationException("Path nested descriptor path is required");
        }

        // Validate further nested descriptors
        PathNested?.Validate();
    }
}
