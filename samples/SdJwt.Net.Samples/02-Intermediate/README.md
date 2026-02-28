# Intermediate Tutorials

Build on your SD-JWT foundation with industry standards and protocols.

## Learning Path (60-90 minutes)

| #   | Tutorial                                             | What You Learn                 | Time   |
| --- | ---------------------------------------------------- | ------------------------------ | ------ |
| 01  | [VerifiableCredentials](01-VerifiableCredentials.cs) | SD-JWT VC format and types     | 15 min |
| 02  | [StatusList](02-StatusList.cs)                       | Credential revocation patterns | 15 min |
| 03  | [OpenId4Vci](03-OpenId4Vci.cs)                       | Credential issuance protocol   | 15 min |
| 04  | [OpenId4Vp](04-OpenId4Vp.cs)                         | Presentation protocol flows    | 15 min |
| 05  | [PresentationExchange](05-PresentationExchange.cs)   | DIF PEX credential selection   | 15 min |

## Prerequisites

- Complete [01-Beginner](../01-Beginner/) tutorials
- Understanding of selective disclosure and key binding

## Key Concepts

### SD-JWT VC (Verifiable Credentials)

Extends core SD-JWT with standardized credential structure:

```json
{
  "vct": "https://credentials.example.com/EmploymentCredential",
  "iss": "https://issuer.example.com",
  "iat": 1234567890,
  "cnf": { "jwk": { ... } },
  "_sd": [ ... ]
}
```

Key additions:

- `vct` (Verifiable Credential Type) - identifies credential schema
- Standardized claim names and structures
- Interoperability with other VC implementations

### Status Lists

Efficient credential lifecycle management:

- **Revocation**: Issuer can invalidate credentials
- **Suspension**: Temporary hold on credential validity
- **Bit-indexed lists**: Compact status storage (millions of credentials)

### OpenID4VCI (Credential Issuance)

Protocol for requesting and receiving credentials:

```
Wallet                         Issuer
  |                              |
  |-- Authorization Request ---->|
  |<-- Authorization Response ---|
  |                              |
  |-- Token Request ------------>|
  |<-- Access Token -------------|
  |                              |
  |-- Credential Request ------->|
  |<-- SD-JWT VC ----------------|
```

### OpenID4VP (Verifiable Presentations)

Protocol for presenting credentials to verifiers:

```
Wallet                         Verifier
  |                              |
  |<-- Authorization Request ----|  (with presentation_definition)
  |                              |
  |-- Authorization Response --->|  (with vp_token)
  |                              |
  |<-- Result -------------------|
```

### Presentation Exchange (DIF PEX)

Declarative format for specifying credential requirements:

```json
{
  "id": "employment-check",
  "input_descriptors": [
    {
      "id": "employment",
      "constraints": {
        "fields": [
          {
            "path": ["$.job_title"],
            "filter": { "type": "string" }
          }
        ]
      }
    }
  ]
}
```

## Package Mapping

| Tutorial              | Package                          | Specification              |
| --------------------- | -------------------------------- | -------------------------- |
| VerifiableCredentials | `SdJwt.Net.Vc`                   | SD-JWT VC draft-15         |
| StatusList            | `SdJwt.Net.StatusList`           | OAuth Status List draft-18 |
| OpenId4Vci            | `SdJwt.Net.Oid4Vci`              | OID4VCI 1.0 Final          |
| OpenId4Vp             | `SdJwt.Net.Oid4Vp`               | OID4VP 1.0 Final           |
| PresentationExchange  | `SdJwt.Net.PresentationExchange` | DIF PEX v2.1.1             |

## Running the Tutorials

```bash
cd samples/SdJwt.Net.Samples
dotnet run
```

Select option `2` (Intermediate Tutorials) from the menu.

## Next Steps

After completing these tutorials:

1. Move to [03-Advanced](../03-Advanced/) for federation and HAIP
2. Explore production patterns in [04-UseCases](../04-UseCases/)
3. Read the full specification documentation in `docs/concepts/`

## Related Documentation

- [Verifiable Credentials Concept](../../../docs/concepts/verifiable-credential-deep-dive.md)
- [Status Lists Concept](../../../docs/concepts/status-list-deep-dive.md)
- [OpenID4VCI Guide](../../../docs/guides/issuing-credentials.md)
- [OpenID4VP Guide](../../../docs/guides/verifying-presentations.md)
