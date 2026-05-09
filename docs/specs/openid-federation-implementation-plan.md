# OpenID Federation 1.0 Implementation Plan

This document tracks implementation status for `specs/openid-federation-1_0.txt`.

## Current Status

`SdJwt.Net.OidFederation` currently provides a partial OpenID Federation implementation:

- Entity Configuration and Entity Statement models.
- Entity Configuration JWT builder.
- Bottom-up trust chain resolution from an entity to configured trust anchors.
- Basic JWT signature and lifetime validation.
- Partial metadata policy application.
- Trust mark and constraint models.
- Trust Chain JSON parsing and serialization for `application/trust-chain+json`.
- Trust Chain syntactic validation, issuer/subject continuity checks, entity identifier extraction, and earliest-expiration calculation.
- OID4VCI and OID4VP helper validators that consume resolved trust chains.

The implementation is not yet a full OpenID Federation 1.0 conformance implementation.

## Implemented OID4VC Integration

OID4VCI Credential Issuers can now be validated using a resolved OpenID Federation trust chain:

- `SdJwt.Net.Oid4Vci.Federation.FederatedCredentialIssuerValidator`
- Requires valid OpenID Federation trust-chain result.
- Requires resolved `openid_credential_issuer` metadata.
- Can enforce expected `credential_issuer`, allowed trust anchors, and required trust marks.

OID4VP Verifiers can now be validated using a resolved OpenID Federation trust chain:

- `SdJwt.Net.Oid4Vp.Federation.FederatedVerifierValidator`
- Requires valid OpenID Federation trust-chain result.
- Requires resolved `openid_relying_party_verifier` or `openid_relying_party` metadata.
- Can enforce authorization request `client_id` binding, allowed trust anchors, and required trust marks.
- Provides DCQL `openid_federation` trusted-authority matching against the resolved chain.

## Phase 1: Core Entity Statement Conformance

Add complete Entity Statement model and validation support for Section 3:

- `metadata`
- `metadata_policy_crit`
- `trust_anchor_hints`
- `trust_mark_issuers`
- `trust_mark_owners`
- `trust_anchor`
- `aud`
- `trust_chain` header
- `peer_trust_chain` header
- Exact claim and header criticality checks.
- Tests for each Section 3.2 MUST requirement.

## Phase 2: Trust Chain Model and Validation

Add first-class Trust Chain support for Sections 4 and 10:

- Parse and serialize `application/trust-chain+json`. Implemented.
- Validate supplied/static trust chains without network fetching. Partially implemented: syntactic validation and issuer/subject continuity are available in `TrustChainDocument`; cryptographic validation is still pending.
- Ensure chain begins with subject Entity Configuration and ends at a trusted anchor.
- Validate issuer/subject continuity across every statement. Implemented.
- Calculate trust chain expiration as the minimum `exp`. Implemented.
- Enforce trust anchor selection and chain choice rules.
- Represent transient validation errors separately from permanent failures.

## Phase 3: Federation Policy Engine

Complete Section 6 metadata policy support:

- Implement policy combination across all superiors in the trust chain.
- Enforce operator order and policy compatibility rules.
- Support critical policy operators through `metadata_policy_crit`.
- Reject unknown critical policy language extensions.
- Apply final resolved policy to subject metadata and return resolved metadata.
- Add negative tests for every policy violation class.

## Phase 4: Constraints

Complete trust-chain constraint enforcement:

- Spec-accurate `max_path_length`.
- Naming constraints over subordinate Entity Identifiers.
- Allowed entity type constraints.
- Constraint evaluation at the correct position in the chain.
- Tests for valid and invalid intermediate/leaf chains.

## Phase 5: Trust Marks

Implement Section 7 Trust Mark validation:

- Signed Trust Mark JWT model and validator.
- Trust Mark delegation JWT model and validator.
- Trust Mark issuer and owner trust-chain validation.
- `trust_mark_issuers` and `trust_mark_owners` handling.
- Trust Mark status endpoint client.
- Resolver filtering so only verified Trust Marks are returned.

## Phase 6: Federation Endpoints

Implement Section 8 endpoint clients and ASP.NET Core endpoint package:

- Fetch endpoint.
- List endpoint.
- Resolve endpoint.
- Trust Mark status endpoint.
- Trust Mark listing endpoint.
- Historical keys endpoint.
- Standard media types and error responses.

Recommended package split:

- `SdJwt.Net.OidFederation`: protocol models, validators, clients.
- `SdJwt.Net.OidFederation.AspNetCore`: HTTP endpoint hosting.

## Phase 7: OpenID Connect Registration

Implement Section 12 registration support:

- Automatic Registration request helpers and validation.
- Explicit Registration request and response models.
- Trust-chain and peer-trust-chain registration handling.
- Registration expiration bounded by trust-chain expiration.
- Tests for successful and failing registration flows.

## Phase 8: Key Rollover and Historical Keys

Implement Section 11:

- Historical key response models.
- Historical key endpoint client.
- Key rollover validation support.
- Key revocation reason handling.

## Phase 9: Interoperability and Conformance

Add conformance fixtures and broad integration tests:

- Spec examples from `specs/openid-federation-1_0.txt`.
- In-memory HTTP federation topology tests.
- Negative tests for each MUST-level validation error.
- End-to-end OID4VCI issuer trust resolution.
- End-to-end OID4VP verifier trust resolution and DCQL trusted-authority matching.
