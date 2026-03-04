using SdJwt.Net.Wallet.Core;

namespace SdJwt.Net.Wallet.Storage;

/// <summary>
/// Abstraction for credential storage operations.
/// </summary>
public interface ICredentialStore
{
    /// <summary>
    /// Stores a credential.
    /// </summary>
    /// <param name="credential">The credential to store.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The stored credential ID.</returns>
    Task<string> StoreAsync(
        StoredCredential credential,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a credential by ID.
    /// </summary>
    /// <param name="credentialId">The credential ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The credential, or null if not found.</returns>
    Task<StoredCredential?> GetAsync(
        string credentialId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing credential.
    /// </summary>
    /// <param name="credential">The credential with updates.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if updated, false if not found.</returns>
    Task<bool> UpdateAsync(
        StoredCredential credential,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a credential by ID.
    /// </summary>
    /// <param name="credentialId">The credential ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if deleted, false if not found.</returns>
    Task<bool> DeleteAsync(
        string credentialId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Queries credentials based on filter criteria.
    /// </summary>
    /// <param name="query">Query options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Matching credentials.</returns>
    Task<IReadOnlyList<StoredCredential>> QueryAsync(
        CredentialQuery? query = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists all credentials.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>All stored credentials.</returns>
    Task<IReadOnlyList<StoredCredential>> ListAllAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the count of stored credentials.
    /// </summary>
    /// <param name="query">Optional query to filter count.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Count of matching credentials.</returns>
    Task<int> CountAsync(
        CredentialQuery? query = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Clears all credentials from the store.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task ClearAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a credential exists.
    /// </summary>
    /// <param name="credentialId">The credential ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the credential exists.</returns>
    Task<bool> ExistsAsync(
        string credentialId,
        CancellationToken cancellationToken = default);
}
