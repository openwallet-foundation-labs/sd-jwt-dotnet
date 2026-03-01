# How to Build a Digital Credential Wallet

This guide demonstrates how to implement a digital wallet for managing, storing, and presenting verifiable credentials using the SdJwt.Net.Wallet package.

The wallet architecture follows patterns established by the EUDI Wallet (Android/iOS), providing a generic, extensible foundation that supports multiple credential formats including SD-JWT VC and mdoc (ISO 18013-5).

---

## Key Decisions

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

## Wallet Architecture Overview

The wallet consists of several key components:

- **GenericWallet** - Main coordinator for wallet operations
- **ICredentialStore** - Storage abstraction for credentials
- **IKeyManager** - Cryptographic key management
- **ICredentialFormatPlugin** - Format-specific handlers (SD-JWT VC, mdoc)
- **Protocol Adapters** - OID4VCI for issuance, OID4VP for presentation

## 1. Set Up the Wallet

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

## 2. Store Credentials

When receiving credentials from an Issuer (via OID4VCI or other means):

```csharp
// Create a credential to store
var credential = new StoredCredential
{
    Format = "vc+sd-jwt",
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

## 3. List and Query Credentials

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

## 4. Find Matching Credentials for Presentation

When a Verifier requests specific credential types:

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

## 5. Create Presentations with Selective Disclosure

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

## 6. Validate Credentials

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

## 7. Implementing Custom Storage

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

## 8. Implementing Key Management

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

## Plugin Architecture

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

### Supported Formats

| Format      | Plugin Class          | Description                   |
| ----------- | --------------------- | ----------------------------- |
| `vc+sd-jwt` | `SdJwtVcFormatPlugin` | SD-JWT Verifiable Credentials |
| `mso_mdoc`  | (Coming soon)         | ISO 18013-5 mdoc/mDL          |

## Best Practices

1. **Use Secure Storage** - Never store credentials in plain text. Use encrypted, hardware-backed storage in production.

2. **Protect Keys** - Use hardware security modules or secure enclaves for key management.

3. **Minimize Disclosure** - Only present the minimum claims required for each interaction.

4. **Validate Before Presenting** - Always validate credentials before creating presentations.

5. **Handle Expiration** - Implement automatic cleanup or renewal of expired credentials.

6. **Audit Logging** - Log all credential operations for security audit trails.

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

## See Also

- [Issuing Credentials](issuing-credentials.md)
- [Verifying Presentations](verifying-presentations.md)
- [Managing Revocation](managing-revocation.md)
- [EUDI Wallet Architecture](https://github.com/eu-digital-identity-wallet)
