# Trusted Release Support A2A Demo

Demonstrates governed agent-to-agent (A2A) delegation using SD-JWT capability
tokens from the `SdJwt.Net.AgentTrust` packages.

A **Coordinator** agent delegates a scoped investigation capability to an
**Investigator** agent, which queries real GitHub and NuGet APIs to diagnose
NuGet release pipeline issues.

This sample complements the MCP Tool Governance demo:

- **MCP Tool Governance**: protects agent-to-tool calls.
- **Trusted Release Support A2A**: protects agent-to-agent delegation.

## What This Demo Proves

```mermaid
flowchart LR
    A[A2A] --> B[Agents can communicate]
    C[SD-JWT Capability Token] --> D[The caller has delegated authority]
    E[iss claim + TrustedIssuers] --> F[The caller identity is cryptographically verified]
    G[GitHub + NuGet APIs] --> H[The investigation uses real external systems]

    B --> I[Governed Release Investigation]
    D --> I
    F --> I
    H --> I
```

The core message is:

> A2A lets agents communicate.
> The SD-JWT capability token carries both identity (`iss` claim) and delegated
> authority (`cap` claim) in a single signed artefact.
> The server verifies both using the SDK's `AgentTrustVerificationMiddleware`.

---

## Architecture

```mermaid
flowchart LR
    User[Support User]

    subgraph Coordinator["ReleaseSupport.Coordinator<br/>Console App"]
        CoordAgent[Coordinator Agent]
        Policy[Policy Pre-flight]
        CapIssuer[SD-JWT Delegation Issuer]
        A2AClient[A2A Client]
    end

    subgraph Investigator["ReleaseSupport.InvestigatorA2A<br/>ASP.NET Core"]
        A2AHost[A2A Endpoints]
        SDKMiddleware[AgentTrustVerificationMiddleware<br/>Authorization: Bearer]
        PolicyEval[Policy Engine]
        InvAgent[Investigator Agent]
        Diagnosis[Release Diagnosis Service]
        Nonce[Replay / Nonce Store]
    end

    subgraph External["External Public APIs"]
        GitHub[GitHub REST API]
        NuGet[NuGet V3 API]
    end

    User --> CoordAgent
    CoordAgent --> Policy
    Policy --> CapIssuer
    CoordAgent --> A2AClient

    A2AClient -->|GET /a2a/.../card| A2AHost
    A2AClient -->|POST /a2a/.../message:stream<br/>Authorization: Bearer capability-token| A2AHost

    A2AHost --> SDKMiddleware
    SDKMiddleware --> Nonce
    SDKMiddleware --> PolicyEval
    SDKMiddleware --> InvAgent
    InvAgent --> Diagnosis
    Diagnosis --> GitHub
    Diagnosis --> NuGet
```

---

## End-to-End Flow

```mermaid
sequenceDiagram
    autonumber

    participant U as Support User
    participant C as Coordinator
    participant P as Policy Engine
    participant S as SD-JWT Delegation Issuer
    participant A as Investigator A2A Server
    participant G as GitHub API
    participant N as NuGet API

    U->>C: Investigate package release
    C->>A: GET /a2a/release-investigator/v1/card
    A-->>C: Agent Card

    C->>P: Check delegation policy
    P-->>C: Allow InvestigatePackageRelease

    C->>S: Mint delegated SD-JWT capability
    S-->>C: Capability token (iss = coordinator identity)

    C->>A: POST /a2a/release-investigator/v1/message:stream<br/>Authorization: Bearer capability-token

    A->>A: SDK middleware: extract token from Authorization header
    A->>A: Verify SD-JWT signature, expiry, audience
    A->>A: Check nonce / replay protection
    A->>A: Verify issuer against TrustedIssuers
    A->>A: Evaluate policy engine

    A->>G: Check tag, release, workflow
    G-->>A: GitHub release state

    A->>N: Check package metadata
    N-->>A: NuGet package state

    A-->>C: Investigation result
    C-->>U: Display diagnosis
```

---

## A2A Boundary

```mermaid
flowchart TB
    subgraph Coordinator["Coordinator Agent"]
        Request[Investigation Request]
        Delegation[Delegated Capability]
    end

    subgraph A2A["A2A Protocol Boundary"]
        Card["GET /a2a/release-investigator/v1/card"]
        Message["POST /a2a/release-investigator/v1/message:stream"]
    end

    subgraph Investigator["Investigator Agent"]
        Verify[SDK Middleware: Verify Token + Policy]
        Execute[Execute Investigation]
    end

    Request --> Card
    Card --> Message
    Delegation --> Message
    Message --> Verify
    Verify --> Execute
```

The A2A request carries a single security artefact:

```http
Authorization: Bearer <sd-jwt-capability-token>
```

The capability token carries both identity and authority:

- **`iss` claim** answers: Which agent is calling?
- **`cap` claim** answers: What is this agent allowed to delegate right now?

The server verifies the token using the SDK's `AgentTrustVerificationMiddleware`,
which reads `Authorization: Bearer` by default, matching the RFC 6750 bearer
token scheme.

---

## Projects

| Project                          | Description                                        |
| -------------------------------- | -------------------------------------------------- |
| `ReleaseSupport.Shared`          | Constants, DTOs, A2A message models                |
| `ReleaseSupport.InvestigatorA2A` | ASP.NET Core A2A server using SDK middleware       |
| `ReleaseSupport.Coordinator`     | Console client that orchestrates the investigation |

---

## Running

### Prerequisites

- .NET 9.0+ SDK
- No paid cloud services required
- Public internet access for GitHub and NuGet APIs

The default mode uses `DevFallbackStatic`, so the demo can run locally without
Microsoft Entra setup.

---

### Step 1: Start the Investigator Server

```bash
cd samples/EntraA2AReleaseSupportDemo/ReleaseSupport.InvestigatorA2A
dotnet run
```

The server starts on:

```text
http://localhost:5052
```

The A2A endpoints are:

```http
GET  http://localhost:5052/a2a/release-investigator/v1/card
POST http://localhost:5052/a2a/release-investigator/v1/message:stream
```

---

### Step 2: Run the Coordinator

In a separate terminal:

```bash
cd samples/EntraA2AReleaseSupportDemo/ReleaseSupport.Coordinator
```

Happy path:

```bash
dotnet run -- \
  --repo openwallet-foundation-labs/sd-jwt-dotnet \
  --package SdJwt.Net \
  --version 1.0.1 \
  --action InvestigatePackageRelease
```

Blocked path:

```bash
dotnet run -- \
  --repo openwallet-foundation-labs/sd-jwt-dotnet \
  --package SdJwt.Net \
  --version 1.0.1 \
  --action RerunWorkflow
```

---

## CLI Arguments

| Argument             | Default                                    | Description                                  |
| -------------------- | ------------------------------------------ | -------------------------------------------- |
| `--repo`             | `openwallet-foundation-labs/sd-jwt-dotnet` | GitHub repository in `owner/name` format     |
| `--package`          | `SdJwt.Net`                                | NuGet package ID                             |
| `--version`          | `1.0.1`                                    | Version to investigate                       |
| `--action`           | `InvestigatePackageRelease`                | Tool action to request                       |
| `--identity-mode`    | `DevFallbackStatic`                        | Identity mode (for future Entra integration) |
| `--investigator-url` | `http://localhost:5052`                    | Investigator base URL                        |

---

## Identity Modes

```mermaid
flowchart LR
    Mode[Identity Mode]

    Mode --> Static[DevFallbackStatic]
    Mode --> AppReg[DevFallbackAppRegistration]
    Mode --> RealEntra[RealEntra]

    Static --> S1[Uses local static agent URI<br/>Identity embedded as iss in capability token]
    AppReg --> S2[Uses Microsoft Entra app registration<br/>Good fallback for OAuth testing]
    RealEntra --> S3[Uses Microsoft Entra Agent ID<br/>Best enterprise demo mode]
```

| Mode                         | Description                                                                        | Requirements                          |
| ---------------------------- | ---------------------------------------------------------------------------------- | ------------------------------------- |
| `DevFallbackStatic`          | Coordinator identity is a static agent URI, embedded as the capability token `iss` | None                                  |
| `DevFallbackAppRegistration` | Uses a Microsoft Entra app registration                                            | Entra app registration                |
| `RealEntra`                  | Uses Microsoft Entra Agent ID                                                      | Tenant support and required licensing |

> `DevFallbackStatic` is for local development only. It demonstrates the protocol
> and capability model, not real enterprise identity.

---

## Capability Token Model

The Coordinator mints a short-lived delegated capability before calling the
Investigator.

```mermaid
classDiagram
    class CapabilityToken {
        string iss
        string aud
        long iat
        long exp
        string jti
        Capability cap
        Context ctx
    }

    class Capability {
        string type
        string fromAgent
        string toAgent
        string tool
        string action
        string repository
        string packageId
        string version
    }

    class Context {
        string workflowId
        string correlationId
        string purpose
    }

    CapabilityToken --> Capability
    CapabilityToken --> Context
```

Example capability payload:

```json
{
  "iss": "agent://entra/dev-local/release-support-coordinator-dev",
  "aud": "agent://entra/dev-local/release-investigator-dev",
  "iat": 1778370000,
  "exp": 1778370300,
  "jti": "cap-a2a-7f2a3e",
  "cap": {
    "type": "agent-delegation",
    "fromAgent": "release-support-coordinator-dev",
    "toAgent": "release-investigator-dev",
    "tool": "release-investigation",
    "action": "InvestigatePackageRelease",
    "repository": "openwallet-foundation-labs/sd-jwt-dotnet",
    "packageId": "SdJwt.Net",
    "version": "1.0.1"
  },
  "ctx": {
    "workflowId": "release-support",
    "correlationId": "corr-123456",
    "purpose": "agent-delegation"
  }
}
```

---

## Server-Side Validation

The server uses the SDK's `AgentTrustVerificationMiddleware` which performs
all validation automatically. The middleware is configured via
`AddAgentTrustVerification()` and `UseAgentTrustVerification()`.

```mermaid
flowchart TD
    Start[Incoming A2A Request]

    Start --> HasBearer{Authorization: Bearer present?}
    HasBearer -- No --> Deny401[401 Unauthorized]
    HasBearer -- Yes --> VerifySig[Verify SD-JWT signature]

    VerifySig --> CheckExpiry{Expired?}
    CheckExpiry -- Yes --> Deny403B[403 Expired capability]
    CheckExpiry -- No --> CheckAudience{Audience matches investigator?}

    CheckAudience -- No --> Deny403C[403 Invalid audience]
    CheckAudience -- Yes --> CheckIssuer{Issuer in TrustedIssuers?}

    CheckIssuer -- No --> Deny403D[403 Untrusted issuer]
    CheckIssuer -- Yes --> CheckReplay{JTI already used?}

    CheckReplay -- Yes --> Deny403F[403 Replay detected]
    CheckReplay -- No --> PolicyEval{Policy engine allows action?}

    PolicyEval -- No --> Deny403G[403 Action denied by policy]
    PolicyEval -- Yes --> WriteReceipt[Write audit receipt]

    WriteReceipt --> Allow[Allow investigation]
```

---

## Demo Scenarios

### Scenario 1: Happy Path -- Published Package

```bash
dotnet run -- --version 1.0.1 --action InvestigatePackageRelease
```

Expected:

```text
Tag found.
Release found.
NuGet package found.
Diagnosis: Package published successfully.
```

---

### Scenario 2: Happy Path -- Unreleased Version

```bash
dotnet run -- --version 99.0.0 --action InvestigatePackageRelease
```

Expected:

```text
Tag not found.
Diagnosis: Version was never tagged.
```

---

### Scenario 3: Blocked Path -- Denied Action

```bash
dotnet run -- --action RerunWorkflow
```

Expected:

```text
Policy engine blocks delegation at step 2.
No A2A request is sent.
```

---

### Scenario 4: Blocked Path -- Capability Tampering

Capability says:

```text
version = 1.0.1
```

Request asks:

```text
version = 2.0.0
```

Expected:

```text
Investigator rejects the request.
Reason: Capability resource does not match requested resource.
```

---

### Scenario 5: Blocked Path -- Replay

The same SD-JWT capability token is reused.

Expected:

```text
Investigator rejects the request.
Reason: Capability token has already been used.
```

---

## Diagnosis Rules

```mermaid
flowchart TD
    Start[Start Diagnosis]

    Start --> Tag{GitHub tag exists?}
    Tag -- No --> NeverTagged[Diagnosis: Version was never tagged]

    Tag -- Yes --> Release{GitHub release exists?}
    Release -- No --> NoRelease[Diagnosis: Tag exists but release was not created]

    Release -- Yes --> Workflow{Publish workflow status?}
    Workflow -- Failed --> PublishFailed[Diagnosis: Release exists but publish workflow failed]

    Workflow -- Success --> NuGet{NuGet package version exists?}
    NuGet -- Yes --> Published[Diagnosis: Package published successfully]
    NuGet -- No --> MissingNuGet[Diagnosis: Publish may have skipped package, used wrong package ID, or NuGet indexing is delayed]

    Workflow -- Unknown --> Unknown[Diagnosis: Unable to determine workflow state]
```

---

## Security Properties

| Property               | Implementation                                                           |
| ---------------------- | ------------------------------------------------------------------------ |
| Scoped delegation      | Capability token contains tool, action, repository, package, and version |
| Time-limited tokens    | 5-minute default lifetime                                                |
| Replay prevention      | Nonce store tracks used token IDs                                        |
| Cryptographic identity | `iss` claim verified against `TrustedIssuers` signing keys               |
| Policy enforcement     | Client-side pre-flight and server-side middleware evaluation             |
| Resource binding       | Request repo/package/version must match the capability                   |
| Deny-by-default        | Unsupported actions are rejected before delegation                       |
| Audit receipts         | SDK middleware emits receipts for every decision                         |

---

## Trust Boundary Summary

```mermaid
flowchart LR
    subgraph Local["Coordinator Trust Boundary"]
        UserInput[User Input]
        Policy[Policy Decision]
        Capability[Capability Minting<br/>iss = coordinator identity]
    end

    subgraph Network["A2A Network Boundary"]
        SDJWT[Authorization: Bearer<br/>SD-JWT Capability Token]
    end

    subgraph Remote["Investigator Trust Boundary"]
        SDKMiddleware[SDK AgentTrustVerificationMiddleware]
        VerifyToken[Verify signature + expiry + audience]
        VerifyIssuer[Verify iss against TrustedIssuers]
        EvalPolicy[Evaluate policy engine]
        Execute[Execute External API Calls]
    end

    UserInput --> Policy
    Policy --> Capability
    Capability --> SDJWT
    SDJWT --> SDKMiddleware
    SDKMiddleware --> VerifyToken
    VerifyToken --> VerifyIssuer
    VerifyIssuer --> EvalPolicy
    EvalPolicy --> Execute
```

---

## Packages Used

| Package                              | Purpose                                                                    |
| ------------------------------------ | -------------------------------------------------------------------------- |
| `SdJwt.Net.AgentTrust.Core`          | Token minting, verification, nonce store                                   |
| `SdJwt.Net.AgentTrust.Policy`        | Rule-based policy engine                                                   |
| `SdJwt.Net.AgentTrust.A2A`           | Delegation issuer with policy pre-flight                                   |
| `SdJwt.Net.AgentTrust.AspNetCore`    | SDK middleware (`AddAgentTrustVerification` / `UseAgentTrustVerification`) |
| `SdJwt.Net.AgentTrust.OpenTelemetry` | Tracing and metrics                                                        |

---

## Recommended Next Steps

```mermaid
flowchart LR
    P1[Phase 1<br/>DevFallbackStatic] --> P2[Phase 2<br/>Entra App Registration]
    P2 --> P3[Phase 3<br/>Real Entra Agent ID]
    P3 --> P4[Phase 4<br/>Optional LLM UX]
    P4 --> P5[Phase 5<br/>Enterprise API Demo]
```

1. Start with `DevFallbackStatic` so the sample is easy to run.
2. Add `DevFallbackAppRegistration` to demonstrate real OAuth token validation.
3. Add `RealEntra` to demonstrate Microsoft Entra Agent ID.
4. Add optional LLM support for natural-language investigation requests.
5. Extend the same pattern to real enterprise tools such as ServiceNow, D365, Salesforce, APIM, or Azure Monitor.
