using System.Text.Json.Serialization;

namespace SdJwt.Net.Oid4Vci.Models;

/// <summary>
/// Represents a response from the Nonce Endpoint according to OID4VCI 1.0 Section 7.
/// The nonce endpoint issues fresh <c>c_nonce</c> values for use in credential request proofs.
/// </summary>
public class NonceEndpointResponse
{
    /// <summary>
    /// Gets or sets the credential nonce.
    /// REQUIRED. String value to be used as <c>nonce</c> claim in proof JWTs.
    /// </summary>
    [JsonPropertyName("c_nonce")]
    public string CNonce { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the nonce expiration time in seconds.
    /// OPTIONAL. Lifetime of <see cref="CNonce"/> in seconds.
    /// </summary>
    [JsonPropertyName("c_nonce_expires_in")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public int? CNonceExpiresIn
    {
        get; set;
    }
}
