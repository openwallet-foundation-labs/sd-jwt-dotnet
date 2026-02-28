# Selective Disclosure Mechanics

This document covers the cryptographic and algorithmic details of SD-JWT selective disclosure. For conceptual introduction and basic usage, see [SD-JWT Deep Dive](sd-jwt-deep-dive.md).

## Prerequisites

Before reading this document, you should understand:

- Basic SD-JWT concepts from [SD-JWT Deep Dive](sd-jwt-deep-dive.md)
- Cryptographic hash functions (SHA-256 family)
- Base64url encoding
- JSON serialization rules

## Cryptographic Foundations

### Why Salts Matter

A disclosure without a salt would be vulnerable to **preimage attacks**. If an attacker knows the possible values of a claim (e.g., `age` is between 18-100), they could hash each possibility and compare against the `_sd` array to discover the hidden value.

**Without salt (vulnerable):**

```text
Possible ages: 18, 19, 20, ..., 100
Attacker computes: HASH("age", 25), HASH("age", 26), ...
Attacker finds match in _sd array -> discovers age = 25
```

**With salt (secure):**

```text
Salt is random 128-bit value: "_26bc4LT-ac6q2KI6cBAceg"
Disclosure: ["_26bc4LT-ac6q2KI6cBAceg", "age", 25]
Attacker cannot guess salt -> cannot precompute hash
```

### Salt Generation Requirements

Per RFC 9901, salts must be:

- Cryptographically random (not predictable)
- At least 128 bits of entropy
- Unique per disclosure (never reused)

```csharp
// SdJwt.Net salt generation
public static string GenerateSalt(int byteLength = 16) // 128 bits
{
    var bytes = new byte[byteLength];
    RandomNumberGenerator.Fill(bytes);
    return Base64UrlEncoder.Encode(bytes);
}
```

### Hash Algorithm Selection

RFC 9901 requires the hash algorithm to be specified in the `_sd_alg` claim. This library supports:

| Algorithm | `_sd_alg` Value | Security Level | Recommendation |
| --- | --- | --- | --- |
| SHA-256 | `sha-256` | Standard | Default, recommended for most use cases |
| SHA-384 | `sha-384` | High | For higher security requirements |
| SHA-512 | `sha-512` | Very High | For maximum security |
| MD5 | N/A | Broken | **BLOCKED** - cryptographically broken |
| SHA-1 | N/A | Broken | **BLOCKED** - collision attacks proven |

## Disclosure Format Specification

### Object Property Disclosure

For object properties, the disclosure is a 3-element JSON array:

```text
[salt, claim_name, claim_value]
```

**Example:**

```json
["_26bc4LT-ac6q2KI6cBAceg", "email", "alice@example.com"]
```

**Encoding process:**

```text
1. JSON serialize: '["_26bc4LT-ac6q2KI6cBAceg","email","alice@example.com"]'
2. UTF-8 encode to bytes
3. Base64url encode: 'WyJfMjZiYzRMVC1hYzZxMktJNmNCQWNlZyIsImVtYWlsIiwiYWxpY2VAZXhhbXBsZS5jb20iXQ'
```

### Array Element Disclosure

For array elements, the disclosure is a 2-element JSON array (no claim name):

```text
[salt, element_value]
```

**Example (disclosing a nationality from a nationalities array):**

```json
["lklxF5jMYlGTPUovMNIvCA", "US"]
```

### Implementation in SdJwt.Net

```csharp
// From Models/Disclosure.cs
public Disclosure(string salt, string claimName, object claimValue)
{
    Salt = salt;
    ClaimName = claimName;
    ClaimValue = claimValue;

    object[] disclosureArray;
    if (string.IsNullOrEmpty(ClaimName))
    {
        // Array element disclosure: [salt, value]
        disclosureArray = new object[] { Salt, ClaimValue };
    }
    else
    {
        // Object property disclosure: [salt, name, value]
        disclosureArray = new object[] { Salt, ClaimName, ClaimValue };
    }

    var json = JsonSerializer.Serialize(disclosureArray, 
        SdJwtConstants.DefaultJsonSerializerOptions);
    EncodedValue = Base64UrlEncoder.Encode(Encoding.UTF8.GetBytes(json));
}
```

## Digest Computation

### Formula

```text
digest = BASE64URL(HASH(ASCII(base64url_encoded_disclosure)))
```

**Step-by-step example:**

```text
1. Disclosure array: ["_26bc4LT-ac6q2KI6cBAceg", "email", "alice@example.com"]

2. JSON serialize (UTF-8):
   '["_26bc4LT-ac6q2KI6cBAceg","email","alice@example.com"]'

3. Base64url encode:
   'WyJfMjZiYzRMVC1hYzZxMktJNmNCQWNlZyIsImVtYWlsIiwiYWxpY2VAZXhhbXBsZS5jb20iXQ'

4. Convert to ASCII bytes (the encoded string IS ASCII)

5. SHA-256 hash the bytes

6. Base64url encode the hash:
   'JnPBS7TpL8ncxL-6mymWKgzZPk4J98xU8C4d1yXt9qE'
```

### Implementation

```csharp
// Digest computation in SdJwt.Net
public static string ComputeDigest(string encodedDisclosure, string algorithm = "sha-256")
{
    using var hashAlgorithm = algorithm switch
    {
        "sha-256" => SHA256.Create(),
        "sha-384" => SHA384.Create(),
        "sha-512" => SHA512.Create(),
        _ => throw new NotSupportedException($"Algorithm {algorithm} is not supported")
    };

    var bytes = Encoding.ASCII.GetBytes(encodedDisclosure);
    var hash = hashAlgorithm.ComputeHash(bytes);
    return Base64UrlEncoder.Encode(hash);
}
```

## Nested Selective Disclosure

SD-JWT supports selective disclosure at any nesting level within JSON objects.

### Example: Nested Address

**Original claims:**

```json
{
  "name": "Alice",
  "address": {
    "street": "123 Main St",
    "city": "Springfield",
    "country": "US"
  }
}
```

**With nested selective disclosure (city and country are disclosable):**

```json
{
  "name": "Alice",
  "address": {
    "street": "123 Main St",
    "_sd": [
      "digest_for_city",
      "digest_for_country"
    ]
  }
}
```

**Separate disclosures:**

```json
["salt1", "city", "Springfield"]
["salt2", "country", "US"]
```

### Implementation

```csharp
var options = new SdIssuanceOptions
{
    DisclosureStructure = new
    {
        address = new
        {
            city = true,     // Selectively disclosable
            country = true   // Selectively disclosable
            // street is NOT listed -> always visible
        }
    }
};
```

## Decoy Digests

Decoy digests prevent information leakage about the number of hidden claims.

### The Privacy Problem

Without decoys, a verifier seeing 3 digests in `_sd` knows there are exactly 3 hidden claims, even if they cannot see the values.

### How Decoys Work

Decoy digests are random hashes with no corresponding disclosure. They are cryptographically indistinguishable from real digests.

```json
{
  "patient": "John",
  "_sd": [
    "real_hash_1",
    "decoy_hash_a",
    "real_hash_2",
    "decoy_hash_b",
    "real_hash_3"
  ]
}
```

Now the verifier cannot determine how many real claims exist.

### Decoy Generation

```csharp
public static string GenerateDecoyDigest()
{
    var randomBytes = new byte[32]; // SHA-256 output size
    RandomNumberGenerator.Fill(randomBytes);
    return Base64UrlEncoder.Encode(randomBytes);
}
```

## Verification Algorithm

When a verifier receives a presentation with disclosures:

```text
1. Parse the SD-JWT and extract the _sd array and _sd_alg

2. For each provided disclosure:
   a. Decode the disclosure from Base64url
   b. Validate the JSON structure (2 or 3 elements)
   c. Compute digest using _sd_alg algorithm
   d. Check if computed digest exists in _sd array
   e. If not found, REJECT (disclosure was not issued)

3. For each digest in _sd array:
   a. Either a matching disclosure was provided (claim revealed)
   b. Or no disclosure provided (claim remains hidden)
   c. Unmatched digests may be decoys

4. Extract revealed claims into the verified payload
```

## Key Binding JWT (KB-JWT) Hash

The KB-JWT contains an `sd_hash` claim that binds it to a specific SD-JWT presentation.

### SD Hash Computation

```text
sd_hash = BASE64URL(SHA-256(ASCII(sd_jwt_without_kb_jwt)))
```

This ensures the KB-JWT cannot be reused with a different SD-JWT presentation.

## JSON Serialization Rules

Consistent JSON serialization is critical for digest matching.

### Rules Enforced by This Library

1. **No whitespace** between elements
2. **UTF-8 encoding** for string values
3. **Consistent key ordering** (as specified in the original claim)
4. **Standard JSON escaping** for special characters

### Why This Matters

If the issuer and verifier use different JSON serialization:

```text
Issuer: '["salt","email","alice@example.com"]'  -> hash A
Verifier: '["salt", "email", "alice@example.com"]'  -> hash B (space added)

hash A != hash B -> Verification fails!
```

## Implementation References

| Component | File | Description |
| --- | --- | --- |
| Disclosure model | [Disclosure.cs](../../src/SdJwt.Net/Models/Disclosure.cs) | Disclosure creation and parsing |
| Hash utilities | [SdJwtUtils.cs](../../src/SdJwt.Net/Internal/SdJwtUtils.cs) | Salt generation and digest computation |
| Parser | [SdJwtParser.cs](../../src/SdJwt.Net/Utils/SdJwtParser.cs) | SD-JWT string parsing |
| Constants | [SdJwtConstants.cs](../../src/SdJwt.Net/SdJwtConstants.cs) | Algorithm names and claim constants |

## Related Concepts

- [SD-JWT Deep Dive](sd-jwt-deep-dive.md) - Conceptual introduction and basic usage
- [Verifiable Credential Deep Dive](verifiable-credential-deep-dive.md) - Using SD-JWT for credentials
- [HAIP Compliance](haip-compliance.md) - Algorithm requirements for high assurance
