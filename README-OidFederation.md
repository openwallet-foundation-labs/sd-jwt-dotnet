# SD-JWT.NET - OpenID Federation 1.0

[![NuGet Version](https://img.shields.io/nuget/v/SdJwt.Net.OidFederation.svg)](https://www.nuget.org/packages/SdJwt.Net.OidFederation/)
[![Build Status](https://github.com/thomas-tran/sd-jwt-dotnet/actions/workflows/ci-cd.yml/badge.svg)](https://github.com/thomas-tran/sd-jwt-dotnet/actions)
[![License](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](https://opensource.org/licenses/Apache-2.0)

A comprehensive, production-ready .NET implementation of **OpenID Federation 1.0** protocol. This library enables trust chain validation, entity configuration management, and federation metadata support for SD-JWT ecosystems.

## ? Features

### ?? Complete OpenID Federation 1.0 Compliance
- **Entity Configuration Management**: Self-signed statements published at `/.well-known/openid-federation`
- **Trust Chain Resolution**: Recursive validation from leaf entities to trust anchors
- **Entity Statement Validation**: Superior entity endorsements with metadata policies
- **Trust Mark Support**: Compliance assertions and policy adherence validation

### ??? Production-Ready Architecture
- **Transport-Agnostic Design**: Pure data models and utilities, works with any HTTP framework
- **Comprehensive Security**: ES256/RS256 signing, JWT validation, timing attack protection
- **Modular Components**: Each model in its own file for better maintainability
- **Strong Typing**: Comprehensive validation and type safety throughout

### ?? Developer Experience
- **Fluent Builder APIs**: Easy-to-use builders for entity configurations
- **Recursive Resolution**: Automatic trust chain traversal with cycle detection
- **Flexible Validation**: Configurable validation options for different scenarios
- **Rich Diagnostics**: Comprehensive logging and error reporting

## ?? Installation

```bash
dotnet add package SdJwt.Net.OidFederation
```

## ?? Quick Start

### Entity Configuration - Publishing Your Federation Metadata

```csharp
using SdJwt.Net.OidFederation.Logic;
using SdJwt.Net.OidFederation.Models;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

// Create signing key
using var ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
var signingKey = new ECDsaSecurityKey(ecdsa);

// Define your entity's JWK Set (public keys)
var jwkSet = new 
{
    keys = new[]
    {
        new 
        {
            kty = "EC",
            crv = "P-256",
            x = "base64url-encoded-x",
            y = "base64url-encoded-y",
            use = "sig"
        }
    }
};

// Build entity configuration
var entityConfig = EntityConfigurationBuilder
    .Create("https://my-issuer.example.com")
    .WithSigningKey(signingKey)
    .WithJwkSet(jwkSet)
    .AddAuthorityHint("https://national-trust.gov")
    .AsCredentialIssuer(new 
    {
        credential_issuer = "https://my-issuer.example.com",
        credential_endpoint = "https://my-issuer.example.com/credential",
        credentials_supported = new[]
        {
            new 
            {
                format = "vc+sd-jwt",
                vct = "UniversityDegree",
                cryptographic_binding_methods_supported = new[] { "jwk" }
            }
        }
    })
    .WithValidity(24) // Valid for 24 hours
    .Build();

Console.WriteLine($"Entity Configuration JWT: {entityConfig}");
```

### Trust Chain Resolution - Validating Federation Entities

```csharp
using SdJwt.Net.OidFederation.Logic;
using Microsoft.IdentityModel.Tokens;

// Configure trust anchors (root entities you trust)
var trustAnchors = new Dictionary<string, SecurityKey>
{
    ["https://national-trust.gov"] = nationalTrustPublicKey,
    ["https://education-trust.gov"] = educationTrustPublicKey
};

// Create HTTP client for federation requests
using var httpClient = new HttpClient();

// Create trust chain resolver
var resolver = new TrustChainResolver(httpClient, trustAnchors);

// Validate an entity's trust chain
var targetEntity = "https://university.example.com";
var result = await resolver.ResolveAsync(targetEntity);

if (result.IsValid)
{
    Console.WriteLine($"? Trust chain valid!");
    Console.WriteLine($"Trust Anchor: {result.TrustAnchor}");
    Console.WriteLine($"Path Length: {result.PathLength}");
    Console.WriteLine($"Chain: {result.GetTrustChainSummary()}");
    
    // Check if entity supports specific protocols
    if (result.SupportsProtocol("openid_credential_issuer"))
    {
        Console.WriteLine("Entity is a valid credential issuer");
        var issuerMetadata = result.GetEffectiveMetadata("openid_credential_issuer");
        // Use issuer metadata...
    }
}
else
{
    Console.WriteLine($"? Trust chain validation failed: {result.ErrorMessage}");
}
```

## ??? Architecture

### Core Components

```
SdJwt.Net.OidFederation/
??? Models/                          # Data Models
?   ??? EntityConfiguration.cs       # Self-signed entity statements
?   ??? EntityStatement.cs          # Superior entity endorsements  
?   ??? EntityMetadata.cs           # Protocol-specific metadata
?   ??? MetadataPolicy.cs           # Policy constraints
?   ??? EntityConstraints.cs        # Operational limitations
?   ??? TrustMark.cs                # Compliance assertions
?   ??? OidFederationConstants.cs   # Protocol constants
??? Logic/                          # Core Logic
    ??? EntityConfigurationBuilder.cs # Entity config creation
    ??? TrustChainResolver.cs         # Trust chain validation  
    ??? TrustChainResult.cs          # Validation results
```

### Federation Entity Types

```
???????????????????
?  Trust Anchor   ? ? Root of trust (self-signed, no authority hints)
?                 ?
???????????????????
          ?
          ?
???????????????????
?  Intermediate   ? ? Can issue statements about subordinates
?   Authority     ?   Has authority hints pointing upward
???????????????????
          ?
          ?
???????????????????
?  Leaf Entity    ? ? End entities (Issuers, Verifiers, etc.)
?                 ?   Has authority hints, provides services
???????????????????
```

## ?? Complete Federation Examples

### 1. Setting Up an Intermediate Authority

```csharp
// Intermediate authority that can issue statements about universities
var authorityConfig = EntityConfigurationBuilder
    .Create("https://university-authority.gov")
    .WithSigningKey(authoritySigningKey)
    .WithJwkSet(authorityJwkSet)
    .AddAuthorityHint("https://national-trust.gov")
    .WithMetadata(new EntityMetadata
    {
        FederationEntity = new FederationEntityMetadata
        {
            FederationFetchEndpoint = "https://university-authority.gov/federation_fetch",
            FederationListEndpoint = "https://university-authority.gov/federation_list",
            Name = "University Accreditation Authority"
        }
    })
    .WithConstraints(new EntityConstraints
    {
        MaxPathLength = 3, // Limit trust chain depth
        NamingConstraints = new NamingConstraints
        {
            Permitted = new[] { "*.university.edu", "*.college.edu" }
        }
    })
    .Build();
```

### 2. Creating Entity Statements

```csharp
// Authority issuing a statement about a university
var entityStatement = new EntityStatement
{
    Issuer = "https://university-authority.gov",
    Subject = "https://state-university.edu", 
    IssuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
    ExpiresAt = DateTimeOffset.UtcNow.AddDays(30).ToUnixTimeSeconds(),
    
    // Constrain what the university can do
    MetadataPolicy = new MetadataPolicy
    {
        OpenIdCredentialIssuer = new MetadataPolicyRules()
    },
    
    // Grant trust marks
    TrustMarks = new[]
    {
        TrustMark.Create(
            CommonTrustMarks.EducationalInstitution, 
            "accredited",
            "https://university-authority.gov",
            "https://state-university.edu",
            validityHours: 8760 // 1 year
        )
    }
};

// Sign the entity statement
var statementJwt = SignEntityStatement(entityStatement, authoritySigningKey);
```

### 3. Advanced Trust Chain Resolution

```csharp
// Resolver with advanced options
var options = new TrustChainResolverOptions
{
    MaxPathLength = 5,
    HttpTimeoutSeconds = 30,
    ClockSkewMinutes = 5,
    EnableCaching = true,
    CacheDurationMinutes = 60
};

var resolver = new TrustChainResolver(httpClient, trustAnchors, options, logger);

// Resolve with specific requirements
var requirements = TrustChainRequirements.Create(
    maxPathLength: 3,
    CommonTrustMarks.EducationalInstitution, // Must have this trust mark
    CommonTrustMarks.SecurityCertified       // And this one
);

var result = await resolver.ResolveAsync("https://state-university.edu");

if (result.IsValid && result.SatisfiesRequirements(requirements))
{
    Console.WriteLine("? University meets all requirements!");
    
    // Get all trust marks
    var trustMarks = result.GetAllTrustMarks();
    foreach (var mark in trustMarks.Where(tm => tm.IsValid()))
    {
        Console.WriteLine($"Trust Mark: {mark.Id} (expires: {mark.ExpiresAt})");
    }
    
    // Check specific capabilities
    if (result.HasTrustMark(CommonTrustMarks.EducationalInstitution))
    {
        Console.WriteLine("University is accredited");
    }
}
```

### 4. Metadata Policy Application

```csharp
// Authority applies policies to constrain subordinate entities
var metadataPolicy = new MetadataPolicy
{
    OpenIdCredentialIssuer = new MetadataPolicyRules()
};

// Only allow specific credential types
metadataPolicy.OpenIdCredentialIssuer.SetFieldPolicy(
    "credentials_supported", 
    PolicyOperators.CreateSubsetOf(
        new { format = "vc+sd-jwt", vct = "UniversityDegree" },
        new { format = "vc+sd-jwt", vct = "TranscriptCredential" }
    )
);

// Require specific cryptographic binding methods
metadataPolicy.OpenIdCredentialIssuer.SetFieldPolicy(
    "cryptographic_binding_methods_supported",
    PolicyOperators.CreateValue(new[] { "jwk", "did" })
);

// Make credential endpoint essential
metadataPolicy.OpenIdCredentialIssuer.SetFieldPolicy(
    "credential_endpoint",
    PolicyOperators.CreateEssential(true)
);
```

## ?? ASP.NET Core Integration

### Publishing Entity Configuration

```csharp
// In your ASP.NET Core application
[HttpGet("/.well-known/openid-federation")]
public IActionResult GetEntityConfiguration()
{
    try
    {
        var entityConfig = EntityConfigurationBuilder
            .Create("https://my-issuer.example.com")
            .WithSigningKey(_signingKey)
            .WithJwkSet(_jwkSet)
            .AddAuthorityHint("https://trusted-authority.gov")
            .AsCredentialIssuer(_credentialIssuerMetadata)
            .Build();

        return Content(entityConfig, OidFederationConstants.ContentTypes.EntityConfiguration);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to generate entity configuration");
        return StatusCode(500);
    }
}
```

### Federation Fetch Endpoint

```csharp
[HttpGet("/federation_fetch")]
public async Task<IActionResult> FederationFetch([FromQuery] string sub)
{
    if (string.IsNullOrWhiteSpace(sub))
        return BadRequest("Missing 'sub' parameter");

    try
    {
        // Look up entity statement for the requested subject
        var statement = await _federationService.GetEntityStatementAsync(Request.Host.Value, sub);
        if (statement == null)
            return NotFound($"No statement found for entity: {sub}");

        var statementJwt = await _federationService.SignEntityStatementAsync(statement);
        
        return Content(statementJwt, OidFederationConstants.ContentTypes.EntityStatement);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to fetch entity statement for {Subject}", sub);
        return StatusCode(500);
    }
}
```

### Dependency Injection Setup

```csharp
// Program.cs or Startup.cs
services.AddHttpClient<TrustChainResolver>();
services.AddSingleton<Dictionary<string, SecurityKey>>(sp =>
{
    // Configure your trust anchors
    return new Dictionary<string, SecurityKey>
    {
        ["https://national-trust.gov"] = LoadTrustAnchorKey("national-trust"),
        ["https://education-trust.gov"] = LoadTrustAnchorKey("education-trust")
    };
});

services.AddSingleton<TrustChainResolverOptions>(new TrustChainResolverOptions
{
    MaxPathLength = 5,
    HttpTimeoutSeconds = 30,
    EnableCaching = true
});

services.AddScoped<TrustChainResolver>();
```

## ?? Testing

Comprehensive test suite covering all federation scenarios:

```bash
# Run all tests
dotnet test

# Run specific test category
dotnet test --filter Category=Federation

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Example Test Patterns

```csharp
[Fact]
public async Task TrustChainResolver_WithValidChain_ShouldReturnSuccess()
{
    // Arrange
    var trustAnchors = new Dictionary<string, SecurityKey>
    {
        ["https://trust-anchor.gov"] = _trustAnchorKey
    };
    
    var mockHttpClient = CreateMockHttpClient();
    var resolver = new TrustChainResolver(mockHttpClient, trustAnchors);

    // Act
    var result = await resolver.ResolveAsync("https://leaf-entity.example.com");

    // Assert
    result.IsValid.Should().BeTrue();
    result.TrustAnchor.Should().Be("https://trust-anchor.gov");
    result.PathLength.Should().BeLessOrEqualTo(3);
}

[Fact]
public void EntityConfigurationBuilder_WithValidSetup_ShouldCreateValidJwt()
{
    // Arrange & Act
    var jwt = EntityConfigurationBuilder
        .Create("https://issuer.example.com")
        .WithSigningKey(_signingKey)
        .WithJwkSet(_jwkSet)
        .Build();

    // Assert
    var tokenHandler = new JwtSecurityTokenHandler();
    tokenHandler.CanReadToken(jwt).Should().BeTrue();
    
    var token = tokenHandler.ReadJwtToken(jwt);
    token.Header.Typ.Should().Be(OidFederationConstants.JwtHeaders.EntityConfigurationType);
}
```

## ?? API Reference

### Core Classes

#### EntityConfigurationBuilder
- **`Create(entityUrl)`** - Creates a new builder instance
- **`WithSigningKey(key, algorithm)`** - Sets the signing key and algorithm
- **`WithJwkSet(jwkSet)`** - Sets the entity's public key set
- **`WithMetadata(metadata)`** - Sets protocol-specific metadata
- **`AddAuthorityHint(url)`** - Adds an authority hint
- **`WithConstraints(constraints)`** - Sets entity constraints
- **`AddTrustMark(trustMark)`** - Adds a trust mark
- **`Build()`** - Creates and signs the entity configuration JWT

#### TrustChainResolver
- **`ResolveAsync(entityUrl)`** - Resolves and validates a trust chain
- **`TrustChainResolver(httpClient, trustAnchors, options)`** - Constructor

#### TrustChainResult
- **`IsValid`** - Whether the trust chain is valid
- **`TrustAnchor`** - The trust anchor that anchors this chain
- **`EntityConfiguration`** - The validated entity configuration
- **`TrustChain`** - The chain of entity statements
- **`GetAllTrustMarks()`** - Gets all applicable trust marks
- **`HasTrustMark(trustMarkId)`** - Checks for specific trust marks
- **`SupportsProtocol(protocol)`** - Checks protocol support

### Extension Methods

#### EntityConfigurationBuilder Extensions
- **`AsCredentialIssuer(metadata)`** - Configures as OID4VCI issuer
- **`AsVerifier(metadata)`** - Configures as OID4VP verifier  
- **`AsOpenIdProvider(metadata)`** - Configures as OIDC provider

#### TrustChainResult Extensions
- **`GetTrustChainSummary()`** - Gets human-readable chain summary
- **`SatisfiesRequirements(requirements)`** - Validates against requirements

## ?? Security Considerations

### Signature Validation

```csharp
// Always validate signatures with appropriate key resolution
var validationParameters = new TokenValidationParameters
{
    ValidateIssuer = true,
    ValidateAudience = false, // Federation JWTs don't have audiences
    ValidateLifetime = true,
    ClockSkew = TimeSpan.FromMinutes(5),
    IssuerSigningKey = await GetSigningKeyAsync(issuer)
};
```

### Trust Anchor Management

```csharp
// Securely manage trust anchors
public class TrustAnchorManager
{
    private readonly Dictionary<string, SecurityKey> _trustAnchors;
    
    public void AddTrustAnchor(string entityUrl, SecurityKey publicKey)
    {
        // Validate entity URL format
        if (!Uri.TryCreate(entityUrl, UriKind.Absolute, out var uri) || uri.Scheme != "https")
            throw new ArgumentException("Trust anchor URL must be HTTPS");
        
        // Store securely
        _trustAnchors[entityUrl] = publicKey;
    }
    
    public bool IsTrusted(string entityUrl) => _trustAnchors.ContainsKey(entityUrl);
}
```

### Cache Security

```csharp
// Implement secure caching with appropriate TTLs
var cacheOptions = new MemoryCacheEntryOptions
{
    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30),
    SlidingExpiration = TimeSpan.FromMinutes(15),
    Priority = CacheItemPriority.Normal
};

// Cache entity configurations securely
_cache.Set($"entity-config:{entityUrl}", entityConfig, cacheOptions);
```

## ?? Contributing

We welcome contributions! Please see our [Contributing Guide](../CONTRIBUTING.md).

### Development Setup
```bash
git clone https://github.com/thomas-tran/sd-jwt-dotnet.git
cd sd-jwt-dotnet
dotnet restore
dotnet build
dotnet test
```

### Code Style
- Follow existing patterns and naming conventions
- Add comprehensive XML documentation
- Include unit tests for new features  
- Update README for API changes

## ?? License

This project is licensed under the Apache License 2.0 - see the [LICENSE](../LICENSE) file for details.

## ?? Related Specifications

- **[OpenID Federation 1.0](https://openid.net/specs/openid-federation-1_0.html)** - Core federation specification
- **[RFC 9901](https://tools.ietf.org/html/rfc9901)** - SD-JWT specification
- **[OID4VCI 1.0](https://openid.net/specs/openid-4-verifiable-credential-issuance-1_0.html)** - Credential issuance protocol
- **[OID4VP 1.0](https://openid.net/specs/openid-4-verifiable-presentations-1_0.html)** - Presentation verification protocol
- **[JWT](https://tools.ietf.org/html/rfc7519)** - JSON Web Token specification

---

**Ready to implement OpenID Federation?** Start with: `dotnet add package SdJwt.Net.OidFederation`