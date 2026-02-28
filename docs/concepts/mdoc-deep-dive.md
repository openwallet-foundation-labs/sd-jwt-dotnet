# mdoc Deep Dive (ISO 18013-5)

This document explains mdoc fundamentals in beginner-friendly terms and maps each concept to the implementation in this repository.

## Prerequisites

Before diving into mdoc, you should understand these foundational concepts:

### What is CBOR?

**CBOR (Concise Binary Object Representation)** is a binary data format similar to JSON but more compact. It is defined in RFC 8949 and used extensively in IoT, mobile credentials, and constrained environments.

```text
JSON: {"name": "Alice", "age": 30}     (29 bytes)
CBOR: A2 64 6E 61 6D 65 65 41 6C 69 63 65 63 61 67 65 18 1E  (18 bytes)
```

Key differences from JSON:

- Binary format (not human-readable)
- Supports more data types (byte strings, tags, undefined)
- More compact, faster to parse
- Used by ISO 18013-5 for all mdoc structures

### What is COSE?

**COSE (CBOR Object Signing and Encryption)** is to CBOR what JOSE/JWS is to JSON. It provides cryptographic operations (signing, encryption, MAC) for CBOR data structures. Defined in RFC 8152.

| JOSE/JWT                  | COSE/mdoc    |
| ------------------------- | ------------ |
| JWS (JSON Web Signature)  | COSE_Sign1   |
| JWK (JSON Web Key)        | COSE_Key     |
| JWE (JSON Web Encryption) | COSE_Encrypt |

### The Problem mdoc Solves

Traditional digital identity systems have several challenges:

1. **Size**: JSON + Base64 encoding is verbose for mobile/NFC
2. **Offline**: Must work without network connectivity
3. **Privacy**: Credentials often over-disclose information
4. **Proximity**: Need secure face-to-face verification

mdoc (mobile document) addresses all these with a compact, privacy-preserving, offline-capable credential format optimized for mobile devices.

## Why mdoc Exists

The ISO 18013-5 standard was developed to enable mobile driving licenses (mDL) - a digital equivalent of the physical driving license stored on a smartphone.

### Key Use Cases

- **TSA checkpoints**: Present mDL via NFC to reader
- **Traffic stops**: Officer verifies license via Bluetooth
- **Age verification**: Prove over 21 without revealing birthdate
- **Car rental**: Present license with selective disclosure
- **International travel**: Cross-border identity verification

### Comparison with SD-JWT VC

| Feature              | SD-JWT VC     | mdoc            |
| -------------------- | ------------- | --------------- |
| Format               | JSON + Base64 | CBOR (binary)   |
| Size                 | Larger        | Compact         |
| Offline              | Limited       | Full support    |
| NFC/BLE              | Complex       | Native design   |
| Selective Disclosure | At issuance   | At presentation |
| Standard             | IETF RFC 9901 | ISO 18013-5     |
| HAIP                 | Supported     | Supported       |

Both formats are supported by OpenID4VP and OpenID4VCI, making them complementary.

## Glossary of Key Terms

| Term                   | Definition                                                     |
| ---------------------- | -------------------------------------------------------------- |
| **mdoc**               | Mobile document - the credential format defined by ISO 18013-5 |
| **mDL**                | Mobile Driving License - the primary use case for mdoc         |
| **MSO**                | Mobile Security Object - contains digests of all claims        |
| **IssuerSigned**       | Issuer-generated portion of the credential                     |
| **DeviceSigned**       | Holder-generated data during presentation                      |
| **NameSpace**          | Logical grouping of data elements (claims)                     |
| **DocType**            | Unique identifier for credential type                          |
| **COSE_Sign1**         | Single-signer COSE signature structure                         |
| **COSE_Key**           | CBOR-encoded cryptographic key                                 |
| **Device Engagement**  | Protocol for establishing device connection                    |
| **Session Transcript** | CBOR binding between request and response                      |

## mdoc Artifact Structure

### Visual Overview

An mdoc document has this hierarchical structure:

```text
Document
  DocType: "org.iso.18013.5.1.mDL"
  IssuerSigned
     NameSpaces: Map<string, Array<IssuerSignedItem>>
        "org.iso.18013.5.1"
           IssuerSignedItem { digestID, random, elementIdentifier, elementValue }
           IssuerSignedItem { digestID, random, elementIdentifier, elementValue }
           ...
     IssuerAuth: COSE_Sign1
        protected: { alg: ES256 }
        payload: MobileSecurityObject
        signature: bytes
  DeviceSigned (optional, for presentations)
     NameSpaces: DeviceNameSpaces
     DeviceAuth: DeviceAuthentication
```

### Mobile Security Object (MSO)

The MSO is the heart of mdoc security. It contains:

```text
MobileSecurityObject
  version: "1.0"
  digestAlgorithm: "SHA-256"
  docType: "org.iso.18013.5.1.mDL"
  valueDigests: Map<namespace, Map<digestID, digest>>
     "org.iso.18013.5.1"
        0: SHA256(IssuerSignedItem[0])
        1: SHA256(IssuerSignedItem[1])
        ...
  deviceKeyInfo
     deviceKey: COSE_Key (holder's public key)
  validityInfo
     signed: datetime
     validFrom: datetime
     validUntil: datetime
```

The MSO is signed by the issuer. This signature covers the **digests** of all data elements, not the values themselves - enabling selective disclosure at presentation time.

### Namespace Structure

Data elements are organized into namespaces:

| Namespace                 | DocType                 | Description            |
| ------------------------- | ----------------------- | ---------------------- |
| `org.iso.18013.5.1`       | `org.iso.18013.5.1.mDL` | Standard mDL elements  |
| `org.iso.18013.5.1.aamva` | `org.iso.18013.5.1.mDL` | US AAMVA extensions    |
| `gov.national.id.1`       | `gov.national.id.1`     | National ID example    |
| Custom namespace          | Custom DocType          | Enterprise credentials |

## How mdoc Selective Disclosure Works

### Architectural Difference from SD-JWT

**SD-JWT**: Selective disclosure is decided at **issuance time**. The issuer creates disclosures, and the holder chooses which to reveal.

**mdoc**: Selective disclosure happens at **presentation time**. All data elements are issued, but the holder chooses which namespaces and elements to include in the DeviceResponse.

### Step-by-Step Example

#### 1. Issuance: All Data Elements Included

```csharp
var mdoc = await new MdocIssuerBuilder()
    .WithDocType("org.iso.18013.5.1.mDL")
    .WithIssuerKey(issuerKey)
    .WithDeviceKey(deviceKey)
    // ALL these elements are included
    .AddMdlElement(MdlDataElement.FamilyName, "Johnson")
    .AddMdlElement(MdlDataElement.GivenName, "Alice")
    .AddMdlElement(MdlDataElement.BirthDate, "1995-07-22")
    .AddMdlElement(MdlDataElement.DocumentNumber, "D1234567")
    .AddMdlElement(MdlDataElement.AgeOver21, true)
    .AddMdlElement(MdlDataElement.ResidentAddress, "123 Main St")
    .BuildAsync(cryptoProvider);
```

#### 2. MSO Contains All Digests

The MSO includes digests for every data element:

```text
valueDigests:
  "org.iso.18013.5.1":
    0: SHA256(family_name item) = "abc123..."
    1: SHA256(given_name item) = "def456..."
    2: SHA256(birth_date item) = "ghi789..."
    3: SHA256(document_number item) = "jkl012..."
    4: SHA256(age_over_21 item) = "mno345..."
    5: SHA256(resident_address item) = "pqr678..."
```

#### 3. Presentation: Holder Selects Elements

For age verification, holder only includes `age_over_21`:

```csharp
// Holder creates selective presentation
var presentation = new Document
{
    DocType = mdoc.DocType,
    IssuerSigned = new IssuerSigned
    {
        NameSpaces = new Dictionary<string, List<IssuerSignedItem>>
        {
            ["org.iso.18013.5.1"] = new()
            {
                // Only include age_over_21
                mdoc.IssuerSigned.NameSpaces["org.iso.18013.5.1"]
                    .First(i => i.ElementIdentifier == "age_over_21")
            }
        },
        IssuerAuth = mdoc.IssuerSigned.IssuerAuth // Same signature!
    }
};
```

#### 4. Verification: Digest Matching

The verifier:

1. Extracts MSO from IssuerAuth
2. Verifies issuer signature over MSO
3. For each presented IssuerSignedItem:
   - Computes `SHA256(item.ToCbor())`
   - Checks digest exists in MSO's valueDigests
4. Validates the MSO's deviceKey matches (if DeviceSigned present)

```csharp
var verifier = new MdocVerifier();
var result = verifier.Verify(presentation, new MdocVerificationOptions
{
    ExpectedDocType = "org.iso.18013.5.1.mDL",
    ValidateExpiry = true
});

// result.VerifiedClaims contains only:
//   age_over_21: true
// All other claims are NOT visible to verifier
```

## End-to-End Lifecycle

### Phase 1: Issuance

The issuer (DMV) creates the mdoc credential:

```csharp
using System.Security.Cryptography;
using SdJwt.Net.Mdoc.Cose;
using SdJwt.Net.Mdoc.Issuer;
using SdJwt.Net.Mdoc.Namespaces;

public class DmvIssuanceService
{
    private readonly ECDsa _issuerKey;
    private readonly ICoseCryptoProvider _crypto;

    public DmvIssuanceService(ECDsa issuerKey)
    {
        _issuerKey = issuerKey;
        _crypto = new DefaultCoseCryptoProvider();
    }

    public async Task<byte[]> IssueMdlAsync(
        DriverApplication application,
        byte[] devicePublicKeyCbor)
    {
        var deviceKey = CoseKey.FromCbor(devicePublicKeyCbor);
        var issuerKey = CoseKey.FromECDsa(_issuerKey);

        var mdoc = await new MdocIssuerBuilder()
            .WithDocType(MdlNamespace.DocType)
            .WithIssuerKey(issuerKey)
            .WithDeviceKey(deviceKey)
            .WithAlgorithm(CoseAlgorithm.ES256)
            // Mandatory elements
            .AddMdlElement(MdlDataElement.FamilyName, application.FamilyName)
            .AddMdlElement(MdlDataElement.GivenName, application.GivenName)
            .AddMdlElement(MdlDataElement.BirthDate, application.BirthDate)
            .AddMdlElement(MdlDataElement.IssueDate, DateTime.UtcNow.ToString("yyyy-MM-dd"))
            .AddMdlElement(MdlDataElement.ExpiryDate, DateTime.UtcNow.AddYears(5).ToString("yyyy-MM-dd"))
            .AddMdlElement(MdlDataElement.IssuingCountry, "US")
            .AddMdlElement(MdlDataElement.IssuingAuthority, "State DMV")
            .AddMdlElement(MdlDataElement.DocumentNumber, application.DocumentNumber)
            // Age verification flags (computed from birthdate)
            .AddMdlElement(MdlDataElement.AgeOver18, application.IsOver18)
            .AddMdlElement(MdlDataElement.AgeOver21, application.IsOver21)
            // Optional
            .AddMdlElement(MdlDataElement.Portrait, application.Photo)
            .AddMdlElement(MdlDataElement.DrivingPrivileges, application.Privileges)
            .WithValidity(
                DateTimeOffset.UtcNow,
                DateTimeOffset.UtcNow.AddYears(5))
            .BuildAsync(_crypto);

        return mdoc.ToCbor();
    }
}
```

### Phase 2: Holder Presentation via OpenID4VP

```csharp
using SdJwt.Net.Mdoc.Handover;
using SdJwt.Net.Mdoc.Models;

public class WalletPresentationService
{
    public byte[] CreatePresentation(
        Document mdoc,
        string[] requestedElements,
        string verifierClientId,
        string nonce,
        string responseUri)
    {
        // Create session transcript for OID4VP binding
        var transcript = SessionTranscript.ForOpenId4Vp(
            clientId: verifierClientId,
            nonce: nonce,
            mdocGeneratedNonce: null,
            responseUri: responseUri);

        // Filter to only requested elements
        var filteredNamespaces = new Dictionary<string, List<IssuerSignedItem>>();
        foreach (var ns in mdoc.IssuerSigned.NameSpaces)
        {
            var filtered = ns.Value
                .Where(item => requestedElements.Contains(item.ElementIdentifier))
                .ToList();

            if (filtered.Any())
                filteredNamespaces[ns.Key] = filtered;
        }

        // Create selective document
        var presentation = new Document
        {
            DocType = mdoc.DocType,
            IssuerSigned = new IssuerSigned
            {
                NameSpaces = filteredNamespaces,
                IssuerAuth = mdoc.IssuerSigned.IssuerAuth
            }
        };

        // Wrap in DeviceResponse
        var response = new DeviceResponse
        {
            Version = "1.0",
            Documents = new List<Document> { presentation },
            Status = 0
        };

        return response.ToCbor();
    }
}
```

### Phase 3: Verifier Validation

```csharp
using SdJwt.Net.Mdoc.Verifier;

public class VerifierService
{
    private readonly MdocVerifier _verifier = new();

    public VerificationResult VerifyPresentation(
        byte[] presentationBytes,
        string expectedDocType,
        string[] requiredElements)
    {
        // Parse device response
        var response = DeviceResponse.FromCbor(presentationBytes);

        if (response.Status != 0)
            return VerificationResult.Failed($"Device error: {response.Status}");

        foreach (var doc in response.Documents)
        {
            // Verify each document
            var options = new MdocVerificationOptions
            {
                ValidateExpiry = true,
                ExpectedDocType = expectedDocType,
                RequiredElements = requiredElements
            };

            var result = _verifier.Verify(doc, options);

            if (!result.IsValid)
                return VerificationResult.Failed(result.Error);

            // Access verified claims
            foreach (var claim in result.VerifiedClaims)
            {
                Console.WriteLine($"Verified: {claim.Key} = {claim.Value}");
            }
        }

        return VerificationResult.Success();
    }
}
```

## Session Transcript and Handover

### Why Session Binding Matters

Without session binding, an attacker could:

1. Intercept a valid mdoc presentation
2. Replay it to another verifier
3. Impersonate the legitimate holder

Session transcript binds the presentation to:

- The specific verifier (audience)
- The specific session (nonce)
- The specific response URI

### OpenID4VP Handover Types

#### 1. Redirect Flow (Same-Device or Cross-Device)

```csharp
var transcript = SessionTranscript.ForOpenId4Vp(
    clientId: "https://verifier.example.com",
    nonce: "session-unique-nonce",
    mdocGeneratedNonce: null,  // Optional device nonce
    responseUri: "https://verifier.example.com/callback");
```

The handover OID4VPHandover is:

```text
[
  clientId,        // "https://verifier.example.com"
  responseUri,     // "https://verifier.example.com/callback"
  nonce,           // "session-unique-nonce"
  mdocNonce        // null or device-generated
]
```

#### 2. DC API Flow (Browser-Based)

```csharp
var transcript = SessionTranscript.ForOpenId4VpDcApi(
    origin: "https://verifier.example.com",
    nonce: "browser-session-nonce",
    clientId: null);  // Defaults to origin
```

The handover OID4VPDcApiHandover is:

```text
[
  "OID4VPDCAPIHandover",  // Fixed tag
  origin,                  // "https://verifier.example.com"
  SHA256(nonce)           // Hash of nonce
]
```

### Session Transcript Structure

```text
SessionTranscript = [
  DeviceEngagementBytes,  // null for OID4VP
  EReaderKeyBytes,        // null for OID4VP
  Handover                // OID4VPHandover or OID4VPDCAPIHandover
]
```

## COSE Cryptography

### Supported Algorithms

| Algorithm | COSE ID | Curve | Security Level |
| --------- | ------- | ----- | -------------- |
| ES256     | -7      | P-256 | HAIP Level 1   |
| ES384     | -35     | P-384 | HAIP Level 2   |
| ES512     | -36     | P-521 | HAIP Level 3   |

### COSE_Key Structure

```csharp
var key = new CoseKey
{
    KeyType = CoseKeyTypes.Ec2,  // 2 = EC2
    Curve = CoseCurves.P256,     // 1 = P-256
    X = xCoordinate,              // 32 bytes
    Y = yCoordinate,              // 32 bytes
    D = privateKey                // 32 bytes (optional, private only)
};

// Serialize
byte[] keyCbor = key.ToCbor();

// Deserialize
var restored = CoseKey.FromCbor(keyCbor);
```

### COSE_Sign1 Structure

```text
COSE_Sign1 = [
  protected: bstr,   // CBOR-encoded { alg: -7 }
  unprotected: {},   // Empty map
  payload: bstr,     // MSO bytes
  signature: bstr    // ECDSA signature
]
```

## Implementation Guide

### Package Structure

```text
SdJwt.Net.Mdoc/
  Cbor/
     ICborSerializable.cs     # Interface for CBOR serialization
     CborUtils.cs             # Helper methods
  Cose/
     CoseAlgorithm.cs         # ES256, ES384, ES512
     CoseKey.cs               # Key representation
     CoseSign1.cs             # Signature structure
     ICoseCryptoProvider.cs   # Abstraction for crypto
     DefaultCoseCryptoProvider.cs  # Default implementation
  Models/
     MobileSecurityObject.cs  # MSO structure
     ValidityInfo.cs          # Validity timestamps
     DigestIdMapping.cs       # Digest mappings
     IssuerSigned.cs          # Issuer data
     Document.cs              # Complete mdoc
     DeviceResponse.cs        # Presentation response
  Issuer/
     MdocIssuer.cs            # Core issuance
     MdocIssuerBuilder.cs     # Fluent API
     MdocIssuerOptions.cs     # Configuration
  Verifier/
     MdocVerifier.cs          # Verification
     MdocVerificationOptions.cs
     MdocVerificationResult.cs
  Handover/
     SessionTranscript.cs     # Session binding
     OpenId4VpHandover.cs     # Redirect flow
     OpenId4VpDcApiHandover.cs  # DC API flow
  Namespaces/
     MdlDataElement.cs        # mDL element enum
     MdlNamespace.cs          # mDL namespace constants
```

### Key Classes

| Class               | Purpose                            |
| ------------------- | ---------------------------------- |
| `MdocIssuerBuilder` | Fluent API for credential creation |
| `MdocVerifier`      | Verification of mdoc presentations |
| `CoseKey`           | COSE key operations                |
| `SessionTranscript` | Session binding for OID4VP         |
| `Document`          | Complete mdoc credential           |
| `DeviceResponse`    | Presentation response container    |

## Security Considerations

### Cryptographic Requirements

1. **Algorithm Strength**: Only HAIP-approved algorithms (ES256+)
2. **Digest Algorithm**: SHA-256, SHA-384, or SHA-512 only
3. **No MD5/SHA-1**: Blocked by HAIP validator

### Replay Attack Prevention

```csharp
// Verifier generates unique nonce per request
var nonce = GenerateCryptographicNonce();

// Include nonce in authorization request
var request = new AuthorizationRequest
{
    Nonce = nonce,
    // ...
};

// Verify nonce in response
if (transcript.Nonce != expectedNonce)
    throw new SecurityException("Nonce mismatch - possible replay attack");
```

### Device Binding

The device key in MSO ensures only the legitimate holder can present:

```text
MSO.deviceKeyInfo.deviceKey = holder's public key

// During presentation, holder proves possession by:
// 1. Creating DeviceSigned with signature using device private key
// 2. Or through session transcript binding in OID4VP
```

## Related Resources

- [Hello mdoc Tutorial](../tutorials/beginner/05-hello-mdoc.md) - Getting started
- [mdoc Issuance Tutorial](../tutorials/intermediate/06-mdoc-issuance.md) - Credential creation
- [mdoc OpenID4VP Integration](../tutorials/advanced/05-mdoc-integration.md) - Presentation flows
- [mdoc Identity Verification Use Case](../use-cases/mdoc-identity-verification.md) - Real-world scenarios
- [ISO 18013-5 Specification](https://www.iso.org/standard/69084.html) - Official standard
