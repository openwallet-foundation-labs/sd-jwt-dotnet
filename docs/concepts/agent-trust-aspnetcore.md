# ASP.NET Core Inbound Verification

> **Level:** Advanced preview extension

## Simple explanation

`SdJwt.Net.AgentTrust.AspNetCore` is middleware that verifies capability tokens on inbound HTTP requests. It sits in the ASP.NET Core pipeline and rejects requests that lack a valid, unexpired, correctly-scoped capability token.

## In one sentence

The middleware extracts an SD-JWT capability token from the `Authorization` header, verifies it, and makes the verified capability available to the endpoint handler.

## What you will learn

- How inbound verification works in the ASP.NET Core pipeline
- What the middleware checks (signature, audience, expiry, replay, capability scope)
- How to configure trusted issuers and audience binding
- How to access the verified capability in endpoint handlers

## Where it fits

```
Agent --> [HTTP + Bearer token] --> ASP.NET Core Pipeline
                                        |
                                   AgentTrust Middleware
                                        |
                                  Verify SD-JWT signature
                                  Check aud, exp, jti
                                  Evaluate inbound policy
                                        |
                                   401/403 or pass
                                        |
                                   Endpoint Handler
                                   (has verified capability)
```

## What the middleware checks

| Step | Check                                                    | Failure response |
| ---- | -------------------------------------------------------- | ---------------- |
| 1    | `Authorization: Bearer` header present                   | 401 Unauthorized |
| 2    | SD-JWT signature valid against trusted issuer keys       | 401 Unauthorized |
| 3    | `aud` matches this service's configured audience         | 403 Forbidden    |
| 4    | `exp` is still in the future (with clock skew tolerance) | 401 Unauthorized |
| 5    | `jti` not already used (nonce store lookup)              | 403 Forbidden    |
| 6    | Inbound policy evaluation (if configured)                | 403 Forbidden    |
| 7    | Record `jti` in nonce store                              | (internal)       |
| 8    | Emit audit receipt                                       | (internal)       |

After verification, the middleware populates `HttpContext.Items` with the verified capability claims. Endpoint handlers can access `cap.tool`, `cap.action`, `cap.limits`, and `ctx` correlation metadata.

|                      |                                                                                                                                                                               |
| -------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Audience**         | ASP.NET Core developers adding capability-token verification to HTTP APIs.                                                                                                    |
| **Purpose**          | Explain the inbound verification middleware pipeline: what it checks, how it integrates with standard authentication, and how endpoint handlers access verified capabilities. |
| **Scope**            | Middleware configuration, 8-step check table, relationship to `[Authorize]`. Out of scope: MCP interceptors (see [MCP Trust Interceptor](agent-trust-mcp.md)).                |
| **Success criteria** | Reader can add `UseAgentTrustVerification()` to the ASP.NET Core pipeline and access verified capability claims in endpoint handlers.                                         |

## Relationship to standard ASP.NET Core authentication

This middleware does not replace `[Authorize]` or ASP.NET Core Identity. It adds an additional verification layer:

| Layer                   | What it does                     | Standard                             |
| ----------------------- | -------------------------------- | ------------------------------------ |
| Transport               | TLS/mTLS                         | ASP.NET Core Kestrel                 |
| Authentication          | Proves caller identity           | OAuth/OIDC via `AddAuthentication()` |
| Authorization (coarse)  | Checks roles/scopes              | `[Authorize(Roles=...)]`             |
| Capability verification | Verifies per-action SD-JWT token | Agent Trust middleware               |

Use standard authentication to know who is calling. Use capability verification to know what this specific call is authorized to do.

## Related concepts

- [Agent Trust Kits](agent-trust-kits.md) - package overview and architecture
- [Agent Trust Profile](agent-trust-profile.md) - capability token model and threat model
- [MCP Trust Interceptor](agent-trust-mcp.md) - MCP-specific trust interceptor
- [Agent Trust Operations](agent-trust-ops.md) - deployment modes and operational guidance
