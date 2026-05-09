# Agent Trust Profile

> **Level:** Advanced preview extension

## Simple explanation

Agent Trust applies SD-JWT selective disclosure to the problem of AI agent authorization.

## What you will learn

- Why API keys and broad OAuth scopes are insufficient for AI agents
- How capability tokens encode per-action, per-resource permissions
- How delegation chains let one agent authorize another
- The relationship between Agent Trust Profile and Agent Trust Kits

**Before Agent Trust:** An AI agent calls a tool with a broad API key. The tool cannot distinguish what the agent is allowed to do, who authorized it, or whether the request is legitimate.

**After Agent Trust:** The agent presents a scoped capability token (an SD-JWT) that says exactly what it can do, who delegated the permission, and when it expires. The tool verifies the token before executing.

### How it compares

| Approach    | Scope control | Delegation chain | Selective disclosure | Audit trail                            |
| ----------- | ------------- | ---------------- | -------------------- | -------------------------------------- |
| API key     | None          | None             | None                 | Minimal                                |
| OAuth token | Broad scopes  | Limited          | None                 | Standard                               |
| mTLS        | Identity only | None             | None                 | Transport                              |
| Agent Trust | Per-action    | Multi-hop        | Per-claim            | Structured receipts, if stored durably |

|            |                                                                                                                                                                                                                                                            |
| ---------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Status     | Preview                                                                                                                                                                                                                                                    |
| Scope      | Project-defined profile for scoped SD-JWT capability tokens, policy enforcement, delegation, and audit                                                                                                                                                     |
| Packages   | `SdJwt.Net.AgentTrust.Core`, `SdJwt.Net.AgentTrust.Policy`, `SdJwt.Net.AgentTrust.AspNetCore`, `SdJwt.Net.AgentTrust.Mcp`, `SdJwt.Net.AgentTrust.A2A`, `SdJwt.Net.AgentTrust.OpenTelemetry`, `SdJwt.Net.AgentTrust.Maf`, `SdJwt.Net.AgentTrust.Policy.Opa` |
| Foundation | SD-JWT per RFC 9901, selective disclosure, key binding, nonce freshness, and policy-constrained delegation                                                                                                                                                 |

## Problem Statement

AI agents and automation services increasingly call tools, APIs, and other agents on behalf of users or workflows. Traditional service accounts and broad API keys are difficult to constrain per action, hard to audit, and risky when an agent is compromised or misdirected.

Agent Trust is a preview .NET extension that explores how SD-JWT, key binding, policy, and selective disclosure can support scoped agent delegation and MCP/API tool governance. It complements existing OAuth, mTLS, API gateway, and MCP authorization patterns; it does not replace them.

### When to use Agent Trust

| Scenario                                   | Use Agent Trust? | Why                                                                       |
| ------------------------------------------ | ---------------- | ------------------------------------------------------------------------- |
| AI agent calls tools on behalf of a user   | Yes              | Scope the agent's authority to specific tools and actions                 |
| Agent delegates a subtask to another agent | Yes              | Delegation chain captures who authorized what                             |
| MCP server needs per-tool authorization    | Yes              | Capability tokens replace broad API keys                                  |
| Human user logs into a web app             | No               | Use standard OAuth 2.0 / OIDC                                             |
| Service-to-service call with fixed scope   | Maybe            | OAuth client credentials may suffice; Agent Trust adds per-action scoping |
| Public API with no agent involvement       | No               | Standard API key or OAuth is sufficient                                   |

### OAuth and Agent Trust: complements, not competitors

Agent Trust does not replace OAuth. It layers on top of it:

| Layer                  | What it does                                                         | Standard                             |
| ---------------------- | -------------------------------------------------------------------- | ------------------------------------ |
| Authentication         | Proves the agent's identity                                          | OAuth 2.0 / OIDC / mTLS              |
| Authorization (coarse) | Grants the agent a set of scopes                                     | OAuth 2.0 scopes                     |
| Authorization (fine)   | Scopes the agent to one tool, one action, one resource, one lifetime | Agent Trust capability token         |
| Delegation             | Captures the chain of who authorized who                             | Agent Trust delegation chain         |
| Audit                  | Structured receipts and telemetry                                    | Agent Trust receipts + OpenTelemetry |

A typical deployment uses OAuth to authenticate the agent, then Agent Trust to constrain what the authenticated agent can do at the individual tool-call level.

## Threat Model

| Threat                                | Control                                                                 |
| ------------------------------------- | ----------------------------------------------------------------------- |
| Prompt injection triggers a tool call | Policy is evaluated before a capability token is minted                 |
| Over-broad service account access     | Tokens are scoped to one tool, action, resource, audience, and lifetime |
| Replay of a captured token            | `jti`, nonce storage, short expiry, and optional sender constraints     |
| Confused-deputy calls                 | `aud` binding and server-side capability validation                     |
| Unbounded agent delegation            | Delegation depth and inherited capability constraints                   |
| Weak auditability                     | Structured receipts and OpenTelemetry correlation                       |

## Token Model

An Agent Trust capability token is an SD-JWT minted for a specific action. The token is short-lived and contains only the claims needed by the target tool or agent.

| Claim | Type     | Purpose                                                              |
| ----- | -------- | -------------------------------------------------------------------- |
| `iss` | Standard | Agent, service, or workload identity minting the capability          |
| `aud` | Standard | Tool, API, MCP server, or downstream agent expected to verify it     |
| `iat` | Standard | Issued-at timestamp                                                  |
| `exp` | Standard | Short expiry, usually seconds to minutes                             |
| `jti` | Standard | Unique token identifier for replay prevention                        |
| `cnf` | Standard | Optional proof-of-possession binding for DPoP, mTLS, or similar keys |
| `cap` | Profile  | Capability object with tool, action, resource, limits, and context   |
| `ctx` | Profile  | Correlation metadata for workflow, step, tenant, or trace            |

## Capability Claims

The `cap` claim carries the machine-enforceable authority:

```json
{
  "tool": "member.lookup",
  "action": "read",
  "resource": "member/12345",
  "limits": {
    "maxResults": 10,
    "maxPayloadBytes": 32768
  }
}
```

Receivers must verify the token before trusting any claim. Application code remains responsible for enforcing resource-specific limits that cannot be enforced by cryptography alone.

## Key Binding

Capability tokens may include sender constraints through `cnf` claims. Deployments can bind tokens to:

- DPoP proof keys
- mTLS client certificates
- workload identity keys
- gateway-managed proof material

Sender constraints are recommended for privileged actions and cross-boundary tool calls.

## Nonce And Replay Protection

Receivers should reject reused `jti` values within the token lifetime plus configured clock skew. Multi-instance deployments need a distributed nonce store, not the in-memory development implementation.

Default posture:

- Short token lifetimes
- Fail closed for privileged actions
- Distributed replay storage in production-like pilots
- Clock skew kept small and explicit

## Delegation Constraints

Agent-to-agent delegation must preserve or narrow authority. A delegated capability should not exceed the original token's action, resource, audience, lifetime, or delegation depth.

Recommended constraints:

- Maximum delegation depth
- No privilege expansion across hops
- Explicit allowed downstream audiences
- Correlation ID propagation
- Receipt emitted at each hop

## MCP Usage

`SdJwt.Net.AgentTrust.Mcp` treats MCP as one adapter target. The profile does not assume MCP is the only agent transport or authorization model.

For MCP tool calls:

1. The client interceptor evaluates policy before the call.
2. A capability token is minted for the requested tool/action.
3. The token is attached using the deployment's chosen metadata or HTTP header convention.
4. The server guard verifies signature, audience, expiry, replay status, and capability claims.
5. The tool enforces application-level constraints and emits an audit receipt.

## ASP.NET Core API Usage

`SdJwt.Net.AgentTrust.AspNetCore` provides inbound middleware and authorization helpers for APIs that receive capability tokens. APIs should still use normal transport security, authentication boundaries, and resource authorization. Agent Trust adds per-action capability proof and audit context.

## OpenTelemetry Audit Model

Agent Trust receipts should include:

- Token ID
- Issuer and audience
- Tool and action
- Decision
- Reason code
- Correlation ID
- Policy version
- Evaluation duration

Receipts should avoid raw sensitive payloads. Use hashes, references, or selective disclosure when the receiver does not need full context.

## Security Considerations

- Agent Trust is a preview project profile, not a completed external standard.
- Do not treat capability tokens as a substitute for TLS, OAuth resource-server protections, or API authorization.
- Keep signing keys in KMS/HSM-backed custody for serious pilots.
- Use constant-time comparisons for sensitive token material.
- Reject weak algorithms and follow the repository HAIP algorithm policy.
- Validate the deployment threat model before using these packages beyond evaluation or pilot environments.

## Standards Relationship

| Area              | Status                                                                                |
| ----------------- | ------------------------------------------------------------------------------------- |
| SD-JWT            | RFC 9901 stable base format                                                           |
| SD-JWT VC         | IETF draft, tracked separately from Agent Trust                                       |
| Delegate SD-JWT   | Individual Internet-Draft; useful input, not a normative dependency                   |
| MCP authorization | External protocol authorization guidance; Agent Trust can complement it as an adapter |
| Agent Trust       | Project-defined preview profile implemented by `SdJwt.Net.AgentTrust.*` packages      |

## Related concepts

- [Agent Trust Kits](../concepts/agent-trust-kits.md)
- [Agent Trust Integration Guide](../guides/agent-trust-integration.md)
- [MCP Trust Demo](../examples/mcp-trust-demo.md)
- [Package Maturity](../../MATURITY.md)
