# Agent Trust Operations

> **Level:** Advanced preview extension

## Simple explanation

This page covers the operational aspects of Agent Trust: how tokens are minted and verified (Core), how policy rules are evaluated, how replay is prevented, how audit receipts are written, how telemetry is collected, and how to deploy Agent Trust in different environments.

|                      |                                                                                                                                                                         |
| -------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Audience**         | Platform engineers deploying and operating Agent Trust in development, staging, and production environments.                                                            |
| **Purpose**          | Provide the class reference for Core, Policy, OTel, and OPA packages, plus operational guidance for replay stores, key custody, audit receipts, and deployment modes.   |
| **Scope**            | Core/Policy/OTel/OPA classes, nonce stores, key custody, deployment modes, operational checklist. Out of scope: MCP/A2A/AspNetCore classes (see their dedicated pages). |
| **Success criteria** | Reader can select the right deployment mode, configure nonce stores and key custody, and set up audit receipt storage for their environment.                            |

## Core classes

| Class                           | Purpose                                                                          |
| ------------------------------- | -------------------------------------------------------------------------------- |
| `CapabilityTokenIssuer`         | Mints SD-JWT capability tokens with scoped claims                                |
| `CapabilityTokenVerifier`       | Verifies tokens: signature, expiry, audience, replay, capability claims          |
| `CapabilityClaim`               | The `cap` claim: tool, toolId, action, resource, limits, delegationDepth         |
| `CapabilityContext`             | The `ctx` claim: correlationId, workflowId, stepId, tenantId, dataClassification |
| `CapabilityLimits`              | Machine-enforceable limits: maxResults, maxInvocations, maxPayloadBytes          |
| `CapabilityTokenOptions`        | Configuration for minting: issuer, audience, capability, context, lifetime       |
| `CapabilityVerificationOptions` | Configuration for verification: audience, trusted issuers, replay, algorithms    |
| `CapabilityVerificationResult`  | Typed verification result with capability, context, error details                |
| `AgentTrustVerificationContext` | Enterprise verification context: security mode, allowed algorithms, tenant       |
| `AgentTrustSecurityMode`        | Security mode enum: Demo, Pilot, Production                                      |
| `MemoryNonceStore`              | In-process nonce/replay store backed by `IMemoryCache`                           |
| `InMemoryKeyCustodyProvider`    | Development key custody; replace with KMS/HSM in production                      |
| `LoggingReceiptWriter`          | Receipt writer that emits structured logs via `ILogger`                          |
| `WorkloadIdentity`              | Binds capability tokens to a workload identity (subjectId, provider, issuer)     |
| `SenderConstraint`              | Proof-of-possession via DPoP or mTLS `cnf` claims                                |
| `RequestBinding`                | Binds a capability token to a specific HTTP request (method, URI, body hash)     |
| `DelegationEvidence`            | Evidence of delegation: parentTokenId, rootIssuer, depth, maxDepth               |
| `ApprovalEvidence`              | Human-in-the-loop approval: approver, timestamp, mechanism, scope                |
| `ToolRegistryEntry`             | Tool metadata: toolId, manifestHash, audience, actions, version                  |
| `CapabilityTemplate`            | Pre-approved capability patterns: templateId, constraints, maxUses               |
| `CapabilityReceipt`             | Structured audit receipt: receiptId, tokenId, event, decision, timestamp         |
| `ReceiptPartitionKey`           | Partition key for receipts: tenantId, date, issuer                               |
| `PolicyObligation`              | Post-decision obligation: type, parameters, enforcement                          |
| `DecisionEffect`                | Policy decision effect enum: Permit, Deny, Indeterminate                         |
| `TokenProfileConstants`         | Token type identifiers: `agent-cap+sd-jwt`, `agent-cap+sd-jwt+demo`              |
| `AgentTrustClaimNames`          | Claim name constants: `cap`, `ctx`, `cnf`, `req_bind`, etc.                      |
| `AgentTrustActivitySource`      | OpenTelemetry `ActivitySource` for distributed tracing                           |

## Policy engine

| Class                      | Purpose                                                                   |
| -------------------------- | ------------------------------------------------------------------------- |
| `DefaultPolicyEngine`      | Deterministic first-match rule engine                                     |
| `PolicyBuilder`            | Fluent API for constructing allow/deny rules                              |
| `PolicyRule`               | A single allow/deny rule with agent/tool/action patterns                  |
| `PolicyConstraintsBuilder` | Builder for per-rule constraints (lifetime, limits, disclosure)           |
| `PolicyRequest`            | Policy evaluation input with userId, tenantId, toolId, dataClassification |
| `PolicyDecision`           | Policy evaluation result with `DecisionEffect`, obligations, policy hash  |
| `PolicyObligation`         | Post-decision obligation: type, parameters, enforcement point             |
| `DecisionEffect`           | Decision enum: Permit, Deny, Indeterminate                                |
| `DelegationChain`          | Tracks delegation depth and allowed actions across agent hops             |

Policy rules are deterministic and can be unit tested. The engine evaluates rules in order and returns the first match. If no rule matches, the default action applies (configurable as allow or deny; deny is recommended for production).

## Replay prevention

The `INonceStore` interface tracks used `jti` values. On first use, the store records the `jti`. On subsequent use within the token lifetime plus clock skew, the store rejects the token.

| Implementation     | Scope          | Use case                              |
| ------------------ | -------------- | ------------------------------------- |
| `MemoryNonceStore` | Single process | Development and testing               |
| Redis-backed store | Distributed    | Multi-instance production deployments |
| SQL-backed store   | Distributed    | Environments without Redis            |

> The in-memory store is single-process only. Multi-instance deployments must use a distributed nonce store to prevent replay across instances.

## Audit receipts

Every allow/deny decision produces a structured receipt containing:

- Token `jti`, tool, action, timestamp, decision (allow/deny)
- Correlation ID for end-to-end workflow tracing
- Optional request/response metadata hashes (no PII)
- Evaluation duration for performance monitoring

Receipts should be stored in an append-only system: event streaming (Kafka, Event Hub) or a structured database (Cosmos DB, PostgreSQL). Ephemeral logs alone are insufficient for audit and compliance.

## OpenTelemetry integration

| Class                                 | Purpose                                                             |
| ------------------------------------- | ------------------------------------------------------------------- |
| `AgentTrustMetrics`                   | Static `Meter` with counters, histograms, and `ActivitySource`      |
| `AgentTrustInstrumentationExtensions` | `AddAgentTrustInstrumentation` extension for `MeterProviderBuilder` |
| `TelemetryReceiptWriter`              | Emits audit receipts as OpenTelemetry counter increments            |

### Metrics (per spec Section 24.1)

| Metric name                                 | Type      | Description                         |
| ------------------------------------------- | --------- | ----------------------------------- |
| `agent_trust.capability.minted`             | Counter   | Capability tokens minted            |
| `agent_trust.capability.verified`           | Counter   | Capability tokens verified          |
| `agent_trust.capability.rejected`           | Counter   | Capability tokens rejected          |
| `agent_trust.policy.evaluated`              | Counter   | Policy evaluations                  |
| `agent_trust.replay.detected`               | Counter   | Replay attempts detected            |
| `agent_trust.pop.failed`                    | Counter   | Proof-of-possession failures        |
| `agent_trust.request_binding.failed`        | Counter   | Request binding validation failures |
| `agent_trust.receipt.written`               | Counter   | Audit receipts written              |
| `agent_trust.mint.duration_ms`              | Histogram | Mint operation duration in ms       |
| `agent_trust.verify.duration_ms`            | Histogram | Verify operation duration in ms     |
| `agent_trust.policy.evaluation_duration_ms` | Histogram | Policy evaluation duration in ms    |

### Distributed tracing activities

`AgentTrustMetrics` exposes `Start*Activity` methods for structured tracing:

`StartMintActivity`, `StartVerifyActivity`, `StartPolicyEvaluateActivity`, `StartRegistryResolveActivity`, `StartReplayConsumeActivity`, `StartReceiptWriteActivity`, `StartDelegationValidateActivity`, `StartPopValidateActivity`, `StartRequestBindingValidateActivity`

Metrics include token mint count, verification count, policy evaluation count, and evaluation duration histograms. All metrics are tagged with tool name and decision (allow/deny) for dashboard filtering.

## OPA external policy

| Class                  | Purpose                                                             |
| ---------------------- | ------------------------------------------------------------------- |
| `OpaOptions`           | Configuration: base URL, policy path, timeout, fail-closed behavior |
| `OpaHttpPolicyEngine`  | `IPolicyEngine` implementation that evaluates policy via OPA HTTP   |
| `OpaServiceExtensions` | `AddOpaPolicy` DI registration extension                            |

Use OPA when policy rules are managed by a central team, need to be updated without redeploying the application, or are shared across services written in different languages.

## Key custody

| Provider                     | Scope       | Use case                                  |
| ---------------------------- | ----------- | ----------------------------------------- |
| `InMemoryKeyCustodyProvider` | Development | Local testing; keys in memory             |
| Azure Key Vault              | Production  | HSM-backed keys with audit logging        |
| AWS KMS / GCP Cloud KMS      | Production  | Cloud-managed keys                        |
| On-premises HSM              | Production  | Regulatory requirements for key isolation |

Configure separate signing keys per agent identity. Do not share keys across agents. Rotate keys on a regular cadence and maintain `kid` (Key ID) consistency so receivers can resolve the correct public key.

## Core security properties

1. **Signature validation** - Every capability token is an SD-JWT signed by the issuing agent's key. The receiver cryptographically verifies the signature against whitelisted issuer keys before processing any claims.

2. **Audience binding** - The `aud` claim binds the token to a specific tool or agent endpoint. A token minted for `tool://payments` is rejected by `tool://billing`. This prevents confused-deputy attacks.

3. **Expiry enforcement** - Tokens have lifetimes measured in seconds to minutes (default: 60 seconds). Clock skew tolerance is configurable (default: 30 seconds). There is no reuse window after expiry.

4. **Replay prevention** - The `jti` claim is a unique identifier. The `INonceStore` records it on first use. Any subsequent use of the same `jti` is rejected, even if the token has not yet expired.

5. **Capability-level constraints** - The `cap` claim carries machine-readable limits (`maxResults`, `maxInvocations`, `maxPayloadBytes`). The tool server is expected to enforce these in its application logic.

## Deployment modes

### Mode 1 - SDK-only (fast adoption)

Agent references `SdJwt.Net.AgentTrust.Core` + `SdJwt.Net.AgentTrust.Policy`. Tool references `SdJwt.Net.AgentTrust.AspNetCore`. Keys in app configuration.

Best for: development, proof of concept, small-scale deployments.

### Mode 2 - Sidecar trust daemon (enterprise hardening)

Agent and tool call a localhost sidecar to mint/verify/sign. Sidecar holds keys (or connects to KMS/HSM) and standardizes policy/audit across services.

Best for: containerized environments, Kubernetes, HSM integration.

### Mode 3 - Gateway enforcement via APIM (platform governance)

API Management fronts tool endpoints. Centralized policy, throttling, observability. APIM validates capability tokens at the gateway before forwarding to backend tools.

Best for: enterprise-wide governance, multi-team agent deployments, compliance-heavy environments.

## Operational checklist

- Keep capability token lifetime short: 30 to 120 seconds for most use cases
- Use a distributed nonce store (Redis, SQL) in multi-instance deployments
- Rotate issuer signing keys regularly and maintain `kid` consistency
- Store audit receipts in a centralized, append-only system
- Prefer fail-closed mode for privileged or write operations
- Test policy rules as part of CI/CD
- Configure separate signing keys per agent identity

## Related concepts

- [Agent Trust Kits](agent-trust-kits.md) - package overview and architecture
- [Agent Trust Profile](agent-trust-profile.md) - capability token model and threat model
- [MCP Trust Interceptor](agent-trust-mcp.md) - MCP-specific trust interceptor
- [ASP.NET Core Middleware](agent-trust-aspnetcore.md) - inbound HTTP verification
- [Agent-to-Agent Delegation](agent-trust-a2a.md) - delegation chains and bounded authority
