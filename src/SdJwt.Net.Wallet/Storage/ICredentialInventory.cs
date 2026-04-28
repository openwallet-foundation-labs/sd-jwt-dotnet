using SdJwt.Net.Wallet.Core;

namespace SdJwt.Net.Wallet.Storage;

/// <summary>
/// Extends <see cref="ICredentialStore"/> with presentation-aware credential matching.
/// </summary>
/// <remarks>
/// OID4VP and DIF PEX need to search credentials by type, issuer, claims, and format
/// to satisfy presentation requests. This capability is a protocol-level concern that
/// belongs in the SDK rather than the wallet application.
/// </remarks>
public interface ICredentialInventory : ICredentialStore
{
    /// <summary>
    /// Finds credentials that satisfy the given presentation definition constraints.
    /// </summary>
    /// <param name="inputDescriptorId">The input descriptor ID from the presentation definition.</param>
    /// <param name="requiredTypes">Required credential types (vct values or doc types).</param>
    /// <param name="requiredFormat">Required credential format (e.g., "vc+sd-jwt", "mso_mdoc").</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Credentials that match the descriptor constraints.</returns>
    Task<IReadOnlyList<StoredCredential>> FindMatchingAsync(
        string inputDescriptorId,
        IReadOnlyList<string>? requiredTypes = null,
        string? requiredFormat = null,
        CancellationToken cancellationToken = default);
}
