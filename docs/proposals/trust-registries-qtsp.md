# Implementation Plan: Trust Registries and QTSP Integration

|                    |                                                                                                                                                                                                                                   |
| ------------------ | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Status**         | Validated implementation plan                                                                                                                                                                                                     |
| **Author**         | SD-JWT .NET Team                                                                                                                                                                                                                  |
| **Created**        | 2026-03-04                                                                                                                                                                                                                        |
| **Reviewed**       | 2026-05-09                                                                                                                                                                                                                        |
| **Packages**       | `SdJwt.Net.Trust` (new), `SdJwt.Net.Eudiw` (extension), `SdJwt.Net.OidFederation` (integration), `SdJwt.Net.Mdoc` (integration)                                                                                                   |
| **Specifications** | [eIDAS 2.0](https://eur-lex.europa.eu/eli/reg/2024/1183), [ETSI TS 119 612](https://www.etsi.org/deliver/etsi_ts/119600_119699/119612/), [DID Web](https://w3c-ccg.github.io/did-method-web/), OpenID Federation 1.0, ISO 18013-5 |

---

## Context

Issuer trust is currently handled in package-specific ways:

- `SdJwt.Net.OidFederation` resolves OpenID Federation trust chains.
- `SdJwt.Net.Eudiw` includes EU List of Trusted Lists and trusted service provider models.
- `SdJwt.Net.Mdoc` validates mdoc structures, but trust anchor selection remains caller-driven.
- `SdJwt.Net.SiopV2` includes DID key/JWK resolver support, but not `did:web` issuer validation.

The remaining gap is a shared trust abstraction that can orchestrate these existing trust sources and add optional adapters for DID Web, X.509/IACA trust anchors, custom registries, and QTSP-qualified trust evidence.

## Direction

1. Add a neutral `SdJwt.Net.Trust` package only for shared contracts, orchestration, cache policy, and result models.
2. Keep framework-specific logic in adapters. Do not move EUDIW or OpenID Federation implementation details into the new package.
3. Start DID support with `did:web`; make other DID methods plugin-driven.
4. Treat EBSI and custom registries as versioned adapters, because registry APIs and governance rules evolve outside the library.
5. Use .NET `X509Chain` and caller-provided trust anchors for X.509/IACA validation where possible.
6. For QTSP checks, report verifiable evidence from trusted lists and certificate chains. Do not claim legal qualification unless the trust service type, certificate status, and chain validation all support that conclusion.

## Goals

1. Define a shared `ITrustResolver` contract and evidence-rich result model.
2. Wrap existing OpenID Federation and EUDIW trust resolution as adapters.
3. Add `did:web` issuer resolution.
4. Add X.509/IACA trust anchor validation for mdoc-oriented deployments.
5. Add registry adapters for custom registries and experimental EBSI-style registries.
6. Add QTSP validation helpers based on trusted list service metadata and certificate evidence.

## Non-goals

- Operating a trust registry.
- Providing legal compliance advice.
- Certificate issuance or lifecycle management.
- Supporting every DID method in the first implementation.

---

## Proposed design

### Shared contracts

```csharp
public interface ITrustResolver
{
    Task<TrustResolutionResult> ResolveAsync(
        TrustSubject subject,
        TrustResolutionOptions options,
        CancellationToken cancellationToken = default);
}

public interface ITrustFrameworkAdapter
{
    string FrameworkId { get; }

    bool CanResolve(TrustSubject subject, TrustResolutionOptions options);

    Task<TrustFrameworkResult> ResolveAsync(
        TrustSubject subject,
        TrustResolutionOptions options,
        CancellationToken cancellationToken = default);
}

public sealed class TrustResolutionResult
{
    public bool IsTrusted { get; init; }

    public string? SelectedFrameworkId { get; init; }

    public IReadOnlyList<TrustEvidence> Evidence { get; init; } = [];

    public IReadOnlyList<string> AuthorizedCredentialTypes { get; init; } = [];

    public DateTimeOffset ResolvedAt { get; init; }
}
```

### Trust evidence model

```csharp
public sealed class TrustEvidence
{
    public required string Source { get; init; }

    public required string EvidenceType { get; init; }

    public bool IsPositive { get; init; }

    public string? SubjectName { get; init; }

    public DateTimeOffset? ValidUntil { get; init; }

    public IReadOnlyDictionary<string, string> Properties { get; init; } =
        new Dictionary<string, string>();
}
```

Evidence types should be explicit, for example:

- `openid-federation-chain`
- `eu-trusted-list-service`
- `did-web-document`
- `x509-chain`
- `iaca-trust-anchor`
- `qtsp-qualified-certificate`
- `registry-entry`

### Adapter phases

| Phase | Component                   | Scope                                                                 |
| ----- | --------------------------- | --------------------------------------------------------------------- |
| 1     | Core trust contracts        | `ITrustResolver`, adapter registry, result aggregation, cache options |
| 2     | Existing framework adapters | OpenID Federation adapter and EUDIW LOTL adapter                      |
| 3     | DID Web adapter             | Resolve `did:web`, validate verification methods, cache DID Documents |
| 4     | X.509/IACA adapter          | Validate certificate chains against configured anchors                |
| 5     | Registry adapters           | Custom registry adapter first; EBSI-style adapter marked experimental |
| 6     | QTSP helper                 | Map trusted list services and certificates to qualified evidence      |
| 7     | Documentation and tests     | Integration examples, negative trust tests, cache expiry tests        |

### QTSP validation helper

```csharp
public sealed class QtspSignatureValidator
{
    public Task<QtspValidationResult> ValidateAsync(
        X509Certificate2 signingCertificate,
        QtspValidationOptions options,
        CancellationToken cancellationToken = default);
}

public sealed class QtspValidationResult
{
    public bool IsQualified { get; init; }

    public string? TrustServiceProvider { get; init; }

    public string? TrustServiceType { get; init; }

    public IReadOnlyList<TrustEvidence> Evidence { get; init; } = [];
}
```

`IsQualified` must be `false` unless all required evidence is present and current.

---

## Security considerations

| Concern                 | Mitigation                                                               |
| ----------------------- | ------------------------------------------------------------------------ |
| Stale trust data        | Cache TTLs, explicit refresh, `ResolvedAt` and evidence validity windows |
| Trust list tampering    | Signature validation where the trust list format supports it             |
| Registry spoofing       | HTTPS-only endpoints plus configured issuer/registry allow-lists         |
| Certificate misuse      | Chain validation, key usage checks, validity period checks               |
| Overstated trust result | Evidence-first result model; no legal status without supporting evidence |

---

## Estimated effort

| Component                                | Effort      |
| ---------------------------------------- | ----------- |
| Core contracts and resolver orchestrator | 4 days      |
| OpenID Federation and EUDIW adapters     | 4 days      |
| DID Web adapter                          | 3 days      |
| X.509/IACA adapter                       | 4 days      |
| Custom registry adapter                  | 3 days      |
| Experimental EBSI-style registry adapter | 3 days      |
| QTSP validation helper                   | 5 days      |
| Tests and documentation                  | 5 days      |
| **Total**                                | **31 days** |

---

## Related documentation

- [EUDIW](../concepts/eudiw.md)
- [HAIP](../concepts/haip.md)
- [Ecosystem Architecture](../concepts/ecosystem-architecture.md)
