namespace SdJwt.Net.Wallet.Protocols;

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
