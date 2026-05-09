# SdJwt.Net.Oid4Vp - OpenID for Verifiable Presentations

[![NuGet Version](https://img.shields.io/nuget/v/SdJwt.Net.Oid4Vp.svg)](https://www.nuget.org/packages/SdJwt.Net.Oid4Vp/)
[![License](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](https://opensource.org/licenses/Apache-2.0)

Implementation of **OpenID4VP 1.0** specification for verifiable presentation verification. Provides protocol support with DCQL, Presentation Exchange v2.1.1 integration, and cross-device flow support.

## Features

-   **OID4VP 1.0 Final**: Specification implementation
-   **DCQL Support**: Credential query models, format metadata validation, credential sets, and DCQL `vp_token` maps
-   **SIOPv2 Combined Responses**: `vp_token id_token` request/response model support with subject-signed ID Tokens from `SdJwt.Net.SiopV2`
-   **Presentation Exchange v2.1.1**: DIF PE integration
-   **Format Validators**: Extensible validation hooks for `dc+sd-jwt`, `mso_mdoc`, `jwt_vc_json`, `jwt_vc_json-ld`, and `ldp_vc`
-   **Cross-Device Flow**: QR code-based presentation flows
-   **Multi-Credential**: Multi-credential presentation support
-   **Security Validation**: Nonce, audience, freshness, transaction data, and verifier info validation with key binding

## Installation

```bash
dotnet add package SdJwt.Net.Oid4Vp
```

## Quick Start

### Create Presentation Request

```csharp
using SdJwt.Net.Oid4Vp.Models;
using SdJwt.Net.Oid4Vp.Verifier;

var presentationRequest = new AuthorizationRequest
{
    ClientId = "https://verifier.example.com",
    ResponseType = "vp_token",
    ResponseMode = "direct_post",
    ResponseUri = "https://verifier.example.com/presentations",
    Nonce = "presentation_nonce_123",
    PresentationDefinition = new PresentationDefinition
    {
        Id = "employment_verification",
        InputDescriptors = new[]
        {
            new InputDescriptor
            {
                Id = "employment_credential",
                Constraints = new Constraints
                {
                    Fields = new[]
                    {
                        new Field { Path = new[] { "$.position" } },
                        new Field { Path = new[] { "$.employment_type" } }
                    }
                }
            }
        }
    }
};
```

### Create a DCQL Presentation Request

```csharp
using SdJwt.Net.Oid4Vp.Models;
using SdJwt.Net.Oid4Vp.Models.Dcql;
using SdJwt.Net.Oid4Vp.Models.Dcql.Formats;

var request = new AuthorizationRequest
{
    ClientId = "https://verifier.example.com",
    ResponseType = "vp_token",
    ResponseMode = "direct_post",
    ResponseUri = "https://verifier.example.com/presentations",
    Nonce = "presentation_nonce_123",
    DcqlQuery = new DcqlQuery
    {
        Credentials = new[]
        {
            new DcqlCredentialQuery
            {
                Id = "employee_credential",
                Format = Oid4VpConstants.SdJwtVcFormat,
                Meta = new SdJwtVcMeta
                {
                    VctValues = new[] { "https://issuer.example.com/credentials/employee" }
                }
            }
        }
    },
    TransactionData = new[] { base64UrlEncodedTransactionData }
};
```

### Validate a DCQL Response

DCQL responses do not use `presentation_submission`. The `vp_token` response is a JSON object keyed by DCQL credential query `id`, and each value is one or more presentations for that query.

```csharp
var options = VpTokenValidationOptions.CreateForOid4Vp("https://verifier.example.com");
options.ExpectedDcqlQuery = request.DcqlQuery;

var result = await validator.ValidateAsync(response, "presentation_nonce_123", options);
```

The validator enforces:

-   DCQL model structure, including unique credential ids, valid claim paths, valid `claim_sets`, and valid `credential_sets` references.
-   `credential_sets` option semantics: an option with multiple ids is an AND, multiple options are OR, and required sets must have at least one satisfied option.
-   `vp_token` response map shape, unknown ids, empty tokens, and `multiple` rules.
-   Verified disclosed claims for SD-JWT VC flows, including `meta.vct_values`, requested claim paths, claim value matching, and `claim_sets`.

### Request a SIOPv2 ID Token with the VP Token

OpenID4VP can request a SIOPv2 subject-signed ID Token alongside the presentation by using the combined response type. `SdJwt.Net.Oid4Vp` models the request and response shape; `SdJwt.Net.SiopV2` issues and validates the `id_token`.

```csharp
var request = AuthorizationRequest.CreateCrossDevice(
    "https://verifier.example.com",
    "https://verifier.example.com/presentations",
    "presentation_nonce_123",
    presentationDefinition);

request.ResponseType = Oid4VpConstants.ResponseTypes.VpTokenIdToken;
request.Scope = "openid";
request.IdTokenType = Oid4VpConstants.IdTokenTypes.SubjectSigned;
request.Validate();

var response = AuthorizationResponse.SuccessWithIdToken(
    vpToken,
    presentationSubmission,
    idToken,
    request.State);
```

### Process VP Token Response (Recommended - OID4VP Compliant)

```csharp
using SdJwt.Net.Oid4Vp.Verifier;

// Create validator with SD-JWT VC validation enabled (recommended)
var validator = new VpTokenValidator(
    keyProvider: async (jwtToken) => {
        // Resolve issuer's public key based on JWT header/payload
        return await GetIssuerPublicKeyAsync(jwtToken.Issuer);
    },
    useSdJwtVcValidation: true); // Enables vct, iss, typ validation

// Use factory method for OID4VP-compliant options
var options = VpTokenValidationOptions.CreateForOid4Vp("https://verifier.example.com");

// Optional: Customize validation
options.ValidIssuers = new[] { "https://trusted-issuer.example.com" };
options.MaxKeyBindingAge = TimeSpan.FromMinutes(5); // Stricter than default

// Optional: enforce the same DIF Presentation Exchange v2.1.1 definition
// that was sent in the authorization request. The validator checks
// presentation_submission after SD-JWT verification, using verified claims.
options.ExpectedPresentationExchangeDefinition = expectedPresentationDefinition;

// Validate VP token
var result = await validator.ValidateAsync(
    vpTokenResponse,
    expectedNonce: "presentation_nonce_123",
    options);

if (result.IsValid)
{
    foreach (var tokenResult in result.ValidatedTokens)
    {
        var vctClaim = tokenResult.Claims["vct"];
        var issuer = tokenResult.Claims["iss"];
        // Use verified claims safely
    }
}
```

`ExpectedPresentationExchangeDefinition` uses the shared `SdJwt.Net.PresentationExchange.Models.PresentationDefinition` model. OID4VP request models remain available for request serialization, but verifier-side PEX constraint enforcement should use the shared Presentation Exchange package.

### Security Features

This library validates the following per OID4VP 1.0:

#### **Nonce Validation** (OID4VP Section 14.1)

```csharp
// Nonce validation is AUTOMATIC when you provide expectedNonce
var result = await validator.ValidateAsync(response, expectedNonce, options);

// The validator ensures:
// - KB-JWT contains 'nonce' claim
// - Nonce matches the expected value from authorization request
// - Prevents replay attacks
```

#### **Audience Validation** (OID4VP Section 8.6)

```csharp
// Enabled by default for security
var options = VpTokenValidationOptions.CreateForOid4Vp("https://verifier.example.com");

// The validator ensures:
// - KB-JWT 'aud' claim matches your client_id
// - Prevents token reuse across different verifiers
// - Can be customized or disabled if needed:
options.ValidateKeyBindingAudience = false; // Not recommended
```

#### **Freshness Validation** (OID4VP Section 14.1)

```csharp
// Enabled by default to prevent replay attacks
var options = VpTokenValidationOptions.CreateForOid4Vp("https://verifier.example.com");
options.MaxKeyBindingAge = TimeSpan.FromMinutes(10); // Default

// The validator ensures:
// - KB-JWT 'iat' claim is present
// - KB-JWT was issued recently (within MaxKeyBindingAge)
// - Includes clock skew tolerance (default: 5 minutes)
```

#### **SD-JWT VC Format Validation** (draft-ietf-oauth-sd-jwt-vc)

```csharp
// Enabled by default when using VpTokenValidator
var validator = new VpTokenValidator(keyProvider, useSdJwtVcValidation: true);

// The validator ensures:
// - 'vct' claim is present and valid
// - 'iss' claim is present
// - 'typ' header is 'dc+sd-jwt'
// - Collision-resistant names are validated
```

### Testing/Development Mode

For testing or development, use relaxed validation:

```csharp
var validator = new VpTokenValidator(keyProvider, useSdJwtVcValidation: false);
var options = VpTokenValidationOptions.CreateForTesting();

// This disables strict OID4VP validations:
// - No issuer validation
// - No audience validation
// - No freshness validation
// - Extended time windows
```

### Migration from v1.0

If you're upgrading from v1.0, note these changes:

```csharp
// OLD (v1.0):
var options = new VpTokenValidationOptions
{
    ValidateKeyBindingAudience = false, // Was default
    ValidateKeyBindingFreshness = false, // Not available
};

// NEW (v1.1) - RECOMMENDED:
var options = VpTokenValidationOptions.CreateForOid4Vp("https://verifier.example.com");

// NEW (v1.1) - If you need old behavior:
var options = new VpTokenValidationOptions
{
    ValidateKeyBindingAudience = false,
    ValidateKeyBindingFreshness = false
};
```

## Use Cases

-   **Employment Verification**: Bank loan applications requiring job verification
-   **Age Verification**: Privacy-preserving age proof for restricted services
-   **Cross-Device Flows**: QR code scanning from mobile to desktop
-   **Complex Requirements**: Multi-credential presentations for compliance
-   **Mixed Format Verification**: SD-JWT VC, W3C VCDM, and ISO mdoc presentations in DCQL flows

## Test Notes

The test project targets `net10.0`. On Windows worktrees with restrictive inherited ACLs, use a serialized test run to avoid MSBuild worker buildup:

```pwsh
$env:MSBUILDDISABLENODEREUSE = "1"
dotnet test tests/SdJwt.Net.Oid4Vp.Tests/SdJwt.Net.Oid4Vp.Tests.csproj -f net10.0 -m:1 -nr:false -p:UseSharedCompilation=false
```

## Documentation

For more examples and protocol implementation patterns, see the [main repository](https://openwallet-foundation-labs.github.io/sd-jwt-dotnet/).

## License

Licensed under the [Apache License 2.0](https://opensource.org/licenses/Apache-2.0).
