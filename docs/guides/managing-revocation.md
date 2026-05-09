# How to manage credential status with Status Lists

| Field                | Value                                                           |
| -------------------- | --------------------------------------------------------------- |
| **Package maturity** | Spec-tracking (Token Status List draft-20)                      |
| **Code status**      | Mixed -- runnable package APIs with illustrative service wiring |
| **Related concept**  | [Verifiable Credentials](../concepts/verifiable-credentials.md) |
| **Related tutorial** | [Tutorials](../tutorials/index.md)                              |

|                      |                                                                                                                                                                                                                                                      |
| -------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Audience**         | Developers implementing credential lifecycle management on issuer and verifier sides.                                                                                                                                                                |
| **Purpose**          | Walk through end-to-end status management using Status Lists - creating lists, issuing credentials with status entries, revoking credentials, and wiring status-list checking into the verifier pipeline - using `SdJwt.Net.StatusList`.             |
| **Scope**            | Status list service setup, credential-to-index binding, revocation operations, CDN publishing, and verifier-side status checking. Out of scope: token introspection (see [Token Introspection](token-introspection.md)), hybrid checking strategies. |
| **Success criteria** | Reader can issue a credential bound to a status list index, revoke it, publish the updated bitstring, and wire status-list checking into their verifier pipeline so that revoked credentials are rejected.                                           |

## What your application still owns

This guide does not provide: production key custody, CDN deployment, cache invalidation strategy, fail-open/fail-closed policy decisions, monitoring and alerting on status list freshness, or incident response for missed revocations.

> **Freshness note:** Status lists are cached artifacts. Token expiry, CDN TTL, and verifier cache TTL all influence how quickly a revocation propagates. Decide on a fail-open vs. fail-closed policy when a fresh status list is temporarily unavailable.

> This guide uses architectural pseudocode for service wiring. For concrete package usage, see `samples/SdJwt.Net.Samples/Standards/VerifiableCredentials/StatusListExample.cs`.

---

## Key decisions

| Decision                                      | Options                | Guidance                                 |
| --------------------------------------------- | ---------------------- | ---------------------------------------- |
| Status list key separate from credential key? | Yes/No                 | Always yes for production                |
| Publishing frequency?                         | On-demand or scheduled | On-demand for immediate revocation needs |
| Cache TTL for verifiers?                      | Seconds to minutes     | Balance between freshness and load       |
| Hosting strategy?                             | CDN, API, or hybrid    | CDN for high-volume verification         |
| Fail-closed on status unavailability?         | Yes/No                 | Yes for high-risk flows                  |

---

## Prerequisites

Ensure your project references the necessary NuGet packages:

```bash
dotnet add package SdJwt.Net.StatusList
dotnet add package SdJwt.Net.Oid4Vci
```

## 1. Configure the status list service (issuer side)

The Status List Service handles generating and hosting the compressed bitstrings. Host these files statically on a CDN for maximum performance and privacy — verifiers should not need to query your API for every user login, as that would expose user activity patterns.

In your `Program.cs`:

```csharp
using SdJwt.Net.StatusList.Issuer;

var builder = WebApplication.CreateBuilder(args);

// Register your status-list infrastructure and app services here.
// Example package primitive used by issuer code:
var statusManager = new StatusListManager(signingKey, SecurityAlgorithms.EcdsaSha256);

var app = builder.Build();
```

## 2. Issue a credential with status (issuer side)

When issuing a credential, include a `status` claim that references the status list URI and the credential's index. The verifier will later check this index to determine revocation status.

```csharp
app.MapPost("/issue-employee-id", (CredentialRequest request) =>
{
    // 1. Assign a unique index for this credential in the status list
    var statusIndex = AllocateNextIndex(request.UserId);

    // 2. Build the SD-JWT VC payload with status reference
    var vcPayload = new SdJwtVcPayload
    {
        Issuer = "https://issuer.example.com",
        Subject = $"did:example:employee:{request.UserId}",
        IssuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
        ExpiresAt = DateTimeOffset.UtcNow.AddYears(1).ToUnixTimeSeconds(),
        Vct = "EmployeeIdCredential",

        // Status claim references the status list URI and index
        Status = new
        {
            status_list = new
            {
                idx = statusIndex,
                uri = "https://issuer.example.com/.well-known/status-list"
            }
        },

        AdditionalData = new Dictionary<string, object>
        {
            ["name"] = request.Name,
            ["department"] = request.Department
        }
    };

    var result = vcIssuer.Issue("EmployeeIdCredential", vcPayload, options);

    return Results.Ok(new { credential = result.Issuance });
});
```

The VC now contains a `status_list` object indicating its URL and index.

## 3. Revoke a credential (issuer side)

When an employee leaves the company, revoke their credential.

```csharp
app.MapPost("/revoke-employee", async (string userId) =>
{
    // Look up the credential's index in the status list
    var credentialIndex = GetCredentialIndex(userId);

    // Revoke the credential by updating the status list token
    var updatedToken = await statusManager.RevokeTokensAsync(
        existingStatusListToken,
        new[] { credentialIndex });

    // Publish the updated token to the CDN / .well-known endpoint
    await PublishStatusListToken(updatedToken);

    return Results.Ok();
});
```

The `StatusListManager` handles compression and encoding natively. The `RevokeTokensAsync()` method sets the status bits for the given indices to `StatusType.Invalid` and returns a new signed status list token.

## 4. Check credential status (verifier side)

When a verifier receives an SD-JWT presentation, it must check whether the credential has been revoked.

Because `SdJwt.Net.Oid4Vp` and `SdJwt.Net.HAIP` integrate with the Status List package, this check runs **automatically** during verification when the `StatusListService` is registered in the verifier's Dependency Injection container.

```csharp
// In the Verifier application, use StatusListVerifier to check credential status.
using SdJwt.Net.StatusList.Verifier;
using SdJwt.Net.StatusList.Models;

var statusVerifier = new StatusListVerifier(httpClient);
```

When verifying a credential, extract its `status` claim and check against the published status list:

```csharp
// Parse the status claim from the presented credential
var statusClaim = new StatusClaim
{
    StatusList = new StatusListReference
    {
        Index = 42,
        Uri = "https://issuer.example.com/.well-known/status-list"
    }
};

var statusResult = await statusVerifier.CheckStatusAsync(
    statusClaim,
    issuerKeyProvider: async iss => await FetchIssuerKey(iss));

if (!statusResult.IsValid)
{
    // Credential has been revoked or suspended
    return Results.Unauthorized();
}
```
