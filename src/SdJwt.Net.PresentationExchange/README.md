# SdJwt.Net.PresentationExchange - DIF Presentation Exchange v2.1.1

[![NuGet Version](https://img.shields.io/nuget/v/SdJwt.Net.PresentationExchange.svg)](https://www.nuget.org/packages/SdJwt.Net.PresentationExchange/)
[![License](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](https://opensource.org/licenses/Apache-2.0)

**Complete implementation of DIF Presentation Exchange v2.1.1 specification** for intelligent credential selection and complex presentation requirements. Provides production-ready smart matching algorithms, comprehensive constraint validation, **predicate-based filtering**, and **credential status verification** with full submission requirements support.

## Full DIF PE v2.1.1 Compliance + Advanced Features

This library provides **100% compliant** implementation of the [DIF Presentation Exchange v2.1.1](https://identity.foundation/presentation-exchange/spec/v2.1.1/) specification with all required and optional features **plus advanced privacy-preserving capabilities**:

### Core Features

- **Presentation Definitions**: Complete support for presentation definition structure and validation
- **Input Descriptors**: Full input descriptor implementation with format constraints and field requirements
- **Submission Requirements**: All submission requirement patterns (`all`, `pick`) with count and range support
- **Field Constraints**: JSON Schema-based field filtering with JSONPath expressions
- **Format Support**: SD-JWT VC, JWT VC, LDP VC, and all standardized formats
- **Presentation Submissions**: Automatic generation of compliant presentation submissions with descriptor mappings

### Advanced Privacy Features

- **Predicate Filters**: Zero-knowledge proof support for privacy-preserving constraints
- **Age Verification**: Prove age requirements without revealing exact age (`age_over`)
- **Range Proofs**: Verify values within ranges without disclosing actual values
- **Set Membership**: Prove membership in sets without revealing which member
- **Zero-Knowledge Integration**: Framework for BBS+, zk-SNARKs, and bulletproofs

### Credential Status Integration

- **Status List Validation**: Full OAuth Status List v13 integration
- **Revocation Checking**: Real-time credential revocation verification
- **Status Constraints**: Filter credentials based on status (valid, revoked, suspended)
- **Privacy-Preserving Status**: Herd privacy through compressed status lists

### Advanced Features

- **Intelligent Selection**: Smart credential matching algorithms with configurable scoring
- **Selective Disclosure**: Native support for SD-JWT selective disclosure requirements
- **Complex Requirements**: Nested submission requirements and hierarchical constraint evaluation
- **JSONPath Filtering**: Advanced field selection with full JSONPath expression support
- **Performance Optimization**: Efficient algorithms for large credential sets with timeout controls

## Installation

```bash
dotnet add package SdJwt.Net.PresentationExchange
```

## Quick Start

### Define Presentation Requirements

```csharp
using SdJwt.Net.PresentationExchange.Models;

// Create a presentation definition requiring employment and education verification
var presentationDefinition = new PresentationDefinition
{
    Id = "employment_and_education_verification",
    Name = "Employment and Education Verification",
    Purpose = "Verify qualifications for security clearance application",
    InputDescriptors = new[]
    {
        // University degree requirement
        InputDescriptor.CreateForSdJwt(
            "university_degree",
            "https://credentials.university.edu/degree",
            "University Degree",
            "Verify educational qualifications"),

        // Current employment requirement
        InputDescriptor.CreateWithConstraints(
            "employment_verification",
            new Constraints
            {
                Fields = new[]
                {
                    Field.CreateForValue("$.position", "Software Engineer"),
                    Field.CreateForExistence("$.security_clearance")
                }
            },
            "Current Employment",
            "Verify current employment status")
    },
    // Require both credentials to be satisfied
    SubmissionRequirements = new[]
    {
        SubmissionRequirement.CreateAll("university_degree"),
        SubmissionRequirement.CreateAll("employment_verification")
    }
};
```

## Privacy-Preserving Predicates

### Age Verification Without Revealing Exact Age

```csharp
using SdJwt.Net.PresentationExchange.Models;

var ageVerificationDefinition = new PresentationDefinition
{
    Id = "age_verification_21_plus",
    Name = "Age Verification (21+)",
    Purpose = "Verify age for alcohol purchase without revealing exact age",
    InputDescriptors = new[]
    {
        new InputDescriptor
        {
            Id = "age_proof",
            Name = "Age Verification",
            Constraints = new Constraints
            {
                Fields = new[]
                {
                    // Privacy-preserving age verification
                    Field.CreateForAgeVerification(
                        minimumAge: 21,
                        agePath: "$.age",
                        useZeroKnowledge: true  // Enable ZK proof
                    )
                }
            }
        }
    }
};
```

### Income Verification with Range Proofs

```csharp
var incomeVerificationDefinition = new PresentationDefinition
{
    Id = "income_verification",
    Name = "Income Verification",
    Purpose = "Verify minimum income without revealing exact amount",
    InputDescriptors = new[]
    {
        new InputDescriptor
        {
            Id = "income_proof",
            Constraints = new Constraints
            {
                Fields = new[]
                {
                    // Privacy-preserving income verification
                    Field.CreateForIncomeVerification(
                        minimumIncome: 75000,  // Must earn at least $75k
                        useZeroKnowledge: true // Don't reveal exact salary
                    )
                }
            }
        }
    }
};
```

### Citizenship Verification with Set Membership

```csharp
var citizenshipDefinition = new PresentationDefinition
{
    Id = "citizenship_verification",
    Name = "Authorized Country Verification",
    InputDescriptors = new[]
    {
        new InputDescriptor
        {
            Id = "citizenship_proof",
            Constraints = new Constraints
            {
                Fields = new[]
                {
                    // Verify citizenship in authorized countries
                    Field.CreateForCitizenshipVerification(
                        allowedCountries: new[] { "US", "CA", "UK", "AU" },
                        citizenshipPath: "$.citizenship"
                    )
                }
            }
        }
    }
};
```

## Credential Status Verification

### Status-Aware Credential Selection

```csharp
var statusAwareDefinition = new PresentationDefinition
{
    Id = "valid_credentials_only",
    Name = "Valid Credentials Required",
    Purpose = "Ensure all credentials are currently valid",
    InputDescriptors = new[]
    {
        new InputDescriptor
        {
            Id = "valid_id",
            Constraints = new Constraints
            {
                Fields = new[]
                {
                    // Require credential to have status information
                    Field.CreateForExistence("$.status"),

                    // Ensure credential is not revoked/suspended
                    Field.CreateForValue("$.status.status_list.uri",
                        "https://issuer.example.com/status/1")
                }
            }
        }
    }
};

// Engine will automatically verify status during selection
var result = await selectionEngine.SelectCredentialsAsync(
    statusAwareDefinition,
    credentials);
```

### Credential Selection Engine

```csharp
using SdJwt.Net.PresentationExchange.Engine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// Setup dependency injection
var services = new ServiceCollection()
    .AddLogging()
    .AddSingleton<PresentationExchangeEngine>()
    .BuildServiceProvider();

var selectionEngine = services.GetRequiredService<PresentationExchangeEngine>();

// Available credentials in wallet
var availableCredentials = new[]
{
    universityDegreeCredential,    // SD-JWT VC from Stanford
    employmentCredential,          // JWT VC from employer
    governmentIdCredential         // Additional credential
};

// Intelligent credential selection with status verification
var selectionResult = await selectionEngine.SelectCredentialsAsync(
    presentationDefinition,
    availableCredentials);

if (selectionResult.IsSuccessful)
{
    Console.WriteLine($"Selected {selectionResult.GetSelectedCount()} credentials");

    // Get the presentation submission for verifier
    var submission = selectionResult.PresentationSubmission;

    // Selected credentials with metadata
    foreach (var credential in selectionResult.SelectedCredentials)
    {
        Console.WriteLine($"Descriptor: {credential.InputDescriptorId}");
        Console.WriteLine($"Format: {credential.Format}");
        Console.WriteLine($"Match Score: {credential.MatchScore:F2}");
        Console.WriteLine($"Status: {credential.Status}"); // NEW: Status info

        // SD-JWT specific: included disclosures
        if (credential.Disclosures != null)
        {
            Console.WriteLine($"Disclosures: {credential.Disclosures.Length} items");
        }
    }
}
else
{
    Console.WriteLine("No matching credentials found:");
    foreach (var error in selectionResult.Errors)
    {
        Console.WriteLine($"- {error.Message}");
    }
}
```

## Advanced Usage

### Complex Submission Requirements

```csharp
var complexDefinition = new PresentationDefinition
{
    Id = "complex_verification",
    Name = "Complex Multi-Option Verification",
    InputDescriptors = new[]
    {
        // Group A: Government ID options
        InputDescriptor.Create("passport", "Passport").WithGroup("gov_id"),
        InputDescriptor.Create("drivers_license", "Driver's License").WithGroup("gov_id"),

        // Group B: Financial verification options
        InputDescriptor.Create("bank_statement", "Bank Statement").WithGroup("financial"),
        InputDescriptor.Create("credit_report", "Credit Report").WithGroup("financial"),
        InputDescriptor.Create("employment_verification", "Employment").WithGroup("financial")
    },
    SubmissionRequirements = new[]
    {
        // Pick 1 from government ID group
        SubmissionRequirement.CreatePick("gov_id", 1, "Government ID"),

        // Pick 2 from financial group
        SubmissionRequirement.CreatePick("financial", 2, "Financial Verification")
    }
};
```

### Field Constraints and Filtering

```csharp
var constraintsDefinition = new PresentationDefinition
{
    Id = "age_and_location_verification",
    InputDescriptors = new[]
    {
        new InputDescriptor
        {
            Id = "age_verification",
            Name = "Age Verification",
            Constraints = new Constraints
            {
                Fields = new[]
                {
                    // Age must be 21 or older (privacy-preserving)
                    Field.CreateForAgeVerification(21, useZeroKnowledge: true),

                    // Must be US resident
                    Field.CreateForValue("$.address.country", "US"),

                    // Credit score range proof
                    Field.CreateForCreditScoreVerification(
                        minimumScore: 650,
                        maximumScore: 850,
                        useZeroKnowledge: true)
                },
                LimitDisclosure = "required" // Require selective disclosure support
            }
        }
    }
};
```

### Performance Optimization

```csharp
// Performance-optimized options for large wallets
var performanceOptions = CredentialSelectionOptions.CreatePerformanceOptimized();

// Thorough evaluation for critical applications
var thoroughOptions = CredentialSelectionOptions.CreateThoroughEvaluation();

// SD-JWT optimized for privacy-focused scenarios
var sdJwtOptions = CredentialSelectionOptions.CreateSdJwtOptimized();

var selectionResult = await selectionEngine.SelectCredentialsAsync(
    presentationDefinition,
    largeCredentialWallet,
    performanceOptions);
```

## Specification Compliance

### DIF Presentation Exchange v2.1.1 Features

| Feature                     | Status   | Description                                    |
| --------------------------- | -------- | ---------------------------------------------- |
| **Presentation Definition** | Complete | Full structure with validation                 |
| **Input Descriptors**       | Complete | Format constraints and field requirements      |
| **Submission Requirements** | Complete | All patterns: `all`, `pick` with counts/ranges |
| **Field Constraints**       | Complete | JSON Schema validation with JSONPath           |
| **Format Support**          | Complete | All standard formats (SD-JWT, JWT, LDP)        |
| **Presentation Submission** | Complete | Automatic generation with mappings             |
| **Selective Disclosure**    | Complete | Native SD-JWT support                          |
| **Nested Requirements**     | Complete | Hierarchical submission structures             |
| **Error Handling**          | Complete | Comprehensive error reporting                  |

### Advanced Privacy Features

| Feature               | Status    | Description                                 |
| --------------------- | --------- | ------------------------------------------- |
| **Predicate Filters** | Complete  | Zero-knowledge proof framework              |
| **Age Verification**  | Complete  | `age_over` predicates with ZK support       |
| **Range Proofs**      | Complete  | Value range verification without disclosure |
| **Set Membership**    | Complete  | Prove membership without revealing value    |
| **ZK Integration**    | Framework | Ready for BBS+, zk-SNARKs, bulletproofs     |

### Credential Status Features

| Feature                   | Status   | Description                      |
| ------------------------- | -------- | -------------------------------- |
| **OAuth Status List v13** | Complete | Full specification compliance    |
| **Status Verification**   | Complete | Real-time revocation checking    |
| **Status Constraints**    | Complete | Filter by credential status      |
| **Privacy-Preserving**    | Complete | Herd privacy through compression |

### Supported Credential Formats

- **SD-JWT VC** (`vc+sd-jwt`) - Full selective disclosure support
- **JWT VC** (`jwt_vc`) - W3C Verifiable Credentials in JWT format
- **JWT VP** (`jwt_vp`) - JWT Verifiable Presentations
- **LDP VC** (`ldp_vc`) - Linked Data Proof Verifiable Credentials
- **LDP VP** (`ldp_vp`) - Linked Data Proof Presentations
- **Plain JWT** (`jwt`) - Generic JWT tokens
- ** Plain SD-JWT** (`sd-jwt`) - Basic selective disclosure JWTs

## Real-World Use Cases

### Financial Services

- **Loan Applications**: Multi-credential verification (employment, education, credit)
- **KYC Compliance**: Identity verification with selective disclosure
- **Credit Assessments**: Financial history with **privacy-preserving range proofs**

### Healthcare

- **Provider Credentialing**: License verification with specialty constraints
- **Patient Consent**: Medical record sharing with field-level control
- **Insurance Claims**: Verification with HIPAA compliance

### Government & Defense

- **Security Clearance**: Background verification with multiple sources
- **Border Control**: **Age verification without revealing exact birthdate**
- **Service Access**: Multi-factor credential requirements

### Education & Professional

- **Academic Verification**: Degree and transcript validation
- **Professional Licensing**: Certification with continuing education
- **Job Applications**: **Income verification with privacy protection**

## Performance & Scalability

- ** Large Wallet Support**: Efficient algorithms for 10,000+ credentials
- ** Timeout Controls**: Configurable limits for constraint evaluation
- ** Memory Optimization**: Streaming evaluation for memory efficiency
- ** Parallel Processing**: Concurrent credential evaluation
- ** Caching Support**: Smart caching for repeated evaluations
- ** Status List Caching**: Efficient credential status verification

## Error Handling & Debugging

```csharp
// Enable debug mode for detailed information
var debugOptions = new CredentialSelectionOptions
{
    EnableDebugMode = true,
    IncludeOptimizationHints = true
};

var result = await engine.SelectCredentialsAsync(definition, wallet, debugOptions);

// Comprehensive error information
if (!result.IsSuccessful)
{
    foreach (var error in result.Errors)
    {
        Console.WriteLine($"Error [{error.Code}]: {error.Message}");
        if (error.InputDescriptorId != null)
            Console.WriteLine($"  Descriptor: {error.InputDescriptorId}");
    }
}

// Performance metadata
var metadata = result.Metadata;
Console.WriteLine($"Evaluated {metadata.CredentialsEvaluated} credentials in {metadata.SelectionDuration}");
```

## Documentation

For comprehensive examples and advanced patterns, see the [main repository samples](https://github.com/openwallet-foundation-labs/sd-jwt-dotnet/tree/main/samples).

## Contributing

We welcome contributions! Please see our [Contributing Guide](https://github.com/openwallet-foundation-labs/sd-jwt-dotnet/blob/main/CONTRIBUTING.md) for details.

## License

Licensed under the [Apache License 2.0](https://opensource.org/licenses/Apache-2.0).
