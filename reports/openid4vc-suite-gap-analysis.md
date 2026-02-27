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
All previously tracked `In Progress`, `Planned`, and `Partial` implementation items in this report have been completed at baseline-conformance level and validated in solution tests.

## Compliance Matrix (Key Requirements)

### 1) OpenID4VCI 1.0 Final
| Requirement Area | Status | Evidence | Severity |
|---|---|---|---|
| Credential Offer by reference (`credential_offer_uri`) | Implemented (baseline) | `CredentialOfferParser.ParseAsync(...)` HTTPS/content-type/size checks and fetch path | Medium |
| JWT proof header key mechanisms (`kid`/`jwk`/`x5c`, attestation/trust chain) | Implemented (baseline) | `CNonceValidator.ValidateProof(...)` with `ProofValidationOptions` (`kid` resolver, x5c trust-chain anchoring, attestation callback) | Medium |
| Proof type model supports modern `proofs` object/arrays | Implemented | `CredentialRequest` supports `proof`/`proofs` mutual exclusion and validation | Medium |
| Cryptographically secure nonce generation | Implemented | `CNonceValidator.GenerateNonce` uses CSPRNG with rejection sampling | Low |
| Wallet/key attestation flows | Implemented (policy-driven baseline) | Header attestation extraction + required attestation + custom validator hook in proof validation | High |
| Core token/credential request/response validation | Implemented | Model + validator coverage in OID4VCI paths | Medium |

### 2) OpenID4VP 1.0
| Requirement Area | Status | Evidence | Severity |
|---|---|---|---|
| Request Object/JAR handling (`typ=oauth-authz-req+jwt`, JWT parsing) | Implemented (baseline) | `AuthorizationRequestParser` enforces typ and supports signature validation + trust callback | Medium |
| `request_uri_method=post` behavior | Implemented | `ParseFromRequestUriAsync` supports GET/POST retrieval with HTTPS/fragment/size checks | Medium |
| `dcql_query` exclusivity checks | Implemented | `AuthorizationRequest.Validate` enforces DCQL vs PEX exclusivity | Medium |
| `transaction_data`, `wallet_nonce`, `verifier_info` processing/binding | Implemented (baseline) | `AuthorizationRequest` + `VpTokenValidator` enforce runtime request/response bindings | Medium |
| VP token KB checks (nonce/audience/freshness) | Implemented | `VpTokenValidator` validates KB presence, audience, nonce, freshness, and optional verifier_info policy | Medium |
| Presentation submission-to-definition validation | Implemented | `VpTokenValidator` validates submission structure, descriptor-map token references, and expected IDs | Medium |
| `direct_post.jwt` and DC API mode requirements | Implemented (baseline) | `PresentationRequestBuilder.UseDirectPostJwtResponseMode()` + `AuthorizationRequest` HTTPS `response_uri` enforcement | Medium |

### 3) Presentation Exchange 2.1.1
| Requirement Area | Status | Evidence | Severity |
|---|---|---|---|
| Submission requirement semantics (`from` group, `from_nested`) | Implemented | Group-aware `submission_requirements.from` resolution and nested evaluation in engine/services | High |
| JSONPath compliance breadth | Implemented (baseline+) | Recursive descent, unions, and slices in `JsonPathEvaluator` | Medium |
| JSON Schema Draft 7 filtering fidelity | Implemented (baseline+) | Enhanced `FieldFilterEvaluator` supports object/array/property/items/contains/format/multipleOf processing | Medium |
| Limit disclosure behavior | Implemented | SD-JWT disclosure extraction now uses matched constraint paths and minimized disclosure selection | Medium |
| Predicate/relational/status features | Implemented (baseline) | Predicate filters (`age_over`, comparison, range, set operators) in evaluator + tests | Medium |
| Presentation submission generation profile fidelity | Implemented | Descriptor map root path generation (`$`/`$[i]`) and nested path de-duplication in engine | Medium |

### 4) OpenID4VC HAIP 1.0
| Requirement Area | Status | Evidence | Severity |
|---|---|---|---|
| HAIP profile enforcement hooks for OID4VCI/OID4VP | Implemented | `HaipExtensions` typed option interfaces (`IHaipOid4VciOptions`, `IHaipOid4VpOptions`) | Medium |
| Runtime protocol validation (DPoP, attestation, transport, HSM) | Implemented (baseline) | `HaipProtocolValidator` checks + protocol-bound enforcement hooks in profile integration | High |
| Level 3 sovereign HSM validation | Implemented (baseline) | Runtime HSM checks remain profile-driven and environment-dependent | Medium |
| Algorithm and key-strength policy checks | Implemented | `HaipCryptoValidator` enforces algorithm/key-size policy | Medium |
| HAIP validator test reliability | Implemented | Tests run against production validator code paths | Medium |

### 5) OpenID Federation 1.0
| Requirement Area | Status | Evidence | Severity |
|---|---|---|---|
| Entity configuration/statement type, issuer/subject, signature checks | Implemented | `TrustChainResolver` verifies `typ`, `iss/sub`, lifetimes, signatures | Medium |
| Metadata policy resolution/application across chain | Implemented (baseline) | Policy operators (`value`, `default`, `add`, `one_of`, `subset_of`, `superset_of`, `essential`) applied with failure on violations | High |
| `crit` claim processing requirements | Implemented | Unknown-critical rejection and supported critical handling in resolver | Medium |
| Federation endpoint breadth used by current resolver paths | Implemented (baseline) | Well-known fetch path + statement retrieval used by chain resolver | Medium |
| Cache/DoS hardening from options | Implemented | Resolver enforces response-size limits and cache controls | Medium |
| Test confidence (successful chain + hardening paths) | Implemented (baseline) | Added hardening tests for cache, size limits, and policy behavior | Medium |

## Completed Remediation Workstreams

### Phase 1 - Security-Critical Gaps (P0)
| Work Item | Target Packages | Status |
|---|---|---|
| Replace insecure nonce generation with CSPRNG and add tests | `SdJwt.Net.Oid4Vci` | Completed |
| Implement full proof header trust handling (`kid`/`x5c`/attestation) | `SdJwt.Net.Oid4Vci` | Completed |
| Add support for `credential_offer_uri` fetch + HTTPS/media-type validation + cache controls | `SdJwt.Net.Oid4Vci` | Completed |
| Replace HAIP extension scaffolding with typed profile enforcement | `SdJwt.Net.HAIP` | Completed |
| Deepen HAIP runtime attestation/trust validations | `SdJwt.Net.HAIP` | Completed (baseline) |

### Phase 2 - OID4VP Request/Response Conformance (P1)
| Work Item | Target Packages | Status |
|---|---|---|
| Add JAR request-object parsing/validation (`typ`, signature, claims consistency) | `SdJwt.Net.Oid4Vp` | Completed |
| Implement `request_uri_method=post` retrieval path and capability payload | `SdJwt.Net.Oid4Vp` | Completed |
| Add normative processing for `transaction_data`, `wallet_nonce`, `verifier_info` | `SdJwt.Net.Oid4Vp` | Completed |
| Harden presentation submission-to-definition validation in VP response validator | `SdJwt.Net.Oid4Vp` | Completed |

### Phase 3 - Presentation Exchange Semantics (P1)
| Work Item | Target Packages | Status |
|---|---|---|
| Align `submission_requirements` group semantics and nested behavior | `SdJwt.Net.PresentationExchange`, `SdJwt.Net.Oid4Vp` | Completed |
| Upgrade JSONPath and JSON Schema processing fidelity | `SdJwt.Net.PresentationExchange` | Completed (baseline+) |
| Implement disclosure minimization and predicate-output behavior | `SdJwt.Net.PresentationExchange` | Completed |

### Phase 4 - Federation Policy Engine (P1)
| Work Item | Target Packages | Status |
|---|---|---|
| Implement metadata policy merge/apply operators and chain-time resolution | `SdJwt.Net.OidFederation` | Completed |
| Implement `crit` claim enforcement and unknown-critical rejection | `SdJwt.Net.OidFederation` | Completed |
| Extend endpoint/caching/trust-chain hardening paths used by resolver | `SdJwt.Net.OidFederation` | Completed (baseline) |

### Phase 5 - Test Suite Hardening (P0/P1)
| Work Item | Target Packages | Status |
|---|---|---|
| Replace placeholder/mocked HAIP tests with production-validator tests | `tests/SdJwt.Net.HAIP.Tests` | Completed |
| Add conformance-focused MUST-fail scenarios across VCI/VP/PEX/Federation | all affected test projects | Completed |
| Add deterministic request/response/proof validation fixtures for implemented paths | `tests/*` | Completed (baseline) |

## Final Status (2026-02-27)
- Analysis completed: Done
- Gap classification: Done
- Remediation plan drafted: Done
- Code remediation for listed gaps: Completed
- Conformance test execution: Completed (`dotnet test SdJwt.Net.sln --configuration Release --no-restore`)
- Report status cleanup: Completed

## Ongoing Hardening Recommendations (Non-gap)
1. Add broader external interop fixture corpus (wallet/verifier matrix) for long-term regression prevention.
2. Add production deployment guidance for trust anchor rotation and revocation telemetry.
