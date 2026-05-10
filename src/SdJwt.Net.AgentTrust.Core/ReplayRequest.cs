namespace SdJwt.Net.AgentTrust.Core;

/// <summary>
/// Request to consume a token for replay prevention.
/// Uses separate consume key (K_consume) and retry key (K_retry)
/// per the normative replay decision algorithm.
/// </summary>
public sealed record ReplayRequest
{
    /// <summary>
    /// Token issuer.
    /// </summary>
    public required string Issuer
    {
        get; init;
    }

    /// <summary>
    /// Token audience.
    /// </summary>
    public required string Audience
    {
        get; init;
    }

    /// <summary>
    /// Tenant identifier for partition scoping.
    /// </summary>
    public required string TenantId
    {
        get; init;
    }

    /// <summary>
    /// Token subject.
    /// </summary>
    public required string Subject
    {
        get; init;
    }

    /// <summary>
    /// Token identifier (jti).
    /// </summary>
    public required string TokenId
    {
        get; init;
    }

    /// <summary>
    /// Replay policy governing this token's usage pattern.
    /// </summary>
    public required ReplayPolicy Policy
    {
        get; init;
    }

    /// <summary>
    /// Token expiry for TTL-based cleanup.
    /// </summary>
    public required DateTimeOffset ExpiresAt
    {
        get; init;
    }

    /// <summary>
    /// SHA-256 hash of the bound request for retry key computation.
    /// </summary>
    public required string RequestHash
    {
        get; init;
    }

    /// <summary>
    /// JWK thumbprint of the proof-of-possession key.
    /// </summary>
    public required string ProofThumbprint
    {
        get; init;
    }

    /// <summary>
    /// Optional idempotency key for retry window matching.
    /// </summary>
    public string? IdempotencyKey
    {
        get; init;
    }

    /// <summary>
    /// Maximum number of allowed invocations for bounded invocation policy.
    /// </summary>
    public int? MaxInvocations
    {
        get; init;
    }

    /// <summary>
    /// Retry window duration for idempotent retry policy.
    /// </summary>
    public TimeSpan? RetryWindow
    {
        get; init;
    }
}
