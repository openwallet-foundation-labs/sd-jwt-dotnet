# How to build an SD-JWT VC issuer service

| Field                | Value                                                                                            |
| -------------------- | ------------------------------------------------------------------------------------------------ |
| **Package maturity** | Stable (SdJwt.Net.Vc) + Spec-tracking (SdJwt.Net.Oid4Vci)                                        |
| **Code status**      | Mixed -- runnable package APIs with illustrative service wiring                                  |
| **Related concept**  | [Verifiable Credentials](../concepts/verifiable-credentials.md), [SD-JWT](../concepts/sd-jwt.md) |
| **Related tutorial** | [Tutorials](../tutorials/README.md)                                                              |

|                      |                                                                                                                                                                                                                                                                       |
| -------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Audience**         | Developers building issuer services using ASP.NET Core.                                                                                                                                                                                                               |
| **Purpose**          | Walk through configuring an issuer endpoint, building a selectively disclosable VC payload, adding holder binding and status references, and returning OID4VCI-compliant responses.                                                                                   |
| **Scope**            | Issuer DI setup, key configuration, `SdJwtVcIssuer` and `SdJwtVcPayload` usage, and under-the-hood mechanics. Out of scope: verification (see [Verifying Presentations](verifying-presentations.md)), revocation (see [Managing Revocation](managing-revocation.md)). |
| **Success criteria** | Reader can stand up an issuer endpoint that signs SD-JWT VCs with selective claims and OID4VCI response formatting.                                                                                                                                                   |

## What your application still owns

This guide does not provide: production key custody, user authentication, authorization policy, trust onboarding, certified wallet functionality, UX and consent screens, durable audit storage, monitoring and incident response, or legal/regulatory compliance review.

---

## Key decisions

| Decision                                      | Options                                        | Guidance                                      |
| --------------------------------------------- | ---------------------------------------------- | --------------------------------------------- |
| Which claims to make selectively disclosable? | Any PII or sensitive data                      | Minimize always-visible claims for privacy    |
| Credential validity period?                   | Hours to years                                 | Shorter for high-risk credentials             |
| Revocation support?                           | Yes/No                                         | Yes for long-lived credentials                |
| Key storage?                                  | Software, HSM, cloud KMS                       | HSM for production environments               |
| HAIP Final profile?                           | None, OID4VCI, OID4VP, DC API, SD-JWT VC, mdoc | Optional hardening; add after core flow works |

---

## Prerequisites

Ensure your project references the necessary NuGet packages:

```bash
dotnet add package SdJwt.Net.Oid4Vci
dotnet add package SdJwt.Net.Vc
```

## 1. Configure the issuer service

Register the Issuer service in your Dependency Injection container (`Program.cs` or `Startup.cs`). This requires setting up your cryptographic signing keys and defining the credential types your service supports.

```csharp
using SdJwt.Net.Vc.Issuer;
using SdJwt.Net.Vc.Models;
using SdJwt.Net.Issuer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

var builder = WebApplication.CreateBuilder(args);

// Generate or load your private signing key (e.g., from Azure Key Vault)
var ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
var issuerSigningKey = new ECDsaSecurityKey(ecdsa);

// Create the issuer instance directly (or register in DI as a singleton)
var vcIssuer = new SdJwtVcIssuer(issuerSigningKey, SecurityAlgorithms.EcdsaSha256);

var app = builder.Build();
```

## 2. Issue a credential

Once OID4VCI handles the OAuth 2.0 component (e.g., the wallet exchanging an authorization code for an access token), your business logic needs to build the actual credential payload.

Use `SdJwtVcIssuer` to construct the payload and issue the credential, explicitly deciding which claims are always visible and which are selectively disclosable.

```csharp
app.MapPost("/issue-degree", (CredentialRequest request) =>
{
    // 1. Build the SD-JWT VC payload
    var vcPayload = new SdJwtVcPayload
    {
        Issuer = "https://issuer.example.com",
        Subject = $"did:example:student:{request.UserId}",
        IssuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
        ExpiresAt = DateTimeOffset.UtcNow.AddYears(5).ToUnixTimeSeconds(),

        // Domain-specific claims via AdditionalData
        AdditionalData = new Dictionary<string, object>
        {
            // Public information (always visible to any verifier)
            ["degreeName"] = "Bachelor of Science in Computer Science",
            ["university"] = "Example University",

            // Private information (holder must explicitly consent to reveal these)
            ["gpa"] = "3.8",
            ["graduationDate"] = "2023-05-15",
            ["honors"] = "Summa Cum Laude"
        }
    };

    // 2. Configure selective disclosure: true = selectively disclosable
    var options = new SdIssuanceOptions
    {
        DisclosureStructure = new
        {
            gpa = true,
            graduationDate = true,
            honors = true
            // degreeName and university stay always visible
        }
    };

    // 3. Issue the SD-JWT VC
    var result = vcIssuer.Issue(
        "UniversityDegreeCredential",
        vcPayload,
        options,
        holderPublicJwk);  // optional holder binding key

    // 4. Return the standard OID4VCI response to the wallet
    return Results.Ok(new
    {
        credential = result.Issuance, // The {JWT}~{disc1}~{disc2} string
        format = "dc+sd-jwt"
    });
});
```

## What happens under the hood?

When `vcIssuer.Issue()` runs:

1. **Salting and hashing:** For `gpa`, `graduationDate`, and `honors`, the `SdJwt.Net` core generates high-entropy salts, creates disclosure strings, hashes them via SHA-256, and places the hashes into the JWT's `_sd` array.
2. **Decoys:** Decoy hashes are injected to prevent observers from guessing how many claims the user has.
3. **Signing:** The core JWT is signed using your ECDSA key.

## Optional: HAIP Final profile validation

If your deployment requires HAIP compliance, add the HAIP package and validate before accepting traffic:

```bash
dotnet add package SdJwt.Net.HAIP
```

```csharp
using SdJwt.Net.HAIP;
using SdJwt.Net.HAIP.Validators;

var haipOptions = new HaipProfileOptions();
haipOptions.Flows.Add(HaipFlow.Oid4VciIssuance);
haipOptions.CredentialProfiles.Add(HaipCredentialProfile.SdJwtVc);
haipOptions.SupportedCredentialFormats.Add(HaipConstants.SdJwtVcFormat);
haipOptions.SupportedJoseAlgorithms.Add(HaipConstants.RequiredJoseAlgorithm);
haipOptions.SupportedHashAlgorithms.Add(HaipConstants.RequiredHashAlgorithm);
haipOptions.SupportsAuthorizationCodeFlow = true;
haipOptions.EnforcesPkceS256 = true;
haipOptions.SupportsPushedAuthorizationRequests = true;
haipOptions.SupportsDpop = true;
haipOptions.SupportsDpopNonce = true;
haipOptions.ValidatesWalletAttestation = true;
haipOptions.ValidatesKeyAttestation = true;
haipOptions.SupportsSdJwtVcCompactSerialization = true;
haipOptions.UsesCnfJwkForSdJwtVcHolderBinding = true;
haipOptions.RequiresKbJwtForHolderBoundSdJwtVc = true;
haipOptions.SupportsStatusListClaim = true;
haipOptions.SupportsSdJwtVcIssuerX5c = true;

var haipResult = new HaipProfileValidator().Validate(haipOptions);
if (!haipResult.IsCompliant)
{
    throw new InvalidOperationException(
        "Issuer configuration does not meet the selected HAIP Final profile.");
}
```

## Next steps

Now that the wallet holds the credential, learn how a Relying Party verifies it in the [Verifying Presentations Guide](verifying-presentations.md).
