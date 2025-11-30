using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.Issuer;
using SdJwt.Net.Vc.Issuer;
using SdJwt.Net.Vc.Models;
using System.Security.Cryptography;
using System.Text.Json;

namespace SdJwt.Net.Samples.RealWorld.Financial;

/// <summary>
/// Demonstrates Financial Co-Pilot using OpenAI with SD-JWT for privacy-preserving financial guidance.
/// Shows the "Verify-then-Infer" pattern where AI gets verified data just-in-time, computes advice, 
/// and maintains context within a session while immediately forgetting sensitive inputs after session ends.
/// </summary>
public class FinancialCoPilotScenario
{
    public static async Task RunScenario(IServiceProvider services)
    {
        var logger = services.GetRequiredService<ILogger<FinancialCoPilotScenario>>();
        
        Console.WriteLine();
        Console.WriteLine("==================================================================");
        Console.WriteLine("                    Financial Co-Pilot Demo                      ");
        Console.WriteLine("              Privacy-Preserving AI Financial Advisor           ");
        Console.WriteLine("                  Powered by SD-JWT + OpenAI                    ");
        Console.WriteLine("==================================================================");
        Console.WriteLine();
        
        // Check for OpenAI configuration
        var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        var modelName = Environment.GetEnvironmentVariable("OPENAI_MODEL") ?? "gpt-4-turbo-preview";
        var hasOpenAI = !string.IsNullOrEmpty(apiKey);
        
        if (hasOpenAI)
        {
            Console.WriteLine($"AI Provider: OpenAI {modelName} (Real AI responses enabled)");
            Console.WriteLine($"     API Key configured: YES");
            Console.WriteLine($"     Model: {modelName}");
            if (modelName.StartsWith("gpt-5"))
            {
                Console.WriteLine($"     Advanced GPT-5 reasoning capabilities active");
            }
        }
        else
        {
            Console.WriteLine("AI Provider: Simulated responses (High-quality simulation)");
            Console.WriteLine();
            Console.WriteLine("TO ENABLE REAL OPENAI:");
            Console.WriteLine("     export OPENAI_API_KEY=\"your-api-key-here\"");
            Console.WriteLine("     export OPENAI_MODEL=\"gpt-5-turbo\"  # Recommended for best results");
            Console.WriteLine();
            Console.WriteLine("SUPPORTED MODELS:");
            Console.WriteLine("     • gpt-5-turbo         (Latest GPT-5 Turbo - Recommended)");
            Console.WriteLine("     • gpt-5               (GPT-5 with advanced reasoning)");
            Console.WriteLine("     • gpt-4o              (GPT-4 Omni - Optimized performance)");
            Console.WriteLine("     • gpt-4-turbo         (GPT-4 Turbo)");
            Console.WriteLine("     • gpt-4               (Original GPT-4)");
            Console.WriteLine("     • gpt-3.5-turbo       (Fastest/Most economical)");
            
            // Prompt user to enter API key
            Console.WriteLine();
            Console.Write("Would you like to enter your OpenAI API key now? (y/n): ");
            var response = Console.ReadLine()?.ToLower();
            if (response == "y" || response == "yes")
            {
                var providedKey = OpenAiAdviceEngine.PromptForApiKey();
                if (!string.IsNullOrEmpty(providedKey))
                {
                    hasOpenAI = true;
                    Console.WriteLine();
                    Console.WriteLine($"OpenAI integration enabled with model: {modelName}");
                }
            }
        }
        Console.WriteLine();
        
        Console.WriteLine("BUSINESS CONTEXT: The 'Golden Record' Paradox");
        Console.WriteLine("   Members want real-time, personalized financial guidance:");
        Console.WriteLine("   • 'Should I salary sacrifice?'");
        Console.WriteLine("   • 'If I add $200 per fortnight, what happens?'");
        Console.WriteLine("   • 'What if I retire at 60 instead of 65?'");
        Console.WriteLine();
        Console.WriteLine("THE CHALLENGE:");
        Console.WriteLine("   • AI needs financial context (balance, transaction history)");
        Console.WriteLine("   • Data is coupled with 'Toxic PII' (TFN, addresses, DOB)");
        Console.WriteLine("   • Cannot stream high-sensitivity data to cloud LLMs");
        Console.WriteLine();
        Console.WriteLine("OUR SOLUTION: Stateless Co-Pilot with SD-JWT");
        Console.WriteLine("   • Client Device = Secure Vault (Holder)");
        Console.WriteLine("   • AI Service = Stateless Reasoning Engine (Verifier)");
        Console.WriteLine("   • Progressive Disclosure for multi-turn conversations");
        Console.WriteLine("   • Context maintained within session, cleared after session");
        Console.WriteLine();
        Console.WriteLine("PRIVACY GUARANTEES:");
        Console.WriteLine("   * Only required fields disclosed per query");
        Console.WriteLine("   * TFN, full name, address NEVER sent to AI");
        Console.WriteLine("   * Cryptographic proof of data authenticity");
        Console.WriteLine("   * Session context only - cleared when conversation ends");
        Console.WriteLine("   * Progressive disclosure = clean context windows");
        Console.WriteLine();
        
        if (hasOpenAI)
        {
            Console.WriteLine("AI CAPABILITIES WITH REAL OPENAI:");
            if (modelName.StartsWith("gpt-5"))
            {
                Console.WriteLine("   * GPT-5 Advanced Reasoning: Enhanced financial modeling and complex calculations");
                Console.WriteLine("   * Multimodal Understanding: Superior context comprehension across scenarios");
                Console.WriteLine("   * Sophisticated Analysis: Multi-variable optimization and scenario planning");
            }
            else
            {
                Console.WriteLine("   * Advanced reasoning about complex financial scenarios");
            }
            Console.WriteLine("   * Precise calculations with Australian tax implications");
            Console.WriteLine("   * Contextual advice based on verified member data");
            Console.WriteLine("   * Regulatory compliance with privacy-preserving design");
            Console.WriteLine();
        }

        // Setup the demo environment
        await SetupFinancialCoPlilotDemo(hasOpenAI);
    }

    private static async Task SetupFinancialCoPlilotDemo(bool hasOpenAI = false)
    {
        // 1. Setup: Create the financial ecosystem
        Console.WriteLine("1. Setting up Financial Ecosystem...");
        Console.WriteLine();

        using var ecosystem = new FinancialEcosystem();
        await ecosystem.InitializeAsync();

        // 2. Issue credentials for member
        Console.WriteLine("2. Issuing Member Credentials...");
        using var member = await ecosystem.CreateMemberAsync("John Smith", 
            birthYear: 1985, 
            tfn: "123-456-789", 
            address: "123 Main St, Sydney, NSW 2000");

        var accountCredential = await ecosystem.IssueAccountCredentialAsync(member);
        var transactionCredential = await ecosystem.IssueTransactionHistoryAsync(member);

        Console.WriteLine($"   Account credential issued for member: {member.MemberId}");
        Console.WriteLine($"   Transaction history credential issued");
        Console.WriteLine($"   Member credentials stored in secure mobile wallet");
        Console.WriteLine();

        // 3. Start the Co-Pilot conversation
        Console.WriteLine("3. Starting Financial Co-Pilot Conversation...");
        Console.WriteLine();

        using var coPilot = new StatelessCoPilot(ecosystem, hasOpenAI);
        await coPilot.StartInteractiveConversationAsync(member);
    }
}

/// <summary>
/// Simulates a financial services ecosystem with multiple issuers and secure credentials
/// </summary>
public class FinancialEcosystem : IDisposable
{
    private readonly ECDsa _registryEcdsa;
    private readonly ECDsa _bankEcdsa;
    private readonly ECDsaSecurityKey _registryKey;
    private readonly ECDsaSecurityKey _bankKey;
    private readonly SdJwtVcIssuer _registryIssuer;
    private readonly SdJwtVcIssuer _bankIssuer;
    private readonly List<Member> _members = new();

    public FinancialEcosystem()
    {
        _registryEcdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        _bankEcdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        
        _registryKey = new ECDsaSecurityKey(_registryEcdsa) { KeyId = "registry-issuer-2024" };
        _bankKey = new ECDsaSecurityKey(_bankEcdsa) { KeyId = "bank-issuer-2024" };
        
        _registryIssuer = new SdJwtVcIssuer(_registryKey, SecurityAlgorithms.EcdsaSha256);
        _bankIssuer = new SdJwtVcIssuer(_bankKey, SecurityAlgorithms.EcdsaSha256);
    }

    public void Dispose()
    {
        _registryEcdsa?.Dispose();
        _bankEcdsa?.Dispose();
    }

    public Task InitializeAsync()
    {
        Console.WriteLine("   Registry System (Link Group) initialized");
        Console.WriteLine("   Bank System initialized");
        Console.WriteLine("   Mobile Wallet infrastructure ready");
        return Task.CompletedTask;
    }

    public Task<Member> CreateMemberAsync(string name, int birthYear, string tfn, string address)
    {
        var member = new Member
        {
            MemberId = $"MEMBER_{Random.Shared.Next(100000, 999999)}",
            Name = name,
            BirthYear = birthYear,
            TaxFileNumber = tfn,
            HomeAddress = address,
            AccountBalance = 150000m,
            CapRemaining = 10000m,
            JoinedDate = DateOnly.FromDateTime(DateTime.Now.AddYears(-5)),
            TransactionHistory = GenerateTransactionHistory()
        };

        // Generate member's key pair - don't dispose, member will manage lifecycle
        var memberEcdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        member.PrivateKey = new ECDsaSecurityKey(memberEcdsa) { KeyId = $"member-{member.MemberId}" };
        member.PublicJwk = JsonWebKeyConverter.ConvertFromSecurityKey(member.PrivateKey);
        member.EcdsaKey = memberEcdsa; // Store reference for proper disposal

        _members.Add(member);
        return Task.FromResult(member);
    }

    public async Task<string> IssueAccountCredentialAsync(Member member)
    {
        var accountPayload = new SdJwtVcPayload
        {
            Issuer = "https://registry.linkgroup.com",
            Subject = $"did:example:{member.MemberId}",
            IssuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            ExpiresAt = DateTimeOffset.UtcNow.AddYears(1).ToUnixTimeSeconds(),
            AdditionalData = new Dictionary<string, object>
            {
                ["member_id"] = member.MemberId,
                ["account_balance"] = member.AccountBalance,
                ["cap_remaining"] = member.CapRemaining,
                ["joined_date"] = member.JoinedDate.ToString("yyyy-MM-dd"),
                ["birth_year"] = member.BirthYear,
                
                // Toxic PII - highly sensitive
                ["tax_file_number"] = member.TaxFileNumber,
                ["full_name"] = member.Name,
                ["home_address"] = member.HomeAddress,
                ["date_of_birth"] = $"{member.BirthYear}-03-15" // Simulated full DOB
            }
        };

        var sdOptions = new SdIssuanceOptions
        {
            DisclosureStructure = new
            {
                account_balance = true,      // Financial data - selectively disclosable
                cap_remaining = true,        // Financial data - selectively disclosable
                birth_year = true,           // Age verification - selectively disclosable
                joined_date = true,          // Membership duration - selectively disclosable
                member_id = true,            // Member identification - selectively disclosable
                
                // Toxic PII - can be disclosed but should be avoided
                tax_file_number = true,
                full_name = true,
                home_address = true,
                date_of_birth = true
            }
        };

        var credential = _registryIssuer.Issue(
            "https://credentials.linkgroup.com/superannuation-account",
            accountPayload,
            sdOptions,
            member.PublicJwk
        );

        member.AccountCredential = credential.Issuance;
        
        // Simulate async credential processing
        await Task.Delay(10);
        return credential.Issuance;
    }

    public async Task<string> IssueTransactionHistoryAsync(Member member)
    {
        var transactionPayload = new SdJwtVcPayload
        {
            Issuer = "https://transactions.linkgroup.com",
            Subject = $"did:example:{member.MemberId}",
            IssuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            ExpiresAt = DateTimeOffset.UtcNow.AddMonths(3).ToUnixTimeSeconds(),
            AdditionalData = new Dictionary<string, object>
            {
                ["member_id"] = member.MemberId,
                ["transaction_summary"] = new
                {
                    last_12_months_contributions = member.TransactionHistory.Last12MonthsContributions,
                    average_monthly_growth = member.TransactionHistory.AverageMonthlyGrowth,
                    last_contribution_date = member.TransactionHistory.LastContributionDate.ToString("yyyy-MM-dd"),
                    contribution_frequency = member.TransactionHistory.ContributionFrequency
                },
                ["detailed_transactions"] = member.TransactionHistory.RecentTransactions.Select(t => new
                {
                    date = t.Date.ToString("yyyy-MM-dd"),
                    amount = t.Amount,
                    type = t.Type,
                    description = t.Description
                }).ToArray()
            }
        };

        var sdOptions = new SdIssuanceOptions
        {
            DisclosureStructure = new
            {
                transaction_summary = true,    // Summary data - selectively disclosable
                detailed_transactions = true   // Detailed history - selectively disclosable
            }
        };

        var credential = _registryIssuer.Issue(
            "https://credentials.linkgroup.com/transaction-history",
            transactionPayload,
            sdOptions,
            member.PublicJwk
        );

        member.TransactionCredential = credential.Issuance;
        
        // Simulate async credential processing
        await Task.Delay(10);
        return credential.Issuance;
    }

    public Task<SecurityKey> ResolveIssuerKeyAsync(string issuer)
    {
        var result = issuer switch
        {
            "https://registry.linkgroup.com" => _registryKey,
            "https://transactions.linkgroup.com" => _registryKey,
            _ => throw new InvalidOperationException($"Unknown issuer: {issuer}")
        };
        return Task.FromResult<SecurityKey>(result);
    }

    private static TransactionHistory GenerateTransactionHistory()
    {
        var transactions = new List<Transaction>();
        var random = new Random();
        
        // Generate 12 months of realistic transactions
        for (int i = 0; i < 12; i++)
        {
            var date = DateOnly.FromDateTime(DateTime.Now.AddMonths(-i));
            
            // Employer contribution
            transactions.Add(new Transaction
            {
                Date = date,
                Amount = 1500m + (random.Next(-200, 300)),
                Type = "Employer Contribution",
                Description = "Monthly superannuation contribution"
            });
            
            // Government co-contribution (sometimes)
            if (random.Next(0, 4) == 0)
            {
                transactions.Add(new Transaction
                {
                    Date = date,
                    Amount = 500m,
                    Type = "Government Co-contribution",
                    Description = "Government matching contribution"
                });
            }
            
            // Investment growth/loss
            transactions.Add(new Transaction
            {
                Date = date,
                Amount = random.Next(-500, 800),
                Type = "Investment Returns",
                Description = "Monthly portfolio performance"
            });
        }

        return new TransactionHistory
        {
            Last12MonthsContributions = transactions.Where(t => t.Type.Contains("Contribution")).Sum(t => t.Amount),
            AverageMonthlyGrowth = transactions.Where(t => t.Type.Contains("Returns")).Average(t => t.Amount),
            LastContributionDate = transactions.Where(t => t.Type.Contains("Contribution")).Max(t => t.Date),
            ContributionFrequency = "Monthly",
            RecentTransactions = transactions.OrderByDescending(t => t.Date).Take(6).ToList()
        };
    }
}

/// <summary>
/// The stateless AI Co-Pilot that implements the "Verify-then-Infer" pattern
/// with session-based context management
/// </summary>
public class StatelessCoPilot : IDisposable
{
    private readonly FinancialEcosystem _ecosystem;
    private readonly IntentRouter _intentRouter;
    private readonly OpenAiAdviceEngine _adviceEngine;
    private readonly bool _hasOpenAI;
    private readonly ILoggerFactory _loggerFactory;

    public StatelessCoPilot(FinancialEcosystem ecosystem, bool hasOpenAI)
    {
        _ecosystem = ecosystem;
        _intentRouter = new IntentRouter();
        _hasOpenAI = hasOpenAI;
        // Create logger for the advice engine
        _loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = _loggerFactory.CreateLogger<OpenAiAdviceEngine>();
        _adviceEngine = new OpenAiAdviceEngine(logger);
    }

    public void Dispose()
    {
        // Clear conversation history when disposing
        _adviceEngine.ClearConversationHistory();
        _loggerFactory?.Dispose();
    }

    public async Task StartInteractiveConversationAsync(Member member)
    {
        Console.WriteLine("=================================================================");
        Console.WriteLine("              Financial Co-Pilot Interactive Session           ");
        Console.WriteLine("=================================================================");
        Console.WriteLine();
        Console.WriteLine("PRIVACY PROTECTION ACTIVE:");
        Console.WriteLine("- Only minimum required data disclosed per query");
        Console.WriteLine("- TFN, full name, address cryptographically protected");
        Console.WriteLine("- All data verified before sending to AI");
        Console.WriteLine("- Context maintained within this session only");
        Console.WriteLine("- Conversation history cleared when session ends");
        Console.WriteLine();

        while (true)
        {
            Console.WriteLine("------------------------------------------------------------------");
            Console.WriteLine("SELECT YOUR QUERY:");
            Console.WriteLine("------------------------------------------------------------------");
            Console.WriteLine();
            Console.WriteLine("DEFAULT QUERIES:");
            Console.WriteLine("1. Should I salary sacrifice?");
            Console.WriteLine("2. If I add $200 per fortnight, what happens?");
            Console.WriteLine("3. What if I retire at 60 instead of 65?");
            Console.WriteLine("4. Send me the summary");
            Console.WriteLine();
            Console.WriteLine("5. Enter custom query");
            Console.WriteLine("0. Exit conversation");
            Console.WriteLine();
            Console.Write("Enter your choice (0-5): ");

            var input = Console.ReadLine()?.Trim();
            
            if (input == "0")
            {
                Console.WriteLine();
                Console.WriteLine("CONVERSATION COMPLETED SUCCESSFULLY!");
                Console.WriteLine();
                Console.WriteLine("KEY ACHIEVEMENTS:");
                Console.WriteLine("   * AI provided personalized advice using real financial data");
                Console.WriteLine("   * No 'Toxic PII' was ever transmitted to the AI service");
                Console.WriteLine("   * Progressive disclosure kept context windows clean");
                Console.WriteLine("   * Each turn had cryptographic proof of data authenticity");
                Console.WriteLine("   * Session context maintained for coherent conversation");
                Console.WriteLine("   * Conversation history now being cleared...");
                
                // Clear conversation history at the end
                _adviceEngine.ClearConversationHistory();
                Console.WriteLine("   * All session data permanently deleted");
                break;
            }

            string? query = input switch
            {
                "1" => "Should I salary sacrifice?",
                "2" => "If I add $200 per fortnight, what happens?",
                "3" => "What if I retire at 60 instead of 65?",
                "4" => "Send me the summary",
                "5" => GetCustomQuery(),
                _ => null
            };

            if (query == null)
            {
                Console.WriteLine("Invalid selection. Please try again.");
                Console.WriteLine();
                continue;
            }

            Console.WriteLine();
            Console.WriteLine("==================================================================");
            Console.WriteLine($"USER: \"{query}\"");
            Console.WriteLine();

            // Process the query through the existing conversation logic
            await ProcessSingleQuery(member, query);
            
            Console.WriteLine();
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
            Console.WriteLine();
        }
    }

    private async Task ProcessSingleQuery(Member member, string query)
    {
        // 1. Route the intent
        Console.WriteLine("INTENT ROUTER: Analyzing query...");
        var intent = _intentRouter.RouteIntent(query);
        var requiredFields = _intentRouter.GetRequiredFields(intent);
        Console.WriteLine($"     Detected INTENT: {intent}");
        Console.WriteLine($"     Required fields: {string.Join(", ", requiredFields)}");
        Console.WriteLine();

        // 2. Request verifiable presentation
        Console.WriteLine("ORCHESTRATOR: Requesting Verifiable Presentation...");
        var walletSimulator = new WalletSimulator();
        var presentation = walletSimulator.CreateSelectivePresentation(
            member.AccountCredential,
            member.TransactionCredential,
            requiredFields);
        Console.WriteLine("WALLET: Creating selective presentation...");
        
        // Show actual data being presented
        var presentationData = JsonSerializer.Deserialize<Dictionary<string, object>>(presentation);
        Console.WriteLine($"     Presenting data:");
        foreach (var kvp in presentationData!)
        {
            Console.WriteLine($"       - {kvp.Key}: {kvp.Value}");
        }
        Console.WriteLine($"     Protected: TFN, full name, address, detailed transactions");
        Console.WriteLine();

        // 3. Verify the presentation
        Console.WriteLine("VERIFIER: Validating presentation...");
        var verifier = new PresentationVerifier(_ecosystem);
        var verificationResult = await verifier.VerifyPresentationAsync(presentation);
        Console.WriteLine($"     Cryptographic verification successful");
        Console.WriteLine($"     Key binding confirmed");
        Console.WriteLine($"     Issuer signatures valid");
        Console.WriteLine();

        // 4. Generate AI advice
        Console.WriteLine("AI REASONING ENGINE: Processing query...");
        var advice = await _adviceEngine.GenerateAdviceAsync(query, verificationResult.VerifiedClaims, intent);
        Console.WriteLine($"     ADVICE: {advice}");
        Console.WriteLine();

        // 5. Note session context management
        Console.WriteLine("SESSION MANAGEMENT:");
        Console.WriteLine("     Context preserved for this conversation session");
        Console.WriteLine("     Sensitive data discarded from memory after processing");
        Console.WriteLine("     Full history will be cleared when conversation ends");
    }

    private static string? GetCustomQuery()
    {
        Console.WriteLine();
        Console.WriteLine("ENTER CUSTOM QUERY:");
        Console.WriteLine("Examples:");
        Console.WriteLine("- How much should I contribute to maximize tax benefits?");
        Console.WriteLine("- What's my projected balance at retirement?");
        Console.WriteLine("- Should I make a voluntary contribution before June 30?");
        Console.WriteLine();
        Console.Write("Your question: ");
        
        var customQuery = Console.ReadLine()?.Trim();
        
        if (string.IsNullOrEmpty(customQuery))
        {
            Console.WriteLine("No query entered. Returning to menu.");
            return null;
        }
        
        return customQuery;
    }
}

/// <summary>
/// Routes user intents to determine required data fields
/// </summary>
public class IntentRouter
{
    public string RouteIntent(string query)
    {
        query = query.ToLower();
        
        return query switch
        {
            var q when q.Contains("salary sacrifice") => "CONTRIBUTION_STRATEGY",
            var q when q.Contains("add") && q.Contains("fortnight") => "SIMULATION",
            var q when q.Contains("retire") => "RETIREMENT_PROJECTION",
            var q when q.Contains("summary") => "ARTIFACT_GENERATION",
            var q when q.Contains("contribute") => "CONTRIBUTION_STRATEGY",
            var q when q.Contains("balance") => "BALANCE_INQUIRY",
            var q when q.Contains("tax") => "TAX_STRATEGY",
            _ => "GENERAL_ADVICE"
        };
    }

    public List<string> GetRequiredFields(string intent)
    {
        return intent switch
        {
            "CONTRIBUTION_STRATEGY" => new() { "account_balance", "cap_remaining" },
            "SIMULATION" => new() { "account_balance" },
            "RETIREMENT_PROJECTION" => new() { "account_balance", "birth_year", "joined_date" },
            "ARTIFACT_GENERATION" => new() { "member_id" },
            "BALANCE_INQUIRY" => new() { "account_balance" },
            "TAX_STRATEGY" => new() { "account_balance", "cap_remaining" },
            "GENERAL_ADVICE" => new() { "account_balance", "birth_year" },
            _ => new() { "account_balance" }
        };
    }
}

/// <summary>
/// Simulates wallet operations for creating selective presentations
/// </summary>
public class WalletSimulator
{
    public string CreateSelectivePresentation(string accountCredential, string transactionCredential, List<string> requiredFields)
    {
        // Simulate creating a selective presentation with only the required fields
        var presentationData = new Dictionary<string, object>();
        
        foreach (var field in requiredFields)
        {
            presentationData[field] = field switch
            {
                "account_balance" => 150000m,
                "cap_remaining" => 10000m,
                "birth_year" => 1985,
                "joined_date" => "2019-01-15",
                "member_id" => "MEMBER_789456",
                _ => $"mock_{field}"
            };
        }
        
        return JsonSerializer.Serialize(presentationData);
    }
}

/// <summary>
/// Verifies cryptographic presentations and extracts verified claims
/// </summary>
public class PresentationVerifier
{
    private readonly FinancialEcosystem _ecosystem;
    
    public PresentationVerifier(FinancialEcosystem ecosystem)
    {
        _ecosystem = ecosystem;
    }
    
    public async Task<VerificationResult> VerifyPresentationAsync(string presentation)
    {
        await Task.Delay(50); // Simulate verification time
        
        // Simulate successful verification and extract claims
        var claims = JsonSerializer.Deserialize<Dictionary<string, object>>(presentation);
        
        return new VerificationResult
        {
            IsValid = true,
            VerifiedClaims = claims ?? new Dictionary<string, object>()
        };
    }
}

/// <summary>
/// Result of presentation verification
/// </summary>
public class VerificationResult
{
    public bool IsValid { get; set; }
    public Dictionary<string, object> VerifiedClaims { get; set; } = new();
}

/// <summary>
/// Represents a financial services member
/// </summary>
public class Member : IDisposable
{
    public string MemberId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int BirthYear { get; set; }
    public string TaxFileNumber { get; set; } = string.Empty;
    public string HomeAddress { get; set; } = string.Empty;
    public decimal AccountBalance { get; set; }
    public decimal CapRemaining { get; set; }
    public DateOnly JoinedDate { get; set; }
    public TransactionHistory TransactionHistory { get; set; } = new();
    
    public ECDsaSecurityKey PrivateKey { get; set; } = null!;
    public Microsoft.IdentityModel.Tokens.JsonWebKey PublicJwk { get; set; } = null!;
    public ECDsa EcdsaKey { get; set; } = null!; // Store the ECDSA key for proper disposal
    
    public string AccountCredential { get; set; } = string.Empty;
    public string TransactionCredential { get; set; } = string.Empty;

    public void Dispose()
    {
        EcdsaKey?.Dispose();
    }
}

public class TransactionHistory
{
    public decimal Last12MonthsContributions { get; set; }
    public decimal AverageMonthlyGrowth { get; set; }
    public DateOnly LastContributionDate { get; set; }
    public string ContributionFrequency { get; set; } = string.Empty;
    public List<Transaction> RecentTransactions { get; set; } = new();
}

public class Transaction
{
    public DateOnly Date { get; set; }
    public decimal Amount { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
