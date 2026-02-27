using Microsoft.Extensions.Logging;
using SdJwt.Net.Utils;
using SdJwt.Net.PresentationExchange.Models;
using SdJwt.Net.PresentationExchange.Services;
using System.Text;

namespace SdJwt.Net.PresentationExchange.Engine;

/// <summary>
/// The main presentation exchange engine that implements DIF PEX 2.1.1 specification.
/// Provides intelligent credential selection based on presentation definitions.
/// </summary>
public class PresentationExchangeEngine
{
    private readonly ILogger<PresentationExchangeEngine> _logger;
    private readonly ConstraintEvaluator _constraintEvaluator;
    private readonly SubmissionRequirementEvaluator _submissionRequirementEvaluator;
    private readonly CredentialFormatDetector _formatDetector;

    /// <summary>
    /// Initializes a new instance of the PresentationExchangeEngine class.
    /// </summary>
    /// <param name="logger">The logger instance</param>
    /// <param name="constraintEvaluator">The constraint evaluator</param>
    /// <param name="submissionRequirementEvaluator">The submission requirement evaluator</param>
    /// <param name="formatDetector">The credential format detector</param>
    public PresentationExchangeEngine(
        ILogger<PresentationExchangeEngine> logger,
        ConstraintEvaluator constraintEvaluator,
        SubmissionRequirementEvaluator submissionRequirementEvaluator,
        CredentialFormatDetector formatDetector)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _constraintEvaluator = constraintEvaluator ?? throw new ArgumentNullException(nameof(constraintEvaluator));
        _submissionRequirementEvaluator = submissionRequirementEvaluator ?? throw new ArgumentNullException(nameof(submissionRequirementEvaluator));
        _formatDetector = formatDetector ?? throw new ArgumentNullException(nameof(formatDetector));
    }

    /// <summary>
    /// Selects credentials from a wallet based on a presentation definition.
    /// This is the core function that implements the DIF PEX credential selection logic.
    /// </summary>
    /// <param name="presentationDefinition">The presentation definition specifying requirements</param>
    /// <param name="credentialWallet">The wallet containing available credentials</param>
    /// <param name="options">Optional selection options</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task that represents the credential selection result</returns>
    public async Task<CredentialSelectionResult> SelectCredentialsAsync(
        PresentationDefinition presentationDefinition,
        IEnumerable<object> credentialWallet,
        CredentialSelectionOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var metadata = new CredentialSelectionMetadata();

        try
        {
            _logger.LogInformation("Starting credential selection for presentation definition: {DefinitionId}",
                presentationDefinition.Id);

            // Validate presentation definition
            presentationDefinition.Validate();

            var credentials = credentialWallet.ToArray();
            metadata.CredentialsEvaluated = credentials.Length;

            options ??= new CredentialSelectionOptions();

            // Apply credential limit
            if (credentials.Length > options.MaxCredentialsToEvaluate)
            {
                _logger.LogWarning("Credential wallet contains {Count} credentials, limiting evaluation to {Limit}",
                    credentials.Length, options.MaxCredentialsToEvaluate);

                credentials = credentials.Take(options.MaxCredentialsToEvaluate).ToArray();
                metadata.CredentialsEvaluated = credentials.Length;
            }

            // Step 1: Evaluate each input descriptor against the credential wallet
            var descriptorMatches = new Dictionary<string, List<CredentialMatch>>();

            foreach (var descriptor in presentationDefinition.InputDescriptors)
            {
                cancellationToken.ThrowIfCancellationRequested();

                _logger.LogDebug("Evaluating input descriptor: {DescriptorId}", descriptor.Id);

                var matches = await EvaluateInputDescriptorAsync(descriptor, credentials, options, cancellationToken);
                descriptorMatches[descriptor.Id] = matches;

                _logger.LogDebug("Found {Count} matches for input descriptor: {DescriptorId}",
                    matches.Count, descriptor.Id);
            }

            // Step 2: Apply submission requirements to select the optimal credential set
            var selectionResult = await ApplySubmissionRequirementsAsync(
                presentationDefinition,
                descriptorMatches,
                options,
                cancellationToken);

            // Step 3: Generate presentation submission
            if (selectionResult.IsSuccessful && selectionResult.SelectedCredentials.Length > 0)
            {
                var presentationSubmission = GeneratePresentationSubmission(
                    presentationDefinition,
                    selectionResult.SelectedCredentials);

                selectionResult.PresentationSubmission = presentationSubmission;
            }

            selectionResult.Metadata = metadata;
            metadata.MarkCompleted();

            _logger.LogInformation("Credential selection completed. Success: {Success}, Selected: {Count}",
                selectionResult.IsSuccessful, selectionResult.GetSelectedCount());

            return selectionResult;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Credential selection was cancelled");
            metadata.MarkCompleted();
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during credential selection");
            metadata.MarkCompleted();

            return CredentialSelectionResult.Failure(
                PresentationExchangeConstants.ErrorCodes.ConstraintEvaluationFailed,
                $"Credential selection failed: {ex.Message}",
                metadata);
        }
    }

    /// <summary>
    /// Evaluates an input descriptor against the credential wallet.
    /// </summary>
    /// <param name="descriptor">The input descriptor to evaluate</param>
    /// <param name="credentials">The credentials to evaluate</param>
    /// <param name="options">Selection options</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task that represents the list of credential matches</returns>
    private async Task<List<CredentialMatch>> EvaluateInputDescriptorAsync(
        InputDescriptor descriptor,
        object[] credentials,
        CredentialSelectionOptions options,
        CancellationToken cancellationToken = default)
    {
        var matches = new List<CredentialMatch>();

        foreach (var credential in credentials)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var match = await EvaluateSingleCredentialAsync(descriptor, credential, options, cancellationToken);
                if (match != null)
                {
                    matches.Add(match);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error evaluating credential against descriptor {DescriptorId}", descriptor.Id);
                // Continue with other credentials
            }
        }

        // Sort matches by score (descending) and return top matches
        var sortedMatches = matches
            .OrderByDescending(m => m.MatchScore)
            .Take(options.MaxMatchesPerDescriptor)
            .ToList();

        return sortedMatches;
    }

    /// <summary>
    /// Evaluates a single credential against an input descriptor.
    /// </summary>
    /// <param name="descriptor">The input descriptor</param>
    /// <param name="credential">The credential to evaluate</param>
    /// <param name="options">Selection options</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task that represents the credential match or null if no match</returns>
    private async Task<CredentialMatch?> EvaluateSingleCredentialAsync(
        InputDescriptor descriptor,
        object credential,
        CredentialSelectionOptions options,
        CancellationToken cancellationToken = default)
    {
        // Step 1: Detect credential format
        var formatInfo = await _formatDetector.DetectFormatAsync(credential, cancellationToken);
        if (!formatInfo.IsSupported)
        {
            return null; // Unsupported format
        }

        // Step 2: Check format constraints
        if (!IsFormatAcceptable(descriptor, formatInfo.Format))
        {
            return null; // Format not acceptable for this descriptor
        }

        // Step 3: Evaluate field constraints
        ConstraintEvaluationResult? constraintResult = null;
        if (descriptor.Constraints != null)
        {
            constraintResult = await _constraintEvaluator.EvaluateAsync(credential, descriptor.Constraints, cancellationToken);
            if (!constraintResult.IsSuccessful)
            {
                return null; // Constraints not satisfied
            }
        }

        // Step 4: Calculate match score
        var matchScore = CalculateMatchScore(descriptor, credential, formatInfo, constraintResult);

        // Step 5: Extract disclosures for SD-JWT credentials
        string[]? disclosures = null;
        if (formatInfo.Format == PresentationExchangeConstants.Formats.SdJwtVc && descriptor.Constraints != null)
        {
            disclosures = ExtractRequiredDisclosures(credential, descriptor.Constraints, constraintResult);
        }

        // Step 6: Build path mappings
        var pathMappings = BuildPathMappings(descriptor, constraintResult);

        return CredentialMatch.Create(
            descriptor.Id,
            credential,
            formatInfo,
            matchScore,
            disclosures,
            pathMappings,
            constraintResult);
    }

    /// <summary>
    /// Checks if a credential format is acceptable for an input descriptor.
    /// </summary>
    /// <param name="descriptor">The input descriptor</param>
    /// <param name="format">The credential format</param>
    /// <returns>True if the format is acceptable</returns>
    private bool IsFormatAcceptable(InputDescriptor descriptor, string format)
    {
        if (descriptor.Format == null)
            return true; // No format constraints means all formats are acceptable

        return descriptor.Format.SupportsFormat(format);
    }

    /// <summary>
    /// Applies submission requirements to select optimal credentials.
    /// </summary>
    /// <param name="presentationDefinition">The presentation definition</param>
    /// <param name="descriptorMatches">Matches for each input descriptor</param>
    /// <param name="options">Selection options</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task that represents the credential selection result</returns>
    private async Task<CredentialSelectionResult> ApplySubmissionRequirementsAsync(
        PresentationDefinition presentationDefinition,
        Dictionary<string, List<CredentialMatch>> descriptorMatches,
        CredentialSelectionOptions options,
        CancellationToken cancellationToken = default)
    {
        // If no submission requirements, select best match for each descriptor
        if (presentationDefinition.SubmissionRequirements == null ||
            presentationDefinition.SubmissionRequirements.Length == 0)
        {
            return SelectBestMatchesForAllDescriptors(presentationDefinition, descriptorMatches, options);
        }

        // Use submission requirement evaluator for complex requirements
        return await _submissionRequirementEvaluator.EvaluateSubmissionRequirementsAsync(
            presentationDefinition.SubmissionRequirements,
            descriptorMatches,
            options,
            presentationDefinition.InputDescriptors,
            cancellationToken);
    }

    /// <summary>
    /// Selects the best match for each input descriptor.
    /// </summary>
    /// <param name="presentationDefinition">The presentation definition</param>
    /// <param name="descriptorMatches">Matches for each input descriptor</param>
    /// <param name="options">Selection options</param>
    /// <returns>The credential selection result</returns>
    private CredentialSelectionResult SelectBestMatchesForAllDescriptors(
        PresentationDefinition presentationDefinition,
        Dictionary<string, List<CredentialMatch>> descriptorMatches,
        CredentialSelectionOptions options)
    {
        var selectedCredentials = new List<SelectedCredential>();
        var errors = new List<CredentialSelectionError>();

        foreach (var descriptor in presentationDefinition.InputDescriptors)
        {
            if (descriptorMatches.TryGetValue(descriptor.Id, out var matches) && matches.Any())
            {
                var bestMatch = matches.First(); // Already sorted by score

                var selectedCredential = new SelectedCredential
                {
                    InputDescriptorId = descriptor.Id,
                    Credential = bestMatch.Credential,
                    Format = bestMatch.Format,
                    MatchScore = bestMatch.MatchScore,
                    Disclosures = bestMatch.Disclosures,
                    PathMappings = bestMatch.PathMappings
                };

                selectedCredentials.Add(selectedCredential);
            }
            else
            {
                errors.Add(new CredentialSelectionError
                {
                    Code = PresentationExchangeConstants.ErrorCodes.NoMatchingCredentials,
                    Message = $"No matching credentials found for input descriptor: {descriptor.Id}",
                    InputDescriptorId = descriptor.Id
                });
            }
        }

        if (errors.Any())
        {
            return CredentialSelectionResult.Failure(errors.ToArray());
        }

        return CredentialSelectionResult.Success(selectedCredentials.ToArray(), null!); // Submission will be generated later
    }

    /// <summary>
    /// Calculates a match score for a credential against an input descriptor.
    /// </summary>
    /// <param name="descriptor">The input descriptor</param>
    /// <param name="credential">The credential</param>
    /// <param name="formatInfo">Format information</param>
    /// <param name="constraintResult">Constraint evaluation result</param>
    /// <returns>The match score (0.0 to 1.0)</returns>
    private double CalculateMatchScore(
        InputDescriptor descriptor,
        object credential,
        CredentialFormatInfo formatInfo,
        ConstraintEvaluationResult? constraintResult)
    {
        double score = 0.0;

        // Base score for format compatibility
        score += 0.2;

        // Bonus for preferred formats
        if (IsPreferredFormat(descriptor, formatInfo.Format))
        {
            score += 0.1;
        }

        // Score based on constraint satisfaction
        if (constraintResult != null)
        {
            var satisfiedCount = constraintResult.GetSatisfiedFieldPaths().Length;
            var totalCount = constraintResult.FieldResults.Count;

            if (totalCount > 0)
            {
                score += 0.5 * (satisfiedCount / (double)totalCount);
            }
            else
            {
                score += 0.5; // No constraints means full score
            }
        }
        else
        {
            score += 0.5; // No constraints to evaluate
        }

        // Bonus for selective disclosure support if preferred
        if (descriptor.Constraints?.PrefersSelectiveDisclosure() == true &&
            formatInfo.SupportsSelectiveDisclosure)
        {
            score += 0.1;
        }

        // Penalty for warnings
        var warningCount = constraintResult?.Warnings.Count ?? 0;
        if (warningCount > 0)
        {
            score -= Math.Min(0.1, warningCount * 0.02);
        }

        return Math.Max(0.0, Math.Min(1.0, score));
    }

    /// <summary>
    /// Checks if a format is preferred for an input descriptor.
    /// </summary>
    /// <param name="descriptor">The input descriptor</param>
    /// <param name="format">The credential format</param>
    /// <returns>True if the format is preferred</returns>
    private bool IsPreferredFormat(InputDescriptor descriptor, string format)
    {
        // Prefer SD-JWT VC if selective disclosure is preferred
        if (descriptor.Constraints?.PrefersSelectiveDisclosure() == true &&
            format == PresentationExchangeConstants.Formats.SdJwtVc)
        {
            return true;
        }

        // Add other preference logic as needed
        return false;
    }

    /// <summary>
    /// Extracts required disclosures for SD-JWT credentials.
    /// </summary>
    /// <param name="credential">The SD-JWT credential</param>
    /// <param name="constraints">The field constraints</param>
    /// <param name="constraintResult">Constraint evaluation result containing matched paths.</param>
    /// <returns>Array of required disclosures</returns>
    private string[]? ExtractRequiredDisclosures(
        object credential,
        Constraints constraints,
        ConstraintEvaluationResult? constraintResult)
    {
        if (constraints.Fields == null || constraints.Fields.Length == 0)
            return null;

        if (credential is not string sdJwtCredential || !sdJwtCredential.Contains('~', StringComparison.Ordinal))
        {
            return null;
        }

        try
        {
            var parsed = SdJwtParser.ParsePresentation(sdJwtCredential);
            if (parsed.Disclosures.Count == 0)
            {
                return Array.Empty<string>();
            }

            var requiredClaimNames = GetRequiredClaimNames(constraints, constraintResult);
            if (requiredClaimNames.Count == 0)
            {
                return constraints.RequiresSelectiveDisclosure()
                    ? Array.Empty<string>()
                    : null;
            }

            var selectedDisclosures = parsed.Disclosures
                .Where(d => !string.IsNullOrWhiteSpace(d.ClaimName) && requiredClaimNames.Contains(d.ClaimName))
                .Select(d => d.EncodedValue)
                .Distinct(StringComparer.Ordinal)
                .ToArray();

            if (selectedDisclosures.Length == 0)
            {
                return constraints.RequiresSelectiveDisclosure()
                    ? Array.Empty<string>()
                    : null;
            }

            return selectedDisclosures;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Unable to parse SD-JWT disclosures for selective-disclosure extraction.");
            return null;
        }
    }

    private static HashSet<string> GetRequiredClaimNames(
        Constraints constraints,
        ConstraintEvaluationResult? constraintResult)
    {
        var requiredClaims = new HashSet<string>(StringComparer.Ordinal);

        if (constraintResult?.FieldResults != null)
        {
            foreach (var fieldResult in constraintResult.FieldResults.Values)
            {
                if (!fieldResult.IsSuccessful)
                {
                    continue;
                }

                var matchedClaimName = ExtractClaimNameFromPath(fieldResult.MatchedPath);
                if (!string.IsNullOrWhiteSpace(matchedClaimName))
                {
                    requiredClaims.Add(matchedClaimName);
                }
            }
        }

        foreach (var field in constraints.Fields ?? Array.Empty<Field>())
        {
            foreach (var path in field.Path ?? Array.Empty<string>())
            {
                var claimName = ExtractClaimNameFromPath(path);
                if (!string.IsNullOrWhiteSpace(claimName))
                {
                    requiredClaims.Add(claimName);
                }
            }
        }

        return requiredClaims;
    }

    private static string? ExtractClaimNameFromPath(string? jsonPath)
    {
        if (string.IsNullOrWhiteSpace(jsonPath))
        {
            return null;
        }

        var normalized = jsonPath.Trim();
        var segments = new List<string>();
        var current = new StringBuilder();
        for (var i = 0; i < normalized.Length; i++)
        {
            var c = normalized[i];
            if (c == '$')
            {
                continue;
            }

            if (c == '.')
            {
                AddPathSegment(segments, current);
                continue;
            }

            if (c == '[')
            {
                AddPathSegment(segments, current);
                var end = normalized.IndexOf(']', i + 1);
                if (end <= i)
                {
                    continue;
                }

                var inner = normalized.Substring(i + 1, end - i - 1).Trim();
                if (!string.IsNullOrWhiteSpace(inner))
                {
                    var unquoted = inner.Trim('\'', '"');
                    if (!int.TryParse(unquoted, out _) &&
                        !string.Equals(unquoted, "*", StringComparison.Ordinal))
                    {
                        segments.Add(unquoted);
                    }
                }

                i = end;
                continue;
            }

            current.Append(c);
        }

        AddPathSegment(segments, current);

        for (var i = segments.Count - 1; i >= 0; i--)
        {
            var candidate = segments[i];
            if (!string.IsNullOrWhiteSpace(candidate) &&
                !string.Equals(candidate, "*", StringComparison.Ordinal))
            {
                return candidate;
            }
        }

        return null;
    }

    private static void AddPathSegment(ICollection<string> segments, StringBuilder current)
    {
        if (current.Length == 0)
        {
            return;
        }

        var segment = current.ToString().Trim();
        if (!string.IsNullOrWhiteSpace(segment))
        {
            segments.Add(segment);
        }

        current.Clear();
    }

    /// <summary>
    /// Builds path mappings based on constraint evaluation results.
    /// </summary>
    /// <param name="descriptor">The input descriptor</param>
    /// <param name="constraintResult">The constraint evaluation result</param>
    /// <returns>Dictionary of path mappings</returns>
    private Dictionary<string, string> BuildPathMappings(
        InputDescriptor descriptor,
        ConstraintEvaluationResult? constraintResult)
    {
        var pathMappings = new Dictionary<string, string>();

        if (constraintResult?.FieldResults == null)
            return pathMappings;

        foreach (var fieldResult in constraintResult.FieldResults)
        {
            if (fieldResult.Value.IsSuccessful && !string.IsNullOrEmpty(fieldResult.Value.MatchedPath))
            {
                pathMappings[fieldResult.Key] = fieldResult.Value.MatchedPath;
            }
        }

        return pathMappings;
    }

    /// <summary>
    /// Generates a presentation submission based on selected credentials.
    /// </summary>
    /// <param name="presentationDefinition">The presentation definition</param>
    /// <param name="selectedCredentials">The selected credentials</param>
    /// <returns>The presentation submission</returns>
    private PresentationSubmission GeneratePresentationSubmission(
        PresentationDefinition presentationDefinition,
        SelectedCredential[] selectedCredentials)
    {
        var submissionId = Guid.NewGuid().ToString();
        var descriptorMappings = new List<InputDescriptorMapping>();

        for (int i = 0; i < selectedCredentials.Length; i++)
        {
            var credential = selectedCredentials[i];
            var rootPath = selectedCredentials.Length == 1 ? "$" : $"$[{i}]";

            var mapping = new InputDescriptorMapping
            {
                Id = credential.InputDescriptorId,
                Format = credential.Format,
                Path = rootPath
            };

            // Add path nested if there are specific field mappings
            if (credential.PathMappings.Any())
            {
                var nestedPaths = BuildNestedPaths(credential.PathMappings.Values);
                if (nestedPaths.Length > 0)
                {
                    mapping.PathNested = new PathMapping
                    {
                        Path = nestedPaths,
                        Format = credential.Format
                    };
                }
            }

            descriptorMappings.Add(mapping);
        }

        return PresentationSubmission.Create(
            submissionId,
            presentationDefinition.Id,
            descriptorMappings.ToArray());
    }

    private static string[] BuildNestedPaths(IEnumerable<string> paths)
    {
        var normalized = new HashSet<string>(StringComparer.Ordinal);
        foreach (var path in paths)
        {
            var normalizedPath = NormalizeJsonPath(path);
            if (!string.IsNullOrWhiteSpace(normalizedPath))
            {
                normalized.Add(normalizedPath);
            }
        }

        if (normalized.Count == 0)
        {
            return Array.Empty<string>();
        }

        var ordered = normalized.OrderBy(p => p.Length).ToList();
        var pruned = new List<string>();
        foreach (var candidate in ordered)
        {
            if (pruned.Any(parent => IsPathPrefix(parent, candidate)))
            {
                continue;
            }

            pruned.Add(candidate);
        }

        return pruned.ToArray();
    }

    private static string NormalizeJsonPath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return string.Empty;
        }

        var value = path.Trim();
        if (value.StartsWith("$", StringComparison.Ordinal))
        {
            return value;
        }

        if (value.StartsWith(".", StringComparison.Ordinal) ||
            value.StartsWith("[", StringComparison.Ordinal))
        {
            return $"${value}";
        }

        return $"$.{value}";
    }

    private static bool IsPathPrefix(string parent, string candidate)
    {
        if (string.Equals(parent, candidate, StringComparison.Ordinal))
        {
            return true;
        }

        if (!candidate.StartsWith(parent, StringComparison.Ordinal))
        {
            return false;
        }

        if (candidate.Length == parent.Length)
        {
            return true;
        }

        var next = candidate[parent.Length];
        return next == '.' || next == '[';
    }
}
