# SD-JWT .NET Samples

Progressive tutorials and industry use cases for the SD-JWT .NET ecosystem.

## Quick Start

```bash
cd samples/SdJwt.Net.Samples
dotnet run
```

Select a tutorial from the menu, or run directly:

```bash
# Run specific tutorial
dotnet run -- 1.1    # Hello SD-JWT
dotnet run -- 4.2    # Loan Application use case

# Run entire category
dotnet run -- B1     # All beginner tutorials
dotnet run -- ALL    # Everything
```

## Tutorial Structure

### [01-Beginner/](01-Beginner/) - Start Here

Foundation concepts for developers new to SD-JWT:

| #   | Tutorial             | Time   | Concepts                              |
| --- | -------------------- | ------ | ------------------------------------- |
| 1.1 | Hello SD-JWT         | 5 min  | Create and parse your first SD-JWT    |
| 1.2 | Selective Disclosure | 10 min | Hide/reveal individual claims         |
| 1.3 | Holder Binding       | 10 min | Cryptographic proof of ownership      |
| 1.4 | Verification Flow    | 15 min | Complete issuer-holder-verifier cycle |

### [02-Intermediate/](02-Intermediate/) - Build Skills

Standards and protocols for production systems:

| #   | Tutorial               | Time   | Package                        |
| --- | ---------------------- | ------ | ------------------------------ |
| 2.1 | Verifiable Credentials | 15 min | SdJwt.Net.Vc                   |
| 2.2 | Status List            | 15 min | SdJwt.Net.StatusList           |
| 2.3 | OpenID4VCI             | 20 min | SdJwt.Net.Oid4Vci              |
| 2.4 | OpenID4VP              | 20 min | SdJwt.Net.Oid4Vp               |
| 2.5 | Presentation Exchange  | 15 min | SdJwt.Net.PresentationExchange |

### [03-Advanced/](03-Advanced/) - Production Ready

Enterprise and government grade implementations:

| #   | Tutorial              | Time   | Focus                     |
| --- | --------------------- | ------ | ------------------------- |
| 3.1 | OpenID Federation     | 20 min | Trust chains and metadata |
| 3.2 | HAIP Compliance       | 15 min | Security levels 1-3       |
| 3.3 | Multi-Credential Flow | 20 min | Combined presentations    |
| 3.4 | Key Rotation          | 15 min | Operational security      |

### [04-UseCases/](04-UseCases/) - Real World

Complete industry implementations you can adapt:

| #   | Use Case                | Industry   | Privacy Pattern                         |
| --- | ----------------------- | ---------- | --------------------------------------- |
| 4.1 | University Degree       | Education  | Prove degree without GPA                |
| 4.2 | Loan Application        | Finance    | Share income range, hide salary         |
| 4.3 | Patient Consent         | Healthcare | HIPAA-aligned data sharing              |
| 4.4 | Cross-Border Identity   | Government | Travel docs without address             |
| 4.5 | Fraud-Resistant Returns | Retail     | Purchase proof without payment details  |
| 4.6 | eSIM Transfer           | Telecom    | Port number without account credentials |

### [Shared/](Shared/) - Utilities

Common code used across tutorials:

- **KeyHelpers.cs** - Cryptographic key generation for examples
- **ConsoleHelpers.cs** - Console output formatting
- **SampleData.cs** - Industry-specific test data

## Learning Path

**Week 1: Fundamentals**

- Complete all Beginner tutorials (1.1-1.4)
- Understand core SD-JWT concepts

**Week 2: Standards**

- Work through Intermediate tutorials based on your needs
- Focus on VC (2.1) and Status List (2.2) for credential systems
- Focus on OpenID (2.3-2.4) for protocol integration

**Week 3: Production**

- Study Advanced tutorials relevant to your deployment
- Government/enterprise: HAIP (3.2) and Federation (3.1)
- Multi-credential: Flow (3.3) and Rotation (3.4)

**Ongoing: Reference**

- Use Cases as templates for your implementations
- Copy and adapt to your specific requirements

## Prerequisites

- .NET 9.0 SDK or later
- Basic understanding of JWTs
- Familiarity with public key cryptography concepts

## Related Documentation

- [Tutorials](../../docs/tutorials/README.md) - Step-by-step learning path
- [Architecture](../../docs/concepts/architecture.md) - System design
- [Use Cases](../../docs/use-cases/) - Industry scenarios

## Package Reference

All tutorials demonstrate functionality from 8 NuGet packages:

| Package                        | Purpose                                        |
| ------------------------------ | ---------------------------------------------- |
| SdJwt.Net                      | Core SD-JWT (RFC 9901) - used in all tutorials |
| SdJwt.Net.Vc                   | Verifiable Credentials - tutorial 2.1          |
| SdJwt.Net.StatusList           | Revocation/suspension - tutorial 2.2           |
| SdJwt.Net.Oid4Vci              | Credential issuance - tutorial 2.3             |
| SdJwt.Net.Oid4Vp               | Presentation protocol - tutorial 2.4           |
| SdJwt.Net.PresentationExchange | DIF PEX queries - tutorial 2.5                 |
| SdJwt.Net.OidFederation        | Trust management - tutorial 3.1                |
| SdJwt.Net.HAIP                 | Security compliance - tutorial 3.2             |
