# SD-JWT Verifiable Credentials for .NET

[![NuGet Version](https://img.shields.io/nuget/v/SdJwt.Net.Vc.svg)](https://www.nuget.org/packages/SdJwt.Net.Vc/)
[![License](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](https://opensource.org/licenses/Apache-2.0)

A .NET library for creating and verifying **SD-JWT Verifiable Credentials** compliant with **draft-ietf-oauth-sd-jwt-vc-13**. Extends the core SD-JWT functionality with comprehensive W3C Verifiable Credentials support, enhanced validation, and production-ready features.

## ?? Features

- **Latest Specification**: Full implementation of draft-ietf-oauth-sd-jwt-vc-13
- **W3C Compatible**: Supports W3C Verifiable Credentials data model v1.1 and v2.0
- **Enhanced Security**: dc+sd-jwt media type with legacy vc+sd-jwt support
- **Type-Safe**: Strongly-typed VC payload models with comprehensive validation
- **Production Ready**: Built on the robust SD-JWT core library with security hardening

## ?? Installation

```bash
dotnet add package SdJwt.Net.Vc
```

> **Note**: This package automatically includes `SdJwt.Net` as a dependency.

## ?? Quick Start

### Creating SD-JWT Verifiable Credentials

```csharp
using SdJwt.Net.Vc.Issuer;
using SdJwt.Net.Vc.Models;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

// Create signing key
using var key = ECDsa.Create();
var signingKey = new ECDsaSecurityKey(key) { KeyId = "issuer-key-1" };

// Create VC issuer
var issuer = new SdJwtVcIssuer(signingKey, SecurityAlgorithms.EcdsaSha256);

// Define credential payload
var vcPayload = new SdJwtVcPayload
{
    Issuer = "https://university.example.com",
    Subject = "did:example:student123",
    IssuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
    ExpirationTime = DateTimeOffset.UtcNow.AddYears(4).ToUnixTimeSeconds(),
    AdditionalData = new Dictionary<string, object>
    {
        ["student_id"] = "2023001",
        ["given_name"] = "Alice",
        ["family_name"] = "Johnson", 
        ["degree"] = "Bachelor of Science",
        ["major"] = "Computer Science",
        ["graduation_date"] = "2023-05-15",
        ["gpa"] = 3.85,
        ["honors"] = "magna cum laude"
    }
};

// Configure selective disclosure
var options = new SdIssuanceOptions
{
    DisclosureStructure = new
    {
        given_name = true,
        family_name = true,
        gpa = true,
        honors = true
    }
};

// Create holder's public key (for key binding)
using var holderKey = ECDsa.Create();
var holderPublicJwk = JsonWebKeyConverter.ConvertFromSecurityKey(
    new ECDsaSecurityKey(holderKey));

// Issue SD-JWT VC
var result = issuer.Issue("UniversityDegree", vcPayload, options, holderPublicJwk);
Console.WriteLine($"SD-JWT VC: {result.Issuance}");
```

### Verifying SD-JWT Verifiable Credentials

```csharp
using SdJwt.Net.Vc.Verifier;

// Create VC verifier
var verifier = new SdJwtVcVerifier(async issuer => 
{
    // In production, resolve issuer's public key from trusted source
    return signingKey; // For demo purposes
});

// Configure validation parameters
var validationParams = new TokenValidationParameters
{
    ValidateIssuer = true,
    ValidIssuer = "https://university.example.com",
    ValidateAudience = false,
    ValidateLifetime = true,
    ClockSkew = TimeSpan.FromMinutes(5)
};

var kbValidationParams = new TokenValidationParameters
{
    ValidateIssuer = false,
    ValidateAudience = true,
    ValidAudience = "https://verifier.example.com",
    ValidateLifetime = false,
    IssuerSigningKey = new ECDsaSecurityKey(holderKey) // Holder's public key
};

// Verify SD-JWT VC with type validation
var result = await verifier.VerifyAsync(
    sdJwtVc, 
    validationParams, 
    kbValidationParams,
    expectedVcType: "UniversityDegree");

// Access VC-specific information
Console.WriteLine($"Credential Type: {result.VerifiableCredentialType}");
Console.WriteLine($"Issuer: {result.SdJwtVcPayload.Issuer}");
Console.WriteLine($"Subject: {result.SdJwtVcPayload.Subject}");
Console.WriteLine($"Key Binding Verified: {result.KeyBindingVerified}");

// Access disclosed claims
Console.WriteLine("Disclosed Claims:");
foreach (var (key, value) in result.SdJwtVcPayload.AdditionalData ?? new Dictionary<string, object>())
{
    Console.WriteLine($"  - {key}: {value}");
}
```

### Holder Creates Selective Presentation

```csharp
using SdJwt.Net.Holder;

// Create holder from issuance
var holder = new SdJwtHolder(result.Issuance);

// Create selective presentation (only disclose specific claims)
var presentation = holder.CreatePresentation(
    disclosure => disclosure.ClaimName == "given_name" || 
                  disclosure.ClaimName == "degree",
    new JwtPayload 
    { 
        ["aud"] = "https://employer.example.com",
        ["nonce"] = "job-application-nonce",
        ["iat"] = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
    },
    new ECDsaSecurityKey(holderKey),
    SecurityAlgorithms.EcdsaSha256
);

Console.WriteLine($"Selective Presentation: {presentation}");
```

## ?? Key Components

### SdJwtVcIssuer
- **Enhanced Validation**: Comprehensive VC structure validation per draft-13 specification
- **Type Safety**: Strongly-typed VC models with extensive property support
- **Media Type Support**: Uses modern `dc+sd-jwt` with legacy `vc+sd-jwt` compatibility
- **Key Binding**: Supports holder key binding for enhanced security

### SdJwtVcVerifier  
- **Specification Compliance**: Full validation according to draft-ietf-oauth-sd-jwt-vc-13
- **Type Validation**: Optional VC type validation for additional security
- **Header Validation**: Ensures proper `typ` claim with transition period support
- **Status Integration**: Works with StatusList for revocation checking

### Enhanced Models
- **`SdJwtVcPayload`**: Complete VC data model with all standard JWT claims
- **`SdJwtVcVerificationResult`**: Comprehensive verification results with VC-specific data
- **Status Integration**: Built-in support for StatusList references

## ?? Advanced Features

### Professional License Example

```csharp
var professionalLicense = new SdJwtVcPayload
{
    Issuer = "https://medical-board.example.com",
    Subject = "did:example:doctor456",
    IssuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
    ExpirationTime = DateTimeOffset.UtcNow.AddYears(2).ToUnixTimeSeconds(),
    AdditionalData = new Dictionary<string, object>
    {
        ["license_number"] = "MD123456",
        ["doctor_name"] = "Dr. Sarah Smith",
        ["specialization"] = "Cardiology",
        ["license_class"] = "Full Practice",
        ["board_certification"] = "American Board of Internal Medicine",
        ["issue_date"] = "2022-01-15",
        ["restrictions"] = new List<string>(), // No restrictions
        ["medical_school"] = "Harvard Medical School",
        ["graduation_year"] = 2018
    }
};

var licenseOptions = new SdIssuanceOptions
{
    DisclosureStructure = new
    {
        specialization = true,
        board_certification = true,
        medical_school = true,
        restrictions = true
    }
};

var licenseResult = issuer.Issue("MedicalLicense", professionalLicense, licenseOptions, holderPublicJwk);
```

### Employment Verification Example

```csharp
var employmentCredential = new SdJwtVcPayload
{
    Issuer = "https://company.example.com",
    Subject = "did:example:employee789",
    IssuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
    AdditionalData = new Dictionary<string, object>
    {
        ["employee_id"] = "EMP789",
        ["employee_name"] = "John Martinez",
        ["position"] = "Senior Software Engineer",
        ["department"] = "Engineering",
        ["start_date"] = "2021-03-15",
        ["employment_type"] = "Full-time",
        ["salary_range"] = "$120,000 - $140,000",
        ["security_clearance"] = "Secret",
        ["manager"] = "Jane Wilson",
        ["office_location"] = "San Francisco, CA"
    }
};

var employmentOptions = new SdIssuanceOptions
{
    DisclosureStructure = new
    {
        position = true,
        department = true,
        start_date = true,
        salary_range = true,
        security_clearance = true,
        office_location = true
    }
};

// For background check - only disclose position and employment dates
var backgroundCheckPresentation = holder.CreatePresentation(
    disclosure => disclosure.ClaimName == "position" || 
                  disclosure.ClaimName == "start_date",
    new JwtPayload 
    { 
        ["aud"] = "https://background-check.example.com",
        ["nonce"] = "background-check-123"
    },
    holderPrivateKey,
    SecurityAlgorithms.EcdsaSha256
);
```

## ?? Integration with Status Lists

For credential revocation and status management:

```csharp
using SdJwt.Net.StatusList.Models;

// Add status reference during credential issuance
var credentialWithStatus = new SdJwtVcPayload
{
    Issuer = "https://issuer.example.com",
    Subject = "did:example:holder123",
    IssuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
    // Include status reference
    Status = new { 
        status_list = new StatusListReference 
        { 
            Index = 12345, 
            Uri = "https://issuer.example.com/status/1" 
        } 
    },
    AdditionalData = new Dictionary<string, object>
    {
        ["credential_data"] = "value"
    }
};

// Status checking is automatically handled during verification
// when using SdJwtVcVerifier with StatusList package installed
```

## ?? Real-World Applications

### Digital Identity Wallet

```csharp
public class DigitalWallet
{
    private readonly SdJwtVcVerifier _verifier;
    private readonly Dictionary<string, string> _credentials = new();

    public async Task<bool> AddCredentialAsync(string credentialType, string sdJwtVc)
    {
        try
        {
            // Verify credential before adding to wallet
            var result = await _verifier.VerifyAsync(sdJwtVc, validationParams, 
                expectedVcType: credentialType);
                
            _credentials[credentialType] = sdJwtVc;
            return true;
        }
        catch (SecurityTokenException)
        {
            return false; // Invalid credential
        }
    }

    public async Task<string?> CreatePresentationAsync(
        string credentialType, 
        string[] claimsToDisclose,
        string audience)
    {
        if (!_credentials.TryGetValue(credentialType, out var sdJwtVc))
            return null;

        var holder = new SdJwtHolder(sdJwtVc);
        return holder.CreatePresentation(
            disclosure => claimsToDisclose.Contains(disclosure.ClaimName),
            new JwtPayload { ["aud"] = audience, ["nonce"] = Guid.NewGuid().ToString() },
            holderPrivateKey,
            SecurityAlgorithms.EcdsaSha256
        );
    }
}
```

### Enterprise Verification Service

```csharp
public class EnterpriseVerificationService
{
    private readonly SdJwtVcVerifier _verifier;
    private readonly ILogger<EnterpriseVerificationService> _logger;

    public async Task<VerificationReport> VerifyCredentialAsync(
        string presentation, 
        string expectedIssuer,
        string expectedType)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            var validationParams = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = expectedIssuer,
                ValidateAudience = true,
                ValidAudience = "https://company.example.com",
                ValidateLifetime = true
            };

            var result = await _verifier.VerifyAsync(
                presentation, validationParams, kbValidationParams, expectedType);

            _logger.LogInformation(
                "Credential verification successful for type {CredentialType} from issuer {Issuer}",
                result.VerifiableCredentialType, result.SdJwtVcPayload.Issuer);

            return new VerificationReport
            {
                IsValid = true,
                CredentialType = result.VerifiableCredentialType,
                Issuer = result.SdJwtVcPayload.Issuer,
                Subject = result.SdJwtVcPayload.Subject,
                DisclosedClaims = result.SdJwtVcPayload.AdditionalData?.Keys.ToList() ?? new(),
                VerificationTime = stopwatch.Elapsed
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Credential verification failed");
            return new VerificationReport { IsValid = false, Error = ex.Message };
        }
    }
}

public class VerificationReport
{
    public bool IsValid { get; set; }
    public string? CredentialType { get; set; }
    public string? Issuer { get; set; }
    public string? Subject { get; set; }
    public List<string> DisclosedClaims { get; set; } = new();
    public TimeSpan VerificationTime { get; set; }
    public string? Error { get; set; }
}
```

## ?? Specification Compliance

### draft-ietf-oauth-sd-jwt-vc-13 Features

- **Media Types**: Full support for `dc+sd-jwt` with `vc+sd-jwt` legacy compatibility
- **Header Validation**: Ensures proper `typ` claim according to specification
- **VCT Claim**: Validates verifiable credential type consistency
- **Payload Structure**: Complete W3C VC data model support

### Security Enhancements

- **Type Validation**: Optional verification of expected credential types
- **Temporal Validation**: Proper handling of `iat`, `exp`, `nbf` claims
- **Key Binding**: Enhanced security through holder key binding
- **Status Integration**: Automatic revocation checking with StatusList

## ?? Security Considerations

### Production Deployment

```csharp
// Recommended production configuration
var productionVerifier = new SdJwtVcVerifier(
    keyProvider: async issuer => await ResolveIssuerKey(issuer),
    logger: logger);

var secureValidationParams = new TokenValidationParameters
{
    // Security requirements
    ValidateIssuer = true,
    ValidateAudience = true,
    ValidateLifetime = true,
    ValidateIssuerSigningKey = true,
    RequireExpirationTime = true,
    RequireSignedTokens = true,
    ClockSkew = TimeSpan.FromMinutes(2), // Minimal clock skew
    
    // Issuer validation
    ValidIssuers = GetTrustedIssuers(),
    
    // Audience validation  
    ValidAudiences = GetValidAudiences()
};
```

### Key Management Best Practices

```csharp
public class SecureKeyProvider
{
    private readonly IMemoryCache _keyCache;
    private readonly HttpClient _httpClient;
    
    public async Task<SecurityKey> ResolveIssuerKeyAsync(string issuer)
    {
        // Check cache first
        if (_keyCache.TryGetValue(issuer, out SecurityKey? cachedKey))
            return cachedKey!;
            
        // Resolve from well-known endpoint with validation
        var key = await ResolveKeyFromWellKnownEndpoint(issuer);
        
        // Validate key meets security requirements
        ValidateKeyStrength(key);
        
        // Cache with appropriate TTL
        _keyCache.Set(issuer, key, TimeSpan.FromHours(1));
        
        return key;
    }
}
```

## ?? Related Packages

- **[SdJwt.Net](https://www.nuget.org/packages/SdJwt.Net/)** - Core SD-JWT functionality (dependency)
- **[SdJwt.Net.StatusList](https://www.nuget.org/packages/SdJwt.Net.StatusList/)** - Credential revocation and status management

## ?? License

Licensed under the Apache License 2.0. See [LICENSE](LICENSE) for details.

---

**Ready for Production?**
- **Enhanced Security**: Automatic algorithm validation and security hardening
- **Comprehensive Testing**: 4+ comprehensive test suites covering real-world scenarios
- **Enterprise Ready**: Built for scale with performance optimizations and monitoring support