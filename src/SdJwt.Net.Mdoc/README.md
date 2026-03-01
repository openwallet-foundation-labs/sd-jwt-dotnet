# SdJwt.Net.Mdoc - ISO 18013-5 Mobile Document Support

[![NuGet Version](https://img.shields.io/nuget/v/SdJwt.Net.Mdoc.svg)](https://www.nuget.org/packages/SdJwt.Net.Mdoc)
[![License](https://img.shields.io/badge/license-Apache%202.0-blue.svg)](LICENSE)

ISO 18013-5 mDL/mdoc implementation for the SD-JWT .NET ecosystem. Provides credential issuance, presentation, and verification for mobile document credentials with OpenID4VP/OpenID4VCI integration and HAIP compliance.

## Overview

This package provides complete support for ISO 18013-5 mobile documents (mdoc), including:

-   **Mobile Driving License (mDL)**: ISO 18013-5 compliant driver's license credentials
-   **CBOR/COSE Encoding**: Native support for CBOR serialization and COSE signing
-   **Selective Disclosure**: Privacy-preserving attribute sharing for mdoc credentials
-   **OpenID4VP Integration**: Present mdoc credentials via OpenID for Verifiable Presentations
-   **OpenID4VCI Integration**: Issue mdoc credentials via OpenID for Verifiable Credential Issuance
-   **HAIP Compliance**: High Assurance Interoperability Profile support
-   **Proximity Presentation**: QR code, NFC, and BLE-based credential sharing

### Supported Document Types

| Document Type | Standard      | Status     |
| ------------- | ------------- | ---------- |
| **mDL**       | ISO 18013-5   | **Stable** |
| **PID**       | EU ARF (mdoc) | Planned    |
| **mID**       | ISO 23220-3   | Planned    |
| **Custom**    | Generic mdoc  | **Stable** |

## Installation

```bash
dotnet add package SdJwt.Net.Mdoc
```

For complete OpenID4VC functionality:

```bash
dotnet add package SdJwt.Net.Oid4Vci    # Credential Issuance
dotnet add package SdJwt.Net.Oid4Vp     # Credential Presentation
dotnet add package SdJwt.Net.HAIP       # High Assurance Compliance
```

## Quick Start

### Issue a Mobile Driving License (mDL)

```csharp
using SdJwt.Net.Mdoc;
using SdJwt.Net.Mdoc.Issuer;

// Create mdoc issuer with COSE signing
var signingKey = MdocKeyFactory.CreateECDsaKey(ECCurve.NamedCurves.nistP256);
var issuer = new MdocIssuer(signingKey);

// Define mDL claims per ISO 18013-5
var mdlClaims = new MdocClaims
{
    DocType = "org.iso.18013.5.1.mDL",
    Namespace = "org.iso.18013.5.1",
    Claims = new Dictionary<string, object>
    {
        ["family_name"] = "Doe",
        ["given_name"] = "Jane",
        ["birth_date"] = "1990-01-15",
        ["issue_date"] = "2023-01-01",
        ["expiry_date"] = "2028-01-01",
        ["issuing_country"] = "US",
        ["issuing_authority"] = "State of California DMV",
        ["document_number"] = "DL12345678",
        ["portrait"] = Convert.FromBase64String("..."), // Photo
        ["driving_privileges"] = new[]
        {
            new { vehicle_category_code = "A", issue_date = "2023-01-01" },
            new { vehicle_category_code = "B", issue_date = "2023-01-01" }
        }
    }
};

// Issue CBOR-encoded mdoc
var mdoc = await issuer.IssueAsync(mdlClaims);
Console.WriteLine($"mDL issued: {mdoc.Length} bytes (CBOR)");
```

### Verify and Parse an mdoc

```csharp
using SdJwt.Net.Mdoc.Verifier;

// Create mdoc verifier
var keyResolver = new TrustListKeyResolver();
var verifier = new MdocVerifier(keyResolver);

// Verify mdoc signature and validity
var verificationResult = await verifier.VerifyAsync(receivedMdoc);

if (verificationResult.IsValid)
{
    var parsedMdoc = verificationResult.ParsedDocument;

    Console.WriteLine($"Document Type: {parsedMdoc.DocType}");
    Console.WriteLine($"Issuer: {parsedMdoc.IssuerAuth.Issuer}");
    Console.WriteLine($"Valid Until: {parsedMdoc.ValidityInfo.ValidUntil}");

    // Access disclosed claims
    var familyName = parsedMdoc.Claims["family_name"];
    var birthDate = parsedMdoc.Claims["birth_date"];

    Console.WriteLine($"Holder: {familyName}, DOB: {birthDate}");
}
else
{
    Console.WriteLine($"Invalid mdoc: {verificationResult.ErrorDescription}");
}
```

### Selective Disclosure with mdoc

```csharp
using SdJwt.Net.Mdoc.Holder;

// Create mdoc holder
var holder = new MdocHolder(storedMdoc);

// Create presentation with selective disclosure
var presentationRequest = new MdocPresentationRequest
{
    DocType = "org.iso.18013.5.1.mDL",
    Namespace = "org.iso.18013.5.1",
    RequestedElements = new[]
    {
        "family_name",
        "given_name",
        "birth_date",
        "portrait",
        "age_over_21"  // Derived attribute
    }
};

// Generate device response (CBOR-encoded)
var deviceResponse = await holder.CreatePresentationAsync(
    presentationRequest,
    sessionTranscript: GenerateSessionTranscript(),
    readerPublicKey: verifierPublicKey
);

Console.WriteLine($"Presentation created: {deviceResponse.Length} bytes");
// Only requested attributes are disclosed
```

## OpenID4VP Integration

### Present mdoc via OpenID4VP

```csharp
using SdJwt.Net.Oid4Vp;
using SdJwt.Net.Mdoc.Oid4Vp;

// Parse OpenID4VP authorization request
var vpRequest = await Oid4VpRequestParser.ParseAsync(authorizationRequestUri);

// Create mdoc presentation response
var mdocAdapter = new MdocOid4VpAdapter();
var vpResponse = await mdocAdapter.CreateResponseAsync(
    vpRequest,
    holder: mdocHolder,
    selectiveClaims: new[] { "family_name", "birth_date", "age_over_18" }
);

// Submit to verifier
await vpRequest.SubmitResponseAsync(vpResponse);
```

## OpenID4VCI Integration

### Receive mdoc via OpenID4VCI

```csharp
using SdJwt.Net.Oid4Vci;
using SdJwt.Net.Mdoc.Oid4Vci;

// Process credential offer
var offer = await Oid4VciOfferParser.ParseAsync(credentialOfferUri);

// Request mdoc mDL credential
var mdocAdapter = new MdocOid4VciAdapter();
var issuedMdoc = await mdocAdapter.RequestCredentialAsync(
    offer,
    credentialType: "org.iso.18013.5.1.mDL",
    holderKeyPair: holdingKey,
    pinCode: userPin
);

// Store in wallet
await wallet.StoreMdocAsync(issuedMdoc);
```

## Proximity Presentation

### QR Code Engagement

```csharp
using SdJwt.Net.Mdoc.Proximity;

// Start proximity presentation (QR code)
var proximityService = new ProximityPresentationService();
var engagement = await proximityService.StartQrEngagementAsync();

Console.WriteLine($"Display QR code: {engagement.QrCodeData}");

// Wait for reader connection
var request = await proximityService.ReceiveRequestAsync();

Console.WriteLine($"Reader requests: {string.Join(", ", request.RequestedElements)}");

// User consent
if (await GetUserConsent(request.RequestedElements))
{
    // Generate and send device response
    await proximityService.SendResponseAsync(
        request,
        disclosedElements: selectedElements,
        signingKey: deviceKey
    );

    Console.WriteLine("Presentation completed successfully");
}
```

### NFC Presentation

```csharp
// Enable NFC engagement (platform-specific)
await proximityService.EnableNfcEngagementAsync();

Console.WriteLine("NFC enabled - tap reader to begin");

// Handling is the same as QR code after engagement
var request = await proximityService.ReceiveRequestAsync();
// ... respond as above
```

## HAIP Compliance

### Enforce HAIP Level 2 for Government mDL

```csharp
using SdJwt.Net.HAIP;

// Create HAIP-compliant mdoc issuer
var haipValidator = new HaipCryptoValidator(HaipLevel.Level2_VeryHigh, logger);

// Validate signing algorithm
if (!haipValidator.ValidateAlgorithm("ES384"))
{
    throw new InvalidOperationException("ES384 required for Level 2 HAIP");
}

// Use stronger key for Level 2
var haipKey = MdocKeyFactory.CreateECDsaKey(ECCurve.NamedCurves.nistP384);
var haipIssuer = new MdocIssuer(haipKey, algorithm: "ES384");

// Issue HAIP-compliant mDL
var haipMdl = await haipIssuer.IssueAsync(mdlClaims);
```

## Features

### CBOR/COSE Support

-   **CBOR Serialization**: Efficient binary encoding per CBOR specification
-   **COSE Signing**: Cryptographic signing with COSE_Sign1 structure
-   **Tag Handling**: Proper CBOR tag processing for dates, binary data, and custom types
-   **Deterministic Encoding**: Reproducible CBOR output for signature verification

### Security Features

-   **Device Binding**: Cryptographic binding to holder's device key
-   **Session Transcript**: Prevents replay attacks in proximity scenarios
-   **Reader Authentication**: Validates reader certificate and permissions
-   **Age Verification**: Privacy-preserving age-over-X derived attributes
-   **Revocation**: Integration with Status List for credential lifecycle management

### Privacy Features

-   **Selective Disclosure**: Share only requested attributes
-   **Derived Attributes**: Prove properties without revealing underlying data (e.g., age_over_21)
-   **Unlinkability**: Different presentations cannot be correlated
-   **Minimal Disclosure**: Request only what is needed per privacy-by-design

## Advanced Usage

### Device Engagement and Session Establishment

```csharp
// Generate device engagement with mdoc holder information
var deviceEngagement = new DeviceEngagement
{
    Version = "1.0",
    Security = new Security
    {
        CipherSuites = new[] { 1 }, // ECDHE with AES-GCM
        DeviceKey = holderPublicKey
    },
    DeviceRetrievalMethods = new[]
    {
        new DeviceRetrievalMethod
        {
            Type = RetrievalType.BLE,
            Version = 1,
            RetrievalOptions = new { ble_peripheral_mode = true }
        }
    }
};

var qrCode = deviceEngagement.ToQrCodeData();
```

### Custom mdoc Document Types

```csharp
// Define custom mdoc type (e.g., employee badge)
var employeeBadge = new MdocClaims
{
    DocType = "com.example.employee.1",
    Namespace = "com.example.employee",
    Claims = new Dictionary<string, object>
    {
        ["employee_id"] = "E12345",
        ["full_name"] = "Jane Doe",
        ["department"] = "Engineering",
        ["clearance_level"] = "Secret",
        ["badge_photo"] = photoBytes,
        ["valid_from"] = "2024-01-01",
        ["valid_until"] = "2025-01-01"
    }
};

var employeeCredential = await issuer.IssueAsync(employeeBadge);
```

### Batch Issuance

```csharp
// Issue multiple mdocs efficiently
var mdocBatch = new[]
{
    mdlClaims,
    passportClaims,
    healthCardClaims
};

var issuedDocs = await issuer.IssueBatchAsync(mdocBatch);

Console.WriteLine($"Issued {issuedDocs.Count} documents");
```

## Platform Support

-   **.NET 8, 9, 10**: Full support with latest optimizations
-   **.NET Standard 2.1**: Cross-platform compatibility
-   **Windows**: Full proximity (QR, NFC, BLE) support
-   **Linux**: QR code and BLE support
-   **macOS**: QR code and BLE support
-   **iOS/Android**: Via MAUI with native proximity APIs

## Specifications

This package implements:

-   [ISO/IEC 18013-5:2021](https://www.iso.org/standard/69084.html) - Mobile driving licence (mDL)
-   [ISO/IEC 18013-7](https://www.iso.org/standard/82772.html) - mDL add-on functions
-   [RFC 8949](https://datatracker.ietf.org/doc/html/rfc8949) - CBOR
-   [RFC 9052](https://datatracker.ietf.org/doc/html/rfc9052) - COSE
-   [OpenID4VP 1.0](https://openid.net/specs/openid-4-verifiable-presentations-1_0.html) - mdoc integration
-   [OpenID4VCI 1.0](https://openid.net/specs/openid-4-verifiable-credential-issuance-1_0.html) - mdoc issuance

## Related Packages

-   [SdJwt.Net](../SdJwt.Net/) - Core SD-JWT implementation (alternative format)
-   [SdJwt.Net.Oid4Vci](../SdJwt.Net.Oid4Vci/) - Credential issuance protocol
-   [SdJwt.Net.Oid4Vp](../SdJwt.Net.Oid4Vp/) - Presentation protocol
-   [SdJwt.Net.HAIP](../SdJwt.Net.HAIP/) - High Assurance security levels
-   [SdJwt.Net.Wallet](../SdJwt.Net.Wallet/) - Wallet infrastructure for mdoc storage
-   [SdJwt.Net.Eudiw](../SdJwt.Net.Eudiw/) - EU Digital Identity Wallet (supports mdoc)

## Contributing

See [CONTRIBUTING.md](../../CONTRIBUTING.md) for development guidelines.

## License

Apache 2.0 - See [LICENSE.txt](../../LICENSE.txt)

## Support

-   **Issues**: [GitHub Issues](https://github.com/openwallet-foundation-labs/sd-jwt-dotnet/issues)
-   **Discussions**: [GitHub Discussions](https://github.com/openwallet-foundation-labs/sd-jwt-dotnet/discussions)
-   **Documentation**: [https://openwallet-foundation-labs.github.io/sd-jwt-dotnet/](https://openwallet-foundation-labs.github.io/sd-jwt-dotnet/)
