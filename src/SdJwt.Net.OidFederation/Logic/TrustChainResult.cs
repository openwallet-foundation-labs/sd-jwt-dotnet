using SdJwt.Net.OidFederation.Models;

namespace SdJwt.Net.OidFederation.Logic;

/// <summary>
/// Represents the result of trust chain resolution and validation.
/// </summary>
public class TrustChainResult
{
    /// <summary>
    /// Gets a value indicating whether the trust chain is valid.
    /// </summary>
    public bool IsValid { get; private set; }

    /// <summary>
    /// Gets the error message if validation failed.
    /// </summary>
    public string? ErrorMessage { get; private set; }

    /// <summary>
    /// Gets the trust anchor URL that anchors this trust chain.
    /// </summary>
    public string? TrustAnchor { get; private set; }

    /// <summary>
    /// Gets the validated entity configuration of the target entity.
    /// </summary>
    public EntityConfiguration? EntityConfiguration { get; private set; }

    /// <summary>
    /// Gets the chain of entity statements from the trust anchor to the target entity.
    /// The chain is ordered from trust anchor (first) to target entity (last).
    /// </summary>
    public IReadOnlyList<EntityStatement> TrustChain { get; private set; } = Array.Empty<EntityStatement>();

    /// <summary>
    /// Gets the validated metadata for the target entity.
    /// This is the final metadata after applying all metadata policies in the trust chain.
    /// </summary>
    public EntityMetadata? ValidatedMetadata { get; private set; }

    /// <summary>
    /// Gets additional validation details and warnings.
    /// </summary>
    public IReadOnlyList<string> ValidationDetails { get; private set; } = Array.Empty<string>();

    private TrustChainResult() { }

    /// <summary>
    /// Creates a successful trust chain result.
    /// </summary>
    /// <param name="trustAnchor">The trust anchor URL</param>
    /// <param name="entityConfiguration">The validated entity configuration</param>
    /// <param name="trustChain">The chain of entity statements</param>
    /// <param name="validatedMetadata">Optional validated metadata</param>
    /// <param name="validationDetails">Optional validation details</param>
    /// <returns>A successful TrustChainResult</returns>
    public static TrustChainResult Success(
        string trustAnchor,
        EntityConfiguration entityConfiguration,
        IEnumerable<EntityStatement> trustChain,
        EntityMetadata? validatedMetadata = null,
        IEnumerable<string>? validationDetails = null)
    {
        return new TrustChainResult
        {
            IsValid = true,
            TrustAnchor = trustAnchor,
            EntityConfiguration = entityConfiguration,
            TrustChain = trustChain.ToArray(),
            ValidatedMetadata = validatedMetadata ?? entityConfiguration.Metadata,
            ValidationDetails = validationDetails?.ToArray() ?? Array.Empty<string>()
        };
    }

    /// <summary>
    /// Creates a failed trust chain result.
    /// </summary>
    /// <param name="errorMessage">The error message</param>
    /// <param name="validationDetails">Optional validation details</param>
    /// <returns>A failed TrustChainResult</returns>
    public static TrustChainResult Failed(string errorMessage, IEnumerable<string>? validationDetails = null)
    {
        return new TrustChainResult
        {
            IsValid = false,
            ErrorMessage = errorMessage,
            ValidationDetails = validationDetails?.ToArray() ?? Array.Empty<string>()
        };
    }

    /// <summary>
    /// Gets the path length of the trust chain.
    /// </summary>
    public int PathLength => TrustChain.Count;

    /// <summary>
    /// Gets all entities in the trust chain from trust anchor to target entity.
    /// </summary>
    /// <returns>Array of entity URLs in the trust chain</returns>
    public string[] GetTrustChainEntities()
    {
        if (!IsValid || EntityConfiguration == null)
            return Array.Empty<string>();

        var entities = new List<string>();
        
        if (!string.IsNullOrEmpty(TrustAnchor))
            entities.Add(TrustAnchor);

        foreach (var statement in TrustChain)
        {
            if (!entities.Contains(statement.Subject))
                entities.Add(statement.Subject);
        }

        return entities.ToArray();
    }

    /// <summary>
    /// Gets all trust marks that apply to the target entity.
    /// This includes trust marks from the entity configuration and all entity statements in the chain.
    /// </summary>
    /// <returns>Array of all applicable trust marks</returns>
    public TrustMark[] GetAllTrustMarks()
    {
        var trustMarks = new List<TrustMark>();

        // Add trust marks from entity configuration
        if (EntityConfiguration?.TrustMarks != null)
            trustMarks.AddRange(EntityConfiguration.TrustMarks);

        // Add trust marks from entity statements
        foreach (var statement in TrustChain)
        {
            if (statement.TrustMarks != null)
                trustMarks.AddRange(statement.TrustMarks);
        }

        return trustMarks.ToArray();
    }

    /// <summary>
    /// Checks if the target entity has a specific trust mark.
    /// </summary>
    /// <param name="trustMarkId">The trust mark identifier to check</param>
    /// <returns>True if the entity has the trust mark</returns>
    public bool HasTrustMark(string trustMarkId)
    {
        return GetAllTrustMarks().Any(tm => tm.Id == trustMarkId && tm.IsValid());
    }

    /// <summary>
    /// Gets the effective metadata for a specific protocol after applying all policies.
    /// </summary>
    /// <param name="protocol">The protocol identifier</param>
    /// <returns>The effective metadata or null if not supported</returns>
    public object? GetEffectiveMetadata(string protocol)
    {
        if (ValidatedMetadata == null)
            return null;

        return ValidatedMetadata.GetProtocolMetadata(protocol);
    }

    /// <summary>
    /// Gets all constraints that apply to the target entity.
    /// </summary>
    /// <returns>Merged constraints from the trust chain</returns>
    public EntityConstraints GetEffectiveConstraints()
    {
        var effectiveConstraints = new EntityConstraints();
        var maxPathLengths = new List<int>();

        // Collect constraints from entity configuration
        if (EntityConfiguration?.Constraints != null)
        {
            if (EntityConfiguration.Constraints.MaxPathLength.HasValue)
                maxPathLengths.Add(EntityConfiguration.Constraints.MaxPathLength.Value);
        }

        // Collect constraints from entity statements
        foreach (var statement in TrustChain)
        {
            if (statement.Constraints?.MaxPathLength.HasValue == true)
                maxPathLengths.Add(statement.Constraints.MaxPathLength.Value);
        }

        // Apply the most restrictive max path length
        if (maxPathLengths.Count > 0)
        {
            effectiveConstraints.MaxPathLength = maxPathLengths.Min();
        }

        return effectiveConstraints;
    }
}

/// <summary>
/// Represents the result of entity configuration validation.
/// </summary>
internal class EntityConfigurationValidationResult
{
    public bool IsValid { get; private set; }
    public string? ErrorMessage { get; private set; }
    public EntityConfiguration? Configuration { get; private set; }

    private EntityConfigurationValidationResult() { }

    public static EntityConfigurationValidationResult Success(EntityConfiguration configuration)
    {
        return new EntityConfigurationValidationResult
        {
            IsValid = true,
            Configuration = configuration
        };
    }

    public static EntityConfigurationValidationResult Failed(string errorMessage)
    {
        return new EntityConfigurationValidationResult
        {
            IsValid = false,
            ErrorMessage = errorMessage
        };
    }
}

/// <summary>
/// Represents the result of entity statement validation.
/// </summary>
internal class EntityStatementValidationResult
{
    public bool IsValid { get; private set; }
    public string? ErrorMessage { get; private set; }
    public EntityStatement? Statement { get; private set; }

    private EntityStatementValidationResult() { }

    public static EntityStatementValidationResult Success(EntityStatement statement)
    {
        return new EntityStatementValidationResult
        {
            IsValid = true,
            Statement = statement
        };
    }

    public static EntityStatementValidationResult Failed(string errorMessage)
    {
        return new EntityStatementValidationResult
        {
            IsValid = false,
            ErrorMessage = errorMessage
        };
    }
}

/// <summary>
/// Provides extension methods for TrustChainResult.
/// </summary>
public static class TrustChainResultExtensions
{
    /// <summary>
    /// Checks if the entity supports a specific protocol with valid trust chain.
    /// </summary>
    /// <param name="result">The trust chain result</param>
    /// <param name="protocol">The protocol identifier</param>
    /// <returns>True if the protocol is supported with valid trust</returns>
    public static bool SupportsProtocol(this TrustChainResult result, string protocol)
    {
        if (!result.IsValid || result.ValidatedMetadata == null)
            return false;

        return result.ValidatedMetadata.SupportsProtocol(protocol);
    }

    /// <summary>
    /// Gets a summary of the trust chain for logging or debugging.
    /// </summary>
    /// <param name="result">The trust chain result</param>
    /// <returns>A human-readable trust chain summary</returns>
    public static string GetTrustChainSummary(this TrustChainResult result)
    {
        if (!result.IsValid)
            return $"Invalid trust chain: {result.ErrorMessage}";

        var entities = result.GetTrustChainEntities();
        var summary = string.Join(" -> ", entities);
        
        return $"Valid trust chain (length {result.PathLength}): {summary}";
    }

    /// <summary>
    /// Validates that the trust chain satisfies specific requirements.
    /// </summary>
    /// <param name="result">The trust chain result</param>
    /// <param name="requirements">The trust chain requirements</param>
    /// <returns>True if all requirements are satisfied</returns>
    public static bool SatisfiesRequirements(this TrustChainResult result, TrustChainRequirements requirements)
    {
        if (!result.IsValid)
            return false;

        // Check path length requirement
        if (requirements.MaxPathLength.HasValue && result.PathLength > requirements.MaxPathLength.Value)
            return false;

        // Check required trust marks
        if (requirements.RequiredTrustMarks != null)
        {
            foreach (var requiredTrustMark in requirements.RequiredTrustMarks)
            {
                if (!result.HasTrustMark(requiredTrustMark))
                    return false;
            }
        }

        // Check required protocols
        if (requirements.RequiredProtocols != null)
        {
            foreach (var requiredProtocol in requirements.RequiredProtocols)
            {
                if (!result.SupportsProtocol(requiredProtocol))
                    return false;
            }
        }

        // Check trust anchor requirement
        if (requirements.AllowedTrustAnchors != null && requirements.AllowedTrustAnchors.Length > 0)
        {
            if (result.TrustAnchor == null || !requirements.AllowedTrustAnchors.Contains(result.TrustAnchor))
                return false;
        }

        return true;
    }
}

/// <summary>
/// Defines requirements for trust chain validation.
/// </summary>
public class TrustChainRequirements
{
    /// <summary>
    /// Gets or sets the maximum allowed path length.
    /// </summary>
    public int? MaxPathLength { get; set; }

    /// <summary>
    /// Gets or sets the required trust marks.
    /// </summary>
    public string[]? RequiredTrustMarks { get; set; }

    /// <summary>
    /// Gets or sets the required protocols.
    /// </summary>
    public string[]? RequiredProtocols { get; set; }

    /// <summary>
    /// Gets or sets the allowed trust anchors.
    /// If specified, the trust chain must terminate at one of these anchors.
    /// </summary>
    public string[]? AllowedTrustAnchors { get; set; }

    /// <summary>
    /// Creates basic trust chain requirements.
    /// </summary>
    /// <param name="maxPathLength">Maximum path length</param>
    /// <param name="requiredTrustMarks">Required trust marks</param>
    /// <returns>A new TrustChainRequirements instance</returns>
    public static TrustChainRequirements Create(int? maxPathLength = null, params string[] requiredTrustMarks)
    {
        return new TrustChainRequirements
        {
            MaxPathLength = maxPathLength,
            RequiredTrustMarks = requiredTrustMarks.Length > 0 ? requiredTrustMarks : null
        };
    }

    /// <summary>
    /// Creates requirements for a specific protocol.
    /// </summary>
    /// <param name="protocol">The required protocol</param>
    /// <param name="maxPathLength">Maximum path length</param>
    /// <returns>A new TrustChainRequirements instance</returns>
    public static TrustChainRequirements ForProtocol(string protocol, int? maxPathLength = null)
    {
        return new TrustChainRequirements
        {
            RequiredProtocols = new[] { protocol },
            MaxPathLength = maxPathLength
        };
    }
}