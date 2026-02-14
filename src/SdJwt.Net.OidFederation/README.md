# SdJwt.Net.OidFederation - OpenID Federation

[![NuGet Version](https://img.shields.io/nuget/v/SdJwt.Net.OidFederation.svg)](https://www.nuget.org/packages/SdJwt.Net.OidFederation/)
[![License](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](https://opensource.org/licenses/Apache-2.0)

Implementation of **OpenID Federation 1.0** specification for trust management and entity federation. Provides complete trust chain resolution, entity configuration, and metadata policy validation.

## Features

- **OpenID Federation 1.0**: Complete specification implementation
- **Trust Chain Resolution**: Automated trust chain validation
- **Entity Configuration**: Automatic fetching and validation
- **Metadata Policy**: Policy application across federation hierarchies  
- **Trust Marks**: Trust mark validation and verification

## Installation

```bash
dotnet add package SdJwt.Net.OidFederation
```

## Quick Start

### Entity Configuration

```csharp
using SdJwt.Net.OidFederation.Models;

var entityConfiguration = new EntityConfiguration
{
    Issuer = "https://federation.example.com",
    Subject = "https://federation.example.com", 
    IssuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
    ExpiresAt = DateTimeOffset.UtcNow.AddDays(30).ToUnixTimeSeconds(),
    Jwks = new JsonWebKeySet
    {
        Keys = new[] { entityPublicKey }
    },
    Authority_Hints = new[] { "https://trust-anchor.example.com" },
    Metadata = new EntityMetadata
    {
        OpenidProvider = new OpenIdProviderMetadata
        {
            Issuer = "https://federation.example.com",
            JwksUri = "https://federation.example.com/.well-known/jwks"
        }
    }
};
```

### Trust Chain Resolution

```csharp
using SdJwt.Net.OidFederation.Services;

var trustChainResolver = new TrustChainResolver(httpClient);

var trustChain = await trustChainResolver.ResolveTrustChainAsync(
    "https://leaf-entity.example.com",
    "https://trust-anchor.example.com");

if (trustChain.IsValid)
{
    var validatedMetadata = trustChain.FinalMetadata;
}
```

## Federation Scenarios

- **University Trust Chains**: Academic institution verification across regions
- **Government Entity Trust**: Cross-agency trust establishment
- **Healthcare Networks**: Medical provider trust verification
- **Corporate Federation**: Enterprise identity federation management

## Documentation

For comprehensive federation setup and trust management patterns, see the [main repository](https://github.com/openwallet-foundation-labs/sd-jwt-dotnet).

## License

Licensed under the [Apache License 2.0](https://opensource.org/licenses/Apache-2.0).
