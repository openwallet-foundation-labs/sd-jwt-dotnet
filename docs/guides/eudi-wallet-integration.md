# How to build an EUDI-compliant wallet

|                      |                                                                                                                                                                                                                                                                    |
| -------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| **Audience**         | Developers building eIDAS 2.0-compliant wallet applications, and compliance officers validating ARF conformance.                                                                                                                                                   |
| **Purpose**          | Walk through configuring an EU Digital Identity Wallet with ARF enforcement, HAIP compliance, PID/mDL validation, member-state checking, and trust list integration using `SdJwt.Net.Eudiw`.                                                                       |
| **Scope**            | EUDI wallet creation and configuration, algorithm/type/claim validation, member-state and issuer trust checks, ARF-enforced storage and presentation, and error handling. Out of scope: general wallet concepts (see [Wallet Integration](wallet-integration.md)). |
| **Success criteria** | Reader can create an EUDI wallet, validate PID credentials, check issuer trust against EU LOTL, and handle ARF compliance exceptions in credential operations.                                                                                                     |

---

## Key decisions

| Decision               | Options                    | Guidance                  |
| ---------------------- | -------------------------- | ------------------------- |
| ARF enforcement level? | Strict, Warning, Disabled  | Strict for production     |
| HAIP compliance level? | Level 1, 2, 3              | Level 2 minimum for EUDIW |
| Trust validation?      | LOTL, Per-issuer, Disabled | LOTL for production       |
| Credential storage?    | In-memory, Secure, HSM     | HSM-backed for production |

---

## Prerequisites

```bash
dotnet add package SdJwt.Net.Eudiw
dotnet add package SdJwt.Net.Wallet
```

## EUDIW overview

The EU Digital Identity Wallet mandated by eIDAS 2.0 requires:

- ARF compliance: only ES256/ES384/ES512 algorithms
- HAIP Level 2: Very High assurance minimum
- EU Trust Lists: issuer validation via LOTL
- PID/mDL support: core credential formats

## 1. Create an EUDI wallet

```csharp
using SdJwt.Net.Eudiw;
using SdJwt.Net.Wallet.Storage;

// Create storage and key management
var store = new InMemoryCredentialStore();
var keyManager = new SoftwareKeyManager();

// Create EUDI wallet with defaults (ARF enforced)
var wallet = new EudiWallet(store, keyManager);

Console.WriteLine($"ARF Enforced: {wallet.IsArfEnforced}");      // true
Console.WriteLine($"HAIP Level: {wallet.MinimumHaipLevel}");     // 2
```

## 2. Configure options

```csharp
var options = new EudiWalletOptions
{
    WalletId = "citizen-wallet-001",
    DisplayName = "My EU Wallet",
    EnforceArfCompliance = true,
    MinimumHaipLevel = 2,                    // HAIP Level 2 (Very High)
    ValidateIssuerTrust = true,              // Validate against EU Trust Lists
    TrustListCacheHours = 6,                 // Cache LOTL for 6 hours
    SupportedCredentialTypes = new[]
    {
        EudiwConstants.Pid.DocType,          // Person Identification Data
        EudiwConstants.Mdl.DocType,          // Mobile Driving License
        "eu.europa.ec.eudi.health.1"         // Health credentials
    }
};

var wallet = new EudiWallet(store, keyManager, eudiOptions: options);
```

## 3. Validate algorithms

EUDIW only allows specific ECDSA algorithms per ARF:

```csharp
// Valid algorithms per EUDIW ARF
wallet.ValidateAlgorithm("ES256"); // true
wallet.ValidateAlgorithm("ES384"); // true
wallet.ValidateAlgorithm("ES512"); // true

// Invalid algorithms
wallet.ValidateAlgorithm("RS256"); // false - RSA not allowed
wallet.ValidateAlgorithm("PS256"); // false - PS not in ARF
wallet.ValidateAlgorithm("HS256"); // false - symmetric not allowed
```

## 4. Validate credential types

```csharp
// Validate PID credential type
var pidResult = wallet.ValidateCredentialType(EudiwConstants.Pid.DocType);
if (pidResult.IsValid)
{
    Console.WriteLine($"Type: {pidResult.CredentialType}"); // Pid
}

// Validate mDL credential type
var mdlResult = wallet.ValidateCredentialType(EudiwConstants.Mdl.DocType);
if (mdlResult.IsValid)
{
    Console.WriteLine($"Type: {mdlResult.CredentialType}"); // Mdl
}

// Unknown types are rejected
var unknownResult = wallet.ValidateCredentialType("custom.credential");
Console.WriteLine(unknownResult.IsValid); // false
```

## 5. Validate PID claims

Person Identification Data must contain mandatory claims:

```csharp
// Complete PID claims
var validClaims = new Dictionary<string, object>
{
    // Mandatory claims per ARF
    ["family_name"] = "Garcia",
    ["given_name"] = "Sofia",
    ["birth_date"] = "1990-05-15",
    ["issuance_date"] = "2025-01-01",
    ["expiry_date"] = "2030-01-01",
    ["issuing_authority"] = "Spanish Ministry of Interior",
    ["issuing_country"] = "ES",

    // Optional claims
    ["age_over_18"] = true,
    ["nationality"] = "ES"
};

var result = wallet.ValidatePidClaims(validClaims);
if (result.IsValid)
{
    // Extract typed PID credential
    var pid = wallet.ExtractPidCredential(validClaims);
    Console.WriteLine($"Name: {pid.GivenName} {pid.FamilyName}");
    Console.WriteLine($"Issued by: {pid.IssuingAuthority}");
}

// Incomplete claims are rejected
var incompleteClaims = new Dictionary<string, object>
{
    ["family_name"] = "Garcia",
    ["given_name"] = "Sofia"
    // Missing: birth_date, issuance_date, expiry_date, etc.
};

var invalidResult = wallet.ValidatePidClaims(incompleteClaims);
Console.WriteLine($"Valid: {invalidResult.IsValid}"); // false
Console.WriteLine($"Missing: {string.Join(", ", invalidResult.MissingClaims)}");
```

## 6. Validate member states

Only EU member state issuers are accepted:

```csharp
// EU member states
wallet.ValidateMemberState("DE"); // true - Germany
wallet.ValidateMemberState("FR"); // true - France
wallet.ValidateMemberState("ES"); // true - Spain
wallet.ValidateMemberState("IT"); // true - Italy

// Non-EU countries
wallet.ValidateMemberState("US"); // false
wallet.ValidateMemberState("GB"); // false - UK left EU
wallet.ValidateMemberState("CH"); // false - Switzerland not EU

// Get all supported member states
var memberStates = wallet.GetSupportedMemberStates();
Console.WriteLine($"Supported: {memberStates.Count} member states"); // 27
```

## 7. Validate issuer trust

Issuers must appear in EU Trust Lists:

```csharp
// German PID provider
var trustResult = await wallet.ValidateIssuerTrustAsync(
    "https://pid-provider.bundesdruckerei.de");

if (trustResult.IsTrusted)
{
    Console.WriteLine($"Country: {trustResult.MemberState}");    // DE
    Console.WriteLine($"Type: {trustResult.ServiceType}");       // QualifiedAttestation
}

// Non-EU issuer
var untrustedResult = await wallet.ValidateIssuerTrustAsync(
    "https://issuer.example.com");

if (!untrustedResult.IsTrusted)
{
    Console.WriteLine($"Errors: {string.Join(", ", untrustedResult.Errors)}");
}
```

## 8. Store credentials with enforcement

Credentials are validated against ARF when stored:

```csharp
try
{
    // Store with automatic ARF validation
    var stored = await wallet.StoreCredentialAsync(parsedCredential);
    Console.WriteLine($"Stored: {stored.Id}");
}
catch (ArfComplianceException ex)
{
    // Credential type or format not ARF-compliant
    Console.WriteLine($"ARF Violations: {string.Join(", ", ex.Violations)}");
}
catch (EudiTrustException ex)
{
    // Issuer not in EU Trust List
    Console.WriteLine($"Trust Error: {ex.Message}");
}
```

## 9. Find specific credential types

```csharp
// Find all PID credentials
var pidCredentials = await wallet.FindPidCredentialsAsync();
foreach (var cred in pidCredentials)
{
    Console.WriteLine($"PID: {cred.Id} from {cred.Issuer}");
}

// Find all mDL credentials
var mdlCredentials = await wallet.FindMdlCredentialsAsync();
foreach (var cred in mdlCredentials)
{
    Console.WriteLine($"mDL: {cred.Id} from {cred.Issuer}");
}
```

## 10. Create presentations with ARF validation

```csharp
// Presentation is validated against ARF before creation
try
{
    var presentation = await wallet.CreatePresentationAsync(
        credentialId: storedCredential.Id,
        disclosurePaths: new[] { "family_name", "birth_date", "age_over_18" },
        audience: "https://verifier.example.eu",
        nonce: "unique-nonce-123"
    );
}
catch (ArfComplianceException ex)
{
    // Credential validity or type issues
    Console.WriteLine($"Cannot present: {ex.Message}");
}
```

## Error handling

### ARF compliance exceptions

```csharp
try
{
    // ... wallet operations
}
catch (ArfComplianceException ex)
{
    // List of specific ARF violations
    foreach (var violation in ex.Violations)
    {
        Console.WriteLine($"- {violation}");
    }
}
```

### Trust exceptions

```csharp
try
{
    // ... wallet operations
}
catch (EudiTrustException ex)
{
    // Issuer not trusted by EU Trust Lists
    Console.WriteLine($"Trust failure: {ex.Message}");
}
```

## Best practices

- Keep `EnforceArfCompliance = true` in production.
- Set `RequireHardwareKeys = true` for high-assurance scenarios.
- Choose an appropriate `TrustListCacheHours` to balance freshness and performance.
- Validate credentials before storing or presenting.
- Audit all trust validation results for compliance.
- Catch both `ArfComplianceException` and `EudiTrustException` explicitly.

---

## See also

- [EUDIW Deep Dive](../concepts/eudiw-deep-dive.md)
- [Wallet Integration Guide](wallet-integration.md)
- [HAIP Compliance](../concepts/haip-compliance.md)
- [EU Digital Identity Wallet Architecture](https://digital-strategy.ec.europa.eu/en/library/european-digital-identity-wallet-architecture-and-reference-framework)
