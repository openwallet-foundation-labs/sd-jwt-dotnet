# JWS JSON Serialization Support

This document describes the JWS JSON Serialization support for SD-JWT as defined in RFC 9901 Section 8.

## Overview

The SD-JWT.NET library now supports JWS JSON Serialization as an alternative to the compact serialization format. This optional feature enables:

- **Flattened JSON Serialization**: A single signature with unprotected header containing disclosures
- **General JSON Serialization**: Multiple signatures with disclosures in the first signature only
- **Interoperability**: Compatibility with systems requiring JSON-based JWT formats
- **Enterprise Features**: Support for multi-signature scenarios

## Key Features

### 1. Format Support

- ? **JWS Flattened JSON Serialization** (RFC 7515)
- ? **JWS General JSON Serialization** (RFC 7515)  
- ? **Bidirectional conversion** between compact and JSON formats
- ? **RFC 9901 compliance** with proper unprotected header handling

### 2. Security Features

- ? **sd_hash calculation** for Key Binding JWTs using JSON serialization
- ? **Proper digest computation** over equivalent compact representation
- ? **Multi-signature validation** with disclosure restrictions
- ? **Format validation** and error handling

## Usage Examples

### Basic Conversion

```csharp
using SdJwt.Net.Serialization;

// Convert compact SD-JWT to Flattened JSON Serialization
string compactSdJwt = "eyJ...~WyJ...~";
var flattenedJson = SdJwtJsonSerializer.ToFlattenedJsonSerialization(compactSdJwt);

// Convert back to compact format
string compactResult = SdJwtJsonSerializer.FromFlattenedJsonSerialization(flattenedJson);
```

### Direct Issuance as JSON

```csharp
using SdJwt.Net.Issuer;

var issuer = new SdIssuer(signingKey, SecurityAlgorithms.EcdsaSha256);

// Issue directly as Flattened JSON Serialization
var flattenedSdJwt = issuer.IssueAsJsonSerialization(payload, options, holderKey);

// Issue as General JSON Serialization  
var generalSdJwt = issuer.IssueAsGeneralJsonSerialization(payload, options, holderKey);
```

### Verification

```csharp
using SdJwt.Net.Verifier;

var verifier = new SdVerifier(keyResolver);
var validationParams = new TokenValidationParameters { /* ... */ };

// Verify JSON serialized SD-JWT
var result = await verifier.VerifyJsonSerializationAsync(jsonSdJwt, validationParams);
```

### Multi-Signature Support

```csharp
// Create additional signatures for General JSON Serialization
var additionalSignatures = new[]
{
    new SdJwtSignature
    {
        Protected = "eyJ...",  // Different signing key header
        Signature = "abc...",  // Signature with different key
        Header = new SdJwtUnprotectedHeader()  // No disclosures/kb_jwt
    }
};

var generalSdJwt = issuer.IssueAsGeneralJsonSerialization(
    payload, options, holderKey, additionalSignatures);
```

## JSON Format Structure

### Flattened JSON Serialization

```json
{
  "protected": "eyJhbGciOiJFUzI1NiIsInR5cCI6InNkK2p3dCJ9",
  "payload": "eyJfc2QiOlsiZGlnZXN0MSIsImRpZ2VzdDIiXSwiaXNzIjoia....",
  "signature": "signature_value",
  "header": {
    "disclosures": [
      "WyJzYWx0MSIsImNsYWltMSIsInZhbHVlMSJd",
      "WyJzYWx0MiIsImNsYWltMiIsInZhbHVlMiJd"
    ],
    "kb_jwt": "eyJhbGciOiJFUzI1NiIsInR5cCI6ImtiK2p3dCJ9.eyJ..."
  }
}
```

### General JSON Serialization

```json
{
  "payload": "eyJfc2QiOlsiZGlnZXN0MSIsImRpZ2VzdDIiXSwiaXNzIjoia....",
  "signatures": [
    {
      "protected": "eyJhbGciOiJFUzI1NiIsInR5cCI6InNkK2p3dCJ9",
      "signature": "signature1",
      "header": {
        "disclosures": [...],
        "kb_jwt": "..."
      }
    },
    {
      "protected": "eyJhbGciOiJSUzI1NiIsImtpZCI6InNpZzIifQ",
      "signature": "signature2", 
      "header": {
        "kid": "additional-key"
      }
    }
  ]
}
```

## API Reference

### SdJwtJsonSerializer

#### Methods

- `ToFlattenedJsonSerialization(string sdJwtCompact)` - Convert compact to flattened JSON
- `ToGeneralJsonSerialization(string sdJwtCompact, SdJwtSignature[]? additionalSignatures)` - Convert to general JSON
- `FromFlattenedJsonSerialization(SdJwtJsonSerialization jsonSerialization)` - Convert from flattened JSON
- `FromGeneralJsonSerialization(SdJwtGeneralJsonSerialization generalSerialization)` - Convert from general JSON
- `CalculateSdHashForJsonSerialization(SdJwtJsonSerialization jsonSerialization, string hashAlgorithm)` - Calculate sd_hash
- `IsValidJsonSerialization(string json)` - Validate JSON format

### SdIssuer Extensions

#### Methods

- `IssueAsJsonSerialization(JwtPayload claims, SdIssuanceOptions options, JsonWebKey? holderPublicKey)` - Issue as flattened JSON
- `IssueAsGeneralJsonSerialization(...)` - Issue as general JSON with optional additional signatures

### SdVerifier Extensions

#### Methods  

- `VerifyJsonSerializationAsync(string jsonSerialization, TokenValidationParameters validationParameters, TokenValidationParameters? kbJwtValidationParameters)` - Verify JSON serialized SD-JWT

## RFC 9901 Compliance

### Implemented Requirements

- ? **Section 8.1**: New unprotected header parameters (`disclosures`, `kb_jwt`)
- ? **Section 8.2**: Flattened JSON Serialization format
- ? **Section 8.3**: General JSON Serialization format  
- ? **Section 8.4**: Verification of JWS JSON Serialized SD-JWT
- ? **Section 11.2**: Media type registration support

### Key Compliance Points

1. **Disclosures Placement**: Only in first signature for General JSON Serialization
2. **KB-JWT Placement**: Only in first signature for General JSON Serialization  
3. **sd_hash Calculation**: Over equivalent compact representation
4. **Unprotected Header**: Not covered by digest (as per RFC)
5. **Multi-signature**: Additional signatures MUST NOT contain disclosures/kb_jwt

## Media Types

The following media types are supported as per RFC 9901:

- `application/sd-jwt` - Compact serialization
- `application/sd-jwt+json` - JSON serialization  
- `application/kb+jwt` - Key Binding JWT

## Error Handling

The implementation provides comprehensive error handling:

```csharp
try 
{
    var result = SdJwtJsonSerializer.ToFlattenedJsonSerialization(invalidSdJwt);
}
catch (ArgumentException ex)
{
    // Invalid SD-JWT format
}
catch (SecurityTokenException ex) 
{
    // Security validation failed
}
```

## Interoperability

This implementation is designed for compatibility with:

- ? **Reference Implementation**: [authlete/sd-jwt](https://github.com/authlete/sd-jwt)
- ? **RFC 9901 Examples**: All appendix examples supported
- ? **Enterprise Systems**: Multi-signature and JSON-based workflows
- ? **Standard Libraries**: System.Text.Json, Microsoft.IdentityModel.Tokens

## Performance Considerations

- **Memory**: JSON serialization uses more memory than compact format
- **CPU**: Additional parsing overhead for JSON formats
- **Network**: Larger payload size compared to compact format
- **Recommendation**: Use compact format for high-throughput scenarios, JSON for interoperability

## Migration Guide

### From Compact Only

```csharp
// Before - Compact only
var compactSdJwt = issuer.Issue(payload, options);

// After - JSON support  
var jsonSdJwt = issuer.IssueAsJsonSerialization(payload, options);
var compact = SdJwtJsonSerializer.FromFlattenedJsonSerialization(jsonSdJwt);
```

### Verification Updates

```csharp
// New JSON verification method
var result = await verifier.VerifyJsonSerializationAsync(jsonString, validationParams);

// Existing compact verification still works
var result = await verifier.VerifyAsync(compactString, validationParams);  
```

## Testing

Comprehensive test coverage includes:

- ? Round-trip conversion testing
- ? RFC 9901 example validation  
- ? Error condition handling
- ? Multi-signature scenarios
- ? Format validation
- ? Interoperability testing

## Future Enhancements

Potential future additions:

- **Advanced multi-signature workflows**
- **Performance optimizations for large payloads**  
- **Additional media type handlers**
- **Enhanced tooling for JSON format debugging**