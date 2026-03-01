using SdJwt.Net.Wallet.Core;

namespace SdJwt.Net.Wallet.Status;

/// <summary>
/// Resolves current status for stored credentials.
/// </summary>
public interface IDocumentStatusResolver
{
    /// <summary>
    /// Resolves status for a stored credential.
    /// </summary>
    /// <param name="credential">The credential to check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Status resolution result.</returns>
    Task<DocumentStatusResult> ResolveStatusAsync(
        StoredCredential credential,
        CancellationToken cancellationToken = default);
}
