namespace SdJwt.Net.PresentationExchange.Models;

/// <summary>
/// Options for controlling credential selection behavior in presentation exchange.
/// </summary>
public class CredentialSelectionOptions
{
    /// <summary>
    /// Gets or sets the maximum number of credentials to evaluate.
    /// Prevents performance issues with very large wallets.
    /// Default: 1000
    /// </summary>
    public int MaxCredentialsToEvaluate { get; set; } = PresentationExchangeConstants.Defaults.MaxCredentials;

    /// <summary>
    /// Gets or sets the maximum number of matches to keep per input descriptor.
    /// Higher values provide more options but consume more memory.
    /// Default: 10
    /// </summary>
    public int MaxMatchesPerDescriptor { get; set; } = 10;

    /// <summary>
    /// Gets or sets the timeout for constraint evaluation in milliseconds.
    /// Default: 5000ms
    /// </summary>
    public int ConstraintEvaluationTimeoutMs { get; set; } = PresentationExchangeConstants.Defaults.ConstraintEvaluationTimeoutMs;

    /// <summary>
    /// Gets or sets whether to prefer credentials that support selective disclosure.
    /// When multiple credentials match, those supporting selective disclosure get higher scores.
    /// Default: true
    /// </summary>
    public bool PreferSelectiveDisclosure { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to allow duplicate credentials to satisfy different input descriptors.
    /// When false, each credential can only satisfy one input descriptor.
    /// Default: false
    /// </summary>
    public bool AllowDuplicateCredentials { get; set; } = false;

    /// <summary>
    /// Gets or sets whether to include optimization hints in the selection result.
    /// When enabled, provides suggestions for better credential organization.
    /// Default: false
    /// </summary>
    public bool IncludeOptimizationHints { get; set; } = false;

    /// <summary>
    /// Gets or sets the credential scoring strategy to use.
    /// Default: Comprehensive
    /// </summary>
    public CredentialScoringStrategy ScoringStrategy { get; set; } = CredentialScoringStrategy.Comprehensive;

    /// <summary>
    /// Gets or sets format preferences for credential selection.
    /// Used to break ties when multiple credentials have the same score.
    /// </summary>
    public Dictionary<string, int> FormatPreferences { get; set; } = new Dictionary<string, int>
    {
        { PresentationExchangeConstants.Formats.SdJwtVc, 100 },
        { PresentationExchangeConstants.Formats.JwtVc, 90 },
        { PresentationExchangeConstants.Formats.LdpVc, 80 },
        { PresentationExchangeConstants.Formats.SdJwt, 70 },
        { PresentationExchangeConstants.Formats.Jwt, 60 },
        { PresentationExchangeConstants.Formats.Ldp, 50 }
    };

    /// <summary>
    /// Gets or sets custom field path mappings for common credential types.
    /// Used to handle variations in credential schema.
    /// </summary>
    public Dictionary<string, PathMappingRule> CustomPathMappings { get; set; } = new Dictionary<string, PathMappingRule>();

    /// <summary>
    /// Gets or sets additional constraints that apply to all input descriptors.
    /// Useful for wallet-wide policies (e.g., only accept credentials from trusted issuers).
    /// </summary>
    public Constraints? GlobalConstraints { get; set; }

    /// <summary>
    /// Gets or sets whether to perform strict validation of presentation definitions.
    /// When enabled, additional checks are performed beyond the basic specification requirements.
    /// Default: true
    /// </summary>
    public bool StrictValidation { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to collect detailed debugging information.
    /// When enabled, additional metadata is collected for troubleshooting.
    /// Default: false
    /// </summary>
    public bool EnableDebugMode { get; set; } = false;

    /// <summary>
    /// Gets or sets the maximum depth for recursive constraint evaluation.
    /// Prevents stack overflow with deeply nested constraints.
    /// Default: 10
    /// </summary>
    public int MaxConstraintDepth { get; set; } = PresentationExchangeConstants.Defaults.MaxConstraintDepth;

    /// <summary>
    /// Gets or sets custom evaluation extensions.
    /// Allows plugging in custom logic for specific use cases.
    /// </summary>
    public List<ICredentialEvaluationExtension> EvaluationExtensions { get; set; } = new List<ICredentialEvaluationExtension>();

    /// <summary>
    /// Validates the options and throws an exception if invalid.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when options are invalid</exception>
    public void Validate()
    {
        if (MaxCredentialsToEvaluate <= 0)
            throw new ArgumentException("MaxCredentialsToEvaluate must be positive", nameof(MaxCredentialsToEvaluate));

        if (MaxMatchesPerDescriptor <= 0)
            throw new ArgumentException("MaxMatchesPerDescriptor must be positive", nameof(MaxMatchesPerDescriptor));

        if (ConstraintEvaluationTimeoutMs <= 0)
            throw new ArgumentException("ConstraintEvaluationTimeoutMs must be positive", nameof(ConstraintEvaluationTimeoutMs));

        if (MaxConstraintDepth <= 0)
            throw new ArgumentException("MaxConstraintDepth must be positive", nameof(MaxConstraintDepth));

        // Validate global constraints if present
        GlobalConstraints?.Validate();

        // Validate custom path mappings
        foreach (var mapping in CustomPathMappings.Values)
        {
            mapping.Validate();
        }
    }

    /// <summary>
    /// Creates default options optimized for performance.
    /// </summary>
    /// <returns>Performance-optimized options</returns>
    public static CredentialSelectionOptions CreatePerformanceOptimized()
    {
        return new CredentialSelectionOptions
        {
            MaxCredentialsToEvaluate = 100,
            MaxMatchesPerDescriptor = 3,
            ConstraintEvaluationTimeoutMs = 2000,
            ScoringStrategy = CredentialScoringStrategy.Fast,
            StrictValidation = false,
            EnableDebugMode = false
        };
    }

    /// <summary>
    /// Creates default options optimized for thoroughness.
    /// </summary>
    /// <returns>Thoroughness-optimized options</returns>
    public static CredentialSelectionOptions CreateThoroughEvaluation()
    {
        return new CredentialSelectionOptions
        {
            MaxCredentialsToEvaluate = 5000,
            MaxMatchesPerDescriptor = 50,
            ConstraintEvaluationTimeoutMs = 10000,
            ScoringStrategy = CredentialScoringStrategy.Comprehensive,
            StrictValidation = true,
            EnableDebugMode = true,
            IncludeOptimizationHints = true
        };
    }

    /// <summary>
    /// Creates default options for SD-JWT focused scenarios.
    /// </summary>
    /// <returns>SD-JWT optimized options</returns>
    public static CredentialSelectionOptions CreateSdJwtOptimized()
    {
        var options = new CredentialSelectionOptions
        {
            PreferSelectiveDisclosure = true,
            ScoringStrategy = CredentialScoringStrategy.SelectiveDisclosureFirst
        };

        // Heavily prefer SD-JWT formats
        options.FormatPreferences[PresentationExchangeConstants.Formats.SdJwtVc] = 200;
        options.FormatPreferences[PresentationExchangeConstants.Formats.SdJwt] = 150;

        return options;
    }
}

/// <summary>
/// Strategies for scoring credential matches.
/// </summary>
public enum CredentialScoringStrategy
{
    /// <summary>
    /// Fast scoring with basic criteria only.
    /// Good for performance-critical scenarios.
    /// </summary>
    Fast,

    /// <summary>
    /// Comprehensive scoring considering all factors.
    /// Provides best match quality but slower.
    /// </summary>
    Comprehensive,

    /// <summary>
    /// Prioritizes credentials with selective disclosure support.
    /// Useful for privacy-focused scenarios.
    /// </summary>
    SelectiveDisclosureFirst,

    /// <summary>
    /// Prioritizes newer credentials based on issuance date.
    /// Useful when credential freshness matters.
    /// </summary>
    PreferNewer,

    /// <summary>
    /// Custom scoring using registered extensions.
    /// Allows complete customization of scoring logic.
    /// </summary>
    Custom
}

/// <summary>
/// Represents a custom path mapping rule for handling credential schema variations.
/// </summary>
public class PathMappingRule
{
    /// <summary>
    /// Gets or sets the credential type this rule applies to.
    /// Can use wildcards like "*.DriverLicense" or specific types.
    /// </summary>
    public string CredentialType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the source path in the input descriptor.
    /// </summary>
    public string SourcePath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the target path in the credential.
    /// </summary>
    public string TargetPath { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the priority of this rule (higher = more priority).
    /// Used when multiple rules could apply to the same path.
    /// </summary>
    public int Priority { get; set; } = 0;

    /// <summary>
    /// Gets or sets whether this rule is case-sensitive.
    /// Default: false
    /// </summary>
    public bool CaseSensitive { get; set; } = false;

    /// <summary>
    /// Gets or sets additional conditions for applying this rule.
    /// </summary>
    public Dictionary<string, object> Conditions { get; set; } = new Dictionary<string, object>();

    /// <summary>
    /// Validates the path mapping rule.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when the rule is invalid</exception>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(CredentialType))
            throw new ArgumentException("CredentialType cannot be empty", nameof(CredentialType));

        if (string.IsNullOrWhiteSpace(SourcePath))
            throw new ArgumentException("SourcePath cannot be empty", nameof(SourcePath));

        if (string.IsNullOrWhiteSpace(TargetPath))
            throw new ArgumentException("TargetPath cannot be empty", nameof(TargetPath));

        if (!SourcePath.StartsWith("$"))
            throw new ArgumentException("SourcePath must be a valid JSON path", nameof(SourcePath));

        if (!TargetPath.StartsWith("$"))
            throw new ArgumentException("TargetPath must be a valid JSON path", nameof(TargetPath));
    }

    /// <summary>
    /// Checks if this rule applies to a given credential type.
    /// </summary>
    /// <param name="credentialType">The credential type to check</param>
    /// <returns>True if the rule applies</returns>
    public bool AppliesTo(string credentialType)
    {
        if (string.IsNullOrWhiteSpace(credentialType))
            return false;

        // Simple wildcard matching
        if (CredentialType.Contains("*"))
        {
            var pattern = CredentialType.Replace("*", ".*");
            var regex = new System.Text.RegularExpressions.Regex(pattern, 
                CaseSensitive ? System.Text.RegularExpressions.RegexOptions.None : System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            return regex.IsMatch(credentialType);
        }

        // Exact match
        return string.Equals(CredentialType, credentialType, 
            CaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase);
    }
}

/// <summary>
/// Interface for custom credential evaluation extensions.
/// </summary>
public interface ICredentialEvaluationExtension
{
    /// <summary>
    /// Gets the name of this extension.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the priority of this extension (higher = evaluated first).
    /// </summary>
    int Priority { get; }

    /// <summary>
    /// Evaluates a credential and potentially modifies the match score.
    /// </summary>
    /// <param name="credential">The credential being evaluated</param>
    /// <param name="inputDescriptor">The input descriptor being matched against</param>
    /// <param name="currentScore">The current match score</param>
    /// <param name="context">Additional context information</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task that represents the modified match score and any additional data</returns>
    Task<CredentialEvaluationExtensionResult> EvaluateAsync(
        object credential,
        InputDescriptor inputDescriptor,
        double currentScore,
        Dictionary<string, object> context,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Result from a credential evaluation extension.
/// </summary>
public class CredentialEvaluationExtensionResult
{
    /// <summary>
    /// Gets or sets the modified match score.
    /// </summary>
    public double ModifiedScore { get; set; }

    /// <summary>
    /// Gets or sets whether the extension recommends selecting this credential.
    /// </summary>
    public bool? Recommended { get; set; }

    /// <summary>
    /// Gets or sets additional metadata from the extension evaluation.
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();

    /// <summary>
    /// Gets or sets any warnings from the extension evaluation.
    /// </summary>
    public List<string> Warnings { get; set; } = new List<string>();

    /// <summary>
    /// Creates a result with a modified score.
    /// </summary>
    /// <param name="modifiedScore">The new score</param>
    /// <param name="recommended">Optional recommendation</param>
    /// <returns>A new extension result</returns>
    public static CredentialEvaluationExtensionResult WithScore(double modifiedScore, bool? recommended = null)
    {
        return new CredentialEvaluationExtensionResult
        {
            ModifiedScore = modifiedScore,
            Recommended = recommended
        };
    }

    /// <summary>
    /// Creates a result that doesn't change the score.
    /// </summary>
    /// <param name="originalScore">The original score to maintain</param>
    /// <param name="recommended">Optional recommendation</param>
    /// <returns>A new extension result</returns>
    public static CredentialEvaluationExtensionResult Unchanged(double originalScore, bool? recommended = null)
    {
        return WithScore(originalScore, recommended);
    }
}