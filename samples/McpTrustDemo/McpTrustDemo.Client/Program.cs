using McpTrustDemo.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using SdJwt.Net.AgentTrust.Core;
using SdJwt.Net.AgentTrust.Mcp;
using SdJwt.Net.AgentTrust.OpenTelemetry;
using SdJwt.Net.AgentTrust.Policy;
using System.Text;

// -------------------------------------------------------------------
// MCP Trust Demo Client
// Simulates multiple AI agents calling an MCP Tool Server with
// capability tokens for bounded authority.
// -------------------------------------------------------------------

Console.WriteLine("=============================================================");
Console.WriteLine("  MCP Tool Marketplace with Agent Trust - Client Demo");
Console.WriteLine("  Demonstrates: SD-JWT capability tokens for MCP tool access");
Console.WriteLine("=============================================================");
Console.WriteLine();

var serverUrl = args.Length > 0 ? args[0] : "http://localhost:5100";

var host = Host.CreateDefaultBuilder(args)
    .ConfigureLogging(logging =>
    {
        logging.SetMinimumLevel(LogLevel.Warning);
        logging.AddFilter("McpTrustDemo", LogLevel.Information);
    })
    .ConfigureServices(services =>
    {
        // Shared signing key (must match server)
        var sharedKeyBytes = Encoding.UTF8.GetBytes("McpTrustDemoSigningKey-32Bytes!!");
        var signingKey = new SymmetricSecurityKey(sharedKeyBytes);

        // Nonce store for replay prevention
        var nonceStore = new MemoryNonceStore();
        services.AddSingleton<INonceStore>(nonceStore);

        // Policy engine (client-side pre-flight check)
        var policyEngine = new DefaultPolicyEngine(
            new PolicyBuilder()
                .Allow("agent://data-analyst", "sql_query", "Read")
                .Allow("agent://data-analyst", "file_browser", "Read")
                .Allow("agent://customer-support", "customer_lookup", "Read")
                .Allow("agent://customer-support", "email_sender", "Send")
                .Allow("agent://code-assistant", "file_browser", "Read")
                .Allow("agent://code-assistant", "code_executor", "Execute")
                .Allow("agent://orchestrator", "sql_query", "Read")
                .Deny("*", "*", "Delete")
                .Deny("*", "secrets_vault", "*")
                .Build());

        services.AddSingleton<IPolicyEngine>(policyEngine);
        services.AddSingleton(signingKey);

        // Register agent profiles
        services.AddSingleton(new AgentRegistry(new Dictionary<string, AgentProfile>
        {
            ["data-analyst"] = new("agent://data-analyst", "Data Analyst Agent",
                new[] { "sql_query", "file_browser" }),
            ["customer-support"] = new("agent://customer-support", "Customer Support Agent",
                new[] { "customer_lookup", "email_sender" }),
            ["code-assistant"] = new("agent://code-assistant", "Code Assistant Agent",
                new[] { "file_browser", "code_executor" })
        }));

        services.AddHttpClient("McpToolServer", client =>
        {
            client.BaseAddress = new Uri(serverUrl);
        });

        // OpenTelemetry
        services.AddOpenTelemetry()
            .WithTracing(tracing => tracing
                .AddAgentTrustInstrumentation()
                .AddConsoleExporter())
            .WithMetrics(metrics => metrics
                .AddAgentTrustInstrumentation()
                .AddConsoleExporter());

        services.AddSingleton<DemoOrchestrator>();
    })
    .Build();

var orchestrator = host.Services.GetRequiredService<DemoOrchestrator>();
await orchestrator.RunDemoAsync();

Console.WriteLine();
Console.WriteLine("=============================================================");
Console.WriteLine("  Demo complete. All scenarios executed.");
Console.WriteLine("=============================================================");
