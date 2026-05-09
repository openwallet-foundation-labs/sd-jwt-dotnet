# Ecosystem architecture

> **Level:** Intermediate architecture

## What you will learn

- How the 21 packages relate to each other across five layers
- Which packages to choose for your use case
- How issuer, verifier, wallet, and agent trust components are composed
- What deployment patterns the ecosystem supports

## Audience & purpose

|              |                                                                                                 |
| ------------ | ----------------------------------------------------------------------------------------------- |
| **Audience** | Architects and senior developers designing credential systems with SD-JWT .NET                  |
| **Purpose**  | Make informed decisions about package selection, deployment topology, and integration patterns  |
| **Scope**    | Current source projects, package relationships, and deployment patterns                         |
| **Success**  | Reader can select the right library, protocol, reference, or preview extension for their system |

---

> SD-JWT .NET is a standards-first .NET library ecosystem.
> This document explains how the ecosystem's packages fit together.
> Unless explicitly marked Stable, packages are not certification claims or finished external standards.

## Context

The SD-JWT .NET Ecosystem provides standards-first .NET libraries, protocol components, reference infrastructure, and preview trust extensions for selective disclosure, verifiable credentials, wallet interoperability, and delegated agent trust.

It is not a standalone wallet, identity provider, authorization server, or compliance certification product. It provides reusable implementation building blocks that issuers, verifiers, wallet frameworks, enterprise APIs, and agent systems can build on.

## How to read this ecosystem

Start from the bottom:

- `SdJwt.Net` is the cryptographic foundation. It implements SD-JWT per RFC 9901.
- Credential packages define what kind of credential is being carried (SD-JWT VC, mdoc, W3C VCDM).
- Protocol packages define how credentials move between issuer, wallet, and verifier (OID4VCI, OID4VP, Presentation Exchange, Federation).
- Reference packages show how wallet and EUDIW-style infrastructure can be composed.
- Agent Trust packages are preview extensions for agent/tool authorization.

You do not need every package. Choose the smallest set that matches your use case.

### Choose by use case

| I want to build            | Start with                                  | Add later                            |
| -------------------------- | ------------------------------------------- | ------------------------------------ |
| Basic selective disclosure | `SdJwt.Net`                                 | `SdJwt.Net.Vc`                       |
| Issuer service             | `SdJwt.Net.Vc`, `SdJwt.Net.Oid4Vci`         | `StatusList`, `HAIP`                 |
| Verifier service           | `SdJwt.Net.Oid4Vp`, `PresentationExchange`  | `StatusList`, `Federation`           |
| Wallet framework           | `Wallet`, `Oid4Vci`, `Oid4Vp`, `Vc`, `Mdoc` | `Eudiw`                              |
| Agent tool governance      | `AgentTrust.Core`, `Policy`                 | `AspNetCore`, `Mcp`, `OpenTelemetry` |

## Package Role In The Ecosystem

| Field                   | Value                                                                                     |
| ----------------------- | ----------------------------------------------------------------------------------------- |
| Ecosystem area          | Cross-ecosystem architecture                                                              |
| Package maturity        | Mixed; see [Standards and Maturity Status](../reference/standards-status.md)              |
| Primary audience        | Architects, senior developers, platform engineers                                         |
| What this document does | Maps package roles, adoption tracks, deployment patterns, and trust boundaries            |
| What it does not do     | Certify deployments, replace protocol docs, or define production wallet/compliance claims |

---

## System architecture

### Hub-and-spoke model

The ecosystem has one standards core and two major adoption tracks: digital credential / wallet interoperability and preview delegated agent trust.

```mermaid
graph TB
    Core["SdJwt.Net<br/>RFC 9901 Core"]

    Core --> Cred["Credential Formats<br/>SD-JWT VC, Status List, mdoc, VCDM"]
    Cred --> Protocol["Protocol Components<br/>OID4VCI, OID4VP, PEX, Federation, DC API"]
    Protocol --> Wallet["Wallet & EUDIW<br/>Reference Infrastructure"]

    Core --> Agent["Preview Agent Trust<br/>Capability SD-JWTs, Policy, MCP/API Guards, A2A"]

    Wallet --> Apps["Issuers / Verifiers / Wallet Frameworks"]
    Agent --> APIs["Enterprise APIs / Agent Runtimes / Tool Servers"]
```

### Layer model

The ecosystem is organized into five layers. Each layer depends only on layers below it. This enforces separation of concerns and allows teams to adopt only the layers they need.

```mermaid
graph TB
    subgraph L5["Layer 5: Application (Your Code)"]
        Issuer["Issuer Service"]
        Verifier["Verifier Service"]
        WalletApp["Wallet Application"]
        AgentRuntime["Agent Runtime"]
    end

    subgraph L4["Layer 4: Trust Extensions & Reference Infrastructure"]
        Eudiw["SdJwt.Net.Eudiw"]
        Wallet["SdJwt.Net.Wallet"]
        ATrust["SdJwt.Net.AgentTrust.*"]
    end

    subgraph L3["Layer 3: Protocol"]
        OID4VCI["SdJwt.Net.Oid4Vci"]
        OID4VP["SdJwt.Net.Oid4Vp"]
        PEX["SdJwt.Net.PresentationExchange"]
        Fed["SdJwt.Net.OidFederation"]
    end

    subgraph L2["Layer 2: Credential Formats & Profiles"]
        Vc["SdJwt.Net.Vc"]
        StatusList["SdJwt.Net.StatusList"]
        Mdoc["SdJwt.Net.Mdoc"]
        VcDm["SdJwt.Net.VcDm"]
        HAIP["SdJwt.Net.HAIP"]
    end

    subgraph L1["Layer 1: Core"]
        Core["SdJwt.Net (RFC 9901)"]
    end

    L5 --> L4
    L5 --> L3
    L4 --> L3
    L4 --> L2
    L3 --> L2
    L2 --> L1

    style L1 fill:#1b4332,color:#fff
    style L2 fill:#2a6478,color:#fff
    style L3 fill:#3d5a80,color:#fff
    style L4 fill:#d62828,color:#fff
    style L5 fill:#555,color:#fff
```

### Layer descriptions

| Layer                       | Packages                                                                                             | Responsibility                                                                                            |
| --------------------------- | ---------------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------- |
| **L1: Core**                | `SdJwt.Net`                                                                                          | SD-JWT creation, parsing, presentation, and verification per RFC 9901. All other packages depend on this. |
| **L2: Credential**          | `SdJwt.Net.Vc`, `SdJwt.Net.StatusList`, `SdJwt.Net.Mdoc`, `SdJwt.Net.VcDm`, `SdJwt.Net.HAIP`         | Credential formats, status, W3C models, and profile-oriented validation helpers                           |
| **L3: Protocol**            | `SdJwt.Net.Oid4Vci`, `SdJwt.Net.Oid4Vp`, `SdJwt.Net.PresentationExchange`, `SdJwt.Net.OidFederation` | OpenID credential issuance, presentation, query, trust federation, and DC API support                     |
| **L4: Reference / Preview** | `SdJwt.Net.Wallet`, `SdJwt.Net.Eudiw`, `SdJwt.Net.AgentTrust.*`                                      | Wallet/EUDIW reference infrastructure plus preview delegated agent trust extensions                       |
| **L5: Application**         | Your code                                                                                            | Issuer services, verifier endpoints, wallet frameworks, enterprise APIs, and agent integrations           |

---

## Package dependency graph

```mermaid
graph LR
    Core["SdJwt.Net"]

    Vc["SdJwt.Net.Vc"] --> Core
    VcDm["SdJwt.Net.VcDm"] --> Core
    StatusList["SdJwt.Net.StatusList"] --> Core
    Mdoc["SdJwt.Net.Mdoc"] --> Core

    OID4VCI["SdJwt.Net.Oid4Vci"] --> Vc
    OID4VCI --> StatusList
    OID4VP["SdJwt.Net.Oid4Vp"] --> Vc
    OID4VP --> StatusList
    PEX["SdJwt.Net.PresentationExchange"] --> Vc
    Fed["SdJwt.Net.OidFederation"] --> Core

    HAIP["SdJwt.Net.HAIP"] --> Core
    Eudiw["SdJwt.Net.Eudiw"] --> Vc
    Eudiw --> Mdoc
    Eudiw --> HAIP
    Wallet["SdJwt.Net.Wallet"] --> Vc
    Wallet --> Mdoc
    Wallet --> StatusList
    Wallet --> OID4VCI
    Wallet --> OID4VP

    ATCore["AgentTrust.Core"] --> Core
    ATPolicy["AgentTrust.Policy"] --> ATCore
    ATAsp["AgentTrust.AspNetCore"] --> ATCore
    ATAsp --> ATPolicy
    ATMaf["AgentTrust.Maf"] --> ATCore
    ATMaf --> ATPolicy

    style Core fill:#1b4332,color:#fff
    style HAIP fill:#d62828,color:#fff
    style Eudiw fill:#d62828,color:#fff
    style ATCore fill:#7b2d8e,color:#fff
```

---

## Component architecture

### Issuer

The issuer creates credentials and manages their lifecycle.

```mermaid
flowchart LR
    subgraph IssuerService["Issuer Service"]
        direction TB
        CredBuilder["Credential Builder"]
        StatusMgr["Status Manager"]
        KeyMgr["Key Manager"]
        Endpoint["OID4VCI Endpoint"]
    end

    CredBuilder --> SdJwtVc["SdJwt.Net.Vc"]
    CredBuilder --> MdocLib["SdJwt.Net.Mdoc"]
    StatusMgr --> StatusList["SdJwt.Net.StatusList"]
    KeyMgr --> HAIP["SdJwt.Net.HAIP"]
    Endpoint --> OID4VCI["SdJwt.Net.Oid4Vci"]
```

| Component          | Package                            | Responsibility                                                              |
| ------------------ | ---------------------------------- | --------------------------------------------------------------------------- |
| Credential Builder | `SdJwt.Net.Vc` or `SdJwt.Net.Mdoc` | Construct SD-JWT VC or mdoc with selective disclosure claims                |
| Status Manager     | `SdJwt.Net.StatusList`             | Assign status list indices, manage revocation/suspension bitstrings         |
| Key Manager        | `SdJwt.Net.HAIP`                   | Enforce HAIP Final flow/profile requirements and ecosystem algorithm policy |
| OID4VCI Endpoint   | `SdJwt.Net.Oid4Vci`                | Handle pre-auth, auth code, batch, and deferred issuance flows              |

### Verifier

The verifier validates presented credentials.

```mermaid
flowchart LR
    subgraph VerifierService["Verifier Service"]
        direction TB
        AuthzReq["Authorization Request"]
        VPValidator["VP Token Validator"]
        StatusCheck["Status Checker"]
        TrustCheck["Trust Resolver"]
    end

    AuthzReq --> OID4VP["SdJwt.Net.Oid4Vp"]
    VPValidator --> PEX["SdJwt.Net.PresentationExchange"]
    StatusCheck --> StatusList["SdJwt.Net.StatusList"]
    TrustCheck --> Fed["SdJwt.Net.OidFederation"]
    TrustCheck --> HAIP["SdJwt.Net.HAIP"]
```

| Component             | Package                                      | Responsibility                                               |
| --------------------- | -------------------------------------------- | ------------------------------------------------------------ |
| Authorization Request | `SdJwt.Net.Oid4Vp`                           | Create OID4VP / DC API requests, same-device or cross-device |
| VP Token Validator    | `SdJwt.Net.PresentationExchange`             | Match credentials against presentation definitions           |
| Status Checker        | `SdJwt.Net.StatusList`                       | Fetch and evaluate status list for revocation/suspension     |
| Trust Resolver        | `SdJwt.Net.OidFederation` + `SdJwt.Net.HAIP` | Resolve trust chains, validate issuer keys                   |

### Wallet

The wallet stores credentials and creates presentations.

```mermaid
flowchart LR
    subgraph WalletInfra["Wallet Infrastructure"]
        direction TB
        CredStore["Credential Store"]
        FormatPlugin["Format Plugins"]
        ProtocolPlugin["Protocol Plugins"]
        KeyStore["Key Manager"]
    end

    CredStore --> Wallet["SdJwt.Net.Wallet"]
    FormatPlugin --> SdJwtVc["SdJwt.Net.Vc"]
    FormatPlugin --> MdocLib["SdJwt.Net.Mdoc"]
    ProtocolPlugin --> OID4VCI["SdJwt.Net.Oid4Vci"]
    ProtocolPlugin --> OID4VP["SdJwt.Net.Oid4Vp"]
    Wallet --> Eudiw["SdJwt.Net.Eudiw"]
```

| Component        | Package                                  | Responsibility                                                |
| ---------------- | ---------------------------------------- | ------------------------------------------------------------- |
| Credential Store | `SdJwt.Net.Wallet`                       | Encrypted storage, search, format-agnostic                    |
| Format Plugins   | `SdJwt.Net.Vc` + `SdJwt.Net.Mdoc`        | Parse, render, present SD-JWT VC or mdoc                      |
| Protocol Plugins | `SdJwt.Net.Oid4Vci` + `SdJwt.Net.Oid4Vp` | Handle issuance and presentation protocol flows               |
| Key Manager      | `SdJwt.Net.Wallet`                       | Software or HSM-backed key storage                            |
| EUDIW Reference  | `SdJwt.Net.Eudiw`                        | ARF-oriented helpers, EU trust-list models, PID/QEAA handling |

---

## Deployment patterns

### Pattern 1: single ecosystem

Simplest deployment - one organization issues, verifies, and hosts status lists.

```mermaid
flowchart LR
    Issuer["Issuer<br/>(SdJwt.Net.Vc + OID4VCI)"]
    Holder["Holder Wallet<br/>(SdJwt.Net.Wallet)"]
    Verifier["Verifier<br/>(SdJwt.Net.Oid4Vp + PEX)"]
    StatusCDN["Status List CDN"]

    Issuer -->|"1. Issue credential"| Holder
    Holder -->|"2. Present"| Verifier
    Verifier -->|"3. Check status"| StatusCDN
    Issuer -->|"Publish status"| StatusCDN
```

**Packages needed**: `SdJwt.Net`, `SdJwt.Net.Vc`, `SdJwt.Net.StatusList`, `SdJwt.Net.Oid4Vci`, `SdJwt.Net.Oid4Vp`

**Use when**: Enterprise issuing employee badges, membership cards, or internal attestations.

### Pattern 2: multi-issuer federation

Multiple issuers, verified via trust chains.

```mermaid
flowchart TB
    TA["Trust Anchor<br/>(Federation Metadata)"]
    IssuerA["Issuer A<br/>(University)"]
    IssuerB["Issuer B<br/>(Government)"]
    Holder["Holder Wallet"]
    Verifier["Verifier<br/>(Employer)"]

    TA -->|"Trust Chain"| IssuerA
    TA -->|"Trust Chain"| IssuerB
    IssuerA -->|"Diploma"| Holder
    IssuerB -->|"ID card"| Holder
    Holder -->|"Present both"| Verifier
    Verifier -->|"Resolve trust"| TA
```

**Additional packages**: `SdJwt.Net.OidFederation`, `SdJwt.Net.PresentationExchange`

**Use when**: Cross-organization verification where issuers and verifiers don't have direct trust.

### Pattern 3: high assurance (HAIP regulated)

Financial, healthcare, or government systems with strict cryptographic requirements.

**Additional packages**: `SdJwt.Net.HAIP`

**Configuration**: Select the applicable HAIP Final flows (`Oid4VciIssuance`, `Oid4VpRedirectPresentation`, or `Oid4VpDigitalCredentialsApiPresentation`) and credential profiles (`SdJwtVc`, `MsoMdoc`). Validate the declared capabilities with `HaipProfileValidator`, reject weak algorithms, and enforce holder binding where required by the selected profile.

### Pattern 4: EUDIW / ARF reference infrastructure

Reference infrastructure for EU Architecture Reference Framework concepts.

**Additional packages**: `SdJwt.Net.Eudiw`, `SdJwt.Net.Mdoc`, `SdJwt.Net.HAIP`

**Features**: ARF-oriented credential type validation, PID/mDL/QEAA handling, EU trust-list models, RP registration validation, and HAIP Final flow/profile validation.

### Pattern 5: AI agent trust

M2M capability-based authorization for AI agent ecosystems.

**Additional packages**: `SdJwt.Net.AgentTrust.Core`, `SdJwt.Net.AgentTrust.Policy`, `SdJwt.Net.AgentTrust.AspNetCore`, `SdJwt.Net.AgentTrust.Maf`

**Features**: Per-action scoped tokens, policy engine, delegation chains, audit receipts, replay prevention.

---

## Security architecture

### Cryptographic controls

| Control                  | Mechanism                                             | Enforcement                             |
| ------------------------ | ----------------------------------------------------- | --------------------------------------- |
| Algorithm allow-list     | HAIP validator rejects weak algorithms (RS256, HS256) | All issuance and verification paths     |
| Key size minimums        | P-256 (ES256), P-384 (ES384), P-521 (ES512)           | ECDsa key validation                    |
| Constant-time comparison | `CryptographicOperations.FixedTimeEquals`             | Signature verification, hash comparison |
| CSPRNG entropy           | `RandomNumberGenerator`                               | Salts, nonces, keys                     |
| Replay prevention        | Nonce + `iat` freshness + `jti` tracking              | Agent Trust, OID4VP flows               |

### Key management recommendations

| Environment | Key Storage                               | Rotation               |
| ----------- | ----------------------------------------- | ---------------------- |
| Development | In-memory (`InMemoryKeyCustodyProvider`)  | Not required           |
| Staging     | Azure Key Vault / AWS KMS (software keys) | Monthly                |
| Production  | HSM-backed Azure Key Vault / AWS CloudHSM | Per-policy (7-90 days) |

### Threat model summary

| Threat                    | Mitigation                                              | Package                                  |
| ------------------------- | ------------------------------------------------------- | ---------------------------------------- |
| Token forgery             | ECDSA signature verification                            | `SdJwt.Net`                              |
| Over-disclosure           | Selective disclosure per credential                     | `SdJwt.Net`, `SdJwt.Net.Mdoc`            |
| Replay attack             | Nonce, `iat`, `jti` validation                          | `SdJwt.Net`, `SdJwt.Net.AgentTrust.Core` |
| Credential revocation gap | Status List with configurable TTL                       | `SdJwt.Net.StatusList`                   |
| Weak algorithm downgrade  | HAIP Final minimums plus ecosystem algorithm allow-list | `SdJwt.Net.HAIP`                         |
| Issuer impersonation      | OpenID Federation trust chains                          | `SdJwt.Net.OidFederation`                |
| Confused-deputy (agents)  | Audience (`aud`) binding                                | `SdJwt.Net.AgentTrust.Core`              |

---

## Non-goals

The ecosystem intentionally does **not** provide:

- A user-facing wallet app (it provides the library infrastructure)
- An authorization server (use your existing OIDC provider)
- Certificate authority / PKI management (integrate with existing CA)
- Database or storage backends (pluggable interfaces provided)
- HTTP transport / web framework (integrates with ASP.NET Core)

---

## Constraints & assumptions

| Constraint                                 | Rationale                                                              |
| ------------------------------------------ | ---------------------------------------------------------------------- |
| .NET 8.0+ required for production packages | Modern cryptographic APIs, `System.Security.Cryptography` improvements |
| ECDSA only (no RSA for credentials)        | HAIP Final minimum support, ARF alignment, and compact signatures      |
| JSON + CBOR serialization only             | RFC 9901 (JSON) + ISO 18013-5 (CBOR)                                   |
| No PQC credential signing yet              | Waiting for NIST PQC standardization in .NET                           |
| Status Lists are eventually consistent     | CDN caching introduces a freshness window (configurable TTL)           |

---

## Alternatives considered

| Decision           | Chosen                    | Alternative               | Why                                                                           |
| ------------------ | ------------------------- | ------------------------- | ----------------------------------------------------------------------------- |
| Core token format  | SD-JWT (RFC 9901)         | BBS+ / AnonCreds          | RFC 9901 is IETF-ratified, broader tooling support; BBS+ not yet standardized |
| Binary format      | mdoc (ISO 18013-5)        | mDoc-JSON                 | ISO 18013-5 mandates CBOR; aligns with EUDIW ARF                              |
| Trust model        | OpenID Federation         | DID:web + DIDComm         | OpenID Federation aligns with eIDAS 2.0 trust lists                           |
| Policy engine      | Deterministic rule engine | OPA / Rego                | Lower complexity, single-library deployment, no sidecar needed                |
| Agent token format | SD-JWT capability tokens  | OAuth2 Client Credentials | Per-action scoping, selective disclosure, replay prevention                   |

---

## Related concepts

- [What SD-JWT .NET Is - and Is Not](what-this-project-is.md) - Ecosystem boundaries and terminology
- [Standards and Maturity Status](../reference/standards-status.md) - Specification and package maturity status
- [Capability Matrix](../reference/capabilities.md) - Feature coverage
- [Concepts Index](README.md) - Reading order for deep dives
- [Enterprise Roadmap](../ENTERPRISE_ROADMAP.md) - Strategic phases
- [Getting Started](../getting-started/quickstart.md) - 15-minute quickstart
