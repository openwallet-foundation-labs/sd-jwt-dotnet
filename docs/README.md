# SD-JWT .NET Documentation

Welcome to the comprehensive documentation for the SD-JWT .NET ecosystem - a production-ready implementation of Selective Disclosure JSON Web Tokens and the complete OpenID for Verifiable Credentials (OpenID4VC) stack.

## Quick Navigation

### Getting Started

- **[Quick Start Guide](samples/getting-started.md)** - Get up and running in 5 minutes
- **[Samples Overview](samples/README.md)** - Interactive examples and tutorials
- **[Developer Guide](developer-guide.md)** - Comprehensive development guide
- **[Architecture Design](architecture-design.md)** - System architecture and design principles

### Package Documentation

- **[SdJwt.Net](../src/SdJwt.Net/README.md)** - Core SD-JWT implementation (RFC 9901)
- **[SdJwt.Net.Vc](../src/SdJwt.Net.Vc/README.md)** - Verifiable Credentials (draft-13)
- **[SdJwt.Net.StatusList](../src/SdJwt.Net.StatusList/README.md)** - Credential lifecycle management
- **[SdJwt.Net.Oid4Vci](../src/SdJwt.Net.Oid4Vci/README.md)** - Credential issuance protocol
- **[SdJwt.Net.Oid4Vp](../src/SdJwt.Net.Oid4Vp/README.md)** - Presentation protocol
- **[SdJwt.Net.PresentationExchange](../src/SdJwt.Net.PresentationExchange/README.md)** - DIF Presentation Exchange v2.1.1
- **[SdJwt.Net.OidFederation](../src/SdJwt.Net.OidFederation/README.md)** - OpenID Federation trust management
- **[SdJwt.Net.HAIP](../src/SdJwt.Net.HAIP/README.md)** - High Assurance Interoperability Profile

### Scenarios & Examples

- **[All Scenarios](samples/scenarios/README.md)** - Real-world use case implementations
- **[Financial Co-Pilot](samples/scenarios/financial/README.md)** - AI-powered privacy-preserving advisor

### Specifications Reference

- **[RFC 9901](specs/rfc9901.txt)** - SD-JWT Core Standard
- **[draft-ietf-oauth-sd-jwt-vc-14](specs/draft-ietf-oauth-sd-jwt-vc-14.txt)** - SD-JWT VC Specification
- **[draft-ietf-oauth-status-list-18](specs/draft-ietf-oauth-status-list-18.txt)** - Status List Specification
- **[draft-ietf-oauth-oid4vp-18](specs/draft-ietf-oauth-oid4vp-18.txt)** - OpenID4VP compliance analysis

### Insights & Articles

- **[Insights Hub](insights/README.md)** - Thought leadership and technical insights
- **[AI & Privacy with SD-JWT](insights/ai-privacy-with-sd-jwt.md)** - Privacy-preserving AI architectures

## Documentation Structure

```
docs/
 README.md (this file)            # Documentation index
 developer-guide.md               # Complete developer guide
 architecture-design.md           # Architecture and design patterns
 samples/                        
    README.md                   # Samples overview
    getting-started.md          # Quick start tutorial
    scenarios/                  # Real-world scenarios
        README.md              
        financial/              # Financial Co-Pilot demo
 specs/                          # Standards specifications
    rfc9901.txt
    draft-ietf-oauth-sd-jwt-vc-13.txt
    draft-ietf-oauth-status-list-13.txt
    SDJWT_NET_OID4VP_SPEC_COMPLIANCE.md
 insights/                       # Thought leadership and technical articles
     README.md                   # Insights index
     ai-privacy-with-sd-jwt.md   # Privacy-preserving AI patterns
     versioning-and-release-strategy.md  # Auto-versioning strategy
```

## Key Concepts

### What is SD-JWT?

Selective Disclosure JSON Web Tokens (SD-JWT) allow the holder of a credential to selectively disclose specific claims to a verifier without revealing the entire credential content. This provides privacy-preserving capabilities essential for modern identity systems.

### OpenID4VC Ecosystem

The SD-JWT .NET ecosystem implements the complete OpenID for Verifiable Credentials stack:

- **OID4VCI** - Standardized credential issuance
- **OID4VP** - Cross-device presentation protocols
- **DIF Presentation Exchange** - Intelligent credential selection
- **OpenID Federation** - Trust chain management
- **HAIP** - High assurance security levels

## Learning Paths

### Beginner Path (30-45 minutes)

1. Start with [Getting Started Guide](samples/getting-started.md)
2. Run Core SD-JWT samples
3. Understand basic selective disclosure concepts

### Intermediate Path (2-3 hours)

1. Review [Developer Guide](developer-guide.md)
2. Explore Verifiable Credentials samples
3. Implement OpenID4VCI and OpenID4VP flows
4. Work with Status Lists

### Advanced Path (4-6 hours)

1. Study [Architecture Design](architecture-design.md)
2. Implement Presentation Exchange
3. Set up OpenID Federation trust chains
4. Deploy HAIP-compliant systems

### Expert Path (Production Deployment)

1. Review real-world scenarios
2. Implement Financial Co-Pilot or similar use case
3. Study security best practices
4. Plan production deployment architecture

## Support & Community

- **Issues**: [GitHub Issues](https://github.com/openwallet-foundation-labs/sd-jwt-dotnet/issues)
- **Discussions**: [GitHub Discussions](https://github.com/openwallet-foundation-labs/sd-jwt-dotnet/discussions)
- **Security**: See [SECURITY.md](../SECURITY.md) for reporting security issues
- **Contributing**: See [CONTRIBUTING.md](../CONTRIBUTING.md) for contribution guidelines

## Resources by Topic

### Privacy & Security

- [AI & Privacy with SD-JWT](insights/ai-privacy-with-sd-jwt.md) - GenAI integration patterns
- [Security Best Practices](samples/SdJwt.Net.Samples/Core/README.md) - Cryptographic security
- [HAIP Compliance Guide](samples/SdJwt.Net.Samples/HAIP/README.md) - Government-grade security

### Real-World Implementation

- [Financial Co-Pilot](samples/scenarios/financial/README.md) - AI advisor with privacy
- [Real-World Scenarios](samples/SdJwt.Net.Samples/RealWorld/README.md) - 4 complete use cases
- [Integration Patterns](samples/SdJwt.Net.Samples/Integration/README.md) - Multi-package workflows

### Standards & Protocols

- [Core Concepts](samples/SdJwt.Net.Samples/Core/README.md) - RFC 9901 fundamentals
- [Standards Guide](samples/SdJwt.Net.Samples/Standards/README.md) - All 6 protocols
- [Specifications](specs/) - Official standard documents

## Version Information

All packages are currently at version **1.0.0**, representing production-ready implementations of their respective specifications. Each package implements specific standard versions as documented in their individual README files.

---

**Last Updated**: February 2026
