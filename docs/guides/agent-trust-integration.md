# How to Integrate Agent Trust Kits

This guide shows how to wire the Agent Trust kits for an end-to-end flow:

1. Agent runtime mints bounded capability tokens.
2. Tool API verifies those tokens.
3. Policy decides allow/deny with explicit constraints.

---

## Prerequisites

```bash
dotnet add package SdJwt.Net.AgentTrust.Core
dotnet add package SdJwt.Net.AgentTrust.Policy
dotnet add package SdJwt.Net.AgentTrust.AspNetCore
dotnet add package SdJwt.Net.AgentTrust.Maf
```

---

## 1. Define Policy

```csharp
using SdJwt.Net.AgentTrust.Core;
using SdJwt.Net.AgentTrust.Policy;

var rules = new PolicyBuilder()
    .Deny("*", "payments", "Delete")
    .Allow("agent://ops-*", "payments", "Read", c =>
    {
        c.MaxLifetime(TimeSpan.FromSeconds(60));
        c.Limits(new CapabilityLimits { MaxResults = 200 });
        c.RequireDisclosure("ctx.correlationId");
    })
    .Build();

IPolicyEngine policyEngine = new DefaultPolicyEngine(rules);
```

---

## 2. Mint Tokens in the Agent Runtime

```csharp
using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.AgentTrust.Core;
using SdJwt.Net.AgentTrust.Maf;
using System.Security.Cryptography;

var key = new SymmetricSecurityKey(RandomNumberGenerator.GetBytes(32));
var nonceStore = new MemoryNonceStore();
var issuer = new CapabilityTokenIssuer(key, SecurityAlgorithms.HmacSha256, nonceStore);

var adapter = new McpTrustAdapter(
    issuer,
    policyEngine,
    "agent://ops-eu",
    new Dictionary<string, string> { ["payments"] = "https://tools.example.com" });

var tokenResult = await adapter.MintForToolCallAsync(
    toolName: "payments",
    arguments: new Dictionary<string, object> { ["action"] = "Read" },
    context: new CapabilityContext { CorrelationId = Guid.NewGuid().ToString("N") });
```

Use `tokenResult.Token` in the outbound tool call header:

- Header: `Authorization`
- Value: `SdJwt <token>`

---

## 3. Verify Tokens in ASP.NET Core Tool API

```csharp
using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.AgentTrust.AspNetCore;
using SdJwt.Net.AgentTrust.Policy;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddAuthorizationBuilder()
    .AddAgentTrustPolicy("payments.read", "payments", "Read");

builder.Services.AddAgentTrustVerification(options =>
{
    options.Audience = "https://tools.example.com";
    options.TrustedIssuers = new Dictionary<string, SecurityKey>
    {
        ["agent://ops-eu"] = signingKey
    };
});

var app = builder.Build();
app.UseAgentTrustVerification();
app.UseAuthorization();
app.MapControllers();
app.Run();
```

Controller example:

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SdJwt.Net.AgentTrust.AspNetCore;

[ApiController]
[Route("payments")]
public sealed class PaymentsController : ControllerBase
{
    [HttpGet("{id}")]
    [Authorize(Policy = "payments.read")]
    [RequireCapability("payments", "Read")]
    public IActionResult GetPayment(string id)
    {
        var issuer = HttpContext.GetAgentIssuer();
        var ctx = HttpContext.GetCapabilityContext();
        return Ok(new { id, issuer, correlationId = ctx?.CorrelationId });
    }
}
```

---

## 4. Production Hardening Checklist

1. Replace in-memory key and nonce stores with production implementations.
2. Use short token lifetime and fail-closed behavior for privileged operations.
3. Persist audit receipts for allow/deny decisions.
4. Maintain strict audience mapping per tool/service.
5. Add policy tests for every high-risk tool action.

---

## See Also

- [Agent Trust Kits Deep Dive](../concepts/agent-trust-kits-deep-dive.md)
- [Agent Trust Tutorial](../tutorials/intermediate/07-agent-trust-kits.md)
- [Agent Trust Examples](../examples/agent-trust-end-to-end.md)
