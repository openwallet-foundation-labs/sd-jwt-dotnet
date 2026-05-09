# Verified Context Gate

> **Pattern type:** Core reusable pattern
> **Maturity:** Stable primitive
> **Key packages:** `SdJwt.Net.Vc`, `SdJwt.Net.Oid4Vp`, `SdJwt.Net.PresentationExchange`

## What it does

Attaches a verified, minimized credential to a request so the receiving system can trust the context before processing. The gate verifies that the credential is authentic, current, and contains only the claims needed for the specific operation.

## When to use it

- An API needs more than identity to authorize a request (department, approval, compliance status)
- An AI system should only receive verified minimum facts, not raw database access
- A workflow step requires proof of a precondition before proceeding

## How it works

1. **Request context**: The requesting system obtains a verifiable credential from an authoritative source.
2. **Selective disclosure**: The credential is presented with only the claims needed for this specific request.
3. **Gate verification**: The receiving system verifies the credential signature, issuer trust, status, and claim values.
4. **Policy evaluation**: The verified claims are evaluated against the operation's access policy.
5. **Proceed or reject**: The request is processed with verified context or rejected with an audit trail.

## Package roles

| Package                          | Role                                                |
| -------------------------------- | --------------------------------------------------- |
| `SdJwt.Net.Vc`                   | Credential format for context claims                |
| `SdJwt.Net.Oid4Vp`               | Presentation protocol for context exchange          |
| `SdJwt.Net.PresentationExchange` | Structured requirements for which claims are needed |
| `SdJwt.Net.StatusList`           | Credential freshness and revocation checking        |

## Application responsibility

Policy rules, context issuer service, integration with existing authorization systems, key management, audit log storage.

## Used by

- [Financial AI](../financial-ai.md) -- verified member context for AI copilots
- [Enterprise API Access](../enterprise-api-access.md) -- verified client context for API authorization
- [Enterprise KYC Onboarding](../enterprise-kyc-onboarding.md) -- verified identity context for onboarding workflows
- [Healthcare Credentials](../healthcare-credential-verification.md) -- verified patient/provider context
- [Cross-Border Government](../crossborder.md) -- verified citizen context for cross-border services
