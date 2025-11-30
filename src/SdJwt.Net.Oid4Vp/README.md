# SdJwt.Net.Oid4Vp - OpenID for Verifiable Presentations

[![NuGet Version](https://img.shields.io/nuget/v/SdJwt.Net.Oid4Vp.svg)](https://www.nuget.org/packages/SdJwt.Net.Oid4Vp/)
[![License](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](https://opensource.org/licenses/Apache-2.0)

Implementation of **OpenID4VP 1.0** specification for verifiable presentation verification. Provides complete protocol support with Presentation Exchange v2.0.0 integration and cross-device flow support.

## Features

- **OID4VP 1.0 Final**: Complete specification implementation  
- **Presentation Exchange v2.0.0**: Full DIF PE integration
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

var presentationRequest = new AuthorizationRequestObject
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

### Process VP Token Response

```csharp
var vpTokenResponse = new AuthorizationResponse
{
    VpToken = new[] { "eyJ0eXAiOiJ2cCtzZC1qd3Q..." },
    PresentationSubmission = new PresentationSubmission
    {
        Id = "submission_123",
        DefinitionId = "employment_verification",
        DescriptorMap = new[]
        {
            new DescriptorMapping
            {
                Id = "employment_credential",
                Format = "vc+sd-jwt",
                Path = "$[0]"
            }
        }
    }
};

// Validate VP token and extract verified claims
var verificationResult = await VerifyVpTokenAsync(vpTokenResponse.VpToken[0]);
```

## Use Cases

- **Employment Verification**: Bank loan applications requiring job verification
- **Age Verification**: Privacy-preserving age proof for restricted services
- **Cross-Device Flows**: QR code scanning from mobile to desktop
- **Complex Requirements**: Multi-credential presentations for compliance

## Documentation

For comprehensive examples and protocol implementation patterns, see the [main repository](https://github.com/openwalletfoundation/sd-jwt-dotnet).

## License

Licensed under the [Apache License 2.0](https://opensource.org/licenses/Apache-2.0).