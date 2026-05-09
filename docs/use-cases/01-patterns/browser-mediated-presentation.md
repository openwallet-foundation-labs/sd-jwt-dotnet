# Browser-Mediated Presentation

> **Pattern type:** Core reusable pattern
> **Maturity:** Spec-tracking (W3C Digital Credentials API draft)
> **Key packages:** `SdJwt.Net.Oid4Vp`, `SdJwt.Net.PresentationExchange`

## What it does

Uses the browser as a credential presentation channel via the W3C Digital Credentials API. The browser mediates between a relying party website and the user's wallet, providing a native UI for credential selection and consent.

## When to use it

- Web applications need to verify credentials without requiring the user to install a browser extension
- Age verification, identity proofing, or credential checks must happen in a browser session
- The verifier wants to leverage the browser's built-in credential selection UX

## How it works

1. **Request creation**: The relying party creates an OID4VP authorization request with a presentation definition.
2. **DC API call**: The website invokes the Digital Credentials API (`navigator.credentials.get()`) with the OID4VP request.
3. **Browser mediation**: The browser presents a native credential picker to the user, showing available wallets and matching credentials.
4. **Wallet interaction**: The user selects a credential and consents to disclosure. The wallet creates an SD-JWT VP with selectively disclosed claims.
5. **Response delivery**: The browser returns the VP to the relying party website.
6. **Server verification**: The relying party's backend verifies the VP, checks credential status, and evaluates the disclosed claims.

## Package roles

| Package                          | Role                                                        |
| -------------------------------- | ----------------------------------------------------------- |
| `SdJwt.Net.Oid4Vp`               | Authorization request/response handling, DC API integration |
| `SdJwt.Net.PresentationExchange` | Presentation definition for specifying required claims      |
| `SdJwt.Net.HAIP`                 | Security profile validation for the presentation            |
| `SdJwt.Net.Vc`                   | SD-JWT VC verification                                      |

## Application responsibility

Frontend JavaScript for DC API invocation, backend verification endpoint, session management, browser compatibility handling, fallback flows for unsupported browsers.

## Used by

- [DC API Web Verification](../dc-api-web-verification.md) -- full reference pattern for browser-based verification
