using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using ReleaseSupport.InvestigatorA2A;
using ReleaseSupport.Shared;
using SdJwt.Net.AgentTrust.AspNetCore;
using SdJwt.Net.AgentTrust.Core;
using SdJwt.Net.AgentTrust.Policy;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// --- Configuration ---
var signingKeyText = builder.Configuration["AgentTrust:SigningKey"]
    ?? "EntraA2ADemoSigningKey-32Bytes!!";
var tenantId = builder.Configuration["Investigator:TenantId"]
    ?? Constants.Agents.DevTenantId;

var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKeyText));

var coordinatorId = string.Format(Constants.Agents.CoordinatorPattern, tenantId);
var investigatorId = string.Format(Constants.Agents.InvestigatorPattern, tenantId);

// --- Agent Trust Services (SDK middleware) ---
// Note: AgentTrustVerificationOptions uses init-only properties, so we construct
// the options instance directly and register via IOptions<T>.
var trustOptions = new AgentTrustVerificationOptions
{
    Audience = investigatorId,
    TrustedIssuers = new Dictionary<string, SecurityKey>(StringComparer.Ordinal)
    {
        [coordinatorId] = signingKey
    },
    ExcludedPaths = ["/health", "/ready", "/.well-known/*", Constants.Endpoints.AgentCard]
};
builder.Services.AddSingleton<IOptions<AgentTrustVerificationOptions>>(Options.Create(trustOptions));
builder.Services.AddSingleton<INonceStore, MemoryNonceStore>();
builder.Services.AddSingleton<CapabilityTokenVerifier>();
builder.Services.AddSingleton<IReceiptWriter, LoggingReceiptWriter>();

// Override the default deny-all policy with demo rules
var policyRules = new PolicyBuilder()
    .Allow(coordinatorId, Constants.Tools.ReleaseInvestigation, Constants.Tools.InvestigateAction)
    .Deny("*", Constants.Tools.ReleaseInvestigation, Constants.Tools.RerunWorkflowAction)
    .AllowDelegation(coordinatorId, investigatorId, maxDepth: 1)
    .Build();
builder.Services.AddSingleton<IPolicyEngine>(new DefaultPolicyEngine(policyRules));

// --- Application Services ---
builder.Services.AddHttpClient<GitHubReleaseClient>();
builder.Services.AddHttpClient<NuGetPackageClient>();
builder.Services.AddSingleton<ReleaseDiagnosisService>();
builder.Services.AddSingleton<ReleaseInvestigatorAgent>();

// --- OpenTelemetry ---
builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService("ReleaseInvestigatorA2A"))
    .WithTracing(tp => tp
        .AddSource("SdJwt.Net.AgentTrust")
        .AddConsoleExporter());

var app = builder.Build();

// --- Middleware (SDK: reads Authorization: Bearer, verifies token, evaluates policy) ---
app.UseAgentTrustVerification();

// --- A2A Endpoints ---

// Agent card discovery (excluded from middleware, no auth required)
app.MapGet(Constants.Endpoints.AgentCard, () =>
{
    var card = new A2AAgentCard
    {
        Name = "Release Investigator",
        Description = "Investigates NuGet release pipeline status using GitHub and NuGet APIs.",
        Version = "1.0.0",
        ProtocolVersion = "1.0",
        RequiresAgentTrust = true,
        Capabilities = [Constants.Tools.InvestigateAction]
    };
    return Results.Json(card);
});

// A2A message endpoint (capability verified by SDK middleware)
app.MapPost(Constants.Endpoints.MessageStream, async (
    HttpContext httpContext,
    ReleaseInvestigatorAgent agent) =>
{
    A2AMessageEnvelope? envelope;
    try
    {
        envelope = await httpContext.Request.ReadFromJsonAsync<A2AMessageEnvelope>();
    }
    catch (JsonException)
    {
        return Results.BadRequest(new
        {
            error = "Invalid A2A message format."
        });
    }

    if (envelope == null)
    {
        return Results.BadRequest(new
        {
            error = "Empty request body."
        });
    }

    var capability = httpContext.GetVerifiedCapability();
    if (capability == null)
    {
        return Results.StatusCode(StatusCodes.Status403Forbidden);
    }

    var result = await agent.HandleAsync(envelope, capability, httpContext.RequestAborted);

    // Wrap the result as an A2A response message
    var response = new A2AMessage
    {
        Kind = "message",
        Role = "agent",
        MessageId = Guid.NewGuid().ToString("N"),
        ContextId = envelope.Message?.ContextId ?? string.Empty,
        Parts =
        [
            new A2AMessagePart
            {
                Kind = "text",
                Text = result.Success
                    ? FormatSuccessResponse(result)
                    : $"Investigation failed: {result.Error}"
            }
        ]
    };

    return Results.Json(new A2AMessageEnvelope { Message = response });
});

app.Run();

static string FormatSuccessResponse(ReleaseInvestigationResult result)
{
    var sb = new StringBuilder();
    sb.AppendLine($"Diagnosis: {result.Diagnosis}");
    sb.AppendLine($"Recommended Action: {result.RecommendedAction}");
    sb.AppendLine();
    sb.AppendLine($"Tag: {result.GitTag}");
    sb.AppendLine($"Release: {result.GitHubRelease}");
    sb.AppendLine($"Workflow: {result.PublishWorkflow}");
    sb.AppendLine($"NuGet: {result.NuGetPackage}");
    return sb.ToString();
}
