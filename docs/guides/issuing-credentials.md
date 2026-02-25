# How to Issue Verifiable Credentials

This guide demonstrates how to configure an Issuer application to generate and issue W3C-compliant Verifiable Credentials (VCs) backed by SD-JWT.

## Prerequisites

Ensure your project references the necessary NuGet packages:

```bash
dotnet add package SdJwt.Net.Oid4Vci
dotnet add package SdJwt.Net.Vc
dotnet add package SdJwt.Net.HAIP
```

## 1. Configure the Issuer Service

First, register the Issuer service in your Dependency Injection container (`Program.cs` or `Startup.cs`). This requires setting up your cryptographic signing keys and defining the credential types your service supports.

```csharp
using SdJwt.Net.Oid4Vci;
using SdJwt.Net.HAIP;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

var builder = WebApplication.CreateBuilder(args);

// Generate or load your private signing key (e.g., from Azure Key Vault)
var ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
var issuerSigningKey = new ECDsaSecurityKey(ecdsa);

builder.Services.AddSdJwtIssuer(options =>
{
    options.IssuerUrl = "https://issuer.example.com";
    options.SigningKey = issuerSigningKey;
    
    // Define the JSON schema types this issuer supports
    options.SupportedCredentialTypes = new[]
    {
        "UniversityDegreeCredential",
        "EmployeeIdCredential"
    };
    
    // We are issuing the 'vc+sd-jwt' format defined in OpenID4VCI
    options.SupportedFormats = new[] { "vc+sd-jwt" };
    
    // Apply HAIP Level 2 Security Policy (e.g., forces DPoP, strictly ES384+)
    options.UseHaipProfile(HaipLevel.Level2_VeryHigh);
});

var app = builder.Build();
```

## 2. Issue a Credential

Once OID4VCI handles the OAuth 2.0 component (e.g., the wallet exchanging an authorization code for an access token), your business logic needs to build the actual credential payload.

Use the `VerifiableCredentialBuilder` to construct the W3C payload, explicitly deciding which claims are plain-text and which are `SelectiveDisclosure` (hidden behind a salt/hash).

```csharp
using SdJwt.Net.Vc;

app.MapPost("/issue-degree", async (
    CredentialRequest request, 
    ISdJwtIssuerService issuer) =>
{
    // 1. Build the W3C Verifiable Credential Payload
    var vcBuilder = new VerifiableCredentialBuilder()
        .WithType("UniversityDegreeCredential")
        .WithIssuer("https://issuer.example.com")
        .WithSubject($"did:example:student:{request.UserId}")
        .WithIssuanceDate(DateTimeOffset.UtcNow)
        .WithExpirationDate(DateTimeOffset.UtcNow.AddYears(5))
        
        // Public information (visible to any verifier)
        .WithCredentialSubject("degreeName", "Bachelor of Science in Computer Science")
        .WithCredentialSubject("university", "Example University")
        
        // Private information (Holder must explicitly consent to reveal these)
        .WithSelectiveCredentialSubject("gpa", "3.8")
        .WithSelectiveCredentialSubject("graduationDate", "2023-05-15")
        .WithSelectiveCredentialSubject("honors", "Summa Cum Laude");

    // 2. The ISdJwtIssuerService handles the complex SD-JWT hashing and signing logic automatically
    // It also enforces the configured HAIP policy.
    var credentialResult = await issuer.CreateCredentialAsync(vcBuilder);
    
    // 3. Return the standard OID4VCI response to the wallet
    return Results.Ok(new 
    {
        credential = credentialResult.SdJwt, // The massive {JWT}~{disc1}~{disc2} string
        format = "vc+sd-jwt"
    });
});
```

## What happens under the hood?

When `issuer.CreateCredentialAsync()` runs:

1. **HAIP Validation:** HAIP intercepts the call to ensure your `issuerSigningKey` meets the requirements for `Level2_VeryHigh` (e.g., throwing an error if you accidentally passed in an insecure RSA 1024-bit key).
2. **Salting & Hashing:** For `gpa`, `graduationDate`, and `honors`, the `SdJwt.Net` core generates high-entropy salts, creates disclosure strings, hashes them via SHA-256, and places the hashes into the JWT's `_sd` array.
3. **Decoys:** Decoy hashes are injected to prevent observers from guessing how many claims the user has.
4. **Signing:** The core JWT is signed using your ECDSA key.

## Next Steps

Now that the wallet holds the credential, learn how a Relying Party verifies it in the [Verifying Presentations Guide](verifying-presentations.md).
