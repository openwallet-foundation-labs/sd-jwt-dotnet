# SD-JWT .NET Ecosystem

[![NuGet Version](https://img.shields.io/nuget/v/SdJwt.Net.svg)](https://www.nuget.org/packages/SdJwt.Net/)
[![GitHub Actions](https://github.com/openwalletfoundation/sd-jwt-dotnet/workflows/.NET%20CI%2FCD/badge.svg)](https://github.com/openwalletfoundation/sd-jwt-dotnet/actions)
[![License](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](https://opensource.org/licenses/Apache-2.0)
[![OpenSSF Scorecard](https://api.securityscorecards.dev/projects/github.com/openwalletfoundation/sd-jwt-dotnet/badge)](https://securityscorecards.dev/viewer/?uri=github.com/openwalletfoundation/sd-jwt-dotnet)

A comprehensive, production-ready .NET ecosystem for **Selective Disclosure JSON Web Tokens (SD-JWTs)** and the complete verifiable credential stack. This project provides enterprise-grade implementations of cutting-edge identity and credential standards with enhanced security, performance optimization, and multi-platform support.

## 🚀 **Quick Start**

```bash
# Core SD-JWT functionality
dotnet add package SdJwt.Net

# Verifiable Credentials
dotnet add package SdJwt.Net.Vc

# Try comprehensive samples
git clone https://github.com/openwalletfoundation/sd-jwt-dotnet.git
cd sd-jwt-dotnet/samples/SdJwt.Net.Samples
dotnet run
```

## 📦 **Package Ecosystem**

### **Production-Ready Core**
| Package | Version | Specification | Status |
|---------|---------|---------------|---------|
| **[SdJwt.Net](README-Core.md)** | 1.0.0 | [RFC 9901](https://tools.ietf.org/rfc/rfc9901.txt) | ✅ **Stable** |

**Core SD-JWT functionality with RFC 9901 compliance, JWS JSON Serialization, and enterprise security.**

### **Verifiable Credential Stack**
| Package | Version | Specification | Status |
|---------|---------|---------------|---------|
| **[SdJwt.Net.Vc](README-Vc.md)** | 0.13.0 | [draft-ietf-oauth-sd-jwt-vc-13](https://datatracker.ietf.org/doc/draft-ietf-oauth-sd-jwt-vc/) | 🟡 **Draft** |
| **[SdJwt.Net.StatusList](README-StatusList.md)** | 0.13.0 | [draft-ietf-oauth-status-list-13](https://datatracker.ietf.org/doc/draft-ietf-oauth-status-list/) | 🟡 **Draft** |

**Complete verifiable credential lifecycle with revocation, suspension, and status management.**

### **OpenID Identity Protocols**
| Package | Version | Specification | Status |
|---------|---------|---------------|---------|
| **[SdJwt.Net.Oid4Vci](README-Oid4Vci.md)** | 1.0.0 | [OpenID4VCI 1.0](https://openid.net/specs/openid-4-verifiable-credential-issuance-1_0.html) | ✅ **Stable** |
| **[SdJwt.Net.Oid4Vp](README-Oid4Vp.md)** | 1.0.0 | [OpenID4VP 1.0](https://openid.net/specs/openid-4-verifiable-presentations-1_0.html) | ✅ **Stable** |

**Complete credential issuance and presentation verification protocols.**

### **Advanced Trust & Exchange**
| Package | Version | Specification | Status |
|---------|---------|---------------|---------|
| **[SdJwt.Net.OidFederation](README-OidFederation.md)** | 1.0.0 | [OpenID Federation 1.0](https://openid.net/specs/openid-federation-1_0.html) | ✅ **Stable** |
| **[SdJwt.Net.PresentationExchange](README-PresentationExchange.md)** | 2.0.0 | [DIF PEX v2.0.0](https://identity.foundation/presentation-exchange/spec/v2.0.0/) | ✅ **Stable** |

**Enterprise federation, trust management, and intelligent credential selection.**

## 🌟 **Key Features**

### **🔐 Enterprise Security**
- **RFC 9901 Compliant**: Full implementation with security hardening
- **Algorithm Enforcement**: Blocks weak algorithms (MD5, SHA-1), enforces SHA-2 family
- **Attack Prevention**: Protection against timing attacks, replay attacks, signature tampering
- **Zero-Trust Architecture**: Cryptographic verification at every layer

### **🚄 High Performance**
- **Multi-Platform Optimized**: .NET 8, 9, and .NET Standard 2.1
- **Modern Cryptography**: Platform-specific optimizations (SHA256.HashData() on .NET 6+)
- **Scalable Operations**: 1,000+ ops/sec for issuance, 10,000+ ops/sec for status checks
- **Memory Efficient**: Optimized allocation patterns for high-throughput scenarios

### **🔄 Standards Compliant**
- **IETF Standards**: RFC 9901, draft-13 specifications
- **OpenID Foundation**: Complete protocol implementations
- **W3C Alignment**: Verifiable Credentials data model compatibility
- **DIF Integration**: Presentation Exchange v2.0.0 support

### **🛠️ Developer Experience**
- **Comprehensive Samples**: 12+ example implementations covering all use cases
- **Fluent APIs**: Intuitive, discoverable interfaces
- **Rich Documentation**: Detailed guides with security considerations
- **Production Ready**: Battle-tested with 200+ comprehensive tests

## 🎯 **Use Cases**

### **🏛️ Government & Civic**
```csharp
// Digital identity for citizens accessing government services
var citizenCredential = await governmentIssuer.IssueDigitalIdAsync(citizen);
var ageProof = citizen.CreateAgeVerificationPresentation(minimumAge: 18);
await servicePortal.VerifyAndGrantAccessAsync(ageProof);
```

### **🎓 Education & Credentials**
```csharp
// University issues degree, student presents to employer
var degree = await university.IssueDegreeCredentialAsync(graduate);
var jobPresentation = graduate.CreateProfessionalPresentation(
    disclosure => disclosure.ClaimName is "degree" or "gpa" or "honors");
await employer.VerifyQualificationsAsync(jobPresentation);
```

### **🏥 Healthcare & Privacy**
```csharp
// Patient shares medical data with specialist
var medicalRecord = await hospital.IssueMedicalCredentialAsync(patient);
var specialistPresentation = patient.CreateSelectiveMedicalPresentation(
    shareConditions: ["allergies", "current_medications"],
    protectInfo: ["full_history", "mental_health"]);
await specialist.ProcessPatientDataAsync(specialistPresentation);
```

### **🏦 Financial Services**
```csharp
// Privacy-preserving loan application
var employmentCredential = await employer.IssueEmploymentVerificationAsync(applicant);
var incomePresentation = applicant.CreateIncomeVerificationPresentation(
    disclose: ["employment_status", "salary_range"],
    protect: ["exact_salary", "performance_reviews"]);
await bank.ProcessLoanApplicationAsync(incomePresentation);
```

## 🏗️ **Architecture Overview**

```
┌─────────────────────────────────────────────────────────┐
│                    Application Layer                     │
├─────────────────────────────────────────────────────────┤
│              Protocol Implementations                    │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  │
│  │   OID4VCI    │  │    OID4VP    │  │ Federation   │  │
│  │              │  │              │  │   & Trust    │  │
│  └──────────────┘  └──────────────┘  └──────────────┘  │
├─────────────────────────────────────────────────────────┤
│           Verifiable Credential Layer                   │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  │
│  │   SD-JWT VC  │  │ Status Lists │  │Presentation  │  │
│  │              │  │& Revocation  │  │  Exchange    │  │
│  └──────────────┘  └──────────────┘  └──────────────┘  │
├─────────────────────────────────────────────────────────┤
│                   Core SD-JWT Layer                     │
│         ┌─────────────────────────────────────┐         │
│         │           SdJwt.Net Core           │         │
│         │     RFC 9901 Implementation       │         │
│         │   Selective Disclosure Engine     │         │
│         └─────────────────────────────────────┘         │
└─────────────────────────────────────────────────────────┘
```

## 💡 **Quick Examples**

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

### **Verifiable Credentials**
```csharp
using SdJwt.Net.Vc.Issuer;

// Issue VC with status
var vcIssuer = new SdJwtVcIssuer(issuerKey, algorithm);
var credential = vcIssuer.Issue("https://university.edu/degree", vcPayload, options);

// Verify with status checking
var vcVerifier = new SdJwtVcVerifier(keyResolver);
var result = await vcVerifier.VerifyAsync(presentation, validationParams);
```

### **Status Management**
```csharp
using SdJwt.Net.StatusList.Issuer;

// Create status list
var statusManager = new StatusListManager(statusKey, algorithm);
var statusList = await statusManager.CreateStatusListTokenAsync(
    statusListUrl, credentialStatuses);

// Check credential status
var statusVerifier = new StatusListVerifier(httpClient);
var isValid = await statusVerifier.CheckStatusAsync(statusClaim, keyResolver);
```

## 🛡️ **Security Features**

### **Cryptographic Security**
- **✅ Approved**: SHA-256, SHA-384, SHA-512, ECDSA P-256/384/521
- **❌ Blocked**: MD5, SHA-1 (automatically rejected)
- **🔐 Enhanced**: Constant-time operations, secure random generation

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

## 🌐 **Platform Support**

### **Supported Frameworks**
- **.NET 8.0** - Full support with modern optimizations
- **.NET 9.0** - Latest features and optimal performance  
- **.NET Standard 2.1** - Backward compatibility for legacy systems

### **Supported Platforms**
- **Windows** (x64, x86, ARM64)
- **Linux** (x64, ARM64) 
- **macOS** (x64, Apple Silicon)
- **Container Ready** (Docker, Kubernetes)
- **Cloud Native** (Azure, AWS, GCP)

## 📈 **Performance Benchmarks**

| Operation | Throughput | Latency | Memory |
|-----------|------------|---------|--------|
| **SD-JWT Issuance** | 1,000+ ops/sec | < 1ms | ~2KB |
| **Presentation Creation** | 2,000+ ops/sec | < 0.5ms | ~1KB |
| **Verification** | 1,500+ ops/sec | < 0.7ms | ~1.5KB |
| **Status List Check** | 10,000+ ops/sec | < 0.1ms | ~512B |

*Benchmarks measured on .NET 9, x64, with P-256 ECDSA*

## 📚 **Documentation**

### **Getting Started**
- **[Comprehensive Samples](samples/SdJwt.Net.Samples/README.md)** - Interactive examples covering all features
- **[Core SD-JWT Guide](README-Core.md)** - Essential selective disclosure concepts
- **[Security Best Practices](docs/security.md)** - Production deployment guidance

### **Specification Implementation**
- **[RFC 9901 Implementation](docs/rfc9901.md)** - Complete SD-JWT specification coverage
- **[Verifiable Credentials](README-Vc.md)** - draft-ietf-oauth-sd-jwt-vc-13
- **[Status Lists](README-StatusList.md)** - draft-ietf-oauth-status-list-13

### **Protocol Integration**
- **[OpenID4VCI Implementation](README-Oid4Vci.md)** - Credential issuance workflows
- **[OpenID4VP Implementation](README-Oid4Vp.md)** - Presentation verification
- **[Federation & Trust](README-OidFederation.md)** - Trust chain management

## 🚀 **Getting Started**

### **1. Choose Your Package**
```bash
# Basic selective disclosure
dotnet add package SdJwt.Net

# Verifiable credentials
dotnet add package SdJwt.Net.Vc

# Complete ecosystem
dotnet add package SdJwt.Net
dotnet add package SdJwt.Net.Vc  
dotnet add package SdJwt.Net.StatusList
dotnet add package SdJwt.Net.Oid4Vci
dotnet add package SdJwt.Net.Oid4Vp
```

### **2. Run Comprehensive Examples**
```bash
git clone https://github.com/openwalletfoundation/sd-jwt-dotnet.git
cd sd-jwt-dotnet/samples/SdJwt.Net.Samples
dotnet run
```

### **3. Explore Real-World Scenarios**
The samples include complete end-to-end workflows:
- University degree to bank loan application
- Job application with background verification  
- Healthcare data sharing with consent
- Government service access with privacy

## 🤝 **Contributing**

We welcome contributions! This project follows open development practices:

### **Development Setup**
```bash
git clone https://github.com/openwalletfoundation/sd-jwt-dotnet.git
cd sd-jwt-dotnet
dotnet restore
dotnet build
dotnet test
```

### **Contribution Areas**
- **🐛 Bug Reports**: Security issues, compatibility problems
- **✨ Feature Requests**: New protocol support, performance improvements
- **📖 Documentation**: Guides, examples, API documentation
- **🧪 Testing**: Platform testing, specification compliance
- **🔧 Samples**: Real-world integration patterns

### **Quality Standards**
- **Security First**: All contributions reviewed for security implications
- **Test Coverage**: Comprehensive test suite for all changes
- **Documentation**: Clear documentation for public APIs
- **Cross-Platform**: Ensure compatibility across supported platforms

## 📊 **Project Status**

### **Stability Matrix**

| Component | Status | Test Coverage | Production Ready |
|-----------|--------|---------------|------------------|
| **Core SD-JWT** | ✅ Stable | 95%+ | ✅ Yes |
| **Verifiable Credentials** | 🟡 Draft | 90%+ | ⚠️ Spec Draft |
| **Status Lists** | 🟡 Draft | 85%+ | ⚠️ Spec Draft |  
| **OpenID4VCI** | ✅ Stable | 90%+ | ✅ Yes |
| **OpenID4VP** | ✅ Stable | 85%+ | ✅ Yes |
| **Federation** | ✅ Stable | 80%+ | ✅ Yes |
| **Presentation Exchange** | ✅ Stable | 90%+ | ✅ Yes |

### **Release Cadence**
- **Major Releases**: Align with specification updates
- **Minor Releases**: New features, non-breaking changes
- **Patch Releases**: Security fixes, bug fixes (as needed)

## 🌟 **Community & Support**

### **Getting Help**
- **📖 Documentation**: Comprehensive guides and API reference
- **💬 Discussions**: [GitHub Discussions](https://github.com/openwalletfoundation/sd-jwt-dotnet/discussions) for community questions
- **🐛 Issues**: [GitHub Issues](https://github.com/openwalletfoundation/sd-jwt-dotnet/issues) for bug reports
- **🔒 Security**: security@openwallet.foundation for security issues

### **Community**
- **Open Wallet Foundation**: Part of the [OpenWallet Foundation](https://openwallet.foundation/) ecosystem
- **Standards Participation**: Active in IETF OAuth WG, OpenID Foundation, DIF
- **Industry Collaboration**: Working with implementers across industries

## 📜 **License**

Licensed under the **Apache License 2.0** - see the [LICENSE](LICENSE.txt) file for details.

This permissive license allows commercial use, modification, distribution, and private use while providing license and copyright notice requirements.

## 🙏 **Acknowledgments**

This project builds upon the excellent work of the global identity standards community:

- **[IETF OAuth Working Group](https://datatracker.ietf.org/wg/oauth/)** - SD-JWT and Status List specifications
- **[OpenID Foundation](https://openid.net/)** - OpenID4VCI, OpenID4VP, and Federation standards  
- **[DIF](https://identity.foundation/)** - Presentation Exchange specification
- **[W3C](https://www.w3.org/)** - Verifiable Credentials data model
- **[Open Wallet Foundation](https://openwallet.foundation/)** - Digital identity standards advancement

### **Special Thanks**
- All specification editors and contributors
- Early adopters and feedback providers
- Security researchers and auditors
- The broader .NET and identity communities

---

<div align="center">

**🚀 Ready to build the future of digital identity with .NET? 🚀**

[**Get Started →**](samples/SdJwt.Net.Samples/README.md) | [**View Documentation →**](docs/) | [**Join Community →**](https://github.com/openwalletfoundation/sd-jwt-dotnet/discussions)

**Selective disclosure meets enterprise .NET development.**

</div>
