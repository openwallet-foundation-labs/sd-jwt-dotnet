# Agent Trust Kit — PoC Use Cases

## Document Information

| Field   | Value                                                                                                                                                                                       |
| ------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Version | 1.0.0                                                                                                                                                                                       |
| Status  | Draft Proposal                                                                                                                                                                              |
| Created | 2026-03-01                                                                                                                                                                                  |
| Related | [Overview](agent-trust-kit-overview.md), [Core](agent-trust-kit-core.md), [MAF](agent-trust-kit-maf.md), [ASP.NET Core](agent-trust-kit-aspnetcore.md), [Policy](agent-trust-kit-policy.md) |

---

## Purpose

This document defines concrete **Proof-of-Concept (PoC) use cases** for the Agent Trust Kit, with runnable code examples that demonstrate end-to-end capability token flows. Each use case validates a specific architectural capability and can serve as a tutorial, test fixture, and adoption reference.

---

## Use Case 1: Agent-to-Tool Call (Core Flow)

**Validates:** Capability token minting, verification, selective disclosure, and audit receipts.

**Scenario:** A procurement agent calls a `MemberLookup` MCP tool server to retrieve fee schedules. The agent must present a scoped capability token limited to the `GetFees` action.

### Code Example

```csharp
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.AgentTrust.Core;
using SdJwt.Net.AgentTrust.Policy;

// === SETUP: Agent-Side (Issuer) ===

// Create agent signing key (use KMS/HSM in production)
using var agentEcdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
var agentKey = new ECDsaSecurityKey(agentEcdsa) { KeyId = "agent-key-1" };

// Initialize nonce store and receipt writer
var nonceStore = new MemoryNonceStore(new MemoryCache(new MemoryCacheOptions()));
var receiptWriter = new LoggingReceiptWriter(logger);

// Create capability token issuer
var issuer = new CapabilityTokenIssuer(
    agentKey,
    SecurityAlgorithms.EcdsaSha256,
    nonceStore);

// === MINT: Create a scoped capability token ===

var tokenResult = issuer.Mint(new CapabilityTokenOptions
{
    Issuer = "agent://procurement-service",
    Audience = "tool://member-lookup-service",
    Capability = new CapabilityClaim
    {
        Tool = "MemberLookup",
        Action = "GetFees",
        Resource = "member:12345",
        Limits = new CapabilityLimits { MaxResults = 50 }
    },
    Context = new CapabilityContext
    {
        CorrelationId = Guid.NewGuid().ToString(),
        WorkflowId = "procurement-workflow-001",
        StepId = "step-3-fee-lookup"
    },
    Lifetime = TimeSpan.FromSeconds(60),
    // Only disclose tool, action, limits to the tool server
    DisclosableClaims = new[] { "cap.tool", "cap.action", "cap.limits" }
});

Console.WriteLine($"Token ID: {tokenResult.TokenId}");
Console.WriteLine($"Expires: {tokenResult.ExpiresAt}");

// === CALL: Attach token to HTTP request ===

using var httpClient = new HttpClient();
httpClient.DefaultRequestHeaders.Add("Authorization", $"SdJwt {tokenResult.Token}");
var response = await httpClient.GetAsync("https://tool-server/api/members/12345/fees");

// === VERIFY: Tool-Side ===

var verifier = new CapabilityTokenVerifier(nonceStore);
var verifyResult = await verifier.VerifyAsync(
    tokenResult.Token,
    new CapabilityVerificationOptions
    {
        ExpectedAudience = "tool://member-lookup-service",
        TrustedIssuers = new Dictionary<string, SecurityKey>
        {
            ["agent://procurement-service"] = agentKey  // public key only in production
        },
        EnforceReplayPrevention = true
    });

if (verifyResult.IsValid)
{
    Console.WriteLine($"Verified! Tool={verifyResult.Capability!.Tool}, " +
                      $"Action={verifyResult.Capability.Action}");

    // Enforce limits in query
    var maxRows = verifyResult.Capability.Limits?.MaxResults ?? 100;

    // Emit audit receipt
    await receiptWriter.WriteAsync(new AuditReceipt
    {
        TokenId = verifyResult.TokenId!,
        Timestamp = DateTimeOffset.UtcNow,
        Decision = ReceiptDecision.Allow,
        Tool = verifyResult.Capability.Tool,
        Action = verifyResult.Capability.Action,
        CorrelationId = verifyResult.Context!.CorrelationId
    });
}
else
{
    Console.WriteLine($"Verification failed: {verifyResult.Error}");
}
```

### What This Demonstrates

| Capability               | Verified By                                              |
| ------------------------ | -------------------------------------------------------- |
| Capability token minting | `CapabilityTokenIssuer.Mint()` with scoped claims        |
| Selective disclosure     | Only `cap.tool`, `cap.action`, `cap.limits` disclosed    |
| Signature verification   | `CapabilityTokenVerifier.VerifyAsync()` with trusted key |
| Audience enforcement     | Token rejected if `aud` does not match tool identity     |
| Replay prevention        | `MemoryNonceStore` blocks duplicate `jti` values         |
| Limits enforcement       | Tool reads `MaxResults` from verified capability         |
| Audit trail              | Receipt written with allow/deny, correlation ID          |

---

## Use Case 2: MAF Middleware Integration

**Validates:** Transparent capability token minting via MAF middleware, zero application code changes.

**Scenario:** An MAF-orchestrated agent calls multiple tools in a workflow. The trust middleware automatically mints and attaches capability tokens for each call.

### Code Example

```csharp
using SdJwt.Net.AgentTrust.Maf;

var builder = Host.CreateApplicationBuilder(args);

// Register Agent Trust services
builder.Services.AddAgentTrust(options =>
{
    options.KeyCustodyProviderType = typeof(InMemoryKeyCustodyProvider);
    options.NonceStoreType = typeof(MemoryNonceStore);
    options.ReceiptWriterType = typeof(LoggingReceiptWriter);
    options.PolicyEngineType = typeof(DefaultPolicyEngine);
});

// Register policy rules
builder.Services.AddSingleton<IReadOnlyList<PolicyRule>>(
    new PolicyBuilder()
        .Allow("agent://procurement-*", "MemberLookup", "*",
            c => c.WithMaxResults(100).WithMaxTokenLifetime(TimeSpan.FromSeconds(60)))
        .Allow("agent://procurement-*", "FeeCalculator", "Calculate",
            c => c.WithMaxTokenLifetime(TimeSpan.FromSeconds(30)))
        .Deny("*", "AdminConsole", "*")
        .Build());

// Build MAF agent with trust middleware
var agent = builder.Services
    .AddAgent("procurement-agent")
    .UseAgentTrust(options =>
    {
        options.AgentId = "agent://procurement-service";
        options.ToolAudienceMapping = new Dictionary<string, string>
        {
            ["MemberLookup"] = "tool://member-lookup-service",
            ["FeeCalculator"] = "tool://fee-calculator-service"
        };
    })
    .AddTool("MemberLookup", memberLookupTool)
    .AddTool("FeeCalculator", feeCalculatorTool)
    .Build();

// When the agent calls a tool, the middleware automatically:
// 1. Evaluates policy (is this agent allowed to call this tool?)
// 2. Mints a scoped capability token
// 3. Attaches the token to the outbound request
// 4. Emits an audit receipt after the call completes
```

### What This Demonstrates

| Capability                   | Verified By                             |
| ---------------------------- | --------------------------------------- |
| Zero-code trust enforcement  | Middleware intercepts all tool calls    |
| Policy-driven authorization  | Rules evaluated before token minting    |
| Per-tool audience mapping    | Each tool gets correctly scoped tokens  |
| Automatic receipt generation | Receipts emitted without developer code |
| Fail-closed on policy deny   | Denied calls never reach the tool       |

---

## Use Case 3: Protected Tool Server (ASP.NET Core)

**Validates:** Inbound verification middleware, authorization attributes, and limits enforcement.

**Scenario:** An ASP.NET Core API acts as a tool server. All endpoints require valid capability tokens. Specific endpoints require specific actions.

### Code Example

```csharp
using SdJwt.Net.AgentTrust.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add Agent Trust verification
builder.Services.AddAgentTrustVerification(options =>
{
    options.Audience = "tool://member-lookup-service";
    options.TrustedIssuers = new Dictionary<string, SecurityKey>
    {
        ["agent://procurement-service"] = agentPublicKey,
        ["agent://hr-service"] = hrAgentPublicKey
    };
    options.ExcludedPaths = new[] { "/health", "/ready" };
    options.EnforceActionConstraints = true;
    options.EnforceLimits = true;
    options.EmitReceipts = true;
});

var app = builder.Build();

// Add verification middleware (after routing, before authorization)
app.UseRouting();
app.UseAgentTrustVerification();
app.UseAuthorization();

// Endpoint with specific capability requirement
app.MapGet("/api/members/{id}/fees",
    [RequireCapability("MemberLookup", "GetFees")]
    async (string id, HttpContext ctx) =>
    {
        var capability = ctx.GetVerifiedCapability();
        var limits = capability?.Limits;
        var maxRows = limits?.MaxResults ?? 100;
        var correlationId = ctx.GetCapabilityContext()?.CorrelationId;

        var fees = await feeService.GetFeesAsync(id, maxRows);

        return Results.Ok(new
        {
            MemberId = id,
            Fees = fees,
            CorrelationId = correlationId,
            LimitApplied = maxRows
        });
    });

// Endpoint requiring different action
app.MapPost("/api/members/{id}/update",
    [RequireCapability("MemberLookup", "UpdateMember")]
    async (string id, MemberUpdateRequest request) =>
    {
        // Only agents with explicit UpdateMember capability reach here
        return Results.Ok(new { Updated = true });
    });

// Health check (excluded from verification)
app.MapGet("/health", () => Results.Ok("healthy"));

app.Run();
```

### What This Demonstrates

| Capability                 | Verified By                                 |
| -------------------------- | ------------------------------------------- |
| Inbound token verification | Middleware rejects missing/invalid tokens   |
| Action-level authorization | `[RequireCapability]` attribute enforcement |
| Limits propagation         | `GetVerifiedCapability()` provides limits   |
| Path exclusion             | `/health` skips verification                |
| Structured error responses | 401/403 with machine-readable error codes   |
| Correlation propagation    | `CorrelationId` available in handler        |

---

## Use Case 4: Multi-Agent Delegation

**Validates:** Delegation tokens, bounded authority, and chain-of-trust auditing.

**Scenario:** An orchestrator agent delegates a claims processing task to a specialist agent. The specialist can only perform the specific actions granted and cannot further delegate.

### Code Example

```csharp
using SdJwt.Net.AgentTrust.Core;
using SdJwt.Net.AgentTrust.Policy;

// === Orchestrator: Create delegation for specialist ===

var delegationRules = new PolicyBuilder()
    .AllowDelegation(
        fromAgent: "agent://orchestrator",
        toAgent: "agent://claims-specialist",
        maxDepth: 1,
        constraints: d => d.WithAllowedActions("ProcessClaim", "GetClaimStatus"))
    .Build();

var policyEngine = new DefaultPolicyEngine(delegationRules);

// Orchestrator evaluates delegation policy
var delegationDecision = await policyEngine.EvaluateAsync(new PolicyRequest
{
    AgentId = "agent://orchestrator",
    Tool = "a2a://claims-specialist",
    Action = "ProcessClaim",
    Context = new CapabilityContext
    {
        CorrelationId = Guid.NewGuid().ToString(),
        WorkflowId = "insurance-claim-2024-001"
    },
    DelegationChain = new DelegationChain
    {
        DelegatedBy = "agent://orchestrator",
        Depth = 0,
        MaxDepth = 1,
        AllowedActions = new[] { "ProcessClaim", "GetClaimStatus" }
    }
});

if (delegationDecision.IsPermitted)
{
    // Mint delegation token
    var delegationToken = issuer.Mint(new CapabilityTokenOptions
    {
        Issuer = "agent://orchestrator",
        Audience = "a2a://claims-specialist",
        Capability = new CapabilityClaim
        {
            Tool = "ClaimsProcessing",
            Action = "ProcessClaim",
            Resource = "claim:INS-2024-00542"
        },
        Context = new CapabilityContext
        {
            CorrelationId = "corr-123",
            WorkflowId = "insurance-claim-2024-001",
            StepId = "delegate-to-specialist"
        },
        Lifetime = TimeSpan.FromMinutes(5)
    });

    // Send delegation token to specialist agent
    await specialistAgent.InvokeAsync("ProcessClaim", delegationToken.Token);
}

// === Specialist: Use delegation to call downstream tools ===

// Specialist verifies the delegation token
var delegationVerify = await verifier.VerifyAsync(
    delegationToken.Token,
    new CapabilityVerificationOptions
    {
        ExpectedAudience = "a2a://claims-specialist",
        TrustedIssuers = orchestratorKeys
    });

if (delegationVerify.IsValid)
{
    // Specialist mints a sub-capability token (bounded by delegation)
    var subToken = issuer.Mint(new CapabilityTokenOptions
    {
        Issuer = "agent://claims-specialist",
        Audience = "tool://claims-database",
        Capability = new CapabilityClaim
        {
            Tool = "ClaimsDatabase",
            Action = "ProcessClaim",
            Resource = "claim:INS-2024-00542",
            Limits = new CapabilityLimits { MaxInvocations = 1 }
        },
        Context = delegationVerify.Context!,
        Lifetime = TimeSpan.FromSeconds(30)
    });

    // Call downstream tool with sub-capability
    var result = await claimsDb.ProcessAsync(subToken.Token);

    // Receipt ties the full chain: orchestrator -> specialist -> tool
    await receiptWriter.WriteAsync(new AuditReceipt
    {
        TokenId = subToken.TokenId,
        Timestamp = DateTimeOffset.UtcNow,
        Decision = ReceiptDecision.Allow,
        Tool = "ClaimsDatabase",
        Action = "ProcessClaim",
        CorrelationId = delegationVerify.Context!.CorrelationId
    });
}
```

### What This Demonstrates

| Capability               | Verified By                                            |
| ------------------------ | ------------------------------------------------------ |
| Delegation token minting | Orchestrator creates bounded delegation token          |
| Bounded authority        | Specialist limited to `ProcessClaim`, `GetClaimStatus` |
| Sub-capability minting   | Specialist mints tool token within delegation scope    |
| Depth enforcement        | `maxDepth=1` prevents further delegation               |
| Chain-of-trust auditing  | Receipts trace: orchestrator -> specialist -> tool     |
| Correlation preservation | Same `CorrelationId` flows through entire chain        |

---

## Use Case 5: Containment Demonstration

**Validates:** Expiry-based containment, replay prevention, and key rotation.

**Scenario:** Demonstrate that compromised tokens are automatically contained without manual intervention.

### Code Example

```csharp
// === Scenario A: Expired token is rejected ===

var shortLivedToken = issuer.Mint(new CapabilityTokenOptions
{
    Issuer = "agent://test-agent",
    Audience = "tool://test-tool",
    Capability = new CapabilityClaim { Tool = "Test", Action = "Read" },
    Context = new CapabilityContext { CorrelationId = "test-1" },
    Lifetime = TimeSpan.FromSeconds(2) // Very short for demo
});

// Wait for expiry
await Task.Delay(TimeSpan.FromSeconds(3));

var expiredResult = await verifier.VerifyAsync(
    shortLivedToken.Token,
    verificationOptions);

Console.WriteLine($"Expired token valid: {expiredResult.IsValid}");
// Output: Expired token valid: False
Console.WriteLine($"Error: {expiredResult.ErrorCode}");
// Output: Error: token_expired

// === Scenario B: Replayed token is rejected ===

var token = issuer.Mint(new CapabilityTokenOptions
{
    Issuer = "agent://test-agent",
    Audience = "tool://test-tool",
    Capability = new CapabilityClaim { Tool = "Test", Action = "Read" },
    Context = new CapabilityContext { CorrelationId = "test-2" },
    Lifetime = TimeSpan.FromSeconds(60)
});

// First use: accepted
var firstUse = await verifier.VerifyAsync(token.Token, verificationOptions);
Console.WriteLine($"First use valid: {firstUse.IsValid}");
// Output: First use valid: True

// Replay: rejected
var replay = await verifier.VerifyAsync(token.Token, verificationOptions);
Console.WriteLine($"Replay valid: {replay.IsValid}");
// Output: Replay valid: False
Console.WriteLine($"Error: {replay.ErrorCode}");
// Output: Error: token_replayed

// === Scenario C: Key rotation invalidates old tokens ===

// Rotate agent key
await keyCustody.RotateKeyAsync("agent-key-1");
// Remove old key from trusted issuers
// All tokens signed with old key are now untrusted
```

### What This Demonstrates

| Capability                  | Verified By                            |
| --------------------------- | -------------------------------------- |
| Expiry-based containment    | Token rejected after `exp` passes      |
| Replay prevention           | Same `jti` rejected on second use      |
| Key rotation containment    | Old-key tokens rejected after rotation |
| No manual revocation needed | Short lifetimes + replay store suffice |

---

## PoC Execution Summary

| Use Case  | Components Exercised                                     | Est. Effort |
| --------- | -------------------------------------------------------- | ----------- |
| UC-1      | Core issuer, verifier, nonce store, receipts             | 2 days      |
| UC-2      | MAF middleware, policy engine, DI registration           | 3 days      |
| UC-3      | ASP.NET Core middleware, authorization attribute, limits | 2 days      |
| UC-4      | Delegation model, policy rules, chain-of-trust           | 3 days      |
| UC-5      | Expiry, replay prevention, key rotation                  | 1 day       |
| **Total** |                                                          | **11 days** |

---

## Recommended PoC Order

1. **UC-1** first — proves the core token lifecycle works end-to-end
2. **UC-5** next — proves containment properties (critical for trust story)
3. **UC-3** next — proves tool server protection (the "receiver" side)
4. **UC-2** next — proves MAF integration (the "sender" side)
5. **UC-4** last — proves multi-agent delegation (advanced scenario)

---

## Success Criteria for PoC

| Criterion                                     | Measurement                                     |
| --------------------------------------------- | ----------------------------------------------- |
| All 5 use cases pass as runnable console apps | `dotnet run` completes without errors           |
| Token minting < 5ms                           | BenchmarkDotNet measurement                     |
| Expired tokens always rejected                | UC-5 Scenario A passes                          |
| Replayed tokens always rejected               | UC-5 Scenario B passes                          |
| Audit receipts produced for all decisions     | Receipt count matches decision count            |
| Delegation chain traceable end-to-end         | UC-4 correlation ID present in all receipts     |
| No modifications to existing 11 packages      | `git diff` shows no changes in `src/SdJwt.Net*` |
