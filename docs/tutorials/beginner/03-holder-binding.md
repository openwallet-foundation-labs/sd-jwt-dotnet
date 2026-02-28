# Tutorial: Holder Binding

Prove cryptographic ownership of a credential with Key Binding JWT.

**Time:** 10 minutes  
**Level:** Beginner  
**Sample:** `samples/SdJwt.Net.Samples/01-Beginner/03-HolderBinding.cs`

## What You Will Learn

- Why holder binding matters for security
- How to embed holder keys in credentials
- How to create Key Binding JWTs (KB-JWT)

## The Problem

Without holder binding, anyone who obtains an SD-JWT can present it. Holder binding ensures only the legitimate holder can use the credential.

## How It Works

1. **Issuance**: Holder's public key is embedded in the credential (`cnf` claim)
2. **Presentation**: Holder signs a KB-JWT with their private key
3. **Verification**: Verifier checks both the SD-JWT and KB-JWT signatures

## Step 1: Generate Holder Keys

```csharp
using var holderEcdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
var holderPrivateKey = new ECDsaSecurityKey(holderEcdsa) { KeyId = "holder-key" };
var holderJwk = JsonWebKeyConverter.ConvertFromSecurityKey(holderPrivateKey);
```

## Step 2: Issue with Holder Binding

Pass the holder's public key during issuance:

```csharp
var result = issuer.Issue(claims, options, holderPublicKey: holderJwk);
```

The SD-JWT now contains a `cnf` (confirmation) claim with the holder's public key.

## Step 3: Create Bound Presentation

When presenting, the holder creates a Key Binding JWT:

```csharp
var presentation = holder.CreatePresentation(
    disclosure => disclosure.ClaimName == "given_name",
    kbJwtPayload: new JwtPayload
    {
        ["aud"] = "https://verifier.example.com",
        ["iat"] = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
        ["nonce"] = "verifier-provided-nonce-12345"
    },
    kbJwtSigningKey: holderPrivateKey,
    kbJwtSigningAlgorithm: SecurityAlgorithms.EcdsaSha256
);
```

## KB-JWT Contents

| Claim | Purpose |
|-------|---------|
| `aud` | Intended verifier (prevents replay to others) |
| `iat` | Issuance time (freshness) |
| `nonce` | Verifier-provided value (prevents replay) |
| `sd_hash` | Hash of the presentation (auto-added) |

## Step 4: Verify with Key Binding

```csharp
var sdJwtValidation = new TokenValidationParameters
{
    ValidateIssuer = true,
    ValidIssuer = "https://issuer.example.com",
    ValidateAudience = false,
    ValidateLifetime = true
};

var kbJwtValidation = new TokenValidationParameters
{
    ValidateIssuer = false,
    ValidateAudience = true,
    ValidAudience = "https://verifier.example.com",
    ValidateLifetime = false
};

var result = await verifier.VerifyAsync(
    presentation,
    sdJwtValidation,
    kbJwtValidation,
    expectedKbJwtNonce: "verifier-provided-nonce-12345"
);

Console.WriteLine($"Key binding verified: {result.KeyBindingVerified}");
```

## Security Benefits

### Prevents Credential Theft

Even if an attacker steals the SD-JWT issuance string, they cannot create valid presentations without the holder's private key.

### Prevents Replay Attacks

The nonce and audience claims ensure presentations cannot be replayed:

- Different verifiers have different audience values
- Each request uses a fresh nonce

### Proves Liveness

The KB-JWT proves the holder actively created this presentation now, not that it was pre-generated.

## Complete Flow

```csharp
// 1. Holder generates keys
using var holderEcdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
var holderKey = new ECDsaSecurityKey(holderEcdsa);
var holderJwk = JsonWebKeyConverter.ConvertFromSecurityKey(holderKey);

// 2. Issuer creates bound credential
var result = issuer.Issue(claims, options, holderJwk);

// 3. Holder creates bound presentation
var holder = new SdJwtHolder(result.Issuance);
var presentation = holder.CreatePresentation(
    d => true,  // Disclose all
    kbJwtPayload: new JwtPayload
    {
        ["aud"] = "https://verifier.example",
        ["nonce"] = "abc123"
    },
    kbJwtSigningKey: holderKey,
    kbJwtSigningAlgorithm: SecurityAlgorithms.EcdsaSha256
);

// 4. Verifier validates both signatures
var verified = await verifier.VerifyAsync(
    presentation, sdJwtParams, kbJwtParams, "abc123"
);
```

## Run the Sample

```bash
cd samples/SdJwt.Net.Samples
dotnet run -- 1.3
```

## Next Steps

- [Verification Flow](04-verification-flow.md) - Complete end-to-end implementation
- [HAIP Compliance](../advanced/02-haip-compliance.md) - High-assurance requirements

## Key Takeaways

1. Holder binding prevents unauthorized credential use
2. The `cnf` claim embeds the holder's public key
3. KB-JWT proves the presenter owns the credential
4. Nonces prevent replay attacks
