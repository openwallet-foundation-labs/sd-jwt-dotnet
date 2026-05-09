# What SD-JWT .NET Is - and Is Not

> SD-JWT .NET is a standards-first .NET library ecosystem.
> This document defines the ecosystem boundary so package docs, samples, and proposals use the same positioning.
> Unless explicitly marked Stable, packages are not certification claims or finished external standards.

## It Is

- A standards-first .NET implementation of RFC 9901 SD-JWT.
- A reusable ecosystem for verifiable credentials and wallet interoperability.
- A set of protocol components for OID4VCI, OID4VP, Presentation Exchange, OpenID Federation, HAIP, mdoc, and related flows.
- Reference infrastructure for wallet and EUDIW-style implementations.
- A preview experimentation area for delegated Agent Trust.

## It Is Not

- Not a standalone consumer wallet.
- Not an identity provider.
- Not an OAuth authorization server.
- Not a certification authority.
- Not an EUDIW-certified wallet product.
- Not a finished external standard for AI-agent authorization.

## Terminology

| Use                                      | Avoid                                            |
| ---------------------------------------- | ------------------------------------------------ |
| library ecosystem                        | full platform                                    |
| reference infrastructure                 | production wallet                                |
| standards-aligned                        | certified or compliant unless certified          |
| profile validation                       | compliance certification                         |
| preview Agent Trust extension            | Agent Trust standard                             |
| capability token minting                 | credential issuance, unless referring to OID4VCI |
| wallet framework / wallet infrastructure | consumer wallet app                              |
| delegated agent trust                    | AI identity standard                             |

## Related Documentation

- [Standards and Maturity Status](../reference/standards-status.md)
- [Ecosystem Architecture](ecosystem-architecture.md)
- [Package Maturity](../../MATURITY.md)
