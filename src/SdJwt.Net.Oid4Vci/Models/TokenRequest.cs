using System.Text.Json.Serialization;

namespace SdJwt.Net.Oid4Vci.Models;

/// <summary>
/// Represents a Token Request according to OID4VCI 1.0 Section 6.1.
/// This is sent to the token endpoint to obtain an access token.
/// </summary>
public class TokenRequest
{
    /// <summary>
    /// Gets or sets the grant type.
    /// REQUIRED. Grant type value, one of the supported grant types.
    /// </summary>
    [JsonPropertyName("grant_type")]
    public string GrantType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the pre-authorized code.
    /// REQUIRED when using pre-authorized code grant. 
    /// The code representing the authorization to obtain Credentials.
    /// </summary>
    [JsonPropertyName("pre-authorized_code")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public string? PreAuthorizedCode { get; set; }

    /// <summary>
    /// Gets or sets the transaction code (PIN).
    /// CONDITIONAL. String value containing a Transaction Code. 
    /// Required if the Authorization Server request the Wallet to send it in the Access Token Request.
    /// </summary>
    [JsonPropertyName("tx_code")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public string? TransactionCode { get; set; }

    /// <summary>
    /// Gets or sets the client ID.
    /// OPTIONAL. Wallet identifier.
    /// </summary>
    [JsonPropertyName("client_id")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public string? ClientId { get; set; }

    /// <summary>
    /// Gets or sets the client assertion.
    /// CONDITIONAL. Client authentication when using client_assertion.
    /// </summary>
    [JsonPropertyName("client_assertion")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public string? ClientAssertion { get; set; }

    /// <summary>
    /// Gets or sets the client assertion type.
    /// CONDITIONAL. Must be "urn:ietf:params:oauth:client-assertion-type:jwt-bearer" when client_assertion is used.
    /// </summary>
    [JsonPropertyName("client_assertion_type")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public string? ClientAssertionType { get; set; }

    /// <summary>
    /// Gets or sets the authorization code.
    /// REQUIRED when using authorization code grant.
    /// </summary>
    [JsonPropertyName("code")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public string? Code { get; set; }

    /// <summary>
    /// Gets or sets the redirect URI.
    /// REQUIRED when using authorization code grant if included in authorization request.
    /// </summary>
    [JsonPropertyName("redirect_uri")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public string? RedirectUri { get; set; }

    /// <summary>
    /// Gets or sets the code verifier for PKCE.
    /// REQUIRED when using authorization code grant with PKCE.
    /// </summary>
    [JsonPropertyName("code_verifier")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public string? CodeVerifier { get; set; }
}