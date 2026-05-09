# How to establish issuer trust with OpenID Federation

| Field                | Value                                                           |
| -------------------- | --------------------------------------------------------------- |
| **Package maturity** | Stable (SdJwt.Net.OidFederation)                                |
| **Code status**      | Mixed -- runnable package APIs with illustrative service wiring |
| **Related concept**  | [OpenID Federation](../concepts/openid-federation.md)           |

|                      |                                                                                                                                                                                                                                            |
| -------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| **Audience**         | Architects designing multi-party trust infrastructure, and developers configuring federation endpoints.                                                                                                                                    |
| **Purpose**          | Show how to set up OpenID Federation 1.0 trust chains - configuring trust anchors on verifiers and publishing entity statements on issuers - using `SdJwt.Net.OidFederation`.                                                              |
| **Scope**            | Trust anchor configuration, trust chain resolution, entity statement publication, and `.well-known` endpoint mapping. Out of scope: higher-level verifier configurations that may invoke federation resolution when explicitly configured. |
| **Success criteria** | Reader can configure a verifier with trust anchors, resolve a multi-hop trust chain to an unknown issuer, and publish an issuer entity statement that integrates into a federation tree.                                                   |

## What your application still owns

This guide does not provide: trust anchor governance and key rotation, metadata policy authoring, entity statement signing key custody, cache eviction strategy, federation tree monitoring, or production `.well-known` endpoint hosting.

> Snippets in this guide are architecture-level pseudocode. For concrete package usage, see `samples/SdJwt.Net.Samples/Standards/OpenId/OpenIdFederationExample.cs`.

---

## Key decisions

| Decision                                     | Options                    | Guidance                         |
| -------------------------------------------- | -------------------------- | -------------------------------- |
| Trust anchor selection?                      | Single or multiple anchors | Multiple for resilience          |
| Metadata policy enforcement?                 | Strict or permissive       | Strict for regulated ecosystems  |
| Cache TTL for entity configurations?         | Minutes to hours           | 5-15 minutes typical             |
| Fallback on resolution failure?              | Reject or use cached       | Reject if cache stale beyond TTL |
| Federation key separate from credential key? | Yes/No                     | Always yes for production        |

---

## Prerequisites

Ensure your project references the necessary NuGet packages:

```bash
dotnet add package SdJwt.Net.OidFederation
```

## 1. Configure federation (verifier side)

Verifiers need to know which Root Authorities (Trust Anchors) they inherently trust. When a verifier receives a credential from an unknown issuer (e.g., a small rural bank), it uses Federation to check whether that bank is endorsed by a trusted authority (e.g., the National Central Bank).

### Set up the federation client

In your verifier's `Program.cs`:

```csharp
using SdJwt.Net.OidFederation.Logic;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Register your configured trust anchors.
var trustAnchors = new Dictionary<string, SecurityKey>
{
    ["https://trust-anchor.example.com"] = trustAnchorPublicKey
};

// Register your app services and use federation primitives as needed.
var resolver = new TrustChainResolver(httpClient, trustAnchors);

var app = builder.Build();
```

### Resolving a trust chain

When a wallet presents a credential, it provides the issuer's unique ID (usually their web address). Your verifier must then dynamically resolve a Trust Chain to that issuer to fetch their authentic public keys.

```csharp
app.MapPost("/verify-login", async (
    PresentationResponse response,
    /* your verifier service */ verifier,
    TrustChainResolver federation) =>
{
    // The presentation claims it was issued by this bank
    string issuerId = "https://small-rural-bank.com";

    // 1. Resolve the Trust Chain!
    // The resolver automatically walks the tree backwards:
    // small-rural-bank -> Regional Authority -> National Financial Authority
    var trustChain = await federation.ResolveAsync(issuerId);

    if (!trustChain.IsValid)
    {
        // The bank is not trusted by any of our configured Trust Anchors!
        return Results.Unauthorized($"Untrusted Issuer: {trustChain.ErrorMessage}");
    }

    // 2. The Trust Chain provides the *verified* metadata for the Issuer,
    // including their authentic Public Keys (JWKS).
    var verifiedMetadata = trustChain.ValidatedMetadata;
    var authenticPublicKeys = verifiedMetadata?.GetProtocolMetadata("openid_credential_issuer");

    // 3. Now verify the SD-JWT signature using the trusted keys
    var sdJwtResult = await verifier.VerifyPresentationAsync(response, authenticPublicKeys);

    if (sdJwtResult.IsValid)
    {
        return Results.Ok("User authenticated successfully.");
    }
});
```

_Note: Higher-level verifier configurations (such as HAIP profiles) may invoke federation resolution when explicitly configured._

## 2. Participating in a federation (issuer side)

As an issuer, you publish an **Entity Statement** (a signed JWT) at your `/.well-known/openid_federation` endpoint. This statement declares who you are, what your public keys are, and points to the authorities that vouch for you.

### Set up the federation endpoint

In your issuer's `Program.cs`:

```csharp
// Issuer-side federation metadata and statement publishing are application-specific.
// Use package builders/services to produce entity configuration and statements.
```

### Federation endpoint publishing

The `SdJwt.Net.OidFederation` package provides builders for entity configurations and statements. Your application is responsible for hosting the `.well-known/openid_federation` endpoint, signing the entity statement with your federation key, and returning it to callers.

```csharp
var app = builder.Build();

// Map your federation endpoints and return signed entity statements.
// Your application owns the endpoint routing and entity statement signing key.
```

When a verifier's server hits `GET https://small-rural-bank.com/.well-known/openid_federation`, the package dynamically generates, signs, and returns your Entity Statement. The verifier then follows the `AuthorityHints` URL to ask `regional-financial-authority.gov` whether they have a signed record vouching for your bank, continuing up the chain until it reaches a Trust Anchor.
