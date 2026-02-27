using System.Text.Json.Serialization;

namespace SdJwt.Net.StatusList.Models;

/// <summary>
/// Represents a Status List Token payload as defined in draft-ietf-oauth-status-list-13.
/// This is the structure of the JWT that contains the actual status list.
/// </summary>
public class StatusListTokenPayload
{
    /// <summary>
    /// Gets or sets the subject of the Status List Token.
    /// Required. Must specify the URI of the Status List Token.
    /// The value must be equal to the uri claim contained in the status_list claim of the Referenced Token.
    /// </summary>
    [JsonPropertyName("sub")]
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the issued at time.
    /// Required. Unix timestamp when the Status List Token was issued.
    /// </summary>
    [JsonPropertyName("iat")]
    public long IssuedAt
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the expiration time.
    /// Recommended. Unix timestamp when the Status List Token expires.
    /// </summary>
    [JsonPropertyName("exp")]
    public long? ExpiresAt
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the time to live in seconds.
    /// Recommended. Maximum amount of time, in seconds, that the Status List Token 
    /// can be cached by a consumer before a fresh copy should be retrieved.
    /// Must be a positive number.
    /// </summary>
    [JsonPropertyName("ttl")]
    public int? TimeToLive
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the status list data.
    /// Required. Contains the Status List conforming to the structure defined in the specification.
    /// </summary>
    [JsonPropertyName("status_list")]
    public StatusList StatusList { get; set; } = new();
}
