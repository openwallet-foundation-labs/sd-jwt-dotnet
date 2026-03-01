# W3C Digital Credentials API Deep Dive

This document explains the W3C Digital Credentials API (DC API) integration in beginner-friendly terms and maps each concept to the implementation in this repository.

## Prerequisites

Before diving into DC API, you should understand these foundational concepts:

### What is the Digital Credentials API?

The **W3C Digital Credentials API** is a browser API that enables web applications to request verifiable credentials from native wallet applications. It extends the existing Credential Management API (`navigator.credentials`) to support digital credentials like mobile driving licenses (mDL) and verifiable credentials.

```javascript
// Browser-side JavaScript
const credential = await navigator.credentials.get({
  digital: {
    providers: [
      {
        protocol: "openid4vp",
        request: {
          /* OpenID4VP authorization request */
        },
      },
    ],
  },
});
```

### The Problem DC API Solves

Traditional OpenID4VP flows require:

1. **Deep links or QR codes**: User must scan QR or handle custom URL schemes
2. **App switching**: User manually navigates between browser and wallet
3. **Complex state management**: Application must track request/response across contexts

DC API solves these with native browser integration that:

- Presents credentials seamlessly within the browser context
- Provides secure origin binding (wallet knows which site is requesting)
- Enables same-device and cross-device presentation flows

### Where DC API Fits in the Ecosystem

| Layer    | Component                    | Role                             |
| -------- | ---------------------------- | -------------------------------- |
| Browser  | Digital Credentials API      | Transport mechanism              |
| Protocol | OpenID4VP                    | Verification protocol            |
| Format   | SD-JWT VC / mdoc             | Credential formats               |
| Query    | DCQL / Presentation Exchange | Specifies required credentials   |
| Trust    | HAIP / EUDIW                 | Security and compliance profiles |

## Glossary of Key Terms

| Term           | Definition                                                      |
| -------------- | --------------------------------------------------------------- |
| **DC API**     | W3C Digital Credentials API - browser interface for credentials |
| **web-origin** | Client ID scheme binding request to browser origin              |
| **dc_api**     | Response mode for plain VP token responses                      |
| **dc_api.jwt** | Response mode for encrypted (JWE) VP token responses            |
| **Origin**     | Browser origin (scheme + host + port) making the request        |
| **Nonce**      | Random value for replay protection                              |
| **VP Token**   | Verifiable Presentation token containing credentials            |

## Implementation Overview

### Architecture

The DC API integration in `SdJwt.Net.Oid4Vp` provides:

```text
SdJwt.Net.Oid4Vp/
   DcApi/
      DcApiConstants.cs          # Protocol and error constants
      DcApiOriginValidator.cs    # Origin validation logic
      DcApiRequestBuilder.cs     # Fluent builder for requests
      DcApiResponseValidator.cs  # Response validation
      Models/
         DcApiRequest.cs         # Request model
         DcApiResponse.cs        # Response model
         DcApiResponseMode.cs    # Response mode enum
```

### Core Components

#### DcApiRequestBuilder

Creates DC API compatible OpenID4VP requests:

```csharp
using SdJwt.Net.Oid4Vp.DcApi;

var request = new DcApiRequestBuilder()
    .WithClientId("https://verifier.example.com")
    .WithNonce(Guid.NewGuid().ToString("N"))
    .WithPresentationDefinition(presentationDefinition)
    .WithResponseMode(DcApiResponseMode.DcApi)
    .Build();
```

Key features:

- Fluent API for readable request construction
- Automatic protocol configuration for `openid4vp`
- Default `web-origin` client ID scheme

#### DcApiResponseValidator

Validates responses received from `navigator.credentials.get()`:

```csharp
using SdJwt.Net.Oid4Vp.DcApi;

var validator = new DcApiResponseValidator(vpTokenValidator);

var result = await validator.ValidateAsync(
    response,
    new DcApiValidationOptions
    {
        ExpectedOrigin = "https://verifier.example.com",
        ExpectedNonce = originalNonce,
        ValidateOrigin = true,
        MaxAge = TimeSpan.FromMinutes(5)
    });

if (result.IsValid)
{
    var credentials = result.Credentials;
}
```

Validation checks:

- Protocol verification (`openid4vp`)
- Origin matching (prevents CSRF)
- Nonce verification (prevents replay)
- Presentation age validation (freshness)
- Optional VP token validation

#### DcApiOriginValidator

Validates browser origins for security:

```csharp
using SdJwt.Net.Oid4Vp.DcApi;

var validator = new DcApiOriginValidator();

// Valid: exact match
bool valid1 = validator.ValidateOrigin(
    "https://verifier.example.com",
    "https://verifier.example.com"); // true

// Invalid: scheme mismatch
bool valid2 = validator.ValidateOrigin(
    "http://verifier.example.com",
    "https://verifier.example.com"); // false

// Invalid: subdomain doesn't match
bool valid3 = validator.ValidateOrigin(
    "https://sub.example.com",
    "https://example.com"); // false
```

## Response Modes

### Plain Response (dc_api)

Use for non-sensitive credentials:

```csharp
.WithResponseMode(DcApiResponseMode.DcApi)
```

The VP token is returned directly in the response. This mode is simpler but the credential content is visible to any intermediary.

### Encrypted Response (dc_api.jwt)

Use for credentials containing sensitive PII:

```csharp
.WithResponseMode(DcApiResponseMode.DcApiJwt)
```

The VP token is wrapped in a JWE (JSON Web Encryption) using the verifier's public key. Only the intended verifier can decrypt the response.

## Security Considerations

### Origin Binding

DC API enforces origin binding through the `web-origin` client ID scheme:

1. Browser automatically includes the page origin in requests
2. Wallet displays the origin to the user during consent
3. Response includes origin for verifier validation
4. Verifier validates origin matches expected client_id

This prevents:

- **Phishing**: User sees which site is requesting credentials
- **CSRF attacks**: Response origin must match request origin

### Nonce Validation

Always generate a unique nonce for each request:

```csharp
var nonce = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

var request = new DcApiRequestBuilder()
    .WithNonce(nonce)
    // ...
    .Build();

// Store nonce for validation
// ...

var result = await validator.ValidateAsync(response, new DcApiValidationOptions
{
    ExpectedNonce = nonce
});
```

This prevents replay attacks where an attacker captures and resends a valid response.

### Presentation Freshness

Validate that presentations are recent:

```csharp
var result = await validator.ValidateAsync(response, new DcApiValidationOptions
{
    MaxAge = TimeSpan.FromMinutes(5),
    ClockSkew = TimeSpan.FromSeconds(30)
});
```

This prevents using stale presentations that may have been captured earlier.

## Integration with mdoc

For mdoc credentials, DC API affects the session transcript:

```csharp
using SdJwt.Net.Mdoc.Handover;

// Create session transcript for DC API flow
var sessionTranscript = SessionTranscript.ForOpenId4VpDcApi(
    origin: "https://verifier.example.com",
    nonce: nonce,
    jwkThumbprint: verifierKeyThumbprint
);
```

The DC API handover structure binds the mdoc presentation to:

- The requesting origin
- The session nonce
- The verifier's key

## Error Handling

The validator returns specific error codes:

| Error Code             | Meaning                                  |
| ---------------------- | ---------------------------------------- |
| `origin_mismatch`      | Response origin differs from client_id   |
| `nonce_mismatch`       | Response nonce differs from expected     |
| `presentation_expired` | Presentation age exceeds maximum allowed |
| `invalid_protocol`     | Protocol is not `openid4vp`              |
| `decryption_failed`    | Failed to decrypt dc_api.jwt response    |

Example error handling:

```csharp
var result = await validator.ValidateAsync(response, options);

if (!result.IsValid)
{
    switch (result.ErrorCode)
    {
        case DcApiConstants.ErrorCodes.OriginMismatch:
            // Potential CSRF attack
            logger.LogWarning("Origin mismatch detected");
            break;
        case DcApiConstants.ErrorCodes.NonceMismatch:
            // Potential replay attack
            logger.LogWarning("Nonce mismatch detected");
            break;
        // ...
    }
}
```

## Comparison with Other Transports

| Feature         | DC API         | Deep Link     | QR Code       |
| --------------- | -------------- | ------------- | ------------- |
| Same device     | Native         | Supported     | Not ideal     |
| Cross device    | Planned        | Not supported | Native        |
| Origin binding  | Built-in       | Manual        | Manual        |
| User experience | Seamless       | App switch    | Scan required |
| Browser support | Limited (2024) | Universal     | Universal     |

## Browser Support

As of 2024, DC API support is:

| Browser | Status         | Notes                            |
| ------- | -------------- | -------------------------------- |
| Chrome  | Origin Trial   | Behind flag, testing in progress |
| Safari  | In Development | iOS 18+ expected                 |
| Firefox | Tracking       | No implementation yet            |
| Edge    | Follows Chrome | Chromium-based                   |

## Complete Example

### Verifier Backend (ASP.NET Core)

```csharp
[ApiController]
[Route("api/verify")]
public class VerificationController : ControllerBase
{
    private readonly DcApiResponseValidator _validator;
    private readonly ILogger<VerificationController> _logger;

    [HttpPost("start")]
    public IActionResult StartVerification()
    {
        var nonce = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

        // Store nonce in session
        HttpContext.Session.SetString("dc_api_nonce", nonce);

        var request = new DcApiRequestBuilder()
            .WithClientId("https://example.com")
            .WithNonce(nonce)
            .WithPresentationDefinition(CreatePresentationDefinition())
            .Build();

        return Ok(request);
    }

    [HttpPost("complete")]
    public async Task<IActionResult> CompleteVerification([FromBody] DcApiResponse response)
    {
        var expectedNonce = HttpContext.Session.GetString("dc_api_nonce");

        var result = await _validator.ValidateAsync(response, new DcApiValidationOptions
        {
            ExpectedOrigin = "https://example.com",
            ExpectedNonce = expectedNonce,
            ValidateOrigin = true,
            MaxAge = TimeSpan.FromMinutes(5)
        });

        if (!result.IsValid)
        {
            _logger.LogWarning("DC API validation failed: {Error}", result.ErrorMessage);
            return BadRequest(new { error = result.ErrorCode });
        }

        // Process verified credentials
        return Ok(new { verified = true, credentials = result.Credentials });
    }
}
```

### Frontend (JavaScript)

```javascript
async function requestCredential() {
  // Get request from backend
  const startResponse = await fetch("/api/verify/start", { method: "POST" });
  const dcApiRequest = await startResponse.json();

  // Request credential via DC API
  const credential = await navigator.credentials.get({
    digital: {
      providers: [
        {
          protocol: dcApiRequest.protocol,
          request: dcApiRequest.request,
        },
      ],
    },
  });

  // Send response to backend
  const completeResponse = await fetch("/api/verify/complete", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(credential),
  });

  return await completeResponse.json();
}
```

## Related Documentation

- [OpenID4VP Deep Dive](openid4vp-deep-dive.md) - The underlying verification protocol
- [mdoc Deep Dive](mdoc-deep-dive.md) - Mobile document format support
- [HAIP Compliance](haip-compliance.md) - High assurance security requirements
- [Presentation Exchange](presentation-exchange-deep-dive.md) - Credential query language

## References

- W3C Digital Credentials API: <https://wicg.github.io/digital-credentials/>
- OpenID4VP Specification: <https://openid.net/specs/openid-4-verifiable-presentations-1_0.html>
- Credential Management API: <https://www.w3.org/TR/credential-management-1/>
