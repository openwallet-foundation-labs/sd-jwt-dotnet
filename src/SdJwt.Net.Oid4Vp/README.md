# SdJwt.Net.Oid4Vp - OpenID for Verifiable Presentations

[![NuGet Version](https://img.shields.io/nuget/v/SdJwt.Net.Oid4Vp.svg)](https://www.nuget.org/packages/SdJwt.Net.Oid4Vp/)
[![License](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](https://opensource.org/licenses/Apache-2.0)

Implementation of **OpenID4VP 1.0** specification for verifiable presentation verification. Provides complete protocol support with Presentation Exchange v2.1.1 integration and cross-device flow support.

## Features

- **OID4VP 1.0 Final**: Complete specification implementation  
- **Presentation Exchange v2.1.1**: Full DIF PE integration
- **Cross-Device Flow**: QR code-based presentation flows
- **Complex Requirements**: Multi-credential presentation support
- **Security Validation**: Comprehensive validation with key binding

## Installation

```bash
dotnet add package SdJwt.Net.Oid4Vp
```

## Quick Start

### Create Presentation Request

```csharp
using SdJwt.Net.Oid4Vp.Models;
using SdJwt.Net.Oid4Vp.Verifier;

var presentationRequest = new AuthorizationRequest
{
    ClientId = "https://verifier.example.com",
    ResponseType = "vp_token",
    ResponseMode = "direct_post",
    ResponseUri = "https://verifier.example.com/presentations",
    Nonce = "presentation_nonce_123",
    PresentationDefinition = new PresentationDefinition
    {
        Id = "employment_verification",
        InputDescriptors = new[]
        {
            new InputDescriptor
            {
                Id = "employment_credential",
                Constraints = new Constraints
                {
                    Fields = new[]
                    {
                        new Field { Path = new[] { "$.position" } },
                        new Field { Path = new[] { "$.employment_type" } }
                    }
                }
            }
        }
    }
};
```

### Process VP Token Response (Recommended - OID4VP Compliant)

```csharp
using SdJwt.Net.Oid4Vp.Verifier;

// Create validator with SD-JWT VC validation enabled (recommended)
var validator = new VpTokenValidator(
    keyProvider: async (jwtToken) => {
        // Resolve issuer's public key based on JWT header/payload
        return await GetIssuerPublicKeyAsync(jwtToken.Issuer);
    },
    useSdJwtVcValidation: true); // Enables vct, iss, typ validation

// Use factory method for OID4VP-compliant options
var options = VpTokenValidationOptions.CreateForOid4Vp("https://verifier.example.com");

// Optional: Customize validation
options.ValidIssuers = new[] { "https://trusted-issuer.example.com" };
options.MaxKeyBindingAge = TimeSpan.FromMinutes(5); // Stricter than default

// Validate VP token
var result = await validator.ValidateAsync(
    vpTokenResponse, 
    expectedNonce: "presentation_nonce_123", 
    options);

if (result.IsValid)
{
    foreach (var tokenResult in result.ValidatedTokens)
    {
        var vctClaim = tokenResult.Claims["vct"];
        var issuer = tokenResult.Claims["iss"];
        // Use verified claims safely
    }
}
```

### Security Features

This library provides comprehensive security validation per OID4VP 1.0:

#### **Nonce Validation** (OID4VP Section 14.1)
```csharp
// Nonce validation is AUTOMATIC when you provide expectedNonce
var result = await validator.ValidateAsync(response, expectedNonce, options);

// The validator ensures:
// - KB-JWT contains 'nonce' claim
// - Nonce matches the expected value from authorization request
// - Prevents replay attacks
```

#### **Audience Validation** (OID4VP Section 8.6)
```csharp
// Enabled by default for security
var options = VpTokenValidationOptions.CreateForOid4Vp("https://verifier.example.com");

// The validator ensures:
// - KB-JWT 'aud' claim matches your client_id
// - Prevents token reuse across different verifiers
// - Can be customized or disabled if needed:
options.ValidateKeyBindingAudience = false; // Not recommended
```

#### **Freshness Validation** (OID4VP Section 14.1)
```csharp
// Enabled by default to prevent replay attacks
var options = VpTokenValidationOptions.CreateForOid4Vp("https://verifier.example.com");
options.MaxKeyBindingAge = TimeSpan.FromMinutes(10); // Default

// The validator ensures:
// - KB-JWT 'iat' claim is present
// - KB-JWT was issued recently (within MaxKeyBindingAge)
// - Includes clock skew tolerance (default: 5 minutes)
```

#### **SD-JWT VC Format Validation** (draft-ietf-oauth-sd-jwt-vc)
```csharp
// Enabled by default when using VpTokenValidator
var validator = new VpTokenValidator(keyProvider, useSdJwtVcValidation: true);

// The validator ensures:
// - 'vct' claim is present and valid
// - 'iss' claim is present
// - 'typ' header is 'dc+sd-jwt'
// - Collision-resistant names are validated
```

### Testing/Development Mode

For testing or development, use relaxed validation:

```csharp
var validator = new VpTokenValidator(keyProvider, useSdJwtVcValidation: false);
var options = VpTokenValidationOptions.CreateForTesting();

// This disables strict OID4VP validations:
// - No issuer validation
// - No audience validation  
// - No freshness validation
// - Extended time windows
```

### Migration from v1.0

If you're upgrading from v1.0, note these changes:

```csharp
// OLD (v1.0):
var options = new VpTokenValidationOptions
{
    ValidateKeyBindingAudience = false, // Was default
    ValidateKeyBindingFreshness = false, // Not available
};

// NEW (v1.1) - RECOMMENDED:
var options = VpTokenValidationOptions.CreateForOid4Vp("https://verifier.example.com");

// NEW (v1.1) - If you need old behavior:
var options = new VpTokenValidationOptions
{
    ValidateKeyBindingAudience = false,
    ValidateKeyBindingFreshness = false
};
```

## Use Cases

- **Employment Verification**: Bank loan applications requiring job verification
- **Age Verification**: Privacy-preserving age proof for restricted services
- **Cross-Device Flows**: QR code scanning from mobile to desktop
- **Complex Requirements**: Multi-credential presentations for compliance

## Documentation

For comprehensive examples and protocol implementation patterns, see the [main repository](https://github.com/openwallet-foundation-labs/sd-jwt-dotnet).

## License

Licensed under the [Apache License 2.0](https://opensource.org/licenses/Apache-2.0).
