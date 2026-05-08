# SD-JWT .NET Use Cases

This directory contains industry-specific use cases demonstrating how the SD-JWT .NET ecosystem solves real-world problems. Each use case addresses a measurable business problem with concrete implementation patterns.

---

## Start Here

| Use Case                                            | Industry               | Key Packages                        |
| --------------------------------------------------- | ---------------------- | ----------------------------------- |
| [Financial AI](financial-ai.md)                     | Finance/Superannuation | SD-JWT VC, Status Lists, HAIP       |
| [AI Agent Authorization](ai-agent-authorization.md) | Enterprise AI/Security | Agent Trust Core, Policy, MCP, A2A  |
| [Cross-Border Government](crossborder.md)           | EU Public Services     | OID4VP, Federation, HAIP            |
| [Telecom eSIM](telco-esim.md)                       | Telecommunications     | SD-JWT VC, Status Lists, PEX        |
| [E-Commerce Returns](retail-ecommerce-returns.md)   | Retail                 | SD-JWT VC, Federation, Status Lists |

---

## Identity and Credentials

| Use Case                                                        | Industry                         | Key Packages                    |
| --------------------------------------------------------------- | -------------------------------- | ------------------------------- |
| [Healthcare Credentials](healthcare-credential-verification.md) | Healthcare/Insurance/Pharmacy    | SD-JWT VC, OID4VP, PEX, HAIP    |
| [Enterprise KYC Onboarding](enterprise-kyc-onboarding.md)       | Enterprise HR/Financial Services | SD-JWT VC, OID4VCI, OID4VP, PEX |
| [mdoc Identity Verification](mdoc-identity-verification.md)     | Government/Travel                | Mdoc, OID4VP, HAIP, PEX         |
| [DC API Web Verification](dc-api-web-verification.md)           | E-Commerce/Financial/Healthcare  | OID4VP, PEX, HAIP               |
| [EUDIW Cross-Border](eudiw-cross-border-verification.md)        | EU Government/Finance/Healthcare | Eudiw, Mdoc, OID4VP, HAIP       |

---

## Operations and Architecture

| Use Case                                        | Focus                          | Key Packages              |
| ----------------------------------------------- | ------------------------------ | ------------------------- |
| [Automated Compliance](automated-compliance.md) | Policy-first data minimization | PEX, OID4VP, Status Lists |
| [Incident Response](incident-response.md)       | Trust containment workflows    | Federation, Status Lists  |

---

## How to read these documents

Each use case follows a consistent structure:

1. **Executive summary** - Business problem and solution overview
2. **Why this matters** - Industry context and urgency
3. **Reference architecture** - Sequence diagrams and component mapping
4. **Integration pattern** - How SD-JWT .NET packages fit
5. **Business value** - Quantifiable outcomes and cost drivers

---

## Notes

- Use cases are implementation-informed but may include forward-looking patterns.
- Use package `README.md` files for API-specific guidance.
- Use `docs/guides` for step-by-step procedures.
- Use `docs/concepts` for protocol and architecture fundamentals.
