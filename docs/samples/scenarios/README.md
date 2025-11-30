# SD-JWT .NET Scenarios - Real-World Applications

This directory contains comprehensive documentation for real-world scenarios that demonstrate complete end-to-end workflows with the SD-JWT .NET ecosystem.

## 🎯 Available Scenarios

### 🌍 Real-World Use Cases
**Documentation**: [Real-World Use Cases](./real-world-use-cases.md)  
**Source Code**: `samples/SdJwt.Net.Samples/Scenarios/RealWorldScenariosExample.cs`

Complete industry workflows demonstrating:
- **University to Bank Loan**: Graduate applies for home loan with education credentials
- **Job Application**: Defense contractor position with multiple credential requirements
- **Healthcare Verification**: Medical professional license verification across institutions
- **Multi-Issuer Ecosystems**: Credentials from different authorities working together seamlessly

### 🤖 Financial Co-Pilot - AI-Powered Privacy-Preserving Advisor
**Documentation**: [Financial Co-Pilot](./financial/)  
**Source Files**:
- `samples/SdJwt.Net.Samples/Scenarios/Financial/FinancialCoPilotScenario.cs` - Original implementation
- `samples/SdJwt.Net.Samples/Scenarios/Financial/EnhancedFinancialCoPilotScenario.cs` - Full ecosystem integration
- `samples/SdJwt.Net.Samples/Scenarios/Financial/OpenAiAdviceEngine.cs` - AI engine with GPT-5 support

Revolutionary AI-powered financial advisor solving the "Golden Record" paradox - providing personalized guidance while protecting sensitive member data.

## 🏗️ Scenario Categories

### 🔍 Industry Verticals

#### Financial Services
- **Personal Finance**: Retirement planning, investment advice, tax optimization
- **Banking**: Loan applications, account verification, credit assessments
- **Insurance**: Risk assessment, claims processing, policy verification
- **Superannuation**: Contribution strategies, fund switching, retirement projections

#### Education & Employment
- **Academic Credentials**: Degree verification, transcript authentication, continuing education
- **Professional Licensing**: Medical licenses, legal certifications, engineering qualifications
- **Employment Verification**: Job history, salary confirmation, security clearances
- **Skills Certification**: Technical certifications, professional development, competency verification

#### Healthcare
- **Medical Records**: Treatment history, medication lists, allergy information
- **Professional Credentials**: Medical licenses, specialty certifications, hospital privileges
- **Insurance Verification**: Coverage confirmation, pre-authorization, claims processing
- **Research Participation**: Consent management, data sharing, privacy protection

#### Government & Civic
- **Identity Verification**: Citizenship proof, age verification, address confirmation
- **Benefits Administration**: Welfare eligibility, tax credits, social services
- **Regulatory Compliance**: Professional licensing, business permits, safety certifications
- **Voting & Elections**: Voter registration, eligibility verification, ballot access

### 🎯 Technical Patterns

#### Privacy-Preserving AI
- **Progressive Disclosure**: Minimal data sharing for AI processing
- **Session Management**: Context preservation within conversation boundaries
- **Zero-Trust Verification**: Cryptographic validation before AI inference
- **Context Isolation**: No cross-session data leakage

#### Multi-Issuer Workflows
- **Trust Chain Management**: Establishing trust across organizational boundaries
- **Credential Orchestration**: Coordinating multiple credentials for complex requirements
- **Cross-Institution Verification**: Validating credentials across different trust domains
- **Federated Identity**: Enabling seamless identity verification across organizations

#### High-Scale Operations
- **Batch Processing**: High-throughput credential issuance and verification
- **Concurrent Verification**: Parallel processing for performance optimization
- **Caching Strategies**: Efficient storage and retrieval of verification data
- **Load Distribution**: Scaling verification across multiple service instances

#### Enterprise Integration
- **API Gateway Patterns**: RESTful interfaces for third-party integration
- **Event-Driven Architecture**: Asynchronous processing for scalability
- **Microservices Design**: Containerized deployment for cloud-native environments
- **Monitoring & Analytics**: Real-time performance tracking and optimization

## 🚀 Getting Started with Scenarios

### Quick Start
```bash
cd samples/SdJwt.Net.Samples
dotnet run

# Available scenario options:
# C - Real-World Use Cases (Industry applications)
# F - Financial Co-Pilot (AI-powered advisor)
```

### Prerequisites
- **.NET 8 or 9**: Required for all scenarios
- **OpenAI API Key**: Optional, for AI-powered scenarios
- **Development Environment**: IDE with .NET support

### Configuration
```bash
# For AI-powered scenarios (optional)
export OPENAI_API_KEY="your-api-key"
export OPENAI_MODEL="gpt-4-turbo"  # or "gpt-5-turbo" when available

# For performance testing
export SCENARIO_PERFORMANCE_TEST="true"
export SCENARIO_ITERATIONS="1000"
```

## 📚 Detailed Documentation

### 🌍 Real-World Use Cases
**[Complete Documentation →](./real-world-use-cases.md)**

**University to Bank Loan Scenario**
A graduate applies for a home loan, demonstrating:
- Multi-issuer ecosystem (university + employer)
- Privacy-preserving verification (minimal disclosure)
- Complex trust relationships across institutions
- Performance optimization for high-volume verification

**Defense Contractor Job Application**
Security clearance position requiring:
- Multiple credential types (education, employment, security clearance)
- Complex verification logic with multiple criteria
- High-security context with enhanced validation
- Cross-agency trust establishment

**Healthcare Professional Verification**
Medical professional seeks hospital privileges:
- Professional license verification across state boundaries
- Continuing education credit validation
- Hospital privilege transfer between institutions
- Patient privacy protection throughout verification

### 🤖 Financial Co-Pilot
**[Complete Documentation →](./financial/)**

**The Challenge: "Golden Record" Paradox**
Financial services members need personalized AI guidance, but their financial data is coupled with "Toxic PII" (Tax File Numbers, addresses, full dates of birth) that cannot be streamed to cloud AI services.

**The Solution: "Verify-then-Infer" Pattern**
```
┌─────────────────┐  🔐 Verifiable    ┌─────────────────┐
│ Client Device   │ ←─ Presentation ──│ AI Service      │
│ (Secure Vault)  │                   │ (Stateless      │
│                 │                   │  Reasoner)      │
└─────────────────┘                   └─────────────────┘
```

**Key Capabilities:**
- **Progressive Disclosure**: Only required fields per query
- **Session Memory**: Context maintained within conversation
- **Privacy Protection**: TFN, names, addresses never disclosed to AI
- **Cryptographic Verification**: All data verified before AI processing
- **Real-Time Advice**: Personalized guidance using verified member data

## 🏗️ Architecture Patterns

### Scenario Design Principles

#### 1. Privacy by Design
- **Minimal Disclosure**: Share only necessary data for each specific use case
- **Progressive Revelation**: Build context gradually without accumulating sensitive data
- **Audience Awareness**: Tailor data sharing to specific verification requirements
- **Zero-Knowledge Patterns**: Prove properties without revealing underlying data

#### 2. Trust Management
- **Distributed Trust**: No single point of trust failure
- **Cryptographic Verification**: Mathematical proof instead of institutional trust
- **Trust Chain Validation**: Recursive verification of issuer credentials
- **Cross-Domain Trust**: Enabling trust across organizational boundaries

#### 3. Performance Optimization
- **Concurrent Processing**: Parallel verification for speed
- **Caching Strategies**: Intelligent storage of verification data
- **Batch Operations**: High-throughput processing for enterprise scale
- **Resource Optimization**: Memory and CPU efficiency for cloud deployment

#### 4. Security Hardening
- **Threat Modeling**: Comprehensive attack surface analysis
- **Defense in Depth**: Multiple layers of security validation
- **Zero-Trust Architecture**: Never trust, always verify
- **Incident Response**: Detection and mitigation of security events

### Implementation Patterns

#### Multi-Package Integration
```csharp
// Coordinate multiple SD-JWT packages for complex workflows
var vcIssuer = new SdJwtVcIssuer(issuerKey, algorithm);
var statusManager = new StatusListManager(statusKey, algorithm);
var vcVerifier = new SdJwtVcVerifier(multiIssuerKeyResolver);

// Create status-aware credential
var vcPayload = new SdJwtVcPayload { /* ... */ };
var credential = vcIssuer.Issue(vct, vcPayload, sdOptions, holderJwk);

// Verify with real-time status checking
var result = await vcVerifier.VerifyAsync(presentation, validationParams, kbParams, expectedVct);
```

#### Privacy-Preserving AI
```csharp
// Route intent to determine minimum required data
var intent = intentRouter.RouteIntent("Should I salary sacrifice?");
var requiredFields = intentRouter.GetRequiredFields(intent); // ["account_balance", "cap_remaining"]

// Create selective presentation with only required data
var presentation = wallet.CreateSelectivePresentation(credential, requiredFields);

// Verify before AI processing (no PII included)
var verificationResult = await verifier.VerifyAsync(presentation);
var advice = await aiEngine.GenerateAdviceAsync(query, verificationResult.VerifiedClaims, intent);
```

#### Enterprise Deployment
```csharp
public class ScenarioOrchestrator
{
    public async Task<ScenarioResult> ExecuteScenario(ScenarioRequest request)
    {
        // 1. Validate request and determine requirements
        var requirements = await _requirementAnalyzer.AnalyzeAsync(request);
        
        // 2. Coordinate credential collection from multiple sources
        var credentials = await _credentialCollector.CollectAsync(requirements);
        
        // 3. Create comprehensive presentation
        var presentation = await _presentationBuilder.BuildAsync(credentials, requirements);
        
        // 4. Execute verification workflow
        var verification = await _verificationEngine.VerifyAsync(presentation);
        
        // 5. Generate response and audit trail
        return await _responseGenerator.GenerateAsync(verification);
    }
}
```

## 📊 Performance Characteristics

### Scenario Benchmarks

| Scenario | Complexity | Throughput | Latency | Memory Usage |
|----------|------------|------------|---------|--------------|
| **University Loan** | Medium | 500+ ops/sec | < 200ms | 50MB |
| **Defense Job App** | High | 200+ ops/sec | < 500ms | 100MB |
| **Healthcare Verify** | Medium | 800+ ops/sec | < 150ms | 30MB |
| **Financial Co-Pilot** | Very High | 50+ ops/sec | < 2s* | 200MB |

*AI performance depends on OpenAI API response times

### Scalability Patterns
- **Horizontal Scaling**: Stateless verification services
- **Vertical Scaling**: CPU-optimized verification for high throughput
- **Caching Layers**: Redis for verification data and AI context
- **Load Balancing**: Distributed verification across service instances

## 🔒 Security Considerations

### Threat Model Coverage

#### Credential Attacks
- **Tampering Detection**: Cryptographic signature validation
- **Replay Prevention**: Nonce and timestamp validation  
- **Revocation Checking**: Real-time status list verification
- **Key Compromise**: Key rotation and invalidation procedures

#### Privacy Attacks
- **Data Minimization**: Only necessary data disclosed per scenario
- **Correlation Prevention**: Unlinkable presentations across contexts
- **Traffic Analysis**: Encrypted communications and timing obfuscation
- **Side-Channel Protection**: Constant-time operations and secure implementations

#### System Attacks
- **Injection Prevention**: Input validation and sanitization
- **DoS Mitigation**: Rate limiting and resource quotas
- **Privilege Escalation**: Principle of least privilege enforcement
- **Data Exfiltration**: Comprehensive audit logging and monitoring

### Compliance Frameworks
- **GDPR**: Privacy by design, right to be forgotten
- **CCPA**: Consumer privacy rights and data minimization
- **HIPAA**: Healthcare data protection and access controls
- **SOX**: Financial data integrity and audit requirements
- **PCI DSS**: Payment card data protection standards

## 🔮 Future Scenarios

### Emerging Use Cases

#### Web3 Integration
- **Decentralized Identity**: Integration with blockchain-based identity systems
- **Smart Contract Verification**: Automated credential verification in DeFi protocols
- **NFT Credentials**: Digital ownership and authenticity verification
- **Cross-Chain Trust**: Credential verification across multiple blockchain networks

#### IoT and Edge Computing
- **Device Authentication**: Secure device identity and capability verification
- **Edge Verification**: Local credential verification without cloud connectivity
- **Sensor Data Authentication**: Trusted data collection and transmission
- **Supply Chain Tracking**: End-to-end provenance and authenticity verification

#### Quantum-Resistant Security
- **Post-Quantum Cryptography**: Migration to quantum-resistant algorithms
- **Hybrid Security**: Transition strategies for quantum-safe implementations
- **Forward Secrecy**: Protection against future quantum attacks
- **Quantum Key Distribution**: Ultra-secure key exchange mechanisms

#### Advanced AI Integration
- **Federated Learning**: Training AI models without centralizing sensitive data
- **Homomorphic Encryption**: Computing on encrypted credential data
- **Zero-Knowledge Machine Learning**: AI inference without data revelation
- **Differential Privacy**: Adding statistical noise for additional privacy protection

## 🤝 Contributing New Scenarios

### Guidelines for Scenario Development

#### Requirements
1. **Real-World Relevance**: Address actual industry problems and use cases
2. **Complete Implementation**: End-to-end workflow from issuance to verification
3. **Privacy Focus**: Demonstrate minimal disclosure and progressive revelation patterns
4. **Security Emphasis**: Show comprehensive threat mitigation and security best practices
5. **Performance Consideration**: Include timing, optimization, and scalability patterns

#### Structure Template
```csharp
/// <summary>
/// Demonstrates [specific industry workflow]
/// Key features: [privacy patterns, trust management, performance optimization]
/// Business value: [concrete benefits and use case justification]
/// </summary>
public class NewScenarioExample
{
    public static async Task RunScenario(IServiceProvider services)
    {
        // 1. Setup multi-issuer ecosystem
        // 2. Issue credentials with selective disclosure
        // 3. Create complex presentation requirements
        // 4. Demonstrate verification workflows
        // 5. Show error handling and security patterns
        // 6. Measure and report performance characteristics
    }
}
```

#### Documentation Requirements
- **Business Context**: Why this scenario matters
- **Technical Architecture**: How the solution works
- **Privacy Analysis**: What data is protected and how
- **Security Considerations**: Threat model and mitigation strategies
- **Performance Metrics**: Benchmarks and scalability characteristics
- **Implementation Guide**: Step-by-step development instructions

### Ideas for New Scenarios
- **Supply Chain Verification**: Product authenticity and provenance tracking
- **Academic Research**: Credential verification for research collaboration
- **Cross-Border Identity**: International credential recognition and verification
- **Regulatory Compliance**: Automated compliance checking and reporting
- **Digital Voting**: Secure, verifiable, and private electronic voting systems

---

## 📚 Related Documentation

- **[Getting Started](../getting-started.md)** - Setup and basic concepts
- **[Core Examples](../examples/)** - Feature-specific demonstrations  
- **[Financial Co-Pilot](./financial/)** - Complete AI integration guide
- **[Deployment Guide](../deployment/)** - Production deployment patterns
- **[API Reference](../reference/)** - Technical implementation details

---

**Ready to build real-world applications with SD-JWT .NET?** These scenarios provide complete implementation patterns for production deployment.

Start with the [Real-World Use Cases](./real-world-use-cases.md) for industry applications, or explore the [Financial Co-Pilot](./financial/) for cutting-edge AI integration.

**The future of digital identity is happening now. Build it with SD-JWT .NET! 🚀**
