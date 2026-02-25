# Selective Disclosure Mechanics in SD-JWT

## The Core Concept

Traditional JSON Web Tokens (JWTs) suffer from an all-or-nothing privacy problem. If a Verifier requests proof of age, they must receive the entire JWT payload, potentially exposing irrelevant attributes like `home_address` or `blood_type`.

**Selective Disclosure for JWTs (SD-JWT)**, defined in IETF RFC 9901, solves this by separating the *claims* from the *signature*.

Instead of signing the plain text claims directly, an SD-JWT signs cryptographically salted hashes of the claims. The actual data is provided separately in a "disclosure array". When presenting the token to a Verifier, the Holder only sends the disclosures for the claims they wish to reveal. The Verifier hashes the provided disclosures and verifies they match the hashes embedded in the signed token.

## How it works (The `SdJwt.Net` Implementation)

### 1. Issuance (The Issuer)

When generating an SD-JWT using the `SdJwtBuilder`, the library performs the following steps for each *selective* claim:

1. **Generate a cryptographic salt:** A high-entropy random string (e.g., 128-bit) is generated.
2. **Create a disclosure string:** A JSON array containing the salt, the claim name, and the claim value: `["_26bc4LT-ac6q2KI6sBAceg", "email", "john@example.com"]`.
3. **Hash the disclosure:** The array is hashed using a strong algorithm (e.g., SHA-256) and Base64Url encoded.
4. **Embed the hash:** The hash is embedded into the JWT payload under the special `_sd` array.
5. **Sign the JWT:** The JWT is signed as normal.

```csharp
// Example using SdJwt.Net Core
var builder = new SdJwtBuilder()
    .WithClaim("name", "John Doe")                    // Always visible (plain text)
    .WithSelectiveDisclosureClaim("email", "john@example.com")  // Hidden behind a hash
    .WithSelectiveDisclosureClaim("age", 30);         // Hidden behind a hash

var sdJwtString = await builder.CreateSdJwtAsync(signingKey);
```

### 2. The Issued Format (The Wallet)

The resulting `sdJwtString` string returned to the Wallet contains three parts separated by tildes (`~`):

`{JWT_Header}.{JWT_Payload}.{JWT_Signature}~{Disclosure1}~{Disclosure2}~`

The `{JWT_Payload}` might look like this:

```json
{
  "iss": "https://issuer.example.com",
  "name": "John Doe",
  "_sd": [
    "V8x1q...hash_for_email...q2A", 
    "xF9b...hash_for_age...Z8c"
  ]
}
```

Notice the `email` and `age` are nowhere to be found in the actual JWT. The Wallet holds the disclosures separately.

### 3. Presentation (The Holder)

When a Verifier requests the user's email, the Wallet creates a **Presentation**. The Wallet drops the disclosure string for `age` and only sends the disclosure string for `email`.

```csharp
var presentation = SdJwtPresentation.Parse(sdJwtString)
    .RevealClaim("email")      // Include this disclosure
    .HideClaim("age")          // Omit this disclosure
    .AddKeyBinding("audience_url"); // Optional: Prove possession of the credential

var presentationString = presentation.ToString();
// Format: {JWT}~{Disclosure_for_Email}~{Key_Binding_JWT}
```

### 4. Verification (The Verifier)

The `SdJwtVerifier` receives the presentation string and performs the inverse:

1. Validates the Issuer's signature on the core JWT.
2. For every disclosure provided in the string (e.g., the email disclosure), it hashes it.
3. It checks if that calculated hash exists in the `_sd` array of the signed JWT payload.
4. If it matches, the Verifier knows the Issuer originally attested to that exact `[salt, name, value]` combination!

Because the `age` disclosure was not sent, the Verifier sees a hash in the `_sd` array but has no way to reverse-engineer it to determine the user's age.

## Advanced Mechanics

### Key Binding (KB-JWT)

To prevent stolen token replay attacks, SD-JWT supports **Key Binding**. The Issuer binds the token to a public key held in the user's wallet (e.g., via the `cnf` claim).

During presentation, the Wallet creates a new, short-lived JWT (the Key Binding JWT) signed by the Wallet's private key, proving to the Verifier that the presenter is the legitimate owner of the SD-JWT.

### Decoy Hashes

To prevent a Verifier from inferring information based on the *number* of hidden claims (e.g., "This person has 3 medical conditions listed"), `SdJwt.Net` automatically injects random **Decoy Hashes** into the `_sd` array. These hashes do not correspond to any valid disclosure, obfuscating the true size of the credential.
