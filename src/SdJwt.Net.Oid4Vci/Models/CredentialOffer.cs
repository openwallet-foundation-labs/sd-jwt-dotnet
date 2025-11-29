using System.Text.Json.Serialization;

namespace SdJwt.Net.Oid4Vci.Models;

/// <summary>
/// Represents a Credential Offer according to OID4VCI 1.0 Section 4.1.1.
/// This is the JSON payload embedded in QR codes for credential issuance flows.
/// </summary>
public class CredentialOffer
{
    /// <summary>
    /// Gets or sets the credential issuer URL.
    /// REQUIRED. URL of the Credential Issuer, as defined in Section 11.2.1.
    /// </summary>
    [JsonPropertyName("credential_issuer")]
    public string CredentialIssuer { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the credential configuration identifiers.
    /// REQUIRED. Array of strings, each string being a Credential Configuration Identifier 
    /// as defined in Section 11.2.3.
    /// </summary>
    [JsonPropertyName("credential_configuration_ids")]
    public string[] CredentialConfigurationIds { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets the grants object containing grant-specific parameters.
    /// OPTIONAL. Object containing grant-specific parameters.
    /// </summary>
    [JsonPropertyName("grants")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public Dictionary<string, object>? Grants { get; set; }

    /// <summary>
    /// Adds a pre-authorized code grant to the grants object.
    /// </summary>
    /// <param name="preAuthorizedCode">The pre-authorized code</param>
    /// <param name="transactionCode">Optional transaction code configuration</param>
    /// <param name="interval">Optional polling interval in seconds</param>
    public void AddPreAuthorizedCodeGrant(string preAuthorizedCode, TransactionCode? transactionCode = null, int? interval = null)
    {
#if NET6_0_OR_GREATER
        ArgumentException.ThrowIfNullOrWhiteSpace(preAuthorizedCode);
#else
        if (string.IsNullOrWhiteSpace(preAuthorizedCode))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(preAuthorizedCode));
#endif

        Grants ??= new Dictionary<string, object>();

        var grant = new PreAuthorizedCodeGrant
        {
            PreAuthorizedCode = preAuthorizedCode,
            TransactionCode = transactionCode,
            Interval = interval
        };

        Grants[Oid4VciConstants.GrantTypes.PreAuthorizedCode] = grant;
    }

    /// <summary>
    /// Adds an authorization code grant to the grants object.
    /// </summary>
    /// <param name="issuerState">Optional issuer state parameter</param>
    /// <param name="authorizationServer">Optional authorization server endpoint</param>
    public void AddAuthorizationCodeGrant(string? issuerState = null, string? authorizationServer = null)
    {
        Grants ??= new Dictionary<string, object>();

        var grant = new AuthorizationCodeGrant
        {
            IssuerState = issuerState,
            AuthorizationServer = authorizationServer
        };

        Grants[Oid4VciConstants.GrantTypes.AuthorizationCode] = grant;
    }

    /// <summary>
    /// Gets the pre-authorized code grant if present.
    /// </summary>
    /// <returns>The pre-authorized code grant or null if not present</returns>
    public PreAuthorizedCodeGrant? GetPreAuthorizedCodeGrant()
    {
        if (Grants?.TryGetValue(Oid4VciConstants.GrantTypes.PreAuthorizedCode, out var grantObj) == true)
        {
            if (grantObj is PreAuthorizedCodeGrant grant)
                return grant;
                
            // Handle case where it might be deserialized as JsonElement
            if (grantObj is System.Text.Json.JsonElement element)
            {
                return System.Text.Json.JsonSerializer.Deserialize<PreAuthorizedCodeGrant>(element.GetRawText());
            }
        }

        return null;
    }

    /// <summary>
    /// Gets the authorization code grant if present.
    /// </summary>
    /// <returns>The authorization code grant or null if not present</returns>
    public AuthorizationCodeGrant? GetAuthorizationCodeGrant()
    {
        if (Grants?.TryGetValue(Oid4VciConstants.GrantTypes.AuthorizationCode, out var grantObj) == true)
        {
            if (grantObj is AuthorizationCodeGrant grant)
                return grant;
                
            // Handle case where it might be deserialized as JsonElement
            if (grantObj is System.Text.Json.JsonElement element)
            {
                return System.Text.Json.JsonSerializer.Deserialize<AuthorizationCodeGrant>(element.GetRawText());
            }
        }

        return null;
    }
}