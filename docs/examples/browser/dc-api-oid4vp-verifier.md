# DC API + OID4VP Verifier Integration

| Field     | Value                                              |
| --------- | -------------------------------------------------- |
| Level     | Advanced                                           |
| Maturity  | Spec-tracking (DC API is a W3C draft)              |
| Runnable  | Conceptual (backend C# + frontend JavaScript)      |
| Packages  | SdJwt.Net.Oid4Vp, SdJwt.Net.PresentationExchange   |
| Standards | OpenID4VP 1.0, W3C Digital Credentials API (draft) |

> **Spec-tracking notice:** The W3C Digital Credentials API is an active draft. Browser support varies and the API surface may change. This example tracks the current specification shape and will be updated as the standard evolves.

This example demonstrates a browser-based credential presentation using the Digital Credentials API:

1. **Backend** builds a DC API request with DCQL or PEX query.
2. **Frontend** calls `navigator.credentials.get()` to invoke the wallet.
3. **Wallet** returns the credential response.
4. **Backend** validates the response using `DcApiResponseValidator`.

---

## 1. Backend: Build a DC API Request

```csharp
using SdJwt.Net.Oid4Vp.DcApi;
using SdJwt.Net.Oid4Vp.Models.Dcql;
using SdJwt.Net.Oid4Vp.Verifier;

// Build a DCQL-based request for an identity credential
var dcqlQuery = new DcqlQuery
{
    Credentials = new[]
    {
        new DcqlCredentialQuery
        {
            Id = "identity_credential",
            Format = "vc+sd-jwt",
            Meta = new SdJwtVcMeta
            {
                VctValues = new[] { "https://credentials.example.com/identity_credential" }
            },
            Claims = new[]
            {
                new DcqlClaimsQuery { Path = new[] { "given_name" } },
                new DcqlClaimsQuery { Path = new[] { "family_name" } },
                new DcqlClaimsQuery
                {
                    Path = new[] { "birthdate" },
                    IntentToRetain = false
                }
            }
        }
    }
};

var requestBuilder = new DcApiRequestBuilder();
var dcApiRequest = requestBuilder.Build(
    dcqlQuery,
    DcApiRequestType.SdJwtVc,
    verifierOrigin: "https://verifier.example.com",
    nonce: Guid.NewGuid().ToString("N"));

// Serialize the request for the frontend
string requestJson = System.Text.Json.JsonSerializer.Serialize(dcApiRequest);
// Return requestJson to the frontend via your API endpoint
```

---

## 2. Frontend: Invoke the Wallet via DC API

```javascript
// Browser-side JavaScript
// Receives dcApiRequest from the backend

async function requestCredential(dcApiRequest) {
  try {
    const credential = await navigator.credentials.get({
      digital: {
        requests: [
          {
            protocol: "openid4vp",
            data: JSON.stringify(dcApiRequest),
          },
        ],
      },
    });

    // Send the wallet's response back to the backend for validation
    const response = await fetch("/api/verify-presentation", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({
        token: credential.data,
        protocol: credential.protocol,
      }),
    });

    const result = await response.json();
    console.log("Verification result:", result);
  } catch (error) {
    if (error.name === "NotAllowedError") {
      console.log("User declined the credential request.");
    } else {
      console.error("DC API error:", error);
    }
  }
}
```

---

## 3. Backend: Validate the DC API Response

```csharp
using SdJwt.Net.Oid4Vp.DcApi;
using SdJwt.Net.Oid4Vp.DcApi.Models;

var responseValidator = new DcApiResponseValidator();

var validationResult = await responseValidator.ValidateAsync(
    new DcApiResponse { Token = receivedToken },
    new DcApiValidationOptions
    {
        ExpectedNonce = originalNonce,
        ExpectedOrigin = "https://verifier.example.com",
        IssuerSigningKeys = trustedIssuerKeys,
        DcqlQuery = dcqlQuery
    });

Console.WriteLine($"Response valid: {validationResult.IsValid}");

if (validationResult.IsValid)
{
    foreach (var credential in validationResult.Credentials)
    {
        Console.WriteLine($"  Credential: {credential.Format}");
        // Access disclosed claims
    }
}
```

---

## 4. Origin Validation

The DC API binds the request to the verifier's web origin. Validate this server-side:

```csharp
var originValidator = new DcApiOriginValidator();
bool originValid = originValidator.Validate(
    receivedOrigin: "https://verifier.example.com",
    expectedOrigin: "https://verifier.example.com");

Console.WriteLine($"Origin valid: {originValid}");
// Output: Origin valid: True
```

---

## Architecture

```
Browser (Verifier Site)           Wallet App             Backend (Verifier)
        |                            |                        |
        |  1. GET /request           |                        |
        |-----------------------------------------------------+
        |                            |    2. DC API request   |
        |  3. navigator.credentials  |                        |
        |     .get({ digital })      |                        |
        |--------------------------->|                        |
        |                            | 4. User consents       |
        |  5. credential.data        |                        |
        |<---------------------------|                        |
        |  6. POST /verify           |                        |
        |-----------------------------------------------------+
        |                            |    7. Validate response|
        |  8. Result                 |                        |
        |<----------------------------------------------------+
```

---

## Expected Outcomes

| Step                    | Result                                    |
| ----------------------- | ----------------------------------------- |
| Build DC API request    | DCQL query with verifier origin and nonce |
| Browser invokes wallet  | User sees credential selection UI         |
| Wallet returns response | VP token with selected disclosures        |
| Backend validates       | Signature, nonce, origin, DCQL match      |

---

## Related

- [Issuer - Wallet - Verifier](../credential-lifecycle/issuer-wallet-verifier.md) -- non-browser credential flow
- [W3C Digital Credentials API](https://wicg.github.io/digital-credentials/)
- [OpenID4VP specification](https://openid.net/specs/openid-4-verifiable-presentations-1_0.html)
