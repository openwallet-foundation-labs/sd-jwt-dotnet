# W3C Verifiable Credentials Data Model 2.0

> **Level:** Intermediate credential format

## Simple explanation

W3C VCDM 2.0 is a different credential data model from SD-JWT VC. Both represent verifiable credentials, but they come from different standards bodies and use different formats.

`SdJwt.Net.VcDm` handles the W3C model (`jwt_vc_json`, `ldp_vc`). `SdJwt.Net.Vc` handles the IETF model (`dc+sd-jwt`). You may need both if your verifier accepts credentials from different ecosystems.

## What you will learn

- How W3C VCDM 2.0 differs from IETF SD-JWT VC
- The core data model: `@context`, `type`, `issuer`, `credentialSubject`
- Breaking changes from VCDM 1.1 to 2.0
- How `SdJwt.Net.VcDm` fits alongside `SdJwt.Net.Vc`

**Spec:** https://www.w3.org/TR/vc-data-model-2.0/  
**Package:** `SdJwt.Net.VcDm`  
**Related:** [Verifiable Credential](verifiable-credentials.md) . [OID4VCI](openid4vci.md)

---

## Overview

The W3C Verifiable Credentials Data Model 2.0 (VCDM 2.0) is a W3C Recommendation that defines the structure of **Verifiable Credentials** and **Verifiable Presentations** using JSON-LD semantics. It is **not** the same as the IETF SD-JWT VC specification (`draft-ietf-oauth-sd-jwt-vc`) used by `SdJwt.Net.Vc`.

---

## The Two-Spec Landscape

The credential ecosystem currently has two parallel, non-normatively-aligned specifications for JSON-based credentials:

| OID4VCI Format           | Specification                                                    | Details                                                                                                    | Package          |
| ------------------------ | ---------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------- | ---------------- |
| `dc+sd-jwt`              | IETF SD-JWT VC (`draft-ietf-oauth-sd-jwt-vc`)                    | `vct` claim (collision-resistant URI), no `@context`, no type array, selective disclosure via `_sd` arrays | `SdJwt.Net.Vc`   |
| `jwt_vc_json` / `ldp_vc` | W3C VCDM 2.0 payload in JWT envelope / with Data Integrity proof | `@context`, `type[]` (JSON-LD semantics), `issuer`, `credentialSubject`, `validFrom`/`validUntil`          | `SdJwt.Net.VcDm` |
| `mso_mdoc`               | ISO 18013-5 (CBOR)                                               | Mobile document format                                                                                     | `SdJwt.Net.Mdoc` |

The IETF spec renamed `vc+sd-jwt` → `dc+sd-jwt` in late 2024 specifically to avoid confusion with W3C's `vc` media type namespace. The "dc" stands for "Digital Credential".

---

## Core Data Model

### VerifiableCredential

```json
{
  "@context": ["https://www.w3.org/ns/credentials/v2", "https://schema.org/"],
  "type": ["VerifiableCredential", "UniversityDegreeCredential"],
  "id": "https://example.edu/credentials/3732",
  "issuer": {
    "id": "https://example.edu/issuers/14",
    "name": "Example University"
  },
  "validFrom": "2024-01-01T00:00:00Z",
  "validUntil": "2028-01-01T00:00:00Z",
  "credentialSubject": {
    "id": "did:example:ebfeb1f712ebc6f1c276e12ec21",
    "degree": {
      "type": "BachelorDegree",
      "name": "Bachelor of Science"
    }
  }
}
```

### Mandatory Properties

| Property            | Constraint                                                          |
| ------------------- | ------------------------------------------------------------------- |
| `@context`          | Array; first element MUST be `https://www.w3.org/ns/credentials/v2` |
| `type`              | Array; MUST contain `"VerifiableCredential"`                        |
| `issuer`            | URL string or object with `id`; REQUIRED                            |
| `credentialSubject` | Object or array; REQUIRED                                           |

### Optional Properties

| Property           | Purpose                                     | Type                   |
| ------------------ | ------------------------------------------- | ---------------------- |
| `id`               | Credential identifier URL                   | string                 |
| `name`             | Human-readable label                        | string or language map |
| `description`      | Short description                           | string or language map |
| `validFrom`        | Start of validity (replaces `issuanceDate`) | ISO 8601 string        |
| `validUntil`       | End of validity (replaces `expirationDate`) | ISO 8601 string        |
| `credentialStatus` | Revocation / suspension info                | object or array        |
| `credentialSchema` | Schema reference                            | object or array        |
| `termsOfUse`       | Usage restrictions                          | object or array        |
| `evidence`         | Claim verification evidence                 | object or array        |
| `refreshService`   | Credential renewal endpoint                 | object or array        |
| `renderMethod`     | Display/rendering hints                     | object or array        |
| `proof`            | Data Integrity proof (ldp_vc only)          | object or array        |

---

## VCDM 1.1 → 2.0 Breaking Changes

| Aspect           | VCDM 1.1                                 | VCDM 2.0                                | Migration            |
| ---------------- | ---------------------------------------- | --------------------------------------- | -------------------- |
| Base context URL | `https://www.w3.org/2018/credentials/v1` | `https://www.w3.org/ns/credentials/v2`  | Update `@context[0]` |
| Issuance date    | `issuanceDate` (mandatory, any string)   | `validFrom` (optional, ISO 8601)        | Rename property      |
| Expiry date      | `expirationDate` (optional, any string)  | `validUntil` (optional, ISO 8601)       | Rename property      |
| Status list type | `StatusList2021Entry`                    | `BitstringStatusListEntry`              | Update `type` string |
| `holder` in VP   | Sometimes serialized as a property       | Role concept only — not a JSON property | Remove from JSON     |

`SdJwt.Net.VcDm` reads (but never writes) `issuanceDate` and `expirationDate` — deserialized values are mapped to `ValidFrom` and `ValidUntil` automatically.

---

## Securing Mechanisms

VCDM 2.0 defines three co-equal securing families:

### jwt_vc_json — JWT Envelope

The credential document is wrapped in a JWT. The cryptographic proof is the JWT signature.

```
Header:  { "alg": "ES256", "kid": "key-1", "typ": "vc+jwt" }
Payload: { ...VerifiableCredential JSON... }
Signature: <ECDSA over header.payload>
```

**Supported by `SdJwt.Net.VcDm`:** provides the credential payload model.  
**JWT signing/verification:** use `System.IdentityModel.Tokens.Jwt`.

### ldp_vc — Data Integrity Proof

The proof is embedded directly in the credential document as a `proof` property:

```json
{
  ...credential...,
  "proof": {
    "type": "DataIntegrityProof",
    "cryptosuite": "ecdsa-rdfc-2019",
    "created": "2024-01-15T09:00:00Z",
    "verificationMethod": "https://example.edu/issuers/14#key-1",
    "proofPurpose": "assertionMethod",
    "proofValue": "z3FXQjecWufY46yg..."
  }
}
```

**Supported by `SdJwt.Net.VcDm`:** models the `DataIntegrityProof` structure.  
**Proof generation/verification:** requires a Data Integrity suite (JSON-LD canonicalization + ECDSA/BBS+), not included in this library.

Active cryptographic suites:

- `ecdsa-rdfc-2019` — ECDSA with RDF Dataset Canonicalization
- `ecdsa-sd-2023` — ECDSA with selective disclosure (comparable to SD-JWT but JSON-LD)
- `bbs-2023` — BBS+ signatures (unlinkable presentations)

---

## credentialStatus — BitstringStatusListEntry

```json
{
  "credentialStatus": {
    "id": "https://example.com/status/3#94567",
    "type": "BitstringStatusListEntry",
    "statusPurpose": "revocation",
    "statusListIndex": "94567",
    "statusListCredential": "https://example.com/status/3"
  }
}
```

The verifier:

1. Fetches the URL in `statusListCredential`
2. Decodes the base64url-compressed bitstring
3. Checks the bit at `statusListIndex`
4. Bit `1` = credential is revoked/suspended

`SdJwt.Net.StatusList` implements the **IETF** version of this pattern (used with `dc+sd-jwt`). The VCDM 2.0 bitstring status list is structurally identical; the difference is the context and type string.

---

## Issuer: String vs Object

VCDM 2.0 allows two forms for `issuer`:

```json
// Simple form
"issuer": "https://example.com/issuers/14"

// Rich form
"issuer": {
  "id": "https://example.com/issuers/14",
  "name": "Example University",
  "description": "A leading educational institution"
}
```

`IssuerJsonConverter` handles both forms transparently. `Issuer.IsSimpleUrl` returns `true` when only `id` is set — the converter then writes the compact string form.

---

## SingleOrArray Properties

Many VCDM 2.0 properties accept either a single object or an array of objects:

```json
// Single
"credentialSubject": { "id": "did:example:alice" }

// Array
"credentialSubject": [
  { "id": "did:example:alice" },
  { "id": "did:example:bob" }
]
```

`SdJwt.Net.VcDm` normalizes all such properties to arrays during deserialization. The affected properties are: `credentialSubject`, `credentialStatus`, `credentialSchema`, `termsOfUse`, `evidence`, `refreshService`, `proof`.

---

## OID4VP Integration

### VP Token structure for ldp_vc

```json
{
  "my_credential_id": ["<JSON-string-of-VP-document>"]
}
```

The key is the DCQL credential query `id` (not the format name). Each value is a base64url-encoded or JSON-serialized VP document. The VP's `proof.challenge` MUST equal the OID4VP request `nonce`; `proof.domain` MUST equal `client_id`.

### VP Token structure for jwt_vc_json

```json
{
  "my_credential_id": ["eyJhbGci...JWT-string..."]
}
```

The JWT payload IS the VerifiablePresentation document. The `nonce` claim in the JWT payload MUST equal the OID4VP request `nonce`; `aud` MUST equal `client_id`.

### DCQL meta for W3C formats

```json
{
  "id": "my_degree",
  "format": "jwt_vc_json",
  "meta": {
    "type_values": [
      [
        "https://www.w3.org/2018/credentials#VerifiableCredential",
        "https://example.org/examples#UniversityDegreeCredential"
      ]
    ]
  }
}
```

`type_values` contains arrays of expanded (full IRI) type URIs, not the compact `type` strings.

---

## What SdJwt.Net.VcDm Does NOT Do

| Feature                           | Why excluded                                  | Alternative                       |
| --------------------------------- | --------------------------------------------- | --------------------------------- |
| JSON-LD `@context` expansion      | Requires RDF processing, large dependency     | `json-ld.net`                     |
| Data Integrity proof signing      | Requires `ecdsa-rdfc-2019` / BBS+ suite       | Separate DI suite library         |
| Data Integrity proof verification | Requires JSON-LD canonicalization (RDFC-2019) | Separate DI suite library         |
| JWT signing for `jwt_vc_json`     | Outside scope; credential payload only        | `System.IdentityModel.Tokens.Jwt` |
| Linked Data Signatures (obsolete) | Replaced by Data Integrity proofs             | Not recommended                   |

---

## Relationship to Other Packages

```
SdJwt.Net.VcDm          ← W3C VCDM 2.0 data model (this package)
SdJwt.Net.Vc            ← IETF SD-JWT VC (dc+sd-jwt) — DIFFERENT spec
SdJwt.Net.Mdoc          ← ISO 18013-5 mdoc
SdJwt.Net.Oid4Vci       ← Protocol layer (uses VcDm for jwt_vc_json / ldp_vc format configs)
SdJwt.Net.Oid4Vp        ← Protocol layer (DCQL meta for W3C formats via W3cVcMeta)
SdJwt.Net.StatusList    ← IETF status list (parallel to VCDM BitstringStatusList)
```

`SdJwt.Net.Oid4Vci` references `SdJwt.Net.VcDm` for the typed `JwtVcJsonCredentialConfiguration` and `LdpVcCredentialConfiguration` format profiles in the issuer metadata model.

---

## Further Reading

- [W3C VC Data Model 2.0](https://www.w3.org/TR/vc-data-model-2.0/)
- [W3C Data Integrity](https://www.w3.org/TR/vc-data-integrity/)
- [Bitstring Status List](https://www.w3.org/TR/vc-bitstring-status-list/)
- [IETF SD-JWT VC](https://datatracker.ietf.org/doc/draft-ietf-oauth-sd-jwt-vc/) (different spec — used by `SdJwt.Net.Vc`)
- [Tutorial 08 — W3C VCDM 2.0](../tutorials/intermediate/08-w3c-vcdm.md)

## Related concepts

- [Verifiable Credentials (SD-JWT VC)](verifiable-credentials.md) - the IETF credential profile
- [OID4VCI](openid4vci.md) - issuance protocol supporting both VCDM and SD-JWT VC formats
- [Status List](status-list.md) - IETF approach to credential status (parallel to VCDM BitstringStatusList)
