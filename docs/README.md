# SD-JWT .NET Documentation

![SD-JWT .NET Logo](images/sdjwtnet.png)

Welcome to the comprehensive documentation for the **SD-JWT .NET** ecosystem—a highly modular, production-ready implementation of Selective Disclosure JSON Web Tokens (RFC 9901) and the OpenID for Verifiable Credentials (OpenID4VC) stack.

This documentation is organized around the [Diátaxis framework](https://diataxis.fr/), splitting content into four distinct categories based on your immediate needs:

## Start Here (Developer Path)

Use this reading order if you are onboarding to the repository:

1. [15-Minute Quickstart](getting-started/quickstart.md)
2. [Running the Sample Architecture](getting-started/running-the-samples.md)
3. [Ecosystem Architecture](concepts/architecture.md)
4. [SD-JWT Deep Dive](concepts/sd-jwt-deep-dive.md)
5. [How to Issue Verifiable Credentials](guides/issuing-credentials.md)
6. [Use Cases Index](use-cases/README.md)

## Docs Folder Map

| Folder             | Purpose                               | Start With                                              |
| ------------------ | ------------------------------------- | ------------------------------------------------------- |
| `getting-started/` | Tutorials and first-run walkthroughs  | [quickstart.md](getting-started/quickstart.md)          |
| `tutorials/`       | Step-by-step tutorials (beginner-advanced) | [README.md](tutorials/README.md)                   |
| `guides/`          | Task-oriented implementation guides   | [issuing-credentials.md](guides/issuing-credentials.md) |
| `concepts/`        | Architecture and protocol explanation | [architecture.md](concepts/architecture.md)             |
| `use-cases/`       | Industry use cases and patterns       | [README.md](use-cases/README.md)                        |
| `images/`          | Shared documentation assets           | `sdjwtnet.png`                                          |

---

## 1. Getting Started (Tutorials)

_Learning-oriented. Goal: Get you up and running successfully in 15 minutes._

- [**15-Minute Quickstart**](getting-started/quickstart.md): Build your first Issuer, Wallet, and Verifier in a single console application.
- [**Running the Sample Architecture**](getting-started/running-the-samples.md): How to launch and play with our interactive CLI that demonstrates all packages working together.

---

## 2. How-To Guides

_Problem-oriented. Goal: Practical, step-by-step guides for solving specific problems using the ecosystem packages._

- [**How to Issue Verifiable Credentials**](guides/issuing-credentials.md): Setting up an `Oid4Vci` Issuer to mint W3C-compliant SD-JWTs.
- [**How to Verify Presentations**](guides/verifying-presentations.md): Configuring a Relying Party to request specific data using Presentation Exchange.
- [**How to Manage Credential Revocation**](guides/managing-revocation.md): Implementing privacy-preserving Status Lists.
- [**How to Establish Trust**](guides/establishing-trust.md): Using OpenID Federation to dynamically resolve Trust Chains.

---

## 3. Concepts

_Understanding-oriented. Goal: Deep architectural dives, diagrams, and "Why" explanations._

- [**Ecosystem Architecture**](concepts/architecture.md): The master architectural overview mapping out the Protocol, Policy, and Core layers.
- [**HAIP Compliance**](concepts/haip-compliance.md): Understanding the High Assurance Interoperability Profile levels and automated compliance enforcement.
- [**Selective Disclosure Mechanics**](concepts/selective-disclosure-mechanics.md): A deep dive into exactly how salts, hashes, and Key Binding JWTs work under the hood.
- [**SD-JWT Deep Dive**](concepts/sd-jwt-deep-dive.md): Purpose, format structure, issuance/presentation/verification mechanics, and references.
- [**Status List Deep Dive**](concepts/status-list-deep-dive.md): Lifecycle status model, token format, and revocation/suspension verification flow.
- [**Verifiable Credential Deep Dive**](concepts/verifiable-credential-deep-dive.md): SD-JWT VC profile claims, lifecycle, and validation expectations.
- [**OID4VCI Deep Dive**](concepts/openid4vci-deep-dive.md): Issuance protocol artifacts and pre-authorized/authorization-code flow behavior.
- [**OID4VP Deep Dive**](concepts/openid4vp-deep-dive.md): Presentation request/response protocol with nonce and submission validation.
- [**HAIP Deep Dive**](concepts/haip-deep-dive.md): Assurance profiles, policy controls, and enforcement pipeline.
- [**Presentation Exchange Deep Dive**](concepts/presentation-exchange-deep-dive.md): Definition/constraint model and credential matching semantics.

---

## 4. Use Cases & Reference

_Industry patterns and advanced implementation scenarios._

- [**Financial AI (Superannuation)**](use-cases/financial-ai.md): Verified context for AI in regulated finance.
- [**Cross-Border Government (EU)**](use-cases/crossborder.md): EUDI Wallet integration with OOTS.
- [**Telecom eSIM Security**](use-cases/telco-esim.md): Fraud-resistant SIM swap and number porting.
- [**E-Commerce Returns**](use-cases/retail-ecommerce-returns.md): Verifiable receipts for instant refunds.
- [**Automated Compliance**](use-cases/automated-compliance.md): Policy-first data minimization.
- [**Incident Response**](use-cases/incident-response.md): Trust containment with federation and status lists.
- [**Post-Quantum Readiness**](use-cases/quantum-key-distribution.md): Migration guidance for PQC.
- [**Use Cases Index**](use-cases/README.md): Full catalog of industry use cases.

---

## API & Package Reference

For detailed API references and code documentation for individual NuGet packages, see the respective source code directories:

- [`SdJwt.Net`](../src/SdJwt.Net/README.md)
- [`SdJwt.Net.Vc`](../src/SdJwt.Net.Vc/README.md)
- [`SdJwt.Net.Oid4Vci`](../src/SdJwt.Net.Oid4Vci/README.md)
- [`SdJwt.Net.Oid4Vp`](../src/SdJwt.Net.Oid4Vp/README.md)
- [`SdJwt.Net.PresentationExchange`](../src/SdJwt.Net.PresentationExchange/README.md)
- [`SdJwt.Net.StatusList`](../src/SdJwt.Net.StatusList/README.md)
- [`SdJwt.Net.OidFederation`](../src/SdJwt.Net.OidFederation/README.md)
- [`SdJwt.Net.HAIP`](../src/SdJwt.Net.HAIP/README.md)

---

## Mermaid Rendering

All architecture and workflow diagrams use GitHub-compatible Mermaid syntax.

- In GitHub, Mermaid fences render natively in Markdown preview and repository pages.
- For local docs rendering, `zensical.toml` now includes Mermaid fence configuration via `pymdownx.superfences`.
