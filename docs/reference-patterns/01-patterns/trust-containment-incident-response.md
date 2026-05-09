# Trust Containment and Incident Response

> **Pattern type:** Core reusable pattern
> **Maturity:** Stable primitive
> **Key packages:** `SdJwt.Net.OidFederation`, `SdJwt.Net.StatusList`

## What it does

Contains credential ecosystem incidents through two coordinated planes: trust containment (federation policy updates) and lifecycle containment (status list revocation). Together they limit the blast radius when an issuer is compromised, a signing key is leaked, or a federation member is found to be malicious.

## When to use it

- An issuer signing key is compromised
- A federation member is issuing fraudulent credentials
- A batch of credentials must be revoked due to a data breach
- Verifiers must stop accepting credentials from a specific issuer

## How it works

1. **Detection**: The incident is detected through monitoring, reporting, or external notification.
2. **Trust containment**: The federation trust anchor updates its subordinate statements to remove or mark the compromised entity. Verifiers that resolve trust chains will stop trusting the entity on their next cache refresh.
3. **Lifecycle containment**: The issuer (or a delegated authority) updates the status list to revoke affected credentials. Verifiers that check status will reject the credentials on their next cache refresh.
4. **Verifier convergence**: Cache invalidation signals are sent to accelerate verifier convergence. The containment window is bounded by the slowest verifier's cache TTL.
5. **Evidence collection**: Immutable evidence artifacts (timestamps, affected credentials, federation state changes) are recorded for post-incident review.

## Package roles

| Package                   | Role                                          |
| ------------------------- | --------------------------------------------- |
| `SdJwt.Net.OidFederation` | Trust chain updates and entity removal        |
| `SdJwt.Net.StatusList`    | Credential revocation and suspension          |
| `SdJwt.Net.HAIP`          | Security baseline enforcement during recovery |

## Application responsibility

Incident detection and triage, SIEM integration, cache invalidation orchestration, evidence storage, post-incident review process, communication to affected parties.

## Used by

- [Incident Response](../incident-response.md) -- full reference workflow for credential ecosystem incidents
