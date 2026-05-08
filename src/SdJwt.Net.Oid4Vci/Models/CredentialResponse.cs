using System.Text.Json.Serialization;

namespace SdJwt.Net.Oid4Vci.Models;

/// <summary>
/// Represents a Credential Response according to OID4VCI 1.0 Section 8.3.
/// Contains either a <see cref="Credentials"/> array (immediate issuance) or a
/// <see cref="TransactionId"/> (deferred issuance).
/// </summary>
public class CredentialResponse
{
    /// <summary>
    /// Gets or sets the issued credentials.
    /// CONDITIONAL. Array of issued credentials. Present when issuance is immediate.
    /// Each element contains one issued credential. The array may be smaller than the
    /// number of proofs submitted (partial batch issuance).
    /// </summary>
    [JsonPropertyName("credentials")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public CredentialResponseItem[]? Credentials
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the transaction identifier for deferred credential issuance.
    /// CONDITIONAL. Present when issuance is deferred (HTTP 202). The wallet must use this
    /// value to poll the Deferred Credential Endpoint.
    /// </summary>
    [JsonPropertyName("transaction_id")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public string? TransactionId
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the notification identifier.
    /// OPTIONAL. Opaque string the wallet must send to the Notification Endpoint when the
    /// credential lifecycle changes (accepted, deleted, failed).
    /// </summary>
    [JsonPropertyName("notification_id")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public string? NotificationId
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the credential nonce.
    /// OPTIONAL. Present for backward compatibility with pre-final spec drafts.
    /// In OID4VCI 1.0 final the nonce is obtained from the dedicated Nonce Endpoint.
    /// </summary>
    [JsonPropertyName("c_nonce")]
    [Obsolete("c_nonce in Credential Response is not used in OID4VCI 1.0 final. Use the Nonce Endpoint instead.")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public string? CNonce
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the credential nonce expiration time in seconds.
    /// OPTIONAL. Backward-compatibility field. See <see cref="CNonce"/>.
    /// </summary>
    [JsonPropertyName("c_nonce_expires_in")]
    [Obsolete("c_nonce_expires_in in Credential Response is not used in OID4VCI 1.0 final. Use the Nonce Endpoint instead.")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public int? CNonceExpiresIn
    {
        get; set;
    }

    /// <summary>
    /// Validates the credential response according to OID4VCI 1.0 Section 8.3.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the response is invalid.</exception>
    public void Validate()
    {
        var hasCredentials = Credentials != null && Credentials.Length > 0;
        var hasTransactionId = !string.IsNullOrWhiteSpace(TransactionId);

        if (!hasCredentials && !hasTransactionId)
            throw new InvalidOperationException("Either credentials or transaction_id must be present.");

        if (hasCredentials && hasTransactionId)
            throw new InvalidOperationException("Cannot have both credentials and transaction_id.");
    }

    /// <summary>
    /// Creates a successful immediate credential response with one or more issued credentials.
    /// </summary>
    /// <param name="credential">The first issued credential value (string for JWT/SD-JWT; object for ldp_vc).</param>
    /// <param name="notificationId">Optional notification identifier.</param>
    /// <returns>A new <see cref="CredentialResponse"/> instance.</returns>
    public static CredentialResponse Success(object credential, string? notificationId = null)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(credential);
#else
        if (credential == null)
            throw new ArgumentNullException(nameof(credential));
#endif

        return new CredentialResponse
        {
            Credentials = new[] { new CredentialResponseItem { Credential = credential } },
            NotificationId = notificationId
        };
    }

    /// <summary>
    /// Creates a successful immediate credential response with multiple issued credentials.
    /// </summary>
    /// <param name="credentials">The issued credential values.</param>
    /// <param name="notificationId">Optional notification identifier.</param>
    /// <returns>A new <see cref="CredentialResponse"/> instance.</returns>
    public static CredentialResponse Success(object[] credentials, string? notificationId = null)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(credentials);
#else
        if (credentials == null)
            throw new ArgumentNullException(nameof(credentials));
#endif

        if (credentials.Length == 0)
            throw new ArgumentException("At least one credential is required.", nameof(credentials));

        return new CredentialResponse
        {
            Credentials = credentials.Select(c => new CredentialResponseItem { Credential = c }).ToArray(),
            NotificationId = notificationId
        };
    }

    /// <summary>
    /// Creates a deferred credential response with a transaction identifier.
    /// </summary>
    /// <param name="transactionId">The transaction identifier for later retrieval via the Deferred Credential Endpoint.</param>
    /// <param name="notificationId">Optional notification identifier.</param>
    /// <returns>A new <see cref="CredentialResponse"/> instance for deferred issuance.</returns>
    public static CredentialResponse Deferred(string transactionId, string? notificationId = null)
    {
#if NET6_0_OR_GREATER
        ArgumentException.ThrowIfNullOrWhiteSpace(transactionId);
#else
        if (string.IsNullOrWhiteSpace(transactionId))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(transactionId));
#endif

        return new CredentialResponse
        {
            TransactionId = transactionId,
            NotificationId = notificationId
        };
    }
}
