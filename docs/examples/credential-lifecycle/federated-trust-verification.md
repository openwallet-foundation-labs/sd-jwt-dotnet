# Federated Trust Verification

| Field     | Value                                                   |
| --------- | ------------------------------------------------------- |
| Level     | Advanced                                                |
| Maturity  | Stable                                                  |
| Runnable  | Conceptual (paste into a console app)                   |
| Packages  | SdJwt.Net.OidFederation, SdJwt.Net.Oid4Vp, SdJwt.Net.Vc |
| Standards | OpenID Federation 1.0                                   |

This example demonstrates how a verifier resolves trust for an unknown issuer using OpenID Federation:

1. Receive a credential from an **unknown issuer**.
2. **Resolve the trust chain** from issuer up to a trust anchor.
3. **Apply metadata policy** from the federation.
4. **Verify** the credential using the trusted keys.

---

## 1. Scenario: Unknown Issuer

A verifier receives a credential issued by `https://issuer.university.example.com`. The verifier does not have a pre-configured trust relationship with this issuer but trusts the federation anchor `https://federation.example.com`.

```csharp
using SdJwt.Net.OidFederation.Logic;
using SdJwt.Net.OidFederation.Models;

// The issuer URI from the credential's `iss` claim
string issuerUri = "https://issuer.university.example.com";

// Trust anchor the verifier is configured to trust
string trustAnchor = "https://federation.example.com";
```

---

## 2. Resolve Trust Chain

The `TrustChainResolver` walks the federation hierarchy:
issuer -> intermediate (optional) -> trust anchor.

```csharp
var resolver = new TrustChainResolver(new TrustChainResolverOptions
{
    TrustAnchors = new[] { trustAnchor },
    MaxChainDepth = 5,
    HttpClient = new HttpClient()
});

TrustChainResult chainResult = await resolver.ResolveAsync(issuerUri);

if (!chainResult.IsValid)
{
    Console.WriteLine($"Trust chain resolution failed: {chainResult.Error}");
    return;
}

Console.WriteLine($"Trust chain resolved: {chainResult.Chain.Count} entities");
Console.WriteLine($"  Leaf: {chainResult.Chain[0].Subject}");
Console.WriteLine($"  Anchor: {chainResult.Chain[^1].Subject}");
// Output:
// Trust chain resolved: 3 entities
//   Leaf: https://issuer.university.example.com
//   Anchor: https://federation.example.com
```

---

## 3. Apply Metadata Policy

Federation intermediaries can enforce constraints on leaf entity metadata (e.g., require specific algorithms, restrict credential types).

```csharp
// The resolved chain includes applied metadata policy
EntityConfiguration resolvedMetadata = chainResult.ResolvedEntityConfiguration;

// Check issuer metadata after policy application
Console.WriteLine($"Issuer metadata resolved with federation constraints.");
Console.WriteLine($"  Subject: {resolvedMetadata.Subject}");

// Extract the issuer's signing keys from the resolved entity configuration
var issuerJwks = resolvedMetadata.Jwks;
Console.WriteLine($"  Trusted keys: {issuerJwks.Keys.Count}");
```

---

## 4. Verify Credential with Trusted Keys

Use the keys from the resolved trust chain to verify the credential.

```csharp
using SdJwt.Net.Vc.Verifier;

var vcVerifier = new SdJwtVcVerifier();

// Verify using the federation-resolved issuer key
var verificationResult = vcVerifier.Verify(
    credentialPresentation,
    new SdJwtVcVerificationPolicy
    {
        IssuerSigningKeys = issuerJwks.Keys,
        ValidIssuer = issuerUri
    });

Console.WriteLine($"Credential valid: {verificationResult.IsValid}");
// Output: Credential valid: True
```

---

## Trust Chain Structure

```
Trust Anchor (federation.example.com)
  |-- Entity Statement about intermediate
  |
  Intermediate (university.example.com)
    |-- Entity Statement about issuer
    |
    Leaf / Issuer (issuer.university.example.com)
      |-- Entity Configuration (self-signed)
```

Each entity statement can include `metadata_policy` that constrains what the subordinate can claim.

---

## Expected Outcomes

| Step                     | Result                                    |
| ------------------------ | ----------------------------------------- |
| Unknown issuer           | No pre-configured trust                   |
| Resolve trust chain      | Chain validated up to trusted anchor      |
| Apply metadata policy    | Issuer metadata constrained by federation |
| Verify with trusted keys | Credential accepted                       |
| Chain broken / expired   | Resolution fails, credential rejected     |

---

## Related

- [Issuer - Wallet - Verifier](../credential-lifecycle/issuer-wallet-verifier.md) -- basic credential flow
- [OpenID Federation spec](https://openid.net/specs/openid-federation-1_0.html)
