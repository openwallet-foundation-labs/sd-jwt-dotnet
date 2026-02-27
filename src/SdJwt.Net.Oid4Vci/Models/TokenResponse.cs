using System.Text.Json.Serialization;

namespace SdJwt.Net.Oid4Vci.Models;

/// <summary>
/// Represents a Token Response according to OID4VCI 1.0 Section 6.2.
/// This is returned from the token endpoint.
/// </summary>
public class TokenResponse
{
    /// <summary>
    /// Gets or sets the access token.
    /// REQUIRED. The access token issued by the authorization server.
    /// </summary>
    [JsonPropertyName("access_token")]
    public string? AccessToken
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the token type.
    /// REQUIRED. The type of the token issued. Value is case insensitive. 
    /// For Bearer tokens, this value SHOULD be "Bearer".
    /// </summary>
    [JsonPropertyName("token_type")]
    public string? TokenType
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the lifetime in seconds of the access token.
    /// RECOMMENDED. The lifetime in seconds of the access token.
    /// </summary>
    [JsonPropertyName("expires_in")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public int? ExpiresIn
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the refresh token.
    /// OPTIONAL. The refresh token, which can be used to obtain new access tokens.
    /// </summary>
    [JsonPropertyName("refresh_token")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public string? RefreshToken
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the scope.
    /// OPTIONAL. The scope of the access token.
    /// </summary>
    [JsonPropertyName("scope")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public string? Scope
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the credential nonce.
    /// CONDITIONAL. Contains a nonce to be used to create a proof of possession of key material 
    /// when requesting a Credential.
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
    /// CONDITIONAL. Lifetime in seconds of the c_nonce.
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
    /// Gets or sets the authorization pending flag.
    /// OPTIONAL. Indicates if authorization is still pending.
    /// </summary>
    [JsonPropertyName("authorization_pending")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public bool? AuthorizationPending
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the polling interval.
    /// OPTIONAL. The minimum amount of time in seconds the client should wait between polling requests.
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
    /// Gets or sets the authorization details.
    /// OPTIONAL. Authorization details as defined in RFC 9396.
    /// </summary>
    [JsonPropertyName("authorization_details")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public object[]? AuthorizationDetails
    {
        get; set;
    }

    /// <summary>
    /// Validates the token response according to OID4VCI 1.0 requirements.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the response is invalid</exception>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(AccessToken))
            throw new InvalidOperationException("AccessToken is required");

        if (string.IsNullOrWhiteSpace(TokenType))
            throw new InvalidOperationException("TokenType is required");

        if (ExpiresIn.HasValue && ExpiresIn.Value < 0)
            throw new InvalidOperationException("ExpiresIn must be non-negative");

        if (CNonceExpiresIn.HasValue && CNonceExpiresIn.Value < 0)
            throw new InvalidOperationException("CNonceExpiresIn must be non-negative");
    }

    /// <summary>
    /// Creates a successful token response.
    /// </summary>
    /// <param name="accessToken">The access token</param>
    /// <param name="expiresIn">Optional token expiration time in seconds</param>
    /// <param name="cNonce">Optional credential nonce</param>
    /// <param name="cNonceExpiresIn">Optional credential nonce expiration time in seconds</param>
    /// <returns>A new TokenResponse instance</returns>
    public static TokenResponse Success(string accessToken, int? expiresIn = null, string? cNonce = null, int? cNonceExpiresIn = null)
    {
#if NET6_0_OR_GREATER
        ArgumentException.ThrowIfNullOrWhiteSpace(accessToken);
#else
        if (string.IsNullOrWhiteSpace(accessToken))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(accessToken));
#endif

        return new TokenResponse
        {
            AccessToken = accessToken,
            TokenType = "Bearer",
            ExpiresIn = expiresIn,
            CNonce = cNonce,
            CNonceExpiresIn = cNonceExpiresIn
        };
    }
}
