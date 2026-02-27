# Post-Quantum Readiness for Sovereign Identity: QKD, PQC, and SD-JWT Trust Infrastructure

## Executive summary

Long-lived identity systems face a "harvest now, decrypt later" risk model: adversaries collect protected material today and attempt cryptanalysis when quantum capability matures.

For high-assurance deployments, post-quantum readiness should focus on:

- Migrating signing and key-management strategy to post-quantum-safe profiles.
- Reducing long-term exposure windows for credentials and trust metadata.
- Separating near-term controls (crypto agility, shorter validity, key rotation) from long-term controls (PQC and specialized links such as QKD where applicable).

SD-JWT ecosystems can adopt this incrementally through issuer, verifier, and federation policy controls.

---

## Threat model for identity ecosystems

Critical assets include:

- Issuer signing keys and trust-anchor signing keys.
- Federation metadata and trust chains.
- Sensitive presentations with long confidentiality requirements.

Risks are not uniform. Many systems need crypto agility now, while only a subset need QKD-grade transport assumptions.

---

## Practical architecture guidance

### 1) Crypto agility first

- Keep algorithms configurable per issuer/verifier policy.
- Prefer short credential validity for high-risk scopes.
- Rotate keys aggressively and automate trust-metadata refresh.

### 2) PQC migration plan

- Track standards maturity and interoperable JOSE/COSE support.
- Introduce dual-signature or transition policies where ecosystem compatibility requires phased rollout.
- Maintain deterministic fallback policy for counterparties not yet upgraded.

### 3) QKD as a scoped control

QKD can be useful for selected sovereign links with dedicated infrastructure and strict operational requirements. It is not a default replacement for internet-scale identity transport.

Use it where:

- Physical network assumptions are acceptable.
- High-value bilateral channels justify hardware and operations overhead.
- Governance mandates dedicated quantum-resistant key establishment.

---

## Implementation status in sd-jwt-dotnet

The repository currently emphasizes standards-based SD-JWT, OpenID4VC, federation, and HAIP controls.

Current state:

- No built-in PQC signer implementation is provided.
- No built-in QKD key-provider integration is provided.
- Extensibility points in application architecture can be used to integrate external cryptographic components.

Recommended approach:

- Treat PQC and QKD integration as deployment-specific extensions.
- Keep package-level contracts and policy layers algorithm-agile.

---

## Example migration controls

Use a phased control table:

| Phase | Objective | Example controls |
|---|---|---|
| Phase 0 | Reduce current exposure | Shorter token lifetimes, key rotation, strict verifier freshness |
| Phase 1 | Prepare for algorithm transition | Configurable algorithm policies, dual-stack validation in staging |
| Phase 2 | Introduce PQC in controlled scope | Selected issuer domains, limited relying-party allowlist |
| Phase 3 | High-assurance transport hardening | Dedicated key-establishment channels (including QKD where justified) |

---

## Public references (URLs)

- NIST Post-Quantum Cryptography project: <https://csrc.nist.gov/projects/post-quantum-cryptography>
- RFC 9901 (SD-JWT): <https://datatracker.ietf.org/doc/rfc9901/>
- OpenID Federation 1.0: <https://openid.net/specs/openid-federation-1_0.html>
- OpenID4VC HAIP 1.0: <https://openid.net/specs/openid4vc-high-assurance-interoperability-profile-1_0.html>

Disclaimer: This article is informational and not legal or cryptographic certification guidance. Validate design decisions with qualified security and compliance experts.
