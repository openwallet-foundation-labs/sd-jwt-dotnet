# SD-JWT .NET tutorials

Step-by-step guides for learning SD-JWT implementation with working code samples.

## Learning path

### Week 1: Fundamentals

Start with these beginner tutorials to understand core SD-JWT concepts:

1. [Hello SD-JWT](beginner/01-hello-sd-jwt.md) - Create your first SD-JWT (5 min)
2. [Selective Disclosure](beginner/02-selective-disclosure.md) - Hide and reveal claims (10 min)
3. [Holder Binding](beginner/03-holder-binding.md) - Cryptographic proof of ownership (10 min)
4. [Verification Flow](beginner/04-verification-flow.md) - Complete issuer-holder-verifier cycle (15 min)
5. [Hello mdoc](beginner/05-hello-mdoc.md) - Create your first ISO 18013-5 credential (10 min)

### Week 2: Standards

Protocol tutorials for credential issuance, presentation, and revocation:

1. [Verifiable Credentials](intermediate/01-verifiable-credentials.md) - SD-JWT VC standard (15 min)
2. [Status List](intermediate/02-status-list.md) - Revocation and suspension (15 min)
3. [OpenID4VCI](intermediate/03-openid4vci.md) - Credential issuance protocol (20 min)
4. [OpenID4VP](intermediate/04-openid4vp.md) - Presentation protocol (20 min)
5. [Presentation Exchange](intermediate/05-presentation-exchange.md) - DIF query language (15 min)
6. [mdoc Issuance](intermediate/06-mdoc-issuance.md) - Complete mdoc credential flows (20 min)
7. [Preview: Agent Trust Kits](intermediate/07-agent-trust-kits.md) - Capability token enforcement for agent tool calls (25 min)
8. [W3C VCDM 2.0](intermediate/08-w3c-vcdm.md) - W3C Verifiable Credentials Data Model (15 min)

### Week 3: Production

Advanced tutorials for enterprise deployment:

1. [OpenID Federation](advanced/01-openid-federation.md) - Trust chain management (20 min)
2. [HAIP Profile Validation](advanced/02-haip-compliance.md) - HAIP Final flows and credential profiles (20 min)
3. [Multi-Credential Flow](advanced/03-multi-credential-flow.md) - Combined presentations (20 min)
4. [Key Rotation](advanced/04-key-rotation.md) - Operational security (15 min)
5. [mdoc OpenID4VP Integration](advanced/05-mdoc-integration.md) - mdoc presentation flows (25 min)
6. [EUDIW / ARF Reference](advanced/06-eudiw-compliance.md) - EU Digital Identity Wallet (25 min)

## Choose by goal

| I want to...                        | Start here                                                       |
| ----------------------------------- | ---------------------------------------------------------------- |
| Issue my first SD-JWT               | [Hello SD-JWT](beginner/01-hello-sd-jwt.md)                      |
| Hide claims from a verifier         | [Selective Disclosure](beginner/02-selective-disclosure.md)      |
| Build a full issuance + verify loop | [Verification Flow](beginner/04-verification-flow.md)            |
| Issue credentials via a protocol    | [OpenID4VCI](intermediate/03-openid4vci.md)                      |
| Present credentials to a verifier   | [OpenID4VP](intermediate/04-openid4vp.md)                        |
| Revoke or suspend credentials       | [Status List](intermediate/02-status-list.md)                    |
| Work with mobile driving licenses   | [Hello mdoc](beginner/05-hello-mdoc.md)                          |
| Validate HAIP profile requirements  | [HAIP Profile Validation](advanced/02-haip-compliance.md)        |
| Authorize AI agent tool calls       | [Preview: Agent Trust Kits](intermediate/07-agent-trust-kits.md) |
| Prepare for EUDIW / eIDAS 2.0       | [EUDIW / ARF Reference](advanced/06-eudiw-compliance.md)         |

## Running the samples

All tutorials have corresponding runnable code in the `samples/SdJwt.Net.Samples/` directory.

```bash
cd samples/SdJwt.Net.Samples
dotnet run
```

Or run specific tutorials directly:

```bash
dotnet run -- 1.1    # Hello SD-JWT
dotnet run -- 2.3    # OpenID4VCI
dotnet run -- 3.2    # HAIP Profile Validation
```

## Prerequisites

- .NET 9.0 SDK or later
- Basic understanding of JWTs and public key cryptography
- IDE with C# support (Visual Studio, VS Code, Rider)

## Related resources

- [Concepts](../concepts/) - Architecture and design principles
- [Guides](../guides/) - Integration patterns
- [Reference Patterns](../reference-patterns/) - Industry reference patterns
