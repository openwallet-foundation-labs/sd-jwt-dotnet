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
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the token type.
    /// REQUIRED. The type of the token issued. Value is case insensitive. 
    /// For Bearer tokens, this value SHOULD be "Bearer".
    /// </summary>
    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = "Bearer";

    /// <summary>
    /// Gets or sets the lifetime in seconds of the access token.
    /// RECOMMENDED. The lifetime in seconds of the access token.
    /// </summary>
    [JsonPropertyName("expires_in")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public int? ExpiresIn { get; set; }

    /// <summary>
    /// Gets or sets the refresh token.
    /// OPTIONAL. The refresh token, which can be used to obtain new access tokens.
    /// </summary>
    [JsonPropertyName("refresh_token")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public string? RefreshToken { get; set; }

    /// <summary>
    /// Gets or sets the scope.
    /// OPTIONAL. The scope of the access token.
    /// </summary>
    [JsonPropertyName("scope")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public string? Scope { get; set; }

    /// <summary>
    /// Gets or sets the credential nonce.
    /// CONDITIONAL. Contains a nonce to be used to create a proof of possession of key material 
    /// when requesting a Credential.
    /// </summary>
    [JsonPropertyName("c_nonce")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public string? CNonce { get; set; }

    /// <summary>
    /// Gets or sets the credential nonce expiration time.
    /// CONDITIONAL. Lifetime in seconds of the c_nonce.
    /// </summary>
    [JsonPropertyName("c_nonce_expires_in")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public int? CNonceExpiresIn { get; set; }

    /// <summary>
    /// Gets or sets the authorization details.
    /// OPTIONAL. Authorization details as defined in RFC 9396.
    /// </summary>
    [JsonPropertyName("authorization_details")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public object[]? AuthorizationDetails { get; set; }
}