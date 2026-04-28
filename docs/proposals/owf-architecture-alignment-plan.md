# OWF Architecture Whitepaper -- Alignment Assessment for SD-JWT .NET

**Status**: Proposal
**Date**: 2026-04-28
**Reference**: [OWF Architecture SIG - Universal Wallet Reference Architecture Whitepaper](https://github.com/openwallet-foundation/architecture-sig/blob/main/docs/papers/architecture-whitepaper.md)

---

## 1. Executive Summary

This document assesses SD-JWT .NET against the OWF Universal Wallet Reference Architecture Whitepaper. It identifies what the SDK already covers, what is within scope to improve, and -- critically -- what belongs in a separate wallet project rather than this repository.

**The whitepaper describes a universal wallet spanning identity, money, and objects.** It is technology-agnostic, delivery-mechanism-agnostic, and explicitly states that UI, platform infrastructure, DevOps, and external integration services are partly or fully out of scope for the logical architecture itself.

**SD-JWT .NET is a credential protocol SDK.** Its mission is to provide high-quality building blocks for issuing, holding, presenting, and verifying identity credentials using SD-JWT (RFC 9901), mdoc, OpenID4VC, DIF PEX, and OpenID Federation. It is not -- and should not become -- a full wallet application, a payment system, or an object inventory platform.

The whitepaper itself acknowledges this:

> "It is not expected that all OpenWallet Foundation projects will support all the requirements and capabilities listed herein."

This document takes that guidance seriously. Rather than chasing 100% whitepaper coverage by adding consent engines, P2P communication stacks, profile managers, and notification services into an SD-JWT library, it draws a clear boundary around what belongs here and what belongs in a downstream wallet project.

---

## 2. Scoping Principle: SDK vs. Wallet

### What this repo should be

An **identity credential protocol SDK** that a wallet (or any other application) consumes as a dependency. The packages should remain focused on:

- Credential formats (SD-JWT, mdoc)
- Issuance protocols (OID4VCI)
- Presentation protocols (OID4VP, DIF PEX)
- Trust infrastructure (OpenID Federation)
- Status and revocation (Status List)
- Compliance profiles (HAIP, EUDIW)
- Agent trust and policy (AgentTrust)
- Lightweight wallet orchestration (`SdJwt.Net.Wallet`) that ties these together

### What this repo should NOT become

A full wallet implementation. The following whitepaper capabilities are real and valid, but they belong in a **separate wallet project** that depends on this SDK:

| Capability                                       | Why it does not belong here                                                                               |
| ------------------------------------------------ | --------------------------------------------------------------------------------------------------------- |
| Consent management (UI, storage, lifecycle)      | Application-level concern; depends on UI framework, jurisdiction, and deployment model                    |
| Profile management (multi-persona)               | Application state management; varies per wallet product                                                   |
| Connection management (peer DIDs, sessions)      | Protocol-layer concern that spans beyond SD-JWT (DIDComm, CHAPI, etc.)                                    |
| Notification service (push, webhook, channels)   | Infrastructure concern tied to platform (iOS, Android, server)                                            |
| P2P communication (BLE, NFC, DIDComm)            | Transport-layer concern; depends on device capabilities                                                   |
| Offline processing and sync                      | Application-level state management                                                                        |
| Administrative agents (delegation, guardianship) | Application-level authorization; varies per deployment                                                    |
| Privacy-preserving edge computation              | Research-grade capability; orthogonal to credential formats                                               |
| Multi-purpose inventory (money, objects)         | Entirely outside the identity credential domain                                                           |
| Encrypted storage backends                       | Platform-specific (Keychain, Android Keystore, file system); SDK provides interfaces, not implementations |

### The boundary

```
+-----------------------------------------------------------------+
|                     Wallet Application                          |
|  (consent, profiles, connections, notifications, P2P, UI, ...)  |
|                                                                 |
|   Depends on:                                                   |
|   +-----------------------------------------------------------+ |
|   |                    sd-jwt-dotnet (this repo)               | |
|   |                                                             | |
|   |  Credential Formats    Protocols       Trust & Compliance   | |
|   |  - SdJwt.Net           - Oid4Vci       - OidFederation     | |
|   |  - SdJwt.Net.Vc        - Oid4Vp        - HAIP              | |
|   |  - SdJwt.Net.Mdoc      - PEX           - StatusList        | |
|   |  - SdJwt.Net.StatusList                 - Eudiw             | |
|   |                                                             | |
|   |  Agent Trust            Orchestration                       | |
|   |  - AgentTrust.Core      - Wallet                           | |
|   |  - AgentTrust.Policy                                       | |
|   |  - AgentTrust.Maf                                          | |
|   |  - AgentTrust.AspNetCore                                   | |
|   +-----------------------------------------------------------+ |
+-----------------------------------------------------------------+
```

---

## 3. Current Coverage of Whitepaper Capabilities

The table below maps every whitepaper capability to this repo, applying the scoping boundary from Section 2.

**Legend**:

- **Full** = fully implemented in this repo
- **Partial** = abstraction or basic support exists; improvements possible within scope
- **Interface Only** = this repo provides the interface; implementation belongs downstream
- **Out of Scope** = belongs in a wallet project, not this SDK

| Whitepaper Capability      | Status             | Package(s)                       | Assessment                                                        |
| -------------------------- | ------------------ | -------------------------------- | ----------------------------------------------------------------- |
| **Agents Layer**           |                    |                                  |                                                                   |
| Issuance Agent             | **Full**           | `Oid4Vci`, `Oid4Vci.AspNetCore`  | OID4VCI client + server; deferred issuance; batch credentials     |
| Presentation Agent         | **Full**           | `Oid4Vp`, `PresentationExchange` | OID4VP + DIF PEX v2.1.1; remote and proximity flows               |
| Policy Engine              | **Partial**        | `AgentTrust.Policy`              | Agent-trust policy; credential-level policy via PEX constraints   |
| P2P Agent                  | **Out of Scope**   | -                                | Transport-layer concern (BLE, NFC, DIDComm)                       |
| Privacy Preserving Edge    | **Out of Scope**   | -                                | Research-grade; orthogonal to credential formats                  |
| Orchestration Agent        | **Out of Scope**   | -                                | Multi-agent coordination is an application concern                |
| Administrative Agent       | **Out of Scope**   | -                                | Delegation and guardianship are application-level                 |
| **SDK and API Layer**      |                    |                                  |                                                                   |
| Keys                       | **Partial**        | `Wallet`                         | `IKeyManager` for sign/verify; lifecycle can be improved in-scope |
| Inventory                  | **Partial**        | `Wallet`                         | `ICredentialStore` for credential inventory; identity-scoped only |
| Transactions               | **Partial**        | `Wallet`                         | Issuance and presentation transaction flows                       |
| Audit Log                  | **Interface Only** | `Wallet`                         | `ITransactionLogger` interface; concrete impl is app-level        |
| Consents                   | **Out of Scope**   | -                                | Application-level; depends on UI and jurisdiction                 |
| Profiles                   | **Out of Scope**   | -                                | Application state management                                      |
| Connections                | **Out of Scope**   | -                                | Peer addressing spans beyond SD-JWT protocols                     |
| Wallet Transfer/Recovery   | **Out of Scope**   | -                                | Platform-specific backup and restore                              |
| Notifications              | **Out of Scope**   | -                                | Platform-specific push/webhook infrastructure                     |
| Messaging                  | **Out of Scope**   | -                                | DIDComm is a separate protocol stack                              |
| **Wallet Services Layer**  |                    |                                  |                                                                   |
| Key Management Service     | **Partial**        | `Wallet`                         | Same as SDK Keys above                                            |
| Inventory Management       | **Partial**        | `Wallet`                         | Same as SDK Inventory above                                       |
| Transactions Processing    | **Partial**        | `Wallet`                         | Same as SDK Transactions above                                    |
| Audit Log Service          | **Interface Only** | `Wallet`                         | Same as SDK Audit Log above                                       |
| All other services         | **Out of Scope**   | -                                | Consent, Profile, Connection, Transfer, Notification mgmt         |
| **Secure Storage Layer**   |                    |                                  |                                                                   |
| Key Storage                | **Interface Only** | `Wallet`                         | `IKeyManager` abstraction; backends are platform-specific         |
| Credential Storage         | **Interface Only** | `Wallet`                         | `ICredentialStore` abstraction; backends are platform-specific    |
| Data Storage               | **Out of Scope**   | -                                | General document storage is not credential-specific               |
| Custodial Key Storage      | **Out of Scope**   | -                                | Custodial MPC is a specialized infrastructure concern             |
| **Network Services Layer** |                    |                                  |                                                                   |
| Trust Registry             | **Full**           | `OidFederation`                  | Trust chain resolution, validation, trust marks, metadata policy  |
| Connectors                 | **Partial**        | `OidFederation`                  | HTTP entity retrieval for federation                              |
| Indexers                   | **Out of Scope**   | -                                | General caching infrastructure                                    |
| Offline Synchronizers      | **Out of Scope**   | -                                | Application-level state sync                                      |
| **P2P Services Layer**     |                    |                                  |                                                                   |
| Offline Processing         | **Out of Scope**   | -                                | Application-level offline capability                              |
| P2P Communication          | **Out of Scope**   | -                                | Transport concern (BLE, NFC, DIDComm)                             |

### Principles Alignment

| Whitepaper Principle | Assessment         | Details                                                                                            |
| -------------------- | ------------------ | -------------------------------------------------------------------------------------------------- |
| **Privacy**          | **Strong**         | Selective disclosure, decoy digests, herd privacy, minimal disclosure via PEX predicates           |
| **Interoperable**    | **Strong**         | Multi-protocol (OID4VCI, OID4VP, DIF PEX), multi-format (SD-JWT, mdoc), federation trust           |
| **Open**             | **Strong**         | Open source under OWF Labs, open standards, detailed documentation                                 |
| **Multi-Purpose**    | **Partial**        | Modular identity credential SDK; money and objects are out of scope and should stay that way       |
| **Portable**         | **Interface Only** | SDK provides `ICredentialStore` and `IKeyManager` abstractions; actual backup/restore is app-level |
| **Equitable Access** | **Partial**        | Multi-platform via netstandard2.1; accessibility/i18n are UI concerns                              |

---

## 4. In-Scope Improvements

These are improvements that strengthen the SDK's role as a credential protocol building block without turning it into a wallet application.

### 4.1 Key Lifecycle in `SdJwt.Net.Wallet`

**Current state**: `IKeyManager` supports sign and verify. No key generation, rotation, expiration, or listing.

**Proposed improvement**: Extend with lifecycle operations that credential protocols need.

```csharp
/// <summary>
/// Extended key management with lifecycle operations relevant to credential protocols.
/// </summary>
public interface IKeyLifecycleManager : IKeyManager
{
    Task<KeyInfo> GenerateKeyAsync(KeyGenerationOptions options, CancellationToken ct = default);
    Task RotateKeyAsync(string keyId, KeyRotationOptions options, CancellationToken ct = default);
    Task<IReadOnlyList<KeyInfo>> ListKeysAsync(KeyFilter filter, CancellationToken ct = default);
    Task<KeyInfo> GetKeyInfoAsync(string keyId, CancellationToken ct = default);
}
```

**Why this belongs here**: Key generation and rotation are tightly coupled to credential issuance and presentation. An issuer needs to rotate signing keys; a holder needs to bind new keys when rotating. These are protocol-level operations, not application-level.

### 4.2 Credential Store Improvements in `SdJwt.Net.Wallet`

**Current state**: `ICredentialStore` provides basic CRUD. No metadata queries, no format-aware filtering.

**Proposed improvement**: Add query capabilities that credential presentation protocols need.

```csharp
/// <summary>
/// Extended credential store with query capabilities for presentation matching.
/// </summary>
public interface ICredentialInventory : ICredentialStore
{
    Task<IReadOnlyList<StoredCredential>> QueryAsync(CredentialQuery query, CancellationToken ct = default);
    Task<IReadOnlyList<StoredCredential>> FindMatchingAsync(
        PresentationDefinition definition, CancellationToken ct = default);
}
```

**Why this belongs here**: OID4VP and DIF PEX need to search credentials by type, issuer, claims, and format to satisfy presentation requests. This is a protocol concern.

### 4.3 Transaction Logging in `SdJwt.Net.Wallet`

**Current state**: `ITransactionLogger` interface exists with no concrete implementation.

**Proposed improvement**: Ship a reference in-memory implementation and a structured event model.

```csharp
/// <summary>
/// Structured audit entry for credential operations.
/// </summary>
public sealed class CredentialAuditEntry
{
    public required string EntryId { get; init; }
    public required DateTimeOffset Timestamp { get; init; }
    public required CredentialOperation Operation { get; init; }
    public required string CredentialId { get; init; }
    public string? CounterpartyId { get; init; }
    public IReadOnlyList<string>? DisclosedClaims { get; init; }
}
```

**Why this belongs here**: Knowing which claims were disclosed to which verifier is a credential-protocol concern, not an application concern. The SDK should model this; the wallet app decides how to persist and display it.

### 4.4 Status List Integration in `SdJwt.Net.Wallet`

**Current state**: `SdJwt.Net.StatusList` exists as a standalone package. Wallet does not automatically check credential status.

**Proposed improvement**: Wire status checking into the presentation flow so credentials with revoked status are flagged before presentation.

**Why this belongs here**: Status checking during presentation is defined by the credential protocol specifications.

### 4.5 HAIP/EUDIW Compliance Hooks

**Current state**: `SdJwt.Net.HAIP` and `SdJwt.Net.Eudiw` enforce algorithm and profile constraints.

**Proposed improvement**: Ensure the `Wallet` orchestration layer can accept a compliance profile and apply it across issuance and presentation flows automatically.

**Why this belongs here**: Compliance enforcement is part of the protocol stack, not the wallet application.

---

## 5. Interface Contracts for Downstream Wallets

The SDK should provide well-designed interfaces (already partially in place) that downstream wallet projects implement. The SDK does not implement these beyond in-memory test doubles.

| Interface                                   | Purpose                | Downstream implements                               |
| ------------------------------------------- | ---------------------- | --------------------------------------------------- |
| `IKeyManager` / `IKeyLifecycleManager`      | Key operations         | HSM, Android Keystore, Apple Keychain, TPM backends |
| `ICredentialStore` / `ICredentialInventory` | Credential persistence | SQLite, encrypted file, cloud-backed storage        |
| `ITransactionLogger`                        | Audit trail            | Database, log aggregation, compliance reporting     |

These interfaces form the **contract surface** between this SDK and a wallet application. The SDK defines the shape; the wallet fills it in.

---

## 6. What a Downstream Wallet Project Would Own

For completeness, these are the whitepaper capabilities that belong in a separate OWF wallet project (not this repo):

| Capability                 | Notes                                                                                         |
| -------------------------- | --------------------------------------------------------------------------------------------- |
| Consent management         | Full lifecycle: grant, revoke, check, expire. Depends on jurisdiction (GDPR vs. other) and UI |
| Profile management         | Multi-persona with credential isolation. Application-level state                              |
| Connection management      | Peer DID sessions, endpoint management, DIDComm connections                                   |
| Notifications              | Push, webhook, in-app. Platform-specific (APNs, FCM, SignalR)                                 |
| Backup and recovery        | Encrypted export/import. Depends on cloud provider and platform APIs                          |
| P2P communication          | BLE, NFC, Wi-Fi Direct transports                                                             |
| Messaging                  | DIDComm v2 or similar message exchange                                                        |
| Offline processing         | Conflict resolution, change tracking, sync                                                    |
| Administrative agents      | Delegation, guardianship, multi-party control                                                 |
| Multi-purpose inventory    | Money, objects, tokens -- entirely different domain                                           |
| Encrypted storage backends | Platform keystores, database encryption, filesystem encryption                                |

A downstream wallet project would:

1. Reference `SdJwt.Net.*` packages as NuGet dependencies
2. Implement `IKeyManager`, `ICredentialStore`, `ITransactionLogger` with real backends
3. Add application-level services (consent, profiles, connections, notifications)
4. Provide UI (mobile, web, CLI) appropriate to the deployment context
5. Handle platform-specific concerns (biometric auth, secure enclave, push notifications)

---

## 7. Recommendations

### For this repo (sd-jwt-dotnet)

1. **Implement the in-scope improvements** from Section 4: key lifecycle, credential query, transaction logging, status integration, compliance hooks
2. **Keep interfaces clean** so downstream wallets can implement them without friction
3. **Do not add new `SdJwt.Net.Wallet.*` sub-packages** for consent, profiles, connections, notifications, P2P, messaging, or recovery -- these are wallet-application concerns
4. **Document the interface contracts** (Section 5) clearly so wallet projects know what to implement
5. **Provide a sample wallet** in `samples/` that demonstrates a minimal integration, not a production wallet

### For the OWF community

1. **Consider a separate `owf-wallet-dotnet` project** (or similar) for the full wallet implementation that consumes this SDK
2. **Use this SDK's interfaces** as the credential-layer contract in that wallet project
3. **Avoid duplicating credential protocol logic** in the wallet project; delegate to this SDK

---

## 8. Revised Coverage Assessment

Measuring against whitepaper capabilities that are **in scope for a credential SDK**:

| In-Scope Capability            | Current        | After Improvements           |
| ------------------------------ | -------------- | ---------------------------- |
| Issuance Agent                 | Full           | Full                         |
| Presentation Agent             | Full           | Full                         |
| Trust Registry                 | Full           | Full                         |
| Policy Engine                  | Partial        | Partial (agent-trust scoped) |
| Key Management                 | Partial        | Full (with lifecycle)        |
| Credential Inventory           | Partial        | Full (with query)            |
| Transaction Processing         | Partial        | Full (with status checks)    |
| Audit Log                      | Interface Only | Interface + Reference Impl   |
| Connectors                     | Partial        | Partial (federation-scoped)  |
| Key Storage (interface)        | Interface Only | Interface Only               |
| Credential Storage (interface) | Interface Only | Interface Only               |

**In-scope coverage: 8 of 11 capabilities at Full after improvements (73%).**

The remaining ~25 whitepaper capabilities are **correctly out of scope** for this SDK and belong in a wallet application project.

---

## Appendix A: Whitepaper Reference

- **Title**: Universal Wallet Reference Architecture Whitepaper
- **Source**: [OWF Architecture SIG](https://github.com/openwallet-foundation/architecture-sig/blob/main/docs/papers/architecture-whitepaper.md)
- **Status**: Working Draft
- **Key Quote**: "It is not expected that all OpenWallet Foundation projects will support all the requirements and capabilities listed herein."
- **Key Quote**: "The architecture does not assume a specific technology or delivery mechanism."
