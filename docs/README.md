# SD-JWT .NET Documentation

![SD-JWT .NET Logo](images/sdjwtnet.png)

Welcome to the comprehensive documentation for the **SD-JWT .NET** ecosystem—a highly modular, production-ready implementation of Selective Disclosure JSON Web Tokens (RFC 9901) and the OpenID for Verifiable Credentials (OpenID4VC) stack.

This documentation is organized around the [Diátaxis framework](https://diataxis.fr/), splitting content into four distinct categories based on your immediate needs:

---

## 🚀 1. Getting Started (Tutorials)

*Learning-oriented. Goal: Get you up and running successfully in 15 minutes.*

* [**15-Minute Quickstart**](getting-started/quickstart.md): Build your first Issuer, Wallet, and Verifier in a single console application.
* [**Running the Sample Architecture**](getting-started/running-the-samples.md): How to launch and play with our interactive CLI that demonstrates all packages working together.

---

## 🛠 2. How-To Guides

*Problem-oriented. Goal: Practical, step-by-step guides for solving specific problems using the ecosystem packages.*

* [**How to Issue Verifiable Credentials**](guides/issuing-credentials.md): Setting up an `Oid4Vci` Issuer to mint W3C-compliant SD-JWTs.
* [**How to Verify Presentations**](guides/verifying-presentations.md): Configuring a Relying Party to request specific data using Presentation Exchange.
* [**How to Manage Credential Revocation**](guides/managing-revocation.md): Implementing privacy-preserving Status Lists.
* [**How to Establish Trust**](guides/establishing-trust.md): Using OpenID Federation to dynamically resolve Trust Chains.

---

## 🧠 3. Concepts

*Understanding-oriented. Goal: Deep architectural dives, diagrams, and "Why" explanations.*

* [**Ecosystem Architecture**](concepts/architecture.md): The master architectural overview mapping out the Protocol, Policy, and Core layers.
* [**HAIP Compliance**](concepts/haip-compliance.md): Understanding the High Assurance Interoperability Profile levels and automated compliance enforcement.
* [**Selective Disclosure Mechanics**](concepts/selective-disclosure-mechanics.md): A deep dive into exactly how salts, hashes, and Key Binding JWTs work under the hood.

---

## 💡 4. Insights & Reference

*Ecosystem patterns, thought leadership, and advanced use cases.*

* [**AI Financial Co-Pilot**](insights/ai-financial-co-pilot.md): Utilizing LLMs with zero-knowledge data retrieval.
* [**Automated Compliance & Data Minimization**](insights/automated-compliance.md): AI-driven Presentation Definition interception.
* [**Quantum Key Distribution (QKD)**](insights/quantum-key-distribution.md): Securing sovereign trust anchors with post-quantum cryptography.
* [**Automated Incident Response**](insights/incident-response.md): Zero-day containment using webhooks and Status Lists.
* [**Versioning Strategy**](insights/versioning-and-release.md): Our automated Semantic Versioning pipeline.

---

## 📦 API & Package Reference

For detailed API references and code documentation for individual NuGet packages, see the respective source code directories:

* [`SdJwt.Net`](../src/SdJwt.Net/README.md)
* [`SdJwt.Net.Vc`](../src/SdJwt.Net.Vc/README.md)
* [`SdJwt.Net.Oid4Vci`](../src/SdJwt.Net.Oid4Vci/README.md)
* [`SdJwt.Net.Oid4Vp`](../src/SdJwt.Net.Oid4Vp/README.md)
* [`SdJwt.Net.PresentationExchange`](../src/SdJwt.Net.PresentationExchange/README.md)
* [`SdJwt.Net.StatusList`](../src/SdJwt.Net.StatusList/README.md)
* [`SdJwt.Net.OidFederation`](../src/SdJwt.Net.OidFederation/README.md)
* [`SdJwt.Net.HAIP`](../src/SdJwt.Net.HAIP/README.md)
