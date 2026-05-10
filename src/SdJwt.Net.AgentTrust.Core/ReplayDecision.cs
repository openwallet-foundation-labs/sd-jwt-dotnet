namespace SdJwt.Net.AgentTrust.Core;

/// <summary>
/// Result of a replay prevention check.
/// </summary>
public sealed record ReplayDecision
{
    /// <summary>
    /// The replay decision outcome.
    /// </summary>
    public required ReplayDecisionKind Kind
    {
        get; init;
    }

    /// <summary>
    /// Error code for rejected decisions.
    /// </summary>
    public string? ErrorCode
    {
        get; init;
    }

    /// <summary>
    /// Human-readable reason for the decision.
    /// </summary>
    public string? Reason
    {
        get; init;
    }

    /// <summary>
    /// Number of times this token has been invoked (for bounded invocation).
    /// </summary>
    public int? InvocationCount
    {
        get; init;
    }

    /// <summary>
    /// Timestamp when this token was first seen.
    /// </summary>
    public DateTimeOffset? FirstSeenAt
    {
        get; init;
    }

    /// <summary>
    /// Reference to cached result for idempotent retries.
    /// </summary>
    public string? CachedResultReference
    {
        get; init;
    }
}
