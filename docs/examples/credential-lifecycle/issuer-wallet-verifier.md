# Issuer - Wallet - Verifier: Full Credential Lifecycle

| Field     | Value                                                                                        |
| --------- | -------------------------------------------------------------------------------------------- |
| Level     | Intermediate                                                                                 |
| Maturity  | Stable                                                                                       |
| Runnable  | Conceptual (paste into a console app)                                                        |
| Packages  | SdJwt.Net, SdJwt.Net.Vc, SdJwt.Net.Oid4Vci, SdJwt.Net.Oid4Vp, SdJwt.Net.PresentationExchange |
| Standards | RFC 9901, SD-JWT VC draft-16, OpenID4VCI 1.0, OpenID4VP 1.0, DIF PEX v2.1.1                  |

This example walks through a complete credential lifecycle:

1. **Issuer** creates an SD-JWT VC with selectively disclosable claims.
2. **Wallet** receives the credential (OID4VCI model) and prepares a presentation with chosen disclosures.
3. **Verifier** builds a presentation request, validates the VP token, and checks constraints.

---

## 1. Issuer: Create an SD-JWT VC

```csharp
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.Issuer;
using SdJwt.Net.Vc.Issuer;
using SdJwt.Net.Vc.Models;

// Issuer key pair (ES256 recommended for production)
var issuerKey = ECDsa.Create(ECCurve.NamedCurves.nistP256);
var issuerSigningKey = new ECDsaSecurityKey(issuerKey);

// Holder key pair (for key binding)
var holderKey = ECDsa.Create(ECCurve.NamedCurves.nistP256);
var holderPublicJwk = JsonWebKeyConverter.ConvertFromECDsaSecurityKey(
    new ECDsaSecurityKey(holderKey));

// Build credential claims
var claims = new Dictionary<string, object>
{
    ["vct"] = "https://credentials.example.com/identity_credential",
    ["iss"] = "https://issuer.example.com",
    ["iat"] = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
    ["given_name"] = "Alice",
    ["family_name"] = "Smith",
    ["birthdate"] = "1990-01-15",
    ["address"] = new Dictionary<string, object>
    {
        ["street_address"] = "123 Main St",
        ["locality"] = "Anytown",
        ["country"] = "US"
    },
    ["cnf"] = new Dictionary<string, object>
    {
        ["jwk"] = JsonSerializer.Deserialize<Dictionary<string, object>>(
            JsonSerializer.Serialize(holderPublicJwk))!
    }
};

// Selectively disclosable fields
var sdPaths = new[]
{
    "given_name",
    "family_name",
    "birthdate",
    "address.street_address",
    "address.locality",
    "address.country"
};

var vcIssuer = new SdJwtVcIssuer();
var issued = vcIssuer.Issue(
    claims,
    issuerSigningKey,
    SecurityAlgorithms.EcdsaSha256,
    new SdIssuanceOptions { SdPaths = sdPaths });

Console.WriteLine("Issued SD-JWT VC:");
Console.WriteLine(issued.SdJwt);
// Output: <issuer-jwt>~<disclosure1>~<disclosure2>~...
```

---

## 2. Wallet: Prepare a Selective Disclosure Presentation

The wallet holds the credential and selects which claims to disclose based on the verifier's request.

```csharp
using SdJwt.Net.Holder;

var holder = new SdJwtHolder();

// Wallet selects only the claims the verifier needs
var presentation = holder.Present(
    issued.SdJwt,
    disclosureSelection: new[] { "given_name", "birthdate" },
    new SdJwtHolderOptions
    {
        HolderKey = new ECDsaSecurityKey(holderKey),
        Algorithm = SecurityAlgorithms.EcdsaSha256,
        Audience = "https://verifier.example.com",
        Nonce = "verifier-nonce-123"
    });

Console.WriteLine("Presentation (with KB-JWT):");
Console.WriteLine(presentation.Presentation);
// Output: <issuer-jwt>~<given_name-disclosure>~<birthdate-disclosure>~<kb-jwt>
```

---

## 3. Verifier: Build Request and Validate

### 3a. Build a Presentation Request with PEX

```csharp
using SdJwt.Net.PresentationExchange.Models;
using SdJwt.Net.Oid4Vp.Verifier;

var presentationDefinition = new PresentationDefinition
{
    Id = "identity-verification",
    InputDescriptors = new[]
    {
        new SdJwt.Net.PresentationExchange.Models.InputDescriptor
        {
            Id = "identity_credential",
            Constraints = new SdJwt.Net.PresentationExchange.Models.Constraints
            {
                Fields = new[]
                {
                    new SdJwt.Net.PresentationExchange.Models.Field
                    {
                        Path = new[] { "$.vct" },
                        Filter = new FieldFilter
                        {
                            Type = "string",
                            Const = "https://credentials.example.com/identity_credential"
                        }
                    },
                    new SdJwt.Net.PresentationExchange.Models.Field
                    {
                        Path = new[] { "$.given_name" }
                    },
                    new SdJwt.Net.PresentationExchange.Models.Field
                    {
                        Path = new[] { "$.birthdate" }
                    }
                }
            }
        }
    }
};
```

### 3b. Validate the VP Token

```csharp
using SdJwt.Net.Oid4Vp.Verifier;

var vpValidator = new VpTokenValidator();

var result = await vpValidator.ValidateAsync(
    presentation.Presentation,
    new VpTokenValidationOptions
    {
        Audience = "https://verifier.example.com",
        Nonce = "verifier-nonce-123",
        IssuerSigningKey = new ECDsaSecurityKey(
            ECDsa.Create(issuerKey.ExportParameters(false))),
        PresentationDefinition = presentationDefinition
    });

Console.WriteLine($"Valid: {result.IsValid}");
// Output: Valid: True

if (result.IsValid)
{
    Console.WriteLine("Disclosed claims verified successfully.");
    // Access verified claims from the result
}
```

---

## Expected Outcomes

| Step                       | Result                                           |
| -------------------------- | ------------------------------------------------ |
| Issue SD-JWT VC            | Signed JWT with 6 selectively disclosable claims |
| Present with 2 disclosures | Only `given_name` and `birthdate` revealed       |
| Validate VP token          | Signature, KB-JWT, nonce, audience all pass      |
| Undisclosed claims         | Not visible to verifier                          |

---

## Related

- [Status List Lifecycle](status-list-lifecycle.md) -- add revocation to this flow
- [Federated Trust Verification](federated-trust-verification.md) -- resolve issuer trust dynamically
- [SD-JWT Concepts](../../concepts/) -- RFC 9901 deep dive
- [Getting Started](../../getting-started/) -- installation and setup
