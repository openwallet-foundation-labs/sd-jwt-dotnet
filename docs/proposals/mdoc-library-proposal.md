# SdJwt.Net.Mdoc Library Proposal

## Document Information

| Field        | Value            |
| ------------ | ---------------- |
| Version      | 1.0.0            |
| Author       | SD-JWT .NET Team |
| Status       | Draft Proposal   |
| Created      | 2026-02-28       |
| Last Updated | 2026-02-28       |

## Executive Summary

This proposal outlines the design, architecture, and implementation plan for `SdJwt.Net.Mdoc`, a new package in the SD-JWT .NET ecosystem that implements ISO/IEC 18013-5 (mDL/mdoc) support. The library will enable .NET applications to issue, present, and verify mobile document credentials, ensuring compliance with OpenID4VP/OpenID4VCI specifications and HAIP requirements.

## Background and Motivation

### Why ISO mDL/mdoc?

1. **Government Identity Use Cases**: Mobile driving licenses (mDL), national IDs, and passports use ISO 18013-5 format
2. **EUDIW Compliance**: EU Digital Identity Wallet requires mDL/mdoc support
3. **HAIP Requirement**: OpenID4VC High Assurance Interoperability Profile mandates support for both SD-JWT VC and ISO mdoc formats
4. **OpenID4VP Integration**: The specification explicitly defines `mso_mdoc` credential format with specific handling requirements

### Reference Implementations Analyzed

| Implementation | Language   | Organization               | Key Insights                                            |
| -------------- | ---------- | -------------------------- | ------------------------------------------------------- |
| mdoc-ts        | TypeScript | OpenWallet Foundation Labs | Pluggable crypto interface, comprehensive CBOR handling |
| waltid-mdoc    | Kotlin     | Walt.id                    | Full credential lifecycle, OID4VC integration           |
| auth0/mdl      | TypeScript | Auth0 Labs                 | Foundation for mdoc-ts                                  |

## Technical Architecture

### Package Structure

```
src/SdJwt.Net.Mdoc/
    SdJwt.Net.Mdoc.csproj
    README.md
    MdocConstants.cs

    Cbor/
        CborSerializer.cs           # CBOR encoding/decoding abstraction
        CborReader.cs               # CBOR data reader utilities
        CborWriter.cs               # CBOR data writer utilities
        CborMap.cs                  # Map structure for CBOR data
        CborArray.cs                # Array structure for CBOR data
        ICborSerializable.cs        # Interface for CBOR-serializable types

    Cose/
        CoseSign1.cs                # COSE_Sign1 structure (RFC 8152)
        CoseMac0.cs                 # COSE_Mac0 structure (RFC 8152)
        CoseKey.cs                  # COSE_Key representation
        CoseAlgorithm.cs            # Algorithm identifiers (-7 ES256, -35 ES384, etc.)
        ICoseCryptoProvider.cs      # Abstraction for COSE cryptographic operations
        DefaultCoseCryptoProvider.cs

    Models/
        Mdoc.cs                     # Complete mdoc credential
        MobileSecurityObject.cs     # MSO structure (issuer-signed data)
        IssuerSignedItem.cs         # Individual data element in IssuerSigned
        IssuerSigned.cs             # IssuerSigned structure
        DeviceSigned.cs             # DeviceSigned structure with deviceAuth
        DeviceAuth.cs               # DeviceAuth (deviceSignature or deviceMac)
        DeviceResponse.cs           # DeviceResponse for presentations
        DeviceRequest.cs            # DeviceRequest structure
        DocRequest.cs               # Individual document request
        ItemsRequest.cs             # Items requested per namespace
        Namespace.cs                # Namespace with data elements
        DataElement.cs              # Individual data element
        DigestIdMapping.cs          # Digest ID to element mapping
        ValidityInfo.cs             # Credential validity information
        DeviceKeyInfo.cs            # Device key binding information

    Handover/
        SessionTranscript.cs        # SessionTranscript CBOR structure
        OpenId4VpHandover.cs        # OpenID4VP Handover (redirect flow)
        OpenId4VpDcApiHandover.cs   # OpenID4VP DC API Handover
        HandoverBuilder.cs          # Builder for constructing handover structures

    Issuer/
        MdocIssuer.cs               # Credential issuance
        MdocIssuerBuilder.cs        # Fluent builder for issuing mdocs
        IssuerOptions.cs            # Configuration options for issuance
        MsoBuilder.cs               # Builder for Mobile Security Object

    Holder/
        MdocHolder.cs               # Credential storage and management
        DeviceResponseBuilder.cs    # Build DeviceResponse for presentation
        SelectiveDisclosure.cs      # Select namespaces/elements to disclose

    Verifier/
        MdocVerifier.cs             # Credential verification
        MdocVerificationResult.cs   # Verification result with details
        MdocVerificationOptions.cs  # Verification configuration
        IssuerAuthValidator.cs      # Validate issuer authentication
        DeviceAuthValidator.cs      # Validate device authentication

    OpenId4Vc/
        MdocCredentialFormat.cs     # mso_mdoc credential format identifier
        MdocVpTokenHandler.cs       # VP Token handling for mdoc
        MdocCredentialRequest.cs    # OID4VCI credential request for mdoc
        MdocCredentialResponse.cs   # OID4VCI credential response for mdoc
        DcqlMdocQuery.cs            # DCQL query support for mdoc

    Namespaces/
        MdlNamespace.cs             # org.iso.18013.5.1 (mDL) namespace
        MdlDataElements.cs          # Standard mDL data elements
        INamespaceDefinition.cs     # Interface for custom namespaces
```

### Core Dependencies

```xml
<ItemGroup>
    <!-- CBOR Serialization -->
    <PackageReference Include="PeterO.Cbor" Version="4.5.3" />

    <!-- Existing ecosystem integration -->
    <ProjectReference Include="..\SdJwt.Net\SdJwt.Net.csproj" />
</ItemGroup>
```

**Note**: `PeterO.Cbor` (maintained by Peter Occil) is the most mature and widely-used CBOR library for .NET with:

- Full CBOR compliance (RFC 8949)
- CDDL support
- Cross-platform compatibility
- Active maintenance
- Apache 2.0 license

### Alternative CBOR Libraries Evaluated

| Library             | Pros                          | Cons                         | Recommendation |
| ------------------- | ----------------------------- | ---------------------------- | -------------- |
| PeterO.Cbor         | Mature, full-featured, stable | Slightly larger footprint    | Primary choice |
| Dahomey.Cbor        | Fast, modern API              | Less mature                  | Alternative    |
| System.Formats.Cbor | Microsoft-supported           | .NET 5+ only, basic features | Fallback       |

## Detailed Component Design

### 1. CBOR Serialization Layer

```csharp
/// <summary>
/// Provides CBOR serialization and deserialization for mdoc structures.
/// </summary>
public interface ICborSerializable
{
    /// <summary>
    /// Serializes this object to CBOR bytes.
    /// </summary>
    byte[] ToCbor();

    /// <summary>
    /// Gets the CBOR object representation.
    /// </summary>
    CBORObject ToCborObject();
}

/// <summary>
/// CBOR serialization utilities for mdoc-specific structures.
/// </summary>
public static class CborSerializer
{
    /// <summary>
    /// Deserializes CBOR bytes to the specified type.
    /// </summary>
    public static T Deserialize<T>(byte[] cborData) where T : ICborSerializable, new();

    /// <summary>
    /// Serializes an object to CBOR bytes using tagged encoding per ISO 18013-5.
    /// </summary>
    public static byte[] Serialize<T>(T value) where T : ICborSerializable;
}
```

### 2. COSE Cryptographic Operations

```csharp
/// <summary>
/// Abstraction for COSE cryptographic operations, enabling platform-specific implementations.
/// </summary>
public interface ICoseCryptoProvider
{
    /// <summary>
    /// Creates a COSE_Sign1 signature.
    /// </summary>
    Task<byte[]> SignAsync(
        byte[] payload,
        CoseKey privateKey,
        CoseAlgorithm algorithm,
        byte[]? externalAad = null);

    /// <summary>
    /// Verifies a COSE_Sign1 signature.
    /// </summary>
    Task<bool> VerifyAsync(
        byte[] signatureStructure,
        CoseKey publicKey,
        byte[]? externalAad = null);

    /// <summary>
    /// Creates a COSE_Mac0 MAC.
    /// </summary>
    Task<byte[]> MacAsync(
        byte[] payload,
        byte[] key,
        CoseAlgorithm algorithm,
        byte[]? externalAad = null);

    /// <summary>
    /// Verifies a COSE_Mac0 MAC.
    /// </summary>
    Task<bool> VerifyMacAsync(
        byte[] macStructure,
        byte[] key,
        byte[]? externalAad = null);
}

/// <summary>
/// COSE algorithm identifiers per RFC 8152 and ISO 18013-5.
/// </summary>
public enum CoseAlgorithm
{
    /// <summary>ECDSA with SHA-256 on P-256 curve.</summary>
    ES256 = -7,

    /// <summary>ECDSA with SHA-384 on P-384 curve.</summary>
    ES384 = -35,

    /// <summary>ECDSA with SHA-512 on P-521 curve.</summary>
    ES512 = -36,

    /// <summary>EdDSA (Ed25519 or Ed448).</summary>
    EdDSA = -8,

    /// <summary>HMAC with SHA-256.</summary>
    HMAC256 = 5
}
```

### 3. Mobile Security Object (MSO)

```csharp
/// <summary>
/// Mobile Security Object as defined in ISO 18013-5 Section 9.1.2.4.
/// Contains the issuer-signed metadata and digest values for verifying IssuerSigned items.
/// </summary>
public class MobileSecurityObject : ICborSerializable
{
    /// <summary>
    /// Version of the MSO structure. Must be "1.0" per ISO 18013-5.
    /// </summary>
    public string Version { get; set; } = "1.0";

    /// <summary>
    /// Digest algorithm used. Must be "SHA-256" or "SHA-384" or "SHA-512".
    /// </summary>
    public string DigestAlgorithm { get; set; } = "SHA-256";

    /// <summary>
    /// Mapping of digest values per namespace.
    /// Key: nameSpace, Value: DigestIdMapping
    /// </summary>
    public Dictionary<string, DigestIdMapping> ValueDigests { get; set; } = new();

    /// <summary>
    /// Device key information for holder binding.
    /// </summary>
    public DeviceKeyInfo DeviceKeyInfo { get; set; } = new();

    /// <summary>
    /// Document type (e.g., "org.iso.18013.5.1.mDL").
    /// </summary>
    public string DocType { get; set; } = string.Empty;

    /// <summary>
    /// Validity information for the credential.
    /// </summary>
    public ValidityInfo ValidityInfo { get; set; } = new();
}

/// <summary>
/// Validity information per ISO 18013-5 Section 9.1.2.4.
/// </summary>
public class ValidityInfo : ICborSerializable
{
    /// <summary>
    /// Timestamp when the MSO was signed.
    /// </summary>
    public DateTimeOffset Signed { get; set; }

    /// <summary>
    /// Timestamp from which the MSO is valid.
    /// </summary>
    public DateTimeOffset ValidFrom { get; set; }

    /// <summary>
    /// Timestamp until which the MSO is valid.
    /// </summary>
    public DateTimeOffset ValidUntil { get; set; }

    /// <summary>
    /// Optional expected update timestamp.
    /// </summary>
    public DateTimeOffset? ExpectedUpdate { get; set; }
}
```

### 4. DeviceResponse Structure

```csharp
/// <summary>
/// DeviceResponse structure per ISO 18013-5 Section 8.3.2.1.2.2.
/// Contains the holder's presentation of one or more documents.
/// </summary>
public class DeviceResponse : ICborSerializable
{
    /// <summary>
    /// Version of the DeviceResponse. Must be "1.0".
    /// </summary>
    public string Version { get; set; } = "1.0";

    /// <summary>
    /// Array of documents being presented.
    /// </summary>
    public List<Document> Documents { get; set; } = new();

    /// <summary>
    /// Array of document errors (if any documents could not be returned).
    /// </summary>
    public List<DocumentError>? DocumentErrors { get; set; }

    /// <summary>
    /// Status code for the overall response (0 = OK).
    /// </summary>
    public int Status { get; set; } = 0;
}

/// <summary>
/// Individual document within a DeviceResponse.
/// </summary>
public class Document : ICborSerializable
{
    /// <summary>
    /// Document type (e.g., "org.iso.18013.5.1.mDL").
    /// </summary>
    public string DocType { get; set; } = string.Empty;

    /// <summary>
    /// Issuer-signed data including namespaces and MSO.
    /// </summary>
    public IssuerSigned IssuerSigned { get; set; } = new();

    /// <summary>
    /// Device-signed data proving possession.
    /// </summary>
    public DeviceSigned? DeviceSigned { get; set; }

    /// <summary>
    /// Errors for specific data elements that could not be returned.
    /// </summary>
    public Dictionary<string, Dictionary<string, int>>? Errors { get; set; }
}
```

### 5. OpenID4VP Integration

```csharp
/// <summary>
/// OpenID4VP Handover structure for mdoc presentations via redirects.
/// Per OpenID4VP Appendix B.2.
/// </summary>
public class OpenId4VpHandover : ICborSerializable
{
    /// <summary>
    /// Fixed identifier: "OpenID4VPHandover".
    /// </summary>
    public const string HandoverType = "OpenID4VPHandover";

    /// <summary>
    /// SHA-256 hash of OpenID4VPHandoverInfo.
    /// </summary>
    public byte[] HandoverInfoHash { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// Creates OpenID4VPHandover from request parameters.
    /// </summary>
    public static OpenId4VpHandover Create(
        string clientId,
        string nonce,
        byte[]? jwkThumbprint,
        string responseUri)
    {
        var handoverInfo = new OpenId4VpHandoverInfo
        {
            ClientId = clientId,
            Nonce = nonce,
            JwkThumbprint = jwkThumbprint,
            ResponseUri = responseUri
        };

        var infoBytes = CborSerializer.Serialize(handoverInfo);
        var hash = SHA256.HashData(infoBytes);

        return new OpenId4VpHandover { HandoverInfoHash = hash };
    }
}

/// <summary>
/// SessionTranscript for OpenID4VP mdoc presentations.
/// </summary>
public class SessionTranscript : ICborSerializable
{
    /// <summary>
    /// DeviceEngagementBytes - null for OpenID4VP.
    /// </summary>
    public byte[]? DeviceEngagementBytes { get; set; }

    /// <summary>
    /// EReaderKeyBytes - null for OpenID4VP.
    /// </summary>
    public byte[]? EReaderKeyBytes { get; set; }

    /// <summary>
    /// Handover structure (OpenID4VPHandover or OpenID4VPDCAPIHandover).
    /// </summary>
    public ICborSerializable Handover { get; set; } = null!;

    /// <summary>
    /// Creates SessionTranscript for OpenID4VP redirect flow.
    /// </summary>
    public static SessionTranscript ForOpenId4Vp(
        string clientId,
        string nonce,
        byte[]? jwkThumbprint,
        string responseUri)
    {
        return new SessionTranscript
        {
            DeviceEngagementBytes = null,
            EReaderKeyBytes = null,
            Handover = OpenId4VpHandover.Create(clientId, nonce, jwkThumbprint, responseUri)
        };
    }

    /// <summary>
    /// Creates SessionTranscript for OpenID4VP DC API flow.
    /// </summary>
    public static SessionTranscript ForOpenId4VpDcApi(
        string origin,
        string nonce,
        byte[]? jwkThumbprint)
    {
        return new SessionTranscript
        {
            DeviceEngagementBytes = null,
            EReaderKeyBytes = null,
            Handover = OpenId4VpDcApiHandover.Create(origin, nonce, jwkThumbprint)
        };
    }
}
```

### 6. mdoc Verifier

```csharp
/// <summary>
/// Verifies mdoc credentials per ISO 18013-5 and OpenID4VP requirements.
/// </summary>
public class MdocVerifier
{
    private readonly ICoseCryptoProvider _cryptoProvider;
    private readonly ILogger<MdocVerifier>? _logger;

    /// <summary>
    /// Initializes a new instance of MdocVerifier.
    /// </summary>
    public MdocVerifier(
        ICoseCryptoProvider? cryptoProvider = null,
        ILogger<MdocVerifier>? logger = null)
    {
        _cryptoProvider = cryptoProvider ?? new DefaultCoseCryptoProvider();
        _logger = logger;
    }

    /// <summary>
    /// Verifies a DeviceResponse from an OpenID4VP presentation.
    /// </summary>
    public async Task<MdocVerificationResult> VerifyDeviceResponseAsync(
        byte[] deviceResponseBytes,
        SessionTranscript sessionTranscript,
        MdocVerificationOptions options)
    {
        var result = new MdocVerificationResult();

        try
        {
            // 1. Decode DeviceResponse
            var deviceResponse = CborSerializer.Deserialize<DeviceResponse>(deviceResponseBytes);

            // 2. Verify each document
            foreach (var document in deviceResponse.Documents)
            {
                var docResult = await VerifyDocumentAsync(
                    document, sessionTranscript, options);
                result.DocumentResults.Add(docResult);
            }

            result.IsValid = result.DocumentResults.All(d => d.IsValid);
        }
        catch (Exception ex)
        {
            result.IsValid = false;
            result.Error = ex.Message;
        }

        return result;
    }

    /// <summary>
    /// Verifies a single document within a DeviceResponse.
    /// </summary>
    public async Task<DocumentVerificationResult> VerifyDocumentAsync(
        Document document,
        SessionTranscript sessionTranscript,
        MdocVerificationOptions options)
    {
        var result = new DocumentVerificationResult { DocType = document.DocType };

        // 1. Verify IssuerAuth (MSO signature)
        var msoVerified = await VerifyIssuerAuthAsync(
            document.IssuerSigned.IssuerAuth,
            options);

        if (!msoVerified.IsValid)
        {
            result.IsValid = false;
            result.Errors.Add("IssuerAuth verification failed");
            return result;
        }

        result.MobileSecurityObject = msoVerified.Mso;

        // 2. Verify MSO validity
        if (!VerifyMsoValidity(msoVerified.Mso!, options))
        {
            result.IsValid = false;
            result.Errors.Add("MSO validity check failed");
            return result;
        }

        // 3. Verify digest values match IssuerSignedItems
        if (!VerifyDigests(document.IssuerSigned, msoVerified.Mso!))
        {
            result.IsValid = false;
            result.Errors.Add("Digest verification failed");
            return result;
        }

        // 4. Verify DeviceAuth if present
        if (document.DeviceSigned != null)
        {
            var deviceAuthVerified = await VerifyDeviceAuthAsync(
                document.DeviceSigned,
                msoVerified.Mso!.DeviceKeyInfo,
                sessionTranscript);

            if (!deviceAuthVerified)
            {
                result.IsValid = false;
                result.Errors.Add("DeviceAuth verification failed");
                return result;
            }
        }

        result.IsValid = true;
        result.DisclosedElements = ExtractDisclosedElements(document.IssuerSigned);

        return result;
    }
}

/// <summary>
/// Options for mdoc verification.
/// </summary>
public class MdocVerificationOptions
{
    /// <summary>
    /// Whether to validate the MSO validity period.
    /// </summary>
    public bool ValidateValidity { get; set; } = true;

    /// <summary>
    /// Clock skew tolerance for validity checks.
    /// </summary>
    public TimeSpan ClockSkew { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Trusted issuer certificates for IssuerAuth validation.
    /// </summary>
    public IEnumerable<X509Certificate2>? TrustedIssuers { get; set; }

    /// <summary>
    /// Whether to require device authentication.
    /// </summary>
    public bool RequireDeviceAuth { get; set; } = true;

    /// <summary>
    /// Expected document type(s).
    /// </summary>
    public IEnumerable<string>? ExpectedDocTypes { get; set; }
}
```

### 7. mdoc Issuer

```csharp
/// <summary>
/// Issues mdoc credentials per ISO 18013-5.
/// </summary>
public class MdocIssuer
{
    private readonly ICoseCryptoProvider _cryptoProvider;

    /// <summary>
    /// Issues a new mdoc credential.
    /// </summary>
    public async Task<Mdoc> IssueAsync(MdocIssuerOptions options)
    {
        // Build namespaced data elements
        var nameSpaces = BuildNameSpaces(options.DataElements);

        // Create IssuerSigned structure
        var issuerSigned = new IssuerSigned();

        foreach (var ns in nameSpaces)
        {
            var items = new List<IssuerSignedItem>();
            foreach (var element in ns.Value)
            {
                var item = new IssuerSignedItem
                {
                    DigestId = element.DigestId,
                    Random = GenerateRandom(),
                    ElementIdentifier = element.Identifier,
                    ElementValue = element.Value
                };
                items.Add(item);
            }
            issuerSigned.NameSpaces[ns.Key] = items;
        }

        // Build MSO
        var mso = BuildMso(issuerSigned, options);

        // Sign MSO with issuer key
        var issuerAuth = await SignMsoAsync(mso, options.IssuerKey, options.Algorithm);
        issuerSigned.IssuerAuth = issuerAuth;

        return new Mdoc
        {
            DocType = options.DocType,
            IssuerSigned = issuerSigned
        };
    }
}

/// <summary>
/// Fluent builder for mdoc issuance.
/// </summary>
public class MdocIssuerBuilder
{
    private readonly List<(string Namespace, string Identifier, object Value)> _elements = new();
    private string _docType = "org.iso.18013.5.1.mDL";
    private CoseKey? _issuerKey;
    private CoseKey? _deviceKey;
    private ValidityInfo? _validity;

    /// <summary>
    /// Sets the document type.
    /// </summary>
    public MdocIssuerBuilder WithDocType(string docType)
    {
        _docType = docType;
        return this;
    }

    /// <summary>
    /// Adds a data element to the specified namespace.
    /// </summary>
    public MdocIssuerBuilder AddElement(string nameSpace, string identifier, object value)
    {
        _elements.Add((nameSpace, identifier, value));
        return this;
    }

    /// <summary>
    /// Adds standard mDL data elements.
    /// </summary>
    public MdocIssuerBuilder AddMdlElement(MdlDataElement element, object value)
    {
        return AddElement(MdlNamespace.Namespace, element.ToString(), value);
    }

    /// <summary>
    /// Sets the issuer signing key.
    /// </summary>
    public MdocIssuerBuilder WithIssuerKey(CoseKey key)
    {
        _issuerKey = key;
        return this;
    }

    /// <summary>
    /// Sets the device key for holder binding.
    /// </summary>
    public MdocIssuerBuilder WithDeviceKey(CoseKey key)
    {
        _deviceKey = key;
        return this;
    }

    /// <summary>
    /// Sets the validity period.
    /// </summary>
    public MdocIssuerBuilder WithValidity(DateTimeOffset validFrom, DateTimeOffset validUntil)
    {
        _validity = new ValidityInfo
        {
            Signed = DateTimeOffset.UtcNow,
            ValidFrom = validFrom,
            ValidUntil = validUntil
        };
        return this;
    }

    /// <summary>
    /// Builds and issues the mdoc.
    /// </summary>
    public async Task<Mdoc> BuildAsync(ICoseCryptoProvider cryptoProvider)
    {
        // Implementation
    }
}
```

### 8. VpTokenValidator Integration

The existing `VpTokenValidator` in `SdJwt.Net.Oid4Vp` will be extended to support mdoc:

```csharp
// Extension to VpTokenValidator for mdoc support
public partial class VpTokenValidator
{
    private readonly MdocVerifier? _mdocVerifier;

    /// <summary>
    /// Validates a VP Token that may contain mdoc credentials.
    /// </summary>
    public async Task<SingleVpTokenResult> ValidateMdocVpTokenAsync(
        string base64UrlEncodedDeviceResponse,
        string clientId,
        string nonce,
        byte[]? jwkThumbprint,
        string responseUri,
        MdocVerificationOptions options)
    {
        var deviceResponseBytes = Base64UrlEncoder.DecodeBytes(base64UrlEncodedDeviceResponse);

        var sessionTranscript = SessionTranscript.ForOpenId4Vp(
            clientId, nonce, jwkThumbprint, responseUri);

        var result = await _mdocVerifier!.VerifyDeviceResponseAsync(
            deviceResponseBytes, sessionTranscript, options);

        return new SingleVpTokenResult
        {
            IsValid = result.IsValid,
            CredentialFormat = "mso_mdoc",
            // Map other properties
        };
    }
}
```

## mDL Namespace Support

```csharp
/// <summary>
/// Standard mDL namespace per ISO 18013-5.
/// </summary>
public static class MdlNamespace
{
    /// <summary>
    /// The mDL namespace identifier.
    /// </summary>
    public const string Namespace = "org.iso.18013.5.1";

    /// <summary>
    /// The mDL document type.
    /// </summary>
    public const string DocType = "org.iso.18013.5.1.mDL";
}

/// <summary>
/// Standard mDL data elements per ISO 18013-5 Section 7.2.1.
/// </summary>
public enum MdlDataElement
{
    /// <summary>Family name of the holder.</summary>
    family_name,

    /// <summary>Given name of the holder.</summary>
    given_name,

    /// <summary>Date of birth.</summary>
    birth_date,

    /// <summary>Date of issue.</summary>
    issue_date,

    /// <summary>Date of expiry.</summary>
    expiry_date,

    /// <summary>Issuing country (ISO 3166-1 alpha-2 or alpha-3).</summary>
    issuing_country,

    /// <summary>Issuing authority.</summary>
    issuing_authority,

    /// <summary>Document number.</summary>
    document_number,

    /// <summary>Portrait image of the holder.</summary>
    portrait,

    /// <summary>Driving privileges.</summary>
    driving_privileges,

    /// <summary>UN distinguishing sign.</summary>
    un_distinguishing_sign,

    /// <summary>Administrative number.</summary>
    administrative_number,

    /// <summary>Sex of holder.</summary>
    sex,

    /// <summary>Height in centimeters.</summary>
    height,

    /// <summary>Weight in kilograms.</summary>
    weight,

    /// <summary>Eye color.</summary>
    eye_colour,

    /// <summary>Hair color.</summary>
    hair_colour,

    /// <summary>Place of birth.</summary>
    birth_place,

    /// <summary>Resident address.</summary>
    resident_address,

    /// <summary>Portrait capture date.</summary>
    portrait_capture_date,

    /// <summary>Age over 18 indicator.</summary>
    age_over_18,

    /// <summary>Age over 21 indicator.</summary>
    age_over_21,

    /// <summary>Age in years.</summary>
    age_in_years,

    /// <summary>Age birth year.</summary>
    age_birth_year,

    /// <summary>Issuing jurisdiction.</summary>
    issuing_jurisdiction,

    /// <summary>Nationality.</summary>
    nationality,

    /// <summary>Resident city.</summary>
    resident_city,

    /// <summary>Resident state.</summary>
    resident_state,

    /// <summary>Resident postal code.</summary>
    resident_postal_code,

    /// <summary>Resident country.</summary>
    resident_country,

    /// <summary>Biometric template for face.</summary>
    biometric_template_face,

    /// <summary>Family name in national characters.</summary>
    family_name_national_character,

    /// <summary>Given name in national characters.</summary>
    given_name_national_character,

    /// <summary>Signature or usual mark.</summary>
    signature_usual_mark
}
```

## HAIP Compliance

The implementation will ensure HAIP compliance:

| HAIP Requirement                         | Implementation                                       |
| ---------------------------------------- | ---------------------------------------------------- |
| Credential Format identifier `mso_mdoc`  | `MdocCredentialFormat.FormatIdentifier = "mso_mdoc"` |
| DCQL query support                       | `DcqlMdocQuery` class with `doctype_value` support   |
| Multiple DeviceResponses per VP Token    | `DeviceResponse.Documents` list handling             |
| ES256 algorithm support (COSE -7)        | `CoseAlgorithm.ES256` default                        |
| SHA-256 digest algorithm                 | `MobileSecurityObject.DigestAlgorithm = "SHA-256"`   |
| SessionTranscript with OpenID4VPHandover | `SessionTranscript.ForOpenId4Vp()` method            |
| DC API SessionTranscript support         | `SessionTranscript.ForOpenId4VpDcApi()` method       |

## Security Considerations

### Cryptographic Requirements

1. **Algorithm Compliance**: Only HAIP-approved algorithms (ES256, ES384, ES512) for COSE signatures
2. **Random Value Generation**: Use `RandomNumberGenerator` for IssuerSignedItem random values
3. **Constant-Time Comparison**: Use `CryptographicOperations.FixedTimeEquals` for digest verification
4. **No Weak Algorithms**: Block MD5/SHA-1 usage consistent with existing codebase

### Verification Requirements

1. **Certificate Chain Validation**: Full X.509 chain validation for IssuerAuth
2. **Validity Period Checks**: MSO validity with configurable clock skew
3. **Digest Verification**: All disclosed elements must have matching digests in MSO
4. **Device Authentication**: SessionTranscript binding verification

## Testing Strategy

```
tests/SdJwt.Net.Mdoc.Tests/
    Cbor/
        CborSerializerTests.cs
        CborStructureTests.cs
    Cose/
        CoseSign1Tests.cs
        CoseMac0Tests.cs
        CoseKeyTests.cs
    Models/
        MobileSecurityObjectTests.cs
        DeviceResponseTests.cs
        IssuerSignedTests.cs
    Handover/
        SessionTranscriptTests.cs
        OpenId4VpHandoverTests.cs
    Issuer/
        MdocIssuerTests.cs
        MdocIssuerBuilderTests.cs
    Verifier/
        MdocVerifierTests.cs
        IssuerAuthValidatorTests.cs
        DeviceAuthValidatorTests.cs
    OpenId4Vc/
        MdocVpTokenHandlerTests.cs
        DcqlMdocQueryTests.cs
    Integration/
        EndToEndTests.cs
        InteroperabilityTests.cs
```

## Implementation Timeline

| Phase   | Duration | Deliverables                                     |
| ------- | -------- | ------------------------------------------------ |
| Phase 1 | 2 weeks  | CBOR/COSE foundation, basic models               |
| Phase 2 | 2 weeks  | MSO, IssuerSigned, DeviceResponse structures     |
| Phase 3 | 2 weeks  | Verifier implementation with SessionTranscript   |
| Phase 4 | 1 week   | Issuer implementation                            |
| Phase 5 | 1 week   | OpenID4VP integration                            |
| Phase 6 | 2 weeks  | Testing, documentation, interoperability testing |

**Total Estimated Effort**: 10-12 weeks (as stated in Enterprise Roadmap)

## Package Dependencies Graph

```
SdJwt.Net (Core)
    |
    +---> SdJwt.Net.Mdoc (new)
    |         |
    |         +---> PeterO.Cbor
    |
    +---> SdJwt.Net.Oid4Vp
              |
              +---> SdJwt.Net.Mdoc (optional integration)
```

## API Design Principles

1. **Consistency**: Follow existing SDK patterns (builders, validators, options classes)
2. **Extensibility**: `ICoseCryptoProvider` allows custom implementations
3. **Testability**: All dependencies injectable, interfaces for mocking
4. **Documentation**: XML docs on all public APIs per AGENTS.md requirements
5. **No Breaking Changes**: New package, existing APIs unchanged

## Open Questions

1. **CBOR Library Choice**: Confirm PeterO.Cbor as primary choice vs alternatives
2. **Hardware Security Module Support**: Should `ICoseCryptoProvider` support HSM integration?
3. **Offline Verification**: Future support for BLE-based presentation flows?
4. **ISO 23220 Support**: Include ISO 23220-4 extensions for additional document types?

## References

1. [ISO/IEC 18013-5:2021](https://www.iso.org/standard/69084.html) - Mobile driving licence (mDL) application
2. [OpenID4VP Appendix B.2](https://openid.net/specs/openid-4-verifiable-presentations-1_0.html) - mso_mdoc format
3. [HAIP 1.0](https://openid.net/specs/openid4vc-high-assurance-interoperability-profile-1_0.html) - Section 5.3.1
4. [RFC 8152](https://datatracker.ietf.org/doc/html/rfc8152) - CBOR Object Signing and Encryption (COSE)
5. [RFC 8949](https://datatracker.ietf.org/doc/html/rfc8949) - Concise Binary Object Representation (CBOR)
6. [mdoc-ts](https://github.com/openwallet-foundation-labs/mdoc-ts) - Reference TypeScript implementation
7. [Walt.id mDL](https://walt.id/mobile-driving-license) - Kotlin reference implementation
