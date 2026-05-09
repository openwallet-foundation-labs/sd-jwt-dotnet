# Package Maturity

This document describes the maturity classification of each package in the
SD-JWT .NET Ecosystem. It helps adopters understand which packages are
production-ready, which track evolving specifications, and which are
reference or experimental.

## Maturity Levels

| Level             | Meaning                                                                                                                                                                                                                           |
| ----------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Stable**        | Implements a ratified standard (RFC or final spec). Comprehensive test coverage. Suitable for production use.                                                                                                                     |
| **Spec-Tracking** | Implements an IETF or OpenID draft that has not yet reached RFC status. API surface may change when the spec advances. Suitable for controlled adoption with the understanding that breaking changes may follow spec updates.     |
| **Profile**       | Implements a compliance profile or interoperability specification layered on top of core standards. Suitable for adoption within the profile scope.                                                                               |
| **Reference**     | Provides reference infrastructure, integration patterns, or jurisdictional compliance wrappers. Not positioned as a standalone product. Suitable for evaluation, prototyping, and as a starting point for custom implementations. |
| **Preview**       | Early-stage or experimental. API surface is not stable. Suitable for pilot projects and evaluation only.                                                                                                                          |

## Package Classification

### Standard Libraries

| Package                | Specification                                                                                     | Maturity          | Notes                                                                       |
| ---------------------- | ------------------------------------------------------------------------------------------------- | ----------------- | --------------------------------------------------------------------------- |
| `SdJwt.Net`            | [RFC 9901](https://datatracker.ietf.org/doc/rfc9901/)                                             | **Stable**        | Core SD-JWT implementation. Ratified IETF standard.                         |
| `SdJwt.Net.Vc`         | [draft-ietf-oauth-sd-jwt-vc-16](https://datatracker.ietf.org/doc/draft-ietf-oauth-sd-jwt-vc/)     | **Spec-Tracking** | Tracks IETF draft-16. Will promote to Stable when the draft becomes an RFC. |
| `SdJwt.Net.StatusList` | [draft-ietf-oauth-status-list-20](https://datatracker.ietf.org/doc/draft-ietf-oauth-status-list/) | **Spec-Tracking** | Tracks IETF draft-20. Will promote to Stable when the draft becomes an RFC. |

### Protocol Components

| Package                          | Specification                                                                               | Maturity   | Notes                                                                 |
| -------------------------------- | ------------------------------------------------------------------------------------------- | ---------- | --------------------------------------------------------------------- |
| `SdJwt.Net.Oid4Vci`              | [OpenID4VCI 1.0](https://openid.net/specs/openid-4-verifiable-credential-issuance-1_0.html) | **Stable** | Final OpenID specification.                                           |
| `SdJwt.Net.Oid4Vp`               | [OpenID4VP 1.0](https://openid.net/specs/openid-4-verifiable-presentations-1_0.html)        | **Stable** | Final OpenID specification.                                           |
| `SdJwt.Net.PresentationExchange` | [DIF PEX v2.1.1](https://identity.foundation/presentation-exchange/spec/v2.1.1/)            | **Stable** | Final DIF specification. Full required and optional feature coverage. |
| `SdJwt.Net.OidFederation`        | [OpenID Federation 1.0](https://openid.net/specs/openid-federation-1_0.html)                | **Stable** | Final OpenID specification. Trust chain resolution and validation.    |

### Profiles and Format Adapters

| Package          | Specification                                                                                             | Maturity    | Notes                                                                         |
| ---------------- | --------------------------------------------------------------------------------------------------------- | ----------- | ----------------------------------------------------------------------------- |
| `SdJwt.Net.HAIP` | [HAIP 1.0](https://openid.net/specs/openid4vc-high-assurance-interoperability-profile-sd-jwt-vc-1_0.html) | **Profile** | High Assurance Interoperability Profile. Draft-aligned compliance validation. |
| `SdJwt.Net.Mdoc` | [ISO 18013-5](https://www.iso.org/standard/69084.html)                                                    | **Stable**  | ISO standard for mobile documents. CBOR/COSE format, distinct from SD-JWT.    |

### Reference Infrastructure

| Package                        | Purpose                                 | Maturity      | Notes                                                                                                                             |
| ------------------------------ | --------------------------------------- | ------------- | --------------------------------------------------------------------------------------------------------------------------------- |
| `SdJwt.Net.Wallet`             | Generic wallet with plugin architecture | **Reference** | Wallet infrastructure primitives for credential management, key management, and protocol adapters. Not a full wallet application. |
| `SdJwt.Net.Eudiw`              | EU Digital Identity Wallet (eIDAS 2.0)  | **Reference** | Jurisdictional compliance profile for EUDIW/ARF. Wraps Wallet, HAIP, and Mdoc packages for EU requirements.                       |
| `SdJwt.Net.Oid4Vci.AspNetCore` | ASP.NET Core issuer reference server    | **Reference** | Not published as a NuGet package. Demonstrates issuer endpoint patterns.                                                          |

### Preview (Agent Trust)

| Package                           | Purpose                                    | Maturity    | Notes                                                     |
| --------------------------------- | ------------------------------------------ | ----------- | --------------------------------------------------------- |
| `SdJwt.Net.AgentTrust.Core`       | Capability token issuance and verification | **Preview** | Experimental capability-based trust for AI agent systems. |
| `SdJwt.Net.AgentTrust.Policy`     | Rule-based policy and delegation engine    | **Preview** | Experimental.                                             |
| `SdJwt.Net.AgentTrust.AspNetCore` | ASP.NET Core middleware for agent trust    | **Preview** | Experimental.                                             |
| `SdJwt.Net.AgentTrust.Maf`        | MAF/MCP adapter for agent tool calls       | **Preview** | Experimental.                                             |

## Promotion Criteria

A package moves to a higher maturity level when:

1. **Spec-Tracking to Stable**: The underlying specification is ratified as an RFC or final standard.
2. **Reference to Stable**: The package reaches comprehensive test coverage and the community validates the API surface through production use.
3. **Preview to Reference/Stable**: The design stabilizes, test coverage reaches a minimum threshold, and the feature set is validated through pilot deployments.

## How to Read This Document

- **Building an issuer or verifier?** Start with Stable packages: `SdJwt.Net`, `SdJwt.Net.Oid4Vci`, `SdJwt.Net.Oid4Vp`.
- **Need credential lifecycle?** Add Spec-Tracking packages: `SdJwt.Net.Vc`, `SdJwt.Net.StatusList`. Be prepared for minor API changes when drafts advance.
- **Building a wallet framework?** Use `SdJwt.Net.Wallet` as reference infrastructure and adapt it to your wallet architecture.
- **EU compliance?** Use `SdJwt.Net.Eudiw` as a reference for eIDAS 2.0 / ARF requirements.
- **Agent trust?** The `AgentTrust.*` packages are experimental. Evaluate in pilot environments only.
