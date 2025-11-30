# SD-JWT .NET Samples - Documentation Index

## 📚 Documentation Structure

This directory contains comprehensive documentation and examples for the SD-JWT .NET ecosystem.

### 🎯 Getting Started
- **[Main README](./README.md)** - Complete developer guide with all examples
- **[Quick Start Guide](#quick-start)** - Get running in 5 minutes

### 📖 Core Examples
| Example | File | Description |
|---------|------|-------------|
| **Core SD-JWT** | [`Examples/CoreSdJwtExample.cs`](./Examples/CoreSdJwtExample.cs) | RFC 9901 basic functionality |
| **JSON Serialization** | [`Examples/JsonSerializationExample.cs`](./Examples/JsonSerializationExample.cs) | Alternative serialization formats |
| **Verifiable Credentials** | [`Examples/VerifiableCredentialsExample.cs`](./Examples/VerifiableCredentialsExample.cs) | SD-JWT VC implementation |
| **Status Lists** | [`Examples/StatusListExample.cs`](./Examples/StatusListExample.cs) | Revocation and suspension |

### 🔗 Protocol Integration
| Protocol | File | Description |
|----------|------|-------------|
| **OpenID4VCI** | [`Examples/OpenId4VciExample.cs`](./Examples/OpenId4VciExample.cs) | Credential issuance flows |
| **OpenID4VP** | [`Examples/OpenId4VpExample.cs`](./Examples/OpenId4VpExample.cs) | Presentation verification |
| **OpenID Federation** | [`Examples/OpenIdFederationExample.cs`](./Examples/OpenIdFederationExample.cs) | Trust management |
| **Presentation Exchange** | [`Examples/PresentationExchangeExample.cs`](./Examples/PresentationExchangeExample.cs) | DIF credential selection |

### 🏗️ Advanced Features
| Feature | File | Description |
|---------|------|-------------|
| **Comprehensive Integration** | [`Examples/ComprehensiveIntegrationExample.cs`](./Examples/ComprehensiveIntegrationExample.cs) | Multi-package workflows |
| **Cross-Platform** | [`Examples/CrossPlatformFeaturesExample.cs`](./Examples/CrossPlatformFeaturesExample.cs) | .NET compatibility |
| **Security Features** | [`Examples/SecurityFeaturesExample.cs`](./Examples/SecurityFeaturesExample.cs) | Production security |

### 🌍 Real-World Scenarios
| Scenario | File | Description |
|----------|------|-------------|
| **Industry Use Cases** | [`Scenarios/RealWorldScenariosExample.cs`](./Scenarios/RealWorldScenariosExample.cs) | Complete workflows |
| **Financial Co-Pilot** | [`Scenarios/FinancialCoPilotScenario.cs`](./Scenarios/FinancialCoPilotScenario.cs) | AI-powered advisor |

### 🤖 AI Integration Documentation
- **[Financial Co-Pilot Overview](./Scenarios/Financial/README.md)** - Privacy-preserving AI architecture
- **[Financial Co-Pilot Introduction](./Scenarios/Financial/FINANCIAL_COPILOT_INTRODUCTION.md)** - Business context, problem, solution, and architecture
- **[OpenAI Setup Guide](./Scenarios/OPENAI_SETUP.md)** - Configuration and model support
- **[GPT-5 Integration](./Scenarios/OPENAI_SETUP.md#gpt-5-enhanced-features)** - Latest AI capabilities

## 🚀 Quick Start

### 1. Run All Examples
```bash
cd samples/SdJwt.Net.Samples
dotnet run
# Select from interactive menu
```

### 2. Core SD-JWT Demo
```bash
dotnet run
# Press "1" for Core SD-JWT Features
```

### 3. AI-Powered Financial Co-Pilot
```bash
# Optional: Configure OpenAI
export OPENAI_API_KEY="your-api-key"
export OPENAI_MODEL="gpt-5-turbo"

dotnet run
# Press "F" for Financial Co-Pilot
```

## 📋 Sample Categories by Complexity

### 🟢 Beginner (Start Here)
1. **Core SD-JWT** - Basic concepts and operations
2. **JSON Serialization** - Alternative data formats
3. **Verifiable Credentials** - Real-world credential types

### 🟡 Intermediate
4. **Status Lists** - Revocation management
5. **OpenID4VCI** - Credential issuance protocols
6. **OpenID4VP** - Presentation verification protocols

### 🟠 Advanced
7. **OpenID Federation** - Trust chain management
8. **Presentation Exchange** - Complex requirements
9. **Comprehensive Integration** - Multi-package workflows

### 🔴 Expert
10. **Security Features** - Production hardening
11. **Cross-Platform** - Deployment optimization
12. **Real-World Scenarios** - Complete ecosystems
13. **Financial Co-Pilot** - AI integration patterns

## 🔧 Development Workflow

### For Learning SD-JWT
```bash
# Follow this sequence:
dotnet run  # Select "1" - Core SD-JWT
dotnet run  # Select "3" - Verifiable Credentials  
dotnet run  # Select "5" - OpenID4VCI
dotnet run  # Select "9" - Comprehensive Integration
```

### For Production Implementation
```bash
# Focus on these examples:
dotnet run  # Select "B" - Security Features
dotnet run  # Select "A" - Cross-Platform Features
dotnet run  # Select "C" - Real-World Scenarios
```

### For AI Integration
```bash
# Configure OpenAI first, then:
dotnet run  # Select "F" - Financial Co-Pilot
# Study: Scenarios/FinancialCoPilotScenario.cs
# Read: Scenarios/Financial/README.md
```

## 📊 Performance Benchmarks

| Operation | Throughput | Latency | Example |
|-----------|------------|---------|---------|
| Credential Issuance | 1,000+ ops/sec | < 1ms | Core SD-JWT |
| Presentation Creation | 2,000+ ops/sec | < 0.5ms | All examples |
| Verification | 1,500+ ops/sec | < 0.7ms | All examples |
| Status Check | 10,000+ ops/sec | < 0.1ms | Status Lists |
| AI Advice* | 50+ ops/sec | < 2s | Financial Co-Pilot |

*Depends on OpenAI model and API performance

## 🔒 Security Considerations

### Production Checklist
- [ ] Review **Security Features** example
- [ ] Implement key rotation (Cross-Platform example)
- [ ] Configure status lists (Status Lists example)
- [ ] Test threat scenarios (Comprehensive Integration)
- [ ] Validate algorithm support (Cross-Platform example)

### AI Integration Security
- [ ] Environment variable configuration (OpenAI Setup)
- [ ] Data minimization verification (Financial Co-Pilot)
- [ ] Session management testing (Financial Co-Pilot)
- [ ] Privacy audit (Financial Co-Pilot README)

## 🤝 Contributing to Documentation

### Adding New Examples
1. Follow the pattern in existing examples
2. Include comprehensive comments
3. Add to main README.md
4. Update this documentation index
5. Test on all target frameworks

### Improving Documentation
1. Keep examples up-to-date with latest packages
2. Ensure cross-platform compatibility
3. Add performance considerations
4. Include security implications
5. Provide real-world context

### Documentation Standards
- **Clear Explanations**: Every step explained
- **Complete Examples**: Self-contained demos
- **Error Handling**: Show proper patterns
- **Performance Notes**: Include timing considerations
- **Security Focus**: Highlight security implications

## 📄 Related Documentation

### Package-Specific READMEs
- [Core](../../README-Core.md) - SD-JWT core functionality
- [VC](../../README-Vc.md) - Verifiable Credentials
- [StatusList](../../README-StatusList.md) - Revocation management
- [OID4VCI](../../README-Oid4Vci.md) - Credential issuance
- [OID4VP](../../README-Oid4Vp.md) - Presentation verification
- [OID Federation](../../README-OidFederation.md) - Trust management
- [Presentation Exchange](../../README-PresentationExchange.md) - Credential selection

### External Resources
- [SD-JWT RFC 9901](https://tools.ietf.org/rfc/rfc9901.html) - Official specification
- [OpenID4VCI](https://openid.net/specs/openid-4-verifiable-credential-issuance-1_0.html) - Issuance protocol
- [OpenID4VP](https://openid.net/specs/openid-4-verifiable-presentations-1_0.html) - Presentation protocol
- [OpenAI API](https://platform.openai.com/docs) - AI integration reference

---

**Ready to build with SD-JWT .NET?** Start with the [Main README](./README.md) and follow the learning path that matches your needs!

For questions or contributions, see the main repository [Contributing Guide](../../CONTRIBUTING.md).
