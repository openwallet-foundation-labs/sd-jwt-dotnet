using SdJwt.Net.Wallet.Core;

namespace SdJwt.Net.Wallet.Attestation;

/// <summary>
/// Provides wallet and key attestations for high-assurance issuance flows.
/// </summary>
public interface IWalletAttestationsProvider
{
    /// <summary>
    /// Gets wallet instance attestation (WIA) for a key used in client authentication.
    /// </summary>
    /// <param name="keyInfo">The key info used for wallet attestation binding.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Attestation token.</returns>
    Task<string> GetWalletAttestationAsync(
        KeyInfo keyInfo,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets wallet unit/key attestation (WUA) for a set of credential keys.
    /// </summary>
    /// <param name="keys">The key infos to attest.</param>
    /// <param name="nonce">Optional issuer nonce.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Attestation token.</returns>
    Task<string> GetKeyAttestationAsync(
        IReadOnlyList<KeyInfo> keys,
        string? nonce = null,
        CancellationToken cancellationToken = default);
}
