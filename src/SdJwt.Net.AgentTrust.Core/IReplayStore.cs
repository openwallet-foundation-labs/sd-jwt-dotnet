namespace SdJwt.Net.AgentTrust.Core;

/// <summary>
/// Distributed token replay prevention store.
/// Supports single-use, idempotent retry, and bounded invocation policies
/// with separate consume key (K_consume) and retry key (K_retry) per spec.
/// </summary>
public interface IReplayStore
{
    /// <summary>
    /// Attempts to consume a token for replay prevention.
    /// Evaluates the replay policy and returns a structured decision.
    /// </summary>
    /// <param name="request">Replay request with token metadata and policy.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Replay decision indicating accept/reject and reason.</returns>
    Task<ReplayDecision> TryConsumeAsync(
        ReplayRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Attempts to mark a token identifier as consumed within a partition.
    /// Returns false if the token was already consumed (replay detected).
    /// Retained for backward compatibility; prefer the overload accepting <see cref="ReplayRequest"/>.
    /// </summary>
    /// <param name="partition">Logical partition key (e.g., tenant, audience).</param>
    /// <param name="tokenId">Unique token identifier (jti).</param>
    /// <param name="expiry">Absolute expiry after which the entry can be evicted.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if marked successfully; false if already consumed.</returns>
    Task<bool> TryConsumeAsync(
        string partition,
        string tokenId,
        DateTimeOffset expiry,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether a token identifier has already been consumed.
    /// </summary>
    /// <param name="partition">Logical partition key.</param>
    /// <param name="tokenId">Unique token identifier (jti).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the token has been consumed.</returns>
    Task<bool> IsConsumedAsync(
        string partition,
        string tokenId,
        CancellationToken cancellationToken = default);
}
