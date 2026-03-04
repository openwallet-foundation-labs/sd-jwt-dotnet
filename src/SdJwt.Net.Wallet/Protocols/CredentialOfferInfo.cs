namespace SdJwt.Net.Wallet.Protocols;

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
