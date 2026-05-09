# How to Read These Reference Patterns

These documents are not turnkey products or compliance certifications. They are reference architecture patterns showing how SD-JWT .NET packages can be composed into real-world trust workflows.

## Three layers in every pattern

Each pattern has three layers:

1. **Business problem** -- why this matters and what is broken today.
2. **Trust pattern** -- how selective disclosure, status lists, federation, OID4VP, HAIP, mdoc, or Agent Trust addresses the problem.
3. **Implementation path** -- which packages, samples, and documentation to start with.

Business readers can stop at layer 1. Architects should focus on layers 1 and 2. Developers should read all three and jump to the linked samples.

## Maturity labels

Every pattern page carries a maturity label. These labels describe what the library provides today, not what a production deployment will look like.

| Label                      | Meaning                                                                                           |
| -------------------------- | ------------------------------------------------------------------------------------------------- |
| Stable primitive           | Based on a stable, tested package capability (SD-JWT, SD-JWT VC, Status Lists, PEX)               |
| Spec-tracking              | Tracks an evolving standard or draft (DC API, OID4VP, EUDIW ARF); API surface may change          |
| Reference infrastructure   | Useful for prototypes and architecture patterns, not a certified or production-hardened product   |
| Preview extension          | Early-stage package or project-defined pattern (Agent Trust); APIs may change between releases    |
| Application responsibility | Must be implemented outside the library (policy rules, UX, key custody, legal review, operations) |

## Code block labels

Code blocks in these documents carry one of two labels:

- **Illustrative application-layer pseudocode.** The code shows how packages could be composed into an application workflow. It references package types but is not a standalone runnable sample.
- **Runnable sample.** The code can be compiled and executed. A link to the sample project is provided.

If a code block is not labeled, treat it as illustrative.

## What these patterns do not cover

Every pattern in this folder has an "application responsibility" section that lists what your team must build, configure, or procure outside the library. Common application responsibilities include:

- **Policy rules** -- which claims to request, which issuers to trust, which actions to allow
- **User experience** -- consent flows, wallet selection, error handling
- **Trust onboarding** -- issuer and verifier enrollment, federation configuration
- **Legal review** -- regulatory compliance, data protection, contractual obligations
- **Key custody** -- hardware security modules, key rotation, secure storage
- **Operations** -- monitoring, alerting, incident response, scaling
- **Storage** -- credential storage, audit log retention, session management

## How to navigate

| If you are...                            | Start with                                                                               |
| ---------------------------------------- | ---------------------------------------------------------------------------------------- |
| A developer wanting to build something   | [Developer path](README.md#developer-path) in the README                                 |
| An architect evaluating the ecosystem    | [Architect path](README.md#enterprise-architect-path) in the README                      |
| Working on wallet / identity systems     | [Wallet / identity path](README.md#wallet--identity-architect-path) in the README        |
| Evaluating agent security                | [Agent security path](README.md#agent-security-path) in the README                       |
| Looking for a specific business problem  | [Choose by business problem](README.md#choose-by-business-problem) table in the README   |
| Looking for a specific technical pattern | [Choose by technical pattern](README.md#choose-by-technical-pattern) table in the README |
| Looking for reusable building blocks     | [Core reusable patterns](01-patterns/) directory                                         |

## Feedback

If a pattern overclaims, is missing a boundary statement, or needs a correction, open an issue or submit a PR.
