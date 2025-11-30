# SD-JWT.NET - OpenID for Verifiable Presentations (OID4VP)

[![NuGet Version](https://img.shields.io/nuget/v/SdJwt.Net.Oid4Vp.svg)](https://www.nuget.org/packages/SdJwt.Net.Oid4Vp/)
[![Build Status](https://github.com/thomas-tran/sd-jwt-dotnet/actions/workflows/ci-cd.yml/badge.svg)](https://github.com/thomas-tran/sd-jwt-dotnet/actions)
[![License](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](https://opensource.org/licenses/Apache-2.0)

A comprehensive, transport-agnostic .NET implementation of **OpenID for Verifiable Presentations (OID4VP) 1.0** protocol. This library provides complete data models, utilities, and validators for implementing verifiable presentation flows with SD-JWT credentials and Presentation Exchange support.

## 🚀 Features

### 📋 Complete OID4VP 1.0 Compliance
- **Cross-Device Flow**: QR code-based presentation request and response using `response_mode=direct_post`
- **Presentation Exchange v2.0.0**: Full DIF Presentation Exchange support with complex presentation definitions
- **Security-First**: Comprehensive validation including signature verification and key binding validation
- **Transport-Agnostic**: Pure data models and utilities, works with any HTTP framework

### 🔧 Modular Architecture
- **Separate Model Files**: Each class in its own file for better maintainability
- **Fluent Builder APIs**: Easy-to-use builders for creating presentation requests
- **Strong Typing**: Comprehensive validation and type safety throughout
- **Flexible Validation**: Configurable validation options for different scenarios

### 🔒 Security & Standards
- **Key Binding Validation**: Ensures the presenter owns the credential through cryptographic proof
- **Nonce Verification**: Proper nonce handling for replay attack prevention
- **Signature Validation**: Complete SD-JWT signature verification using the core library
- **Error Handling**: Comprehensive error responses following OID4VP specifications

## 📦 Installation

```bash
dotnet add package SdJwt.Net.Oid4Vp
```

## 🎯 Quick Start

### Verifier: Creating a Presentation Request

```csharp
using SdJwt.Net.Oid4Vp.Verifier;
using SdJwt.Net.Oid4Vp.Models;

// Create a simple credential request
var request = PresentationRequestBuilder
    .Create("https://verifier.example.com", "https://verifier.example.com/response")
    .WithName("University Verification")
    .WithPurpose("We need to verify your university degree for employment.")
    .RequestCredential("UniversityDegree")
    .Build();

// Generate QR code URI for cross-device flow
var qrCodeUri = PresentationRequestBuilder
    .Create("https://verifier.example.com", "https://verifier.example.com/response")
    .RequestCredential("DriversLicense", "Please present your drivers license")
    .BuildUri();

Console.WriteLine($"Show this QR code to user: {qrCodeUri}");
// Output: openid4vp://?request=%7B%22client_id%22%3A%22https%3A...
```

### Wallet: Parsing a Presentation Request

```csharp
using SdJwt.Net.Oid4Vp.Client;

// Parse presentation request from QR code
var request = AuthorizationRequestParser.Parse(qrCodeUri);

Console.WriteLine($"Verifier: {request.ClientId}");
Console.WriteLine($"Purpose: {request.PresentationDefinition?.Purpose}");

// Check what credentials are being requested
var firstDescriptor = request.PresentationDefinition?.InputDescriptors[0];
if (firstDescriptor?.Constraints?.Fields != null)
{
    foreach (var field in firstDescriptor.Constraints.Fields)
    {
        Console.WriteLine($"Required field: {string.Join(", ", field.Path)}");
    }
}
```

### Verifier: Validating VP Tokens

```csharp
using SdJwt.Net.Oid4Vp.Verifier;
using SdJwt.Net.Oid4Vp.Models;

// Create VP token validator
var keyProvider = async (JwtSecurityToken jwt) =>
{
    // Return the appropriate SecurityKey for verification
    // This could fetch from a key registry, DID document, etc.
    return await GetSecurityKeyAsync(jwt);
};

var validator = new VpTokenValidator(keyProvider);

// Validation options
var options = new VpTokenValidationOptions
{
    ValidateIssuer = true,
    ValidIssuers = new[] { "https://university.example.com" },
    ValidateLifetime = true,
    ClockSkew = TimeSpan.FromMinutes(5)
};

// Validate the response
var authResponse = /* received from wallet */;
var expectedNonce = /* nonce from original request */;

var result = await validator.ValidateAsync(authResponse, expectedNonce, options);

if (result.IsValid)
{
    Console.WriteLine("✅ Presentation verified successfully!");
    
    foreach (var token in result.ValidatedTokens)
    {
        Console.WriteLine($"Token {token.Index}: {token.Claims.Count} claims extracted");
        
        // Access specific claims
        if (token.Claims.TryGetValue("given_name", out var givenName))
        {
            Console.WriteLine($"Given name: {givenName}");
        }
    }
}
else
{
    Console.WriteLine($"❌ Validation failed: {result.Error}");
}
```

## 🏗️ Architecture

### Core Components

```
SdJwt.Net.Oid4Vp/
├── Models/                           # Data Models
│   ├── Oid4VpConstants.cs           # Protocol constants
│   ├── AuthorizationRequest.cs       # QR code payload
│   ├── AuthorizationResponse.cs      # Wallet response
│   ├── PresentationDefinition.cs    # PE definition
│   ├── PresentationSubmission.cs    # PE submission
│   ├── InputDescriptor.cs           # Input requirements
│   ├── Constraints.cs               # Field constraints
│   ├── Field.cs                     # Individual field rules
│   ├── SubmissionRequirement.cs     # Submission rules
│   └── InputDescriptorMapping.cs    # Response mappings
├── Verifier/                        # Verifier Utilities
│   ├── PresentationRequestBuilder.cs # Request builder
│   └── VpTokenValidator.cs          # Response validator
└── Client/                          # Wallet Utilities
    └── AuthorizationRequestParser.cs # Request parser
```

### Transport Agnostic Design

This library provides data models and utilities only. You implement the HTTP transport:

```csharp
// Example with ASP.NET Core
[HttpPost("/presentation/response")]
public async Task<IActionResult> ReceivePresentationResponse(
    [FromForm] AuthorizationResponse response)
{
    try
    {
        // Validate the response
        var result = await _vpTokenValidator.ValidateAsync(
            response, _sessionService.GetNonce(), _validationOptions);
        
        if (result.IsValid)
        {
            // Process successful presentation
            await _presentationService.ProcessVerifiedPresentation(result);
            return Ok(new { status = "verified" });
        }
        else
        {
            return BadRequest(new { error = result.Error });
        }
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { error = "Validation failed" });
    }
}
```

## 📖 Complete OID4VP Flow Example

### 1. Verifier Creates Request

```csharp
// Create a complex presentation request
var request = PresentationRequestBuilder
    .Create("https://employer.example.com", "https://employer.example.com/verify")
    .WithName("Employment Verification")
    .WithPurpose("We need to verify your identity and qualifications for this position.")
    .WithState("session-12345")
    
    // Request university degree
    .RequestCredentialFromIssuer(
        credentialType: "UniversityDegree",
        issuer: "https://university.example.com",
        purpose: "Verify educational qualifications")
    
    // Request identity credential
    .RequestCredential(
        credentialType: "NationalID", 
        purpose: "Verify your identity")
    
    // Require both credentials
    .RequireAll("All Documents", "Both credentials are required")
    
    // Add custom field constraints
    .WithField(Field.CreateForPath("$.degree_level")
        .WithStringEnum("Bachelor", "Master", "PhD"))
    
    .Build();

// Generate QR code
var qrUri = PresentationRequestBuilder
    .Create("https://employer.example.com", "https://employer.example.com/verify")
    // ... same configuration as above
    .BuildUri();
```

### 2. Wallet Parses and Responds

```csharp
// Parse the request
var parsedRequest = AuthorizationRequestParser.Parse(qrUri);

// Find matching credentials (implementation specific)
var matchingCredentials = await _walletService.FindMatchingCredentials(
    parsedRequest.PresentationDefinition!);

// Create presentation submission
var submission = PresentationSubmission.CreateMultiple(
    id: "submission-" + Guid.NewGuid(),
    definitionId: parsedRequest.PresentationDefinition.Id,
    InputDescriptorMapping.CreateForSdJwt("input_1", 0), // University degree
    InputDescriptorMapping.CreateForSdJwt("input_2", 1)  // National ID
);

// Create the response
var vpTokens = new[]
{
    await _walletService.CreatePresentation(matchingCredentials[0], parsedRequest.Nonce),
    await _walletService.CreatePresentation(matchingCredentials[1], parsedRequest.Nonce)
};

var response = AuthorizationResponse.Success(vpTokens, submission, parsedRequest.State);

// Send to verifier (HTTP POST to response_uri)
await _httpClient.PostAsJsonAsync(parsedRequest.ResponseUri!, response);
```

### 3. Verifier Validates Response

```csharp
// Comprehensive validation
var options = new VpTokenValidationOptions
{
    ValidateIssuer = true,
    ValidIssuers = new[] 
    { 
        "https://university.example.com", 
        "https://government.example.com" 
    },
    ValidateLifetime = true,
    ValidateKeyBindingLifetime = true,
    
    // Custom validation
    CustomValidation = async (verificationResult, cancellationToken) =>
    {
        // Check if degree is from last 10 years
        var claims = verificationResult.ClaimsPrincipal.Claims;
        var gradDateClaim = claims.FirstOrDefault(c => c.Type == "graduation_date");
        
        if (gradDateClaim != null && DateTime.TryParse(gradDateClaim.Value, out var gradDate))
        {
            if (DateTime.UtcNow.Subtract(gradDate).TotalDays > 365 * 10)
            {
                return CustomValidationResult.Failed("Degree is too old");
            }
        }
        
        return CustomValidationResult.Success();
    }
};

var result = await validator.ValidateAsync(response, originalNonce, options);

if (result.IsValid)
{
    // Extract verified information
    var universityToken = result.ValidatedTokens[0];
    var identityToken = result.ValidatedTokens[1];
    
    var applicantName = identityToken.Claims["full_name"];
    var degreeLevel = universityToken.Claims["degree_level"];
    var university = universityToken.VerificationResult?.ClaimsPrincipal.FindFirst("iss")?.Value;
    
    Console.WriteLine($"Verified: {applicantName} has {degreeLevel} from {university}");
}
```

## 🌟 Advanced Features

### Complex Presentation Definitions

```csharp
// Create sophisticated requirements
var builder = PresentationRequestBuilder
    .Create("https://verifier.example.com", "https://verifier.example.com/response")
    .WithName("Multi-Credential Verification")
    
    // Multiple education credentials
    .RequestCredential("UniversityDegree", "University degree required")
    .RequestCredential("HighSchoolDiploma", "High school diploma required") 
    .RequestCredential("ProfessionalCertification", "Professional cert required")
    
    // Pick any 2 out of 3 educational credentials
    .RequirePickRange(
        min: 2, 
        max: 3,
        name: "Educational Requirements",
        purpose: "At least 2 educational credentials required")
    
    // Identity verification (separate requirement)
    .RequestCredential("GovernmentID", "Government issued ID required")
    
    // Custom field constraints
    .WithField(Field.CreateForPath("$.age")
        .WithFilter(new Dictionary<string, object>
        {
            ["type"] = "number",
            ["minimum"] = 18,
            ["maximum"] = 65
        }))
        
    .Build();
```

### Status List Integration

```csharp
var options = new VpTokenValidationOptions
{
    CustomValidation = async (verificationResult, cancellationToken) =>
    {
        // Check credential status using Status List
        var statusClaim = verificationResult.ClaimsPrincipal.FindFirst("status")?.Value;
            
        if (!string.IsNullOrEmpty(statusClaim))
        {
            var isRevoked = await _statusListService.IsRevokedAsync(statusClaim, cancellationToken);
            if (isRevoked)
            {
                return CustomValidationResult.Failed("Credential has been revoked");
            }
        }
        
        return CustomValidationResult.Success();
    }
};
```

### Error Handling

```csharp
// Create error response
var errorResponse = AuthorizationResponse.Error(
    error: Oid4VpConstants.ErrorCodes.AccessDenied,
    errorDescription: "User denied the presentation request",
    state: originalRequest.State);

// Detailed validation errors
try
{
    var result = await validator.ValidateAsync(response, nonce, options);
}
catch (Exception ex)
{
    _logger.LogError(ex, "VP token validation failed");
    
    // Return appropriate error response
    return AuthorizationResponse.Error(
        Oid4VpConstants.ErrorCodes.InvalidRequest,
        "Presentation validation failed");
}
```

## 🧪 Testing

Comprehensive test suite with full coverage:

```bash
# Run all tests
dotnet test

# Run specific test category
dotnet test --filter Category=OID4VP

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

Example test pattern:
```csharp
[Fact]
public void PresentationRequestBuilder_WithComplexRequirements_CreatesValidRequest()
{
    var builder = PresentationRequestBuilder
        .Create("https://verifier.example.com", "https://verifier.example.com/response")
        .RequestCredential("UniversityDegree")
        .RequestCredential("DriverLicense")
        .RequirePick(1, "Educational OR Driving", "Either credential is acceptable");

    var request = builder.Build();
    
    Assert.Equal(2, request.PresentationDefinition!.InputDescriptors.Length);
    Assert.Single(request.PresentationDefinition.SubmissionRequirements!);
    Assert.Equal("pick", request.PresentationDefinition.SubmissionRequirements[0].Rule);
}
```

## 📚 API Reference

### Models
- **`AuthorizationRequest`** - The QR code payload with presentation requirements
- **`AuthorizationResponse`** - Wallet response with VP tokens and submission
- **`PresentationDefinition`** - Defines what credentials are required (PE v2.0.0)
- **`InputDescriptor`** - Specific credential requirements within a definition
- **`Constraints`** - Field-level constraints and selective disclosure rules
- **`Field`** - Individual field requirements with JSONPath and filters
- **`SubmissionRequirement`** - Rules for how inputs must be submitted (all/pick)
- **`PresentationSubmission`** - Maps provided credentials to requirements
- **`InputDescriptorMapping`** - Maps individual credentials to descriptors

### Builders
- **`PresentationRequestBuilder`** - Fluent API for creating authorization requests
- **`Field.Create*()`** - Factory methods for common field constraints
- **`SubmissionRequirement.Require*()`** - Factory methods for submission rules

### Validators
- **`VpTokenValidator`** - Validates authorization responses and VP tokens
- **`AuthorizationRequestParser`** - Parses and validates authorization request URIs

### Constants
- **`Oid4VpConstants`** - All protocol constants organized by category:
  - `ResponseModes` - direct_post, direct_post.jwt, fragment, query
  - `ResponseTypes` - vp_token
  - `ClientIdSchemes` - redirect_uri, entity_id, did, web, x509_san_*
  - `ErrorCodes` - Standard OID4VP error codes
  - `PresentationExchange` - PE v2.0.0 specific constants

## 📈 Performance

### Optimizations
- **Memory Efficient**: Minimal allocations in validation paths
- **Fast JSON**: Optimized serialization with `System.Text.Json`
- **Async Throughout**: Non-blocking operations for I/O
- **Configurable Validation**: Skip unnecessary checks for performance

### Benchmarks
Performance measured for:
- Authorization request creation and parsing
- VP token validation (full flow)
- Presentation definition matching
- URI encoding/decoding

## 🔒 Security Considerations

### Validation Best Practices

```csharp
// Secure validation configuration
var options = new VpTokenValidationOptions
{
    // Always validate signatures
    ValidateIssuer = true,
    ValidIssuers = GetTrustedIssuers(),
    
    // Validate token lifetime
    ValidateLifetime = true,
    ClockSkew = TimeSpan.FromMinutes(2), // Minimal skew
    
    // Require key binding
    ValidateKeyBindingLifetime = true,
    
    // Custom validations for business logic
    CustomValidation = ValidateBusinessRules
};
```

### Nonce Management

```csharp
// Generate cryptographically secure nonce
var builder = PresentationRequestBuilder
    .Create(clientId, responseUri)
    .WithNonce(GenerateSecureNonce(32))  // 32 bytes = 256 bits
    .RequestCredential("Credential");

// Store nonce with expiration
await _nonceService.StoreNonceAsync(builder.GetNonce(), TimeSpan.FromMinutes(10));
```

### Error Information Disclosure

```csharp
// Avoid leaking sensitive information in errors
public IActionResult HandleValidationError(string error)
{
    _logger.LogWarning("Validation failed: {Error}", error);
    
    // Return generic error to client
    return BadRequest(AuthorizationResponse.Error(
        Oid4VpConstants.ErrorCodes.InvalidRequest,
        "The presentation could not be validated"));
}
```

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
- Follow existing patterns and naming conventions
- Add comprehensive XML documentation
- Include unit tests for new features
- Update README for API changes

## 📄 License

This project is licensed under the Apache License 2.0 - see the [LICENSE](../../LICENSE) file for details.

## 📖 Related Specifications

- **[OpenID4VP 1.0](https://openid.net/specs/openid-4-verifiable-presentations-1_0.html)** - Core specification
- **[DIF Presentation Exchange v2.0.0](https://identity.foundation/presentation-exchange/)** - Presentation definition format
- **[RFC 9101](https://tools.ietf.org/html/rfc9101)** - JWT Access Tokens  
- **[SD-JWT](https://tools.ietf.org/html/rfc9901)** - Selective Disclosure for JWTs
- **[OID4VCI 1.0](https://openid.net/specs/openid-4-verifiable-credential-issuance-1_0.html)** - Credential issuance

## 🔗 Related Packages

- **[SdJwt.Net](../SdJwt.Net/README.md)** - Core SD-JWT functionality
- **[SdJwt.Net.Vc](../SdJwt.Net.Vc/README.md)** - SD-JWT Verifiable Credentials
- **[SdJwt.Net.Oid4Vci](../SdJwt.Net.Oid4Vci/README.md)** - OpenID for Verifiable Credential Issuance
- **[SdJwt.Net.StatusList](../SdJwt.Net.StatusList/README.md)** - Status List for revocation
- **[SdJwt.Net.PresentationExchange](../SdJwt.Net.PresentationExchange/README.md)** - DIF Presentation Exchange
- **[SdJwt.Net.OidFederation](../SdJwt.Net.OidFederation/README.md)** - OpenID Federation

---

**Ready to implement OID4VP verification flows?** Start with: `dotnet add package SdJwt.Net.Oid4Vp`