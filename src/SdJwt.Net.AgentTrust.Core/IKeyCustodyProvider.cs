using Microsoft.IdentityModel.Tokens;

namespace SdJwt.Net.AgentTrust.Core;

/// <summary>
/// Provides signing keys for capability issuance.
/// </summary>
public interface IKeyCustodyProvider
{
    /// <summary>
    /// Gets the signing key for an agent.
    /// </summary>
    Task<SecurityKey> GetSigningKeyAsync(string agentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the signing algorithm for an agent.
    /// </summary>
    Task<string> GetSigningAlgorithmAsync(string agentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Rotates a signing key.
    /// </summary>
    Task RotateKeyAsync(string agentId, CancellationToken cancellationToken = default);
}

