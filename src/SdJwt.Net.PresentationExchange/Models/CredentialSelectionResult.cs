using System.Text.Json.Serialization;

namespace SdJwt.Net.PresentationExchange.Models;

/// <summary>
/// Represents the result of credential selection from presentation exchange.
/// Contains matched credentials and metadata about the selection process.
/// </summary>
public class CredentialSelectionResult
{
    /// <summary>
    /// Gets or sets the selected credentials that match the presentation definition.
    /// Will be empty if no matching credentials were found.
    /// </summary>
    public SelectedCredential[] SelectedCredentials { get; set; } = Array.Empty<SelectedCredential>();

    /// <summary>
    /// Gets or sets the presentation submission that describes how the selection satisfies the definition.
    /// Will be null if no valid selection could be made.
    /// </summary>
    public PresentationSubmission? PresentationSubmission
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the credential selection was successful.
    /// </summary>
    public bool IsSuccessful
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets any errors that occurred during credential selection.
    /// Will be empty if the selection was successful.
    /// </summary>
    public CredentialSelectionError[] Errors { get; set; } = Array.Empty<CredentialSelectionError>();

    /// <summary>
    /// Gets or sets any warnings generated during credential selection.
    /// These don't prevent successful selection but may indicate potential issues.
    /// </summary>
    public CredentialSelectionWarning[] Warnings { get; set; } = Array.Empty<CredentialSelectionWarning>();

    /// <summary>
    /// Gets or sets metadata about the selection process.
    /// </summary>
    public CredentialSelectionMetadata? Metadata
    {
        get; set;
    }

    /// <summary>
    /// Creates a successful credential selection result.
    /// </summary>
    /// <param name="selectedCredentials">The selected credentials</param>
    /// <param name="presentationSubmission">The presentation submission</param>
    /// <param name="metadata">Optional selection metadata</param>
    /// <returns>A successful CredentialSelectionResult</returns>
    public static CredentialSelectionResult Success(
        SelectedCredential[] selectedCredentials,
        PresentationSubmission presentationSubmission,
        CredentialSelectionMetadata? metadata = null)
    {
        return new CredentialSelectionResult
        {
            IsSuccessful = true,
            SelectedCredentials = selectedCredentials ?? Array.Empty<SelectedCredential>(),
            PresentationSubmission = presentationSubmission,
            Metadata = metadata ?? new CredentialSelectionMetadata()
        };
    }

    /// <summary>
    /// Creates a failed credential selection result.
    /// </summary>
    /// <param name="errors">The errors that caused the failure</param>
    /// <param name="metadata">Optional selection metadata</param>
    /// <returns>A failed CredentialSelectionResult</returns>
    public static CredentialSelectionResult Failure(
        CredentialSelectionError[] errors,
        CredentialSelectionMetadata? metadata = null)
    {
        return new CredentialSelectionResult
        {
            IsSuccessful = false,
            Errors = errors ?? Array.Empty<CredentialSelectionError>(),
            Metadata = metadata ?? new CredentialSelectionMetadata()
        };
    }

    /// <summary>
    /// Creates a failed credential selection result with a single error.
    /// </summary>
    /// <param name="errorCode">The error code</param>
    /// <param name="message">The error message</param>
    /// <param name="metadata">Optional selection metadata</param>
    /// <returns>A failed CredentialSelectionResult</returns>
    public static CredentialSelectionResult Failure(
        string errorCode,
        string message,
        CredentialSelectionMetadata? metadata = null)
    {
        var error = new CredentialSelectionError
        {
            Code = errorCode,
            Message = message
        };

        return Failure(new[] { error }, metadata);
    }

    /// <summary>
    /// Gets the total number of credentials selected.
    /// </summary>
    /// <returns>The number of selected credentials</returns>
    public int GetSelectedCount()
    {
        return SelectedCredentials?.Length ?? 0;
    }

    /// <summary>
    /// Gets credentials selected for a specific input descriptor.
    /// </summary>
    /// <param name="inputDescriptorId">The input descriptor ID</param>
    /// <returns>Array of credentials selected for the descriptor</returns>
    public SelectedCredential[] GetCredentialsForDescriptor(string inputDescriptorId)
    {
        if (SelectedCredentials == null)
            return Array.Empty<SelectedCredential>();

        return SelectedCredentials
            .Where(c => c.InputDescriptorId == inputDescriptorId)
            .ToArray();
    }

    /// <summary>
    /// Gets all input descriptors that have selected credentials.
    /// </summary>
    /// <returns>Array of input descriptor IDs that have selections</returns>
    public string[] GetSatisfiedDescriptorIds()
    {
        if (SelectedCredentials == null)
            return Array.Empty<string>();

        return SelectedCredentials
            .Select(c => c.InputDescriptorId)
            .Distinct()
            .ToArray();
    }

    /// <summary>
    /// Adds a warning to the result.
    /// </summary>
    /// <param name="warning">The warning to add</param>
    public void AddWarning(CredentialSelectionWarning warning)
    {
        if (warning == null)
            return;

        var warnings = Warnings?.ToList() ?? new List<CredentialSelectionWarning>();
        warnings.Add(warning);
        Warnings = warnings.ToArray();
    }

    /// <summary>
    /// Adds a warning to the result.
    /// </summary>
    /// <param name="code">The warning code</param>
    /// <param name="message">The warning message</param>
    /// <param name="inputDescriptorId">Optional input descriptor ID associated with the warning</param>
    public void AddWarning(string code, string message, string? inputDescriptorId = null)
    {
        AddWarning(new CredentialSelectionWarning
        {
            Code = code,
            Message = message,
            InputDescriptorId = inputDescriptorId
        });
    }
}

/// <summary>
/// Represents a selected credential with metadata about the selection.
/// </summary>
public class SelectedCredential
{
    /// <summary>
    /// Gets or sets the input descriptor ID that this credential satisfies.
    /// </summary>
    public string InputDescriptorId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the credential data (SD-JWT, JWT, etc.).
    /// </summary>
    public object Credential { get; set; } = new();

    /// <summary>
    /// Gets or sets the format of the credential.
    /// </summary>
    public string Format { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the path mapping for field requirements.
    /// Maps input descriptor field paths to actual credential paths.
    /// </summary>
    public Dictionary<string, string> PathMappings { get; set; } = new();

    /// <summary>
    /// Gets or sets the disclosures to include for selective disclosure credentials.
    /// Only applicable for SD-JWT credentials.
    /// </summary>
    public string[]? Disclosures
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets metadata about the credential selection.
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();

    /// <summary>
    /// Gets or sets the match confidence score (0.0 to 1.0).
    /// Higher scores indicate better matches to the input descriptor requirements.
    /// </summary>
    public double MatchScore
    {
        get; set;
    }

    /// <summary>
    /// Creates a selected credential for an SD-JWT.
    /// </summary>
    /// <param name="inputDescriptorId">The input descriptor ID</param>
    /// <param name="sdJwtCredential">The SD-JWT credential</param>
    /// <param name="disclosures">The disclosures to include</param>
    /// <param name="pathMappings">Optional path mappings</param>
    /// <param name="matchScore">Optional match score</param>
    /// <returns>A new SelectedCredential instance</returns>
    public static SelectedCredential ForSdJwt(
        string inputDescriptorId,
        object sdJwtCredential,
        string[]? disclosures = null,
        Dictionary<string, string>? pathMappings = null,
        double matchScore = 1.0)
    {
        return new SelectedCredential
        {
            InputDescriptorId = inputDescriptorId,
            Credential = sdJwtCredential,
            Format = PresentationExchangeConstants.Formats.SdJwtVc,
            Disclosures = disclosures,
            PathMappings = pathMappings ?? new Dictionary<string, string>(),
            MatchScore = matchScore
        };
    }

    /// <summary>
    /// Creates a selected credential for a JWT VC.
    /// </summary>
    /// <param name="inputDescriptorId">The input descriptor ID</param>
    /// <param name="jwtCredential">The JWT credential</param>
    /// <param name="pathMappings">Optional path mappings</param>
    /// <param name="matchScore">Optional match score</param>
    /// <returns>A new SelectedCredential instance</returns>
    public static SelectedCredential ForJwtVc(
        string inputDescriptorId,
        object jwtCredential,
        Dictionary<string, string>? pathMappings = null,
        double matchScore = 1.0)
    {
        return new SelectedCredential
        {
            InputDescriptorId = inputDescriptorId,
            Credential = jwtCredential,
            Format = PresentationExchangeConstants.Formats.JwtVc,
            PathMappings = pathMappings ?? new Dictionary<string, string>(),
            MatchScore = matchScore
        };
    }
}

/// <summary>
/// Represents an error that occurred during credential selection.
/// </summary>
public class CredentialSelectionError
{
    /// <summary>
    /// Gets or sets the error code.
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the error message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the input descriptor ID associated with this error.
    /// Optional. Used when the error is specific to a particular descriptor.
    /// </summary>
    public string? InputDescriptorId
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets additional error details.
    /// </summary>
    public Dictionary<string, object> Details { get; set; } = new();
}

/// <summary>
/// Represents a warning generated during credential selection.
/// </summary>
public class CredentialSelectionWarning
{
    /// <summary>
    /// Gets or sets the warning code.
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the warning message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the input descriptor ID associated with this warning.
    /// Optional. Used when the warning is specific to a particular descriptor.
    /// </summary>
    public string? InputDescriptorId
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets additional warning details.
    /// </summary>
    public Dictionary<string, object> Details { get; set; } = new();
}

/// <summary>
/// Contains metadata about the credential selection process.
/// </summary>
public class CredentialSelectionMetadata
{
    /// <summary>
    /// Gets or sets the timestamp when selection started.
    /// </summary>
    public DateTimeOffset SelectionStarted { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets or sets the timestamp when selection completed.
    /// </summary>
    public DateTimeOffset? SelectionCompleted
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the total duration of the selection process.
    /// </summary>
    public TimeSpan? SelectionDuration
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the total number of credentials evaluated.
    /// </summary>
    public int CredentialsEvaluated
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the number of credentials that passed initial format filtering.
    /// </summary>
    public int CredentialsPassedFormatFilter
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the number of credentials that passed constraint evaluation.
    /// </summary>
    public int CredentialsPassedConstraints
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets additional metadata properties.
    /// </summary>
    public Dictionary<string, object> AdditionalProperties { get; set; } = new();

    /// <summary>
    /// Marks the selection as completed and calculates duration.
    /// </summary>
    public void MarkCompleted()
    {
        SelectionCompleted = DateTimeOffset.UtcNow;
        SelectionDuration = SelectionCompleted - SelectionStarted;
    }

    /// <summary>
    /// Adds an additional property to the metadata.
    /// </summary>
    /// <param name="key">The property key</param>
    /// <param name="value">The property value</param>
    public void AddProperty(string key, object value)
    {
        AdditionalProperties[key] = value;
    }
}

/// <summary>
/// Represents a presentation submission as defined in DIF Presentation Exchange 2.1.1.
/// Describes how the presented credentials satisfy the presentation definition.
/// </summary>
public class PresentationSubmission
{
    /// <summary>
    /// Gets or sets the unique identifier for this presentation submission.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the identifier of the presentation definition being satisfied.
    /// </summary>
    [JsonPropertyName("definition_id")]
    public string DefinitionId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the descriptor mappings that show how credentials map to input descriptors.
    /// </summary>
    [JsonPropertyName("descriptor_map")]
    public InputDescriptorMapping[] DescriptorMap { get; set; } = Array.Empty<InputDescriptorMapping>();

    /// <summary>
    /// Validates the presentation submission.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the submission is invalid</exception>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Id))
            throw new InvalidOperationException("Presentation submission ID is required");

        if (string.IsNullOrWhiteSpace(DefinitionId))
            throw new InvalidOperationException("Definition ID is required");

        if (DescriptorMap == null || DescriptorMap.Length == 0)
            throw new InvalidOperationException("At least one descriptor mapping is required");

        foreach (var mapping in DescriptorMap)
        {
            mapping?.Validate();
        }
    }

    /// <summary>
    /// Creates a presentation submission.
    /// </summary>
    /// <param name="id">The submission ID</param>
    /// <param name="definitionId">The presentation definition ID</param>
    /// <param name="descriptorMap">The descriptor mappings</param>
    /// <returns>A new PresentationSubmission instance</returns>
    public static PresentationSubmission Create(string id, string definitionId, InputDescriptorMapping[] descriptorMap)
    {
        return new PresentationSubmission
        {
            Id = id,
            DefinitionId = definitionId,
            DescriptorMap = descriptorMap
        };
    }
}

/// <summary>
/// Represents a mapping between an input descriptor and presented credentials.
/// </summary>
public class InputDescriptorMapping
{
    /// <summary>
    /// Gets or sets the input descriptor ID.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the format of the presented credential.
    /// </summary>
    [JsonPropertyName("format")]
    public string Format { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the path to the credential in the presentation.
    /// </summary>
    [JsonPropertyName("path")]
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the path mappings for specific field requirements.
    /// Optional. Used when field paths in the credential differ from the descriptor paths.
    /// </summary>
    [JsonPropertyName("path_nested")]
    public PathMapping? PathNested
    {
        get; set;
    }

    /// <summary>
    /// Validates the input descriptor mapping.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the mapping is invalid</exception>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Id))
            throw new InvalidOperationException("Input descriptor mapping ID is required");

        if (string.IsNullOrWhiteSpace(Format))
            throw new InvalidOperationException("Format is required");

        if (string.IsNullOrWhiteSpace(Path))
            throw new InvalidOperationException("Path is required");

        PathNested?.Validate();
    }
}

/// <summary>
/// Represents nested path mappings for field requirements.
/// </summary>
public class PathMapping
{
    /// <summary>
    /// Gets or sets the field path mappings.
    /// </summary>
    [JsonPropertyName("path")]
    public string[]? Path
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the format of the nested data.
    /// </summary>
    [JsonPropertyName("format")]
    public string? Format
    {
        get; set;
    }

    /// <summary>
    /// Validates the path mapping.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the mapping is invalid</exception>
    public void Validate()
    {
        if (Path != null)
        {
            foreach (var path in Path)
            {
                if (string.IsNullOrWhiteSpace(path))
                    throw new InvalidOperationException("Path cannot be null or empty");
            }
        }
    }
}
