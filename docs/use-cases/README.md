# SD-JWT .NET Use Cases

This directory contains industry-specific use cases demonstrating how the SD-JWT .NET ecosystem solves real-world problems.

---

## Start Here

| Use Case                                          | Industry               | Key Packages                        |
| ------------------------------------------------- | ---------------------- | ----------------------------------- |
| [Financial AI](financial-ai.md)                   | Finance/Superannuation | SD-JWT VC, Status Lists, HAIP       |
| [Cross-Border Government](crossborder.md)         | EU Public Services     | OID4VP, Federation, HAIP            |
| [Telecom eSIM](telco-esim.md)                     | Telecommunications     | SD-JWT VC, Status Lists, PEX        |
| [E-Commerce Returns](retail-ecommerce-returns.md) | Retail                 | SD-JWT VC, Federation, Status Lists |

---

## Operations and Architecture

| Use Case                                              | Focus                          | Key Packages              |
| ----------------------------------------------------- | ------------------------------ | ------------------------- |
| [Automated Compliance](automated-compliance.md)       | Policy-first data minimization | PEX, OID4VP, Status Lists |
| [Incident Response](incident-response.md)             | Trust containment workflows    | Federation, Status Lists  |
| [Post-Quantum Readiness](quantum-key-distribution.md) | Cryptographic migration        | HAIP, Federation          |

---

## How to Read These Documents

Each use case follows a consistent structure:

1. **Executive Summary** - Business problem and solution overview
2. **Why This Matters** - Industry context and urgency
3. **Reference Architecture** - Sequence diagrams and component mapping
4. **Integration Pattern** - How SD-JWT .NET packages fit
5. **Code Examples** - Application-layer orchestration patterns

---

## Notes

- Use cases are implementation-informed but may include forward-looking patterns.
- Use package `README.md` files for API-specific guidance.
- Use `docs/guides` for step-by-step procedures.
- Use `docs/concepts` for protocol and architecture fundamentals.
