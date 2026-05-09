# SdJwt.Net.HAIP - OpenID4VC HAIP

Implementation helpers for the OpenID4VC High Assurance Interoperability Profile 1.0 Final.

HAIP 1.0 Final does **not** define "Level 1", "Level 2", or "Level 3" compliance tiers. Earlier versions of this package exposed those names as local policy helpers. They are retained for source compatibility, but new integrations should use HAIP Final flow and credential profile validation.

## What HAIP Final Covers

HAIP Final defines interoperable requirements for selected OpenID4VC flows:

-   OpenID4VCI credential issuance
-   OpenID4VP presentation via redirects
-   OpenID4VP presentation via W3C Digital Credentials API

For each selected flow, at least one credential profile is required:

-   IETF SD-JWT VC using `dc+sd-jwt`
-   ISO mdoc using `mso_mdoc`

## Installation

```bash
dotnet add package SdJwt.Net.HAIP
```

For full OpenID4VC integrations, also reference the protocol and credential packages you use:

```bash
dotnet add package SdJwt.Net.Oid4Vci
dotnet add package SdJwt.Net.Oid4Vp
dotnet add package SdJwt.Net.Vc
dotnet add package SdJwt.Net.Mdoc
dotnet add package SdJwt.Net.StatusList
```

## HAIP Final Validation

`HaipProfileValidator` validates declared implementation capabilities against the HAIP Final flow/profile requirements this package can check.

```csharp
using SdJwt.Net.HAIP;
using SdJwt.Net.HAIP.Validators;

var options = new HaipProfileOptions();
options.Flows.Add(HaipFlow.Oid4VciIssuance);
options.Flows.Add(HaipFlow.Oid4VpRedirectPresentation);
options.CredentialProfiles.Add(HaipCredentialProfile.SdJwtVc);

options.SupportedCredentialFormats.Add(HaipConstants.SdJwtVcFormat);
options.SupportedJoseAlgorithms.Add(HaipConstants.RequiredJoseAlgorithm);
options.SupportedHashAlgorithms.Add(HaipConstants.RequiredHashAlgorithm);

options.SupportsAuthorizationCodeFlow = true;
options.EnforcesPkceS256 = true;
options.SupportsPushedAuthorizationRequests = true;
options.SupportsDpop = true;
options.SupportsDpopNonce = true;
options.ValidatesWalletAttestation = true;
options.ValidatesKeyAttestation = true;

options.SupportsDcql = true;
options.SupportsSignedPresentationRequests = true;
options.ValidatesVerifierAttestation = true;

options.SupportsSdJwtVcCompactSerialization = true;
options.UsesCnfJwkForSdJwtVcHolderBinding = true;
options.RequiresKbJwtForHolderBoundSdJwtVc = true;
options.SupportsStatusListClaim = true;
options.SupportsSdJwtVcIssuerX5c = true;

var validator = new HaipProfileValidator();
var result = validator.Validate(options);

if (!result.IsCompliant)
{
    foreach (var violation in result.Violations)
    {
        Console.WriteLine($"{violation.Description}: {violation.RecommendedAction}");
    }
}
```

## Minimum Cryptographic Support

HAIP Final requires entities to support:

-   JOSE `ES256` for validation
-   COSE ES256 identifiers `-7` or `-9`, as applicable
-   SHA-256 digest generation and validation for SD-JWT VC and ISO mdoc

Ecosystems may require additional suites. Configure those as ecosystem policy; do not treat them as HAIP-defined levels.

## Requirement Catalog

Use `HaipRequirementCatalog` to enumerate the package-local HAIP Final requirement IDs for selected flows and credential profiles:

```csharp
foreach (var requirement in HaipRequirementCatalog.GetRequirements(options))
{
    Console.WriteLine($"{requirement.Id}: {requirement.Title}");
}
```

`HaipProfileValidator` also records applicable requirement IDs in `HaipComplianceResult.Metadata["applicable_requirements"]`.

## Legacy APIs

The following APIs remain available for compatibility with existing code but are not HAIP Final concepts:

-   `HaipLevel.Level1_High`
-   `HaipLevel.Level2_VeryHigh`
-   `HaipLevel.Level3_Sovereign`
-   Level-specific algorithm arrays in `HaipConstants`
-   `HaipCryptoValidator` and `HaipProtocolValidator` constructors that take `HaipLevel`

Use `HaipProfileValidator` for new HAIP 1.0 Final integrations.

## Related Documentation

-   [HAIP Concepts](../../docs/concepts/haip.md)
-   [HAIP Compliance Guide](../../docs/concepts/haip-compliance.md)
