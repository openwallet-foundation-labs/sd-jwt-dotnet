# Quickstart: 15 Minutes to SD-JWT

This tutorial will get you up and running with the `SdJwt.Net` core library in under 15 minutes. We will act as all three parties (Issuer, Holder/Wallet, and Verifier) within a single console application.

## 1. Create a New Project

Open your terminal and create a new .NET 8 or 9 Console Application:

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
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using SdJwt.Net;
using System.Text.Json;

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

The Issuer creates the credential using the `SdJwtBuilder`. We will add some plain-text claims that anyone can see, and some `SelectiveDisclosure` claims that are hidden behind cryptographic hashes.

Append this to `Program.cs`:

```csharp
Console.WriteLine("\n[1] Issuer is building the SD-JWT...");

var builder = new SdJwtBuilder()
    .WithIssuer("https://issuer.example.com")
    .WithSubject("user123")
    
    // Always visible claims
    .WithClaim("name", "Jane Doe")
    .WithClaim("nationality", "US")
    
    // Hidden claims (require user consent to reveal)
    .WithSelectiveDisclosureClaim("email", "jane@example.com")
    .WithSelectiveDisclosureClaim("age", 28)
    .WithSelectiveDisclosureClaim("address", "123 Main St")
    
    // Bind the credential to the user's wallet key!
    .WithConfirmationClaim(walletKey);

// The Issuer signs the token using their private key
var sdJwtString = await builder.CreateSdJwtAsync(issuerKey);

Console.WriteLine("\nRaw SD-JWT String (sent to wallet):");
Console.WriteLine(sdJwtString);
```

Run the application (`dotnet run`). You will see a massive string separated by tildes (`~`).

* The first part is the signed JWT payload. *Notice that the email, age, and address are not in the payload!*
* The subsequent parts are the **Disclosures** (the salt + claim name + claim value).

## 4. The Wallet: Creating a Presentation

The Wallet has received the `sdJwtString` and stored it securely.

Now, a Verifier (a website) asks the user for their **email** and **age**, but explicitly *not* their address or nationality.

Append this to `Program.cs`:

```csharp
Console.WriteLine("\n[2] Wallet is creating a Presentation...");

// The wallet parses the massive string it received from the Issuer
var presentationBuilder = SdJwtPresentation.Parse(sdJwtString)
    
    // The user explicitly consents to reveal these two claims
    .RevealClaim("email")
    .RevealClaim("age")
    
    // The wallet drops the "address" disclosure
    .HideClaim("address")
    
    // Provide Proof of Possession! 
    // The wallet creates a temporary "Key Binding JWT" signed with its own private key,
    // proving the credential wasn't stolen.
    .AddKeyBinding(new KeyBindingOptions
    {
        Audience = "https://verifier.example.com", // The website asking for data
        Nonce = "random-nonce-123",                // To prevent replay attacks
        SigningKey = walletKey                     // The wallet's private key
    });

var presentationString = presentationBuilder.ToString();

Console.WriteLine("\nPresentation String (sent to verifier):");
Console.WriteLine(presentationString);
```

If you run the app again, you'll see the presentation string is slightly shorter than the original string—that's because the Wallet intentionally dropped the disclosure containing the address!

## 5. The Verifier: Validating the Presentation

The Verifier receives the `presentationString` over an API endpoint. They must ensure it's authentic, untampered, and actually belongs to the user presenting it.

Append this to `Program.cs`:

```csharp
Console.WriteLine("\n[3] Verifier is checking the Presentation...");

var verifier = new SdJwtVerifier();

try
{
    // The verifier must know the Issuer's Public Key to verify the signature
    var verificationResult = await verifier.VerifyAsync(
        presentationToken: presentationString,
        issuerPublicKey: issuerKey,
        options: new VerificationOptions
        {
            ExpectedAudience = "https://verifier.example.com",
            ExpectedNonce = "random-nonce-123"
        });

    Console.WriteLine($"\nIs Valid? {verificationResult.IsValid}");
    
    Console.WriteLine("\nRevealed Claims:");
    foreach (var claim in verificationResult.RevealedClaims)
    {
        Console.WriteLine($"- {claim.Key}: {claim.Value}");
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
3. Acted as a **Verifier** to cryptographically prove the data was authentic and untampered, *without ever seeing the hidden attributes*.

## Next Steps

This tutorial demonstrated the core cryptographic engine. In a real-world scenario, you don't pass strings around via console variables—you use HTTP protocols!

* To learn how to issue credentials over OAuth 2.0 (OpenID4VCI), read [How to Issue Verifiable Credentials](../guides/issuing-credentials.md).
* To learn how to request and verify presentations over HTTP (OpenID4VP), read [How to Verify Presentations](../guides/verifying-presentations.md).
