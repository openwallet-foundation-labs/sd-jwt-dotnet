# Tutorial: HAIP Compliance

Implement High Assurance Interoperability Profile security levels.

**Time:** 20 minutes  
**Level:** Advanced  
**Sample:** `samples/SdJwt.Net.Samples/03-Advanced/02-HaipCompliance.cs`

## What You Will Learn

- HAIP security levels and requirements
- Algorithm restrictions per level
- Compliance validation

## What is HAIP?

High Assurance Interoperability Profile (HAIP) specifies security requirements for credential ecosystems used in high-stakes scenarios:

- Government ID
- Financial services
- Healthcare records

## Security Levels

| Level   | Use Case             | Requirements                      |
| ------- | -------------------- | --------------------------------- |
| Level 1 | General purpose      | ES256+, RS256+                    |
| Level 2 | Financial, corporate | ES256+, P-256+, no RSA            |
| Level 3 | Government, critical | ES384+, P-384+, strict validation |

## Step 1: Configure HAIP Validator

```csharp
using SdJwt.Net.HAIP;
using SdJwt.Net.HAIP.Models;

var validator = new HaipValidator(SecurityLevel.Level2);
```

## Step 2: Validate Keys

```csharp
// Generate compliant key
var keyGenerator = new HaipKeyGenerator(SecurityLevel.Level2);
var issuerKey = keyGenerator.GenerateKey();  // P-256 for Level 2

// Validate existing key
var existingKey = LoadKey();
if (!validator.IsKeyCompliant(existingKey))
{
    throw new SecurityException("Key does not meet HAIP Level 2 requirements");
}
```

## Step 3: Validate Algorithms

```csharp
// Check algorithm compliance
var algorithm = SecurityAlgorithms.EcdsaSha256;

if (!validator.IsAlgorithmCompliant(algorithm))
{
    throw new SecurityException($"Algorithm {algorithm} not allowed at Level 2");
}
```

## Step 4: HAIP-Compliant Issuance

```csharp
var haipIssuer = new HaipSdJwtIssuer(
    signingKey: issuerKey,
    securityLevel: SecurityLevel.Level2
);

var payload = new Dictionary<string, object>
{
    ["iss"] = "https://gov.example.com",
    ["sub"] = "citizen-123",
    ["given_name"] = "Alice",
    ["family_name"] = "Smith",
    ["national_id"] = "123-45-6789"
};

var options = new SdIssuanceOptions
{
    DisclosureStructure = new
    {
        given_name = true,
        family_name = true,
        national_id = true
    }
};

// Issuance validates algorithm compliance automatically
var credential = haipIssuer.Issue(payload, options, holderPublicKey);
```

## Step 5: HAIP-Compliant Verification

```csharp
var haipVerifier = new HaipSdVerifier(
    keyResolver: ResolveIssuerKey,
    securityLevel: SecurityLevel.Level2
);

var result = await haipVerifier.VerifyAsync(
    presentation,
    sdJwtParams,
    kbJwtParams
);

// Verifier automatically rejects non-compliant algorithms
```

## Algorithm Reference

### Level 1 (Minimum Security)

```csharp
// Allowed signing algorithms
var level1Algorithms = new[]
{
    SecurityAlgorithms.EcdsaSha256,   // ES256
    SecurityAlgorithms.EcdsaSha384,   // ES384
    SecurityAlgorithms.EcdsaSha512,   // ES512
    SecurityAlgorithms.RsaSha256,     // RS256
    SecurityAlgorithms.RsaSha384,     // RS384
    SecurityAlgorithms.RsaSha512      // RS512
};
```

### Level 2 (High Security)

```csharp
// RSA not allowed
var level2Algorithms = new[]
{
    SecurityAlgorithms.EcdsaSha256,   // ES256 (P-256)
    SecurityAlgorithms.EcdsaSha384,   // ES384 (P-384)
    SecurityAlgorithms.EcdsaSha512    // ES512 (P-521)
};

// Minimum curve: P-256
```

### Level 3 (Critical Security)

```csharp
// Minimum ES384
var level3Algorithms = new[]
{
    SecurityAlgorithms.EcdsaSha384,   // ES384 (P-384)
    SecurityAlgorithms.EcdsaSha512    // ES512 (P-521)
};

// Minimum curve: P-384
// Additional: strict timestamp validation
```

## Blocked Algorithms

HAIP explicitly blocks weak cryptography:

```csharp
// NEVER allowed at any level
var blockedAlgorithms = new[]
{
    "none",           // No signature
    "HS256",          // Symmetric (not for credentials)
    "RS256" /* at Level 2+ */
};

// NEVER allowed hash algorithms
var blockedHashes = new[]
{
    "MD5",            // Cryptographically broken
    "SHA1"            // Deprecated, collision attacks
};
```

## Timestamp Validation

Higher levels require stricter timing:

```csharp
// Level 3 requires:
var level3Options = new HaipValidationOptions
{
    MaxClockSkew = TimeSpan.FromMinutes(1),     // Tighter tolerance
    RequireIat = true,                           // Must have issued-at
    MaxCredentialAge = TimeSpan.FromHours(24),   // Freshness requirement
    RequireExp = true                            // Must have expiration
};
```

## Reporting Compliance

```csharp
var report = validator.GenerateComplianceReport(credential);

Console.WriteLine($"HAIP Level: {report.Level}");
Console.WriteLine($"Signing Algorithm: {report.Algorithm} - {report.AlgorithmCompliant}");
Console.WriteLine($"Key Curve: {report.KeyCurve} - {report.KeyCompliant}");
Console.WriteLine($"Timestamps: {report.TimestampCompliant}");
Console.WriteLine($"Overall: {(report.IsCompliant ? "PASS" : "FAIL")}");
```

## Migration Path

Upgrade from Level 1 to Level 2:

```csharp
// 1. Generate new Level 2 compliant keys
var newIssuerKey = keyGenerator.GenerateKey(SecurityLevel.Level2);

// 2. Publish new keys (keep old for transition)
var jwks = new JsonWebKeySet
{
    Keys = { oldKey, newIssuerKey }
};

// 3. Issue new credentials with Level 2 algorithm
var newCredentials = haipIssuer.Issue(payload, options);

// 4. After transition period, remove old keys
```

## Run the Sample

```bash
cd samples/SdJwt.Net.Samples
dotnet run -- 3.2
```

## Next Steps

- [Multi-Credential Flow](03-multi-credential-flow.md) - Complex presentations
- [Key Rotation](04-key-rotation.md) - Key management

## Key Takeaways

1. HAIP defines three security levels for different use cases
2. Higher levels restrict algorithms and require elliptic curves
3. MD5 and SHA1 are blocked at all levels
4. Use `HaipValidator` to check compliance before deployment
