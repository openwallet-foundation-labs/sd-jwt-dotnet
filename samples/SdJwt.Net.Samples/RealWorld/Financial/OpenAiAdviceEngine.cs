using Microsoft.Extensions.Logging;
using System.Text.Json;
using OpenAI;
using OpenAI.Chat;

namespace SdJwt.Net.Samples.RealWorld.Financial;

/// <summary>
/// Real OpenAI-powered advice engine that generates actual financial advice using GPT models
/// Supports GPT-4, GPT-4 Turbo, and future models like GPT-5
/// To enable: Set environment variable OPENAI_API_KEY with your OpenAI API key
/// </summary>
public class OpenAiAdviceEngine
{
    private readonly ILogger _logger;
    private readonly OpenAIClient? _openAiClient;
    private readonly string _modelName;
    private readonly bool _isConfigured;
    private readonly List<ConversationTurn> _conversationHistory = new();

    public OpenAiAdviceEngine(ILogger logger)
    {
        _logger = logger;

        // Get configuration from environment variables
        var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        var modelName = Environment.GetEnvironmentVariable("OPENAI_MODEL") ?? "gpt-4o"; // Default to GPT-4o for reliability

        _modelName = modelName;

        if (!string.IsNullOrEmpty(apiKey))
        {
            try
            {
                _openAiClient = new OpenAIClient(apiKey);
                _isConfigured = true;
                _logger.LogInformation("OpenAI client configured successfully with model: {Model}", _modelName);

                // Log available models for user reference
                _logger.LogInformation("Supported models: gpt-5-turbo, gpt-5, gpt-4o, gpt-4-turbo, gpt-4, gpt-3.5-turbo");
                _logger.LogInformation("To use a different model, set OPENAI_MODEL environment variable");

                // GPT-5 support with enhanced capabilities
                if (_modelName.StartsWith("gpt-5"))
                {
                    _logger.LogInformation("GPT-5 model detected: {Model} - Using advanced reasoning and multimodal capabilities", _modelName);
                    _logger.LogInformation("GPT-5 features: Enhanced financial modeling, complex reasoning, improved context understanding");
                }
                else if (_modelName.StartsWith("gpt-4o"))
                {
                    _logger.LogInformation("GPT-4o model detected: {Model} - Using optimized performance with multimodal support", _modelName);
                }
                else if (_modelName.StartsWith("gpt-4"))
                {
                    _logger.LogInformation("GPT-4 model detected: {Model} - Using advanced language understanding", _modelName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize OpenAI client");
                _isConfigured = false;
            }
        }
        else
        {
            _logger.LogInformation("No OpenAI API key found. Using simulated responses");
            _logger.LogInformation("To enable real AI: Set OPENAI_API_KEY environment variable");
            _isConfigured = false;
        }
    }

    public static string? PromptForApiKey()
    {
        Console.WriteLine();
        Console.WriteLine("OPENAI API KEY SETUP");
        Console.WriteLine("====================");
        Console.WriteLine();
        Console.WriteLine("To enable real OpenAI responses, please provide your API key:");
        Console.WriteLine("(You can get one from: https://platform.openai.com/api-keys)");
        Console.WriteLine();
        Console.Write("Enter your OpenAI API key (or press Enter to use simulated responses): ");

        var apiKey = Console.ReadLine()?.Trim();

        if (!string.IsNullOrEmpty(apiKey))
        {
            Environment.SetEnvironmentVariable("OPENAI_API_KEY", apiKey);
            Console.WriteLine("API key set successfully for this session.");
            return apiKey;
        }

        Console.WriteLine("Using simulated AI responses.");
        return null;
    }

    public void AddToConversationHistory(string question, Dictionary<string, object> verifiedData, string intent, string response)
    {
        _conversationHistory.Add(new ConversationTurn
        {
            Question = question,
            VerifiedData = verifiedData,
            Intent = intent,
            Response = response,
            Timestamp = DateTimeOffset.UtcNow
        });
    }

    public void ClearConversationHistory()
    {
        _conversationHistory.Clear();
        _logger.LogInformation("Conversation history cleared - Fresh context window for new session");
    }

    public async Task<string> GenerateAdviceAsync(string question, Dictionary<string, object> verifiedData, string intent)
    {
        // Log the verified data being processed (for transparency)
        var dataJson = JsonSerializer.Serialize(verifiedData, new JsonSerializerOptions
        {
            WriteIndented = true
        });
        _logger.LogInformation("Processing verified financial data for intent: {Intent}", intent);
        _logger.LogDebug("Verified data: {Data}", dataJson);

        if (!_isConfigured || _openAiClient == null)
        {
            _logger.LogInformation("Using simulated AI response (no OpenAI API key configured)");
            var simulatedResponse = GetSimulatedAdvice(question, verifiedData, intent);
            AddToConversationHistory(question, verifiedData, intent, simulatedResponse);
            return simulatedResponse;
        }

        try
        {
            _logger.LogInformation("Calling OpenAI {Model} for financial advice...", _modelName);

            var systemPrompt = BuildSystemPrompt();
            var userPrompt = BuildUserPrompt(question, verifiedData, intent);

            var chatMessages = new List<ChatMessage>
            {
                new SystemChatMessage(systemPrompt),
                new UserChatMessage(userPrompt)
            };

            var chatCompletionOptions = new ChatCompletionOptions
            {
                Temperature = _modelName.StartsWith("gpt-5") ? 0.2f : 0.3f, // Lower temperature for GPT-5's enhanced reasoning
                MaxOutputTokenCount = _modelName.StartsWith("gpt-5") ? 1200 : 800, // More tokens for GPT-5's detailed analysis
                TopP = 0.9f,
                FrequencyPenalty = 0.0f,
                PresencePenalty = 0.0f
            };

            var response = await _openAiClient.GetChatClient(_modelName).CompleteChatAsync(chatMessages, chatCompletionOptions);

            var advice = response.Value.Content[0].Text;

            _logger.LogInformation("Successfully generated AI advice using {Model}", _modelName);
            _logger.LogDebug("AI Response length: {Length} characters", advice.Length);

            AddToConversationHistory(question, verifiedData, intent, advice);
            return advice;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate AI advice from OpenAI, falling back to simulation");
            var simulatedResponse = GetSimulatedAdvice(question, verifiedData, intent);
            AddToConversationHistory(question, verifiedData, intent, simulatedResponse);
            return simulatedResponse;
        }
    }

    private string BuildSystemPrompt()
    {
        var conversationContext = "";
        if (_conversationHistory.Any())
        {
            conversationContext = "\n\nCONVERSATION HISTORY:\n";
            conversationContext += "You have access to this member's previous questions in this session:\n";

            foreach (var turn in _conversationHistory)
            {
                conversationContext += $"- Question: \"{turn.Question}\" -> Intent: {turn.Intent}\n";
                conversationContext += $"  Verified Data Used: {string.Join(", ", turn.VerifiedData.Keys)}\n";
            }

            conversationContext += "\nUse this context to provide coherent, connected advice across the conversation.\n";
        }

        // Enhanced prompt for GPT-5 with advanced reasoning capabilities
        var modelSpecificInstructions = "";
        if (_modelName.StartsWith("gpt-5"))
        {
            modelSpecificInstructions = """
            
            GPT-5 ENHANCED CAPABILITIES:
            - Use your advanced reasoning to provide sophisticated financial modeling
            - Leverage your improved mathematical capabilities for precise calculations
            - Apply complex scenario analysis for retirement planning
            - Consider multiple variables simultaneously in your recommendations
            - Use your enhanced context understanding to provide more nuanced advice
            """;
        }

        return $"""
        You are a professional financial advisor specializing in Australian superannuation and retirement planning.
        
        CRITICAL CONTEXT:
        - You are receiving cryptographically verified financial data via SD-JWT (Selective Disclosure JSON Web Tokens)
        - This data has been selectively disclosed - you only see the minimum information needed for this specific question
        - The data is mathematically proven to be authentic and unmodified
        - You must NOT ask for additional personal information beyond what's provided
        - This is a privacy-preserving AI interaction where sensitive PII is protected
        
        YOUR ROLE:
        - Provide practical, actionable financial advice based solely on the verified data provided
        - Focus on Australian superannuation rules, contribution caps, and tax implications
        - Be specific with calculations and recommendations
        - Acknowledge the privacy-preserving nature of this interaction
        - Mention that your advice is based on cryptographically verified financial data
        - Remember previous questions in this conversation session to provide coherent advice
        
        RESPONSE GUIDELINES:
        - Keep responses concise but comprehensive (3-4 sentences maximum)
        - Include specific dollar amounts and percentages where relevant
        - Mention key dates (like June 30 for contribution caps)
        - Suggest concrete next steps
        - Use Australian financial terminology and tax rates
        - Provide detailed mathematical reasoning for complex scenarios
        
        CONSTRAINTS:
        - Base advice ONLY on the verified data provided
        - Do NOT request additional personal information
        - Remember context within this conversation session only
        - Focus on the specific question asked{modelSpecificInstructions}{conversationContext}
        """;
    }

    private string BuildUserPrompt(string question, Dictionary<string, object> verifiedData, string intent)
    {
        var dataJson = JsonSerializer.Serialize(verifiedData, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        var contextualInfo = intent switch
        {
            "CONTRIBUTION_STRATEGY" => "This is about optimizing superannuation contributions and tax benefits.",
            "SIMULATION" => "This is about projecting future balance growth with additional contributions.",
            "RETIREMENT_PROJECTION" => "This is about comparing retirement scenarios and their financial impact.",
            "ARTIFACT_GENERATION" => "This is about creating a summary of advice provided in this conversation session.",
            _ => "This is a general financial advice request."
        };

        return $"""
        MEMBER QUESTION: "{question}"
        
        CONTEXT: {contextualInfo}
        
        CRYPTOGRAPHICALLY VERIFIED FINANCIAL DATA (SD-JWT):
        {dataJson}
        
        PRIVACY NOTE: This data was selectively disclosed from the member's secure credentials. 
        Sensitive information like Tax File Numbers, full names, and addresses are cryptographically protected and not visible to this AI.
        
        Please provide specific financial advice based on this verified data. Include calculations where possible and mention that your response is based on cryptographically verified information.
        """;
    }

    private string GetSimulatedAdvice(string question, Dictionary<string, object> verifiedData, string intent)
    {
        // Enhanced simulated responses that consider conversation history and showcase GPT-5 capabilities
        return intent switch
        {
            "CONTRIBUTION_STRATEGY" =>
                "Based on your cryptographically verified balance of $150,000 and $10,000 remaining concessional contribution cap, " +
                "salary sacrificing the full $10,000 before June 30 would optimize your tax position. At a 32.5% marginal tax rate, " +
                "this saves $3,250 in tax while boosting your super balance to $160,000. The strategy also reduces your taxable income " +
                "to potentially lower your marginal rate threshold. Consider implementing automatic fortnightly deductions of $385 " +
                "to capture this benefit systematically without exceeding the annual $27,500 concessional cap.",

            "SIMULATION" =>
                "Adding $200 fortnightly ($5,200 annually) to your verified $150,000 balance creates compound growth acceleration. " +
                "With 6% annual returns, your balance grows to $205,312 next year versus $159,000 without additional contributions—a " +
                "$46,312 advantage. This extra $5,200 saves ~$1,690 in tax at a 32.5% rate, effectively costing only $3,510 net. " +
                "Over 25 years until retirement, this strategy compounds to approximately $280,000 additional retirement capital, " +
                "demonstrating the powerful interaction between tax savings and compound growth in superannuation.",

            "RETIREMENT_PROJECTION" =>
                "Retiring at 60 versus 65 fundamentally alters your wealth accumulation trajectory. With your verified birth year (1985) " +
                "and current $150,000 balance, the 5-year reduction in accumulation phase costs approximately $185,000-$240,000 in " +
                "final retirement capital, assuming continued contributions and 6% returns. However, early access to super at 60 " +
                "provides liquidity advantages and tax-free withdrawals after preservation age. To bridge this gap, increase annual " +
                "contributions to $15,000+ now, or consider phased retirement working 3 days/week from 60-65 to maintain some contributions " +
                "while accessing super benefits. The optimal strategy depends on your risk tolerance and lifestyle priorities.",

            "ARTIFACT_GENERATION" => GenerateEnhancedSummaryFromHistory(),

            _ =>
                "I can provide sophisticated financial advice based on cryptographically verified data from your secure SD-JWT credentials. " +
                "My analysis incorporates complex financial modeling, tax optimization strategies, and long-term wealth accumulation scenarios. " +
                "Please share the relevant verified information through your secure wallet to receive detailed, personalized recommendations " +
                "that consider multiple variables and their interactions across your retirement planning timeline."
        };
    }

    private string GenerateEnhancedSummaryFromHistory()
    {
        if (!_conversationHistory.Any())
        {
            return "No previous conversation history found in this session. " +
                   "Please engage with some financial questions first, then request a summary to receive a comprehensive " +
                   "Statement of Advice with sophisticated analysis and integrated recommendations across all discussed topics.";
        }

        var summary = "COMPREHENSIVE STATEMENT OF ADVICE - Advanced Session Analysis:\n\n";

        summary += $"Session Metadata:\n";
        summary += $"- Date/Time: {DateTimeOffset.UtcNow:yyyy-MM-dd HH:mm:ss}\n";
        summary += $"- Total Queries: {_conversationHistory.Count}\n";
        summary += $"- AI Model: {_modelName}\n";
        summary += $"- Analysis Type: Privacy-Preserving SD-JWT Verified Data\n\n";

        summary += "VERIFIED FINANCIAL DATA INVENTORY:\n";
        var allDataKeys = _conversationHistory.SelectMany(h => h.VerifiedData.Keys).Distinct().ToList();
        foreach (var key in allDataKeys)
        {
            var sampleValue = _conversationHistory.First(h => h.VerifiedData.ContainsKey(key)).VerifiedData[key];
            summary += $"- {key}: {sampleValue} (Cryptographically Verified)\n";
        }

        summary += "\nCONVERSATION FLOW & INTEGRATED ANALYSIS:\n";
        for (int i = 0; i < _conversationHistory.Count; i++)
        {
            var turn = _conversationHistory[i];
            summary += $"\n{i + 1}. Intent: {turn.Intent}\n";
            summary += $"   Question: \"{turn.Question}\"\n";
            summary += $"   Data Used: {string.Join(", ", turn.VerifiedData.Keys)}\n";
            summary += $"   Key Advice: {turn.Response.Substring(0, Math.Min(150, turn.Response.Length))}...\n";
        }

        summary += "\nINTEGRATED RECOMMENDATIONS:\n";
        summary += "Based on the verified data and conversation flow, your optimal strategy combines:\n";
        summary += "1. Immediate tax optimization through strategic salary sacrificing\n";
        summary += "2. Regular additional contributions for compound growth acceleration\n";
        summary += "3. Long-term retirement planning with flexible timeline considerations\n";
        summary += "4. Ongoing monitoring and adjustment based on legislative changes\n\n";

        summary += "PRIVACY & COMPLIANCE AUDIT:\n";
        summary += $"- Cryptographic Verification: ✓ All {allDataKeys.Count} data points mathematically proven authentic\n";
        summary += "- Selective Disclosure: ✓ Minimum necessary data exposure for each query\n";
        summary += "- PII Protection: ✓ No Tax File Numbers, full names, or addresses transmitted\n";
        summary += "- Session Security: ✓ Context maintained only during conversation, cleared at completion\n";
        summary += "- Regulatory Compliance: ✓ Advice based solely on verified, consented data disclosure\n\n";

        summary += "This Statement of Advice demonstrates advanced AI financial guidance with enterprise-grade privacy protection.";

        return summary;
    }

    private class ConversationTurn
    {
        public string Question { get; set; } = string.Empty;
        public Dictionary<string, object> VerifiedData { get; set; } = new();
        public string Intent { get; set; } = string.Empty;
        public string Response { get; set; } = string.Empty;
        public DateTimeOffset Timestamp { get; set; }
    }
}
