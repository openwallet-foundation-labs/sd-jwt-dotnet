# SdJwt.Net.AgentTrust.AspNetCore

ASP.NET Core integration for inbound agent capability verification and endpoint authorization.

## Install

```bash
dotnet add package SdJwt.Net.AgentTrust.AspNetCore
```

## What This Package Provides

-   `AgentTrustVerificationMiddleware` to verify inbound SD-JWT capability tokens.
-   `AgentTrustVerificationOptions` for audience, trusted issuers, header settings, and excluded paths.
-   `RequireCapabilityAttribute` for tool/action authorization on controllers or endpoints.
-   `HttpContext` extensions (`GetVerifiedCapability`, `GetCapabilityContext`, `GetAgentIssuer`).
-   Service and pipeline helpers in `AgentTrustAspNetCoreExtensions`.

## Minimal Setup

```csharp
using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.AgentTrust.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services
    .AddAgentTrustVerification(options =>
    {
        options.Audience = "https://tool-api.example.com";
        options.TrustedIssuers = new Dictionary<string, SecurityKey>
        {
            ["agent://assistant-1"] = new SymmetricSecurityKey(
                Convert.FromBase64String(builder.Configuration["AgentTrust:Key"]!))
        };
    });

var app = builder.Build();
app.UseAgentTrustVerification();
app.MapControllers();
app.Run();
```

## Endpoint Protection Example

```csharp
using Microsoft.AspNetCore.Mvc;
using SdJwt.Net.AgentTrust.AspNetCore;

[ApiController]
[Route("tools/customers")]
public sealed class CustomersController : ControllerBase
{
    [HttpGet("{id}")]
    [RequireCapability("crm", "Read")]
    public IActionResult GetCustomer(string id)
    {
        var capability = HttpContext.GetVerifiedCapability();
        return Ok(new { id, capability?.Tool, capability?.Action });
    }
}
```

## Defaults

-   Header name: `Authorization`
-   Header prefix: `SdJwt`
-   Excluded paths: `/health`, `/ready`, `/.well-known/*`
-   Replay prevention and receipt writing are enabled by default.

## Related Packages

-   `SdJwt.Net.AgentTrust.Core`
-   `SdJwt.Net.AgentTrust.Policy`
