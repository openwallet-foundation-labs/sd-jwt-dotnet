# SdJwt.Net.PresentationExchange - DIF Presentation Exchange v2.1.1

[![NuGet Version](https://img.shields.io/nuget/v/SdJwt.Net.PresentationExchange.svg)](https://www.nuget.org/packages/SdJwt.Net.PresentationExchange/)
[![License](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](https://opensource.org/licenses/Apache-2.0)

**Implementation of DIF Presentation Exchange v2.1.1** for credential selection, presentation definition modeling, presentation submission validation, and OID4VP integration. The package uses in-repository JSONPath and JSON Schema subset evaluators and does not require paid JSON Schema dependencies.

## DIF PE v2.1.1 Support

This library implements the core [DIF Presentation Exchange v2.1.1](https://identity.foundation/presentation-exchange/spec/v2.1.1/) structures and verifier-side checks used by SD-JWT and OID4VP flows:

-   **Presentation Definitions**: Structure validation, duplicate ID detection, format constraints, frames, submission requirements, and input descriptor validation.
-   **Input Descriptors**: Required constraints, field constraints, holder and same-subject directive shape validation, and status directive modeling.
-   **Field Constraints**: JSONPath field resolution and built-in JSON Schema subset filtering for common PEX filters such as `type`, `const`, `enum`, `pattern`, numeric ranges, string lengths, array containment, and object requirements.
-   **Presentation Submissions**: Descriptor map validation, definition binding, format checks, path resolution, and constraint evaluation against submitted JSON envelopes or verified OID4VP claim sets.
-   **OID4VP Integration**: `SdJwt.Net.Oid4Vp` can validate `presentation_submission` against a shared PEX definition after SD-JWT verification, so PEX constraints are evaluated only on verified disclosed claims.

### Current Boundaries

-   **Predicate fields**: The spec `predicate` property is modeled and validated, but cryptographic zero-knowledge predicate proofs are not generated or verified by this package.
-   **Status directives**: PEX status objects are modeled and validated for shape. Runtime status-list revocation checks should be performed by `SdJwt.Net.StatusList` or the verifier's credential policy layer.
-   **JSON-LD frames**: `frame` is modeled for serialization and request compatibility. JSON-LD framing execution is outside this package.
-   **JSONPath and JSON Schema**: The evaluators cover the subset used by this library and tests. They are intentionally dependency-light rather than wrappers around a paid JSON Schema engine.

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

## Predicate and Status Modeling

PEX v2.1.1 includes request metadata for predicate and credential status handling. This package validates those request shapes and lets verifiers express the requirement, while cryptographic predicate proof execution and status-list revocation checks remain the responsibility of the credential format verifier and policy layer.

```csharp
var ageCheck = new Field
{
    Path = new[] { "$.age_over_21" },
    Predicate = "required",
    Filter = new FieldFilter
    {
        Type = "boolean",
        Const = true
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

// Use the StatusList package or verifier policy layer to verify runtime status.
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

// Credential selection with status verification
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

| Feature                     | Status      | Description                                                  |
| --------------------------- | ----------- | ------------------------------------------------------------ |
| **Presentation Definition** | Supported   | Structure, frame serialization, and duplicate ID validation  |
| **Input Descriptors**       | Supported   | Required constraints, format constraints, and field checks   |
| **Submission Requirements** | Supported   | `all` and `pick` with positive `count`, `min`, and `max`     |
| **Field Constraints**       | Supported   | JSONPath plus a built-in JSON Schema subset evaluator        |
| **Presentation Submission** | Supported   | Descriptor map, format, path, and required descriptor checks |
| **OID4VP Validation**       | Supported   | Validates PEX submissions against verified disclosed claims  |
| **Status Directives**       | Model-level | Shape validation only; perform revocation checks separately  |
| **Predicate Property**      | Model-level | `required` and `preferred` validation; no ZK proof execution |
| **JSON-LD Frames**          | Model-level | Serialized and validated as request data; no framing runtime |

### Dependency Model

The package intentionally avoids paid JSON Schema dependencies. Field filters are evaluated by the in-repository `FieldFilterEvaluator`, which covers the subset needed for SD-JWT VC and OID4VP presentation checks.

### Supported Credential Formats

-   **SD-JWT VC** (`dc+sd-jwt`) - Full selective disclosure support
-   **JWT VC** (`jwt_vc`) - W3C Verifiable Credentials in JWT format
-   **JWT VP** (`jwt_vp`) - JWT Verifiable Presentations
-   **LDP VC** (`ldp_vc`) - Linked Data Proof Verifiable Credentials
-   **LDP VP** (`ldp_vp`) - Linked Data Proof Presentations
-   **Plain JWT** (`jwt`) - Generic JWT tokens
-   **Plain SD-JWT** (`sd-jwt`) - Basic selective disclosure JWTs

## Real-World Use Cases

### Financial Services

-   **Loan Applications**: Multi-credential verification (employment, education, credit)
-   **KYC Compliance**: Identity verification with selective disclosure
-   **Credit Assessments**: Financial history with selective disclosure and policy checks

### Healthcare

-   **Provider Credentialing**: License verification with specialty constraints
-   **Patient Consent**: Medical record sharing with field-level control
-   **Insurance Claims**: Verification with HIPAA compliance

### Government & Defense

-   **Security Clearance**: Background verification with multiple sources
-   **Border Control**: Age-over claims without revealing exact birthdate when issued as separate claims
-   **Service Access**: Multi-factor credential requirements

### Education & Professional

-   **Academic Verification**: Degree and transcript validation
-   **Professional Licensing**: Certification with continuing education
-   **Job Applications**: **Income verification with privacy protection**

## Performance & Scalability

-   **Large Wallet Support**: Efficient algorithms for 10,000+ credentials
-   **Timeout Controls**: Configurable limits for constraint evaluation
-   **Memory Optimization**: Streaming evaluation for memory efficiency
-   **Parallel Processing**: Concurrent credential evaluation
-   **Caching Support**: Smart caching for repeated evaluations
-   **Status List Caching**: Efficient credential status verification

## Error Handling & Debugging

```csharp
// Enable debug mode for detailed information
var debugOptions = new CredentialSelectionOptions
{
    EnableDebugMode = true,
    IncludeOptimizationHints = true
};

var result = await engine.SelectCredentialsAsync(definition, wallet, debugOptions);

// Error information
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

For more examples and advanced patterns, see the [main repository samples](https://openwallet-foundation-labs.github.io/sd-jwt-dotnet/getting-started/running-the-samples/).

## Contributing

We welcome contributions! Please see our [Contributing Guide](https://github.com/openwallet-foundation-labs/sd-jwt-dotnet/blob/main/CONTRIBUTING.md) for details.

## License

Licensed under the [Apache License 2.0](https://opensource.org/licenses/Apache-2.0).
