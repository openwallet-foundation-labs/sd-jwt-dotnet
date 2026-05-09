# Status List

> **Level:** Intermediate lifecycle management

## Simple explanation

Credentials need a way to be revoked or suspended after issuance. Status lists solve this without the verifier contacting the issuer for every check.

The issuer publishes a compressed bitstring where each credential has a position. Verifiers fetch the list and check locally. The issuer never learns which credential was checked.

## What you will learn

- Why status lists exist and how they preserve holder privacy
- How the bitstring encoding maps credential indices to status values
- How to create, update, and verify status lists with `SdJwt.Net.StatusList`
- How to configure caching and fail-open vs fail-closed policies

|                      |                                                                                                                                                                                                                                                                                            |
| -------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| **Audience**         | Developers implementing credential revocation or suspension, and operations teams managing status infrastructure.                                                                                                                                                                          |
| **Purpose**          | Explain how status lists enable privacy-preserving credential lifecycle management (revocation, suspension) at scale, with working `SdJwt.Net.StatusList` code examples.                                                                                                                   |
| **Scope**            | Status list data model, token structure, bit encoding, issuer-side creation and updates, verifier-side checking with caching, and operational considerations (TTL, fail-open vs fail-closed). Out of scope: credential issuance (see [VC](verifiable-credentials.md)), base SD-JWT format. |
| **Success criteria** | Reader can create a status list for thousands of credentials, update individual status entries, verify credential status with proper caching, and configure fail-open/fail-closed policies.                                                                                                |

## Prerequisites

Before reading this document, you should understand:

| Prerequisite     | Why Needed                   | Resource                        |
| ---------------- | ---------------------------- | ------------------------------- |
| SD-JWT VC basics | Status lists apply to VCs    | [VC](verifiable-credentials.md) |
| JWT structure    | Status lists are signed JWTs | [SD-JWT](sd-jwt.md)             |

## Glossary

| Term                  | Definition                                                   |
| --------------------- | ------------------------------------------------------------ |
| **Status List**       | Compressed bitstring representing status of many credentials |
| **Status List Token** | Signed JWT containing the status list data                   |
| **Referenced Token**  | A credential that points to a status list                    |
| **Index**             | Position of a credential's status within the status list     |
| **Bits**              | Number of bits per status entry (1, 2, 4, or 8)              |
| **Revocation**        | Permanent invalidation of a credential                       |
| **Suspension**        | Temporary invalidation (can be lifted)                       |
| **TTL**               | Time-to-live for caching the status list                     |

## Why status lists matter

A credential is valid when issued but may become invalid later:

- Employee leaves company (revoke employment credential)
- Driver's license suspended (suspend, don't revoke)
- University discovers fraud (revoke degree)

Without status lists, a verifier must call the issuer for each credential check, revealing which credentials are being verified and adding load to issuer infrastructure.

With status lists, a single signed token represents the status of thousands of credentials. The verifier fetches it anonymously and caches it locally, so the issuer never learns which specific credential is being checked.

```mermaid
flowchart LR
    subgraph "Without Status List"
        V1[Verifier] -->|"Check credential #12345"| I1[Issuer]
        I1 -->|"Status: Revoked"| V1
        Note1[Privacy leak: Issuer knows which credential is checked]
    end

    subgraph "With Status List"
        V2[Verifier] -->|"GET /status/list1"| I2[Issuer]
        I2 -->|"Compressed bitstring for 100K credentials"| V2
        V2 -->|"Check bit #12345 locally"| V2
        Note2[Privacy preserved: Issuer doesn't know which credential]
    end
```

## How it works: the data model

### 1. Credential points to status list

When issuing a credential, include a `status` claim:

```json
{
  "iss": "https://university.example.edu",
  "vct": "https://credentials.example.edu/degree",
  "sub": "did:example:student123",
  "given_name": "Alice",
  "degree": "Bachelor of Science",
  "status": {
    "status_list": {
      "idx": 42,
      "uri": "https://university.example.edu/status/degrees-2024"
    }
  }
}
```

| Field                    | Purpose                                       |
| ------------------------ | --------------------------------------------- |
| `status.status_list.idx` | This credential's position in the status list |
| `status.status_list.uri` | Where to fetch the status list token          |

### 2. Status list token structure

The status endpoint returns a signed JWT (`statuslist+jwt`):

**Header:**

```json
{
  "typ": "statuslist+jwt",
  "alg": "ES256",
  "kid": "status-key-2024"
}
```

**Payload:**

```json
{
  "sub": "https://university.example.edu/status/degrees-2024",
  "iat": 1701234567,
  "exp": 1701238167,
  "ttl": 3600,
  "status_list": {
    "bits": 2,
    "lst": "eNrbuRgAAhcBXQ",
    "aggregation_uri": "https://university.example.edu/status/aggregation"
  }
}
```

| Field              | Required | Purpose                                        |
| ------------------ | -------- | ---------------------------------------------- |
| `sub`              | Yes      | Must match the `uri` in referenced credentials |
| `iat`              | Yes      | When the status list was created               |
| `exp`              | No       | When the status list expires                   |
| `ttl`              | No       | How long verifiers may cache (seconds)         |
| `status_list.bits` | Yes      | Bits per entry: 1, 2, 4, or 8                  |
| `status_list.lst`  | Yes      | Base64url-encoded compressed bitstring         |
| `aggregation_uri`  | No       | For discovering multiple status lists          |

### 3. Decoding the status value

The `lst` field is a compressed bitstring. To check credential at index 42:

```
1. Base64url decode -> compressed bytes
2. DEFLATE decompress -> raw bitstring
3. Extract bits at position (idx * bits_per_entry)
4. Interpret value according to status semantics
```

## Status value semantics

The number of bits determines how many distinct statuses you can represent:

| Bits | Max Statuses | Use Case                               |
| ---- | ------------ | -------------------------------------- |
| 1    | 2            | Valid (0) / Revoked (1)                |
| 2    | 4            | Valid / Revoked / Suspended / Reserved |
| 4    | 16           | Application-specific needs             |
| 8    | 256          | Rich status taxonomy                   |

**Standard values (this implementation):**

| Value | Hex    | Meaning              |
| ----- | ------ | -------------------- |
| 0     | `0x00` | Valid                |
| 1     | `0x01` | Invalid (Revoked)    |
| 2     | `0x02` | Suspended            |
| 3     | `0x03` | Application-specific |

## Complete verification flow

```mermaid
sequenceDiagram
    autonumber
    participant Issuer
    participant StatusEndpoint as Status Endpoint
    participant Wallet
    participant Verifier

    Note over Issuer: Issuance Phase
    Issuer->>Issuer: Assign index 42 to credential
    Issuer->>Wallet: Issue credential with status.idx=42

    Note over Verifier: Verification Phase
    Wallet->>Verifier: Present credential
    Verifier->>Verifier: Validate signature, structure, expiry

    alt Status claim present
        Verifier->>StatusEndpoint: GET status_list.uri
        StatusEndpoint-->>Verifier: statuslist+jwt
        Verifier->>Verifier: Validate status token signature
        Verifier->>Verifier: Check exp, iat freshness
        Verifier->>Verifier: Decompress lst
        Verifier->>Verifier: Read value at index 42

        alt Status = Valid (0)
            Verifier-->>Wallet: Accept credential
        else Status = Revoked (1)
            Verifier-->>Wallet: Reject: credential revoked
        else Status = Suspended (2)
            Verifier-->>Wallet: Reject: credential suspended
        end
    else No status claim
        Verifier-->>Wallet: Accept (no revocation checking)
    end
```

## Code example: issuer creating status list

```csharp
using SdJwt.Net.StatusList.Issuer;
using SdJwt.Net.StatusList.Models;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

// Setup signing key (use separate key from credential signing)
using var ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
var statusSigningKey = new ECDsaSecurityKey(ecdsa) { KeyId = "status-key-2024" };

// Create status list manager
var manager = new StatusListManager(statusSigningKey, SecurityAlgorithms.EcdsaSha256);

// Initialize status values for 10,000 credentials
// All start as Valid (0)
var statusValues = new byte[10000];

// Revoke credential at index 42
statusValues[42] = (byte)StatusType.Invalid;

// Suspend credential at index 100
statusValues[100] = (byte)StatusType.Suspended;

// Create the status list token
string statusListToken = await manager.CreateStatusListTokenAsync(
    subject: "https://university.example.edu/status/degrees-2024",
    statusValues: statusValues,
    bits: 2,  // 2 bits = 4 possible states
    validUntil: DateTime.UtcNow.AddHours(24),
    timeToLive: 3600  // Cache for 1 hour
);

// Publish at the status endpoint
await PublishStatusListAsync(
    uri: "https://university.example.edu/status/degrees-2024",
    token: statusListToken
);
```

## Code example: updating status (revocation)

```csharp
// Revoke a credential
var updates = new Dictionary<int, StatusType>
{
    [42] = StatusType.Invalid,  // Revoke credential at index 42
    [99] = StatusType.Suspended  // Suspend credential at index 99
};

string updatedToken = await manager.UpdateStatusAsync(
    existingToken: currentStatusListToken,
    updates: updates
);

// Publish the updated status list
await PublishStatusListAsync(uri, updatedToken);
```

## Code example: verifier checking status

```csharp
using SdJwt.Net.StatusList.Verifier;
using SdJwt.Net.StatusList.Models;

// Create verifier (with caching)
var verifier = new StatusListVerifier(
    httpClient: httpClient,
    memoryCache: cache,
    logger: logger
);

// Extract status claim from credential
var statusClaim = new StatusClaim
{
    StatusList = new StatusListReference
    {
        Index = 42,
        Uri = "https://university.example.edu/status/degrees-2024"
    }
};

// Check status
var result = await verifier.CheckStatusAsync(
    statusClaim: statusClaim,
    issuerKeyProvider: async uri => await GetStatusSigningKey(uri),
    options: new StatusListOptions
    {
        EnableStatusChecking = true,
        CacheDuration = TimeSpan.FromMinutes(15),
        FailOnStatusCheckError = true  // Fail-closed behavior
    }
);

switch (result.Status)
{
    case StatusType.Valid:
        Console.WriteLine("Credential is valid");
        break;
    case StatusType.Invalid:
        Console.WriteLine($"Credential REVOKED at index {statusClaim.StatusList.Index}");
        break;
    case StatusType.Suspended:
        Console.WriteLine("Credential is SUSPENDED");
        break;
}
```

## Operational considerations

### Key separation

Use separate keys for credential signing and status list signing:

```csharp
// Credential signing key (long-term, high security)
var credentialKey = LoadFromHsm("credential-signing-key");

// Status list signing key (can be rotated more frequently)
var statusKey = LoadFromHsm("status-signing-key");
```

### Caching strategy

| Scenario               | TTL          | Rationale                        |
| ---------------------- | ------------ | -------------------------------- |
| High-value credentials | 5-15 minutes | Near real-time revocation needed |
| Standard credentials   | 1-4 hours    | Balance freshness and load       |
| Low-risk scenarios     | 24 hours     | Reduce issuer load               |

### Fail-open vs fail-closed

| Behavior    | When to Use                        | Risk                                     |
| ----------- | ---------------------------------- | ---------------------------------------- |
| Fail-closed | High-security: financial, medical  | Service disruption if status unavailable |
| Fail-open   | Low-risk: newsletters, preferences | May accept revoked credentials           |

```csharp
// Fail-closed (reject if status check fails)
options.FailOnStatusCheckError = true;

// Fail-open (accept if status check fails)
options.FailOnStatusCheckError = false;
```

## Implementation references

| Component             | File                                                                                                         | Description                 |
| --------------------- | ------------------------------------------------------------------------------------------------------------ | --------------------------- |
| Status claim model    | [StatusClaim.cs](../../src/SdJwt.Net.StatusList/Models/StatusClaim.cs)                                       | Credential status reference |
| Status list reference | [StatusListReference.cs](../../src/SdJwt.Net.StatusList/Models/StatusListReference.cs)                       | idx + uri structure         |
| Token payload         | [StatusListTokenPayload.cs](../../src/SdJwt.Net.StatusList/Models/StatusListTokenPayload.cs)                 | JWT payload model           |
| Status list data      | [StatusListData.cs](../../src/SdJwt.Net.StatusList/Models/StatusListData.cs)                                 | Bits + lst structure        |
| Status type enum      | [StatusType.cs](../../src/SdJwt.Net.StatusList/Models/StatusType.cs)                                         | Valid/Invalid/Suspended     |
| Issuer manager        | [StatusListManager.cs](../../src/SdJwt.Net.StatusList/Issuer/StatusListManager.cs)                           | Create/update status lists  |
| Verifier              | [StatusListVerifier.cs](../../src/SdJwt.Net.StatusList/Verifier/StatusListVerifier.cs)                       | Check credential status     |
| Package overview      | [README.md](../../src/SdJwt.Net.StatusList/README.md)                                                        | Quick start                 |
| Sample code           | [StatusListExample.cs](../../samples/SdJwt.Net.Samples/Standards/VerifiableCredentials/StatusListExample.cs) | Working examples            |

## Beginner pitfalls to avoid

### 1. Not validating credential before status check

Always validate the credential (signature, structure, expiry) before checking status.

```csharp
// WRONG order
var statusResult = await CheckStatusAsync(credential);
var signatureValid = await ValidateSignatureAsync(credential);

// RIGHT order
var signatureValid = await ValidateSignatureAsync(credential);
if (signatureValid)
{
    var statusResult = await CheckStatusAsync(credential);
}
```

### 2. Ignoring TTL and expiry

Honor `ttl` for cache duration and `exp` for validity. Do not cache status lists indefinitely.

```csharp
// Check if status list has expired
if (statusListPayload.ExpiresAt.HasValue)
{
    var expiry = DateTimeOffset.FromUnixTimeSeconds(statusListPayload.ExpiresAt.Value);
    if (DateTimeOffset.UtcNow > expiry)
    {
        // Must fetch fresh status list
        await RefreshStatusListAsync(uri);
    }
}
```

### 3. Using same key for credentials and status lists

Use separate keys for credential issuance and status list signing, with potentially different rotation schedules.

### 4. Not handling status check failures

Define explicit fail-open or fail-closed behavior instead of crashing or hanging when the status endpoint is unavailable.

## Frequently asked questions

### Q: What happens if the status endpoint is down?

**A:** Depends on your configuration:

- `FailOnStatusCheckError = true`: Verification fails (fail-closed)
- `FailOnStatusCheckError = false`: Status check skipped (fail-open)

Choose based on your security requirements.

### Q: How do I undo a revocation?

**A:** Use suspension instead of revocation if you might need to restore validity. Update the status value back to `Valid (0)`.

### Q: Can I have multiple status lists per issuer?

**A:** Yes. Each credential points to a specific `uri`. You might have separate status lists for:

- Different credential types
- Different time periods (degrees-2024, degrees-2025)
- Different geographic regions

### Q: How large can a status list be?

**A:** With DEFLATE compression, a status list for 1 million credentials with 2-bit entries is approximately 250KB. The compressed format is very efficient.

### Q: Should I include status in every credential?

**A:** Include status if:

- The credential can be revoked (employment, certifications)
- The credential can be suspended (licenses)

Do not include status for immutable credentials where revocation is not meaningful.

## Related concepts

- [Verifiable Credential](verifiable-credentials.md) - VCs that reference status lists
- [OID4VP](openid4vp.md) - Presenting credentials with status checks
- [SD-JWT](sd-jwt.md) - Base format for credentials
