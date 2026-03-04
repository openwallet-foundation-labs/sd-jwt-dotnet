namespace SdJwt.Net.Wallet.Issuance;

/// <summary>
/// Per-issuer wallet configuration for OpenID4VCI flows.
/// </summary>
public class WalletIssuerConfiguration
{
    /// <summary>
    /// Logical issuer name used by the wallet.
    /// </summary>
    public string IssuerName { get; set; } = string.Empty;

    /// <summary>
    /// Credential issuer identifier/URL.
    /// </summary>
    public string CredentialIssuer { get; set; } = string.Empty;

    /// <summary>
    /// Optional OAuth client ID for this issuer.
    /// </summary>
    public string? ClientId
    {
        get; set;
    }

    /// <summary>
    /// Optional redirect URI for authorization code flow.
    /// </summary>
    public string? RedirectUri
    {
        get; set;
    }

    /// <summary>
    /// Optional additional metadata for issuer-specific behavior.
    /// </summary>
    public IDictionary<string, object>? Metadata
    {
        get; set;
    }
}
