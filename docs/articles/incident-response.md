# Incident Response for Credential Ecosystems: Automated Trust Containment with Federation and Status Lists

## Executive summary

The highest-impact identity incident is issuer key compromise. Attackers can mint cryptographically valid but fraudulent credentials until trust is revoked and verifiers stop accepting affected artifacts.

Effective response requires automation:

- Detect compromise signals quickly.
- Contain trust at federation level.
- Revoke or suspend affected credentials at status-list level.
- Propagate verifier-side cache invalidation rapidly.

This article describes a production-oriented control model using `SdJwt.Net.OidFederation` and `SdJwt.Net.StatusList`.

---

## Why manual response fails

Manual incident playbooks create long exposure windows:

- Human approvals delay trust severing.
- Revocation actions are applied inconsistently.
- Verifiers continue using cached trust metadata.

In identity ecosystems, minutes matter.

---

## Containment architecture

Use two coordinated controls.

### 1) Federation containment

- Remove or mark compromised entities per policy.
- Republish affected trust metadata.
- Ensure verifiers refresh chains and reject compromised paths.

### 2) Status-list containment

- Identify credential populations linked to compromised keys or issuers.
- Mark them revoked/suspended according to incident policy.
- Publish updated status-list tokens and force rapid verifier refresh.

---

## Implementation status in sd-jwt-dotnet

The repository provides key components:

- `SdJwt.Net.OidFederation`: trust-chain resolution and entity-configuration modeling.
- `SdJwt.Net.StatusList`: issuance and verification primitives for token status lists.
- `SdJwt.Net.Vc` and `SdJwt.Net`: verifier pipelines that can consume status and trust results.

Scope note:

- Incident webhooks, SIEM connectors, and CDN publication workflows are application responsibilities.
- Method names and service contracts in this article are illustrative integration patterns, not package APIs.

---

## Application-layer orchestration pattern

```csharp
// Illustrative orchestration flow in your incident service.
public async Task HandleIssuerCompromiseAsync(string compromisedEntityId, CancellationToken ct)
{
    // 1) Record high-severity incident and freeze risky operations.
    await _incidentLog.RecordAsync(compromisedEntityId, "issuer_key_compromise", ct);

    // 2) Update federation trust state according to your policy model.
    await _trustPolicyStore.MarkEntityRevokedAsync(compromisedEntityId, ct);
    await _federationPublisher.PublishUpdatedMetadataAsync(ct);

    // 3) Resolve affected credential references and mark status entries.
    var affected = await _credentialIndex.FindByIssuerAsync(compromisedEntityId, ct);
    await _statusListWorkflow.ApplyRevocationsAsync(affected, ct);
    await _statusListWorkflow.PublishAsync(ct);

    // 4) Trigger verifier cache invalidation channel.
    await _cacheInvalidation.NotifyAsync(compromisedEntityId, ct);
}
```

---

## Verifier-side response requirements

For fast containment, verifiers should:

- Use short trust-metadata cache TTLs in high-assurance environments.
- Revalidate status entries during presentation verification.
- Support explicit invalidation events for major incidents.
- Fail closed when trust chain or status verification is inconclusive.

---

## Control checklist

- Automated runbook tested with red-team scenarios.
- Entity-level blast-radius mapping (issuer -> credential set).
- Status-list publication SLO and verifier refresh SLO.
- Immutable incident evidence: timestamps, policy decisions, affected scopes.
- Recovery playbook for key rollover and controlled trust restoration.

---

## Public references (URLs)

- OpenID Federation 1.0: <https://openid.net/specs/openid-federation-1_0.html>
- SD-JWT VC draft: <https://datatracker.ietf.org/doc/draft-ietf-oauth-sd-jwt-vc/>
- OAuth Token Status List draft: <https://datatracker.ietf.org/doc/draft-ietf-oauth-status-list/>
- RFC 9901 (SD-JWT): <https://datatracker.ietf.org/doc/rfc9901/>

Disclaimer: This article is informational and not incident-response legal guidance. Validate controls with security, legal, and compliance teams.
