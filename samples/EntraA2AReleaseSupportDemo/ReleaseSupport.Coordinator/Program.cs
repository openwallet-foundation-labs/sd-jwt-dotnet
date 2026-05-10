using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using ReleaseSupport.Coordinator;
using ReleaseSupport.Shared;
using SdJwt.Net.AgentTrust.A2A;
using SdJwt.Net.AgentTrust.Core;
using SdJwt.Net.AgentTrust.Policy;
using System.Text;

// --- Parse CLI arguments ---
var repo = GetArg(args, "--repo") ?? "openwallet-foundation-labs/sd-jwt-dotnet";
var packageId = GetArg(args, "--package") ?? "SdJwt.Net";
var version = GetArg(args, "--version") ?? "1.0.1";
var action = GetArg(args, "--action") ?? Constants.Tools.InvestigateAction;
var identityModeStr = GetArg(args, "--identity-mode") ?? "DevFallbackStatic";
var investigatorBaseUrl = GetArg(args, "--investigator-url") ?? "http://localhost:5052";

if (!Enum.TryParse<IdentityMode>(identityModeStr, ignoreCase: true, out var identityMode))
{
    Console.WriteLine($"Invalid identity mode: {identityModeStr}");
    Console.WriteLine("Valid modes: DevFallbackStatic, DevFallbackAppRegistration, RealEntra");
    return 1;
}

// --- Configuration ---
var signingKeyText = Environment.GetEnvironmentVariable("AGENT_TRUST_SIGNING_KEY")
    ?? "EntraA2ADemoSigningKey-32Bytes!!";
var tenantId = Environment.GetEnvironmentVariable("AGENT_TRUST_TENANT_ID")
    ?? Constants.Agents.DevTenantId;

var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKeyText));
var coordinatorId = string.Format(Constants.Agents.CoordinatorPattern, tenantId);
var investigatorId = string.Format(Constants.Agents.InvestigatorPattern, tenantId);

// --- Build host ---
var builder = Host.CreateApplicationBuilder(args);

var nonceStore = new MemoryNonceStore();
builder.Services.AddSingleton<INonceStore>(nonceStore);

var issuer = new CapabilityTokenIssuer(signingKey, SecurityAlgorithms.HmacSha256, nonceStore);
builder.Services.AddSingleton(issuer);

var policyRules = new PolicyBuilder()
    .Allow(coordinatorId, Constants.Tools.ReleaseInvestigation, Constants.Tools.InvestigateAction)
    .Deny("*", Constants.Tools.ReleaseInvestigation, Constants.Tools.RerunWorkflowAction)
    .AllowDelegation(coordinatorId, investigatorId, maxDepth: 1)
    .Build();
var policyEngine = new DefaultPolicyEngine(policyRules);
builder.Services.AddSingleton<IPolicyEngine>(policyEngine);

var delegationIssuer = new A2ADelegationIssuer(issuer, policyEngine);
builder.Services.AddSingleton(delegationIssuer);
builder.Services.AddSingleton(new DelegatedCapabilityIssuer(delegationIssuer));

builder.Services.AddHttpClient<A2AInvestigatorClient>(client =>
{
    client.BaseAddress = new Uri(investigatorBaseUrl);
});

builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService("ReleaseCoordinator"))
    .WithTracing(tp => tp
        .AddSource("SdJwt.Net.AgentTrust")
        .AddConsoleExporter());

using var host = builder.Build();

// --- Run workflow ---
var client = host.Services.GetRequiredService<A2AInvestigatorClient>();
var capabilityIssuer = host.Services.GetRequiredService<DelegatedCapabilityIssuer>();

var workflow = new CoordinatorWorkflow(
    client, capabilityIssuer, coordinatorId, investigatorId);

Console.WriteLine($"Repository:    {repo}");
Console.WriteLine($"Package:       {packageId}");
Console.WriteLine($"Version:       {version}");
Console.WriteLine($"Action:        {action}");
Console.WriteLine($"Identity Mode: {identityMode}");
Console.WriteLine($"Investigator:  {investigatorBaseUrl}");

await workflow.RunAsync(repo, packageId, version, action);

return 0;

static string? GetArg(string[] args, string name)
{
    for (var i = 0; i < args.Length - 1; i++)
    {
        if (string.Equals(args[i], name, StringComparison.OrdinalIgnoreCase))
        {
            return args[i + 1];
        }
    }
    return null;
}
