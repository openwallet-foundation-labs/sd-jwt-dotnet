# SdJwt.Net.OidFederation - OpenID Federation

[![NuGet Version](https://img.shields.io/nuget/v/SdJwt.Net.OidFederation.svg)](https://www.nuget.org/packages/SdJwt.Net.OidFederation/)
[![License](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](https://opensource.org/licenses/Apache-2.0)

Implementation of **OpenID Federation 1.0** for trust management and entity federation. The package provides trust chain resolution, entity configuration validation, metadata policy processing, and trust mark handling.

## Features

- **OpenID Federation 1.0**: Core model and validation support
- **Trust Chain Resolution**: Resolve chains from entity to configured trust anchors
- **Metadata Policy Processing**: Apply policy operators across the chain
- **Trust Marks and Constraints**: Evaluate trust marks and path constraints
- **HTTP + Cache Controls**: Resolver options for timeout, max size, and cache behavior

## Installation

```bash
dotnet add package SdJwt.Net.OidFederation
```

## Quick Start

### Entity Configuration Model

```csharp
using SdJwt.Net.OidFederation.Models;
using Microsoft.IdentityModel.Tokens;

var entityConfiguration = EntityConfiguration.Create(
    entityUrl: "https://issuer.example.com",
    jwkSet: new
    {
        keys = new[] { JsonWebKeyConverter.ConvertFromSecurityKey(entityPublicKey) }
    },
    validityHours: 24);

entityConfiguration.AuthorityHints = new[] { "https://trust-anchor.example.com" };
entityConfiguration.Metadata = new EntityMetadata
{
    OpenIdCredentialIssuer = new
    {
        credential_issuer = "https://issuer.example.com",
        credential_endpoint = "https://issuer.example.com/credentials"
    }
};

entityConfiguration.Validate();
```

### Trust Chain Resolution

```csharp
using SdJwt.Net.OidFederation.Logic;
using Microsoft.IdentityModel.Tokens;

var trustAnchors = new Dictionary<string, SecurityKey>
{
    ["https://trust-anchor.example.com"] = trustAnchorPublicKey
};

var resolver = new TrustChainResolver(httpClient, trustAnchors);
var trustChain = await resolver.ResolveAsync("https://leaf-entity.example.com");

if (trustChain.IsValid)
{
    var validatedMetadata = trustChain.ValidatedMetadata;
    var chainEntities = trustChain.GetTrustChainEntities();
}
else
{
    Console.WriteLine($"Trust resolution failed: {trustChain.ErrorMessage}");
}
```

### Enforce Trust Requirements

```csharp
var requirements = TrustChainRequirements.ForProtocol(
    protocol: "openid_credential_issuer",
    maxPathLength: 5);

requirements.AllowedTrustAnchors = new[] { "https://trust-anchor.example.com" };
requirements.RequiredTrustMarks = new[] { "https://trust.example.com/marks/regulated-issuer" };

if (!trustChain.SatisfiesRequirements(requirements))
{
    throw new InvalidOperationException("Issuer does not satisfy federation requirements.");
}
```

## Federation Scenarios

- **University Trust Chains**: Academic institution verification across regions
- **Government Entity Trust**: Cross-agency trust establishment
- **Healthcare Networks**: Medical provider trust verification
- **Corporate Federation**: Enterprise identity federation management

## Documentation

For complete end-to-end integration, see:
- [Federation guide](../../docs/guides/establishing-trust.md)
- [Architecture deep dive](../../docs/concepts/architecture.md)

## License

Licensed under the [Apache License 2.0](https://opensource.org/licenses/Apache-2.0).
