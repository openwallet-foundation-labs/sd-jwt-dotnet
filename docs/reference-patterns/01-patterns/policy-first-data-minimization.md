# Policy-First Data Minimization

> **Pattern type:** Core reusable pattern
> **Maturity:** Stable primitive
> **Key packages:** `SdJwt.Net.PresentationExchange`, `SdJwt.Net.Oid4Vp`, `SdJwt.Net.StatusList`

## What it does

Defines a disclosure policy before requesting data. The verifier specifies exactly which claims it needs via a presentation definition. The holder discloses only those claims. The policy is the starting point, not an afterthought.

## When to use it

- Regulatory frameworks require minimum-necessary data sharing (HIPAA, GDPR, eIDAS)
- Audit teams need evidence that only required claims were requested and received
- Data minimization must be provable, not just claimed

## How it works

1. **Policy definition**: The verifier creates a presentation definition specifying required claims, formats, and constraints.
2. **Request**: The verifier sends the presentation definition to the holder via OID4VP.
3. **Selective disclosure**: The holder's wallet matches credentials to the definition and discloses only the required claims.
4. **Verification**: The verifier confirms the presentation satisfies the definition and verifies credential authenticity.
5. **Audit evidence**: The presentation definition and verified response form an auditable record of what was requested and received.

## Package roles

| Package                          | Role                                      |
| -------------------------------- | ----------------------------------------- |
| `SdJwt.Net.PresentationExchange` | Structured disclosure requirements        |
| `SdJwt.Net.Oid4Vp`               | Presentation protocol                     |
| `SdJwt.Net.Vc`                   | Selectively disclosable credential format |
| `SdJwt.Net.StatusList`           | Credential freshness checking             |

## Application responsibility

Policy authoring, regulatory mapping, consent UX, audit log storage, legal review of disclosure requirements.

## Used by

- [Policy-First Data Minimization](../policy-first-data-minimization.md) -- full reference pattern
- [Financial AI](../financial-ai.md) -- minimum-necessary member data for AI copilots
- [Healthcare Credentials](../healthcare-credential-verification.md) -- HIPAA minimum necessary support
- [Supplier Onboarding](../supplier-onboarding.md) -- minimum claims for procurement eligibility
- [Education Skills Passport](../education-skills-passport.md) -- prove qualification without full transcript
- [Construction Readiness](../construction-readiness-passport.md) -- minimum claims per milestone gate
