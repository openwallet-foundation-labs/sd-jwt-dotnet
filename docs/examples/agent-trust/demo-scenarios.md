# Agent Trust Demo Scenarios

| Field        | Value                                                   |
| ------------ | ------------------------------------------------------- |
| Example type | Scenario catalogue                                      |
| Maturity     | Preview                                                 |
| Packages     | AgentTrust.Core, Policy, AspNetCore, Mcp, A2A           |
| Runnable     | Via `samples/McpTrustDemo`                              |
| Source       | [MCP Tool Governance Demo](mcp-tool-governance-demo.md) |

> **Preview boundary:** These scenarios use Agent Trust preview packages. Agent Trust is a project-defined pattern for scoped agent/tool authorization. It is not an IETF, OpenID Foundation, MCP, or OWF standard.

---

## Scenario catalogue

Each scenario validates a specific Agent Trust capability. Scenarios 1-8 are covered by the `McpTrustDemo` scripted client.

### Core authorization

| #   | Scenario          | Agent            | Tool / Action           | Expected result | Demonstrates               |
| --- | ----------------- | ---------------- | ----------------------- | --------------- | -------------------------- |
| 1   | Authorized access | Data Analyst     | sql_query / Read        | HTTP 200        | Mint, verify, execute      |
| 2   | Authorized access | Customer Support | customer_lookup / Read  | HTTP 200        | Per-agent policy scoping   |
| 3   | Authorized access | Code Assistant   | code_executor / Execute | HTTP 200        | Action-level authorization |

### Denial enforcement

| #   | Scenario                  | Agent          | Tool / Action         | Expected result  | Demonstrates               |
| --- | ------------------------- | -------------- | --------------------- | ---------------- | -------------------------- |
| 4   | Cross-boundary denial     | Data Analyst   | email_sender / Send   | Client-side deny | Agent boundary enforcement |
| 5   | Action denial             | Code Assistant | file_browser / Delete | Client-side deny | Action restriction         |
| 6   | Sensitive resource denial | Data Analyst   | secrets_vault / Read  | Client-side deny | Blanket deny rules         |

### Containment

| #   | Scenario                  | Agent        | Tool / Action           | Expected result      | Demonstrates             |
| --- | ------------------------- | ------------ | ----------------------- | -------------------- | ------------------------ |
| 7   | Replay attack prevention  | Data Analyst | sql_query (reuse token) | HTTP 403             | JTI-based replay store   |
| 8   | Agent-to-agent delegation | Orchestrator | sql_query / Read        | HTTP 200 (delegated) | Bounded delegation chain |

### Additional validation scenarios

| #   | Scenario                  | Expected result | Demonstrates                             |
| --- | ------------------------- | --------------- | ---------------------------------------- |
| 9   | Expired token             | Rejected        | Expiry-based containment                 |
| 10  | Audience mismatch         | Rejected        | Token scoped to wrong tool server        |
| 11  | Unknown issuer            | Rejected        | Untrusted agent key rejected             |
| 12  | Key rotation              | Old tokens fail | Tokens signed with rotated key rejected  |
| 13  | Delegation depth exceeded | Rejected        | Depth-limited delegation enforcement     |
| 14  | Scope attenuation         | Restricted      | Sub-agent receives equal or lesser scope |

---

## LLM client scenarios

When using the LLM variant (`McpTrustDemo.Llm`), the LLM autonomously selects tools. The trust layer gates access:

| Prompt                          | LLM decision            | Trust result             |
| ------------------------------- | ----------------------- | ------------------------ |
| "Show me Engineering employees" | Calls `sql_query`       | Allowed -- data returned |
| "Look up Acme Corporation"      | Calls `customer_lookup` | Allowed -- data returned |
| "List files in /reports"        | Calls `file_browser`    | Allowed -- data returned |
| "Send email to bob@..."         | Calls `email_sender`    | Denied by policy         |
| "Execute Python code"           | Calls `code_executor`   | Denied by deny rule      |
| "Read database password"        | Calls `secrets_vault`   | Denied by deny rule      |

The LLM receives the denial reason and explains to the user why the action is blocked.

---

## Running scenarios

```pwsh
# Scripted client (no AI required)
dotnet run --project samples/McpTrustDemo/McpTrustDemo.Server
dotnet run --project samples/McpTrustDemo/McpTrustDemo.Client

# LLM client (requires OpenAI key)
$env:OPENAI_API_KEY = "sk-..."
dotnet run --project samples/McpTrustDemo/McpTrustDemo.Llm
```

---

## Related

- [MCP Tool Governance Demo](mcp-tool-governance-demo.md) -- full walkthrough
- [Agent Trust End-to-End](agent-trust-end-to-end.md) -- minimal code example
- [Agent Trust Integration Guide](../../guides/agent-trust-integration.md)
- [Agent Trust Kits](../../concepts/agent-trust-kits.md)
