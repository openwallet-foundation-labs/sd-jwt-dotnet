# Federated Trust Onboarding

> **Pattern type:** Core reusable pattern
> **Maturity:** Stable primitive
> **Key packages:** `SdJwt.Net.OidFederation`

## What it does

Dynamically resolves issuer trust across organizations using OpenID Federation trust chains. Instead of maintaining static trust lists, verifiers resolve trust at verification time by walking the federation hierarchy.

## When to use it

- Issuers span multiple organizations, jurisdictions, or industries
- Static trust lists are difficult to maintain and keep current
- New issuers must be onboarded without manual verifier configuration
- Trust must be revocable at the federation level (not just the credential level)

## How it works

1. **Federation setup**: A trust anchor publishes its entity configuration and subordinate statements.
2. **Issuer registration**: Issuers register as subordinate entities in the federation, publishing their own entity configurations.
3. **Trust resolution**: When a verifier receives a credential, it resolves the issuer's trust chain back to a known trust anchor.
4. **Policy evaluation**: The verifier evaluates the resolved trust chain against its trust policy (accepted anchors, metadata constraints).
5. **Dynamic updates**: When an issuer is removed from the federation, trust resolution fails for that issuer's credentials without requiring verifier-side changes.

## Package roles

| Package                   | Role                                                                 |
| ------------------------- | -------------------------------------------------------------------- |
| `SdJwt.Net.OidFederation` | Trust chain resolution, entity configuration, subordinate statements |

## Application responsibility

Trust anchor operation, federation membership management, trust policy configuration, entity configuration hosting, cache management.

## Used by

- [Retail E-Commerce Returns](../retail-ecommerce-returns.md) -- merchant trust onboarding for marketplace returns
- [Telecom eSIM](../telco-esim.md) -- carrier trust across jurisdictions
- [Cross-Border Government](../crossborder.md) -- cross-border issuer trust
- [EUDIW Cross-Border](../eudiw-cross-border-verification.md) -- EU member state trust framework
- [Supplier Onboarding](../supplier-onboarding.md) -- evidence provider trust across industries
- [Insurance Claims Evidence](../insurance-claims-evidence.md) -- evidence provider trust resolution
- [Incident Response](../incident-response.md) -- trust containment during incidents
