# SdJwt.Net.AgentTrust.Maf

Middleware and adapters for propagating Agent Trust capability tokens in Model Context Protocol (MCP) and MAF-style tool execution pipelines.

## Install

```bash
dotnet add package SdJwt.Net.AgentTrust.Maf
```

## What This Package Provides

-   `AgentTrustMiddleware` for policy-aware token minting in function/tool calls.
-   `McpTrustAdapter` for minting capability tokens per MCP tool invocation.
-   `AgentTrustMiddlewareOptions` to configure agent identity, audience mapping, and failure mode.
-   `FunctionCallContext` and `IAgentBuilder` abstractions for middleware integration.
-   DI helpers in `AgentTrustExtensions` (`AddAgentTrust`, `UseAgentTrust`).

## MCP Adapter Example

```csharp
using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.AgentTrust.Core;
using SdJwt.Net.AgentTrust.Maf;
using SdJwt.Net.AgentTrust.Policy;
using System.Security.Cryptography;

var key = new SymmetricSecurityKey(RandomNumberGenerator.GetBytes(32));
var nonceStore = new MemoryNonceStore();
var issuer = new CapabilityTokenIssuer(key, SecurityAlgorithms.HmacSha256, nonceStore);
var policy = new DefaultPolicyEngine(new PolicyBuilder().Allow("*", "sql", "Read").Build());

var adapter = new McpTrustAdapter(
    issuer,
    policy,
    "agent://assistant-1",
    new Dictionary<string, string> { ["sql"] = "https://sql.tools.internal" });

var token = await adapter.MintForToolCallAsync(
    "sql",
    new Dictionary<string, object> { ["action"] = "Read" },
    new CapabilityContext { CorrelationId = Guid.NewGuid().ToString("N") });
```

## Middleware Behavior

-   Evaluates policy before minting a token.
-   Maps tool name to audience using `ToolAudienceMapping`.
-   Writes minted token to call metadata as `<prefix> <token>` under `TokenHeaderName`.
-   Supports fail-closed (`FailOnMintError = true`) and fail-open execution.

## Related Packages

-   `SdJwt.Net.AgentTrust.Core`
-   `SdJwt.Net.AgentTrust.Policy`
-   `SdJwt.Net.AgentTrust.AspNetCore`
