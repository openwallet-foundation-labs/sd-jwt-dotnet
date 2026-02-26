# SdJwt.Net Core - RFC 9901 Implementation

[![NuGet Version](https://img.shields.io/nuget/v/SdJwt.Net.svg)](https://www.nuget.org/packages/SdJwt.Net/)
[![License](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](https://opensource.org/licenses/Apache-2.0)

A production-ready .NET library for **Selective Disclosure JSON Web Tokens (SD-JWTs)** compliant with [RFC 9901](https://tools.ietf.org/rfc/rfc9901.txt). This is the core library that provides fundamental SD-JWT functionality with enhanced security, performance optimization, and comprehensive multi-platform support.

## Features

- **RFC 9901 Compliant**: Complete implementation of Selective Disclosure for JSON Web Tokens
- **JWS JSON Serialization**: Full support for Flattened and General JSON formats (RFC 9901 Section 8)
- **Enhanced Security**: Blocks weak algorithms (MD5, SHA-1), enforces approved SHA-2 family
- **Multi-Platform**: .NET 8, 9, and .NET Standard 2.1 with platform-specific optimizations
- **Production Ready**: Battle-tested with comprehensive tests and security hardening

## Installation

```bash
dotnet add package SdJwt.Net
```

## Quick Start

### SD-JWT Structure

```mermaid
graph LR
    subgraph "SD-JWT Compact Format"
        Header[Base64url Header<br/>alg, typ]
        Payload[Base64url Payload<br/>iss, iat, exp<br/>_sd: hashes of hidden claims
        _sd_alg: sha-256]
        Sig[Base64url Signature]
        Disc1[~Disclosure 1<br/>salt + claim_name + value]
        Disc2[~Disclosure 2<br/>salt + claim_name + value]
        KB[~KB-JWT<br/>nonce + aud + iat]
    end

    Header -.->|.|  Payload
    Payload -.->|.| Sig
    Sig -.->|~| Disc1
    Disc1 -.->|~| Disc2
    Disc2 -.->|~| KB

    style Header fill:#1b4332,color:#fff
    style Payload fill:#2d6a4f,color:#fff
    style Sig fill:#40916c,color:#fff
    style KB fill:#d62828,color:#fff
```

### Basic SD-JWT Creation

```csharp
using SdJwt.Net.Issuer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

// Create signing key
using var key = ECDsa.Create();
var signingKey = new ECDsaSecurityKey(key) { KeyId = "issuer-key-1" };

// Create SD-JWT issuer
var issuer = new SdIssuer(signingKey, SecurityAlgorithms.EcdsaSha256);

// Define claims with selective disclosure
var claims = new JwtPayload
{
    ["iss"] = "https://issuer.example.com",
    ["given_name"] = "John",
    ["family_name"] = "Doe",
    ["email"] = "john.doe@example.com",
    ["address"] = new { 
        street = "123 Main St", 
        city = "Anytown", 
        state = "CA" 
    }
};

// Configure selective disclosure
var options = new SdIssuanceOptions
{
    DisclosureStructure = new
    {
        given_name = true,
        family_name = true,
        email = true,
        address = new { city = true, state = true }
    }
};

// Issue SD-JWT
var result = issuer.Issue(claims, options, holderJwk);
```

### Holder Creates Presentation

```csharp
using SdJwt.Net.Holder;

// Create holder from issuance
var holder = new SdJwtHolder(result.Issuance);

// Create selective presentation (only disclose email and city)
var presentation = holder.CreatePresentation(
    disclosure => disclosure.ClaimName == "email" || disclosure.ClaimName == "city",
    keyBindingJwt, holderPrivateKey, SecurityAlgorithms.EcdsaSha256);

// For SD-JWT (without KB-JWT), compact output follows RFC 9901 and ends with "~".
// For SD-JWT+KB, the final component is the KB-JWT (no trailing empty component).
```

### Verification

```mermaid
sequenceDiagram
    participant Holder as Holder (Wallet)
    participant Verifier as Verifier

    Holder->>Verifier: VP Token (SD-JWT~email~KB-JWT)

    Note over Verifier: Step 1 - Parse & split
    Verifier->>Verifier: Split on ~ separator
    Verifier->>Verifier: Decode header / payload / sig

    Note over Verifier: Step 2 - Verify issuer signature
    Verifier->>Verifier: Resolve issuer JWKS
    Verifier->>Verifier: Validate JWT signature

    Note over Verifier: Step 3 - Reconstruct disclosures
    Verifier->>Verifier: For each disclosure: SHA-256(salt+name+value)
    Verifier->>Verifier: Verify hash appears in _sd array

    Note over Verifier: Step 4 - Verify Key Binding
    Verifier->>Verifier: Verify KB-JWT signature with holder key
    Verifier->>Verifier: Check nonce matches expected value
    Verifier->>Verifier: Check aud matches verifier URL

    Verifier-->>Holder: Verification Result + Revealed Claims
```

```csharp
using SdJwt.Net.Verifier;

// Create verifier with key resolver
var verifier = new SdVerifier(async issuer => 
{
    // Resolve issuer's public key from trusted source
    return await ResolveIssuerKeyAsync(issuer);
});

// Verify presentation
var validationParams = new TokenValidationParameters
{
    ValidateIssuer = true,
    ValidIssuer = "https://issuer.example.com",
    ValidateAudience = false,
    ValidateLifetime = true
};

// Verify presentation with expected nonce for replay protection
var expectedNonce = "job-application-2024-12345";
var result = await verifier.VerifyAsync(presentation, validationParams, kbParams, expectedNonce);

if (result.KeyBindingVerified)
{
    Console.WriteLine($"Key Binding Verified. Nonce: {result.KeyBindingJwtPayload?["nonce"]}");
}

// Optional strict policy controls (strict mode is enabled by default)
var verifierOptions = new SdVerifierOptions
{
    StrictMode = true,
    KeyBinding = new KeyBindingValidationPolicy
    {
        RequireKeyBinding = true,
        ExpectedAudience = "https://verifier.example.com",
        MaxKeyBindingJwtAge = TimeSpan.FromMinutes(5)
    }
};
```

## Security Features

- **Algorithm Enforcement**: Blocks MD5, SHA-1; enforces SHA-2 family
- **Constant-time Operations**: Protection against timing attacks
- **Input Validation**: Comprehensive validation throughout APIs
- **Cross-platform Security**: Consistent guarantees across platforms

## Documentation

For comprehensive examples and advanced usage, see the [main repository](https://github.com/openwallet-foundation-labs/sd-jwt-dotnet).

## License

Licensed under the [Apache License 2.0](https://opensource.org/licenses/Apache-2.0).
