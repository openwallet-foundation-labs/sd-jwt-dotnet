# SD-JWT .NET Scenarios

This directory contains comprehensive real-world scenarios demonstrating complete end-to-end workflows with the SD-JWT .NET ecosystem.

## 🎯 Available Scenarios

### 🌍 Real-World Use Cases
**File:** [`RealWorldScenariosExample.cs`](./RealWorldScenariosExample.cs)

Complete industry workflows demonstrating:
- **University to Bank Loan**: Graduate applies for home loan with education credentials
- **Job Application**: Defense contractor position with multiple credential requirements
- **Healthcare Verification**: Medical professional license verification
- **Multi-Issuer Ecosystems**: Credentials from different authorities working together

### 🤖 Financial Co-Pilot - AI-Powered Privacy-Preserving Advisor
**Files:** 
- [`FinancialCoPilotScenario.cs`](./FinancialCoPilotScenario.cs) - Main scenario implementation
- [`Financial/OpenAiAdviceEngine.cs`](./Financial/OpenAiAdviceEngine.cs) - AI engine with GPT-5 support
- [`Financial/README.md`](./Financial/README.md) - Detailed documentation
- [`Financial/FINANCIAL_COPILOT_INTRODUCTION.md`](./Financial/FINANCIAL_COPILOT_INTRODUCTION.md) - Comprehensive business context and architecture guide

#### The Challenge: "Golden Record" Paradox
Financial services members need personalized AI guidance but their financial data is coupled with sensitive PII (Tax File Numbers, addresses, full dates of birth) that cannot be streamed to cloud AI services.

#### Our Solution: "Verify-then-Infer" Pattern
```
┌─────────────────┐  🔐 Verifiable    ┌─────────────────┐
│ Client Device   │ ←─ Presentation ──│ AI Service      │
│ (Secure Vault)  │                   │ (Stateless      │
│                 │                   │  Reasoner)      │
└─────────────────┘                   └─────────────────┘
```

#### Key Features
- **GPT-5 Integration**: Latest OpenAI models for sophisticated financial reasoning
- **Session Memory**: Maintains context within conversation, clears after session end
- **Privacy Protection**: TFN, names, addresses never disclosed to AI
- **Real-Time Advice**: Personalized guidance using cryptographically verified data
- **Australian Focus**: Superannuation, tax implications, retirement planning

#### Quick Start
```bash
# Optional: Enable real AI responses
export OPENAI_API_KEY="your-api-key-here"
export OPENAI_MODEL="gpt-5-turbo"

# Run the scenario
cd samples/SdJwt.Net.Samples
dotnet run
# Select "F" for Financial Co-Pilot
```

#### Architecture Highlights
- **Progressive Disclosure**: Only required data per query (account_balance, cap_remaining, etc.)
- **Cryptographic Verification**: All data verified before AI processing
- **Stateless Processing**: No persistent storage of sensitive member data
- **Session Context**: AI maintains conversation context within session only

## 🏗️ Scenario Architecture Patterns

### Multi-Issuer Ecosystems
```csharp
// University issues education credential
var universityCredential = educationIssuer.Issue(degreeData, sdOptions, holderJwk);

// Employer issues employment credential  
var employmentCredential = employerIssuer.Issue(jobData, sdOptions, holderJwk);

// Bank verifies both for loan application
var presentation = holder.CreateMultiCredentialPresentation(
    new[] { universityCredential, employmentCredential },
    loanRequirements);

var verificationResult = await bankVerifier.VerifyAsync(presentation);
```

### Privacy-Preserving AI Integration
```csharp
// 1. Route intent to determine required fields
var intent = intentRouter.RouteIntent("Should I salary sacrifice?");
var requiredFields = intentRouter.GetRequiredFields(intent); // ["account_balance", "cap_remaining"]

// 2. Create selective presentation with only required data
var presentation = wallet.CreateSelectivePresentation(credential, requiredFields);

// 3. Verify cryptographically before AI processing
var verificationResult = await verifier.VerifyAsync(presentation);

// 4. Send verified data to AI (no PII included)
var advice = await aiEngine.GenerateAdviceAsync(query, verificationResult.VerifiedClaims, intent);
```

### Status-Aware Workflows
```csharp
// Check credential status before accepting
var statusResult = await statusVerifier.CheckStatusAsync(credential);
if (statusResult.Status == CredentialStatus.Revoked)
{
    return new VerificationResult { IsValid = false, Reason = "Credential revoked" };
}

// Proceed with verification
var result = await verifier.VerifyAsync(presentation);
```

## 📚 Learning from Scenarios

### Real-World Implementation Patterns
1. **Trust Chain Management**: How different issuers establish trust
2. **Privacy by Design**: Minimal disclosure principles in practice
3. **Error Handling**: Production-ready error scenarios
4. **Performance Optimization**: High-throughput verification patterns
5. **Security Hardening**: Attack prevention and threat mitigation

### AI Integration Best Practices
1. **Intent Classification**: Determining minimum required data per query
2. **Progressive Disclosure**: Building conversation context while protecting privacy
3. **Session Management**: Maintaining context within sessions, clearing between sessions
4. **Data Minimization**: Only sending verified, required fields to AI services
5. **Cryptographic Verification**: Ensuring data authenticity before AI processing

## 🔧 Technical Implementation

### Multi-Package Integration
The scenarios demonstrate integration across the entire SD-JWT .NET ecosystem:

- **Core**: Basic selective disclosure and verification
- **VC**: Verifiable credential standards compliance  
- **StatusList**: Revocation and suspension management
- **OID4VCI**: Credential issuance protocols
- **OID4VP**: Presentation verification protocols
- **OidFederation**: Trust chain establishment
- **PresentationExchange**: Complex requirement matching

### Performance Characteristics
| Scenario | Complexity | Performance | Key Features |
|----------|------------|-------------|--------------|
| **Real-World** | High | 100+ ops/sec | Multi-issuer, complex requirements |
| **Financial Co-Pilot** | Very High | 50+ ops/sec* | AI integration, session management |

*AI performance depends on OpenAI API response times

## 🔒 Security Considerations

### Threat Model Coverage
- **Credential Tampering**: Cryptographic signature validation
- **Privacy Leakage**: Minimal disclosure enforcement  
- **Replay Attacks**: Nonce and timestamp validation
- **Trust Chain Attacks**: Issuer key verification
- **AI Data Exposure**: Verified data only, no PII transmission

### Production Deployment
- **Key Management**: Secure key generation and rotation
- **Monitoring**: Verification metrics and error tracking
- **Scalability**: High-throughput concurrent processing
- **Compliance**: Regulatory requirement adherence
- **Incident Response**: Attack detection and mitigation

## 🚀 Running Scenarios

### Interactive Execution
```bash
cd samples/SdJwt.Net.Samples
dotnet run

# Available options:
# C - Real-World Scenarios (Industry use cases)
# F - Financial Co-Pilot (AI-powered advisor)
```

### Automated Testing
```bash
# Run all scenarios in sequence
dotnet run -- --automated

# Run specific scenario
dotnet run -- --scenario financial-copilot
dotnet run -- --scenario real-world
```

### Configuration Options
```bash
# Financial Co-Pilot with OpenAI
export OPENAI_API_KEY="your-key"
export OPENAI_MODEL="gpt-5-turbo"

# Performance testing mode
export SCENARIO_PERFORMANCE_TEST="true"
export SCENARIO_ITERATIONS="1000"
```

## 📊 Scenario Metrics

### Real-World Scenarios
- **Universities Simulated**: 3 major institutions
- **Employers**: 5 different company types
- **Verification Requirements**: 12 different patterns
- **Trust Relationships**: Cross-institutional verification

### Financial Co-Pilot
- **Intent Types**: 8 different financial advice categories
- **Data Fields**: 15 selectively disclosable fields
- **Privacy Protection**: 100% PII protection rate
- **Session Management**: Complete context isolation

## 🤝 Contributing New Scenarios

### Guidelines
1. **Real-World Relevance**: Address actual industry problems
2. **Complete Workflows**: End-to-end implementation
3. **Privacy Focus**: Demonstrate minimal disclosure
4. **Performance Consideration**: Include timing and optimization
5. **Security Emphasis**: Show threat mitigation

### Scenario Template
```csharp
/// <summary>
/// Demonstrates [specific industry workflow]
/// Key features: [privacy patterns, trust management, performance]
/// </summary>
public class NewScenarioExample
{
    public static async Task RunScenario(IServiceProvider services)
    {
        // 1. Setup multi-issuer ecosystem
        // 2. Issue credentials with selective disclosure
        // 3. Create complex presentation requirements
        // 4. Demonstrate verification workflows
        // 5. Show error handling and security
        // 6. Measure performance characteristics
    }
}
```

---

**Ready to build production SD-JWT applications?** These scenarios provide complete implementation patterns for real-world deployment.

For detailed AI integration guidance, see the [Financial Co-Pilot Documentation](./Financial/README.md).

For OpenAI configuration, see the [Setup Guide](./OPENAI_SETUP.md).
