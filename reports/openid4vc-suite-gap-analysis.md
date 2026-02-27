# OpenID4VCI/OID4VP/PEX/HAIP/Federation Gap Analysis

## Scope
- Repository: `sd-jwt-dotnet`
- Specs reviewed:
  - `specs/openid-4-verifiable-credential-issuance-1_0-final.md`
  - `specs/openid-4-verifiable-presentations-1_0.md`
  - `specs/presentation-exchange 2.1.1.md`
  - `specs/openid4vc-haip-1_0.txt`
  - `specs/openid-federation-1_0.txt`
- Analysis date: 2026-02-27

## Executive Summary
- The codebase now has stronger runtime conformance across OID4VCI, OID4VP request object handling, HAIP runtime checks, and OpenID Federation `crit` enforcement.
- Remaining work is concentrated in deeper Presentation Exchange semantic fidelity, richer OID4VP runtime bindings (`transaction_data`, `wallet_nonce`, `verifier_info`), and full OpenID Federation metadata policy processing.

## Compliance Matrix (Key Requirements)

### 1) OpenID4VCI 1.0 Final
| Requirement Area | Status | Evidence | Severity |
|---|---|---|---|
| Credential Offer by reference (`credential_offer_uri`) | Implemented (with async fetch path) | Async retrieval support with HTTPS/content-type/size checks via `CredentialOfferParser.ParseAsync(...)`; sync parser intentionally rejects `credential_offer_uri` and directs callers to async path | Medium |
| JWT proof header key mechanisms (`kid`/`jwk`/`x5c`, key attestation/trust chain handling) | Partial | `CNonceValidator.ValidateProof(...)` now supports `jwk`, `x5c`, and `kid` via resolver callback; end-to-end trust chain / attestation policy remains external | Medium |
| Proof type model supports modern `proofs` object/arrays | Partial | `CredentialProof` is singular object only (`src/SdJwt.Net.Oid4Vci/Models/CredentialProof.cs`) | High |
| Cryptographically secure nonce generation | Implemented | `GenerateNonce` now uses `RandomNumberGenerator` with rejection sampling in `CNonceValidator` | Low |
| Wallet/key attestation flows from spec appendices | Not implemented | No production attestation processing pipeline in OID4VCI package; only generic models/validators | High |
| Core token/credential request/response shape and validation | Implemented (baseline) | Models + validators in `TokenRequest`, `TokenResponse`, `CredentialRequest`, `CredentialResponse` | Medium |

### 2) OpenID4VP 1.0
| Requirement Area | Status | Evidence | Severity |
|---|---|---|---|
| Request Object/JAR handling (`typ=oauth-authz-req+jwt`, JWT parsing) | Partial | Parser now supports compact JAR request objects and enforces `typ=oauth-authz-req+jwt`; signature trust validation is still caller responsibility | Medium |
| `request_uri_method=post` behavior | Implemented (baseline) | `ParseFromRequestUriAsync` supports both GET and POST retrieval with HTTPS/fragment/size checks | Medium |
| `dcql_query` support with exclusivity checks | Implemented (model-level) | `AuthorizationRequest.Validate` enforces DCQL vs PE exclusivity (`.../Models/AuthorizationRequest.cs:351-374`) | Medium |
| `transaction_data`, `wallet_nonce`, `verifier_info` normative processing/binding | Not implemented | Fields exist but no processing logic in validator/parser paths | High |
| VP token KB checks (nonce/audience/freshness) | Partial-strong | `VpTokenValidator` validates KB presence/freshness/audience options (`.../Verifier/VpTokenValidator.cs`) | Medium |
| `direct_post.jwt` and DC API mode requirements (esp. HAIP profile) | Partial | Constants include `direct_post.jwt`, but builder defaults to `direct_post`; no DC API transport implementation | Medium |

### 3) Presentation Exchange 2.1.1
| Requirement Area | Status | Evidence | Severity |
|---|---|---|---|
| Submission requirement semantics (`from` group, `from_nested`, strict rule behavior) | Partial/misaligned | OID4VP model uses `From` as descriptor IDs array, diverging from group-based semantics (`src/SdJwt.Net.Oid4Vp/Models/SubmissionRequirement.cs`) | High |
| JSONPath compliance breadth | Partial | Custom simplified evaluator (`src/SdJwt.Net.PresentationExchange/Services/JsonPathEvaluator.cs`) | Medium |
| JSON Schema Draft 7 filtering fidelity | Partial | `FieldFilterEvaluator` has simplified object/property checks (`.../Services/FieldFilterEvaluator.cs:362-365`) | Medium |
| Limit disclosure behavior | Partial | Engine now extracts a minimized disclosure set for SD-JWT credentials based on constraint paths; advanced JSONPath-to-disclosure mapping remains limited | Medium |
| Predicate/relational/status features | Partial | Models exist; end-to-end proving/derivation semantics are incomplete in engine pipeline | Medium |
| Presentation submission generation/mapping | Partial | Generated paths are generic and not spec-profile aware (`.../Engine/PresentationExchangeEngine.cs:438`) | Medium |

### 4) OpenID4VC HAIP 1.0
| Requirement Area | Status | Evidence | Severity |
|---|---|---|---|
| HAIP profile enforcement for OID4VCI/OID4VP flows | Partial (mostly scaffolding) | Reflection-based option setting on `object` (`src/SdJwt.Net.HAIP/Extensions/HaipExtensions.cs`) | High |
| Runtime protocol validation (DPoP, wallet attestation, transport, HSM) | Partial | Placeholder fail-paths replaced with runtime heuristic checks in `HaipProtocolValidator`; production-grade attestation verification still needs protocol-bound integrations | High |
| Level 3 sovereign HSM validation | Partial | `HaipCryptoValidator.CheckECKeyHSMBacking` now has heuristic detection (kid/type hints); hardware-backed proof remains environment-dependent | Medium |
| Algorithm and key-strength policy checks | Implemented (core) | `HaipCryptoValidator` validates allowed/forbidden algorithms and key sizes | Medium |
| Test reliability for HAIP protocol validator | Weak | Test file defines local mock validator classes, not the production class (`tests/SdJwt.Net.HAIP.Tests/Validators/HaipValidatorTests.cs:103+`) | High |

### 5) OpenID Federation 1.0
| Requirement Area | Status | Evidence | Severity |
|---|---|---|---|
| Entity configuration/statement type, issuer/subject, signature checks | Implemented (baseline) | `TrustChainResolver` validates `typ`, `iss/sub`, lifetime, signature (`.../Logic/TrustChainResolver.cs`) | Medium |
| Metadata policy resolution/application across chain | Not implemented | Models exist; no full policy application pipeline in resolver | High |
| `crit` claim processing requirements | Implemented (baseline) | `TrustChainResolver` now enforces supported critical JWT header parameters and critical claim presence/knownness for entity statements | Medium |
| Federation endpoint breadth (list/resolve/trust mark status/etc.) | Partial | Resolver only fetches well-known + fetch endpoint; broader endpoints absent | Medium |
| Cache/DoS hardening behavior from options | Partial | Options exist; no full caching strategy implementation visible in resolver path | Medium |
| Test confidence for successful trust-chain cases | Weak | Multiple tests intentionally expect failure due environment/validation complexity (`tests/SdJwt.Net.OidFederation.Tests/Logic/TrustChainResolverTests.cs`) | Medium |

## Cross-Cutting Risks
- A number of packages provide strong DTO/spec-shape coverage without complete protocol behavior required for conformance claims.
- Several tests validate serialization and constructors more than normative behavior.
- Some critical security-sensitive behavior is placeholder/simulated in HAIP and partial in OID4VCI proof processing.

## Implementation Plan

### Phase 1 - Security-Critical Gaps (Priority: P0)
| Work Item | Target Packages | Status |
|---|---|---|
| Replace insecure nonce generation with CSPRNG and add tests | `SdJwt.Net.Oid4Vci` | Completed |
| Implement full proof header key resolution (`kid` + trust mechanism, `x5c`), and enforce key-attestation compatibility checks | `SdJwt.Net.Oid4Vci` | In Progress |
| Add support for `credential_offer_uri` fetch + HTTPS/media-type validation + cache controls | `SdJwt.Net.Oid4Vci` | Completed (async path) |
| Replace HAIP protocol placeholders with real request-context validators (DPoP, wallet attestation, transport, HSM policy) | `SdJwt.Net.HAIP` | In Progress |

### Phase 2 - OID4VP Request/Response Conformance (Priority: P1)
| Work Item | Target Packages | Status |
|---|---|---|
| Add JAR request-object parsing/validation (`typ`, signature, claims consistency) | `SdJwt.Net.Oid4Vp` | In Progress |
| Implement `request_uri_method=post` retrieval path and capability payload | `SdJwt.Net.Oid4Vp` | Completed |
| Add normative processing for `transaction_data`, `wallet_nonce`, `verifier_info` | `SdJwt.Net.Oid4Vp` | Planned |
| Harden presentation submission-to-definition validation in VP response validator | `SdJwt.Net.Oid4Vp` | Planned |

### Phase 3 - Presentation Exchange Semantics (Priority: P1)
| Work Item | Target Packages | Status |
|---|---|---|
| Align submission requirement model with group-based `from` semantics and nested rules | `SdJwt.Net.PresentationExchange`, `SdJwt.Net.Oid4Vp` | Planned |
| Upgrade JSONPath and JSON Schema processing fidelity to PE 2.1.1 expectations | `SdJwt.Net.PresentationExchange` | Planned |
| Implement actual disclosure minimization and predicate-output behavior | `SdJwt.Net.PresentationExchange` | Planned |

### Phase 4 - Federation Policy Engine (Priority: P1)
| Work Item | Target Packages | Status |
|---|---|---|
| Implement metadata policy merge/apply operators and chain-time resolution | `SdJwt.Net.OidFederation` | Planned |
| Implement `crit` claim enforcement and unknown-critical rejection | `SdJwt.Net.OidFederation` | Completed (baseline) |
| Extend endpoint coverage and caching/trust-chain refresh strategy | `SdJwt.Net.OidFederation` | Planned |

### Phase 5 - Test Suite Hardening (Priority: P0/P1)
| Work Item | Target Packages | Status |
|---|---|---|
| Replace placeholder/mocked-in-file HAIP tests with real validator integration tests | `tests/SdJwt.Net.HAIP.Tests` | Planned |
| Add conformance-focused negative tests for VCI/VP/PEX/Federation MUST-fail scenarios | all affected test projects | Planned |
| Add deterministic interop fixtures for request/response objects and proof validation | `tests/*` | Planned |

## Status Update
- Analysis completed: Done
- Gap classification: Done
- Fix plan drafted: Done
- Code remediation started: In progress
- Conformance test expansion started: In progress

## Implementation Progress (2026-02-27)
- Completed: OID4VCI nonce generation hardened to CSPRNG in `CNonceValidator.GenerateNonce`.
- Completed: OID4VCI `credential_offer_uri` support via `CredentialOfferParser.ParseAsync` with HTTPS/content-type/size checks and tests.
- Completed: OID4VCI proof header key support expanded to `jwk`, `x5c`, and `kid` resolver-based validation.
- Completed: OID4VP request-object parser adds compact JAR handling (`typ=oauth-authz-req+jwt`) and `request_uri_method=post` support.
- Completed: OpenID Federation resolver now enforces critical header/claim semantics.
- In progress: HAIP runtime checks are now non-placeholder heuristics; full production attestation/trust integrations remain.
- In progress: Presentation Exchange disclosure minimization improved from placeholder to path-based selection; advanced semantic coverage remains.

## Recommended Execution Order
1. Phase 1 (P0 security and mandatory protocol behavior)
2. Phase 2 (OID4VP request-object correctness)
3. Phase 3 (Presentation Exchange semantic correctness)
4. Phase 4 (Federation policy processing)
5. Phase 5 (test suite and conformance hardening in parallel after each phase)
