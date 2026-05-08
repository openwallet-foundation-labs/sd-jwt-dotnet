# Tutorial: Status list

Implement credential revocation and suspension with Token Status Lists.

**Time:** 15 minutes  
**Level:** Intermediate  
**Sample:** `samples/SdJwt.Net.Samples/02-Intermediate/02-StatusList.cs`

## What you will learn

- How credential status works
- Creating and managing status lists
- Checking credential validity

## Why status lists?

Credentials may need to be invalidated before expiration:

- Employee leaves company
- License suspended
- Credential compromised

## Status list architecture

```text
┌─────────────┐        ┌──────────────┐        ┌──────────────┐
│   Issuer    │───────>│ Status List  │<───────│   Verifier   │
│             │ update │   Service    │ check  │              │
└─────────────┘        └──────────────┘        └──────────────┘
                              │
                              │ hosts
                              ▼
                       ┌──────────────┐
                       │ Status Token │
                       │  (JWT with   │
                       │  bit array)  │
                       └──────────────┘
```

## Step 1: Create status list manager

```csharp
using SdJwt.Net.StatusList;
using SdJwt.Net.StatusList.Models;

// Create a status list that can hold 100,000 credentials
var statusList = new StatusList(capacity: 100000);
```

## Step 2: Issue credential with status reference

```csharp
var statusIndex = 42;  // Unique index for this credential

var payload = new SdJwtVcPayload
{
    Issuer = "https://issuer.example.com",
    Subject = "holder-123",
    AdditionalData = new Dictionary<string, object>
    {
        ["given_name"] = "Alice",
        ["status"] = new
        {
            status_list = new
            {
                idx = statusIndex,
                uri = "https://issuer.example.com/.well-known/status/1"
            }
        }
    }
};
```

## Step 3: Publish status list token

```csharp
// Create the Status List Token
var statusListToken = StatusListManager.CreateStatusListTokenAsync(
    statusList,
    issuerKey,
    "https://issuer.example.com",
    SecurityAlgorithms.EcdsaSha256
);

// Host at: https://issuer.example.com/.well-known/status/1
```

## Step 4: Check credential status (verifier)

```csharp
// Fetch status list from issuer
var httpClient = new HttpClient();
var statusListToken = await httpClient.GetStringAsync(
    "https://issuer.example.com/.well-known/status/1"
);

// Parse and check status
var statusList = StatusListManager.ParseStatusListToken(statusListToken);
var status = statusList.GetStatus(statusIndex: 42);

if (status == CredentialStatus.Revoked)
{
    throw new Exception("Credential has been revoked");
}
```

## Step 5: Revoke a credential (issuer)

```csharp
// Mark credential as revoked
statusList.SetStatus(index: 42, status: CredentialStatus.Revoked);

// Re-publish the updated status list token
var updatedToken = StatusListManager.CreateStatusListTokenAsync(
    statusList,
    issuerKey,
    "https://issuer.example.com",
    SecurityAlgorithms.EcdsaSha256
);
```

## Status types

| Status    | Value | Use Case                |
| --------- | ----- | ----------------------- |
| Valid     | 0     | Credential is active    |
| Revoked   | 1     | Permanently invalidated |
| Suspended | 2     | Temporarily invalid     |

## Suspension vs revocation

```csharp
// Suspend temporarily (can be undone)
statusList.SetStatus(42, CredentialStatus.Suspended);

// Later: reinstate
statusList.SetStatus(42, CredentialStatus.Valid);

// Revoke permanently
statusList.SetStatus(42, CredentialStatus.Revoked);
```

## Complete verification flow

```csharp
// 1. Verify SD-JWT signature
var result = await verifier.VerifyAsync(presentation, params);

// 2. Extract status reference
var statusClaim = result.ClaimsPrincipal.FindFirst("status");
var statusRef = JsonSerializer.Deserialize<StatusReference>(statusClaim.Value);

// 3. Fetch and verify status list
var statusListToken = await httpClient.GetStringAsync(statusRef.Uri);
var statusList = StatusListManager.ParseStatusListToken(statusListToken);

// 4. Check specific credential status
var status = statusList.GetStatus(statusRef.Index);
if (status != CredentialStatus.Valid)
{
    throw new Exception($"Credential status: {status}");
}

// 5. Continue with verified credential
Console.WriteLine("Credential is valid and not revoked");
```

## Caching considerations

```csharp
// Status lists can be cached with appropriate TTL
var statusListToken = await httpClient.GetStringAsync(statusUri);
var payload = ParseJwtPayload(statusListToken);
var expiresAt = DateTimeOffset.FromUnixTimeSeconds(payload.Exp);

// Cache until expiration or shorter interval
var cacheDuration = TimeSpan.FromMinutes(15);
```

## Run the sample

```bash
cd samples/SdJwt.Net.Samples
dotnet run -- 2.2
```

## Next steps

- [OpenID4VCI](03-openid4vci.md) - Issue credentials via protocol
- [OpenID4VP](04-openid4vp.md) - Present credentials via protocol

## Key takeaways

1. Status lists enable revocation without credential recall
2. Each credential has a unique index in the list
3. Verifiers must check status before accepting credentials
4. Suspension allows temporary invalidation
