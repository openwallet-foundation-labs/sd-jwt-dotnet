# Tutorial: Presentation Exchange

Define credential requirements using DIF Presentation Exchange v2.1.1.

**Time:** 15 minutes  
**Level:** Intermediate  
**Sample:** `samples/SdJwt.Net.Samples/02-Intermediate/05-PresentationExchange.cs`

## What You Will Learn

- Presentation Definition structure
- Field constraints and filters
- Submission requirements

## What is Presentation Exchange?

A query language for specifying:

- What credentials are needed
- Which fields must be present
- What values are acceptable

## Basic Presentation Definition

```csharp
using SdJwt.Net.PresentationExchange.Models;

var definition = new PresentationDefinition
{
    Id = "age-verification",
    Name = "Age Verification",
    Purpose = "Verify you are over 21",
    InputDescriptors = new[]
    {
        new InputDescriptor
        {
            Id = "age-credential",
            Name = "Age Proof",
            Constraints = new Constraints
            {
                Fields = new[]
                {
                    new Field
                    {
                        Path = new[] { "$.age_over_21" },
                        Filter = new FieldFilter
                        {
                            Type = "boolean",
                            Const = true
                        }
                    }
                }
            }
        }
    }
};
```

## Field Path Syntax

Use JSONPath expressions:

```csharp
// Root-level claim
new Field { Path = new[] { "$.given_name" } }

// Nested claim
new Field { Path = new[] { "$.address.city" } }

// Alternative paths (first match wins)
new Field { Path = new[] { "$.birthdate", "$.date_of_birth" } }
```

## Filter Types

### Exact Match

```csharp
new FieldFilter
{
    Type = "string",
    Const = "United States"
}
```

### Enum (Any Of)

```csharp
new FieldFilter
{
    Type = "string",
    Enum = new object[] { "US", "CA", "MX" }
}
```

### Pattern (Regex)

```csharp
new FieldFilter
{
    Type = "string",
    Pattern = "^[A-Z]{2}-[0-9]{6}$"  // License format
}
```

### Numeric Range

```csharp
new FieldFilter
{
    Type = "integer",
    Minimum = 21,
    Maximum = 120
}
```

## Requiring Selective Disclosure

```csharp
var descriptor = new InputDescriptor
{
    Id = "id-credential",
    Constraints = new Constraints
    {
        LimitDisclosure = "required",  // Must use SD-JWT
        Fields = new[] { ... }
    }
};
```

## Multiple Credentials

Request several credentials:

```csharp
var definition = new PresentationDefinition
{
    Id = "loan-application",
    InputDescriptors = new[]
    {
        new InputDescriptor
        {
            Id = "identity",
            Name = "Government ID",
            Constraints = new Constraints
            {
                Fields = new[]
                {
                    new Field { Path = new[] { "$.vct" }, Filter = new FieldFilter { Const = "GovernmentID" } },
                    new Field { Path = new[] { "$.given_name" } },
                    new Field { Path = new[] { "$.family_name" } }
                }
            }
        },
        new InputDescriptor
        {
            Id = "income",
            Name = "Income Verification",
            Constraints = new Constraints
            {
                Fields = new[]
                {
                    new Field { Path = new[] { "$.annual_income" }, Filter = new FieldFilter { Type = "number", Minimum = 50000 } }
                }
            }
        }
    }
};
```

## Submission Requirements

Specify how many descriptors must be satisfied:

```csharp
var definition = new PresentationDefinition
{
    Id = "flexible-verification",
    InputDescriptors = new[]
    {
        new InputDescriptor { Id = "passport", Group = new[] { "identity" }, ... },
        new InputDescriptor { Id = "drivers-license", Group = new[] { "identity" }, ... },
        new InputDescriptor { Id = "national-id", Group = new[] { "identity" }, ... }
    },
    SubmissionRequirements = new[]
    {
        new SubmissionRequirement
        {
            Rule = "pick",
            Count = 1,          // Only need one
            From = "identity"   // From identity group
        }
    }
};
```

## Presentation Submission

Wallet responds with submission mapping:

```csharp
var submission = new PresentationSubmission
{
    Id = Guid.NewGuid().ToString(),
    DefinitionId = "loan-application",
    DescriptorMap = new[]
    {
        new DescriptorMapEntry
        {
            Id = "identity",
            Format = "vc+sd-jwt",
            Path = "$.verifiableCredential[0]"
        },
        new DescriptorMapEntry
        {
            Id = "income",
            Format = "vc+sd-jwt",
            Path = "$.verifiableCredential[1]"
        }
    }
};
```

## Evaluating Credentials

```csharp
using SdJwt.Net.PresentationExchange.Services;

var evaluator = new PresentationDefinitionEvaluator();

// Check if credential matches descriptor
var matches = evaluator.Evaluate(definition, credential);

foreach (var match in matches)
{
    Console.WriteLine($"Descriptor {match.DescriptorId}: {match.Satisfied}");
}
```

## Run the Sample

```bash
cd samples/SdJwt.Net.Samples
dotnet run -- 2.5
```

## Next Steps

- [OpenID Federation](../advanced/01-openid-federation.md) - Trust management
- [Multi-Credential Flow](../advanced/03-multi-credential-flow.md) - Combined presentations

## Key Takeaways

1. Presentation Exchange defines credential requirements
2. Field paths use JSONPath syntax
3. Filters constrain acceptable values
4. Submission requirements enable flexible matching
