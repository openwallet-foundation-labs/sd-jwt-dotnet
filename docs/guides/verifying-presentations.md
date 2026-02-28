# How to Verify Presentations

This guide demonstrates how to configure a Relying Party (Verifier) to request, receive, and securely verify an SD-JWT Verifiable Presentation from a User's Wallet.

Note: this guide focuses on end-to-end architecture. Some snippets are pseudocode for application wiring; for concrete APIs see `samples/SdJwt.Net.Samples`.

---

## Key Decisions

| Decision | Options | Guidance |
|----------|---------|----------|
| Trust model? | Static allow-list or Federation | Federation for 3+ issuers |
| Status check failure behavior? | Reject or step-up | Reject for high-risk flows |
| Cache TTL for status/trust? | Minutes to hours | Shorter for critical flows |
| Nonce binding? | Required or optional | Always required for production |
| HAIP enforcement? | None, Level 1, 2, 3 | Match issuer HAIP level |

---

## Prerequisites

Ensure your project references the necessary NuGet packages:

```bash
dotnet add package SdJwt.Net.Oid4Vp
dotnet add package SdJwt.Net.PresentationExchange
dotnet add package SdJwt.Net.HAIP
```

## 1. Configure the Verifier Service

Register verifier-related services in your dependency injection container.

```csharp
using SdJwt.Net.PresentationExchange;

var builder = WebApplication.CreateBuilder(args);

// Register services used by your verifier pipeline.
// Presentation Exchange provides a concrete registration helper:
builder.Services.AddPresentationExchange();

var app = builder.Build();
```

## 2. Request Data (Presentation Exchange)

When a user clicks "Login" or "Verify Age" on your site, you must formulate a request telling their wallet exactly what data you need. We use the **DIF Presentation Exchange v2.1.1** format for this.

```csharp
using SdJwt.Net.Oid4Vp;

app.MapPost("/request-verification", async (/* your verifier service */ verifier) =>
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

    // Generate the OpenID4VP Authorization Request URI
    var requestUri = await verifier.CreatePresentationRequestAsync(new PresentationRequestOptions
    {
        PresentationDefinition = definition,
        CallbackUri = "https://verifier.example.com/api/callback", // Where the wallet POSTs the token
        Nonce = Guid.NewGuid().ToString() // Crucial for preventing replay attacks!
    });

    // Return the URI to the frontend to render as a QR code or universal link
    return Results.Ok(new { request_uri = requestUri });
});
```

## 3. Verify the Submission

Once the user approves the request in their wallet, the wallet will `POST` the massive SD-JWT string back to your `CallbackUri`.

```csharp
app.MapPost("/api/callback", async (
    PresentationResponse response, 
    /* your verifier service */ verifier,
    IPresentationExchangeService peService) =>
{
    try 
    {
        // 1. Core verification: 
        // - Fetches Issuer Public Keys (e.g. via .well-known/jwks.json or Federation)
        // - Verifies the cryptographic signature
        // - Hashes the presented disclosures and ensures they match the payload
        // - Evaluates the Key Binding JWT against the Nonce and Audience
        var sdJwtResult = await verifier.VerifyPresentationAsync(response);
        
        if (!sdJwtResult.IsValid)
        {
            return Results.BadRequest($"Invalid Token: {sdJwtResult.ErrorMessage}");
        }

        // 2. Logic verification:
        // - Did the user actually provide the GPA we asked for?
        // - Was the GPA >= 3.0?
        var peResult = await peService.EvaluatePresentationAsync(
            sdJwtResult.Claims, // Only the safely revealed claims!
            expectedDefinition // Retrieve the definition we saved in Step 2
        );

        if (!peResult.IsValid)
        {
            return Results.BadRequest($"Presentation Exchange Failed: {peResult.ErrorMessage}");
        }

        // 3. Success! Consume the data.
        var gpa = peResult.MatchedClaims["university_degree_requirement"]["gpa"];
        return Results.Ok($"Successfully hired candidate with GPA: {gpa}");
    }
    catch (Exception ex)
    {
        // Invalid signatures, expired tokens, HAIP policy failures...
        return Results.Unauthorized(); 
    }
});
```

## Security Note on Trust

In the above code, `verifier.VerifyPresentationAsync` will inherently trust any issuer that has a valid public key. In production, you must combine this with [OpenID Federation Trust Chains](establishing-trust.md) to ensure the Issuer is actually authorized to issue University Degrees!
