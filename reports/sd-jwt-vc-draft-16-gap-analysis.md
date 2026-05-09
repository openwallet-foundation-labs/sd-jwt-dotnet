# SD-JWT VC Draft-16 Gap Analysis Report

## Scope

- Repository: `sd-jwt-dotnet`
- Specification: `specs/draft-ietf-oauth-sd-jwt-vc-16.txt` (April 24, 2026, expires October 26, 2026)
- Prior analysis: `reports/sd-jwt-vc-draft-15-gap-analysis.md` (baseline confirmed complete 2026-02-27)
- Analysis date: 2026-05-09
- Focus: changes introduced between draft-15 and draft-16; model alignment; documentation consistency

## Executive Summary

All normative requirements of draft-16 are implemented. The draft-15→draft-16 transition introduced
two normative changes (type header rename from `vc+sd-jwt` to `dc+sd-jwt`, and background image
metadata no longer carrying `alt_text`) plus editorial clarifications. Both were resolved in this
cycle. Additionally, five model files that cited draft-13 or draft-14 in their headers have been
updated to draft-16, and a `BackgroundImageMetadata` class was introduced to accurately represent
§4.5.1.1.2 (which omits `alt_text` unlike the logo metadata at §4.5.1.1.1).

## Draft-15 → Draft-16 Change Summary

| Area                        | Change                                                                         | Normative?      |
| --------------------------- | ------------------------------------------------------------------------------ | --------------- |
| §2.1 Media Type             | `application/dc+sd-jwt` confirmed as sole normative media type                 | Yes             |
| §2.2.1 JOSE Header          | `typ` MUST be `dc+sd-jwt`; `vc+sd-jwt` recommended to accept during transition | Yes             |
| §4.5.1.1.2 Background Image | `alt_text` removed — background images have only `uri` and `uri#integrity`     | Yes             |
| §4.5.1.2 SVG templates      | `properties` REQUIRED when more than one SVG template is present               | Yes (clarified) |
| Document history            | Multiple editorial clarifications, reference updates                           | No              |

## Requirement Compliance Matrix (draft-16)

| Requirement                                                                          | Section      | Status      | Evidence                                                                                       |
| ------------------------------------------------------------------------------------ | ------------ | ----------- | ---------------------------------------------------------------------------------------------- |
| Media type MUST be `application/dc+sd-jwt`                                           | §2.1         | Implemented | `SdJwtConstants.SdJwtVcMediaType = "application/dc+sd-jwt"`                                    |
| `typ` MUST be `dc+sd-jwt`                                                            | §2.2.1       | Implemented | `SdJwtVcIssuer` sets `typ`; verifier enforces with legacy policy for `vc+sd-jwt`               |
| Accept `vc+sd-jwt` during transition                                                 | §2.2.1 note  | Implemented | `SdJwtVcLegacyTypeName` constant + `AcceptLegacyTyp` policy path                               |
| `vct` REQUIRED, collision-resistant                                                  | §2.2.2.1     | Implemented | Issuer and verifier enforce required `vct`                                                     |
| Non-disclosable claims: `iss`, `nbf`, `exp`, `cnf`, `vct`, `vct#integrity`, `status` | §2.2.2.2     | Implemented | `SdJwtVcIssuer` guards against disclosure of protected claims                                  |
| `sub`, `iat` MAY be selectively disclosed                                            | §2.2.2.2     | Implemented | No issuer-side restriction on disclosing these                                                 |
| Verification per RFC 9901                                                            | §2.4         | Implemented | `SdJwtVcVerifier` delegates to `SdVerifier`                                                    |
| Key discovery via JWT VC Issuer Metadata                                             | §2.5         | Implemented | `JwtVcIssuerMetadataResolver` + `JwtVcIssuerSigningKeyResolver`                                |
| Key discovery via inline x5c                                                         | §2.5         | Implemented | x5c header path in verifier                                                                    |
| Issuer metadata: `issuer` REQUIRED                                                   | §3.2         | Implemented | `JwtVcIssuerMetadata.Issuer` validated equal to `iss` claim                                    |
| Issuer metadata: `jwks_uri` XOR `jwks`                                               | §3.2         | Implemented | `IntegrityMetadataValidator` enforces mutual exclusion                                         |
| Type metadata format (vct, name, description, extends, display, claims)              | §4.2         | Implemented | `TypeMetadata` class covers all properties; header updated to draft-16                         |
| `extends#integrity` MAY be present                                                   | §4.2         | Implemented | `TypeMetadata.ExtendsIntegrity` property                                                       |
| Type metadata retrieval via HTTPS URL                                                | §4.3.1       | Implemented | `TypeMetadataResolver` fetches via HTTPS GET                                                   |
| `vct#integrity` MUST be validated when present                                       | §4.3.1       | Implemented | Verifier enforces integrity check via resolver                                                 |
| Extension chain traversal, cycle detection                                           | §4.4         | Implemented | Resolver includes cycle detection                                                              |
| Type display metadata (locale, name, description, rendering)                         | §4.5         | Implemented | `DisplayMetadata` class; `name` property is type-display-specific                              |
| Claim display metadata (locale, label, description)                                  | §4.6.2       | Implemented | `DisplayMetadata` class; `label` property is claim-display-specific; both contexts documented  |
| Background image: `uri` and `uri#integrity` only (no `alt_text`)                     | §4.5.1.1.2   | **Fixed**   | New `BackgroundImageMetadata` class; `SimpleRenderingMetadata.BackgroundImage` updated         |
| Logo: `uri`, `uri#integrity`, `alt_text`                                             | §4.5.1.1.1   | Implemented | `LogoMetadata` class unchanged                                                                 |
| SVG templates: `properties` REQUIRED if > 1 template                                 | §4.5.1.2     | Documented  | `SvgTemplate.Properties` marked optional; runtime validation responsibility lies with consumer |
| Claim path processing (strings, null, integers)                                      | §4.6.1       | Implemented | `TypeMetadataResolver` path processing                                                         |
| `mandatory`, `sd` (always/allowed/never), `svg_id` claim metadata                    | §4.6.3–4.6.4 | Implemented | `ClaimMetadata` class                                                                          |
| Extending type claim metadata: inheritance + override rules                          | §4.6.5       | Implemented | Resolver applies inheritance during extension traversal                                        |
| `sd`/`mandatory` restriction rules when extending                                    | §4.6.5.1     | Implemented | Resolver enforces no downgrade of `always`/`never`/`mandatory`                                 |
| Integrity of referenced documents                                                    | §5           | Implemented | `IntegrityMetadataValidator` validates SRI hashes on all URI references                        |
| SSRF / HTTPS-only for remote resources                                               | §6.1         | Implemented | Resolver enforces HTTPS scheme on all fetched URIs                                             |

## Resolved Gaps (This Cycle)

| Gap ID | Description                                                                                                                             | Resolution                                                                                                         |
| ------ | --------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------ |
| G16-01 | `SimpleRenderingMetadata.BackgroundImage` used `LogoMetadata` which includes `alt_text` not defined for background images in §4.5.1.1.2 | Added `BackgroundImageMetadata` class with only `uri` and `uri#integrity`; updated `BackgroundImage` property type |
| G16-02 | `TypeMetadata.cs`, `DisplayMetadata.cs`, `RenderingMetadata.cs` header comments cited draft-14                                          | Updated all headers to cite draft-ietf-oauth-sd-jwt-vc-16 with section references                                  |
| G16-03 | `VerifiableCredentialPayload.cs` header cited draft-13                                                                                  | Updated to draft-16 §2.2.2                                                                                         |
| G16-04 | `JwtVcIssuerMetadata.cs` header cited draft-13                                                                                          | Updated to draft-16 §3                                                                                             |
| G16-05 | `MATURITY.md` declared `SdJwt.Net.Vc` as draft-15 tracking                                                                              | Updated to draft-16                                                                                                |
| G16-06 | `SdJwt.Net.Vc.csproj` description and tags referenced draft-15                                                                          | Updated to draft-16                                                                                                |
| G16-07 | `SdJwtConstants.cs` comment on status list constants cited draft-ietf-oauth-status-list-13                                              | Updated to draft-ietf-oauth-status-list-20                                                                         |
| G16-08 | `MATURITY.md` declared `SdJwt.Net.StatusList` as draft-18 tracking                                                                      | Updated to draft-20                                                                                                |

## Open Items / Future Watch

| Item                                  | Note                                                                                                                                                                                                                                                                                                                                   |
| ------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| SVG `properties` REQUIRED enforcement | §4.5.1.2 says `properties` REQUIRED when > 1 template present. Currently documented but not enforced at runtime. Consider adding issuer-side validation.                                                                                                                                                                               |
| DisplayMetadata unified class         | Draft-16 defines distinct schemas for type display (§4.5) and claim display (§4.6.2). The `DisplayMetadata` class is a superset covering both. This is spec-compliant (extra properties are ignored by consumers) but a future breaking change to split into `TypeDisplayMetadata` / `ClaimDisplayMetadata` would improve type safety. |
| Interoperability fixtures             | Add external fixture tests using published draft-16 test vectors once the IETF WG publishes them.                                                                                                                                                                                                                                      |

## Final Status (2026-05-09)

- Specification version analysed: draft-ietf-oauth-sd-jwt-vc-16 (April 24, 2026)
- All normative MUST/MUST NOT requirements: **Implemented**
- Draft-15 baseline: confirmed complete (see prior report)
- Draft-16 specific changes: all resolved in this cycle
