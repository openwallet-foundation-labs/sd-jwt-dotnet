# Implementation Plan: Assurance Profile Extension Point

|                    |                                                                                                                          |
| ------------------ | ------------------------------------------------------------------------------------------------------------------------ |
| **Status**         | Deferred                                                                                                                 |
| **Priority**       | Deferred                                                                                                                 |
| **Author**         | SD-JWT .NET Team                                                                                                         |
| **Created**        | 2026-03-04                                                                                                               |
| **Reviewed**       | 2026-05-09                                                                                                               |
| **Maturity**       | Preview                                                                                                                  |
| **Package**        | `SdJwt.Net.Eudiw` (extension), `SdJwt.Net.HAIP` (extension), `SdJwt.Net.Mdoc` (integration), `SdJwt.Net.Trust` (future) |
| **New package?**   | No                                                                                                                       |
| **Public API?**    | Yes (interface only)                                                                                                     |

---

## Context

Digital identity ecosystems are region-specific, but the implementation should not fork by region. The repository already has strong building blocks for EU, HAIP, OpenID4VC, mdoc, and wallet workflows. The missing piece is a technical profile layer that can select and validate a known set of requirements without embedding legal conclusions in the library.

Current coverage:

- `SdJwt.Net.Eudiw` implements EU Digital Identity Wallet profile logic and trust list models.
- `SdJwt.Net.HAIP` implements HAIP final requirement catalogs and validation helpers.
- `SdJwt.Net.Mdoc` implements ISO 18013-5 credential handling.
- `SdJwt.Net.Oid4Vp`, `SdJwt.Net.Oid4Vci`, and `SdJwt.Net.Wallet` provide reusable protocol and wallet workflows.

## Why deferred

The core extension-point idea is sound, but concrete profile implementations need the trust resolver (P2) to exist first. The region-specific shells (APAC, Americas, EMEA) also risk overcommitting to requirements that change frequently. This proposal retains only the abstraction and builder contracts for future implementation.

## Direction

1. Implement assurance profiles as technical validation profiles, not legal compliance certificates.
2. Use explicit configuration. Do not auto-detect a region from credential content.
3. Model HAIP requirements by final flow/profile identifiers, not legacy numeric levels.
4. Keep profile packages optional and layered over existing implementation packages.
5. Depend on `SdJwt.Net.Trust` only when the trust-resolver proposal is implemented.
6. Keep profile data versioned, because national frameworks and ecosystem rules change independently from protocol specs.

## Goals

1. Define `IAssuranceProfile` for technical requirements, validation, and package mapping.
2. Provide a custom profile builder for private and ecosystem-specific deployments.
3. Add concise docs that explain supported checks and explicitly exclude legal advice.

## Non-goals

- Legal compliance advice.
- Region-specific wallet UX.
- Management of regional certificate authorities or registries.
- Hard-coding national rules for specific regions.
- Providing APAC/Americas/EMEA profile shells (deferred until demand and rule data exist).

---

## Proposed design

### Assurance profile interface

```csharp
public interface IAssuranceProfile
{
    string ProfileId { get; }

    string DisplayName { get; }

    string Version { get; }

    IReadOnlyList<string> SupportedFormats { get; }

    IReadOnlyList<string> AllowedAlgorithms { get; }

    IReadOnlyList<string> RequiredHaipFlows { get; }

    IReadOnlyList<string> RequiredCredentialProfiles { get; }

    IReadOnlyList<string> RequiredTrustFrameworks { get; }

    Task<AssuranceValidationResult> ValidateAsync(
        AssuranceValidationContext context,
        CancellationToken cancellationToken = default);
}
```

### Profile builder

```csharp
var custom = new AssuranceProfileBuilder("enterprise-eu")
    .WithFormats("dc+sd-jwt", "mso_mdoc")
    .WithAlgorithms("ES256", "ES384")
    .WithHaipFlow("openid4vp-dc-api")
    .WithCredentialProfile("sd-jwt-vc")
    .WithTrustFramework("eidas-lotl")
    .Build();

var result = await custom.ValidateAsync(new AssuranceValidationContext
{
    Credential = credential,
    IssuerIdentifier = "https://issuer.example.de",
    CredentialType = "eu.europa.ec.eudi.pid.1"
});
```

### Validation output

```csharp
public sealed class AssuranceValidationResult
{
    public bool IsValid { get; init; }

    public required string ProfileId { get; init; }

    public IReadOnlyList<AssuranceValidationFinding> Findings { get; init; } = [];
}

public sealed class AssuranceValidationFinding
{
    public required string RequirementId { get; init; }

    public required string Message { get; init; }

    public bool Passed { get; init; }
}
```

Findings should state what was checked, not broader regulatory conclusions.

---

## Implementation phases (when un-deferred)

| Phase | Component                  | Scope                                                             |
| ----- | -------------------------- | ----------------------------------------------------------------- |
| 1     | Core profile contracts     | `IAssuranceProfile`, result models, validation context            |
| 2     | Custom profile builder     | Format, algorithm, HAIP flow, credential profile, trust framework |
| 3     | Trust resolver integration | Add trust checks after `SdJwt.Net.Trust` exists                   |
| 4     | Tests and documentation    | Positive and negative examples for each profile type              |

---

## Security considerations

| Concern                            | Mitigation                                             |
| ---------------------------------- | ------------------------------------------------------ |
| Incorrect profile selection        | Explicit profile ID required                           |
| Overstated compliance              | Technical findings only; no legal compliance language  |
| Stale requirements                 | Versioned profile definitions and documentation dates  |
| Cross-region credential acceptance | Profile validation checks formats, algorithms, trust   |
| Weak custom profile configuration  | Defaults use existing HAIP and algorithm allow-listing |

---

## Estimated effort

| Component                     | Effort      |
| ----------------------------- | ----------- |
| Core `IAssuranceProfile` model | 3 days     |
| Custom profile builder        | 3 days      |
| Trust resolver integration    | 4 days      |
| Tests and documentation       | 4 days      |
| **Total**                     | **14 days** |

---

## Related documentation

- [EUDIW](../../concepts/eudiw.md)
- [HAIP](../../concepts/haip.md)
- [Trust Registries Implementation Plan](../trust-registries-qtsp.md)
