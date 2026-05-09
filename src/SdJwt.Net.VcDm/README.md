# SdJwt.Net.VcDm

W3C Verifiable Credentials Data Model 2.0 typed models, validation, and serialization for .NET.

## Overview

This package provides a pure data-model layer for [W3C VC Data Model 2.0](https://www.w3.org/TR/vc-data-model-2.0/). It is **distinct from `SdJwt.Net.Vc`**, which implements the IETF SD-JWT VC specification (`draft-ietf-oauth-sd-jwt-vc`) — a parallel, non-normatively-aligned credential format that does not use JSON-LD or W3C VCDM semantics.

**This package handles:**

-   `jwt_vc_json` — W3C VCDM 2.0 credential payload wrapped in a JWT
-   `ldp_vc` — W3C VCDM 2.0 credential with embedded Data Integrity `proof`

**Use `SdJwt.Net.Vc` for:**

-   `dc+sd-jwt` — IETF SD-JWT VC (selective disclosure, no JSON-LD)

## Credential Format Map

| OID4VCI Format | Package              | Spec                          |
| -------------- | -------------------- | ----------------------------- |
| `dc+sd-jwt`    | `SdJwt.Net.Vc`       | draft-ietf-oauth-sd-jwt-vc    |
| `mso_mdoc`     | `SdJwt.Net.Mdoc`     | ISO 18013-5                   |
| `jwt_vc_json`  | **`SdJwt.Net.VcDm`** | W3C VCDM 2.0 + JOSE           |
| `ldp_vc`       | **`SdJwt.Net.VcDm`** | W3C VCDM 2.0 + Data Integrity |

## Quick Start

```csharp
using SdJwt.Net.VcDm.Models;
using SdJwt.Net.VcDm.Validation;

// Build a W3C Verifiable Credential
var credential = new VerifiableCredential
{
    Context = [VcDmContexts.V2, "https://schema.org/"],
    Type = ["VerifiableCredential", "UniversityDegreeCredential"],
    Id = "https://example.edu/credentials/3732",
    Issuer = new Issuer("https://example.edu/issuers/14"),
    ValidFrom = DateTimeOffset.UtcNow,
    CredentialSubject = new CredentialSubject
    {
        Id = "did:example:ebfeb1f712ebc6f1c276e12ec21",
        AdditionalClaims = new Dictionary<string, object>
        {
            ["degree"] = new { type = "BachelorDegree", name = "Bachelor of Science" }
        }
    }
};

// Validate the credential structure
var validator = new VcDmValidator();
var result = validator.Validate(credential);
if (!result.IsValid)
    Console.WriteLine(string.Join(", ", result.Errors));

// Serialize to JSON
var json = JsonSerializer.Serialize(credential, VcDmSerializerOptions.Default);
```

## Key Classes

| Class                    | Purpose                                                 |
| ------------------------ | ------------------------------------------------------- |
| `VerifiableCredential`   | W3C VCDM 2.0 credential document model                  |
| `VerifiablePresentation` | W3C VCDM 2.0 presentation document model                |
| `Issuer`                 | String URL or `{ id, name, description }` object        |
| `CredentialSubject`      | Subject with optional `id` + additional claims          |
| `CredentialStatus`       | Base class; `BitstringStatusListEntry` subtype          |
| `CredentialSchema`       | Schema reference (`id` + `type`)                        |
| `TermsOfUse`             | Usage restriction (`type` + optional properties)        |
| `Evidence`               | Evidence of claims (`id`, `type`, arbitrary properties) |
| `VcDmContexts`           | Well-known `@context` URL constants                     |
| `VcDmValidator`          | Structural validation (context, required fields)        |
| `VcDmSerializerOptions`  | Pre-configured `JsonSerializerOptions`                  |

## VCDM 1.1 Backward Compatibility

The library reads (but does not write) the deprecated VCDM 1.1 properties:

| VCDM 1.1 (deprecated)                    | VCDM 2.0 (canonical)                   |
| ---------------------------------------- | -------------------------------------- |
| `issuanceDate`                           | `validFrom`                            |
| `expirationDate`                         | `validUntil`                           |
| `https://www.w3.org/2018/credentials/v1` | `https://www.w3.org/ns/credentials/v2` |

## What This Package Does NOT Do

-   No JSON-LD `@context` expansion or RDF processing (requires `json-ld.net` or equivalent)
-   No Data Integrity proof generation or verification (requires `ecdsa-rdfc-2019` / `bbs-2023` suite)
-   No JWT signing or verification (use `System.IdentityModel.Tokens.Jwt`)

## License

Apache-2.0 — OpenWallet Foundation Labs
