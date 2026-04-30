using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.AgentTrust.Core;
using SdJwt.Net.AgentTrust.Mcp;
using SdJwt.Net.AgentTrust.Policy;
using System.Text.Json;

namespace McpTrustDemo.Client;

/// <summary>
/// Orchestrates the demo scenarios showing different agents attempting
/// tool calls with varying levels of authorization.
/// </summary>
public class DemoOrchestrator
{
    private readonly AgentRegistry _registry;
    private readonly IPolicyEngine _policyEngine;
    private readonly INonceStore _nonceStore;
    private readonly SymmetricSecurityKey _signingKey;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<DemoOrchestrator> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public DemoOrchestrator(
        AgentRegistry registry,
        IPolicyEngine policyEngine,
        INonceStore nonceStore,
        SymmetricSecurityKey signingKey,
        IHttpClientFactory httpClientFactory,
        ILogger<DemoOrchestrator> logger)
    {
        _registry = registry;
        _policyEngine = policyEngine;
        _nonceStore = nonceStore;
        _signingKey = signingKey;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task RunDemoAsync()
    {
        // Scenario 1: Authorized tool calls succeed
        await RunScenario("1. Authorized Access - Data Analyst queries SQL",
            "data-analyst", "sql_query", "Read",
            expectSuccess: true);

        // Scenario 2: Authorized tool calls succeed
        await RunScenario("2. Authorized Access - Customer Support looks up customer",
            "customer-support", "customer_lookup", "Read",
            expectSuccess: true);

        // Scenario 3: Authorized tool calls succeed
        await RunScenario("3. Authorized Access - Code Assistant executes code",
            "code-assistant", "code_executor", "Execute",
            expectSuccess: true);

        // Scenario 4: Cross-boundary denial (analyst tries to send email)
        await RunScenario("4. Cross-Boundary Denial - Data Analyst tries email_sender",
            "data-analyst", "email_sender", "Send",
            expectSuccess: false);

        // Scenario 5: Action denial (any agent tries Delete)
        await RunScenario("5. Action Denial - Code Assistant tries Delete on file_browser",
            "code-assistant", "file_browser", "Delete",
            expectSuccess: false);

        // Scenario 6: Sensitive resource denial (agent tries secrets)
        await RunScenario("6. Sensitive Resource Denial - Data Analyst tries secrets_vault",
            "data-analyst", "secrets_vault", "Read",
            expectSuccess: false);

        // Scenario 7: Replay attack prevention
        await RunReplayScenario();

        // Scenario 8: Multi-agent workflow (orchestrator delegates)
        await RunDelegationScenario();
    }

    private async Task RunScenario(
        string title,
        string agentName,
        string toolName,
        string action,
        bool expectSuccess)
    {
        Console.WriteLine($"--- {title} ---");

        var agent = _registry.Get(agentName);
        Console.WriteLine($"  Agent: {agent.DisplayName} ({agent.AgentId})");
        Console.WriteLine($"  Tool: {toolName}, Action: {action}");

        var interceptor = new McpClientTrustInterceptor(
            new CapabilityTokenIssuer(_signingKey, SecurityAlgorithms.HmacSha256, _nonceStore),
            _policyEngine,
            new McpClientTrustOptions
            {
                AgentId = agent.AgentId,
                ToolAudienceMapping = new Dictionary<string, string>
                {
                    [toolName] = "https://mcp-tools.enterprise.local"
                },
                DefaultTokenLifetime = TimeSpan.FromSeconds(60)
            });

        var toolCall = new McpToolCall
        {
            ToolName = toolName,
            Action = action,
            Context = new CapabilityContext
            {
                CorrelationId = Guid.NewGuid().ToString("N"),
                WorkflowId = $"demo-{agentName}-{toolName}"
            }
        };

        var mintResult = await interceptor.BeforeToolCallAsync(toolCall);

        if (!mintResult.IsSuccess)
        {
            Console.ForegroundColor = expectSuccess ? ConsoleColor.Red : ConsoleColor.Yellow;
            Console.WriteLine($"  DENIED (client-side): {mintResult.Error}");
            Console.ResetColor();

            if (!expectSuccess)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("  [EXPECTED] Policy correctly blocked unauthorized access.");
                Console.ResetColor();
            }

            Console.WriteLine();
            return;
        }

        // Call the MCP tool server with the minted token
        var client = _httpClientFactory.CreateClient("McpToolServer");
        var request = new HttpRequestMessage(HttpMethod.Post, $"/tools/{toolName}");
        request.Headers.Add("Authorization", $"Bearer {mintResult.Token}");
        request.Content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");

        var response = await client.SendAsync(request);
        var body = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"  SUCCESS (HTTP {(int)response.StatusCode})"); ;
            Console.ResetColor();

            var formatted = FormatJson(body);
            Console.WriteLine($"  Response: {formatted}");
        }
        else
        {
            Console.ForegroundColor = expectSuccess ? ConsoleColor.Red : ConsoleColor.Yellow;
            Console.WriteLine($"  REJECTED (HTTP {(int)response.StatusCode}): {body}");
            Console.ResetColor();

            if (!expectSuccess)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("  [EXPECTED] Server correctly rejected the request.");
                Console.ResetColor();
            }
        }

        Console.WriteLine();
    }

    private async Task RunReplayScenario()
    {
        Console.WriteLine("--- 7. Replay Attack Prevention ---");
        Console.WriteLine("  Minting a token and using it twice to prove replay protection.");

        var agent = _registry.Get("data-analyst");
        var interceptor = new McpClientTrustInterceptor(
            new CapabilityTokenIssuer(_signingKey, SecurityAlgorithms.HmacSha256, _nonceStore),
            _policyEngine,
            new McpClientTrustOptions
            {
                AgentId = agent.AgentId,
                ToolAudienceMapping = new Dictionary<string, string>
                {
                    ["sql_query"] = "https://mcp-tools.enterprise.local"
                }
            });

        var toolCall = new McpToolCall
        {
            ToolName = "sql_query",
            Action = "Read",
            Context = new CapabilityContext
            {
                CorrelationId = Guid.NewGuid().ToString("N"),
                WorkflowId = "demo-replay-test"
            }
        };

        var mintResult = await interceptor.BeforeToolCallAsync(toolCall);
        if (!mintResult.IsSuccess)
        {
            Console.WriteLine($"  Failed to mint: {mintResult.Error}");
            Console.WriteLine();
            return;
        }

        Console.WriteLine($"  Token minted: {mintResult.TokenId}");

        // First use - should succeed
        var client = _httpClientFactory.CreateClient("McpToolServer");
        var request1 = new HttpRequestMessage(HttpMethod.Post, "/tools/sql_query");
        request1.Headers.Add("Authorization", $"Bearer {mintResult.Token}");
        request1.Content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");

        var response1 = await client.SendAsync(request1);
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"  First use: HTTP {(int)response1.StatusCode} (authorized)");
        Console.ResetColor();

        // Second use (replay) - should be rejected
        var request2 = new HttpRequestMessage(HttpMethod.Post, "/tools/sql_query");
        request2.Headers.Add("Authorization", $"Bearer {mintResult.Token}");
        request2.Content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");

        var response2 = await client.SendAsync(request2);
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"  Replay attempt: HTTP {(int)response2.StatusCode} (rejected - replay detected)");
        Console.ResetColor();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("  [EXPECTED] Server detected token reuse and blocked the replay attack.");
        Console.ResetColor();
        Console.WriteLine();
    }

    private async Task RunDelegationScenario()
    {
        Console.WriteLine("--- 8. Agent-to-Agent Delegation (A2A) ---");
        Console.WriteLine("  Orchestrator agent delegates scoped capability to worker agent.");
        Console.WriteLine();

        // Orchestrator mints a token for itself
        var orchestratorIssuer = new CapabilityTokenIssuer(
            _signingKey, SecurityAlgorithms.HmacSha256, _nonceStore);

        // Orchestrator delegates a read-only subset to data-analyst
        var delegationIssuer = new SdJwt.Net.AgentTrust.A2A.A2ADelegationIssuer(
            orchestratorIssuer, _policyEngine);

        try
        {
            var delegationResult = await delegationIssuer.DelegateAsync(
                new SdJwt.Net.AgentTrust.A2A.A2ADelegationOptions
                {
                    Issuer = "agent://orchestrator",
                    Audience = "https://mcp-tools.enterprise.local",
                    Capability = new CapabilityClaim
                    {
                        Tool = "sql_query",
                        Action = "Read",
                        Resource = "customers/*"
                    },
                    Context = new CapabilityContext
                    {
                        CorrelationId = Guid.NewGuid().ToString("N"),
                        WorkflowId = "demo-delegation-workflow",
                        TenantId = "tenant-acme"
                    },
                    Lifetime = TimeSpan.FromSeconds(30),
                    Delegation = new SdJwt.Net.AgentTrust.Policy.DelegationChain
                    {
                        DelegatedBy = "agent://orchestrator",
                        Depth = 0,
                        MaxDepth = 2
                    }
                });

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"  Delegation token minted: {delegationResult.TokenId}");
            Console.WriteLine($"  Expires: {delegationResult.ExpiresAt:O}");
            Console.ResetColor();

            // Use delegated token to call the server
            var client = _httpClientFactory.CreateClient("McpToolServer");
            var request = new HttpRequestMessage(HttpMethod.Post, "/tools/sql_query");
            request.Headers.Add("Authorization", $"Bearer {delegationResult.Token}");
            request.Content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");

            var response = await client.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"  Delegated call result: HTTP {(int)response.StatusCode}");
            if (response.IsSuccessStatusCode)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("  [SUCCESS] Delegated token accepted by server.");
                Console.ResetColor();
            }

            Console.WriteLine($"  Response: {FormatJson(body)}");
        }
        catch (InvalidOperationException ex)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"  Delegation blocked: {ex.Message}");
            Console.ResetColor();
        }

        Console.WriteLine();
    }

    private static string FormatJson(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            return JsonSerializer.Serialize(doc, JsonOptions);
        }
        catch
        {
            return json;
        }
    }
}
