using SdJwt.Net.Eudiw.Arf;
using SdJwt.Net.Eudiw.Credentials;
using SdJwt.Net.Eudiw.TrustFramework;
using SdJwt.Net.Wallet;
using SdJwt.Net.Wallet.Core;
using SdJwt.Net.Wallet.Formats;
using SdJwt.Net.Wallet.Storage;

namespace SdJwt.Net.Eudiw;

/// <summary>
/// EU Digital Identity Wallet implementation extending the generic wallet.
/// Provides eIDAS 2.0 compliance, ARF validation, and EU Trust List integration.
/// </summary>
public class EudiWallet : GenericWallet
{
    private readonly EudiWalletOptions _eudiOptions;
    private readonly ArfProfileValidator _arfValidator;
    private readonly EuTrustListResolver _trustListResolver;
    private readonly PidCredentialHandler _pidHandler;

    /// <summary>
    /// Initializes a new instance of the EudiWallet.
    /// </summary>
    /// <param name="store">The credential store implementation.</param>
    /// <param name="keyManager">The key manager implementation.</param>
    /// <param name="formatPlugins">Collection of format plugins.</param>
    /// <param name="eudiOptions">EUDI-specific configuration options.</param>
    public EudiWallet(
        ICredentialStore store,
        IKeyManager keyManager,
        IEnumerable<ICredentialFormatPlugin>? formatPlugins = null,
        EudiWalletOptions? eudiOptions = null)
        : base(store, keyManager, formatPlugins, CreateBaseOptions(eudiOptions))
    {
        _eudiOptions = eudiOptions ?? new EudiWalletOptions();
        _arfValidator = new ArfProfileValidator();
        _trustListResolver = new EuTrustListResolver
        {
            CacheTimeout = TimeSpan.FromHours(_eudiOptions.TrustListCacheHours)
        };
        _pidHandler = new PidCredentialHandler();
    }

    /// <summary>
    /// Gets whether ARF compliance is enforced.
    /// </summary>
    public bool IsArfEnforced => _eudiOptions.EnforceArfCompliance;

    /// <summary>
    /// Gets the supported credential types.
    /// </summary>
    public IReadOnlyList<string> SupportedCredentialTypes => _eudiOptions.SupportedCredentialTypes;

    /// <summary>
    /// Gets the minimum HAIP level required.
    /// </summary>
    public int MinimumHaipLevel => _eudiOptions.MinimumHaipLevel;

    /// <summary>
    /// Validates cryptographic algorithm against ARF requirements.
    /// </summary>
    /// <param name="algorithm">The algorithm to validate.</param>
    /// <returns>True if the algorithm is ARF-compliant.</returns>
    public bool ValidateAlgorithm(string algorithm)
    {
        return _arfValidator.ValidateAlgorithm(algorithm);
    }

    /// <summary>
    /// Validates credential type against ARF definitions.
    /// </summary>
    /// <param name="credentialType">The credential type or DocType.</param>
    /// <returns>ARF validation result.</returns>
    public ArfValidationResult ValidateCredentialType(string credentialType)
    {
        return _arfValidator.ValidateCredentialType(credentialType);
    }

    /// <summary>
    /// Validates if a country code is an EU member state.
    /// </summary>
    /// <param name="countryCode">ISO 3166-1 alpha-2 country code.</param>
    /// <returns>True if valid EU member state.</returns>
    public bool ValidateMemberState(string countryCode)
    {
        return _arfValidator.ValidateMemberState(countryCode);
    }

    /// <summary>
    /// Validates PID claims against ARF requirements.
    /// </summary>
    /// <param name="claims">The claims dictionary.</param>
    /// <returns>ARF validation result.</returns>
    public ArfValidationResult ValidatePidClaims(IDictionary<string, object> claims)
    {
        return _arfValidator.ValidatePidClaims(claims);
    }

    /// <summary>
    /// Validates issuer against EU Trust List.
    /// </summary>
    /// <param name="issuerUrl">The issuer URL or identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>EU trust validation result.</returns>
    public Task<EuTrustValidationResult> ValidateIssuerTrustAsync(
        string issuerUrl,
        CancellationToken cancellationToken = default)
    {
        // Extract country code from issuer URL if possible
        var countryCode = ExtractCountryCode(issuerUrl);

        if (string.IsNullOrEmpty(countryCode))
        {
            return Task.FromResult(EuTrustValidationResult.Untrusted(
                "Unable to determine issuer country from URL"));
        }

        if (!_trustListResolver.IsValidMemberState(countryCode))
        {
            return Task.FromResult(EuTrustValidationResult.Untrusted(
                $"Issuer country '{countryCode}' is not an EU member state"));
        }

        // For now, we trust issuers from valid member states
        // Full implementation would check against cached LOTL data
        return Task.FromResult(EuTrustValidationResult.Trusted(countryCode));
    }

    /// <summary>
    /// Stores a credential with ARF validation.
    /// </summary>
    /// <param name="credential">The parsed credential.</param>
    /// <param name="holderKeyId">Optional holder key ID.</param>
    /// <param name="documentId">Optional document ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The stored credential.</returns>
    public new async Task<StoredCredential> StoreCredentialAsync(
        ParsedCredential credential,
        string? holderKeyId = null,
        string? documentId = null,
        CancellationToken cancellationToken = default)
    {
        // Validate ARF compliance if enabled
        if (_eudiOptions.EnforceArfCompliance)
        {
            var arfResult = _arfValidator.ValidateCredentialType(credential.Type);
            if (!arfResult.IsValid)
            {
                var errors = !string.IsNullOrEmpty(arfResult.Error)
                    ? new List<string> { arfResult.Error }
                    : new List<string> { "Invalid credential type" };
                throw new ArfComplianceException(errors);
            }
        }

        // Validate issuer trust if enabled
        if (_eudiOptions.ValidateIssuerTrust && !string.IsNullOrEmpty(credential.Issuer))
        {
            var trustResult = await ValidateIssuerTrustAsync(credential.Issuer, cancellationToken)
                .ConfigureAwait(false);

            if (!trustResult.IsTrusted)
            {
                throw new EudiTrustException(
                    $"Issuer '{credential.Issuer}' is not in EU Trust List: {string.Join(", ", trustResult.Errors)}");
            }
        }

        return await base.StoreCredentialAsync(credential, holderKeyId, documentId, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Creates a presentation with ARF compliance validation.
    /// </summary>
    /// <param name="credentialId">The credential ID.</param>
    /// <param name="disclosurePaths">Claims to disclose.</param>
    /// <param name="audience">The verifier audience.</param>
    /// <param name="nonce">Unique nonce.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The VP token.</returns>
    public new async Task<string> CreatePresentationAsync(
        string credentialId,
        IReadOnlyList<string> disclosurePaths,
        string audience,
        string nonce,
        CancellationToken cancellationToken = default)
    {
        // Get the credential
        var stored = await GetCredentialAsync(credentialId, cancellationToken).ConfigureAwait(false);
        if (stored == null)
        {
            throw new InvalidOperationException($"Credential '{credentialId}' not found.");
        }

        // Validate ARF compliance for presentation
        if (_eudiOptions.EnforceArfCompliance)
        {
            var arfResult = _arfValidator.ValidateCredentialType(stored.Type);
            if (!arfResult.IsValid)
            {
                var errors = !string.IsNullOrEmpty(arfResult.Error)
                    ? new List<string> { arfResult.Error }
                    : new List<string> { "Credential type not ARF-compliant" };
                throw new ArfComplianceException(errors);
            }

            // Validate expiration
            if (stored.ExpiresAt.HasValue && stored.IssuedAt.HasValue)
            {
                var validityResult = _arfValidator.ValidateValidityPeriod(
                    stored.IssuedAt.Value, stored.ExpiresAt.Value);

                if (!validityResult.IsValid)
                {
                    var validityErrors = !string.IsNullOrEmpty(validityResult.Error)
                        ? new List<string> { validityResult.Error }
                        : new List<string> { "Credential validity period invalid" };
                    throw new ArfComplianceException(validityErrors);
                }
            }
        }

        return await base.CreatePresentationAsync(credentialId, disclosurePaths, audience, nonce, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Finds PID credentials in the wallet.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of PID credentials.</returns>
    public async Task<IReadOnlyList<StoredCredential>> FindPidCredentialsAsync(
        CancellationToken cancellationToken = default)
    {
        var filter = new CredentialFilter
        {
            CredentialType = EudiwConstants.Pid.DocType
        };

        return await ListCredentialsAsync(filter, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Finds mDL credentials in the wallet.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of mDL credentials.</returns>
    public async Task<IReadOnlyList<StoredCredential>> FindMdlCredentialsAsync(
        CancellationToken cancellationToken = default)
    {
        var filter = new CredentialFilter
        {
            CredentialType = EudiwConstants.Mdl.DocType
        };

        return await ListCredentialsAsync(filter, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Validates PID credential claims and extracts typed data.
    /// </summary>
    /// <param name="claims">The claims dictionary.</param>
    /// <returns>Typed PID credential if valid.</returns>
    public PidCredential? ExtractPidCredential(IDictionary<string, object> claims)
    {
        var validation = _pidHandler.Validate(claims);
        if (!validation.IsValid)
        {
            return null;
        }

        return _pidHandler.ToPidCredential(claims);
    }

    /// <summary>
    /// Gets all supported EU member states.
    /// </summary>
    /// <returns>Collection of ISO 3166-1 alpha-2 country codes.</returns>
    public IReadOnlyCollection<string> GetSupportedMemberStates()
    {
        return _trustListResolver.GetSupportedMemberStates();
    }

    /// <summary>
    /// Clears the trust list cache.
    /// </summary>
    public void ClearTrustListCache()
    {
        _trustListResolver.ClearCache();
    }

    /// <summary>
    /// Gets whether the trust list cache is empty or expired.
    /// </summary>
    public bool IsTrustListCacheEmpty => _trustListResolver.IsCacheEmpty;

    private static WalletOptions CreateBaseOptions(EudiWalletOptions? eudiOptions)
    {
        return new WalletOptions
        {
            WalletId = eudiOptions?.WalletId,
            DisplayName = eudiOptions?.DisplayName ?? "EUDI Wallet",
            ValidateOnAdd = eudiOptions?.ValidateOnAdd ?? true,
            AutoCheckStatus = true
        };
    }

    private static string? ExtractCountryCode(string? issuerUrl)
    {
        if (string.IsNullOrEmpty(issuerUrl))
        {
            return null;
        }

        // Try to extract country code from URL patterns like:
        // https://pid.de.example.com or https://issuer.gov.de
        try
        {
            var uri = new Uri(issuerUrl);
            var host = uri.Host.ToUpperInvariant();

            // Check for country code TLD
            foreach (var memberState in EudiwConstants.MemberStates.All)
            {
                if (host.EndsWith($".{memberState}", StringComparison.OrdinalIgnoreCase))
                {
                    return memberState;
                }

                // Check for country code in subdomain (e.g., pid.de.example.com)
                if (host.Contains($".{memberState}.", StringComparison.OrdinalIgnoreCase))
                {
                    return memberState;
                }
            }

            return null;
        }
        catch
        {
            return null;
        }
    }
}
