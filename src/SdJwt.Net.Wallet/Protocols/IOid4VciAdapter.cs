using SdJwt.Net.Wallet.Core;

namespace SdJwt.Net.Wallet.Protocols;

/// <summary>
/// Adapter interface for OpenID4VCI credential issuance protocol.
/// </summary>
public interface IOid4VciAdapter
{
    /// <summary>
    /// Parses a credential offer URI or JSON.
    /// </summary>
    /// <param name="offer">The credential offer (URI or JSON string).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Parsed credential offer information.</returns>
    Task<CredentialOfferInfo> ParseOfferAsync(
        string offer,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Resolves issuer metadata.
    /// </summary>
    /// <param name="issuer">The issuer identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Issuer metadata as dictionary.</returns>
    Task<IDictionary<string, object>> ResolveIssuerMetadataAsync(
        string issuer,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Exchanges a grant for an access token.
    /// </summary>
    /// <param name="tokenEndpoint">The token endpoint URL.</param>
    /// <param name="options">Token exchange options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Token exchange result.</returns>
    Task<TokenResult> ExchangeTokenAsync(
        string tokenEndpoint,
        TokenExchangeOptions options,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Requests a credential from the issuer.
    /// </summary>
    /// <param name="credentialEndpoint">The credential endpoint URL.</param>
    /// <param name="options">Credential request options.</param>
    /// <param name="keyManager">Key manager for proof creation.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Issuance result.</returns>
    Task<IssuanceResult> RequestCredentialAsync(
        string credentialEndpoint,
        CredentialRequestOptions options,
        IKeyManager keyManager,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Polls for a deferred credential.
    /// </summary>
    /// <param name="deferredEndpoint">The deferred credential endpoint.</param>
    /// <param name="transactionId">The transaction ID.</param>
    /// <param name="accessToken">The access token.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Issuance result.</returns>
    Task<IssuanceResult> PollDeferredCredentialAsync(
        string deferredEndpoint,
        string transactionId,
        string accessToken,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Starts the authorization code flow for credential issuance.
    /// </summary>
    /// <param name="authorizationEndpoint">The authorization endpoint URL.</param>
    /// <param name="clientId">The client ID.</param>
    /// <param name="redirectUri">The redirect URI.</param>
    /// <param name="scope">The requested scope.</param>
    /// <param name="authorizationDetails">Authorization details for specific credentials.</param>
    /// <param name="state">State for CSRF protection.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The authorization URL to redirect to.</returns>
    Task<string> BuildAuthorizationUrlAsync(
        string authorizationEndpoint,
        string clientId,
        string redirectUri,
        string? scope = null,
        string? authorizationDetails = null,
        string? state = null,
        CancellationToken cancellationToken = default);
}
