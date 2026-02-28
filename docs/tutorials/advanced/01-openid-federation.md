# Tutorial: OpenID Federation

Establish trust between issuers, holders, and verifiers using OpenID Federation.

**Time:** 25 minutes  
**Level:** Advanced  
**Sample:** `samples/SdJwt.Net.Samples/03-Advanced/01-OpenIdFederation.cs`

## What You Will Learn

- Trust chain concept and structure
- Entity statements and metadata
- Resolving and validating trust

## The Trust Problem

How does a verifier know to trust an issuer?

- Self-asserted metadata can be spoofed
- Manual trust lists don't scale
- Certificate authorities are complex

**Solution:** OpenID Federation creates hierarchical trust anchors.

## Trust Hierarchy

```
                 ┌─────────────────┐
                 │  Trust Anchor   │
                 │  (Root of Trust)│
                 └────────┬────────┘
                          │
           ┌──────────────┼──────────────┐
           │              │              │
     ┌─────┴─────┐  ┌─────┴─────┐  ┌─────┴─────┐
     │ Authority │  │ Authority │  │ Authority │
     │  (Govt)   │  │(Industry) │  │ (Region)  │
     └─────┬─────┘  └─────┬─────┘  └─────┬─────┘
           │              │              │
      ┌────┴────┐    ┌────┴────┐    ┌────┴────┐
      │ Issuer  │    │ Issuer  │    │ Verifier│
      └─────────┘    └─────────┘    └─────────┘
```

## Step 1: Trust Anchor Configuration

The trust anchor publishes its entity configuration:

```csharp
using SdJwt.Net.OidFederation.Models;

var trustAnchorConfig = new EntityConfiguration
{
    Issuer = "https://federation.example.gov",
    Subject = "https://federation.example.gov",
    IssuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
    ExpiresAt = DateTimeOffset.UtcNow.AddYears(1).ToUnixTimeSeconds(),
    Jwks = new JsonWebKeySet { Keys = { trustAnchorPublicKey } },
    Metadata = new EntityMetadata
    {
        FederationEntity = new FederationEntityMetadata
        {
            FederationFetchEndpoint = "https://federation.example.gov/fetch",
            FederationListEndpoint = "https://federation.example.gov/list"
        }
    }
};
```

## Step 2: Subordinate Entity Statement

Trust anchor issues statement about subordinate:

```csharp
var subordinateStatement = new EntityStatement
{
    Issuer = "https://federation.example.gov",           // Trust Anchor
    Subject = "https://university.example.edu",          // Subordinate
    IssuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
    ExpiresAt = DateTimeOffset.UtcNow.AddMonths(6).ToUnixTimeSeconds(),
    Jwks = new JsonWebKeySet { Keys = { universityPublicKey } },
    MetadataPolicy = new MetadataPolicy
    {
        // Constrain what the university can claim
        CredentialIssuer = new PolicyConstraints
        {
            AllowedCredentialTypes = new[] { "UniversityDegree", "StudentID" }
        }
    }
};

// Sign with trust anchor key
var signedStatement = SignEntityStatement(subordinateStatement, trustAnchorKey);
```

## Step 3: Entity Configuration (Leaf)

The issuer publishes its own configuration:

```csharp
var issuerConfig = new EntityConfiguration
{
    Issuer = "https://university.example.edu",
    Subject = "https://university.example.edu",
    IssuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
    ExpiresAt = DateTimeOffset.UtcNow.AddDays(7).ToUnixTimeSeconds(),
    Jwks = new JsonWebKeySet { Keys = { universityPublicKey } },
    AuthorityHints = new[] { "https://federation.example.gov" },
    Metadata = new EntityMetadata
    {
        CredentialIssuer = new CredentialIssuerMetadata
        {
            CredentialIssuer = "https://university.example.edu",
            CredentialEndpoint = "https://university.example.edu/credential"
        }
    }
};
```

## Step 4: Build Trust Chain

```csharp
using SdJwt.Net.OidFederation.Services;

var resolver = new TrustChainResolver(httpClient);

// Resolve trust chain from issuer to trust anchor
var trustChain = await resolver.ResolveAsync(
    leafEntity: "https://university.example.edu",
    trustAnchor: "https://federation.example.gov"
);

// Trust chain contains:
// [0] Leaf entity configuration (self-signed)
// [1] Subordinate statement (signed by intermediate or anchor)
// [2] ... more intermediates if present ...
// [n] Trust anchor configuration (self-signed)
```

## Step 5: Validate Trust Chain

```csharp
var validator = new TrustChainValidator();

var result = validator.Validate(trustChain, trustAnchorPublicKey);

if (result.IsValid)
{
    Console.WriteLine("Trust chain is valid");
    Console.WriteLine($"Issuer is trusted for: {string.Join(", ", result.AllowedCredentialTypes)}");
}
else
{
    Console.WriteLine($"Trust validation failed: {result.Error}");
}
```

## Step 6: Integrate with Verification

```csharp
public async Task<SecurityKey> ResolveIssuerKey(string issuer)
{
    // 1. Resolve trust chain
    var trustChain = await resolver.ResolveAsync(issuer, knownTrustAnchor);
    
    // 2. Validate trust chain
    var validationResult = validator.Validate(trustChain, trustAnchorKey);
    if (!validationResult.IsValid)
    {
        throw new SecurityException($"Issuer not trusted: {validationResult.Error}");
    }
    
    // 3. Return issuer's key from validated chain
    return trustChain.LeafConfiguration.Jwks.Keys.First();
}

// Use in verification
var verifier = new SdVerifier(ResolveIssuerKey);
var result = await verifier.VerifyAsync(presentation, params);
```

## Metadata Policies

Intermediates can constrain subordinates:

```csharp
var policy = new MetadataPolicy
{
    CredentialIssuer = new PolicyConstraints
    {
        // Restrict allowed credential types
        AllowedCredentialTypes = new[] { "DriverLicense" },
        
        // Require certain metadata
        RequiredMetadata = new[] { "logo_uri", "policy_uri" },
        
        // Constrain values
        AllowedAlgorithms = new[] { "ES256", "ES384" }
    }
};
```

## Multiple Trust Anchors

Support multiple federations:

```csharp
var trustedAnchors = new[]
{
    "https://federation.example.gov",
    "https://industry-trust.example.org"
};

foreach (var anchor in trustedAnchors)
{
    try
    {
        var chain = await resolver.ResolveAsync(issuer, anchor);
        if (validator.Validate(chain, GetAnchorKey(anchor)).IsValid)
        {
            return chain;  // Found valid trust path
        }
    }
    catch { /* Try next anchor */ }
}

throw new Exception("No valid trust path found");
```

## Run the Sample

```bash
cd samples/SdJwt.Net.Samples
dotnet run -- 3.1
```

## Next Steps

- [HAIP Compliance](02-haip-compliance.md) - Security levels
- [Multi-Credential Flow](03-multi-credential-flow.md) - Complex presentations

## Key Takeaways

1. OpenID Federation establishes hierarchical trust
2. Trust chains link entities to trust anchors
3. Metadata policies constrain subordinate capabilities
4. Verifiers resolve trust before accepting credentials
