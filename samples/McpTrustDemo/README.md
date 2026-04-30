# MCP Tool Marketplace with Agent Trust

A production-realistic demo showing how AI agents access MCP tool servers with **cryptographically-enforced, least-privilege capability tokens** using SD-JWT.

## Problem Statement

Organizations deploying multiple AI agents (data analysts, customer support bots, code assistants) face a critical security gap: **MCP (Model Context Protocol) has no built-in authorization model.** Any agent that can reach a tool server can invoke any tool without limits.

This demo solves that problem by adding a trust layer:

- Each agent receives **scoped, time-limited capability tokens** (SD-JWT format)
- The tool server **verifies tokens per tool call** and enforces policy
- **Replay attacks** are detected and blocked
- **Cross-agent boundary violations** are denied at both client and server
- **Full audit trail** with OpenTelemetry observability

## Architecture

```
+-----------------------------------------------------------+
|  AI Agents (MCP Clients)                                  |
|                                                           |
|  +------------------+  +------------------+               |
|  | Data Analyst     |  | Customer Support |  ...          |
|  | sql_query: Read  |  | customer: Read   |               |
|  | file_browser: Rd |  | email: Send      |               |
|  +--------+---------+  +--------+---------+               |
|           |                      |                        |
|           v                      v                        |
|  +----------------------------------------------------+  |
|  |  McpClientTrustInterceptor                         |  |
|  |  - Evaluates policy (pre-flight)                   |  |
|  |  - Mints scoped SD-JWT capability token            |  |
|  |  - Attaches token to outbound request              |  |
|  +----------------------------------------------------+  |
+-----------------------------------------------------------+
            |
            |  HTTP + Authorization: SdJwt <token>
            v
+-----------------------------------------------------------+
|  MCP Tool Server (ASP.NET Core)                           |
|                                                           |
|  +----------------------------------------------------+  |
|  |  AgentTrustVerificationMiddleware                  |  |
|  |  - Extracts token from request                     |  |
|  |  - Verifies signature, expiry, audience            |  |
|  |  - Checks replay (nonce store)                     |  |
|  +----------------------------------------------------+  |
|                         |                                 |
|                         v                                 |
|  +----------------------------------------------------+  |
|  |  McpServerTrustGuard                               |  |
|  |  - Validates tool-name matches token cap.tool      |  |
|  |  - Evaluates server-side policy                    |  |
|  |  - Emits audit receipt                             |  |
|  +----------------------------------------------------+  |
|                         |                                 |
|                         v                                 |
|  +----------+  +------------+  +----------+  +---------+ |
|  | sql_query|  |file_browser|  |email_send|  |code_exec| |
|  +----------+  +------------+  +----------+  +---------+ |
|                                                           |
|  +----------------------------------------------------+  |
|  |  OpenTelemetry: Traces + Metrics                   |  |
|  |  agent_trust.tokens.minted/verified/rejected       |  |
|  |  agent_trust.policy.evaluations/denials            |  |
|  +----------------------------------------------------+  |
+-----------------------------------------------------------+
```

## Demo Scenarios

| #   | Scenario                  | Agent                  | Tool                    | Expected Result     |
| --- | ------------------------- | ---------------------- | ----------------------- | ------------------- |
| 1   | Authorized access         | Data Analyst           | sql_query (Read)        | SUCCESS             |
| 2   | Authorized access         | Customer Support       | customer_lookup (Read)  | SUCCESS             |
| 3   | Authorized access         | Code Assistant         | code_executor (Execute) | SUCCESS             |
| 4   | Cross-boundary denial     | Data Analyst           | email_sender (Send)     | DENIED              |
| 5   | Action denial             | Code Assistant         | file_browser (Delete)   | DENIED              |
| 6   | Sensitive resource denial | Data Analyst           | secrets_vault (Read)    | DENIED              |
| 7   | Replay attack prevention  | Data Analyst           | sql_query (reuse token) | BLOCKED             |
| 8   | Agent-to-agent delegation | Orchestrator -> Worker | sql_query (Read)        | SUCCESS (delegated) |

## Prerequisites

- .NET 9.0 SDK or later
- No external dependencies required (runs fully in-process with simulated tools)

## Running the Demo

### Option 1: Two terminals (recommended for visibility)

**Terminal 1 - Start the MCP Tool Server:**

```pwsh
cd samples/McpTrustDemo/McpTrustDemo.Server
dotnet run
```

The server starts on `http://localhost:5100`.

**Terminal 2 - Run the Client:**

```pwsh
cd samples/McpTrustDemo/McpTrustDemo.Client
dotnet run
```

### Option 2: Single terminal with background server

```pwsh
cd samples/McpTrustDemo

# Start server in background
Start-Job { dotnet run --project McpTrustDemo.Server }

# Wait for server to be ready
Start-Sleep -Seconds 3

# Run client
dotnet run --project McpTrustDemo.Client

# Clean up
Get-Job | Stop-Job | Remove-Job
```

### Custom server URL

```pwsh
dotnet run --project McpTrustDemo.Client -- http://localhost:5100
```

## Policy Configuration

The demo uses a declarative policy engine with these rules:

```csharp
// Data Analyst: read-only access to SQL and files
Allow("agent://data-analyst", "sql_query", "Read")
Allow("agent://data-analyst", "file_browser", "Read")

// Customer Support: customer data + limited email
Allow("agent://customer-support", "customer_lookup", "Read")
Allow("agent://customer-support", "email_sender", "Send")

// Code Assistant: file reading + code execution
Allow("agent://code-assistant", "file_browser", "Read")
Allow("agent://code-assistant", "code_executor", "Execute")

// Global denials
Deny("*", "*", "Delete")           // No agent can delete anything
Deny("*", "secrets_vault", "*")    // No agent can access secrets
```

## Integration with Microsoft Agent Framework

This demo is designed to integrate with the Microsoft Agent Framework (MAF) / Semantic Kernel agent patterns. The `McpClientTrustInterceptor` can be wired into any agent framework's tool-calling pipeline:

### Semantic Kernel Integration

```csharp
// Register as a filter in Semantic Kernel's function invocation pipeline
public class AgentTrustFilter : IFunctionInvocationFilter
{
    private readonly McpClientTrustInterceptor _interceptor;

    public async Task OnFunctionInvocationAsync(
        FunctionInvocationContext context,
        Func<FunctionInvocationContext, Task> next)
    {
        // Mint capability token before calling the MCP tool
        var result = await _interceptor.BeforeToolCallAsync(new McpToolCall
        {
            ToolName = context.Function.Name,
            Action = DeriveAction(context),
            Context = new CapabilityContext
            {
                CorrelationId = context.ChatHistory?.GetCorrelationId(),
                WorkflowId = context.Function.PluginName
            }
        });

        if (!result.IsSuccess)
        {
            throw new UnauthorizedAccessException(
                $"Agent trust denied: {result.Error}");
        }

        // Attach token to outbound call metadata
        context.Arguments["_agent_trust_token"] = result.Token;
        await next(context);
    }
}
```

### Microsoft Agent Framework (MAF) Integration

The `SdJwt.Net.AgentTrust.Maf` package provides first-class middleware:

```csharp
// In agent builder configuration
agentBuilder.Use<AgentTrustMiddleware>(options =>
{
    options.AgentId = "agent://my-agent";
    options.ToolAudienceMapping = new Dictionary<string, string>
    {
        ["sql_query"] = "https://mcp-tools.enterprise.local"
    };
});
```

### Azure AI Agent Service

When deploying to Azure AI Agent Service, configure the trust interceptor as middleware in the agent's tool pipeline. The signing keys can be stored in Azure Key Vault and resolved via `IKeyCustodyProvider`.

## Deployment Guide

### Local Development

The demo runs out of the box with shared symmetric keys. For production:

1. Replace symmetric keys with asymmetric (EC P-256 or RSA) key pairs
2. Publish JWKS endpoints for each agent's public key
3. Configure the server's `TrustedIssuers` to resolve keys from JWKS URIs

### Docker Deployment

```dockerfile
# Server
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
COPY publish/ /app
WORKDIR /app
EXPOSE 5100
ENTRYPOINT ["dotnet", "McpTrustDemo.Server.dll"]
```

```pwsh
# Build and run
dotnet publish McpTrustDemo.Server -c Release -o publish
docker build -t mcp-trust-server .
docker run -p 5100:5100 mcp-trust-server
```

### Azure Container Apps

```pwsh
# Deploy with Azure Developer CLI
azd init --template mcp-trust-demo
azd up
```

Or manually:

```pwsh
az containerapp up \
  --name mcp-trust-server \
  --resource-group rg-mcp-trust \
  --image mcp-trust-server:latest \
  --target-port 5100 \
  --ingress external
```

### Production Considerations

| Concern           | Solution                                                         |
| ----------------- | ---------------------------------------------------------------- |
| Key management    | Azure Key Vault + `IKeyCustodyProvider`                          |
| Key rotation      | JWKS endpoint with key rollover                                  |
| Replay prevention | Distributed nonce store (Redis)                                  |
| Policy management | OPA (Open Policy Agent) via `SdJwt.Net.AgentTrust.Policy.Opa`    |
| Observability     | OpenTelemetry -> Azure Monitor / Prometheus + Grafana            |
| Agent identity    | Workload Identity (SPIFFE/Entra) via `IWorkloadIdentityProvider` |
| Multi-tenant      | `CapabilityContext.TenantId` with tenant-scoped policies         |

## Security Model

### What capability tokens prevent

1. **Privilege escalation** - Agents cannot access tools outside their policy scope
2. **Replay attacks** - Each token is single-use (nonce store)
3. **Token theft** - Short-lived tokens (30-120s) with audience binding
4. **Lateral movement** - Tool-name validation ensures tokens cannot be used across tools
5. **Unauthorized delegation** - Depth-limited chains with cryptographic hop verification

### Trust boundaries enforced

```
Human -> Orchestrator Agent -> Worker Agent -> MCP Tool Server
  |          |                    |               |
  | grants   | delegates         | presents      | verifies
  | policy   | scoped token      | token         | signature + policy
  |          | (depth=0)         | (depth=1)     | + replay + audience
```

## Packages Used

| Package                              | Role in Demo                        |
| ------------------------------------ | ----------------------------------- |
| `SdJwt.Net.AgentTrust.Core`          | Token minting and verification      |
| `SdJwt.Net.AgentTrust.Policy`        | Rule-based authorization engine     |
| `SdJwt.Net.AgentTrust.Mcp`           | Client interceptor + server guard   |
| `SdJwt.Net.AgentTrust.AspNetCore`    | HTTP middleware for the tool server |
| `SdJwt.Net.AgentTrust.A2A`           | Agent-to-agent delegation           |
| `SdJwt.Net.AgentTrust.OpenTelemetry` | Metrics and tracing                 |

## Expected Output

```
=============================================================
  MCP Tool Marketplace with Agent Trust - Client Demo
=============================================================

--- 1. Authorized Access - Data Analyst queries SQL ---
  Agent: Data Analyst Agent (agent://data-analyst)
  Tool: sql_query, Action: Read
  SUCCESS (HTTP 200)
  Response: { "tool": "sql_query", "result": [...] }

--- 4. Cross-Boundary Denial - Data Analyst tries email_sender ---
  Agent: Data Analyst Agent (agent://data-analyst)
  Tool: email_sender, Action: Send
  DENIED (client-side): Policy denied.
  [EXPECTED] Policy correctly blocked unauthorized access.

--- 7. Replay Attack Prevention ---
  Token minted: abc123...
  First use: HTTP 200 (authorized)
  Replay attempt: HTTP 403 (rejected - replay detected)
  [EXPECTED] Server detected token reuse and blocked the replay attack.

--- 8. Agent-to-Agent Delegation (A2A) ---
  Delegation token minted: def456...
  Delegated call result: HTTP 200
  [SUCCESS] Delegated token accepted by server.
=============================================================
  Demo complete. All scenarios executed.
=============================================================
```

## LLM Demo (Real-World End-to-End)

The `McpTrustDemo.Llm` project connects a **real OpenAI LLM** to the same MCP tool server. The LLM autonomously decides which tools to call; the SD-JWT trust layer gates every invocation.

### What it demonstrates

| Prompt                          | LLM Decision            | Trust Layer                |
| ------------------------------- | ----------------------- | -------------------------- |
| "Show me Engineering employees" | Calls `sql_query`       | **ALLOWED** - token minted |
| "Look up Acme Corporation"      | Calls `customer_lookup` | **ALLOWED** - token minted |
| "List files in /reports"        | Calls `file_browser`    | **ALLOWED** - token minted |
| "Send email to bob@..."         | Calls `email_sender`    | **DENIED** - policy blocks |
| "Run this Python code"          | Calls `code_executor`   | **DENIED** - policy blocks |
| "Get database password"         | Calls `secrets_vault`   | **DENIED** - policy blocks |

The LLM receives the denial reason and explains to the user why it can't perform the action.

### Prerequisites

- .NET 9.0 SDK or later
- An OpenAI API key with available quota (`gpt-4o-mini` by default)

### Running the LLM Demo

**Terminal 1 - Start the MCP Tool Server:**

```pwsh
dotnet run --project samples/McpTrustDemo/McpTrustDemo.Server
```

**Terminal 2 - Run the LLM Agent:**

```pwsh
$env:OPENAI_API_KEY = "sk-..."
dotnet run --project samples/McpTrustDemo/McpTrustDemo.Llm
```

### Environment Variables

| Variable         | Default                 | Description                       |
| ---------------- | ----------------------- | --------------------------------- |
| `OPENAI_API_KEY` | (required)              | Your OpenAI API key               |
| `OPENAI_MODEL`   | `gpt-4o-mini`           | Model to use for function calling |
| `MCP_SERVER_URL` | `http://localhost:5100` | MCP tool server URL               |

### How it works

```
User Prompt
    │
    ▼
┌─────────────────────────────────┐
│  OpenAI (gpt-4o-mini)           │
│  System prompt: available tools │
│  → Decides: "call sql_query"    │
└──────────────┬──────────────────┘
               │ function_call
               ▼
┌─────────────────────────────────┐
│  TrustedToolExecutor            │
│  1. Policy pre-flight check     │
│  2. Mint SD-JWT capability token│
│  3. HTTP call with SdJwt header │
│  4. Return result OR denial     │
└──────────────┬──────────────────┘
               │
               ▼
┌─────────────────────────────────┐
│  MCP Tool Server                │
│  Verify token → Execute → Reply │
└─────────────────────────────────┘
               │
               ▼
┌─────────────────────────────────┐
│  OpenAI (continuation)          │
│  Formats result for user        │
│  OR explains why denied         │
└─────────────────────────────────┘
```

### Interactive mode

After the 6 demo prompts run automatically, the LLM demo enters interactive mode where you can ask any question. Try prompts that push boundaries:

```
You: Delete all the files in /reports
You: What's the database connection string?
You: Look up customer Acme and email them a summary
```

## Further Reading

- [SD-JWT RFC 9901](https://www.rfc-editor.org/rfc/rfc9901)
- [Model Context Protocol](https://modelcontextprotocol.io/)
- [Agent Trust Concepts](../../docs/concepts/)
- [Microsoft Agent Framework](https://learn.microsoft.com/azure/ai-services/agents/)
