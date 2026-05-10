namespace SdJwt.Net.AgentTrust.Core;

/// <summary>
/// Production-grade capability authority that evaluates policy,
/// binds tokens to proof-of-possession and request data, and emits audit receipts.
/// This is the recommended entry point for production minting.
/// </summary>
public interface ICapabilityAuthority
{
    /// <summary>
    /// Evaluates policy and mints a capability token if authorized.
    /// </summary>
    /// <param name="request">The capability mint request with agent, audience, and scope.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The minted capability token or a denial result.</returns>
    /// <exception cref="AgentTrustPolicyDeniedException">When policy denies the request.</exception>
    Task<CapabilityMintResult> MintAsync(
        CapabilityMintRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Delegates a capability to a downstream agent with attenuated scope.
    /// </summary>
    /// <param name="request">The delegation request with parent token and scope restrictions.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The delegated capability token result.</returns>
    Task<CapabilityMintResult> DelegateAsync(
        CapabilityDelegationRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Introspects a capability token to retrieve its current status and claims.
    /// </summary>
    /// <param name="request">The introspection request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The introspection result.</returns>
    Task<CapabilityIntrospectionResult> IntrospectAsync(
        CapabilityIntrospectionRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes a capability token, key, agent, tool, or other trust entity.
    /// </summary>
    /// <param name="request">The revocation request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The revocation result.</returns>
    Task<RevocationResult> RevokeAsync(
        RevocationRequest request,
        CancellationToken cancellationToken = default);
}
