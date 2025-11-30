# Financial Co-Pilot Scenario - Privacy-Preserving AI Financial Advisor

## 📋 Table of Contents

- [Business Context](#-business-context)
- [The Problem](#-the-problem)
- [Our Solution](#-our-solution)
- [Architecture Overview](#-architecture-overview)
- [How It Works](#-how-it-works)
- [Technical Implementation](#-technical-implementation)
- [Security & Privacy](#-security--privacy)
- [AI Integration](#-ai-integration)
- [Getting Started](#-getting-started)

## 🏦 Business Context

### Financial Services Digital Transformation

The Financial Co-Pilot scenario demonstrates a revolutionary approach to AI-powered financial advisory services that addresses critical industry challenges:

**Market Drivers:**
- **Member Expectations**: Customers demand real-time, personalized financial guidance
- **Regulatory Compliance**: Strict privacy regulations (GDPR, CCPA, Australian Privacy Act)
- **Competitive Pressure**: FinTech disruption requiring traditional institutions to innovate
- **Cost Optimization**: Need to provide premium advisory services at scale

**Current Industry Pain Points:**
- **Data Silos**: Member financial data scattered across multiple systems
- **Privacy Concerns**: Reluctance to share sensitive financial information with AI
- **Generic Advice**: One-size-fits-all guidance that doesn't address individual circumstances
- **Trust Gap**: Members skeptical of AI-generated financial recommendations

### Australian Superannuation Context

This scenario specifically addresses the complexity of Australian superannuation (retirement savings) system:

- **$3.5 Trillion Industry**: Australia's superannuation system manages massive retirement savings
- **Complex Rules**: Contribution caps, tax implications, preservation ages create confusion
- **Individual Optimization**: Each member's situation requires personalized strategy
- **Regulatory Oversight**: APRA, ATO, and ASIC oversight requires compliance-first approach

## 🎯 The Problem

### "The Golden Record Paradox"

Financial institutions face a fundamental challenge we call the **"Golden Record Paradox"**:

```
┌─────────────────────────────────────────────────┐
│                                                 │
│  Members WANT personalized AI financial advice │
│                                                 │
│  ↓                                              │
│                                                 │
│  AI NEEDS comprehensive financial context       │
│                                                 │
│  ↓                                              │
│                                                 │
│  Financial data is COUPLED with "Toxic PII"    │
│                                                 │
│  ↓                                              │
│                                                 │
│  CANNOT stream sensitive data to cloud AI      │
│                                                 │
└─────────────────────────────────────────────────┘
```

### Specific Technical Challenges

#### 1. **Data Privacy Dilemma**
```
Financial Context Required:        Toxic PII Included:
• Account balance                  • Tax File Number (TFN)
• Contribution history            • Full legal name
• Investment performance          • Complete date of birth
• Transaction patterns            • Home address
• Tax implications               • Phone numbers
```

#### 2. **Real-Time Personalization Requirements**

**Member Questions Requiring Immediate, Personalized Responses:**

- *"Should I salary sacrifice this year?"*
  - **Needs**: Current balance, contribution cap remaining, tax bracket
  - **Privacy Risk**: TFN, employment details, income level

- *"If I add $200 per fortnight, what happens to my retirement?"*
  - **Needs**: Current balance, historical growth, age, retirement goals
  - **Privacy Risk**: DOB, employment status, family circumstances

- *"What if I retire at 60 instead of 65?"*
  - **Needs**: Projection models, preservation age rules, current contributions
  - **Privacy Risk**: Full financial profile, health status, employment plans

#### 3. **Regulatory Compliance Constraints**

- **Australian Privacy Principles (APP)**: Strict data minimization requirements
- **Superannuation Industry Supervision Act**: Fiduciary duty of care
- **Anti-Money Laundering**: Know Your Customer obligations
- **Data Sovereignty**: Requirements to keep data within Australian jurisdiction

#### 4. **Technical Integration Challenges**

- **Legacy Systems**: Core financial services platforms not designed for AI integration
- **Real-Time Requirements**: Sub-second response times expected
- **Scale**: Millions of members requiring simultaneous access
- **Multi-Channel**: Web, mobile app, phone, in-person consistency

## 💡 Our Solution

### **Stateless Co-Pilot with "Verify-then-Infer" Pattern**

Our solution transforms the traditional AI advisory model using **Selective Disclosure JSON Web Tokens (SD-JWT)** to enable privacy-preserving AI interactions:

```
Traditional Approach (BROKEN):
Member Data → Cloud AI → Privacy Risk

Our Approach (SECURE):
Member Credentials → Selective Disclosure → Verified Data → AI Reasoning → Advice
```

### Core Innovation: Progressive Disclosure

Instead of streaming complete member profiles to AI, we implement **just-in-time selective disclosure**:

1. **Client Device** = Secure Vault containing complete credentials
2. **AI Service** = Stateless reasoning engine receiving minimal verified data
3. **Progressive Context** = Session memory without persistent sensitive data storage

### Key Solution Components

#### 1. **Cryptographically Secured Member Credentials**
```
┌─────────────────────┐
│   Member Wallet     │
│                     │
│ ┌─────────────────┐ │
│ │ Account Cred    │ │ ← Registry issued, selectively disclosable
│ │ • Balance       │ │
│ │ • Cap remaining │ │
│ │ • TFN (hidden)  │ │
│ └─────────────────┘ │
│                     │
│ ┌─────────────────┐ │
│ │Transaction Cred │ │ ← Bank issued, selectively disclosable
│ │ • History       │ │
│ │ • Patterns      │ │
│ │ • Details(hide) │ │
│ └─────────────────┘ │
└─────────────────────┘
```

#### 2. **Intent-Based Data Minimization**
```csharp
// Example: "Should I salary sacrifice?" only needs:
var requiredFields = new[] { "account_balance", "cap_remaining" };

// Not disclosed: TFN, full_name, address, detailed_transactions
```

#### 3. **Session-Based Context Management**
```
Conversation Turn 1: Balance data only
Conversation Turn 2: + Growth projections (context maintained)
Conversation Turn 3: + Age data for retirement planning
Session End: Complete memory cleanup
```

## 🏗️ Architecture Overview

### High-Level System Architecture

```
┌─────────────────────┐    ┌─────────────────────┐    ┌─────────────────────┐
│                     │    │                     │    │                     │
│  Member's Device    │    │   Verifier/AI       │    │    Issuer Systems   │
│   (Holder)          │    │     Service         │    │                     │
│                     │    │                     │    │                     │
│ ┌─────────────────┐ │    │ ┌─────────────────┐ │    │ ┌─────────────────┐ │
│ │ Secure Wallet   │ │    │ │ Intent Router   │ │    │ │ Registry        │ │
│ │                 │ │    │ │                 │ │    │ │ (Link Group)    │ │
│ │ • SD-JWT Creds  │ │    │ │ • Query Analysis│ │    │ │                 │ │
│ │ • Private Keys  │ │    │ │ • Field Mapping │ │    │ │ • Account Creds │ │
│ │ • Presentation  │ │    │ └─────────────────┘ │    │ │ • Member Data   │ │
│ │   Builder       │ │    │                     │    │ └─────────────────┘ │
│ └─────────────────┘ │    │ ┌─────────────────┐ │    │                     │
│                     │◄──►│ │ VP Verifier     │ │    │ ┌─────────────────┐ │
│ ┌─────────────────┐ │    │ │                 │ │    │ │ Bank System     │ │
│ │ User Interface  │ │    │ │ • Crypto Verify │ │    │ │                 │ │
│ │                 │ │    │ │ • Claim Extract │ │    │ │ • Transaction   │ │
│ │ • Chat UI       │ │    │ │ • Key Binding   │ │    │ │   Credentials   │ │
│ │ • Query Input   │ │    │ └─────────────────┘ │    │ │ • History Data  │ │
│ │ • Advice Output │ │    │                     │    │ └─────────────────┘ │
│ └─────────────────┘ │    │ ┌─────────────────┐ │    │                     │
│                     │    │ │ AI Advice       │ │    │                     │
│                     │    │ │ Engine          │ │    │                     │
│                     │    │ │                 │ │    │                     │
│                     │    │ │ • OpenAI GPT-5  │ │    │                     │
│                     │    │ │ • Session Mgmt  │ │    │                     │
│                     │    │ │ • Context Track │ │    │                     │
│                     │    │ └─────────────────┘ │    │                     │
└─────────────────────┘    └─────────────────────┘    └─────────────────────┘
```

### Component Details

#### **Member's Device (Holder)**
- **Wallet Simulator**: Manages SD-JWT credentials securely
- **Presentation Builder**: Creates context-specific data disclosures
- **User Interface**: Chat-based interaction with AI advisor

#### **Verifier/AI Service**
- **Intent Router**: Analyzes user queries to determine required data fields
- **VP Verifier**: Cryptographically validates selective presentations
- **AI Advice Engine**: Generates personalized financial guidance using verified data

#### **Issuer Systems**
- **Registry System**: Issues account and membership credentials
- **Bank System**: Issues transaction history and financial performance credentials

### Data Flow Architecture

```
1. User Query → 2. Intent Analysis → 3. Required Fields → 4. Selective Presentation
     ↑                                                           ↓
     │                                                           │
8. AI Response ← 7. Generate Advice ← 6. Verified Claims ← 5. Crypto Verification
```

**Detailed Flow:**
1. **User Query**: Member asks financial question via chat interface
2. **Intent Analysis**: System determines what data is needed for response
3. **Required Fields**: Minimal field list generated (e.g., balance, cap_remaining)
4. **Selective Presentation**: Wallet creates presentation with only required fields
5. **Crypto Verification**: Verifier validates signatures and extracts claims
6. **Verified Claims**: Clean, validated data passed to AI engine
7. **Generate Advice**: AI processes query with verified context
8. **AI Response**: Personalized financial advice returned to member

## 🔄 How It Works

### Detailed Interaction Flow

#### **Phase 1: Ecosystem Setup**

```
Registry System Initialization:
┌─────────────────────────────────┐
│ Link Group Registry             │
│ • Generate signing keys         │
│ • Publish credential schemas    │
│ • Setup issuance endpoints      │
└─────────────────────────────────┘

Bank System Initialization:
┌─────────────────────────────────┐
│ Financial Institution           │
│ • Transaction processing ready  │
│ • History compilation active    │
│ • Integration with registry     │
└─────────────────────────────────┘
```

#### **Phase 2: Credential Issuance**

```csharp
// Account Credential (Registry-issued)
var accountCredential = new SdJwtVcPayload
{
    Issuer = "https://registry.linkgroup.com",
    Subject = "did:example:member_789456",
    AdditionalData = new Dictionary<string, object>
    {
        // Selectively disclosable financial data
        ["member_id"] = "MEMBER_789456",
        ["account_balance"] = 150000m,
        ["cap_remaining"] = 10000m,
        ["joined_date"] = "2019-01-15",
        ["birth_year"] = 1985,
        
        // Toxic PII (hidden by default)
        ["tax_file_number"] = "123-456-789",
        ["full_name"] = "John Smith",
        ["home_address"] = "123 Main St, Sydney",
        ["date_of_birth"] = "1985-03-15"
    }
};

// Selective disclosure configuration
var sdOptions = new SdIssuanceOptions
{
    DisclosureStructure = new
    {
        // Financial data - can be disclosed
        member_id = true,
        account_balance = true,
        cap_remaining = true,
        birth_year = true,
        joined_date = true,
        
        // Toxic PII - protected by default
        tax_file_number = true,  // Available but not disclosed
        full_name = true,        // Available but not disclosed
        home_address = true,     // Available but not disclosed
        date_of_birth = true     // Available but not disclosed
    }
};
```

#### **Phase 3: Multi-Turn Conversation**

**Turn 1: Contribution Strategy**
```
User: "Should I salary sacrifice?"

Intent Router Analysis:
├─ Detected Intent: CONTRIBUTION_STRATEGY
├─ Required Fields: ["account_balance", "cap_remaining"]
└─ Privacy Level: MEDIUM

Selective Presentation:
┌─────────────────────────────────┐
│ Disclosed Data Only:            │
│ • account_balance: 150000       │
│ • cap_remaining: 10000          │
│                                 │
│ Protected (NOT sent):           │
│ • tax_file_number: [HIDDEN]     │
│ • full_name: [HIDDEN]           │
│ • home_address: [HIDDEN]        │
└─────────────────────────────────┘

AI Response:
"Based on your verified balance of $150,000 and $10,000 
remaining contribution cap, salary sacrificing the full 
amount before June 30 would save $2,500-$3,700 in tax 
depending on your marginal rate..."
```

**Turn 2: Growth Projection (Building Context)**
```
User: "If I add $200 per fortnight, what happens?"

Intent Router Analysis:
├─ Detected Intent: SIMULATION
├─ Required Fields: ["account_balance"] (reuse from session)
├─ Session Context: Previous advice about salary sacrifice
└─ Privacy Level: LOW

AI Response (with context):
"Adding $200 fortnightly to your verified $150,000 balance, 
combined with the salary sacrifice strategy we discussed, 
would accelerate your retirement savings significantly. 
Your balance would grow to ~$205,312 by next year..."
```

**Turn 3: Retirement Planning (Additional Data)**
```
User: "What if I retire at 60 instead of 65?"

Intent Router Analysis:
├─ Detected Intent: RETIREMENT_PROJECTION
├─ Required Fields: ["account_balance", "birth_year", "joined_date"]
├─ Session Context: Previous advice + growth projections
└─ Privacy Level: MEDIUM

New Selective Presentation:
┌─────────────────────────────────┐
│ Additional Disclosed Data:      │
│ • birth_year: 1985              │
│ • joined_date: 2019-01-15       │
│                                 │
│ Still Protected:                │
│ • tax_file_number: [HIDDEN]     │
│ • full_name: [HIDDEN]           │
│ • date_of_birth: [HIDDEN]       │
└─────────────────────────────────┘

AI Response (comprehensive context):
"Retiring at 60 versus 65 (you're currently 39) significantly 
impacts your accumulation phase. With your current $150,000 
balance and the strategies we've discussed, early retirement 
could cost $180,000-$250,000 in compound growth..."
```

**Turn 4: Summary Generation**
```
User: "Send me the summary"

Intent Router Analysis:
├─ Detected Intent: ARTIFACT_GENERATION
├─ Required Fields: ["member_id"]
├─ Session Context: ALL previous conversation
└─ Privacy Level: AUDIT

AI Response (Statement of Advice):
"COMPREHENSIVE STATEMENT OF ADVICE
Session Date: 2024-12-19
Total Questions: 3
Member: MEMBER_789456

VERIFIED DATA USED:
• Account Balance: $150,000 (cryptographically verified)
• Cap Remaining: $10,000 (cryptographically verified)
• Age Context: Born 1985 (cryptographically verified)

ADVICE PROVIDED:
1. Salary Sacrifice: $10,000 annually saves $2,500-$3,700
2. Additional Contributions: $200 fortnightly adds $200,000 over 20 years
3. Retirement Timing: Early retirement at 60 costs $180,000-$250,000

PRIVACY AUDIT TRAIL:
• No PII transmitted to AI service
• All data cryptographically verified
• Session context cleared at completion"
```

### Session Management & Privacy

#### **Within Session (Context Maintained)**
```
Conversation Memory:
├─ Question History: Stored for coherent responses
├─ Data Context: Accumulated verified claims
├─ Advice Evolution: Building comprehensive guidance
└─ Privacy Boundary: No PII ever stored
```

#### **Session End (Complete Cleanup)**
```
Memory Cleanup Process:
├─ Clear conversation history
├─ Delete verified claims
├─ Reset AI context
├─ Log privacy audit
└─ Fresh state for next session
```

## 🔧 Technical Implementation

### Core Technology Stack

#### **SD-JWT Implementation**
```csharp
// Core selective disclosure functionality
using SdJwt.Net.Issuer;
using SdJwt.Net.Vc.Issuer;
using SdJwt.Net.Vc.Verifier;
using SdJwt.Net.Holder;

// Credential issuance with selective disclosure
var vcIssuer = new SdJwtVcIssuer(registryKey, SecurityAlgorithms.EcdsaSha256);
var credential = vcIssuer.Issue(vct, vcPayload, sdOptions, holderPublicKey);

// Selective presentation creation
var holder = new SdJwtHolder(credential.Issuance);
var presentation = holder.CreatePresentation(
    disclosure => requiredFields.Contains(disclosure.ClaimName),
    keyBindingJwt, holderPrivateKey, algorithm);

// Cryptographic verification
var verifier = new SdJwtVcVerifier(keyResolver);
var result = await verifier.VerifyAsync(presentation, validationParams, kbParams, vct);
```

#### **AI Integration Architecture**
```csharp
public class OpenAiAdviceEngine
{
    private readonly OpenAIClient _openAiClient;
    private readonly List<ConversationTurn> _conversationHistory;
    
    public async Task<string> GenerateAdviceAsync(
        string question, 
        Dictionary<string, object> verifiedClaims, 
        string intent)
    {
        // Build context-aware prompt
        var systemPrompt = BuildSystemPrompt();
        var userPrompt = BuildUserPrompt(question, verifiedClaims, intent);
        
        // GPT-5 optimization
        var options = new ChatCompletionOptions
        {
            Temperature = 0.2f,        // Lower for financial accuracy
            MaxOutputTokenCount = 1200, // Higher for detailed analysis
            Model = "gpt-5-turbo"      // Latest reasoning capabilities
        };
        
        // Generate advice with session context
        var response = await _openAiClient.GetChatClient(_modelName)
            .CompleteChatAsync(messages, options);
            
        // Store in session context
        AddToConversationHistory(question, verifiedClaims, intent, response);
        
        return response.Value.Content[0].Text;
    }
    
    public void ClearConversationHistory()
    {
        _conversationHistory.Clear();
        _logger.LogInformation("Session context cleared - privacy cleanup complete");
    }
}
```

#### **Intent Classification System**
```csharp
public class IntentRouter
{
    public string RouteIntent(string query)
    {
        return query.ToLower() switch
        {
            var q when q.Contains("salary sacrifice") => "CONTRIBUTION_STRATEGY",
            var q when q.Contains("add") && q.Contains("fortnight") => "SIMULATION",
            var q when q.Contains("retire") => "RETIREMENT_PROJECTION",
            var q when q.Contains("summary") => "ARTIFACT_GENERATION",
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
            _ => new() { "account_balance" }
        };
    }
}
```

### Security Implementation

#### **Cryptographic Verification**
```csharp
public class PresentationVerifier
{
    public async Task<VerificationResult> VerifyPresentationAsync(string presentation)
    {
        // 1. Parse SD-JWT structure
        var parsedSdJwt = SdJwtParser.Parse(presentation);
        
        // 2. Verify issuer signature
        var issuerKey = await _keyResolver(parsedSdJwt.JwtPayload.Issuer);
        var signatureValid = await _jwtHandler.ValidateTokenAsync(
            parsedSdJwt.Jwt, validationParameters);
            
        // 3. Verify key binding (holder proof)
        if (parsedSdJwt.KeyBindingJwt != null)
        {
            var kbResult = await _jwtHandler.ValidateTokenAsync(
                parsedSdJwt.KeyBindingJwt, kbValidationParameters);
        }
        
        // 4. Extract and validate disclosed claims
        var disclosedClaims = ExtractDisclosedClaims(parsedSdJwt);
        
        return new VerificationResult
        {
            IsValid = true,
            VerifiedClaims = disclosedClaims
        };
    }
}
```

## 🔒 Security & Privacy

### Privacy-by-Design Architecture

#### **Data Minimization Principles**
```
Traditional AI Advisory:
┌─────────────────────────────────┐
│ Complete Member Profile         │
│ ├─ Personal: Name, DOB, TFN     │
│ ├─ Financial: All accounts      │
│ ├─ History: Complete txn log    │
│ └─ Behavioral: All interactions │
└─────────────────────────────────┘
                ↓
        [PRIVACY RISK HIGH]

Our Selective Disclosure:
┌─────────────────────────────────┐
│ Query-Specific Fields Only      │
│ ├─ Query 1: Balance + Cap       │
│ ├─ Query 2: + Growth data       │
│ ├─ Query 3: + Age context       │
│ └─ PII: NEVER transmitted       │
└─────────────────────────────────┘
                ↓
        [PRIVACY RISK MINIMAL]
```

#### **Cryptographic Guarantees**

**1. Authenticity**: Every piece of data is cryptographically signed by trusted issuers
```
Data Pipeline: Registry → Sign → Member Wallet → Selective Present → AI Service
Verification: ✓ Signature ✓ Key Binding ✓ Temporal Validity ✓ Claim Integrity
```

**2. Selective Disclosure**: Zero-knowledge proof that claims exist without revealing them
```
Credential Structure:
├─ Public Claims: Issuer, subject, timestamps
├─ Selectively Disclosable: Financial data (can be revealed)
├─ Hidden Claims: PII (cryptographically protected)
└─ Proof Structure: Mathematical proof without data exposure
```

**3. Key Binding**: Proof that the presenter legitimately owns the credential
```
Key Binding JWT:
├─ Audience: AI service endpoint
├─ Nonce: Replay attack prevention
├─ Holder Signature: Proof of possession
└─ Temporal Bounds: Time-limited validity
```

### Privacy Audit Trail

Every interaction creates a comprehensive privacy audit:

```json
{
  "session_id": "session-12345",
  "timestamp": "2024-12-19T10:30:00Z",
  "interactions": [
    {
      "turn": 1,
      "intent": "CONTRIBUTION_STRATEGY",
      "data_disclosed": ["account_balance", "cap_remaining"],
      "data_protected": ["tax_file_number", "full_name", "address"],
      "cryptographic_proof": "sha256:abc123...",
      "ai_model": "gpt-5-turbo"
    }
  ],
  "privacy_guarantees": {
    "pii_transmitted": false,
    "data_minimization": true,
    "cryptographic_verification": true,
    "session_cleanup": "completed"
  }
}
```

## 🤖 AI Integration

### GPT-5 Enhanced Capabilities

#### **Model Selection Strategy**
```
Production Deployment:
├─ Primary: gpt-5-turbo (best balance of performance/cost)
├─ Fallback: gpt-4o (reliable alternative)
├─ Development: gpt-3.5-turbo (cost-effective testing)
└─ Future: gpt-5-specialized (finance-specific models)
```

#### **Advanced Prompting for Financial Domain**
```csharp
private string BuildSystemPrompt()
{
    return $"""
    You are a professional financial advisor specializing in Australian superannuation.
    
    CRITICAL CONTEXT:
    - You receive cryptographically verified financial data via SD-JWT
    - Data is selectively disclosed - you only see minimum required information
    - This is privacy-preserving AI - sensitive PII is protected
    
    YOUR EXPERTISE:
    - Australian superannuation rules and contribution caps
    - Tax implications and optimization strategies
    - Retirement planning and preservation ages
    - Compound growth calculations and projections
    
    RESPONSE REQUIREMENTS:
    - Base advice ONLY on cryptographically verified data provided
    - Include specific calculations with Australian tax rates
    - Mention key compliance dates (June 30, preservation ages)
    - Provide actionable next steps
    - Acknowledge the privacy-preserving nature
    
    SESSION CONTEXT:
    {_conversationHistory.Any() ? BuildConversationContext() : "New session - no prior context"}
    """;
}

private string BuildUserPrompt(string question, Dictionary<string, object> verifiedData, string intent)
{
    return $"""
    MEMBER QUESTION: "{question}"
    
    CONTEXT: {GetIntentDescription(intent)}
    
    CRYPTOGRAPHICALLY VERIFIED DATA:
    {JsonSerializer.Serialize(verifiedData, new JsonSerializerOptions { WriteIndented = true })}
    
    PRIVACY NOTE: This data was selectively disclosed from secure credentials.
    Sensitive PII (TFN, full names, addresses) is cryptographically protected.
    
    Provide specific financial advice based on this verified data.
    """;
}
```

#### **Session Context Management**
```csharp
public class ConversationTurn
{
    public string Question { get; set; }
    public Dictionary<string, object> VerifiedData { get; set; }
    public string Intent { get; set; }
    public string Response { get; set; }
    public DateTimeOffset Timestamp { get; set; }
}

private string GenerateSummaryFromHistory()
{
    if (!_conversationHistory.Any())
        return "No conversation history - please ask questions first";
        
    var summary = new StringBuilder();
    summary.AppendLine("STATEMENT OF ADVICE - Session Summary");
    summary.AppendLine($"Date: {DateTimeOffset.UtcNow:yyyy-MM-dd}");
    summary.AppendLine($"Questions: {_conversationHistory.Count}");
    
    // Privacy audit
    var allDataUsed = _conversationHistory
        .SelectMany(h => h.VerifiedData.Keys)
        .Distinct()
        .ToList();
    
    summary.AppendLine("\nVERIFIED DATA USED:");
    foreach (var field in allDataUsed)
    {
        var sampleValue = _conversationHistory
            .First(h => h.VerifiedData.ContainsKey(field))
            .VerifiedData[field];
        summary.AppendLine($"- {field}: {sampleValue}");
    }
    
    summary.AppendLine("\nADVICE PROGRESSION:");
    for (int i = 0; i < _conversationHistory.Count; i++)
    {
        var turn = _conversationHistory[i];
        summary.AppendLine($"{i + 1}. {turn.Intent}: \"{turn.Question}\"");
        summary.AppendLine($"   Response: {turn.Response.Substring(0, Math.Min(200, turn.Response.Length))}...");
    }
    
    summary.AppendLine("\nPRIVACY COMPLIANCE:");
    summary.AppendLine("- All data cryptographically verified");
    summary.AppendLine("- Selective disclosure enforced");
    summary.AppendLine("- No PII transmitted to AI");
    summary.AppendLine("- Session context cleared at completion");
    
    return summary.ToString();
}
```

### Model Optimization

#### **Performance Tuning**
```csharp
var chatCompletionOptions = new ChatCompletionOptions
{
    // GPT-5 optimized settings
    Temperature = _modelName.StartsWith("gpt-5") ? 0.2f : 0.3f,
    MaxOutputTokenCount = _modelName.StartsWith("gpt-5") ? 1200 : 800,
    TopP = 0.9f,
    FrequencyPenalty = 0.0f,
    PresencePenalty = 0.0f
};
```

#### **Cost Optimization**
```
Model Selection Matrix:
                    Cost/Demo  Quality   Speed    Use Case
gpt-5-turbo        $0.02-0.05   ★★★★★     ★★★★☆    Production
gpt-5              $0.04-0.08   ★★★★★     ★★★☆☆    Complex analysis
gpt-4o             $0.015-0.04  ★★★★☆     ★★★★★    Balanced
gpt-4-turbo        $0.01-0.03   ★★★☆☆     ★★★★☆    Standard
gpt-3.5-turbo      $0.002-0.004 ★★☆☆☆     ★★★★★    Testing
```

## 🚀 Getting Started

### Prerequisites

#### **1. Environment Setup**
```bash
# Clone the repository
git clone https://github.com/openwallet-foundation-labs/sd-jwt-dotnet.git
cd sd-jwt-dotnet

# Build the solution
dotnet restore
dotnet build

# Navigate to samples
cd samples/SdJwt.Net.Samples
```

#### **2. OpenAI Configuration (Optional)**
```bash
# For real AI responses
export OPENAI_API_KEY="your-openai-api-key-here"
export OPENAI_MODEL="gpt-5-turbo"

# For simulated responses (no API key needed)
# Demo works with high-quality simulated responses
```

### Quick Start Demo

#### **Run the Financial Co-Pilot**
```bash
# Start the interactive demo
dotnet run

# Select option "F" for Financial Co-Pilot
# Follow the interactive conversation flow
```

#### **Sample Conversation Flow**
```
1. Select: "Should I salary sacrifice?"
   → See intent analysis and selective disclosure
   → Receive personalized tax optimization advice

2. Select: "If I add $200 per fortnight, what happens?"
   → Observe session context building
   → Get compound growth projections

3. Select: "What if I retire at 60 instead of 65?"
   → Watch additional data disclosure
   → Receive comprehensive retirement analysis

4. Select: "Send me the summary"
   → Generate Statement of Advice
   → Review complete privacy audit trail
```

### Advanced Configuration

#### **Production Deployment**
```csharp
// Production-ready configuration
services.AddSingleton<OpenAiAdviceEngine>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<OpenAiAdviceEngine>>();
    return new OpenAiAdviceEngine(logger);
});

services.Configure<FinancialCoPilotOptions>(options =>
{
    options.DefaultModel = "gpt-5-turbo";
    options.EnableSessionLogging = true;
    options.PrivacyAuditRequired = true;
    options.MaxSessionDuration = TimeSpan.FromHours(1);
});
```

#### **Integration with Real Systems**
```csharp
// Replace simulation with real issuer integration
var realEcosystem = new ProductionFinancialEcosystem(
    registryEndpoint: "https://registry.linkgroup.com",
    bankEndpoint: "https://api.yourbank.com",
    credentialSchemas: productionSchemas);
```

### Monitoring & Analytics

#### **Privacy Compliance Monitoring**
```csharp
public class PrivacyAuditService
{
    public async Task LogInteractionAsync(PrivacyAuditLog log)
    {
        // Verify no PII was transmitted
        var piiDetected = DetectPII(log.DisclosedData);
        if (piiDetected.Any())
        {
            await _alertingService.RaisePrivacyAlertAsync(piiDetected);
        }
        
        // Log for compliance reporting
        await _auditRepository.SaveAuditLogAsync(log);
    }
}
```

#### **Performance Metrics**
```csharp
public class PerformanceTracker
{
    public void TrackInteraction(string intent, TimeSpan responseTime, string model)
    {
        _metrics.RecordValue("financial_copilot.response_time", 
            responseTime.TotalMilliseconds, 
            new { intent, model });
            
        _metrics.IncrementCounter("financial_copilot.interactions", 
            new { intent, model });
    }
}
```

---

## 📞 Support & Resources

- **📖 Documentation**: [Full API Reference](../README.md)
- **💬 Discussions**: [GitHub Discussions](https://github.com/openwallet-foundation-labs/sd-jwt-dotnet/discussions)
- **🐛 Issues**: [Report Issues](https://github.com/openwallet-foundation-labs/sd-jwt-dotnet/issues)
- **🔧 OpenAI Setup**: [Detailed Configuration Guide](./OPENAI_SETUP.md)

**Ready to build the future of privacy-preserving AI financial services?** The Financial Co-Pilot scenario demonstrates how to combine cutting-edge AI with bulletproof privacy protection using SD-JWT technology. 🚀
