using System.Text.Json.Serialization;

namespace SdJwt.Net.Oid4Vp.Models;

/// <summary>
/// Represents a Field constraint according to DIF Presentation Exchange v2.0.0.
/// Describes a specific field requirement within a credential.
/// </summary>
public class Field
{
    /// <summary>
    /// Gets or sets the path to the field in the credential.
    /// REQUIRED. JSONPath expression identifying the field.
    /// </summary>
    [JsonPropertyName("path")]
    public string[] Path { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets the unique identifier for this field.
    /// OPTIONAL. Identifier to be used in the Presentation Submission.
    /// </summary>
    [JsonPropertyName("id")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public string? Id { get; set; }

    /// <summary>
    /// Gets or sets the purpose of this field constraint.
    /// OPTIONAL. Describes why this field is being requested.
    /// </summary>
    [JsonPropertyName("purpose")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public string? Purpose { get; set; }

    /// <summary>
    /// Gets or sets the name of this field.
    /// OPTIONAL. Human-friendly name for this field.
    /// </summary>
    [JsonPropertyName("name")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the filter for this field.
    /// OPTIONAL. JSON Schema object to validate the field value.
    /// </summary>
    [JsonPropertyName("filter")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public Dictionary<string, object>? Filter { get; set; }

    /// <summary>
    /// Gets or sets whether this field is optional.
    /// OPTIONAL. If true, the field is not required to be present.
    /// </summary>
    [JsonPropertyName("optional")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public bool? Optional { get; set; }

    /// <summary>
    /// Gets or sets the intent to retain flag.
    /// OPTIONAL. Indicates whether the Verifier intends to retain the field value.
    /// </summary>
    [JsonPropertyName("intent_to_retain")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public bool? IntentToRetain { get; set; }

    /// <summary>
    /// Creates a field constraint for the credential type (vct).
    /// </summary>
    /// <param name="credentialType">The expected credential type value</param>
    /// <returns>A new Field instance</returns>
    public static Field CreateForCredentialType(string credentialType)
    {
#if NET6_0_OR_GREATER
        ArgumentException.ThrowIfNullOrWhiteSpace(credentialType);
#else
        if (string.IsNullOrWhiteSpace(credentialType))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(credentialType));
#endif

        return new Field
        {
            Path = new[] { Oid4VpConstants.JsonPaths.CredentialType },
            Filter = new Dictionary<string, object>
            {
                ["type"] = "string",
                ["const"] = credentialType
            }
        };
    }

    /// <summary>
    /// Creates a field constraint for the issuer.
    /// </summary>
    /// <param name="issuer">The expected issuer value</param>
    /// <param name="purpose">Optional purpose description</param>
    /// <returns>A new Field instance</returns>
    public static Field CreateForIssuer(string issuer, string? purpose = null)
    {
#if NET6_0_OR_GREATER
        ArgumentException.ThrowIfNullOrWhiteSpace(issuer);
#else
        if (string.IsNullOrWhiteSpace(issuer))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(issuer));
#endif

        return new Field
        {
            Path = new[] { Oid4VpConstants.JsonPaths.Issuer },
            Purpose = purpose ?? "Verify the credential issuer",
            Filter = new Dictionary<string, object>
            {
                ["type"] = "string",
                ["const"] = issuer
            }
        };
    }

    /// <summary>
    /// Creates a field constraint for the subject.
    /// </summary>
    /// <param name="subject">The expected subject value</param>
    /// <param name="purpose">Optional purpose description</param>
    /// <returns>A new Field instance</returns>
    public static Field CreateForSubject(string subject, string? purpose = null)
    {
#if NET6_0_OR_GREATER
        ArgumentException.ThrowIfNullOrWhiteSpace(subject);
#else
        if (string.IsNullOrWhiteSpace(subject))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(subject));
#endif

        return new Field
        {
            Path = new[] { Oid4VpConstants.JsonPaths.Subject },
            Purpose = purpose ?? "Verify the credential subject",
            Filter = new Dictionary<string, object>
            {
                ["type"] = "string",
                ["const"] = subject
            }
        };
    }

    /// <summary>
    /// Creates a field constraint for a custom JSONPath with string type.
    /// </summary>
    /// <param name="jsonPath">The JSONPath expression</param>
    /// <param name="name">Optional human-friendly name</param>
    /// <param name="purpose">Optional purpose description</param>
    /// <param name="optional">Whether this field is optional</param>
    /// <returns>A new Field instance</returns>
    public static Field CreateForPath(
        string jsonPath, 
        string? name = null, 
        string? purpose = null, 
        bool? optional = null)
    {
#if NET6_0_OR_GREATER
        ArgumentException.ThrowIfNullOrWhiteSpace(jsonPath);
#else
        if (string.IsNullOrWhiteSpace(jsonPath))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(jsonPath));
#endif

        return new Field
        {
            Path = new[] { jsonPath },
            Name = name,
            Purpose = purpose,
            Optional = optional,
            Filter = new Dictionary<string, object>
            {
                ["type"] = "string"
            }
        };
    }

    /// <summary>
    /// Creates a field constraint with a specific filter.
    /// </summary>
    /// <param name="jsonPath">The JSONPath expression</param>
    /// <param name="filter">The JSON Schema filter object</param>
    /// <param name="name">Optional human-friendly name</param>
    /// <param name="purpose">Optional purpose description</param>
    /// <param name="optional">Whether this field is optional</param>
    /// <returns>A new Field instance</returns>
    public static Field CreateWithFilter(
        string jsonPath,
        Dictionary<string, object> filter,
        string? name = null,
        string? purpose = null,
        bool? optional = null)
    {
#if NET6_0_OR_GREATER
        ArgumentException.ThrowIfNullOrWhiteSpace(jsonPath);
        ArgumentNullException.ThrowIfNull(filter);
#else
        if (string.IsNullOrWhiteSpace(jsonPath))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(jsonPath));
        if (filter == null)
            throw new ArgumentNullException(nameof(filter));
#endif

        return new Field
        {
            Path = new[] { jsonPath },
            Name = name,
            Purpose = purpose,
            Optional = optional,
            Filter = filter
        };
    }

    /// <summary>
    /// Adds a string constraint to the filter.
    /// </summary>
    /// <param name="value">The expected string value</param>
    /// <returns>This Field for method chaining</returns>
    public Field WithStringValue(string value)
    {
#if NET6_0_OR_GREATER
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
#else
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(value));
#endif

        Filter ??= new Dictionary<string, object>();
        Filter["type"] = "string";
        Filter["const"] = value;
        return this;
    }

    /// <summary>
    /// Adds an array constraint to the filter.
    /// </summary>
    /// <param name="allowedValues">The allowed string values</param>
    /// <returns>This Field for method chaining</returns>
    public Field WithStringEnum(params string[] allowedValues)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(allowedValues);
#else
        if (allowedValues == null)
            throw new ArgumentNullException(nameof(allowedValues));
#endif

        if (allowedValues.Length == 0)
            throw new ArgumentException("At least one value is required", nameof(allowedValues));

        Filter ??= new Dictionary<string, object>();
        Filter["type"] = "string";
        Filter["enum"] = allowedValues;
        return this;
    }

    /// <summary>
    /// Marks this field as optional.
    /// </summary>
    /// <param name="optional">Whether this field is optional (default: true)</param>
    /// <returns>This Field for method chaining</returns>
    public Field AsOptional(bool optional = true)
    {
        Optional = optional;
        return this;
    }

    /// <summary>
    /// Sets the intent to retain flag.
    /// </summary>
    /// <param name="retain">Whether the verifier intends to retain this field value</param>
    /// <returns>This Field for method chaining</returns>
    public Field WithIntentToRetain(bool retain = true)
    {
        IntentToRetain = retain;
        return this;
    }

    /// <summary>
    /// Validates this field constraint according to DIF Presentation Exchange requirements.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the field is invalid</exception>
    public void Validate()
    {
        if (Path == null || Path.Length == 0)
        {
            throw new InvalidOperationException("Field path is required");
        }

        foreach (var path in Path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new InvalidOperationException("Field path cannot contain null or empty values");
            }
        }
    }
}