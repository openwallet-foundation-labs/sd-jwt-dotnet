# How to issue verifiable credentials

|                      |                                                                                                                                                                                                                                                                                         |
| -------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Audience**         | Developers building issuer services using ASP.NET Core.                                                                                                                                                                                                                                 |
| **Purpose**          | Walk through configuring an issuer endpoint, building a selectively disclosable VC payload, and returning OID4VCI-compliant responses with HAIP enforcement.                                                                                                                            |
| **Scope**            | Issuer DI setup, key configuration, `VerifiableCredentialBuilder` usage, HAIP policy enforcement, and under-the-hood mechanics. Out of scope: verification (see [Verifying Presentations](verifying-presentations.md)), revocation (see [Managing Revocation](managing-revocation.md)). |
| **Success criteria** | Reader can stand up an issuer endpoint that signs SD-JWT VCs with selective claims, HAIP Final capability validation, and OID4VCI response formatting.                                                                                                                                  |

---

## Key decisions

| Decision                                      | Options                                        | Guidance                                          |
| --------------------------------------------- | ---------------------------------------------- | ------------------------------------------------- |
| Which claims to make selectively disclosable? | Any PII or sensitive data                      | Minimize always-visible claims for privacy        |
| Credential validity period?                   | Hours to years                                 | Shorter for high-risk credentials                 |
| Revocation support?                           | Yes/No                                         | Yes for long-lived credentials                    |
| Key storage?                                  | Software, HSM, cloud KMS                       | HSM for production environments                   |
| HAIP Final profile?                           | None, OID4VCI, OID4VP, DC API, SD-JWT VC, mdoc | Use OID4VCI plus SD-JWT VC for SD-JWT VC issuance |

---

## Prerequisites

Ensure your project references the necessary NuGet packages:

```bash
dotnet add package SdJwt.Net.Oid4Vci
dotnet add package SdJwt.Net.Vc
dotnet add package SdJwt.Net.HAIP
```

## 1. Configure the issuer service

Register the Issuer service in your Dependency Injection container (`Program.cs` or `Startup.cs`). This requires setting up your cryptographic signing keys and defining the credential types your service supports.

```csharp
using SdJwt.Net.Oid4Vci;
using SdJwt.Net.HAIP;
using SdJwt.Net.HAIP.Validators;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

var builder = WebApplication.CreateBuilder(args);

// Generate or load your private signing key (e.g., from Azure Key Vault)
var ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
var issuerSigningKey = new ECDsaSecurityKey(ecdsa);

builder.Services.AddSdJwtIssuer(options =>
{
    options.IssuerUrl = "https://issuer.example.com";
    options.SigningKey = issuerSigningKey;

    // Define the JSON schema types this issuer supports
    options.SupportedCredentialTypes = new[]
    {
        "UniversityDegreeCredential",
        "EmployeeIdCredential"
    };

    // HAIP Final SD-JWT VC profile uses the dc+sd-jwt format identifier.
    options.SupportedFormats = new[] { HaipConstants.SdJwtVcFormat };
});

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
    throw new InvalidOperationException("Issuer configuration does not meet the selected HAIP Final profile.");
}

var app = builder.Build();
```

## 2. Issue a credential

Once OID4VCI handles the OAuth 2.0 component (e.g., the wallet exchanging an authorization code for an access token), your business logic needs to build the actual credential payload.

Use the `VerifiableCredentialBuilder` to construct the W3C payload, explicitly deciding which claims are plain-text and which are `SelectiveDisclosure` (hidden behind a salt/hash).

```csharp
using SdJwt.Net.Vc;

app.MapPost("/issue-degree", async (
    CredentialRequest request,
    ISdJwtIssuerService issuer) =>
{
    // 1. Build the W3C Verifiable Credential Payload
    var vcBuilder = new VerifiableCredentialBuilder()
        .WithType("UniversityDegreeCredential")
        .WithIssuer("https://issuer.example.com")
        .WithSubject($"did:example:student:{request.UserId}")
        .WithIssuanceDate(DateTimeOffset.UtcNow)
        .WithExpirationDate(DateTimeOffset.UtcNow.AddYears(5))

        // Public information (visible to any verifier)
        .WithCredentialSubject("degreeName", "Bachelor of Science in Computer Science")
        .WithCredentialSubject("university", "Example University")

        // Private information (Holder must explicitly consent to reveal these)
        .WithSelectiveCredentialSubject("gpa", "3.8")
        .WithSelectiveCredentialSubject("graduationDate", "2023-05-15")
        .WithSelectiveCredentialSubject("honors", "Summa Cum Laude");

    // 2. The ISdJwtIssuerService handles SD-JWT hashing and signing automatically.
    // It also enforces the configured HAIP policy.
    var credentialResult = await issuer.CreateCredentialAsync(vcBuilder);

    // 3. Return the standard OID4VCI response to the wallet
    return Results.Ok(new
    {
        credential = credentialResult.SdJwt, // The massive {JWT}~{disc1}~{disc2} string
        format = "dc+sd-jwt"
    });
});
```

## What happens under the hood?

When `issuer.CreateCredentialAsync()` runs:

1. **HAIP validation:** The service should fail closed if the configured OID4VCI and SD-JWT VC capabilities do not satisfy the selected HAIP Final profile.
2. **Salting and hashing:** For `gpa`, `graduationDate`, and `honors`, the `SdJwt.Net` core generates high-entropy salts, creates disclosure strings, hashes them via SHA-256, and places the hashes into the JWT's `_sd` array.
3. **Decoys:** Decoy hashes are injected to prevent observers from guessing how many claims the user has.
4. **Signing:** The core JWT is signed using your ECDSA key.

## Next steps

Now that the wallet holds the credential, learn how a Relying Party verifies it in the [Verifying Presentations Guide](verifying-presentations.md).
