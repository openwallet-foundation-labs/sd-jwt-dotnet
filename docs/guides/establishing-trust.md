# How to Establish Trust with OpenID Federation

This guide demonstrates how to configure and use the `SdJwt.Net.OidFederation` package to implement scalable, automated trust infrastructure.

In a small deployment, a Verifier can manually hardcode a list of trusted Issuers (e.g., "I only trust credentials signed by `https://university.example.edu` using Key A"). But in a Sovereign or global ecosystem (like the European Digital Identity Wallet), manual lists are impossible to maintain.

**OpenID Federation 1.0** solves this by creating "Trust Chains" mimicking the DNS or X.509 certificate systems, where intermediate authorities vouch for leaf entities.

## Prerequisites

Ensure your project references the necessary NuGet packages:

```bash
dotnet add package SdJwt.Net.OidFederation
```

## 1. Configure Federation (The Verifier)

Verifiers need to know which Root Authorities (Trust Anchors) they inherently trust. When a Verifier receives a credential from an unknown Issuer (e.g., a small rural bank), the Verifier will use Federation to see if that bank is endorsed by a trusted Authority (e.g., the National Central Bank).

### Setup the Federation Client

In your Verifier's `Program.cs`:

```csharp
using SdJwt.Net.OidFederation;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenIdFederation(options =>
{
    // Your own Entity ID
    options.EntityId = "https://verifier.example.com";
    
    // The Root Authorities you trust unconditionally
    options.TrustAnchors = new[]
    {
        "https://financial-federation.gov", // National Financial Authority
        "https://trust.eudi.europa.eu"      // EU Trust Anchor
    };
    
    // Automatically publish your own metadata
    options.FederationEndpoint = "/.well-known/openid_federation";
});

var app = builder.Build();
```

### Resolving a Trust Chain

When a Wallet presents a credential, it provides the Issuer's unique ID (usually their web address). Your Verifier must now dynamically resolve a Trust Chain to that Issuer to fetch their authentic public keys.

```csharp
app.MapPost("/verify-login", async (
    PresentationResponse response, 
    ISdJwtVerifierService verifier,
    IFederationService federation) =>
{
    // The presentation claims it was issued by this bank
    string issuerId = "https://small-rural-bank.com";

    // 1. Resolve the Trust Chain!
    // The FederationService automatically walks the tree backwards:
    // small-rural-bank -> Regional Authority -> National Financial Authority
    var trustChain = await federation.ResolveTrustChainAsync(issuerId);

    if (!trustChain.IsValid)
    {
        // The bank is not trusted by any of our configured Trust Anchors!
        return Results.Unauthorized($"Untrusted Issuer: {trustChain.ErrorMessage}");
    }

    // 2. Crucially, the Trust Chain provides the *verified* metadata for the Issuer,
    // including their authentic Public Keys (JWKS).
    var verifiedMetadata = trustChain.LeafEntity.Metadata;
    var authenticPublicKeys = verifiedMetadata.Jwks;

    // 3. Now verify the SD-JWT signature using the trusted keys
    var sdJwtResult = await verifier.VerifyPresentationAsync(response, authenticPublicKeys);
    
    if (sdJwtResult.IsValid)
    {
        return Results.Ok("User authenticated successfully.");
    }
});
```

*Note: If you are using `SdJwt.Net.HAIP`, this trust chain resolution happens entirely automatically behind the scenes!*

## 2. Participating in a Federation (The Issuer)

As an Issuer, you want Verifiers to trust you. To achieve this, you publish an **Entity Statement** (a signed JWT) at your `/.well-known/openid_federation` endpoint. This statement declares who you are, what your public keys are, and points to the Authorities that vouch for you.

### Setup the Federation Endpoint

In your Issuer's `Program.cs`:

```csharp
builder.Services.AddOpenIdFederation(options =>
{
    options.EntityId = "https://small-rural-bank.com";
    
    // The private key used to sign your own Entity Statement
    options.SigningKey = myFederationKey;
    
    // Declare the Authorities you intend to be subordinate to
    options.AuthorityHints = new[]
    {
        "https://regional-financial-authority.gov"
    };
    
    options.FederationEndpoint = "/.well-known/openid_federation";
});
```

### Automatic Endpoint Publishing

The `SdJwt.Net.OidFederation` package automatically maps the required `.well-known` endpoints.

```csharp
var app = builder.Build();

// Automatically exposes GET /.well-known/openid_federation
app.MapOpenIdFederationEndpoints(); 
```

When a Verifier's server hits `GET https://small-rural-bank.com/.well-known/openid_federation`, the package will dynamically generate, sign, and return your Entity Statement. The Verifier will then follow the `AuthorityHints` URL to ask the `regional-financial-authority.gov` if they have a signed record vouching for your bank, continuing up the chain until it hits a Trust Anchor.
