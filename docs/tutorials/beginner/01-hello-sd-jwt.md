# Tutorial: Hello SD-JWT

Create your first Selective Disclosure JWT in 5 minutes.

**Time:** 5 minutes  
**Level:** Beginner  
**Sample:** `samples/SdJwt.Net.Samples/01-Beginner/01-HelloSdJwt.cs`

## What You Will Learn

- How to create an SD-JWT issuer
- How to sign claims with selective disclosure
- How to parse and inspect an SD-JWT structure

## Prerequisites

- .NET 9.0 SDK installed
- Basic understanding of JWTs

## Step 1: Create Keys

Every SD-JWT system needs cryptographic keys. The issuer signs with a private key, and verifiers check with the public key.

```csharp
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;

// Create an ECDSA key pair (P-256 curve)
using var ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
var issuerKey = new ECDsaSecurityKey(ecdsa) { KeyId = "my-key-1" };
```

## Step 2: Create the Issuer

The `SdIssuer` class handles SD-JWT creation:

```csharp
using SdJwt.Net.Issuer;

var issuer = new SdIssuer(issuerKey, SecurityAlgorithms.EcdsaSha256);
```

## Step 3: Define Claims

Create a JWT payload with the claims you want to include:

```csharp
using System.IdentityModel.Tokens.Jwt;

var claims = new JwtPayload
{
    ["iss"] = "https://issuer.example.com",
    ["sub"] = "user123",
    ["iat"] = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
    ["given_name"] = "Alice",
    ["family_name"] = "Smith",
    ["email"] = "alice@example.com"
};
```

## Step 4: Configure Selective Disclosure

Specify which claims can be selectively disclosed:

```csharp
using SdJwt.Net.Models;

var options = new SdIssuanceOptions
{
    DisclosureStructure = new
    {
        given_name = true,   // Can be hidden/revealed
        family_name = true,  // Can be hidden/revealed
        email = true         // Can be hidden/revealed
    }
};
```

## Step 5: Issue the SD-JWT

```csharp
var result = issuer.Issue(claims, options);

Console.WriteLine($"SD-JWT created with {result.Disclosures.Count} disclosures");
Console.WriteLine($"Issuance: {result.Issuance}");
```

## Understanding the Output

The `result.Issuance` string contains:

- The signed JWT (with digests instead of actual claim values)
- Disclosure strings (Base64URL encoded claim data)
- Separated by `~` characters

Format: `<JWT>~<disclosure1>~<disclosure2>~...~`

## Complete Example

```csharp
using System.Security.Cryptography;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.Issuer;
using SdJwt.Net.Models;

// Setup
using var ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
var issuerKey = new ECDsaSecurityKey(ecdsa) { KeyId = "key-1" };
var issuer = new SdIssuer(issuerKey, SecurityAlgorithms.EcdsaSha256);

// Claims
var claims = new JwtPayload
{
    ["iss"] = "https://issuer.example.com",
    ["sub"] = "user123",
    ["given_name"] = "Alice",
    ["family_name"] = "Smith"
};

// Options
var options = new SdIssuanceOptions
{
    DisclosureStructure = new { given_name = true, family_name = true }
};

// Issue
var result = issuer.Issue(claims, options);
Console.WriteLine($"Created SD-JWT with {result.Disclosures.Count} disclosures");
```

## Run the Sample

```bash
cd samples/SdJwt.Net.Samples
dotnet run -- 1.1
```

## Next Steps

- [Selective Disclosure](02-selective-disclosure.md) - Learn to hide and reveal specific claims
- [Holder Binding](03-holder-binding.md) - Add cryptographic proof of ownership

## Key Concepts

| Term       | Description                                  |
| ---------- | -------------------------------------------- |
| SD-JWT     | Selective Disclosure JSON Web Token          |
| Disclosure | Base64URL-encoded claim that can be revealed |
| Digest     | Hash of a disclosure stored in the JWT       |
| Issuance   | Complete SD-JWT string with all disclosures  |
