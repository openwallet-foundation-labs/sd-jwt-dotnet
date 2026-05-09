# Examples

Practical integration examples that combine multiple SD-JWT .NET packages in realistic flows.

## Start here

| Goal                                 | Example                                                                      |
| ------------------------------------ | ---------------------------------------------------------------------------- |
| Issue, hold, and verify a credential | [Issuer - Wallet - Verifier](credential-lifecycle/issuer-wallet-verifier.md) |
| Add revocation to credentials        | [Status List Lifecycle](credential-lifecycle/status-list-lifecycle.md)       |
| See Agent Trust in action            | [MCP Tool Governance Demo](agent-trust/mcp-tool-governance-demo.md)          |

## Maturity model

| Label         | Meaning                                                       |
| ------------- | ------------------------------------------------------------- |
| Stable        | Based on published IETF/OpenID standards                      |
| Spec-tracking | Tracks an active draft; API may change with the specification |
| Preview       | Project-defined pattern, not an external standard             |

---

## Credential lifecycle (Stable)

End-to-end flows covering issuance, presentation, revocation, and trust.

- [Issuer - Wallet - Verifier](credential-lifecycle/issuer-wallet-verifier.md) -- OID4VCI issuance, SD-JWT VC selective disclosure, OID4VP presentation, PEX validation
- [Status List Lifecycle](credential-lifecycle/status-list-lifecycle.md) -- Issue with status reference, publish, verify, revoke, re-verify
- [Federated Trust Verification](credential-lifecycle/federated-trust-verification.md) -- Resolve trust chain for unknown issuer via OpenID Federation

## Browser (Spec-tracking)

Browser-based credential presentation using the W3C Digital Credentials API.

- [DC API + OID4VP Verifier](browser/dc-api-oid4vp-verifier.md) -- Backend builds DC API request, frontend calls `navigator.credentials.get()`, backend validates

## mdoc (Stable)

ISO 18013-5 mobile document verification.

- [mdoc Verifier](mdoc/mdoc-verifier.md) -- Parse DeviceResponse, verify MSO signature, validate SessionTranscript, extract namespaced claims

## Agent Trust (Preview)

Scoped agent/tool authorization using capability SD-JWTs. Agent Trust is a project-defined pattern; it is not an IETF, OpenID Foundation, MCP, or OWF standard.

- [MCP Tool Governance Demo](agent-trust/mcp-tool-governance-demo.md) -- Flagship runnable demo with scripted and LLM-powered clients
- [Agent Trust End-to-End](agent-trust/agent-trust-end-to-end.md) -- Minimal code example: mint, verify, enforce
- [Demo Scenarios](agent-trust/demo-scenarios.md) -- Scenario catalogue covering authorization, denial, and containment

---

## Related documentation

- [Getting Started](../getting-started/) -- Installation and setup
- [Concepts](../concepts/) -- Architecture and protocol deep dives
- [Guides](../guides/) -- Task-oriented how-to guides
- [Agent Trust PoC Design Rationale](../project/archive/agent-trust-poc-e2e.md) -- Historical design proposal
