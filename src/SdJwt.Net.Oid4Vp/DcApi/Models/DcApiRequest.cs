using System.Text.Json;
using System.Text.Json.Serialization;
using SdJwt.Net.Oid4Vp.Models;

namespace SdJwt.Net.Oid4Vp.DcApi.Models;

/// <summary>
/// Represents a Digital Credentials API request compatible with navigator.credentials.get().
/// </summary>
public class DcApiRequest
{
    /// <summary>
    /// Protocol identifier. Must be "openid4vp".
    /// </summary>
    [JsonPropertyName("protocol")]
    public string Protocol { get; set; } = DcApiConstants.Protocol;

    /// <summary>
    /// The OpenID4VP authorization request object.
    /// </summary>
    [JsonPropertyName("request")]
    public DcApiAuthorizationRequest Request { get; set; } = new();

    /// <summary>
    /// Converts the request to a JSON structure for navigator.credentials.get().
    /// </summary>
    /// <returns>JSON string suitable for DC API invocation.</returns>
    public string ToNavigatorCredentialsPayload()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = false
        };

        return JsonSerializer.Serialize(this, options);
    }
}

/// <summary>
/// OpenID4VP authorization request for DC API.
/// </summary>
public class DcApiAuthorizationRequest
{
    /// <summary>
    /// Client identifier (typically the verifier's origin URL).
    /// </summary>
    [JsonPropertyName("client_id")]
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// Client ID scheme. Defaults to "web-origin" for DC API.
    /// </summary>
    [JsonPropertyName("client_id_scheme")]
    public string ClientIdScheme { get; set; } = DcApiConstants.WebOriginScheme;

    /// <summary>
    /// Response type. Always "vp_token" for OpenID4VP.
    /// </summary>
    [JsonPropertyName("response_type")]
    public string ResponseType { get; set; } = "vp_token";

    /// <summary>
    /// Response mode for DC API ("dc_api" or "dc_api.jwt").
    /// </summary>
    [JsonPropertyName("response_mode")]
    public string ResponseMode { get; set; } = DcApiConstants.ResponseModes.DcApi;

    /// <summary>
    /// Nonce for replay protection.
    /// </summary>
    [JsonPropertyName("nonce")]
    public string Nonce { get; set; } = string.Empty;

    /// <summary>
    /// Presentation definition describing required credentials.
    /// </summary>
    [JsonPropertyName("presentation_definition")]
    public PresentationDefinition? PresentationDefinition
    {
        get; set;
    }
}

/// <summary>
/// Digital credential provider configuration for navigator.credentials.get().
/// </summary>
public class DigitalCredentialProvider
{
    /// <summary>
    /// Protocol identifier.
    /// </summary>
    [JsonPropertyName("protocol")]
    public string Protocol { get; set; } = DcApiConstants.Protocol;

    /// <summary>
    /// The raw request object to send to the wallet.
    /// </summary>
    [JsonPropertyName("request")]
    public object Request { get; set; } = new();
}
