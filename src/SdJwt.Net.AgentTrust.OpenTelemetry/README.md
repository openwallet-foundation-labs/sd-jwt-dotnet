# SdJwt.Net.AgentTrust.OpenTelemetry

OpenTelemetry instrumentation for Agent Trust Kit capability token operations.

## Features

-   Trace enrichment for mint and verify operations
-   Counter and histogram metrics for token operations
-   `TracerProviderBuilder` and `MeterProviderBuilder` extensions

## Usage

```csharp
using SdJwt.Net.AgentTrust.OpenTelemetry;

builder.Services.AddOpenTelemetry()
    .WithTracing(t => t.AddAgentTrustInstrumentation())
    .WithMetrics(m => m.AddAgentTrustInstrumentation());
```
