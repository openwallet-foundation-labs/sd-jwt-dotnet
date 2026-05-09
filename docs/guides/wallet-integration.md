# How to use reference wallet infrastructure

| Field                | Value                                                           |
| -------------------- | --------------------------------------------------------------- |
| **Package maturity** | Reference (SdJwt.Net.Wallet)                                    |
| **Code status**      | Runnable package APIs with illustrative wiring                  |
| **Related concept**  | [Verifiable Credentials](../concepts/verifiable-credentials.md) |

> **Boundary:** This is reference wallet infrastructure. It is not a production mobile wallet, a certified SDK, a key custody solution, or a replacement for platform-specific secure storage.

|                      |                                                                                                                                                                                                                                                |
| -------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Audience**         | Developers building wallet applications for storing, managing, and presenting verifiable credentials.                                                                                                                                          |
| **Purpose**          | Walk through the wallet plugin architecture - credential storage, key management, format plugins, and OID4VC protocol adapters - with step-by-step code for common operations using `SdJwt.Net.Wallet`.                                        |
| **Scope**            | Wallet setup, credential storage/query, selective disclosure presentation, validation, custom storage/key providers, and OID4VC adapters. Out of scope: EUDIW-specific compliance (see [EUDI Wallet Integration](eudi-wallet-integration.md)). |
| **Success criteria** | Reader can create a wallet, store credentials, present selective claims to a verifier, implement custom secure storage and HSM key management, and wire OID4VCI/OID4VP adapters.                                                               |

## What your application still owns

This guide does not provide: production key custody, platform-specific secure storage (iOS Keychain, Android Keystore, HSM), certified wallet builds, user authentication, consent UX, credential backup and recovery, or compliance with regional wallet certification schemes.

---

## Key decisions

| Decision            | Options                          | Guidance                                 |
| ------------------- | -------------------------------- | ---------------------------------------- |
| Credential storage? | In-memory, Secure storage, Cloud | Secure storage for production            |
| Key management?     | Software, Hardware, HSM          | Hardware-backed for production           |
| Credential formats? | SD-JWT VC, mdoc, JWT             | SD-JWT VC for selective disclosure       |
| Protocol adapters?  | OID4VCI, OID4VP, custom          | OID4VCI for issuance, OID4VP for present |

---

## Prerequisites

```bash
dotnet add package SdJwt.Net.Wallet
```

## Wallet architecture overview

The wallet has several key components:

- `GenericWallet` — main coordinator for wallet operations
- `ICredentialStore` — storage abstraction for credentials
- `IKeyManager` — cryptographic key management
- `ICredentialFormatPlugin` — format-specific handlers (SD-JWT VC, mdoc)
- Protocol adapters — OID4VCI for issuance, OID4VP for presentation

## 1. Set up the wallet

```csharp
using SdJwt.Net.Wallet;
using SdJwt.Net.Wallet.Storage;
using SdJwt.Net.Wallet.Formats;

// Create components
var credentialStore = new InMemoryCredentialStore();
var keyManager = new YourKeyManager(); // Implement IKeyManager
var sdJwtPlugin = new SdJwtVcFormatPlugin();

// Create the wallet
var wallet = new GenericWallet(
    credentialStore,
    keyManager,
    new ICredentialFormatPlugin[] { sdJwtPlugin }
);
```

## 2. Store credentials

When receiving credentials from an issuer (via OID4VCI or other means):

```csharp
// Create a credential to store
var credential = new StoredCredential
{
    Format = "dc+sd-jwt",
    RawCredential = sdJwtString,
    CredentialType = "IdentityCredential",
    IssuerIdentifier = "https://issuer.example.com",
    Claims = new Dictionary<string, object>
    {
        ["given_name"] = "John",
        ["family_name"] = "Doe",
        ["birth_date"] = "1990-01-15"
    }
};

// Store in the wallet
string credentialId = await wallet.StoreCredentialAsync(credential);
Console.WriteLine($"Stored credential: {credentialId}");
```

## 3. List and query credentials

```csharp
// List all credentials
var allCredentials = await wallet.ListCredentialsAsync();

foreach (var cred in allCredentials)
{
    Console.WriteLine($"ID: {cred.Id}, Type: {cred.CredentialType}");
}

// Query by type
var identityCredentials = await credentialStore.QueryAsync(
    new CredentialQuery
    {
        Types = new[] { "IdentityCredential" }
    }
);

// Query by issuer
var fromIssuer = await credentialStore.QueryAsync(
    new CredentialQuery
    {
        Issuers = new[] { "https://issuer.example.com" }
    }
);
```

## 4. Find matching credentials for presentation

When a verifier requests specific credential types:

```csharp
// Find credentials matching a request
var matchingCredentials = await wallet.FindMatchingCredentialsAsync(
    types: new[] { "IdentityCredential", "DriversLicenseCredential" },
    issuers: new[] { "https://trusted-issuer.gov" }
);

if (matchingCredentials.Any())
{
    Console.WriteLine($"Found {matchingCredentials.Count()} matching credentials");
}
```

## 5. Create presentations with selective disclosure

Present only the claims needed for a specific interaction:

```csharp
// Create a selective disclosure presentation
string presentation = await wallet.CreatePresentationAsync(
    credentialId: credentialId,
    claimsToDisclose: new[] { "given_name", "birth_date" }, // Only these claims
    audience: "https://verifier.example.com",
    nonce: "unique-nonce-from-verifier"
);

// The presentation only reveals given_name and birth_date
// family_name remains hidden
```

## 6. Validate credentials

```csharp
// Validate a credential
var validationResult = await wallet.ValidateCredentialAsync(credentialId);

if (validationResult.IsValid)
{
    Console.WriteLine("Credential is valid");
}
else
{
    Console.WriteLine($"Validation failed: {validationResult.ErrorMessage}");
}

// Check revocation status
var statusResult = await wallet.CheckStatusAsync(credentialId);

if (statusResult.IsActive)
{
    Console.WriteLine("Credential is active");
}
```

## 7. Implementing custom storage

For production use, implement secure storage:

```csharp
public class SecureCredentialStore : ICredentialStore
{
    public async Task<string> StoreAsync(StoredCredential credential)
    {
        // Encrypt and store securely
        var encrypted = await EncryptCredential(credential);
        await _secureStorage.SaveAsync(credential.Id, encrypted);
        return credential.Id;
    }

    public async Task<StoredCredential?> GetByIdAsync(string id)
    {
        var encrypted = await _secureStorage.LoadAsync(id);
        if (encrypted == null) return null;
        return await DecryptCredential(encrypted);
    }

    public async Task<IEnumerable<StoredCredential>> QueryAsync(CredentialQuery query)
    {
        // Query implementation with filtering
        var all = await GetAllDecrypted();
        return FilterByQuery(all, query);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        return await _secureStorage.DeleteAsync(id);
    }

    public async Task<bool> UpdateAsync(StoredCredential credential)
    {
        var encrypted = await EncryptCredential(credential);
        return await _secureStorage.UpdateAsync(credential.Id, encrypted);
    }
}
```

## 8. Implementing key management

```csharp
public class HardwareKeyManager : IKeyManager
{
    public async Task<KeyInfo> GenerateKeyAsync(KeyGenerationOptions options)
    {
        // Generate key in hardware security module
        var keyId = await _hsm.GenerateKeyPairAsync(options.Algorithm);
        return new KeyInfo
        {
            KeyId = keyId,
            Algorithm = options.Algorithm,
            PublicKey = await _hsm.GetPublicKeyAsync(keyId)
        };
    }

    public async Task<byte[]> SignAsync(string keyId, byte[] data)
    {
        return await _hsm.SignAsync(keyId, data);
    }

    public async Task<KeyInfo?> GetKeyInfoAsync(string keyId)
    {
        if (!await _hsm.KeyExistsAsync(keyId)) return null;
        return new KeyInfo
        {
            KeyId = keyId,
            PublicKey = await _hsm.GetPublicKeyAsync(keyId)
        };
    }

    public async Task<bool> DeleteKeyAsync(string keyId)
    {
        return await _hsm.DeleteKeyAsync(keyId);
    }
}
```

## Plugin architecture

The wallet uses a plugin system for credential format handling. Each format plugin implements:

```csharp
public interface ICredentialFormatPlugin
{
    string FormatIdentifier { get; }
    bool CanHandle(string format);

    Task<ParsedCredential> ParseAsync(string rawCredential, ParseOptions? options = null);
    Task<string> CreatePresentationAsync(StoredCredential credential, PresentationContext context);
    Task<ValidationResult> ValidateAsync(StoredCredential credential, ValidationContext context);
}
```

### Supported formats

| Format      | Plugin Class          | Description                   |
| ----------- | --------------------- | ----------------------------- |
| `dc+sd-jwt` | `SdJwtVcFormatPlugin` | SD-JWT Verifiable Credentials |
| `mso_mdoc`  | (Coming soon)         | ISO 18013-5 mdoc/mDL          |

## Best practices

- Never store credentials in plain text. Use encrypted, hardware-backed storage in production.
- Use hardware security modules or secure enclaves for key management.
- Only present the minimum claims required for each interaction.
- Validate credentials before creating presentations.
- Implement automatic cleanup or renewal of expired credentials.
- Log operation metadata and evidence receipts. Do not log raw credential values, undisclosed claims, or private key material.

## Integration with OID4VCI/OID4VP

The wallet provides adapter interfaces for OpenID4VC protocols:

```csharp
// OID4VCI adapter for credential issuance
public interface IOid4VciAdapter
{
    Task<CredentialOffer> ParseOfferAsync(string offerUri);
    Task<TokenResponse> RequestTokenAsync(AuthorizationDetails details);
    Task<StoredCredential> RequestCredentialAsync(CredentialRequest request);
}

// OID4VP adapter for credential presentation
public interface IOid4VpAdapter
{
    Task<AuthorizationRequest> ParseRequestAsync(string requestUri);
    Task<AuthorizationResponse> CreateResponseAsync(
        AuthorizationRequest request,
        IEnumerable<StoredCredential> selectedCredentials
    );
}
```

---

## See also

- [Issuing Credentials](issuing-credentials.md)
- [Verifying Presentations](verifying-presentations.md)
- [Managing Revocation](managing-revocation.md)
- [EUDI Wallet Architecture](https://github.com/eu-digital-identity-wallet)
