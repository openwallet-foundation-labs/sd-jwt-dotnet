namespace SdJwt.Net.AgentTrust.Core;

/// <summary>
/// Outcome of a replay prevention check.
/// </summary>
public enum ReplayDecisionKind
{
    /// <summary>
    /// Token is new and accepted for first use.
    /// </summary>
    AcceptedNew,

    /// <summary>
    /// Token is an idempotent retry within the retry window.
    /// </summary>
    AcceptedRetry,

    /// <summary>
    /// Token is accepted within bounded invocation limits.
    /// </summary>
    AcceptedBoundedInvocation,

    /// <summary>
    /// Token was already consumed and this is a replay attack.
    /// </summary>
    RejectedReplay,

    /// <summary>
    /// Token retry window has expired.
    /// </summary>
    RejectedRetryWindowExpired,

    /// <summary>
    /// Replay store is unavailable; fail closed.
    /// </summary>
    RejectedStoreUnavailable,

    /// <summary>
    /// Replay policy violation.
    /// </summary>
    RejectedPolicyViolation
}
