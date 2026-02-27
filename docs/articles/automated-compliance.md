# Automated Compliance for Selective Disclosure: Policy-First Data Minimization with SD-JWT

## Executive summary

Static compliance rules are difficult to maintain in high-change domains. Teams eventually encode thousands of narrow conditions, then spend more time patching policy logic than delivering secure user journeys.

A better pattern is:

- Define explicit disclosure policy per transaction intent.
- Enforce that policy at presentation-request time.
- Optionally use AI to propose policy refinements, never to bypass hard controls.

With this model, SD-JWT and SD-JWT VC provide cryptographic verification for disclosed claims, while Presentation Exchange defines exactly what the verifier can request.

---

## The operational problem

Most privacy incidents in verifiable credential systems are not signature failures. They are over-request failures:

- Request templates ask for more claims than needed.
- Teams cannot explain why each claim was necessary.
- Audits reveal weak purpose binding between user intent and requested data.

This is a governance problem first, then a technical problem.

---

## Architecture pattern: policy-first disclosure gate

Use a disclosure gate in front of wallet requests.

1. A business flow declares intent (for example, age verification, benefits eligibility, account recovery).
2. The gate maps intent to an approved claim policy.
3. The gate generates a constrained Presentation Definition.
4. The holder presents only permitted claims via SD-JWT VC selective disclosure.
5. The verifier validates integrity, issuer trust, freshness, and status before processing.

This design keeps minimization deterministic and auditable.

---

## Where AI can help safely

AI can help with policy analysis and drafts. It should not be the final policy authority.

Safe AI roles:

- Suggesting policy diffs when regulations or procedures change.
- Flagging request templates that exceed policy.
- Classifying new intents to candidate policy families.

Unsafe AI roles:

- Dynamically approving extra claims outside policy.
- Replacing mandatory controls with model confidence scores.

Fail-closed rule: if policy cannot be resolved, block the request.

---

## Implementation status in sd-jwt-dotnet

Current packages provide core building blocks:

- `SdJwt.Net.PresentationExchange`: presentation-definition modeling and evaluation components.
- `SdJwt.Net.Oid4Vp`: OpenID4VP request and response models.
- `SdJwt.Net.Vc` and `SdJwt.Net`: selective-disclosure verification primitives.
- `SdJwt.Net.StatusList`: status validation integration for credential lifecycle controls.

Important scope note:

- The repository does not ship a built-in "AI compliance assessor" service.
- Any AI-assisted policy workflow should be implemented in your application layer around these packages.

---

## Example control flow (application layer)

```csharp
// Illustrative application-layer flow, not a framework hook.
public async Task<PresentationDefinition> BuildPolicyBoundRequestAsync(
    string intentCode,
    TransactionContext context,
    CancellationToken ct)
{
    // 1) Resolve policy from approved catalog.
    var policy = await _policyStore.GetPolicyAsync(intentCode, ct);
    if (policy is null)
    {
        throw new InvalidOperationException("No approved disclosure policy for intent.");
    }

    // 2) Optional: AI suggests tightenings only (never broadening).
    var suggestion = await _policyAdvisor.SuggestReductionsAsync(policy, context, ct);
    var effectivePolicy = policy.ApplyReductionOnlySuggestion(suggestion);

    // 3) Generate Presentation Definition from effective policy.
    return _presentationDefinitionFactory.Create(effectivePolicy);
}
```

---

## Controls and evidence

Minimum controls for production:

- Policy versioning per intent.
- Immutable evidence record: intent, policy version, requested claims, disclosed claims.
- Cryptographic verification result and trust-chain result.
- Operator override logging for exceptional paths.

This creates an auditable trail for "why this data was requested" and "what was actually disclosed."

---

## Public references (URLs)

- RFC 9901 (SD-JWT): <https://datatracker.ietf.org/doc/rfc9901/>
- SD-JWT VC draft: <https://datatracker.ietf.org/doc/draft-ietf-oauth-sd-jwt-vc/>
- OpenID4VP 1.0: <https://openid.net/specs/openid-4-verifiable-presentations-1_0.html>
- DIF Presentation Exchange v2.1.1: <https://identity.foundation/presentation-exchange/spec/v2.1.1/>
- GDPR Article 5 principles: <https://gdpr-info.eu/art-5-gdpr/>

Disclaimer: This article is informational and not legal advice. Validate obligations with legal and compliance teams.
