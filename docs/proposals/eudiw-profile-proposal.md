# eIDAS 2.0 / EUDIW Profile Proposal

## Document Information

| Field        | Value            |
| ------------ | ---------------- |
| Version      | 1.0.0            |
| Author       | SD-JWT .NET Team |
| Status       | Draft Proposal   |
| Created      | 2026-03-01       |
| Last Updated | 2026-03-01       |

## Executive Summary

This proposal outlines the design, architecture, and implementation plan for `SdJwt.Net.Eudiw`, a new package providing ready-to-use configuration and validation for EU Digital Identity Wallet (EUDIW) compliance under eIDAS 2.0 regulation. The package builds on the existing HAIP foundation and Phase 2 mdoc implementation to enable enterprises to verify credentials from EU member state wallets.

## Background and Motivation

### Why EUDIW Support?

1. **Regulatory Requirement**: EU Regulation 2024/1183 mandates EUDIW adoption by member states by 2026
2. **Market Access**: Enterprises operating in the EU must support EUDIW verification
3. **Trust Framework**: EUDIW defines specific requirements for issuer trust, credential types, and relying party registration
4. **Interoperability**: ARF (Architecture Reference Framework) ensures cross-border credential acceptance

### EU Regulatory Context

| Regulation           | Description                                | Effective Date |
| -------------------- | ------------------------------------------ | -------------- |
| eIDAS 2.0            | EU Digital Identity framework update       | 2024           |
| Regulation 2024/1183 | EUDIW implementation regulation            | 2024-2026      |
| ARF 1.4              | Architecture Reference Framework for EUDIW | 2024           |

### Credential Types in EUDIW Ecosystem

| Type | Full Name                                      | Format    | Use Case                     |
| ---- | ---------------------------------------------- | --------- | ---------------------------- |
| PID  | Person Identification Data                     | mdoc      | National ID equivalent       |
| mDL  | Mobile Driving License                         | mdoc      | Driver's license             |
| QEAA | Qualified Electronic Attestation of Attributes | SD-JWT VC | Diplomas, professional certs |
| EAA  | Electronic Attestation of Attributes           | SD-JWT VC | Loyalty cards, memberships   |

## Technical Architecture

### Package Structure

```
src/SdJwt.Net.Eudiw/
    SdJwt.Net.Eudiw.csproj
    README.md
    EudiwConstants.cs

    TrustFramework/
        IEuTrustListResolver.cs          # Trust list resolution interface
        EuTrustListResolver.cs           # Default implementation
        TrustValidationResult.cs         # Trust validation result
        ListOfTrustedLists.cs            # LOTL structure
        TrustedServiceProvider.cs        # TSP structure
        TrustServiceType.cs              # TSP service types

    Arf/
        ArfProfileValidator.cs           # ARF compliance validation
        ArfValidationResult.cs           # ARF validation result
        ArfCredentialType.cs             # PID, mDL, QEAA, EAA types
        ArfRequirements.cs               # ARF requirement definitions

    Credentials/
        PidCredentialHandler.cs          # PID credential processing
        PidClaims.cs                      # PID mandatory/optional claims
        PidValidationResult.cs           # PID validation result
        QeaaHandler.cs                   # QEAA credential processing
        QeaaValidationResult.cs          # QEAA validation result
        EaaHandler.cs                    # EAA credential processing

    RelyingParty/
        IRpRegistry.cs                   # RP registry interface
        RpRegistrationValidator.cs       # RP registration validation
        RpValidationResult.cs            # RP validation result
        RpRegistration.cs                # RP registration model
        RpStatus.cs                      # RP status enumeration

    Verification/
        EudiwVerificationService.cs      # Main verification orchestrator
        EudiwVerificationResult.cs       # Complete verification result
        EudiwVerificationOptions.cs      # Verification configuration
        CredentialVerificationResult.cs  # Per-credential result
```

### Dependencies

```xml
<ItemGroup>
    <ProjectReference Include="..\SdJwt.Net\SdJwt.Net.csproj" />
    <ProjectReference Include="..\SdJwt.Net.Vc\SdJwt.Net.Vc.csproj" />
    <ProjectReference Include="..\SdJwt.Net.Mdoc\SdJwt.Net.Mdoc.csproj" />
    <ProjectReference Include="..\SdJwt.Net.HAIP\SdJwt.Net.HAIP.csproj" />
    <ProjectReference Include="..\SdJwt.Net.Oid4Vp\SdJwt.Net.Oid4Vp.csproj" />
</ItemGroup>
```

## Detailed Component Design

### 1. EUDIW Constants

```csharp
namespace SdJwt.Net.Eudiw;

/// <summary>
/// Constants for EU Digital Identity Wallet ecosystem.
/// </summary>
public static class EudiwConstants
{
    /// <summary>
    /// Person Identification Data (PID) constants.
    /// </summary>
    public static class Pid
    {
        /// <summary>
        /// DocType for PID credentials.
        /// </summary>
        public const string DocType = "eu.europa.ec.eudi.pid.1";

        /// <summary>
        /// Namespace for PID data elements.
        /// </summary>
        public const string Namespace = "eu.europa.ec.eudi.pid.1";
    }

    /// <summary>
    /// Mobile Driving License (mDL) constants per EUDIW ARF.
    /// </summary>
    public static class Mdl
    {
        /// <summary>
        /// DocType for EU mDL credentials.
        /// </summary>
        public const string DocType = "org.iso.18013.5.1.mDL";

        /// <summary>
        /// Namespace for mDL data elements.
        /// </summary>
        public const string Namespace = "org.iso.18013.5.1";
    }

    /// <summary>
    /// Trust list constants.
    /// </summary>
    public static class TrustList
    {
        /// <summary>
        /// EU List of Trusted Lists (LOTL) URL.
        /// </summary>
        public const string LotlUrl = "https://ec.europa.eu/tools/lotl/eu-lotl.xml";

        /// <summary>
        /// Alternative LOTL URL for JSON format.
        /// </summary>
        public const string LotlJsonUrl = "https://eudi.ec.europa.eu/trust/lotl.json";
    }

    /// <summary>
    /// ARF-mandated cryptographic algorithms.
    /// </summary>
    public static class Algorithms
    {
        /// <summary>
        /// Signature algorithm (HAIP Level 2 minimum).
        /// </summary>
        public const string SignatureAlgorithm = "ES256";

        /// <summary>
        /// Digest algorithm.
        /// </summary>
        public const string DigestAlgorithm = "SHA-256";
    }
}
```

### 2. EU Trust List Framework

```csharp
namespace SdJwt.Net.Eudiw.TrustFramework;

/// <summary>
/// Interface for resolving and validating issuers via EU Trust Lists.
/// </summary>
public interface IEuTrustListResolver
{
    /// <summary>
    /// Validates an issuer's certificate against EU Trust Lists.
    /// </summary>
    /// <param name="issuerCertificate">The issuer's X.509 certificate.</param>
    /// <param name="credentialType">The type of credential being validated.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Trust validation result.</returns>
    Task<TrustValidationResult> ValidateIssuerAsync(
        X509Certificate2 issuerCertificate,
        string credentialType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the current EU List of Trusted Lists (LOTL).
    /// </summary>
    Task<ListOfTrustedLists> GetLotlAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets trusted service providers for a specific EU member state.
    /// </summary>
    /// <param name="memberStateCode">ISO 3166-1 alpha-2 country code (e.g., "DE", "FR").</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<IReadOnlyList<TrustedServiceProvider>> GetTrustedProvidersAsync(
        string memberStateCode,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of trust validation against EU Trust Lists.
/// </summary>
public class TrustValidationResult
{
    /// <summary>
    /// Indicates whether the issuer is trusted.
    /// </summary>
    public bool IsTrusted { get; init; }

    /// <summary>
    /// Reason for untrusted status, if applicable.
    /// </summary>
    public string? Reason { get; init; }

    /// <summary>
    /// Information about the trusted issuer.
    /// </summary>
    public TrustedServiceProvider? IssuerInfo { get; init; }

    /// <summary>
    /// The member state where the issuer is registered.
    /// </summary>
    public string? MemberState { get; init; }

    /// <summary>
    /// Creates a trusted result.
    /// </summary>
    public static TrustValidationResult Trusted(
        TrustedServiceProvider provider,
        string memberState);

    /// <summary>
    /// Creates an untrusted result.
    /// </summary>
    public static TrustValidationResult Untrusted(string reason);
}

/// <summary>
/// EU List of Trusted Lists structure.
/// </summary>
public class ListOfTrustedLists
{
    /// <summary>
    /// Version of the LOTL.
    /// </summary>
    public int SequenceNumber { get; init; }

    /// <summary>
    /// Issue date of this LOTL version.
    /// </summary>
    public DateTimeOffset IssueDate { get; init; }

    /// <summary>
    /// Next scheduled update date.
    /// </summary>
    public DateTimeOffset NextUpdate { get; init; }

    /// <summary>
    /// Pointers to member state Trusted Lists.
    /// </summary>
    public IReadOnlyList<TrustedListPointer> TrustedLists { get; init; }
        = Array.Empty<TrustedListPointer>();
}

/// <summary>
/// Pointer to a member state's Trusted List.
/// </summary>
public class TrustedListPointer
{
    /// <summary>
    /// Member state code (ISO 3166-1 alpha-2).
    /// </summary>
    public string Territory { get; init; } = string.Empty;

    /// <summary>
    /// URL of the member state's Trusted List.
    /// </summary>
    public string TslLocation { get; init; } = string.Empty;
}

/// <summary>
/// Trusted Service Provider from EU Trust Lists.
/// </summary>
public class TrustedServiceProvider
{
    /// <summary>
    /// Name of the trust service provider.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Type of trust service.
    /// </summary>
    public TrustServiceType ServiceType { get; init; }

    /// <summary>
    /// Service status (granted, withdrawn, etc.).
    /// </summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>
    /// Service endpoint URL.
    /// </summary>
    public string? ServiceEndpoint { get; init; }

    /// <summary>
    /// X.509 certificates for the service.
    /// </summary>
    public IReadOnlyList<X509Certificate2> Certificates { get; init; }
        = Array.Empty<X509Certificate2>();
}

/// <summary>
/// Types of trust services in eIDAS framework.
/// </summary>
public enum TrustServiceType
{
    /// <summary>
    /// Qualified certificate for electronic signature.
    /// </summary>
    QualifiedCertificateSignature,

    /// <summary>
    /// Qualified certificate for electronic seal.
    /// </summary>
    QualifiedCertificateSeal,

    /// <summary>
    /// Qualified attestation of attributes provider.
    /// </summary>
    QualifiedAttestation,

    /// <summary>
    /// PID provider.
    /// </summary>
    PidProvider,

    /// <summary>
    /// Electronic attestation of attributes provider.
    /// </summary>
    ElectronicAttestation
}
```

### 3. ARF Profile Validator

```csharp
namespace SdJwt.Net.Eudiw.Arf;

/// <summary>
/// Validates credentials against EU Architecture Reference Framework requirements.
/// </summary>
public class ArfProfileValidator
{
    private readonly HaipCryptoValidator _haipValidator;

    /// <summary>
    /// Initializes the ARF validator with HAIP crypto validation.
    /// </summary>
    public ArfProfileValidator(HaipCryptoValidator haipValidator);

    /// <summary>
    /// Validates a credential against ARF requirements.
    /// </summary>
    /// <param name="credential">The credential to validate (SD-JWT VC or mdoc).</param>
    /// <param name="credentialType">Expected credential type.</param>
    /// <returns>ARF validation result.</returns>
    public ArfValidationResult ValidateCredential(
        object credential,
        ArfCredentialType credentialType);

    /// <summary>
    /// Validates cryptographic algorithms meet ARF requirements.
    /// </summary>
    public bool ValidateCryptoAlgorithms(string algorithm);

    /// <summary>
    /// Validates credential structure matches ARF schema.
    /// </summary>
    public bool ValidateStructure(object credential, ArfCredentialType type);
}

/// <summary>
/// Result of ARF profile validation.
/// </summary>
public class ArfValidationResult
{
    /// <summary>
    /// Indicates whether the credential is ARF-compliant.
    /// </summary>
    public bool IsCompliant { get; init; }

    /// <summary>
    /// List of ARF violations, if any.
    /// </summary>
    public IReadOnlyList<string> Violations { get; init; } = Array.Empty<string>();

    /// <summary>
    /// HAIP compliance level achieved.
    /// </summary>
    public int HaipLevel { get; init; }

    /// <summary>
    /// Creates a compliant result.
    /// </summary>
    public static ArfValidationResult Compliant(int haipLevel);

    /// <summary>
    /// Creates a non-compliant result.
    /// </summary>
    public static ArfValidationResult NonCompliant(IEnumerable<string> violations);
}

/// <summary>
/// EUDIW credential types per ARF specification.
/// </summary>
public enum ArfCredentialType
{
    /// <summary>
    /// Person Identification Data - EU-wide identity.
    /// </summary>
    PID,

    /// <summary>
    /// Mobile Driving License - ISO 18013-5 compliant.
    /// </summary>
    MDL,

    /// <summary>
    /// Qualified Electronic Attestation of Attributes.
    /// Issued by qualified trust service providers.
    /// </summary>
    QEAA,

    /// <summary>
    /// Electronic Attestation of Attributes.
    /// Issued by non-qualified providers.
    /// </summary>
    EAA
}
```

### 4. PID Credential Handler

```csharp
namespace SdJwt.Net.Eudiw.Credentials;

/// <summary>
/// Handles Person Identification Data (PID) credentials.
/// </summary>
public class PidCredentialHandler
{
    /// <summary>
    /// Validates and extracts claims from a PID credential.
    /// </summary>
    /// <param name="document">The mdoc document containing PID data.</param>
    /// <returns>PID validation result with extracted claims.</returns>
    public PidValidationResult ValidatePid(Document document);

    /// <summary>
    /// Validates that all mandatory PID fields are present.
    /// </summary>
    public bool ValidateMandatoryFields(PidClaims claims);

    /// <summary>
    /// Extracts PID claims from a validated mdoc.
    /// </summary>
    public PidClaims ExtractClaims(Document document);
}

/// <summary>
/// PID mandatory and optional claims per ARF specification.
/// </summary>
public class PidClaims
{
    // Mandatory claims
    /// <summary>Family name(s).</summary>
    public string FamilyName { get; set; } = string.Empty;

    /// <summary>Given name(s).</summary>
    public string GivenName { get; set; } = string.Empty;

    /// <summary>Date of birth.</summary>
    public DateOnly BirthDate { get; set; }

    /// <summary>Age attestation: over 18.</summary>
    public bool? AgeOver18 { get; set; }

    /// <summary>Age attestation: over 21.</summary>
    public bool? AgeOver21 { get; set; }

    /// <summary>Member state that issued the PID.</summary>
    public string IssuingCountry { get; set; } = string.Empty;

    /// <summary>Authority that issued the PID.</summary>
    public string IssuingAuthority { get; set; } = string.Empty;

    /// <summary>Date the PID was issued.</summary>
    public DateOnly IssuanceDate { get; set; }

    /// <summary>Date the PID expires.</summary>
    public DateOnly ExpiryDate { get; set; }

    // Optional claims
    /// <summary>Family name at birth.</summary>
    public string? FamilyNameBirth { get; set; }

    /// <summary>Given name at birth.</summary>
    public string? GivenNameBirth { get; set; }

    /// <summary>Place of birth.</summary>
    public string? BirthPlace { get; set; }

    /// <summary>Nationality.</summary>
    public string? Nationality { get; set; }

    /// <summary>Resident address.</summary>
    public string? ResidentAddress { get; set; }

    /// <summary>Gender.</summary>
    public string? Gender { get; set; }
}

/// <summary>
/// Result of PID credential validation.
/// </summary>
public class PidValidationResult
{
    /// <summary>
    /// Indicates whether the PID is valid.
    /// </summary>
    public bool IsValid { get; init; }

    /// <summary>
    /// Error message if validation failed.
    /// </summary>
    public string? Error { get; init; }

    /// <summary>
    /// Extracted PID claims.
    /// </summary>
    public PidClaims? Claims { get; init; }

    /// <summary>
    /// Creates a successful validation result.
    /// </summary>
    public static PidValidationResult Success(PidClaims claims);

    /// <summary>
    /// Creates a failed validation result.
    /// </summary>
    public static PidValidationResult Failure(string error);

    /// <summary>
    /// Creates a result for invalid document type.
    /// </summary>
    public static PidValidationResult InvalidDocType();
}
```

### 5. QEAA Handler

```csharp
namespace SdJwt.Net.Eudiw.Credentials;

/// <summary>
/// Handles Qualified Electronic Attestation of Attributes (QEAA).
/// </summary>
public class QeaaHandler
{
    private readonly IEuTrustListResolver _trustResolver;

    /// <summary>
    /// Initializes handler with EU trust list resolver.
    /// </summary>
    public QeaaHandler(IEuTrustListResolver trustResolver);

    /// <summary>
    /// Validates a QEAA credential.
    /// </summary>
    /// <param name="sdJwtVc">The SD-JWT VC credential.</param>
    /// <returns>QEAA validation result.</returns>
    public Task<QeaaValidationResult> ValidateQeaaAsync(string sdJwtVc);

    /// <summary>
    /// Checks if issuer is a Qualified Trust Service Provider.
    /// </summary>
    public Task<bool> IsQualifiedProviderAsync(string issuerUrl);
}

/// <summary>
/// Result of QEAA validation.
/// </summary>
public class QeaaValidationResult
{
    /// <summary>
    /// Indicates whether the QEAA is valid.
    /// </summary>
    public bool IsValid { get; init; }

    /// <summary>
    /// Whether the issuer is a qualified provider.
    /// </summary>
    public bool IsQualifiedIssuer { get; init; }

    /// <summary>
    /// Extracted attestation claims.
    /// </summary>
    public IReadOnlyDictionary<string, object>? Claims { get; init; }

    /// <summary>
    /// Validation error, if any.
    /// </summary>
    public string? Error { get; init; }
}
```

### 6. Relying Party Registration Validator

```csharp
namespace SdJwt.Net.Eudiw.RelyingParty;

/// <summary>
/// Interface for accessing EU RP registration registry.
/// </summary>
public interface IRpRegistry
{
    /// <summary>
    /// Gets registration information for a relying party.
    /// </summary>
    Task<RpRegistration?> GetRegistrationAsync(
        string clientId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Validates relying party registration in the EU ecosystem.
/// </summary>
public class RpRegistrationValidator
{
    private readonly IRpRegistry _registry;

    /// <summary>
    /// Initializes validator with RP registry.
    /// </summary>
    public RpRegistrationValidator(IRpRegistry registry);

    /// <summary>
    /// Validates that an RP is registered and authorized.
    /// </summary>
    /// <param name="rpClientId">RP client identifier.</param>
    /// <param name="requestedCredentialTypes">Credential types being requested.</param>
    public Task<RpValidationResult> ValidateRpAsync(
        string rpClientId,
        string[] requestedCredentialTypes);
}

/// <summary>
/// Relying party registration information.
/// </summary>
public class RpRegistration
{
    /// <summary>
    /// RP client identifier.
    /// </summary>
    public string ClientId { get; init; } = string.Empty;

    /// <summary>
    /// RP display name.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Credential types the RP is authorized to request.
    /// </summary>
    public IReadOnlyList<string> AuthorizedCredentialTypes { get; init; }
        = Array.Empty<string>();

    /// <summary>
    /// RP registration status.
    /// </summary>
    public RpStatus Status { get; init; }

    /// <summary>
    /// Member state where RP is registered.
    /// </summary>
    public string MemberState { get; init; } = string.Empty;
}

/// <summary>
/// RP registration status.
/// </summary>
public enum RpStatus
{
    /// <summary>RP is active and authorized.</summary>
    Active,

    /// <summary>RP registration is suspended.</summary>
    Suspended,

    /// <summary>RP registration is revoked.</summary>
    Revoked,

    /// <summary>RP registration is pending approval.</summary>
    Pending
}

/// <summary>
/// Result of RP registration validation.
/// </summary>
public class RpValidationResult
{
    /// <summary>
    /// Indicates whether the RP is valid and authorized.
    /// </summary>
    public bool IsValid { get; init; }

    /// <summary>
    /// Error or reason for invalid status.
    /// </summary>
    public string? Reason { get; init; }

    /// <summary>
    /// The RP registration if valid.
    /// </summary>
    public RpRegistration? Registration { get; init; }

    /// <summary>
    /// Creates a valid result.
    /// </summary>
    public static RpValidationResult Valid(RpRegistration registration);

    /// <summary>
    /// Creates a not registered result.
    /// </summary>
    public static RpValidationResult NotRegistered();

    /// <summary>
    /// Creates a not authorized result.
    /// </summary>
    public static RpValidationResult NotAuthorizedForCredentialTypes();

    /// <summary>
    /// Creates an inactive result.
    /// </summary>
    public static RpValidationResult Inactive(RpStatus status);
}
```

### 7. EUDIW Verification Service

```csharp
namespace SdJwt.Net.Eudiw.Verification;

/// <summary>
/// Main orchestrator for EUDIW credential verification.
/// </summary>
public class EudiwVerificationService
{
    private readonly ArfProfileValidator _arfValidator;
    private readonly IEuTrustListResolver _trustResolver;
    private readonly RpRegistrationValidator _rpValidator;
    private readonly PidCredentialHandler _pidHandler;
    private readonly MdocVerifier _mdocVerifier;

    /// <summary>
    /// Initializes the verification service with dependencies.
    /// </summary>
    public EudiwVerificationService(
        ArfProfileValidator arfValidator,
        IEuTrustListResolver trustResolver,
        RpRegistrationValidator rpValidator,
        PidCredentialHandler pidHandler,
        MdocVerifier mdocVerifier);

    /// <summary>
    /// Verifies a complete EUDIW presentation.
    /// </summary>
    /// <param name="presentation">The DeviceResponse from the wallet.</param>
    /// <param name="rpClientId">The relying party client ID.</param>
    /// <param name="expectedNonce">Expected nonce for replay protection.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public Task<EudiwVerificationResult> VerifyPresentationAsync(
        DeviceResponse presentation,
        string rpClientId,
        string expectedNonce,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Complete result of EUDIW verification.
/// </summary>
public class EudiwVerificationResult
{
    /// <summary>
    /// Overall success status.
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// RP validation result.
    /// </summary>
    public RpValidationResult? RpValidation { get; init; }

    /// <summary>
    /// Individual credential verification results.
    /// </summary>
    public IReadOnlyList<CredentialVerificationResult> Credentials { get; init; }
        = Array.Empty<CredentialVerificationResult>();

    /// <summary>
    /// Verification timestamp.
    /// </summary>
    public DateTimeOffset Timestamp { get; init; }
}

/// <summary>
/// Result for a single credential in the presentation.
/// </summary>
public class CredentialVerificationResult
{
    /// <summary>
    /// Document type that was verified.
    /// </summary>
    public string DocType { get; init; } = string.Empty;

    /// <summary>
    /// Whether verification succeeded.
    /// </summary>
    public bool IsValid { get; init; }

    /// <summary>
    /// Error if verification failed.
    /// </summary>
    public string? Error { get; init; }

    /// <summary>
    /// Verified claims from the credential.
    /// </summary>
    public IReadOnlyDictionary<string, object>? Claims { get; init; }

    /// <summary>
    /// Information about the trusted issuer.
    /// </summary>
    public TrustedServiceProvider? IssuerInfo { get; init; }

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    public static CredentialVerificationResult Success(
        string docType,
        IReadOnlyDictionary<string, object> claims,
        TrustedServiceProvider issuerInfo);

    /// <summary>
    /// Creates a failed result.
    /// </summary>
    public static CredentialVerificationResult Failed(string docType, string error);

    /// <summary>
    /// Creates a non-compliant result.
    /// </summary>
    public static CredentialVerificationResult NonCompliant(
        string docType,
        IEnumerable<string> violations);

    /// <summary>
    /// Creates an untrusted issuer result.
    /// </summary>
    public static CredentialVerificationResult UntrustedIssuer(
        string docType,
        string reason);
}
```

## Security Considerations

### Trust Chain Validation

All credentials must be validated against the EU trust hierarchy:

1. **LOTL Verification**: Verify LOTL signature with EU root certificate
2. **Member State TL**: Verify national Trusted List signatures
3. **Issuer Certificate**: Validate issuer certificate is on approved TSP list
4. **Certificate Chain**: Full X.509 path validation

### Cryptographic Requirements

Per ARF and HAIP specifications:

| Requirement     | Minimum       | Recommended           |
| --------------- | ------------- | --------------------- |
| Signature       | ES256 (P-256) | ES384 (P-384)         |
| Digest          | SHA-256       | SHA-384               |
| Key Length (EC) | 256-bit       | 384-bit               |
| Certificate     | X.509 v3      | Qualified certificate |

### RP Authorization Model

Relying parties must be registered and authorized for specific credential types:

```csharp
// RP must be registered
if (!await _rpValidator.IsRegisteredAsync(rpClientId))
    return EudiwVerificationResult.RpNotRegistered();

// RP must be authorized for requested credential types
if (!registration.AuthorizedCredentialTypes.Contains("eu.europa.ec.eudi.pid.1"))
    return EudiwVerificationResult.RpNotAuthorized();
```

## Implementation Plan

### Phase 1: Core Framework (Week 1-2)

| Task                | Description                     | Priority |
| ------------------- | ------------------------------- | -------- |
| `EudiwConstants`    | Define all EUDIW constants      | High     |
| `ArfCredentialType` | Credential type enumeration     | High     |
| `TrustServiceType`  | Trust service types             | High     |
| `RpStatus`          | RP status enumeration           | High     |
| Project structure   | Create project and dependencies | High     |

### Phase 2: Trust Framework (Week 2-4)

| Task                     | Description                | Priority |
| ------------------------ | -------------------------- | -------- |
| `IEuTrustListResolver`   | Trust resolution interface | High     |
| `EuTrustListResolver`    | Default implementation     | High     |
| `TrustValidationResult`  | Trust validation result    | High     |
| `ListOfTrustedLists`     | LOTL parsing               | High     |
| `TrustedServiceProvider` | TSP model                  | High     |

### Phase 3: ARF Validation (Week 4-5)

| Task                  | Description                 | Priority |
| --------------------- | --------------------------- | -------- |
| `ArfProfileValidator` | ARF compliance validation   | High     |
| `ArfValidationResult` | Validation result model     | High     |
| HAIP integration      | Crypto algorithm validation | High     |

### Phase 4: Credential Handlers (Week 5-6)

| Task                   | Description      | Priority |
| ---------------------- | ---------------- | -------- |
| `PidCredentialHandler` | PID processing   | High     |
| `PidClaims`            | PID claims model | High     |
| `QeaaHandler`          | QEAA processing  | Medium   |
| `EaaHandler`           | EAA processing   | Medium   |

### Phase 5: RP Validation (Week 6-7)

| Task                      | Description        | Priority |
| ------------------------- | ------------------ | -------- |
| `IRpRegistry`             | Registry interface | High     |
| `RpRegistrationValidator` | RP validation      | High     |
| `RpRegistration`          | Registration model | High     |

### Phase 6: Verification Service (Week 7-8)

| Task                       | Description             | Priority |
| -------------------------- | ----------------------- | -------- |
| `EudiwVerificationService` | Main orchestrator       | High     |
| `EudiwVerificationResult`  | Complete result model   | High     |
| Integration tests          | End-to-end verification | High     |

## Test Strategy

Following TDD methodology:

### Unit Tests

```text
tests/SdJwt.Net.Eudiw.Tests/
    TrustFramework/
        EuTrustListResolverTests.cs
        TrustValidationResultTests.cs
    Arf/
        ArfProfileValidatorTests.cs
    Credentials/
        PidCredentialHandlerTests.cs
        QeaaHandlerTests.cs
    RelyingParty/
        RpRegistrationValidatorTests.cs
    Verification/
        EudiwVerificationServiceTests.cs
```

### Test Categories

| Category         | Coverage Target | Examples                        |
| ---------------- | --------------- | ------------------------------- |
| Trust Validation | 100%            | Valid chain, revoked, unknown   |
| ARF Compliance   | 100%            | Valid structure, missing fields |
| PID Processing   | 100%            | Valid PID, missing mandatory    |
| RP Validation    | 100%            | Registered, suspended, revoked  |
| Integration      | 90%             | Full verification flows         |

## Success Criteria

| Metric              | Target                             |
| ------------------- | ---------------------------------- |
| Unit test pass rate | 100%                               |
| Code coverage       | >= 90%                             |
| API documentation   | All public methods documented      |
| ARF compliance      | All mandatory requirements met     |
| Interop testing     | Validate against reference wallets |

## Appendix: ARF Requirements Summary

### PID Mandatory Data Elements

| Element             | Type    | Description                |
| ------------------- | ------- | -------------------------- |
| `family_name`       | string  | Current family name(s)     |
| `given_name`        | string  | Current given name(s)      |
| `birth_date`        | date    | Date of birth              |
| `age_over_18`       | boolean | Age attestation (optional) |
| `issuance_date`     | date    | Date PID was issued        |
| `expiry_date`       | date    | Date PID expires           |
| `issuing_authority` | string  | Authority that issued PID  |
| `issuing_country`   | string  | ISO 3166-1 alpha-2 code    |

### Trust Service Status Values

| Status             | Description                        |
| ------------------ | ---------------------------------- |
| `granted`          | Service is operational and trusted |
| `withdrawn`        | Service has been withdrawn         |
| `suspended`        | Service is temporarily suspended   |
| `undersupervision` | Service is under supervision       |

## References

- [EU Regulation 2024/1183](https://eur-lex.europa.eu/legal-content/EN/TXT/?uri=CELEX:32024R1183)
- [EUDIW Architecture Reference Framework](https://github.com/eu-digital-identity-wallet/eudi-doc-architecture-and-reference-framework)
- [EU List of Trusted Lists](https://ec.europa.eu/tools/lotl/)
- [OpenID4VC HAIP](https://openid.net/specs/openid4vc-high-assurance-interoperability-profile-1_0.html)
