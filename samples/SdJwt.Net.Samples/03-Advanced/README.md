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
| 02       | HAIP Compliance        | 15 min | Security levels, algorithm restrictions   |
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

Three security levels with increasing requirements:

| Level | Key Sizes | Algorithms | Use Case              |
| ----- | --------- | ---------- | --------------------- |
| 1     | P-256     | ES256      | Standard applications |
| 2     | P-384     | ES384      | Government, financial |
| 3     | P-521     | ES512      | Maximum security      |

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
