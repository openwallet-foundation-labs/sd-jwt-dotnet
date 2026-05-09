# HAIP Integration Guide

|                      |                                                                                                                                                                                  |
| -------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Audience**         | Developers integrating HAIP Final validation into issuer, wallet, or verifier services.                                                                                          |
| **Purpose**          | Show how to use `SdJwt.Net.HAIP` as a fail-closed policy gate for OpenID4VC HAIP 1.0 Final flows and credential profiles.                                                        |
| **Scope**            | Package setup, profile option construction, requirement catalog usage, audit metadata, and testing. Out of scope: concrete OAuth, DPoP, attestation, SD-JWT VC, or mdoc parsing. |
| **Success criteria** | Reader can validate selected HAIP Final capabilities and report applicable requirement IDs in audit logs.                                                                        |

## Quick start

```csharp
using SdJwt.Net.HAIP;
using SdJwt.Net.HAIP.Validators;

var options = new HaipProfileOptions();
options.Flows.Add(HaipFlow.Oid4VciIssuance);
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
options.SupportsSdJwtVcCompactSerialization = true;
options.UsesCnfJwkForSdJwtVcHolderBinding = true;
options.RequiresKbJwtForHolderBoundSdJwtVc = true;
options.SupportsStatusListClaim = true;
options.SupportsSdJwtVcIssuerX5c = true;

var result = new HaipProfileValidator().Validate(options);
```

## Validate at startup

For services with fixed capabilities, validate once during startup and fail closed if the service configuration does not meet the selected HAIP Final profile.

```csharp
builder.Services.AddSingleton<HaipProfileValidator>();
builder.Services.AddSingleton(sp =>
{
    var options = BuildHaipOptionsFromConfiguration(builder.Configuration);
    var result = sp.GetRequiredService<HaipProfileValidator>().Validate(options);

    if (!result.IsCompliant)
    {
        var message = string.Join("; ", result.Violations.Select(v => v.Description));
        throw new InvalidOperationException($"HAIP Final configuration is not compliant: {message}");
    }

    return result;
});
```

## Use the requirement catalog

`HaipRequirementCatalog` provides a shared list for documentation, operational dashboards, and conformance reports.

```csharp
var applicable = HaipRequirementCatalog.GetRequirements(options);

foreach (var requirement in applicable)
{
    logger.LogInformation(
        "HAIP requirement {RequirementId}: {Title} ({Status})",
        requirement.Id,
        requirement.Title,
        requirement.Status);
}
```

`HaipProfileValidator` also stores applicable requirement IDs in `HaipComplianceResult.Metadata["applicable_requirements"]`.

## Per-flow examples

| Scenario                                                | Selected flow                             | Selected profile        |
| ------------------------------------------------------- | ----------------------------------------- | ----------------------- |
| Issuer issuing SD-JWT VC credentials                    | `Oid4VciIssuance`                         | `SdJwtVc`               |
| Verifier receiving SD-JWT VC through redirect OID4VP    | `Oid4VpRedirectPresentation`              | `SdJwtVc`               |
| Browser verifier using Digital Credentials API for mdoc | `Oid4VpDigitalCredentialsApiPresentation` | `MsoMdoc`               |
| Wallet supporting both SD-JWT VC and mdoc               | One or more OID4VC flows                  | `SdJwtVc` and `MsoMdoc` |

## Testing

```csharp
[Fact]
public void HaipProfile_WithMissingDpop_ShouldFail()
{
    var options = BuildCompleteSdJwtVcOptions();
    options.SupportsDpop = false;

    var result = new HaipProfileValidator().Validate(options);

    Assert.False(result.IsCompliant);
    Assert.Contains(result.Violations, v => v.Description.Contains("DPoP"));
}
```

## Legacy compatibility

`HaipLevel`, `HaipCryptoValidator`, and `HaipProtocolValidator` are still available for existing callers, but they are not HAIP Final conformance levels. Use them only as local policy helpers or during migration.

## Related documentation

- [HAIP](haip.md)
- [HAIP sample](../../samples/SdJwt.Net.Samples/03-Advanced/02-HaipCompliance.cs)
- [Package README](../../src/SdJwt.Net.HAIP/README.md)
