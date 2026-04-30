using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.AgentTrust.Core;
using SdJwt.Net.AgentTrust.Mcp;
using SdJwt.Net.AgentTrust.Policy;
using System.Net.Http.Json;
using System.Text.Json;

namespace McpTrustDemo.Llm;

/// <summary>
/// Executes MCP tool calls through the SD-JWT trust layer.
/// Mints a capability token per call, attaches it, and handles
/// policy denials and server rejections gracefully.
/// </summary>
public class TrustedToolExecutor
{
    private readonly McpClientTrustInterceptor _interceptor;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<TrustedToolExecutor> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public TrustedToolExecutor(
        IPolicyEngine policyEngine,
        INonceStore nonceStore,
        SymmetricSecurityKey signingKey,
        IHttpClientFactory httpClientFactory,
        ILogger<TrustedToolExecutor> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;

        _interceptor = new McpClientTrustInterceptor(
            new CapabilityTokenIssuer(signingKey, SecurityAlgorithms.HmacSha256, nonceStore),
            policyEngine,
            new McpClientTrustOptions
            {
                AgentId = "agent://enterprise-assistant",
                ToolAudienceMapping = new Dictionary<string, string>
                {
                    ["sql_query"] = "https://mcp-tools.enterprise.local",
                    ["customer_lookup"] = "https://mcp-tools.enterprise.local",
                    ["file_browser"] = "https://mcp-tools.enterprise.local",
                    ["email_sender"] = "https://mcp-tools.enterprise.local",
                    ["code_executor"] = "https://mcp-tools.enterprise.local",
                    ["secrets_vault"] = "https://mcp-tools.enterprise.local"
                },
                DefaultTokenLifetime = TimeSpan.FromSeconds(60)
            });
    }

    /// <summary>
    /// Attempts to execute a tool call through the trust layer.
    /// Returns a structured result the LLM can reason about.
    /// </summary>
    public async Task<ToolExecutionResult> ExecuteAsync(
        string toolName,
        string action,
        Dictionary<string, object>? arguments = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Tool call: {Tool}/{Action}", toolName, action);

        // Step 1: Policy pre-flight + token minting
        var mintResult = await _interceptor.BeforeToolCallAsync(
            new McpToolCall
            {
                ToolName = toolName,
                Action = action,
                Arguments = arguments,
                Context = new CapabilityContext
                {
                    CorrelationId = Guid.NewGuid().ToString("N"),
                    WorkflowId = "llm-agent-session"
                }
            }, cancellationToken);

        if (!mintResult.IsSuccess)
        {
            _logger.LogWarning("Trust layer DENIED: {Tool}/{Action} - {Error}",
                toolName, action, mintResult.Error);

            return new ToolExecutionResult
            {
                Success = false,
                ToolName = toolName,
                Action = action,
                Error = $"ACCESS DENIED by trust policy: {mintResult.Error}",
                DeniedByPolicy = true
            };
        }

        // Step 2: Call the MCP tool server with the capability token
        var client = _httpClientFactory.CreateClient("McpToolServer");
        var request = new HttpRequestMessage(HttpMethod.Post, $"/tools/{toolName}");
        request.Headers.Add("Authorization", $"Bearer {mintResult.Token}");
        request.Content = JsonContent.Create(arguments ?? new Dictionary<string, object>());

        try
        {
            var response = await client.SendAsync(request, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                return new ToolExecutionResult
                {
                    Success = true,
                    ToolName = toolName,
                    Action = action,
                    Data = body,
                    TokenId = mintResult.TokenId
                };
            }

            return new ToolExecutionResult
            {
                Success = false,
                ToolName = toolName,
                Action = action,
                Error = $"Server rejected (HTTP {(int)response.StatusCode}): {body}",
                DeniedByPolicy = false
            };
        }
        catch (HttpRequestException ex)
        {
            return new ToolExecutionResult
            {
                Success = false,
                ToolName = toolName,
                Action = action,
                Error = $"Connection error: {ex.Message}. Is the MCP server running on {client.BaseAddress}?"
            };
        }
    }
}

/// <summary>
/// Result of a trusted tool execution.
/// </summary>
public class ToolExecutionResult
{
    public bool Success { get; init; }
    public string ToolName { get; init; } = string.Empty;
    public string Action { get; init; } = string.Empty;
    public string? Data { get; init; }
    public string? Error { get; init; }
    public string? TokenId { get; init; }
    public bool DeniedByPolicy { get; init; }

    public override string ToString()
    {
        if (Success)
            return $"[{ToolName}] Success: {Data}";
        return $"[{ToolName}] Failed: {Error}";
    }
}
