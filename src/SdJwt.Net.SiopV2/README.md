# SdJwt.Net.SiopV2 - Self-Issued OpenID Provider v2

SIOPv2 helpers for subject-signed ID Tokens and OpenID4VP combined flows.

## Features

-   Subject-signed ID Token issuance
-   SIOPv2 ID Token validation for JWK thumbprint subject syntax (draft-13 Section 6.1)
-   DID subject syntax validation via `IDidKeyResolver` (draft-13 Section 6.2)
-   Concrete resolvers for `did:key` (Ed25519, P-256, P-384, P-521) and `did:jwk`
-   RFC 7638 JWK thumbprint subject calculation
-   Static `siopv2:` and `openid:` provider metadata helpers

## JWK Thumbprint Subject (default)

```csharp
using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.SiopV2;
using System.Security.Cryptography;

using var ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
var signingKey = new ECDsaSecurityKey(ecdsa);
var publicJwk = JsonWebKeyConverter.ConvertFromSecurityKey(signingKey);

var issuer = new SelfIssuedIdTokenIssuer(signingKey, SecurityAlgorithms.EcdsaSha256, publicJwk);
var idToken = issuer.Issue(new SelfIssuedIdTokenOptions
{
    Audience = "https://rp.example.com",
    Nonce = "nonce-123"
});

var validator = new SelfIssuedIdTokenValidator();
var result = await validator.ValidateAsync(idToken, new SelfIssuedIdTokenValidationParameters
{
    ExpectedAudience = "https://rp.example.com",
    ExpectedNonce = "nonce-123"
});
```

## DID Subject Syntax

Use `IDidKeyResolver` to validate tokens where `sub` is a Decentralized Identifier.
Two concrete resolvers are provided out of the box.

### did:key

Supports Ed25519 (multicodec 0xED01), P-256 (0x1200), P-384 (0x1201), and P-521 (0x1202).
The multibase prefix `z` (base58btc) is the only accepted encoding per spec.

```csharp
using SdJwt.Net.SiopV2.Did;

var resolver = new DidKeyResolver();

var result = await validator.ValidateAsync(idToken, new SelfIssuedIdTokenValidationParameters
{
    ExpectedAudience = "https://rp.example.com",
    ExpectedNonce = "nonce-123",
    DidKeyResolver = resolver
});
// result.Subject == "did:key:z6Mk..."
```

### did:jwk

The DID encodes a JSON Web Key as a base64url string directly in the DID identifier.

```csharp
using SdJwt.Net.SiopV2.Did;

var resolver = new DidJwkResolver();

var result = await validator.ValidateAsync(idToken, new SelfIssuedIdTokenValidationParameters
{
    ExpectedAudience = "https://rp.example.com",
    ExpectedNonce = "nonce-123",
    DidKeyResolver = resolver
});
// result.Subject == "did:jwk:eyJrdHkiOiJFQyIsImNydiI6IlAtMjU2IiwieCI6Ii4uLiIsInkiOiIuLi4ifQ"
```

### Custom DID method

Implement `IDidKeyResolver` to support any DID method:

```csharp
public class MyDidResolver : IDidKeyResolver
{
    public async Task<SecurityKey> ResolveKeyAsync(
        string did, string? keyId, CancellationToken cancellationToken = default)
    {
        // resolve did document, return verification key
    }
}
```
