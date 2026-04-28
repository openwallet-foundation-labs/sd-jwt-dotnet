# SdJwt.Net.AgentTrust.A2A

Agent-to-Agent (A2A) protocol integration for multi-agent delegation and trust.

## Features

-   Agent card discovery and validation
-   Delegation token profiles for inter-agent trust
-   Trust chain validation across agent boundaries
-   Cross-agent capability propagation

## Usage

```csharp
using SdJwt.Net.AgentTrust.A2A;

// Create an agent card for discovery
var card = new AgentCard
{
    AgentId = "agent://alpha",
    DisplayName = "Alpha Agent",
    Capabilities = new[] { "Weather.Read", "Calendar.Write" },
    TrustEndpoint = "https://alpha.example.com/.well-known/agent-trust"
};

// Validate a delegation chain
var validator = new DelegationChainValidator(maxDepth: 3);
var result = await validator.ValidateAsync(delegationToken);
```
