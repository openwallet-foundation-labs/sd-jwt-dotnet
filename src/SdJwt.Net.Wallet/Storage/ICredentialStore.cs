using SdJwt.Net.Wallet.Core;

namespace SdJwt.Net.Wallet.Storage;

/// <summary>
/// Query options for filtering credentials.
/// </summary>
public class CredentialQuery
{
    /// <summary>
    /// Filter by credential types.
    /// </summary>
    public IReadOnlyList<string>? Types
    {
        get; set;
    }

    /// <summary>
    /// Filter by issuers.
    /// </summary>
    public IReadOnlyList<string>? Issuers
    {
        get; set;
    }

    /// <summary>
    /// Filter by format ID.
    /// </summary>
    public string? FormatId
    {
        get; set;
    }

    /// <summary>
    /// Include only valid (not expired, not revoked) credentials.
    /// </summary>
    public bool OnlyValid { get; set; } = true;

    /// <summary>
    /// Include only credentials with key binding.
    /// </summary>
    public bool? HasKeyBinding
    {
        get; set;
    }

    /// <summary>
    /// Maximum number of results.
    /// </summary>
    public int? Limit
    {
        get; set;
    }

    /// <summary>
    /// Order by field.
    /// </summary>
    public string? OrderBy
    {
        get; set;
    }

    /// <summary>
    /// Order direction (asc/desc).
    /// </summary>
    public bool Descending
    {
        get; set;
    }
}

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

/// <summary>
/// In-memory implementation of credential storage for testing and development.
/// </summary>
public class InMemoryCredentialStore : ICredentialStore
{
    private readonly Dictionary<string, StoredCredential> _credentials = new(StringComparer.Ordinal);
    private readonly object _lock = new();

    /// <inheritdoc/>
    public Task<string> StoreAsync(StoredCredential credential, CancellationToken cancellationToken = default)
    {
        if (credential == null)
        {
            throw new ArgumentNullException(nameof(credential));
        }

        lock (_lock)
        {
            var id = string.IsNullOrEmpty(credential.Id) ? Guid.NewGuid().ToString("N") : credential.Id;
            var stored = credential with
            {
                Id = id
            };
            _credentials[id] = stored;
            return Task.FromResult(id);
        }
    }

    /// <inheritdoc/>
    public Task<StoredCredential?> GetAsync(string credentialId, CancellationToken cancellationToken = default)
    {
        if (credentialId == null)
        {
            throw new ArgumentNullException(nameof(credentialId));
        }

        lock (_lock)
        {
            return Task.FromResult(_credentials.TryGetValue(credentialId, out var credential)
                ? credential
                : null);
        }
    }

    /// <inheritdoc/>
    public Task<bool> UpdateAsync(StoredCredential credential, CancellationToken cancellationToken = default)
    {
        if (credential == null)
        {
            throw new ArgumentNullException(nameof(credential));
        }

        if (string.IsNullOrEmpty(credential.Id))
        {
            throw new ArgumentException("Credential ID is required for update.", nameof(credential));
        }

        lock (_lock)
        {
            if (!_credentials.ContainsKey(credential.Id))
            {
                return Task.FromResult(false);
            }

            _credentials[credential.Id] = credential;
            return Task.FromResult(true);
        }
    }

    /// <inheritdoc/>
    public Task<bool> DeleteAsync(string credentialId, CancellationToken cancellationToken = default)
    {
        if (credentialId == null)
        {
            throw new ArgumentNullException(nameof(credentialId));
        }

        lock (_lock)
        {
            return Task.FromResult(_credentials.Remove(credentialId));
        }
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<StoredCredential>> QueryAsync(CredentialQuery? query = null, CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            IEnumerable<StoredCredential> results = _credentials.Values;

            if (query != null)
            {
                // Filter by types
                if (query.Types != null && query.Types.Count > 0)
                {
                    var typeSet = new HashSet<string>(query.Types, StringComparer.OrdinalIgnoreCase);
                    results = results.Where(c => c.Type != null && typeSet.Contains(c.Type));
                }

                // Filter by issuers
                if (query.Issuers != null && query.Issuers.Count > 0)
                {
                    var issuerSet = new HashSet<string>(query.Issuers, StringComparer.OrdinalIgnoreCase);
                    results = results.Where(c => c.Issuer != null && issuerSet.Contains(c.Issuer));
                }

                // Filter by format
                if (!string.IsNullOrEmpty(query.FormatId))
                {
                    results = results.Where(c =>
                        string.Equals(c.Format, query.FormatId, StringComparison.OrdinalIgnoreCase));
                }

                // Filter by validity
                if (query.OnlyValid)
                {
                    var now = DateTimeOffset.UtcNow;
                    results = results.Where(c =>
                        (!c.ExpiresAt.HasValue || c.ExpiresAt.Value > now) &&
                        c.Status != CredentialStatusType.Revoked &&
                        c.Status != CredentialStatusType.Suspended);
                }

                // Filter by key binding
                if (query.HasKeyBinding.HasValue)
                {
                    results = results.Where(c =>
                        (!string.IsNullOrEmpty(c.BoundKeyId)) == query.HasKeyBinding.Value);
                }

                // Apply ordering
                if (!string.IsNullOrEmpty(query.OrderBy))
                {
                    results = query.OrderBy.ToLowerInvariant() switch
                    {
                        "issuedat" => query.Descending
                            ? results.OrderByDescending(c => c.IssuedAt)
                            : results.OrderBy(c => c.IssuedAt),
                        "expiresat" => query.Descending
                            ? results.OrderByDescending(c => c.ExpiresAt)
                            : results.OrderBy(c => c.ExpiresAt),
                        "type" => query.Descending
                            ? results.OrderByDescending(c => c.Type)
                            : results.OrderBy(c => c.Type),
                        _ => results
                    };
                }

                // Apply limit
                if (query.Limit.HasValue && query.Limit.Value > 0)
                {
                    results = results.Take(query.Limit.Value);
                }
            }

            return Task.FromResult<IReadOnlyList<StoredCredential>>(results.ToList());
        }
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<StoredCredential>> ListAllAsync(CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            return Task.FromResult<IReadOnlyList<StoredCredential>>(_credentials.Values.ToList());
        }
    }

    /// <inheritdoc/>
    public Task<int> CountAsync(CredentialQuery? query = null, CancellationToken cancellationToken = default)
    {
        if (query == null)
        {
            lock (_lock)
            {
                return Task.FromResult(_credentials.Count);
            }
        }

        // Use QueryAsync and count results for filtered count
        return QueryAsync(query, cancellationToken).ContinueWith(t => t.Result.Count, cancellationToken);
    }

    /// <inheritdoc/>
    public Task ClearAsync(CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            _credentials.Clear();
            return Task.CompletedTask;
        }
    }

    /// <inheritdoc/>
    public Task<bool> ExistsAsync(string credentialId, CancellationToken cancellationToken = default)
    {
        if (credentialId == null)
        {
            throw new ArgumentNullException(nameof(credentialId));
        }

        lock (_lock)
        {
            return Task.FromResult(_credentials.ContainsKey(credentialId));
        }
    }
}
