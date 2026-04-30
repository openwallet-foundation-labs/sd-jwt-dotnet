using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using System.ComponentModel;
using System.Text.Json;

namespace McpTrustDemo.Llm;

/// <summary>
/// Runs an interactive LLM agent loop with trust-gated tool calling.
/// The LLM decides which tools to invoke; the trust layer enforces
/// whether the call is permitted via SD-JWT capability tokens.
/// </summary>
public class LlmAgentRunner
{
    private readonly IChatClient _chatClient;
    private readonly TrustedToolExecutor _toolExecutor;
    private readonly ILogger<LlmAgentRunner> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public LlmAgentRunner(
        IChatClient chatClient,
        TrustedToolExecutor toolExecutor,
        ILogger<LlmAgentRunner> logger)
    {
        _chatClient = chatClient;
        _toolExecutor = toolExecutor;
        _logger = logger;
    }

    /// <summary>
    /// Runs the interactive agent loop with preset demo prompts and
    /// an optional interactive mode.
    /// </summary>
    public async Task RunInteractiveAsync()
    {
        var tools = CreateToolDefinitions();

        var systemPrompt = """
            You are an enterprise AI assistant with access to the following MCP tools:
            - sql_query: Query employee and business data (action: Read)
            - customer_lookup: Look up customer information (action: Read)
            - file_browser: Browse and read files (action: Read)
            - email_sender: Send emails (action: Send)
            - code_executor: Execute code snippets (action: Execute)
            - secrets_vault: Access secrets and credentials (action: Read)

            When a user asks a question, decide which tool(s) to call.
            You MUST use the available tools to answer questions - do not make up data.
            If a tool call is denied by the trust layer, explain to the user that
            you don't have permission for that action and suggest alternatives.
            """;

        // Run through demo scenarios first
        var demoPrompts = new[]
        {
            "Show me all employees in the Engineering department.",
            "Look up the details for our customer Acme Corporation.",
            "List the files in the /reports directory.",
            "Send an email to bob@example.com about the quarterly report.",
            "Execute this Python code: print('hello world')",
            "Read the database password from the secrets vault."
        };

        Console.WriteLine("Running demo scenarios (LLM picks tools, trust layer gates):");
        Console.WriteLine("-------------------------------------------------------------");
        Console.WriteLine();

        foreach (var prompt in demoPrompts)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"User: {prompt}");
            Console.ResetColor();
            Console.WriteLine();

            await RunSingleTurnAsync(systemPrompt, prompt, tools);

            Console.WriteLine();
            Console.WriteLine("---");
            Console.WriteLine();
        }

        // Interactive mode
        Console.WriteLine("=============================================================");
        Console.WriteLine("  Interactive mode - type a question or 'quit' to exit");
        Console.WriteLine("=============================================================");
        Console.WriteLine();

        while (true)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("You: ");
            Console.ResetColor();

            var input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input) || input.Equals("quit", StringComparison.OrdinalIgnoreCase))
                break;

            Console.WriteLine();
            await RunSingleTurnAsync(systemPrompt, input, tools);
            Console.WriteLine();
        }
    }

    private async Task RunSingleTurnAsync(string systemPrompt, string userMessage, IList<AITool> tools)
    {
        var messages = new List<ChatMessage>
        {
            new(ChatRole.System, systemPrompt),
            new(ChatRole.User, userMessage)
        };

        var options = new ChatOptions { Tools = tools };

        // LLM may produce multiple rounds of tool calls
        const int maxRounds = 5;
        for (var round = 0; round < maxRounds; round++)
        {
            ChatResponse response;
            try
            {
                response = await _chatClient.GetResponseAsync(messages, options);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"  LLM error: {ex.Message}");
                Console.ResetColor();
                return;
            }

            // Process response messages
            var hasToolCalls = false;
            foreach (var msg in response.Messages)
            {
                messages.Add(msg);

                foreach (var content in msg.Contents)
                {
                    if (content is FunctionCallContent functionCall)
                    {
                        hasToolCalls = true;
                        await HandleToolCallAsync(functionCall, messages);
                    }
                    else if (content is TextContent text && !string.IsNullOrWhiteSpace(text.Text))
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine($"  Assistant: {text.Text}");
                        Console.ResetColor();
                    }
                }
            }

            if (!hasToolCalls)
                break;
        }
    }

    private async Task HandleToolCallAsync(FunctionCallContent functionCall, List<ChatMessage> messages)
    {
        var toolName = functionCall.Name;
        var action = DeriveAction(toolName);

        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine($"  [LLM wants: {toolName}/{action}]");
        Console.ResetColor();

        // Convert arguments
        Dictionary<string, object>? args = null;
        if (functionCall.Arguments != null)
        {
            args = new Dictionary<string, object>();
            foreach (var kvp in functionCall.Arguments)
            {
                if (kvp.Value != null)
                    args[kvp.Key] = kvp.Value;
            }
        }

        // Execute through trust layer
        var result = await _toolExecutor.ExecuteAsync(toolName, action, args);

        if (result.DeniedByPolicy)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"  >> TRUST LAYER DENIED: {result.Error}");
            Console.ResetColor();
        }
        else if (result.Success)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"  >> Tool executed (token: {result.TokenId?[..8]}...)");
            Console.ResetColor();
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"  >> Tool failed: {result.Error}");
            Console.ResetColor();
        }

        // Feed result back to LLM
        var resultText = result.Success
            ? result.Data ?? "Success (no data)"
            : result.Error ?? "Unknown error";

        messages.Add(new ChatMessage(ChatRole.Tool,
            [new FunctionResultContent(functionCall.CallId, resultText)]));
    }

    private static string DeriveAction(string toolName) => toolName switch
    {
        "email_sender" => "Send",
        "code_executor" => "Execute",
        _ => "Read"
    };

    private static IList<AITool> CreateToolDefinitions()
    {
        return
        [
            AIFunctionFactory.Create(
                (string query) => "placeholder",
                new AIFunctionFactoryOptions
                {
                    Name = "sql_query",
                    Description = "Execute a SQL query against the employee/business database. " +
                        "Use this to look up employees, departments, sales data, etc."
                }),
            AIFunctionFactory.Create(
                (string customerId) => "placeholder",
                new AIFunctionFactoryOptions
                {
                    Name = "customer_lookup",
                    Description = "Look up customer information by ID or name. " +
                        "Returns customer details, tier, contact history, and open tickets."
                }),
            AIFunctionFactory.Create(
                (string path) => "placeholder",
                new AIFunctionFactoryOptions
                {
                    Name = "file_browser",
                    Description = "Browse and list files in a directory. " +
                        "Use this to find reports, data files, and documents."
                }),
            AIFunctionFactory.Create(
                (string to, string subject, string body) => "placeholder",
                new AIFunctionFactoryOptions
                {
                    Name = "email_sender",
                    Description = "Send an email to a recipient. " +
                        "Requires to address, subject, and body."
                }),
            AIFunctionFactory.Create(
                (string code, string language) => "placeholder",
                new AIFunctionFactoryOptions
                {
                    Name = "code_executor",
                    Description = "Execute a code snippet in a sandboxed environment. " +
                        "Supports Python, JavaScript, and C#."
                }),
            AIFunctionFactory.Create(
                (string secretName) => "placeholder",
                new AIFunctionFactoryOptions
                {
                    Name = "secrets_vault",
                    Description = "Read a secret or credential from the secure vault. " +
                        "Use for database passwords, API keys, connection strings."
                })
        ];
    }
}
