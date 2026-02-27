using System.Text.Json.Serialization;

namespace SdJwt.Net.Oid4Vp.Models.Dcql;

/// <summary>
/// Represents a single credential query within a DCQL query as defined in OID4VP 1.0 Section 6.1.
/// Specifies the type and claims of a credential the verifier is requesting.
/// </summary>
public class DcqlCredentialQuery
{
    /// <summary>
    /// Gets or sets the unique identifier for this credential query within the DCQL query.
    /// REQUIRED. Must be unique within the enclosing <see cref="DcqlQuery.Credentials"/> array.
    /// Used in <see cref="DcqlCredentialSetQuery.Options"/> to reference this query.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the credential format identifier.
    /// REQUIRED. Identifies the format of the requested credential (e.g., "vc+sd-jwt", "mso_mdoc").
    /// </summary>
    [JsonPropertyName("format")]
    public string Format { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets format-specific metadata for the credential query.
    /// OPTIONAL. Contains additional format-specific parameters for the credential request.
    /// </summary>
    [JsonPropertyName("meta")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public Dictionary<string, object>? Meta
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the claims required from this credential.
    /// OPTIONAL. If omitted, the verifier requests the credential but does not mandate
    /// specific claim disclosures beyond what the format requires.
    /// </summary>
    [JsonPropertyName("claims")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public DcqlClaimsQuery[]? Claims
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets alternative sets of claims that satisfy this credential query.
    /// OPTIONAL. Each inner array is a set of claim IDs (from <see cref="Claims"/>) that,
    /// if all disclosed, would satisfy one acceptable presentation.
    /// Only valid if <see cref="Claims"/> is also present.
    /// </summary>
    [JsonPropertyName("claim_sets")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public string[][]? ClaimSets
    {
        get; set;
    }

    /// <summary>
    /// Validates this credential query according to OID4VP 1.0 Section 6.1.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the query is invalid.</exception>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Id))
            throw new InvalidOperationException("DCQL credential query 'id' is required and must not be empty.");

        if (string.IsNullOrWhiteSpace(Format))
            throw new InvalidOperationException($"DCQL credential query '{Id}' requires a 'format' value.");
    }
}
