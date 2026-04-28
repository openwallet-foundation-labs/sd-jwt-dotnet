namespace SdJwt.Net.AgentTrust.Core;

/// <summary>
/// Static workload identity provider for development, testing, and PoC scenarios.
/// Returns a preconfigured identity without calling an external identity provider.
/// </summary>
public class StaticWorkloadIdentityProvider : IWorkloadIdentityProvider
{
    private readonly WorkloadIdentity _identity;

    /// <summary>
    /// Initializes a new provider with a fixed identity.
    /// </summary>
    /// <param name="identity">The identity to return.</param>
    public StaticWorkloadIdentityProvider(WorkloadIdentity identity)
    {
        _identity = identity ?? throw new ArgumentNullException(nameof(identity));
    }

    /// <inheritdoc/>
    public Task<WorkloadIdentity> GetIdentityAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_identity);
    }

    /// <inheritdoc/>
    public Task<WorkloadIdentity?> ValidateAsync(string credential, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(credential))
        {
            return Task.FromResult<WorkloadIdentity?>(null);
        }

        if (string.Equals(credential, _identity.Credential, StringComparison.Ordinal) ||
            string.Equals(credential, _identity.SubjectId, StringComparison.Ordinal))
        {
            return Task.FromResult<WorkloadIdentity?>(_identity);
        }

        return Task.FromResult<WorkloadIdentity?>(null);
    }
}
