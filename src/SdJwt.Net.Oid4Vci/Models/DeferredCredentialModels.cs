using System.Text.Json.Serialization;

namespace SdJwt.Net.Oid4Vci.Models;

/// <summary>
/// Represents a Deferred Credential Request according to OID4VCI 1.0 Section 9.
/// This is sent to retrieve a credential that was issued asynchronously.
/// </summary>
public class DeferredCredentialRequest
{
    /// <summary>
    /// Gets or sets the acceptance token.
    /// REQUIRED. The acceptance token received in a previous credential response.
    /// </summary>
    [JsonPropertyName("acceptance_token")]
    public string AcceptanceToken { get; set; } = string.Empty;
}

/// <summary>
/// Represents a Deferred Credential Response according to OID4VCI 1.0 Section 9.
/// This is the response when retrieving a deferred credential.
/// </summary>
public class DeferredCredentialResponse
{
    /// <summary>
    /// Gets or sets the issued credential.
    /// CONDITIONAL. The issued Credential. Present when the credential is ready.
    /// </summary>
    [JsonPropertyName("credential")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public string? Credential
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the issuance pending flag.
    /// CONDITIONAL. Present when the credential is not yet ready.
    /// </summary>
    [JsonPropertyName("issuance_pending")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public bool? IssuancePending
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the retry interval in seconds.
    /// OPTIONAL. Number of seconds to wait before making the next request.
    /// </summary>
    [JsonPropertyName("interval")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public int? Interval
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the credential nonce.
    /// OPTIONAL. A fresh nonce for the next credential request.
    /// </summary>
    [JsonPropertyName("c_nonce")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public string? CNonce
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the credential nonce expiration time.
    /// OPTIONAL. Lifetime of the c_nonce in seconds.
    /// </summary>
    [JsonPropertyName("c_nonce_expires_in")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public int? CNonceExpiresIn
    {
        get; set;
    }

    /// <summary>
    /// Creates a successful deferred credential response.
    /// </summary>
    /// <param name="credential">The issued credential</param>
    /// <param name="cNonce">Optional new nonce</param>
    /// <param name="cNonceExpiresIn">Optional nonce expiration</param>
    /// <returns>A successful DeferredCredentialResponse</returns>
    public static DeferredCredentialResponse Success(string credential, string? cNonce = null, int? cNonceExpiresIn = null)
    {
#if NET6_0_OR_GREATER
        ArgumentException.ThrowIfNullOrWhiteSpace(credential);
#else
        if (string.IsNullOrWhiteSpace(credential))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(credential));
#endif

        return new DeferredCredentialResponse
        {
            Credential = credential,
            CNonce = cNonce,
            CNonceExpiresIn = cNonceExpiresIn
        };
    }

    /// <summary>
    /// Creates a pending deferred credential response.
    /// </summary>
    /// <param name="interval">Optional retry interval in seconds</param>
    /// <param name="cNonce">Optional new nonce</param>
    /// <param name="cNonceExpiresIn">Optional nonce expiration</param>
    /// <returns>A pending DeferredCredentialResponse</returns>
    public static DeferredCredentialResponse Pending(int? interval = null, string? cNonce = null, int? cNonceExpiresIn = null)
    {
        return new DeferredCredentialResponse
        {
            IssuancePending = true,
            Interval = interval,
            CNonce = cNonce,
            CNonceExpiresIn = cNonceExpiresIn
        };
    }
}
