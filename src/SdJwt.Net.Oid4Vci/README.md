# SD-JWT.NET - OpenID for Verifiable Credential Issuance (OID4VCI)

[![NuGet Version](https://img.shields.io/nuget/v/SdJwt.Net.Oid4Vci.svg)](https://www.nuget.org/packages/SdJwt.Net.Oid4Vci/)
[![Build Status](https://github.com/thomas-tran/sd-jwt-dotnet/actions/workflows/ci-cd.yml/badge.svg)](https://github.com/thomas-tran/sd-jwt-dotnet/actions)
[![License](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](https://opensource.org/licenses/Apache-2.0)

A comprehensive, transport-agnostic .NET implementation of **OpenID for Verifiable Credential Issuance (OID4VCI) 1.0** protocol. This library provides complete data models, utilities, and helpers for implementing OID4VCI flows with SD-JWT credentials.

## 🚀 Features

### 📋 Complete OID4VCI 1.0 Compliance
- **Full Specification Implementation**: Complete support for OpenID4VCI 1.0
- **All Grant Types**: Pre-authorized code, authorization code, and refresh token flows
- **Deferred Issuance**: Support for asynchronous credential issuance
- **Credential Notifications**: Lifecycle management and acceptance tracking
- **Multiple Proof Types**: JWT, CWT, and Linked Data Proof support

### 🔧 Modular Architecture
- **Separate Model Files**: Each class in its own file for better maintainability
- **Transport-Agnostic Design**: Use with any HTTP framework (ASP.NET Core, minimal APIs, etc.)
- **Type-Safe Models**: Strongly-typed classes with comprehensive validation
- **Builder Pattern**: Fluent APIs for creating credential offers

### 🔒 Security & Standards
- **RFC Compliance**: Follows OAuth 2.0, OpenID Connect, and OID4VCI specifications
- **Secure Proof Validation**: Comprehensive JWT proof-of-possession validation
- **Nonce Management**: Cryptographically secure nonce generation and validation
- **Error Handling**: Detailed error responses with proper status codes

## 📦 Installation

```bash
dotnet add package SdJwt.Net.Oid4Vci
```

## 🎯 Quick Start

### Issuer: Creating a Credential Offer

```csharp
using SdJwt.Net.Oid4Vci.Issuer;
using SdJwt.Net.Oid4Vci.Models;

// Create a credential offer using the builder pattern
var offer = CredentialOfferBuilder
    .Create("https://issuer.example.com")
    .AddConfigurationId("UniversityDegree_SDJWT")
    .AddConfigurationId("EmployeeID_SDJWT")
    .UsePreAuthorizedCode("eyJhbGci...pre-auth-code", pinLength: 4)
    .Build();

// Generate QR code URI
var qrUri = CredentialOfferBuilder
    .Create("https://issuer.example.com")
    .AddConfigurationId("UniversityDegree_SDJWT")
    .UsePreAuthorizedCode("eyJhbGci...pre-auth-code")
    .BuildUri();

Console.WriteLine($"QR Code URI: {qrUri}");
// Output: openid-credential-offer://?credential_offer=%7B%22credential_issuer%22...
```

### Wallet: Parsing a Credential Offer

```csharp
using SdJwt.Net.Oid4Vci.Client;

// Parse credential offer from QR code or deep link
var offer = CredentialOfferParser.Parse(qrCodeUri);

Console.WriteLine($"Issuer: {offer.CredentialIssuer}");
Console.WriteLine($"Credentials: {string.Join(", ", offer.CredentialConfigurationIds)}");

// Check for pre-authorized code
var preAuthGrant = offer.GetPreAuthorizedCodeGrant();
if (preAuthGrant != null)
{
    Console.WriteLine($"Pre-auth code: {preAuthGrant.PreAuthorizedCode}");
    if (preAuthGrant.TransactionCode != null)
    {
        Console.WriteLine($"PIN required: {preAuthGrant.TransactionCode.Length} digits");
    }
}
```

### Token Exchange

```csharp
using SdJwt.Net.Oid4Vci.Models;

// Create token request for pre-authorized flow
var tokenRequest = new TokenRequest
{
    GrantType = Oid4VciConstants.GrantTypes.PreAuthorizedCode,
    PreAuthorizedCode = preAuthGrant.PreAuthorizedCode,
    TransactionCode = "1234", // PIN entered by user
    ClientId = "wallet-client-123"
};

// Send to token endpoint (transport implementation not included)
var tokenResponse = await HttpClient.PostTokenRequestAsync(tokenRequest);

Console.WriteLine($"Access Token: {tokenResponse.AccessToken}");
Console.WriteLine($"C-Nonce: {tokenResponse.CNonce}");
```

### Credential Request

```csharp
using SdJwt.Net.Oid4Vci.Client;
using SdJwt.Net.Oid4Vci.Models;

// Create proof of possession JWT
var proofJwt = ProofBuilder
    .CreateJwtProof(holderKey, SecurityAlgorithms.EcdsaSha256)
    .WithIssuer("wallet-client-123") // Optional client ID
    .WithAudience(offer.CredentialIssuer)
    .WithNonce(tokenResponse.CNonce!)
    .Build();

// Create credential request
var credentialRequest = CredentialRequest.Create("UniversityDegree", proofJwt);

// Or use credential identifier
var credentialRequest2 = CredentialRequest.CreateByIdentifier(
    "degree-config-id", proofJwt);

// Send to credential endpoint (transport implementation not included)
var credentialResponse = await HttpClient.PostCredentialRequestAsync(
    credentialRequest, tokenResponse.AccessToken);

if (credentialResponse.Credential != null)
{
    Console.WriteLine($"Received credential: {credentialResponse.Credential}");
}
else if (credentialResponse.AcceptanceToken != null)
{
    Console.WriteLine("Credential will be available later");
    // Handle deferred issuance
}
```

### Deferred Credential Issuance

```csharp
// For deferred issuance, poll the deferred endpoint
var deferredRequest = new DeferredCredentialRequest
{
    AcceptanceToken = credentialResponse.AcceptanceToken!
};

// Poll until credential is ready
DeferredCredentialResponse? deferredResponse;
do
{
    await Task.Delay(TimeSpan.FromSeconds(5)); // Wait before polling
    deferredResponse = await HttpClient.PostDeferredRequestAsync(
        deferredRequest, tokenResponse.AccessToken);
    
} while (deferredResponse.IssuancePending == true);

Console.WriteLine($"Deferred credential: {deferredResponse.Credential}");
```

## 🏗️ Architecture

### Modular Design
The library is organized with each model class in its own file for better maintainability:

```
SdJwt.Net.Oid4Vci/
├── Models/
│   ├── Oid4VciConstants.cs              # Protocol constants
│   ├── CredentialOffer.cs               # Credential offer model
│   ├── CredentialRequest.cs             # Credential request model
│   ├── CredentialResponse.cs            # Credential response model
│   ├── CredentialProof.cs               # Proof of possession model
│   ├── TokenRequest.cs                  # Token request model
│   ├── TokenResponse.cs                 # Token response model
│   ├── TransactionCode.cs               # PIN/transaction code model
│   ├── PreAuthorizedCodeGrant.cs        # Pre-authorized grant model
│   ├── AuthorizationCodeGrant.cs        # Authorization grant model
│   ├── DeferredCredentialModels.cs      # Deferred issuance models
│   ├── CredentialNotificationModels.cs  # Notification models
│   └── *ErrorResponse.cs                # Error response models
├── Issuer/
│   ├── CredentialOfferBuilder.cs        # Fluent offer builder
│   └── CNonceValidator.cs               # Nonce validation utilities
└── Client/
    ├── CredentialOfferParser.cs         # Offer parsing utilities
    └── ProofBuilder.cs                  # Proof creation utilities
```

### Transport Agnostic
This library provides data models and utilities only. You provide the HTTP transport:

```csharp
// Example with HttpClient (you implement the HTTP calls)
public class Oid4VciClient
{
    private readonly HttpClient _httpClient;
    
    public async Task<TokenResponse> RequestTokenAsync(
        string tokenEndpoint, TokenRequest request)
    {
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(tokenEndpoint, content);
        
        // Handle response and deserialize TokenResponse
        // Error handling, status code validation, etc.
    }
}
```

## 📖 Complete OID4VCI Flow Example

```csharp
using SdJwt.Net.Oid4Vci.Models;
using SdJwt.Net.Oid4Vci.Issuer;
using SdJwt.Net.Oid4Vci.Client;

// 1. Issuer creates credential offer
var offer = CredentialOfferBuilder
    .Create("https://university.example.com")
    .AddConfigurationId("UniversityDegree_SDJWT")
    .UsePreAuthorizedCode("auth-code-123", pinLength: 4)
    .Build();

var offerUri = CredentialOfferParser.CreateUri(offer);

// 2. Wallet scans QR code and parses offer
var parsedOffer = CredentialOfferParser.Parse(offerUri);
var preAuthGrant = parsedOffer.GetPreAuthorizedCodeGrant()!;

// 3. Wallet requests access token
var tokenRequest = new TokenRequest
{
    GrantType = Oid4VciConstants.GrantTypes.PreAuthorizedCode,
    PreAuthorizedCode = preAuthGrant.PreAuthorizedCode,
    TransactionCode = "1234" // User enters PIN
};

// 4. Wallet creates proof of possession
var proofJwt = ProofBuilder
    .CreateJwtProof(walletKey, SecurityAlgorithms.EcdsaSha256)
    .WithAudience(parsedOffer.CredentialIssuer)
    .WithNonce(tokenResponse.CNonce!)
    .Build();

// 5. Wallet requests credential
var credentialRequest = CredentialRequest.Create("UniversityDegree", proofJwt);

// 6. Process credential response
if (credentialResponse.Credential != null)
{
    // Immediate issuance
    await StoreCredential(credentialResponse.Credential);
}
else if (credentialResponse.AcceptanceToken != null)
{
    // Deferred issuance - poll later
    await PollForDeferredCredential(credentialResponse.AcceptanceToken);
}
```

## 🔐 Security Features

### Proof Validation
```csharp
using SdJwt.Net.Oid4Vci.Issuer;

// Validate proof of possession JWT
try
{
    var result = CNonceValidator.ValidateProof(
        proofJwt: request.Proof.Jwt,
        expectedCNonce: storedNonce,
        expectedIssuerUrl: "https://issuer.example.com"
    );
    
    Console.WriteLine($"Valid proof from: {result.ClientId}");
    var holderPublicKey = result.PublicKey; // Extract holder's public key
}
catch (ProofValidationException ex)
{
    return new CredentialErrorResponse 
    { 
        Error = Oid4VciConstants.CredentialErrorCodes.InvalidProof,
        ErrorDescription = ex.Message
    };
}
```

### Secure Nonce Generation
```csharp
// Generate cryptographically secure nonces
var nonce = CNonceValidator.GenerateNonce(32);
var shortNonce = CNonceValidator.GenerateNonce(16);
```

### Error Handling
```csharp
// Create standardized error responses
var errorResponse = CredentialErrorResponse.Create(
    error: Oid4VciConstants.CredentialErrorCodes.UnsupportedCredentialType,
    description: "The requested credential type is not supported",
    errorUri: "https://issuer.example.com/errors#unsupported_type"
);

var tokenError = TokenErrorResponse.Create(
    error: Oid4VciConstants.TokenErrorCodes.InvalidGrant,
    description: "The pre-authorized code has expired"
);
```

## 📊 Supported Grant Types

### Pre-Authorized Code Flow
Perfect for in-person credential issuance:

```csharp
var offer = CredentialOfferBuilder
    .Create("https://issuer.example.com")
    .AddConfigurationId("DriversLicense")
    .UsePreAuthorizedCode("pre-auth-123", 
        pinLength: 6, 
        inputMode: Oid4VciConstants.InputModes.Numeric)
    .Build();
```

### Authorization Code Flow  
Standard OAuth 2.0 flow for web-based issuance:

```csharp
var offer = CredentialOfferBuilder
    .Create("https://issuer.example.com") 
    .AddConfigurationId("UniversityDegree")
    .UseAuthorizationCode(
        issuerState: "session-123",
        authorizationServer: "https://auth.university.example.com")
    .Build();
```

### Custom Grants
Support for future or proprietary grant types:

```csharp
var customGrant = new { custom_parameter = "value" };
var offer = CredentialOfferBuilder
    .Create("https://issuer.example.com")
    .AddConfigurationId("CustomCredential")
    .AddCustomGrant("urn:custom:grant-type", customGrant)
    .Build();
```

## 🌟 Advanced Features

### Credential Notifications
Track credential lifecycle events:

```csharp
// Wallet notifies issuer about credential acceptance
var notification = new CredentialNotificationRequest
{
    NotificationId = credentialResponse.NotificationId!,
    Event = CredentialNotificationEvents.CredentialAccepted,
    EventDescription = "Credential successfully stored in wallet"
};

// Send notification (transport implementation not included)
await HttpClient.PostNotificationAsync(notification);
```

### Batch Operations
Issue multiple credentials in one flow:

```csharp
var offer = CredentialOfferBuilder
    .Create("https://issuer.example.com")
    .AddConfigurationIds("DriversLicense", "VehicleRegistration", "InsuranceCard")
    .UsePreAuthorizedCode("batch-auth-code-456")
    .Build();

// Wallet can request each credential separately using the same access token
foreach (var configId in offer.CredentialConfigurationIds)
{
    var request = CredentialRequest.CreateByIdentifier(configId, proofJwt);
    var response = await RequestCredentialAsync(request);
    await ProcessCredentialAsync(response);
}
```

## 🔧 Integration Examples

### ASP.NET Core Minimal API

```csharp
using SdJwt.Net.Oid4Vci.Models;

var app = WebApplication.Create(args);

// Token endpoint
app.MapPost("/token", async (TokenRequest request) =>
{
    // Validate request
    if (request.GrantType != Oid4VciConstants.GrantTypes.PreAuthorizedCode)
    {
        return Results.BadRequest(TokenErrorResponse.Create(
            Oid4VciConstants.TokenErrorCodes.UnsupportedGrantType));
    }
    
    // Validate pre-authorized code
    if (!await ValidatePreAuthorizedCode(request.PreAuthorizedCode))
    {
        return Results.BadRequest(TokenErrorResponse.Create(
            Oid4VciConstants.TokenErrorCodes.InvalidGrant));
    }
    
    // Issue access token
    var tokenResponse = new TokenResponse
    {
        AccessToken = GenerateAccessToken(),
        TokenType = Oid4VciConstants.TokenTypes.Bearer,
        ExpiresIn = 3600,
        CNonce = CNonceValidator.GenerateNonce(),
        CNonceExpiresIn = 300
    };
    
    return Results.Ok(tokenResponse);
});

// Credential endpoint
app.MapPost("/credential", async (CredentialRequest request) =>
{
    // Validate proof of possession
    var proofResult = CNonceValidator.ValidateProof(
        request.Proof.Jwt, expectedNonce, issuerUrl);
        
    // Issue credential
    var credential = await IssueCredential(request.Vct, proofResult.PublicKey);
    
    return Results.Ok(CredentialResponse.Success(credential));
});
```

### Controller-Based API

```csharp
using Microsoft.AspNetCore.Mvc;
using SdJwt.Net.Oid4Vci.Models;

[ApiController]
[Route("api/oid4vci")]
public class Oid4VciController : ControllerBase
{
    [HttpPost("token")]
    public async Task<ActionResult<TokenResponse>> Token([FromBody] TokenRequest request)
    {
        try
        {
            // Validate and process token request
            var response = await ProcessTokenRequest(request);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            var error = TokenErrorResponse.Create(
                Oid4VciConstants.TokenErrorCodes.InvalidRequest, 
                ex.Message);
            return BadRequest(error);
        }
    }
    
    [HttpPost("credential")]
    public async Task<ActionResult<CredentialResponse>> Credential(
        [FromBody] CredentialRequest request)
    {
        try 
        {
            // Extract and validate access token
            var accessToken = ExtractBearerToken(Request);
            await ValidateAccessToken(accessToken);
            
            // Process credential request
            var response = await ProcessCredentialRequest(request);
            return Ok(response);
        }
        catch (ProofValidationException ex)
        {
            var error = CredentialErrorResponse.Create(
                Oid4VciConstants.CredentialErrorCodes.InvalidProof,
                ex.Message);
            return BadRequest(error);
        }
    }
}
```

## 🧪 Testing

Comprehensive test suite included:

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test category
dotnet test --filter Category=OID4VCI
```

Example test structure:
```csharp
[Fact]
public void CredentialOffer_Serialization_ProducesCorrectJson()
{
    var offer = new CredentialOffer
    {
        CredentialIssuer = "https://issuer.example.com",
        CredentialConfigurationIds = new[] { "DriversLicense" }
    };
    
    offer.AddPreAuthorizedCodeGrant("code-123", 
        new TransactionCode { Length = 4, InputMode = "numeric" });
    
    var json = JsonSerializer.Serialize(offer);
    
    Assert.Contains("\"credential_issuer\"", json);
    Assert.Contains("\"pre-authorized_code\"", json);
}
```

## 📚 API Reference

### Constants
All protocol constants are organized in `Oid4VciConstants`:
- `GrantTypes` - Grant type identifiers
- `ProofTypes` - Proof type identifiers  
- `TokenErrorCodes` - RFC 6749 error codes
- `CredentialErrorCodes` - OID4VCI specific error codes
- `InputModes` - Transaction code input modes

### Models
Each model is in its own file for better organization:
- **Offers**: `CredentialOffer`, `TransactionCode`, `PreAuthorizedCodeGrant`, `AuthorizationCodeGrant`
- **Requests**: `TokenRequest`, `CredentialRequest`, `DeferredCredentialRequest`
- **Responses**: `TokenResponse`, `CredentialResponse`, `DeferredCredentialResponse`  
- **Proofs**: `CredentialProof`
- **Errors**: `TokenErrorResponse`, `CredentialErrorResponse`
- **Notifications**: `CredentialNotificationRequest`, `CredentialNotificationResponse`

### Builders
- `CredentialOfferBuilder` - Fluent API for creating offers
- `ProofBuilder` - Helper for creating proof JWTs

### Utilities
- `CredentialOfferParser` - Parse offers from URIs
- `CNonceValidator` - Proof validation and nonce utilities

## 📈 Performance

### Optimizations
- **Memory Efficient**: Minimal allocations in critical paths
- **Fast JSON**: Optimized serialization with `System.Text.Json`
- **Async Throughout**: Non-blocking operations
- **Multi-Target**: Platform-specific optimizations

### Benchmarks
Performance is measured and optimized for:
- Credential offer creation and parsing
- Proof validation operations
- JSON serialization/deserialization
- URI encoding/decoding

## 🤝 Contributing

We welcome contributions! Please see our [Contributing Guide](../../CONTRIBUTING.md).

### Development Setup
```bash
git clone https://github.com/thomas-tran/sd-jwt-dotnet.git
cd sd-jwt-dotnet
dotnet restore
dotnet build
dotnet test
```

### Code Style
- Follow existing naming conventions
- Add XML documentation for public APIs
- Include unit tests for new features
- Update README for API changes

## 📄 License

This project is licensed under the Apache License 2.0 - see the [LICENSE](../../LICENSE) file for details.

## 📖 Related Specifications

- **[OpenID4VCI 1.0](https://openid.net/specs/openid-4-verifiable-credential-issuance-1_0.html)** - Core specification
- **[RFC 6749](https://tools.ietf.org/html/rfc6749)** - OAuth 2.0 Authorization Framework
- **[RFC 7636](https://tools.ietf.org/html/rfc7636)** - PKCE
- **[RFC 9101](https://tools.ietf.org/html/rfc9101)** - JWT Access Tokens
- **[SD-JWT](https://tools.ietf.org/html/rfc9901)** - Selective Disclosure for JWTs

## 🔗 Related Packages

- **[SdJwt.Net](../SdJwt.Net/README.md)** - Core SD-JWT functionality
- **[SdJwt.Net.Vc](../SdJwt.Net.Vc/README.md)** - SD-JWT Verifiable Credentials
- **[SdJwt.Net.Oid4Vp](../SdJwt.Net.Oid4Vp/README.md)** - OpenID for Verifiable Presentations
- **[SdJwt.Net.StatusList](../SdJwt.Net.StatusList/README.md)** - Status List for revocation
- **[SdJwt.Net.PresentationExchange](../SdJwt.Net.PresentationExchange/README.md)** - DIF Presentation Exchange

---

**Ready to implement OID4VCI?** Start with: `dotnet add package SdJwt.Net.Oid4Vci`