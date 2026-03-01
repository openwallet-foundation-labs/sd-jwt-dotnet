# SdJwt.Net.Eudiw

EU Digital Identity Wallet (EUDIW) profile implementation for the SD-JWT .NET ecosystem.

## Overview

This package provides ready-to-use configuration and validation for EUDIW compliance under eIDAS 2.0 regulation:

-   **ARF Compliance**: Architecture Reference Framework validation
-   **EU Trust Lists**: LOTL integration and issuer trust validation
-   **PID Handling**: Person Identification Data credential processing
-   **QEAA/EAA**: Qualified and non-qualified attestation support
-   **RP Registration**: Relying Party registration validation

## Installation

```bash
dotnet add package SdJwt.Net.Eudiw
```

## Quick Start

### Validate ARF Compliance

```csharp
using SdJwt.Net.Eudiw.Arf;

var validator = new ArfProfileValidator();

// Validate algorithm is ARF-compliant
bool isValid = validator.ValidateAlgorithm("ES256"); // true

// Validate credential type
var result = validator.ValidateCredentialType("eu.europa.ec.eudi.pid.1");
if (result.IsValid)
{
    Console.WriteLine($"Credential type: {result.CredentialType}"); // Pid
}
```

### Process PID Credentials

```csharp
using SdJwt.Net.Eudiw.Credentials;

var handler = new PidCredentialHandler();

var claims = new Dictionary<string, object>
{
    ["family_name"] = "Mustermann",
    ["given_name"] = "Erika",
    ["birth_date"] = "1964-08-12",
    ["issuance_date"] = "2024-01-01",
    ["expiry_date"] = "2029-01-01",
    ["issuing_authority"] = "Bundesdruckerei",
    ["issuing_country"] = "DE"
};

var validation = handler.Validate(claims);
if (validation.IsValid)
{
    var credential = handler.ToPidCredential(claims);
    Console.WriteLine($"{credential.GivenName} {credential.FamilyName}");
}
```

### Validate EU Member State

```csharp
using SdJwt.Net.Eudiw.Arf;

var validator = new ArfProfileValidator();

// Check if issuing country is EU member state
bool isEu = validator.ValidateMemberState("DE"); // true
bool isNotEu = validator.ValidateMemberState("US"); // false
```

### Validate RP Registration

```csharp
using SdJwt.Net.Eudiw.RelyingParty;

var validator = new RpRegistrationValidator();

var registration = new RpRegistration
{
    ClientId = "https://rp.example.com",
    OrganizationName = "Example Corp",
    RedirectUris = new[] { "https://rp.example.com/callback" },
    ResponseTypes = new[] { "vp_token" },
    TrustFramework = "eu.eudiw.trust"
};

var result = validator.Validate(registration);
if (result.IsValid)
{
    Console.WriteLine("RP registration is valid");
}
```

## Credential Types

| Type | Format    | Description                                         |
| ---- | --------- | --------------------------------------------------- |
| PID  | mdoc      | Person Identification Data (national ID equivalent) |
| mDL  | mdoc      | Mobile Driving License per ISO 18013-5              |
| QEAA | SD-JWT VC | Qualified attestations (diplomas, certifications)   |
| EAA  | SD-JWT VC | Non-qualified attestations (memberships)            |

## ARF Algorithm Support

The package enforces ARF-mandated cryptographic algorithms:

| Algorithm | Support                 |
| --------- | ----------------------- |
| ES256     | Required (HAIP Level 2) |
| ES384     | Supported               |
| ES512     | Supported               |

## EU Member States

Full support for all 27 EU member states:

AT, BE, BG, CY, CZ, DE, DK, EE, ES, FI, FR, GR, HR, HU, IE, IT, LT, LU, LV, MT, NL, PL, PT, RO, SE, SI, SK

## Related Packages

-   [SdJwt.Net](../SdJwt.Net/) - Core SD-JWT implementation
-   [SdJwt.Net.Mdoc](../SdJwt.Net.Mdoc/) - ISO 18013-5 mdoc support
-   [SdJwt.Net.HAIP](../SdJwt.Net.HAIP/) - High Assurance Interoperability Profile
-   [SdJwt.Net.Oid4Vp](../SdJwt.Net.Oid4Vp/) - OpenID4VP presentation protocol

## License

Apache 2.0 - See [LICENSE](../../LICENSE.txt)
