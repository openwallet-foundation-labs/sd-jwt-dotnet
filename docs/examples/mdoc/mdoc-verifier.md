# mdoc Verifier

| Field     | Value                                 |
| --------- | ------------------------------------- |
| Level     | Advanced                              |
| Maturity  | Stable                                |
| Runnable  | Conceptual (paste into a console app) |
| Packages  | SdJwt.Net.Mdoc                        |
| Standards | ISO 18013-5                           |

> **Scope note:** This library implements the data-level verification of mdoc credentials (CBOR parsing, MSO signature validation, claim extraction). It does not implement NFC/BLE device engagement, and it is not a certified mDL reader. For proximity transport, integrate a separate device engagement layer.

This example demonstrates mdoc verification:

1. **Parse** a `DeviceResponse` from CBOR bytes.
2. **Verify** the MSO (Mobile Security Object) issuer signature.
3. **Validate** the `SessionTranscript` binding.
4. **Extract** namespaced claims from the verified document.

---

## 1. Parse a DeviceResponse

```csharp
using SdJwt.Net.Mdoc.Models;
using SdJwt.Net.Mdoc.Verifier;

// Received from wallet (via OID4VP, DC API, or proximity transport)
byte[] deviceResponseBytes = GetDeviceResponseBytes();

// Parse the CBOR-encoded DeviceResponse
var deviceResponse = DeviceResponse.FromCbor(deviceResponseBytes);

Console.WriteLine($"Documents received: {deviceResponse.Documents.Count}");
Console.WriteLine($"  DocType: {deviceResponse.Documents[0].DocType}");
// Output:
// Documents received: 1
//   DocType: org.iso.18013.5.1.mDL
```

---

## 2. Verify MSO Issuer Signature

```csharp
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using SdJwt.Net.Mdoc.Verifier;
using SdJwt.Net.Mdoc.Models;

// Issuer's trusted certificate (or root CA)
var issuerCert = new X509Certificate2("issuer-cert.pem");

var verifier = new MdocVerifier();

var result = verifier.Verify(
    deviceResponse.Documents[0],
    new MdocVerificationOptions
    {
        TrustedCertificates = new[] { issuerCert },
        ExpectedDocType = "org.iso.18013.5.1.mDL"
    });

Console.WriteLine($"MSO signature valid: {result.IsValid}");
Console.WriteLine($"Issuer: {result.IssuerCertificate?.Subject}");

if (!result.IsValid)
{
    Console.WriteLine($"Error: {result.Error}");
    return;
}
```

---

## 3. Validate SessionTranscript

The `SessionTranscript` binds the response to the presentation session, preventing replay.

```csharp
using SdJwt.Net.Mdoc.Handover;

// For OID4VP presentations
var handover = new OpenId4VpHandover(
    clientId: "https://verifier.example.com",
    responseUri: "https://verifier.example.com/callback",
    nonce: "session-nonce-123",
    mdocGeneratedNonce: "mdoc-nonce-456");

var sessionTranscript = new SessionTranscript(handover);

// Verify the document's DeviceAuth is bound to this session
var deviceAuthResult = verifier.Verify(
    deviceResponse.Documents[0],
    new MdocVerificationOptions
    {
        TrustedCertificates = new[] { issuerCert },
        ExpectedDocType = "org.iso.18013.5.1.mDL",
        SessionTranscript = sessionTranscript
    });

Console.WriteLine($"Session binding valid: {deviceAuthResult.IsValid}");
```

---

## 4. Extract Namespaced Claims

```csharp
Document doc = deviceResponse.Documents[0];

// mdoc claims are organized by namespace
// ISO 18013-5 mDL uses "org.iso.18013.5.1" namespace
var issuerSigned = doc.IssuerSigned;

foreach (var ns in issuerSigned.NameSpaces)
{
    Console.WriteLine($"Namespace: {ns.Key}");

    foreach (IssuerSignedItem item in ns.Value)
    {
        Console.WriteLine($"  {item.ElementIdentifier}: {item.ElementValue}");
    }
}

// Example output:
// Namespace: org.iso.18013.5.1
//   family_name: Smith
//   given_name: Alice
//   birth_date: 1990-01-15
//   document_number: DL-123456
//   issuing_authority: State DMV
//   portrait: [byte array]
```

---

## DC API Handover Variant

For browser-based presentations using the DC API:

```csharp
using SdJwt.Net.Mdoc.Handover;

var dcApiHandover = new OpenId4VpDcApiHandover(
    origin: "https://verifier.example.com",
    nonce: "dc-api-nonce-789");

var dcApiTranscript = new SessionTranscript(dcApiHandover);
```

---

## Expected Outcomes

| Step                       | Result                                      |
| -------------------------- | ------------------------------------------- |
| Parse DeviceResponse       | Documents extracted from CBOR               |
| Verify MSO signature       | Issuer certificate chain validated          |
| Validate SessionTranscript | Response bound to this presentation session |
| Extract claims             | Namespaced data elements available          |
| Untrusted issuer cert      | Verification fails                          |
| Wrong SessionTranscript    | Device auth binding fails                   |

---

## Related

- [Issuer - Wallet - Verifier](../credential-lifecycle/issuer-wallet-verifier.md) -- SD-JWT credential flow
- [DC API + OID4VP](../browser/dc-api-oid4vp-verifier.md) -- browser-based presentation
- [ISO 18013-5](https://www.iso.org/standard/69084.html)
