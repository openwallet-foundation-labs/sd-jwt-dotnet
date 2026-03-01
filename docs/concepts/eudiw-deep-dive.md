# EU Digital Identity Wallet (EUDIW) Deep Dive

This document explains the EU Digital Identity Wallet ecosystem in beginner-friendly terms and maps each concept to the implementation in this repository.

## Prerequisites

Before diving into EUDIW, you should understand these foundational concepts:

### What is eIDAS 2.0?

**eIDAS 2.0** (Electronic Identification, Authentication and Trust Services) is the EU regulation that mandates digital identity wallets for all EU citizens by 2026. It builds on the original eIDAS regulation to enable:

- Cross-border digital identity verification
- Privacy-preserving credential presentation
- Qualified electronic signatures from mobile devices
- Trust interoperability between member states

### What is the Architecture Reference Framework (ARF)?

The **Architecture Reference Framework** is the technical specification that defines how EUDIW implementations must work:

- Credential formats (mdoc for PID/mDL, SD-JWT VC for attestations)
- Cryptographic algorithms (HAIP-compliant)
- Trust infrastructure (EU Trust Lists)
- Protocol requirements (OpenID4VCI, OpenID4VP)

### The Problem EUDIW Solves

Citizens across EU member states currently face:

1. **Fragmented identity**: Different digital IDs per country and service
2. **Privacy concerns**: Over-disclosure of personal data
3. **Cross-border friction**: National IDs not accepted elsewhere
4. **Trust complexity**: No unified way to verify credential issuers

EUDIW provides:

- Single wallet app for all digital credentials
- Selective disclosure (show only what's needed)
- Cross-border acceptance (French wallet works in Germany)
- Unified trust infrastructure (EU Trust Lists)

## Glossary of Key Terms

| Term             | Definition                                                 |
| ---------------- | ---------------------------------------------------------- |
| **EUDIW**        | EU Digital Identity Wallet - the app citizens use          |
| **ARF**          | Architecture Reference Framework - technical specification |
| **PID**          | Person Identification Data - core identity credential      |
| **mDL**          | Mobile Driving License - per ISO 18013-5                   |
| **QEAA**         | Qualified Electronic Attestation of Attributes             |
| **EAA**          | Electronic Attestation of Attributes (non-qualified)       |
| **RP**           | Relying Party - service requesting credentials             |
| **LOTL**         | List of Trusted Lists - EU trust anchor                    |
| **TSP**          | Trust Service Provider - credential issuers                |
| **Member State** | EU country participating in EUDIW ecosystem                |
| **DocType**      | Credential type identifier for mdoc format                 |
| **vct**          | Verifiable Credential Type for SD-JWT VC format            |

## Implementation Overview

### Package Structure

The `SdJwt.Net.Eudiw` package provides:

```text
SdJwt.Net.Eudiw/
   Arf/
      ArfCredentialType.cs       # Credential type enumeration
      ArfProfileValidator.cs     # ARF compliance validation
      ArfValidationResult.cs     # Validation result model
   Credentials/
      PidCredentialHandler.cs    # PID processing and validation
      QeaaHandler.cs             # Qualified attestation handling
   RelyingParty/
      RpRegistration.cs          # RP registration model
      RpRegistrationValidator.cs # RP validation logic
   TrustFramework/
      EuTrustListResolver.cs     # EU Trust List integration
      TrustedServiceProvider.cs  # TSP model
      TrustServiceType.cs        # TSP type enumeration
      TrustValidationResult.cs   # Trust validation result
   EudiwConstants.cs             # Constants and definitions
```

### Core Components

#### ArfProfileValidator

Validates credentials against ARF requirements:

```csharp
using SdJwt.Net.Eudiw.Arf;

var validator = new ArfProfileValidator();

// Validate cryptographic algorithm
bool isValidAlg = validator.ValidateAlgorithm("ES256"); // true
bool isInvalidAlg = validator.ValidateAlgorithm("RS256"); // false (not ARF-compliant)

// Validate credential type
var result = validator.ValidateCredentialType("eu.europa.ec.eudi.pid.1");
if (result.IsValid)
{
    Console.WriteLine($"Credential type: {result.CredentialType}"); // Pid
}

// Validate EU member state
bool isEu = validator.ValidateMemberState("DE"); // true
bool isNotEu = validator.ValidateMemberState("US"); // false
```

Key validations:

- **Algorithm compliance**: Only HAIP-approved algorithms
- **Credential types**: PID, mDL, QEAA, EAA
- **Member states**: All 27 EU countries
- **PID claims**: Mandatory and optional fields

#### PidCredentialHandler

Processes Person Identification Data credentials:

```csharp
using SdJwt.Net.Eudiw.Credentials;

var handler = new PidCredentialHandler();

var claims = new Dictionary<string, object>
{
    // Mandatory PID claims
    ["family_name"] = "Mustermann",
    ["given_name"] = "Erika",
    ["birth_date"] = "1964-08-12",
    ["issuance_date"] = "2024-01-01",
    ["expiry_date"] = "2029-01-01",
    ["issuing_authority"] = "Bundesdruckerei",
    ["issuing_country"] = "DE",

    // Optional PID claims
    ["birth_place"] = "Berlin",
    ["nationality"] = "DE",
    ["age_over_18"] = true
};

// Validate claims
var validation = handler.Validate(claims);
if (validation.IsValid)
{
    // Convert to typed model
    var credential = handler.ToPidCredential(claims);
    Console.WriteLine($"{credential.GivenName} {credential.FamilyName}");
    Console.WriteLine($"Issued by: {credential.IssuingAuthority}");
}
else
{
    foreach (var error in validation.Errors)
    {
        Console.WriteLine($"Validation error: {error}");
    }
}
```

Mandatory PID claims per ARF:

| Claim               | Description                   | Example           |
| ------------------- | ----------------------------- | ----------------- |
| `family_name`       | Current family name           | "Mustermann"      |
| `given_name`        | Current first name            | "Erika"           |
| `birth_date`        | Date of birth                 | "1964-08-12"      |
| `issuance_date`     | Credential issuance date      | "2024-01-01"      |
| `expiry_date`       | Credential expiration         | "2029-01-01"      |
| `issuing_authority` | Authority that issued the PID | "Bundesdruckerei" |
| `issuing_country`   | Member state ISO code         | "DE"              |

#### QeaaHandler

Handles Qualified Electronic Attestations of Attributes:

```csharp
using SdJwt.Net.Eudiw.Credentials;

var handler = new QeaaHandler();

// University diploma as QEAA
var diplomaClaims = new Dictionary<string, object>
{
    ["degree_type"] = "Masters",
    ["field_of_study"] = "Computer Science",
    ["issuing_institution"] = "Technical University of Munich",
    ["graduation_date"] = "2023-07-15",
    ["issuing_country"] = "DE"
};

var validation = handler.Validate(diplomaClaims, QeaaType.EducationalCredential);
```

QEAA types supported:

- Educational credentials (diplomas, certificates)
- Professional qualifications (licenses)
- Healthcare credentials (prescriptions)
- Travel documents (visas)
- Financial attestations (creditworthiness)

#### RpRegistrationValidator

Validates Relying Party registrations:

```csharp
using SdJwt.Net.Eudiw.RelyingParty;

var validator = new RpRegistrationValidator();

var registration = new RpRegistration
{
    ClientId = "https://bank.example.de",
    OrganizationName = "Example Bank AG",
    RedirectUris = new[] { "https://bank.example.de/callback" },
    ResponseTypes = new[] { "vp_token" },
    TrustFramework = "eu.eudiw.trust",
    Contacts = new[] { "security@bank.example.de" }
};

var result = validator.Validate(registration);
if (result.IsValid)
{
    Console.WriteLine("RP registration is valid for EUDIW ecosystem");
}
else
{
    foreach (var error in result.Errors)
    {
        Console.WriteLine($"Registration error: {error}");
    }
}
```

#### EuTrustListResolver

Resolves and validates issuers against EU Trust Lists:

```csharp
using SdJwt.Net.Eudiw.TrustFramework;

var resolver = new EuTrustListResolver();

// Resolve Trust Service Provider
var tsp = await resolver.ResolveAsync(
    issuerIdentifier: "https://pid.bundesdruckerei.de",
    memberState: "DE"
);

if (tsp != null)
{
    Console.WriteLine($"TSP: {tsp.Name}");
    Console.WriteLine($"Service Type: {tsp.ServiceType}");
    Console.WriteLine($"Status: {tsp.Status}");

    // Validate trust chain
    var trustResult = await resolver.ValidateTrustChainAsync(tsp);
    if (trustResult.IsValid)
    {
        Console.WriteLine("Issuer is trusted under EU Trust Lists");
    }
}
```

## Credential Types

### Person Identification Data (PID)

The PID is the core identity credential in EUDIW:

```csharp
// DocType for mdoc format
const string PidDocType = "eu.europa.ec.eudi.pid.1";

// Namespace for PID claims
const string PidNamespace = "eu.europa.ec.eudi.pid.1";
```

PID supports two formats:

| Format    | Use Case            | Transport        |
| --------- | ------------------- | ---------------- |
| mdoc      | NFC/BLE proximity   | Device proximity |
| SD-JWT VC | Online/cross-device | OpenID4VP        |

### Mobile Driving License (mDL)

Follows ISO 18013-5 with EUDIW extensions:

```csharp
// DocType for mDL
const string MdlDocType = "org.iso.18013.5.1.mDL";

// EUDIW-specific mDL claims
var mDlClaims = new Dictionary<string, object>
{
    ["family_name"] = "Mustermann",
    ["given_name"] = "Erika",
    ["birth_date"] = "1964-08-12",
    ["document_number"] = "D1234567890",
    ["driving_privileges"] = new[] { "B", "C1" },
    ["issue_date"] = "2024-01-01",
    ["expiry_date"] = "2034-01-01",
    ["issuing_country"] = "DE",
    ["issuing_authority"] = "Kraftfahrt-Bundesamt"
};
```

### Qualified Attestations (QEAA)

High-trust attestations from qualified TSPs:

```csharp
// VCT prefix for QEAA
const string QeaaVctPrefix = "eu.europa.ec.eudi.qeaa";

// Example: Professional license as QEAA
var lawyerLicense = new Dictionary<string, object>
{
    ["vct"] = "eu.europa.ec.eudi.qeaa.professional.lawyer.de",
    ["bar_association"] = "Rechtsanwaltskammer Berlin",
    ["license_number"] = "RAK-B-12345",
    ["admission_date"] = "2010-05-15",
    ["specializations"] = new[] { "Corporate Law", "M&A" }
};
```

### Non-Qualified Attestations (EAA)

Lower-trust attestations for general use:

```csharp
// VCT prefix for EAA
const string EaaVctPrefix = "eu.europa.ec.eudi.eaa";

// Example: Gym membership as EAA
var gymMembership = new Dictionary<string, object>
{
    ["vct"] = "eu.europa.ec.eudi.eaa.membership.gym",
    ["membership_id"] = "GYM-2024-001",
    ["valid_from"] = "2024-01-01",
    ["valid_until"] = "2024-12-31",
    ["membership_type"] = "Premium"
};
```

## Algorithm Requirements

EUDIW enforces HAIP-compliant algorithms:

| Algorithm | Status      | Security Level |
| --------- | ----------- | -------------- |
| ES256     | Required    | HAIP Level 2   |
| ES384     | Supported   | HAIP Level 2+  |
| ES512     | Supported   | HAIP Level 3   |
| RS256     | Not allowed | -              |
| RS384     | Not allowed | -              |
| HS256     | Not allowed | -              |

```csharp
using SdJwt.Net.Eudiw.Arf;

var validator = new ArfProfileValidator();

// These pass ARF validation
validator.ValidateAlgorithm("ES256"); // true
validator.ValidateAlgorithm("ES384"); // true
validator.ValidateAlgorithm("ES512"); // true

// These fail ARF validation
validator.ValidateAlgorithm("RS256"); // false
validator.ValidateAlgorithm("HS256"); // false
validator.ValidateAlgorithm("EdDSA"); // false (not in ARF yet)
```

## EU Member States

All 27 EU member states are supported:

| Code | Country        | Code | Country    |
| ---- | -------------- | ---- | ---------- |
| AT   | Austria        | BE   | Belgium    |
| BG   | Bulgaria       | CY   | Cyprus     |
| CZ   | Czech Republic | DE   | Germany    |
| DK   | Denmark        | EE   | Estonia    |
| ES   | Spain          | FI   | Finland    |
| FR   | France         | GR   | Greece     |
| HR   | Croatia        | HU   | Hungary    |
| IE   | Ireland        | IT   | Italy      |
| LT   | Lithuania      | LU   | Luxembourg |
| LV   | Latvia         | MT   | Malta      |
| NL   | Netherlands    | PL   | Poland     |
| PT   | Portugal       | RO   | Romania    |
| SE   | Sweden         | SI   | Slovenia   |
| SK   | Slovakia       |      |            |

```csharp
using SdJwt.Net.Eudiw;

// Access all member states
var allStates = EudiwConstants.MemberStates.All;
Console.WriteLine($"Supported states: {string.Join(", ", allStates)}");
```

## Trust Infrastructure

### EU Trust Lists

The EUDIW trust infrastructure is built on EU Trust Lists:

```text
LOTL (List of Trusted Lists)
   /--- Member State Trust Lists ---\
   |                                |
   v                                v
 DE Trust List              FR Trust List
   |                          |
   v                          v
TSP 1 (Bundesdruckerei)    TSP 1 (ANTS)
TSP 2 (D-Trust)            TSP 2 (Docaposte)
   ...                        ...
```

### Trust Validation

```csharp
using SdJwt.Net.Eudiw.TrustFramework;

public class EuTrustService
{
    private readonly EuTrustListResolver _resolver;

    public async Task<TrustValidationResult> ValidateIssuerAsync(
        string issuerIdentifier,
        ArfCredentialType expectedType)
    {
        // Resolve TSP from Trust Lists
        var tsp = await _resolver.ResolveAsync(issuerIdentifier);

        if (tsp == null)
        {
            return TrustValidationResult.Failed("Issuer not found in EU Trust Lists");
        }

        // Validate TSP is authorized for credential type
        var authorizedTypes = GetAuthorizedTypes(tsp.ServiceType);
        if (!authorizedTypes.Contains(expectedType))
        {
            return TrustValidationResult.Failed(
                $"TSP not authorized to issue {expectedType} credentials");
        }

        // Validate TSP status
        if (tsp.Status != TspStatus.Granted)
        {
            return TrustValidationResult.Failed($"TSP status: {tsp.Status}");
        }

        // Validate certificate chain
        return await _resolver.ValidateTrustChainAsync(tsp);
    }
}
```

## Integration with Other Packages

### With SdJwt.Net.Oid4Vp

```csharp
using SdJwt.Net.Eudiw.Arf;
using SdJwt.Net.Oid4Vp.Verifier;

public class EudiwVerifier
{
    private readonly ArfProfileValidator _arfValidator;
    private readonly VpTokenValidator _vpValidator;

    public async Task<VerificationResult> VerifyEudiwCredential(
        string vpToken,
        string expectedCredentialType)
    {
        // First validate VP token
        var vpResult = await _vpValidator.ValidateAsync(vpToken);
        if (!vpResult.IsValid)
        {
            return VerificationResult.Failed(vpResult.ErrorMessage);
        }

        // Validate ARF algorithm compliance
        if (!_arfValidator.ValidateAlgorithm(vpResult.Algorithm))
        {
            return VerificationResult.Failed("Algorithm not ARF-compliant");
        }

        // Validate credential type
        var typeResult = _arfValidator.ValidateCredentialType(vpResult.CredentialType);
        if (!typeResult.IsValid)
        {
            return VerificationResult.Failed(typeResult.ErrorMessage);
        }

        // Validate issuing country is EU member state
        if (!_arfValidator.ValidateMemberState(vpResult.IssuingCountry))
        {
            return VerificationResult.Failed("Issuer not from EU member state");
        }

        return VerificationResult.Success(vpResult.Credentials);
    }
}
```

### With SdJwt.Net.Mdoc

```csharp
using SdJwt.Net.Eudiw.Arf;
using SdJwt.Net.Mdoc.Verifier;

public class EudiwMdocVerifier
{
    private readonly ArfProfileValidator _arfValidator;
    private readonly MdocVerifier _mdocVerifier;

    public async Task<VerificationResult> VerifyPidAsync(byte[] mdocBytes)
    {
        // Verify mdoc signature
        var mdocResult = await _mdocVerifier.VerifyAsync(mdocBytes);
        if (!mdocResult.IsValid)
        {
            return VerificationResult.Failed(mdocResult.ErrorMessage);
        }

        // Validate DocType is PID
        var typeResult = _arfValidator.ValidateCredentialType(mdocResult.DocType);
        if (!typeResult.IsValid || typeResult.CredentialType != ArfCredentialType.Pid)
        {
            return VerificationResult.Failed("Credential is not a valid PID");
        }

        // Validate PID claims
        var pidResult = _arfValidator.ValidatePidClaims(mdocResult.Claims);
        if (!pidResult.IsValid)
        {
            return VerificationResult.Failed(pidResult.ErrorMessage);
        }

        return VerificationResult.Success(mdocResult.Claims);
    }
}
```

### With SdJwt.Net.HAIP

```csharp
using SdJwt.Net.Eudiw.Arf;
using SdJwt.Net.HAIP;

public class EudiwHaipValidator
{
    private readonly ArfProfileValidator _arfValidator;
    private readonly HaipCryptoValidator _haipValidator;

    public ValidationResult ValidateSecurityLevel(
        string algorithm,
        string credentialType)
    {
        // ARF validation
        if (!_arfValidator.ValidateAlgorithm(algorithm))
        {
            return ValidationResult.Failed("Algorithm not ARF-compliant");
        }

        // HAIP validation
        var haipResult = _haipValidator.ValidateAlgorithm(
            algorithm,
            HaipSecurityLevel.Level2);

        if (!haipResult.IsValid)
        {
            return ValidationResult.Failed("Algorithm does not meet HAIP Level 2");
        }

        // For PID, require Level 2+
        var typeResult = _arfValidator.ValidateCredentialType(credentialType);
        if (typeResult.CredentialType == ArfCredentialType.Pid)
        {
            var pidHaipResult = _haipValidator.ValidateAlgorithm(
                algorithm,
                HaipSecurityLevel.Level3);

            if (!pidHaipResult.IsValid)
            {
                // Level 2 is acceptable for now, warn about future requirement
                return ValidationResult.Warning(
                    "Level 2 accepted; Level 3 recommended for PID");
            }
        }

        return ValidationResult.Success();
    }
}
```

## Timeline and Adoption

| Date      | Milestone                              |
| --------- | -------------------------------------- |
| 2024 Q1   | eIDAS 2.0 regulation published         |
| 2024-2025 | Large-Scale Pilots (LSPs) testing      |
| 2025 Q4   | Member states finalize implementations |
| 2026      | EUDIW mandatory acceptance begins      |
| 2027+     | Full ecosystem operational             |

## Related Documentation

- [HAIP Compliance](haip-compliance.md) - Security requirements
- [mdoc Deep Dive](mdoc-deep-dive.md) - Mobile document format
- [OpenID4VP Deep Dive](openid4vp-deep-dive.md) - Presentation protocol
- [OpenID4VCI Deep Dive](openid4vci-deep-dive.md) - Issuance protocol
- [Status List Deep Dive](status-list-deep-dive.md) - Revocation checking

## References

- eIDAS 2.0 Regulation: <https://eur-lex.europa.eu/legal-content/EN/TXT/?uri=CELEX%3A32024R1183>
- Architecture Reference Framework: <https://digital-strategy.ec.europa.eu/en/library/european-digital-identity-wallet-architecture-and-reference-framework>
- EU Trust Lists: <https://eidas.ec.europa.eu/efda/tl-browser/>
- EUDIW Large-Scale Pilots: <https://digital-strategy.ec.europa.eu/en/policies/eudi-wallet-pilots>
