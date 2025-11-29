# SD-JWT Core for .NET

[![NuGet Version](https://img.shields.io/nuget/v/SdJwt.Net.svg)](https://www.nuget.org/packages/SdJwt.Net/)
[![License](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](https://opensource.org/licenses/Apache-2.0)

A production-ready .NET library for creating and verifying **Selectively Disclosable JSON Web Tokens (SD-JWTs)** compliant with **RFC 9901**. This is the core library that provides fundamental SD-JWT functionality with enhanced security, performance optimization, and comprehensive multi-platform support.

## ?? Features

- **RFC 9901 Compliant**: Complete implementation of Selective Disclosure for JSON Web Tokens
- **JWS JSON Serialization**: Full support for Flattened and General JSON formats (RFC 9901 Section 8)
- **Enhanced Security**: Blocks weak algorithms (MD5, SHA-1), enforces approved SHA-2 family
- **Multi-Platform**: .NET 8, 9, 10, and .NET Standard 2.1 with platform-specific optimizations
- **Production Ready**: Battle-tested with 77+ comprehensive tests and security hardening

## ?? Installation

```bash
dotnet add package SdJwt.Net
```

## ?? Quick Start

### Basic SD-JWT Creation

```csharp
using SdJwt.Net.Issuer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.IdentityModel.Tokens.Jwt;

// Create signing key
using var key = ECDsa.Create();
var signingKey = new ECDsaSecurityKey(key) { KeyId = "issuer-key-1" };

// Create SD-JWT issuer
var issuer = new SdIssuer(signingKey, SecurityAlgorithms.EcdsaSha256);

// Define claims
var claims = new JwtPayload
{
    ["iss"] = "https://issuer.example.com",
    ["sub"] = "user123",
    ["iat"] = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
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
var result = issuer.Issue(claims, options);
Console.WriteLine($"SD-JWT Issuance: {result.Issuance}");
Console.WriteLine($"Number of Disclosures: {result.Disclosures.Count}");
```

### Holder Creates Presentation

```csharp
using SdJwt.Net.Holder;

// Create holder from issuance
var holder = new SdJwtHolder(result.Issuance);

// Create holder's key pair for key binding
using var holderKey = ECDsa.Create();
var holderPrivateKey = new ECDsaSecurityKey(holderKey);

// Create selective presentation (only disclose email and city)
var presentation = holder.CreatePresentation(
    disclosure => disclosure.ClaimName == "email" || disclosure.ClaimName == "city",
    new JwtPayload 
    { 
        ["aud"] = "https://verifier.example.com",
        ["nonce"] = "presentation-nonce-123",
        ["iat"] = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
    },
    holderPrivateKey,
    SecurityAlgorithms.EcdsaSha256
);

Console.WriteLine($"Presentation: {presentation}");
```

### Verification

```csharp
using SdJwt.Net.Verifier;

// Create verifier with key provider
var verifier = new SdVerifier(async issuer => 
{
    // In production, resolve the issuer's public key from a trusted source
    return signingKey; // For demo purposes
});

// Verify presentation
var validationParams = new TokenValidationParameters
{
    ValidateIssuer = true,
    ValidIssuer = "https://issuer.example.com",
    ValidateAudience = false,
    ValidateLifetime = true,
    ClockSkew = TimeSpan.FromMinutes(5)
};

var kbValidationParams = new TokenValidationParameters
{
    ValidateIssuer = false,
    ValidateAudience = true,
    ValidAudience = "https://verifier.example.com",
    ValidateLifetime = false,
    IssuerSigningKey = new ECDsaSecurityKey(holderKey) // Holder's public key
};

var verificationResult = await verifier.VerifyAsync(presentation, validationParams, kbValidationParams);

Console.WriteLine($"Verification Successful: {verificationResult.KeyBindingVerified}");
Console.WriteLine("Disclosed Claims:");
foreach (var claim in verificationResult.ClaimsPrincipal.Claims)
{
    if (!claim.Type.StartsWith("_sd"))
        Console.WriteLine($"  {claim.Type}: {claim.Value}");
}
```

### JWS JSON Serialization Support

```csharp
// Issue as JWS Flattened JSON Serialization
var jsonSdJwt = issuer.IssueAsJsonSerialization(claims, options);
Console.WriteLine($"JSON SD-JWT: {jsonSdJwt.SdJwt}");

// Convert between formats
var compactSdJwt = "eyJ...~WyJ...~";
var flattenedJson = SdJwtJsonSerializer.ToFlattenedJsonSerialization(compactSdJwt);
var generalJson = SdJwtJsonSerializer.ToGeneralJsonSerialization(compactSdJwt);

// Verify JSON serialized SD-JWT
var jsonResult = await verifier.VerifyJsonSerializationAsync(
    flattenedJson, validationParams, kbValidationParams);
```

## ?? Security Features

### Algorithm Enforcement

The library enforces strong cryptographic practices:

```csharp
// ? Approved algorithms (automatically validated)
SdJwtUtils.IsApprovedHashAlgorithm("SHA-256"); // true
SdJwtUtils.IsApprovedHashAlgorithm("SHA-384"); // true
SdJwtUtils.IsApprovedHashAlgorithm("SHA-512"); // true

// ? Weak algorithms are blocked
try 
{
    SdJwtUtils.CreateDigest("MD5", disclosure);
}
catch (NotSupportedException ex)
{
    // "Hash algorithm 'MD5' is cryptographically weak and not supported for SD-JWT"
}
```

### Performance Optimizations

**Platform-specific optimizations:**
- **.NET 6+**: Uses modern static `SHA256.HashData()` methods for optimal performance
- **.NET Standard 2.1**: Falls back to traditional `Create()` pattern for compatibility
- **Cross-platform**: Consistent security across Windows, Linux, macOS

### Security Hardening

- **Constant-time comparisons** for sensitive operations
- **Salt entropy requirements** (128-bit minimum)
- **Input validation** throughout all APIs
- **RFC 9901 security considerations** fully implemented

## ??? Architecture

This core library provides:

### Core Components
- **`SdIssuer`**: Creates SD-JWTs with selective disclosure
- **`SdJwtHolder`**: Manages SD-JWT presentations and selective disclosure
- **`SdVerifier`**: Verifies SD-JWTs and key binding
- **`SdJwtParser`**: Parses and inspects SD-JWT structures

### Models and Utilities
- **`Disclosure`**: Represents individual selectively disclosable claims
- **`ParsedSdJwt`**: Structured representation of SD-JWT components
- **`SdIssuanceOptions`**: Configuration for selective disclosure structure
- **`SdJwtUtils`**: Cryptographic utilities with security enforcement

### JSON Serialization Models
- **`SdJwtJsonSerialization`**: JWS Flattened JSON Serialization (RFC 9901 Section 8.1)
- **`SdJwtGeneralJsonSerialization`**: JWS General JSON Serialization (RFC 9901 Section 8.2)
- **`SdJwtJsonSerializer`**: Format conversion utilities

## ?? Multi-Platform Support

### Framework Compatibility

| Framework | Status | Optimizations |
|-----------|--------|---------------|
| .NET 8.0+ | ? Full Support | Modern crypto APIs, enhanced performance |
| .NET 9.0+ | ? Full Support | Latest features, optimal performance |
| .NET 10.0+ | ? Full Support | Forward compatibility |
| .NET Standard 2.1 | ? Full Support | Backward compatibility, traditional APIs |

### Platform Coverage
- ? **Windows** (x64, x86, ARM64)
- ? **Linux** (x64, ARM64)
- ? **macOS** (x64, ARM64/Apple Silicon)
- ? **Docker containers**
- ? **Cloud platforms** (Azure, AWS, GCP)

## ?? Testing and Quality

### Comprehensive Test Coverage
- **90+ test cases** covering core functionality
- **Security validation tests** for algorithm enforcement
- **Cross-platform compatibility tests**
- **RFC 9901 compliance tests**
- **Performance regression tests**

### Quality Metrics
- **100% pass rate** across all supported platforms
- **Memory efficiency** validated through profiling
- **Thread safety** verified for concurrent operations
- **Input validation** tested for edge cases and malicious inputs

## ?? Related Packages

For specialized use cases, consider these additional packages:

- **[SdJwt.Net.Vc](https://www.nuget.org/packages/SdJwt.Net.Vc/)** - SD-JWT Verifiable Credentials (draft-ietf-oauth-sd-jwt-vc-13)
- **[SdJwt.Net.StatusList](https://www.nuget.org/packages/SdJwt.Net.StatusList/)** - Status List for credential revocation (draft-ietf-oauth-status-list-13)

## ?? Documentation

### Specifications
- [RFC 9901](https://tools.ietf.org/rfc/rfc9901.txt) - The specification this library implements
- [JWS RFC 7515](https://tools.ietf.org/html/rfc7515) - JSON Web Signature specification

### Implementation Details
- **Constant-time Operations**: Protection against timing attacks
- **Memory Management**: Efficient allocation patterns
- **Error Handling**: Comprehensive exception hierarchies
- **Logging Integration**: Structured logging throughout

## ?? Migration and Compatibility

### From Version 0.x
- **Breaking Changes**: Minimal, mostly additive features
- **New Features**: JSON serialization, enhanced security
- **Compatibility**: .NET Standard 2.1 support maintained

### Best Practices
- **Algorithm Selection**: Use SHA-256 or stronger for all operations
- **Key Management**: Implement proper key rotation policies
- **Error Handling**: Handle `NotSupportedException` for algorithm validation
- **Performance**: Use modern .NET versions for optimal performance

## ?? License

Licensed under the Apache License 2.0. See [LICENSE](LICENSE) for details.

---

**Next Steps:**
- **For Verifiable Credentials**: Add `SdJwt.Net.Vc` package
- **For Revocation Support**: Add `SdJwt.Net.StatusList` package
- **For Advanced Features**: Explore JSON serialization and multi-signature support