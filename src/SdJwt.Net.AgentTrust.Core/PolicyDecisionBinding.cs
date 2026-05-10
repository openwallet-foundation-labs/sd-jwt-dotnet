namespace SdJwt.Net.AgentTrust.Core;

/// <summary>
/// Binds a capability token to the policy decision that authorized it.
/// </summary>
public record PolicyDecisionBinding
{
    /// <summary>
    /// Unique identifier of the policy decision.
    /// </summary>
    public string DecisionId { get; set; } = string.Empty;

    /// <summary>
    /// Policy identifier that produced the decision.
    /// </summary>
    public string PolicyId { get; set; } = string.Empty;

    /// <summary>
    /// Policy version for audit reproducibility.
    /// </summary>
    public string? PolicyVersion
    {
        get; set;
    }

    /// <summary>
    /// SHA-256 hash of the policy rule set used for the evaluation.
    /// </summary>
    public string? PolicyHash
    {
        get; set;
    }

    /// <summary>
    /// Timestamp of the policy evaluation.
    /// </summary>
    public DateTimeOffset EvaluatedAt
    {
        get; set;
    }
}
