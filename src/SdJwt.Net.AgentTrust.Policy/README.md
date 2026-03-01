# SdJwt.Net.AgentTrust.Policy

Policy evaluation layer for agent trust decisions, including rule-based allow/deny logic and delegation constraints.

## Install

```bash
dotnet add package SdJwt.Net.AgentTrust.Policy
```

## What This Package Provides

-   `IPolicyEngine` abstraction for policy evaluation.
-   `DefaultPolicyEngine` for wildcard rule matching and deterministic allow/deny decisions.
-   `PolicyBuilder` for fluent rule configuration.
-   Delegation model support via `DelegationChain` and `DelegationTokenOptions`.
-   Constraint model (`PolicyConstraints`) for max token lifetime, required disclosures, and capability limits.

## Quick Start

```csharp
using SdJwt.Net.AgentTrust.Core;
using SdJwt.Net.AgentTrust.Policy;

var rules = new PolicyBuilder()
    .Deny("*", "payments", "Delete")
    .Allow("agent://ops-*", "payments", "Read", c =>
    {
        c.MaxLifetime(TimeSpan.FromSeconds(45));
        c.RequireDisclosure("ctx.correlationId");
        c.Limits(new CapabilityLimits { MaxResults = 100 });
    })
    .Build();

var policyEngine = new DefaultPolicyEngine(rules);

var decision = await policyEngine.EvaluateAsync(new PolicyRequest
{
    AgentId = "agent://ops-eu",
    Tool = "payments",
    Action = "Read",
    Resource = "merchant/42",
    Context = new CapabilityContext { CorrelationId = "corr-1" }
});

if (!decision.IsPermitted)
{
    throw new InvalidOperationException(decision.DenialReason);
}
```

## Rule Semantics

-   Rules are evaluated by descending `Priority`.
-   `*` wildcard matching is supported for agent/tool/action/resource patterns.
-   First matching rule decides the outcome.
-   Deny rules should have higher priority than allow rules.

## Delegation Notes

-   `DefaultPolicyEngine` validates delegation depth (`Depth <= MaxDepth`).
-   Optional `AllowedActions` in a delegation chain are enforced before rule matching.
-   Use short lifetimes and explicit disclosures for delegated actions.

## Related Packages

-   `SdJwt.Net.AgentTrust.Core`
-   `SdJwt.Net.AgentTrust.AspNetCore`
-   `SdJwt.Net.AgentTrust.Maf`
