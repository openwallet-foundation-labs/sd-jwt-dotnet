# Scoped Agent Capability Token

> **Pattern type:** Core reusable pattern
> **Maturity:** Preview extension
> **Key packages:** `SdJwt.Net.AgentTrust.Core`, `SdJwt.Net.AgentTrust.Policy`, `SdJwt.Net.AgentTrust.Mcp`

## What it does

Scopes each AI agent tool call to a specific tool, action, resource, and time window using an SD-JWT capability token. The token is minted per request and verified before the tool executes.

## When to use it

- AI agents call tools with broad API keys or OAuth tokens
- Least-privilege enforcement is needed per tool call, not per session
- Multi-agent delegation requires bounded, auditable capability transfer
- Audit teams need per-call evidence of what was authorized

## How it works

1. **Policy check**: When an agent requests a tool call, the policy engine evaluates whether the request should be authorized.
2. **Token minting**: If authorized, a scoped capability token (SD-JWT format) is minted with the specific tool, action, resource, and expiry.
3. **Token attachment**: The token is attached to the tool call request.
4. **Verification**: The tool endpoint verifies the token signature, claims, and expiry before executing.
5. **Audit receipt**: The allow/deny decision is recorded with correlation IDs.

## Package roles

| Package                           | Role                                      |
| --------------------------------- | ----------------------------------------- |
| `SdJwt.Net.AgentTrust.Core`       | Capability token minting and verification |
| `SdJwt.Net.AgentTrust.Policy`     | Rule-based policy evaluation              |
| `SdJwt.Net.AgentTrust.Mcp`        | MCP tool call interception                |
| `SdJwt.Net.AgentTrust.AspNetCore` | Inbound verification middleware           |
| `SdJwt.Net.AgentTrust.A2A`        | Agent-to-agent delegation                 |

## Application responsibility

Agent framework integration, policy rule definitions, delegation depth limits, orchestration runtime, LLM integration, audit log storage.

## Used by

- [AI Agent Authorization](../ai-agent-authorization.md) -- full reference pattern
- [Enterprise API Access](../enterprise-api-access.md) -- verified client context with capability tokens
- [Financial AI](../financial-ai.md) -- agent trust for AI copilot tool calls
