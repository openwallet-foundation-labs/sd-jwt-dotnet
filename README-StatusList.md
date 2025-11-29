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

## ?? Status Checking (Verifier Side)

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
                throw new SecurityTokenException("? Credential has been revoked");
            }
            else if (statusResult.Status == StatusType.Suspended)
            {
                throw new SecurityTokenException("?? Credential is temporarily suspended");
            }

            Console.WriteLine("? Credential is valid and active");
        }
    }
    catch (JsonException ex)
    {
        logger.LogWarning(ex, "Failed to parse status claim");
    }
}
else
{
    Console.WriteLine("? Credential is valid (no status claim present)");
}
```

## ??? Advanced Status Management

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

### Batch Status Operations

```csharp
public class BatchStatusManager
{
    private readonly StatusListManager _statusManager;
    private readonly IStatusListStorage _storage;
    
    public async Task<string> BatchRevokeCredentialsAsync(
        string listId,
        IEnumerable<int> credentialIndices)
    {
        // Get current status list with version control
        var (currentStatusToken, currentETag) = await _storage.GetStatusListWithETagAsync(listId);
        
        // Parse status bits using safe library method
        var statusBits = StatusListManager.GetBitsFromToken(currentStatusToken);
        
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
            
        // Save with optimistic concurrency control
        var success = await _storage.TrySaveStatusListAsync(listId, updatedToken, expectedETag: currentETag);
        if (!success)
        {
            throw new ConcurrencyException("Status list was modified during batch operation. Please retry.");
        }
            
        Console.WriteLine($"Batch revoked {revokedCount} credentials");
        return updatedToken;
    }
    
    public async Task<StatusStatistics> AnalyzeStatusListAsync(string statusToken)
    {
        var statusBits = StatusListManager.GetBitsFromToken(statusToken);
        
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

public class ConcurrencyException : Exception
{
    public ConcurrencyException(string message) : base(message) { }
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
    private readonly IStatusListStorage _storage;
    private readonly ILogger<StatusListController> _logger;

    [HttpGet("revocation/{listId}")]
    public async Task<IActionResult> GetRevocationList(string listId)
    {
        try
        {
            var cacheKey = $"status-list-{listId}";
            
            // Check cache first with ETag
            if (_cache.TryGetValue(cacheKey, out CachedStatusList? cached))
            {
                // Support conditional requests (HTTP 304)
                if (Request.Headers.TryGetValue("If-None-Match", out var ifNoneMatch) && 
                    ifNoneMatch == cached.ETag)
                {
                    return StatusCode(304); // Not Modified
                }
                
                SetCacheHeaders(cached.Token, cached.ETag);
                return Ok(cached.Token);
            }
            
            // Retrieve from storage with ETag
            var (statusToken, etag) = await _storage.GetStatusListWithETagAsync(listId);
            if (statusToken == null)
                return NotFound();
            
            // Cache for 15 minutes
            var cachedList = new CachedStatusList(statusToken, etag);
            _cache.Set(cacheKey, cachedList, TimeSpan.FromMinutes(15));
            
            SetCacheHeaders(statusToken, etag);
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
            // 1. Get current token with version (ETag) for optimistic concurrency
            var (currentToken, currentETag) = await _storage.GetStatusListWithETagAsync(listId);
            if (currentToken == null)
                return NotFound();
                
            // 2. Optimistic Concurrency Check
            if (Request.Headers.TryGetValue("If-Match", out var ifMatch) && ifMatch != currentETag)
            {
                return StatusCode(412, new { error = "precondition_failed", 
                    message = "Status list has changed since you last retrieved it. Please refresh and retry." });
            }

            // 3. Parse and modify bits using safe library method
            var statusBits = StatusListManager.GetBitsFromToken(currentToken);
            
            var revokedCount = 0;
            foreach (var index in request.CredentialIndices)
            {
                if (index >= 0 && index < statusBits.Length && !statusBits[index])
                {
                    statusBits[index] = true; // Revoke
                    revokedCount++;
                }
            }
            
            // 4. Create new token
            var updatedToken = await _statusManager.CreateStatusListTokenFromBitArrayAsync(
                "https://issuer.example.com", statusBits);
                
            // 5. Try to save with optimistic concurrency control
            var (success, newETag) = await _storage.TrySaveStatusListAsync(listId, updatedToken, expectedETag: currentETag);
            
            if (!success)
            {
                return StatusCode(409, new { error = "conflict", 
                    message = "Status list was modified by another process. Please retry." });
            }
            
            // 6. Invalidate cache
            _cache.Remove($"status-list-{listId}");
            
            _logger.LogInformation(
                "Revoked {RevokedCount} credentials in list {ListId}", 
                revokedCount, listId);
                
            return Ok(new { 
                message = $"Revoked {revokedCount} credentials",
                newETag = newETag,
                totalRevoked = revokedCount
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to revoke credentials in list {ListId}", listId);
            return StatusCode(500, new { error = "internal_error", message = "Failed to process revocation request" });
        }
    }
    
    private void SetCacheHeaders(string statusToken, string etag)
    {
        Response.Headers.Add("Content-Type", "application/statuslist+jwt");
        Response.Headers.Add("Cache-Control", "public, max-age=900"); // 15 minutes
        Response.Headers.Add("ETag", $"\"{etag}\"");
    }
}

public class RevokeCredentialsRequest
{
    public int[] CredentialIndices { get; set; } = Array.Empty<int>();
}

public record CachedStatusList(string Token, string ETag);

// Storage interface for proper abstraction
public interface IStatusListStorage
{
    Task<(string? token, string etag)> GetStatusListWithETagAsync(string listId);
    Task<(bool success, string newETag)> TrySaveStatusListAsync(string listId, string token, string expectedETag);
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
        if (_cache.TryGetValue(cacheKey, out CachedStatusData? cached))
        {
            return new StatusCheckResult
            {
                IsActive = !cached.StatusBits[statusRef.Index],
                FromCache = true,
                LastChecked = cached.LastChecked,
                ETag = cached.ETag
            };
        }
        
        // Fetch with conditional request support
        var request = new HttpRequestMessage(HttpMethod.Get, statusRef.Uri);
        
        // Add conditional headers if we have cached ETag
        if (_cache.TryGetValue($"{cacheKey}-etag", out string? cachedETag))
        {
            request.Headers.Add("If-None-Match", cachedETag);
        }
        
        try
        {
            using var response = await _httpClient.SendAsync(request, cancellationToken);
            
            if (response.StatusCode == HttpStatusCode.NotModified)
            {
                // Status list unchanged, use cached result
                if (_cache.TryGetValue(cacheKey, out CachedStatusData? cachedData))
                {
                    return new StatusCheckResult 
                    { 
                        IsActive = !cachedData.StatusBits[statusRef.Index],
                        FromCache = true,
                        LastChecked = cachedData.LastChecked,
                        ETag = cachedData.ETag
                    };
                }
            }
            
            response.EnsureSuccessStatusCode();
            var statusToken = await response.Content.ReadAsStringAsync(cancellationToken);
            
            // Parse using safe library method
            var statusBits = StatusListManager.GetBitsFromToken(statusToken);
            var etag = response.Headers.ETag?.Tag ?? GenerateETag(statusToken);
            
            // Cache parsed data
            var statusData = new CachedStatusData(statusBits, DateTime.UtcNow, etag);
            _cache.Set(cacheKey, statusData, TimeSpan.FromMinutes(15));
            _cache.Set($"{cacheKey}-etag", etag, TimeSpan.FromMinutes(15));
            
            var result = new StatusCheckResult
            {
                IsActive = !statusBits[statusRef.Index],
                FromCache = false,
                LastChecked = DateTime.UtcNow,
                ETag = etag
            };
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to check status for {Uri}", statusRef.Uri);
            
            // Graceful degradation: assume active if status check fails
            return new StatusCheckResult 
            { 
                IsActive = true, 
                Error = ex.Message,
                LastChecked = DateTime.UtcNow
            };
        }
    }
    
    private static string GenerateETag(string content)
    {
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(content));
        return Convert.ToHexString(hash)[..16]; // First 16 hex chars
    }
}

public record CachedStatusData(BitArray StatusBits, DateTime LastChecked, string ETag);

public class StatusCheckResult
{
    public bool IsActive { get; init; }
    public bool FromCache { get; init; }
    public DateTime LastChecked { get; init; }
    public string? Error { get; init; }
    public string? ETag { get; init; }
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

### Concurrency Control (Critical for Production)

Status list updates in multi-user environments require proper concurrency control to prevent race conditions:

```csharp
// ? DANGEROUS: Race condition possible
public async Task RevokeCredentialUnsafe(int credentialIndex)
{
    var currentList = await GetStatusList(); // Version A
    currentList[credentialIndex] = true;     // Another admin might update between these lines
    await SaveStatusList(currentList);       // Overwrites other admin's changes!
}

// ? SAFE: Optimistic concurrency control
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

### Storage Implementation Pattern

```csharp
public class SqlServerStatusListStorage : IStatusListStorage
{
    public async Task<(string? token, string etag)> GetStatusListWithETagAsync(string listId)
    {
        var query = "SELECT Token, CONVERT(varchar(50), Version) as ETag FROM StatusLists WHERE ListId = @listId";
        // ... execute query
        return (token, etag);
    }
    
    public async Task<(bool success, string newETag)> TrySaveStatusListAsync(
        string listId, string token, string expectedETag)
    {
        var query = @"
            UPDATE StatusLists 
            SET Token = @token, Version = Version + 1, LastModified = GETUTCDATE()
            WHERE ListId = @listId AND CONVERT(varchar(50), Version) = @expectedETag";
            
        var rowsAffected = await connection.ExecuteAsync(query, new { listId, token, expectedETag });
        
        if (rowsAffected == 0)
            return (false, ""); // Optimistic concurrency failure
            
        // Get new ETag
        var newETag = await connection.QuerySingleAsync<string>(
            "SELECT CONVERT(varchar(50), Version) FROM StatusLists WHERE ListId = @listId", 
            new { listId });
            
        return (true, newETag);
    }
}
```

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

## ?? Production Deployment Checklist

### ? Critical Requirements
- [ ] **Optimistic Concurrency**: Implement ETag-based version control for all status list updates
- [ ] **HTTP Client Configuration**: Provide `HttpClient` dependency for status list fetching
- [ ] **Storage Abstraction**: Use `IStatusListStorage` interface with transactional ETag support
- [ ] **Error Handling**: Implement retry logic with exponential backoff for concurrent updates
- [ ] **Caching Strategy**: Use HTTP conditional requests (ETags) to minimize network overhead

### ? Security Hardening
- [ ] **Algorithm Validation**: Verify status list JWT signatures with approved algorithms only
- [ ] **Input Sanitization**: Validate all credential indices and status list URIs
- [ ] **Fail Secure**: Default to "credential is revoked" if status check fails
- [ ] **Rate Limiting**: Protect status endpoints from abuse
- [ ] **HTTPS Only**: Never fetch status lists over unencrypted connections

### ? Performance Optimization
- [ ] **Compression**: Enable GZIP compression for status list responses
- [ ] **CDN Deployment**: Cache status lists at edge locations
- [ ] **Database Indexing**: Index status lists by `ListId` and `Version` columns
- [ ] **Connection Pooling**: Configure HTTP client with appropriate connection limits
- [ ] **Background Updates**: Process bulk revocations asynchronously

### ? Monitoring & Observability
- [ ] **Metrics Collection**: Track revocation rates, response times, cache hit rates
- [ ] **Alert Thresholds**: Monitor for unusual revocation patterns or performance degradation
- [ ] **Audit Logging**: Log all status changes with administrator identification
- [ ] **Health Checks**: Implement status list endpoint availability monitoring

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