# Implementation Guides

These guides show how to wire SD-JWT .NET packages into issuer, verifier, wallet, trust, status, and agent workflows.

Use [concepts](../concepts/README.md) first if you are learning the standards.
Use [tutorials](../tutorials/README.md) first if you want a runnable walkthrough.
Use implementation guides when you are integrating packages into an application.

---

## Choose by role

| I am building...                  | Start here                                                                        |
| --------------------------------- | --------------------------------------------------------------------------------- |
| Issuer service                    | [How to build an SD-JWT VC issuer service](issuing-credentials.md)                |
| Verifier service                  | [How to build an OID4VP verifier pipeline](verifying-presentations.md)            |
| Credential lifecycle              | [How to manage credential status with Status Lists](managing-revocation.md)       |
| Multi-party trust                 | [How to establish issuer trust with OpenID Federation](establishing-trust.md)     |
| Holder-side wallet infrastructure | [How to use reference wallet infrastructure](wallet-integration.md)               |
| EUDIW / ARF prototype             | [How to use EUDIW / ARF reference helpers](eudi-wallet-integration.md)            |
| Agent tool-call governance        | [How to integrate preview Agent Trust into tool APIs](agent-trust-integration.md) |
| Real-time status (advanced)       | [How to combine Status Lists and Introspection](token-introspection.md)           |

## Concepts first

If you are new to the topic, read the [concept page](../concepts/README.md) first.

## Tutorials first

If you want a runnable walkthrough, run the [tutorial](../tutorials/README.md) first.

## Guides next

Use guides when integrating the capability into your application.
