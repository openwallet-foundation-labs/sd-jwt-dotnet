using SdJwt.Net.Wallet.Core;

namespace SdJwt.Net.Wallet.Storage;

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
                if (query.Types != null && query.Types.Count > 0)
                {
                    var typeSet = new HashSet<string>(query.Types, StringComparer.OrdinalIgnoreCase);
                    results = results.Where(c => c.Type != null && typeSet.Contains(c.Type));
                }

                if (query.Issuers != null && query.Issuers.Count > 0)
                {
                    var issuerSet = new HashSet<string>(query.Issuers, StringComparer.OrdinalIgnoreCase);
                    results = results.Where(c => c.Issuer != null && issuerSet.Contains(c.Issuer));
                }

                if (!string.IsNullOrEmpty(query.FormatId))
                {
                    results = results.Where(c =>
                        string.Equals(c.Format, query.FormatId, StringComparison.OrdinalIgnoreCase));
                }

                if (query.OnlyValid)
                {
                    var now = DateTimeOffset.UtcNow;
                    results = results.Where(c =>
                        (!c.ExpiresAt.HasValue || c.ExpiresAt.Value > now) &&
                        c.Status != CredentialStatusType.Revoked &&
                        c.Status != CredentialStatusType.Suspended);
                }

                if (query.HasKeyBinding.HasValue)
                {
                    results = results.Where(c =>
                        (!string.IsNullOrEmpty(c.BoundKeyId)) == query.HasKeyBinding.Value);
                }

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
