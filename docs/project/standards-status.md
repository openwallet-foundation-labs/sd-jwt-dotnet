# Standards and Maturity Status

> SD-JWT .NET is a standards-first .NET library ecosystem.
> This document summarizes specification status and package maturity across the ecosystem.
> Unless explicitly marked Stable, packages are not certification claims or finished external standards.

## Status Matrix

| Area                  | Specification                     | Specification Status | Package                          | Package Maturity | Notes                                                       |
| --------------------- | --------------------------------- | -------------------- | -------------------------------- | ---------------- | ----------------------------------------------------------- |
| Core SD-JWT           | RFC 9901                          | RFC                  | `SdJwt.Net`                      | Stable           | Core selective disclosure, presentation, and verification   |
| SD-JWT VC             | SD-JWT VC                         | Active IETF draft    | `SdJwt.Net.Vc`                   | Spec-Tracking    | API may follow draft changes                                |
| Token Status List     | OAuth Status List                 | Active IETF draft    | `SdJwt.Net.StatusList`           | Spec-Tracking    | Status, revocation, suspension, and freshness checks        |
| W3C VCDM              | Verifiable Credentials Data Model | W3C Recommendation   | `SdJwt.Net.VcDm`                 | Stable           | Typed models and serializers                                |
| OID4VCI               | OpenID4VCI 1.0                    | OpenID Final         | `SdJwt.Net.Oid4Vci`              | Stable           | OAuth-protected credential issuance                         |
| OID4VP                | OpenID4VP 1.0                     | OpenID Final         | `SdJwt.Net.Oid4Vp`               | Stable           | Presentation protocol and DC API support                    |
| SIOPv2                | Self-Issued OP v2                 | OpenID draft         | `SdJwt.Net.SiopV2`               | Spec-Tracking    | Subject-signed ID token helpers                             |
| Presentation Exchange | DIF PEX v2.1.1                    | DIF Final            | `SdJwt.Net.PresentationExchange` | Stable           | Credential query and selection                              |
| OpenID Federation     | OpenID Federation 1.0             | OpenID Final         | `SdJwt.Net.OidFederation`        | Stable           | Trust chain resolution                                      |
| HAIP                  | OpenID4VC HAIP 1.0                | OpenID Final profile | `SdJwt.Net.HAIP`                 | Profile          | Validates declared profile capabilities and policy switches |
| mdoc                  | ISO 18013-5                       | ISO standard         | `SdJwt.Net.Mdoc`                 | Stable           | Mobile document format                                      |
| Wallet infrastructure | Project reference design          | Reference            | `SdJwt.Net.Wallet`               | Reference        | Not a standalone consumer wallet                            |
| EUDIW / ARF           | eIDAS 2.0 / ARF concepts          | Regional framework   | `SdJwt.Net.Eudiw`                | Reference        | Not a certified EU Digital Identity Wallet                  |
| Agent Trust           | Project-defined profile           | Preview              | `SdJwt.Net.AgentTrust.*`         | Preview          | Not an IETF, OpenID Foundation, or OWF standard             |

## How To Read This Page

- **Stable** means the package targets a ratified or final specification and is suitable for production use subject to normal engineering review.
- **Spec-Tracking** means the package follows an active draft and may change as the specification changes.
- **Profile** means the package implements validation or policy checks over one or more base specifications.
- **Reference** means the package provides reusable infrastructure or patterns, not a finished product.
- **Preview** means the package is intended for evaluation, pilots, and feedback.

## Related Documentation

- [Package Maturity](../MATURITY.md)
- [What SD-JWT .NET Is - and Is Not](concepts/what-this-project-is.md)
- [Ecosystem Architecture](concepts/ecosystem-architecture.md)
