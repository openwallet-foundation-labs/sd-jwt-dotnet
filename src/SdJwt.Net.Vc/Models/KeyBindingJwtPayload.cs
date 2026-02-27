using System.Text.Json.Serialization;

namespace SdJwt.Net.Vc.Models;

/// <summary>
/// Represents a Key Binding JWT payload for SD-JWT VC presentations.
/// Used when the holder needs to prove possession of the key referenced in the cnf claim.
/// </summary>
public class KeyBindingJwtPayload
{
    /// <summary>
    /// Gets or sets the audience for the Key Binding JWT.
    /// Required. Identifier of the intended audience (Verifier).
    /// </summary>
    [JsonPropertyName("aud")]
    public string? Audience
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the nonce value.
    /// Optional. Cryptographic nonce provided by the Verifier.
    /// </summary>
    [JsonPropertyName("nonce")]
    public string? Nonce
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the issued at time.
    /// Required. Unix timestamp when the Key Binding JWT was created.
    /// </summary>
    [JsonPropertyName("iat")]
    public long IssuedAt
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the SD-JWT hash.
    /// Required. Hash of the Issuer-signed JWT and Disclosures.
    /// </summary>
    [JsonPropertyName("sd_hash")]
    public string? SdHash
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets any additional Key Binding JWT claims.
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, object>? AdditionalData
    {
        get; set;
    }
}
