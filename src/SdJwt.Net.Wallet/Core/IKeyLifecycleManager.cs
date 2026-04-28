namespace SdJwt.Net.Wallet.Core;

/// <summary>
/// Extends <see cref="IKeyManager"/> with key rotation and lifecycle operations
/// relevant to credential protocols.
/// </summary>
/// <remarks>
/// Key rotation is tightly coupled to credential issuance and presentation:
/// an issuer needs to rotate signing keys, and a holder needs to bind new keys
/// when rotating. These are protocol-level operations.
/// </remarks>
public interface IKeyLifecycleManager : IKeyManager
{
    /// <summary>
    /// Rotates a key by generating a replacement and marking the old key
    /// for decommission. The old key remains available for verification
    /// until <paramref name="options"/>.<see cref="KeyRotationOptions.DecommissionOldKey"/>
    /// is set.
    /// </summary>
    /// <param name="currentKeyId">The ID of the key to rotate.</param>
    /// <param name="options">Rotation options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Information about the new replacement key.</returns>
    Task<KeyInfo> RotateKeyAsync(
        string currentKeyId,
        KeyRotationOptions options,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists keys matching the specified filter criteria.
    /// </summary>
    /// <param name="filter">Filter criteria for key listing.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Keys matching the filter.</returns>
    Task<IReadOnlyList<KeyInfo>> ListKeysAsync(
        KeyFilter filter,
        CancellationToken cancellationToken = default);
}
