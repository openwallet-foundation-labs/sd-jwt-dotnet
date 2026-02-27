# Core SD-JWT Examples

This directory contains fundamental examples demonstrating the core concepts of Selective Disclosure JSON Web Tokens (SD-JWT) as defined in [RFC 9901](https://www.rfc-editor.org/rfc/rfc9901.html).

## Examples in This Directory

### 1. CoreSdJwtExample.cs - RFC 9901 Basics

**Learning Objectives**:

- Understand SD-JWT structure and lifecycle
- Create credentials with selective disclosure
- Generate presentations with key binding
- Verify SD-JWT presentations cryptographically

**Key Concepts Demonstrated**:

- **Issuer**: Creating SD-JWTs with selectively disclosable claims
- **Holder**: Selecting which claims to disclose
- **Verifier**: Validating signatures and disclosed claims
- **Key Binding**: Proving possession of the credential

**Code Highlights**:

```csharp
// Issuer creates SD-JWT with selective disclosure
var output = issuer.Issue(claims, options, holderJwk);

// Holder creates presentation with only required claims
var presentation = holder.CreatePresentation(selector, kbPayload, holderKey, SecurityAlgorithms.EcdsaSha256);

// Verifier validates the presentation
var result = await verifier.VerifyAsync(presentation, validationParams, kbParams, expectedNonce);
```

### 2. JsonSerializationExample.cs - Alternative Serialization Formats

**Learning Objectives**:

- Work with JWS JSON Serialization (RFC 7515)
- Convert between compact and JSON formats
- Handle both Flattened and General JSON serialization

**Key Concepts Demonstrated**:

- **Compact Serialization**: Traditional JWT format (header.payload.signature)
- **Flattened JSON**: Object-based format with single signature
- **General JSON**: Object-based format supporting multiple signatures
- **Round-trip Conversion**: Lossless format transformations

**Code Highlights**:

```csharp
// Create SD-JWT in Flattened JSON format
var flattenedJson = issuer.IssueAsJsonSerialization(claims, disclosableClaimNames);

// Convert to General JSON format
var generalJson = JsonSerializationConverter.ToGeneralSerialization(flattenedJson);

// Convert to compact format
var compact = JsonSerializationConverter.ToCompact(generalJson);
```

**When to Use Each Format**:

- **Compact**: HTTP headers, QR codes, space-constrained scenarios
- **Flattened JSON**: Single issuer, better readability
- **General JSON**: Multiple signers, advanced scenarios

### 3. SecurityFeaturesExample.cs - Production Security Patterns

**Learning Objectives**:

- Implement cryptographic best practices
- Prevent common security vulnerabilities
- Handle edge cases and attack scenarios

**Key Concepts Demonstrated**:

- **Signature Validation**: Cryptographic verification of issuer authenticity
- **Nonce Validation**: Replay attack prevention
- **Algorithm Security**: Approved vs. blocked crypto algorithms
- **Disclosure Validation**: Preventing duplicate or tampered disclosures
- **Error Handling**: Secure failure modes

**Security Patterns Covered**:

```csharp
// 1. Strong cryptographic algorithms only
options.AllowedAlgorithms = new[] { "ES256", "ES384", "ES512" };

// 2. Nonce-based replay protection
var nonce = GenerateSecureNonce();
var presentation = holder.CreatePresentation(selector, kbPayload, holderKey, SecurityAlgorithms.EcdsaSha256);

// 3. Comprehensive validation
var result = await verifier.VerifyAsync(presentation, options);

// 4. Secure error handling
if (!result.IsValid)
{
    logger.LogWarning("Validation failed: {Reason}", result.ErrorMessage);
    throw new SecurityTokenException("Invalid SD-JWT presentation");
}
```

**Security Checklist**:

- Use ES256 or stronger algorithms (ES384, ES512)
- Validate all signatures (issuer SD-JWT + holder key binding)
- Check nonces to prevent replay attacks
- Verify disclosure integrity (no duplicates, valid hashes)
- Validate JWT claims (exp, nbf, iat when present)
- Use secure random number generation
- Implement proper error handling without leaking information

## Learning Path

### Beginner (30 minutes)

1. **Start with CoreSdJwtExample.cs**
   - Run the example and observe the output
   - Modify which claims are disclosed
   - Experiment with adding/removing disclosures

### Intermediate (60 minutes)

2. **Explore JsonSerializationExample.cs**

   - Convert between different formats
   - Understand when to use each format
   - Practice round-trip conversions

3. **Study SecurityFeaturesExample.cs**
   - Review security best practices
   - Understand attack prevention mechanisms
   - Implement validation error handling

### Advanced (90+ minutes)

4. **Combine concepts from all three examples**
   - Create a mini-application using SD-JWT
   - Implement all three roles (Issuer, Holder, Verifier)
   - Apply security patterns throughout

## Running the Examples

### From Interactive Menu

```bash
cd samples/SdJwt.Net.Samples
dotnet run
# Select Core examples (options 1-3)
```

### Programmatically

```csharp
// In your own project
dotnet add package SdJwt.Net

// Use the examples as reference:
using SdJwt.Net.Issuer;
using SdJwt.Net.Holder;
using SdJwt.Net.Verifier;
```

## Key Takeaways

### From CoreSdJwtExample.cs

- SD-JWT enables privacy-preserving credentials
- Three distinct roles with clear responsibilities
- Key binding proves credential possession

### From JsonSerializationExample.cs

- Multiple serialization formats serve different needs
- Compact format is most common but not always optimal
- JSON serialization supports advanced scenarios

### From SecurityFeaturesExample.cs

- Security must be built-in, not added later
- Cryptographic validation is non-negotiable
- Proper error handling prevents information leakage

## Common Patterns

### Creating an SD-JWT

```csharp
var issuer = new SdIssuer(signingCredentials, hashAlgorithm);
var output = issuer.Issue(userClaims, issuanceOptions, holderJwk);
var sdJwt = output.Issuance;
```

### Creating a Presentation

```csharp
var holder = new SdJwtHolder(sdJwt);
var presentation = holder.CreatePresentation(
    credentialsToDisclose: new[] { "email" }, // Only email, not phone/address
    holderKey: holderSigningKey,
    nonce: verifierNonce,
    audience: verifierIdentifier
);
```

### Verifying a Presentation

```csharp
var verifier = new SdVerifier(jwt => Task.FromResult<SecurityKey>(issuerPublicKey));
var result = await verifier.VerifyAsync(
    presentation: sdJwtPresentation,
    expectedNonce: nonce,
    expectedAudience: myIdentifier
);

if (result.IsValid)
{
    var disclosedClaims = result.Claims; // Verified claims
}
```

## Next Steps

After mastering these core concepts:

- Explore [Standards examples](../Standards/) for protocol implementations
- Review [Integration examples](../Integration/) for multi-package workflows
- Try [Real-World scenarios](../RealWorld/) for production patterns

## Related Documentation

- **[RFC 9901](https://www.rfc-editor.org/rfc/rfc9901.html)** - SD-JWT specification
- **[Developer Guide](../../../docs/README.md)** - Comprehensive ecosystem guide
- **[Package Documentation](../../../src/SdJwt.Net/README.md)** - Core package API reference
- **[Security Guidelines](../../../SECURITY.md)** - Security best practices

---

**Last Updated**: February 11, 2026
