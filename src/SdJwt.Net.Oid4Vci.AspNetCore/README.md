# SdJwt.Net.Oid4Vci.AspNetCore

**ASP.NET Core server-side endpoints for OpenID for Verifiable Credential Issuance (OID4VCI) 1.0**

## Overview

This package provides plug-and-play ASP.NET Core Minimal API endpoints implementing the [OID4VCI 1.0](https://openid.net/specs/openid-4-verifiable-credential-issuance-1_0.html) protocol server side:

| Endpoint                                    | Section | Description                        |
| ------------------------------------------- | ------- | ---------------------------------- |
| `GET /.well-known/openid-credential-issuer` | 11.2    | Issuer metadata document           |
| `POST /token`                               | 6       | Pre-authorized code token exchange |
| `POST /credential`                          | 7       | Credential issuance with JWT proof |
| `POST /deferred-credential`                 | 9       | Deferred credential retrieval      |

## Quick Start

### 1. Register Services

```csharp
builder.Services.AddOid4VciIssuer(options =>
{
    options.IssuerUrl = "https://issuer.example.com";
    options.AccessTokenLifetimeSeconds = 300;
    options.CNonceLifetimeSeconds = 300;
    options.CredentialConfigurationsSupported = new Dictionary<string, JsonElement>
    {
        ["IdentityCredential"] = JsonSerializer.SerializeToElement(new
        {
            format = "dc+sd-jwt",
            vct = "https://credentials.example.com/identity"
        })
    };
})
.UseInMemoryServices()          // For development
.UseCredentialIssuer<MyIssuer>(); // Your SD-JWT signing implementation
```

### 2. Map Endpoints

```csharp
app.MapOid4VciMetadata();
app.MapOid4VciToken();
app.MapOid4VciCredential();
app.MapOid4VciDeferredCredential();
```

### 3. Register Pre-Authorized Codes

```csharp
var tokenService = app.Services.GetRequiredService<InMemoryAccessTokenService>();
tokenService.RegisterPreAuthorizedCode("SplxlOBeZQQYbYS6WxSbIA", txCode: null, ["IdentityCredential"]);
```

## Architecture

```
IServiceCollection.AddOid4VciIssuer()
    │
    ├── IOid4VciBuilder.UseInMemoryServices()
    │       ├── InMemoryAccessTokenService  (IAccessTokenService)
    │       └── InMemoryDeferredCredentialStore  (IDeferredCredentialStore)
    │
    └── IOid4VciBuilder.UseCredentialIssuer<T>()
            └── ICredentialIssuer  (your SD-JWT signing implementation)
```

## Implementing ICredentialIssuer

```csharp
public class MyCredentialIssuer : ICredentialIssuer
{
    public async Task<CredentialIssuanceResult> IssueAsync(
        CredentialRequest request, string accessToken, CancellationToken ct)
    {
        // 1. Validate credential configuration ID and format
        // 2. Build SD-JWT claims for the holder
        // 3. Sign and return
        var sdJwt = "..."; // your SD-JWT string
        return new CredentialIssuanceResult(CredentialResponse.Success(sdJwt));
    }
}
```

## Production Considerations

-   Replace `InMemoryAccessTokenService` with a JWT-based or Redis-backed implementation.
-   Replace `InMemoryDeferredCredentialStore` with a distributed store (Redis, Azure Table, etc.).
-   Enable HTTPS and configure proper CORS policies.
-   Implement consent and holder binding in your `ICredentialIssuer`.
