using SdJwt.Net.Wallet.Core;

namespace SdJwt.Net.Wallet.Protocols;

/// <summary>
/// Represents the result of credential issuance.
/// </summary>
public class IssuanceResult
{
    /// <summary>
    /// Whether the issuance was successful.
    /// </summary>
    public bool IsSuccessful
    {
        get; set;
    }

    /// <summary>
    /// The issued credentials (may be deferred).
    /// </summary>
    public IReadOnlyList<StoredCredential> Credentials { get; set; } = [];

    /// <summary>
    /// Transaction ID for deferred issuance.
    /// </summary>
    public string? TransactionId
    {
        get; set;
    }

    /// <summary>
    /// Deferred credential endpoint for polling.
    /// </summary>
    public string? DeferredEndpoint
    {
        get; set;
    }

    /// <summary>
    /// Error code if unsuccessful.
    /// </summary>
    public string? ErrorCode
    {
        get; set;
    }

    /// <summary>
    /// Error description if unsuccessful.
    /// </summary>
    public string? ErrorDescription
    {
        get; set;
    }

    /// <summary>
    /// C_Nonce from the issuer for subsequent requests.
    /// </summary>
    public string? CNonce
    {
        get; set;
    }

    /// <summary>
    /// C_Nonce expiration time in seconds.
    /// </summary>
    public int? CNonceExpiresIn
    {
        get; set;
    }
}

/// <summary>
/// Represents a parsed credential offer.
/// </summary>
public class CredentialOfferInfo
{
    /// <summary>
    /// The credential issuer identifier.
    /// </summary>
    public string CredentialIssuer { get; set; } = string.Empty;

    /// <summary>
    /// Credential configuration IDs offered.
    /// </summary>
    public IReadOnlyList<string> CredentialConfigurationIds { get; set; } = [];

    /// <summary>
    /// Pre-authorized code grant if available.
    /// </summary>
    public PreAuthorizedCodeGrant? PreAuthorizedCode
    {
        get; set;
    }

    /// <summary>
    /// Authorization code grant if available.
    /// </summary>
    public AuthorizationCodeGrant? AuthorizationCode
    {
        get; set;
    }
}

/// <summary>
/// Pre-authorized code grant details.
/// </summary>
public class PreAuthorizedCodeGrant
{
    /// <summary>
    /// The pre-authorized code.
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Whether TX code (PIN) is required.
    /// </summary>
    public bool TxCodeRequired
    {
        get; set;
    }

    /// <summary>
    /// TX code input mode hint.
    /// </summary>
    public string? TxCodeInputMode
    {
        get; set;
    }

    /// <summary>
    /// TX code length hint.
    /// </summary>
    public int? TxCodeLength
    {
        get; set;
    }

    /// <summary>
    /// Description for TX code.
    /// </summary>
    public string? TxCodeDescription
    {
        get; set;
    }
}

/// <summary>
/// Authorization code grant details.
/// </summary>
public class AuthorizationCodeGrant
{
    /// <summary>
    /// Issuer state for authorization.
    /// </summary>
    public string? IssuerState
    {
        get; set;
    }

    /// <summary>
    /// Authorization server identifier.
    /// </summary>
    public string? AuthorizationServer
    {
        get; set;
    }
}

/// <summary>
/// Options for credential request.
/// </summary>
public class CredentialRequestOptions
{
    /// <summary>
    /// The access token for the request.
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Credential configuration ID to request.
    /// </summary>
    public string CredentialConfigurationId { get; set; } = string.Empty;

    /// <summary>
    /// Key ID to use for proof.
    /// </summary>
    public string KeyId { get; set; } = string.Empty;

    /// <summary>
    /// C_Nonce from the issuer.
    /// </summary>
    public string? CNonce
    {
        get; set;
    }

    /// <summary>
    /// Issuer identifier for proof audience.
    /// </summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>
    /// Proof type (jwt, cwt, ldp_vp).
    /// </summary>
    public string ProofType { get; set; } = "jwt";
}

/// <summary>
/// Options for token exchange.
/// </summary>
public class TokenExchangeOptions
{
    /// <summary>
    /// Pre-authorized code for the exchange.
    /// </summary>
    public string? PreAuthorizedCode
    {
        get; set;
    }

    /// <summary>
    /// TX code (PIN) if required.
    /// </summary>
    public string? TxCode
    {
        get; set;
    }

    /// <summary>
    /// Authorization code for the exchange.
    /// </summary>
    public string? AuthorizationCode
    {
        get; set;
    }

    /// <summary>
    /// Code verifier for PKCE.
    /// </summary>
    public string? CodeVerifier
    {
        get; set;
    }

    /// <summary>
    /// Redirect URI used in authorization.
    /// </summary>
    public string? RedirectUri
    {
        get; set;
    }
}

/// <summary>
/// Result of token exchange.
/// </summary>
public class TokenResult
{
    /// <summary>
    /// Whether the exchange was successful.
    /// </summary>
    public bool IsSuccessful
    {
        get; set;
    }

    /// <summary>
    /// The access token.
    /// </summary>
    public string? AccessToken
    {
        get; set;
    }

    /// <summary>
    /// Token type (usually "Bearer").
    /// </summary>
    public string? TokenType
    {
        get; set;
    }

    /// <summary>
    /// Expiration in seconds.
    /// </summary>
    public int? ExpiresIn
    {
        get; set;
    }

    /// <summary>
    /// Refresh token if provided.
    /// </summary>
    public string? RefreshToken
    {
        get; set;
    }

    /// <summary>
    /// C_Nonce for credential requests.
    /// </summary>
    public string? CNonce
    {
        get; set;
    }

    /// <summary>
    /// C_Nonce expiration in seconds.
    /// </summary>
    public int? CNonceExpiresIn
    {
        get; set;
    }

    /// <summary>
    /// Authorization details.
    /// </summary>
    public string? AuthorizationDetails
    {
        get; set;
    }

    /// <summary>
    /// Error code if unsuccessful.
    /// </summary>
    public string? ErrorCode
    {
        get; set;
    }

    /// <summary>
    /// Error description.
    /// </summary>
    public string? ErrorDescription
    {
        get; set;
    }
}

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
