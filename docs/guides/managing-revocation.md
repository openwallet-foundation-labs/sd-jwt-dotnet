# How to Manage Credential Revocation

|                      |                                                                                                                                                                                                                                                         |
| -------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Audience**         | Developers implementing credential lifecycle management on issuer and verifier sides.                                                                                                                                                                   |
| **Purpose**          | Walk through end-to-end revocation using Status Lists - creating lists, issuing credentials with status entries, revoking credentials, and verifier-side status checking - using `SdJwt.Net.StatusList`.                                                |
| **Scope**            | Status list service setup, credential-to-index binding, revocation operations, CDN publishing, and automatic verifier-side checking. Out of scope: token introspection (see [Token Introspection](token-introspection.md)), hybrid checking strategies. |
| **Success criteria** | Reader can issue a credential bound to a status list index, revoke it, publish the updated bitstring, and verify that the verifier pipeline automatically rejects revoked credentials.                                                                  |

> This guide uses architectural pseudocode for service wiring. For concrete package usage, see `samples/SdJwt.Net.Samples/Standards/VerifiableCredentials/StatusListExample.cs`.

---

## Key Decisions

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

## 1. Configure the Status List Service (The Issuer)

The Status List Service handles generating and hosting the compressed bitstrings. We recommend hosting these files statically on a CDN for maximum performance and privacy (Verifiers shouldn't have to ping your API for every single user login, as this compromises the user's privacy).

In your `Program.cs`:

```csharp
using SdJwt.Net.StatusList.Issuer;

var builder = WebApplication.CreateBuilder(args);

// Register your status-list infrastructure and app services here.
// Example package primitive used by issuer code:
var statusManager = new StatusListManager(signingKey, SecurityAlgorithms.EcdsaSha256);

var app = builder.Build();
```

## 2. Issue a Credential with Status (The Issuer)

When issuing a credential, you must bind it to a specific index in a specific Status List.

```csharp
app.MapPost("/issue-employee-id", async (
    CredentialRequest request,
    ISdJwtIssuerService issuer,
    /* your status list service */ statusList) =>
{
    var credentialBuilder = new VerifiableCredentialBuilder()
        // ... (standard VC configuration) ...
        .WithType("EmployeeIdCredential");

    // 1. Ask the StatusList Service for a new, unused index
    // This reserves the index for this specific credential
    var statusEntry = await statusList.AddCredentialAsync(request.UserId);

    // 2. Add the Status List Entry claim to the credential payload
    // This tells future Verifiers exactly where to look!
    credentialBuilder.WithStatusListEntry(statusEntry);

    var credential = await issuer.CreateCredentialAsync(credentialBuilder);

    return Results.Ok(new { credential = credential.SdJwt });
});
```

The VC now contains a `status_list` object indicating its URL and index.

## 3. Revoke a Credential (The Issuer)

When an employee leaves the company, you must revoke their credential.

```csharp
app.MapPost("/revoke-employee", async (
    string userId,
    /* your status list service */ statusList) =>
{
    // Revoke the credential associated with this internal User ID
    // This updates the bitfield in the database
    await statusList.RevokeCredentialAsync(userId, reason: "Employee departure");

    // Force a rebuild of the compressed bitstring for the CDN
    await statusList.PublishStatusListsAsync();

    return Results.Ok();
});
```

The `SdJwt.Net.StatusList` package handles the complex compression and encoding natively. The `PublishStatusListsAsync()` method outputs a JSON file containing the Base64-encoded ZLIB-compressed bitstring.

## 4. Check Credential Status (The Verifier)

When a Verifier receives an SD-JWT Presentation, they must check if the credential has been revoked.

Because `SdJwt.Net.Oid4Vp` and `SdJwt.Net.HAIP` integrate seamlessly with the Status List package, this check occurs **automatically** during verification if the `StatusListService` is registered in the Verifier's Dependency Injection container.

```csharp
// In the Verifier application, use StatusListVerifier and your preferred cache strategy.
// Example package primitive:
var statusVerifier = new StatusListVerifier(httpClient);
```

When you call `verifier.VerifyPresentationAsync(response)`, the Verifier will:

1. Parse the `status_list` object from the presented SD-JWT.
2. Check if it already has an unexpired copy of that Status List URL in its cache.
3. If not, it fetches the compressed bitstring, decompresses it, and parses the bitfield.
4. It checks the specific index for the credential.
5. If the bit indicates `Revoked` or `Suspended`, the verification will fail with an error!

```csharp
var sdJwtResult = await verifier.VerifyPresentationAsync(response);

if (!sdJwtResult.IsValid)
{
    if (sdJwtResult.ErrorMessage.Contains("Revoked"))
    {
        return Results.Unauthorized("User credential has been revoked.");
    }
}
```
