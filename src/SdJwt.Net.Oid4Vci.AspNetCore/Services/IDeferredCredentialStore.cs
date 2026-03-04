using SdJwt.Net.Oid4Vci.Models;

namespace SdJwt.Net.Oid4Vci.AspNetCore.Services;

/// <summary>
/// Manages deferred credential transactions.
/// </summary>
public interface IDeferredCredentialStore
{
    /// <summary>
    /// Stores the original credential request under a transaction identifier for later retrieval.
    /// </summary>
    /// <param name="transactionId">The transaction identifier (acceptance token).</param>
    /// <param name="request">The original credential request.</param>
    /// <param name="accessToken">The access token that authorized the original request.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    Task SaveAsync(
        string transactionId,
        CredentialRequest request,
        string accessToken,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves and removes the stored credential request for the given transaction identifier.
    /// </summary>
    /// <param name="transactionId">The transaction identifier from the deferred credential request.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The original request and access token, or <c>null</c> if not found or already redeemed.</returns>
    Task<(CredentialRequest Request, string AccessToken)?> RetrieveAsync(
        string transactionId,
        CancellationToken cancellationToken = default);
}
