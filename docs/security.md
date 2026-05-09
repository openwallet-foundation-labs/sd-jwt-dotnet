# Security Model

SD-JWT .NET is designed for verifiable credential and trust infrastructure deployments that need explicit verification, selective disclosure, and strong cryptographic defaults. This page summarizes security behavior and deployment guidance. See [MATURITY.md](../MATURITY.md) for package-level stability.

## Cryptographic Controls

| Control                  | Guidance                                                                  |
| ------------------------ | ------------------------------------------------------------------------- |
| Approved hash algorithms | SHA-256, SHA-384, SHA-512                                                 |
| Signing algorithms       | ECDSA P-256/384/521 and other supported algorithms allowed by the package |
| Blocked weak algorithms  | MD5 and SHA-1 are rejected by HAIP-oriented validation paths              |
| Constant-time operations | Security-sensitive comparisons should use constant-time APIs              |
| Entropy                  | Salts, nonces, and generated keys use cryptographic random generation     |

## Defensive Verification

The libraries provide verification patterns for:

- Signature integrity
- Key binding
- Audience and issuer checks
- Nonce and issued-at freshness
- Status-list validation where configured
- Replay-sensitive Agent Trust capability tokens
- Weak algorithm rejection in high-assurance paths

Applications still need to configure trusted issuers, key resolution, storage, and application-specific authorization.

## HAIP Profile Guidance

The `SdJwt.Net.HAIP` package provides validation helpers and profile-oriented checks for high-assurance SD-JWT VC deployments, including algorithm strength, proof-of-possession, wallet binding, and deployment-specific key-management requirements.

Some sample code uses project-defined assurance enum values for local policy demonstrations. These are not official HAIP certification levels.

## Privacy Properties

| Property              | Mechanism                                                               |
| --------------------- | ----------------------------------------------------------------------- |
| Selective disclosure  | Holders disclose only selected claims from an issued SD-JWT             |
| Context isolation     | Presentations can be audience-specific and nonce-bound                  |
| Correlation reduction | Holders can create different presentations for different verifier needs |
| Minimal disclosure    | Verifiers should request only claims required for the transaction       |

SD-JWT provides selective disclosure. It is not a general-purpose zero-knowledge proof system.

## Key Custody and Secretless Deployment

Production deployments should avoid long-lived signing secrets in application configuration. Common patterns include:

- Azure Key Vault or Managed HSM
- Managed Identity or Workload Identity for key access
- Cloud KMS equivalents
- Hardware-backed platform key stores
- HSM-backed signing for regulated deployments

## Agent Trust Preview

The `SdJwt.Net.AgentTrust.*` packages are Preview. They are suitable for evaluation, controlled pilots, and architecture exploration. They should complement existing TLS, OAuth/resource-server controls, API authorization, gateway policy, and MCP authorization guidance.

## Related Documentation

- [Agent Trust Profile](agent-trust/agent-trust-profile.md)
- [HAIP Deep Dive](concepts/haip-deep-dive.md)
- [Status List Deep Dive](concepts/status-list-deep-dive.md)
- [Package Maturity](../MATURITY.md)
