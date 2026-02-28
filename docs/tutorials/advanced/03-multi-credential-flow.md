# Tutorial: Multi-Credential Flow

Present multiple credentials in a single authorization response.

**Time:** 25 minutes  
**Level:** Advanced  
**Sample:** `samples/SdJwt.Net.Samples/03-Advanced/03-MultiCredentialFlow.cs`

## What You Will Learn

- Combine credentials from multiple issuers
- Structure multi-credential presentations
- Handle complex verification scenarios

## Use Case

A mortgage application requires:

1. Government ID (from DMV)
2. Proof of income (from employer)
3. Credit score (from credit bureau)

## Step 1: Define Multi-Credential Request

```csharp
using SdJwt.Net.PresentationExchange.Models;

var definition = new PresentationDefinition
{
    Id = "mortgage-application",
    Name = "Mortgage Application Requirements",
    Purpose = "Verify identity, income, and creditworthiness",
    InputDescriptors = new[]
    {
        new InputDescriptor
        {
            Id = "government-id",
            Group = new[] { "identity" },
            Name = "Government-Issued ID",
            Purpose = "Verify legal identity",
            Constraints = new Constraints
            {
                LimitDisclosure = "required",
                Fields = new[]
                {
                    new Field
                    {
                        Path = new[] { "$.vct" },
                        Filter = new FieldFilter { Pattern = "^.*(DriversLicense|Passport|NationalID).*$" }
                    },
                    new Field { Path = new[] { "$.given_name" } },
                    new Field { Path = new[] { "$.family_name" } },
                    new Field { Path = new[] { "$.birthdate" } }
                }
            }
        },
        new InputDescriptor
        {
            Id = "employment-proof",
            Group = new[] { "financial" },
            Name = "Employment Verification",
            Purpose = "Verify income source",
            Constraints = new Constraints
            {
                Fields = new[]
                {
                    new Field { Path = new[] { "$.vct" }, Filter = new FieldFilter { Const = "EmploymentCredential" } },
                    new Field { Path = new[] { "$.employer_name" } },
                    new Field { Path = new[] { "$.annual_salary" }, Filter = new FieldFilter { Type = "number", Minimum = 50000 } },
                    new Field { Path = new[] { "$.employment_status" }, Filter = new FieldFilter { Const = "full-time" } }
                }
            }
        },
        new InputDescriptor
        {
            Id = "credit-score",
            Group = new[] { "financial" },
            Name = "Credit Report",
            Purpose = "Assess creditworthiness",
            Constraints = new Constraints
            {
                Fields = new[]
                {
                    new Field { Path = new[] { "$.vct" }, Filter = new FieldFilter { Const = "CreditScoreCredential" } },
                    new Field { Path = new[] { "$.score" }, Filter = new FieldFilter { Type = "integer", Minimum = 650 } }
                }
            }
        }
    },
    SubmissionRequirements = new[]
    {
        new SubmissionRequirement
        {
            Rule = "all",
            From = "identity"
        },
        new SubmissionRequirement
        {
            Rule = "all",
            From = "financial"
        }
    }
};
```

## Step 2: Wallet Selects Credentials

```csharp
// Wallet holds multiple credentials
var wallet = new CredentialWallet();

// Find credentials matching each descriptor
var governmentId = wallet.FindMatching(definition.InputDescriptors[0]).First();
var employment = wallet.FindMatching(definition.InputDescriptors[1]).First();
var creditScore = wallet.FindMatching(definition.InputDescriptors[2]).First();
```

## Step 3: Create Individual Presentations

```csharp
var nonce = request.Nonce;
var audience = request.ClientId;

// Government ID presentation
var idHolder = new SdJwtHolder(governmentId);
var idPresentation = idHolder.CreatePresentation(
    d => d.ClaimName is "given_name" or "family_name" or "birthdate",
    kbJwtPayload: new JwtPayload
    {
        ["aud"] = audience,
        ["nonce"] = nonce
    },
    kbJwtSigningKey: holderKey,
    kbJwtSigningAlgorithm: SecurityAlgorithms.EcdsaSha256
);

// Employment presentation
var empHolder = new SdJwtHolder(employment);
var empPresentation = empHolder.CreatePresentation(
    d => d.ClaimName is "employer_name" or "annual_salary" or "employment_status",
    kbJwtPayload: new JwtPayload
    {
        ["aud"] = audience,
        ["nonce"] = nonce
    },
    kbJwtSigningKey: holderKey,
    kbJwtSigningAlgorithm: SecurityAlgorithms.EcdsaSha256
);

// Credit score presentation
var creditHolder = new SdJwtHolder(creditScore);
var creditPresentation = creditHolder.CreatePresentation(
    d => d.ClaimName == "score",
    kbJwtPayload: new JwtPayload
    {
        ["aud"] = audience,
        ["nonce"] = nonce
    },
    kbJwtSigningKey: holderKey,
    kbJwtSigningAlgorithm: SecurityAlgorithms.EcdsaSha256
);
```

## Step 4: Build Multi-Token Response

```csharp
var response = new AuthorizationResponse
{
    // Array of VP tokens for multiple credentials
    VpTokens = new[]
    {
        idPresentation,
        empPresentation,
        creditPresentation
    },
    PresentationSubmission = new PresentationSubmission
    {
        Id = Guid.NewGuid().ToString(),
        DefinitionId = "mortgage-application",
        DescriptorMap = new[]
        {
            new DescriptorMapEntry
            {
                Id = "government-id",
                Format = "vc+sd-jwt",
                Path = "$[0]"  // First token
            },
            new DescriptorMapEntry
            {
                Id = "employment-proof",
                Format = "vc+sd-jwt",
                Path = "$[1]"  // Second token
            },
            new DescriptorMapEntry
            {
                Id = "credit-score",
                Format = "vc+sd-jwt",
                Path = "$[2]"  // Third token
            }
        }
    },
    State = request.State
};
```

## Step 5: Verifier Processes Multi-Token Response

```csharp
public async Task<MultiCredentialResult> VerifyMultiCredentialResponse(
    AuthorizationResponse response,
    PresentationDefinition definition)
{
    var results = new Dictionary<string, VerificationResult>();
    
    foreach (var mapping in response.PresentationSubmission.DescriptorMap)
    {
        // Extract token using path
        var tokenIndex = ExtractArrayIndex(mapping.Path);
        var vpToken = response.VpTokens[tokenIndex];
        
        // Find corresponding descriptor
        var descriptor = definition.InputDescriptors
            .First(d => d.Id == mapping.Id);
        
        // Verify the credential
        var verifier = new SdVerifier(ResolveIssuerKey);
        var result = await verifier.VerifyAsync(vpToken, params);
        
        // Validate against descriptor constraints
        ValidateAgainstDescriptor(result, descriptor);
        
        results[mapping.Id] = result;
    }
    
    // Verify submission requirements are met
    ValidateSubmissionRequirements(results, definition.SubmissionRequirements);
    
    return new MultiCredentialResult
    {
        GovernmentId = results["government-id"],
        Employment = results["employment-proof"],
        CreditScore = results["credit-score"]
    };
}
```

## Step 6: Cross-Credential Validation

```csharp
// Verify name consistency across credentials
var idName = results["government-id"].GetClaim("given_name");
var empName = results["employment-proof"].GetClaim("employee_given_name");

if (idName != empName)
{
    throw new SecurityException("Name mismatch between credentials");
}

// Verify same holder bound all credentials
var idHolder = results["government-id"].GetClaim("cnf");
var empHolder = results["employment-proof"].GetClaim("cnf");
var creditHolder = results["credit-score"].GetClaim("cnf");

if (!AllEqual(idHolder, empHolder, creditHolder))
{
    throw new SecurityException("Credentials bound to different holders");
}
```

## Alternative: Submission Requirements with Choice

```csharp
// Allow either passport OR driver's license
var flexibleDefinition = new PresentationDefinition
{
    InputDescriptors = new[]
    {
        new InputDescriptor { Id = "passport", Group = new[] { "identity" }, ... },
        new InputDescriptor { Id = "drivers-license", Group = new[] { "identity" }, ... },
        new InputDescriptor { Id = "employment", Group = new[] { "financial" }, ... }
    },
    SubmissionRequirements = new[]
    {
        new SubmissionRequirement
        {
            Rule = "pick",
            Count = 1,
            From = "identity"  // Need exactly one identity document
        },
        new SubmissionRequirement
        {
            Rule = "all",
            From = "financial"  // Need all financial documents
        }
    }
};
```

## Run the Sample

```bash
cd samples/SdJwt.Net.Samples
dotnet run -- 3.3
```

## Next Steps

- [Key Rotation](04-key-rotation.md) - Manage key lifecycle
- [Use Cases](../../use-cases/) - Industry implementations

## Key Takeaways

1. Multi-credential flows combine credentials from multiple issuers
2. Descriptor maps link tokens to requirements
3. Cross-credential validation ensures consistency
4. Same holder binding prevents credential mixing attacks
