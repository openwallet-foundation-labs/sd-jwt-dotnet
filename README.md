# SD-JWT .NET Ecosystem

![SD-JWT .NET Logo](docs/images/sdjwtnet.png)

[![NuGet Version](https://img.shields.io/nuget/v/SdJwt.Net.svg)](https://www.nuget.org/packages/SdJwt.Net/)
[![CI](https://github.com/openwallet-foundation-labs/sd-jwt-dotnet/actions/workflows/ci-validation.yml/badge.svg)](https://github.com/openwallet-foundation-labs/sd-jwt-dotnet/actions/workflows/ci-validation.yml)
[![License](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](https://opensource.org/licenses/Apache-2.0)

A comprehensive, production-ready .NET ecosystem for **Selective Disclosure JSON Web Tokens (SD-JWTs)** and the complete verifiable credential stack. This project provides enterprise-grade implementations of cutting-edge identity and credential standards with enhanced security, performance optimization, and multi-platform support.

## Quick Start

```bash
# Core SD-JWT functionality
dotnet add package SdJwt.Net

# Verifiable Credentials
dotnet add package SdJwt.Net.Vc

# Try comprehensive samples
git clone https://github.com/openwallet-foundation-labs/sd-jwt-dotnet.git
cd sd-jwt-dotnet/samples/SdJwt.Net.Samples
dotnet run
```

## Package Ecosystem

### **Core**

| Package                                  | Release        | Specification                                         | Status     |
| ---------------------------------------- | -------------- | ----------------------------------------------------- | ---------- |
| **[SdJwt.Net](src/SdJwt.Net/README.md)** | NuGet (MinVer) | [RFC 9901](https://datatracker.ietf.org/doc/rfc9901/) | **Stable** |

**Core SD-JWT functionality with RFC 9901 compliance, JWS JSON Serialization, and enterprise security.**

### **Verifiable Credential Stack**

| Package                                                        | Release        | Specification                                                                                     | Status       |
| -------------------------------------------------------------- | -------------- | ------------------------------------------------------------------------------------------------- | ------------ |
| **[SdJwt.Net.Vc](src/SdJwt.Net.Vc/README.md)**                 | NuGet (MinVer) | [draft-ietf-oauth-sd-jwt-vc-15](https://datatracker.ietf.org/doc/draft-ietf-oauth-sd-jwt-vc/)     | **Draft-15** |
| **[SdJwt.Net.StatusList](src/SdJwt.Net.StatusList/README.md)** | NuGet (MinVer) | [draft-ietf-oauth-status-list-18](https://datatracker.ietf.org/doc/draft-ietf-oauth-status-list/) | **Draft-18** |

**Complete verifiable credential lifecycle with revocation, suspension, and status management.**

### **OpenID Identity Protocols**

| Package                                                  | Release        | Specification                                                                               | Status     |
| -------------------------------------------------------- | -------------- | ------------------------------------------------------------------------------------------- | ---------- |
| **[SdJwt.Net.Oid4Vci](src/SdJwt.Net.Oid4Vci/README.md)** | NuGet (MinVer) | [OpenID4VCI 1.0](https://openid.net/specs/openid-4-verifiable-credential-issuance-1_0.html) | **Stable** |
| **[SdJwt.Net.Oid4Vp](src/SdJwt.Net.Oid4Vp/README.md)**   | NuGet (MinVer) | [OpenID4VP 1.0](https://openid.net/specs/openid-4-verifiable-presentations-1_0.html)        | **Stable** |

**Complete credential issuance and presentation verification protocols.**

### **Advanced Trust & Security**

| Package                                                                            | Release        | Specification                                                                                             | Status     |
| ---------------------------------------------------------------------------------- | -------------- | --------------------------------------------------------------------------------------------------------- | ---------- |
| **[SdJwt.Net.OidFederation](src/SdJwt.Net.OidFederation/README.md)**               | NuGet (MinVer) | [OpenID Federation 1.0](https://openid.net/specs/openid-federation-1_0.html)                              | **Stable** |
| **[SdJwt.Net.PresentationExchange](src/SdJwt.Net.PresentationExchange/README.md)** | NuGet (MinVer) | [DIF PEX v2.1.1](https://identity.foundation/presentation-exchange/spec/v2.1.1/)                          | **Stable** |
| **[SdJwt.Net.HAIP](src/SdJwt.Net.HAIP/README.md)**                                 | NuGet (MinVer) | [HAIP 1.0](https://openid.net/specs/openid4vc-high-assurance-interoperability-profile-sd-jwt-vc-1_0.html) | **Draft**  |

**Enterprise federation, trust management, intelligent credential selection, and high assurance compliance.**

### **ISO Credential Formats**

| Package                                            | Release        | Specification                                              | Status     |
| -------------------------------------------------- | -------------- | ---------------------------------------------------------- | ---------- |
| **[SdJwt.Net.Mdoc](src/SdJwt.Net.Mdoc/README.md)** | NuGet (MinVer) | [ISO 18013-5](https://www.iso.org/standard/69084.html) mDL | **Stable** |

**ISO 18013-5 mobile document (mdoc/mDL) support for driver's licenses and government credentials.**

### **Wallet Infrastructure**

| Package                                                | Release        | Specification                                                          | Status     |
| ------------------------------------------------------ | -------------- | ---------------------------------------------------------------------- | ---------- |
| **[SdJwt.Net.Wallet](src/SdJwt.Net.Wallet/README.md)** | NuGet (MinVer) | Generic wallet with plugin architecture                                | **Stable** |
| **[SdJwt.Net.Eudiw](src/SdJwt.Net.Eudiw/README.md)**   | NuGet (MinVer) | [eIDAS 2.0](https://eur-lex.europa.eu/eli/reg/2024/1183) EU Wallet ARF | **Stable** |

**Digital credential wallet infrastructure with EU Digital Identity Wallet (EUDIW) support.**

## Key Features

### Enterprise Security

- **RFC 9901 Compliant**: Full implementation with security hardening
- **HAIP Support**: High Assurance Interoperability Profile for government and enterprise
- **Algorithm Enforcement**: Blocks weak algorithms (MD5, SHA-1), enforces SHA-2 family
- **Attack Prevention**: Protection against timing attacks, replay attacks, signature tampering
- **Zero-Trust Architecture**: Cryptographic verification at every layer

### High Performance

- **Multi-Platform Optimized**: .NET 8, 9, 10 and .NET Standard 2.1
- **Modern Cryptography**: Platform-specific optimizations (SHA256.HashData() on .NET 6+)
- **Scalable Operations**: Optimized for high-throughput issuance and verification
- **Memory Efficient**: Optimized allocation patterns for high-volume scenarios

### Standards Compliant

- **IETF Standards**: RFC 9901 and SD-JWT VC draft-15
- **OpenID Foundation**: Complete protocol implementations
- **W3C Alignment**: Verifiable Credentials data model compatibility
- **DIF Integration**: Presentation Exchange v2.1.1 support
- **HAIP Compliance**: High assurance security profiles

### Developer Experience

- **Comprehensive Samples**: 19 hands-on tutorials organized by skill level
- **Fluent APIs**: Intuitive, discoverable interfaces
- **Rich Documentation**: Detailed guides with security considerations
- **Production Ready**: Battle-tested with 1400+ comprehensive tests

## Use Cases

### Government & Civic (HAIP Level 3 - Sovereign)

```csharp
// Digital identity for citizens accessing government services
var citizenCredential = await governmentIssuer.IssueDigitalIdAsync(citizen);
var ageProof = citizen.CreateAgeVerificationPresentation(minimumAge: 18);
await servicePortal.VerifyAndGrantAccessAsync(ageProof);
```

### Education & Credentials

```csharp
// University issues degree, student presents to employer
var degree = await university.IssueDegreeCredentialAsync(graduate);
var jobPresentation = graduate.CreateProfessionalPresentation(
    disclosure => disclosure.ClaimName is "degree" or "gpa" or "honors");
await employer.VerifyQualificationsAsync(jobPresentation);
```

### Healthcare & Privacy

```csharp
// Patient shares medical data with specialist
var medicalRecord = await hospital.IssueMedicalCredentialAsync(patient);
var specialistPresentation = patient.CreateSelectiveMedicalPresentation(
    shareConditions: ["allergies", "current_medications"],
    protectInfo: ["full_history", "mental_health"]);
await specialist.ProcessPatientDataAsync(specialistPresentation);
```

### Financial Services (HAIP Level 2 - Very High)

```csharp
// Privacy-preserving loan application with HAIP compliance
var employmentCredential = await employer.IssueEmploymentVerificationAsync(applicant);
var incomePresentation = applicant.CreateIncomeVerificationPresentation(
    disclose: ["employment_status", "salary_range"],
    protect: ["exact_salary", "performance_reviews"]);
await bank.ProcessLoanApplicationAsync(incomePresentation);
```

### Mobile Driving License (ISO 18013-5 mdoc)

```csharp
// DMV issues mDL, citizen presents at TSA checkpoint
using SdJwt.Net.Mdoc.Issuer;
using SdJwt.Net.Mdoc.Namespaces;

var mdl = await new MdocIssuerBuilder()
    .WithDocType("org.iso.18013.5.1.mDL")
    .WithIssuerKey(dmvSigningKey)
    .WithDeviceKey(citizenDeviceKey)
    .AddMdlElement(MdlDataElement.FamilyName, "Johnson")
    .AddMdlElement(MdlDataElement.GivenName, "Alice")
    .AddMdlElement(MdlDataElement.AgeOver21, true)
    .BuildAsync(cryptoProvider);

// Citizen presents only age verification (not birthdate)
await checkpoint.VerifyAgeOnlyAsync(mdl, selectElements: ["age_over_21"]);
```

## Architecture Overview

```mermaid
graph TB
    subgraph ApplicationLayer[Application Layer]
        WalletApp[Wallet Application]
        IssuerApp[Issuer Service]
        VerifierApp[Verifier Service]
        GovApp[Government Portal]
    end

    subgraph ProtocolLayer[Protocol Layer]
        OID4VCI[SdJwt.Net.Oid4Vci: Credential Issuance]
        OID4VP[SdJwt.Net.Oid4Vp: Presentations]
        PEx[SdJwt.Net.PresentationExchange: DIF PE v2.1.1]
        OidFed[SdJwt.Net.OidFederation: Trust Chains]
    end

    subgraph ComplianceLayer[Compliance Layer]
        HAIP[SdJwt.Net.HAIP: Level 1 / 2 / 3]
    end

    subgraph CoreLayer[Core Layer]
        Core[SdJwt.Net: RFC 9901]
        Vc[SdJwt.Net.Vc: W3C VC]
        Status[SdJwt.Net.StatusList: Revocation]
        Mdoc[SdJwt.Net.Mdoc: ISO 18013-5]
    end

    WalletApp --> OID4VP
    WalletApp --> OID4VCI
    IssuerApp --> OID4VCI
    VerifierApp --> OID4VP
    VerifierApp --> PEx
    GovApp --> HAIP

    OID4VCI --> HAIP
    OID4VP --> HAIP
    PEx --> HAIP
    OidFed --> HAIP

    HAIP --> Core
    HAIP --> Vc
    HAIP --> Status
    HAIP --> Mdoc
    OidFed --> Core
    OID4VP --> Mdoc

    style HAIP fill:#d62828,color:#fff
    style Core fill:#1b4332,color:#fff
    style Mdoc fill:#2a6478,color:#fff
```

## Quick Examples

### **Basic SD-JWT**

```csharp
using SdJwt.Net.Issuer;

// Create issuer
var issuer = new SdIssuer(signingKey, SecurityAlgorithms.EcdsaSha256);

// Issue with selective disclosure
var credential = issuer.Issue(claims, new SdIssuanceOptions
{
    DisclosureStructure = new { email = true, address = new { city = true } }
});

// Holder creates presentation
var holder = new SdJwtHolder(credential.Issuance);
var presentation = holder.CreatePresentation(
    disclosure => disclosure.ClaimName == "email");
```

### **HAIP-Compliant Verifiable Credentials**

```csharp
using SdJwt.Net.Vc.Issuer;
using SdJwt.Net.HAIP;

// Government issuer with Level 3 compliance
var haipValidator = new HaipCryptoValidator(HaipLevel.Level3_Sovereign, logger);
var keyValidation = haipValidator.ValidateKeyCompliance(signingKey, "ES512");

if (keyValidation.IsCompliant)
{
    var vcIssuer = new SdJwtVcIssuer(issuerKey, algorithm);
    var credential = vcIssuer.Issue("https://gov.example/national-id", vcPayload, options);
}
```

### **Status Management**

```csharp
using SdJwt.Net.StatusList.Issuer;

// Create status list
var statusManager = new StatusListManager(statusKey, algorithm);
var statusValues = new byte[] { 0, 1, 2 }; // valid, invalid, suspended
var statusList = await statusManager.CreateStatusListTokenAsync(
    statusListUrl, statusValues, bits: 2);

// Check credential status
var statusVerifier = new StatusListVerifier(httpClient);
var statusResult = await statusVerifier.CheckStatusAsync(statusClaim, keyResolver);
var isValid = statusResult.IsValid;

// Verify presentation with expected nonce
var result = await verifier.VerifyAsync(presentation, validationParams, kbParams, "expected-nonce");
```

## Security Features

### Cryptographic Security

- **Approved**: SHA-256, SHA-384, SHA-512, ECDSA P-256/384/521
- **Blocked**: MD5, SHA-1 (automatically rejected)
- **Enhanced**: Constant-time operations, secure random generation

### **HAIP Compliance Levels**

- **Level 1 (High)**: ES256+, PS256+, proof of possession
- **Level 2 (Very High)**: ES384+, PS384+, wallet attestation, DPoP
- **Level 3 (Sovereign)**: ES512+, PS512+, HSM backing, qualified signatures

### **Attack Prevention**

- **Signature Tampering**: Cryptographic detection and prevention
- **Replay Attacks**: Nonce and timestamp validation
- **Timing Attacks**: Constant-time comparison operations
- **Key Confusion**: Strong key binding validation

### **Privacy Protection**

- **Selective Disclosure**: Granular claim-level privacy control
- **Zero-Knowledge Patterns**: Prove properties without revealing data
- **Context Isolation**: Audience-specific presentations
- **Correlation Resistance**: Multiple unlinkable presentations

## Platform Support

### **Supported Frameworks**

- **.NET 8.0** - Full support with modern optimizations
- **.NET 9.0** - Latest features and optimal performance
- **.NET 10.0** - Full support
- **.NET Standard 2.1** - Backward compatibility for legacy systems

### **Supported Platforms**

- **Windows** (x64, x86, ARM64)
- **Linux** (x64, ARM64)
- **macOS** (x64, Apple Silicon)
- **Container Ready** (Docker, Kubernetes)
- **Cloud Native** (Azure, AWS, GCP)

## Performance Benchmarks

Performance is measured with a real BenchmarkDotNet harness in [`benchmarks/SdJwt.Net.Benchmarks`](benchmarks/SdJwt.Net.Benchmarks).

Run benchmarks locally:

```pwsh
dotnet run --configuration Release --project benchmarks/SdJwt.Net.Benchmarks/SdJwt.Net.Benchmarks.csproj -- --job short --warmupCount 1 --iterationCount 3 --exporters markdown json
```

Benchmark results are generated in:

- `benchmarks/SdJwt.Net.Benchmarks/BenchmarkDotNet.Artifacts/results/`

The CI `performance-benchmarks` job executes the same harness and uploads result artifacts for each run.

## Documentation

### **Getting Started**

- [Documentation Portal](docs/README.md) - Main entry point to all documentation
- [15-Minute Quickstart](docs/getting-started/quickstart.md) - Tutorial to get up and running quickly
- [Ecosystem Architecture](docs/concepts/architecture.md) - Deep dive into system architecture
- [Interactive Samples](samples/SdJwt.Net.Samples/README.md) - 19 tutorials with interactive CLI (Beginner to Advanced)
- [Package Documentation](src/SdJwt.Net/README.md) - Core package API reference

### **Standards Implementation**

- [Verifiable Credentials](src/SdJwt.Net.Vc/README.md) - SD-JWT VC specification
- [Status Lists](src/SdJwt.Net.StatusList/README.md) - Credential lifecycle management
- [OpenID4VCI](src/SdJwt.Net.Oid4Vci/README.md) - Credential issuance protocols
- [OpenID4VP](src/SdJwt.Net.Oid4Vp/README.md) - Presentation protocols
- [mdoc/mDL](src/SdJwt.Net.Mdoc/README.md) - ISO 18013-5 mobile documents

### **Advanced Features**

- [OpenID Federation](src/SdJwt.Net.OidFederation/README.md) - Trust chain management
- [Presentation Exchange](src/SdJwt.Net.PresentationExchange/README.md) - Credential selection
- [HAIP Compliance](src/SdJwt.Net.HAIP/README.md) - High assurance security profiles

### **Enterprise Planning**

- [Enterprise Roadmap](docs/ENTERPRISE_ROADMAP.md) - Strategic roadmap with ISO mDL/mdoc, DC API, eIDAS 2.0

## Installation

### **Core Package**

```bash
dotnet add package SdJwt.Net
```

### **Complete Ecosystem**

```bash
# Full verifiable credential stack
dotnet add package SdJwt.Net
dotnet add package SdJwt.Net.Vc
dotnet add package SdJwt.Net.StatusList

# OpenID protocols
dotnet add package SdJwt.Net.Oid4Vci
dotnet add package SdJwt.Net.Oid4Vp

# Advanced features
dotnet add package SdJwt.Net.OidFederation
dotnet add package SdJwt.Net.PresentationExchange
dotnet add package SdJwt.Net.HAIP

# ISO credential formats
dotnet add package SdJwt.Net.Mdoc
```

### **Try Comprehensive Examples**

```bash
git clone https://github.com/openwallet-foundation-labs/sd-jwt-dotnet.git
cd sd-jwt-dotnet/samples/SdJwt.Net.Samples
dotnet run
```

## Contributing

We welcome contributions! Please see the [CONTRIBUTING.md](CONTRIBUTING.md) file for detailed guidelines and instructions.

## Community & Support

### Getting Help

- **Documentation**: Comprehensive guides and API reference
- **Discussions**: [GitHub Discussions](https://github.com/openwallet-foundation-labs/sd-jwt-dotnet/discussions) for community questions
- **Issues**: [GitHub Issues](https://github.com/openwallet-foundation-labs/sd-jwt-dotnet/issues) for bug reports
- **Security**: Report security issues to [tldinteractive@gmail.com](mailto:tldinteractive@gmail.com) or see [SECURITY.md](SECURITY.md)

### **Community**

- **Open Wallet Foundation**: Part of the [OpenWallet Foundation](https://openwallet.foundation/) ecosystem
- **Standards Participation**: Active in IETF OAuth WG, OpenID Foundation, DIF
- **Industry Collaboration**: Working with implementers across industries

## License

Licensed under the **Apache License 2.0** - see the [LICENSE](LICENSE.txt) file for details.

This permissive license allows commercial use, modification, distribution, and private use while providing license and copyright notice requirements.

## Acknowledgments

This project builds upon the excellent work of the global identity standards community:

- **[IETF OAuth Working Group](https://datatracker.ietf.org/wg/oauth/)** - SD-JWT and Status List specifications
- **[OpenID Foundation](https://openid.net/)** - OpenID4VCI, OpenID4VP, Federation, and HAIP standards
- **[DIF](https://identity.foundation/)** - Presentation Exchange specification
- **[W3C](https://www.w3.org/)** - Verifiable Credentials data model
- **[Open Wallet Foundation](https://openwallet.foundation/)** - Digital identity standards advancement

### **Special Thanks**

- All specification editors and contributors
- Early adopters and feedback providers
- Security researchers and auditors
- The broader .NET and identity communities

---
