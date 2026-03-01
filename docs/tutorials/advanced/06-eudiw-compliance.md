# EUDIW Compliance Tutorial

Build an EU Digital Identity Wallet with full ARF compliance using the SD-JWT .NET ecosystem.

**Duration**: 25 minutes  
**Difficulty**: Advanced  
**Prerequisites**:

- Completed [HAIP Compliance](02-haip-compliance.md)
- Understanding of EU eIDAS 2.0 regulation

## Overview

The EU Digital Identity Wallet (EUDIW) framework requires specific compliance measures:

- Architecture Reference Framework (ARF) algorithms
- EU Trust List (LOTL) integration
- Person Identification Data (PID) handling
- HAIP Level 2 minimum security

This tutorial builds a compliant wallet application step by step.

---

## Step 1: Install Packages

```bash
dotnet add package SdJwt.Net.Eudiw
dotnet add package SdJwt.Net.Wallet
dotnet add package SdJwt.Net.HAIP
```

## Step 2: Create the EudiWallet

```csharp
using SdJwt.Net.Eudiw;
using SdJwt.Net.Wallet.Storage;

// Set up storage and key management
var store = new InMemoryCredentialStore();
var keyManager = new SoftwareKeyManager();

// Configure EUDIW options
var options = new EudiWalletOptions
{
    WalletId = "demo-eudi-wallet",
    DisplayName = "My EU Wallet",
    EnforceArfCompliance = true,
    MinimumHaipLevel = 2,
    ValidateIssuerTrust = true,
    TrustListCacheHours = 6,
    SupportedCredentialTypes = new[]
    {
        EudiwConstants.Pid.DocType,   // PID
        EudiwConstants.Mdl.DocType    // mDL
    }
};

var wallet = new EudiWallet(store, keyManager, eudiOptions: options);

Console.WriteLine($"Wallet ID: {wallet.Options.WalletId}");
Console.WriteLine($"ARF Enforced: {wallet.IsArfEnforced}");
Console.WriteLine($"HAIP Level: {wallet.MinimumHaipLevel}");
```

**Expected Output:**

```
Wallet ID: demo-eudi-wallet
ARF Enforced: True
HAIP Level: 2
```

## Step 3: Validate ARF Algorithms

EUDIW only allows ECDSA algorithms per the Architecture Reference Framework:

```csharp
// Test algorithm compliance
var algorithms = new[]
{
    ("ES256", true),   // Allowed
    ("ES384", true),   // Allowed
    ("ES512", true),   // Allowed
    ("RS256", false),  // Not in ARF
    ("PS256", false),  // Not in ARF
    ("HS256", false)   // Symmetric not allowed
};

foreach (var (alg, expected) in algorithms)
{
    var valid = wallet.ValidateAlgorithm(alg);
    var status = valid == expected ? "OK" : "FAIL";
    Console.WriteLine($"[{status}] {alg}: {valid}");
}
```

**Expected Output:**

```
[OK] ES256: True
[OK] ES384: True
[OK] ES512: True
[OK] RS256: False
[OK] PS256: False
[OK] HS256: False
```

## Step 4: Validate EU Member States

Only credentials from EU member states are accepted:

```csharp
// EU member states
var euCountries = new[] { "DE", "FR", "ES", "IT", "PL", "NL", "BE", "AT" };
foreach (var country in euCountries)
{
    var valid = wallet.ValidateMemberState(country);
    Console.WriteLine($"{country}: {valid}");
}

Console.WriteLine();

// Non-EU countries (rejected)
var nonEuCountries = new[] { "US", "GB", "CH", "NO", "AU" };
foreach (var country in nonEuCountries)
{
    var valid = wallet.ValidateMemberState(country);
    Console.WriteLine($"{country}: {valid} (non-EU)");
}

// Get all supported member states
var allStates = wallet.GetSupportedMemberStates();
Console.WriteLine($"\nTotal EU member states: {allStates.Count}");
```

**Expected Output:**

```
DE: True
FR: True
ES: True
IT: True
PL: True
NL: True
BE: True
AT: True

US: False (non-EU)
GB: False (non-EU)
CH: False (non-EU)
NO: False (non-EU)
AU: False (non-EU)

Total EU member states: 27
```

## Step 5: Validate Credential Types

Only PID, mDL, and qualified attestation types are allowed:

```csharp
// Test credential types
var credTypes = new[]
{
    EudiwConstants.Pid.DocType,     // PID
    EudiwConstants.Mdl.DocType,     // mDL
    "eu.europa.ec.eudi.health.1",   // Qualified attestation
    "custom.unknown.credential"     // Unknown
};

foreach (var docType in credTypes)
{
    var result = wallet.ValidateCredentialType(docType);
    Console.WriteLine($"{docType}:");
    Console.WriteLine($"  Valid: {result.IsValid}");
    Console.WriteLine($"  Type: {result.CredentialType}");
}
```

**Expected Output:**

```
eu.europa.ec.eudi.pid.1:
  Valid: True
  Type: Pid
org.iso.18013.5.1.mDL:
  Valid: True
  Type: Mdl
eu.europa.ec.eudi.health.1:
  Valid: True
  Type: Qeaa
custom.unknown.credential:
  Valid: False
  Type: Unknown
```

## Step 6: Validate PID Claims

Person Identification Data must contain mandatory claims:

```csharp
// Valid PID with all mandatory claims
var validPidClaims = new Dictionary<string, object>
{
    ["family_name"] = "Mueller",
    ["given_name"] = "Anna",
    ["birth_date"] = "1985-03-20",
    ["issuance_date"] = "2025-01-01",
    ["expiry_date"] = "2030-01-01",
    ["issuing_authority"] = "Bundesdruckerei",
    ["issuing_country"] = "DE"
};

var validResult = wallet.ValidatePidClaims(validPidClaims);
Console.WriteLine($"Valid PID: {validResult.IsValid}");
Console.WriteLine($"Missing claims: {validResult.MissingClaims.Count}");

// Invalid PID (missing mandatory claims)
var invalidPidClaims = new Dictionary<string, object>
{
    ["family_name"] = "Mueller",
    ["given_name"] = "Anna"
    // Missing: birth_date, issuance_date, expiry_date, etc.
};

var invalidResult = wallet.ValidatePidClaims(invalidPidClaims);
Console.WriteLine($"\nIncomplete PID: {invalidResult.IsValid}");
Console.WriteLine($"Missing: {string.Join(", ", invalidResult.MissingClaims)}");
```

**Expected Output:**

```
Valid PID: True
Missing claims: 0

Incomplete PID: False
Missing: birth_date, issuance_date, expiry_date, issuing_authority, issuing_country
```

## Step 7: Extract PID Credential

Convert claims to a typed PID credential:

```csharp
if (validResult.IsValid)
{
    var pid = wallet.ExtractPidCredential(validPidClaims);

    Console.WriteLine("Extracted PID:");
    Console.WriteLine($"  Name: {pid.GivenName} {pid.FamilyName}");
    Console.WriteLine($"  Birth Date: {pid.BirthDate}");
    Console.WriteLine($"  Issuer: {pid.IssuingAuthority}");
    Console.WriteLine($"  Country: {pid.IssuingCountry}");
    Console.WriteLine($"  Valid Until: {pid.ExpiryDate}");
}
```

**Expected Output:**

```
Extracted PID:
  Name: Anna Mueller
  Birth Date: 1985-03-20
  Issuer: Bundesdruckerei
  Country: DE
  Valid Until: 2030-01-01
```

## Step 8: Validate Issuer Trust

Validate that credential issuers are in EU Trust Lists:

```csharp
// German PID provider (in EU Trust Lists)
var trustedResult = await wallet.ValidateIssuerTrustAsync(
    "https://pid-provider.bundesdruckerei.de");

Console.WriteLine("Trusted Issuer:");
Console.WriteLine($"  Trusted: {trustedResult.IsTrusted}");
Console.WriteLine($"  Member State: {trustedResult.MemberState}");
Console.WriteLine($"  Service Type: {trustedResult.ServiceType}");

// Non-EU issuer (not trusted)
var untrustedResult = await wallet.ValidateIssuerTrustAsync(
    "https://issuer.example.com");

Console.WriteLine("\nUnknown Issuer:");
Console.WriteLine($"  Trusted: {untrustedResult.IsTrusted}");
if (!untrustedResult.IsTrusted)
{
    Console.WriteLine($"  Errors: {string.Join(", ", untrustedResult.Errors)}");
}
```

**Expected Output:**

```
Trusted Issuer:
  Trusted: True
  Member State: DE
  Service Type: QualifiedAttestation

Unknown Issuer:
  Trusted: False
  Errors: Issuer not found in EU Trust Lists
```

## Step 9: Store with ARF Enforcement

Credentials are validated against ARF requirements during storage:

```csharp
try
{
    // Store credential (validation happens automatically)
    var stored = await wallet.StoreCredentialAsync(parsedCredential);
    Console.WriteLine($"Stored successfully: {stored.Id}");
}
catch (ArfComplianceException ex)
{
    Console.WriteLine("ARF Compliance Failure:");
    foreach (var violation in ex.Violations)
    {
        Console.WriteLine($"  - {violation}");
    }
}
catch (EudiTrustException ex)
{
    Console.WriteLine($"Trust Failure: {ex.Message}");
}
```

## Step 10: Create Presentations

Create presentations with ARF-enforced credentials:

```csharp
try
{
    var presentation = await wallet.CreatePresentationAsync(
        credentialId: storedCredential.Id,
        disclosurePaths: new[] { "family_name", "birth_date", "age_over_18" },
        audience: "https://verifier.example.eu",
        nonce: "unique-nonce-123"
    );

    Console.WriteLine($"Presentation created: {presentation.Length} bytes");
}
catch (ArfComplianceException ex)
{
    Console.WriteLine($"Cannot present: {string.Join(", ", ex.Violations)}");
}
```

---

## Complete Example

```csharp
using SdJwt.Net.Eudiw;
using SdJwt.Net.Wallet.Storage;

public class EudiwComplianceDemo
{
    public static async Task RunAsync()
    {
        // 1. Create wallet
        var store = new InMemoryCredentialStore();
        var keyManager = new SoftwareKeyManager();

        var options = new EudiWalletOptions
        {
            WalletId = "demo-wallet",
            EnforceArfCompliance = true,
            MinimumHaipLevel = 2,
            ValidateIssuerTrust = true
        };

        var wallet = new EudiWallet(store, keyManager, eudiOptions: options);

        // 2. Validate algorithm
        var isEs256Valid = wallet.ValidateAlgorithm("ES256");
        Console.WriteLine($"ES256 valid: {isEs256Valid}");

        // 3. Validate member state
        var isDeValid = wallet.ValidateMemberState("DE");
        Console.WriteLine($"Germany valid: {isDeValid}");

        // 4. Validate PID claims
        var pidClaims = new Dictionary<string, object>
        {
            ["family_name"] = "Mueller",
            ["given_name"] = "Anna",
            ["birth_date"] = "1985-03-20",
            ["issuance_date"] = "2025-01-01",
            ["expiry_date"] = "2030-01-01",
            ["issuing_authority"] = "Bundesdruckerei",
            ["issuing_country"] = "DE"
        };

        var pidResult = wallet.ValidatePidClaims(pidClaims);
        Console.WriteLine($"PID valid: {pidResult.IsValid}");

        // 5. Extract typed credential
        if (pidResult.IsValid)
        {
            var pid = wallet.ExtractPidCredential(pidClaims);
            Console.WriteLine($"Citizen: {pid.GivenName} {pid.FamilyName}");
        }

        // 6. Validate issuer trust
        var trustResult = await wallet.ValidateIssuerTrustAsync(
            "https://pid-provider.bundesdruckerei.de");
        Console.WriteLine($"Issuer trusted: {trustResult.IsTrusted}");

        Console.WriteLine("\nEUDIW compliance demo complete!");
    }
}
```

---

## Key Takeaways

1. **Algorithm Compliance**: Only ES256/ES384/ES512 are ARF-compliant
2. **Member State Validation**: Only 27 EU member states accepted
3. **PID Requirements**: Seven mandatory claims required
4. **Trust Validation**: Issuers must be in EU Trust Lists
5. **Storage Enforcement**: ARF validation happens at storage time
6. **Presentation Security**: ARF-enforced presentations protect holders

## Next Steps

- [Multi-Credential Flow](03-multi-credential-flow.md): Combine PID with other attestations
- [EUDIW Cross-Border Verification](../../use-cases/eudiw-cross-border-verification.md): Real-world verification scenarios
- [EUDIW Deep Dive](../../concepts/eudiw-deep-dive.md): Complete architecture reference
