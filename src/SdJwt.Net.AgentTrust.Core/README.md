# SdJwt.Net.AgentTrust.Core

Core primitives for issuing and verifying SD-JWT capability tokens used in agent-to-tool and agent-to-agent trust flows.

## Install

```bash
dotnet add package SdJwt.Net.AgentTrust.Core
```

## What This Package Provides

-   `CapabilityTokenIssuer` for minting SD-JWT capability tokens.
-   `CapabilityTokenVerifier` for validating signature, audience, expiry, and replay constraints.
-   `IKeyCustodyProvider` and `InMemoryKeyCustodyProvider` for signing key access.
-   `INonceStore` and `MemoryNonceStore` for replay prevention.
-   `IReceiptWriter` and `LoggingReceiptWriter` for allow/deny audit receipts.
-   Capability data models (`CapabilityClaim`, `CapabilityContext`, `CapabilityLimits`).

## Quick Start

```csharp
using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.AgentTrust.Core;
using System.Security.Cryptography;

var signingBytes = RandomNumberGenerator.GetBytes(32);
var signingKey = new SymmetricSecurityKey(signingBytes);
var nonceStore = new MemoryNonceStore();

var issuer = new CapabilityTokenIssuer(
    signingKey,
    SecurityAlgorithms.HmacSha256,
    nonceStore);

var minted = issuer.Mint(new CapabilityTokenOptions
{
    Issuer = "agent://assistant-1",
    Audience = "https://tool-api.example.com",
    Capability = new CapabilityClaim
    {
        Tool = "crm",
        Action = "Read",
        Resource = "customer-profile",
        Limits = new CapabilityLimits { MaxResults = 25 }
    },
    Context = new CapabilityContext
    {
        CorrelationId = Guid.NewGuid().ToString("N"),
        WorkflowId = "wf-123"
    },
    Lifetime = TimeSpan.FromMinutes(1)
});

var verifier = new CapabilityTokenVerifier(nonceStore);
var verification = await verifier.VerifyAsync(
    minted.Token,
    new CapabilityVerificationOptions
    {
        ExpectedAudience = "https://tool-api.example.com",
        TrustedIssuers = new Dictionary<string, SecurityKey>
        {
            ["agent://assistant-1"] = signingKey
        }
    });

if (!verification.IsValid)
{
    throw new InvalidOperationException(verification.Error);
}
```

## Core Validation Behavior

-   Requires `iss`, `aud`, `jti`, `exp`, `cap`, and `ctx` claims.
-   Validates signature against issuer key in `TrustedIssuers`.
-   Enforces audience equality with `ExpectedAudience`.
-   Rejects expired tokens with configurable `ClockSkewTolerance`.
-   Enforces replay protection when `EnforceReplayPrevention` is enabled.

## Recommended Production Practices

-   Use a hardware-backed key provider through `IKeyCustodyProvider`.
-   Keep token lifetime short (default is 60 seconds).
-   Store replay nonces in distributed storage for multi-node deployments.
-   Write receipts to durable audit storage via a custom `IReceiptWriter`.

## Related Packages

-   `SdJwt.Net.AgentTrust.Policy`
-   `SdJwt.Net.AgentTrust.AspNetCore`
-   `SdJwt.Net.AgentTrust.Maf`
