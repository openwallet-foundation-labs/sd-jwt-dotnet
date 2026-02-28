# Tutorial: Hello mdoc

Create your first ISO 18013-5 mobile document credential in 10 minutes.

**Time:** 10 minutes  
**Level:** Beginner  
**Sample:** `samples/SdJwt.Net.Samples/01-Beginner/05-HelloMdoc.cs`

## What You Will Learn

- How to create an mdoc issuer
- How to build and sign a mobile document credential
- How to understand the CBOR-based document structure

## Prerequisites

- .NET 9.0 SDK installed
- Basic understanding of digital credentials
- Completed [Hello SD-JWT](01-hello-sd-jwt.md)

## Step 1: Install the Package

Add the mdoc package to your project:

```bash
dotnet add package SdJwt.Net.Mdoc
```

## Step 2: Create Cryptographic Keys

Every mdoc system needs two keys: an issuer key (for signing) and a device key (for holder binding):

```csharp
using System.Security.Cryptography;
using SdJwt.Net.Mdoc.Cose;

// Create issuer signing key (P-256 curve - ECDSA)
using var issuerEcdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
var issuerKey = CoseKey.FromECDsa(issuerEcdsa);

// Create device key for holder binding
using var deviceEcdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
var deviceKey = CoseKey.FromECDsa(deviceEcdsa);
```

## Step 3: Create the Issuer Builder

Use `MdocIssuerBuilder` for fluent credential construction:

```csharp
using SdJwt.Net.Mdoc.Issuer;

var builder = new MdocIssuerBuilder()
    .WithDocType("org.iso.18013.5.1.mDL")
    .WithIssuerKey(issuerKey)
    .WithDeviceKey(deviceKey);
```

## Step 4: Add Data Elements

Add claims using the mDL namespace helpers:

```csharp
using SdJwt.Net.Mdoc.Namespaces;

builder
    .AddMdlElement(MdlDataElement.FamilyName, "Doe")
    .AddMdlElement(MdlDataElement.GivenName, "John")
    .AddMdlElement(MdlDataElement.BirthDate, "1990-05-15")
    .AddMdlElement(MdlDataElement.IssueDate, "2024-01-01")
    .AddMdlElement(MdlDataElement.ExpiryDate, "2029-01-01")
    .AddMdlElement(MdlDataElement.IssuingCountry, "US")
    .AddMdlElement(MdlDataElement.IssuingAuthority, "State DMV")
    .AddMdlElement(MdlDataElement.DocumentNumber, "DL123456789");
```

## Step 5: Set Validity and Build

```csharp
using SdJwt.Net.Mdoc.Cose;

var cryptoProvider = new DefaultCoseCryptoProvider();

builder.WithValidity(
    validFrom: DateTimeOffset.UtcNow,
    validUntil: DateTimeOffset.UtcNow.AddYears(5));

var mdoc = await builder.BuildAsync(cryptoProvider);

Console.WriteLine($"Created mdoc with DocType: {mdoc.DocType}");
Console.WriteLine($"Contains {mdoc.IssuerSigned.NameSpaces.Count} namespace(s)");
```

## Understanding the Output

The resulting `Document` contains:

- **DocType**: The credential type identifier (e.g., `org.iso.18013.5.1.mDL`)
- **IssuerSigned**: Namespaced data elements with COSE signature
- **DeviceSigned**: Optional device-generated data (for presentations)

Structure:

```text
Document
  DocType: "org.iso.18013.5.1.mDL"
  IssuerSigned
     NameSpaces: {"org.iso.18013.5.1": [...items]}
     IssuerAuth: COSE_Sign1 (contains MSO)
```

## Complete Example

```csharp
using System.Security.Cryptography;
using SdJwt.Net.Mdoc.Cose;
using SdJwt.Net.Mdoc.Issuer;
using SdJwt.Net.Mdoc.Namespaces;

// Setup keys
using var issuerEcdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
using var deviceEcdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);

var issuerKey = CoseKey.FromECDsa(issuerEcdsa);
var deviceKey = CoseKey.FromECDsa(deviceEcdsa);
var cryptoProvider = new DefaultCoseCryptoProvider();

// Build and issue mdoc
var mdoc = await new MdocIssuerBuilder()
    .WithDocType("org.iso.18013.5.1.mDL")
    .WithIssuerKey(issuerKey)
    .WithDeviceKey(deviceKey)
    .AddMdlElement(MdlDataElement.FamilyName, "Doe")
    .AddMdlElement(MdlDataElement.GivenName, "John")
    .AddMdlElement(MdlDataElement.BirthDate, "1990-05-15")
    .AddMdlElement(MdlDataElement.DocumentNumber, "DL123456")
    .WithValidity(
        DateTimeOffset.UtcNow,
        DateTimeOffset.UtcNow.AddYears(5))
    .BuildAsync(cryptoProvider);

Console.WriteLine($"Created mdoc: {mdoc.DocType}");
```

## Run the Sample

```bash
cd samples/SdJwt.Net.Samples
dotnet run -- 1.5
```

## Next Steps

- [mdoc Issuance](../intermediate/06-mdoc-issuance.md) - Complete credential issuance flows
- [mdoc Deep Dive](../../concepts/mdoc-deep-dive.md) - Understanding ISO 18013-5

## Key Concepts

| Term       | Description                             |
| ---------- | --------------------------------------- |
| mdoc       | Mobile document format per ISO 18013-5  |
| CBOR       | Concise Binary Object Representation    |
| COSE       | CBOR Object Signing and Encryption      |
| MSO        | Mobile Security Object (signed digests) |
| mDL        | Mobile Driving License                  |
| Device Key | Holder's key for proof of possession    |
