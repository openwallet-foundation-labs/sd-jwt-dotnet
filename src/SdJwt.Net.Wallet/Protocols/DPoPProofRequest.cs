namespace SdJwt.Net.Wallet.Protocols;

/// <summary>
/// DPoP proof request input.
/// </summary>
public class DPoPProofRequest
{
    /// <summary>
    /// HTTP method (htm claim), for example POST.
    /// </summary>
    public string HttpMethod { get; set; } = "POST";

    /// <summary>
    /// HTTP target URI (htu claim).
    /// </summary>
    public string HttpUri { get; set; } = string.Empty;

    /// <summary>
    /// Optional nonce received from authorization server.
    /// </summary>
    public string? Nonce
    {
        get; set;
    }
}
