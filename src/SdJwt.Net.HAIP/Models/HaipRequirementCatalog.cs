namespace SdJwt.Net.HAIP;

/// <summary>
/// Scope of a HAIP 1.0 Final requirement represented by this package.
/// </summary>
public enum HaipRequirementScope
{
    /// <summary>
    /// Requirement applies to all selected HAIP Final flows and credential profiles.
    /// </summary>
    Common,

    /// <summary>
    /// Requirement applies to a selected OpenID4VC flow.
    /// </summary>
    Flow,

    /// <summary>
    /// Requirement applies to a selected credential format profile.
    /// </summary>
    CredentialProfile
}

/// <summary>
/// Validation status for a HAIP 1.0 Final requirement in this package.
/// </summary>
public enum HaipRequirementImplementationStatus
{
    /// <summary>
    /// The package validates the requirement as part of HAIP Final profile validation.
    /// </summary>
    Validated,

    /// <summary>
    /// The package records the requirement but delegates detailed enforcement to ecosystem policy.
    /// </summary>
    PolicyDependent,

    /// <summary>
    /// The requirement is tracked but not yet implemented.
    /// </summary>
    Planned
}

/// <summary>
/// Describes one HAIP 1.0 Final requirement tracked by this package.
/// </summary>
public sealed class HaipRequirement
{
    /// <summary>
    /// Gets the stable package-local identifier for the requirement.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets the scope where this requirement applies.
    /// </summary>
    public HaipRequirementScope Scope { get; set; }

    /// <summary>
    /// Gets the flow where this requirement applies, when scoped to a flow.
    /// </summary>
    public HaipFlow? Flow { get; set; }

    /// <summary>
    /// Gets the credential profile where this requirement applies, when scoped to a credential profile.
    /// </summary>
    public HaipCredentialProfile? CredentialProfile { get; set; }

    /// <summary>
    /// Gets the short requirement title.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets implementation notes for this package.
    /// </summary>
    public string Notes { get; set; } = string.Empty;

    /// <summary>
    /// Gets the current implementation status.
    /// </summary>
    public HaipRequirementImplementationStatus Status { get; set; }
}

/// <summary>
/// Machine-readable HAIP 1.0 Final requirement catalog used by validators, docs, and samples.
/// </summary>
public static class HaipRequirementCatalog
{
    /// <summary>
    /// Gets the HAIP 1.0 Final requirements tracked by this package.
    /// </summary>
    public static IReadOnlyList<HaipRequirement> Requirements { get; } = new[]
    {
        Common("HAIP-COMMON-JOSE-ES256", "JOSE ES256 validation support", "All HAIP entities must support ES256 validation.", HaipRequirementImplementationStatus.Validated),
        Common("HAIP-COMMON-SHA-256", "SHA-256 digest support", "SD-JWT VC and ISO mdoc profile validation requires SHA-256 support.", HaipRequirementImplementationStatus.Validated),
        Flow("HAIP-VCI-AUTH-CODE", HaipFlow.Oid4VciIssuance, "Authorization code flow", "OID4VCI HAIP deployments use the authorization code flow.", HaipRequirementImplementationStatus.Validated),
        Flow("HAIP-VCI-PKCE-S256", HaipFlow.Oid4VciIssuance, "PKCE S256", "Authorization code flow requires PKCE using S256.", HaipRequirementImplementationStatus.Validated),
        Flow("HAIP-VCI-PAR", HaipFlow.Oid4VciIssuance, "Pushed Authorization Requests", "PAR is required where the Authorization Endpoint is used.", HaipRequirementImplementationStatus.Validated),
        Flow("HAIP-VCI-DPOP", HaipFlow.Oid4VciIssuance, "DPoP sender constraint", "Sender-constrained access tokens require DPoP support.", HaipRequirementImplementationStatus.Validated),
        Flow("HAIP-VCI-DPOP-NONCE", HaipFlow.Oid4VciIssuance, "DPoP nonce handling", "DPoP nonce handling is required for replay protection.", HaipRequirementImplementationStatus.Validated),
        Flow("HAIP-VCI-ATTESTATION", HaipFlow.Oid4VciIssuance, "Wallet and key attestation", "Wallet Attestation and Key Attestation must be cryptographically validated where used.", HaipRequirementImplementationStatus.Validated),
        Flow("HAIP-VP-DCQL", HaipFlow.Oid4VpRedirectPresentation, "DCQL credential query", "OpenID4VP HAIP presentations use DCQL for credential queries.", HaipRequirementImplementationStatus.Validated),
        Flow("HAIP-VP-SIGNED-REQUEST", HaipFlow.Oid4VpRedirectPresentation, "Signed presentation request", "Signed request object validation is required where signed requests are used.", HaipRequirementImplementationStatus.Validated),
        Flow("HAIP-VP-VERIFIER-ATTESTATION", HaipFlow.Oid4VpRedirectPresentation, "Verifier attestation", "Verifier attestation validation is required where verifier attestation is used.", HaipRequirementImplementationStatus.Validated),
        Flow("HAIP-DCAPI", HaipFlow.Oid4VpDigitalCredentialsApiPresentation, "Digital Credentials API", "W3C Digital Credentials API support is required for the DC API flow.", HaipRequirementImplementationStatus.Validated),
        Profile("HAIP-SDJWTVC-FORMAT", HaipCredentialProfile.SdJwtVc, "SD-JWT VC dc+sd-jwt format", "The SD-JWT VC profile uses the dc+sd-jwt credential format identifier.", HaipRequirementImplementationStatus.Validated),
        Profile("HAIP-SDJWTVC-COMPACT", HaipCredentialProfile.SdJwtVc, "SD-JWT compact serialization", "SD-JWT VC credentials and presentations use compact serialization.", HaipRequirementImplementationStatus.Validated),
        Profile("HAIP-SDJWTVC-HOLDER-BINDING", HaipCredentialProfile.SdJwtVc, "SD-JWT VC holder binding", "Holder-bound SD-JWT VC credentials use cnf.jwk and KB-JWT presentations.", HaipRequirementImplementationStatus.Validated),
        Profile("HAIP-SDJWTVC-STATUS-LIST", HaipCredentialProfile.SdJwtVc, "Token Status List", "Credential status uses status.status_list when status is present.", HaipRequirementImplementationStatus.Validated),
        Profile("HAIP-SDJWTVC-X5C", HaipCredentialProfile.SdJwtVc, "SD-JWT VC x5c issuer key resolution", "Issuer key resolution supports x5c where HAIP policy requires it.", HaipRequirementImplementationStatus.Validated),
        Profile("HAIP-MDOC-FORMAT", HaipCredentialProfile.MsoMdoc, "ISO mdoc mso_mdoc format", "The mdoc profile uses the mso_mdoc credential format identifier.", HaipRequirementImplementationStatus.Validated),
        Profile("HAIP-MDOC-COSE", HaipCredentialProfile.MsoMdoc, "COSE ES256 validation", "mdoc validation requires COSE ES256 support.", HaipRequirementImplementationStatus.Validated),
        Profile("HAIP-MDOC-DEVICE-SIGNATURE", HaipCredentialProfile.MsoMdoc, "mdoc device signature", "mdoc presentations require device signature validation.", HaipRequirementImplementationStatus.Validated),
        Profile("HAIP-MDOC-X5CHAIN", HaipCredentialProfile.MsoMdoc, "mdoc x5chain trust", "x5chain validation is policy dependent where x5chain is used.", HaipRequirementImplementationStatus.Validated),
    };

    /// <summary>
    /// Gets catalog requirements that apply to the provided selected flows and credential profiles.
    /// </summary>
    /// <param name="options">The selected HAIP Final profile options.</param>
    /// <returns>The matching requirement list.</returns>
    public static IReadOnlyList<HaipRequirement> GetRequirements(HaipProfileOptions options)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        return Requirements
            .Where(requirement => AppliesTo(requirement, options))
            .ToArray();
    }

    private static bool AppliesTo(HaipRequirement requirement, HaipProfileOptions options)
    {
        return requirement.Scope switch
        {
            HaipRequirementScope.Common => true,
            HaipRequirementScope.Flow => requirement.Flow.HasValue && options.Flows.Contains(requirement.Flow.Value),
            HaipRequirementScope.CredentialProfile => requirement.CredentialProfile.HasValue && options.CredentialProfiles.Contains(requirement.CredentialProfile.Value),
            _ => false
        };
    }

    private static HaipRequirement Common(string id, string title, string notes, HaipRequirementImplementationStatus status)
    {
        return new HaipRequirement
        {
            Id = id,
            Scope = HaipRequirementScope.Common,
            Title = title,
            Notes = notes,
            Status = status
        };
    }

    private static HaipRequirement Flow(string id, HaipFlow flow, string title, string notes, HaipRequirementImplementationStatus status)
    {
        return new HaipRequirement
        {
            Id = id,
            Scope = HaipRequirementScope.Flow,
            Flow = flow,
            Title = title,
            Notes = notes,
            Status = status
        };
    }

    private static HaipRequirement Profile(string id, HaipCredentialProfile profile, string title, string notes, HaipRequirementImplementationStatus status)
    {
        return new HaipRequirement
        {
            Id = id,
            Scope = HaipRequirementScope.CredentialProfile,
            CredentialProfile = profile,
            Title = title,
            Notes = notes,
            Status = status
        };
    }
}
