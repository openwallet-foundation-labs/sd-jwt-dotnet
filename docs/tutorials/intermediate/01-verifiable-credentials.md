# Tutorial: Verifiable Credentials

Implement SD-JWT Verifiable Credentials per draft-ietf-oauth-sd-jwt-vc.

**Time:** 15 minutes  
**Level:** Intermediate  
**Sample:** `samples/SdJwt.Net.Samples/02-Intermediate/01-VerifiableCredentials.cs`

## What You Will Learn

- SD-JWT VC structure and required claims
- VCT (Verifiable Credential Type) identifiers
- Credential metadata and status

## SD-JWT VC vs Base SD-JWT

| Feature | Base SD-JWT | SD-JWT VC |
|---------|-------------|-----------|
| vct claim | Optional | Required |
| iss claim | Optional | Required |
| Credential type | Free-form | Standardized |
| Status support | Manual | Built-in |

## Step 1: Create VC Issuer

```csharp
using SdJwt.Net.Vc.Issuer;
using SdJwt.Net.Vc.Models;

var vcIssuer = new SdJwtVcIssuer(issuerKey, SecurityAlgorithms.EcdsaSha256);
```

## Step 2: Define Credential Payload

```csharp
var payload = new SdJwtVcPayload
{
    Issuer = "https://university.example.edu",
    Subject = "did:example:student123",
    IssuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
    ExpiresAt = DateTimeOffset.UtcNow.AddYears(4).ToUnixTimeSeconds(),
    AdditionalData = new Dictionary<string, object>
    {
        ["given_name"] = "Alice",
        ["family_name"] = "Smith",
        ["degree_type"] = "Bachelor of Science",
        ["field_of_study"] = "Computer Science",
        ["graduation_date"] = "2025-05-15",
        ["gpa"] = 3.85,
        ["honors"] = "Magna Cum Laude"
    }
};
```

## Step 3: Configure Selective Disclosure

```csharp
var options = new SdIssuanceOptions
{
    DisclosureStructure = new
    {
        given_name = true,
        family_name = true,
        gpa = true,       // Sensitive
        honors = true     // Optional to reveal
    }
};
```

## Step 4: Issue with VCT

```csharp
// VCT identifies the credential type
var vctIdentifier = "https://credentials.example.edu/UniversityDegree";

var credential = vcIssuer.Issue(
    vctIdentifier,
    payload,
    options,
    holderPublicKey
);
```

## Credential Structure

The issued SD-JWT VC contains:

```json
{
  "vct": "https://credentials.example.edu/UniversityDegree",
  "iss": "https://university.example.edu",
  "sub": "did:example:student123",
  "iat": 1700000000,
  "exp": 1830000000,
  "degree_type": "Bachelor of Science",
  "field_of_study": "Computer Science",
  "_sd": ["hash1", "hash2", "hash3"],
  "cnf": { "jwk": { ... } }
}
```

Note: `given_name`, `family_name`, `gpa`, and `honors` are replaced with digests in `_sd`.

## Step 5: Holder Presentation

```csharp
var holder = new SdJwtHolder(credential.Issuance);

// Present degree without GPA
var presentation = holder.CreatePresentation(
    d => d.ClaimName is "given_name" or "family_name" or "honors",
    kbJwtPayload: new JwtPayload
    {
        ["aud"] = "https://employer.example.com",
        ["nonce"] = "job-application-nonce"
    },
    kbJwtSigningKey: holderKey,
    kbJwtSigningAlgorithm: SecurityAlgorithms.EcdsaSha256
);
```

## Step 6: Verifier Validation

```csharp
var verifier = new SdVerifier(_ => Task.FromResult<SecurityKey>(issuerPublicKey));

var result = await verifier.VerifyAsync(presentation, sdJwtParams, kbJwtParams);

// Check credential type
var vct = result.ClaimsPrincipal.FindFirst("vct")?.Value;
if (vct != "https://credentials.example.edu/UniversityDegree")
{
    throw new Exception("Unexpected credential type");
}
```

## VCT Best Practices

### Use Resolvable URIs

```csharp
// Good: Resolvable URL with schema
var vct = "https://credentials.example.edu/schemas/UniversityDegree/v1";

// Acceptable: URN for private credentials
var vct = "urn:example:credentials:employee-badge:v1";
```

### Version Your Types

```csharp
// Include version in VCT
var vct = "https://creds.example/DriverLicense/v2";
```

## Run the Sample

```bash
cd samples/SdJwt.Net.Samples
dotnet run -- 2.1
```

## Next Steps

- [Status List](02-status-list.md) - Add revocation support
- [OpenID4VCI](03-openid4vci.md) - Issuance protocol

## Key Takeaways

1. SD-JWT VC adds standardized structure to SD-JWTs
2. VCT identifies the credential type
3. Use `SdJwtVcIssuer` and `SdJwtVcPayload` for VC issuance
4. Certain claims (vct, iss) cannot be selectively disclosed
