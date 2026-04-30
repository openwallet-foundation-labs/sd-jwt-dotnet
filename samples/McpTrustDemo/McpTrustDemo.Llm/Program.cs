using McpTrustDemo.Llm;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using OpenAI;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using SdJwt.Net.AgentTrust.Core;
using SdJwt.Net.AgentTrust.Mcp;
using SdJwt.Net.AgentTrust.OpenTelemetry;
using SdJwt.Net.AgentTrust.Policy;
using System.Text;

// -------------------------------------------------------------------
// MCP Trust Demo - Real-World LLM Agent
// An enterprise AI assistant powered by OpenAI that uses SD-JWT
// capability tokens to gate every tool invocation.
// -------------------------------------------------------------------

Console.WriteLine("=============================================================");
Console.WriteLine("  MCP Tool Marketplace - LLM Agent with Trust Boundaries");
Console.WriteLine("  Real-world: OpenAI decides tools, SD-JWT gates access");
Console.WriteLine("=============================================================");
Console.WriteLine();

var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
if (string.IsNullOrWhiteSpace(apiKey))
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("ERROR: Set OPENAI_API_KEY environment variable.");
    Console.WriteLine("  PowerShell: $env:OPENAI_API_KEY = 'sk-...'");
    Console.WriteLine("  Bash:       export OPENAI_API_KEY='sk-...'");
    Console.ResetColor();
    return;
}

var serverUrl = Environment.GetEnvironmentVariable("MCP_SERVER_URL") ?? "http://localhost:5100";
var model = Environment.GetEnvironmentVariable("OPENAI_MODEL") ?? "gpt-4o-mini";

var host = Host.CreateDefaultBuilder(args)
    .ConfigureLogging(logging =>
    {
        logging.SetMinimumLevel(LogLevel.Warning);
        logging.AddFilter("McpTrustDemo", LogLevel.Information);
    })
    .ConfigureServices(services =>
    {
        // Shared signing key (must match MCP server)
        var sharedKeyBytes = Encoding.UTF8.GetBytes("McpTrustDemoSigningKey-32Bytes!!");
        var signingKey = new SymmetricSecurityKey(sharedKeyBytes);
        services.AddSingleton(signingKey);

        // Nonce store for replay prevention
        services.AddSingleton<INonceStore>(new MemoryNonceStore());

        // Policy engine - defines what THIS agent is allowed to do
        var policyEngine = new DefaultPolicyEngine(
            new PolicyBuilder()
                .Allow("agent://enterprise-assistant", "sql_query", "Read")
                .Allow("agent://enterprise-assistant", "customer_lookup", "Read")
                .Allow("agent://enterprise-assistant", "file_browser", "Read")
                .Deny("*", "secrets_vault", "*")
                .Deny("*", "*", "Delete")
                .Deny("*", "code_executor", "Execute")
                .Build());
        services.AddSingleton<IPolicyEngine>(policyEngine);

        // OpenAI via Microsoft.Extensions.AI
        services.AddSingleton<IChatClient>(sp =>
        {
            var openAiClient = new OpenAIClient(apiKey);
            return openAiClient.GetChatClient(model).AsIChatClient();
        });

        // HTTP client for MCP tool server
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

        services.AddSingleton<TrustedToolExecutor>();
        services.AddSingleton<LlmAgentRunner>();
    })
    .Build();

var runner = host.Services.GetRequiredService<LlmAgentRunner>();
await runner.RunInteractiveAsync();
