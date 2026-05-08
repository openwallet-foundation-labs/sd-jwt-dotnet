# Advanced Tutorials

Master enterprise-grade credential systems with trust infrastructure and high-security requirements.

## Prerequisites

You should be comfortable with:

- SD-JWT issuance and verification
- OpenID4VCI and OpenID4VP flows
- Presentation Exchange definitions

## Learning Path

| Tutorial | Topic                  | Time   | Key Concepts                              |
| -------- | ---------------------- | ------ | ----------------------------------------- |
| 01       | OpenID Federation      | 20 min | Trust chains, entity statements, metadata |
| 02       | HAIP Compliance        | 20 min | HAIP Final flows and credential profiles  |
| 03       | Multi-Credential Flows | 20 min | Combined workflows, batch operations      |
| 04       | Key Rotation           | 15 min | Key lifecycle, migration patterns         |

## Package Coverage

| Package                 | Tutorial        |
| ----------------------- | --------------- |
| SdJwt.Net.OidFederation | Tutorial 01     |
| SdJwt.Net.HAIP          | Tutorial 02     |
| All packages            | Tutorial 03, 04 |

## Key Concepts

### OpenID Federation

Trust is established through cryptographic chains rather than pre-shared lists:

```
Trust Anchor
    |
    v
Intermediate Authority
    |
    v
Credential Issuer
```

Each entity publishes signed metadata at `/.well-known/openid-federation`.

### HAIP (High Assurance Interoperability Profile)

OpenID4VC HAIP 1.0 Final is flow/profile based:

| Area                  | Examples                                                                                     |
| --------------------- | -------------------------------------------------------------------------------------------- |
| Flows                 | OID4VCI issuance, OID4VP redirect, W3C Digital Credentials API                               |
| Credential profiles   | SD-JWT VC with `dc+sd-jwt`, ISO mdoc with `mso_mdoc`                                         |
| Required capabilities | ES256 validation, SHA-256 digests, DCQL, DPoP, attestation, status lists, x5c/x5chain policy |

### Multi-Credential Patterns

Real applications often combine multiple credentials:

- Identity verification + Address proof
- Degree + Employment history
- Age verification + Membership status

## Running Tutorials

```bash
# From solution root
dotnet run --project samples/SdJwt.Net.Samples -- advanced

# Or run specific tutorial
dotnet run --project samples/SdJwt.Net.Samples -- advanced federation
```
