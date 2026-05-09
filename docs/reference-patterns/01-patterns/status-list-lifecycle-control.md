# Status List Lifecycle Control

> **Pattern type:** Core reusable pattern
> **Maturity:** Stable primitive
> **Key packages:** `SdJwt.Net.StatusList`

## What it does

Manages credential lifecycle through a compact, privacy-preserving status list. Issuers can revoke, suspend, or let credentials expire. Verifiers check status before accepting a credential.

## When to use it

- Credentials must be revocable after issuance (insurance policies, certifications, licenses)
- A credential compromise requires fast containment
- Business processes need to distinguish between active, suspended, and revoked credentials
- A single-use credential (return receipt, voucher) must be consumed and marked used

## How it works

1. **Issuance**: The issuer assigns a status list index to each credential at issuance time.
2. **Status update**: When a credential must be revoked or suspended, the issuer updates the bit at the assigned index.
3. **Publication**: The issuer publishes the updated status list (a compact JWT containing a bitstring).
4. **Verification**: The verifier fetches the status list and checks the credential's index bit before accepting.
5. **Caching**: Verifiers cache the status list according to the TTL. Freshness depends on cache configuration.

## Package roles

| Package                | Role                                                                        |
| ---------------------- | --------------------------------------------------------------------------- |
| `SdJwt.Net.StatusList` | Status list creation, update, and verification (Token Status List draft-20) |

## Application responsibility

Status list hosting and distribution, credential index assignment, cache TTL configuration, revocation workflow triggers, storage for status list state.

## Used by

- [Incident Response](../incident-response.md) -- credential revocation during security incidents
- [Retail E-Commerce Returns](../retail-ecommerce-returns.md) -- single-use return receipt lifecycle
- [Telecom eSIM](../telco-esim.md) -- eSIM credential revocation on SIM swap
- [Insurance Claims Evidence](../insurance-claims-evidence.md) -- policy and certificate status checking
- [Education Skills Passport](../education-skills-passport.md) -- credential revocation for withdrawn qualifications
- [Construction Readiness](../construction-readiness-passport.md) -- real-time status of approvals and permits
