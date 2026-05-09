using System.Text.Json.Serialization;
using SdJwt.Net.VcDm.Serialization;

namespace SdJwt.Net.VcDm.Models;

/// <summary>
/// Base class for W3C VCDM 2.0 <c>credentialStatus</c> objects.
/// All implementations MUST provide a <c>type</c> string.
/// Deserialized via <see cref="CredentialStatusConverter"/>.
/// </summary>
[JsonConverter(typeof(CredentialStatusConverter))]
public abstract class CredentialStatus
{
    /// <summary>
    /// URI identifying this specific status entry. REQUIRED by VCDM 2.0.
    /// </summary>
    [JsonPropertyName("id")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public string? Id
    {
        get; set;
    }

    /// <summary>
    /// The status mechanism type identifier. REQUIRED.
    /// </summary>
    [JsonPropertyName("type")]
    public abstract string Type
    {
        get;
    }
}

/// <summary>
/// W3C Bitstring Status List 2021 entry — the VCDM 2.0 recommended revocation mechanism.
/// Replaces the deprecated <c>StatusList2021Entry</c> type from VCDM 1.1.
/// </summary>
public sealed class BitstringStatusListEntry : CredentialStatus
{
    /// <inheritdoc/>
    public override string Type => "BitstringStatusListEntry";

    /// <summary>
    /// Purpose of this status entry: "revocation" or "suspension".
    /// </summary>
    [JsonPropertyName("statusPurpose")]
    public string StatusPurpose { get; set; } = "revocation";

    /// <summary>
    /// Integer index (as string) into the bitstring status list.
    /// </summary>
    [JsonPropertyName("statusListIndex")]
    public string StatusListIndex { get; set; } = string.Empty;

    /// <summary>
    /// URL of the Verifiable Credential that contains the status list bitstring.
    /// </summary>
    [JsonPropertyName("statusListCredential")]
    public string StatusListCredential { get; set; } = string.Empty;
}

/// <summary>
/// VCDM 1.1 / earlier-draft status type. Accepted when reading for backward compatibility.
/// Do not generate this; use <see cref="BitstringStatusListEntry"/> instead.
/// </summary>
[Obsolete("StatusList2021Entry was superseded by BitstringStatusListEntry in VCDM 2.0.")]
public sealed class StatusList2021Entry : CredentialStatus
{
    /// <inheritdoc/>
    public override string Type => "StatusList2021Entry";

    /// <summary>Purpose of this status entry.</summary>
    [JsonPropertyName("statusPurpose")]
    public string StatusPurpose { get; set; } = "revocation";

    /// <summary>Integer index (as string) into the bitstring status list.</summary>
    [JsonPropertyName("statusListIndex")]
    public string StatusListIndex { get; set; } = string.Empty;

    /// <summary>URL of the Verifiable Credential that contains the status list bitstring.</summary>
    [JsonPropertyName("statusListCredential")]
    public string StatusListCredential { get; set; } = string.Empty;
}

/// <summary>
/// Catch-all for unknown credential status types encountered during deserialization.
/// Preserves the raw JSON properties in <see cref="AdditionalProperties"/>.
/// </summary>
public sealed class UnknownCredentialStatus : CredentialStatus
{
    private readonly string _type;

    /// <inheritdoc/>
    public override string Type => _type;

    /// <summary>Raw JSON properties from the unrecognized status object.</summary>
    [JsonExtensionData]
    public Dictionary<string, object>? AdditionalProperties
    {
        get; set;
    }

    /// <summary>Initializes an instance with the given unknown type string.</summary>
    public UnknownCredentialStatus(string type) => _type = type;
}
