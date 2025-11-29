# SD-JWT Status List for .NET

[![NuGet Version](https://img.shields.io/nuget/v/SdJwt.Net.StatusList.svg)](https://www.nuget.org/packages/SdJwt.Net.StatusList/)
[![License](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](https://opensource.org/licenses/Apache-2.0)

A .NET library for **Status List** functionality in SD-JWTs, compliant with **draft-ietf-oauth-status-list-13**. Provides scalable, privacy-preserving credential revocation, suspension, and status management capabilities with enterprise-grade performance and security.

## Features

- **Latest Specification**: Full implementation of draft-ietf-oauth-status-list-13
- **Multi-Purpose Status**: Support for revocation, suspension, and custom status types
- **Scalable Architecture**: Efficient compressed bit-array based status tracking for millions of credentials
- **Privacy Preserving**: Anonymous status checking with no individual credential identification
- **Production Ready**: Compression, caching, retry mechanisms, and comprehensive monitoring support

## Installation

```bash
dotnet add package SdJwt.Net.StatusList
```

> **Note**: This package automatically includes `SdJwt.Net` as a dependency.

## Quick Start

### Creating Status Lists (Issuer Side)

```csharp
using SdJwt.Net.StatusList.Issuer;
using SdJwt.Net.StatusList.Models;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Collections;

// Create signing key
using var key = ECDsa.Create();
var signingKey = new ECDsaSecurityKey(key) { KeyId = "status-key-1" };

// Create status list manager
var statusManager = new StatusListManager(signingKey, SecurityAlgorithms.EcdsaSha256);

// Create status list supporting 10,000 credentials
var statusBits = new BitArray(10000); // All false = active credentials

// Create status list token for revocation tracking
var statusListToken = await statusManager.CreateStatusListTokenFromBitArrayAsync(
    issuer: "https://issuer.example.com",
    statusBits: statusBits);

Console.WriteLine($"Status List Token: {statusListToken}");
Console.WriteLine($"Supports {statusBits.Length:N0} credentials");
```

### Adding Status References to Credentials

```csharp
using SdJwt.Net.StatusList.Models;

// Create status reference for a credential
var statusReference = new StatusListReference
{
    Index = 12345,  // Unique index for this credential (0-based)
    Uri = "https://issuer.example.com/status/revocation/1"
};

// Include in SD-JWT VC payload
var vcPayload = new SdJwtVcPayload
{
    Issuer = "https://issuer.example.com",
    Subject = "did:example:holder123",
    IssuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
    
    // Add status reference
    Status = new { status_list = statusReference },
    
    AdditionalData = new Dictionary<string, object>
    {
        ["degree"] = "Bachelor of Science",
        ["major"] = "Computer Science"
    }
};

Console.WriteLine($"Credential {statusReference.Index} linked to status list");
```

### Updating Status Lists (Revocation/Suspension)

```csharp
// Get current status list with version info
var (currentStatusToken, currentETag) = await GetStatusListWithETagFromStorage("revocation-1");

// Parse status bits using library method
var updatedStatusBits = StatusListManager.GetBitsFromToken(currentStatusToken);

// Revoke specific credentials by setting their bits to 1
var credentialsToRevoke = new[] { 100, 101, 102, 500, 1205 };

foreach (var index in credentialsToRevoke)
{
    updatedStatusBits[index] = true; // 1 = revoked
}

// Create updated status list token
var updatedToken = await statusManager.CreateStatusListTokenFromBitArrayAsync(
    "https://issuer.example.com", updatedStatusBits);

// Save with optimistic concurrency control
var success = await SaveStatusListWithETagToStorage("revocation-1", updatedToken, expectedETag: currentETag);
if (!success)
{
    throw new InvalidOperationException("Status list was modified by another process. Please retry.");
}

Console.WriteLine($"Revoked {credentialsToRevoke.Length} credentials");
```

## Status Checking (Verifier Side)

### Automatic Status Verification

```csharp
using SdJwt.Net.Vc.Verifier;
using SdJwt.Net.StatusList.Verifier;

// Create VC verifier with automatic status checking
var httpClient = new HttpClient();
var statusListFetcher = new HttpStatusListFetcher(httpClient);

var verifier = new SdJwtVcVerifier(
    keyProvider: async issuer => await ResolveIssuerPublicKey(issuer),
    statusListFetcher: statusListFetcher,  // Required for HTTP fetching
    logger: logger);

// Configure validation with status checking
var validationParams = new TokenValidationParameters
{
    ValidateIssuer = true,
    ValidIssuer = "https://issuer.example.com",
    ValidateAudience = false,
    ValidateLifetime = true
};

try
{
    // This will automatically check status if status claim is present
    var result = await verifier.VerifyAsync(presentation, validationParams, kbValidationParams);
    
    Console.WriteLine("Credential is valid and active");
    Console.WriteLine($"Key Binding Verified: {result.KeyBindingVerified}");
}
catch (SecurityTokenException ex) when (ex.Message.Contains("revoked"))
{
    Console.WriteLine("Credential has been revoked");
}
catch (SecurityTokenException ex) when (ex.Message.Contains("suspended"))
{
    Console.WriteLine("Credential is temporarily suspended");
}
```

### Manual Status Checking

```csharp
using SdJwt.Net.StatusList.Verifier;

// Create dedicated status verifier for manual status checking
var httpClient = new HttpClient();
var statusVerifier = new StatusListVerifier(
    httpClient: httpClient,
    memoryCache: new MemoryCache(new MemoryCacheOptions()),
    logger: logger);

// For automatic status checking, you need to manually check status claims
// Extract status claim from the verified credential
var verificationResult = await verifier.VerifyAsync(presentation, validationParams, kbValidationParams);
var statusJson = verificationResult.ClaimsPrincipal.FindFirst("status")?.Value;

if (!string.IsNullOrEmpty(statusJson))
{
    try
    {
        var statusClaim = JsonSerializer.Deserialize<StatusClaim>(statusJson, SdJwtConstants.DefaultJsonSerializerOptions);
        if (statusClaim?.StatusList != null)
        {
            // Check status using the dedicated verifier
            var statusResult = await statusVerifier.CheckStatusAsync(
                statusClaim,
                async issuer => await ResolveIssuerPublicKey(issuer));

            if (statusResult.Status == StatusType.Invalid)
            {
                throw new SecurityTokenException("Credential has been revoked");
            }
            else if (statusResult.Status == StatusType.Suspended)
            {
                throw new SecurityTokenException("Credential is temporarily suspended");
            }

            Console.WriteLine("Credential is valid and active");
        }
    }
    catch (JsonException ex)
    {
        logger.LogWarning(ex, "Failed to parse status claim");
    }
}
else
{
    Console.WriteLine("Credential is valid (no status claim present)");
}
```

## Advanced Status Management

### Multi-Bit Status Lists (Custom Status Types)

```csharp
// Create status list with 2 bits per credential (4 possible states)
var statusManager = new StatusListManager(signingKey, SecurityAlgorithms.EcdsaSha256);

// Initialize status list for 10,000 credentials with 2 bits each
var credentialCount = 10000;
var bitsPerCredential = 2;
var statusBits = statusManager.CreateStatusBits(credentialCount, bitsPerCredential);

// Status encoding:
// 00 = Active (0)
// 01 = Suspended (1)  
// 10 = Revoked (2)
// 11 = Under Investigation (3)

// Use the library's safe API instead of manual bit manipulation
statusManager.SetCredentialStatus(statusBits, credentialIndex: 150, StatusType.Suspended);
statusManager.SetCredentialStatus(statusBits, credentialIndex: 300, StatusType.Revoked);
statusManager.SetCredentialStatus(statusBits, credentialIndex: 450, StatusType.UnderInvestigation);

// Create status list token
var multiStatusToken = await statusManager.CreateStatusListTokenFromBitArrayAsync(
    "https://issuer.example.com", statusBits, bitsPerCredential: 2);

// Query status safely
var credential150Status = statusManager.GetCredentialStatus(statusBits, 150, bitsPerCredential: 2);
Console.WriteLine($"Credential 150: {credential150Status}"); // Output: Suspended

public enum StatusType
{
    Active = 0,
    Suspended = 1,
    Revoked = 2,
    UnderInvestigation = 3
}
```

## Security Considerations

### Concurrency Control (Critical for Production)

Status list updates in multi-user environments require proper concurrency control to prevent race conditions:

```csharp
// DANGEROUS: Race condition possible
public async Task RevokeCredentialUnsafe(int credentialIndex)
{
    var currentList = await GetStatusList(); // Version A
    currentList[credentialIndex] = true;     // Another admin might update between these lines
    await SaveStatusList(currentList);       // Overwrites other admin's changes!
}

// SAFE: Optimistic concurrency control
public async Task RevokeCredentialSafe(int credentialIndex)
{
    int maxRetries = 3;
    for (int attempt = 0; attempt < maxRetries; attempt++)
    {
        var (currentList, etag) = await GetStatusListWithETag();
        currentList[credentialIndex] = true;
        
        var success = await TrySaveStatusList(currentList, expectedETag: etag);
        if (success) return; // Success!
        
        // Another process modified the list, retry with fresh data
        await Task.Delay(100 * (attempt + 1)); // Exponential backoff
    }
    throw new ConcurrencyException("Failed to update status list after retries");
}
```

### Privacy-Preserving Features

- **Anonymous Checking**: Status requests don't reveal which specific credential is being checked
- **Batch Privacy**: Large status lists prevent credential enumeration attacks
- **No Correlation**: Multiple checks can't be linked to the same credential
- **Compressed Storage**: GZIP compression prevents status pattern analysis

## Production Deployment Checklist

### Critical Requirements
- [ ] **Optimistic Concurrency**: Implement ETag-based version control for all status list updates
- [ ] **HTTP Client Configuration**: Provide `HttpClient` dependency for status list fetching
- [ ] **Storage Abstraction**: Use `IStatusListStorage` interface with transactional ETag support
- [ ] **Error Handling**: Implement retry logic with exponential backoff for concurrent updates
- [ ] **Caching Strategy**: Use HTTP conditional requests (ETags) to minimize network overhead

### Security Hardening
- [ ] **Algorithm Validation**: Verify status list JWT signatures with approved algorithms only
- [ ] **Input Sanitization**: Validate all credential indices and status list URIs
- [ ] **Fail Secure**: Default to "credential is revoked" if status check fails
- [ ] **Rate Limiting**: Protect status endpoints from abuse
- [ ] **HTTPS Only**: Never fetch status lists over unencrypted connections

### Performance Optimization
- [ ] **Compression**: Enable GZIP compression for status list responses
- [ ] **CDN Deployment**: Cache status lists at edge locations
- [ ] **Database Indexing**: Index status lists by `ListId` and `Version` columns
- [ ] **Connection Pooling**: Configure HTTP client with appropriate connection limits
- [ ] **Background Updates**: Process bulk revocations asynchronously

## Related Packages

- **[SdJwt.Net](https://www.nuget.org/packages/SdJwt.Net/)** - Core SD-JWT functionality (dependency)
- **[SdJwt.Net.Vc](https://www.nuget.org/packages/SdJwt.Net.Vc/)** - SD-JWT Verifiable Credentials integration

## License

Licensed under the Apache License 2.0. See [LICENSE](LICENSE) for details.

---

**Enterprise Ready Features:**
- **Scalability**: Support for millions of credentials with efficient bit array compression
- **Performance**: Optimized caching, conditional requests, and batch operations
- **Monitoring**: Comprehensive metrics and analytics for production deployment
- **Security**: Privacy-preserving design with robust validation and error handling
