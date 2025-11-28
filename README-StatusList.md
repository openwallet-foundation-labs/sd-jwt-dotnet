# SD-JWT Status List for .NET

[![NuGet Version](https://img.shields.io/nuget/v/SdJwt.Net.StatusList.svg)](https://www.nuget.org/packages/SdJwt.Net.StatusList/)
[![License](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](https://opensource.org/licenses/Apache-2.0)

A .NET library for **Status List** functionality in SD-JWTs, compliant with **draft-ietf-oauth-status-list-13**. Provides scalable, privacy-preserving credential revocation, suspension, and status management capabilities with enterprise-grade performance and security.

## ?? Features

- **Latest Specification**: Full implementation of draft-ietf-oauth-status-list-13
- **Multi-Purpose Status**: Support for revocation, suspension, and custom status types
- **Scalable Architecture**: Efficient compressed bit-array based status tracking for millions of credentials
- **Privacy Preserving**: Anonymous status checking with no individual credential identification
- **Production Ready**: Compression, caching, retry mechanisms, and comprehensive monitoring support

## ?? Installation

```bash
dotnet add package SdJwt.Net.StatusList
```

> **Note**: This package automatically includes `SdJwt.Net` as a dependency.

## ?? Quick Start

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
// Get current status list
var currentStatusList = await GetStatusListFromStorage("revocation-1");

// Revoke specific credentials by setting their bits to 1
var credentialsToRevoke = new[] { 100, 101, 102, 500, 1205 };
var updatedStatusBits = ParseStatusListBits(currentStatusList);

foreach (var index in credentialsToRevoke)
{
    updatedStatusBits[index] = true; // 1 = revoked
}

// Create updated status list token
var updatedToken = await statusManager.CreateStatusListTokenFromBitArrayAsync(
    "https://issuer.example.com", updatedStatusBits);

// Store updated status list
await SaveStatusListToStorage("revocation-1", updatedToken);

Console.WriteLine($"Revoked {credentialsToRevoke.Length} credentials");
```

## ?? Status Checking (Verifier Side)

### Automatic Status Verification

```csharp
using SdJwt.Net.Vc.Verifier;

// Create VC verifier with automatic status checking
var verifier = new SdJwtVcVerifier(
    keyProvider: async issuer => await ResolveIssuerPublicKey(issuer),
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
    
    Console.WriteLine("? Credential is valid and active");
    Console.WriteLine($"Key Binding Verified: {result.KeyBindingVerified}");
}
catch (SecurityTokenException ex) when (ex.Message.Contains("revoked"))
{
    Console.WriteLine("? Credential has been revoked");
}
catch (SecurityTokenException ex) when (ex.Message.Contains("suspended"))
{
    Console.WriteLine("?? Credential is temporarily suspended");
}
```

### Manual Status Checking

```csharp
using SdJwt.Net.StatusList.Verifier;

// Create dedicated status verifier
var statusVerifier = new StatusListVerifier(
    httpClient: new HttpClient(),
    memoryCache: new MemoryCache(new MemoryCacheOptions()));

// Extract status reference from credential
var statusRef = new StatusListReference
{
    Index = 12345,
    Uri = "https://issuer.example.com/status/revocation/1"
};

// Check status
var statusResult = await statusVerifier.CheckStatusAsync(
    statusRef,
    async issuer => await ResolveIssuerPublicKey(issuer));

Console.WriteLine($"Status: {(statusResult.IsActive ? "Active" : "Revoked")}");
Console.WriteLine($"Last Updated: {statusResult.LastChecked}");
```

## ??? Advanced Status Management

### Multi-Bit Status Lists (Custom Status Types)

```csharp
// Create status list with 2 bits per credential (4 possible states)
var advancedStatusBits = new BitArray(20000); // 10,000 credentials * 2 bits each

// Status encoding:
// 00 = Active
// 01 = Suspended  
// 10 = Revoked
// 11 = Under Investigation

var statusManager = new StatusListManager(signingKey, SecurityAlgorithms.EcdsaSha256);

// Set credential 150 to "Suspended" (01)
SetCredentialStatus(advancedStatusBits, credentialIndex: 150, status: 1, bitsPerCredential: 2);

// Set credential 300 to "Revoked" (10) 
SetCredentialStatus(advancedStatusBits, credentialIndex: 300, status: 2, bitsPerCredential: 2);

var multiStatusToken = await statusManager.CreateStatusListTokenFromBitArrayAsync(
    "https://issuer.example.com", advancedStatusBits);

// Helper method for multi-bit status
void SetCredentialStatus(BitArray bits, int credentialIndex, int status, int bitsPerCredential)
{
    var startBit = credentialIndex * bitsPerCredential;
    for (int i = 0; i < bitsPerCredential; i++)
    {
        bits[startBit + i] = ((status >> i) & 1) == 1;
    }
}
```

### Batch Status Operations

```csharp
public class BatchStatusManager
{
    private readonly StatusListManager _statusManager;
    
    public async Task<string> BatchRevokeCredentialsAsync(
        string currentStatusToken,
        IEnumerable<int> credentialIndices)
    {
        var statusBits = await ParseStatusListAsync(currentStatusToken);
        
        // Batch revoke all specified credentials
        var revokedCount = 0;
        foreach (var index in credentialIndices)
        {
            if (index >= 0 && index < statusBits.Length && !statusBits[index])
            {
                statusBits[index] = true; // Revoke
                revokedCount++;
            }
        }
        
        var updatedToken = await _statusManager.CreateStatusListTokenFromBitArrayAsync(
            "https://issuer.example.com", statusBits);
            
        Console.WriteLine($"Batch revoked {revokedCount} credentials");
        return updatedToken;
    }
    
    public async Task<StatusStatistics> AnalyzeStatusListAsync(string statusToken)
    {
        var statusBits = await ParseStatusListAsync(statusToken);
        
        return new StatusStatistics
        {
            TotalCredentials = statusBits.Length,
            ActiveCredentials = statusBits.Cast<bool>().Count(b => !b),
            RevokedCredentials = statusBits.Cast<bool>().Count(b => b),
            RevocationRate = (double)statusBits.Cast<bool>().Count(b => b) / statusBits.Length
        };
    }
}

public record StatusStatistics
{
    public int TotalCredentials { get; init; }
    public int ActiveCredentials { get; init; }
    public int RevokedCredentials { get; init; }
    public double RevocationRate { get; init; }
}
```

## ?? HTTP Integration and Hosting

### ASP.NET Core Status Endpoint

```csharp
[ApiController]
[Route("api/status")]
public class StatusListController : ControllerBase
{
    private readonly StatusListManager _statusManager;
    private readonly IMemoryCache _cache;
    private readonly ILogger<StatusListController> _logger;

    [HttpGet("revocation/{listId}")]
    public async Task<IActionResult> GetRevocationList(string listId)
    {
        try
        {
            var cacheKey = $"status-list-{listId}";
            
            // Check cache first
            if (_cache.TryGetValue(cacheKey, out string? cachedToken))
            {
                SetCacheHeaders(cachedToken!);
                return Ok(cachedToken);
            }
            
            // Retrieve from storage
            var statusToken = await GetStatusListFromStorage(listId);
            if (statusToken == null)
                return NotFound();
            
            // Cache for 15 minutes
            _cache.Set(cacheKey, statusToken, TimeSpan.FromMinutes(15));
            
            SetCacheHeaders(statusToken);
            return Ok(statusToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve status list {ListId}", listId);
            return StatusCode(500);
        }
    }
    
    [HttpPost("revocation/{listId}/revoke")]
    public async Task<IActionResult> RevokeCredentials(
        string listId, 
        [FromBody] RevokeCredentialsRequest request)
    {
        try
        {
            var currentToken = await GetStatusListFromStorage(listId);
            if (currentToken == null)
                return NotFound();
                
            var statusBits = await ParseStatusListAsync(currentToken);
            
            // Revoke specified credentials
            var revokedCount = 0;
            foreach (var index in request.CredentialIndices)
            {
                if (index >= 0 && index < statusBits.Length && !statusBits[index])
                {
                    statusBits[index] = true;
                    revokedCount++;
                }
            }
            
            var updatedToken = await _statusManager.CreateStatusListTokenFromBitArrayAsync(
                "https://issuer.example.com", statusBits);
                
            await SaveStatusListToStorage(listId, updatedToken);
            
            // Invalidate cache
            _cache.Remove($"status-list-{listId}");
            
            _logger.LogInformation(
                "Revoked {RevokedCount} credentials in list {ListId}", 
                revokedCount, listId);
                
            return Ok(new { message = $"Revoked {revokedCount} credentials" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to revoke credentials in list {ListId}", listId);
            return StatusCode(500);
        }
    }
    
    private void SetCacheHeaders(string statusToken)
    {
        Response.Headers.Add("Content-Type", "application/statuslist+jwt");
        Response.Headers.Add("Cache-Control", "public, max-age=900"); // 15 minutes
        
        // Generate ETag for conditional requests
        var etag = GenerateETag(statusToken);
        Response.Headers.Add("ETag", etag);
    }
}

public class RevokeCredentialsRequest
{
    public int[] CredentialIndices { get; set; } = Array.Empty<int>();
}
```

### High-Performance Status Checking

```csharp
public class OptimizedStatusVerifier
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly ILogger<OptimizedStatusVerifier> _logger;
    
    public async Task<StatusCheckResult> CheckStatusOptimizedAsync(
        StatusListReference statusRef,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = $"status-{statusRef.Uri}";
        
        // Check cache first
        if (_cache.TryGetValue(cacheKey, out BitArray? cachedBits))
        {
            return new StatusCheckResult
            {
                IsActive = !cachedBits![statusRef.Index],
                FromCache = true,
                LastChecked = DateTime.UtcNow
            };
        }
        
        // Fetch with conditional request support
        var request = new HttpRequestMessage(HttpMethod.Get, statusRef.Uri);
        
        // Add conditional headers if we have cached data
        if (_cache.TryGetValue($"{cacheKey}-etag", out string? etag))
        {
            request.Headers.Add("If-None-Match", etag);
        }
        
        try
        {
            using var response = await _httpClient.SendAsync(request, cancellationToken);
            
            if (response.StatusCode == HttpStatusCode.NotModified)
            {
                // Status list unchanged, use cached result
                var cachedResult = _cache.Get<StatusCheckResult>($"{cacheKey}-result");
                return cachedResult ?? new StatusCheckResult { IsActive = true };
            }
            
            response.EnsureSuccessStatusCode();
            var statusToken = await response.Content.ReadAsStringAsync(cancellationToken);
            
            // Parse and cache status list
            var statusBits = await ParseStatusListAsync(statusToken);
            
            _cache.Set(cacheKey, statusBits, TimeSpan.FromMinutes(15));
            if (response.Headers.ETag != null)
            {
                _cache.Set($"{cacheKey}-etag", response.Headers.ETag.Tag, TimeSpan.FromMinutes(15));
            }
            
            var result = new StatusCheckResult
            {
                IsActive = !statusBits[statusRef.Index],
                FromCache = false,
                LastChecked = DateTime.UtcNow
            };
            
            _cache.Set($"{cacheKey}-result", result, TimeSpan.FromMinutes(15));
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to check status for {Uri}", statusRef.Uri);
            
            // Graceful degradation: assume active if status check fails
            return new StatusCheckResult { IsActive = true, Error = ex.Message };
        }
    }
}

public class StatusCheckResult
{
    public bool IsActive { get; init; }
    public bool FromCache { get; init; }
    public DateTime LastChecked { get; init; }
    public string? Error { get; init; }
}
```

## ?? Monitoring and Analytics

### Status List Metrics

```csharp
public class StatusListMetrics
{
    private readonly IMetricsLogger _metrics;
    
    public async Task<StatusListAnalytics> AnalyzeStatusListPerformanceAsync(
        string statusListUri)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            // Fetch status list
            var response = await _httpClient.GetAsync(statusListUri);
            response.EnsureSuccessStatusCode();
            
            var statusToken = await response.Content.ReadAsStringAsync();
            var statusBits = await ParseStatusListAsync(statusToken);
            
            var analytics = new StatusListAnalytics
            {
                TotalCredentials = statusBits.Length,
                ActiveCredentials = statusBits.Cast<bool>().Count(b => !b),
                RevokedCredentials = statusBits.Cast<bool>().Count(b => b),
                ResponseTime = stopwatch.Elapsed,
                ResponseSize = response.Content.Headers.ContentLength ?? 0,
                CompressionEffective = IsCompressionEffective(response),
                LastModified = response.Content.Headers.LastModified?.DateTime
            };
            
            analytics.RevocationRate = (double)analytics.RevokedCredentials / analytics.TotalCredentials;
            analytics.CompressionRatio = CalculateCompressionRatio(statusBits, analytics.ResponseSize);
            
            // Record metrics
            _metrics.RecordValue("status_list_response_time_ms", stopwatch.ElapsedMilliseconds);
            _metrics.RecordValue("status_list_size_bytes", analytics.ResponseSize);
            _metrics.RecordValue("status_list_revocation_rate", analytics.RevocationRate);
            
            return analytics;
        }
        catch (Exception ex)
        {
            _metrics.Increment("status_list_check_failures");
            throw;
        }
    }
}

public record StatusListAnalytics
{
    public int TotalCredentials { get; init; }
    public int ActiveCredentials { get; init; }
    public int RevokedCredentials { get; init; }
    public double RevocationRate { get; init; }
    public TimeSpan ResponseTime { get; init; }
    public long ResponseSize { get; init; }
    public double CompressionRatio { get; init; }
    public bool CompressionEffective { get; init; }
    public DateTime? LastModified { get; init; }
}
```

## ?? Security Considerations

### Privacy-Preserving Features

- **Anonymous Checking**: Status requests don't reveal which specific credential is being checked
- **Batch Privacy**: Large status lists prevent credential enumeration attacks
- **No Correlation**: Multiple checks can't be linked to the same credential
- **Compressed Storage**: GZIP compression prevents status pattern analysis

### Production Security Configuration

```csharp
var secureStatusOptions = new StatusListOptions
{
    // Security settings
    ValidateStatusListIssuer = true,        // Verify issuer signature
    ValidateStatusListTiming = true,        // Check expiration times
    MaxStatusListAge = TimeSpan.FromHours(6), // Maximum age for cached lists
    
    // Performance settings
    EnableCaching = true,
    CacheDuration = TimeSpan.FromMinutes(30),
    StatusCheckTimeout = TimeSpan.FromSeconds(10),
    
    // Error handling
    FailOnStatusCheckError = true,          // Fail secure
    RetryPolicy = new RetryPolicy
    {
        MaxRetries = 2,
        BaseDelay = TimeSpan.FromMilliseconds(500),
        UseExponentialBackoff = true
    }
};
```

## ?? Related Packages

- **[SdJwt.Net](https://www.nuget.org/packages/SdJwt.Net/)** - Core SD-JWT functionality (dependency)
- **[SdJwt.Net.Vc](https://www.nuget.org/packages/SdJwt.Net.Vc/)** - SD-JWT Verifiable Credentials integration

## ?? License

Licensed under the Apache License 2.0. See [LICENSE](LICENSE) for details.

---

**Enterprise Ready Features:**
- **Scalability**: Support for millions of credentials with efficient bit array compression
- **Performance**: Optimized caching, conditional requests, and batch operations
- **Monitoring**: Comprehensive metrics and analytics for production deployment
- **Security**: Privacy-preserving design with robust validation and error handling