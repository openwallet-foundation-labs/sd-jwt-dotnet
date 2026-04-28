using Microsoft.IdentityModel.Tokens;

namespace SdJwt.Net.AgentTrust.Core;

/// <summary>
/// Resolves signing keys from a JWKS endpoint or other key source.
/// Used by token verifiers to dynamically discover trusted issuer keys.
/// </summary>
public interface IJwksKeyResolver
{
    /// <summary>
    /// Resolves signing keys for the given issuer.
    /// </summary>
    /// <param name="issuer">The issuer identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A collection of security keys associated with the issuer.</returns>
    Task<IReadOnlyList<SecurityKey>> ResolveKeysAsync(string issuer, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resolves a specific key by key ID.
    /// </summary>
    /// <param name="issuer">The issuer identifier.</param>
    /// <param name="keyId">The key identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The matching security key, or null if not found.</returns>
    Task<SecurityKey?> ResolveKeyAsync(string issuer, string keyId, CancellationToken cancellationToken = default);
}
