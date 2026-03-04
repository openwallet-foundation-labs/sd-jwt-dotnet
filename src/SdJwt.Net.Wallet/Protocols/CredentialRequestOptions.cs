namespace SdJwt.Net.Wallet.Protocols;

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
