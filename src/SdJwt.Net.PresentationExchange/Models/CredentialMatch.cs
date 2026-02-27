using SdJwt.Net.PresentationExchange.Services;

namespace SdJwt.Net.PresentationExchange.Models;

/// <summary>
/// Represents a credential match for an input descriptor in presentation exchange.
/// Contains the credential and metadata about how well it matches the requirements.
/// </summary>
public class CredentialMatch
{
    /// <summary>
    /// Gets or sets the input descriptor ID that this credential matches.
    /// </summary>
    public string InputDescriptorId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the credential object (SD-JWT, JWT VC, etc.).
    /// </summary>
    public object Credential { get; set; } = new();

    /// <summary>
    /// Gets or sets the format of the credential.
    /// </summary>
    public string Format { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the match score indicating how well the credential satisfies the descriptor (0.0 to 1.0).
    /// Higher scores indicate better matches.
    /// </summary>
    public double MatchScore
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the disclosures to include for selective disclosure credentials (SD-JWT only).
    /// </summary>
    public string[]? Disclosures
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the path mappings for field requirements.
    /// Maps input descriptor field paths to actual credential paths.
    /// </summary>
    public Dictionary<string, string> PathMappings { get; set; } = new();

    /// <summary>
    /// Gets or sets the constraint evaluation result for this match.
    /// Contains details about which constraints were satisfied.
    /// </summary>
    public ConstraintEvaluationResult? ConstraintResult
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the credential format information.
    /// Contains details about the detected format and its capabilities.
    /// </summary>
    public CredentialFormatInfo FormatInfo { get; set; } = new();

    /// <summary>
    /// Creates a credential match for an SD-JWT credential.
    /// </summary>
    /// <param name="inputDescriptorId">The input descriptor ID</param>
    /// <param name="credential">The credential object</param>
    /// <param name="formatInfo">The format information</param>
    /// <param name="matchScore">The match score</param>
    /// <param name="disclosures">Optional disclosures for selective disclosure</param>
    /// <param name="pathMappings">Optional path mappings</param>
    /// <param name="constraintResult">Optional constraint evaluation result</param>
    /// <returns>A new CredentialMatch instance</returns>
    public static CredentialMatch Create(
        string inputDescriptorId,
        object credential,
        CredentialFormatInfo formatInfo,
        double matchScore,
        string[]? disclosures = null,
        Dictionary<string, string>? pathMappings = null,
        ConstraintEvaluationResult? constraintResult = null)
    {
        return new CredentialMatch
        {
            InputDescriptorId = inputDescriptorId,
            Credential = credential,
            Format = formatInfo.Format,
            MatchScore = matchScore,
            Disclosures = disclosures,
            PathMappings = pathMappings ?? new Dictionary<string, string>(),
            ConstraintResult = constraintResult,
            FormatInfo = formatInfo
        };
    }

    /// <summary>
    /// Gets additional metadata about the match.
    /// </summary>
    /// <returns>Dictionary containing match metadata</returns>
    public Dictionary<string, object> GetMetadata()
    {
        var metadata = new Dictionary<string, object>
        {
            ["format"] = Format,
            ["matchScore"] = MatchScore,
            ["hasDisclosures"] = Disclosures?.Length > 0,
            ["disclosureCount"] = Disclosures?.Length ?? 0,
            ["pathMappingCount"] = PathMappings.Count,
            ["constraintsSatisfied"] = ConstraintResult?.IsSuccessful ?? false
        };

        if (FormatInfo != null)
        {
            metadata["supportsSelectiveDisclosure"] = FormatInfo.SupportsSelectiveDisclosure;
            metadata["isVerifiableCredential"] = FormatInfo.IsVerifiableCredential;
        }

        return metadata;
    }

    /// <summary>
    /// Checks if this match has all required disclosures.
    /// </summary>
    /// <param name="requiredPaths">The required field paths</param>
    /// <returns>True if all required paths are available</returns>
    public bool HasRequiredDisclosures(string[] requiredPaths)
    {
        if (requiredPaths == null || requiredPaths.Length == 0)
            return true;

        if (Disclosures == null || Disclosures.Length == 0)
            return false;

        // For a complete implementation, you would need to parse the disclosures
        // and check if they cover all required paths. This is a simplified version.
        return PathMappings.Keys.Intersect(requiredPaths).Count() == requiredPaths.Length;
    }
}
