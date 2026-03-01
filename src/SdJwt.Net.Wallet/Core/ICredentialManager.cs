namespace SdJwt.Net.Wallet.Core;

/// <summary>
/// Manages credentials in the wallet.
/// </summary>
public interface ICredentialManager
{
    /// <summary>
    /// Stores a credential in the wallet.
    /// </summary>
    /// <param name="credential">The parsed credential to store.</param>
    /// <param name="holderKeyId">Optional key ID for holder binding.</param>
    /// <param name="documentId">Optional document ID for batch credentials.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The stored credential with ID.</returns>
    Task<StoredCredential> StoreCredentialAsync(
        ParsedCredential credential,
        string? holderKeyId = null,
        string? documentId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds credentials matching a presentation definition.
    /// </summary>
    /// <param name="filter">Filter criteria.</param>
    /// <param name="requiredClaims">Optional required claim paths.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Matching credentials.</returns>
    Task<IReadOnlyList<StoredCredential>> FindMatchingCredentialsAsync(
        CredentialFilter filter,
        IReadOnlyList<string>? requiredClaims = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a credential by ID.
    /// </summary>
    /// <param name="credentialId">The credential ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The credential, or null if not found.</returns>
    Task<StoredCredential?> GetCredentialAsync(
        string credentialId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists all credentials optionally filtered.
    /// </summary>
    /// <param name="filter">Optional filter criteria.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of credentials.</returns>
    Task<IReadOnlyList<StoredCredential>> ListCredentialsAsync(
        CredentialFilter? filter = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a credential.
    /// </summary>
    /// <param name="credentialId">The credential ID to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if deleted, false if not found.</returns>
    Task<bool> DeleteCredentialAsync(
        string credentialId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Records credential usage for tracking policies.
    /// </summary>
    /// <param name="credentialId">The credential ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task RecordUsageAsync(
        string credentialId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Extended credential manager supporting batch credentials.
/// </summary>
public interface IBatchCredentialManager : ICredentialManager
{
    /// <summary>
    /// Gets the number of valid credentials for a document.
    /// </summary>
    /// <param name="documentId">The document ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Count of valid credentials.</returns>
    Task<int> GetCredentialsCountAsync(
        string documentId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds an available credential based on policy (EUDI feature).
    /// </summary>
    /// <param name="documentId">The document ID.</param>
    /// <param name="policy">The credential policy.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An available credential handle, or null.</returns>
    Task<StoredCredential?> FindAvailableCredentialAsync(
        string documentId,
        CredentialPolicy policy = CredentialPolicy.RotateUse,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Consumes a credential and applies the policy.
    /// For OneTimeUse: deletes the credential after use.
    /// For RotateUse: increments usage counter.
    /// </summary>
    /// <param name="credentialId">The credential ID.</param>
    /// <param name="policy">The credential policy.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task ConsumeCredentialAsync(
        string credentialId,
        CredentialPolicy policy,
        CancellationToken cancellationToken = default);
}
