# Ecosystem Capabilities

## Audience & purpose

|              |                                                                                                        |
| ------------ | ------------------------------------------------------------------------------------------------------ |
| **Audience** | Enterprise architects, technical leads, procurement teams evaluating the SD-JWT .NET ecosystem         |
| **Purpose**  | Single-page capability assessment for adoption decisions                                               |
| **Scope**    | All implemented, planned, and proposed features across the ecosystem                                   |
| **Success**  | Reader can determine whether the ecosystem meets their requirements without reading any other document |

---

## Capability matrix

### Credential formats

| Capability                         | Status      | Package          | Specification                                                                                 | Details                                        |
| ---------------------------------- | ----------- | ---------------- | --------------------------------------------------------------------------------------------- | ---------------------------------------------- |
| SD-JWT (Selective Disclosure JWT)  | Implemented | `SdJwt.Net`      | [RFC 9901](https://datatracker.ietf.org/doc/rfc9901/)                                         | [Concepts](concepts/sd-jwt.md)                 |
| SD-JWT VC (Verifiable Credentials) | Implemented | `SdJwt.Net.Vc`   | [draft-ietf-oauth-sd-jwt-vc-16](https://datatracker.ietf.org/doc/draft-ietf-oauth-sd-jwt-vc/) | [Concepts](concepts/verifiable-credentials.md) |
| mdoc / mDL (ISO Mobile Documents)  | Implemented | `SdJwt.Net.Mdoc` | [ISO 18013-5](https://www.iso.org/standard/69084.html)                                        | [Concepts](concepts/mdoc.md)                   |
| JWS JSON Serialization             | Implemented | `SdJwt.Net`      | RFC 9901 Section 5                                                                            | [Concepts](concepts/sd-jwt.md)                 |

### Issuance

| Capability                                      | Status      | Package             | Specification                                                                               | Details                            |
| ----------------------------------------------- | ----------- | ------------------- | ------------------------------------------------------------------------------------------- | ---------------------------------- |
| Pre-Authorized Code Flow                        | Implemented | `SdJwt.Net.Oid4Vci` | [OpenID4VCI 1.0](https://openid.net/specs/openid-4-verifiable-credential-issuance-1_0.html) | [Concepts](concepts/openid4vci.md) |
| Authorization Code Flow                         | Implemented | `SdJwt.Net.Oid4Vci` | OpenID4VCI 1.0                                                                              | [Concepts](concepts/openid4vci.md) |
| Batch Credential Issuance                       | Implemented | `SdJwt.Net.Oid4Vci` | OpenID4VCI 1.0                                                                              | [Concepts](concepts/openid4vci.md) |
| Deferred Credential Issuance                    | Implemented | `SdJwt.Net.Oid4Vci` | OpenID4VCI 1.0                                                                              | [Concepts](concepts/openid4vci.md) |
| Credential Offer                                | Implemented | `SdJwt.Net.Oid4Vci` | OpenID4VCI 1.0                                                                              | [Concepts](concepts/openid4vci.md) |
| Notification Endpoint                           | Implemented | `SdJwt.Net.Oid4Vci` | OpenID4VCI 1.0                                                                              | [Concepts](concepts/openid4vci.md) |
| Proof Validation (JWT / CWT)                    | Implemented | `SdJwt.Net.Oid4Vci` | OpenID4VCI 1.0                                                                              | [Concepts](concepts/openid4vci.md) |
| Credential Request/Response Encryption Metadata | Implemented | `SdJwt.Net.Oid4Vci` | OpenID4VCI 1.0                                                                              | [Concepts](concepts/openid4vci.md) |
| W3C VCDM Credential Issuance Metadata           | Implemented | `SdJwt.Net.Oid4Vci` | OpenID4VCI 1.0 + W3C VCDM 2.0                                                               | [Concepts](concepts/w3c-vcdm.md)   |
| mdoc Credential Issuance (`mso_mdoc`)           | Implemented | `SdJwt.Net.Mdoc`    | ISO 18013-5 + OpenID4VCI                                                                    | [Concepts](concepts/mdoc.md)       |

### Presentation & verification

| Capability                             | Status        | Package                                 | Specification                                                                         | Details                                             |
| -------------------------------------- | ------------- | --------------------------------------- | ------------------------------------------------------------------------------------- | --------------------------------------------------- |
| OpenID4VP Authorization Requests       | Implemented   | `SdJwt.Net.Oid4Vp`                      | [OpenID4VP 1.0](https://openid.net/specs/openid-4-verifiable-presentations-1_0.html)  | [Concepts](concepts/openid4vp.md)                   |
| SIOPv2 subject-signed ID Tokens        | Implemented   | `SdJwt.Net.SiopV2`                      | [SIOPv2 draft 13](https://openid.net/specs/openid-connect-self-issued-v2-1_0-13.html) | [Package README](../src/SdJwt.Net.SiopV2/README.md) |
| Combined `vp_token id_token` responses | Implemented   | `SdJwt.Net.Oid4Vp` + `SdJwt.Net.SiopV2` | OpenID4VP 1.0 + SIOPv2                                                                | [Concepts](concepts/openid4vp.md)                   |
| DCQL Credential Queries                | Implemented   | `SdJwt.Net.Oid4Vp`                      | OpenID4VP 1.0                                                                         | [Concepts](concepts/openid4vp.md)                   |
| JAR (JWT Authorization Requests)       | Implemented   | `SdJwt.Net.Oid4Vp`                      | OpenID4VP 1.0                                                                         | [Concepts](concepts/openid4vp.md)                   |
| Transaction Data Binding               | Implemented   | `SdJwt.Net.Oid4Vp`                      | OpenID4VP 1.0                                                                         | [Concepts](concepts/openid4vp.md)                   |
| Key Binding JWT (KB-JWT)               | Implemented   | `SdJwt.Net`                             | RFC 9901                                                                              | [Concepts](concepts/sd-jwt.md)                      |
| Presentation Exchange (DIF PEX)        | Implemented   | `SdJwt.Net.PresentationExchange`        | [DIF PEX v2.1.1](https://identity.foundation/presentation-exchange/spec/v2.1.1/)      | [Concepts](concepts/presentation-exchange.md)       |
| W3C Digital Credentials API            | Spec-tracking | `SdJwt.Net.Oid4Vp`                      | [W3C DC API](https://www.w3.org/TR/digital-credentials/)                              | [Concepts](concepts/dc-api.md)                      |
| Delivery via QR Codes & Deep Links     | Proposed      | -                                       | OID4VP request URI transport                                                          | [Plan](proposals/delivery-qr-deep-links.md)         |
| Multi-credential OID4VP sessions       | Proposed      | -                                       | OID4VP DCQL + PEX                                                                     | [Plan](proposals/bundles-batch-credentials.md)      |

### Status & lifecycle management

| Capability                                                  | Status      | Package                | Specification                                                                                     | Details                                             |
| ----------------------------------------------------------- | ----------- | ---------------------- | ------------------------------------------------------------------------------------------------- | --------------------------------------------------- |
| Token Status List (revocation/suspension)                   | Implemented | `SdJwt.Net.StatusList` | [draft-ietf-oauth-status-list-20](https://datatracker.ietf.org/doc/draft-ietf-oauth-status-list/) | [Concepts](concepts/status-list.md)                 |
| Multi-bit Status Values (Valid/Invalid/Suspended)           | Implemented | `SdJwt.Net.StatusList` | draft-ietf-oauth-status-list-20                                                                   | [Concepts](concepts/status-list.md)                 |
| Status List Freshness Validation                            | Implemented | `SdJwt.Net.StatusList` | draft-ietf-oauth-status-list-20                                                                   | [Concepts](concepts/status-list.md)                 |
| Set Revocation & Suspension                                 | Proposed    | -                      | Extends StatusList                                                                                | [Plan](proposals/credential-lifecycle-controls.md)  |
| Set Expiration & Validity Controls                          | Proposed    | -                      | Data-function driven                                                                              | [Plan](proposals/credential-lifecycle-controls.md)  |
| Expiration & Revocation Checks (Bitstring Status List v1.0) | Proposed    | -                      | [Bitstring Status List v1.0](https://www.w3.org/TR/vc-bitstring-status-list/)                     | [Plan](proposals/credential-lifecycle-controls.md)  |
| Credential Status Validation (wallet-side)                  | Implemented | `SdJwt.Net.Wallet`     | StatusList + Wallet integration                                                                   | [Package README](../src/SdJwt.Net.Wallet/README.md) |
| Credential Status Polling (wallet-side)                     | Proposed    | -                      | Scheduled status refresh                                                                          | [Plan](proposals/credential-lifecycle-controls.md)  |
| Token Introspection (RFC 7662)                              | Implemented | `SdJwt.Net.StatusList` | [RFC 7662](https://datatracker.ietf.org/doc/html/rfc7662)                                         | [Guide](guides/token-introspection.md)              |

### Trust infrastructure

| Capability                                 | Status      | Package                   | Specification                                                                                               | Details                                    |
| ------------------------------------------ | ----------- | ------------------------- | ----------------------------------------------------------------------------------------------------------- | ------------------------------------------ |
| OpenID Federation (Trust Chains)           | Implemented | `SdJwt.Net.OidFederation` | [OpenID Federation 1.0](https://openid.net/specs/openid-federation-1_0.html)                                | [Guide](guides/establishing-trust.md)      |
| HAIP final profile validation              | Implemented | `SdJwt.Net.HAIP`          | [HAIP 1.0 Final](https://openid.net/specs/openid4vc-high-assurance-interoperability-profile-1_0-final.html) | [Concepts](concepts/haip.md)               |
| EU Trust Lists (LOTL)                      | Implemented | `SdJwt.Net.Eudiw`         | eIDAS 2.0                                                                                                   | [Concepts](concepts/eudiw.md)              |
| Issuer Trust Validation (DID/PKI/IACA-DSC) | Proposed    | -                         | DID Web + PKI + IACA                                                                                        | [Plan](proposals/trust-registries-qtsp.md) |
| Trust Registries (eIDAS2, EBSI, custom)    | Proposed    | -                         | eIDAS2 / EBSI-style / custom                                                                                | [Plan](proposals/trust-registries-qtsp.md) |
| QTSP qualified evidence checks             | Proposed    | -                         | eIDAS Regulation + ETSI trusted lists                                                                       | [Plan](proposals/trust-registries-qtsp.md) |

### Display & metadata

| Capability                                 | Status      | Package             | Specification            | Details                                              |
| ------------------------------------------ | ----------- | ------------------- | ------------------------ | ---------------------------------------------------- |
| OID4VCI Issuer/Credential Display Metadata | Implemented | `SdJwt.Net.Oid4Vci` | OID4VCI display metadata | [Package README](../src/SdJwt.Net.Oid4Vci/README.md) |
| SD-JWT VC Type/Claim Display Metadata      | Implemented | `SdJwt.Net.Vc`      | SD-JWT VC draft-16       | [Package README](../src/SdJwt.Net.Vc/README.md)      |

### Wallet infrastructure

| Capability                           | Status      | Package            | Specification                                            | Details                        |
| ------------------------------------ | ----------- | ------------------ | -------------------------------------------------------- | ------------------------------ |
| Generic Wallet (plugin architecture) | Implemented | `SdJwt.Net.Wallet` | Project design                                           | [Concepts](concepts/wallet.md) |
| SD-JWT VC Format Plugin              | Implemented | `SdJwt.Net.Wallet` | RFC 9901 + SD-JWT VC                                     | [Concepts](concepts/wallet.md) |
| mdoc Format Plugin                   | Proposed    | -                  | ISO 18013-5                                              | [Concepts](concepts/wallet.md) |
| EUDIW (EU Digital Identity Wallet)   | Implemented | `SdJwt.Net.Eudiw`  | [eIDAS 2.0](https://eur-lex.europa.eu/eli/reg/2024/1183) | [Concepts](concepts/eudiw.md)  |
| ARF Profile Validation               | Implemented | `SdJwt.Net.Eudiw`  | EU ARF                                                   | [Concepts](concepts/eudiw.md)  |
| PID Credential Handling              | Implemented | `SdJwt.Net.Eudiw`  | EU ARF                                                   | [Concepts](concepts/eudiw.md)  |
| QEAA Handling                        | Implemented | `SdJwt.Net.Eudiw`  | EU ARF                                                   | [Concepts](concepts/eudiw.md)  |
| RP Registration Validation           | Implemented | `SdJwt.Net.Eudiw`  | EU ARF                                                   | [Concepts](concepts/eudiw.md)  |

### Agent trust

| Capability                            | Status      | Package                              | Specification  | Details                                  |
| ------------------------------------- | ----------- | ------------------------------------ | -------------- | ---------------------------------------- |
| Capability Token Minting (SD-JWT)     | Implemented | `SdJwt.Net.AgentTrust.Core`          | Project design | [Concepts](concepts/agent-trust-kits.md) |
| Capability Token Verification         | Implemented | `SdJwt.Net.AgentTrust.Core`          | Project design | [Concepts](concepts/agent-trust-kits.md) |
| Policy Engine (rule-based allow/deny) | Implemented | `SdJwt.Net.AgentTrust.Policy`        | Project design | [Concepts](concepts/agent-trust-kits.md) |
| Delegation Chain Enforcement          | Implemented | `SdJwt.Net.AgentTrust.Policy`        | Project design | [Concepts](concepts/agent-trust-kits.md) |
| ASP.NET Core Inbound Guard            | Implemented | `SdJwt.Net.AgentTrust.AspNetCore`    | Project design | [Concepts](concepts/agent-trust-kits.md) |
| MAF/MCP Outbound Propagation          | Implemented | `SdJwt.Net.AgentTrust.Maf`           | Project design | [Concepts](concepts/agent-trust-kits.md) |
| Audit Receipts                        | Implemented | `SdJwt.Net.AgentTrust.Core`          | Project design | [Concepts](concepts/agent-trust-kits.md) |
| Replay Prevention (Nonce Store)       | Implemented | `SdJwt.Net.AgentTrust.Core`          | Project design | [Concepts](concepts/agent-trust-kits.md) |
| Workload Identity Binding             | Implemented | `SdJwt.Net.AgentTrust.Core`          | Project design | [Concepts](concepts/agent-trust-kits.md) |
| Sender Constraint (DPoP/mTLS)         | Implemented | `SdJwt.Net.AgentTrust.Core`          | Project design | [Concepts](concepts/agent-trust-kits.md) |
| OpenTelemetry Metrics                 | Implemented | `SdJwt.Net.AgentTrust.OpenTelemetry` | Project design | [Concepts](concepts/agent-trust-kits.md) |
| Telemetry Receipt Writer              | Implemented | `SdJwt.Net.AgentTrust.OpenTelemetry` | Project design | [Concepts](concepts/agent-trust-kits.md) |
| OPA Policy Engine (HTTP)              | Implemented | `SdJwt.Net.AgentTrust.Policy.Opa`    | Project design | [Concepts](concepts/agent-trust-kits.md) |
| MCP Client Trust Interceptor          | Implemented | `SdJwt.Net.AgentTrust.Mcp`           | Project design | [Concepts](concepts/agent-trust-kits.md) |
| MCP Server Trust Guard                | Implemented | `SdJwt.Net.AgentTrust.Mcp`           | Project design | [Concepts](concepts/agent-trust-kits.md) |
| A2A Delegation Chain Validation       | Implemented | `SdJwt.Net.AgentTrust.A2A`           | Project design | [Concepts](concepts/agent-trust-kits.md) |
| A2A Delegation Token Issuance         | Implemented | `SdJwt.Net.AgentTrust.A2A`           | Project design | [Concepts](concepts/agent-trust-kits.md) |

### Security

| Capability                       | Status      | Package          | Details                                           |
| -------------------------------- | ----------- | ---------------- | ------------------------------------------------- |
| ECDSA P-256/384/521              | Implemented | `SdJwt.Net`      | All core cryptographic operations                 |
| SHA-256/384/512 enforcement      | Implemented | `SdJwt.Net`      | MD5/SHA-1 blocked at validation layer             |
| Constant-time comparisons        | Implemented | `SdJwt.Net`      | `CryptographicOperations.FixedTimeEquals`         |
| CSPRNG for all entropy           | Implemented | `SdJwt.Net`      | `RandomNumberGenerator` throughout                |
| Replay attack prevention         | Implemented | `SdJwt.Net`      | Nonce + `iat` freshness validation                |
| HAIP Final requirement catalog   | Implemented | `SdJwt.Net.HAIP` | Flow/profile requirement IDs for audit logs       |
| Algorithm allow-list enforcement | Implemented | `SdJwt.Net.HAIP` | Legacy local policy helper blocks weak algorithms |
| Wallet Attestation               | Partial     | `SdJwt.Net.HAIP` | HAIP final requires cryptographic validation      |

### Assurance profiles

| Capability                        | Status   | Details                                                   |
| --------------------------------- | -------- | --------------------------------------------------------- |
| Assurance profile extension point | Deferred | [Plan](proposals/deferred/regional-assurance-profiles.md) |
| Custom ecosystem profiles         | Deferred | [Plan](proposals/deferred/regional-assurance-profiles.md) |

### Platform support

| Capability              | Status      | Details                   |
| ----------------------- | ----------- | ------------------------- |
| .NET 8.0                | Implemented | Full support              |
| .NET 9.0                | Implemented | Full support              |
| .NET 10.0               | Implemented | Full support              |
| .NET Standard 2.1       | Implemented | Backward compatibility    |
| Windows / Linux / macOS | Implemented | x64, ARM64, Apple Silicon |
| Container Ready         | Compatible  | Docker, Kubernetes        |
| Cloud Native            | Compatible  | Azure, AWS, GCP           |

---

## Status legend

| Tag             | Meaning                                            |
| --------------- | -------------------------------------------------- |
| **Implemented** | Code complete, tested, available in NuGet packages |
| **Planned**     | Approved for development, design complete          |
| **Proposed**    | Design proposal written, awaiting approval         |

---

## Package quick reference

| Package                              | Purpose                         | Spec      |
| ------------------------------------ | ------------------------------- | --------- |
| `SdJwt.Net`                          | Core SD-JWT (RFC 9901)          | Final     |
| `SdJwt.Net.Vc`                       | SD-JWT VC profile               | draft-16  |
| `SdJwt.Net.StatusList`               | Token Status List               | draft-20  |
| `SdJwt.Net.Oid4Vci`                  | OpenID4VCI issuance             | 1.0 Final |
| `SdJwt.Net.Oid4Vp`                   | OpenID4VP presentation + DC API | 1.0       |
| `SdJwt.Net.SiopV2`                   | Self-issued ID Tokens           | draft-13  |
| `SdJwt.Net.VcDm`                     | W3C VCDM 2.0 data model         | 2.0       |
| `SdJwt.Net.PresentationExchange`     | DIF PEX credential query        | v2.1.1    |
| `SdJwt.Net.OidFederation`            | OpenID Federation trust         | 1.0       |
| `SdJwt.Net.HAIP`                     | High Assurance Interoperability | 1.0       |
| `SdJwt.Net.Mdoc`                     | ISO 18013-5 mdoc/mDL            | 2021      |
| `SdJwt.Net.Wallet`                   | Generic wallet with plugins     | -         |
| `SdJwt.Net.Eudiw`                    | EU Digital Identity Wallet      | eIDAS 2.0 |
| `SdJwt.Net.AgentTrust.Core`          | Capability token mint/verify    | -         |
| `SdJwt.Net.AgentTrust.Policy`        | Policy engine + delegation      | -         |
| `SdJwt.Net.AgentTrust.AspNetCore`    | Inbound verification middleware | -         |
| `SdJwt.Net.AgentTrust.Maf`           | MAF/MCP outbound propagation    | -         |
| `SdJwt.Net.AgentTrust.OpenTelemetry` | Agent trust metrics + telemetry | -         |
| `SdJwt.Net.AgentTrust.Policy.Opa`    | OPA external policy engine      | -         |
| `SdJwt.Net.AgentTrust.Mcp`           | MCP trust interceptor/guard     | -         |
| `SdJwt.Net.AgentTrust.A2A`           | Agent-to-agent delegation       | -         |

---

## Related documentation

- [Ecosystem Architecture](concepts/ecosystem-architecture.md) - Architecture overview
- [Enterprise Roadmap](enterprise-roadmap.md) - Strategic phases and timeline
- [Getting Started](getting-started/quickstart.md) - 15-minute quickstart
- [Tutorials](tutorials/README.md) - 3-week progressive learning path
- [Reference Patterns](reference-patterns/README.md) - Industry reference patterns and trust workflows
