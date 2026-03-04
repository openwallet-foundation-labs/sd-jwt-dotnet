namespace SdJwt.Net.Wallet.Protocols;

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

    /// <summary>
    /// Optional DPoP proof JWT to include with the token request.
    /// </summary>
    public string? DPoPProof
    {
        get; set;
    }
}
