# How to Manage Credential Revocation

This guide demonstrates how to configure and use the `SdJwt.Net.StatusList` package to implement privacy-preserving credential revocation.

Unlike traditional JWTs, Verifiable Credentials (VCs) are often long-lived and held independently in user wallets. If a driver's license is suspended, the Issuer cannot simply "delete" the credential from the user's phone.

To solve this, Issuers publish heavily compressed **Status Lists** (bitstrings) describing the current state (Valid, Revoked, Suspended) of millions of credentials. Verifiers download these lists to check a credential's status during presentation.

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
using SdJwt.Net.StatusList;

var builder = WebApplication.CreateBuilder(args);

// Configure the Status List Service
builder.Services.AddSdJwtStatusList(options =>
{
    // The base URL where your Status Lists will be hosted
    options.BaseUrl = "https://status.example.com";
    
    // The size of the bitfield (e.g., 131,072 entries)
    options.StatusListSize = 131072;
    
    // How often Verifiers should fetch a new list
    options.RefreshInterval = TimeSpan.FromHours(1);
});

var app = builder.Build();
```

## 2. Issue a Credential with Status (The Issuer)

When issuing a credential, you must bind it to a specific index in a specific Status List.

```csharp
app.MapPost("/issue-employee-id", async (
    CredentialRequest request, 
    ISdJwtIssuerService issuer,
    ISdJwtStatusListService statusList) =>
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
    ISdJwtStatusListService statusList) =>
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
// In the Verifier's Program.cs
builder.Services.AddSdJwtVerifier(options => { /* ... */ });

// Tell the Verifier how to handle Status Lists
builder.Services.AddSdJwtStatusListClient(options =>
{
    // Use Redis to aggressively cache Status Lists so we aren't
    // hammering the Issuer's CDN on every login
    options.UseDistributedCache();
});
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
