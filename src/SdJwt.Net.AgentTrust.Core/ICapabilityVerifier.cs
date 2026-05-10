namespace SdJwt.Net.AgentTrust.Core;

/// <summary>
/// Verifies capability tokens against an <see cref="AgentTrustVerificationContext"/>.
/// </summary>
public interface ICapabilityVerifier
{
    /// <summary>
    /// Verifies a capability token and returns a structured verification result.
    /// </summary>
    /// <param name="token">The compact-serialized SD-JWT capability token.</param>
    /// <param name="context">Verification context with trust anchors, policy, and bindings.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The verification result.</returns>
    Task<CapabilityVerificationResult> VerifyAsync(
        string token,
        AgentTrustVerificationContext context,
        CancellationToken cancellationToken = default);
}
