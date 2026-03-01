# W3C Digital Credentials API Integration Proposal

## Document Information

| Field        | Value            |
| ------------ | ---------------- |
| Version      | 1.0.0            |
| Author       | SD-JWT .NET Team |
| Status       | Draft Proposal   |
| Created      | 2026-03-01       |
| Last Updated | 2026-03-01       |

## Executive Summary

This proposal outlines the design, architecture, and implementation plan for W3C Digital Credentials API (DC API) integration in the SD-JWT .NET ecosystem. The DC API enables web applications to request digital credentials from native wallets through browser APIs, providing a seamless user experience for credential verification flows.

## Background and Motivation

### Why Digital Credentials API?

1. **Browser-Native Integration**: Wallets can be invoked directly via `navigator.credentials.get()` without custom URI schemes
2. **Consumer Application Enablement**: Critical for web-based credential verification scenarios
3. **Cross-Platform Support**: Works across Chrome, Edge, Safari with native wallet integration
4. **OpenID4VP Alignment**: DC API is a transport mechanism for OpenID4VP protocol

### W3C Specification Status

| Aspect           | Status                                   |
| ---------------- | ---------------------------------------- |
| Working Group    | W3C Federated Identity                   |
| Specification    | Candidate Recommendation (CR)            |
| Browser Support  | Chrome 125+, Edge 125+, Safari (partial) |
| Protocol Binding | OpenID4VP 1.0                            |

### Reference Implementations Analyzed

| Implementation      | Language   | Organization | Key Insights                          |
| ------------------- | ---------- | ------------ | ------------------------------------- |
| digital-credentials | JavaScript | W3C          | Reference implementation for browsers |
| openid4vc-ts        | TypeScript | Animo        | TypeScript OID4VP with DC API support |
| AusweisApp2         | C++        | Governikus   | German eID wallet with DC API         |

## Technical Architecture

### Package Structure

DC API integration extends the existing `SdJwt.Net.Oid4Vp` package:

```
src/SdJwt.Net.Oid4Vp/
    DcApi/
        DcApiRequestBuilder.cs       # Creates DC API compatible requests
        DcApiResponseParser.cs       # Parses DC API response envelope
        DcApiResponseValidator.cs    # Validates DC API responses
        DcApiJwtHandler.cs           # Handles dc_api.jwt encrypted responses
        DcApiOriginValidator.cs      # Browser origin validation
        DcApiConstants.cs            # DC API specific constants
        Models/
            DcApiRequest.cs          # DC API request structure
            DcApiResponse.cs         # DC API response structure
            DcApiResponseMode.cs     # dc_api, dc_api.jwt modes
            DcApiValidationResult.cs # Validation result model
            DcApiValidationOptions.cs # Validation configuration
            DigitalCredentialProvider.cs # Provider configuration
```

### Dependencies

- `SdJwt.Net.Oid4Vp` (existing package - extended)
- `SdJwt.Net.PresentationExchange` (existing - reused)
- No additional NuGet dependencies required

## Detailed Component Design

### 1. DC API Constants

```csharp
namespace SdJwt.Net.Oid4Vp.DcApi;

/// <summary>
/// Constants for W3C Digital Credentials API integration.
/// </summary>
public static class DcApiConstants
{
    /// <summary>
    /// Protocol identifier for OpenID4VP over DC API.
    /// </summary>
    public const string Protocol = "openid4vp";

    /// <summary>
    /// Response modes for DC API.
    /// </summary>
    public static class ResponseModes
    {
        /// <summary>
        /// Plain response mode - VP token returned directly.
        /// </summary>
        public const string DcApi = "dc_api";

        /// <summary>
        /// Encrypted (JWE) response mode for privacy-sensitive credentials.
        /// </summary>
        public const string DcApiJwt = "dc_api.jwt";
    }

    /// <summary>
    /// Client ID scheme for DC API web origin binding.
    /// </summary>
    public const string WebOriginScheme = "web-origin";

    /// <summary>
    /// Standard digital credential type for navigator.credentials.
    /// </summary>
    public const string CredentialType = "digital";
}
```

### 2. DC API Request Models

```csharp
namespace SdJwt.Net.Oid4Vp.DcApi.Models;

/// <summary>
/// Represents a Digital Credentials API request compatible with navigator.credentials.get().
/// </summary>
public class DcApiRequest
{
    /// <summary>
    /// Protocol identifier. Must be "openid4vp".
    /// </summary>
    public string Protocol { get; set; } = DcApiConstants.Protocol;

    /// <summary>
    /// The OpenID4VP authorization request object.
    /// </summary>
    public AuthorizationRequest Request { get; set; } = new();

    /// <summary>
    /// Converts the request to a JSON structure for navigator.credentials.get().
    /// </summary>
    public string ToNavigatorCredentialsPayload();
}

/// <summary>
/// Digital credential provider configuration for navigator.credentials.get().
/// </summary>
public class DigitalCredentialProvider
{
    /// <summary>
    /// Protocol identifier.
    /// </summary>
    public string Protocol { get; set; } = DcApiConstants.Protocol;

    /// <summary>
    /// The raw request object to send to the wallet.
    /// </summary>
    public object Request { get; set; } = new();
}
```

### 3. DC API Response Mode Enum

```csharp
namespace SdJwt.Net.Oid4Vp.DcApi.Models;

/// <summary>
/// Response modes supported by the Digital Credentials API.
/// </summary>
public enum DcApiResponseMode
{
    /// <summary>
    /// Plain response - VP token returned directly in response.
    /// </summary>
    DcApi,

    /// <summary>
    /// Encrypted response - VP token wrapped in JWE for privacy.
    /// </summary>
    DcApiJwt
}
```

### 4. DC API Request Builder

```csharp
namespace SdJwt.Net.Oid4Vp.DcApi;

/// <summary>
/// Builder for creating DC API compatible OpenID4VP requests.
/// </summary>
public class DcApiRequestBuilder
{
    private string _clientId = string.Empty;
    private string _clientIdScheme = DcApiConstants.WebOriginScheme;
    private string _nonce = string.Empty;
    private string _responseType = "vp_token";
    private DcApiResponseMode _responseMode = DcApiResponseMode.DcApi;
    private PresentationDefinition? _presentationDefinition;
    private JsonWebKey? _encryptionKey;

    /// <summary>
    /// Sets the client identifier (typically the verifier's origin URL).
    /// </summary>
    public DcApiRequestBuilder WithClientId(string clientId);

    /// <summary>
    /// Sets the client ID scheme. Defaults to "web-origin".
    /// </summary>
    public DcApiRequestBuilder WithClientIdScheme(string scheme);

    /// <summary>
    /// Sets the nonce for replay protection.
    /// </summary>
    public DcApiRequestBuilder WithNonce(string nonce);

    /// <summary>
    /// Sets the presentation definition describing required credentials.
    /// </summary>
    public DcApiRequestBuilder WithPresentationDefinition(PresentationDefinition definition);

    /// <summary>
    /// Sets the response mode. Use DcApiJwt for encrypted responses.
    /// </summary>
    public DcApiRequestBuilder WithResponseMode(DcApiResponseMode mode);

    /// <summary>
    /// Sets the encryption key for dc_api.jwt response mode.
    /// Required when response mode is DcApiJwt.
    /// </summary>
    public DcApiRequestBuilder WithEncryptionKey(JsonWebKey key);

    /// <summary>
    /// Builds the DC API request.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when required parameters are missing or invalid.
    /// </exception>
    public DcApiRequest Build();
}
```

### 5. DC API Response Validation

```csharp
namespace SdJwt.Net.Oid4Vp.DcApi;

/// <summary>
/// Validation options for DC API responses.
/// </summary>
public class DcApiValidationOptions
{
    /// <summary>
    /// Expected browser origin for origin validation.
    /// Must match the client_id when using web-origin scheme.
    /// </summary>
    public string ExpectedOrigin { get; set; } = string.Empty;

    /// <summary>
    /// Expected nonce value for replay protection.
    /// </summary>
    public string ExpectedNonce { get; set; } = string.Empty;

    /// <summary>
    /// Maximum age allowed for the presentation.
    /// </summary>
    public TimeSpan MaxAge { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// JWK for decrypting dc_api.jwt responses.
    /// Required when expecting encrypted responses.
    /// </summary>
    public JsonWebKey? DecryptionKey { get; set; }

    /// <summary>
    /// Whether to validate the browser origin strictly.
    /// </summary>
    public bool ValidateOrigin { get; set; } = true;
}

/// <summary>
/// Result of DC API response validation.
/// </summary>
public class DcApiValidationResult
{
    /// <summary>
    /// Indicates whether the response is valid.
    /// </summary>
    public bool IsValid { get; init; }

    /// <summary>
    /// Error message if validation failed.
    /// </summary>
    public string? Error { get; init; }

    /// <summary>
    /// Error code if validation failed.
    /// </summary>
    public string? ErrorCode { get; init; }

    /// <summary>
    /// The verified credentials from the response.
    /// </summary>
    public IReadOnlyList<VerifiedCredential> VerifiedCredentials { get; init; } = Array.Empty<VerifiedCredential>();

    /// <summary>
    /// The presentation submission describing the credentials.
    /// </summary>
    public PresentationSubmission? PresentationSubmission { get; init; }

    /// <summary>
    /// Creates a successful validation result.
    /// </summary>
    public static DcApiValidationResult Success(
        IReadOnlyList<VerifiedCredential> credentials,
        PresentationSubmission? submission = null);

    /// <summary>
    /// Creates a failed validation result.
    /// </summary>
    public static DcApiValidationResult Failure(string error, string errorCode);
}

/// <summary>
/// Validates responses received via the Digital Credentials API.
/// </summary>
public class DcApiResponseValidator
{
    /// <summary>
    /// Initializes a new instance with the specified VP token validator.
    /// </summary>
    public DcApiResponseValidator(VpTokenValidator vpTokenValidator);

    /// <summary>
    /// Validates a DC API response.
    /// </summary>
    /// <param name="response">The raw response from navigator.credentials.get().</param>
    /// <param name="options">Validation options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Validation result with verified credentials or error details.</returns>
    public Task<DcApiValidationResult> ValidateAsync(
        DcApiResponse response,
        DcApiValidationOptions options,
        CancellationToken cancellationToken = default);
}
```

### 6. DC API Origin Validator

```csharp
namespace SdJwt.Net.Oid4Vp.DcApi;

/// <summary>
/// Validates browser origins for DC API requests.
/// </summary>
public class DcApiOriginValidator
{
    /// <summary>
    /// Validates that the response origin matches the expected client_id.
    /// </summary>
    /// <param name="responseOrigin">Origin from the DC API response.</param>
    /// <param name="expectedClientId">Expected client_id (verifier URL).</param>
    /// <returns>True if origins match, false otherwise.</returns>
    public bool ValidateOrigin(string responseOrigin, string expectedClientId);

    /// <summary>
    /// Extracts the origin from a URL.
    /// </summary>
    public static string ExtractOrigin(string url);
}
```

### 7. DC API JWT Handler (Encrypted Responses)

```csharp
namespace SdJwt.Net.Oid4Vp.DcApi;

/// <summary>
/// Handles dc_api.jwt encrypted response mode.
/// </summary>
public class DcApiJwtHandler
{
    /// <summary>
    /// Decrypts a dc_api.jwt response.
    /// </summary>
    /// <param name="encryptedResponse">The JWE-encrypted response.</param>
    /// <param name="decryptionKey">Private key for decryption.</param>
    /// <returns>The decrypted DC API response.</returns>
    public DcApiResponse Decrypt(string encryptedResponse, JsonWebKey decryptionKey);

    /// <summary>
    /// Validates the JWE structure and algorithms.
    /// </summary>
    public bool ValidateJweStructure(string jwe);
}
```

## Security Considerations

### Origin Validation

Browser origin must be validated against the `client_id` to prevent malicious sites from intercepting credential responses:

```csharp
// Origin validation is critical for security
if (!_originValidator.ValidateOrigin(response.Origin, options.ExpectedOrigin))
{
    return DcApiValidationResult.Failure(
        "Origin mismatch: response origin does not match expected client_id",
        "origin_mismatch");
}
```

### Nonce Binding

The nonce from the request must be bound in the session transcript and validated in the response:

1. Request includes `nonce` parameter
2. Wallet binds nonce in key binding JWT `aud` claim
3. Validator verifies nonce matches expected value

### Encrypted Responses (dc_api.jwt)

For privacy-sensitive credentials, `dc_api.jwt` mode encrypts the response:

- JWE using verifier's public key
- Prevents browser extensions from reading credential data
- Required for PII-containing credentials in production

### CORS Configuration

Backend services must return appropriate CORS headers:

```text
Access-Control-Allow-Origin: <verifier-origin>
Access-Control-Allow-Methods: POST, OPTIONS
Access-Control-Allow-Headers: Content-Type
Access-Control-Max-Age: 86400
```

## API Usage Examples

### Creating a DC API Request

```csharp
// Create request
var request = new DcApiRequestBuilder()
    .WithClientId("https://verifier.example.com")
    .WithNonce(Guid.NewGuid().ToString())
    .WithPresentationDefinition(new PresentationDefinition
    {
        Id = "id-verification",
        InputDescriptors = new[]
        {
            new InputDescriptor
            {
                Id = "identity_credential",
                Format = new Format { SdJwt = new SdJwtFormat() },
                Constraints = new Constraints
                {
                    Fields = new[]
                    {
                        new Field { Path = new[] { "$.given_name" } },
                        new Field { Path = new[] { "$.family_name" } }
                    }
                }
            }
        }
    })
    .WithResponseMode(DcApiResponseMode.DcApi)
    .Build();

// Get payload for JavaScript
string jsPayload = request.ToNavigatorCredentialsPayload();
```

### JavaScript Frontend Integration

```javascript
// Fetch request from backend
const dcRequest = await fetch("/api/dc-request").then((r) => r.json());

// Call Digital Credentials API
const credential = await navigator.credentials.get({
  digital: {
    providers: [
      {
        protocol: "openid4vp",
        request: dcRequest,
      },
    ],
  },
});

// Submit to backend for validation
const result = await fetch("/api/verify", {
  method: "POST",
  headers: { "Content-Type": "application/json" },
  body: JSON.stringify(credential),
});
```

### Validating DC API Response

```csharp
// Parse response from frontend
var response = JsonSerializer.Deserialize<DcApiResponse>(requestBody);

// Validate
var validator = new DcApiResponseValidator(vpTokenValidator);
var result = await validator.ValidateAsync(response, new DcApiValidationOptions
{
    ExpectedOrigin = "https://verifier.example.com",
    ExpectedNonce = sessionNonce,
    MaxAge = TimeSpan.FromMinutes(5)
});

if (result.IsValid)
{
    foreach (var credential in result.VerifiedCredentials)
    {
        Console.WriteLine($"Verified: {credential.Type}");
        Console.WriteLine($"Given Name: {credential.Claims["given_name"]}");
    }
}
else
{
    Console.WriteLine($"Validation failed: {result.Error}");
}
```

## Implementation Plan

### Phase 1: Core Infrastructure (Week 1-2)

| Task                  | Description               | Priority |
| --------------------- | ------------------------- | -------- |
| `DcApiConstants`      | Add DC API constants      | High     |
| `DcApiResponseMode`   | Response mode enumeration | High     |
| `DcApiRequest` model  | Request structure         | High     |
| `DcApiResponse` model | Response structure        | High     |
| `DcApiRequestBuilder` | Fluent request builder    | High     |

### Phase 2: Validation (Week 2-3)

| Task                     | Description              | Priority |
| ------------------------ | ------------------------ | -------- |
| `DcApiOriginValidator`   | Origin validation        | High     |
| `DcApiValidationOptions` | Validation configuration | High     |
| `DcApiValidationResult`  | Result model             | High     |
| `DcApiResponseValidator` | Main validation logic    | High     |

### Phase 3: Encrypted Responses (Week 3-4)

| Task                       | Description               | Priority |
| -------------------------- | ------------------------- | -------- |
| `DcApiJwtHandler`          | JWE decryption            | High     |
| Encrypted response parsing | dc_api.jwt support        | High     |
| Algorithm validation       | HAIP-compliant algorithms | High     |

### Phase 4: Integration and Samples (Week 4-5)

| Task                     | Description                 | Priority |
| ------------------------ | --------------------------- | -------- |
| Update `Oid4VpConstants` | Add DC API response modes   | Medium   |
| JavaScript sample        | Browser integration example | Medium   |
| ASP.NET Core sample      | Backend API example         | Medium   |
| Documentation            | API docs and tutorials      | Medium   |

## Test Strategy

Following TDD methodology:

### Unit Tests

```text
tests/SdJwt.Net.Oid4Vp.Tests/
    DcApi/
        DcApiRequestBuilderTests.cs
        DcApiResponseValidatorTests.cs
        DcApiOriginValidatorTests.cs
        DcApiJwtHandlerTests.cs
```

### Test Categories

| Category       | Coverage Target | Examples                             |
| -------------- | --------------- | ------------------------------------ |
| Request Build  | 100%            | Valid builds, missing params, modes  |
| Origin Check   | 100%            | Same origin, cross-origin, malformed |
| Response Parse | 100%            | Valid, malformed, encrypted          |
| Validation     | 100%            | Nonce, expiry, signatures            |
| Integration    | 90%             | End-to-end flows                     |

## Success Criteria

| Metric                | Target                        |
| --------------------- | ----------------------------- |
| Unit test pass rate   | 100%                          |
| Code coverage         | >= 90%                        |
| API documentation     | All public methods documented |
| Browser compatibility | Chrome 125+, Edge 125+        |
| Sample application    | Working end-to-end demo       |

## Appendix: W3C DC API Specification

### navigator.credentials.get() Structure

```javascript
{
    digital: {
        providers: [{
            protocol: "openid4vp",
            request: {
                client_id: "https://verifier.example.com",
                client_id_scheme: "web-origin",
                response_type: "vp_token",
                response_mode: "dc_api",
                nonce: "n-0S6_WzA2Mj",
                presentation_definition: { ... }
            }
        }]
    }
}
```

### Response Structure

```javascript
{
    protocol: "openid4vp",
    response: {
        vp_token: "eyJ...",
        presentation_submission: { ... }
    },
    origin: "https://wallet.example.com"
}
```
