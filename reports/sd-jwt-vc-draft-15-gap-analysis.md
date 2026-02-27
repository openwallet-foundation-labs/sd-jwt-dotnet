# SD-JWT VC Draft-15 Gap Analysis Report

## Scope
- Repository: `sd-jwt-dotnet`
- Specification: `specs/draft-ietf-oauth-sd-jwt-vc-15.txt`
- Analysis date: 2026-02-26
- Focus: `src/SdJwt.Net.Vc`, relevant core SD-JWT components, and related constants/docs/tests

## Executive Summary
- The implementation has been advanced to substantial draft-15 alignment for core issuance, verification, metadata, and integrity requirements.
- Core SD-JWT cryptographic verification is strong and reusable, but SD-JWT VC draft-15 specific requirements around metadata retrieval/validation and integrity are mostly not implemented.
- Several high-severity compliance gaps exist where required claims can be overridden/disclosed incorrectly and where key binding can be validated without `cnf`.

## Requirement Compliance Matrix
| Requirement (draft-15) | Status | Evidence | Gap Severity | Notes |
|---|---|---|---|---|
| 3.1 media type MUST be `application/dc+sd-jwt` (L323-324) | Partial | Constant exists: `SdJwtVcMediaType = application/dc+sd-jwt` in `src/SdJwt.Net/SdJwtConstants.cs:90` | Medium | OID4VCI/OID4VP constants still use `vc+sd-jwt`: `src/SdJwt.Net.Oid4Vci/Models/Oid4VciConstants.cs:12`, `src/SdJwt.Net.Oid4Vp/Models/Oid4VpConstants.cs:15`. |
| 3.2 data format MUST be SD-JWT; JWS JSON OPTIONAL (L343-345, L351-355) | Implemented | Issuance/presentation format in core issuer/parser/verifier supports compact and JSON serialization: `src/SdJwt.Net/Issuer/SdIssuer.cs:140-163`, `src/SdJwt.Net/Verifier/SdVerifier.cs:237-272` | Low | Base behavior aligns. |
| 3.2.1 `typ` MUST be `dc+sd-jwt` (L362-364) | Implemented | VC issuer sets type via `SdJwtConstants.SdJwtVcTypeName`: `src/SdJwt.Net.Vc/Issuer/SdJwtVcIssuer.cs:88`; verifier enforces `dc+sd-jwt`: `src/SdJwt.Net.Vc/Verifier/SdJwtVcVerifier.cs:74-80` | Low | Strict enforcement works. |
| Transitional recommendation: accept `vc+sd-jwt` and `dc+sd-jwt` (L382-384) | Implemented | Policy supports legacy typ acceptance (`AcceptLegacyTyp`, default true) in verifier typ validation | Low | RECOMMENDED requirement now supported via policy. |
| 3.2.2 `vct` REQUIRED and Collision-Resistant Name (L405-407, L530) | Implemented | Issuer and verifier validate collision-resistant form: `src/SdJwt.Net.Vc/Issuer/SdJwtVcIssuer.cs:181-183`, `src/SdJwt.Net.Vc/Verifier/SdJwtVcVerifier.cs:372-373` | Low | Behavior present, validation is simplified heuristic. |
| Non-disclosable registered claims MUST NOT be in disclosures (L510-511) | Implemented | VC issuer now rejects reserved claim overrides and protected disclosure selections in issuance path | Low | Guardrails added in `SdJwtVcIssuer` validation flow. |
| `cnf` REQUIRED when KB is supported; KB verification MUST use `cnf` (L524-529, L988-990) | Implemented | Core verifier now requires `cnf` when KB-JWT validation is performed and validates with `cnf.jwk` only | Low | Legacy permissive fallback removed. |
| If no selectively disclosable claims, MUST NOT include `_sd` or disclosures (L578-579) | Implemented | Core issuer removes `_sd` when empty and emits no disclosures: `src/SdJwt.Net/Issuer/SdIssuer.cs:90-95` | Low | Compliant behavior. |
| Recipient MUST process/verify per RFC9901 (L978-982) | Implemented | VC verifier delegates to base RFC9901 verifier: `src/SdJwt.Net.Vc/Verifier/SdJwtVcVerifier.cs:53` | Low | Strong base compliance. |
| Status SHOULD be checked when present (L996-998) | Partial | Verifier policy now supports required status checks with pluggable status validator (`ISdJwtVcStatusValidator`) | Medium | Hook implemented; concrete StatusList integration still pending. |
| Issuer key determination via permitted mechanism; reject if not valid (L1021-1051) | Partial | Provided externally via `issuerKeyProvider`; library does not implement mechanism selection/validation itself: `src/SdJwt.Net.Verifier/SdVerifier.cs:33-35,68-73` | Medium | Delegated model is flexible but not draft-15-complete out of the box. |
| JWT VC Issuer Metadata endpoint/content rules (Section 4, L1069-1134, L1188-1190) | Implemented | Added HTTP resolver + validation component and metadata-based signing key resolver (`JwtVcIssuerSigningKeyResolver`) with `iss`->metadata->JWKS selection integrated via `SdJwtVcVerifier` constructor overloads | Low | Supports inline `jwks` and remote `jwks_uri` with size/content checks. |
| Type Metadata format/retrieval/validation rules (Section 5, L1268+, L1304, L1317, L1368, L1954) | Partial | Added type metadata resolver with vct matching, extension traversal, cycle detection, and metadata constraints (`path`/`sd`/`svg_id`) | Medium | Full metadata merge semantics and all display/rendering rule checks still pending. |
| `vct#integrity` / integrity metadata MUST be validated when present (L1323, L1389-1392) | Implemented | Verifier now enforces `vct#integrity` with resolver-backed SRI validation when present | Low | Requires configured type metadata resolver. |
| Display/Claim metadata constraints (Section 7/8: locale/name/path/sd/svg_id constraints, SVG safety) | Partial | Claim metadata validation added for `path`, `sd` values, and `svg_id` format/uniqueness | Medium | SVG rendering safety controls still not implemented. |

## Additional Consistency Gaps
| Area | Status | Evidence | Severity |
|---|---|---|---|
| Package/docs still claim draft-13/14 | Implemented | Updated VC package and root README/.csproj references to draft-15 alignment | Low |
| Tests include permissive metadata assumptions | Inconsistent with draft-15 | Allows both `jwks` and `jwks_uri` in metadata test: `tests/SdJwt.Net.Vc.Tests/VcEnhancedTests.cs` (test name indicates allowance) | Medium |

## Implementation Plan to Close Gaps

### Workstream 1: Claim Safety and KB Strictness
Status: Completed
1. Add reserved-claim guardrail in `SdJwtVcIssuer.Issue` to reject `AdditionalData` keys that conflict with protected claims (`iss`, `nbf`, `exp`, `cnf`, `vct`, `vct#integrity`, `status`, `_sd`, `_sd_alg`).
2. Enforce non-disclosable claim policy by validating `SdIssuanceOptions.DisclosureStructure` against protected claim set before calling core issuer.
3. In core verifier, when KB is provided/required, require `cnf` claim and fail if key cannot be resolved from allowed confirmation methods.
4. Add negative tests for claim override/disclosure attempts and KB-without-cnf rejection.

### Workstream 2: JWT VC Issuer Metadata (Section 4)
Status: Completed
1. Introduce `IJwtVcIssuerMetadataResolver` and default HTTP implementation with strict HTTPS, DNS/IP safety checks, response size/time limits.
2. Implement well-known URL construction from `iss` and path normalization per Section 4.1.
3. Validate response (`issuer` equality with `iss`; exactly one of `jwks`/`jwks_uri`; JWK set shape validation).
4. Integrate resolver into VC verifier as optional built-in issuer key mechanism.

### Workstream 3: Type Metadata + Integrity (Sections 5/6/8)
Status: In Progress
1. Add `ITypeMetadataResolver` with retrieval methods (URL/direct/local cache abstraction).
2. Implement `vct` equality checks against credential and metadata reference.
3. Implement integrity metadata verification (`vct#integrity`, `extends#integrity`, `uri#integrity`) using SRI-compliant hashing/parsing.
4. Implement type-extension processing with cycle detection and conflict rules for `sd`/`mandatory` overrides.
5. Add validation APIs for claim metadata constraints (`path`, `sd` enum, `svg_id` rules).

### Workstream 4: Verifier Policy Surface
Status: In Progress
1. Add explicit `SdJwtVcVerificationPolicy` object: require status check, required metadata validation, allowed issuer signature mechanisms, legacy typ acceptance toggle.
2. Wire policy into `SdJwtVcVerifier.Verify*` methods and return structured diagnostics.
3. Add hooks/integrations for StatusList validation when `status` claim is present.

### Workstream 5: Ecosystem and Documentation Alignment
Status: In Progress
1. Update package/docs references from draft-13/14 to draft-15 where implemented.
2. Evaluate OpenID package format constants and align with current interoperability profile decisions (keep backward compatibility where needed).
3. Convert permissive tests to normative tests (e.g., reject both `jwks` and `jwks_uri`).

## Status Update (Current)
- Analysis completed: Done
- Gap classification completed: Done
- Remediation plan defined: Done
- Code changes for compliance: In progress
- Draft-15 conformance test suite: In progress

## Implementation Progress (2026-02-26)
- Completed: Workstream 1.1 reserved-claim override guard in VC issuer (`AdditionalData` conflict rejection).
- Completed: Workstream 1.2 disclosure policy guard for non-disclosable registered claims.
- Completed: Workstream 1.3 strict KB verification dependency on `cnf` in base verifier.
- Completed: Workstream 1.4 negative tests added for claim override/disclosure violations and KB without `cnf`.
- Completed: Workstream 2 core resolver and metadata validation components.
- Completed: Workstream 2.4 verifier constructor integration for metadata-based issuer signature key resolution.
- Completed: Workstream 3 core type resolver, extension-cycle detection, and SRI integrity validation.
- Completed: Workstream 4 policy object + verifier integration for metadata and status checks.
- Completed: Workstream 5.1 package/documentation references updated to draft-15.
- Completed: Workstream 5.2 OID4VCI/OID4VP SD-JWT VC format constants aligned to `dc+sd-jwt` with legacy compatibility.
- Remaining: Full SVG/display safety processing and final Workstream 5 test/doc cleanup.

## Recommended Delivery Sequence
1. Workstream 1 (security-sensitive correctness)
2. Workstream 3 (integrity and type metadata core)
3. Workstream 2 (issuer metadata retrieval/validation)
4. Workstream 4 (policy and API consolidation)
5. Workstream 5 (docs/constants/tests cleanup)
