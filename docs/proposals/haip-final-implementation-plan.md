# HAIP 1.0 Final Implementation Plan

## Goal

Bring `SdJwt.Net.HAIP`, `SdJwt.Net.Oid4Vci`, `SdJwt.Net.Oid4Vp`, `SdJwt.Net.Vc`, `SdJwt.Net.Mdoc`, and `SdJwt.Net.StatusList` into an auditable implementation of the OpenID4VC High Assurance Interoperability Profile 1.0 Final.

## Current Position

The repository contains reusable building blocks for OID4VCI, OID4VP, DCQL, SD-JWT VC, mdoc, x5c validation, key attestation hooks, and Token Status List. The current `SdJwt.Net.HAIP` package is a preliminary policy helper. It does not yet implement the final HAIP flow-by-flow conformance model.

## Phase 1: Requirement Matrix

- Replace the level-centric HAIP model with explicit flow conformance targets:
  - OID4VCI issuance
  - OID4VP redirect presentation
  - OID4VP via W3C Digital Credentials API
  - SD-JWT VC credential profile
  - ISO mdoc credential profile
- Add a machine-readable HAIP requirement catalog covering each final-spec MUST and SHOULD the library can validate.
- Keep ecosystem-specific extension points configurable instead of hard-coding jurisdictional assumptions.

## Phase 2: OID4VCI Profile

- Enforce authorization code flow support.
- Enforce PKCE `S256` for authorization code flows.
- Enforce PAR where the Authorization Endpoint is used.
- Validate authorization response issuer binding.
- Implement DPoP proof validation for sender-constrained access tokens, including nonce handling.
- Validate Wallet Attestation and Key Attestation cryptographically rather than by property presence.
- Enforce `nonce_endpoint` metadata when key-bound credential configurations are advertised.
- Verify signed Credential Issuer Metadata with `x5c` when required by policy.

## Phase 3: OID4VP Profile

- Add HAIP validators for redirect-based OpenID4VP.
- Add HAIP validators for W3C Digital Credentials API presentation.
- Enforce DCQL request and response shape for HAIP flows.
- Validate signed presentation requests, verifier attestation, `transaction_data`, and `verifier_info` according to configured ecosystem policy.
- Ensure `dc+sd-jwt` and `mso_mdoc` VP format validation is wired into the HAIP conformance result.

## Phase 4: SD-JWT VC Profile

- Require the `dc+sd-jwt` credential format identifier.
- Enforce compact serialization support.
- Validate `cnf.jwk` when holder binding is required.
- Require KB-JWT on presentation when a credential is cryptographically holder-bound.
- Enforce `status.status_list` compatibility with Token Status List.
- Mandate x5c-based issuer key resolution where HAIP requires it.

## Phase 5: ISO mdoc Profile

- Add a direct `SdJwt.Net.Mdoc` dependency to `SdJwt.Net.HAIP`.
- Validate mdoc VP tokens through the mdoc parser and verifier.
- Enforce supported COSE algorithms and SHA-256 digest support.
- Validate device signature, doctype matching, and x5chain/trust-anchor policy.

## Phase 6: Token Status List

- Align code, package metadata, and docs with `draft-ietf-oauth-status-list-20`.
- Keep JWT Status List Token support as the primary implemented path.
- Track CWT/COSE referenced-token status support as a separate implementation item if full mdoc status validation is required.
- Add conformance tests for status type values, ZLIB compressed byte arrays, TTL caching, `sub` to `uri` matching, and expiration handling.

## Phase 7: Documentation and Conformance

- Update capability docs from broad "implemented" claims to specific flow-level status.
- Add HAIP examples for each supported flow.
- Run OpenID Foundation conformance suites for OID4VCI and OID4VP when available.
- Document unsupported or policy-dependent HAIP extension points explicitly.
