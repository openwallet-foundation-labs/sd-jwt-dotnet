# Tutorial: Selective Disclosure

Learn to hide and reveal claims when presenting credentials.

**Time:** 10 minutes  
**Level:** Beginner  
**Sample:** `samples/SdJwt.Net.Samples/01-Beginner/02-SelectiveDisclosure.cs`

## What You Will Learn

- How the holder controls which claims to reveal
- How to create presentations with selected disclosures
- Privacy-preserving credential sharing

## The Three Actors

1. **Issuer** - Creates and signs the SD-JWT
2. **Holder** - Receives the credential and controls disclosure
3. **Verifier** - Requests and validates specific claims

## Step 1: Issue a Credential (Issuer)

```csharp
var claims = new JwtPayload
{
    ["iss"] = "https://employer.example",
    ["sub"] = "employee-123",
    ["given_name"] = "Bob",
    ["family_name"] = "Johnson",
    ["employee_id"] = "EMP-456",
    ["department"] = "Engineering",
    ["salary"] = 95000,
    ["start_date"] = "2020-03-15"
};

var options = new SdIssuanceOptions
{
    DisclosureStructure = new
    {
        given_name = true,
        family_name = true,
        employee_id = true,
        department = true,
        salary = true,        // Sensitive!
        start_date = true
    }
};

var result = issuer.Issue(claims, options);
```

## Step 2: Store as Holder

The holder receives the full issuance string containing all disclosures:

```csharp
using SdJwt.Net.Holder;

var holder = new SdJwtHolder(result.Issuance);
```

## Step 3: Create Selective Presentation

When presenting to a verifier, the holder chooses what to reveal:

```csharp
// Scenario: Proving employment without revealing salary
var presentation = holder.CreatePresentation(
    disclosure => 
        disclosure.ClaimName == "given_name" ||
        disclosure.ClaimName == "family_name" ||
        disclosure.ClaimName == "department"
    // salary is NOT included
);
```

## What Happens

| Claim | In Presentation |
|-------|-----------------|
| given_name | Revealed |
| family_name | Revealed |
| department | Revealed |
| employee_id | Hidden |
| salary | Hidden |
| start_date | Hidden |

The verifier can prove Bob works in Engineering, but cannot see his salary.

## Step 4: Verify Selectively Disclosed Claims

```csharp
using SdJwt.Net.Verifier;

var verifier = new SdVerifier(_ => Task.FromResult<SecurityKey>(issuerPublicKey));

var result = await verifier.VerifyAsync(presentation, validationParams);

// Only disclosed claims are visible
foreach (var claim in result.ClaimsPrincipal.Claims)
{
    Console.WriteLine($"{claim.Type}: {claim.Value}");
}
// Output:
// given_name: Bob
// family_name: Johnson
// department: Engineering
// (salary is NOT visible)
```

## Privacy Patterns

### Minimum Disclosure

Only reveal what is strictly necessary:

```csharp
// Age verification: prove over 21 without showing birthdate
var presentation = holder.CreatePresentation(
    d => d.ClaimName == "age_over_21"
);
```

### Graduated Disclosure

Different contexts require different levels:

```csharp
// For job application: name and department
var jobPresentation = holder.CreatePresentation(
    d => d.ClaimName is "given_name" or "family_name" or "department"
);

// For background check: more details
var backgroundPresentation = holder.CreatePresentation(
    d => d.ClaimName is "given_name" or "family_name" or "employee_id" or "start_date"
);
```

## Complete Example

```csharp
// Issuer creates credential
var issuer = new SdIssuer(issuerKey, SecurityAlgorithms.EcdsaSha256);
var result = issuer.Issue(claims, options);

// Holder stores and selectively presents
var holder = new SdJwtHolder(result.Issuance);
var presentation = holder.CreatePresentation(
    d => d.ClaimName == "department"  // Only reveal department
);

// Verifier validates
var verifier = new SdVerifier(_ => Task.FromResult<SecurityKey>(issuerKey));
var verified = await verifier.VerifyAsync(presentation, validationParams);

// Department is visible, salary is not
```

## Run the Sample

```bash
cd samples/SdJwt.Net.Samples
dotnet run -- 1.2
```

## Next Steps

- [Holder Binding](03-holder-binding.md) - Prove you own the credential
- [Verification Flow](04-verification-flow.md) - Complete end-to-end flow

## Key Takeaways

1. Holders control what claims to reveal
2. Verifiers only see disclosed claims
3. Hidden claims remain cryptographically protected
4. The same credential supports multiple disclosure scenarios
