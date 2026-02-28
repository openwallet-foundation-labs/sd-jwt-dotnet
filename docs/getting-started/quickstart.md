# Quickstart: 15 Minutes to SD-JWT

This tutorial will get you up and running with the `SdJwt.Net` core library in under 15 minutes. We will act as all three parties (Issuer, Holder/Wallet, and Verifier) within a single console application.

## 1. Create a New Project

Open your terminal and create a new .NET 8, 9, or 10 Console Application:

```bash
dotnet new console -n SdJwtQuickstart
cd SdJwtQuickstart
```

Install the core foundation package:

```bash
dotnet add package SdJwt.Net
```

## 2. Generate Cryptographic Keys

SD-JWTs require public key cryptography (like ECDSA or RSA) to sign the tokens. In a real application, the Issuer and the Verifier would have different keys, and the Wallet would have its own key for Key Binding.

For this tutorial, open `Program.cs` and replace the contents with the following:

```csharp
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.Holder;
using SdJwt.Net.Issuer;
using SdJwt.Net.Verifier;

Console.WriteLine("--- SD-JWT Quickstart ---");

// 1. Generate keys for our actors
// The Issuer signs the credential
using var issuerAlgorithm = ECDsa.Create(ECCurve.NamedCurves.nistP256);
var issuerKey = new ECDsaSecurityKey(issuerAlgorithm) { KeyId = "issuer-key-1" };

// The Wallet signs a "Proof of Possession" during presentation
using var walletAlgorithm = ECDsa.Create(ECCurve.NamedCurves.nistP256);
var walletKey = new ECDsaSecurityKey(walletAlgorithm) { KeyId = "wallet-key-1" };
```

## 3. The Issuer: Creating the SD-JWT

The Issuer creates the credential with `SdIssuer`. We add claims and define which fields are selectively disclosable.

Append this to `Program.cs`:

```csharp
Console.WriteLine("\n[1] Issuer is building the SD-JWT...");

var issuer = new SdIssuer(issuerKey, SecurityAlgorithms.EcdsaSha256);
var walletJwk = JsonWebKeyConverter.ConvertFromSecurityKey(walletKey);

var claims = new JwtPayload
{
    ["iss"] = "https://issuer.example.com",
    ["sub"] = "user123",
    ["name"] = "Jane Doe",
    ["nationality"] = "US",
    ["email"] = "jane@example.com",
    ["age"] = 28,
    ["address"] = "123 Main St"
};

var options = new SdIssuanceOptions
{
    DisclosureStructure = new
    {
        email = true,
        age = true,
        address = true
    }
};

var issuance = issuer.Issue(claims, options, walletJwk);
var sdJwtString = issuance.Issuance;

Console.WriteLine("\nRaw SD-JWT String (sent to wallet):");
Console.WriteLine(sdJwtString);
```

Run the application (`dotnet run`). You will see a massive string separated by tildes (`~`).

- The first part is the signed JWT payload. _Notice that the email, age, and address are not in the payload!_
- The subsequent parts are the **Disclosures** (the salt + claim name + claim value).

## 4. The Wallet: Creating a Presentation

The Wallet has received the `sdJwtString` and stored it securely.

Now, a Verifier (a website) asks the user for their **email** and **age**, but explicitly _not_ their address or nationality.

Append this to `Program.cs`:

```csharp
Console.WriteLine("\n[2] Wallet is creating a Presentation...");

var holder = new SdJwtHolder(sdJwtString);

var keyBindingPayload = new JwtPayload
{
    [JwtRegisteredClaimNames.Aud] = "https://verifier.example.com",
    [JwtRegisteredClaimNames.Iat] = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
    ["nonce"] = "random-nonce-123"
};

// Reveal only email + age; keep address hidden
var presentationString = holder.CreatePresentation(
    disclosure => disclosure.ClaimName == "email" || disclosure.ClaimName == "age",
    keyBindingPayload,
    walletKey,
    SecurityAlgorithms.EcdsaSha256);

Console.WriteLine("\nPresentation String (sent to verifier):");
Console.WriteLine(presentationString);
```

If you run the app again, you'll see the presentation string is slightly shorter than the original string—that's because the Wallet intentionally dropped the disclosure containing the address!

## 5. The Verifier: Validating the Presentation

The Verifier receives the `presentationString` over an API endpoint. They must ensure it's authentic, untampered, and actually belongs to the user presenting it.

Append this to `Program.cs`:

```csharp
Console.WriteLine("\n[3] Verifier is checking the Presentation...");

var verifier = new SdVerifier(jwt =>
{
    var issuerId = jwt.Payload.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Iss)?.Value;
    return Task.FromResult<SecurityKey>(issuerId == "https://issuer.example.com"
        ? issuerKey
        : throw new InvalidOperationException("Unknown issuer"));
});

try
{
    var validationParams = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = "https://issuer.example.com",
        ValidateAudience = false,
        ValidateLifetime = false
    };

    var kbParams = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = true,
        ValidAudience = "https://verifier.example.com",
        ValidateLifetime = false,
        IssuerSigningKey = walletKey
    };

    var verificationResult = await verifier.VerifyAsync(
        presentationString,
        validationParams,
        kbParams,
        "random-nonce-123");

    Console.WriteLine($"\nIs Valid? {verificationResult.IsValid}");
    Console.WriteLine($"Key Binding Verified? {verificationResult.KeyBindingVerified}");

    Console.WriteLine("\nRevealed Claims (JWT payload view):");
    foreach (var claim in verificationResult.ClaimsPrincipal.Claims)
    {
        Console.WriteLine($"- {claim.Type}: {claim.Value}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"\nVerification Failed: {ex.Message}");
}
```

## Summary

Run the application one last time. You have successfully:

1. Acted as an **Issuer** to cryptographically salt and hide sensitive attributes.
2. Acted as a **Wallet** to selectively reveal only a subset of those attributes while proving possession.
3. Acted as a **Verifier** to cryptographically prove the data was authentic and untampered, _without ever seeing the hidden attributes_.

## Next Steps

This tutorial demonstrated the core cryptographic engine. In a real-world scenario, you don't pass strings around via console variables—you use HTTP protocols!

- To learn how to issue credentials over OAuth 2.0 (OpenID4VCI), read [How to Issue Verifiable Credentials](../guides/issuing-credentials.md).
- To learn how to request and verify presentations over HTTP (OpenID4VP), read [How to Verify Presentations](../guides/verifying-presentations.md).
