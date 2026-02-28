# Tutorial: Key Rotation

Manage cryptographic key lifecycle for issuers and holders.

**Time:** 20 minutes  
**Level:** Advanced  
**Sample:** `samples/SdJwt.Net.Samples/03-Advanced/04-KeyRotation.cs`

## What You Will Learn

- Key rotation strategies
- Publishing new keys
- Validating during transition periods

## Why Rotate Keys?

- Limit exposure from potential compromise
- Comply with security policies
- Upgrade to stronger algorithms

## Key Lifecycle

```
┌───────────┐     ┌───────────┐     ┌───────────┐     ┌───────────┐
│  Created  │────>│  Active   │────>│ Retiring  │────>│  Retired  │
│           │     │ (signing) │     │(verify    │     │ (deleted) │
│           │     │           │     │ only)     │     │           │
└───────────┘     └───────────┘     └───────────┘     └───────────┘
```

## Step 1: Generate New Key

```csharp
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

// Generate new signing key
var newKey = ECDsa.Create(ECCurve.NamedCurves.nistP256);
var newSecurityKey = new ECDsaSecurityKey(newKey)
{
    KeyId = $"key-{DateTimeOffset.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}"
};
```

## Step 2: Publish Updated JWKS

```csharp
// Maintain both old and new keys during transition
var jwks = new JsonWebKeySet();

// Add new key (will be used for signing)
var newJwk = JsonWebKeyConverter.ConvertFromECDsaSecurityKey(newSecurityKey);
newJwk.Use = "sig";
newJwk.Alg = SecurityAlgorithms.EcdsaSha256;
jwks.Keys.Add(newJwk);

// Keep old key for verification during transition
var oldJwk = JsonWebKeyConverter.ConvertFromECDsaSecurityKey(oldSecurityKey);
oldJwk.Use = "sig";
oldJwk.Alg = SecurityAlgorithms.EcdsaSha256;
jwks.Keys.Add(oldJwk);

// Publish at /.well-known/jwks.json
var jwksJson = JsonSerializer.Serialize(jwks);
```

## Step 3: Update Issuer to Use New Key

```csharp
public class KeyRotatingIssuer
{
    private SecurityKey _activeSigningKey;
    private readonly List<SecurityKey> _validationKeys = new();

    public void RotateKey(SecurityKey newKey)
    {
        // Move current key to validation-only
        if (_activeSigningKey != null)
        {
            _validationKeys.Add(_activeSigningKey);
        }

        // Set new active signing key
        _activeSigningKey = newKey;

        // Publish updated JWKS
        PublishJwks();
    }

    public string Issue(Dictionary<string, object> payload, SdIssuanceOptions options)
    {
        var issuer = new SdIssuer(_activeSigningKey, SecurityAlgorithms.EcdsaSha256);
        return issuer.Issue(payload, options).Issuance;
    }
}
```

## Step 4: Verifier Handles Multiple Keys

```csharp
public class KeyResolvingVerifier
{
    private readonly HttpClient _httpClient;
    private readonly Dictionary<string, JsonWebKeySet> _keyCache = new();

    public async Task<SecurityKey> ResolveKey(string issuer, string keyId)
    {
        // Fetch JWKS (with caching)
        if (!_keyCache.TryGetValue(issuer, out var jwks))
        {
            var jwksUrl = $"{issuer}/.well-known/jwks.json";
            var jwksJson = await _httpClient.GetStringAsync(jwksUrl);
            jwks = JsonSerializer.Deserialize<JsonWebKeySet>(jwksJson);
            _keyCache[issuer] = jwks;
        }

        // Find key by ID
        var key = jwks.Keys.FirstOrDefault(k => k.KeyId == keyId);
        if (key == null)
        {
            // Refresh cache in case of rotation
            var jwksUrl = $"{issuer}/.well-known/jwks.json";
            var jwksJson = await _httpClient.GetStringAsync(jwksUrl);
            jwks = JsonSerializer.Deserialize<JsonWebKeySet>(jwksJson);
            _keyCache[issuer] = jwks;

            key = jwks.Keys.FirstOrDefault(k => k.KeyId == keyId)
                ?? throw new SecurityException($"Unknown key: {keyId}");
        }

        return JsonWebKeyConverter.ConvertToSecurityKey(key);
    }
}
```

## Step 5: Holder Key Rotation

```csharp
public class HolderKeyManager
{
    private ECDsaSecurityKey _currentKey;
    private readonly List<ECDsaSecurityKey> _previousKeys = new();

    public void RotateHolderKey()
    {
        // Archive current key
        if (_currentKey != null)
        {
            _previousKeys.Add(_currentKey);
        }

        // Generate new key
        var ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        _currentKey = new ECDsaSecurityKey(ecdsa)
        {
            KeyId = $"holder-{Guid.NewGuid():N}"
        };
    }

    public ECDsaSecurityKey GetKeyForCredential(string credentialKeyId)
    {
        // Check if credential uses current key
        if (_currentKey.KeyId == credentialKeyId)
        {
            return _currentKey;
        }

        // Search previous keys
        return _previousKeys.FirstOrDefault(k => k.KeyId == credentialKeyId)
            ?? throw new InvalidOperationException("Key not found for credential");
    }
}
```

## Rotation Strategies

### Time-Based Rotation

```csharp
public class ScheduledKeyRotation
{
    private readonly TimeSpan _rotationInterval = TimeSpan.FromDays(90);
    private DateTimeOffset _lastRotation;

    public bool ShouldRotate()
    {
        return DateTimeOffset.UtcNow - _lastRotation > _rotationInterval;
    }

    public async Task RotateIfNeeded()
    {
        if (ShouldRotate())
        {
            await PerformRotation();
            _lastRotation = DateTimeOffset.UtcNow;
        }
    }
}
```

### Usage-Based Rotation

```csharp
public class UsageBasedRotation
{
    private int _signatureCount = 0;
    private const int MaxSignatures = 1_000_000;

    public bool ShouldRotate()
    {
        return _signatureCount >= MaxSignatures;
    }

    public void RecordSignature()
    {
        Interlocked.Increment(ref _signatureCount);
    }
}
```

## Transition Timeline

```
Day 0:   Generate new key, add to JWKS
Day 1:   Start signing with new key
Day 30:  Remove old key from JWKS
Day 60:  Securely destroy old key
```

## Emergency Rotation

If a key is compromised:

```csharp
public async Task EmergencyRotation(string compromisedKeyId)
{
    // 1. Immediately remove compromised key from JWKS
    await RemoveKeyFromJwks(compromisedKeyId);

    // 2. Generate and publish new key
    var newKey = GenerateNewKey();
    await PublishKey(newKey);

    // 3. Revoke all credentials signed with compromised key
    await RevokeCredentials(compromisedKeyId);

    // 4. Log incident for audit
    _auditLog.RecordKeyCompromise(compromisedKeyId, DateTimeOffset.UtcNow);

    // 5. Notify affected holders
    await NotifyCredentialReissuance(compromisedKeyId);
}
```

## Run the Sample

```bash
cd samples/SdJwt.Net.Samples
dotnet run -- 3.4
```

## Best Practices

1. **Always include key IDs** - Enable verifiers to select correct key
2. **Overlap transition periods** - Keep old keys valid during rotation
3. **Automate rotation** - Reduce human error in key management
4. **Secure key storage** - Use HSM or key vault for production
5. **Audit key usage** - Track signatures per key for compliance

## Key Takeaways

1. Key rotation limits exposure from compromise
2. Transition periods allow credential verification continuity
3. JWKS enables dynamic key discovery
4. Emergency procedures should be documented and tested
