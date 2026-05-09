# SD-JWT .NET Reference Patterns

Reference architecture patterns for building privacy-preserving credential, wallet, verifier, trust, status, and agent-governance workflows on .NET.

These patterns are not turnkey products, compliance certifications, or guaranteed business outcomes. Read [How to Read These Patterns](00-how-to-read-these-patterns.md) for maturity labels, code block conventions, and navigation guidance.

---

## What are you trying to prove?

| Question                                            | Pattern                          | Start here                                                          |
| --------------------------------------------------- | -------------------------------- | ------------------------------------------------------------------- |
| A person owns a credential                          | SD-JWT VC / mdoc / OID4VP        | [DC API Web Verification](dc-api-web-verification.md)               |
| A credential is still valid                         | Status List                      | [Incident Response](incident-response.md)                           |
| An issuer is trusted                                | OpenID Federation / trust policy | [Cross-Border Government](crossborder.md)                           |
| A verifier should ask only for required claims      | Presentation Exchange            | [Policy-First Data Minimization](policy-first-data-minimization.md) |
| A browser should mediate wallet presentation        | DC API + OID4VP                  | [DC API Web Verification](dc-api-web-verification.md)               |
| An AI agent is allowed to call a tool               | Agent Trust capability token     | [AI Agent Authorization](ai-agent-authorization.md)                 |
| An AI system should only see verified minimum facts | Verified Context Gate            | [Financial AI](financial-ai.md)                                     |

---

## Choose your path

### Developer path

Start with intuitive examples, then move to browser verification, identity, agent trust, and operations.

1. [E-Commerce Returns](retail-ecommerce-returns.md) -- verifiable receipts, status lists, federation
2. [DC API Web Verification](dc-api-web-verification.md) -- browser-mediated credential presentation
3. [Enterprise KYC Onboarding](enterprise-kyc-onboarding.md) -- credential issuance and verification flows
4. [AI Agent Authorization](ai-agent-authorization.md) -- scoped capability tokens for tool calls
5. [Incident Response](incident-response.md) -- trust containment and revocation

### Enterprise architect path

Business value, governance, data minimization, and operating model.

1. [Financial AI](financial-ai.md) -- verified context gate for AI copilots
2. [Policy-First Data Minimization](policy-first-data-minimization.md) -- audit-ready selective disclosure
3. [Enterprise KYC Onboarding](enterprise-kyc-onboarding.md) -- workforce credential verification
4. [Incident Response](incident-response.md) -- trust containment workflows
5. [AI Agent Authorization](ai-agent-authorization.md) -- agent governance architecture
6. [Enterprise API Access](enterprise-api-access.md) -- verified client context for APIs

### Wallet / identity architect path

Wallet interoperability, EUDIW, mdoc, and verifier flows.

1. [EUDIW Cross-Border](eudiw-cross-border-verification.md) -- ARF reference verification
2. [mdoc Identity Verification](mdoc-identity-verification.md) -- ISO 18013-5 mobile documents
3. [DC API Web Verification](dc-api-web-verification.md) -- browser-mediated presentation
4. [Cross-Border Government](crossborder.md) -- cross-border credential exchange
5. [Enterprise KYC Onboarding](enterprise-kyc-onboarding.md) -- verifiable onboarding flows

### Agent security path

Least privilege, revocation, fraud, audit, and containment.

1. [AI Agent Authorization](ai-agent-authorization.md) -- scoped capability tokens
2. [Incident Response](incident-response.md) -- trust containment workflows
3. [Policy-First Data Minimization](policy-first-data-minimization.md) -- provable data minimization
4. [Telecom eSIM](telco-esim.md) -- fraud-resistant credential lifecycle
5. [E-Commerce Returns](retail-ecommerce-returns.md) -- single-use credential controls

---

## Choose by business problem

| Business problem                               | Use case                                                              |
| ---------------------------------------------- | --------------------------------------------------------------------- |
| AI agents with too-broad permissions           | [AI Agent Authorization](ai-agent-authorization.md)                   |
| AI copilot accessing more data than necessary  | [Financial AI](financial-ai.md)                                       |
| API authorization beyond OAuth scopes          | [Enterprise API Access](enterprise-api-access.md)                     |
| Return fraud costing millions annually         | [E-Commerce Returns](retail-ecommerce-returns.md)                     |
| eSIM fraud and SIM-swap attacks                | [Telecom eSIM](telco-esim.md)                                         |
| Oversharing in healthcare credential exchanges | [Healthcare Credentials](healthcare-credential-verification.md)       |
| Slow, document-heavy employee onboarding       | [Enterprise KYC Onboarding](enterprise-kyc-onboarding.md)             |
| Document-heavy supplier onboarding             | [Supplier Onboarding](supplier-onboarding.md)                         |
| Qualification verification without transcripts | [Education and Skills Passport](education-skills-passport.md)         |
| Untrusted evidence in insurance claims         | [Insurance Claims Evidence](insurance-claims-evidence.md)             |
| Chasing PDFs across construction workflows     | [Construction Readiness Passport](construction-readiness-passport.md) |
| Cross-border government data sharing with AI   | [Cross-Border Government](crossborder.md)                             |
| EU cross-border credential acceptance          | [EUDIW Cross-Border](eudiw-cross-border-verification.md)              |
| Age or license verification on websites        | [DC API Web Verification](dc-api-web-verification.md)                 |
| Mobile identity at airport checkpoints         | [mdoc Identity Verification](mdoc-identity-verification.md)           |
| Credential compromise containment              | [Incident Response](incident-response.md)                             |
| Proving data minimization for auditors         | [Policy-First Data Minimization](policy-first-data-minimization.md)   |

## Choose by technical pattern

| Technical pattern                   | Use cases                                                                                                                                                                                                                                |
| ----------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Selective disclosure (SD-JWT VC)    | All use cases                                                                                                                                                                                                                            |
| Status lists (revocation/lifecycle) | [E-Commerce Returns](retail-ecommerce-returns.md), [Telecom eSIM](telco-esim.md), [Incident Response](incident-response.md), [Insurance Claims](insurance-claims-evidence.md)                                                            |
| OpenID Federation (trust chains)    | [Cross-Border Government](crossborder.md), [E-Commerce Returns](retail-ecommerce-returns.md), [Telecom eSIM](telco-esim.md), [Supplier Onboarding](supplier-onboarding.md)                                                               |
| HAIP profile validation             | [DC API Web Verification](dc-api-web-verification.md), [EUDIW Cross-Border](eudiw-cross-border-verification.md), [Financial AI](financial-ai.md)                                                                                         |
| Presentation Exchange (PEX)         | [Healthcare Credentials](healthcare-credential-verification.md), [Enterprise KYC](enterprise-kyc-onboarding.md), [Telecom eSIM](telco-esim.md), [Supplier Onboarding](supplier-onboarding.md), [Education](education-skills-passport.md) |
| Agent Trust (capability tokens)     | [AI Agent Authorization](ai-agent-authorization.md), [Financial AI](financial-ai.md), [Enterprise API Access](enterprise-api-access.md)                                                                                                  |
| mdoc (ISO 18013-5)                  | [mdoc Identity Verification](mdoc-identity-verification.md), [EUDIW Cross-Border](eudiw-cross-border-verification.md)                                                                                                                    |
| EUDIW / ARF reference models        | [EUDIW Cross-Border](eudiw-cross-border-verification.md), [Cross-Border Government](crossborder.md)                                                                                                                                      |
| OID4VCI (credential issuance)       | [Education and Skills Passport](education-skills-passport.md), [Construction Readiness](construction-readiness-passport.md)                                                                                                              |

---

## Core reusable patterns

These trust patterns appear across multiple industry use cases. Each pattern describes a single composable building block.

| Pattern                                                                         | Summary                                                                    |
| ------------------------------------------------------------------------------- | -------------------------------------------------------------------------- |
| [Verified Context Gate](01-patterns/verified-context-gate.md)                   | Attach verified, minimized context to a request before processing          |
| [Policy-First Data Minimization](01-patterns/policy-first-data-minimization.md) | Define disclosure policy before requesting data                            |
| [Scoped Agent Capability Token](01-patterns/scoped-agent-capability-token.md)   | Scope each agent tool call to a specific action, resource, and time window |
| [Status List Lifecycle Control](01-patterns/status-list-lifecycle-control.md)   | Revoke, suspend, or expire credentials in real time                        |
| [Federated Trust Onboarding](01-patterns/federated-trust-onboarding.md)         | Dynamically resolve issuer trust across organizations                      |
| [Multi-Format Verifier](01-patterns/multi-format-verifier.md)                   | Accept both SD-JWT VC and mdoc credentials in one verifier                 |
| [Browser-Mediated Presentation](01-patterns/browser-mediated-presentation.md)   | Use the browser as a credential presentation channel                       |
| [Trust Containment](01-patterns/trust-containment-incident-response.md)         | Contain credential incidents through federation and status updates         |

---

## Reference pattern catalogue

### Flagship patterns

These patterns best explain the ecosystem's positioning as trust infrastructure.

#### Financial AI / Verified Context Gate

Problem: AI copilots need member data but sharing full records creates regulatory risk.
Pattern: SD-JWT VC provides verifiable data minimization so copilots operate within regulatory boundaries.
Packages: SD-JWT VC, Status Lists, HAIP, PEX.
Status: Stable primitives; verified context gate is a reference architecture.
[Read the full pattern](financial-ai.md)

#### AI Agent Authorization

Problem: Agents often use broad credentials to call tools.
Pattern: Scoped SD-JWT capability tokens per tool call.
Packages: AgentTrust.Core, Policy, MCP, AspNetCore.
Status: Preview reference pattern.
[Read the full pattern](ai-agent-authorization.md)

#### Policy-First Data Minimization

Problem: Proving to auditors that only required data was requested and received.
Pattern: Policy-defined presentation exchange with auditable evidence.
Packages: PEX, OID4VP, Status Lists.
Status: Stable primitives.
[Read the full pattern](policy-first-data-minimization.md)

#### Incident Response

Problem: Issuer key compromise requires fast, coordinated containment.
Pattern: Federation trust updates and status list revocation in parallel.
Packages: OpenID Federation, Status Lists, HAIP.
Status: Stable primitives.
[Read the full pattern](incident-response.md)

### Developer-friendly examples

These patterns are intuitive to understand and easier to prototype.

#### E-Commerce Returns

Problem: Return fraud costs retailers over $100B annually; tightening policies hurts honest customers.
Pattern: Verifiable receipt credentials with status list lifecycle and federation trust.
Packages: SD-JWT VC, Status Lists, Federation, PEX.
Status: Proposed reference credential pattern.
[Read the full pattern](retail-ecommerce-returns.md)

#### DC API Web Verification

Problem: Web applications need credential verification without browser extensions.
Pattern: W3C Digital Credentials API with OID4VP backend verification.
Packages: OID4VP, PEX, HAIP.
Status: Spec-tracking (W3C draft).
[Read the full pattern](dc-api-web-verification.md)

#### Enterprise KYC Onboarding

Problem: Employee and contractor onboarding is paper-heavy, slow, and fraud-prone.
Pattern: Verifiable credentials for identity, right-to-work, and professional licensing.
Packages: SD-JWT VC, OID4VCI, OID4VP, PEX, Status Lists, Federation.
Status: Stable primitives.
[Read the full pattern](enterprise-kyc-onboarding.md)

#### mdoc Identity Verification

Problem: Mobile identity verification across government, travel, and enterprise.
Pattern: ISO 18013-5 mdoc with OID4VP and HAIP profile validation.
Packages: Mdoc, OID4VP, HAIP, PEX.
Status: Stable primitives.
[Read the full pattern](mdoc-identity-verification.md)

### Enterprise workflows

#### Enterprise API Access

Problem: OAuth scopes are too coarse for context-aware API authorization.
Pattern: Verified client context tokens attached to API requests.
Packages: SD-JWT VC, OID4VP, AgentTrust.AspNetCore, Status Lists.
Status: Reference architecture.
[Read the full pattern](enterprise-api-access.md)

#### Supplier Onboarding

Problem: Supplier verification is document-heavy, fraud-prone, and audit-heavy.
Pattern: Verifiable credentials from authoritative sources with federation trust.
Packages: SD-JWT VC, OID4VP, PEX, Federation, Status Lists.
Status: Reference architecture.
[Read the full pattern](supplier-onboarding.md)

#### Education and Skills Passport

Problem: Qualification verification requires full transcripts and manual confirmation.
Pattern: Selectively disclosable education credentials with OID4VCI issuance.
Packages: SD-JWT VC, OID4VCI, OID4VP, PEX, Status Lists.
Status: Reference architecture.
[Read the full pattern](education-skills-passport.md)

#### Insurance Claims Evidence

Problem: Claims automation fails when evidence is untrusted.
Pattern: Verifiable evidence credentials from authoritative providers.
Packages: SD-JWT VC, PEX, Status Lists, OID4VP, Federation.
Status: Reference architecture.
[Read the full pattern](insurance-claims-evidence.md)

#### Construction Readiness Passport

Problem: Construction readiness requires chasing PDFs across multiple authorities.
Pattern: Verifiable readiness credentials with milestone-gated verification.
Packages: SD-JWT VC, OID4VCI, OID4VP, PEX, Status Lists.
Status: Reference architecture.
[Read the full pattern](construction-readiness-passport.md)

### Regulated and advanced ecosystem patterns

These patterns have high value but more legal, regulatory, and trust-framework complexity.

#### Healthcare Credentials

Problem: Healthcare data breaches average $9.77M per incident; HIPAA requires minimum necessary disclosure.
Pattern: Selective disclosure for patient identity, insurance, and provider trust.
Packages: SD-JWT VC, OID4VP, PEX, Status Lists, HAIP.
Status: Stable primitives.
[Read the full pattern](healthcare-credential-verification.md)

#### Telecom eSIM

Problem: eSIM fraud and SIM-swap attacks exploit weak identity verification.
Pattern: Verifiable subscriber credentials with status list lifecycle.
Packages: SD-JWT VC, Status Lists, PEX, Federation, HAIP.
Status: Proposed reference credential pattern.
[Read the full pattern](telco-esim.md)

#### Cross-Border Government

Problem: Cross-border government services need verified citizen data with AI governance.
Pattern: EUDIW, federation, and HAIP for cross-border credential exchange.
Packages: EUDIW, OID4VP, Federation, HAIP, PEX.
Status: Reference infrastructure.
[Read the full pattern](crossborder.md)

#### EUDIW Cross-Border

Problem: EU member states need interoperable credential verification infrastructure.
Pattern: EUDIW / ARF reference verification with mdoc and SD-JWT VC support.
Packages: EUDIW, Mdoc, OID4VP, HAIP.
Status: Spec-tracking (eIDAS 2.0 / ARF).
[Read the full pattern](eudiw-cross-border-verification.md)

---

## Common boundaries

Production deployments require work outside the library:

- **Legal review** -- regulatory compliance, data protection, contractual obligations
- **Policy rules** -- which claims to request, which issuers to trust, which actions to allow
- **User experience** -- consent flows, wallet selection, error handling
- **Trust onboarding** -- issuer and verifier enrollment, federation configuration
- **Key custody** -- hardware security modules, key rotation, secure storage
- **Storage** -- credential storage, audit log retention, session management
- **Operations** -- monitoring, alerting, incident response, scaling, security assessment

---

## Further reading

- [How to Read These Patterns](00-how-to-read-these-patterns.md) -- maturity labels, code conventions, navigation
- [Core Reusable Patterns](01-patterns/) -- composable trust building blocks
- [Getting Started](../getting-started/) -- package installation and first steps
- [Concepts](../concepts/) -- protocol and architecture fundamentals
- [Guides](../guides/) -- step-by-step procedures
- [Package READMEs](../../src/) -- API-specific guidance
