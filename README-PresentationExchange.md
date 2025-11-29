# SdJwt.Net.PresentationExchange

[![NuGet](https://img.shields.io/nuget/v/SdJwt.Net.PresentationExchange.svg)](https://www.nuget.org/packages/SdJwt.Net.PresentationExchange)
[![License](https://img.shields.io/badge/license-Apache%202.0-blue.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-8%2B-purple.svg)]()

A comprehensive .NET implementation of the **DIF Presentation Exchange 2.1.1** specification. This library provides intelligent credential selection, constraint evaluation, and presentation submission capabilities for SD-JWT and other verifiable credential ecosystems.

## ?? The Problem This Solves

When a **Verifier** requests credentials from a **Holder's wallet**, they send a complex query called a **Presentation Definition**. For example:

> "I need a government ID, specifically the 'age' field, and it must be from one of these 3 trusted issuers"

Without Presentation Exchange, developers write brittle if/else logic to parse these requests and match credentials. This library eliminates that complexity by implementing the standard algorithmic approach.

## ?? Key Features

- **Complete DIF PEX 2.1.1 Implementation**: Full support for input descriptors, constraints, submission requirements
- **Intelligent Credential Selection**: Automatically finds the best matching credentials from a wallet
- **SD-JWT Optimized**: First-class support for Selective Disclosure JWT credentials
- **Multi-Format Support**: Works with SD-JWT, JWT-VC, JSON-LD, and other credential formats
- **Complex Constraint Evaluation**: JSON Schema-based field validation and JSON path queries
- **Performance Optimized**: Configurable limits and caching for large wallets
- **Extensible Architecture**: Plugin system for custom evaluation logic

## ?? Installation

```bash
dotnet add package SdJwt.Net.PresentationExchange
```

## ?? Quick Start

### Simple Credential Selection

```csharp
using SdJwt.Net.PresentationExchange;

// Create a simple selector
var selector = PresentationExchangeFactory.CreateSimpleSelector();

// Your wallet credentials
var wallet = new[]
{
    sdJwtDriverLicense,
    jwtUniversityDegree,
    jsonPassport
};

// Select credentials by type
var selected = await selector.SelectByTypeAsync(wallet, "DriverLicense");

// Select SD-JWT with specific disclosures
var (credential, disclosures) = await selector.SelectSdJwtWithDisclosuresAsync(
    wallet, 
    "DriverLicense", 
    requiredFields: new[] { "name", "age" });
```

### Advanced Presentation Exchange

```csharp
// Create the presentation exchange engine
var engine = PresentationExchangeFactory.CreateEngine();

// Define what you want (this would typically come from a verifier)
var presentationDefinition = new PresentationDefinition
{
    Id = "age_verification",
    Name = "Age Verification",
    Purpose = "We need to verify you are over 21",
    InputDescriptors = new[]
    {
        InputDescriptor.CreateForSdJwt(
            "government_id",
            "DriverLicense",
            "Government-issued ID")
        {
            Constraints = Constraints.Create(
                Field.CreateForIssuer("https://trusted-dmv.gov"),
                Field.CreateForExistence("$.age"),
                Field.CreateForRange("$.age", minimum: 21)
            )
        }
    }
};

// Select matching credentials
var result = await engine.SelectCredentialsAsync(presentationDefinition, wallet);

if (result.IsSuccessful)
{
    Console.WriteLine($"Selected {result.SelectedCredentials.Length} credentials");
    Console.WriteLine($"Presentation submission ID: {result.PresentationSubmission?.Id}");
}
```

### Complex Selection with Submission Requirements

```csharp
var presentationDefinition = new PresentationDefinition
{
    Id = "identity_verification",
    Name = "Identity Verification",
    InputDescriptors = new[]
    {
        InputDescriptor.CreateForSdJwt("drivers_license", "DriverLicense"),
        InputDescriptor.CreateForSdJwt("state_id", "StateID"),
        InputDescriptor.CreateForJwtVc("passport", new[] { "Passport" })
    },
    // Pick any ONE of the above (flexible identity verification)
    SubmissionRequirements = new[]
    {
        SubmissionRequirement.CreatePickNested(
            new[]
            {
                SubmissionRequirement.CreateAll("drivers_license"),
                SubmissionRequirement.CreateAll("state_id"),
                SubmissionRequirement.CreateAll("passport")
            },
            count: 1)
    }
};

var result = await engine.SelectCredentialsAsync(presentationDefinition, wallet);
```

## ??? Advanced Configuration

### Performance Optimization

```csharp
var options = CredentialSelectionOptions.CreatePerformanceOptimized();
options.MaxCredentialsToEvaluate = 50;
options.MaxMatchesPerDescriptor = 3;
options.ConstraintEvaluationTimeoutMs = 1000;

var result = await engine.SelectCredentialsAsync(definition, wallet, options);
```

### SD-JWT Focused Selection

```csharp
var options = CredentialSelectionOptions.CreateSdJwtOptimized();
var result = await engine.SelectCredentialsAsync(definition, wallet, options);
```

### Dependency Injection Setup

```csharp
services.AddPresentationExchange(config =>
{
    config.DefaultSelectionOptions = CredentialSelectionOptions.CreateThoroughEvaluation();
    config.EnablePerformanceMonitoring = true;
});

// Inject and use
public class VerificationService
{
    private readonly PresentationExchangeEngine _engine;
    
    public VerificationService(PresentationExchangeEngine engine)
    {
        _engine = engine;
    }
}
```

## ?? Detailed Examples

### Building Complex Constraints

```csharp
var constraints = new Constraints
{
    Fields = new[]
    {
        // Must be a driver license
        Field.CreateForType("DriverLicense"),
        
        // From specific issuers
        Field.CreateForValues("$.iss", new[] 
        { 
            "https://ca-dmv.gov", 
            "https://ny-dmv.gov" 
        }),
        
        // Age must be present and >= 21
        Field.CreateForRange("$.age", minimum: 21),
        
        // License must not be expired
        Field.Create("$.exp", FieldFilter.CreateRange(
            minimum: DateTimeOffset.UtcNow.ToUnixTimeSeconds())),
            
        // Optional: driving privileges  
        Field.CreateForExistence("$.driving_privileges", optional: true)
    },
    
    // Require selective disclosure
    LimitDisclosure = "required"
};
```

### Custom Format Detection

```csharp
public class CustomFormatDetector : ICredentialFormatDetector
{
    public int Priority => 100;
    
    public async Task<CredentialFormatInfo?> TryDetectAsync(
        object credential, 
        CancellationToken cancellationToken = default)
    {
        if (credential is MyCustomCredentialType custom)
        {
            return new CredentialFormatInfo
            {
                Format = "custom_format",
                IsSupported = true,
                SupportsSelectiveDisclosure = custom.HasSelectiveDisclosure
            };
        }
        return null;
    }
}

var engine = PresentationExchangeFactory.CreateEngine(services =>
{
    services.AddSingleton<ICredentialFormatDetector, CustomFormatDetector>();
});
```

### Custom Evaluation Extensions

```csharp
public class TrustScoreExtension : ICredentialEvaluationExtension
{
    public string Name => "TrustScoreEvaluator";
    public int Priority => 50;
    
    public async Task<CredentialEvaluationExtensionResult> EvaluateAsync(
        object credential,
        InputDescriptor inputDescriptor,
        double currentScore,
        Dictionary<string, object> context,
        CancellationToken cancellationToken = default)
    {
        var trustScore = await CalculateTrustScore(credential);
        var modifiedScore = currentScore * (trustScore / 100.0);
        
        return CredentialEvaluationExtensionResult.WithScore(modifiedScore);
    }
}
```

## ?? Understanding Results

```csharp
var result = await engine.SelectCredentialsAsync(definition, wallet);

if (result.IsSuccessful)
{
    foreach (var selected in result.SelectedCredentials)
    {
        Console.WriteLine($"Selected credential for: {selected.InputDescriptorId}");
        Console.WriteLine($"Format: {selected.Format}");
        Console.WriteLine($"Match score: {selected.MatchScore}");
        
        if (selected.Disclosures != null)
        {
            Console.WriteLine($"Disclosures: {string.Join(", ", selected.Disclosures)}");
        }
    }
    
    // The presentation submission can be sent to the verifier
    var submissionJson = JsonSerializer.Serialize(result.PresentationSubmission);
}
else
{
    foreach (var error in result.Errors)
    {
        Console.WriteLine($"Error: {error.Code} - {error.Message}");
    }
}
```

## ?? Supported Standards

- **DIF Presentation Exchange 2.1.1**: Complete implementation
- **JSON Schema**: For constraint validation
- **JSON Path**: For field queries  
- **SD-JWT**: Selective Disclosure JWT support
- **W3C Verifiable Credentials**: JWT and JSON-LD formats
- **OpenID4VP**: Compatible with OpenID for Verifiable Presentations

## ? Performance Characteristics

| Wallet Size | Selection Time | Memory Usage |
|-------------|----------------|--------------|
| 10 credentials | < 50ms | < 1MB |
| 100 credentials | < 500ms | < 5MB |
| 1000 credentials | < 2s | < 20MB |
| 10000+ credentials | Use performance options | Configurable |

## ?? Integration Examples

### With ASP.NET Core

```csharp
[ApiController]
[Route("api/[controller]")]
public class VerificationController : ControllerBase
{
    private readonly PresentationExchangeEngine _engine;
    
    [HttpPost("select-credentials")]
    public async Task<IActionResult> SelectCredentials(
        [FromBody] CredentialSelectionRequest request)
    {
        var result = await _engine.SelectCredentialsAsync(
            request.PresentationDefinition,
            request.Wallet);
            
        return Ok(result);
    }
}
```

### With EUDI Wallet Scenarios

```csharp
// Scenario: EU Digital Identity Wallet requesting mDL
var eudiPresentationDef = PresentationDefinition.Create(
    "eudi_mdl_request",
    new[]
    {
        InputDescriptor.CreateForSdJwt("mdl", "org.iso.18013.5.1.mDL")
        {
            Constraints = Constraints.CreateWithSelectiveDisclosure(
                Field.CreateForExistence("$.family_name"),
                Field.CreateForExistence("$.given_name"),
                Field.CreateForExistence("$.birth_date"),
                Field.CreateForMinimumAge(18, "$.birth_date")
            )
        }
    },
    "Mobile Driving License",
    "Please present your mobile driving license for age verification"
);
```

## ?? Related Packages

- **SdJwt.Net**: Core SD-JWT implementation
- **SdJwt.Net.Vc**: Verifiable Credentials support  
- **SdJwt.Net.Oid4Vp**: OpenID4VP integration
- **SdJwt.Net.StatusList**: Status List 2021 support

## ?? Contributing

Contributions are welcome! Please read our [Contributing Guidelines](CONTRIBUTING.md) for details.

## ?? License

This project is licensed under the Apache 2.0 License - see the [LICENSE](LICENSE) file for details.

## ?? References

- [DIF Presentation Exchange 2.1.1](https://identity.foundation/presentation-exchange/spec/v2.1.1/)
- [SD-JWT Specification](https://www.ietf.org/archive/id/draft-ietf-oauth-selective-disclosure-jwt-07.html)
- [W3C Verifiable Credentials](https://www.w3.org/TR/vc-data-model/)
- [OpenID4VP](https://openid.net/specs/openid-4-verifiable-presentations-1_0.html)

## ?? Support

- ?? [Create an issue](https://github.com/thomas-tran/sd-jwt-dotnet/issues)
- ?? [Join discussions](https://github.com/thomas-tran/sd-jwt-dotnet/discussions)
- ?? [Documentation](https://github.com/thomas-tran/sd-jwt-dotnet/wiki)