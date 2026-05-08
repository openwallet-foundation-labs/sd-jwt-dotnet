# Phase 1: OID4VCI Formats & OID4VP — Research

**Researched:** 2026-05-08
**Domain:** OpenID for Verifiable Credential Issuance (OID4VCI 1.0 final) and OpenID for Verifiable Presentations (OID4VP 1.0 draft-30)
**Confidence:** HIGH — all claims sourced directly from the local spec files at `specs/openid-4-verifiable-credential-issuance-1_0-final.md` (draft-17, approved final text) and `specs/openid-4-verifiable-presentations-1_0.md` (draft-30)

---

## Summary

The project already ships functional OID4VCI and OID4VP libraries (`SdJwt.Net.Oid4Vci`, `SdJwt.Net.Oid4Vp`). However, the existing models have several gaps relative to the final specifications: the `CredentialRequest` still carries a top-level `format` + format-specific fields (pre-final approach) rather than the spec-mandated `credential_configuration_id` / `credential_identifier` routing; `CredentialResponse` uses `credential` (singular, old) instead of the spec's `credentials` array; error codes are partially out of date; and multi-format support (mso_mdoc, ldp_vc, jwt_vc_json) has thin or no typed model coverage. OID4VP's existing support is built around Presentation Exchange 2.x, but the final OID4VP 1.0 spec has migrated to DCQL (Digital Credentials Query Language) as its primary query language.

**Primary recommendation:** Align `CredentialRequest`, `CredentialResponse`, issuer-metadata models, and proof types with OID4VCI final section numbering. Add typed `CredentialFormatProfile` subclasses for all four format identifiers. In OID4VP, add first-class DCQL support alongside the existing PE path.

---

## Architectural Responsibility Map

| Capability                                                | Primary Tier             | Secondary Tier   | Rationale                                 |
| --------------------------------------------------------- | ------------------------ | ---------------- | ----------------------------------------- |
| Credential issuance protocol (token, credential endpoint) | API / Backend (Issuer)   | Wallet (Client)  | OAuth 2.0 resource server pattern         |
| Credential offer parsing                                  | Wallet (Client)          | —                | Wallet receives offers from QR/deep-link  |
| Issuer metadata (.well-known)                             | API / Backend (Issuer)   | Wallet reads it  | Discovery document served by issuer       |
| Proof-of-possession (key binding)                         | Wallet (Client)          | Issuer validates | Wallet builds JWT proofs, issuer checks   |
| Credential format serialisation                           | Library (shared)         | —                | Both issuer and wallet need format models |
| Presentation request                                      | API / Backend (Verifier) | —                | Verifier builds DCQL/PE request           |
| VP Token construction                                     | Wallet (Client)          | —                | Wallet builds presentations               |
| VP Token validation                                       | API / Backend (Verifier) | —                | Verifier validates holder binding         |
| Response encryption                                       | Wallet + Verifier        | —                | Both sides participate in JWE             |

---

## Standard Stack

### Core (already in use)

| Library                           | Version                                  | Purpose             | Why Standard                                              |
| --------------------------------- | ---------------------------------------- | ------------------- | --------------------------------------------------------- |
| `System.Text.Json`                | Built-in (.NET 6+)                       | JSON serialisation  | Used throughout; `JsonPropertyName` already on all models |
| `System.IdentityModel.Tokens.Jwt` | Current (Microsoft.IdentityModel.\* 7.x) | JWT parsing/signing | `ProofBuilder` already creates JWTs                       |
| `PeterO.Cbor` or `Dahomey.Cbor`   | varies                                   | CBOR for mso_mdoc   | Required for ISO 18013-5 IssuerSigned encoding            |

### Supporting

| Library                                 | Version | Purpose                   | When to Use                                       |
| --------------------------------------- | ------- | ------------------------- | ------------------------------------------------- |
| `Microsoft.IdentityModel.JsonWebTokens` | 7.x     | JWT validation            | Credential proof validation on issuer side        |
| `Jose-JWT`                              | 5.x     | JWE (response encryption) | When `credential_response_encryption` is required |

### Alternatives Considered

| Instead of                  | Could Use         | Tradeoff                                                      |
| --------------------------- | ----------------- | ------------------------------------------------------------- |
| `System.Text.Json` for CBOR | `Newtonsoft.Json` | CBOR is binary, JSON libs cannot encode it; CBOR lib required |
| `PeterO.Cbor`               | `Dahomey.Cbor`    | PeterO is more actively maintained for OID4VCI use cases      |

---

## OID4VCI Final Specification — Complete Protocol Detail

### 1. Protocol Flows

#### Authorization Code Flow

[CITED: specs/openid-4-verifiable-credential-issuance-1_0-final.md §4 "Authorization Code Flow"]

1. (Optional) Issuer sends a Credential Offer containing `credential_configuration_ids` + `grants.authorization_code.issuer_state`
2. Wallet fetches `/.well-known/openid-credential-issuer` metadata
3. Wallet sends Authorization Request to `authorization_endpoint` with `authorization_details` (type `openid_credential`, `credential_configuration_id`) or `scope`; RECOMMENDED to use PKCE and PAR
4. Authorization Server returns `code`
5. Wallet sends Token Request (`grant_type=authorization_code`, `code`, `code_verifier`)
6. AS returns `access_token` + optional `authorization_details` containing `credential_identifiers`
7. Wallet sends Credential Request (`credential_identifier` OR `credential_configuration_id`, `proofs`)
8. Issuer returns Credential Response (`credentials` array) — or `transaction_id` for deferred

#### Pre-Authorized Code Flow

[CITED: specs/openid-4-verifiable-credential-issuance-1_0-final.md §5 "Pre-Authorized Code Flow"]

1. Issuer sends Credential Offer with `grants["urn:ietf:params:oauth:grant-type:pre-authorized_code"].pre-authorized_code` and optional `tx_code`
2. Wallet fetches issuer metadata
3. Wallet (after user interaction if `tx_code` present) sends Token Request: `grant_type=urn:ietf:params:oauth:grant-type:pre-authorized_code`, `pre-authorized_code`, optional `tx_code`
4. AS returns Access Token
5. Same as Auth Code steps 7-8

### 2. Credential Offer Parameters

[CITED: specs/openid-4-verifiable-credential-issuance-1_0-final.md §4.1.1]

```json
{
  "credential_issuer": "https://issuer.example.com",
  "credential_configuration_ids": [
    "UniversityDegree_JWT",
    "org.iso.18013.5.1.mDL"
  ],
  "grants": {
    "urn:ietf:params:oauth:grant-type:pre-authorized_code": {
      "pre-authorized_code": "oaKazRN8I0IbtZ0C7JuMn5",
      "tx_code": {
        "input_mode": "numeric",
        "length": 6,
        "description": "Enter the PIN from your SMS"
      }
    },
    "authorization_code": {
      "issuer_state": "eyJhbGci...",
      "authorization_server": "https://as.example.com"
    }
  }
}
```

**Delivery:** Either inline as `credential_offer=<url-encoded-json>` or by reference as `credential_offer_uri=<url>`.  
**Custom scheme:** `openid-credential-offer://?credential_offer=...`  
**Note:** `tx_code` object MAY be empty (just signals a code is required without specifying parameters).

### 3. Token Endpoint

[CITED: specs/openid-4-verifiable-credential-issuance-1_0-final.md §6]

**Token Request (Pre-Auth):**

```
POST /token
grant_type=urn:ietf:params:oauth:grant-type:pre-authorized_code
&pre-authorized_code=SplxlOBeZQQYbYS6WxSbIA
&tx_code=493536
```

**Token Request (Auth Code):**

```
POST /token
grant_type=authorization_code
&code=SplxlOBeZQQYbYS6WxSbIA
&code_verifier=dBjftJeZ4CVP-mB92K27uhbUJU1p1r_wW1gFWFOEjXk
&redirect_uri=https://wallet.example.org/cb
```

**Token Response (with authorization_details):**

```json
{
  "access_token": "eyJhbGci...",
  "token_type": "Bearer",
  "expires_in": 86400,
  "authorization_details": [
    {
      "type": "openid_credential",
      "credential_configuration_id": "UniversityDegreeCredential",
      "credential_identifiers": [
        "CivilEngineeringDegree-2023",
        "ElectricalEngineeringDegree-2023"
      ]
    }
  ]
}
```

**Token Error Codes (additions to RFC 6749):**

- `invalid_request` — tx_code provided but not expected, or expected but missing
- `invalid_grant` — wrong tx_code or expired pre-authorized_code
- `invalid_client` — Pre-Auth without client_id when AS requires it

### 4. Nonce Endpoint

[CITED: specs/openid-4-verifiable-credential-issuance-1_0-final.md §7]

**Request:** `POST /nonce` (no body, no auth required)  
**Response:**

```json
{ "c_nonce": "wKI4LT17ac15ES9bw8ac4" }
```

`Cache-Control: no-store` REQUIRED. If issuer has nonce endpoint, `proofs` MUST contain `c_nonce` in each JWT body as `nonce` claim.

### 5. Credential Endpoint

[CITED: specs/openid-4-verifiable-credential-issuance-1_0-final.md §8]

**Request (current spec — `credential_identifier` path):**

```json
POST /credential
Authorization: Bearer <token>
Content-Type: application/json

{
  "credential_identifier": "CivilEngineeringDegree-2023",
  "proofs": {
    "jwt": ["<jwt1>", "<jwt2>"]
  }
}
```

**Request (current spec — `credential_configuration_id` path):**

```json
{
  "credential_configuration_id": "org.iso.18013.5.1.mDL",
  "proofs": {
    "jwt": ["<jwt1>"]
  }
}
```

**IMPORTANT — Gap in existing code:**

- Existing `CredentialRequest` model has `format` + `vct`/`doctype` at the top level. The final spec routes via `credential_identifier` XOR `credential_configuration_id`. The `format` field is NOT present in a Credential Request in the final spec — it is resolved from the issuer metadata by the issuer.
- Existing `CredentialResponse` has a single `credential` field. The final spec uses a `credentials` array.

**Credential Response (immediate):**

```json
HTTP/1.1 200 OK
{
  "credentials": [
    { "credential": "LUpixVCWJk0eOt4CXQe1NXK....WZwmhmn9OQp6YxX0a2L" },
    { "credential": "YXNkZnNhZGZkamZqZGFza23....29tZTIzMjMyMzIzMjMy" }
  ],
  "notification_id": "3fwe98js"
}
```

**Credential Response (deferred — 202 Accepted):**

```json
HTTP/1.1 202 Accepted
{
  "transaction_id": "8xLOxBtZp8",
  "interval": 3600
}
```

**Credential Error Codes (final spec):**

- `invalid_credential_request` — malformed request (replaces old `invalid_request` in credential context)
- `unknown_credential_configuration` — requested config unknown (replaces old `unsupported_credential_type`)
- `unknown_credential_identifier` — identifier unknown
- `invalid_proof` — proof missing or invalid
- `invalid_nonce` — c_nonce invalid; wallet should refresh nonce
- `invalid_encryption_parameters` — encryption parameters missing/invalid
- `credential_request_denied` — unrecoverable; wallet MUST NOT retry

**Note:** Existing `Oid4VciConstants.CredentialErrorCodes` uses old names (`UnsupportedCredentialFormat`, `UnsupportedCredentialType`, `InvalidOrMissingProof`). These need to be updated/supplemented.

**Credential Response Encryption:**  
Client sends in request:

```json
{
  "credential_response_encryption": {
    "jwk": { "kty": "EC", "crv": "P-256", ... },
    "enc": "A128GCM",
    "zip": "DEF"
  }
}
```

Response is JWE with `Content-Type: application/jwt` when encryption is used.

### 6. Deferred Credential Endpoint

[CITED: specs/openid-4-verifiable-credential-issuance-1_0-final.md §9]

**Request:**

```json
POST /deferred_credential
Authorization: Bearer <token>
{ "transaction_id": "8xLOxBtZp8" }
```

**Response (ready):** Same as Credential Response `200 OK` with `credentials` array.  
**Response (still waiting):** `202 Accepted` with same `transaction_id` + new `interval`.  
**Additional error:** `invalid_transaction_id`

**Note:** Existing `CredentialResponse` has `[JsonIgnore] TransactionId`. The final spec uses `transaction_id` in the JSON body. `DeferredCredentialModels.cs` needs review.

### 7. Notification Endpoint

[CITED: specs/openid-4-verifiable-credential-issuance-1_0-final.md §10]

**Request:**

```json
POST /notification
Authorization: Bearer <token>
{
  "notification_id": "3fwe98js",
  "event": "credential_accepted",
  "event_description": "Stored successfully"
}
```

Events: `credential_accepted` | `credential_failure` | `credential_deleted`  
Response: `204 No Content` on success.  
Error codes: `invalid_notification_id`, `invalid_notification_request`

### 8. Issuer Metadata (/.well-known/openid-credential-issuer)

[CITED: specs/openid-4-verifiable-credential-issuance-1_0-final.md §11.2]

**Retrieval:** `GET https://issuer.example.com/.well-known/openid-credential-issuer`  
Inserted between host and path: `https://issuer.example.com/.well-known/openid-credential-issuer/tenant`

**Response:** `application/json` (unsigned) or `application/jwt` (signed, `typ: openidvci-issuer-metadata+jwt`)

**Top-level parameters:**

```json
{
  "credential_issuer": "https://issuer.example.com",
  "authorization_servers": ["https://as.example.com"],
  "credential_endpoint": "https://issuer.example.com/credential",
  "nonce_endpoint": "https://issuer.example.com/nonce",
  "deferred_credential_endpoint": "https://issuer.example.com/deferred",
  "notification_endpoint": "https://issuer.example.com/notification",
  "credential_request_encryption": {
    "jwks": { "keys": [...] },
    "enc_values_supported": ["A128GCM"],
    "encryption_required": false
  },
  "credential_response_encryption": {
    "alg_values_supported": ["ECDH-ES"],
    "enc_values_supported": ["A128GCM"],
    "encryption_required": false
  },
  "batch_credential_issuance": {
    "batch_size": 10
  },
  "display": [{ "name": "My Issuer", "locale": "en-US" }],
  "credential_configurations_supported": { ... }
}
```

**Per-credential-configuration object:**

```json
{
  "format": "dc+sd-jwt",
  "vct": "https://credentials.example.com/identity_credential",
  "scope": "IdentityCredential",
  "credential_signing_alg_values_supported": ["ES256"],
  "cryptographic_binding_methods_supported": ["jwk", "did:example"],
  "proof_types_supported": {
    "jwt": {
      "proof_signing_alg_values_supported": ["ES256"],
      "key_attestations_required": {
        "key_storage": ["iso_18045_moderate"]
      }
    }
  },
  "credential_metadata": {
    "display": [
      {
        "name": "Identity Credential",
        "locale": "en-US",
        "logo": { "uri": "https://...", "alt_text": "..." },
        "description": "...",
        "background_color": "#12107c",
        "text_color": "#FFFFFF"
      }
    ],
    "claims": [
      {
        "path": ["given_name"],
        "display": [{ "name": "Given Name", "locale": "en-US" }],
        "mandatory": true
      }
    ]
  }
}
```

### 9. Proof Types

[CITED: specs/openid-4-verifiable-credential-issuance-1_0-final.md §13]

Three proof types in `proofs` object:

#### `jwt` — Primary proof type

JOSE Header:

- `alg`: REQUIRED (not `none`, not MAC)
- `typ`: REQUIRED = `openid4vci-proof+jwt`
- `kid` XOR `jwk` XOR `x5c`: REQUIRED (one of these)
- `key_attestation`: OPTIONAL (key attestation JWT)
- `trust_chain`: OPTIONAL

JWT Body:

- `iss`: OPTIONAL = `client_id`
- `aud`: REQUIRED = Credential Issuer Identifier
- `iat`: REQUIRED = current time
- `nonce`: OPTIONAL but REQUIRED when issuer has nonce endpoint = `c_nonce`

#### `di_vp` — W3C Data Integrity VP

Value is a W3C Verifiable Presentation with DataIntegrityProof. The `challenge` = `c_nonce`, `domain` = issuer identifier.

#### `attestation` — Key attestation without possession proof

Value is exactly one Key Attestation JWT (`typ: key-attestation+jwt`).

**Note:** Existing code uses `proof` (singular) in `CredentialRequest`. The final spec uses `proofs` (object keyed by proof type, each value an array). The old `proof` field was from pre-final drafts. The existing `CredentialProofs` class covers the multi-proof case but `CredentialRequest.Proof` (singular) should be deprecated.

### 10. Batch Credential Issuance

[CITED: specs/openid-4-verifiable-credential-issuance-1_0-final.md §3.3]

- Single request with multiple entries in `proofs.jwt` array = batch request
- Each key gets at most one credential bound to it
- All credentials in one request share the same `credential_configuration_id` / `credential_identifier` (same format + dataset, different cryptographic material)
- Issuer may issue fewer credentials than keys provided
- `batch_credential_issuance.batch_size` in metadata specifies max `proofs` array size (MUST be >= 2)

---

## OID4VCI Credential Format Profiles

### Format 1: `dc+sd-jwt` (IETF SD-JWT VC)

[CITED: specs/openid-4-verifiable-credential-issuance-1_0-final.md §14.4]

**Format Identifier:** `dc+sd-jwt`

**Issuer Metadata — additional field:**

```json
{ "vct": "https://credentials.example.com/identity_credential" }
```

**Credential Request:** No format-specific fields in request body. Routing via `credential_configuration_id` / `credential_identifier`.

**Credential Response:** `credential` value = SD-JWT string (already base64url-delimited with `.`). MUST NOT be re-encoded.

**Legacy:** `vc+sd-jwt` still accepted by existing code for backwards compatibility — keep that.

### Format 2: `mso_mdoc` (ISO/IEC 18013-5 mdoc)

[CITED: specs/openid-4-verifiable-credential-issuance-1_0-final.md §14.3]

**Format Identifier:** `mso_mdoc`

**Issuer Metadata — additional field:**

```json
{ "doctype": "org.iso.18013.5.1.mDL" }
```

**Signing algorithm identifiers:** COSE numeric algorithm IDs (e.g., `-7` = ECDSA/SHA-256). May also use fully-specified algorithms per draft-ietf-jose-fully-specified-algorithms.

**Credential Response:** `credential` value = base64url-encoded CBOR-encoded `IssuerSigned` structure. This is binary data so MUST be base64url-encoded.

**Note:** Existing `CredentialRequest` has `DocType` and `Claims` fields which correspond to the pre-final format-in-request pattern. In the final spec these live in the issuer metadata only; the request just carries `credential_configuration_id`.

### Format 3: `jwt_vc_json` (W3C VC as JWT, no JSON-LD)

[CITED: specs/openid-4-verifiable-credential-issuance-1_0-final.md §14.1.1]

**Format Identifier:** `jwt_vc_json`

**Issuer Metadata — additional field:**

```json
{
  "credential_definition": {
    "type": ["VerifiableCredential", "UniversityDegreeCredential"]
  }
}
```

**Processing rule:** The credential offer, authorization details, credential request, and issuer metadata including `credential_definition` MUST NOT be processed using JSON-LD rules when `format = jwt_vc_json`.

**Credential Response:** `credential` value = JWT string. Already base64url-delimited; MUST NOT be re-encoded.

**Algorithm identifiers:** JWS Algorithm Names from IANA JOSE registry.

### Format 4: `ldp_vc` (W3C VC with Data Integrity, JSON-LD)

[CITED: specs/openid-4-verifiable-credential-issuance-1_0-final.md §14.1.2]

**Format Identifier:** `ldp_vc`

**Issuer Metadata — additional fields:**

```json
{
  "credential_definition": {
    "@context": [
      "https://www.w3.org/2018/credentials/v1",
      "https://example.org/examples/v1"
    ],
    "type": ["VerifiableCredential", "UniversityDegreeCredential"]
  }
}
```

**Credential Response:** `credential` value = JSON object. MUST NOT be re-encoded.

**Algorithm identifiers:** Data Integrity suite identifiers from Linked Data Cryptographic Suite Registry.

### Format 5: `jwt_vc_json-ld` (W3C VC as JWT, using JSON-LD)

[CITED: specs/openid-4-verifiable-credential-issuance-1_0-final.md §14.1.3]

Same metadata structure as `ldp_vc` (`@context` + `type` in `credential_definition`).  
Response encoding: same as `jwt_vc_json` (JWT string, no re-encoding).

---

## OID4VP Final Specification — Complete Protocol Detail

### 1. Protocol Flows

#### Same-Device Flow

[CITED: specs/openid-4-verifiable-presentations-1_0.md §3.1]

Verifier redirects Wallet to Authorization Endpoint. Wallet builds VP Token, returns via redirect (default `fragment` Response Mode).

#### Cross-Device Flow

[CITED: specs/openid-4-verifiable-presentations-1_0.md §3.2]

1. Verifier creates QR code containing Auth Request with `request_uri`
2. Wallet GETs (or POSTs if `request_uri_method=post`) the Request Object from `request_uri`
3. Wallet sends Authorization Response via HTTP POST to `response_uri` (Response Mode `direct_post`)

### 2. Authorization Request Parameters

[CITED: specs/openid-4-verifiable-presentations-1_0.md §4]

**Existing parameters (changes/requirements):**

- `client_id`: REQUIRED. May use Client Identifier Prefix: `redirect_uri:`, `openid_federation:`, `decentralized_identifier:`, `verifier_attestation:`, `x509_san_dns:`, `x509_hash:`, `origin:` (DC API only)
- `nonce`: REQUIRED. Fresh random string. MUST be in Key Binding JWT of SD-JWT VC response.
- `response_type`: `vp_token` or `vp_token id_token`
- `response_mode`: REQUIRED. `direct_post` | `direct_post.jwt` | `fragment` | `query`
- `state`: REQUIRED when requesting credentials without holder binding proof

**New parameters:**

- `dcql_query`: REQUIRED (or `scope`). JSON object — the DCQL query
- `client_metadata`: OPTIONAL. JSON with `jwks`, `vp_formats_supported`, `encrypted_response_enc_values_supported`
- `request_uri_method`: OPTIONAL. `get` (default) or `post`
- `transaction_data`: OPTIONAL. Array of base64url-encoded JSON transaction data objects
- `verifier_info`: OPTIONAL. Array of attestations about the Verifier
- `response_uri`: REQUIRED with `direct_post`. Target for HTTP POST response

**IMPORTANT GAP:** Existing OID4VP code still uses `presentation_definition` (Presentation Exchange). The final spec defines `dcql_query` as the primary mechanism. PE is no longer the spec-defined mechanism in OID4VP 1.0 final.

**Request Object (JAR):** `typ` MUST be `oauth-authz-req+jwt`. `aud` = `iss` for dynamic discovery or `https://self-issued.me/v2` for static.

### 3. DCQL — Digital Credentials Query Language

[CITED: specs/openid-4-verifiable-presentations-1_0.md §7]

```json
{
  "credentials": [
    {
      "id": "my_identity_credential",
      "format": "dc+sd-jwt",
      "meta": {
        "vct_values": ["https://credentials.example.com/identity_credential"]
      },
      "trusted_authorities": [
        { "type": "aki", "values": ["s9tIpPmhxdiuNkHMEWNpYim8S8Y"] }
      ],
      "require_cryptographic_holder_binding": true,
      "claims": [
        { "id": "last_name", "path": ["family_name"] },
        { "id": "given", "path": ["given_name"] }
      ],
      "claim_sets": [["last_name", "given"]]
    }
  ],
  "credential_sets": [
    {
      "options": [["my_identity_credential"]],
      "required": true
    }
  ]
}
```

**Credential Query fields:**

- `id`: REQUIRED. Alphanumeric + `_` + `-`. Unique per request.
- `format`: REQUIRED. Format identifier.
- `multiple`: OPTIONAL. Boolean, default `false`. If `true`, multiple presentations may match.
- `meta`: REQUIRED. Format-specific metadata constraints.
- `trusted_authorities`: OPTIONAL. Array of trusted authority constraints (`aki`, `etsi_tl`, `openid_federation`).
- `require_cryptographic_holder_binding`: OPTIONAL, default `true`.
- `claims`: OPTIONAL. Array of claims path queries.
- `claim_sets`: OPTIONAL. Alternative combinations of claims to satisfy (ordered by preference).

**Claims Path Pointer:** Non-empty array of `string | null | int`.

- String = key in JSON object / namespace in mdoc (first two elements for mdoc)
- Null = all elements of array
- Non-negative integer = array index

**Format-specific `meta` parameters:**

- `dc+sd-jwt`: `{ "vct_values": ["<type-url>"] }` — REQUIRED
- `mso_mdoc`: `{ "doctype_value": "org.iso.18013.5.1.mDL" }` — REQUIRED
- `jwt_vc_json` / `ldp_vc`: `{ "type_values": [["<expanded-iri>", ...], ...] }` — REQUIRED

### 4. VP Token Response

[CITED: specs/openid-4-verifiable-presentations-1_0.md §8]

`vp_token` = JSON-encoded object, keys are `id` values from DCQL, values are arrays of presentations.

```json
{
  "my_identity_credential": ["eyJhbGci...QMA"]
}
```

When `multiple: false` (default), array has exactly one element.

**Format-specific presentation encoding:**

- `dc+sd-jwt`: SD-JWT+KB string (if holder binding required). `nonce` in KB-JWT = request `nonce`; `aud` = `client_id`.
- `mso_mdoc`: base64url-encoded `DeviceResponse` CBOR. SessionTranscript uses `OpenID4VPHandover` CBOR structure.
- `jwt_vc_json`: JWT string. VP `nonce` = request `nonce`; VP `aud` = `client_id`.
- `ldp_vc`: JSON object. VP proof `challenge` = request `nonce`; `domain` = `client_id`.

### 5. Response Modes

[CITED: specs/openid-4-verifiable-presentations-1_0.md §8.2]

#### `direct_post`

- Wallet HTTP POSTs to `response_uri` with `application/x-www-form-urlencoded`
- Body: `vp_token=...&state=...`
- Verifier MUST return `200 OK` with JSON: `{ "redirect_uri": "..." }` (optional redirect)

#### `direct_post.jwt`

- Same flow but response is encrypted: body is `response=<JWE>`
- JWE payload = `{ "vp_token": {...} }`
- Encryption algorithm from `client_metadata.jwks` + `encrypted_response_enc_values_supported`
- Default `enc` algorithm = `A128GCM`

### 6. OID4VP Format-Specific vp_formats_supported

#### `dc+sd-jwt`

```json
{
  "sd-jwt_alg_values": ["ES256"],
  "kb-jwt_alg_values": ["ES256"]
}
```

#### `mso_mdoc`

```json
{
  "issuerauth_alg_values": [-7, -9],
  "deviceauth_alg_values": [-65537, -9]
}
```

#### `jwt_vc_json`

```json
{ "alg_values": ["ES256K", "ES384"] }
```

#### `ldp_vc`

```json
{
  "proof_type_values": ["DataIntegrityProof"],
  "cryptosuite_values": ["eddsa-2022", "ecdsa-2019"]
}
```

### 7. mdoc Handover (SessionTranscript)

[CITED: specs/openid-4-verifiable-presentations-1_0.md §10.3]

For redirect-based flows:

```cddl
OpenID4VPHandover = ["OpenID4VPHandover", sha256(OpenID4VPHandoverInfo)]
OpenID4VPHandoverInfo = [clientId, nonce, jwkThumbprint_or_null, redirectUri_or_responseUri]
```

For DC API flows:

```cddl
OpenID4VPDCAPIHandover = ["OpenID4VPDCAPIHandover", sha256(OpenID4VPDCAPIHandoverInfo)]
OpenID4VPDCAPIHandoverInfo = [origin, nonce, jwkThumbprint_or_null]
```

`jwkThumbprint`: SHA-256 JWK Thumbprint of Verifier's encryption key when using `direct_post.jwt`; otherwise `null`.

---

## Existing Code Gaps — Critical Delta Analysis

### OID4VCI Gaps

| Gap                                                  | Current Code                                               | Final Spec                                                                                        | Priority |
| ---------------------------------------------------- | ---------------------------------------------------------- | ------------------------------------------------------------------------------------------------- | -------- |
| `CredentialRequest.format` field                     | Present, required in request                               | NOT in request; resolved by issuer from metadata                                                  | HIGH     |
| `CredentialRequest.Vct` / `DocType` in request body  | Present as request fields                                  | Belong in issuer metadata only                                                                    | HIGH     |
| `CredentialResponse.credential` (singular)           | Single `credential` field                                  | `credentials` array of objects `{ "credential": ... }`                                            | HIGH     |
| `CredentialResponse.AcceptanceToken`                 | Maps to `acceptance_token` JSON                            | Final spec uses `transaction_id`                                                                  | HIGH     |
| `CredentialResponse.TransactionId` is `[JsonIgnore]` | Not serialised                                             | MUST be serialised as `transaction_id`                                                            | HIGH     |
| `CredentialErrorCodes` names                         | `UnsupportedCredentialFormat`, `UnsupportedCredentialType` | `unknown_credential_configuration`, `unknown_credential_identifier`, `invalid_credential_request` | MEDIUM   |
| Missing `invalid_nonce` error                        | Absent                                                     | Distinct code signalling nonce refresh needed                                                     | MEDIUM   |
| Proof types                                          | `proof` (singular) on request                              | `proofs` (object by type, array values) — spec removed `proof` singular                           | MEDIUM   |
| `c_nonce` in response                                | Present on `CredentialResponse`                            | Moved to Nonce Endpoint; no longer in Credential Response in final spec                           | MEDIUM   |
| `credential_configuration_id` routing                | Absent                                                     | Required alternative to `credential_identifier`                                                   | HIGH     |
| Missing Issuer Metadata model                        | No typed `CredentialIssuerMetadata` class                  | Required for well-known endpoint implementation                                                   | HIGH     |
| `batch_credential_issuance`                          | Not modelled                                               | Metadata + multi-proof array in `proofs`                                                          | MEDIUM   |

### OID4VP Gaps

| Gap                        | Current Code                           | Final Spec                                                                                                          | Priority |
| -------------------------- | -------------------------------------- | ------------------------------------------------------------------------------------------------------------------- | -------- |
| `presentation_definition`  | Primary mechanism                      | DCQL (`dcql_query`) is the primary mechanism                                                                        | HIGH     |
| DCQL model                 | `DcqlQuery.cs` et al exist but partial | Full `DcqlCredentialQuery` with `meta`, `trusted_authorities`, `claim_sets`, `require_cryptographic_holder_binding` | HIGH     |
| VP Token structure         | Not clear                              | `{ "<id>": ["<presentation>"] }` keyed by DCQL `id`                                                                 | HIGH     |
| `client_id` prefix parsing | `ClientIdSchemes` constants only       | Parser for `prefix:rest` pattern needed                                                                             | MEDIUM   |
| mdoc handover CBOR         | Exists in DcApi                        | Redirect-based `OpenID4VPHandover` also needed                                                                      | MEDIUM   |
| `verifier_info` parameter  | Absent                                 | New optional array parameter in auth request                                                                        | LOW      |
| `transaction_data`         | Absent                                 | New optional parameter, required rejection if unrecognized                                                          | MEDIUM   |

---

## Architecture Patterns

### Recommended Pattern: Format Profile Hierarchy (OID4VCI)

```
CredentialFormatProfile (abstract)
├── SdJwtVcFormatProfile        ("dc+sd-jwt")
│   └── Properties: Vct
├── MsoMdocFormatProfile        ("mso_mdoc")
│   └── Properties: Doctype, Claims (namespace structure)
├── JwtVcJsonFormatProfile      ("jwt_vc_json")
│   └── Properties: CredentialDefinition (Type array)
├── LdpVcFormatProfile          ("ldp_vc")
│   └── Properties: CredentialDefinition (@context + Type)
└── JwtVcJsonLdFormatProfile    ("jwt_vc_json-ld")
    └── (same as ldp_vc metadata)
```

Each profile:

- `string FormatIdentifier` (read-only)
- `CredentialConfiguration ToMetadata()` for issuer metadata
- `void ValidateRequest(CredentialRequest)` for issuer-side validation
- `object EncodeCredential(...)` for response encoding

### Credential Request Routing Pattern

```csharp
// Final spec routing logic
if (credentialIdentifiers != null) {
    // use credential_identifier from token response
} else {
    // use credential_configuration_id
}
// Never use format/vct/doctype in the request body
```

### DCQL Query Evaluation Pattern (OID4VP)

```
DcqlQuery
  -> For each CredentialQuery in .credentials:
      -> Match format against wallet credentials
      -> Apply .meta constraints (vct_values, doctype_value, type_values)
      -> Apply .trusted_authorities matching
      -> Apply .claims path selection
      -> Apply .claim_sets combination rules
  -> Apply .credential_sets required/optional logic
  -> Return matched credentials per query id
```

### Recommended Project Structure

```
src/SdJwt.Net.Oid4Vci/
├── Models/
│   ├── CredentialRequest.cs          (refactor: remove format/vct/doctype)
│   ├── CredentialResponse.cs         (refactor: credentials array)
│   ├── CredentialOffer.cs            (minor updates)
│   ├── IssuerMetadata.cs             (NEW: CredentialIssuerMetadata)
│   ├── CredentialConfiguration.cs    (NEW: per-config metadata object)
│   ├── Formats/
│   │   ├── SdJwtVcCredentialConfig.cs
│   │   ├── MsoMdocCredentialConfig.cs
│   │   ├── JwtVcJsonCredentialConfig.cs
│   │   └── LdpVcCredentialConfig.cs
│   ├── Proofs/
│   │   ├── JwtProof.cs
│   │   ├── DiVpProof.cs
│   │   └── AttestationProof.cs
│   └── Oid4VciConstants.cs           (update error codes)
│
src/SdJwt.Net.Oid4Vp/
├── Models/
│   ├── Dcql/
│   │   ├── DcqlQuery.cs
│   │   ├── DcqlCredentialQuery.cs    (add meta, trusted_authorities, claim_sets)
│   │   ├── DcqlClaimsQuery.cs
│   │   ├── DcqlCredentialSetQuery.cs
│   │   └── Formats/                  (NEW: format-specific meta classes)
│   │       ├── SdJwtVcMeta.cs
│   │       ├── MsoMdocMeta.cs
│   │       └── W3cVcMeta.cs
│   ├── AuthorizationRequest.cs       (add dcql_query, transaction_data, verifier_info)
│   └── AuthorizationResponse.cs      (VP Token as Dictionary<string, string[]>)
```

---

## Don't Hand-Roll

| Problem                                | Don't Build                        | Use Instead                                               | Why                                           |
| -------------------------------------- | ---------------------------------- | --------------------------------------------------------- | --------------------------------------------- |
| JWE encryption for credential response | Custom AES-GCM wrapper             | `Jose-JWT` or `Microsoft.IdentityModel`                   | JOSE RFC compliance, key agreement edge cases |
| CBOR encoding for mdoc                 | Manual byte encoding               | `PeterO.Cbor`                                             | CDDL type rules, indefinite-length arrays     |
| JWT proof validation                   | Custom signature check             | `System.IdentityModel.Tokens.Jwt`                         | Key ID resolution, algorithm agility          |
| SHA-256 JWK Thumbprint (RFC 7638)      | Manual JSON sort + hash            | `Microsoft.IdentityModel.Tokens.JsonWebKey` or `Jose-JWT` | Required members ordering per RFC             |
| Base64url encoding                     | `Convert.ToBase64String` + replace | `Microsoft.IdentityModel.Tokens.Base64UrlEncoder`         | Padding removal, URL-safe chars               |

---

## Common Pitfalls

### Pitfall 1: `credential` (singular) vs `credentials` (array) in Response

**What goes wrong:** Client parses `credential` field, gets null; issuer writes `credential` field, verifiers reject.
**Why it happens:** Pre-final spec drafts used `credential` singular. Final spec uses `credentials: [{"credential": ...}]` array.
**How to avoid:** Update `CredentialResponse` to `credentials` array. Keep `credential` as deprecated read-only shim for backward compat.

### Pitfall 2: `format` in Credential Request

**What goes wrong:** Wallet sends `format: "dc+sd-jwt"` in request body; spec-compliant issuer rejects or ignores it.
**Why it happens:** Pre-final drafts required `format` in the request. Final spec removes it — routing is via `credential_configuration_id` or `credential_identifier`.
**How to avoid:** Remove `format` from `CredentialRequest` (or mark as deprecated/ignored).

### Pitfall 3: `acceptance_token` vs `transaction_id`

**What goes wrong:** Deferred flow breaks; wallet sends `acceptance_token` in deferred request, issuer expects `transaction_id`.
**Why it happens:** Old pre-final drafts used `acceptance_token`. Final spec uses `transaction_id` in both the response and the deferred request.
**How to avoid:** Replace `acceptance_token` with `transaction_id` in both models.

### Pitfall 4: `c_nonce` in Credential Response

**What goes wrong:** Wallet expects `c_nonce` in Credential Response, gets null; uses stale nonce.
**Why it happens:** Pre-final spec included `c_nonce` in Credential Response. Final spec separates nonce distribution to the dedicated Nonce Endpoint.
**How to avoid:** Implement Nonce Endpoint. Keep backward-compat `c_nonce` read in Credential Response but don't rely on it.

### Pitfall 5: mdoc `IssuerSigned` vs `DeviceResponse` encoding

**What goes wrong:** Issuer returns raw CBOR bytes as string; wallet fails to parse.
**Why it happens:** Confusion between issuance format (`IssuerSigned` = base64url-encoded CBOR) and presentation format (`DeviceResponse` = different CBOR structure).
**How to avoid:** During issuance, encode `IssuerSigned` CBOR as base64url string. During presentation, encode entire `DeviceResponse` as base64url string for VP Token.

### Pitfall 6: DCQL `meta` field is REQUIRED

**What goes wrong:** DCQL query omits `meta`; wallet cannot match credential type.
**Why it happens:** Developers assume `meta` is optional.
**Why it matters:** Spec says `meta: REQUIRED`. Missing `meta` is a `{}` empty object (no constraints), but for typed requests `vct_values`/`doctype_value` are required within `meta`.
**How to avoid:** Validate DCQL query; require non-empty `meta` when format is `dc+sd-jwt` or `mso_mdoc`.

### Pitfall 7: VP Token is keyed by DCQL `id`, not format

**What goes wrong:** Wallet returns `{"dc+sd-jwt": ["..."]}` instead of `{"my_credential_id": ["..."]}`.
**Why it happens:** Old Presentation Exchange used path-based mapping. DCQL uses `id` as the key.
**How to avoid:** VP Token builder must use the `id` field from the `CredentialQuery`, not the format identifier.

### Pitfall 8: Nonce replay in VP presentations

**What goes wrong:** Verifier accepts replayed VP Token from previous session.
**Why it happens:** Not checking that `nonce` in KB-JWT / VP proof matches the fresh `nonce` in the request.
**How to avoid:** Always store nonce in session; validate nonce in KB-JWT `nonce` claim (SD-JWT) or VP `nonce` claim (jwt_vc_json) or `challenge` (ldp_vc).

---

## State of the Art

| Old Approach                                    | Current Approach                      | When Changed                 | Impact                              |
| ----------------------------------------------- | ------------------------------------- | ---------------------------- | ----------------------------------- |
| `format` in Credential Request                  | `credential_configuration_id` routing | OID4VCI 1.0 final (draft-17) | Remove `format` from request model  |
| `credential` singular in response               | `credentials` array                   | OID4VCI 1.0 final            | Update response parser + builder    |
| `acceptance_token` for deferred                 | `transaction_id`                      | OID4VCI 1.0 final            | Rename in deferred models           |
| `proof` (singular)                              | `proofs` object keyed by type         | OID4VCI 1.0 final            | Both still accepted by some issuers |
| `c_nonce` in credential response                | Dedicated Nonce Endpoint              | OID4VCI 1.0 final            | Add Nonce Endpoint support          |
| Presentation Exchange `presentation_definition` | DCQL `dcql_query`                     | OID4VP 1.0 final (draft-30)  | Primary query language change       |
| `presentation_submission` in response           | VP Token keyed by DCQL `id`           | OID4VP 1.0 final             | Response structure change           |
| `proof_type` identifier was `ldp_vp`, `cwt`     | Still valid; `di_vp` added            | OID4VCI 1.0 final            | Add `di_vp` proof type              |

**Deprecated/outdated:**

- `vc+sd-jwt` format identifier: Replaced by `dc+sd-jwt`. Keep for backward compat but don't use in new code.
- `presentation_definition` + `presentation_submission` pattern: OID4VP 1.0 migrated to DCQL. Existing PE module still valid for PE-based profiles.
- `proof` (singular key in request body): Superseded by `proofs` object.

---

## Code Examples

### JWT Proof Construction

```csharp
// Source: OID4VCI 1.0 §13.1 JWT Proof Type
// Header: { "typ": "openid4vci-proof+jwt", "alg": "ES256", "jwk": { ... } }
// Body:   { "aud": "<issuer-url>", "iat": <now>, "nonce": "<c_nonce>" }
var header = new JwtHeader(signingCredentials)
{
    { "typ", "openid4vci-proof+jwt" }
};
var payload = new JwtPayload
{
    { "aud", credentialIssuerUrl },
    { "iat", DateTimeOffset.UtcNow.ToUnixTimeSeconds() },
    { "nonce", cNonce }  // Only when issuer has nonce endpoint
};
```

### Credential Request (Final Spec)

```csharp
// Source: OID4VCI 1.0 §8.2 Credential Request
// Route via credential_identifier (when token response had authorization_details)
var request = new CredentialRequest
{
    CredentialIdentifier = "CivilEngineeringDegree-2023",
    Proofs = new CredentialProofs
    {
        Jwt = new[] { proofJwt1, proofJwt2 }
    }
};
// OR route via credential_configuration_id (when using scope)
var request2 = new CredentialRequest
{
    CredentialConfigurationId = "UniversityDegreeCredential",
    Proofs = new CredentialProofs { Jwt = new[] { proofJwt1 } }
};
```

### Credential Response (Final Spec)

```csharp
// Source: OID4VCI 1.0 §8.3 Credential Response
// credentials is an array; each element is { "credential": "<value>" }
var response = new CredentialResponse
{
    Credentials = new[]
    {
        new CredentialResponseItem { Credential = sdJwtString },
        new CredentialResponseItem { Credential = sdJwtString2 }
    },
    NotificationId = "3fwe98js"
};
```

### DCQL Query Building

```csharp
// Source: OID4VP 1.0 §7 DCQL
var query = new DcqlQuery
{
    Credentials = new[]
    {
        new DcqlCredentialQuery
        {
            Id = "id_card",
            Format = "dc+sd-jwt",
            Meta = new SdJwtVcMeta { VctValues = new[] { "https://example.com/id_card" } },
            RequireCryptographicHolderBinding = true,
            Claims = new[]
            {
                new DcqlClaimsQuery { Id = "ln", Path = new object[] { "family_name" } },
                new DcqlClaimsQuery { Id = "fn", Path = new object[] { "given_name" } }
            }
        }
    }
};
```

### VP Token Construction

```csharp
// Source: OID4VP 1.0 §8.1 Response Parameters
// vp_token is keyed by DCQL credential query id
var vpToken = new Dictionary<string, string[]>
{
    { "id_card", new[] { sdJwtKbPresentation } }
};
```

---

## Validation Architecture

### Test Framework

| Property           | Value                                       |
| ------------------ | ------------------------------------------- |
| Framework          | xUnit (existing in all test projects)       |
| Config file        | None detected — tests use `dotnet test`     |
| Quick run command  | `dotnet test tests/SdJwt.Net.Oid4Vci.Tests` |
| Full suite command | `dotnet test SdJwt.Net.sln`                 |

### Phase Requirements to Test Map

| Req | Behavior                                                      | Test Type   | Command                                                             |
| --- | ------------------------------------------------------------- | ----------- | ------------------------------------------------------------------- |
| R1  | `CredentialRequest` serialises without `format` field         | Unit        | `dotnet test tests/SdJwt.Net.Oid4Vci.Tests -k "CredentialRequest"`  |
| R2  | `CredentialResponse` deserialises `credentials` array         | Unit        | `dotnet test tests/SdJwt.Net.Oid4Vci.Tests -k "CredentialResponse"` |
| R3  | `transaction_id` serialised in deferred response              | Unit        | `dotnet test tests/SdJwt.Net.Oid4Vci.Tests -k "Deferred"`           |
| R4  | mso_mdoc config serialises `doctype` not `format` in metadata | Unit        | `dotnet test tests/SdJwt.Net.Oid4Vci.Tests -k "MsoMdoc"`            |
| R5  | DCQL query with `meta` required for dc+sd-jwt                 | Unit        | `dotnet test tests/SdJwt.Net.Oid4Vp.Tests -k "Dcql"`                |
| R6  | VP Token keys match DCQL credential query `id`                | Unit        | `dotnet test tests/SdJwt.Net.Oid4Vp.Tests -k "VpToken"`             |
| R7  | mdoc DeviceResponse encoding in VP Token                      | Integration | `dotnet test tests/SdJwt.Net.Mdoc.Tests`                            |

---

## Security Domain

### Applicable ASVS Categories

| ASVS Category         | Applies | Standard Control                                                |
| --------------------- | ------- | --------------------------------------------------------------- |
| V2 Authentication     | yes     | Wallet Attestation JWT (`oauth-client-attestation+jwt`)         |
| V3 Session Management | yes     | `nonce` freshness; `transaction_id` single-use                  |
| V4 Access Control     | yes     | Access Token scope; `credential_configuration_id` authorization |
| V5 Input Validation   | yes     | DCQL query validation; proof type validation                    |
| V6 Cryptography       | yes     | JOSE algorithm agility; no `none` alg; no symmetric proof       |

### Known Threat Patterns

| Pattern                      | STRIDE                 | Standard Mitigation                                                                       |
| ---------------------------- | ---------------------- | ----------------------------------------------------------------------------------------- |
| Pre-Auth Code replay         | Spoofing               | `tx_code` second channel; short code lifetime                                             |
| Key proof replay             | Spoofing               | `c_nonce` from Nonce Endpoint; short nonce lifetime                                       |
| VP Token replay              | Spoofing/Elevation     | `nonce` binding in KB-JWT; single-use nonce                                               |
| Transaction Code phishing    | Social Engineering     | Wallet shows issuer domain before accepting code                                          |
| Credential Offer injection   | Tampering              | Wallet validates issuer trust before fetching metadata                                    |
| Response substitution (mdoc) | Tampering              | JWK Thumbprint in SessionTranscript for encrypted responses                               |
| Stale issuer metadata        | Information Disclosure | `Cache-Control: no-store` on nonce endpoint; validate `credential_issuer` == metadata URL |
| PKCE downgrade               | Tampering              | Always require `code_challenge_method=S256`                                               |

---

## Open Questions

1. **Should `presentation_definition` be deprecated or run in parallel with DCQL?**

   - What we know: The final OID4VP 1.0 spec defines DCQL as the primary; PE is not in the spec
   - What's unclear: Whether to maintain backward compat for PE-based wallets/verifiers
   - Recommendation: Keep existing PE module as is; add DCQL as primary; mark PE paths as legacy

2. **`c_nonce` backward compat in Credential Response**

   - What we know: Final spec removes `c_nonce` from Credential Response; uses Nonce Endpoint
   - What's unclear: Whether to keep `c_nonce` in response model for older issuers
   - Recommendation: Keep as optional `[JsonIgnore(WhenWritingNull)]` read field; don't generate it

3. **`credential_configuration_id` in Credential Request — typed or string?**

   - What we know: It's a string key from the issuer metadata map
   - What's unclear: Whether to strongly-type the routing per format or keep as string
   - Recommendation: Keep as string; format-specific validation happens server-side

4. **mso_mdoc CBOR library choice**
   - What we know: Both `PeterO.Cbor` and `Dahomey.Cbor` are in ecosystem; `SdJwt.Net.Mdoc` project likely already has one
   - Recommendation: Check what `SdJwt.Net.Mdoc` already uses and align

---

## Environment Availability

| Dependency               | Required By    | Available         | Version         | Fallback |
| ------------------------ | -------------- | ----------------- | --------------- | -------- |
| .NET SDK                 | All            | ✓                 | per global.json | —        |
| `dotnet test`            | Tests          | ✓                 | —               | —        |
| xUnit                    | Tests          | ✓                 | —               | —        |
| `SdJwt.Net.Mdoc` project | mso_mdoc tests | ✓ (csproj exists) | —               | —        |

---

## Assumptions Log

| #   | Claim                                                                           | Section                  | Risk if Wrong                                        |
| --- | ------------------------------------------------------------------------------- | ------------------------ | ---------------------------------------------------- |
| A1  | `SdJwt.Net.Mdoc` already has a CBOR library dependency                          | Environment Availability | May need to add PeterO.Cbor or equivalent            |
| A2  | Existing `DcqlCredentialQuery` model is structurally sound, just missing fields | Code Gaps                | May need full rewrite if underlying model is wrong   |
| A3  | Project targets .NET 6+ (based on `#if NET6_0_OR_GREATER` guards)               | Standard Stack           | If .NET Standard 2.0 required, some APIs unavailable |

---

## Sources

### Primary (HIGH confidence — direct spec read)

- `specs/openid-4-verifiable-credential-issuance-1_0-final.md` (draft-17, approved final) — all OID4VCI claims
- `specs/openid-4-verifiable-presentations-1_0.md` (draft-30) — all OID4VP claims
- Codebase (`src/SdJwt.Net.Oid4Vci/`, `src/SdJwt.Net.Oid4Vp/`) — current implementation state

### Metadata

**Confidence breakdown:**

- Standard Stack: HIGH — sourced from existing csproj files
- OID4VCI Protocol: HIGH — full spec text read locally
- OID4VP Protocol: HIGH — full spec text read locally
- Code Gaps: HIGH — direct inspection of existing model files
- DCQL: HIGH — complete section 7 of OID4VP spec read

**Research date:** 2026-05-08
**Valid until:** 2026-08-08 (90 days — specs are approved/stable)
