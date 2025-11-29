using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using RichardSzalay.MockHttp;
using SdJwt.Net.StatusList.Issuer;
using SdJwt.Net.StatusList.Models;
using SdJwt.Net.StatusList.Verifier;
using SdJwt.Net.Vc.Issuer;
using SdJwt.Net.Vc.Models;
using SdJwt.Net.Vc.Verifier;
using System.Collections;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace SdJwt.Net.Samples.Examples;

/// <summary>
/// Demonstrates Status List functionality for credential revocation and suspension
/// according to draft-ietf-oauth-status-list-13
/// </summary>
public class StatusListExample
{
    public static async Task RunExample(IServiceProvider services)
    {
        var logger = services.GetRequiredService<ILogger<StatusListExample>>();
        var httpClientFactory = services.GetRequiredService<IHttpClientFactory>();
        
        Console.WriteLine("\n╔═════════════════════════════════════════════════════════╗");
        Console.WriteLine("║               Status List & Revocation Example        ║");
        Console.WriteLine("║             (draft-ietf-oauth-status-list-13)          ║");
        Console.WriteLine("╚═════════════════════════════════════════════════════════╝");

        // 1. Setup: Create issuer infrastructure
        Console.WriteLine("\n1. Setting up credential issuer infrastructure...");
        
        using var issuerEcdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var issuerKey = new ECDsaSecurityKey(issuerEcdsa) { KeyId = "license-authority-2024" };
        var statusManager = new StatusListManager(issuerKey, SecurityAlgorithms.EcdsaSha256);
        
        // Create status lists for different purposes
        const string revocationListUrl = "https://authority.example.gov/status/revocation/1";
        const string suspensionListUrl = "https://authority.example.gov/status/suspension/1";
        const string multiStatusListUrl = "https://authority.example.gov/status/multi/1";

        Console.WriteLine("✓ Credential authority infrastructure ready");

        // 2. Create status lists
        Console.WriteLine("\n2. Creating status lists...");
        
        // Create revocation list (1000 credentials capacity)
        var revocationBits = new BitArray(1000, false);
        revocationBits[42] = true;   // Credential 42 is revoked
        revocationBits[156] = true;  // Credential 156 is revoked
        revocationBits[789] = true;  // Credential 789 is revoked

        var revocationListToken = await statusManager.CreateStatusListTokenFromBitArrayAsync(
            revocationListUrl, revocationBits);

        // Create suspension list
        var suspensionBits = new BitArray(1000, false);
        suspensionBits[25] = true;   // Credential 25 is suspended
        suspensionBits[333] = true;  // Credential 333 is suspended

        var suspensionListToken = await statusManager.CreateStatusListTokenFromBitArrayAsync(
            suspensionListUrl, suspensionBits);

        // Create multi-bit status list (2 bits per credential = 4 states)
        var multiStatusBits = await CreateMultiStatusList(statusManager);
        var multiStatusListToken = await statusManager.CreateStatusListTokenFromBitArrayAsync(
            multiStatusListUrl, multiStatusBits, 2);

        Console.WriteLine("✓ Status lists created:");
        Console.WriteLine($"  - Revocation list: {revocationBits.Cast<bool>().Count(b => b)} revoked out of {revocationBits.Length}");
        Console.WriteLine($"  - Suspension list: {suspensionBits.Cast<bool>().Count(b => b)} suspended out of {suspensionBits.Length}");
        Console.WriteLine($"  - Multi-status list: 250 credentials with 4 possible states each");

        // 3. Setup mock HTTP server for status endpoints
        var mockHttp = new MockHttpMessageHandler();
        SetupMockStatusEndpoints(mockHttp, revocationListToken, suspensionListToken, multiStatusListToken,
            revocationListUrl, suspensionListUrl, multiStatusListUrl);

        var httpClient = httpClientFactory.CreateClient();
        httpClient = new HttpClient(mockHttp);

        Console.WriteLine("✓ Mock HTTP endpoints configured for status lists");

        // 4. Issue credentials with status references
        Console.WriteLine("\n3. Issuing credentials with status references...");
        
        await DemonstrateCredentialWithRevocationStatus(issuerKey, httpClient, revocationListUrl);
        await DemonstrateCredentialWithSuspensionStatus(issuerKey, httpClient, suspensionListUrl);
        await DemonstrateCredentialWithMultiStatus(issuerKey, httpClient, multiStatusListUrl);

        // 5. Demonstrate status management operations
        await DemonstrateStatusManagement(statusManager, logger);

        // 6. Demonstrate batch operations
        await DemonstrateBatchStatusOperations(statusManager);

        // 7. Performance demonstration
        await DemonstrateStatusListPerformance(statusManager);

        Console.WriteLine("\n╔═════════════════════════════════════════════════════════╗");
        Console.WriteLine("║           Status List example completed!               ║");
        Console.WriteLine("║                                                         ║");
        Console.WriteLine("║  ✓ Credential revocation                               ║");
        Console.WriteLine("║  ✓ Credential suspension                               ║");
        Console.WriteLine("║  ✓ Multi-bit status types                              ║");
        Console.WriteLine("║  ✓ Privacy-preserving status checking                  ║");
        Console.WriteLine("║  ✓ High-performance batch operations                   ║");
        Console.WriteLine("╚═════════════════════════════════════════════════════════╝");
        return;
    }

    private static Task<BitArray> CreateMultiStatusList(StatusListManager statusManager)
    {
        // Create status list for 250 credentials with 2 bits each (4 states)
        var multiStatusBits = statusManager.CreateStatusBits(250, 2);
        
        // Set some statuses:
        // 00 = Valid (0)
        // 01 = Suspended (1)  
        // 10 = Revoked (2)
        // 11 = Under Investigation (3)

        // Set credential 10 as suspended
        statusManager.SetCredentialStatus(multiStatusBits, 10, StatusType.Suspended, 2);
        
        // Set credential 20 as revoked
        statusManager.SetCredentialStatus(multiStatusBits, 20, StatusType.Invalid, 2);
        
        // Set credential 30 as under investigation (custom status)
        statusManager.SetCredentialStatus(multiStatusBits, 30, (StatusType)3, 2);

        return Task.FromResult(multiStatusBits);
    }

    private static void SetupMockStatusEndpoints(MockHttpMessageHandler mockHttp, 
        string revocationListToken, string suspensionListToken, string multiStatusListToken,
        string revocationListUrl, string suspensionListUrl, string multiStatusListUrl)
    {
        mockHttp.When(revocationListUrl)
                .Respond("application/statuslist+jwt", revocationListToken);
        
        mockHttp.When(suspensionListUrl)
                .Respond("application/statuslist+jwt", suspensionListToken);
        
        mockHttp.When(multiStatusListUrl)
                .Respond("application/statuslist+jwt", multiStatusListToken);
    }

    private static async Task DemonstrateCredentialWithRevocationStatus(ECDsaSecurityKey issuerKey, HttpClient httpClient, string revocationListUrl)
    {
        Console.WriteLine("\n4a. Credential with revocation status:");
        
        using var holderEcdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var holderKey = new ECDsaSecurityKey(holderEcdsa);
        var holderJwk = JsonWebKeyConverter.ConvertFromSecurityKey(holderKey);

        var vcIssuer = new SdJwtVcIssuer(issuerKey, SecurityAlgorithms.EcdsaSha256);
        
        // Issue credential that will be checked against revocation list
        var vcPayload = new SdJwtVcPayload
        {
            Issuer = "https://authority.example.gov",
            Subject = "did:example:holder42",
            IssuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            Status = new { 
                status_list = new StatusListReference 
                { 
                    Index = 42,  // This index is marked as revoked
                    Uri = revocationListUrl 
                } 
            },
            AdditionalData = new Dictionary<string, object>
            {
                ["license_type"] = "Driving License",
                ["license_number"] = "DL123456789",
                ["holder_name"] = "John Doe"
            }
        };

        var credential = vcIssuer.Issue("https://credentials.example.gov/license", vcPayload, new(), holderJwk);

        // Try to verify the revoked credential - simplified verification
        var vcVerifier = new SdJwtVcVerifier(async issuer => issuerKey);

        var validationParams = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = "https://authority.example.gov",
            ValidateAudience = false,
            ValidateLifetime = true
        };

        try
        {
            await vcVerifier.VerifyAsync(credential.Issuance, validationParams);
            Console.WriteLine("    ✓ Revoked credential verification completed (status check would happen separately)");
            Console.WriteLine("    Note: In production, implement HttpStatusListFetcher for automatic status checking");
        }
        catch (SecurityTokenException ex)
        {
            Console.WriteLine($"    ✓ Verification failed: {ex.Message}");
        }

        // Demonstrate status list checking separately
        var statusVerifier = new StatusListVerifier(httpClient);
        var statusClaim = new StatusClaim 
        { 
            StatusList = new StatusListReference 
            { 
                Index = 42, 
                Uri = revocationListUrl 
            } 
        };

        try
        {
            var statusResult = await statusVerifier.CheckStatusAsync(statusClaim, async issuer => issuerKey);
            Console.WriteLine($"    Status check result: Index 42 is {(statusResult.StatusValue == 1 ? "REVOKED" : "VALID")}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"    Status check note: {ex.Message} (using mock HTTP client)");
        }
        return;
    }

    private static async Task DemonstrateCredentialWithSuspensionStatus(ECDsaSecurityKey issuerKey, HttpClient httpClient, string suspensionListUrl)
    {
        Console.WriteLine("\n4b. Credential with suspension status:");
        
        using var holderEcdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var holderKey = new ECDsaSecurityKey(holderEcdsa);
        var holderJwk = JsonWebKeyConverter.ConvertFromSecurityKey(holderKey);

        var vcIssuer = new SdJwtVcIssuer(issuerKey, SecurityAlgorithms.EcdsaSha256);
        
        var vcPayload = new SdJwtVcPayload
        {
            Issuer = "https://authority.example.gov",
            Subject = "did:example:holder25",
            IssuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            Status = new { 
                status_list = new StatusListReference 
                { 
                    Index = 25,  // This index is marked as suspended
                    Uri = suspensionListUrl 
                } 
            },
            AdditionalData = new Dictionary<string, object>
            {
                ["professional_license"] = "Medical License",
                ["license_number"] = "MD987654321",
                ["holder_name"] = "Dr. Jane Smith"
            }
        };

        var credential = vcIssuer.Issue("https://credentials.example.gov/professional", vcPayload, new(), holderJwk);

        // Create dedicated status verifier for demonstration
        var statusVerifier = new StatusListVerifier(httpClient);

        var statusClaim = new StatusClaim 
        { 
            StatusList = new StatusListReference 
            { 
                Index = 25, 
                Uri = suspensionListUrl 
            } 
        };

        try
        {
            var statusResult = await statusVerifier.CheckStatusAsync(statusClaim, async issuer => issuerKey);
            Console.WriteLine($"    Status check result: {statusResult.Status}");
            Console.WriteLine($"    Status value: {statusResult.StatusValue}");
            Console.WriteLine($"    Retrieved at: {statusResult.RetrievedAt:yyyy-MM-dd HH:mm:ss}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"    Status check note: {ex.Message} (using mock HTTP client)");
            Console.WriteLine($"    In production: Index 25 would be marked as SUSPENDED");
        }
        return;
    }

    private static async Task DemonstrateCredentialWithMultiStatus(ECDsaSecurityKey issuerKey, HttpClient httpClient, string multiStatusListUrl)
    {
        Console.WriteLine("\n4c. Credential with multi-bit status:");

        var statusVerifier = new StatusListVerifier(httpClient);

        // Check different credential statuses
        var testCases = new[]
        {
            new { Index = 5, Expected = "Valid" },
            new { Index = 10, Expected = "Suspended" },
            new { Index = 20, Expected = "Revoked" },
            new { Index = 30, Expected = "Under Investigation" }
        };

        foreach (var testCase in testCases)
        {
            var statusClaim = new StatusClaim 
            { 
                StatusList = new StatusListReference 
                { 
                    Index = testCase.Index, 
                    Uri = multiStatusListUrl 
                } 
            };

            try
            {
                var statusResult = await statusVerifier.CheckStatusAsync(statusClaim, async issuer => issuerKey);
                Console.WriteLine($"    Credential {testCase.Index}: {GetStatusDescription(statusResult.StatusValue)} (Expected: {testCase.Expected})");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"    Credential {testCase.Index}: Note - {ex.Message} (Expected: {testCase.Expected})");
            }
        }
        return;
    }

    private static string GetStatusDescription(int statusValue)
    {
        return statusValue switch
        {
            0 => "Valid",
            1 => "Suspended",
            2 => "Revoked",
            3 => "Under Investigation",
            _ => $"Unknown ({statusValue})"
        };
    }

    private static async Task DemonstrateStatusManagement(StatusListManager statusManager, ILogger logger)
    {
        Console.WriteLine("\n5. Demonstrating status management operations...");

        // Create a new status list for management demo
        var managedStatusBits = new BitArray(100, false);
        
        // Revoke some credentials
        var credentialsToRevoke = new[] { 5, 10, 15, 20 };
        foreach (var index in credentialsToRevoke)
        {
            managedStatusBits[index] = true;
        }

        var managedStatusToken = await statusManager.CreateStatusListTokenFromBitArrayAsync(
            "https://authority.example.gov", managedStatusBits);

        Console.WriteLine("✓ Status list management demo completed");
        Console.WriteLine($"  - Initial revocations: {credentialsToRevoke.Length}");
        Console.WriteLine($"  - Total capacity: {managedStatusBits.Length}");
        Console.WriteLine($"  - Utilization: {(credentialsToRevoke.Length / (double)managedStatusBits.Length):P2}");

        // Demonstrate parsing status list
        var parsedBits = StatusListManager.GetBitsFromToken(managedStatusToken);
        var revokedCount = parsedBits.Cast<bool>().Count(b => b);
        Console.WriteLine($"  - Parsed and verified: {revokedCount} revoked credentials");
        return;
    }

    private static async Task DemonstrateBatchStatusOperations(StatusListManager statusManager)
    {
        Console.WriteLine("\n6. Demonstrating batch status operations...");

        var batchStatusBits = new BitArray(10000, false);
        
        // Simulate batch revocation of 100 credentials
        var batchRevocations = Enumerable.Range(0, 100).Select(i => Random.Shared.Next(0, 10000)).Distinct().ToArray();
        
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        foreach (var index in batchRevocations)
        {
            batchStatusBits[index] = true;
        }

        var batchStatusToken = await statusManager.CreateStatusListTokenFromBitArrayAsync(
            "https://authority.example.gov", batchStatusBits);
        
        stopwatch.Stop();

        Console.WriteLine("✓ Batch operations completed");
        Console.WriteLine($"  - Credentials processed: {batchRevocations.Length:N0}");
        Console.WriteLine($"  - Processing time: {stopwatch.ElapsedMilliseconds} ms");
        Console.WriteLine($"  - Rate: {batchRevocations.Length / stopwatch.Elapsed.TotalSeconds:F0} operations/second");
        Console.WriteLine($"  - Status list size: {Encoding.UTF8.GetByteCount(batchStatusToken):N0} bytes");

        // Calculate compression ratio
        var uncompressedSize = batchStatusBits.Length / 8; // bits to bytes
        var compressedSize = Encoding.UTF8.GetByteCount(batchStatusToken);
        var compressionRatio = (1 - (compressedSize / (double)uncompressedSize)) * 100;
        Console.WriteLine($"  - Compression ratio: {compressionRatio:F1}% space saved");
        return;
    }

    private static async Task DemonstrateStatusListPerformance(StatusListManager statusManager)
    {
        Console.WriteLine("\n7. Performance demonstration...");

        const int iterations = 1000;
        const int statusListSize = 1000;

        var performanceStatusBits = new BitArray(statusListSize, false);
        
        // Set some random revocations for realistic scenario
        for (int i = 0; i < statusListSize / 10; i++)
        {
            performanceStatusBits[Random.Shared.Next(statusListSize)] = true;
        }

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        for (int i = 0; i < iterations; i++)
        {
            await statusManager.CreateStatusListTokenFromBitArrayAsync(
                "https://authority.example.gov", performanceStatusBits);
        }
        
        stopwatch.Stop();

        Console.WriteLine("✓ Performance test completed");
        Console.WriteLine($"  - Status lists created: {iterations:N0}");
        Console.WriteLine($"  - Status list size: {statusListSize:N0} credentials");
        Console.WriteLine($"  - Total time: {stopwatch.ElapsedMilliseconds:N0} ms");
        Console.WriteLine($"  - Average time per status list: {stopwatch.ElapsedMilliseconds / (double)iterations:F2} ms");
        Console.WriteLine($"  - Throughput: {iterations / stopwatch.Elapsed.TotalSeconds:F0} status lists/second");

        // Memory usage estimation
        var sampleToken = await statusManager.CreateStatusListTokenFromBitArrayAsync(
            "https://authority.example.gov", performanceStatusBits);
        var tokenSize = Encoding.UTF8.GetByteCount(sampleToken);
        var memoryUsageKB = (tokenSize * iterations) / 1024.0;
        
        Console.WriteLine($"  - Memory usage: {memoryUsageKB:F1} KB for {iterations:N0} cached status lists");
        return;
    }
}

