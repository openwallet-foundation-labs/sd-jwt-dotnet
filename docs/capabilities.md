# Ecosystem Capabilities

## Audience & Purpose

|              |                                                                                                        |
| ------------ | ------------------------------------------------------------------------------------------------------ |
| **Audience** | Enterprise architects, technical leads, procurement teams evaluating the SD-JWT .NET ecosystem         |
| **Purpose**  | Single-page capability assessment for adoption decisions                                               |
| **Scope**    | All implemented, planned, and proposed features across the ecosystem                                   |
| **Success**  | Reader can determine whether the ecosystem meets their requirements without reading any other document |

---

## Capability Matrix

### Credential Formats

| Capability                         | Status      | Package          | Specification                                                                                 | Details                                                  |
| ---------------------------------- | ----------- | ---------------- | --------------------------------------------------------------------------------------------- | -------------------------------------------------------- |
| SD-JWT (Selective Disclosure JWT)  | Implemented | `SdJwt.Net`      | [RFC 9901](https://datatracker.ietf.org/doc/rfc9901/)                                         | [Deep Dive](concepts/sd-jwt-deep-dive.md)                |
| SD-JWT VC (Verifiable Credentials) | Implemented | `SdJwt.Net.Vc`   | [draft-ietf-oauth-sd-jwt-vc-15](https://datatracker.ietf.org/doc/draft-ietf-oauth-sd-jwt-vc/) | [Deep Dive](concepts/verifiable-credential-deep-dive.md) |
| mdoc / mDL (ISO Mobile Documents)  | Implemented | `SdJwt.Net.Mdoc` | [ISO 18013-5](https://www.iso.org/standard/69084.html)                                        | [Deep Dive](concepts/mdoc-deep-dive.md)                  |
| JWS JSON Serialization             | Implemented | `SdJwt.Net`      | RFC 9901 Section 5                                                                            | [Deep Dive](concepts/sd-jwt-deep-dive.md)                |

### Issuance

| Capability                            | Status      | Package             | Specification                                                                               | Details                                       |
| ------------------------------------- | ----------- | ------------------- | ------------------------------------------------------------------------------------------- | --------------------------------------------- |
| Pre-Authorized Code Flow              | Implemented | `SdJwt.Net.Oid4Vci` | [OpenID4VCI 1.0](https://openid.net/specs/openid-4-verifiable-credential-issuance-1_0.html) | [Deep Dive](concepts/openid4vci-deep-dive.md) |
| Authorization Code Flow               | Implemented | `SdJwt.Net.Oid4Vci` | OpenID4VCI 1.0                                                                              | [Deep Dive](concepts/openid4vci-deep-dive.md) |
| Batch Credential Issuance             | Implemented | `SdJwt.Net.Oid4Vci` | OpenID4VCI 1.0                                                                              | [Deep Dive](concepts/openid4vci-deep-dive.md) |
| Deferred Credential Issuance          | Implemented | `SdJwt.Net.Oid4Vci` | OpenID4VCI 1.0                                                                              | [Deep Dive](concepts/openid4vci-deep-dive.md) |
| Credential Offer                      | Implemented | `SdJwt.Net.Oid4Vci` | OpenID4VCI 1.0                                                                              | [Deep Dive](concepts/openid4vci-deep-dive.md) |
| Notification Endpoint                 | Implemented | `SdJwt.Net.Oid4Vci` | OpenID4VCI 1.0                                                                              | [Deep Dive](concepts/openid4vci-deep-dive.md) |
| Proof Validation (JWT / CWT)          | Implemented | `SdJwt.Net.Oid4Vci` | OpenID4VCI 1.0                                                                              | [Deep Dive](concepts/openid4vci-deep-dive.md) |
| mdoc Credential Issuance (`mso_mdoc`) | Implemented | `SdJwt.Net.Mdoc`    | ISO 18013-5 + OpenID4VCI                                                                    | [Deep Dive](concepts/mdoc-deep-dive.md)       |

### Presentation & Verification

| Capability                                  | Status      | Package                          | Specification                                                                        | Details                                                  |
| ------------------------------------------- | ----------- | -------------------------------- | ------------------------------------------------------------------------------------ | -------------------------------------------------------- |
| OpenID4VP Authorization Requests            | Implemented | `SdJwt.Net.Oid4Vp`               | [OpenID4VP 1.0](https://openid.net/specs/openid-4-verifiable-presentations-1_0.html) | [Deep Dive](concepts/openid4vp-deep-dive.md)             |
| JAR (JWT Authorization Requests)            | Implemented | `SdJwt.Net.Oid4Vp`               | OpenID4VP 1.0                                                                        | [Deep Dive](concepts/openid4vp-deep-dive.md)             |
| Transaction Data Binding                    | Implemented | `SdJwt.Net.Oid4Vp`               | OpenID4VP 1.0                                                                        | [Deep Dive](concepts/openid4vp-deep-dive.md)             |
| Key Binding JWT (KB-JWT)                    | Implemented | `SdJwt.Net`                      | RFC 9901                                                                             | [Deep Dive](concepts/sd-jwt-deep-dive.md)                |
| Presentation Exchange (DIF PEX)             | Implemented | `SdJwt.Net.PresentationExchange` | [DIF PEX v2.1.1](https://identity.foundation/presentation-exchange/spec/v2.1.1/)     | [Deep Dive](concepts/presentation-exchange-deep-dive.md) |
| W3C Digital Credentials API                 | Implemented | `SdJwt.Net.Oid4Vp`               | [W3C DC API](https://wicg.github.io/digital-credentials/)                            | [Deep Dive](concepts/dc-api-deep-dive.md)                |
| Delivery via QR Codes & Deep Links          | Proposed    | -                                | OID4VP transport                                                                     | [Proposal](proposals/delivery-qr-deep-links.md)          |
| Bundles / Batch (multi-credential sessions) | Proposed    | -                                | OID4VP + PEX                                                                         | [Proposal](proposals/bundles-batch-credentials.md)       |

### Status & Lifecycle Management

| Capability                                                  | Status      | Package                | Specification                                                                                     | Details                                                  |
| ----------------------------------------------------------- | ----------- | ---------------------- | ------------------------------------------------------------------------------------------------- | -------------------------------------------------------- |
| Token Status List (revocation/suspension)                   | Implemented | `SdJwt.Net.StatusList` | [draft-ietf-oauth-status-list-18](https://datatracker.ietf.org/doc/draft-ietf-oauth-status-list/) | [Deep Dive](concepts/status-list-deep-dive.md)           |
| Multi-bit Status Values (Valid/Revoked/Suspended)           | Implemented | `SdJwt.Net.StatusList` | draft-ietf-oauth-status-list-18                                                                   | [Deep Dive](concepts/status-list-deep-dive.md)           |
| Status List Freshness Validation                            | Implemented | `SdJwt.Net.StatusList` | draft-ietf-oauth-status-list-18                                                                   | [Deep Dive](concepts/status-list-deep-dive.md)           |
| Set Revocation & Suspension                                 | Proposed    | -                      | Extends StatusList                                                                                | [Proposal](proposals/credential-lifecycle-controls.md)   |
| Set Expiration & Validity Controls                          | Proposed    | -                      | Data-function driven                                                                              | [Proposal](proposals/credential-lifecycle-controls.md)   |
| Expiration & Revocation Checks (Bitstring Status List v1.0) | Proposed    | -                      | [Bitstring Status List v1.0](https://www.w3.org/TR/vc-bitstring-status-list/)                     | [Proposal](proposals/credential-lifecycle-controls.md)   |
| Credential Status Validation (wallet-side)                  | Proposed    | -                      | StatusList + Wallet integration                                                                   | [Proposal](proposals/credential-lifecycle-controls.md)   |
| Token Introspection (RFC 7662)                              | Proposed    | -                      | [RFC 7662](https://datatracker.ietf.org/doc/html/rfc7662)                                         | [Proposal](proposals/token-introspection-enhancement.md) |

### Trust Infrastructure

| Capability                                 | Status      | Package                   | Specification                                                                                             | Details                                        |
| ------------------------------------------ | ----------- | ------------------------- | --------------------------------------------------------------------------------------------------------- | ---------------------------------------------- |
| OpenID Federation (Trust Chains)           | Implemented | `SdJwt.Net.OidFederation` | [OpenID Federation 1.0](https://openid.net/specs/openid-federation-1_0.html)                              | [Guide](guides/establishing-trust.md)          |
| HAIP Level 1 (High)                        | Implemented | `SdJwt.Net.HAIP`          | [HAIP 1.0](https://openid.net/specs/openid4vc-high-assurance-interoperability-profile-sd-jwt-vc-1_0.html) | [Deep Dive](concepts/haip-deep-dive.md)        |
| HAIP Level 2 (Very High)                   | Implemented | `SdJwt.Net.HAIP`          | HAIP 1.0                                                                                                  | [Deep Dive](concepts/haip-deep-dive.md)        |
| HAIP Level 3 (Sovereign)                   | Implemented | `SdJwt.Net.HAIP`          | HAIP 1.0                                                                                                  | [Deep Dive](concepts/haip-deep-dive.md)        |
| EU Trust Lists (LOTL)                      | Implemented | `SdJwt.Net.Eudiw`         | eIDAS 2.0                                                                                                 | [Deep Dive](concepts/eudiw-deep-dive.md)       |
| Issuer Trust Validation (DID/PKI/IACA-DSC) | Proposed    | -                         | DID + PKI + IACA                                                                                          | [Proposal](proposals/trust-registries-qtsp.md) |
| Trust Registries (eIDAS2, EBSI, custom)    | Proposed    | -                         | eIDAS2 / EBSI                                                                                             | [Proposal](proposals/trust-registries-qtsp.md) |
| QTSPs (Qualified Signature Support)        | Proposed    | -                         | eIDAS Regulation                                                                                          | [Proposal](proposals/trust-registries-qtsp.md) |

### Display & Metadata

| Capability                                   | Status   | Package | Specification            | Details                                          |
| -------------------------------------------- | -------- | ------- | ------------------------ | ------------------------------------------------ |
| Issuer Metadata (credential branding)        | Proposed | -       | OID4VCI display metadata | [Proposal](proposals/issuer-metadata-display.md) |
| Embedded Display Data (per-instance visuals) | Proposed | -       | Credential rendering     | [Proposal](proposals/issuer-metadata-display.md) |

### Wallet Infrastructure

| Capability                           | Status      | Package            | Specification                                            | Details                                   |
| ------------------------------------ | ----------- | ------------------ | -------------------------------------------------------- | ----------------------------------------- |
| Generic Wallet (plugin architecture) | Implemented | `SdJwt.Net.Wallet` | Project design                                           | [Deep Dive](concepts/wallet-deep-dive.md) |
| SD-JWT VC Format Plugin              | Implemented | `SdJwt.Net.Wallet` | RFC 9901 + SD-JWT VC                                     | [Deep Dive](concepts/wallet-deep-dive.md) |
| mdoc Format Plugin                   | Implemented | `SdJwt.Net.Wallet` | ISO 18013-5                                              | [Deep Dive](concepts/wallet-deep-dive.md) |
| EUDIW (EU Digital Identity Wallet)   | Implemented | `SdJwt.Net.Eudiw`  | [eIDAS 2.0](https://eur-lex.europa.eu/eli/reg/2024/1183) | [Deep Dive](concepts/eudiw-deep-dive.md)  |
| ARF Profile Validation               | Implemented | `SdJwt.Net.Eudiw`  | EU ARF                                                   | [Deep Dive](concepts/eudiw-deep-dive.md)  |
| PID Credential Handling              | Implemented | `SdJwt.Net.Eudiw`  | EU ARF                                                   | [Deep Dive](concepts/eudiw-deep-dive.md)  |
| QEAA Handling                        | Implemented | `SdJwt.Net.Eudiw`  | EU ARF                                                   | [Deep Dive](concepts/eudiw-deep-dive.md)  |
| RP Registration Validation           | Implemented | `SdJwt.Net.Eudiw`  | EU ARF                                                   | [Deep Dive](concepts/eudiw-deep-dive.md)  |

### Agent Trust

| Capability                            | Status      | Package                           | Specification  | Details                                             |
| ------------------------------------- | ----------- | --------------------------------- | -------------- | --------------------------------------------------- |
| Capability Token Minting (SD-JWT)     | Implemented | `SdJwt.Net.AgentTrust.Core`       | Project design | [Deep Dive](concepts/agent-trust-kits-deep-dive.md) |
| Capability Token Verification         | Implemented | `SdJwt.Net.AgentTrust.Core`       | Project design | [Deep Dive](concepts/agent-trust-kits-deep-dive.md) |
| Policy Engine (rule-based allow/deny) | Implemented | `SdJwt.Net.AgentTrust.Policy`     | Project design | [Deep Dive](concepts/agent-trust-kits-deep-dive.md) |
| Delegation Chain Enforcement          | Implemented | `SdJwt.Net.AgentTrust.Policy`     | Project design | [Deep Dive](concepts/agent-trust-kits-deep-dive.md) |
| ASP.NET Core Inbound Guard            | Implemented | `SdJwt.Net.AgentTrust.AspNetCore` | Project design | [Deep Dive](concepts/agent-trust-kits-deep-dive.md) |
| MAF/MCP Outbound Propagation          | Implemented | `SdJwt.Net.AgentTrust.Maf`        | Project design | [Deep Dive](concepts/agent-trust-kits-deep-dive.md) |
| Audit Receipts                        | Implemented | `SdJwt.Net.AgentTrust.Core`       | Project design | [Deep Dive](concepts/agent-trust-kits-deep-dive.md) |
| Replay Prevention (Nonce Store)       | Implemented | `SdJwt.Net.AgentTrust.Core`       | Project design | [Deep Dive](concepts/agent-trust-kits-deep-dive.md) |

### Security

| Capability                       | Status      | Package          | Details                                   |
| -------------------------------- | ----------- | ---------------- | ----------------------------------------- |
| ECDSA P-256/384/521              | Implemented | `SdJwt.Net`      | All core cryptographic operations         |
| SHA-256/384/512 enforcement      | Implemented | `SdJwt.Net`      | MD5/SHA-1 blocked at validation layer     |
| Constant-time comparisons        | Implemented | `SdJwt.Net`      | `CryptographicOperations.FixedTimeEquals` |
| CSPRNG for all entropy           | Implemented | `SdJwt.Net`      | `RandomNumberGenerator` throughout        |
| Replay attack prevention         | Implemented | `SdJwt.Net`      | Nonce + `iat` freshness validation        |
| Algorithm allow-list enforcement | Implemented | `SdJwt.Net.HAIP` | HAIP validator blocks weak algorithms     |
| Wallet Attestation               | Implemented | `SdJwt.Net.HAIP` | HAIP Level 2+                             |

### Regional Alignment

| Capability                  | Status   | Details                                     |
| --------------------------- | -------- | ------------------------------------------- |
| EMEA (eIDAS2, EBSI, SWIYU)  | Proposed | [Proposal](proposals/regional-alignment.md) |
| APAC (NZ DISTF, AU, TH, JP) | Proposed | [Proposal](proposals/regional-alignment.md) |
| Americas (US, CA, BR)       | Proposed | [Proposal](proposals/regional-alignment.md) |
| Custom Ecosystems           | Proposed | [Proposal](proposals/regional-alignment.md) |

### Platform Support

| Capability              | Status      | Details                                 |
| ----------------------- | ----------- | --------------------------------------- |
| .NET 8.0                | Implemented | Full support with modern optimizations  |
| .NET 9.0                | Implemented | Latest features and optimal performance |
| .NET 10.0               | Implemented | Full support                            |
| .NET Standard 2.1       | Implemented | Backward compatibility                  |
| Windows / Linux / macOS | Implemented | x64, ARM64, Apple Silicon               |
| Container Ready         | Implemented | Docker, Kubernetes                      |
| Cloud Native            | Implemented | Azure, AWS, GCP                         |

---

## Status Legend

| Tag             | Meaning                                            |
| --------------- | -------------------------------------------------- |
| **Implemented** | Code complete, tested, available in NuGet packages |
| **Planned**     | Approved for development, design complete          |
| **Proposed**    | Design proposal written, awaiting approval         |

---

## Package Quick Reference

| Package                           | Purpose                         | Spec      |
| --------------------------------- | ------------------------------- | --------- |
| `SdJwt.Net`                       | Core SD-JWT (RFC 9901)          | Final     |
| `SdJwt.Net.Vc`                    | SD-JWT VC profile               | draft-15  |
| `SdJwt.Net.StatusList`            | Token Status List               | draft-18  |
| `SdJwt.Net.Oid4Vci`               | OpenID4VCI issuance             | 1.0 Final |
| `SdJwt.Net.Oid4Vp`                | OpenID4VP presentation + DC API | 1.0       |
| `SdJwt.Net.PresentationExchange`  | DIF PEX credential query        | v2.1.1    |
| `SdJwt.Net.OidFederation`         | OpenID Federation trust         | 1.0       |
| `SdJwt.Net.HAIP`                  | High Assurance Interoperability | 1.0       |
| `SdJwt.Net.Mdoc`                  | ISO 18013-5 mdoc/mDL            | 2021      |
| `SdJwt.Net.Wallet`                | Generic wallet with plugins     | -         |
| `SdJwt.Net.Eudiw`                 | EU Digital Identity Wallet      | eIDAS 2.0 |
| `SdJwt.Net.AgentTrust.Core`       | Capability token mint/verify    | -         |
| `SdJwt.Net.AgentTrust.Policy`     | Policy engine + delegation      | -         |
| `SdJwt.Net.AgentTrust.AspNetCore` | Inbound verification middleware | -         |
| `SdJwt.Net.AgentTrust.Maf`        | MAF/MCP outbound propagation    | -         |

---

## Related Documentation

- [Ecosystem Architecture](concepts/ecosystem-architecture.md) - Master architecture overview
- [Enterprise Roadmap](ENTERPRISE_ROADMAP.md) - Strategic phases and timeline
- [Getting Started](getting-started/quickstart.md) - 15-minute quickstart
- [Tutorials](tutorials/README.md) - 3-week progressive learning path
- [Use Cases](use-cases/README.md) - Industry scenarios with working examples
