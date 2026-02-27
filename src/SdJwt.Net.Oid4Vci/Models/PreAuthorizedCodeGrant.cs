using System.Text.Json.Serialization;

namespace SdJwt.Net.Oid4Vci.Models;

/// <summary>
/// Represents pre-authorized code grant configuration according to OID4VCI 1.0 Section 4.1.1.
/// </summary>
public class PreAuthorizedCodeGrant
{
    /// <summary>
    /// Gets or sets the pre-authorized code.
    /// REQUIRED. The code representing the authorization to obtain Credentials of a certain type.
    /// </summary>
    [JsonPropertyName("pre-authorized_code")]
    public string PreAuthorizedCode { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the transaction code configuration (optional).
    /// OPTIONAL. Contains information about whether the Authorization Server expects 
    /// presentation of a transaction code by the End-User.
    /// </summary>
    [JsonPropertyName("tx_code")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public TransactionCode? TransactionCode
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the interval between polling requests in seconds.
    /// OPTIONAL. Number of seconds to wait between polling requests to the token endpoint. 
    /// If no value is provided, a default value of 5 seconds is recommended.
    /// </summary>
    [JsonPropertyName("interval")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public int? Interval
    {
        get; set;
    }
}
