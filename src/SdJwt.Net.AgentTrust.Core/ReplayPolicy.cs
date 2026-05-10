namespace SdJwt.Net.AgentTrust.Core;

/// <summary>
/// Replay prevention policy for a capability token.
/// </summary>
public enum ReplayPolicy
{
    /// <summary>
    /// Token can only be used once (default for most operations).
    /// </summary>
    SingleUse,

    /// <summary>
    /// Token allows idempotent retries within a retry window.
    /// </summary>
    IdempotentRetry,

    /// <summary>
    /// Token allows bounded invocations up to a limit.
    /// </summary>
    BoundedInvocation
}
