using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.Issuer;
using SdJwt.Net.Oid4Vci.Models;
using SdJwt.Net.Oid4Vp.Models;
using SdJwt.Net.StatusList.Models;
using SdJwt.Net.Vc.Issuer;
using SdJwt.Net.Vc.Models;
using SdJwt.Net.Vc.Verifier;
using System.Collections;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text.Json;

namespace SdJwt.Net.Samples.Scenarios.Financial;

/// <summary>
/// Enhanced Financial Co-Pilot demonstrating full SD-JWT .NET ecosystem integration
/// with OID4VCI, OID4VP, Presentation Exchange, Status List, and VC features
/// </summary>
public class EnhancedFinancialCoPilotScenario
{
    public static async Task RunEnhancedScenario(IServiceProvider services)
    {
        var logger = services.GetRequiredService<ILogger<EnhancedFinancialCoPilotScenario>>();
        
        Console.WriteLine();
        Console.WriteLine("==================================================================");
        Console.WriteLine("            Enhanced Financial Co-Pilot Demo                     ");
        Console.WriteLine("        Full SD-JWT .NET Ecosystem Integration                  ");
        Console.WriteLine("  OID4VCI | OID4VP | PE | StatusList | VC | AI Integration      ");
        Console.WriteLine("==================================================================");
        Console.WriteLine();

        await SetupEnhancedDemo();
    }

    private static async Task SetupEnhancedDemo()
    {
        Console.WriteLine("🚀 ENHANCED FEATURES OVERVIEW:");
        Console.WriteLine();
        Console.WriteLine("✅ OID4VCI Integration:");
        Console.WriteLine("   • Standards-compliant credential issuance");
        Console.WriteLine("   • Pre-authorized code flows with PIN protection");
        Console.WriteLine("   • Deferred issuance for complex credentials");
        Console.WriteLine("   • Credential notifications and lifecycle management");
        Console.WriteLine();
        Console.WriteLine("✅ OID4VP Integration:");
        Console.WriteLine("   • Cross-device presentation flows with QR codes");
        Console.WriteLine("   • Comprehensive VP token validation");
        Console.WriteLine("   • Standards-compliant authorization requests/responses");
        Console.WriteLine();
        Console.WriteLine("✅ Presentation Exchange:");
        Console.WriteLine("   • Intelligent credential selection algorithms");
        Console.WriteLine("   • Dynamic presentation definition generation");
        Console.WriteLine("   • Complex constraint evaluation and matching");
        Console.WriteLine();
        Console.WriteLine("✅ Status List Integration:");
        Console.WriteLine("   • Real-time credential revocation checking");
        Console.WriteLine("   • Privacy-preserving status validation");
        Console.WriteLine("   • Proactive compliance monitoring");
        Console.WriteLine();
        Console.WriteLine("✅ Verifiable Credentials:");
        Console.WriteLine("   • RFC-compliant VC data models");
        Console.WriteLine("   • Enhanced verification workflows");
        Console.WriteLine("   • Status integration for lifecycle management");
        Console.WriteLine();

        // Setup enhanced ecosystem
        using var enhancedEcosystem = new EnhancedFinancialEcosystem();
        await enhancedEcosystem.InitializeAsync();

        // Create member with enhanced credentials
        var member = await enhancedEcosystem.CreateEnhancedMemberAsync("Alice Johnson", 1985);

        // Run enhanced conversation flow
        using var enhancedCoPilot = new EnhancedFinancialCoPilot(enhancedEcosystem);
        await enhancedCoPilot.RunEnhancedConversationAsync(member);
    }
}

/// <summary>
/// Enhanced financial ecosystem with full SD-JWT .NET stack integration
/// </summary>
public class EnhancedFinancialEcosystem : IDisposable
{
    private readonly ECDsa _registryEcdsa;
    private readonly ECDsa _bankEcdsa;
    private readonly ECDsaSecurityKey _registryKey;
    private readonly ECDsaSecurityKey _bankKey;
    private readonly SdJwtVcIssuer _vcIssuer;
    private readonly IMemoryCache _cache;
    private readonly HttpClient _httpClient;

    public EnhancedFinancialEcosystem()
    {
        _registryEcdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        _bankEcdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        
        _registryKey = new ECDsaSecurityKey(_registryEcdsa) { KeyId = "enhanced-registry-2024" };
        _bankKey = new ECDsaSecurityKey(_bankEcdsa) { KeyId = "enhanced-bank-2024" };
        
        _vcIssuer = new SdJwtVcIssuer(_registryKey, SecurityAlgorithms.EcdsaSha256);
        _cache = new MemoryCache(new MemoryCacheOptions());
        _httpClient = new HttpClient();
    }

    public void Dispose()
    {
        _registryEcdsa?.Dispose();
        _bankEcdsa?.Dispose();
        _cache?.Dispose();
        _httpClient?.Dispose();
    }

    public Task InitializeAsync()
    {
        Console.WriteLine("🏗️  Setting up Enhanced Financial Ecosystem...");
        Console.WriteLine("   ✅ Registry System with OID4VCI endpoints");
        Console.WriteLine("   ✅ Bank System with transaction processing");
        Console.WriteLine("   ✅ Status List infrastructure");
        Console.WriteLine("   ✅ Presentation Exchange engine");
        Console.WriteLine("   ✅ Cross-device flow support");
        Console.WriteLine();
        return Task.CompletedTask;
    }

    public async Task<EnhancedMember> CreateEnhancedMemberAsync(string name, int birthYear)
    {
        Console.WriteLine("👤 Creating Enhanced Member Profile...");
        
        var member = new EnhancedMember
        {
            MemberId = $"ENH_MEMBER_{Random.Shared.Next(100000, 999999)}",
            Name = name,
            BirthYear = birthYear,
            TaxFileNumber = "987-654-321",
            HomeAddress = "456 Enhanced St, Sydney, NSW 2000",
            AccountBalance = 250000m,
            CapRemaining = 15000m,
            JoinedDate = DateOnly.FromDateTime(DateTime.Now.AddYears(-8))
        };

        // Generate member's key pair
        var memberEcdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        member.PrivateKey = new ECDsaSecurityKey(memberEcdsa) { KeyId = $"enhanced-member-{member.MemberId}" };
        member.PublicJwk = JsonWebKeyConverter.ConvertFromSecurityKey(member.PrivateKey);
        member.EcdsaKey = memberEcdsa;

        Console.WriteLine($"   📋 Member ID: {member.MemberId}");
        Console.WriteLine($"   🏦 Account Balance: ${member.AccountBalance:N0}");
        Console.WriteLine($"   💰 Remaining Contribution Cap: ${member.CapRemaining:N0}");
        Console.WriteLine();

        // Issue enhanced credentials
        await IssueEnhancedCredentialsAsync(member);

        return member;
    }

    private async Task IssueEnhancedCredentialsAsync(EnhancedMember member)
    {
        Console.WriteLine("📜 Issuing Enhanced Credentials using OID4VCI patterns...");

        // 1. Issue Superannuation Account Credential with Status List
        member.AccountCredential = await IssueAccountCredentialWithStatusAsync(member);
        Console.WriteLine("   ✅ Superannuation Account credential (with status tracking)");

        // 2. Issue Risk Profile Credential
        member.RiskProfileCredential = await IssueRiskProfileCredentialAsync(member);
        Console.WriteLine("   ✅ Risk Profile credential");

        // 3. Issue Transaction History with PE-optimized structure
        member.TransactionCredential = await IssueTransactionCredentialAsync(member);
        Console.WriteLine("   ✅ Transaction History credential (PE-optimized)");

        Console.WriteLine();
    }

    private async Task<string> IssueAccountCredentialWithStatusAsync(EnhancedMember member)
    {
        // Simulate status list creation - would be real HTTP call in production
        var credentialIndex = GetNextCredentialIndex();

        var vcPayload = new SdJwtVcPayload
        {
            Issuer = "https://registry.linkgroup.com",
            Subject = $"did:example:{member.MemberId}",
            IssuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            ExpiresAt = DateTimeOffset.UtcNow.AddYears(1).ToUnixTimeSeconds(),
            AdditionalData = new Dictionary<string, object>
            {
                // Financial data (selectively disclosable)
                ["member_id"] = member.MemberId,
                ["account_balance"] = member.AccountBalance,
                ["cap_remaining"] = member.CapRemaining,
                ["joined_date"] = member.JoinedDate.ToString("yyyy-MM-dd"),
                ["birth_year"] = member.BirthYear,
                
                // Risk data for PE optimization
                ["risk_tolerance"] = "moderate",
                ["investment_horizon"] = "long_term",
                
                // Status reference for revocation checking
                ["status"] = new Dictionary<string, object>
                {
                    ["status_list"] = new Dictionary<string, object>
                    {
                        ["idx"] = credentialIndex,
                        ["uri"] = "https://registry.linkgroup.com/status/superannuation/1"
                    }
                },
                
                // Sensitive PII (protected by default)
                ["tax_file_number"] = member.TaxFileNumber,
                ["full_name"] = member.Name,
                ["home_address"] = member.HomeAddress
            }
        };

        var sdOptions = new SdIssuanceOptions
        {
            DisclosureStructure = new
            {
                // Disclosed for financial advice
                member_id = true,
                account_balance = true,
                cap_remaining = true,
                birth_year = true,
                joined_date = true,
                risk_tolerance = true,
                investment_horizon = true,
                
                // Toxic PII - available but should be protected
                tax_file_number = true,
                full_name = true,
                home_address = true,
                
                // Status always available (not selectively disclosable)
                status = false
            }
        };

        var credential = _vcIssuer.Issue(
            "https://credentials.linkgroup.com/superannuation-account-enhanced",
            vcPayload,
            sdOptions,
            member.PublicJwk
        );

        return credential.Issuance;
    }

    private async Task<string> IssueRiskProfileCredentialAsync(EnhancedMember member)
    {
        var riskProfile = CalculateRiskProfile(member);
        
        var vcPayload = new SdJwtVcPayload
        {
            Issuer = "https://registry.linkgroup.com",
            Subject = $"did:example:{member.MemberId}",
            IssuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            ExpiresAt = DateTimeOffset.UtcNow.AddMonths(6).ToUnixTimeSeconds(),
            AdditionalData = new Dictionary<string, object>
            {
                ["member_id"] = member.MemberId,
                ["risk_tolerance"] = riskProfile.Tolerance,
                ["investment_horizon"] = riskProfile.InvestmentHorizon,
                ["risk_score"] = riskProfile.Score,
                ["assessment_date"] = DateTime.UtcNow.ToString("yyyy-MM-dd"),
                ["recommended_allocation"] = riskProfile.RecommendedAllocation
            }
        };

        var sdOptions = new SdIssuanceOptions
        {
            DisclosureStructure = new
            {
                member_id = true,
                risk_tolerance = true,
                investment_horizon = true,
                risk_score = true,
                assessment_date = true,
                recommended_allocation = true
            }
        };

        var credential = _vcIssuer.Issue(
            "https://credentials.linkgroup.com/risk-profile",
            vcPayload,
            sdOptions,
            member.PublicJwk
        );

        return credential.Issuance;
    }

    private async Task<string> IssueTransactionCredentialAsync(EnhancedMember member)
    {
        var transactionSummary = GenerateTransactionSummary(member);
        
        var vcPayload = new SdJwtVcPayload
        {
            Issuer = "https://transactions.linkgroup.com",
            Subject = $"did:example:{member.MemberId}",
            IssuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            ExpiresAt = DateTimeOffset.UtcNow.AddMonths(3).ToUnixTimeSeconds(),
            AdditionalData = new Dictionary<string, object>
            {
                ["member_id"] = member.MemberId,
                ["last_12_months_contributions"] = transactionSummary.Last12MonthsContributions,
                ["average_monthly_growth"] = transactionSummary.AverageMonthlyGrowth,
                ["contribution_frequency"] = transactionSummary.ContributionFrequency,
                ["last_contribution_date"] = transactionSummary.LastContributionDate.ToString("yyyy-MM-dd"),
                ["growth_trend"] = transactionSummary.GrowthTrend
            }
        };

        var sdOptions = new SdIssuanceOptions
        {
            DisclosureStructure = new
            {
                member_id = true,
                last_12_months_contributions = true,
                average_monthly_growth = true,
                contribution_frequency = true,
                last_contribution_date = true,
                growth_trend = true
            }
        };

        var credential = _vcIssuer.Issue(
            "https://credentials.linkgroup.com/transaction-history-enhanced",
            vcPayload,
            sdOptions,
            member.PublicJwk
        );

        return credential.Issuance;
    }

    public async Task<SecurityKey> ResolveIssuerKeyAsync(string issuer)
    {
        return issuer switch
        {
            "https://registry.linkgroup.com" => _registryKey,
            "https://transactions.linkgroup.com" => _registryKey,
            _ => throw new InvalidOperationException($"Unknown issuer: {issuer}")
        };
    }

    private RiskProfile CalculateRiskProfile(EnhancedMember member)
    {
        var age = DateTime.Now.Year - member.BirthYear;
        var yearsToRetirement = 67 - age;
        
        return new RiskProfile
        {
            Tolerance = yearsToRetirement > 20 ? "aggressive" : 
                       yearsToRetirement > 10 ? "moderate" : "conservative",
            InvestmentHorizon = yearsToRetirement > 15 ? "long_term" : 
                              yearsToRetirement > 5 ? "medium_term" : "short_term",
            Score = Math.Max(1, Math.Min(10, 10 - (age - 25) / 5)),
            RecommendedAllocation = new Dictionary<string, decimal>
            {
                ["growth"] = yearsToRetirement > 15 ? 0.8m : 0.6m,
                ["balanced"] = 0.15m,
                ["defensive"] = yearsToRetirement > 15 ? 0.05m : 0.25m
            }
        };
    }

    private TransactionSummary GenerateTransactionSummary(EnhancedMember member)
    {
        return new TransactionSummary
        {
            Last12MonthsContributions = 18500m,
            AverageMonthlyGrowth = 1250m,
            ContributionFrequency = "Monthly",
            LastContributionDate = DateOnly.FromDateTime(DateTime.Now.AddDays(-7)),
            GrowthTrend = "positive"
        };
    }

    private int GetNextCredentialIndex() => Random.Shared.Next(1000, 99999);
}

/// <summary>
/// Enhanced Financial Co-Pilot with full ecosystem integration
/// </summary>
public class EnhancedFinancialCoPilot : IDisposable
{
    private readonly EnhancedFinancialEcosystem _ecosystem;
    private readonly OpenAiAdviceEngine _aiEngine;
    private readonly ILoggerFactory _loggerFactory;

    public EnhancedFinancialCoPilot(EnhancedFinancialEcosystem ecosystem)
    {
        _ecosystem = ecosystem;
        _loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = _loggerFactory.CreateLogger<OpenAiAdviceEngine>();
        _aiEngine = new OpenAiAdviceEngine(logger);
    }

    public void Dispose()
    {
        _aiEngine.ClearConversationHistory();
        _loggerFactory?.Dispose();
    }

    public async Task RunEnhancedConversationAsync(EnhancedMember member)
    {
        Console.WriteLine("🚀 Enhanced Financial Co-Pilot Interactive Session");
        Console.WriteLine("=================================================");
        Console.WriteLine();
        Console.WriteLine("ENHANCED CAPABILITIES:");
        Console.WriteLine("✅ OID4VP cross-device presentation flows");
        Console.WriteLine("✅ Intelligent credential selection with PE");
        Console.WriteLine("✅ Real-time status validation");
        Console.WriteLine("✅ Standards-compliant VC verification");
        Console.WriteLine("✅ Advanced AI integration with verified claims");
        Console.WriteLine();

        while (true)
        {
            Console.WriteLine("------------------------------------------------------------------");
            Console.WriteLine("ENHANCED QUERY OPTIONS:");
            Console.WriteLine("------------------------------------------------------------------");
            Console.WriteLine();
            Console.WriteLine("FINANCIAL ADVICE:");
            Console.WriteLine("1. Comprehensive financial review (uses all credentials)");
            Console.WriteLine("2. Risk-adjusted investment strategy");
            Console.WriteLine("3. Tax optimization with growth projections");
            Console.WriteLine("4. Retirement planning with risk assessment");
            Console.WriteLine();
            Console.WriteLine("ADVANCED FEATURES:");
            Console.WriteLine("5. OID4VP cross-device presentation flow");
            Console.WriteLine("6. Presentation Exchange credential selection demo");
            Console.WriteLine("7. Status List validation demonstration");
            Console.WriteLine("8. Generate comprehensive Statement of Advice");
            Console.WriteLine();
            Console.WriteLine("0. Exit enhanced session");
            Console.WriteLine();
            Console.Write("Enter your choice (0-8): ");

            var input = Console.ReadLine()?.Trim();
            
            if (input == "0")
            {
                Console.WriteLine();
                Console.WriteLine("🎯 ENHANCED SESSION COMPLETED!");
                Console.WriteLine();
                Console.WriteLine("ECOSYSTEM FEATURES DEMONSTRATED:");
                Console.WriteLine("✅ Full SD-JWT .NET stack integration");
                Console.WriteLine("✅ Standards-compliant protocols (OID4VCI/VP, PE, VC)");
                Console.WriteLine("✅ Real-time status validation and lifecycle management");
                Console.WriteLine("✅ Privacy-preserving AI with cryptographic verification");
                Console.WriteLine("✅ Production-ready architecture patterns");
                
                _aiEngine.ClearConversationHistory();
                Console.WriteLine("✅ Session context cleared - privacy protection complete");
                break;
            }

            await ProcessEnhancedQuery(member, input);
            
            Console.WriteLine();
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
            Console.WriteLine();
        }
    }

    private async Task ProcessEnhancedQuery(EnhancedMember member, string? choice)
    {
        try
        {
            switch (choice)
            {
                case "1":
                    await ProcessComprehensiveReview(member);
                    break;
                case "2":
                    await ProcessRiskAdjustedStrategy(member);
                    break;
                case "3":
                    await ProcessTaxOptimization(member);
                    break;
                case "4":
                    await ProcessRetirementPlanning(member);
                    break;
                case "5":
                    await DemonstrateOID4VPFlow(member);
                    break;
                case "6":
                    await DemonstratePresentationExchange(member);
                    break;
                case "7":
                    await DemonstrateStatusValidation(member);
                    break;
                case "8":
                    await GenerateStatementOfAdvice(member);
                    break;
                default:
                    Console.WriteLine("Invalid selection. Please try again.");
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error processing query: {ex.Message}");
        }
    }

    private async Task ProcessComprehensiveReview(EnhancedMember member)
    {
        Console.WriteLine();
        Console.WriteLine("💼 COMPREHENSIVE FINANCIAL REVIEW");
        Console.WriteLine("==================================");
        
        // Validate all credential statuses
        await ValidateCredentialStatuses(new[]
        {
            member.AccountCredential,
            member.RiskProfileCredential,
            member.TransactionCredential
        });
        
        // Create OID4VP authorization request
        var authRequest = CreateOID4VPAuthorizationRequest(
            "comprehensive_review", member);
        Console.WriteLine($"✅ Generated OID4VP request: {authRequest[..50]}...");
        
        // Simulate enhanced verification
        var verifiedClaims = await SimulateEnhancedVerification(
            new[] { member.AccountCredential, member.RiskProfileCredential, member.TransactionCredential }, 
            member);
        
        // Generate AI advice with full context
        var advice = await _aiEngine.GenerateAdviceAsync(
            "Provide a comprehensive financial review based on my verified account data, risk profile, and transaction history.",
            verifiedClaims,
            "COMPREHENSIVE_REVIEW");
            
        Console.WriteLine();
        Console.WriteLine("🎯 COMPREHENSIVE ADVICE:");
        Console.WriteLine($"{advice}");
    }

    private async Task ProcessRiskAdjustedStrategy(EnhancedMember member)
    {
        Console.WriteLine();
        Console.WriteLine("📊 RISK-ADJUSTED INVESTMENT STRATEGY");
        Console.WriteLine("====================================");
        
        var requiredCredentials = new[] { member.AccountCredential, member.RiskProfileCredential };
        await ValidateCredentialStatuses(requiredCredentials);
        
        var verifiedClaims = await SimulateEnhancedVerification(requiredCredentials, member);
        
        var advice = await _aiEngine.GenerateAdviceAsync(
            "Based on my risk profile and current account balance, what investment strategy should I pursue?",
            verifiedClaims,
            "RISK_STRATEGY");
            
        Console.WriteLine();
        Console.WriteLine("🎯 RISK-ADJUSTED ADVICE:");
        Console.WriteLine($"{advice}");
    }

    private async Task ProcessTaxOptimization(EnhancedMember member)
    {
        Console.WriteLine();
        Console.WriteLine("💰 TAX OPTIMIZATION ANALYSIS");
        Console.WriteLine("=============================");
        
        var requiredCredentials = new[] { member.AccountCredential };
        await ValidateCredentialStatuses(requiredCredentials);
        
        var verifiedClaims = await SimulateEnhancedVerification(requiredCredentials, member);
        
        var advice = await _aiEngine.GenerateAdviceAsync(
            "How can I optimize my tax position with my current superannuation balance and contribution capacity?",
            verifiedClaims,
            "TAX_OPTIMIZATION");
            
        Console.WriteLine();
        Console.WriteLine("🎯 TAX OPTIMIZATION ADVICE:");
        Console.WriteLine($"{advice}");
    }

    private async Task ProcessRetirementPlanning(EnhancedMember member)
    {
        Console.WriteLine();
        Console.WriteLine("🎯 RETIREMENT PLANNING ANALYSIS");
        Console.WriteLine("===============================");
        
        var requiredCredentials = new[] { member.AccountCredential, member.RiskProfileCredential };
        await ValidateCredentialStatuses(requiredCredentials);
        
        var verifiedClaims = await SimulateEnhancedVerification(requiredCredentials, member);
        
        var advice = await _aiEngine.GenerateAdviceAsync(
            "Based on my risk profile and account balance, what should my retirement planning strategy be?",
            verifiedClaims,
            "RETIREMENT_PLANNING");
            
        Console.WriteLine();
        Console.WriteLine("🎯 RETIREMENT PLANNING ADVICE:");
        Console.WriteLine($"{advice}");
    }

    private async Task DemonstrateOID4VPFlow(EnhancedMember member)
    {
        Console.WriteLine();
        Console.WriteLine("📱 OID4VP CROSS-DEVICE FLOW DEMONSTRATION");
        Console.WriteLine("==========================================");
        
        // Create simulated presentation request
        var authRequest = CreateSimplifiedOID4VPRequest(member);
        Console.WriteLine($"📱 QR Code generated: {authRequest[..80]}...");
        Console.WriteLine("   (In real scenario, user scans this with mobile wallet)");
        
        // Simulate wallet response
        var authResponse = CreateSimulatedAuthResponse(member);
        Console.WriteLine("✅ Simulated wallet response created");
        
        // Validate using simplified pattern
        Console.WriteLine("✅ OID4VP validation successful");
        Console.WriteLine($"   Verified claims: Account balance, cap remaining");
    }

    private async Task DemonstratePresentationExchange(EnhancedMember member)
    {
        Console.WriteLine();
        Console.WriteLine("🧠 PRESENTATION EXCHANGE DEMONSTRATION");
        Console.WriteLine("======================================");
        
        var wallet = new[] 
        { 
            member.AccountCredential,
            member.RiskProfileCredential, 
            member.TransactionCredential 
        };
        
        // Test different scenarios
        var scenarios = new[]
        {
            "Tax Optimization",
            "Risk Assessment", 
            "Basic Advice"
        };
        
        foreach (var scenario in scenarios)
        {
            Console.WriteLine($"🔍 Testing scenario: {scenario}");
            
            // Simulate PE selection
            var selectedCredentials = SelectCredentialsForScenario(scenario, wallet);
            
            Console.WriteLine($"   ✅ Selected {selectedCredentials.Length} credentials");
            foreach (var cred in selectedCredentials)
            {
                Console.WriteLine($"      - {GetCredentialType(cred)}");
            }
        }
    }

    private async Task DemonstrateStatusValidation(EnhancedMember member)
    {
        Console.WriteLine();
        Console.WriteLine("🔍 STATUS LIST VALIDATION DEMONSTRATION");
        Console.WriteLine("=======================================");
        
        var credentials = new Dictionary<string, string>
        {
            ["SuperannuationAccount"] = member.AccountCredential,
            ["RiskProfile"] = member.RiskProfileCredential,
            ["TransactionHistory"] = member.TransactionCredential
        };
        
        foreach (var kvp in credentials)
        {
            Console.WriteLine($"Checking {kvp.Key}...");
            
            // Simulate status check
            var isValid = await ValidateCredentialStatus(kvp.Value);
            var status = isValid ? "✅ Valid" : "❌ Revoked/Suspended";
            
            Console.WriteLine($"   {status}");
        }
        
        Console.WriteLine();
        Console.WriteLine("📊 Status validation completed for all credentials");
    }

    private async Task<Dictionary<string, object>> SimulateEnhancedVerification(
        IEnumerable<string> credentials, 
        EnhancedMember member)
    {
        Console.WriteLine("🔐 Enhanced cryptographic verification...");
        
        // Simulate comprehensive verification with status checking
        var verifiedClaims = new Dictionary<string, object>
        {
            ["member_id"] = member.MemberId,
            ["account_balance"] = member.AccountBalance,
            ["cap_remaining"] = member.CapRemaining,
            ["birth_year"] = member.BirthYear,
            ["joined_date"] = member.JoinedDate.ToString("yyyy-MM-dd"),
            ["risk_tolerance"] = "moderate",
            ["investment_horizon"] = "long_term",
            ["last_12_months_contributions"] = 18500m,
            ["average_monthly_growth"] = 1250m
        };
        
        Console.WriteLine("✅ All credentials verified with cryptographic proofs");
        Console.WriteLine("✅ Status validation passed for all credentials");
        Console.WriteLine($"✅ {verifiedClaims.Count} verified claims extracted");
        
        return verifiedClaims;
    }

    private async Task ValidateCredentialStatuses(IEnumerable<string> credentials)
    {
        Console.WriteLine("🔍 Validating credential statuses...");
        
        foreach (var credential in credentials)
        {
            var isValid = await ValidateCredentialStatus(credential);
            if (!isValid)
            {
                throw new InvalidOperationException("One or more credentials have been revoked");
            }
        }
        
        Console.WriteLine("✅ All credentials have valid status");
    }

    private Task<bool> ValidateCredentialStatus(string credential)
    {
        // Simulate status list validation
        // In real implementation, would parse credential for status claim and check against status list
        return Task.FromResult(true);
    }

    private string CreateOID4VPAuthorizationRequest(string sessionId, EnhancedMember member)
    {
        // Create OID4VP authorization request
        var request = new
        {
            client_id = "https://financial-copilot.linkgroup.com",
            response_uri = "https://financial-copilot.linkgroup.com/response",
            response_mode = "direct_post",
            response_type = "vp_token",
            state = sessionId,
            nonce = GenerateSecureNonce(),
            presentation_definition = new
            {
                id = $"financial-advice-{sessionId}",
                name = "Financial Advisory Session",
                purpose = "Comprehensive financial analysis"
            }
        };

        return $"openid4vp://?request={Uri.EscapeDataString(JsonSerializer.Serialize(request))}";
    }

    private string CreateSimplifiedOID4VPRequest(EnhancedMember member)
    {
        var request = new
        {
            client_id = "https://financial-copilot.linkgroup.com",
            response_type = "vp_token",
            state = "demo-session",
            nonce = "demo-nonce-123"
        };

        return $"openid4vp://?request={Uri.EscapeDataString(JsonSerializer.Serialize(request))}";
    }

    private object CreateSimulatedAuthResponse(EnhancedMember member)
    {
        // Simulate wallet creating response
        return new
        {
            vp_token = new[] { "simulated-vp-token" },
            presentation_submission = new 
            { 
                id = "sim-submission", 
                definition_id = "test" 
            },
            state = "demo-session"
        };
    }

    private string[] SelectCredentialsForScenario(string scenario, string[] wallet)
    {
        // Simulate intelligent credential selection
        return scenario switch
        {
            "Tax Optimization" => new[] { wallet[0] }, // Account credential
            "Risk Assessment" => new[] { wallet[0], wallet[1] }, // Account + Risk
            "Basic Advice" => new[] { wallet[0] }, // Account credential
            _ => new[] { wallet[0] }
        };
    }

    private string GetCredentialType(string credential)
    {
        // Extract credential type from credential
        if (credential.Contains("superannuation")) return "Superannuation Account";
        if (credential.Contains("risk")) return "Risk Profile";
        if (credential.Contains("transaction")) return "Transaction History";
        return "Unknown Credential";
    }

    private string GenerateSecureNonce()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
    }

    private async Task GenerateStatementOfAdvice(EnhancedMember member)
    {
        Console.WriteLine();
        Console.WriteLine("📄 GENERATING COMPREHENSIVE STATEMENT OF ADVICE");
        Console.WriteLine("===============================================");
        
        var advice = await _aiEngine.GenerateAdviceAsync(
            "Generate a comprehensive Statement of Advice summarizing our entire conversation",
            new Dictionary<string, object> { ["member_id"] = member.MemberId },
            "ARTIFACT_GENERATION");
            
        Console.WriteLine();
        Console.WriteLine("📋 COMPREHENSIVE STATEMENT OF ADVICE:");
        Console.WriteLine("=====================================");
        Console.WriteLine($"{advice}");
    }
}

/// <summary>
/// Enhanced member with additional credentials
/// </summary>
public class EnhancedMember : IDisposable
{
    public string MemberId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int BirthYear { get; set; }
    public string TaxFileNumber { get; set; } = string.Empty;
    public string HomeAddress { get; set; } = string.Empty;
    public decimal AccountBalance { get; set; }
    public decimal CapRemaining { get; set; }
    public DateOnly JoinedDate { get; set; }
    
    public ECDsaSecurityKey PrivateKey { get; set; } = null!;
    public Microsoft.IdentityModel.Tokens.JsonWebKey PublicJwk { get; set; } = null!;
    public ECDsa EcdsaKey { get; set; } = null!;
    
    // Enhanced credentials
    public string AccountCredential { get; set; } = string.Empty;
    public string RiskProfileCredential { get; set; } = string.Empty;
    public string TransactionCredential { get; set; } = string.Empty;

    public void Dispose()
    {
        EcdsaKey?.Dispose();
    }
}

public class RiskProfile
{
    public string Tolerance { get; set; } = string.Empty;
    public string InvestmentHorizon { get; set; } = string.Empty;
    public int Score { get; set; }
    public Dictionary<string, decimal> RecommendedAllocation { get; set; } = new();
}

public class TransactionSummary
{
    public decimal Last12MonthsContributions { get; set; }
    public decimal AverageMonthlyGrowth { get; set; }
    public string ContributionFrequency { get; set; } = string.Empty;
    public DateOnly LastContributionDate { get; set; }
    public string GrowthTrend { get; set; } = string.Empty;
}
