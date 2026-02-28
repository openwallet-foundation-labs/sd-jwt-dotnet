# SD-JWT .NET Tutorials

Step-by-step guides for learning SD-JWT implementation with working code samples.

## Learning Path

### Week 1: Fundamentals

Start with these beginner tutorials to understand core SD-JWT concepts:

1. [Hello SD-JWT](beginner/01-hello-sd-jwt.md) - Create your first SD-JWT (5 min)
2. [Selective Disclosure](beginner/02-selective-disclosure.md) - Hide and reveal claims (10 min)
3. [Holder Binding](beginner/03-holder-binding.md) - Cryptographic proof of ownership (10 min)
4. [Verification Flow](beginner/04-verification-flow.md) - Complete issuer-holder-verifier cycle (15 min)

### Week 2: Standards

Build production skills with protocol tutorials:

1. [Verifiable Credentials](intermediate/01-verifiable-credentials.md) - SD-JWT VC standard (15 min)
2. [Status List](intermediate/02-status-list.md) - Revocation and suspension (15 min)
3. [OpenID4VCI](intermediate/03-openid4vci.md) - Credential issuance protocol (20 min)
4. [OpenID4VP](intermediate/04-openid4vp.md) - Presentation protocol (20 min)
5. [Presentation Exchange](intermediate/05-presentation-exchange.md) - DIF query language (15 min)

### Week 3: Production

Advanced tutorials for enterprise deployment:

1. [OpenID Federation](advanced/01-openid-federation.md) - Trust chain management (20 min)
2. [HAIP Compliance](advanced/02-haip-compliance.md) - Security levels 1-3 (15 min)
3. [Multi-Credential Flow](advanced/03-multi-credential-flow.md) - Combined presentations (20 min)
4. [Key Rotation](advanced/04-key-rotation.md) - Operational security (15 min)

## Running the Samples

All tutorials have corresponding runnable code in the `samples/SdJwt.Net.Samples/` directory.

```bash
cd samples/SdJwt.Net.Samples
dotnet run
```

Or run specific tutorials directly:

```bash
dotnet run -- 1.1    # Hello SD-JWT
dotnet run -- 2.3    # OpenID4VCI
dotnet run -- 3.2    # HAIP Compliance
```

## Prerequisites

- .NET 9.0 SDK or later
- Basic understanding of JWTs and public key cryptography
- IDE with C# support (Visual Studio, VS Code, Rider)

## Related Resources

- [Concepts](../concepts/) - Architecture and design principles
- [Guides](../guides/) - Integration patterns
- [Use Cases](../use-cases/) - Industry scenarios
