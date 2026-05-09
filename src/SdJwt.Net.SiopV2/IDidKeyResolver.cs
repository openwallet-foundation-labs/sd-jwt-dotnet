using Microsoft.IdentityModel.Tokens;

namespace SdJwt.Net.SiopV2;

/// <summary>
/// Resolves DID verification keys for SIOPv2 DID subject syntax validation.
/// </summary>
public interface IDidKeyResolver
{
    /// <summary>
    /// Resolves the verification key for a DID and optional key identifier.
    /// </summary>
    /// <param name="did">The DID subject.</param>
    /// <param name="keyId">The optional JOSE key identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The resolved security key.</returns>
    Task<SecurityKey> ResolveKeyAsync(string did, string? keyId, CancellationToken cancellationToken = default);
}
