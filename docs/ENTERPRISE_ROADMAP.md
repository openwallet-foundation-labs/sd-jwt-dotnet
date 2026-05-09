# Enterprise Capability & Roadmap

| Field        | Value      |
| ------------ | ---------- |
| Version      | 3.0.0      |
| Last Updated | 2026-05-09 |
| Status       | Active     |

## Executive summary

The SD-JWT .NET ecosystem is a standards-first library ecosystem providing stable core packages, reference infrastructure, and preview extensions for the OpenID4VC verifiable-credentials stack. 21 NuGet packages plus an ASP.NET Core issuer reference server. 2,500+ xUnit tests. RFC 9901, OpenID4VC, SIOPv2, W3C VCDM 2.0, ISO 18013-5, eIDAS 2.0, Agent Trust. See [MATURITY.md](../MATURITY.md) for per-package stability classifications.

This document covers the current capabilities, specification alignment, and planned roadmap for trust infrastructure, credential lifecycle management, and regional alignment.

For the full feature matrix, see [Capability Matrix](reference/reference/capabilities.md).

---

## Current capabilities

### Implementation status

Core specifications are covered by 2,500+ xUnit tests:

| Package                              | Specification       | Version   | Maturity      | Delivery Status |
| ------------------------------------ | ------------------- | --------- | ------------- | --------------- |
| `SdJwt.Net`                          | RFC 9901 (SD-JWT)   | Final     | Stable        | Implemented     |
| `SdJwt.Net.Vc`                       | SD-JWT VC           | draft-16  | Spec-tracking | Implemented     |
| `SdJwt.Net.VcDm`                     | W3C VCDM 2.0        | 2.0       | Stable        | Implemented     |
| `SdJwt.Net.SiopV2`                   | SIOPv2              | draft-13  | Spec-tracking | Implemented     |
| `SdJwt.Net.StatusList`               | Token Status List   | draft-20  | Spec-tracking | Implemented     |
| `SdJwt.Net.Oid4Vci`                  | OpenID4VCI          | 1.0 Final | Stable        | Implemented     |
| `SdJwt.Net.Oid4Vci.AspNetCore`       | OpenID4VCI Server   | 1.0 Final | Reference     | Implemented     |
| `SdJwt.Net.Oid4Vp`                   | OpenID4VP           | 1.0       | Stable        | Implemented     |
| `SdJwt.Net.PresentationExchange`     | DIF PEX             | v2.1.1    | Stable        | Implemented     |
| `SdJwt.Net.OidFederation`            | OpenID Federation   | 1.0       | Stable        | Implemented     |
| `SdJwt.Net.HAIP`                     | HAIP Profile        | 1.0       | Stable        | Implemented     |
| `SdJwt.Net.Mdoc`                     | ISO 18013-5         | 2021      | Stable        | Implemented     |
| `SdJwt.Net.Wallet`                   | OWF Wallet Arch     | -         | Reference     | Implemented     |
| `SdJwt.Net.Eudiw`                    | eIDAS 2.0 / ARF     | 2024/1183 | Reference     | Implemented     |
| `SdJwt.Net.AgentTrust.Core`          | Agent Trust Core    | -         | Preview       | Implemented     |
| `SdJwt.Net.AgentTrust.Policy`        | Agent Trust Policy  | -         | Preview       | Implemented     |
| `SdJwt.Net.AgentTrust.AspNetCore`    | Agent Trust ASP.NET | -         | Preview       | Implemented     |
| `SdJwt.Net.AgentTrust.Maf`           | Agent Trust MAF     | -         | Preview       | Implemented     |
| `SdJwt.Net.AgentTrust.OpenTelemetry` | Agent Trust OTel    | -         | Preview       | Implemented     |
| `SdJwt.Net.AgentTrust.Policy.Opa`    | Agent Trust OPA     | -         | Preview       | Implemented     |
| `SdJwt.Net.AgentTrust.Mcp`           | Agent Trust MCP     | -         | Preview       | Implemented     |
| `SdJwt.Net.AgentTrust.A2A`           | Agent Trust A2A     | -         | Preview       | Implemented     |

### Specification alignment checklist

| Category           | Requirement                          | Status | Evidence                                                  |
| ------------------ | ------------------------------------ | ------ | --------------------------------------------------------- |
| **Security**       | No weak cryptography (MD5/SHA-1)     | Pass   | HAIP validator blocks, tests verify rejection             |
| **Security**       | Constant-time comparisons            | Pass   | `CryptographicOperations.FixedTimeEquals` used throughout |
| **Security**       | CSPRNG for random generation         | Pass   | `RandomNumberGenerator` used for all entropy              |
| **Security**       | Replay attack prevention             | Pass   | Nonce and `iat` freshness validation enforced             |
| **Spec alignment** | RFC 9901 alignment                   | Pass   | Internal gap analysis completed, all issues remediated    |
| **Spec alignment** | SD-JWT VC draft-16 alignment         | Pass   | Internal gap analysis completed, all issues remediated    |
| **Spec alignment** | OpenID4VC suite alignment            | Pass   | Internal gap analysis completed, all issues remediated    |
| **Quality**        | Zero compiler warnings               | Pass   | `TreatWarningsAsErrors=true` enforced                     |
| **Quality**        | XML documentation on all public APIs | Pass   | `GenerateDocumentationFile=true`                          |
| **Quality**        | 2,500+ xUnit test suite              | Pass   | Covered by CI                                             |
| **Quality**        | Multi-framework support              | Pass   | .NET 8.0, 9.0, 10.0, netstandard2.1                       |
| **Operations**     | CI/CD pipeline                       | Pass   | GitHub Actions with quality gates                         |
| **Operations**     | Automated releases                   | Pass   | Release Please with draft review                          |
| **Operations**     | NuGet publishing                     | Pass   | Trusted Publishing (OIDC)                                 |

### Internal gap analysis reports

Detailed remediation work is documented in:

- [RFC 9901 Gap Analysis](../reports/rfc9901-gap-analysis.md)
- [SD-JWT VC Draft-16 Gap Analysis](../reports/sd-jwt-vc-draft-16-gap-analysis.md)
- [OpenID4VC Suite Gap Analysis](../reports/openid4vc-suite-gap-analysis.md)

### OWF Architecture Alignment

Wallet infrastructure interfaces align with the OWF Universal Wallet Reference Architecture:

| Component                          | Description                                             | Status      |
| ---------------------------------- | ------------------------------------------------------- | ----------- |
| `IKeyLifecycleManager`             | Key rotation and filtered listing beyond `IKeyManager`  | Implemented |
| `ICredentialInventory`             | PEX-aware credential matching (`FindMatchingAsync`)     | Implemented |
| `InMemoryTransactionLogger`        | Reference `ITransactionLogger` implementation           | Implemented |
| `CredentialAuditEntry`             | Credential-level audit with disclosed claims tracking   | Implemented |
| `StatusListDocumentStatusResolver` | Bridges `StatusListVerifier` to wallet status interface | Implemented |
| `IAssuranceProfile`                | Pluggable assurance profile hook on `WalletOptions`     | Implemented |

---

## Roadmap

### Phase 1: Foundation Hardening -- Implemented

Strict specification alignment for all JWT-based credential flows.

| Deliverable                          | Maturity      | Status      |
| ------------------------------------ | ------------- | ----------- |
| RFC 9901 strict alignment            | Stable        | Implemented |
| SD-JWT VC draft-16 alignment         | Spec-tracking | Implemented |
| Status List draft-20 support         | Spec-tracking | Implemented |
| OpenID4VCI 1.0 Final implementation  | Stable        | Implemented |
| OpenID4VP 1.0 implementation         | Stable        | Implemented |
| DIF PEX v2.1.1 implementation        | Stable        | Implemented |
| OpenID Federation 1.0 implementation | Stable        | Implemented |
| HAIP 1.0 profile validation          | Stable        | Implemented |

### Phase 2: ISO mDL/mdoc Support -- Implemented

Format-level primitives for ISO 18013-5 mobile document credentials.

**Package**: `SdJwt.Net.Mdoc`

| Component                    | Description                                      | Status      |
| ---------------------------- | ------------------------------------------------ | ----------- |
| CBOR serialization           | ISO 18013-5 CBOR data structures via PeterO.Cbor | Implemented |
| COSE cryptography            | COSE_Sign1 operations (RFC 8152)                 | Implemented |
| Mobile Security Object (MSO) | Issuer-signed credential structure               | Implemented |
| DeviceResponse handling      | Presentation format for mdoc                     | Implemented |
| SessionTranscript            | CBOR-encoded session binding                     | Implemented |
| OpenID4VPHandover            | OID4VP integration (redirect + DC API)           | Implemented |
| mdoc verifier                | `MdocVerifier` for document validation           | Implemented |
| mdoc issuer                  | `MdocIssuerBuilder` fluent API                   | Implemented |
| mDL namespace support        | org.iso.18013.5.1 standard elements              | Implemented |
| ICoseCryptoProvider          | Pluggable cryptographic abstraction              | Implemented |

Design reference: [mdoc concepts](concepts/mdoc.md), [mdoc Identity Verification](reference-patterns/mdoc-identity-verification.md)

### Phase 3: W3C Digital Credentials API -- Implemented (Spec-tracking)

Browser-based wallet interactions via the Digital Credentials API. The DC API is a W3C Working Draft; this implementation tracks the current specification and will be updated as the standard evolves.

**Package**: Extends `SdJwt.Net.Oid4Vp`

| Component                  | Description                       | Status      |
| -------------------------- | --------------------------------- | ----------- |
| `DcApiRequestBuilder`      | DC API compatible request builder | Implemented |
| `DcApiResponseValidator`   | Response parsing and validation   | Implemented |
| `dc_api` response mode     | Support `response_mode=dc_api`    | Implemented |
| `dc_api.jwt` response mode | Encrypted response support        | Implemented |
| `DcApiOriginValidator`     | Browser origin vs client_id       | Implemented |
| `OpenId4VpDcApiHandover`   | DC API session transcript         | Implemented |

Design reference: [DC API concepts](concepts/dc-api.md), [DC API + OID4VP example](examples/browser/dc-api-oid4vp-verifier.md)

### Phase 4: eIDAS 2.0 / EUDIW ARF Reference Helpers -- Implemented

ARF-oriented reference helpers for PID/mDL validation patterns and EU trust-list integration points. This package provides reference implementations to help teams prepare for EUDIW ecosystem integration as the underlying EU infrastructure becomes available.

**Package**: `SdJwt.Net.Eudiw`

| Component                 | Description                                              | Status      |
| ------------------------- | -------------------------------------------------------- | ----------- |
| `ArfProfileValidator`     | Validate configured ARF-oriented reference policy        | Implemented |
| `IEuTrustListResolver`    | Reference integration point for EU trust list validation | Implemented |
| `PidCredentialHandler`    | PID credential processing                                | Implemented |
| `QeaaHandler`             | Reference integration point for QEAA handling            | Implemented |
| `RpRegistrationValidator` | Reference integration point for EU RP registration       | Implemented |

The `ArfProfileValidator` uses `ArfValidationResult.IsValid` to indicate whether a credential satisfies the configured validation policy. For detailed API design, see the [EUDIW concepts](concepts/eudiw.md).

Design reference: [EUDIW concepts](concepts/eudiw.md)

### Phase 5: Agent Trust Preview Extensions -- Implemented

Observability, external policy engines, MCP protocol integration, and agent-to-agent delegation. These are preview extensions; APIs, token formats, and policy schemas may change. Agent Trust is a project-defined pattern, not an external standard.

| Component                      | Description                                    | Status      |
| ------------------------------ | ---------------------------------------------- | ----------- |
| Workload Identity Binding      | Bind capability tokens to workload identities  | Implemented |
| Sender Constraint (DPoP/mTLS)  | Proof-of-possession for capability tokens      | Implemented |
| JWKS Key Resolver (HTTP)       | Fetch trusted issuer keys from JWKS endpoints  | Implemented |
| OpenTelemetry Metrics          | Counters, histograms for token ops and policy  | Implemented |
| Telemetry Receipt Writer       | Emit structured receipts as OTel metrics       | Implemented |
| OPA Policy Engine              | Externalize policy evaluation via OPA HTTP API | Implemented |
| MCP Client Trust Interceptor   | Attach capability tokens to MCP tool calls     | Implemented |
| MCP Server Trust Guard         | Verify capability tokens on MCP tool execution | Implemented |
| A2A Delegation Chain Validator | Validate ordered delegation token chains       | Implemented |
| A2A Delegation Token Issuer    | Mint delegation tokens with depth enforcement  | Implemented |

**Packages**: `SdJwt.Net.AgentTrust.OpenTelemetry`, `SdJwt.Net.AgentTrust.Policy.Opa`, `SdJwt.Net.AgentTrust.Mcp`, `SdJwt.Net.AgentTrust.A2A`

### Phase 6: Token Introspection -- Implemented (Optional)

Optional real-time token status checking via OAuth 2.0 Token Introspection as a complement to privacy-preserving Status Lists. Introspection reveals to the issuer which credential is being verified and when; prefer Status Lists as the primary status mechanism.

**Implementation**: `SdJwt.Net.StatusList` includes `TokenIntrospectionClient`, `ITokenIntrospectionClient`, `HybridStatusChecker`, and related tests.

### Phase 7: Presentation Delivery -- Proposed

Samples and helper utilities for QR code and deep link delivery of OID4VP requests, and multi-credential bundle sessions.

| Deliverable                      | Proposal                                                            |
| -------------------------------- | ------------------------------------------------------------------- |
| QR Code & Deep Link transport    | [Delivery via QR & Deep Links](proposals/delivery-qr-deep-links.md) |
| Multi-credential OID4VP sessions | [Multi-Credential Sessions](proposals/bundles-batch-credentials.md) |

### Phase 8: Credential Status & Lifecycle Workflows -- Proposed

Programmatic revocation/suspension APIs, dynamic validity, and Bitstring Status List v1.0. Wallet-side status polling is optional and may be deferred.

| Deliverable                                | Proposal                                                                    |
| ------------------------------------------ | --------------------------------------------------------------------------- |
| Lifecycle controls + Bitstring Status List | [Credential Lifecycle Controls](proposals/credential-lifecycle-controls.md) |

### Phase 9: Trust Infrastructure -- Proposed

Pluggable trust resolver abstraction and trust registry integration. QTSP integration depends on the availability of production-ready EU trust list infrastructure.

| Deliverable                           | Proposal                                                      |
| ------------------------------------- | ------------------------------------------------------------- |
| Pluggable trust resolver & registries | [Trust Registries & QTSP](proposals/trust-registries-qtsp.md) |

### Phase 10: Regional Alignment -- Deferred

Extension framework (`IAssuranceProfile`) for regional profile hooks. Concrete regional profiles will be added as the underlying specifications stabilize and ecosystem demand materializes.

| Deliverable                 | Proposal                                                                |
| --------------------------- | ----------------------------------------------------------------------- |
| Assurance profile framework | [Assurance Profiles](proposals/deferred/regional-assurance-profiles.md) |

---

## Prioritization matrix

| Phase    | Delivery Status | Business Impact                            | Regulatory Driver    | Dependencies |
| -------- | --------------- | ------------------------------------------ | -------------------- | ------------ |
| Phase 1  | Implemented     | Foundation for all VC use cases            | RFC/OpenID alignment | None         |
| Phase 2  | Implemented     | Government ID, travel, age verification    | ISO 18013-5          | None         |
| Phase 3  | Implemented     | Browser-based credential presentation      | W3C Working Draft    | None         |
| Phase 4  | Implemented     | EU market reference infrastructure         | eIDAS 2.0            | Phase 2      |
| Phase 5  | Implemented     | Agent trust observability and protocols    | Project design       | Phase 4      |
| Phase 6  | Implemented     | Real-time verification (privacy trade-off) | None                 | None         |
| Phase 7  | Proposed        | UX for cross-device and batch flows        | OpenID4VP transport  | Phase 1      |
| Phase 8  | Proposed        | Credential lifecycle governance            | Bitstring v1.0       | Phase 1      |
| Phase 9  | Proposed        | Pluggable trust resolution                 | eIDAS 2.0, EBSI      | Phase 4      |
| Phase 10 | Deferred        | Extension framework for regional profiles  | National frameworks  | Phase 9      |

---

## Out of scope

The following are explicitly out of scope for this project:

| Item                                | Rationale                                                                             |
| ----------------------------------- | ------------------------------------------------------------------------------------- |
| Certified EUDI Wallet               | Certification is a deployment concern, not a library concern                          |
| Legal compliance assessment         | Legal advice is outside the scope of an SDK project                                   |
| Browser-level DC API implementation | Browser vendors implement the DC API; this library is the backend                     |
| NFC/BLE device engagement           | Proximity transport is a device concern; this library handles data-level verification |
| Production key management           | Use Azure Key Vault, AWS KMS, or HSMs; the library provides interfaces                |
| Certified mDL reader                | ISO 18013-5 reader certification is a deployment/regulatory concern                   |
| Agent Trust standardization         | Agent Trust is a project-defined pattern; standardization is deferred                 |

---

## Success metrics

### Quality and coverage

| Metric                  | Target                                  | Measurement              |
| ----------------------- | --------------------------------------- | ------------------------ |
| Test coverage           | Maintain 2,500+ xUnit tests             | CI pipeline              |
| Build status            | Zero warnings                           | `TreatWarningsAsErrors`  |
| Documentation           | All public APIs documented              | CS1591 warnings = 0      |
| Release cadence         | Monthly minor releases                  | Release Please analytics |
| Spec alignment coverage | Gap analysis delta shrinks each release | Gap analysis reports     |

### Interoperability

| Metric                 | Target                                      | Measurement              |
| ---------------------- | ------------------------------------------- | ------------------------ |
| Ecosystem integration  | Interop with at least 2 external wallets    | Manual integration tests |
| Cross-platform support | Consistent behavior across .NET 8/9/10      | Multi-TFM CI matrix      |
| Standards test vectors | Pass published test vectors where available | Automated test suites    |

### Developer experience

| Metric                  | Target                                     | Measurement                 |
| ----------------------- | ------------------------------------------ | --------------------------- |
| Package install to demo | Under 10 minutes for core flow             | Tutorial walkthrough        |
| API discoverability     | XML docs + IntelliSense on all public APIs | `GenerateDocumentationFile` |
| Example coverage        | Runnable example for each major capability | docs/examples/ index        |
| Community adoption      | NuGet download growth                      | NuGet statistics            |

---

## Governance

### Versioning

This project follows [Semantic Versioning 2.0.0](https://semver.org/). Version management is automated via MinVer with Git tags.

### Release process

1. Contributors submit PRs with [Conventional Commits](https://www.conventionalcommits.org/)
2. Release Please generates draft release PR with changelog
3. Maintainers review and merge release PR
4. NuGet packages published automatically via Trusted Publishing

### Contribution guidelines

See [CONTRIBUTING.md](../CONTRIBUTING.md) for detailed guidelines.

---

## Risk assessment

| Risk                       | Likelihood | Impact | Mitigation                                                                                    |
| -------------------------- | ---------- | ------ | --------------------------------------------------------------------------------------------- |
| EU trust list availability | Medium     | Medium | Implement fallback mechanisms; EUDIW package is a reference integration point                 |
| DC API spec changes        | High       | Medium | Spec is Working Draft; track W3C working group, abstract integration, expect breaking changes |
| Breaking spec changes      | Low        | High   | Version-specific implementations, deprecation policy                                          |
| OWF governance alignment   | Medium     | Medium | Track TAC lifecycle, maintain interop with OWF labs                                           |

---

## Appendix: Specification references

| Specification     | Location                                                                                                                    | Version   |
| ----------------- | --------------------------------------------------------------------------------------------------------------------------- | --------- |
| RFC 9901 (SD-JWT) | [specs/rfc9901.txt](../specs/rfc9901.txt)                                                                                   | Final     |
| SD-JWT VC         | [specs/draft-ietf-oauth-sd-jwt-vc-16.txt](../specs/draft-ietf-oauth-sd-jwt-vc-16.txt)                                       | draft-16  |
| Token Status List | [Token Status List](https://datatracker.ietf.org/doc/draft-ietf-oauth-status-list/)                                         | draft-20  |
| OpenID4VCI        | [specs/openid-4-verifiable-credential-issuance-1_0-final.md](../specs/openid-4-verifiable-credential-issuance-1_0-final.md) | 1.0 Final |
| OpenID4VP         | [specs/openid-4-verifiable-presentations-1_0.md](../specs/openid-4-verifiable-presentations-1_0.md)                         | 1.0       |
| DIF PEX           | [specs/presentation-exchange 2.1.1.md](../specs/presentation-exchange%202.1.1.md)                                           | v2.1.1    |
| OpenID Federation | [specs/openid-federation-1_0.txt](../specs/openid-federation-1_0.txt)                                                       | 1.0       |
| HAIP              | [specs/openid4vc-haip-1_0.txt](../specs/openid4vc-haip-1_0.txt)                                                             | 1.0       |
| ISO 18013-5       | External (ISO purchase required)                                                                                            | 2021      |
| eIDAS 2.0         | External (EU Official Journal)                                                                                              | 2024/1183 |
