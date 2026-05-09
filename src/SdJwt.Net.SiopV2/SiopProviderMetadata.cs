using System.Text.Json.Serialization;

namespace SdJwt.Net.SiopV2;

/// <summary>
/// Self-Issued OP metadata values.
/// </summary>
public class SiopProviderMetadata
{
    /// <summary>
    /// Gets or sets the authorization endpoint.
    /// </summary>
    [JsonPropertyName("authorization_endpoint")]
    public string AuthorizationEndpoint { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets supported response types.
    /// </summary>
    [JsonPropertyName("response_types_supported")]
    public string[] ResponseTypesSupported { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets supported scopes.
    /// </summary>
    [JsonPropertyName("scopes_supported")]
    public string[] ScopesSupported { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets supported subject types.
    /// </summary>
    [JsonPropertyName("subject_types_supported")]
    public string[] SubjectTypesSupported { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets supported ID Token signing algorithms.
    /// </summary>
    [JsonPropertyName("id_token_signing_alg_values_supported")]
    public string[] IdTokenSigningAlgValuesSupported { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets supported request object signing algorithms.
    /// </summary>
    [JsonPropertyName("request_object_signing_alg_values_supported")]
    public string[] RequestObjectSigningAlgValuesSupported { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets supported subject syntax types.
    /// </summary>
    [JsonPropertyName("subject_syntax_types_supported")]
    public string[] SubjectSyntaxTypesSupported { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets supported ID Token types.
    /// </summary>
    [JsonPropertyName("id_token_types_supported")]
    public string[] IdTokenTypesSupported { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Gets or sets supported VP formats.
    /// </summary>
    [JsonPropertyName("vp_formats_supported")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public Dictionary<string, object>? VpFormatsSupported
    {
        get; set;
    }

    /// <summary>
    /// Creates static SIOPv2 metadata for the <c>siopv2:</c> authorization endpoint.
    /// </summary>
    /// <returns>The metadata instance.</returns>
    public static SiopProviderMetadata CreateSiopV2Defaults()
    {
        return new SiopProviderMetadata
        {
            AuthorizationEndpoint = "siopv2:",
            ResponseTypesSupported = new[] { SiopConstants.ResponseTypes.IdToken },
            ScopesSupported = new[] { SiopConstants.Scopes.OpenId },
            SubjectTypesSupported = new[] { "pairwise" },
            IdTokenSigningAlgValuesSupported = new[] { "ES256" },
            RequestObjectSigningAlgValuesSupported = new[] { "ES256" },
            SubjectSyntaxTypesSupported = new[] { SiopConstants.SubjectSyntaxTypes.JwkThumbprint },
            IdTokenTypesSupported = new[] { SiopConstants.IdTokenTypes.SubjectSigned }
        };
    }

    /// <summary>
    /// Creates static metadata for the <c>openid:</c> endpoint supporting SIOPv2 and OpenID4VP.
    /// </summary>
    /// <returns>The metadata instance.</returns>
    public static SiopProviderMetadata CreateOpenIdDefaults()
    {
        return new SiopProviderMetadata
        {
            AuthorizationEndpoint = "openid:",
            ResponseTypesSupported = new[] { SiopConstants.ResponseTypes.VpToken, SiopConstants.ResponseTypes.IdToken },
            ScopesSupported = new[] { SiopConstants.Scopes.OpenId },
            SubjectTypesSupported = new[] { "pairwise" },
            IdTokenSigningAlgValuesSupported = new[] { "ES256" },
            RequestObjectSigningAlgValuesSupported = new[] { "ES256" },
            SubjectSyntaxTypesSupported = new[] { SiopConstants.SubjectSyntaxTypes.JwkThumbprint },
            IdTokenTypesSupported = new[] { SiopConstants.IdTokenTypes.SubjectSigned },
            VpFormatsSupported = new Dictionary<string, object>
            {
                ["dc+sd-jwt"] = new Dictionary<string, object>
                {
                    ["sd-jwt_alg_values"] = new[] { "ES256" },
                    ["kb-jwt_alg_values"] = new[] { "ES256" }
                }
            }
        };
    }
}
