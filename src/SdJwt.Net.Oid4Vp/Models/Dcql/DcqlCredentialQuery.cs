using SdJwt.Net.Oid4Vp.Models.Dcql.Formats;
using SdJwt.Net.Oid4Vp.Models;
using System.Text.Json.Serialization;

namespace SdJwt.Net.Oid4Vp.Models.Dcql;

/// <summary>
/// Represents a single credential query within a DCQL query as defined in OID4VP 1.0 Section 7.
/// Specifies the format, type constraints, trusted authority constraints, and required claims
/// of a credential the verifier is requesting.
/// </summary>
public class DcqlCredentialQuery
{
    /// <summary>
    /// Gets or sets the unique identifier for this credential query.
    /// REQUIRED. Must be unique within the enclosing <see cref="DcqlQuery.Credentials"/> array.
    /// The wallet uses this value as the key in the VP Token response object.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the credential format identifier.
    /// REQUIRED. Identifies the format of the requested credential (e.g., <c>dc+sd-jwt</c>, <c>mso_mdoc</c>).
    /// </summary>
    [JsonPropertyName("format")]
    public string Format { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether multiple credential presentations may satisfy this query.
    /// OPTIONAL. When <see langword="true"/>, the wallet may present more than one credential
    /// matching this query. Defaults to <see langword="false"/> (exactly one presentation).
    /// </summary>
    [JsonPropertyName("multiple")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
#endif
    public bool Multiple
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets whether the holder must prove cryptographic control of the credential's subject key.
    /// OPTIONAL. Defaults to <see langword="true"/>. When <see langword="false"/>, the verifier
    /// accepts bearer credentials without a Key Binding JWT.
    /// </summary>
    [JsonPropertyName("require_cryptographic_holder_binding")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
#endif
    public bool RequireCryptographicHolderBinding { get; set; } = true;

    /// <summary>
    /// Gets or sets the format-specific metadata constraints for this query.
    /// REQUIRED. Use <see cref="Formats.SdJwtVcMeta"/> for <c>dc+sd-jwt</c>,
    /// <see cref="Formats.MsoMdocMeta"/> for <c>mso_mdoc</c>,
    /// and <see cref="Formats.W3cVcMeta"/> for <c>jwt_vc_json</c>/<c>ldp_vc</c>.
    /// </summary>
    [JsonPropertyName("meta")]
    [JsonConverter(typeof(DcqlMetaConverter))]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public IDcqlMeta? Meta
    {
        get; set;
    }

    /// <summary>
    /// Gets or sets the trusted authority constraints.
    /// OPTIONAL. When present, the wallet MUST only present credentials issued under one of
    /// the listed trusted authorities.
    /// </summary>
    [JsonPropertyName("trusted_authorities")]
#if NET6_0_OR_GREATER
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
#endif
    public TrustedAuthority[]? TrustedAuthorities
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
    /// if all disclosed, would satisfy one acceptable presentation. Sets are listed in
    /// preference order. Only valid when <see cref="Claims"/> is also present.
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
    /// Validates this credential query according to OID4VP 1.0 Section 7.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the query is invalid.</exception>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Id))
            throw new InvalidOperationException("DCQL credential query 'id' is required and must not be empty.");

        if (string.IsNullOrWhiteSpace(Format))
            throw new InvalidOperationException($"DCQL credential query '{Id}' requires a 'format' value.");

        ValidateFormatMetadata();
        ValidateClaims();

        if (ClaimSets != null && Claims == null)
            throw new InvalidOperationException(
                $"DCQL credential query '{Id}': 'claim_sets' requires 'claims' to also be present.");

        if (ClaimSets != null)
            ValidateClaimSets();

        if (TrustedAuthorities != null)
        {
            foreach (var ta in TrustedAuthorities)
            {
                if (string.IsNullOrWhiteSpace(ta.Type))
                    throw new InvalidOperationException(
                        $"DCQL credential query '{Id}': each trusted_authority must have a non-empty 'type'.");
                if (ta.Values == null || ta.Values.Length == 0)
                    throw new InvalidOperationException(
                        $"DCQL credential query '{Id}': each trusted_authority must have at least one value.");
            }
        }
    }

    private void ValidateFormatMetadata()
    {
        if (Meta == null)
            throw new InvalidOperationException($"DCQL credential query '{Id}' requires a 'meta' object.");

        switch (Format)
        {
            case Oid4VpConstants.SdJwtVcFormat:
            case Oid4VpConstants.SdJwtVcLegacyFormat:
                if (Meta is not SdJwtVcMeta sdJwtMeta ||
                    sdJwtMeta.VctValues == null ||
                    sdJwtMeta.VctValues.Length == 0 ||
                    sdJwtMeta.VctValues.Any(string.IsNullOrWhiteSpace))
                {
                    throw new InvalidOperationException(
                        $"DCQL credential query '{Id}': dc+sd-jwt requires non-empty meta.vct_values.");
                }
                break;

            case Oid4VpConstants.MsoMdocFormat:
                if (Meta is not MsoMdocMeta mdocMeta || string.IsNullOrWhiteSpace(mdocMeta.DoctypeValue))
                {
                    throw new InvalidOperationException(
                        $"DCQL credential query '{Id}': mso_mdoc requires meta.doctype_value.");
                }
                break;

            case Oid4VpConstants.JwtVcJsonFormat:
            case Oid4VpConstants.LdpVcFormat:
            case Oid4VpConstants.JwtVcJsonLdFormat:
                if (Meta is not W3cVcMeta w3cMeta ||
                    w3cMeta.TypeValues == null ||
                    w3cMeta.TypeValues.Length == 0 ||
                    w3cMeta.TypeValues.Any(option => option == null || option.Length == 0 || option.Any(string.IsNullOrWhiteSpace)))
                {
                    throw new InvalidOperationException(
                        $"DCQL credential query '{Id}': W3C VC formats require non-empty meta.type_values.");
                }
                break;

            default:
                break;
        }
    }

    private void ValidateClaimSets()
    {
        var claimIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (var claim in Claims!)
        {
            if (string.IsNullOrWhiteSpace(claim.Id))
            {
                throw new InvalidOperationException(
                    $"DCQL credential query '{Id}': 'claim_sets' requires referenced claims to have ids.");
            }

            claimIds.Add(claim.Id!);
        }

        foreach (var claimSet in ClaimSets!)
        {
            if (claimSet == null || claimSet.Length == 0)
                throw new InvalidOperationException(
                    $"DCQL credential query '{Id}': each claim_sets entry must contain at least one claim id.");

            foreach (var claimId in claimSet)
            {
                if (string.IsNullOrWhiteSpace(claimId) || !claimIds.Contains(claimId))
                    throw new InvalidOperationException(
                        $"DCQL credential query '{Id}': claim_sets references unknown claim id '{claimId}'.");
            }
        }
    }

    private void ValidateClaims()
    {
        if (Claims == null)
        {
            return;
        }

        var claimIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (var claim in Claims)
        {
            if (claim == null)
            {
                throw new InvalidOperationException($"DCQL credential query '{Id}': 'claims' must not contain null entries.");
            }

            claim.Validate();

            if (!string.IsNullOrWhiteSpace(claim.Id) && !claimIds.Add(claim.Id!))
            {
                throw new InvalidOperationException(
                    $"DCQL credential query '{Id}': duplicate claim id '{claim.Id}'.");
            }
        }
    }
}
