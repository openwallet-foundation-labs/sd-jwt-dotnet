# How to build an OID4VP verifier pipeline

| Field                | Value                                                                                                   |
| -------------------- | ------------------------------------------------------------------------------------------------------- |
| **Package maturity** | Stable (SdJwt.Net.Oid4Vp, SdJwt.Net.PresentationExchange)                                               |
| **Code status**      | Mixed -- runnable package APIs with illustrative service wiring                                         |
| **Related concept**  | [Verifiable Credentials](../concepts/verifiable-credentials.md), [HAIP](../concepts/haip-compliance.md) |
| **Related tutorial** | [Tutorials](../tutorials/index.md)                                                                      |

|                      |                                                                                                                                                                                                                                                                                          |
| -------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Audience**         | Developers building relying-party (verifier) services using ASP.NET Core.                                                                                                                                                                                                                |
| **Purpose**          | Walk through creating a Presentation Exchange request, receiving wallet responses via OID4VP, and performing three-layer verification (cryptographic, PEX constraint, business rule).                                                                                                    |
| **Scope**            | Verifier DI setup, Presentation Exchange definition, authorization request generation, callback verification, and trust considerations. Out of scope: issuance (see [Issuing Credentials](issuing-credentials.md)), trust chain setup (see [Establishing Trust](establishing-trust.md)). |
| **Success criteria** | Reader can build a PEX-based credential request, verify the SD-JWT presentation response, and extract matched claims for business logic.                                                                                                                                                 |

## What your application still owns

This guide does not provide: production key custody, user authentication, authorization policy, trust onboarding, certified wallet functionality, UX and consent screens, durable audit storage, monitoring and incident response, or legal/regulatory compliance review.

## Verifier pipeline overview

A typical OID4VP verifier pipeline follows these stages:

```
OID4VP response --> protocol validation --> nonce/audience check
  --> SD-JWT or mdoc cryptographic verification --> holder binding
  --> PEX constraint matching --> status list check
  --> trust chain resolution --> business-level decision
```

> Some snippets are architecture-level pseudocode. For concrete APIs, see `samples/SdJwt.Net.Samples`.

---

## Key decisions

| Decision                       | Options                         | Guidance                       |
| ------------------------------ | ------------------------------- | ------------------------------ |
| Trust model?                   | Static allow-list or Federation | Federation for 3+ issuers      |
| Status check failure behavior? | Reject or step-up               | Reject for high-risk flows     |
| Cache TTL for status/trust?    | Minutes to hours                | Shorter for critical flows     |
| Nonce binding?                 | Required or optional            | Always required for production |

---

## Prerequisites

Ensure your project references the necessary NuGet packages:

```bash
dotnet add package SdJwt.Net.Oid4Vp
dotnet add package SdJwt.Net.PresentationExchange
```

## 1. Configure the verifier service

Register verifier-related services in your dependency injection container.

```csharp
using SdJwt.Net.PresentationExchange;

var builder = WebApplication.CreateBuilder(args);

// Register services used by your verifier pipeline.
// Presentation Exchange provides a concrete registration helper:
builder.Services.AddPresentationExchange();

var app = builder.Build();
```

## 2. Request data (Presentation Exchange)

When a user clicks "Login" or "Verify Age" on your site, you must formulate a request telling their wallet exactly what data you need. This guide uses the **DIF Presentation Exchange v2.1.1** format.

```csharp
using SdJwt.Net.Oid4Vp;
using SdJwt.Net.Oid4Vp.Verifier;

app.MapPost("/request-verification", async (/* your verifier service */ HttpContext context) =>
{
    // Define precisely what you need from the user's wallet
    var definition = new PresentationDefinition
    {
        Id = Guid.NewGuid().ToString(),
        InputDescriptors = new[]
        {
            new InputDescriptor
            {
                Id = "university_degree_requirement",
                Constraints = new Constraints
                {
                    Fields = new[]
                    {
                        // 1. Must be a University Degree
                        new Field
                        {
                            Path = new[] { "$.vc.type" },
                            Filter = new { contains = new { @const = "UniversityDegreeCredential" } }
                        },
                        // 2. We specifically require them to reveal their GPA, and it must be >= 3.0
                        new Field
                        {
                            Path = new[] { "$.vc.credentialSubject.gpa" },
                            Filter = new { type = "number", minimum = 3.0 }
                        }
                    }
                }
            }
        }
    };

    // Build and return the OID4VP Authorization Request URI
    // (your service is responsible for generating the nonce and storing state)
    var nonce = Guid.NewGuid().ToString();
    // Return definition and nonce to the frontend to render as a QR code or universal link
    return Results.Ok(new { presentation_definition = definition, nonce });
});
```

## 3. Verify the submission

Once the user approves the request in their wallet, the wallet will `POST` the SD-JWT string back to your `CallbackUri`.

```csharp
app.MapPost("/api/callback", async (
    AuthorizationResponse response) =>
{
    try
    {
        // Create a VpTokenValidator with your issuer key resolver
        var vpTokenValidator = new VpTokenValidator(
            keyProvider: async jwt => await FetchIssuerKey(jwt),
            useSdJwtVcValidation: true);

        // Configure validation options
        var options = new VpTokenValidationOptions
        {
            RequireKeyBinding = true,
            ValidateIssuer = true,
            ValidIssuers = new[] { "https://trusted-issuer.example.com" },
            ValidateKeyBindingAudience = true,
            ValidKeyBindingAudiences = new[] { "https://verifier.example.com" },
            ValidateKeyBindingLifetime = true,
            ValidateKeyBindingFreshness = true,
            MaxKeyBindingAge = TimeSpan.FromMinutes(10)
        };

        var result = await vpTokenValidator.ValidateAsync(
            response,
            expectedNonce: savedNonce,
            options);

        // Result is VpTokenValidationResult
        // Access verified claims via ClaimsPrincipal
        var gpa = result.ClaimsPrincipal.FindFirst("gpa")?.Value;
        return Results.Ok($"Successfully verified candidate with GPA: {gpa}");
    }
    catch (Exception ex)
    {
        // Invalid signatures, expired tokens, etc.
        return Results.Unauthorized();
    }
});
```

## 4. Optional: HAIP Final profile validation

For high-assurance verifier deployments, validate that the verifier supports the selected HAIP Final flow and credential profile before accepting traffic.

```bash
dotnet add package SdJwt.Net.HAIP
```

var haipOptions = new HaipProfileOptions();
haipOptions.Flows.Add(HaipFlow.Oid4VpRedirectPresentation);
haipOptions.CredentialProfiles.Add(HaipCredentialProfile.SdJwtVc);
haipOptions.SupportedCredentialFormats.Add(HaipConstants.SdJwtVcFormat);
haipOptions.SupportedJoseAlgorithms.Add(HaipConstants.RequiredJoseAlgorithm);
haipOptions.SupportedHashAlgorithms.Add(HaipConstants.RequiredHashAlgorithm);
haipOptions.SupportsDcql = true;
haipOptions.SupportsSignedPresentationRequests = true;
haipOptions.ValidatesVerifierAttestation = true;
haipOptions.SupportsSdJwtVcCompactSerialization = true;
haipOptions.UsesCnfJwkForSdJwtVcHolderBinding = true;
haipOptions.RequiresKbJwtForHolderBoundSdJwtVc = true;
haipOptions.SupportsStatusListClaim = true;
haipOptions.SupportsSdJwtVcIssuerX5c = true;

var haipResult = new HaipProfileValidator().Validate(haipOptions);
if (!haipResult.IsCompliant)
{
throw new InvalidOperationException("Verifier configuration does not meet the selected HAIP Final profile.");
}

```

## Security note on trust

In the above code, `verifier.VerifyPresentationAsync` will trust any issuer that has a valid public key. In production, combine this with [OpenID Federation Trust Chains](establishing-trust.md) to ensure the issuer is authorized to issue University Degrees.

## HAIP enforcement (key decisions table)

| Decision                       | Options                                        | Guidance                                        |
| ------------------------------ | ---------------------------------------------- | ----------------------------------------------- |
| HAIP enforcement?              | None, OID4VP redirect, DC API, SD-JWT VC, mdoc | Optional hardening; match the selected flow     |
```
