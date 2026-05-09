# SdJwt.Net.SiopV2 - Self-Issued OpenID Provider v2

SIOPv2 helpers for subject-signed ID Tokens and OpenID4VP combined flows.

## Features

-   Subject-signed ID Token issuance
-   SIOPv2 ID Token validation for JWK thumbprint subject syntax
-   RFC 7638 JWK thumbprint subject calculation
-   Static `siopv2:` and `openid:` provider metadata helpers
-   DID subject syntax extension point through `IDidKeyResolver`

## Example

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
