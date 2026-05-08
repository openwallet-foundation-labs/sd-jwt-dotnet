# Tutorial 08: W3C Verifiable Credentials Data Model 2.0

**Package:** `SdJwt.Net.VcDm`  
**Time:** ~15 minutes  
**Prerequisites:** Tutorial 01 (Verifiable Credentials / SD-JWT VC)

---

## Learning objectives

- Understand why VCDM 2.0 and SD-JWT VC are **different specs** with different data models
- Build `VerifiableCredential` and `VerifiablePresentation` typed models
- Use `BitstringStatusListEntry` for revocation (the VCDM 2.0 replacement for StatusList2021Entry)
- Add `credentialSchema`, `termsOfUse`, and `evidence` optional fields
- Read VCDM 1.1 backward-compatible properties (`issuanceDate`, `expirationDate`)
- Validate credential structure with `VcDmValidator`
- Understand where VCDM 2.0 fits in the OID4VCI / OID4VP format map

---

## The format map

Before writing any code, understand which package handles which OID4VCI credential format:

| OID4VCI Format | Package              | Spec                          | Data Model                     |
| -------------- | -------------------- | ----------------------------- | ------------------------------ |
| `dc+sd-jwt`    | `SdJwt.Net.Vc`       | IETF SD-JWT VC draft-15       | No JSON-LD; `vct` claim        |
| `mso_mdoc`     | `SdJwt.Net.Mdoc`     | ISO 18013-5                   | CBOR                           |
| `jwt_vc_json`  | **`SdJwt.Net.VcDm`** | W3C VCDM 2.0 + JWT            | JSON-LD; `@context` + `type[]` |
| `ldp_vc`       | **`SdJwt.Net.VcDm`** | W3C VCDM 2.0 + Data Integrity | JSON-LD with embedded `proof`  |

`SdJwt.Net.Vc` explicitly states it does **not** implement W3C VCDM. The IETF SD-JWT VC spec uses "Verifiable **Digital** Credential" to distinguish itself — the `dc` in `dc+sd-jwt` stands for "Digital Credential", not "Data Model Compliant". These are parallel, independent specifications.

---

## Step 1 — Install the package

```xml
<PackageReference Include="SdJwt.Net.VcDm" Version="1.0.0" />
```

No external dependencies — `SdJwt.Net.VcDm` uses only `System.Text.Json` (built into .NET 6+).

---

## Step 2 — Build a VerifiableCredential

```csharp
using SdJwt.Net.VcDm.Models;
using SdJwt.Net.VcDm.Serialization;
using System.Text.Json;

var credential = new VerifiableCredential
{
    // @context: first entry MUST be the VCDM 2.0 URL
    Context = [VcDmContexts.V2, "https://schema.org/"],

    // type: MUST contain "VerifiableCredential"
    Type = ["VerifiableCredential", "UniversityDegreeCredential"],

    Id = "https://example.edu/credentials/3732",

    // Issuer: plain URL string or { id, name, description } object
    Issuer = new Issuer("https://example.edu/issuers/14") { Name = "Example University" },

    // validFrom replaces VCDM 1.1 issuanceDate
    ValidFrom  = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero),
    ValidUntil = new DateTimeOffset(2028, 1, 1, 0, 0, 0, TimeSpan.Zero),

    CredentialSubject =
    [
        new CredentialSubject
        {
            Id = "did:example:ebfeb1f712ebc6f1c276e12ec21",
            AdditionalClaims = new Dictionary<string, object>
            {
                ["degree"] = new { type = "BachelorDegree", name = "Bachelor of Science" },
                ["alumniOf"] = "Example University"
            }
        }
    ]
};
```

**Key VCDM 2.0 vs VCDM 1.1 changes:**

| VCDM 1.1 (deprecated)                    | VCDM 2.0                               |
| ---------------------------------------- | -------------------------------------- |
| `issuanceDate` (mandatory, string)       | `validFrom` (optional, ISO 8601)       |
| `expirationDate` (optional, string)      | `validUntil` (optional, ISO 8601)      |
| `https://www.w3.org/2018/credentials/v1` | `https://www.w3.org/ns/credentials/v2` |
| `StatusList2021Entry`                    | `BitstringStatusListEntry`             |

---

## Step 3 — Add credentialStatus

```csharp
credential.CredentialStatus =
[
    new BitstringStatusListEntry
    {
        Id = "https://example.edu/status/3#94567",
        StatusPurpose = "revocation",   // or "suspension"
        StatusListIndex = "94567",
        StatusListCredential = "https://example.edu/status/3"
    }
];
```

The verifier fetches `statusListCredential`, decodes the bitstring, and checks the bit at `statusListIndex`. `SdJwt.Net.StatusList` handles this for the IETF status list format (used with `dc+sd-jwt`). For VCDM 2.0 credentials, the same bitstring approach applies with `BitstringStatusListEntry`.

---

## Step 4 — Add optional properties

```csharp
// Schema reference for structural validation
credential.CredentialSchema =
[
    new CredentialSchema
    {
        Id   = "https://example.edu/schemas/degree.json",
        Type = "JsonSchema"
    }
];

// Usage restrictions
credential.TermsOfUse =
[
    new TermsOfUse { Type = "TrustFrameworkPolicy", Id = "https://policy.example.com/edu/v1" }
];

// How the issuer verified the subject's claims
credential.Evidence =
[
    new Evidence
    {
        Id   = "https://example.edu/evidence/001",
        Type = ["DocumentVerification"],
        AdditionalProperties = new Dictionary<string, object>
        {
            ["verifier"]         = "https://example.edu/registrar",
            ["evidenceDocument"] = "DegreeApplication",
            ["subjectPresence"]  = "Physical"
        }
    }
];
```

---

## Step 5 — Validate

```csharp
using SdJwt.Net.VcDm.Validation;

var validator = new VcDmValidator();
var result    = validator.Validate(credential);

if (!result.IsValid)
    throw new Exception(string.Join("; ", result.Errors));
```

`VcDmValidator` checks:

- `@context[0]` is the VCDM 2.0 URL (or VCDM 1.1 for backward compat)
- `type` contains `"VerifiableCredential"`
- `issuer` is present
- `credentialSubject` is non-empty
- Each `credentialStatus` entry has a `type`
- Each `credentialSchema` entry has `id` and `type`
- Each `termsOfUse` entry has a `type`
- Each `evidence` entry has a non-empty `type` array
- `validFrom` ≤ `validUntil` when both are present

---

## Step 6 — Serialize / deserialize

```csharp
// Serialize
var json = JsonSerializer.Serialize(credential, VcDmSerializerOptions.Default);

// Deserialize
var restored = JsonSerializer.Deserialize<VerifiableCredential>(json, VcDmSerializerOptions.Default);
```

`VcDmSerializerOptions.Default` registers all converters:

- `IssuerJsonConverter` — `"https://example.com"` string ↔ `{ "id": "...", "name": "..." }` object
- `CredentialStatusConverter` — type-discriminated deserialization for status subtypes
- `SingleOrArrayConverter<T>` — normalises VCDM single-object-or-array properties to arrays
- `Iso8601DateTimeOffsetConverter` — ISO 8601 string ↔ `DateTimeOffset`

---

## Step 7 — Read a VCDM 1.1 credential

```csharp
var vcdm11Json = """
    {
      "@context": ["https://www.w3.org/2018/credentials/v1"],
      "type": ["VerifiableCredential"],
      "issuer": "https://example.edu",
      "issuanceDate": "2023-01-01T00:00:00Z",
      "expirationDate": "2027-01-01T00:00:00Z",
      "credentialSubject": { "id": "did:example:alice" }
    }
    """;

var legacy = JsonSerializer.Deserialize<VerifiableCredential>(vcdm11Json, VcDmSerializerOptions.Default);

// issuanceDate → ValidFrom, expirationDate → ValidUntil
Console.WriteLine(legacy!.ValidFrom);   // 2023-01-01
Console.WriteLine(legacy.ValidUntil);  // 2027-01-01
```

The library reads `issuanceDate` and `expirationDate` silently and maps them to `ValidFrom` / `ValidUntil`. It never _writes_ them — new credentials always use `validFrom` / `validUntil`.

---

## Step 8 — Build a VerifiablePresentation (ldp_vc flow)

For `ldp_vc` presentations in OID4VP, the VP contains:

- The credential as a JSON object (not a JWT string)
- An embedded `DataIntegrityProof` that binds the presentation to the verifier

```csharp
var presentation = new VerifiablePresentation
{
    Context = [VcDmContexts.V2],
    Type    = ["VerifiablePresentation"],
    VerifiableCredential = [ /* serialized ldp_vc credential object */ ],
    Proof =
    [
        new DataIntegrityProof
        {
            Cryptosuite       = "ecdsa-rdfc-2019",
            ProofPurpose      = "authentication",
            VerificationMethod = "did:example:alice#key-1",
            Challenge         = "nonce-from-oid4vp-request",  // MUST match request nonce
            Domain            = "https://verifier.example.com", // MUST match client_id
            ProofValue        = "z3FXQjecWufY46yg..."
        }
    ]
};
```

> **Note:** `SdJwt.Net.VcDm` models the `proof` structure but does **not** generate or verify Data Integrity proofs. Cryptographic suites (`ecdsa-rdfc-2019`, `bbs-2023`) require a separate library that implements JSON-LD canonicalization (RDFC-2019).

---

## OID4VP VP token encoding

For `ldp_vc` in OID4VP, the VP Token is:

```json
{
  "my_credential_id": ["<JSON-serialized-VP-as-string>"]
}
```

Where `my_credential_id` is the DCQL credential query `id` — **not** the format name.

For `jwt_vc_json`, the credential is wrapped in a JWT before being placed in the VP Token:

```json
{
  "my_credential_id": ["eyJhbGci...JWT-wrapped-VC..."]
}
```

---

## What this package does not do

| Feature                           | Status       | Alternative                       |
| --------------------------------- | ------------ | --------------------------------- |
| JSON-LD `@context` expansion      | Not included | `json-ld.net`                     |
| Data Integrity proof generation   | Not included | `ecdsa-rdfc-2019` suite library   |
| Data Integrity proof verification | Not included | Requires JSON-LD canonicalization |
| JWT signing (`jwt_vc_json`)       | Not included | `System.IdentityModel.Tokens.Jwt` |
| JWT verification                  | Not included | `System.IdentityModel.Tokens.Jwt` |

---

## Next steps

- **Tutorial 03** — OID4VCI credential issuance (covers how `jwt_vc_json` and `ldp_vc` are issued via the credential endpoint)
- **Tutorial 04** — OID4VP presentation (covers how VCDM credentials are presented via DCQL)
- **Concept doc** — [W3C VCDM 2.0 Deep Dive](../../concepts/w3c-vcdm-deep-dive.md)
