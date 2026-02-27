# SD-JWT .NET Insights & Articles

This directory contains thought leadership articles and technical insights about building privacy-preserving systems with Selective Disclosure JSON Web Tokens (SD-JWT).

## Articles

### [Verified Advice Context for AI in Superannuation and Finance](verified-advice-context.md)

**Topic**: Evidence-backed production case study for policy-constrained AI guidance using verifiable minimal data

**Key Concepts**:

- Verified Advice Context (VAC) pattern
- Advice-mode policy gates for AU regulatory contexts
- Deterministic decisioning plus LLM narration split
- OID4VP callback-driven protocol flow
- Verified and fresh model input contracts
- Threat model and trust model choices
- Conformance-aware production profile planning

**Target Audience**:

- Enterprise architects designing AI systems with regulated data
- Financial services building personalized AI advisors
- Government agencies implementing privacy-preserving services
- Developers implementing SD-JWT in production

**Reading Time**: 18-22 minutes

**Key Takeaway**: Production AI guidance in finance requires more than selective disclosure. Teams need policy gating, deterministic calculation, freshness checks, and auditable verification before model invocation.

**Companion Documents**:

- [VAC Reference Architecture](verified-advice-reference-architecture.md)
- [VAC Implementation Guide](verified-advice-implementation-guide.md)

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

- **[Samples](../samples/SdJwt.Net.Samples/README.md)** - Code examples and running demos
- **[Developer Guide](../developer-guide.md)** - Technical reference
- **[Architecture Design](../architecture-design.md)** - System architecture patterns
- **[VAC Case Study](verified-advice-context.md)** - Business and risk framing for production
- **[VAC Reference Architecture](verified-advice-reference-architecture.md)** - Security and trust design
- **[VAC Implementation Guide](verified-advice-implementation-guide.md)** - Engineer-facing rollout guidance

---

**Last Updated**: February 2026
**License**: Apache 2.0
