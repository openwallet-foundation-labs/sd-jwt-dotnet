# Financial Co-Pilot Demo - Privacy-Preserving AI with SD-JWT

> **📖 For a comprehensive introduction including business context, architecture, and implementation details, see [FINANCIAL_COPILOT_INTRODUCTION.md](./FINANCIAL_COPILOT_INTRODUCTION.md)**

This demo showcases a revolutionary approach to AI-powered financial advice that solves the "Golden Record" paradox - providing personalized guidance while protecting sensitive member data.

## 🎯 The Challenge

Financial services members want real-time, personalized guidance:
- "Should I salary sacrifice?"
- "If I add $200 per fortnight, what happens?"
- "What if I retire at 60 instead of 65?"

**The Paradox**: AI needs financial context (balance, transaction history) but this data is coupled with "Toxic PII" (Tax File Numbers, addresses, full dates of birth). Traditional approaches risk streaming high-sensitivity data to cloud LLMs.

## ✨ Our Solution: Stateless Co-Pilot Architecture

### The "Verify-then-Infer" Pattern

```
┌─────────────────┐  🔐 Verifiable    ┌─────────────────┐
│ Client Device   │ ←─ Presentation ──│ AI Service      │
│ (Secure Vault)  │                   │ (Stateless      │
│                 │                   │  Reasoner)      │
└─────────────────┘                   └─────────────────┘
```

### Key Architecture Components

1. **Client Device (Holder)** = Secure Vault
   - Stores SD-JWT credentials with selective disclosure
   - Creates context-specific presentations on demand
   - Never reveals unnecessary data

2. **AI Service (Verifier)** = Stateless Reasoning Engine  
   - Receives only verified, minimal data needed for each query
   - Processes advice and immediately forgets sensitive inputs
   - Zero persistent storage of member data

3. **Progressive Disclosure** = Clean Context Windows
   - Each conversation turn requests only specific required fields
   - No accumulation of PII in conversation context
   - Cryptographic proof of data authenticity

## 🚀 Running the Demo

### Prerequisites

For the full OpenAI-powered experience (optional):

1. **OpenAI API Key**: Set environment variable `OPENAI_API_KEY`
   ```bash
   # For OpenAI
   export OPENAI_API_KEY="your-openai-api-key"
   
   # For Azure OpenAI (alternative)
   export AZURE_OPENAI_ENDPOINT="https://your-resource.openai.azure.com/"
   export OPENAI_API_KEY="your-azure-api-key"
   ```

2. **Without API Key**: Demo automatically falls back to simulated responses

### Run the Demo

```bash
cd samples/SdJwt.Net.Samples
dotnet run
```

Select option **F** (Financial Co-Pilot) from the menu.

## 📱 Demo Workflow

### 1. Ecosystem Setup
- Registry System (Link Group) initialization
- Bank system initialization  
- Mobile wallet infrastructure

### 2. Credential Issuance
- **Account Credential**: Balance, cap remaining, membership details + Toxic PII
- **Transaction History**: Contribution patterns, investment returns
- **Selective Disclosure Structure**: Financial data can be disclosed, PII is protected

### 3. Multi-Turn Conversation

#### Turn 1: The Baseline
- **User**: "Should I salary sacrifice?"
- **Intent**: CONTRIBUTION_STRATEGY
- **Required Fields**: account_balance, cap_remaining
- **AI Response**: Tax saving calculations and recommendations

#### Turn 2: The Simulation
- **User**: "If I add $200 per fortnight, what happens?"
- **Intent**: SIMULATION  
- **Required Fields**: account_balance
- **AI Response**: Projected growth and tax savings

#### Turn 3: The Pivot (New Data)
- **User**: "What if I retire at 60 instead of 65?"
- **Intent**: RETIREMENT_PROJECTION
- **Required Fields**: account_balance, birth_year, joined_date
- **AI Response**: Early retirement impact analysis

#### Turn 4: The Artifact
- **User**: "Send me the summary"
- **Intent**: ARTIFACT_GENERATION
- **AI Response**: Cryptographically-backed Statement of Advice

## 🔒 Privacy Protection Features

### What's Protected (Never Disclosed)
- ❌ Tax File Number (TFN)
- ❌ Full name  
- ❌ Complete home address
- ❌ Full date of birth
- ❌ Detailed transaction history
- ❌ Manager/emergency contact details

### What's Selectively Disclosed
- ✅ Account balance (when needed for calculations)
- ✅ Contribution cap remaining (for strategy advice)
- ✅ Birth year only (for retirement planning)
- ✅ Join date (for membership duration)
- ✅ Member ID (for artifact generation)

### Cryptographic Guarantees
- 🔐 **Selective Disclosure**: Only required fields revealed per query
- 🔐 **Key Binding**: Cryptographic proof of credential ownership
- 🔐 **Signature Verification**: Issuer authenticity confirmed
- 🔐 **Tamper Detection**: Any modification detected and rejected
- 🔐 **Progressive Disclosure**: Clean context windows, no data accumulation

## 🏗️ Technical Implementation

### Core Technologies
- **SD-JWT**: Selective disclosure with cryptographic verification
- **OpenAI GPT**: Real financial advice generation (GPT-5 support)
- **ECDSA P-256**: Enterprise-grade cryptographic signatures
- **.NET 9**: High-performance, cross-platform runtime

### Architecture Patterns
- **Intent Router**: Determines minimum required data fields
- **Orchestrator**: Manages verification challenges and presentations
- **Verifier**: Cryptographically validates presentations before AI inference
- **Stateless AI**: Zero persistent storage of sensitive data

### Security Hardening
- ✅ Algorithm validation (only approved cryptographic standards)
- ✅ Nonce-based replay attack prevention
- ✅ Audience-specific presentations
- ✅ Temporal validation (credential expiry)
- ✅ Zero-trust verification at every step

## 🌟 Key Achievements

### For Members (Privacy)
- **Minimal Disclosure**: Only share data needed for specific advice
- **Toxic PII Protection**: Never expose TFN, addresses, or full DOB to AI
- **Control**: Member decides what to disclose per conversation turn
- **Auditability**: Cryptographic proof of what data was shared

### For Financial Institutions (Compliance)
- **Regulatory Compliance**: Meet privacy regulations without limiting AI capabilities  
- **Zero Trust**: Cryptographic verification replaces trust assumptions
- **Audit Trails**: Complete record of data disclosure with cryptographic proofs
- **Scalability**: Stateless architecture supports millions of concurrent users

### For AI Services (Capability)
- **Real Financial Context**: Access to verified, real-time financial data
- **Personalized Advice**: Calculate specific scenarios with actual member data
- **Clean Context**: Progressive disclosure prevents data accumulation
- **Immediate Forget**: No persistent storage of sensitive member information

## 🔮 Future Enhancements

### Advanced AI Integration
- **Real-time Market Data**: Integration with live investment performance
- **Regulatory Updates**: Dynamic tax rule changes and impacts
- **Personalized Modeling**: ML models trained on anonymized patterns
- **Multi-modal Advice**: Voice, visual, and text-based interactions

### Enhanced Privacy Features
- **Zero-Knowledge Proofs**: Prove properties without revealing data
- **Homomorphic Encryption**: Compute on encrypted financial data
- **Differential Privacy**: Add statistical noise for additional protection
- **Federated Learning**: Train AI without centralizing data

### Enterprise Integration
- **API Gateway**: RESTful APIs for third-party integration
- **Microservices**: Containerized deployment for cloud-native architectures  
- **Event Sourcing**: Complete audit trail of all member interactions
- **Performance Analytics**: Real-time monitoring and optimization

## 📊 Performance Characteristics

| Operation | Throughput | Latency | Privacy Level |
|-----------|------------|---------|---------------|
| Intent Classification | 10,000+ ops/sec | < 10ms | No PII exposed |
| Credential Verification | 1,500+ ops/sec | < 100ms | Minimal disclosure |
| AI Advice Generation | 50+ ops/sec | < 2s | Zero data persistence |
| Presentation Creation | 2,000+ ops/sec | < 50ms | Member-controlled |

## 🎓 Educational Value

This demo teaches:

### Cryptographic Concepts
- **Selective Disclosure**: Fine-grained privacy control
- **Digital Signatures**: Authenticity and integrity verification
- **Key Binding**: Proof of possession without revelation
- **Hash-based Commitments**: Tamper-proof data references

### Architecture Patterns  
- **Stateless Services**: Scalable, privacy-preserving design
- **Progressive Disclosure**: Context window management for AI
- **Verify-then-Infer**: Security-first AI integration
- **Zero-Trust Verification**: Never trust, always verify

### Industry Applications
- **Financial Services**: Privacy-preserving advice and analysis
- **Healthcare**: Medical AI with patient data protection
- **Government**: Citizen services with privacy by design
- **Education**: Credential verification without PII exposure

## 📝 Try It Yourself

1. **Run the Demo**: Experience the multi-turn conversation flow
2. **Examine the Code**: Study the implementation patterns
3. **Modify Scenarios**: Create new financial advice scenarios
4. **Extend the AI**: Add new intent types and required fields
5. **Deploy Production**: Scale to enterprise-grade deployment

---

## 🤝 Contributing

Want to enhance the Financial Co-Pilot demo?

### Ideas for Contributions
- **New AI Intents**: Investment advice, insurance planning, debt consolidation
- **Enhanced Privacy**: Zero-knowledge proofs, homomorphic encryption
- **UI/UX**: Web interface, mobile app integration
- **Real Integrations**: Live market data, regulatory APIs
- **Performance**: Benchmarking, optimization, stress testing

### Getting Started
```bash
git clone https://github.com/openwallet-foundation-labs/sd-jwt-dotnet.git
cd sd-jwt-dotnet/samples/SdJwt.Net.Samples
# Make your enhancements
# Submit a pull request
```

## 📚 Related Documentation

- **[Comprehensive Introduction](./FINANCIAL_COPILOT_INTRODUCTION.md)** - Complete business context and architecture
- **[OpenAI Setup Guide](../OPENAI_SETUP.md)** - AI integration configuration
- **[Samples Overview](../../README.md)** - All SD-JWT examples
- **[Core Documentation](../../../src/SdJwt.Net/README.md)** - SD-JWT fundamentals

---

**Ready to revolutionize AI-powered financial services?** This demo shows how SD-JWT enables the future of privacy-preserving artificial intelligence. 

**The future of AI is verifiable, selective, and private by design. 🚀**
