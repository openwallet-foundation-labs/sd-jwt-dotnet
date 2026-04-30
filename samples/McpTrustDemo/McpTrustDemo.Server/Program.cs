using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using SdJwt.Net.AgentTrust.AspNetCore;
using SdJwt.Net.AgentTrust.Core;
using SdJwt.Net.AgentTrust.Mcp;
using SdJwt.Net.AgentTrust.OpenTelemetry;
using SdJwt.Net.AgentTrust.Policy;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// -------------------------------------------------------------------
// Shared signing key (in production, resolve from JWKS endpoint)
// -------------------------------------------------------------------
var sharedKeyBytes = Encoding.UTF8.GetBytes(
    builder.Configuration["AgentTrust:SigningKey"] ?? "McpTrustDemoSigningKey-32Bytes!!");
var signingKey = new SymmetricSecurityKey(sharedKeyBytes);

// -------------------------------------------------------------------
// Policy engine: define what agents can do with each tool
// -------------------------------------------------------------------
var policyRules = new PolicyBuilder()
    // Data Analyst can query and summarize, but not modify
    .Allow("agent://data-analyst", "sql_query", "Read", c =>
    {
        c.MaxLifetime(TimeSpan.FromSeconds(120));
        c.Limits(new CapabilityLimits { MaxResults = 1000 });
    })
    .Allow("agent://data-analyst", "file_browser", "Read")
    // Customer Support can read customer data and send emails
    .Allow("agent://customer-support", "customer_lookup", "Read")
    .Allow("agent://customer-support", "email_sender", "Send", c =>
    {
        c.MaxLifetime(TimeSpan.FromSeconds(30));
        c.Limits(new CapabilityLimits { MaxInvocations = 5 });
    })
    // Code Assistant can read files and execute code in sandbox
    .Allow("agent://code-assistant", "file_browser", "Read")
    .Allow("agent://code-assistant", "code_executor", "Execute", c =>
    {
        c.MaxLifetime(TimeSpan.FromSeconds(60));
        c.Limits(new CapabilityLimits { MaxPayloadBytes = 10240 });
    })
    // Orchestrator can delegate sql_query:Read to worker agents
    .Allow("agent://orchestrator", "sql_query", "Read")
    // Enterprise LLM assistant: read-only access to safe tools
    .Allow("agent://enterprise-assistant", "sql_query", "Read")
    .Allow("agent://enterprise-assistant", "customer_lookup", "Read")
    .Allow("agent://enterprise-assistant", "file_browser", "Read")
    // Global deny: no agent can delete anything
    .Deny("*", "*", "Delete")
    // Global deny: no agent can access secrets
    .Deny("*", "secrets_vault", "*")
    .Build();

var policyEngine = new DefaultPolicyEngine(policyRules);

// -------------------------------------------------------------------
// Register Agent Trust services
// -------------------------------------------------------------------
var nonceStore = new MemoryNonceStore();
builder.Services.AddSingleton<INonceStore>(nonceStore);
builder.Services.AddSingleton<IPolicyEngine>(policyEngine);
builder.Services.AddSingleton(new CapabilityTokenVerifier(nonceStore));

builder.Services.AddMcpServerTrust(options =>
{
    options.Audience = "https://mcp-tools.enterprise.local";
    options.TrustedIssuers = new Dictionary<string, SecurityKey>
    {
        ["agent://data-analyst"] = signingKey,
        ["agent://customer-support"] = signingKey,
        ["agent://code-assistant"] = signingKey,
        ["agent://orchestrator"] = signingKey,
        ["agent://enterprise-assistant"] = signingKey
    };
});

var verificationOptions = new AgentTrustVerificationOptions
{
    Audience = "https://mcp-tools.enterprise.local",
    TrustedIssuers = new Dictionary<string, SecurityKey>
    {
        ["agent://data-analyst"] = signingKey,
        ["agent://customer-support"] = signingKey,
        ["agent://code-assistant"] = signingKey,
        ["agent://orchestrator"] = signingKey,
        ["agent://enterprise-assistant"] = signingKey
    },
    ExcludedPaths = ["/health", "/tools/manifest"],
    EmitReceipts = true
};
builder.Services.AddSingleton(Options.Create(verificationOptions));

builder.Services.AddSingleton<IReceiptWriter>(new TelemetryReceiptWriter(new LoggingReceiptWriter()));

// -------------------------------------------------------------------
// OpenTelemetry observability
// -------------------------------------------------------------------
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .AddAgentTrustInstrumentation()
        .AddConsoleExporter())
    .WithMetrics(metrics => metrics
        .AddAgentTrustInstrumentation()
        .AddConsoleExporter());

var app = builder.Build();

// -------------------------------------------------------------------
// Middleware pipeline
// Note: Per-endpoint McpServerTrustGuard is used instead of global
// middleware to avoid double nonce consumption (replay false positives).
// -------------------------------------------------------------------

// -------------------------------------------------------------------
// Health check (excluded from trust verification)
// -------------------------------------------------------------------
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTimeOffset.UtcNow }));

// -------------------------------------------------------------------
// Tool manifest endpoint (excluded from trust verification)
// -------------------------------------------------------------------
app.MapGet("/tools/manifest", () => Results.Ok(new McpToolTrustManifest[]
{
    new()
    {
        Name = "sql_query",
        Audience = "https://mcp-tools.enterprise.local",
        Actions = new[] { "Read" },
        RequiresCapabilityToken = true,
        MaxTokenLifetime = TimeSpan.FromSeconds(120)
    },
    new()
    {
        Name = "file_browser",
        Audience = "https://mcp-tools.enterprise.local",
        Actions = new[] { "Read", "List" },
        RequiresCapabilityToken = true,
        MaxTokenLifetime = TimeSpan.FromSeconds(60)
    },
    new()
    {
        Name = "customer_lookup",
        Audience = "https://mcp-tools.enterprise.local",
        Actions = new[] { "Read" },
        RequiresCapabilityToken = true
    },
    new()
    {
        Name = "email_sender",
        Audience = "https://mcp-tools.enterprise.local",
        Actions = new[] { "Send" },
        RequiresCapabilityToken = true,
        MaxTokenLifetime = TimeSpan.FromSeconds(30)
    },
    new()
    {
        Name = "code_executor",
        Audience = "https://mcp-tools.enterprise.local",
        Actions = new[] { "Execute" },
        RequiresCapabilityToken = true,
        MaxTokenLifetime = TimeSpan.FromSeconds(60)
    },
    new()
    {
        Name = "secrets_vault",
        Audience = "https://mcp-tools.enterprise.local",
        Actions = new[] { "Read", "Write" },
        RequiresCapabilityToken = true
    }
}));

// -------------------------------------------------------------------
// MCP Tool Endpoints (protected by capability tokens)
// -------------------------------------------------------------------
app.MapPost("/tools/sql_query", async (HttpContext context, McpServerTrustGuard guard) =>
{
    var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
    var result = await guard.VerifyToolCallAsync("sql_query", token);

    if (!result.IsValid)
    {
        return Results.Json(new { error = result.Error, code = result.ErrorCode }, statusCode: 403);
    }

    // Simulated SQL query execution
    return Results.Ok(new
    {
        tool = "sql_query",
        action = result.Capability?.Action,
        result = new[]
        {
            new { id = 1, name = "Alice Johnson", email = "alice@example.com", department = "Engineering" },
            new { id = 2, name = "Bob Smith", email = "bob@example.com", department = "Sales" },
            new { id = 3, name = "Carol Williams", email = "carol@example.com", department = "Marketing" }
        },
        metadata = new
        {
            rowsReturned = 3,
            executionTimeMs = 42,
            correlationId = result.Context?.CorrelationId
        }
    });
});

app.MapPost("/tools/file_browser", async (HttpContext context, McpServerTrustGuard guard) =>
{
    var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
    var result = await guard.VerifyToolCallAsync("file_browser", token);

    if (!result.IsValid)
    {
        return Results.Json(new { error = result.Error, code = result.ErrorCode }, statusCode: 403);
    }

    return Results.Ok(new
    {
        tool = "file_browser",
        action = result.Capability?.Action,
        files = new[]
        {
            new { path = "/reports/q4-summary.csv", size = 24576, modified = "2026-04-28T10:30:00Z" },
            new { path = "/reports/annual-review.pdf", size = 1048576, modified = "2026-04-15T14:00:00Z" },
            new { path = "/data/customers.json", size = 51200, modified = "2026-04-30T08:00:00Z" }
        },
        correlationId = result.Context?.CorrelationId
    });
});

app.MapPost("/tools/customer_lookup", async (HttpContext context, McpServerTrustGuard guard) =>
{
    var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
    var result = await guard.VerifyToolCallAsync("customer_lookup", token);

    if (!result.IsValid)
    {
        return Results.Json(new { error = result.Error, code = result.ErrorCode }, statusCode: 403);
    }

    return Results.Ok(new
    {
        tool = "customer_lookup",
        customer = new
        {
            id = "cust-9281",
            name = "Acme Corporation",
            tier = "Enterprise",
            lastContact = "2026-04-25T09:15:00Z",
            openTickets = 2
        },
        correlationId = result.Context?.CorrelationId
    });
});

app.MapPost("/tools/email_sender", async (HttpContext context, McpServerTrustGuard guard) =>
{
    var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
    var result = await guard.VerifyToolCallAsync("email_sender", token);

    if (!result.IsValid)
    {
        return Results.Json(new { error = result.Error, code = result.ErrorCode }, statusCode: 403);
    }

    return Results.Ok(new
    {
        tool = "email_sender",
        status = "sent",
        messageId = Guid.NewGuid().ToString("N"),
        correlationId = result.Context?.CorrelationId
    });
});

app.MapPost("/tools/code_executor", async (HttpContext context, McpServerTrustGuard guard) =>
{
    var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
    var result = await guard.VerifyToolCallAsync("code_executor", token);

    if (!result.IsValid)
    {
        return Results.Json(new { error = result.Error, code = result.ErrorCode }, statusCode: 403);
    }

    return Results.Ok(new
    {
        tool = "code_executor",
        action = result.Capability?.Action,
        output = "Hello, World!\nExecution completed in 120ms.",
        exitCode = 0,
        correlationId = result.Context?.CorrelationId
    });
});

app.MapPost("/tools/secrets_vault", async (HttpContext context, McpServerTrustGuard guard) =>
{
    var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
    var result = await guard.VerifyToolCallAsync("secrets_vault", token);

    if (!result.IsValid)
    {
        return Results.Json(new { error = result.Error, code = result.ErrorCode }, statusCode: 403);
    }

    // This should never be reached because policy denies all agents
    return Results.Ok(new { tool = "secrets_vault", secret = "SHOULD_NOT_BE_VISIBLE" });
});

app.Run();
