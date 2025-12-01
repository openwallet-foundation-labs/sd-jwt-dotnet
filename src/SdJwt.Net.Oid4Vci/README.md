# SdJwt.Net.Oid4Vci - OpenID for Verifiable Credential Issuance

[![NuGet Version](https://img.shields.io/nuget/v/SdJwt.Net.Oid4Vci.svg)](https://www.nuget.org/packages/SdJwt.Net.Oid4Vci/)
[![License](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](https://opensource.org/licenses/Apache-2.0)

Implementation of **OpenID4VCI 1.0** specification for standardized verifiable credential issuance. Provides complete protocol support with transport-agnostic design for any HTTP framework.

## Features

- **OID4VCI 1.0 Final**: Complete specification implementation
- **Multiple Grant Types**: Authorization Code, Pre-authorized Code, custom grants
- **Transport Agnostic**: Pure data models for any HTTP framework
- **Deferred Issuance**: Asynchronous credential delivery support
- **QR Code Integration**: Mobile wallet workflow support

## Installation

```bash
dotnet add package SdJwt.Net.Oid4Vci
```

## Quick Start

### Create Credential Offer

```csharp
using SdJwt.Net.Oid4Vci.Models;

var credentialOffer = new CredentialOffer
{
    CredentialIssuer = "https://university.example.edu",
    CredentialConfigurationIds = new[] { "university_degree" },
    Grants = new GrantsOffered
    {
        PreAuthorizedCode = new PreAuthorizedCodeGrant
        {
            PreAuthorizedCode = "abc123",
            UserPinRequired = true
        }
    }
};

// Generate QR code URL
var qrUrl = $"openid-credential-offer://?credential_offer={Uri.EscapeDataString(JsonSerializer.Serialize(credentialOffer))}";
```

### Process Credential Request

```csharp
var credentialRequest = new CredentialRequest
{
    Format = "vc+sd-jwt",
    CredentialDefinition = new CredentialDefinition
    {
        Type = new[] { "VerifiableCredential", "UniversityDegree" }
    },
    Proof = new ProofOfPossession
    {
        ProofType = "jwt",
        Jwt = holderProofJwt
    }
};

// Validate request and issue credential
var credentialResponse = new CredentialResponse
{
    Credential = issuedCredential,
    Format = "vc+sd-jwt"
};
```

## Advanced Workflows

- **Pre-authorized Code Flow**: University degree issuance with PIN protection
- **Authorization Code Flow**: Government ID issuance with user consent
- **Batch Issuance**: Corporate onboarding with multiple credentials
- **Deferred Issuance**: Manual approval workflows for sensitive credentials

## Documentation

For comprehensive examples and protocol implementation guides, see the [main repository](https://github.com/thomas-tran/sd-jwt-dotnet).

## License

Licensed under the [Apache License 2.0](https://opensource.org/licenses/Apache-2.0).
