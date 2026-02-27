# OpenID4VCI/OID4VP/PEX/HAIP/Federation Gap Analysis

## Scope
- Repository: `sd-jwt-dotnet`
- Specs reviewed:
  - `specs/openid-4-verifiable-credential-issuance-1_0-final.md`
  - `specs/openid-4-verifiable-presentations-1_0.md`
  - `specs/presentation-exchange 2.1.1.md`
  - `specs/openid4vc-haip-1_0.txt`
  - `specs/openid-federation-1_0.txt`
- Analysis date: 2026-02-26

## Executive Summary
- The codebase has strong model coverage and partial protocol validation, but significant spec-critical gaps remain in runtime behavior for OID4VCI, OID4VP request object handling, full Presentation Exchange semantics, HAIP enforcement, and OpenID Federation policy processing.
- Current maturity is best described as: `Foundational implementation + validation helpers`, not full end-to-end normative conformance for the five specs above.

## Compliance Matrix (Key Requirements)

### 1) OpenID4VCI 1.0 Final
| Requirement Area | Status | Evidence | Severity |
|---|---|---|---|
| Credential Offer by reference (`credential_offer_uri`) | Not implemented | `src/SdJwt.Net.Oid4Vci/Client/CredentialOfferParser.cs:82-85` throws `NotSupportedException` | High |
| JWT proof header key mechanisms (`kid`/`jwk`/`x5c`, key attestation/trust chain handling) | Partial | Validation only supports `jwk`; `kid` path explicitly unsupported in `src/SdJwt.Net.Oid4Vci/Issuer/CNonceValidator.cs:238-244` | High |
| Proof type model supports modern `proofs` object/arrays | Partial | `CredentialProof` is singular object only (`src/SdJwt.Net.Oid4Vci/Models/CredentialProof.cs`) | High |
| Cryptographically secure nonce generation | Not compliant | `GenerateNonce` uses `Random` in `src/SdJwt.Net.Oid4Vci/Issuer/CNonceValidator.cs:204-206` | High |
| Wallet/key attestation flows from spec appendices | Not implemented | No production attestation processing pipeline in OID4VCI package; only generic models/validators | High |
| Core token/credential request/response shape and validation | Implemented (baseline) | Models + validators in `TokenRequest`, `TokenResponse`, `CredentialRequest`, `CredentialResponse` | Medium |

### 2) OpenID4VP 1.0
| Requirement Area | Status | Evidence | Severity |
|---|---|---|---|
| Request Object/JAR handling (`typ=oauth-authz-req+jwt`, JWT parsing) | Partial | Parser assumes JSON request object; no JAR JWT validation path (`src/SdJwt.Net.Oid4Vp/Client/AuthorizationRequestParser.cs`) | High |
| `request_uri_method=post` behavior | Not implemented | `ParseFromRequestUriAsync` always GETs (`...AuthorizationRequestParser.cs:89-93`); no POST capability exchange | High |
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
| Limit disclosure behavior | Partial | Evaluated at constraint level, but engine disclosure extraction is placeholder returning `null` (`.../Engine/PresentationExchangeEngine.cs:384-395`) | High |
| Predicate/relational/status features | Partial | Models exist; end-to-end proving/derivation semantics are incomplete in engine pipeline | Medium |
| Presentation submission generation/mapping | Partial | Generated paths are generic and not spec-profile aware (`.../Engine/PresentationExchangeEngine.cs:438`) | Medium |

### 4) OpenID4VC HAIP 1.0
| Requirement Area | Status | Evidence | Severity |
|---|---|---|---|
| HAIP profile enforcement for OID4VCI/OID4VP flows | Partial (mostly scaffolding) | Reflection-based option setting on `object` (`src/SdJwt.Net.HAIP/Extensions/HaipExtensions.cs`) | High |
| Runtime protocol validation (DPoP, wallet attestation, transport, HSM) | Not production-ready | Multiple explicit placeholders and simulated failures in `HaipProtocolValidator` | Critical |
| Level 3 sovereign HSM validation | Not implemented | Hardcoded non-HSM result (`HaipCryptoValidator.CheckECKeyHSMBacking` returns false) | High |
| Algorithm and key-strength policy checks | Implemented (core) | `HaipCryptoValidator` validates allowed/forbidden algorithms and key sizes | Medium |
| Test reliability for HAIP protocol validator | Weak | Test file defines local mock validator classes, not the production class (`tests/SdJwt.Net.HAIP.Tests/Validators/HaipValidatorTests.cs:103+`) | High |

### 5) OpenID Federation 1.0
| Requirement Area | Status | Evidence | Severity |
|---|---|---|---|
| Entity configuration/statement type, issuer/subject, signature checks | Implemented (baseline) | `TrustChainResolver` validates `typ`, `iss/sub`, lifetime, signature (`.../Logic/TrustChainResolver.cs`) | Medium |
| Metadata policy resolution/application across chain | Not implemented | Models exist; no full policy application pipeline in resolver | High |
| `crit` claim processing requirements | Not implemented | No explicit crit enforcement in resolver validation path | High |
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
| Replace insecure nonce generation with CSPRNG and add tests | `SdJwt.Net.Oid4Vci` | Planned |
| Implement full proof header key resolution (`kid` + trust mechanism, `x5c`), and enforce key-attestation compatibility checks | `SdJwt.Net.Oid4Vci` | Planned |
| Add support for `credential_offer_uri` fetch + HTTPS/media-type validation + cache controls | `SdJwt.Net.Oid4Vci` | Planned |
| Replace HAIP protocol placeholders with real request-context validators (DPoP, wallet attestation, transport, HSM policy) | `SdJwt.Net.HAIP` | Planned |

### Phase 2 - OID4VP Request/Response Conformance (Priority: P1)
| Work Item | Target Packages | Status |
|---|---|---|
| Add JAR request-object parsing/validation (`typ`, signature, claims consistency) | `SdJwt.Net.Oid4Vp` | Planned |
| Implement `request_uri_method=post` retrieval path and capability payload | `SdJwt.Net.Oid4Vp` | Planned |
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
| Implement `crit` claim enforcement and unknown-critical rejection | `SdJwt.Net.OidFederation` | Planned |
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
- Code remediation started: Not started
- Conformance test expansion started: Not started

## Recommended Execution Order
1. Phase 1 (P0 security and mandatory protocol behavior)
2. Phase 2 (OID4VP request-object correctness)
3. Phase 3 (Presentation Exchange semantic correctness)
4. Phase 4 (Federation policy processing)
5. Phase 5 (test suite and conformance hardening in parallel after each phase)
