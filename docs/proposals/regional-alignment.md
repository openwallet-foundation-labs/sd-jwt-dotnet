# Implementation Plan: Regional Alignment

|              |                                                                                                                         |
| ------------ | ----------------------------------------------------------------------------------------------------------------------- |
| **Status**   | Validated implementation plan                                                                                           |
| **Author**   | SD-JWT .NET Team                                                                                                        |
| **Created**  | 2026-03-04                                                                                                              |
| **Reviewed** | 2026-05-09                                                                                                              |
| **Packages** | `SdJwt.Net.Eudiw` (extension), `SdJwt.Net.HAIP` (extension), `SdJwt.Net.Mdoc` (integration), `SdJwt.Net.Trust` (future) |

---

## Context

Digital identity ecosystems are region-specific, but the implementation should not fork by region. The repository already has strong building blocks for EU, HAIP, OpenID4VC, mdoc, and wallet workflows. The missing piece is a technical profile layer that can select and validate a known set of requirements without embedding legal conclusions in the library.

Current coverage:

- `SdJwt.Net.Eudiw` implements EU Digital Identity Wallet profile logic and trust list models.
- `SdJwt.Net.HAIP` implements HAIP final requirement catalogs and validation helpers.
- `SdJwt.Net.Mdoc` implements ISO 18013-5 credential handling.
- `SdJwt.Net.Oid4Vp`, `SdJwt.Net.Oid4Vci`, and `SdJwt.Net.Wallet` provide reusable protocol and wallet workflows.

## Direction

1. Implement regional profiles as technical validation profiles, not legal compliance certificates.
2. Use explicit configuration. Do not auto-detect a region from credential content.
3. Model HAIP requirements by final flow/profile identifiers, not legacy numeric levels.
4. Keep regional packages optional and layered over existing implementation packages.
5. Depend on `SdJwt.Net.Trust` only when the trust-resolver proposal is implemented.
6. Keep profile data versioned, because national frameworks and ecosystem rules change independently from protocol specs.

## Goals

1. Define `IRegionalProfile` for technical requirements, validation, and package mapping.
2. Provide an EMEA/EUDIW profile over existing `SdJwt.Net.Eudiw` and HAIP validators.
3. Provide starter APAC and Americas profile shells for owner-supplied rules.
4. Provide a custom profile builder for private ecosystems.
5. Add concise docs that explain supported checks and explicitly exclude legal advice.

## Non-goals

- Legal compliance advice.
- Region-specific wallet UX.
- Management of regional certificate authorities or registries.
- Hard-coding every national rule in the first implementation.

---

## Proposed design

### Regional profile interface

```csharp
public interface IRegionalProfile
{
    string ProfileId { get; }

    string DisplayName { get; }

    string Version { get; }

    IReadOnlyList<string> SupportedFormats { get; }

    IReadOnlyList<string> AllowedAlgorithms { get; }

    IReadOnlyList<string> RequiredHaipFlows { get; }

    IReadOnlyList<string> RequiredCredentialProfiles { get; }

    IReadOnlyList<string> RequiredTrustFrameworks { get; }

    Task<RegionalValidationResult> ValidateAsync(
        RegionalValidationContext context,
        CancellationToken cancellationToken = default);
}
```

### Profile factory

```csharp
var profile = RegionalProfileFactory.Create("emea-eudiw");

var custom = new CustomRegionalProfileBuilder("enterprise-eu")
    .WithFormats("vc+sd-jwt", "mso_mdoc")
    .WithAlgorithms("ES256", "ES384")
    .WithHaipFlow("openid4vp-dc-api")
    .WithCredentialProfile("sd-jwt-vc")
    .WithTrustFramework("eidas-lotl")
    .Build();

var result = await profile.ValidateAsync(new RegionalValidationContext
{
    Credential = credential,
    IssuerIdentifier = "https://issuer.example.de",
    CredentialType = "eu.europa.ec.eudi.pid.1"
});
```

### Regional profile scope

| Profile family | First implementation scope                                                    | Notes                                               |
| -------------- | ----------------------------------------------------------------------------- | --------------------------------------------------- |
| EMEA/EUDIW     | Wrap existing EUDIW, HAIP, OpenID4VC, and mdoc validators                     | Highest confidence because implementation exists    |
| APAC           | Configuration shells for NZ, AU, Thailand, and Japan owner-supplied rules     | Do not claim framework compliance without rule data |
| Americas       | Configuration shells for US mDL, Canadian, and Brazil owner-supplied rules    | Use `SdJwt.Net.Mdoc` for mDL technical checks       |
| Custom         | Builder-driven private ecosystem profile with explicit algorithms and formats | Useful for enterprise and pilot deployments         |

### Validation output

```csharp
public sealed class RegionalValidationResult
{
    public bool IsValid { get; init; }

    public required string ProfileId { get; init; }

    public IReadOnlyList<RegionalValidationFinding> Findings { get; init; } = [];
}

public sealed class RegionalValidationFinding
{
    public required string RequirementId { get; init; }

    public required string Message { get; init; }

    public bool Passed { get; init; }
}
```

Findings should state what was checked, not broader regulatory conclusions.

---

## Implementation phases

| Phase | Component                  | Scope                                                             |
| ----- | -------------------------- | ----------------------------------------------------------------- |
| 1     | Core profile contracts     | Interfaces, result models, validation context                     |
| 2     | EMEA/EUDIW profile         | Reuse EUDIW, HAIP, OID4VC, and mdoc validators                    |
| 3     | Custom profile builder     | Format, algorithm, HAIP flow, credential profile, trust framework |
| 4     | APAC and Americas shells   | Configuration-only profiles with no unsupported legal claims      |
| 5     | Trust resolver integration | Add trust checks after `SdJwt.Net.Trust` exists                   |
| 6     | Tests and documentation    | Positive and negative examples for each profile type              |

---

## Security considerations

| Concern                            | Mitigation                                             |
| ---------------------------------- | ------------------------------------------------------ |
| Incorrect profile selection        | Explicit profile ID required                           |
| Overstated compliance              | Technical findings only; no legal compliance language  |
| Stale regional requirements        | Versioned profile definitions and documentation dates  |
| Cross-region credential acceptance | Profile validation checks formats, algorithms, trust   |
| Weak custom profile configuration  | Defaults use existing HAIP and algorithm allow-listing |

---

## Estimated effort

| Component                     | Effort      |
| ----------------------------- | ----------- |
| Core `IRegionalProfile` model | 3 days      |
| EMEA/EUDIW profile            | 4 days      |
| Custom profile builder        | 3 days      |
| APAC profile shells           | 3 days      |
| Americas profile shells       | 3 days      |
| Trust resolver integration    | 4 days      |
| Tests and documentation       | 5 days      |
| **Total**                     | **25 days** |

---

## Related documentation

- [EUDIW](../concepts/eudiw.md)
- [HAIP](../concepts/haip.md)
- [Trust Registries Implementation Plan](trust-registries-qtsp.md)
- [Capability Matrix](../reference/capabilities.md)
