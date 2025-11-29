# SD-JWT for .NET

[![NuGet Version](https://img.shields.io/nuget/v/SdJwt.Net.svg)](https://www.nuget.org/packages/SdJwt.Net/)
[![Build Status](https://github.com/thomas-tran/sd-jwt-dotnet/actions/workflows/ci-cd.yml/badge.svg)](https://github.com/thomas-tran/sd-jwt-dotnet/actions)
[![License](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](https://opensource.org/licenses/Apache-2.0)

A comprehensive, production-ready .NET ecosystem for **Selectively Disclosable JSON Web Tokens (SD-JWTs)** and **Verifiable Credentials**. This modular library suite provides secure, compliant implementations of IETF specifications with enhanced security, performance optimization, and cross-platform support.

## ?? Package Overview

The SD-JWT.NET ecosystem consists of six modular packages that can be used independently or together:

| Package | Purpose | Specification | Version | Installation |
|---------|---------|---------------|---------|--------------|
| **[SdJwt.Net](https://www.nuget.org/packages/SdJwt.Net/)** | Core SD-JWT functionality | RFC 9901 | 1.0.0 | `dotnet add package SdJwt.Net` |
| **[SdJwt.Net.Vc](https://www.nuget.org/packages/SdJwt.Net.Vc/)** | Verifiable Credentials | draft-ietf-oauth-sd-jwt-vc-13 | 0.13.0 | `dotnet add package SdJwt.Net.Vc` |
| **[SdJwt.Net.StatusList](https://www.nuget.org/packages/SdJwt.Net.StatusList/)** | Credential revocation | draft-ietf-oauth-status-list-13 | 0.13.0 | `dotnet add package SdJwt.Net.StatusList` |
| **[SdJwt.Net.Oid4Vci](https://www.nuget.org/packages/SdJwt.Net.Oid4Vci/)** | OpenID4VCI Protocol | OID4VCI 1.0 Final | 1.0.0 | `dotnet add package SdJwt.Net.Oid4Vci` |
| **[SdJwt.Net.Oid4Vp](https://www.nuget.org/packages/SdJwt.Net.Oid4Vp/)** | OpenID4VP Protocol | OID4VP 1.0 Final | 1.0.0 | `dotnet add package SdJwt.Net.Oid4Vp` |
| **[SdJwt.Net.OidFederation](https://www.nuget.org/packages/SdJwt.Net.OidFederation/)** | OpenID Federation | OpenID Federation 1.0 | 1.0.0 | `dotnet add package SdJwt.Net.OidFederation` |

## ?? Quick Start

### Core SD-JWT Usage

For basic selective disclosure functionality:

```bash
dotnet add package SdJwt.Net
```

```csharp
using SdJwt.Net.Issuer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.IdentityModel.Tokens.Jwt;

// Create signing key
using var key = ECDsa.Create();
var issuer = new SdIssuer(new ECDsaSecurityKey(key), SecurityAlgorithms.EcdsaSha256);

// Define claims
var claims = new JwtPayload
{
    ["iss"] = "https://issuer.example.com",
    ["sub"] = "user123",
    ["given_name"] = "John",
    ["family_name"] = "Doe",
    ["email"] = "john.doe@example.com"
};

// Configure selective disclosure
var options = new SdIssuanceOptions
{
    DisclosureStructure = new { given_name = true, email = true }
};

// Issue SD-JWT
var result = issuer.Issue(claims, options);
Console.WriteLine($"SD-JWT: {result.Issuance}");
```

### Verifiable Credentials

For verifiable credentials according to draft-ietf-oauth-sd-jwt-vc-13:

```bash
dotnet add package SdJwt.Net.Vc
```

```csharp
using SdJwt.Net.Vc.Issuer;
using SdJwt.Net.Vc.Models;

// Create VC issuer
var vcIssuer = new SdJwtVcIssuer(signingKey, SecurityAlgorithms.EcdsaSha256);

// Define credential payload
var vcPayload = new SdJwtVcPayload
{
    Vct = "https://credentials.example.com/identity_credential",
    Issuer = "https://dmv.example.com",
    Subject = "did:example:123",
    AdditionalData = new Dictionary<string, object>
    {
        ["given_name"] = "John",
        ["family_name"] = "Doe",
        ["license_class"] = "A"
    }
};

// Issue SD-JWT VC
var result = vcIssuer.Issue(vcPayload, options);
```

### Status List (Revocation)

For credential revocation capabilities:

```bash
dotnet add package SdJwt.Net.StatusList
```

```csharp
using SdJwt.Net.StatusList.Issuer;
using SdJwt.Net.StatusList.Models;
using System.Collections;

// Create status list manager
var statusManager = new StatusListManager(signingKey, SecurityAlgorithms.EcdsaSha256);

// Create status list with 1000 credential slots
var statusBits = new BitArray(1000);
var statusListToken = await statusManager.CreateStatusListTokenFromBitArrayAsync(
    "https://issuer.example.com/status/1", statusBits);

// Add status reference to credentials
var statusReference = new StatusListReference 
{ 
    Index = 42, 
    Uri = "https://issuer.example.com/status/1" 
};
```

### OpenID4VCI Protocol

For credential issuance workflows:

```bash
dotnet add package SdJwt.Net.Oid4Vci
```

```csharp
using SdJwt.Net.Oid4Vci.Models;

// Create credential offer
var offer = new CredentialOffer
{
    CredentialIssuer = "https://issuer.example.com",
    CredentialConfigurationIds = new[] { "UniversityDegree_SDJWT" },
    Grants = new Dictionary<string, object>
    {
        ["urn:ietf:params:oauth:grant-type:pre-authorized_code"] = new
        {
            pre_authorized_code = "auth-code-123",
            user_pin_required = true
        }
    }
};

// Generate offer URI for QR code
var offerUri = offer.ToUri();
Console.WriteLine($"Credential Offer: {offerUri}");
```

### OpenID4VP Protocol

For verifiable presentation workflows:

```bash
dotnet add package SdJwt.Net.Oid4Vp
```

```csharp
using SdJwt.Net.Oid4Vp.Verifier;
using SdJwt.Net.Oid4Vp.Models;

// Verifier: Create presentation request
var request = PresentationRequestBuilder
    .Create("https://verifier.example.com", "https://verifier.example.com/response")
    .WithPurpose("We need to verify your university degree for employment.")
    .RequestCredential("UniversityDegree")
    .BuildUri();

// Generate QR code for wallet
Console.WriteLine($"Presentation Request: {request}");
```

### OpenID Federation

For trust chain validation and federation management:

```bash
dotnet add package SdJwt.Net.OidFederation
```

```csharp
using SdJwt.Net.OidFederation.Logic;
using SdJwt.Net.OidFederation.Models;

// Build entity configuration for your issuer
var entityConfig = EntityConfigurationBuilder
    .Create("https://my-issuer.example.com")
    .WithSigningKey(signingKey)
    .WithJwkSet(jwkSet)
    .AddAuthorityHint("https://national-trust.gov")
    .AsCredentialIssuer(credentialIssuerMetadata)
    .Build();

// Publish at /.well-known/openid-federation
// ... 

// Validate trust chains for other entities
var trustAnchors = new Dictionary<string, SecurityKey>
{
    ["https://national-trust.gov"] = nationalTrustPublicKey
};

var resolver = new TrustChainResolver(httpClient, trustAnchors);
var result = await resolver.ResolveAsync("https://university.example.com");

if (result.IsValid)
{
    Console.WriteLine($"? Trust chain valid: {result.GetTrustChainSummary()}");
    
    if (result.HasTrustMark(CommonTrustMarks.EducationalInstitution))
    {
        Console.WriteLine("University is properly accredited");
    }
}
```

## ??? Architecture

The modular design provides flexibility while maintaining interoperability:

```
???????????????????????????????????????????????????????????????????????????????????
?                        Applications                                             ?
???????????????????????????????????????????????????????????????????????????????????
?SdJwt.Net?SdJwt.Net.StatusList?SdJwt.Net.Oid4Vci?SdJwt.Net.Oid4Vp?SdJwt.Net?
?   .Vc   ?                 ?                 ?                 ?OidFed   ?
?         ?                 ?                 ?                 ?         ?
?• VC     ? • Status Lists  ? • Issuance      ? • Verification  ?• Trust  ?
?  Issuers? • Revocation    ?   Protocol      ?   Protocol      ?  Chains ?
?• VC     ? • Performance   ? • Offer Builder ? • Request       ?• Entity ?
?  Verify ?                 ? • Proof Builder ?   Builder       ?  Config ?
?• VC Mod ?                 ?                 ? • VP Validator  ?• Federat?
???????????????????????????????????????????????????????????????????????????????????
?                       SdJwt.Net (Core)                                         ?
?                                                                                 ?
? • RFC 9901 Implementation    • JWS JSON Serialization                          ?
? • Issuers & Verifiers       • Security & Performance                           ?
? • Holders & Presentations   • Multi-platform Support                           ?
???????????????????????????????????????????????????????????????????????????????????
```

## ? Key Features

### ?? Security First
- **RFC 9901 Compliant**: Complete implementation with security considerations
- **Enhanced Algorithm Security**: Blocks weak algorithms (MD5, SHA-1), enforces approved SHA-2 family
- **Trust Chain Validation**: OpenID Federation 1.0 compliant trust management
- **Constant-time Operations**: Protection against timing attacks
- **Cross-Platform Compatibility**: Optimized for .NET 8+ with .NET Standard 2.1 fallback
- **Input Validation**: Comprehensive validation throughout

### ?? Performance Optimized
- **Multi-target Support**: .NET 8, 9, and .NET Standard 2.1
- **Platform-Specific Optimizations**: Modern static hash methods on .NET 6+, traditional patterns for older frameworks
- **Memory Efficient**: Minimal allocations in hot paths
- **Caching Support**: Built-in caching for status lists, keys, and federation data
- **Async Throughout**: Non-blocking operations for scalability

### ?? Developer Experience
- **Modular Design**: Use only what you need
- **Type Safety**: Strong typing with comprehensive models
- **Extensive Documentation**: Complete API documentation and examples
- **Rich Tooling**: IntelliSense, debugging symbols, source linking

## ?? Supported Specifications

| Specification | Status | Package | Version | Features |
|---------------|--------|---------|---------|----------|
| **RFC 9901** | ? Complete | SdJwt.Net | 1.0.0 | Core SD-JWT, JWS JSON serialization |
| **SD-JWT VC** | ? Complete | SdJwt.Net.Vc | 0.13.0 | VC support, type safety (draft-ietf-oauth-sd-jwt-vc-13) |
| **Status List** | ? Complete | SdJwt.Net.StatusList | 0.13.0 | Revocation, suspension, privacy (draft-ietf-oauth-status-list-13) |
| **OID4VCI 1.0** | ? Complete | SdJwt.Net.Oid4Vci | 1.0.0 | Protocol models, flows, transport-agnostic |
| **OID4VP 1.0** | ? Complete | SdJwt.Net.Oid4Vp | 1.0.0 | Presentation Exchange, cross-device flow |
| **OpenID Federation 1.0** | ? Complete | SdJwt.Net.OidFederation | 1.0.0 | Trust chains, entity config, metadata policies |

## ?? Use Cases

### Basic Selective Disclosure
Just need core SD-JWT functionality? Use the core package:
- Privacy-preserving authentication
- Minimal data sharing
- Consent-based disclosure

### Digital Credentials
Building a credential system? Add the VC package:
- University diplomas
- Professional licenses
- Government-issued IDs
- Employment verification

### Enterprise Deployments
Need revocation capabilities? Include StatusList:
- Large-scale credential management
- Real-time revocation checking
- Privacy-preserving status verification

### Credential Issuance Protocols
Implementing issuance workflows? Use OID4VCI:
- QR code credential offers
- Mobile wallet integration
- Pre-authorized issuance flows
- Deferred credential delivery

### Verification & Presentation Protocols
Building verification services? Use OID4VP:
- Cross-device presentation flows
- QR code-based verification
- Presentation Exchange (DIF)
- Complex presentation requirements

### Federation & Trust Management
Need trust chain validation? Use OpenID Federation:
- Trust anchor management
- Entity configuration publishing
- Recursive trust validation
- Metadata policy enforcement
- Trust mark validation

## ?? Platform Compatibility

### Excellent Multi-Platform Support

| Platform | .NET 8.0 | .NET 9.0 | .NET Standard 2.1 |
|----------|-----------|-----------|-------------------|
| **Windows** | ? | ? | ? |
| **Linux** | ? | ? | ? |
| **macOS** | ? | ? | ? |
| **Docker** | ? | ? | ? |

### Algorithm Support

**Approved Algorithms (RFC 9901 Compliant):**
- ? **SHA-256** (Default, recommended)
- ? **SHA-384** (Higher security)
- ? **SHA-512** (Highest security)

**Blocked Weak Algorithms:**
- ? **MD5** (Cryptographically broken)
- ? **SHA-1** (Cryptographically weak)

**Signing Algorithm Support:**
- ? **ES256/ES384/ES512** (ECDSA - Recommended for Federation)
- ? **RS256/RS384/RS512** (RSA-PKCS1)
- ? **PS256/PS384/PS512** (RSA-PSS)
- ? **HS256/HS384/HS512** (HMAC)
- ? **EdDSA** (Ed25519 - .NET 8+ only)

## ?? Documentation

Complete documentation is available:

- **[Core Documentation](README-Core.md)** - Core SD-JWT functionality
- **[VC Documentation](README-Vc.md)** - Verifiable Credentials guide
- **[StatusList Documentation](README-StatusList.md)** - Revocation management
- **[OID4VCI Documentation](README-Oid4Vci.md)** - Credential issuance protocol
- **[OID4VP Documentation](README-Oid4Vp.md)** - Presentation verification protocol
- **[OpenID Federation Documentation](README-OidFederation.md)** - Trust chain management
- **[CHANGELOG](CHANGELOG.md)** - Version history and updates

## ?? Examples

Complete examples demonstrate real-world usage:

```csharp
// Complete workflow with federation trust
var issuer = new SdIssuer(signingKey, SecurityAlgorithms.EcdsaSha256);
var holder = new SdJwtHolder(issuerOutput.Issuance);
var verifier = new SdVerifier(keyProvider);

// Validate issuer trust chain
var trustResolver = new TrustChainResolver(httpClient, trustAnchors);
var trustResult = await trustResolver.ResolveAsync("https://issuer.example.com");

if (trustResult.IsValid && trustResult.SupportsProtocol("openid_credential_issuer"))
{
    Console.WriteLine("? Issuer is trusted and can issue credentials");
    
    // Issue with selective disclosure
    var issuance = issuer.Issue(claims, options, holderPublicKey);

    // Create presentation (selective disclosure)
    var presentation = holder.CreatePresentation(
        disclosure => disclosure.ClaimName == "email",
        keyBindingPayload, holderPrivateKey, SecurityAlgorithms.EcdsaSha256);

    // Verify presentation
    var result = await verifier.VerifyAsync(presentation, validationParams, kbValidationParams);
    Console.WriteLine($"Verified: {result.KeyBindingVerified}");
}
```

### Complete Federation Ecosystem Example

Demonstrates full federation trust chain with credential issuance and verification:

```csharp
// 1. Trust Anchor: National Education Authority
var trustAnchorConfig = EntityConfigurationBuilder
    .Create("https://education.gov")
    .WithSigningKey(trustAnchorKey)
    .WithJwkSet(trustAnchorJwkSet)
    .WithMetadata(new EntityMetadata { /* Authority metadata */ })
    .Build();

// 2. Intermediate Authority: State University System
var authorityConfig = EntityConfigurationBuilder
    .Create("https://university-system.state.edu")
    .WithSigningKey(authorityKey)
    .WithJwkSet(authorityJwkSet)
    .AddAuthorityHint("https://education.gov")
    .WithConstraints(new EntityConstraints 
    { 
        NamingConstraints = new NamingConstraints
        {
            Permitted = new[] { "*.university.state.edu" }
        }
    })
    .Build();

// 3. Leaf Entity: Specific University
var universityConfig = EntityConfigurationBuilder
    .Create("https://tech.university.state.edu")
    .WithSigningKey(universityKey)
    .WithJwkSet(universityJwkSet)
    .AddAuthorityHint("https://university-system.state.edu")
    .AsCredentialIssuer(credentialIssuerMetadata)
    .Build();

// 4. Verifier validates the entire chain
var trustAnchors = new Dictionary<string, SecurityKey>
{
    ["https://education.gov"] = trustAnchorPublicKey
};

var resolver = new TrustChainResolver(httpClient, trustAnchors);
var result = await resolver.ResolveAsync("https://tech.university.state.edu");

if (result.IsValid)
{
    Console.WriteLine($"? University trust chain valid!");
    Console.WriteLine($"Trust path: {result.GetTrustChainSummary()}");
    
    // Trust marks validation
    if (result.HasTrustMark(CommonTrustMarks.EducationalInstitution))
    {
        Console.WriteLine("University is accredited for education");
        
        // Now safe to accept credentials from this university
        var credentials = await ProcessCredentialIssuance();
    }
}
```

## ?? Contributing

We welcome contributions! Development setup:

1. Clone the repository
2. Restore packages: `dotnet restore`
3. Build solution: `dotnet build`
4. Run tests: `dotnet test`

### Project Structure
```
src/
??? SdJwt.Net/              # Core SD-JWT functionality (RFC 9901)
??? SdJwt.Net.Vc/           # Verifiable Credentials extension
??? SdJwt.Net.StatusList/   # Status List implementation
??? SdJwt.Net.Oid4Vci/      # OpenID4VCI protocol implementation
??? SdJwt.Net.Oid4Vp/       # OpenID4VP protocol implementation
??? SdJwt.Net.OidFederation/# OpenID Federation implementation
tests/
??? SdJwt.Net.Tests/        # Core functionality tests
??? SdJwt.Net.Vc.Tests/     # VC-specific tests
??? SdJwt.Net.StatusList.Tests/ # Status list tests
??? SdJwt.Net.Oid4Vci.Tests/    # OID4VCI protocol tests
??? SdJwt.Net.Oid4Vp.Tests/     # OID4VP protocol tests
??? SdJwt.Net.OidFederation.Tests/ # Federation tests
samples/
??? SdJwt.Net.Samples/      # Complete workflow demonstrations
```

## ?? Security Considerations

### Hash Algorithm Security
The library enforces strong cryptographic practices:

```csharp
// ? Approved algorithms
SdJwtUtils.IsApprovedHashAlgorithm("SHA-256"); // Returns: true
SdJwtUtils.IsApprovedHashAlgorithm("SHA-384"); // Returns: true

// ? Blocked weak algorithms
SdJwtUtils.CreateDigest("MD5", disclosure); // Throws NotSupportedException
```

### Federation Security
Trust chain validation provides additional security layers:

```csharp
// Validate entity before accepting credentials
var trustResult = await resolver.ResolveAsync(credentialIssuer);
if (!trustResult.IsValid)
{
    throw new SecurityException($"Issuer not trusted: {trustResult.ErrorMessage}");
}

// Check specific trust requirements
var requirements = TrustChainRequirements.ForProtocol("openid_credential_issuer");
if (!trustResult.SatisfiesRequirements(requirements))
{
    throw new SecurityException("Issuer does not meet protocol requirements");
}
```

### Performance Optimizations
- **Modern .NET**: Uses `SHA256.HashData()` for optimal performance
- **Legacy .NET**: Falls back to traditional `Create()` pattern for compatibility
- **Cross-platform**: Consistent security across all supported platforms
- **Caching**: Secure federation data caching with appropriate TTLs

## ?? License

This project is licensed under the Apache License 2.0 - see the [LICENSE](LICENSE) file for details.

## ?? Acknowledgments

- [RFC 9901](https://tools.ietf.org/rfc/rfc9901.txt) - Selective Disclosure for JSON Web Tokens
- [IETF OAuth Working Group](https://datatracker.ietf.org/wg/oauth/) - Specification development
- [OpenID Foundation](https://openid.net/) - SD-JWT, OID4VCI, OID4VP, and OpenID Federation specifications development
- [W3C](https://www.w3.org/) - Verifiable Credentials data model
- [DIF](https://identity.foundation/) - Presentation Exchange specification
- [Open Wallet Foundation](https://openwallet.foundation/) - Digital identity standards

---

**Ready to get started?** Choose the package that fits your needs:
- **Basic selective disclosure**: `dotnet add package SdJwt.Net`
- **Verifiable credentials**: `dotnet add package SdJwt.Net.Vc`
- **Credential revocation**: `dotnet add package SdJwt.Net.StatusList`
- **Issuance protocols**: `dotnet add package SdJwt.Net.Oid4Vci`
- **Verification protocols**: `dotnet add package SdJwt.Net.Oid4Vp`
- **Federation & trust**: `dotnet add package SdJwt.Net.OidFederation`