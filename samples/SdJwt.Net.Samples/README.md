# SD-JWT .NET Samples

This directory contains comprehensive examples and real-world scenarios demonstrating the complete SD-JWT .NET ecosystem. The samples are professionally organized into logical categories for easy navigation and learning progression.

## Directory Organization

### [Core/](Core/) - Fundamental SD-JWT Features

Learn the essential concepts of Selective Disclosure JSON Web Tokens (RFC 9901):

- **CoreSdJwtExample.cs** - Basic SD-JWT creation and verification
- **JsonSerializationExample.cs** - Alternative serialization formats
- **SecurityFeaturesExample.cs** - Production security patterns

 [Start here with Core README](Core/README.md)

### [Standards/](Standards/) - Protocol & Standards Compliance

Explore industry standards and protocols:

- **VerifiableCredentials/** - SD-JWT VC (draft-15) and Status List (draft-13)
- **OpenId/** - OpenID4VCI, OpenID4VP, OpenID Federation
- **PresentationExchange/** - DIF Presentation Exchange v2.1.1

 [Choose a standard with Standards README](Standards/README.md)

### [Integration/](Integration/) - Advanced Multi-Package Features

See how packages work together:

- **ComprehensiveIntegrationExample.cs** - Full ecosystem workflows
- **CrossPlatformFeaturesExample.cs** - .NET compatibility patterns

### [HAIP/](HAIP/) - High Assurance Interoperability Profile

Implement government-grade security:

- **BasicHaipExample.cs** - Level 1: Basic assurance
- **EnterpriseHaipExample.cs** - Level 2: Very High assurance
- **GovernmentHaipExample.cs** - Level 3: Sovereign identity

 [Understand HAIP levels with HAIP README](HAIP/README.md)

### [RealWorld/](RealWorld/) - Production-Ready Scenarios

See complete industry implementations:

- **RealWorldScenarios.cs** - Four end-to-end use case implementations
- **Financial/FinancialCoPilotScenario.cs** - Privacy-preserving AI advisor
- **Financial/EnhancedFinancialCoPilotScenario.cs** - Full ecosystem integration

 [Explore complete workflows with Financial Co-Pilot docs](./RealWorld/Financial/README.md)

### [Infrastructure/](Infrastructure/) - Supporting Code

Shared utilities and test data:

- **Configuration/** - JSON serialization options
- **Data/** - Sample credentials and test data

## Quick Start

### Run All Examples

```bash
cd samples/SdJwt.Net.Samples
dotnet run
```

You'll see an interactive menu:

```
==================================================================
                 SD-JWT .NET Sample Explorer                    
==================================================================

CORE FEATURES:
1. Core SD-JWT Example
2. JSON Serialization Example  
3. Security Features Example

VERIFIABLE CREDENTIALS:
4. Verifiable Credentials Example
5. Status Lists Example

PROTOCOL INTEGRATION:
6. OpenID4VCI Example
7. OpenID4VP Example
8. OpenID Federation Example
9. Presentation Exchange Example

ADVANCED FEATURES:
A. Comprehensive Integration Example
B. Cross-Platform Features Example

REAL-WORLD SCENARIOS:
C. Real-World Use Cases
F. Financial Co-Pilot (AI-Powered)

0. Exit
```

### Recommended Learning Path

#### Beginner (30-45 minutes)

1. **Options 1-3** (Core SD-JWT)
   - Understand selective disclosure basics
   - See key binding in action
   - Learn security best practices

#### Intermediate (60-90 minutes)

2. **Options 4-7** (Verifiable Credentials & Protocols)
   - Work with industry-standard credentials
   - Learn credential issuance (OID4VCI)
   - Master presentation flows (OID4VP)

#### Advanced (90-120 minutes)

3. **Options 8-9, A-B** (Federation & Integration)
   - Set up trust chains (OpenID Federation)
   - Use intelligent selection (Presentation Exchange)
   - Combine multiple packages

#### Expert (Production Ready)

4. **Options C, F** (Real-World Scenarios)
   - Implement complete use cases
   - See all packages working together
   - Deploy production patterns

## Documentation by Topic

### By Complexity

- **Beginner**: [Core README](Core/README.md)
- **Intermediate**: [Standards README](Standards/README.md)
- **Advanced**: [Integration examples](Integration/)
- **Expert**: [Real-World scenarios](RealWorld/) & [Financial Co-Pilot](./RealWorld/Financial/README.md)

### By Use Case

- **Education**: Degrees & certifications (Standards examples)
- **Finance**: Account opening, loan verification (Financial Co-Pilot)
- **Government**: ID verification, access control (HAIP examples)
- **Healthcare**: Patient consent, medical records (Standards examples)

### By Standard

- **RFC 9901** (Core SD-JWT): [Core README](Core/README.md)
- **Draft-15/13** (SD-JWT VC + Status List): [Standards README](Standards/README.md)
- **OID4VCI/VP**: [Standards README](Standards/README.md)
- **OpenID Federation**: [Standards README](Standards/README.md)
- **DIF PE**: [Standards README](Standards/README.md)
- **HAIP**: [HAIP README](HAIP/README.md)

## Package Ecosystem Overview

All 8 packages demonstrated:

| Package | Version | Standard | Example | Use Case |
|---------|---------|----------|---------|----------|
| **SdJwt.Net** | MinVer (tag-driven) | RFC 9901 | Core | Foundation |
| **SdJwt.Net.Vc** | MinVer (tag-driven) | Draft-15 | Standards | Credentials |
| **SdJwt.Net.StatusList** | MinVer (tag-driven) | Draft-13 | Standards | Revocation |
| **SdJwt.Net.Oid4Vci** | MinVer (tag-driven) | OID4VCI 1.0 | Standards | Issuance |
| **SdJwt.Net.Oid4Vp** | MinVer (tag-driven) | OID4VP 1.0 | Standards | Presentation |
| **SdJwt.Net.PresentationExchange** | MinVer (tag-driven) | DIF PE v2.1.1 | Standards | Selection |
| **SdJwt.Net.OidFederation** | MinVer (tag-driven) | OpenID Federation 1.0 | Standards | Trust |
| **SdJwt.Net.HAIP** | MinVer (tag-driven) | HAIP 1.0 | HAIP | Assurance |

## Common Tasks

### I want to

**...understand selective disclosure basics**
 Run example 1 (Core SD-JWT Example)
 Read [Core README](Core/README.md)

**...create credentials the standard way**
 Run example 6 (OpenID4VCI Example)
 Read [Standards README](Standards/README.md)

**...verify credentials from other systems**
 Run example 7 (OpenID4VP Example)
 Read [Standards README](Standards/README.md)

**...build trust between organizations**
 Run example 8 (OpenID Federation Example)
 Read [Standards README](Standards/README.md)

**...select credentials intelligently**
 Run example 9 (Presentation Exchange Example)
 Read [Standards README](Standards/README.md)

**...build a production application**
 Run option C (Real-World Use Cases)
 Read [Real-World scenarios docs](./RealWorld/Financial/README.md)

**...deploy with government-grade security**
 Run HAIP examples (options from menu)
 Read [HAIP README](HAIP/README.md)

## Platform Support

- **.NET 9.0** - Latest with full support
- **.NET 10.0** - Latest with full support
- **.NET 8.0** - LTS version (libraries)

## Related Documentation

- **[Getting Started Guide](../../docs/getting-started/running-the-samples.md)** - Step-by-step tutorial
- **[Developer Guide](../../docs/README.md)** - Comprehensive ecosystem guide
- **[Architecture Design](../../docs/concepts/architecture.md)** - System architecture
- **[Documentation Index](../../docs/README.md)** - All documentation

## Support & Troubleshooting

- **Questions?** Check [GitHub Discussions](https://github.com/openwallet-foundation-labs/sd-jwt-dotnet/discussions)
- **Bugs?** Report on [GitHub Issues](https://github.com/openwallet-foundation-labs/sd-jwt-dotnet/issues)
- **Security?** See [SECURITY.md](../../SECURITY.md)

---

**Last Updated**: February 11, 2026
**Test Coverage**: All examples tested and working
**Latest Versions**: Versioned via MinVer from repository tags

