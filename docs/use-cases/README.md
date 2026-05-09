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

## Choose by business problem

| Business problem                               | Use case                                                        |
| ---------------------------------------------- | --------------------------------------------------------------- |
| AI agents with too-broad permissions           | [AI Agent Authorization](ai-agent-authorization.md)             |
| Return fraud costing millions annually         | [E-Commerce Returns](retail-ecommerce-returns.md)               |
| eSIM fraud and SIM-swap attacks                | [Telecom eSIM](telco-esim.md)                                   |
| Oversharing in healthcare credential exchanges | [Healthcare Credentials](healthcare-credential-verification.md) |
| Slow, document-heavy employee onboarding       | [Enterprise KYC Onboarding](enterprise-kyc-onboarding.md)       |
| Cross-border government data sharing with AI   | [Cross-Border Government](crossborder.md)                       |
| Age or license verification on websites        | [DC API Web Verification](dc-api-web-verification.md)           |
| EU cross-border credential acceptance          | [EUDIW Cross-Border](eudiw-cross-border-verification.md)        |
| AI copilot accessing more data than necessary  | [Financial AI](financial-ai.md)                                 |
| Credential compromise containment              | [Incident Response](incident-response.md)                       |
| Proving data minimization for auditors         | [Automated Compliance](automated-compliance.md)                 |
| Mobile identity at airport checkpoints         | [mdoc Identity Verification](mdoc-identity-verification.md)     |

## Choose by technical pattern

| Technical pattern                   | Use cases                                                                                                                                        |
| ----------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------ |
| Selective disclosure (SD-JWT VC)    | All use cases                                                                                                                                    |
| Status lists (revocation/lifecycle) | [E-Commerce Returns](retail-ecommerce-returns.md), [Telecom eSIM](telco-esim.md), [Incident Response](incident-response.md)                      |
| OpenID Federation (trust chains)    | [Cross-Border Government](crossborder.md), [E-Commerce Returns](retail-ecommerce-returns.md), [Telecom eSIM](telco-esim.md)                      |
| HAIP profile validation             | [DC API Web Verification](dc-api-web-verification.md), [EUDIW Cross-Border](eudiw-cross-border-verification.md), [Financial AI](financial-ai.md) |
| Presentation Exchange (PEX)         | [Healthcare Credentials](healthcare-credential-verification.md), [Enterprise KYC](enterprise-kyc-onboarding.md), [Telecom eSIM](telco-esim.md)   |
| Agent Trust (capability tokens)     | [AI Agent Authorization](ai-agent-authorization.md), [Financial AI](financial-ai.md)                                                             |
| mdoc (ISO 18013-5)                  | [mdoc Identity Verification](mdoc-identity-verification.md), [EUDIW Cross-Border](eudiw-cross-border-verification.md)                            |
| EUDIW / ARF reference models        | [EUDIW Cross-Border](eudiw-cross-border-verification.md), [Cross-Border Government](crossborder.md)                                              |

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
