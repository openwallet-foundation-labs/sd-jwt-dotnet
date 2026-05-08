using System.Text.Json.Serialization;

namespace SdJwt.Net.Oid4Vci.Models;

/// <summary>
/// Represents a Deferred Credential Request according to OID4VCI 1.0 Section 9.
/// Sent to the Deferred Credential Endpoint to retrieve a previously deferred credential.
/// </summary>
public class DeferredCredentialRequest
{
    /// <summary>
    /// Gets or sets the transaction identifier.
    /// REQUIRED. The <c>transaction_id</c> received in the original <see cref="CredentialResponse"/>.
    /// </summary>
    [JsonPropertyName("transaction_id")]
    public string TransactionId { get; set; } = string.Empty;
}

/// <summary>
/// Represents a Deferred Credential Response according to OID4VCI 1.0 Section 9.
/// Either contains a <see cref="Credentials"/> array (credential ready) or a
/// <see cref="TransactionId"/> with optional <see cref="Interval"/> (still pending).
/// </summary>
public class DeferredCredentialResponse
{
    /// <summary>
    /// Gets or sets the issued credentials.
    /// CONDITIONAL. Present when the credential is ready (HTTP 200).
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
    /// Gets or sets the transaction identifier.
    /// CONDITIONAL. Present when the credential is not yet ready (HTTP 202).
    /// The wallet should use this value in the next Deferred Credential Request.
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
    /// Gets or sets the retry interval in seconds.
    /// OPTIONAL. Number of seconds the wallet should wait before making the next Deferred Credential Request.
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
    /// Gets or sets the notification identifier.
    /// OPTIONAL. Present when the credential is ready.
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
    /// Creates a successful deferred credential response when the credential is ready.
    /// </summary>
    /// <param name="credential">The issued credential value.</param>
    /// <param name="notificationId">Optional notification identifier.</param>
    /// <returns>A successful <see cref="DeferredCredentialResponse"/>.</returns>
    public static DeferredCredentialResponse Success(object credential, string? notificationId = null)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(credential);
#else
        if (credential == null)
            throw new ArgumentNullException(nameof(credential));
#endif

        return new DeferredCredentialResponse
        {
            Credentials = new[] { new CredentialResponseItem { Credential = credential } },
            NotificationId = notificationId
        };
    }

    /// <summary>
    /// Creates a still-pending deferred credential response.
    /// </summary>
    /// <param name="transactionId">The transaction identifier for the next retry.</param>
    /// <param name="interval">Optional retry interval in seconds.</param>
    /// <returns>A pending <see cref="DeferredCredentialResponse"/>.</returns>
    public static DeferredCredentialResponse Pending(string transactionId, int? interval = null)
    {
#if NET6_0_OR_GREATER
        ArgumentException.ThrowIfNullOrWhiteSpace(transactionId);
#else
        if (string.IsNullOrWhiteSpace(transactionId))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(transactionId));
#endif

        return new DeferredCredentialResponse
        {
            TransactionId = transactionId,
            Interval = interval
        };
    }
}
