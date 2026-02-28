# Beginner Tutorials

Welcome to SD-JWT .NET! This folder contains tutorials for developers new to selective disclosure.

## Learning Path (30-45 minutes)

| # | Tutorial | What You Learn | Time |
|---|----------|----------------|------|
| 01 | [HelloSdJwt](01-HelloSdJwt.cs) | Create your first SD-JWT | 5 min |
| 02 | [SelectiveDisclosure](02-SelectiveDisclosure.cs) | Understand `_sd` digests and disclosure | 10 min |
| 03 | [HolderBinding](03-HolderBinding.cs) | Bind credentials to a holder key | 10 min |
| 04 | [VerificationFlow](04-VerificationFlow.cs) | Complete issuer-holder-verifier round-trip | 15 min |

## Prerequisites

- .NET 9.0 or later
- Basic C# knowledge
- Understanding of JWTs (helpful but not required)

## Running the Tutorials

From the samples directory:

```bash
cd samples/SdJwt.Net.Samples
dotnet run
```

Select option `1` (Beginner Tutorials) from the menu.

## Key Concepts

### What is SD-JWT?

SD-JWT (Selective Disclosure JWT) is a standard (RFC 9901) that extends regular JWTs to allow holders to selectively reveal only the claims they choose.

```
Regular JWT:
  Verifier sees: { name, email, address, ssn, birthdate }

SD-JWT:
  Verifier sees: { name } (only what holder discloses)
```

### The Three Roles

1. **Issuer** - Creates the SD-JWT with all claims
2. **Holder** - Receives the SD-JWT and controls what to disclose
3. **Verifier** - Validates the presentation and sees only disclosed claims

### Key Terms

| Term | Meaning |
|------|---------|
| `_sd` | Array of digests for selectively disclosable claims |
| Disclosure | Base64-encoded `[salt, claim_name, claim_value]` |
| Key Binding | Cryptographic proof that holder controls the credential |
| KB-JWT | Key Binding JWT - proves holder possession |

## Next Steps

After completing these tutorials:

1. Move to [02-Intermediate](../02-Intermediate/) for Verifiable Credentials
2. Explore standards like OpenID4VCI and OpenID4VP
3. See real-world use cases in [04-UseCases](../04-UseCases/)

## Related Documentation

- [SD-JWT Deep Dive](../../../docs/concepts/sd-jwt-deep-dive.md)
- [RFC 9901 Specification](https://www.rfc-editor.org/rfc/rfc9901.html)
- [Architecture Overview](../../../docs/concepts/architecture.md)
