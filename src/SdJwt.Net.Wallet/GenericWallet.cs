using System.Collections.Concurrent;
using SdJwt.Net.Wallet.Attestation;
using SdJwt.Net.Wallet.Audit;
using SdJwt.Net.Wallet.Core;
using SdJwt.Net.Wallet.Formats;
using SdJwt.Net.Wallet.Issuance;
using SdJwt.Net.Wallet.Protocols;
using SdJwt.Net.Wallet.Sessions;
using SdJwt.Net.Wallet.Status;
using SdJwt.Net.Wallet.Storage;

namespace SdJwt.Net.Wallet;

/// <summary>
/// Generic extensible wallet for verifiable credentials.
/// Supports multiple credential formats and protocols through plugins.
/// </summary>
public class GenericWallet : ICredentialManager, IBatchCredentialManager
{
    private readonly ICredentialStore _store;
    private readonly IKeyManager _keyManager;
    private readonly Dictionary<string, ICredentialFormatPlugin> _formatPlugins;
    private readonly WalletOptions _options;
    private readonly IOid4VciAdapter? _oid4VciAdapter;
    private readonly IOid4VpAdapter? _oid4VpAdapter;
    private readonly IWalletAttestationsProvider? _walletAttestationsProvider;
    private readonly ITransactionLogger? _transactionLogger;
    private readonly IDPoPProofProvider? _dPoPProofProvider;
    private readonly IDocumentStatusResolver? _documentStatusResolver;
    private readonly ConcurrentDictionary<string, WalletIssuerConfiguration> _issuerConfigurations =
        new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, PendingIssuanceSession> _pendingIssuanceSessions =
        new(StringComparer.OrdinalIgnoreCase);

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
        _oid4VciAdapter = _options.Oid4VciAdapter;
        _oid4VpAdapter = _options.Oid4VpAdapter;
        _walletAttestationsProvider = _options.WalletAttestationsProvider;
        _transactionLogger = _options.TransactionLogger;
        _dPoPProofProvider = _options.DPoPProofProvider;
        _documentStatusResolver = _options.DocumentStatusResolver;

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

    /// <summary>
    /// Registers or updates an issuer configuration for OpenID4VCI flows.
    /// </summary>
    /// <param name="configuration">Issuer configuration.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public Task RegisterIssuerConfigurationAsync(
        WalletIssuerConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
        if (configuration == null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }

        if (string.IsNullOrWhiteSpace(configuration.IssuerName))
        {
            throw new ArgumentException("Issuer name cannot be null or empty.", nameof(configuration));
        }

        if (string.IsNullOrWhiteSpace(configuration.CredentialIssuer))
        {
            throw new ArgumentException("Credential issuer cannot be null or empty.", nameof(configuration));
        }

        _issuerConfigurations[configuration.IssuerName] = configuration;
        return Task.CompletedTask;
    }

    /// <summary>
    /// Gets a registered issuer configuration by name.
    /// </summary>
    /// <param name="issuerName">Issuer logical name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The issuer configuration if found; otherwise null.</returns>
    public Task<WalletIssuerConfiguration?> GetIssuerConfigurationAsync(
        string issuerName,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(issuerName))
        {
            throw new ArgumentException("Issuer name cannot be null or empty.", nameof(issuerName));
        }

        return Task.FromResult(
            _issuerConfigurations.TryGetValue(issuerName, out var configuration)
                ? configuration
                : null);
    }

    /// <summary>
    /// Lists all registered issuer configurations.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Registered issuer configurations.</returns>
    public Task<IReadOnlyList<WalletIssuerConfiguration>> ListIssuerConfigurationsAsync(
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<WalletIssuerConfiguration> issuers = _issuerConfigurations.Values
            .OrderBy(i => i.IssuerName, StringComparer.OrdinalIgnoreCase)
            .ToList();
        return Task.FromResult(issuers);
    }

    /// <summary>
    /// Resolves issuer metadata by a registered issuer name.
    /// </summary>
    /// <param name="issuerName">Issuer logical name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Issuer metadata dictionary.</returns>
    public async Task<IDictionary<string, object>> ResolveIssuerMetadataByNameAsync(
        string issuerName,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(issuerName))
        {
            throw new ArgumentException("Issuer name cannot be null or empty.", nameof(issuerName));
        }

        if (_oid4VciAdapter == null)
        {
            throw new InvalidOperationException("OID4VCI adapter is not configured.");
        }

        if (!_issuerConfigurations.TryGetValue(issuerName, out var configuration))
        {
            throw new InvalidOperationException($"Issuer configuration '{issuerName}' is not registered.");
        }

        return await _oid4VciAdapter.ResolveIssuerMetadataAsync(
                configuration.CredentialIssuer,
                cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Starts a resumable issuance session.
    /// </summary>
    /// <param name="offer">Credential offer to process later.</param>
    /// <param name="tokenExchangeOptions">Optional token exchange options override.</param>
    /// <param name="credentialConfigurationId">Optional credential configuration ID override.</param>
    /// <param name="keyId">Optional key ID override.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The pending issuance session details.</returns>
    public Task<PendingIssuanceSession> StartIssuanceSessionAsync(
        CredentialOfferInfo offer,
        TokenExchangeOptions? tokenExchangeOptions = null,
        string? credentialConfigurationId = null,
        string? keyId = null,
        CancellationToken cancellationToken = default)
    {
        if (offer == null)
        {
            throw new ArgumentNullException(nameof(offer));
        }

        var pending = new PendingIssuanceSession
        {
            SessionId = Guid.NewGuid().ToString("N"),
            Offer = offer,
            TokenExchangeOptions = tokenExchangeOptions,
            CredentialConfigurationId = credentialConfigurationId,
            KeyId = keyId
        };

        _pendingIssuanceSessions[pending.SessionId] = pending;
        return Task.FromResult(pending);
    }

    /// <summary>
    /// Resumes a previously started issuance session.
    /// </summary>
    /// <param name="sessionId">Pending session identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Issuance result.</returns>
    public async Task<IssuanceResult> ResumeIssuanceSessionAsync(
        string sessionId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
        {
            throw new ArgumentException("Session ID cannot be null or empty.", nameof(sessionId));
        }

        if (!_pendingIssuanceSessions.TryGetValue(sessionId, out var pending))
        {
            throw new InvalidOperationException($"Pending issuance session '{sessionId}' was not found.");
        }

        var result = await AcceptCredentialOfferAsync(
                pending.Offer,
                pending.TokenExchangeOptions,
                pending.CredentialConfigurationId,
                pending.KeyId,
                cancellationToken)
            .ConfigureAwait(false);

        if (result.IsSuccessful)
        {
            _pendingIssuanceSessions.TryRemove(sessionId, out _);
        }

        return result;
    }

    /// <summary>
    /// Processes an OpenID4VCI credential offer URI or JSON payload.
    /// </summary>
    /// <param name="offer">The credential offer URI or JSON payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The parsed credential offer information.</returns>
    public async Task<CredentialOfferInfo> ProcessCredentialOfferAsync(
        string offer,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(offer))
        {
            throw new ArgumentException("Credential offer cannot be null or empty.", nameof(offer));
        }

        if (_oid4VciAdapter == null)
        {
            throw new InvalidOperationException("OID4VCI adapter is not configured.");
        }

        return await _oid4VciAdapter.ParseOfferAsync(offer, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Accepts a parsed OpenID4VCI credential offer and stores issued credentials.
    /// </summary>
    /// <param name="offer">The parsed credential offer.</param>
    /// <param name="tokenExchangeOptions">Optional token exchange options override.</param>
    /// <param name="credentialConfigurationId">Optional credential configuration ID override.</param>
    /// <param name="keyId">Optional key ID override for proof generation.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The issuance result including stored credentials.</returns>
    public async Task<IssuanceResult> AcceptCredentialOfferAsync(
        CredentialOfferInfo offer,
        TokenExchangeOptions? tokenExchangeOptions = null,
        string? credentialConfigurationId = null,
        string? keyId = null,
        CancellationToken cancellationToken = default)
    {
        if (offer == null)
        {
            throw new ArgumentNullException(nameof(offer));
        }

        if (_oid4VciAdapter == null)
        {
            throw new InvalidOperationException("OID4VCI adapter is not configured.");
        }

        if (string.IsNullOrWhiteSpace(offer.CredentialIssuer))
        {
            throw new InvalidOperationException("Credential offer is missing the credential issuer.");
        }

        try
        {
            var metadata = await _oid4VciAdapter.ResolveIssuerMetadataAsync(
                    offer.CredentialIssuer,
                    cancellationToken)
                .ConfigureAwait(false);

            var tokenEndpoint = GetRequiredMetadataValue(metadata, "token_endpoint");
            var credentialEndpoint = GetRequiredMetadataValue(metadata, "credential_endpoint");

            var exchangeOptions = tokenExchangeOptions ?? BuildTokenExchangeOptions(offer);
            exchangeOptions = await EnsureDpopProofAsync(
                    exchangeOptions,
                    tokenEndpoint,
                    cancellationToken)
                .ConfigureAwait(false);
            var tokenResult = await _oid4VciAdapter.ExchangeTokenAsync(tokenEndpoint, exchangeOptions, cancellationToken)
                .ConfigureAwait(false);

            if (!tokenResult.IsSuccessful)
            {
                var failedResult = new IssuanceResult
                {
                    IsSuccessful = false,
                    ErrorCode = tokenResult.ErrorCode,
                    ErrorDescription = tokenResult.ErrorDescription,
                    CNonce = tokenResult.CNonce,
                    CNonceExpiresIn = tokenResult.CNonceExpiresIn
                };
                await LogTransactionAsync(
                        TransactionType.Issuance,
                        TransactionStatus.Error,
                        "AcceptCredentialOffer",
                        failedResult.ErrorDescription,
                        cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
                return failedResult;
            }

            if (string.IsNullOrWhiteSpace(tokenResult.AccessToken))
            {
                throw new InvalidOperationException("Token exchange succeeded but no access token was returned.");
            }

            var selectedCredentialConfigurationId = credentialConfigurationId ??
                offer.CredentialConfigurationIds.FirstOrDefault();
            if (string.IsNullOrWhiteSpace(selectedCredentialConfigurationId))
            {
                throw new InvalidOperationException("No credential configuration ID is available in the offer.");
            }

            var selectedKeyId = keyId;
            if (string.IsNullOrWhiteSpace(selectedKeyId))
            {
                var generatedKey = await _keyManager.GenerateKeyAsync(
                        _options.DefaultKeyOptions ?? new KeyGenerationOptions(),
                        cancellationToken)
                    .ConfigureAwait(false);
                selectedKeyId = generatedKey.KeyId;
            }

            if (string.IsNullOrWhiteSpace(selectedKeyId))
            {
                throw new InvalidOperationException("Unable to determine a key ID for credential proof creation.");
            }

            var requestOptions = new CredentialRequestOptions
            {
                AccessToken = tokenResult.AccessToken,
                CredentialConfigurationId = selectedCredentialConfigurationId,
                KeyId = selectedKeyId,
                CNonce = tokenResult.CNonce,
                Issuer = offer.CredentialIssuer
            };

            var issuanceResult = await _oid4VciAdapter.RequestCredentialAsync(
                    credentialEndpoint,
                    requestOptions,
                    _keyManager,
                    cancellationToken)
                .ConfigureAwait(false);

            var storedResult = await StoreIssuedCredentialsAsync(issuanceResult, cancellationToken).ConfigureAwait(false);
            await LogTransactionAsync(
                    TransactionType.Issuance,
                    storedResult.IsSuccessful ? TransactionStatus.Completed : TransactionStatus.Error,
                    "AcceptCredentialOffer",
                    storedResult.ErrorDescription,
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            return storedResult;
        }
        catch (Exception ex)
        {
            await LogTransactionAsync(
                    TransactionType.Issuance,
                    TransactionStatus.Error,
                    "AcceptCredentialOffer",
                    ex.Message,
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            throw;
        }
    }

    /// <summary>
    /// Processes an OpenID4VP presentation request URI or JSON payload.
    /// </summary>
    /// <param name="request">The presentation request URI or JSON payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The parsed presentation request information.</returns>
    public async Task<PresentationRequestInfo> ProcessPresentationRequestAsync(
        string request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request))
        {
            throw new ArgumentException("Presentation request cannot be null or empty.", nameof(request));
        }

        if (_oid4VpAdapter == null)
        {
            throw new InvalidOperationException("OID4VP adapter is not configured.");
        }

        return await _oid4VpAdapter.ParseRequestAsync(request, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Resolves an OpenID4VP request URI to the actual request object.
    /// </summary>
    /// <param name="requestUri">The request URI.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The resolved presentation request information.</returns>
    public async Task<PresentationRequestInfo> ResolvePresentationRequestUriAsync(
        string requestUri,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(requestUri))
        {
            throw new ArgumentException("Request URI cannot be null or empty.", nameof(requestUri));
        }

        if (_oid4VpAdapter == null)
        {
            throw new InvalidOperationException("OID4VP adapter is not configured.");
        }

        return await _oid4VpAdapter.ResolveRequestUriAsync(requestUri, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Creates and submits a presentation response using the configured OpenID4VP adapter.
    /// </summary>
    /// <param name="request">The parsed presentation request.</param>
    /// <param name="options">Optional submission options. If no matches are provided, matching is performed automatically.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The presentation submission result.</returns>
    public async Task<PresentationSubmissionResult> CreateAndSubmitPresentationAsync(
        PresentationRequestInfo request,
        PresentationSubmissionOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (_oid4VpAdapter == null)
        {
            throw new InvalidOperationException("OID4VP adapter is not configured.");
        }

        try
        {
            var configuredOptions = options ?? new PresentationSubmissionOptions();
            IReadOnlyList<CredentialMatch> matches = configuredOptions.Matches;

            if (matches.Count == 0)
            {
                var availableCredentials = await _store.ListAllAsync(cancellationToken).ConfigureAwait(false);
                matches = await _oid4VpAdapter.FindMatchingCredentialsAsync(
                        request,
                        availableCredentials,
                        cancellationToken)
                    .ConfigureAwait(false);
            }

            var submitOptions = new PresentationSubmissionOptions
            {
                Matches = matches,
                KeyManager = configuredOptions.KeyManager ?? _keyManager,
                State = configuredOptions.State
            };

            var result = await _oid4VpAdapter.SubmitPresentationAsync(request, submitOptions, cancellationToken)
                .ConfigureAwait(false);
            await LogTransactionAsync(
                    TransactionType.Presentation,
                    result.IsSuccessful ? TransactionStatus.Completed : TransactionStatus.Error,
                    "CreateAndSubmitPresentation",
                    result.ErrorDescription,
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            return result;
        }
        catch (Exception ex)
        {
            await LogTransactionAsync(
                    TransactionType.Presentation,
                    TransactionStatus.Error,
                    "CreateAndSubmitPresentation",
                    ex.Message,
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            throw;
        }
    }

    /// <summary>
    /// Generates wallet attestation for a specific wallet key.
    /// </summary>
    /// <param name="keyId">The key identifier to attest.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Wallet attestation token.</returns>
    public async Task<string> GenerateWalletAttestationAsync(
        string keyId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(keyId))
        {
            throw new ArgumentException("Key ID cannot be null or empty.", nameof(keyId));
        }

        if (_walletAttestationsProvider == null)
        {
            throw new InvalidOperationException("Wallet attestations provider is not configured.");
        }

        var keyInfo = await _keyManager.GetKeyInfoAsync(keyId, cancellationToken).ConfigureAwait(false);
        if (keyInfo == null)
        {
            throw new InvalidOperationException($"Key '{keyId}' not found.");
        }

        try
        {
            var token = await _walletAttestationsProvider
                .GetWalletAttestationAsync(keyInfo, cancellationToken)
                .ConfigureAwait(false);
            await LogTransactionAsync(
                    TransactionType.Attestation,
                    TransactionStatus.Completed,
                    "GenerateWalletAttestation",
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            return token;
        }
        catch (Exception ex)
        {
            await LogTransactionAsync(
                    TransactionType.Attestation,
                    TransactionStatus.Error,
                    "GenerateWalletAttestation",
                    ex.Message,
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            throw;
        }
    }

    /// <summary>
    /// Generates key attestation token for one or more wallet keys.
    /// </summary>
    /// <param name="keyIds">The key identifiers to attest.</param>
    /// <param name="nonce">Optional nonce from issuer.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Key attestation token.</returns>
    public async Task<string> GenerateKeyAttestationAsync(
        IReadOnlyList<string> keyIds,
        string? nonce = null,
        CancellationToken cancellationToken = default)
    {
        if (keyIds == null)
        {
            throw new ArgumentNullException(nameof(keyIds));
        }

        if (keyIds.Count == 0)
        {
            throw new ArgumentException("At least one key ID must be provided.", nameof(keyIds));
        }

        if (_walletAttestationsProvider == null)
        {
            throw new InvalidOperationException("Wallet attestations provider is not configured.");
        }

        var keys = new List<KeyInfo>(keyIds.Count);
        foreach (var keyId in keyIds)
        {
            if (string.IsNullOrWhiteSpace(keyId))
            {
                throw new ArgumentException("Key IDs cannot contain null or empty values.", nameof(keyIds));
            }

            var keyInfo = await _keyManager.GetKeyInfoAsync(keyId, cancellationToken).ConfigureAwait(false);
            if (keyInfo == null)
            {
                throw new InvalidOperationException($"Key '{keyId}' not found.");
            }

            keys.Add(keyInfo);
        }

        try
        {
            var token = await _walletAttestationsProvider
                .GetKeyAttestationAsync(keys, nonce, cancellationToken)
                .ConfigureAwait(false);
            await LogTransactionAsync(
                    TransactionType.Attestation,
                    TransactionStatus.Completed,
                    "GenerateKeyAttestation",
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            return token;
        }
        catch (Exception ex)
        {
            await LogTransactionAsync(
                    TransactionType.Attestation,
                    TransactionStatus.Error,
                    "GenerateKeyAttestation",
                    ex.Message,
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            throw;
        }
    }

    /// <summary>
    /// Creates a remote OpenID4VP presentation session.
    /// </summary>
    /// <returns>Presentation session abstraction for remote verifier flows.</returns>
    public IPresentationSession CreateRemotePresentationSession()
    {
        return new RemotePresentationSession(this);
    }

    /// <summary>
    /// Creates a proximity presentation session placeholder.
    /// </summary>
    /// <returns>Presentation session abstraction for proximity flows.</returns>
    public IPresentationSession CreateProximityPresentationSession()
    {
        return new ProximityPresentationSession();
    }

    /// <summary>
    /// Builds an authorization URL for OpenID4VCI authorization code flow.
    /// </summary>
    /// <param name="authorizationEndpoint">Authorization endpoint URL.</param>
    /// <param name="clientId">Wallet client identifier.</param>
    /// <param name="redirectUri">Wallet redirect URI.</param>
    /// <param name="scope">Optional scope.</param>
    /// <param name="authorizationDetails">Optional authorization details payload.</param>
    /// <param name="state">Optional state for CSRF protection.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The authorization URL to open in the user agent.</returns>
    public async Task<string> BuildAuthorizationUrlAsync(
        string authorizationEndpoint,
        string clientId,
        string redirectUri,
        string? scope = null,
        string? authorizationDetails = null,
        string? state = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(authorizationEndpoint))
        {
            throw new ArgumentException("Authorization endpoint cannot be null or empty.", nameof(authorizationEndpoint));
        }

        if (string.IsNullOrWhiteSpace(clientId))
        {
            throw new ArgumentException("Client ID cannot be null or empty.", nameof(clientId));
        }

        if (string.IsNullOrWhiteSpace(redirectUri))
        {
            throw new ArgumentException("Redirect URI cannot be null or empty.", nameof(redirectUri));
        }

        if (_oid4VciAdapter == null)
        {
            throw new InvalidOperationException("OID4VCI adapter is not configured.");
        }

        return await _oid4VciAdapter.BuildAuthorizationUrlAsync(
                authorizationEndpoint,
                clientId,
                redirectUri,
                scope,
                authorizationDetails,
                state,
                cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Polls issuer deferred endpoint and stores credentials when issuance is ready.
    /// </summary>
    /// <param name="deferredEndpoint">Deferred credential endpoint.</param>
    /// <param name="transactionId">Deferred transaction identifier.</param>
    /// <param name="accessToken">Access token for deferred endpoint access.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The issuance result including stored credentials if any are returned.</returns>
    public async Task<IssuanceResult> PollDeferredCredentialAsync(
        string deferredEndpoint,
        string transactionId,
        string accessToken,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(deferredEndpoint))
        {
            throw new ArgumentException("Deferred endpoint cannot be null or empty.", nameof(deferredEndpoint));
        }

        if (string.IsNullOrWhiteSpace(transactionId))
        {
            throw new ArgumentException("Transaction ID cannot be null or empty.", nameof(transactionId));
        }

        if (string.IsNullOrWhiteSpace(accessToken))
        {
            throw new ArgumentException("Access token cannot be null or empty.", nameof(accessToken));
        }

        if (_oid4VciAdapter == null)
        {
            throw new InvalidOperationException("OID4VCI adapter is not configured.");
        }

        var issuanceResult = await _oid4VciAdapter.PollDeferredCredentialAsync(
                deferredEndpoint,
                transactionId,
                accessToken,
                cancellationToken)
            .ConfigureAwait(false);

        return await StoreIssuedCredentialsAsync(issuanceResult, cancellationToken).ConfigureAwait(false);
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

        if (_documentStatusResolver != null)
        {
            var statusResult = await _documentStatusResolver.ResolveStatusAsync(stored, cancellationToken)
                .ConfigureAwait(false);
            return MapDocumentStatus(statusResult?.Status ?? DocumentStatus.Reserved);
        }

        // Return cached status when no resolver is configured.
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

    /// <inheritdoc/>
    public async Task<int> GetCredentialsCountAsync(
        string documentId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(documentId))
        {
            throw new ArgumentException("Document ID cannot be null or empty.", nameof(documentId));
        }

        var credentials = await _store.ListAllAsync(cancellationToken).ConfigureAwait(false);
        return credentials.Count(c =>
            string.Equals(c.DocumentId, documentId, StringComparison.Ordinal) &&
            IsCredentialUsable(c));
    }

    /// <inheritdoc/>
    public async Task<StoredCredential?> FindAvailableCredentialAsync(
        string documentId,
        CredentialPolicy policy = CredentialPolicy.RotateUse,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(documentId))
        {
            throw new ArgumentException("Document ID cannot be null or empty.", nameof(documentId));
        }

        var credentials = await _store.ListAllAsync(cancellationToken).ConfigureAwait(false);
        var candidates = credentials
            .Where(c => string.Equals(c.DocumentId, documentId, StringComparison.Ordinal) && IsCredentialUsable(c))
            .ToList();

        if (candidates.Count == 0)
        {
            return null;
        }

        return policy switch
        {
            CredentialPolicy.OneTimeUse => candidates[0],
            CredentialPolicy.RotateUse => candidates
                .OrderBy(c => c.UsageCount)
                .ThenBy(c => c.StoredAt)
                .First(),
            _ => candidates
                .OrderBy(c => c.UsageCount)
                .ThenBy(c => c.StoredAt)
                .First()
        };
    }

    /// <inheritdoc/>
    public async Task ConsumeCredentialAsync(
        string credentialId,
        CredentialPolicy policy,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(credentialId))
        {
            throw new ArgumentException("Credential ID cannot be null or empty.", nameof(credentialId));
        }

        var credential = await _store.GetAsync(credentialId, cancellationToken).ConfigureAwait(false);
        if (credential == null)
        {
            throw new InvalidOperationException($"Credential '{credentialId}' not found.");
        }

        if (policy == CredentialPolicy.OneTimeUse)
        {
            var deleted = await _store.DeleteAsync(credentialId, cancellationToken).ConfigureAwait(false);
            if (!deleted)
            {
                throw new InvalidOperationException($"Credential '{credentialId}' could not be deleted.");
            }

            return;
        }

        var updated = credential with
        {
            UsageCount = credential.UsageCount + 1
        };
        var isUpdated = await _store.UpdateAsync(updated, cancellationToken).ConfigureAwait(false);
        if (!isUpdated)
        {
            throw new InvalidOperationException($"Credential '{credentialId}' could not be updated.");
        }
    }

    private async Task LogTransactionAsync(
        TransactionType type,
        TransactionStatus status,
        string operation,
        string? error = null,
        IDictionary<string, object>? metadata = null,
        CancellationToken cancellationToken = default)
    {
        if (_transactionLogger == null)
        {
            return;
        }

        try
        {
            await _transactionLogger.LogAsync(
                    new TransactionLog
                    {
                        Type = type,
                        Status = status,
                        Operation = operation,
                        Error = error,
                        Metadata = metadata
                    },
                    cancellationToken)
                .ConfigureAwait(false);
        }
        catch
        {
            // Transaction logging is best-effort and must not break protocol flows.
        }
    }

    private async Task<TokenExchangeOptions> EnsureDpopProofAsync(
        TokenExchangeOptions options,
        string tokenEndpoint,
        CancellationToken cancellationToken)
    {
        if (_dPoPProofProvider == null || !string.IsNullOrWhiteSpace(options.DPoPProof))
        {
            return options;
        }

        var proof = await _dPoPProofProvider.CreateProofAsync(
                new DPoPProofRequest
                {
                    HttpMethod = "POST",
                    HttpUri = tokenEndpoint
                },
                cancellationToken)
            .ConfigureAwait(false);

        if (string.IsNullOrWhiteSpace(proof))
        {
            return options;
        }

        return new TokenExchangeOptions
        {
            PreAuthorizedCode = options.PreAuthorizedCode,
            TxCode = options.TxCode,
            AuthorizationCode = options.AuthorizationCode,
            CodeVerifier = options.CodeVerifier,
            RedirectUri = options.RedirectUri,
            DPoPProof = proof
        };
    }

    private static CredentialStatusType MapDocumentStatus(DocumentStatus status)
    {
        return status switch
        {
            DocumentStatus.Valid => CredentialStatusType.Valid,
            DocumentStatus.Invalid => CredentialStatusType.Revoked,
            DocumentStatus.Suspended => CredentialStatusType.Suspended,
            DocumentStatus.ApplicationSpecific => CredentialStatusType.Unknown,
            DocumentStatus.Reserved => CredentialStatusType.Unknown,
            _ => CredentialStatusType.Unknown
        };
    }

    private static TokenExchangeOptions BuildTokenExchangeOptions(CredentialOfferInfo offer)
    {
        return new TokenExchangeOptions
        {
            PreAuthorizedCode = offer.PreAuthorizedCode?.Code
        };
    }

    private async Task<IssuanceResult> StoreIssuedCredentialsAsync(
        IssuanceResult issuanceResult,
        CancellationToken cancellationToken)
    {
        if (!issuanceResult.IsSuccessful || issuanceResult.Credentials.Count == 0)
        {
            return issuanceResult;
        }

        var storedCredentials = new List<StoredCredential>(issuanceResult.Credentials.Count);
        foreach (var issuedCredential in issuanceResult.Credentials)
        {
            var storeId = await _store.StoreAsync(issuedCredential, cancellationToken).ConfigureAwait(false);
            storedCredentials.Add(issuedCredential with
            {
                Id = storeId
            });
        }

        return new IssuanceResult
        {
            IsSuccessful = issuanceResult.IsSuccessful,
            Credentials = storedCredentials,
            TransactionId = issuanceResult.TransactionId,
            DeferredEndpoint = issuanceResult.DeferredEndpoint,
            ErrorCode = issuanceResult.ErrorCode,
            ErrorDescription = issuanceResult.ErrorDescription,
            CNonce = issuanceResult.CNonce,
            CNonceExpiresIn = issuanceResult.CNonceExpiresIn
        };
    }

    private static bool IsCredentialUsable(StoredCredential credential)
    {
        var isExpired = credential.ExpiresAt.HasValue && credential.ExpiresAt.Value <= DateTimeOffset.UtcNow;
        var isInvalidStatus = credential.Status is CredentialStatusType.Revoked or CredentialStatusType.Suspended;
        return !isExpired && !isInvalidStatus;
    }

    private static string GetRequiredMetadataValue(IDictionary<string, object> metadata, string key)
    {
        if (metadata.TryGetValue(key, out var value))
        {
            return CoerceMetadataValue(value, key);
        }

        foreach (var (metadataKey, metadataValue) in metadata)
        {
            if (metadataKey.Equals(key, StringComparison.OrdinalIgnoreCase))
            {
                return CoerceMetadataValue(metadataValue, key);
            }
        }

        throw new InvalidOperationException($"Issuer metadata is missing '{key}'.");
    }

    private static string CoerceMetadataValue(object? value, string key)
    {
        if (value is string valueString && !string.IsNullOrWhiteSpace(valueString))
        {
            return valueString;
        }

        if (value is Uri uri)
        {
            return uri.ToString();
        }

        if (value != null)
        {
            var converted = value.ToString();
            if (!string.IsNullOrWhiteSpace(converted))
            {
                return converted;
            }
        }

        throw new InvalidOperationException($"Issuer metadata value '{key}' is empty.");
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
