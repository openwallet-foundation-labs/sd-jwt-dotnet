using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.AgentTrust.AspNetCore;
using SdJwt.Net.AgentTrust.Core;
using SdJwt.Net.AgentTrust.Maf;
using SdJwt.Net.AgentTrust.Policy;
using SdJwt.Net.Samples.Shared;
using System.Text;

namespace SdJwt.Net.Samples.Intermediate;

/// <summary>
/// Tutorial 06: Agent Trust Kits.
///
/// LEARNING OBJECTIVES:
/// - Build capability token policy for agent tool calls.
/// - Mint capability SD-JWT tokens with MCP/MAF adapter.
/// - Verify inbound capability tokens with ASP.NET Core middleware.
/// - Observe replay protection and denied policy behavior.
///
/// TIME: ~20 minutes
/// </summary>
public static class AgentTrustKits
{
    public static async Task Run()
    {
        ConsoleHelpers.PrintHeader("Tutorial 06: Agent Trust Kits");

        Console.WriteLine("This tutorial shows a full bounded-authority flow:");
        Console.WriteLine("Agent runtime mints a capability token, tool API verifies it,");
        Console.WriteLine("and policy decides whether the action is allowed.");
        Console.WriteLine();

        // =====================================================================
        // STEP 1: Build policy
        // =====================================================================
        ConsoleHelpers.PrintStep(1, "Define allow/deny policy rules");

        var policyEngine = new DefaultPolicyEngine(
            new PolicyBuilder()
                .Deny("*", "ledger", "Delete")
                .Allow("agent://finance-*", "ledger", "Read", c =>
                {
                    c.MaxLifetime(TimeSpan.FromSeconds(60));
                    c.Limits(new CapabilityLimits { MaxResults = 100 });
                    c.RequireDisclosure("ctx.correlationId");
                })
                .Build());

        ConsoleHelpers.PrintSuccess("Policy engine configured");
        Console.WriteLine("  Rule 1: Deny all Delete on ledger");
        Console.WriteLine("  Rule 2: Allow finance agents to Read ledger");

        // =====================================================================
        // STEP 2: Setup issuer and MCP adapter
        // =====================================================================
        ConsoleHelpers.PrintStep(2, "Initialize capability issuer and MCP adapter");

        var keyBytes = Encoding.UTF8.GetBytes("0123456789ABCDEF0123456789ABCDEF");
        var signingKey = new SymmetricSecurityKey(keyBytes);
        var nonceStore = new MemoryNonceStore();
        var issuer = new CapabilityTokenIssuer(signingKey, SecurityAlgorithms.HmacSha256, nonceStore);

        var adapter = new McpTrustAdapter(
            issuer,
            policyEngine,
            "agent://finance-eu",
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["ledger"] = "https://tools.example.com"
            });

        ConsoleHelpers.PrintSuccess("MCP adapter ready");
        ConsoleHelpers.PrintInfo("Agent Id", "agent://finance-eu");
        ConsoleHelpers.PrintInfo("Tool Audience", "https://tools.example.com");

        // =====================================================================
        // STEP 3: Mint a capability token
        // =====================================================================
        ConsoleHelpers.PrintStep(3, "Mint a capability token for tool call");

        var minted = await adapter.MintForToolCallAsync(
            toolName: "ledger",
            arguments: new Dictionary<string, object> { ["action"] = "Read" },
            context: new CapabilityContext
            {
                CorrelationId = Guid.NewGuid().ToString("N"),
                WorkflowId = "wf-ledger-sync"
            });

        ConsoleHelpers.PrintSuccess("Capability token minted");
        ConsoleHelpers.PrintInfo("Token Id", minted.TokenId);
        ConsoleHelpers.PrintPreview("Token", minted.Token, 64);

        // =====================================================================
        // STEP 4: Verify with ASP.NET Core middleware
        // =====================================================================
        ConsoleHelpers.PrintStep(4, "Verify inbound token with AgentTrust middleware");

        var middleware = new AgentTrustVerificationMiddleware(
            next: _ => Task.CompletedTask,
            verifier: new CapabilityTokenVerifier(nonceStore),
            policyEngine: policyEngine,
            receiptWriter: new LoggingReceiptWriter(),
            options: Options.Create(new AgentTrustVerificationOptions
            {
                Audience = "https://tools.example.com",
                TrustedIssuers = new Dictionary<string, SecurityKey>
                {
                    ["agent://finance-eu"] = signingKey
                }
            }));

        var requestContext = new DefaultHttpContext();
        requestContext.Request.Path = "/ledger/entries";
        requestContext.Request.Headers.Authorization = $"SdJwt {minted.Token}";

        await middleware.InvokeAsync(requestContext);

        ConsoleHelpers.PrintInfo("HTTP status", requestContext.Response.StatusCode);
        var capability = requestContext.GetVerifiedCapability();
        if (capability != null)
        {
            ConsoleHelpers.PrintSuccess("Capability available in HttpContext");
            ConsoleHelpers.PrintInfo("Capability Tool", capability.Tool);
            ConsoleHelpers.PrintInfo("Capability Action", capability.Action);
        }
        else
        {
            ConsoleHelpers.PrintError("Capability missing from HttpContext");
        }

        // =====================================================================
        // STEP 5: Replay and deny behavior
        // =====================================================================
        ConsoleHelpers.PrintStep(5, "Observe replay protection and policy deny");

        var replayContext = new DefaultHttpContext();
        replayContext.Request.Path = "/ledger/entries";
        replayContext.Request.Headers.Authorization = $"SdJwt {minted.Token}";
        await middleware.InvokeAsync(replayContext);
        ConsoleHelpers.PrintInfo("Replay request status", replayContext.Response.StatusCode);
        Console.WriteLine("  (Expected 403 because replay prevention marks token-id as used)");

        try
        {
            await adapter.MintForToolCallAsync(
                toolName: "ledger",
                arguments: new Dictionary<string, object> { ["action"] = "Delete" },
                context: new CapabilityContext { CorrelationId = Guid.NewGuid().ToString("N") });

            ConsoleHelpers.PrintError("Unexpected: delete token minted");
        }
        catch (InvalidOperationException ex)
        {
            ConsoleHelpers.PrintSuccess("Delete action denied by policy");
            ConsoleHelpers.PrintInfo("Reason", ex.Message);
        }

        // =====================================================================
        // COMPLETION
        // =====================================================================
        ConsoleHelpers.PrintCompletion("Tutorial 06: Agent Trust Kits", new[]
        {
            "Built rule-based policy with explicit allow/deny",
            "Minted capability SD-JWT via MCP adapter",
            "Verified token with ASP.NET Core middleware",
            "Observed replay prevention in action",
            "Validated policy-denied tool action"
        });

        Console.WriteLine();
        Console.WriteLine("NEXT: Advanced tutorials for trust, HAIP, and operations");
    }
}
