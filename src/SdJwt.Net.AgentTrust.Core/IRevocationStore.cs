namespace SdJwt.Net.AgentTrust.Core;

/// <summary>
/// Store for checking whether tokens, agents, tools, or other entities
/// have been revoked. Supports the 10 revocation targets defined in the spec.
/// </summary>
public interface IRevocationStore
{
    /// <summary>
    /// Checks whether any of the identifiers in the check have been revoked.
    /// Returns true if revoked, false if active.
    /// </summary>
    Task<bool> IsRevokedAsync(
        RevocationCheck check,
        CancellationToken cancellationToken = default);
}
