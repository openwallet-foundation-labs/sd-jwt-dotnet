# Tutorial: OpenID4VP

Implement credential presentation using the OpenID for Verifiable Presentations protocol.

**Time:** 20 minutes  
**Level:** Intermediate  
**Sample:** `samples/SdJwt.Net.Samples/02-Intermediate/04-OpenId4Vp.cs`

## What you will learn

- OpenID4VP authorization request flow
- Presentation definition creation
- Response handling and validation

## Protocol overview

```text
┌────────┐                              ┌──────────┐
│ Wallet │                              │ Verifier │
└───┬────┘                              └────┬─────┘
    │                                        │
    │  1. Authorization Request              │
    │  (with presentation_definition)        │
    │ <──────────────────────────────────────│
    │                                        │
    │  2. User consent                       │
    │                                        │
    │  3. Authorization Response             │
    │  (with vp_token)                       │
    │ ──────────────────────────────────────>│
    │                                        │
    │  4. Verification                       │
    │                                        │
    └────────────────────────────────────────┘
```

## Step 1: Verifier creates request

```csharp
using SdJwt.Net.Oid4Vp.Models;
using SdJwt.Net.PresentationExchange.Models;

var authRequest = new AuthorizationRequest
{
    ResponseType = "vp_token",
    ClientId = "https://verifier.example.com",
    RedirectUri = "https://verifier.example.com/callback",
    Nonce = Guid.NewGuid().ToString(),
    State = "session-state-123",
    PresentationDefinition = new PresentationDefinition
    {
        Id = "employment-verification",
        InputDescriptors = new[]
        {
            new InputDescriptor
            {
                Id = "employee-credential",
                Name = "Employment Proof",
                Purpose = "Verify current employment",
                Constraints = new Constraints
                {
                    Fields = new[]
                    {
                        new Field
                        {
                            Path = new[] { "$.vct" },
                            Filter = new FieldFilter
                            {
                                Type = "string",
                                Const = "https://hr.example/EmployeeCredential"
                            }
                        },
                        new Field
                        {
                            Path = new[] { "$.employer_name" }
                        }
                    }
                }
            }
        }
    }
};
```

## Step 2: Send request to wallet

```csharp
// Option A: Same-device (deep link)
var requestUri = $"openid4vp://?{BuildQueryString(authRequest)}";

// Option B: Cross-device (QR code)
var qrContent = $"openid4vp://?request_uri={Uri.EscapeDataString(hostedRequestUri)}";
```

## Step 3: Wallet processes request

```csharp
// Parse authorization request
var request = ParseAuthorizationRequest(requestUri);

// Find matching credentials in wallet
var matchingCredentials = wallet.FindCredentials(request.PresentationDefinition);

// User selects which credentials to present
var selectedCredential = matchingCredentials.First();

// Create selective presentation
var holder = new SdJwtHolder(selectedCredential);
var presentation = holder.CreatePresentation(
    d => d.ClaimName == "employer_name",  // Only required fields
    kbJwtPayload: new JwtPayload
    {
        ["aud"] = request.ClientId,
        ["nonce"] = request.Nonce
    },
    kbJwtSigningKey: holderKey,
    kbJwtSigningAlgorithm: SecurityAlgorithms.EcdsaSha256
);
```

## Step 4: Wallet sends response

```csharp
var authResponse = new AuthorizationResponse
{
    VpToken = presentation,
    PresentationSubmission = new PresentationSubmission
    {
        Id = Guid.NewGuid().ToString(),
        DefinitionId = request.PresentationDefinition.Id,
        DescriptorMap = new[]
        {
            new DescriptorMapEntry
            {
                Id = "employee-credential",
                Format = "vc+sd-jwt",
                Path = "$"
            }
        }
    },
    State = request.State
};

// POST to redirect_uri
await httpClient.PostAsync(request.RedirectUri, authResponse);
```

## Step 5: Verifier validates response

```csharp
// Validate state matches
if (response.State != expectedState)
    throw new SecurityException("State mismatch");

// Verify the SD-JWT presentation
var verifier = new SdVerifier(ResolveIssuerKey);
var result = await verifier.VerifyAsync(
    response.VpToken,
    sdJwtParams,
    kbJwtParams,
    expectedNonce
);

// Check presentation submission matches definition
ValidatePresentationSubmission(
    response.PresentationSubmission,
    originalRequest.PresentationDefinition
);

// Extract verified claims
var employerName = result.ClaimsPrincipal.FindFirst("employer_name")?.Value;
Console.WriteLine($"Verified employment at: {employerName}");
```

## Response modes

### Direct post

Response sent directly to verifier backend:

```csharp
var request = new AuthorizationRequest
{
    ResponseMode = "direct_post",
    ResponseUri = "https://verifier.example.com/response"
};
```

### Fragment

Response in URL fragment (same-device):

```csharp
var request = new AuthorizationRequest
{
    ResponseMode = "fragment",
    RedirectUri = "https://verifier.example.com/callback"
};
```

## Multiple credentials

Request multiple credentials at once:

```csharp
var definition = new PresentationDefinition
{
    Id = "full-verification",
    InputDescriptors = new[]
    {
        new InputDescriptor { Id = "id-credential", ... },
        new InputDescriptor { Id = "address-credential", ... },
        new InputDescriptor { Id = "employment-credential", ... }
    }
};
```

## Run the sample

```bash
cd samples/SdJwt.Net.Samples
dotnet run -- 2.4
```

## Next steps

- [Presentation Exchange](05-presentation-exchange.md) - Advanced query syntax
- [OpenID Federation](../advanced/01-openid-federation.md) - Trust establishment

## Key takeaways

1. OpenID4VP standardizes credential presentation
2. Presentation definitions specify required credentials
3. Wallet creates selective disclosures based on requirements
4. Nonces prevent replay attacks
