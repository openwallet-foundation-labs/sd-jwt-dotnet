namespace SdJwt.Net.Wallet.Protocols;

/// <summary>
/// Creates DPoP proofs for outbound OpenID4VCI requests.
/// </summary>
public interface IDPoPProofProvider
{
    /// <summary>
    /// Creates a DPoP proof JWT for a target HTTP request.
    /// </summary>
    /// <param name="request">DPoP request description.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>DPoP proof JWT.</returns>
    Task<string> CreateProofAsync(
        DPoPProofRequest request,
        CancellationToken cancellationToken = default);
}
