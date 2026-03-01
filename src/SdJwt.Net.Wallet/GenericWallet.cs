using SdJwt.Net.Wallet.Core;
using SdJwt.Net.Wallet.Formats;
using SdJwt.Net.Wallet.Protocols;
using SdJwt.Net.Wallet.Storage;

namespace SdJwt.Net.Wallet;

/// <summary>
/// Configuration options for the generic wallet.
/// </summary>
public class WalletOptions
{
    /// <summary>
    /// Wallet instance identifier.
    /// </summary>
    public string? WalletId
    {
        get; set;
    }

    /// <summary>
    /// Display name for the wallet.
    /// </summary>
    public string? DisplayName
    {
        get; set;
    }

    /// <summary>
    /// Whether to automatically validate credentials on add.
    /// </summary>
    public bool ValidateOnAdd { get; set; } = true;

    /// <summary>
    /// Whether to automatically check credential status.
    /// </summary>
    public bool AutoCheckStatus
    {
        get; set;
    }

    /// <summary>
    /// Default key generation options.
    /// </summary>
    public KeyGenerationOptions? DefaultKeyOptions
    {
        get; set;
    }
}

/// <summary>
/// Generic extensible wallet for verifiable credentials.
/// Supports multiple credential formats and protocols through plugins.
/// </summary>
public class GenericWallet : ICredentialManager
{
    private readonly ICredentialStore _store;
    private readonly IKeyManager _keyManager;
    private readonly Dictionary<string, ICredentialFormatPlugin> _formatPlugins;
    private readonly WalletOptions _options;

    /// <summary>
    /// Initializes a new instance of the GenericWallet.
    /// </summary>
    /// <param name="store">The credential store implementation.</param>
    /// <param name="keyManager">The key manager implementation.</param>
    /// <param name="formatPlugins">Collection of format plugins.</param>
    /// <param name="options">Wallet configuration options.</param>
    public GenericWallet(
        ICredentialStore store,
        IKeyManager keyManager,
        IEnumerable<ICredentialFormatPlugin>? formatPlugins = null,
        WalletOptions? options = null)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _keyManager = keyManager ?? throw new ArgumentNullException(nameof(keyManager));
        _options = options ?? new WalletOptions();

        _formatPlugins = new Dictionary<string, ICredentialFormatPlugin>(StringComparer.OrdinalIgnoreCase);
        if (formatPlugins != null)
        {
            foreach (var plugin in formatPlugins)
            {
                RegisterFormatPlugin(plugin);
            }
        }
    }

    /// <summary>
    /// Gets the wallet ID.
    /// </summary>
    public string WalletId => _options.WalletId ?? "default";

    /// <summary>
    /// Gets the display name.
    /// </summary>
    public string DisplayName => _options.DisplayName ?? "Generic Wallet";

    /// <summary>
    /// Gets the key manager.
    /// </summary>
    public IKeyManager KeyManager => _keyManager;

    /// <summary>
    /// Registers a credential format plugin.
    /// </summary>
    /// <param name="plugin">The plugin to register.</param>
    public void RegisterFormatPlugin(ICredentialFormatPlugin plugin)
    {
        if (plugin == null)
        {
            throw new ArgumentNullException(nameof(plugin));
        }

        _formatPlugins[plugin.FormatId] = plugin;
    }

    /// <summary>
    /// Gets the format plugin for a credential.
    /// </summary>
    /// <param name="credential">The raw credential.</param>
    /// <returns>The matching format plugin.</returns>
    public ICredentialFormatPlugin? GetFormatPlugin(string credential)
    {
        return _formatPlugins.Values.FirstOrDefault(p => p.CanHandle(credential));
    }

    /// <summary>
    /// Gets a format plugin by format ID.
    /// </summary>
    /// <param name="formatId">The format ID.</param>
    /// <returns>The format plugin if found.</returns>
    public ICredentialFormatPlugin? GetFormatPluginById(string formatId)
    {
        return _formatPlugins.TryGetValue(formatId, out var plugin) ? plugin : null;
    }

    /// <inheritdoc/>
    public async Task<StoredCredential> StoreCredentialAsync(
        ParsedCredential credential,
        string? holderKeyId = null,
        string? documentId = null,
        CancellationToken cancellationToken = default)
    {
        if (credential == null)
        {
            throw new ArgumentNullException(nameof(credential));
        }

        // Validate if configured
        if (_options.ValidateOnAdd && !string.IsNullOrEmpty(credential.Format))
        {
            var plugin = GetFormatPluginById(credential.Format);
            if (plugin != null)
            {
                var validation = await plugin.ValidateAsync(credential, new ValidationContext(), cancellationToken)
                    .ConfigureAwait(false);

                if (!validation.IsValid)
                {
                    throw new InvalidOperationException(
                        $"Credential validation failed: {string.Join(", ", validation.Errors ?? Array.Empty<string>())}");
                }
            }
        }

        // Create stored credential
        var stored = new StoredCredential
        {
            Id = Guid.NewGuid().ToString("N"),
            Format = credential.Format,
            Type = credential.Type,
            Issuer = credential.Issuer,
            Subject = credential.Subject,
            RawCredential = credential.RawCredential,
            IssuedAt = credential.IssuedAt,
            ExpiresAt = credential.ExpiresAt,
            Status = DetermineStatus(credential),
            HolderKeyId = holderKeyId,
            DocumentId = documentId
        };

        // Store the credential
        var id = await _store.StoreAsync(stored, cancellationToken).ConfigureAwait(false);

        return stored with
        {
            Id = id
        };
    }

    /// <inheritdoc/>
    public Task<StoredCredential?> GetCredentialAsync(string credentialId, CancellationToken cancellationToken = default)
    {
        return _store.GetAsync(credentialId, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<StoredCredential>> FindMatchingCredentialsAsync(
        CredentialFilter filter,
        IReadOnlyList<string>? requiredClaims = null,
        CancellationToken cancellationToken = default)
    {
        var query = new CredentialQuery
        {
            Types = filter.CredentialType != null ? new[] { filter.CredentialType } : null,
            Issuers = filter.Issuer != null ? new[] { filter.Issuer } : null,
            FormatId = filter.Format,
            OnlyValid = !filter.IncludeExpired
        };

        var credentials = await _store.QueryAsync(query, cancellationToken).ConfigureAwait(false);

        // TODO: Further filter by requiredClaims if needed (would require parsing each credential)
        return credentials;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<StoredCredential>> ListCredentialsAsync(
        CredentialFilter? filter = null,
        CancellationToken cancellationToken = default)
    {
        if (filter == null)
        {
            return await _store.ListAllAsync(cancellationToken).ConfigureAwait(false);
        }

        var query = new CredentialQuery
        {
            Types = filter.CredentialType != null ? new[] { filter.CredentialType } : null,
            Issuers = filter.Issuer != null ? new[] { filter.Issuer } : null,
            FormatId = filter.Format,
            OnlyValid = !filter.IncludeExpired
        };

        return await _store.QueryAsync(query, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public Task<bool> DeleteCredentialAsync(string credentialId, CancellationToken cancellationToken = default)
    {
        return _store.DeleteAsync(credentialId, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task RecordUsageAsync(string credentialId, CancellationToken cancellationToken = default)
    {
        var stored = await _store.GetAsync(credentialId, cancellationToken).ConfigureAwait(false);
        if (stored == null)
        {
            throw new InvalidOperationException($"Credential '{credentialId}' not found.");
        }

        var updated = stored with
        {
            UsageCount = stored.UsageCount + 1
        };
        await _store.UpdateAsync(updated, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<string> CreatePresentationAsync(
        string credentialId,
        IReadOnlyList<string> disclosurePaths,
        string audience,
        string nonce,
        CancellationToken cancellationToken = default)
    {
        var stored = await _store.GetAsync(credentialId, cancellationToken).ConfigureAwait(false);
        if (stored == null)
        {
            throw new InvalidOperationException($"Credential '{credentialId}' not found.");
        }

        if (string.IsNullOrEmpty(stored.BoundKeyId))
        {
            throw new InvalidOperationException("Credential does not have a bound key for key binding JWT.");
        }

        var plugin = GetFormatPluginById(stored.Format);
        if (plugin == null)
        {
            throw new InvalidOperationException($"No format plugin found for format '{stored.Format}'.");
        }

        // Parse the credential
        var parsed = await plugin.ParseAsync(stored.RawCredential, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        var context = new PresentationContext
        {
            Audience = audience,
            Nonce = nonce,
            KeyId = stored.BoundKeyId
        };

        return await plugin.CreatePresentationAsync(parsed, disclosurePaths, context, _keyManager, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<ValidationResult> ValidateCredentialAsync(
        string credentialId,
        CancellationToken cancellationToken = default)
    {
        var stored = await _store.GetAsync(credentialId, cancellationToken).ConfigureAwait(false);
        if (stored == null)
        {
            return new ValidationResult
            {
                IsValid = false,
                Errors = [$"Credential '{credentialId}' not found."]
            };
        }

        var plugin = GetFormatPluginById(stored.Format);
        if (plugin == null)
        {
            return new ValidationResult
            {
                IsValid = false,
                Errors = [$"No format plugin found for format '{stored.Format}'."]
            };
        }

        var parsed = await plugin.ParseAsync(stored.RawCredential, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        return await plugin.ValidateAsync(parsed, new ValidationContext(), cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Checks credential status.
    /// </summary>
    /// <param name="credentialId">The credential ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The credential status.</returns>
    public async Task<CredentialStatusType> CheckStatusAsync(
        string credentialId,
        CancellationToken cancellationToken = default)
    {
        var stored = await _store.GetAsync(credentialId, cancellationToken).ConfigureAwait(false);
        if (stored == null)
        {
            throw new InvalidOperationException($"Credential '{credentialId}' not found.");
        }

        // Check expiration first
        if (stored.ExpiresAt.HasValue && stored.ExpiresAt.Value <= DateTimeOffset.UtcNow)
        {
            return CredentialStatusType.Expired;
        }

        // Return cached status - real status checking would be done through status list
        return stored.Status;
    }

    /// <summary>
    /// Queries credentials based on filter criteria.
    /// </summary>
    /// <param name="query">Query options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Matching credentials.</returns>
    public Task<IReadOnlyList<StoredCredential>> QueryCredentialsAsync(
        CredentialQuery query,
        CancellationToken cancellationToken = default)
    {
        return _store.QueryAsync(query, cancellationToken);
    }

    /// <summary>
    /// Binds a key to a credential for key binding JWT operations.
    /// </summary>
    /// <param name="credentialId">The credential ID.</param>
    /// <param name="keyId">The key ID to bind.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if binding was successful.</returns>
    public async Task<bool> BindKeyAsync(
        string credentialId,
        string keyId,
        CancellationToken cancellationToken = default)
    {
        var stored = await _store.GetAsync(credentialId, cancellationToken).ConfigureAwait(false);
        if (stored == null)
        {
            return false;
        }

        // Verify the key exists
        if (!await _keyManager.KeyExistsAsync(keyId, cancellationToken).ConfigureAwait(false))
        {
            throw new InvalidOperationException($"Key '{keyId}' not found.");
        }

        var updated = stored with
        {
            BoundKeyId = keyId
        };
        return await _store.UpdateAsync(updated, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the count of stored credentials.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The credential count.</returns>
    public Task<int> GetCredentialCountAsync(CancellationToken cancellationToken = default)
    {
        return _store.CountAsync(null, cancellationToken);
    }

    private static CredentialStatusType DetermineStatus(ParsedCredential parsed)
    {
        if (parsed.ExpiresAt.HasValue && parsed.ExpiresAt.Value <= DateTimeOffset.UtcNow)
        {
            return CredentialStatusType.Expired;
        }

        return CredentialStatusType.Valid;
    }
}
