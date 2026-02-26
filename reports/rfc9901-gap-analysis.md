# RFC 9901 Gap Analysis Report

## Scope
- Repository: `SdJwt.Net` ecosystem
- Specification: `specs/rfc9901.txt`
- Focus: SD-JWT compact format, KB binding, disclosure processing, verifier rejection rules

## Requirement Mapping
| RFC Requirement | Evidence in Previous Code | Severity | Remediation Status |
|---|---|---|---|
| RFC 4 / 7.3: SD-JWT without KB must end with trailing `~` | Issuer/holder omitted terminal empty component; parser used `RemoveEmptyEntries` | High | Fixed |
| RFC 4.3.1: `sd_hash` covers `<issuer-jwt>~<selected disclosures>~` | Holder/verifier hashed only JWT component | High | Fixed |
| RFC 7.1(3.c): Object/array disclosure shape must match context | Rehydration accepted mismatched shapes | High | Fixed |
| RFC 7.1(3.c): Reject reserved disclosure claim names and key collisions | No explicit rejection for `_sd`, `...`, or same-level collisions | High | Fixed |
| RFC 7.1(3.d): Remove unresolved array digest placeholders | Unresolved `{"...":"digest"}` entries were retained | Medium | Fixed |
| RFC 7.1(4): Reject duplicate embedded digests | No payload-wide duplicate digest check | High | Fixed |
| RFC 4.1 / 4.2.1: Reserved names must not be regular claims | Issuer skipped `_sd` in processing and did not fail closed | High | Fixed |
| RFC 4.2.4.1: hide source order of `_sd` digests | Randomized shuffle used weak per-call seeding pattern | Low | Fixed (deterministic lexical reordering) |
| RFC test depth for MUST-fail scenarios | Coverage focused mostly on happy paths | Medium | Partially fixed (core strict tests extended) |

## Implemented Changes
- Added strict compact parsing that preserves terminal empty segments and differentiates SD-JWT vs SD-JWT+KB.
- Added canonical compact SD component (`CompactSdJwt`) in parsed presentation for correct KB hash validation.
- Updated holder presentation construction to:
  - emit trailing empty segment for non-KB presentations,
  - compute `sd_hash` from canonical compact SD component.
- Hardened verifier:
  - payload-wide embedded digest duplicate detection,
  - strict object/array disclosure shape checks,
  - reserved-name and key-collision rejection,
  - removal of unresolved array digest placeholders,
  - recursive removal of `_sd` keys before processed payload output.
- Hardened issuer:
  - reserved-claim payload rejection (`_sd`, `...`),
  - duplicate embedded digest rejection before signing,
  - deterministic digest ordering.
- Added strict behavior option types:
  - `SdJwtHolderOptions`,
  - `SdVerifierOptions`,
  - `KeyBindingValidationPolicy`.

## Residual Risk / Follow-up
- Additional dependent packages may require further test updates where old permissive parsing assumptions existed.
- More RFC 7.1 negative-path tests should be expanded in non-core packages that wrap SD-JWT parsing.
