# Status List Lifecycle

| Field     | Value                                          |
| --------- | ---------------------------------------------- |
| Level     | Intermediate                                   |
| Maturity  | Stable                                         |
| Runnable  | Conceptual (paste into a console app)          |
| Packages  | SdJwt.Net.Vc, SdJwt.Net.StatusList             |
| Standards | SD-JWT VC draft-16, Token Status List draft-20 |

This example demonstrates the credential status lifecycle:

1. **Issue** a credential with a status list reference.
2. **Publish** the status list token.
3. **Verify** the credential is active.
4. **Revoke** the credential.
5. **Verify** the credential is now rejected.

---

## 1. Setup: Create a Status List

```csharp
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.StatusList.Issuer;
using SdJwt.Net.StatusList.Models;

var issuerKey = ECDsa.Create(ECCurve.NamedCurves.nistP256);
var signingKey = new ECDsaSecurityKey(issuerKey);

// Create a status list manager
// The list tracks credential status at assigned indices
var statusListManager = new StatusListManager(new StatusListManagerOptions
{
    Issuer = "https://issuer.example.com",
    StatusListUri = "https://issuer.example.com/.well-known/status-list",
    SigningKey = signingKey,
    Algorithm = SecurityAlgorithms.EcdsaSha256,
    BitsPerStatus = 2 // Supports: 0=valid, 1=revoked, 2=suspended
});
```

---

## 2. Issue Credential with Status Reference

When issuing a credential, embed a `status` claim pointing to the issuer's status list.

```csharp
// Allocate an index in the status list for this credential
int credentialIndex = statusListManager.AllocateIndex();

// The status reference to embed in the SD-JWT VC
var statusReference = new StatusListReference
{
    Uri = "https://issuer.example.com/.well-known/status-list",
    Index = credentialIndex
};

// Include the status claim when issuing the SD-JWT VC
// (See issuer-wallet-verifier.md for full issuance flow)
var credentialClaims = new Dictionary<string, object>
{
    ["vct"] = "https://credentials.example.com/identity_credential",
    ["iss"] = "https://issuer.example.com",
    ["iat"] = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
    ["given_name"] = "Alice",
    ["status"] = new Dictionary<string, object>
    {
        ["status_list"] = new Dictionary<string, object>
        {
            ["idx"] = credentialIndex,
            ["uri"] = statusReference.Uri
        }
    }
};

Console.WriteLine($"Credential issued at status index: {credentialIndex}");
```

---

## 3. Publish the Status List Token

The issuer publishes the status list as a signed JWT at the well-known URI.

```csharp
// Generate the signed status list token
string statusListToken = statusListManager.CreateStatusListToken();

// Serve this token at: GET https://issuer.example.com/.well-known/status-list
// Content-Type: application/statuslist+jwt
Console.WriteLine("Status list token published.");
```

---

## 4. Verify Credential Status (Active)

```csharp
using SdJwt.Net.StatusList.Verifier;

var statusVerifier = new StatusListVerifier();

var statusResult = await statusVerifier.CheckAsync(
    statusListToken,
    credentialIndex,
    new StatusListOptions
    {
        IssuerSigningKey = new ECDsaSecurityKey(
            ECDsa.Create(issuerKey.ExportParameters(false)))
    });

Console.WriteLine($"Status check result: {statusResult.Status}");
// Output: Status check result: Valid
Console.WriteLine($"Is active: {statusResult.IsActive}");
// Output: Is active: True
```

---

## 5. Revoke the Credential

```csharp
// Issuer revokes the credential by updating the status list
statusListManager.SetStatus(credentialIndex, StatusType.Revoked);

// Re-publish the updated status list token
string updatedStatusListToken = statusListManager.CreateStatusListToken();
Console.WriteLine("Credential revoked. Updated status list published.");
```

---

## 6. Verify After Revocation (Rejected)

```csharp
var revokedResult = await statusVerifier.CheckAsync(
    updatedStatusListToken,
    credentialIndex,
    new StatusListOptions
    {
        IssuerSigningKey = new ECDsaSecurityKey(
            ECDsa.Create(issuerKey.ExportParameters(false)))
    });

Console.WriteLine($"Status after revocation: {revokedResult.Status}");
// Output: Status after revocation: Revoked
Console.WriteLine($"Is active: {revokedResult.IsActive}");
// Output: Is active: False
```

---

## Status Values

| Value | Meaning   | Description                          |
| ----- | --------- | ------------------------------------ |
| 0     | Valid     | Credential is active                 |
| 1     | Revoked   | Permanently invalidated              |
| 2     | Suspended | Temporarily inactive, can be resumed |

---

## Expected Outcomes

| Step               | Result                                 |
| ------------------ | -------------------------------------- |
| Issue with status  | Credential includes status reference   |
| Check active       | `IsActive = true`                      |
| Revoke             | Status list updated at index           |
| Check after revoke | `IsActive = false`, `Status = Revoked` |

---

## Related

- [Issuer - Wallet - Verifier](issuer-wallet-verifier.md) -- full issuance and presentation flow
- [Federated Trust Verification](federated-trust-verification.md) -- resolve issuer trust
- [Token Status List spec](https://www.ietf.org/archive/id/draft-ietf-oauth-status-list-20.html)
