namespace SdJwt.Net.AgentTrust.Core;

/// <summary>
/// Resolves workload identity credentials for an agent runtime.
/// Implementations may integrate with Entra Workload ID, SPIFFE/SPIRE,
/// Kubernetes service account federation, or other identity providers.
/// </summary>
public interface IWorkloadIdentityProvider
{
    /// <summary>
    /// Resolves the workload identity for the current runtime.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The resolved workload identity.</returns>
    Task<WorkloadIdentity> GetIdentityAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates a presented workload identity credential.
    /// </summary>
    /// <param name="credential">The credential string to validate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The validated workload identity, or null if invalid.</returns>
    Task<WorkloadIdentity?> ValidateAsync(string credential, CancellationToken cancellationToken = default);
}
