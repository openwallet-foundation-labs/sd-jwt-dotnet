# Verified Advice Context (VAC) Implementation Guide

## Purpose

Engineer-facing guide to implement VAC with production controls using the SdJwt.Net ecosystem.

---

## 1) Define your production profile first

Before coding, publish a profile statement:

- supported OID4VCI features
- supported OID4VP features
- trust model baseline
- operating modes (guidance vs licensed advice path)
- unsupported features and explicit failure behavior

Use repository gap reports as initial baseline:

- `reports/openid4vc-suite-gap-analysis.md`
- `reports/sd-jwt-vc-draft-15-gap-analysis.md`

---

## 2) Build an intent taxonomy with mode gates

Define each intent with:

- mode (`guidance_only` or `licensed_workflow_required`)
- required claims
- forbidden claims
- freshness policy
- escalation behavior

Example (`vac-intents.yaml`):

```yaml
intents:
  super_contribution_guidance:
    mode: guidance_only
    required_claims:
      - concessional_contributions_ytd
      - concessional_cap_remaining
      - salary_band
      - employer_contribution_rate
      - insurance_premium_impact_flag
      - age_band
    forbidden_claims:
      - tfn
      - exact_dob
      - home_address
      - beneficiary_details
    freshness:
      max_age_hours: 24
      fail_closed: true
    escalation: none

  cover_change_recommendation:
    mode: licensed_workflow_required
    required_claims:
      - insurance_category
      - premium_band
      - age_band
    forbidden_claims:
      - tfn
      - home_address
    freshness:
      max_age_hours: 24
      fail_closed: true
    escalation: licensed_adviser_review
```

---

## 3) Build Presentation Exchange templates from policy

Policy should generate request templates, not vice versa.

Minimal descriptor example:

```json
{
  "id": "super_contribution_guidance",
  "input_descriptors": [
    {
      "id": "member-financial-context",
      "constraints": {
        "fields": [
          { "path": ["$.concessional_contributions_ytd"] },
          { "path": ["$.concessional_cap_remaining"] },
          { "path": ["$.salary_band"] },
          { "path": ["$.employer_contribution_rate"] },
          { "path": ["$.insurance_premium_impact_flag"] },
          { "path": ["$.age_band"] }
        ]
      }
    }
  ]
}
```

---

## 4) Implement protocol-realistic OID4VP endpoints

Do not model this as "gateway directly calls wallet" unless you are explicitly in embedded wallet SDK mode.

Recommended endpoint pattern:

1. `POST /guidance/session`
   - classify intent
   - create policy context
2. `GET /oid4vp/authorize?session_id=...`
   - generate request object / request URI
   - persist `state`, `nonce`, expiry
3. wallet deep link / redirect / QR handoff
4. `POST /oid4vp/callback`
   - receive `vp_token` and `state`
   - run verification pipeline
5. `POST /guidance/respond`
   - run deterministic calculations
   - call LLM narration on verified + fresh envelope

State/nonce persistence requirements:

- cryptographically random identifiers
- one-time use
- tight expiration
- bound to session and audience

---

## 5) Enforce verification pipeline order

Run checks in strict order and fail closed:

1. state/nonce replay checks
2. SD-JWT and KB verification
3. issuer trust resolution
4. status list checks
5. freshness checks (`as_of` vs max-age)
6. policy conformance checks (forbidden claims)
7. deterministic calculation
8. LLM narration

No bypass path should exist to step 8.

---

## 6) Add deterministic calculators

Use deterministic services for:

- contribution cap and carry-forward calculations
- insurance impact calculations
- projection scenario math

LLM outputs should be constrained to narrative layers:

- explanation
- caveats
- next action options

---

## 7) Define the model input contract

The model should receive:

- trace id
- intent and mode
- verified claims with `as_of` timestamps
- verification outcomes
- deterministic engine outputs

The model should not receive:

- raw credentials
- raw holder payloads
- forbidden or unnecessary claims

---

## 8) Testing strategy (minimum)

### Policy and disclosure tests

- forbidden claims must never appear in requests
- claim list must match intent contract exactly

### Security and protocol tests

- replay attack tests (state/nonce reuse)
- invalid KB-JWT rejection
- audience mismatch rejection
- stale data rejection by max-age policy

### Regression tests

- deterministic output consistency by fixture
- LLM narrative wrapper tests using mocked model outputs

---

## 9) Operational runbooks

Define and test:

1. issuer key compromise procedure
2. status endpoint outage behavior
3. trust resolver outage behavior
4. model outage fallback behavior
5. stale data remediation flow (refresh/re-issue)

Every runbook should map to an alert and measurable recovery objective.

---

## 10) Metrics and SLOs

Recommended production metrics:

- unverified model calls (target: 0)
- median disclosed claims per intent
- freshness check failure rate
- policy violation blocks per release
- verification latency p95
- incident containment time for trust/status events

---

## 11) C# endpoint skeleton (OID4VP callback-centric)

```csharp
[ApiController]
public sealed class Oid4VpController : ControllerBase
{
    [HttpGet("/oid4vp/authorize")]
    public IActionResult Authorize([FromQuery] string sessionId)
    {
        var session = _sessionStore.Get(sessionId);
        var authz = _oid4vpRequestFactory.CreateSignedRequestObject(session);
        _stateStore.Save(authz.State, authz.Nonce, sessionId, authz.ExpiresAtUtc);
        return Ok(new { request_uri = authz.RequestUri, expires_at = authz.ExpiresAtUtc });
    }

    [HttpPost("/oid4vp/callback")]
    public async Task<IActionResult> Callback([FromForm] string state, [FromForm] string vp_token, CancellationToken ct)
    {
        var stateRecord = _stateStore.Consume(state); // one-time use
        var verification = await _vpVerifier.VerifyAsync(vp_token, stateRecord.Nonce, ct);
        if (!verification.IsSuccessful) return Unauthorized();

        var policy = _policyStore.Get(stateRecord.Intent);
        var envelope = _envelopeBuilder.BuildVerifiedFreshEnvelope(verification, policy);
        await _guidanceQueue.EnqueueAsync(stateRecord.SessionId, envelope, ct);
        return Ok();
    }
}
```

---

## Related documents

- Case study: [verified-advice-context.md](verified-advice-context.md)
- Reference architecture: [verified-advice-reference-architecture.md](verified-advice-reference-architecture.md)
