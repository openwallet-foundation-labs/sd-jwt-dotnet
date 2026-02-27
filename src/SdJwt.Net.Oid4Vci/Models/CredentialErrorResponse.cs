using System.Text.Json.Serialization;

namespace SdJwt.Net.Oid4Vci.Models;

/// <summary>
/// Represents an error response for credential requests according to OID4VCI 1.0 Section 7.3.1.
/// </summary>
public class CredentialErrorResponse
{
    /// <summary>
    /// Gets or sets the error code.
    /// REQUIRED. Error code as defined in the OID4VCI specification.
    /// </summary>
    [JsonPropertyName("error")]
    public string Error { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the error description.
    /// OPTIONAL. Human-readable ASCII text providing additional information.
    /// </summary>
    [JsonPropertyName("error_description")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public string? ErrorDescription
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the error URI.
    /// OPTIONAL. A URI identifying a human-readable web page with information about the error.
    /// </summary>
    [JsonPropertyName("error_uri")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public string? ErrorUri
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the credential nonce for retry attempts.
    /// OPTIONAL. A fresh nonce for retry attempts.
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
    /// Creates an error response with the specified error code.
    /// </summary>
    /// <param name="error">The error code</param>
    /// <param name="description">Optional error description</param>
    /// <param name="errorUri">Optional error URI</param>
    /// <param name="cNonce">Optional new nonce for retry</param>
    /// <param name="cNonceExpiresIn">Optional nonce expiration time</param>
    /// <returns>A new CredentialErrorResponse instance</returns>
    public static CredentialErrorResponse Create(string error, string? description = null, string? errorUri = null, string? cNonce = null, int? cNonceExpiresIn = null)
    {
#if NET6_0_OR_GREATER
        ArgumentException.ThrowIfNullOrWhiteSpace(error);
#else
        if (string.IsNullOrWhiteSpace(error))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(error));
#endif

        return new CredentialErrorResponse
        {
            Error = error,
            ErrorDescription = description,
            ErrorUri = errorUri,
            CNonce = cNonce,
            CNonceExpiresIn = cNonceExpiresIn
        };
    }
}
