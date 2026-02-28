# Enterprise Roadmap

## Document Information

| Field | Value |
|-------|-------|
| Version | 1.0.0 |
| Last Updated | 2026-02-28 |
| Status | Active |

## Executive Summary

The SD-JWT .NET ecosystem provides a production-ready, enterprise-grade implementation of the complete OpenID4VC stack for JWT-based verifiable credentials. This roadmap outlines the current state, validates enterprise readiness, and defines the strategic path to achieve full credential format coverage including ISO mDL/mdoc support.

## Current State Assessment

### Implementation Status

All core specifications are fully implemented with comprehensive test coverage:

| Package | Specification | Version | Status | Test Coverage |
|---------|--------------|---------|--------|---------------|
| `SdJwt.Net` | RFC 9901 (SD-JWT) | Final | Complete | 1483 tests |
| `SdJwt.Net.Vc` | SD-JWT VC | draft-15 | Complete | Included |
| `SdJwt.Net.StatusList` | Token Status List | draft-18 | Complete | Included |
| `SdJwt.Net.Oid4Vci` | OpenID4VCI | 1.0 Final | Complete | Included |
| `SdJwt.Net.Oid4Vp` | OpenID4VP | 1.0 | Complete | Included |
| `SdJwt.Net.PresentationExchange` | DIF PEX | v2.1.1 | Complete | Included |
| `SdJwt.Net.OidFederation` | OpenID Federation | 1.0 | Complete | Included |
| `SdJwt.Net.HAIP` | HAIP Profile | 1.0 | Complete | Included |

### Enterprise Readiness Checklist

| Category | Requirement | Status | Evidence |
|----------|-------------|--------|----------|
| **Security** | No weak cryptography (MD5/SHA-1) | Pass | HAIP validator blocks, tests verify rejection |
| **Security** | Constant-time comparisons | Pass | `CryptographicOperations.FixedTimeEquals` used throughout |
| **Security** | CSPRNG for random generation | Pass | `RandomNumberGenerator` used for all entropy |
| **Security** | Replay attack prevention | Pass | Nonce and `iat` freshness validation enforced |
| **Compliance** | RFC 9901 conformance | Pass | Gap analysis completed, all issues remediated |
| **Compliance** | SD-JWT VC draft-15 conformance | Pass | Gap analysis completed, all issues remediated |
| **Compliance** | OpenID4VC suite conformance | Pass | Gap analysis completed, all issues remediated |
| **Quality** | Zero compiler warnings | Pass | `TreatWarningsAsErrors=true` enforced |
| **Quality** | XML documentation on all public APIs | Pass | `GenerateDocumentationFile=true` |
| **Quality** | Comprehensive test suite | Pass | 1483 tests, all passing |
| **Quality** | Multi-framework support | Pass | .NET 8.0, 9.0, 10.0, netstandard2.1 |
| **Operations** | CI/CD pipeline | Pass | GitHub Actions with quality gates |
| **Operations** | Automated releases | Pass | Release Please with draft review |
| **Operations** | NuGet publishing | Pass | Trusted Publishing (OIDC) |

### Gap Analysis Reports

Detailed remediation work is documented in:

- [RFC 9901 Gap Analysis](../reports/rfc9901-gap-analysis.md)
- [SD-JWT VC Draft-15 Gap Analysis](../reports/sd-jwt-vc-draft-15-gap-analysis.md)
- [OpenID4VC Suite Gap Analysis](../reports/openid4vc-suite-gap-analysis.md)

## Strategic Roadmap

### Phase 1: Foundation Hardening (Q1-Q2 2026) - COMPLETE

**Objective**: Ensure all JWT-based credential flows are production-ready.

| Deliverable | Status | Notes |
|-------------|--------|-------|
| RFC 9901 strict compliance | Complete | All MUST requirements implemented |
| SD-JWT VC draft-15 alignment | Complete | Type/integrity metadata validation |
| Status List draft-18 support | Complete | Multi-bit values, freshness validation |
| OpenID4VCI 1.0 Final implementation | Complete | Proof validation, batch credentials, notifications |
| OpenID4VP 1.0 implementation | Complete | JAR, transaction data binding, KB validation |
| DIF PEX v2.1.1 implementation | Complete | Submission requirements, predicate filters |
| OpenID Federation 1.0 implementation | Complete | Trust chain resolution, metadata policies |
| HAIP 1.0 compliance | Complete | Level 1/2/3 validation, wallet attestation |

### Phase 2: ISO mDL/mdoc Support (Q2-Q3 2026) - PLANNED

**Objective**: Add support for ISO 18013-5 mobile document credentials.

**Justification**:

- Government-issued identity documents (driver's licenses, national IDs) use ISO mDL format
- OpenID4VP and OpenID4VCI specs explicitly define `mso_mdoc` credential format
- Required for EU Digital Identity Wallet (EUDIW) compliance
- Critical for enterprise identity verification use cases

**Package**: `SdJwt.Net.Mdoc` (new)

| Component | Description | Priority |
|-----------|-------------|----------|
| CBOR serialization | ISO 18013-5 CBOR data structures | Critical |
| Mobile Security Object (MSO) | Issuer-signed credential structure | Critical |
| DeviceResponse handling | Presentation format for mdoc | Critical |
| SessionTranscript | CBOR-encoded session binding | Critical |
| OpenID4VPHandover | OID4VP integration per spec | Critical |
| mdoc verifier integration | Extend `VpTokenValidator` for mdoc | Critical |
| mdoc credential issuance | Extend OID4VCI for `mso_mdoc` | High |
| Namespace/element mapping | ISO 18013-5 namespace handling | High |

**Dependencies**:

- CBOR library (e.g., `PeterO.Cbor` or similar)
- COSE signature validation

**Estimated Effort**: 8-12 weeks

### Phase 3: W3C Digital Credentials API Integration (Q3 2026) - PLANNED

**Objective**: Enable browser-based wallet interactions via the Digital Credentials API.

**Justification**:

- W3C specification reaching Candidate Recommendation
- Enables web applications to request credentials without custom protocols
- Chrome, Edge, and Safari implementing native support
- Critical for consumer-facing wallet applications

**Package**: Extend `SdJwt.Net.Oid4Vp`

| Component | Description | Priority |
|-----------|-------------|----------|
| `dc_api` response mode | Support `response_mode=dc_api` | High |
| `dc_api.jwt` response mode | Encrypted response support | High |
| Request object formatting | DC API compatible request structure | High |
| Response parsing | Handle DC API response envelope | Medium |
| Browser integration samples | JavaScript + .NET backend examples | Medium |

**Estimated Effort**: 4-6 weeks

### Phase 4: eIDAS 2.0 / EUDIW Profile (Q3-Q4 2026) - PLANNED

**Objective**: Provide ready-to-use configuration for EU Digital Identity Wallet compliance.

**Justification**:

- EU Regulation 2024/1183 mandates EUDIW adoption by 2026
- Enterprises operating in EU must comply with eIDAS 2.0
- EUDIW Architecture Reference Framework (ARF) defines specific requirements
- Builds on existing HAIP foundation with EU-specific constraints

**Package**: `SdJwt.Net.Eudiw` (new)

| Component | Description | Priority |
|-----------|-------------|----------|
| ARF profile validator | Enforce ARF-specific requirements | High |
| Trust framework integration | EU trust list validation | High |
| EU credential types | PID, mDL, attestations | High |
| Qualified attestation support | QEAA handling | Medium |
| Relying party registration | EU RP registration validation | Medium |

**Dependencies**:

- Phase 2 (mdoc support) must be complete
- EU trust list infrastructure access

**Estimated Effort**: 6-8 weeks

### Phase 5: Token Introspection Enhancement (Q4 2026) - PLANNED

**Objective**: Add real-time token status checking via OAuth 2.0 Token Introspection.

**Justification**:

- Some enterprises prefer real-time status over cached status lists
- Complements existing Status List support
- Required for high-frequency, low-latency verification scenarios
- Supports hybrid status checking strategies

**Package**: Extend `SdJwt.Net.StatusList`

| Component | Description | Priority |
|-----------|-------------|----------|
| Introspection endpoint client | OAuth 2.0 introspection request | Medium |
| Response parsing | Active/inactive status handling | Medium |
| Hybrid status strategy | Combine status list + introspection | Medium |
| Caching policies | Optimize introspection calls | Low |

**Estimated Effort**: 2-3 weeks

## Prioritization Matrix

| Phase | Priority | Business Impact | Regulatory Driver | Dependencies |
|-------|----------|-----------------|-------------------|--------------|
| Phase 1 | P0 (Complete) | Foundation for all VC use cases | RFC/OpenID compliance | None |
| Phase 2 | P0 | Government ID, travel, age verification | ISO 18013-5, EUDIW | None |
| Phase 3 | P1 | Consumer web applications | W3C standardization | None |
| Phase 4 | P1 | EU market access | eIDAS 2.0 | Phase 2 |
| Phase 5 | P2 | Real-time verification optimization | None | None |

## Governance

### Versioning

This project follows [Semantic Versioning 2.0.0](https://semver.org/):

- **Major**: Breaking API changes
- **Minor**: New features, backward compatible
- **Patch**: Bug fixes, backward compatible

Version management is automated via MinVer with Git tags.

### Release Process

1. Contributors submit PRs with [Conventional Commits](https://www.conventionalcommits.org/)
2. Release Please generates draft release PR with changelog
3. Maintainers review and merge release PR
4. NuGet packages published automatically via Trusted Publishing

### Contribution Guidelines

See [CONTRIBUTING.md](../CONTRIBUTING.md) for detailed guidelines.

## Risk Assessment

| Risk | Likelihood | Impact | Mitigation |
|------|------------|--------|------------|
| CBOR library compatibility | Low | High | Evaluate multiple libraries, create abstraction |
| EU trust list availability | Medium | Medium | Implement fallback mechanisms |
| DC API spec changes | Medium | Low | Track W3C working group, abstract integration |
| Breaking spec changes | Low | High | Version-specific implementations, deprecation policy |

## Success Metrics

| Metric | Target | Measurement |
|--------|--------|-------------|
| Test coverage | Maintain 1400+ tests | CI pipeline |
| Build status | Zero warnings | `TreatWarningsAsErrors` |
| Documentation | All public APIs documented | CS1591 warnings = 0 |
| Release cadence | Monthly minor releases | Release Please analytics |
| Community adoption | NuGet download growth | NuGet statistics |

## Appendix: Specification References

| Specification | Location | Version |
|---------------|----------|---------|
| RFC 9901 (SD-JWT) | [specs/rfc9901.txt](../specs/rfc9901.txt) | Final |
| SD-JWT VC | [specs/draft-ietf-oauth-sd-jwt-vc-15.txt](../specs/draft-ietf-oauth-sd-jwt-vc-15.txt) | draft-15 |
| Token Status List | [specs/draft-ietf-oauth-status-list-18.txt](../specs/draft-ietf-oauth-status-list-18.txt) | draft-18 |
| OpenID4VCI | [specs/openid-4-verifiable-credential-issuance-1_0-final.md](../specs/openid-4-verifiable-credential-issuance-1_0-final.md) | 1.0 Final |
| OpenID4VP | [specs/openid-4-verifiable-presentations-1_0.md](../specs/openid-4-verifiable-presentations-1_0.md) | 1.0 |
| DIF PEX | [specs/presentation-exchange 2.1.1.md](../specs/presentation-exchange%202.1.1.md) | v2.1.1 |
| OpenID Federation | [specs/openid-federation-1_0.txt](../specs/openid-federation-1_0.txt) | 1.0 |
| HAIP | [specs/openid4vc-haip-1_0.txt](../specs/openid4vc-haip-1_0.txt) | 1.0 |
| ISO 18013-5 | External (ISO purchase required) | 2021 |
| eIDAS 2.0 | External (EU Official Journal) | 2024/1183 |
