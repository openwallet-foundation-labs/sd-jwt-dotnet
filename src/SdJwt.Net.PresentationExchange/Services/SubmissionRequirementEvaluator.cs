using Microsoft.Extensions.Logging;
using SdJwt.Net.PresentationExchange.Models;
using SdJwt.Net.PresentationExchange.Engine;

namespace SdJwt.Net.PresentationExchange.Services;

/// <summary>
/// Evaluates submission requirements to determine optimal credential selection.
/// Implements the complex logic for "all", "pick", min/max constraints from DIF PEX 2.1.1.
/// </summary>
public class SubmissionRequirementEvaluator
{
    private readonly ILogger<SubmissionRequirementEvaluator> _logger;

    /// <summary>
    /// Initializes a new instance of the SubmissionRequirementEvaluator class.
    /// </summary>
    /// <param name="logger">The logger instance</param>
    public SubmissionRequirementEvaluator(ILogger<SubmissionRequirementEvaluator> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Evaluates submission requirements against credential matches.
    /// </summary>
    /// <param name="submissionRequirements">The submission requirements to evaluate</param>
    /// <param name="descriptorMatches">Matches for each input descriptor</param>
    /// <param name="options">Selection options</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task that represents the credential selection result</returns>
    public async Task<CredentialSelectionResult> EvaluateSubmissionRequirementsAsync(
        SubmissionRequirement[] submissionRequirements,
        Dictionary<string, List<CredentialMatch>> descriptorMatches,
        CredentialSelectionOptions options,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Evaluating submission requirements. Count: {Count}", submissionRequirements.Length);

            // Create evaluation context
            var context = new SubmissionEvaluationContext
            {
                DescriptorMatches = descriptorMatches,
                Options = options,
                SelectedCredentials = new List<SelectedCredential>(),
                UsedCredentials = new HashSet<object>(),
                SatisfiedDescriptors = new HashSet<string>()
            };

            // Evaluate each submission requirement
            foreach (var requirement in submissionRequirements)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var requirementResult = await EvaluateSubmissionRequirementAsync(requirement, context, cancellationToken);
                if (!requirementResult.IsSuccessful)
                {
                    _logger.LogWarning("Submission requirement not satisfied: {RequirementName}", 
                        requirement.Name ?? "Unnamed");
                    
                    return CredentialSelectionResult.Failure(
                        PresentationExchangeConstants.ErrorCodes.SubmissionRequirementNotSatisfied,
                        $"Submission requirement could not be satisfied: {requirement.Name ?? requirement.Rule}");
                }
            }

            // Check if all requirements were satisfied
            if (context.SelectedCredentials.Any())
            {
                _logger.LogInformation("All submission requirements satisfied. Selected credentials: {Count}", 
                    context.SelectedCredentials.Count);

                return CredentialSelectionResult.Success(context.SelectedCredentials.ToArray(), null!);
            }
            else
            {
                return CredentialSelectionResult.Failure(
                    PresentationExchangeConstants.ErrorCodes.NoMatchingCredentials,
                    "No credentials could be selected to satisfy submission requirements");
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Submission requirement evaluation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during submission requirement evaluation");
            
            return CredentialSelectionResult.Failure(
                PresentationExchangeConstants.ErrorCodes.SubmissionRequirementNotSatisfied,
                $"Submission requirement evaluation failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Evaluates a single submission requirement.
    /// </summary>
    /// <param name="requirement">The submission requirement to evaluate</param>
    /// <param name="context">The evaluation context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task that represents the requirement evaluation result</returns>
    private async Task<RequirementEvaluationResult> EvaluateSubmissionRequirementAsync(
        SubmissionRequirement requirement,
        SubmissionEvaluationContext context,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Evaluating submission requirement: {Rule}", requirement.Rule);

        return requirement.Rule switch
        {
            PresentationExchangeConstants.SubmissionRules.All => await EvaluateAllRequirementAsync(requirement, context, cancellationToken),
            PresentationExchangeConstants.SubmissionRules.Pick => await EvaluatePickRequirementAsync(requirement, context, cancellationToken),
            _ => RequirementEvaluationResult.Failure($"Unknown submission rule: {requirement.Rule}")
        };
    }

    /// <summary>
    /// Evaluates an "all" submission requirement.
    /// </summary>
    /// <param name="requirement">The "all" requirement</param>
    /// <param name="context">The evaluation context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task that represents the requirement evaluation result</returns>
    private async Task<RequirementEvaluationResult> EvaluateAllRequirementAsync(
        SubmissionRequirement requirement,
        SubmissionEvaluationContext context,
        CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrEmpty(requirement.From))
        {
            // Simple case: single descriptor reference
            return await EvaluateDirectDescriptorRequirementAsync(requirement.From, context, cancellationToken);
        }

        if (requirement.FromNested != null && requirement.FromNested.Length > 0)
        {
            // Complex case: nested requirements - all must be satisfied
            foreach (var nestedRequirement in requirement.FromNested)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var nestedResult = await EvaluateSubmissionRequirementAsync(nestedRequirement, context, cancellationToken);
                if (!nestedResult.IsSuccessful)
                {
                    return RequirementEvaluationResult.Failure($"Nested requirement failed: {nestedResult.ErrorMessage}");
                }
            }

            return RequirementEvaluationResult.Success();
        }

        return RequirementEvaluationResult.Failure("All requirement must specify either 'from' or 'from_nested'");
    }

    /// <summary>
    /// Evaluates a "pick" submission requirement.
    /// </summary>
    /// <param name="requirement">The "pick" requirement</param>
    /// <param name="context">The evaluation context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task that represents the requirement evaluation result</returns>
    private async Task<RequirementEvaluationResult> EvaluatePickRequirementAsync(
        SubmissionRequirement requirement,
        SubmissionEvaluationContext context,
        CancellationToken cancellationToken = default)
    {
        var minCount = requirement.GetEffectiveMinCount();
        var maxCount = requirement.GetEffectiveMaxCount();

        _logger.LogDebug("Pick requirement - Min: {Min}, Max: {Max}", minCount, maxCount);

        if (!string.IsNullOrEmpty(requirement.From))
        {
            // Pick from a single descriptor (usually with count constraints)
            return await EvaluatePickFromDescriptorAsync(requirement.From, minCount, maxCount, context, cancellationToken);
        }

        if (requirement.FromNested != null && requirement.FromNested.Length > 0)
        {
            // Pick from nested requirements
            return await EvaluatePickFromNestedAsync(requirement.FromNested, minCount, maxCount, context, cancellationToken);
        }

        return RequirementEvaluationResult.Failure("Pick requirement must specify either 'from' or 'from_nested'");
    }

    /// <summary>
    /// Evaluates a direct descriptor requirement.
    /// </summary>
    /// <param name="descriptorId">The input descriptor ID</param>
    /// <param name="context">The evaluation context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task that represents the requirement evaluation result</returns>
    private Task<RequirementEvaluationResult> EvaluateDirectDescriptorRequirementAsync(
        string descriptorId,
        SubmissionEvaluationContext context,
        CancellationToken cancellationToken = default)
    {
        if (!context.DescriptorMatches.TryGetValue(descriptorId, out var matches) || !matches.Any())
        {
            return Task.FromResult(RequirementEvaluationResult.Failure($"No matches found for descriptor: {descriptorId}"));
        }

        // Select the best match that hasn't been used yet
        var bestMatch = matches.FirstOrDefault(m => !context.UsedCredentials.Contains(m.Credential));
        
        if (bestMatch == null)
        {
            return Task.FromResult(RequirementEvaluationResult.Failure($"No unused matches available for descriptor: {descriptorId}"));
        }

        // Add to selection
        var selectedCredential = new SelectedCredential
        {
            InputDescriptorId = descriptorId,
            Credential = bestMatch.Credential,
            Format = bestMatch.Format,
            MatchScore = bestMatch.MatchScore,
            Disclosures = bestMatch.Disclosures,
            PathMappings = bestMatch.PathMappings
        };

        context.SelectedCredentials.Add(selectedCredential);
        context.UsedCredentials.Add(bestMatch.Credential);
        context.SatisfiedDescriptors.Add(descriptorId);

        _logger.LogDebug("Selected credential for descriptor: {DescriptorId}, Score: {Score}", 
            descriptorId, bestMatch.MatchScore);

        return Task.FromResult(RequirementEvaluationResult.Success());
    }

    /// <summary>
    /// Evaluates a pick requirement from a single descriptor with count constraints.
    /// </summary>
    /// <param name="descriptorId">The input descriptor ID</param>
    /// <param name="minCount">Minimum required count</param>
    /// <param name="maxCount">Maximum allowed count</param>
    /// <param name="context">The evaluation context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task that represents the requirement evaluation result</returns>
    private Task<RequirementEvaluationResult> EvaluatePickFromDescriptorAsync(
        string descriptorId,
        int minCount,
        int? maxCount,
        SubmissionEvaluationContext context,
        CancellationToken cancellationToken = default)
    {
        if (!context.DescriptorMatches.TryGetValue(descriptorId, out var matches) || !matches.Any())
        {
            if (minCount > 0)
            {
                return Task.FromResult(RequirementEvaluationResult.Failure($"No matches found for descriptor: {descriptorId}"));
            }
            return Task.FromResult(RequirementEvaluationResult.Success()); // Optional requirement
        }

        // Get available matches (not yet used)
        var availableMatches = matches.Where(m => !context.UsedCredentials.Contains(m.Credential)).ToList();
        
        if (availableMatches.Count < minCount)
        {
            return Task.FromResult(RequirementEvaluationResult.Failure(
                $"Insufficient matches for descriptor {descriptorId}. Required: {minCount}, Available: {availableMatches.Count}"));
        }

        // Select up to maxCount matches (or all if no max)
        var countToSelect = maxCount.HasValue ? Math.Min(availableMatches.Count, maxCount.Value) : Math.Max(minCount, 1);
        var selectedMatches = availableMatches.Take(countToSelect);

        foreach (var match in selectedMatches)
        {
            var selectedCredential = new SelectedCredential
            {
                InputDescriptorId = descriptorId,
                Credential = match.Credential,
                Format = match.Format,
                MatchScore = match.MatchScore,
                Disclosures = match.Disclosures,
                PathMappings = match.PathMappings
            };

            context.SelectedCredentials.Add(selectedCredential);
            context.UsedCredentials.Add(match.Credential);
        }

        context.SatisfiedDescriptors.Add(descriptorId);

        _logger.LogDebug("Selected {Count} credentials for descriptor: {DescriptorId}", 
            countToSelect, descriptorId);

        return Task.FromResult(RequirementEvaluationResult.Success());
    }

    /// <summary>
    /// Evaluates a pick requirement from nested requirements.
    /// </summary>
    /// <param name="nestedRequirements">The nested requirements to pick from</param>
    /// <param name="minCount">Minimum required count</param>
    /// <param name="maxCount">Maximum allowed count</param>
    /// <param name="context">The evaluation context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task that represents the requirement evaluation result</returns>
    private async Task<RequirementEvaluationResult> EvaluatePickFromNestedAsync(
        SubmissionRequirement[] nestedRequirements,
        int minCount,
        int? maxCount,
        SubmissionEvaluationContext context,
        CancellationToken cancellationToken = default)
    {
        var satisfiedCount = 0;
        var maxToSatisfy = maxCount ?? nestedRequirements.Length;

        // Try to satisfy nested requirements up to the limit
        foreach (var nestedRequirement in nestedRequirements)
        {
            if (satisfiedCount >= maxToSatisfy)
                break;

            cancellationToken.ThrowIfCancellationRequested();

            // Create a temporary context to test if we can satisfy this requirement
            var tempContext = context.Clone();
            var nestedResult = await EvaluateSubmissionRequirementAsync(nestedRequirement, tempContext, cancellationToken);
            
            if (nestedResult.IsSuccessful)
            {
                // Apply the changes from temp context to main context
                context.Merge(tempContext);
                satisfiedCount++;
            }
        }

        // Check if we satisfied enough requirements
        if (satisfiedCount < minCount)
        {
            return RequirementEvaluationResult.Failure(
                $"Could not satisfy minimum nested requirements. Required: {minCount}, Satisfied: {satisfiedCount}");
        }

        _logger.LogDebug("Satisfied {Count} nested requirements (Min: {Min}, Max: {Max})", 
            satisfiedCount, minCount, maxCount);

        return RequirementEvaluationResult.Success();
    }
}

/// <summary>
/// Evaluation context for submission requirements.
/// </summary>
internal class SubmissionEvaluationContext
{
    public Dictionary<string, List<CredentialMatch>> DescriptorMatches { get; set; } = new();
    public CredentialSelectionOptions Options { get; set; } = new();
    public List<SelectedCredential> SelectedCredentials { get; set; } = new();
    public HashSet<object> UsedCredentials { get; set; } = new();
    public HashSet<string> SatisfiedDescriptors { get; set; } = new();

    /// <summary>
    /// Creates a clone of this context for testing purposes.
    /// </summary>
    /// <returns>A cloned context</returns>
    public SubmissionEvaluationContext Clone()
    {
        return new SubmissionEvaluationContext
        {
            DescriptorMatches = DescriptorMatches,
            Options = Options,
            SelectedCredentials = new List<SelectedCredential>(SelectedCredentials),
            UsedCredentials = new HashSet<object>(UsedCredentials),
            SatisfiedDescriptors = new HashSet<string>(SatisfiedDescriptors)
        };
    }

    /// <summary>
    /// Merges changes from another context into this one.
    /// </summary>
    /// <param name="other">The context to merge from</param>
    public void Merge(SubmissionEvaluationContext other)
    {
        // Add only new selections
        foreach (var credential in other.SelectedCredentials)
        {
            if (!UsedCredentials.Contains(credential.Credential))
            {
                SelectedCredentials.Add(credential);
                UsedCredentials.Add(credential.Credential);
            }
        }

        // Merge satisfied descriptors
        foreach (var descriptor in other.SatisfiedDescriptors)
        {
            SatisfiedDescriptors.Add(descriptor);
        }
    }
}

/// <summary>
/// Result of evaluating a submission requirement.
/// </summary>
internal class RequirementEvaluationResult
{
    public bool IsSuccessful { get; set; }
    public string? ErrorMessage { get; set; }

    public static RequirementEvaluationResult Success()
    {
        return new RequirementEvaluationResult { IsSuccessful = true };
    }

    public static RequirementEvaluationResult Failure(string errorMessage)
    {
        return new RequirementEvaluationResult 
        { 
            IsSuccessful = false, 
            ErrorMessage = errorMessage 
        };
    }
}