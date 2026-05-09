# Implementation Plan: Trust Registries and QTSP Integration

|                    |                                                                                                      |
| ------------------ | ---------------------------------------------------------------------------------------------------- |
| **Status**         | Planned                                                                                              |
| **Priority**       | P2 - Implement later                                                                                 |
| **Author**         | SD-JWT .NET Team                                                                                     |
| **Created**        | 2026-03-04                                                                                           |
| **Reviewed**       | 2026-05-09                                                                                           |
| **Maturity**       | Stable (core contracts); Spec-tracking (adapters)                                                    |
| **Package**        | `SdJwt.Net.Trust` (new - abstractions only)                                                          |
| **New package?**   | Yes - shared contracts, orchestration, cache policy, and result models only                          |
| **Public API?**    | Yes                                                                                                  |
| **Specifications** | [OpenID Federation 1.0](https://openid.net/specs/openid-federation-1_0.html), ISO 18013-5, eIDAS 2.0 |

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
3. Separate trust evidence resolution from trust policy evaluation. The resolver gathers evidence; the policy evaluator decides whether that evidence is sufficient for a credential type and use case.
4. Start DID support with `did:web`; make other DID methods plugin-driven.
5. Treat EBSI and custom registries as versioned adapters, because registry APIs and governance rules evolve outside the library.
6. Use .NET `X509Chain` and caller-provided trust anchors for X.509/IACA validation where possible.
7. For QTSP checks, report verifiable evidence from trusted lists and certificate chains. Do not claim legal qualification unless the trust service type, certificate status, and chain validation all support that conclusion.

---

## Implementation scope

### Implement first

- `SdJwt.Net.Trust` core contracts and abstractions
- Trust evidence model
- Trust policy evaluator (separate from evidence resolution)
- Adapter registry with priority and conflict handling
- OpenID Federation adapter
- X.509/IACA adapter
- Custom registry adapter
- Cache policy with invalidation
- Negative test cases

### Deferred

- QTSP evidence resolver (helper, not core)
- EBSI adapter
- Broad DID method support beyond `did:web`

---

## Goals

1. Define a shared `ITrustResolver` contract and evidence-rich result model.
2. Define a separate `ITrustPolicyEvaluator` for trust policy decisions.
3. Wrap existing OpenID Federation and EUDIW trust resolution as adapters.
4. Add `did:web` issuer resolution.
5. Add X.509/IACA trust anchor validation for mdoc-oriented deployments.
6. Add custom registry adapter for enterprise registries.
7. Support trust purpose (trusted for what?) in resolution options.
8. Handle adapter conflicts with priority, required adapters, any-of/all-of policy, and negative evidence.

## Non-goals

- Operating a trust registry.
- Providing legal compliance advice.
- Certificate issuance or lifecycle management.
- Supporting every DID method in the first implementation.
- Claiming legal qualification status (QTSP) without full evidence chain.

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

public interface ITrustPolicyEvaluator
{
    Task<TrustPolicyResult> EvaluateAsync(
        TrustResolutionResult evidence,
        TrustPolicyOptions options,
        CancellationToken cancellationToken = default);
}

public interface ITrustFrameworkAdapter
{
    string FrameworkId { get; }
    int Priority { get; }

    bool CanResolve(TrustSubject subject, TrustResolutionOptions options);

    Task<TrustFrameworkResult> ResolveAsync(
        TrustSubject subject,
        TrustResolutionOptions options,
        CancellationToken cancellationToken = default);
}
```

#### Trust resolution options with purpose

```csharp
public sealed class TrustResolutionOptions
{
    public string? CredentialType { get; init; }
    public string? Purpose { get; init; } // issuance, verification, wallet_attestation, issuer_key, rp_registration
    public IReadOnlyList<string> RequiredFrameworks { get; init; } = [];
    public AdapterConflictPolicy ConflictPolicy { get; init; } = AdapterConflictPolicy.AnyOf;
}

public enum AdapterConflictPolicy
{
    AnyOf,
    AllOf,
    PriorityFirst
}
```

#### Trust resolution result

```csharp
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

Evidence types:

- `openid-federation-chain`
- `eu-trusted-list-service`
- `did-web-document`
- `x509-chain`
- `iaca-trust-anchor`
- `qtsp-qualified-certificate` (deferred)
- `registry-entry`

### Cache invalidation model

```csharp
public sealed class TrustCacheOptions
{
    public TimeSpan DefaultTtl { get; init; } = TimeSpan.FromHours(1);
    public bool RespectSourceExpiry { get; init; } = true;
    public bool FailOpenOnStaleCache { get; init; } = false;
    public bool AllowManualRefresh { get; init; } = true;
}
```

Cache must respect:

- `validUntil` / `nextUpdate` from source metadata
- Source-specific expiry signals
- Manual refresh triggers
- Fail-open/fail-closed policy on stale cache

### Adapter conflict handling

When two adapters disagree:

- **Adapter priority**: higher-priority adapter wins in `PriorityFirst` mode
- **Required adapters**: adapters in `RequiredFrameworks` must agree
- **Any-of / All-of policy**: configurable via `AdapterConflictPolicy`
- **Negative evidence**: adapter returning negative evidence is flagged in results
- **Stale evidence**: evidence past `ValidUntil` is flagged but not silently dropped

### Adapter phases

| Phase | Component                   | Scope                                                                                          |
| ----- | --------------------------- | ---------------------------------------------------------------------------------------------- |
| 1     | Core trust contracts        | `ITrustResolver`, `ITrustPolicyEvaluator`, adapter registry, result aggregation, cache options |
| 2     | Existing framework adapters | OpenID Federation adapter and EUDIW LOTL adapter                                               |
| 3     | DID Web adapter             | Resolve `did:web`, validate verification methods, cache DID Documents                          |
| 4     | X.509/IACA adapter          | Validate certificate chains against configured anchors                                         |
| 5     | Custom registry adapter     | Custom registry adapter for enterprise deployments                                             |
| 6     | Documentation and tests     | Integration examples, negative trust tests, cache expiry tests                                 |

### QTSP evidence resolver (deferred)

```csharp
public sealed class QtspEvidenceResolver
{
    public Task<QtspEvidenceResult> ResolveAsync(
        X509Certificate2 signingCertificate,
        QtspResolutionOptions options,
        CancellationToken cancellationToken = default);
}

public sealed class QtspEvidenceResult
{
    public bool HasQualifiedEvidence { get; init; }
    public string? TrustServiceProvider { get; init; }
    public string? TrustServiceType { get; init; }
    public IReadOnlyList<TrustEvidence> Evidence { get; init; } = [];
}
```

`HasQualifiedEvidence` must be `false` unless all required evidence is present and current.

---

## Security considerations

| Concern                 | Mitigation                                                               |
| ----------------------- | ------------------------------------------------------------------------ |
| Stale trust data        | Cache TTLs, explicit refresh, `ResolvedAt` and evidence validity windows |
| Trust list tampering    | Signature validation where the trust list format supports it             |
| Registry spoofing       | HTTPS-only endpoints plus configured issuer/registry allow-lists         |
| Certificate misuse      | Chain validation, key usage checks, validity period checks               |
| Overstated trust result | Evidence-first result model; no legal status without supporting evidence |
| Adapter conflict        | Configurable conflict policy; negative evidence surfaced in results      |

---

## Acceptance criteria

```text
Given two adapters with conflicting trust results,
when AllOf policy is configured,
then the overall result is not trusted.

Given an OpenID Federation trust chain that resolves successfully,
when the adapter returns positive evidence,
then the evidence includes chain metadata and validUntil.

Given a cached trust result past its validUntil,
when fail-closed policy is configured,
then the resolver re-fetches before returning a result.

Given a trust resolution with Purpose="issuance" and CredentialType="dc+sd-jwt",
when the adapter finds no matching credential type authorization,
then the result is not trusted.

Given an X.509 certificate chain with an expired intermediate,
when the IACA adapter validates the chain,
then the result includes negative evidence with the expiry details.
```

---

## Interop test requirements

- OpenID Federation: resolve a real trust chain from a test federation
- Negative test: untrusted issuer returns negative evidence
- Cache test: stale cache triggers re-fetch
- Conflict test: two adapters disagreeing handled per policy
- X.509 test: valid and expired certificate chains

---

## Estimated effort

| Component                                           | Effort      |
| --------------------------------------------------- | ----------- |
| Core contracts and resolver orchestrator            | 4 days      |
| OpenID Federation and EUDIW adapters                | 4 days      |
| DID Web adapter                                     | 3 days      |
| X.509/IACA adapter                                  | 4 days      |
| Custom registry adapter                             | 3 days      |
| Tests and documentation                             | 5 days      |
| **Subtotal (first implementation)**                 | **23 days** |
| QTSP evidence resolver (deferred)                   | 5 days      |
| Experimental EBSI-style registry adapter (deferred) | 3 days      |

---

## Related documentation

- [EUDIW](../concepts/eudiw.md)
- [HAIP](../concepts/haip.md)
- [Ecosystem Architecture](../concepts/ecosystem-architecture.md)
