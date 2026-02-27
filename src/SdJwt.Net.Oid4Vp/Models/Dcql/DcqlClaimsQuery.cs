using System.Text.Json.Serialization;

namespace SdJwt.Net.Oid4Vp.Models.Dcql;

/// <summary>
/// Represents a claim path query within a DCQL credential query as defined in OID4VP 1.0 Section 6.3.
/// Specifies a path to a claim that the verifier requires, along with optional acceptable values.
/// </summary>
public class DcqlClaimsQuery
{
    /// <summary>
    /// Gets or sets the path to the claim being requested.
    /// REQUIRED. An array of path elements that identifies the claim within the credential.
    /// Each element is either a string (object key) or null (any array element).
    /// </summary>
    [JsonPropertyName("path")]
    public object[] Path { get; set; } = Array.Empty<object>();

    /// <summary>
    /// Gets or sets the acceptable values for the claim.
    /// OPTIONAL. If present, the claim value in the presented credential MUST match
    /// one of the specified values. Supports strings, numbers, and booleans.
    /// </summary>
    [JsonPropertyName("values")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public object[]? Values
    {
        get; set;
    }
}
