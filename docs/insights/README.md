# SD-JWT .NET Insights & Articles

This directory contains thought leadership articles and technical insights about building privacy-preserving systems with Selective Disclosure JSON Web Tokens (SD-JWT).

## Articles

### [Verified Advice Context for AI in Superannuation and Finance](ai-financial-co-pilot.md)

**Topic**: Policy-driven, privacy-preserving AI advice with verifiable data minimization

**Key Concepts**:

- Verified Advice Context (VAC) pattern
- Intent-to-claim policy mapping for AI requests
- Superannuation contribution strategy copilot design
- OID4VCI issuance and OID4VP selective presentation
- Presentation Exchange for minimum-claim requests
- Trust and revocation checks before model invocation
- Auditability and governance controls for regulated AI

**Target Audience**:

- Enterprise architects designing AI systems with regulated data
- Financial services building personalized AI advisors
- Government agencies implementing privacy-preserving services
- Developers implementing SD-JWT in production

**Reading Time**: 12-15 minutes

**Key Takeaway**: AI personalization in finance does not require full-profile data sharing. With SD-JWT and policy-constrained selective disclosure, systems can deliver useful guidance while reducing privacy and governance risk.

---

### [Automated Compliance: AI-Powered Context-Aware Data Minimization](automated-compliance.md)

**Topic**: Replacing brittle static rules with AI-driven compliance for presentation definitions.

**Key Concepts**:

- Proactive Data Minimization
- LLM Context-Aware Request Rewriting
- HAIP Audit Trails for Compliance Proofs

---

### [Quantum Key Distribution (QKD): Securing Sovereign Trust](quantum-key-distribution.md)

**Topic**: Protecting Top Secret / Sovereign credentials from "Harvest Now, Decrypt Later" quantum threats.

**Key Concepts**:

- Quantum Key Distribution for Symmetric Encryption
- Post-Quantum Cryptography (PQC) Signatures (ML-DSA)
- Integrating QKD hardware APIs into the `.NET` ecosystem

---

### [Incident Response: Automated Trust Revocation and Recovery](incident-response.md)

**Topic**: Surviving zero-day issuer compromises with millisecond-scale, automated severing of trust.

**Key Concepts**:

- SIEM Integrations with OpenID Federation Webhooks
- Instant Token Status List updates via CDN
- Verifier Cache Management and Server-Sent Events

---

## Research & Development

This section evolves as the community explores new patterns and use cases with SD-JWT.

### Planned Articles

- Privacy patterns for healthcare data
- Multi-issuer credential ecosystems
- Cross-border credential exchange
- Blockchain integration with SD-JWT
- Performance optimization for high-volume scenarios

---

## Contributing

Have an insight or pattern worth sharing?

1. Write your article in markdown format
2. Include:
   - Clear problem statement
   - Solution architecture (diagrams welcome)
   - Code examples where applicable
   - References to standards and packages
   - Practical implementation guidance
3. Submit a pull request with your article

Articles should be:

- **Practical**: Real problems, real solutions
- **Well-structured**: Clear sections with progressive detail
- **Referenced**: Link to standards, specs, and code
- **Evergreen**: Timeless patterns over trendy techniques

---

## Related Documentation

- **[Samples](../samples/README.md)** - Code examples and running demos
- **[Developer Guide](../developer-guide.md)** - Technical reference
- **[Architecture Design](../architecture-design.md)** - System architecture patterns
- **[Financial Co-Pilot](samples/scenarios/financial/README.md)** - Practical AI integration example

---

**Last Updated**: February 2026
**License**: Apache 2.0
