# Tutorial: mdoc Credential Issuance

Build production-ready mdoc credentials with namespaces, validity, and custom claims.

**Time:** 20 minutes  
**Level:** Intermediate  
**Sample:** `samples/SdJwt.Net.Samples/02-Intermediate/06-MdocIssuance.cs`

## What You Will Learn

- How to create complete mDL credentials with all required elements
- How to work with custom namespaces
- How to handle COSE key operations
- How to serialize and deserialize mdoc documents

## Prerequisites

- Completed [Hello mdoc](../beginner/05-hello-mdoc.md)
- Understanding of cryptographic key management
- Familiarity with JSON/CBOR concepts

## Complete mDL Issuance

### Step 1: Configure the Issuer

```csharp
using System.Security.Cryptography;
using SdJwt.Net.Mdoc.Cose;
using SdJwt.Net.Mdoc.Issuer;
using SdJwt.Net.Mdoc.Namespaces;

// Production issuance requires proper key management
using var issuerKey = ECDsa.Create(ECCurve.NamedCurves.nistP256);
using var deviceKey = ECDsa.Create(ECCurve.NamedCurves.nistP256);

var cryptoProvider = new DefaultCoseCryptoProvider();
```

### Step 2: Build Complete mDL

```csharp
var mdoc = await new MdocIssuerBuilder()
    .WithDocType(MdlNamespace.DocType) // "org.iso.18013.5.1.mDL"
    .WithIssuerKey(CoseKey.FromECDsa(issuerKey))
    .WithDeviceKey(CoseKey.FromECDsa(deviceKey))
    .WithAlgorithm(CoseAlgorithm.ES256)
    // Required mDL elements
    .AddMdlElement(MdlDataElement.FamilyName, "Johnson")
    .AddMdlElement(MdlDataElement.GivenName, "Alice Marie")
    .AddMdlElement(MdlDataElement.BirthDate, "1995-07-22")
    .AddMdlElement(MdlDataElement.IssueDate, "2024-03-01")
    .AddMdlElement(MdlDataElement.ExpiryDate, "2029-03-01")
    .AddMdlElement(MdlDataElement.IssuingCountry, "US")
    .AddMdlElement(MdlDataElement.IssuingAuthority, "California DMV")
    .AddMdlElement(MdlDataElement.DocumentNumber, "D1234567")
    .AddMdlElement(MdlDataElement.UnDistinguishingSign, "USA")
    // Optional personal data
    .AddMdlElement(MdlDataElement.Sex, "F")
    .AddMdlElement(MdlDataElement.Height, 165)
    .AddMdlElement(MdlDataElement.Weight, 58)
    .AddMdlElement(MdlDataElement.EyeColour, "brown")
    .AddMdlElement(MdlDataElement.HairColour, "black")
    // Address
    .AddMdlElement(MdlDataElement.ResidentAddress, "123 Main Street")
    .AddMdlElement(MdlDataElement.ResidentCity, "Los Angeles")
    .AddMdlElement(MdlDataElement.ResidentState, "CA")
    .AddMdlElement(MdlDataElement.ResidentPostalCode, "90001")
    .AddMdlElement(MdlDataElement.ResidentCountry, "US")
    // Age flags (computed from birth date)
    .AddMdlElement(MdlDataElement.AgeOver18, true)
    .AddMdlElement(MdlDataElement.AgeOver21, true)
    // Validity period
    .WithValidity(
        DateTimeOffset.UtcNow,
        DateTimeOffset.UtcNow.AddYears(5))
    .BuildAsync(cryptoProvider);
```

## Working with Custom Namespaces

### Adding Custom Claims

For credentials beyond mDL, use generic namespaces:

```csharp
var credentialDocType = "org.example.employee.badge.1";
var credentialNamespace = "org.example.employee.1";

var employeeBadge = await new MdocIssuerBuilder()
    .WithDocType(credentialDocType)
    .WithIssuerKey(CoseKey.FromECDsa(issuerKey))
    .WithDeviceKey(CoseKey.FromECDsa(deviceKey))
    // Custom namespace claims
    .AddClaim(credentialNamespace, "employee_id", "EMP-2024-001")
    .AddClaim(credentialNamespace, "full_name", "Alice Johnson")
    .AddClaim(credentialNamespace, "department", "Engineering")
    .AddClaim(credentialNamespace, "clearance_level", 3)
    .AddClaim(credentialNamespace, "building_access", new[] { "HQ", "Lab-A", "Lab-B" })
    .AddClaim(credentialNamespace, "hire_date", "2020-06-15")
    // Access control flags
    .AddClaim(credentialNamespace, "is_manager", true)
    .AddClaim(credentialNamespace, "remote_access_allowed", true)
    .WithValidity(
        DateTimeOffset.UtcNow,
        DateTimeOffset.UtcNow.AddYears(1))
    .BuildAsync(cryptoProvider);
```

### Multiple Namespaces

mdoc credentials can contain multiple namespaces:

```csharp
var credential = await new MdocIssuerBuilder()
    .WithDocType("org.example.multi.credential")
    .WithIssuerKey(CoseKey.FromECDsa(issuerKey))
    .WithDeviceKey(CoseKey.FromECDsa(deviceKey))
    // Primary namespace
    .AddClaim("org.example.identity", "name", "Alice Johnson")
    .AddClaim("org.example.identity", "birth_date", "1995-07-22")
    // Employment namespace
    .AddClaim("org.example.employment", "employer", "TechCorp Inc")
    .AddClaim("org.example.employment", "position", "Senior Engineer")
    // Certification namespace
    .AddClaim("org.example.certs", "iso27001_certified", true)
    .AddClaim("org.example.certs", "security_clearance", "Secret")
    .WithValidity(
        DateTimeOffset.UtcNow,
        DateTimeOffset.UtcNow.AddYears(2))
    .BuildAsync(cryptoProvider);
```

## COSE Key Operations

### Creating Keys from Raw Materials

```csharp
using SdJwt.Net.Mdoc.Cose;

// From existing ECDsa parameters
var parameters = ecdsa.ExportParameters(includePrivateParameters: true);

var coseKey = new CoseKey
{
    KeyType = CoseKeyTypes.Ec2,
    Curve = CoseCurves.P256,
    X = parameters.Q.X!,
    Y = parameters.Q.Y!,
    D = parameters.D // Private key component
};

// Get public key only (for sharing)
var publicCoseKey = coseKey.GetPublicKey();
```

### Key Serialization

```csharp
// Serialize to CBOR bytes
byte[] keyBytes = coseKey.ToCbor();

// Deserialize from CBOR
var restoredKey = CoseKey.FromCbor(keyBytes);

// Convert back to .NET ECDsa
using var restored = restoredKey.ToECDsa();
```

### Algorithm Selection

```csharp
// ES256 (P-256) - Broad compatibility, recommended default
var es256Builder = new MdocIssuerBuilder()
    .WithAlgorithm(CoseAlgorithm.ES256);

// ES384 (P-384) - Higher security
var es384Builder = new MdocIssuerBuilder()
    .WithAlgorithm(CoseAlgorithm.ES384);

// ES512 (P-521) - Maximum security
var es512Builder = new MdocIssuerBuilder()
    .WithAlgorithm(CoseAlgorithm.ES512);
```

## Document Serialization

### Serialize Complete Document

```csharp
// Serialize document to CBOR
byte[] documentBytes = mdoc.ToCbor();
Console.WriteLine($"Document size: {documentBytes.Length} bytes");

// Deserialize document
using SdJwt.Net.Mdoc.Models;
var restored = Document.FromCbor(documentBytes);
```

### Access Document Components

```csharp
// Get document type
Console.WriteLine($"DocType: {mdoc.DocType}");

// Access issuer-signed data
var issuerSigned = mdoc.IssuerSigned;

// List namespaces
foreach (var ns in issuerSigned.NameSpaces)
{
    Console.WriteLine($"Namespace: {ns.Key}");
    foreach (var item in ns.Value)
    {
        Console.WriteLine($"  {item.ElementIdentifier}: {item.ElementValue}");
    }
}
```

## Complete Example: DMV Issuer Service

```csharp
using System.Security.Cryptography;
using SdJwt.Net.Mdoc.Cose;
using SdJwt.Net.Mdoc.Issuer;
using SdJwt.Net.Mdoc.Namespaces;

public class DmvMdlService
{
    private readonly ECDsa _issuerKey;
    private readonly ICoseCryptoProvider _crypto;

    public DmvMdlService(ECDsa issuerKey)
    {
        _issuerKey = issuerKey;
        _crypto = new DefaultCoseCryptoProvider();
    }

    public async Task<byte[]> IssueMdlAsync(
        string familyName,
        string givenName,
        DateTime birthDate,
        string documentNumber,
        byte[] devicePublicKey)
    {
        // Parse device key from wallet
        var deviceKey = CoseKey.FromCbor(devicePublicKey);

        // Calculate age flags
        var age = DateTime.UtcNow.Year - birthDate.Year;
        if (birthDate.Date > DateTime.UtcNow.AddYears(-age)) age--;

        // Issue credential
        var mdoc = await new MdocIssuerBuilder()
            .WithDocType(MdlNamespace.DocType)
            .WithIssuerKey(CoseKey.FromECDsa(_issuerKey))
            .WithDeviceKey(deviceKey)
            .WithAlgorithm(CoseAlgorithm.ES256)
            .AddMdlElement(MdlDataElement.FamilyName, familyName)
            .AddMdlElement(MdlDataElement.GivenName, givenName)
            .AddMdlElement(MdlDataElement.BirthDate, birthDate.ToString("yyyy-MM-dd"))
            .AddMdlElement(MdlDataElement.IssueDate, DateTime.UtcNow.ToString("yyyy-MM-dd"))
            .AddMdlElement(MdlDataElement.ExpiryDate, DateTime.UtcNow.AddYears(5).ToString("yyyy-MM-dd"))
            .AddMdlElement(MdlDataElement.IssuingCountry, "US")
            .AddMdlElement(MdlDataElement.IssuingAuthority, "State DMV")
            .AddMdlElement(MdlDataElement.DocumentNumber, documentNumber)
            .AddMdlElement(MdlDataElement.AgeOver18, age >= 18)
            .AddMdlElement(MdlDataElement.AgeOver21, age >= 21)
            .WithValidity(
                DateTimeOffset.UtcNow,
                DateTimeOffset.UtcNow.AddYears(5))
            .BuildAsync(_crypto);

        return mdoc.ToCbor();
    }
}
```

## Run the Sample

```bash
cd samples/SdJwt.Net.Samples
dotnet run -- 2.6
```

## Next Steps

- [mdoc OpenID4VP Integration](../advanced/05-mdoc-integration.md) - Presentation protocols
- [OpenID4VCI](03-openid4vci.md) - Credential issuance with mdoc format

## Key Concepts

| Concept        | Description                           |
| -------------- | ------------------------------------- |
| Namespace      | Grouping of related claims in mdoc    |
| DocType        | Unique identifier for credential type |
| IssuerAuth     | COSE_Sign1 signature over MSO         |
| Validity       | Signed validity period in MSO         |
| Device Binding | Holder's public key in credential     |
