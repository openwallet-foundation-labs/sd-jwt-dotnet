# SdJwt.Net.PresentationExchange - DIF Presentation Exchange v2.0

[![NuGet Version](https://img.shields.io/nuget/v/SdJwt.Net.PresentationExchange.svg)](https://www.nuget.org/packages/SdJwt.Net.PresentationExchange/)
[![License](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](https://opensource.org/licenses/Apache-2.0)

Implementation of **DIF Presentation Exchange v2.0.0** specification for intelligent credential selection and complex presentation requirements. Provides smart matching algorithms and comprehensive constraint validation.

## Features

- **DIF PEX v2.0.0**: Complete specification implementation
- **Intelligent Selection**: Smart credential matching algorithms
- **Complex Requirements**: Support for all submission requirement patterns
- **JSONPath Filtering**: Advanced field selection and constraints
- **Format Support**: SD-JWT, JWT VC, and Linked Data credentials

## Installation

```bash
dotnet add package SdJwt.Net.PresentationExchange
```

## Quick Start

### Define Presentation Requirements

```csharp
using SdJwt.Net.PresentationExchange.Models;

var presentationDefinition = new PresentationDefinition
{
    Id = "employment_and_education_verification",
    Name = "Employment and Education Verification",
    Purpose = "Verify qualifications for security clearance application",
    InputDescriptors = new[]
    {
        new InputDescriptor
        {
            Id = "university_degree",
            Name = "University Degree",
            Constraints = new Constraints
            {
                Fields = new[]
                {
                    new Field { 
                        Path = new[] { "$.degree" },
                        Filter = JObject.Parse(@"{ ""pattern"": "".*(Bachelor|Master|Doctor).*"" }")
                    },
                    new Field { Path = new[] { "$.graduation_date" } }
                }
            }
        },
        new InputDescriptor
        {
            Id = "employment_verification",
            Name = "Current Employment",
            Constraints = new Constraints
            {
                Fields = new[]
                {
                    new Field { Path = new[] { "$.position" } },
                    new Field { Path = new[] { "$.security_clearance" } }
                }
            }
        }
    }
};
```

### Credential Selection Engine

```csharp
using SdJwt.Net.PresentationExchange.Services;

var selectionEngine = new PresentationExchangeEngine();

var availableCredentials = new[]
{
    universityDegreeCredential,
    employmentCredential,
    governmentIdCredential
};

var selectionResult = await selectionEngine.SelectCredentialsAsync(
    presentationDefinition, 
    availableCredentials);

if (selectionResult.IsSuccessful)
{
    var selectedCredentials = selectionResult.SelectedCredentials;
    var submissionMapping = selectionResult.PresentationSubmission;
}
```

## Advanced Features

- **Submission Requirements**: "All", "Pick N", and "Pick Range" patterns
- **Field Constraints**: JSON Schema validation and JSONPath filtering
- **Multi-Format Support**: Automatic credential format detection
- **Performance Optimization**: Efficient algorithms for large credential sets

## Use Cases

- **Security Clearance**: Government contractor requiring multiple credentials
- **Loan Application**: Bank requiring employment, education, and identity verification
- **Professional Licensing**: Healthcare provider credentialing with multiple requirements
- **Academic Verification**: Graduate school application with transcript requirements

## Documentation

For comprehensive examples and advanced selection patterns, see the [main repository](https://github.com/openwalletfoundation/sd-jwt-dotnet).

## License

Licensed under the [Apache License 2.0](https://opensource.org/licenses/Apache-2.0).