# Multi-Format Verifier

> **Pattern type:** Core reusable pattern
> **Maturity:** Stable primitive (SD-JWT VC) + Spec-tracking (mdoc)
> **Key packages:** `SdJwt.Net.Vc`, `SdJwt.Net.Mdoc`, `SdJwt.Net.Oid4Vp`, `SdJwt.Net.HAIP`

## What it does

Accepts both SD-JWT VC and mdoc (ISO 18013-5) credentials in a single verifier flow. HAIP profile validation enforces a common security baseline across formats.

## When to use it

- Verifiers must accept credentials from multiple wallet ecosystems (EUDIW, mDL, enterprise wallets)
- Government identity documents use mdoc while enterprise credentials use SD-JWT VC
- A single verification endpoint must handle both formats without format-specific branching in application code

## How it works

1. **Format detection**: The verifier receives a presentation and detects whether it contains SD-JWT VC or mdoc credentials.
2. **Format-specific verification**: Each credential is verified according to its format rules (SD-JWT signature verification or mdoc COSE verification).
3. **HAIP validation**: Both formats are validated against the HAIP profile for algorithm, key, and binding requirements.
4. **Claim extraction**: Verified claims are extracted into a normalized structure regardless of source format.
5. **Policy evaluation**: The normalized claims are evaluated against the verifier's business policy.

## Package roles

| Package                          | Role                                             |
| -------------------------------- | ------------------------------------------------ |
| `SdJwt.Net.Vc`                   | SD-JWT VC verification                           |
| `SdJwt.Net.Mdoc`                 | mdoc (ISO 18013-5) verification                  |
| `SdJwt.Net.Oid4Vp`               | Presentation protocol for both formats           |
| `SdJwt.Net.HAIP`                 | Security profile validation across formats       |
| `SdJwt.Net.PresentationExchange` | Structured queries that can target either format |

## Application responsibility

Format priority policy, claim normalization logic, verifier endpoint hosting, UX for format selection, key trust configuration per format.

## Used by

- [mdoc Identity Verification](../mdoc-identity-verification.md) -- full mdoc verification reference
- [EUDIW Cross-Border](../eudiw-cross-border-verification.md) -- PID and attestation verification across formats
- [DC API Web Verification](../dc-api-web-verification.md) -- browser-mediated multi-format presentation
