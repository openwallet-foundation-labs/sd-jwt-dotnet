# SdJwt.Net.AgentTrust.Policy.Opa

Open Policy Agent (OPA) integration for externalized agent trust policy evaluation.

## Features

-   HTTP-based OPA policy evaluation implementing `IPolicyEngine`
-   Configurable endpoint and policy path
-   Request/response mapping to OPA input/output formats
-   DI registration extensions

## Usage

```csharp
using SdJwt.Net.AgentTrust.Policy.Opa;

builder.Services.AddAgentTrustOpaPolicy(options =>
{
    options.BaseUrl = "http://localhost:8181";
    options.PolicyPath = "/v1/data/agenttrust/allow";
});
```
