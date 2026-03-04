# Tutorial: Agent Trust Kits

Build a bounded agent-to-tool trust flow using capability SD-JWTs.

**Time:** 25 minutes  
**Level:** Intermediate  
**Sample:** `samples/SdJwt.Net.Samples/02-Intermediate/06-AgentTrustKits.cs`

## What You Will Learn

- How to mint capability tokens for tool calls
- How to enforce policy-based allow/deny decisions
- How to verify capability tokens in ASP.NET Core
- How MAF middleware and MCP adapter fit into the flow

## Step 1: Install Packages

```bash
dotnet add package SdJwt.Net.AgentTrust.Core
dotnet add package SdJwt.Net.AgentTrust.Policy
dotnet add package SdJwt.Net.AgentTrust.AspNetCore
dotnet add package SdJwt.Net.AgentTrust.Maf
```

## Step 2: Define Policy Rules

```csharp
using SdJwt.Net.AgentTrust.Core;
using SdJwt.Net.AgentTrust.Policy;

var rules = new PolicyBuilder()
    .Deny("*", "billing", "Delete")
    .Allow("agent://finance-*", "billing", "Read", c =>
    {
        c.MaxLifetime(TimeSpan.FromSeconds(60));
        c.Limits(new CapabilityLimits { MaxResults = 50 });
    })
    .Build();

var policyEngine = new DefaultPolicyEngine(rules);
```

## Step 3: Mint a Capability Token

```csharp
using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.AgentTrust.Core;
using SdJwt.Net.AgentTrust.Maf;
using System.Security.Cryptography;

var signingKey = new SymmetricSecurityKey(RandomNumberGenerator.GetBytes(32));
var nonceStore = new MemoryNonceStore();
var issuer = new CapabilityTokenIssuer(
    signingKey,
    SecurityAlgorithms.HmacSha256,
    nonceStore);

var adapter = new McpTrustAdapter(
    issuer,
    policyEngine,
    "agent://finance-eu",
    new Dictionary<string, string> { ["billing"] = "https://tools.example.com" });

var tokenResult = await adapter.MintForToolCallAsync(
    "billing",
    new Dictionary<string, object> { ["action"] = "Read" },
    new CapabilityContext { CorrelationId = Guid.NewGuid().ToString("N") });
```

## Step 4: Verify in ASP.NET Core Tool API

```csharp
using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.AgentTrust.AspNetCore;

builder.Services.AddAgentTrustVerification(options =>
{
    options.Audience = "https://tools.example.com";
    options.TrustedIssuers = new Dictionary<string, SecurityKey>
    {
        ["agent://finance-eu"] = signingKey
    };
});

app.UseAgentTrustVerification();
```

Protect endpoint:

```csharp
[RequireCapability("billing", "Read")]
[HttpGet("invoices/{id}")]
public IActionResult GetInvoice(string id) => Ok();
```

## Step 5: Send Request with Token

Set request header:

```text
Authorization: SdJwt <tokenResult.Token>
```

If verification and policy checks pass, the request continues to your endpoint.

Run the runnable sample:

```bash
cd samples/SdJwt.Net.Samples
dotnet run -- 2.6
```

## Troubleshooting

1. `401 Unauthorized`: missing or malformed header/token.
2. `403 Forbidden`: token invalid, expired, or policy denied.
3. Replay rejection: same token reused with replay prevention enabled.
4. Audience mismatch: `aud` does not equal configured `options.Audience`.

## Next Steps

- [Agent Trust Integration Guide](../../guides/agent-trust-integration.md)
- [Agent Trust Kits Deep Dive](../../concepts/agent-trust-kits-deep-dive.md)
- [Agent Trust End-to-End Example](../../examples/agent-trust-end-to-end.md)
