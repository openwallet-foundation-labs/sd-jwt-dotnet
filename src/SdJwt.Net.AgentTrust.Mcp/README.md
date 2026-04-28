# SdJwt.Net.AgentTrust.Mcp

Model Context Protocol (MCP) integration for agent trust capability token propagation.

## Features

-   MCP tool manifest with trust metadata
-   Client-side interceptor for outbound token propagation
-   Server-side guard for inbound token verification
-   Tool capability discovery

## Usage

### Client side (agent runtime)

```csharp
using SdJwt.Net.AgentTrust.Mcp;

var interceptor = new McpClientTrustInterceptor(issuer, policyEngine, options);
var result = await interceptor.BeforeToolCallAsync(toolCall);
// Attach result.Token to the MCP request metadata
```

### Server side (MCP tool server)

```csharp
var guard = new McpServerTrustGuard(verifier, policyEngine, options);
var decision = await guard.VerifyToolCallAsync(toolName, token);
if (!decision.IsValid) { /* reject */ }
```
