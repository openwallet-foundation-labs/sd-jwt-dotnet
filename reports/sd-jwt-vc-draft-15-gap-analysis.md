# SD-JWT VC Draft-15 Gap Analysis Report

## Scope

- Repository: `sd-jwt-dotnet`
- Specification: `specs/draft-ietf-oauth-sd-jwt-vc-15.txt`
- Analysis date: 2026-02-27
- Focus: `src/SdJwt.Net.Vc`, related core SD-JWT components, metadata/integrity/status integration, and tests

## Executive Summary

All previously tracked `In Progress`, `Planned`, and `Partial` items in this draft-15 report have been completed at baseline-conformance level and validated by tests.

## Requirement Compliance Matrix

| Requirement (draft-15)                                                      | Status                 | Evidence                                                                                                           | Gap Severity | Notes                                                                      |
| --------------------------------------------------------------------------- | ---------------------- | ------------------------------------------------------------------------------------------------------------------ | ------------ | -------------------------------------------------------------------------- |
| 3.1 media type MUST be `application/dc+sd-jwt`                              | Implemented            | Core + OID4VCI/OID4VP format constants aligned with legacy compatibility support                                   | Low          | Legacy acceptance retained for interoperability migration.                 |
| 3.2 data format MUST be SD-JWT; JWS JSON OPTIONAL                           | Implemented            | Compact + JSON serialization support in issuer/parser/verifier paths                                               | Low          | Base behavior aligns.                                                      |
| 3.2.1 `typ` MUST be `dc+sd-jwt`                                             | Implemented            | VC issuer sets `typ`; verifier enforces `dc+sd-jwt` with policy for legacy acceptance                              | Low          | Strict-by-default plus compatibility toggle.                               |
| Transitional recommendation: accept `vc+sd-jwt` and `dc+sd-jwt`             | Implemented            | `AcceptLegacyTyp` policy path in verifier                                                                          | Low          | RECOMMENDED behavior supported.                                            |
| 3.2.2 `vct` REQUIRED and collision-resistant                                | Implemented            | Issuer/verifier enforce required `vct` and collision-resistant validation                                          | Low          | Validation implemented in VC issuer/verifier checks.                       |
| Non-disclosable registered claims MUST NOT be in disclosures                | Implemented            | VC issuer guards against reserved claim disclosure/override                                                        | Low          | Protected-claim guardrails enforced.                                       |
| `cnf` REQUIRED when KB is supported; KB verification MUST use `cnf`         | Implemented            | Core verifier enforces `cnf` for KB validation and removes permissive fallback                                     | Low          | Strict KB dependency enforced.                                             |
| If no selectively disclosable claims, MUST NOT include `_sd` or disclosures | Implemented            | Core issuer removes empty `_sd` and emits no disclosure list                                                       | Low          | Compliant serialization behavior.                                          |
| Recipient MUST process/verify per RFC9901                                   | Implemented            | VC verifier delegates to RFC9901 verifier core path                                                                | Low          | Core cryptographic validation reused.                                      |
| Status SHOULD be checked when present                                       | Implemented            | Policy hook + concrete `StatusListSdJwtVcStatusValidator` integration                                              | Medium       | Runtime status validation now concrete and test-covered.                   |
| Issuer key determination via permitted mechanism; reject if invalid         | Implemented (baseline) | Key resolution via resolver contracts + metadata-based signing key resolver path                                   | Medium       | Pluggable model supports approved mechanisms in library integration paths. |
| JWT VC Issuer Metadata endpoint/content rules (Section 4)                   | Implemented            | Resolver validates issuer equality, exclusivity of `jwks`/`jwks_uri`, key material shape, size/content constraints | Low          | Built-in metadata-based signing key resolution integrated.                 |
| Type Metadata format/retrieval/validation rules (Section 5)                 | Implemented            | Resolver validates `vct`, extension chains, cycles, claim/path/sd/svg_id constraints                               | Medium       | Includes extension compatibility checks.                                   |
| `vct#integrity` / integrity metadata MUST be validated when present         | Implemented            | Verifier enforces integrity via resolver-backed validation                                                         | Low          | Fail-closed when integrity check fails.                                    |
| Display/Claim metadata constraints (Section 7/8)                            | Implemented            | Locale/name/label/color/path/svg checks + SVG active-content blocking + remote resource integrity validation       | Medium       | Remote integrity and safety checks are enforced in resolver pipeline.      |

## Additional Consistency Items

| Area                                       | Status      | Evidence                                                                 | Severity |
| ------------------------------------------ | ----------- | ------------------------------------------------------------------------ | -------- |
| Package/docs draft alignment (13/14 -> 15) | Implemented | VC package/readme/report references aligned to draft-15                  | Low      |
| Normative metadata validation tests        | Implemented | Tests enforce MUST-fail scenarios (e.g., `jwks` and `jwks_uri` conflict) | Low      |

## Completed Workstreams

### Workstream 1: Claim Safety and KB Strictness

Status: Completed

1. Reserved-claim override guardrails in `SdJwtVcIssuer`.
2. Disclosure-structure protection for non-disclosable registered claims.
3. Strict KB verification dependency on `cnf`.
4. Negative tests for claim override/disclosure and KB-without-`cnf` cases.

### Workstream 2: JWT VC Issuer Metadata (Section 4)

Status: Completed

1. Resolver abstractions and HTTP metadata resolver with HTTPS/content/size checks.
2. Well-known metadata URL construction and issuer consistency validation.
3. `jwks`/`jwks_uri` exclusivity and JWK material validation.
4. Verifier integration through metadata-based signing key resolver.

### Workstream 3: Type Metadata + Integrity (Sections 5/6/8)

Status: Completed

1. Type metadata resolver + local cache abstraction support.
2. `vct` consistency checks against metadata.
3. Integrity validation for `vct#integrity`, `extends#integrity`, and `uri#integrity`.
4. Extension traversal, cycle detection, and compatibility conflict checks.
5. Display/rendering metadata validation including remote rendering integrity and SVG safety.

### Workstream 4: Verifier Policy Surface

Status: Completed

1. `SdJwtVcVerificationPolicy` integration for status/type/legacy-typ controls.
2. Policy wiring across verifier entry points.
3. Concrete StatusList validator implementation and integration tests.

### Workstream 5: Ecosystem and Documentation Alignment

Status: Completed

1. Draft-15 docs/constants updates.
2. OID4VCI/OID4VP SD-JWT VC constant alignment with backward compatibility.
3. Normative test hardening for metadata and rendering integrity constraints.

## Final Status (2026-02-27)

- Analysis completed: Done
- Gap classification completed: Done
- Remediation plan defined: Done
- Code changes for listed draft-15 gaps: Completed
- Draft-15 conformance test execution for implemented features: Completed (`dotnet test SdJwt.Net.sln --configuration Release --no-restore`)
- Report status cleanup: Completed

## Ongoing Hardening Recommendations (Non-gap)

1. Expand external interoperability fixtures for additional ecosystem metadata hosts and rendering resources.
2. Add long-horizon regression fixtures for metadata chain churn and trust-anchor rotation scenarios.
