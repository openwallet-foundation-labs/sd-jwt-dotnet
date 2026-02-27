# SdJwt.Net.Vc - Verifiable Credentials

[![NuGet Version](https://img.shields.io/nuget/v/SdJwt.Net.Vc.svg)](https://www.nuget.org/packages/SdJwt.Net.Vc/)
[![License](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](https://opensource.org/licenses/Apache-2.0)

Implementation of **SD-JWT-based Verifiable Credentials** aligned with [draft-ietf-oauth-sd-jwt-vc-15](https://datatracker.ietf.org/doc/draft-ietf-oauth-sd-jwt-vc/). Provides strongly-typed models and comprehensive validation for verifiable credentials using the SD-JWT format.

## Features

- **Draft 15 Alignment**: Core SD-JWT VC issuance and verification with draft-15 updates
- **Type Safety**: Strongly-typed models for all VC components
- **Media Type Support**: Support for both `dc+sd-jwt` and legacy `vc+sd-jwt` media types
- **Status Integration**: Built-in support for status claims and revocation checking
- **VCT Validation**: Collision-Resistant Name validation for VCT claims

## Installation

```bash
dotnet add package SdJwt.Net.Vc
```

## Quick Start

### Issue a Verifiable Credential

```csharp
using SdJwt.Net.Vc.Issuer;

var vcIssuer = new SdJwtVcIssuer(issuerKey, SecurityAlgorithms.EcdsaSha256);

var vcPayload = new SdJwtVcPayload
{
    Issuer = "https://university.example.edu",
    Subject = "did:example:student123",
    IssuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
    ExpiresAt = DateTimeOffset.UtcNow.AddYears(4).ToUnixTimeSeconds(),
    AdditionalData = new Dictionary<string, object>
    {
        ["degree"] = "Bachelor of Science",
        ["major"] = "Computer Science",
        ["gpa"] = 3.8,
        ["graduation_date"] = "2024-06-15"
    }
};

var credential = vcIssuer.Issue(
    "https://university.edu/credentials/degree", 
    vcPayload, 
    sdOptions, 
    holderJwk);
```

### Verify a Verifiable Credential

```csharp
using SdJwt.Net.Vc.Verifier;

var vcVerifier = new SdJwtVcVerifier(keyResolver);
var expectedNonce = "nonce-123";

var result = await vcVerifier.VerifyAsync(
    presentation,
    validationParams,
    kbJwtValidationParameters: kbParams,
    expectedKbJwtNonce: expectedNonce,
    expectedVctType: "https://university.edu/credentials/degree");

var verifiedClaims = result.ClaimsPrincipal;
var verifiedVct = result.VctType;
```

## Real-World Examples

- **Medical Licenses**: Healthcare professional credentials
- **University Degrees**: Academic achievement verification
- **Employment Records**: Job position and verification workflows
- **Government IDs**: Citizen identity credentials

## Documentation

For comprehensive examples and integration patterns, see the [main repository](https://github.com/openwallet-foundation-labs/sd-jwt-dotnet).

## License

Licensed under the [Apache License 2.0](https://opensource.org/licenses/Apache-2.0).
