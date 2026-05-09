# Core Reusable Patterns

This directory contains reusable trust patterns that appear across multiple industry use cases. Each pattern describes a single building block that can be composed into larger workflows.

Industry use cases in the parent directory reference these patterns rather than repeating the same explanations.

## Pattern catalogue

| Pattern                                                                           | What it does                                                               | Key packages                  |
| --------------------------------------------------------------------------------- | -------------------------------------------------------------------------- | ----------------------------- |
| [Verified Context Gate](verified-context-gate.md)                                 | Attach verified, minimized context to a request before processing          | SD-JWT VC, OID4VP, PEX        |
| [Policy-First Data Minimization](policy-first-data-minimization.md)               | Define disclosure policy before requesting data                            | PEX, OID4VP, Status Lists     |
| [Scoped Agent Capability Token](scoped-agent-capability-token.md)                 | Scope each agent tool call to a specific action, resource, and time window | AgentTrust.Core, Policy, MCP  |
| [Status List Lifecycle Control](status-list-lifecycle-control.md)                 | Revoke, suspend, or expire credentials in real time                        | Status Lists                  |
| [Federated Trust Onboarding](federated-trust-onboarding.md)                       | Dynamically resolve issuer trust across organizations                      | OpenID Federation             |
| [Multi-Format Verifier](multi-format-verifier.md)                                 | Accept both SD-JWT VC and mdoc credentials in one verifier                 | SD-JWT VC, Mdoc, OID4VP, HAIP |
| [Browser-Mediated Presentation](browser-mediated-presentation.md)                 | Use the browser as a credential presentation channel                       | OID4VP, DC API, PEX           |
| [Trust Containment and Incident Response](trust-containment-incident-response.md) | Contain credential incidents through federation and status updates         | Federation, Status Lists      |

## How these relate to industry use cases

Each industry use case composes one or more of these patterns:

| Industry use case              | Patterns used                                              |
| ------------------------------ | ---------------------------------------------------------- |
| Financial AI                   | Verified Context Gate, Policy-First Data Minimization      |
| AI Agent Authorization         | Scoped Agent Capability Token                              |
| Policy-First Data Minimization | Policy-First Data Minimization, Status List Lifecycle      |
| Incident Response              | Trust Containment, Status List Lifecycle                   |
| Retail E-Commerce Returns      | Status List Lifecycle, Federated Trust Onboarding          |
| DC API Web Verification        | Browser-Mediated Presentation, Multi-Format Verifier       |
| Enterprise KYC Onboarding      | Verified Context Gate, Federated Trust Onboarding          |
| mdoc Identity Verification     | Multi-Format Verifier                                      |
| Healthcare Credentials         | Policy-First Data Minimization, Verified Context Gate      |
| EUDIW Cross-Border             | Multi-Format Verifier, Federated Trust Onboarding          |
| Telecom eSIM                   | Status List Lifecycle, Federated Trust Onboarding          |
| Cross-Border Government        | Verified Context Gate, Federated Trust Onboarding          |
| Enterprise API Access          | Verified Context Gate, Scoped Agent Capability Token       |
| Supplier Onboarding            | Federated Trust Onboarding, Policy-First Data Minimization |
| Education Skills Passport      | Policy-First Data Minimization, Status List Lifecycle      |
| Insurance Claims Evidence      | Status List Lifecycle, Federated Trust Onboarding          |
| Construction Readiness         | Status List Lifecycle, Policy-First Data Minimization      |
