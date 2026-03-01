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
