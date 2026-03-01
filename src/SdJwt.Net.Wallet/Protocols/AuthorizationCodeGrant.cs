namespace SdJwt.Net.Wallet.Protocols;

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
