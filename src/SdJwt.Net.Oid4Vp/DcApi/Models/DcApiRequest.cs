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
    /// Digital credential request options.
    /// </summary>
    [JsonPropertyName("digital")]
    public DigitalCredentialRequestOptions Digital { get; set; } = new();

    /// <summary>
    /// Gets the first protocol identifier in the request.
    /// </summary>
    [JsonIgnore]
    public string Protocol => Digital.Requests.FirstOrDefault()?.Protocol ?? string.Empty;

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
/// Options for requesting digital credentials through <c>navigator.credentials.get()</c>.
/// </summary>
public class DigitalCredentialRequestOptions
{
    /// <summary>
    /// Gets or sets the credential requests offered to the user agent.
    /// </summary>
    [JsonPropertyName("requests")]
    public DigitalCredentialGetRequest[] Requests { get; set; } = Array.Empty<DigitalCredentialGetRequest>();
}

/// <summary>
/// A single Digital Credentials API presentation request.
/// </summary>
public class DigitalCredentialGetRequest
{
    /// <summary>
    /// Gets or sets the presentation protocol identifier.
    /// </summary>
    [JsonPropertyName("protocol")]
    public string Protocol { get; set; } = DcApiConstants.Protocols.OpenId4VpV1Unsigned;

    /// <summary>
    /// Gets or sets protocol-specific presentation request data.
    /// </summary>
    [JsonPropertyName("data")]
    public object Data { get; set; } = new();
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
    public string? ClientId
    {
        get; set;
    }

    /// <summary>
    /// Client ID scheme for signed DC API requests.
    /// </summary>
    [JsonPropertyName("client_id_scheme")]
    public string? ClientIdScheme
    {
        get; set;
    }

    /// <summary>
    /// Expected verifier origins for signed DC API requests.
    /// </summary>
    [JsonPropertyName("expected_origins")]
    public string[]? ExpectedOrigins
    {
        get; set;
    }

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
/// Digital credential request configuration for navigator.credentials.get().
/// </summary>
public class DigitalCredentialProvider : DigitalCredentialGetRequest
{
    /// <summary>
    /// Gets or sets the protocol-specific request object to send to the wallet.
    /// </summary>
    [JsonPropertyName("request")]
    [Obsolete("Use Data. The current Digital Credentials API uses the 'data' member.")]
    public object Request
    {
        get => Data;
        set => Data = value;
    }
}
