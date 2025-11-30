# SD-JWT .NET Samples - Professional Organization Structure

This directory contains comprehensive examples and real-world scenarios demonstrating the complete SD-JWT .NET ecosystem. The samples are professionally organized into logical categories for easy navigation and learning progression.

## 🏗️ **New Organization Structure**

### **Core** - Fundamental SD-JWT Features
```
Core/
├── CoreSdJwtExample.cs         # RFC 9901 compliant SD-JWT basics
├── JsonSerializationExample.cs # JWS JSON serialization patterns  
└── SecurityFeaturesExample.cs  # Security best practices & validation
```
**Learning Focus**: Master the fundamental concepts of selective disclosure, key binding, and cryptographic verification.

### **Standards** - Protocol & Standards Compliance
```
Standards/
├── VerifiableCredentials/
│   ├── VerifiableCredentialsExample.cs  # SD-JWT VC implementation
│   └── StatusListExample.cs             # Credential lifecycle management
├── OpenId/
│   ├── OpenId4VciExample.cs            # Credential issuance protocols
│   ├── OpenId4VpExample.cs             # Presentation protocols
│   └── OpenIdFederationExample.cs      # Trust management
└── PresentationExchange/
    └── PresentationExchangeExample.cs   # DIF PE v2.0.0 integration
```
**Learning Focus**: Understand industry standards and protocols for interoperable credential ecosystems.

### **Integration** - Advanced Multi-Package Features
```
Integration/
├── ComprehensiveIntegrationExample.cs  # Full ecosystem workflows
└── CrossPlatformFeaturesExample.cs     # Platform compatibility patterns
```
**Learning Focus**: Learn how to combine multiple packages for production-ready solutions.

### **RealWorld** - Production-Ready Scenarios
```
RealWorld/
├── RealWorldScenarios.cs              # Industry use case patterns
└── Financial/
    ├── FinancialCoPilotScenario.cs     # Privacy-preserving AI advisor
    ├── EnhancedFinancialCoPilotScenario.cs  # Full ecosystem integration
    ├── OpenAiAdviceEngine.cs           # AI integration patterns
    └── README.md                       # Scenario-specific documentation
```
**Learning Focus**: Apply SD-JWT concepts to solve real business problems with production-quality implementations.

### **Infrastructure** - Supporting Code & Configuration
```
Infrastructure/
├── Configuration/
│   └── CachedJsonSerializerOptions.cs  # Shared JSON configuration
└── Data/
    ├── SampleIssuanceFile.cs           # Data models
    ├── sample-data.json                # Test data
    ├── sample-issuance.json            # Sample credentials
    └── presentation-definitions.json   # PE definitions
```
**Supporting Elements**: Reusable infrastructure components and test data.

## 🚀 **Getting Started**

### **Prerequisites**
- .NET 9.0 or later
- Optional: OpenAI API key for AI-powered scenarios

### **Run the Demo**
```bash
cd samples/SdJwt.Net.Samples
dotnet run
```

### **Learning Progression**

#### **1. Beginner Path** (30-45 minutes)
1. **Core SD-JWT Features** - Understand selective disclosure basics
2. **Security Features** - Learn cryptographic validation patterns  
3. **JSON Serialization** - Master different serialization formats

#### **2. Standards Path** (60-90 minutes)  
1. **Verifiable Credentials** - Industry-standard credential formats
2. **OpenID4VCI** - Credential issuance protocols
3. **OpenID4VP** - Presentation and verification workflows
4. **Status Lists** - Credential lifecycle management

#### **3. Advanced Path** (90-120 minutes)
1. **Presentation Exchange** - Intelligent credential selection
2. **OpenID Federation** - Trust chain management  
3. **Comprehensive Integration** - Multi-package workflows
4. **Cross-Platform Features** - Production deployment patterns

#### **4. Real-World Applications** (60+ minutes)
1. **Financial Co-Pilot** - Privacy-preserving AI with SD-JWT
2. **Industry Scenarios** - Healthcare, education, government use cases
3. **Enhanced Integration** - Full ecosystem with all 6 packages

## 📚 **Documentation References**

### **Complete Documentation Hub**
- **[Main Documentation](../../../../docs/samples/README.md)** - Comprehensive samples overview
- **[Getting Started Guide](../../../../docs/samples/getting-started.md)** - Step-by-step setup
- **[Scenarios Overview](../../../../docs/samples/scenarios/README.md)** - Real-world applications

### **Financial Co-Pilot Deep Dive**
- **[Financial Co-Pilot Hub](../../../../docs/samples/scenarios/financial/README.md)** - Complete feature guide
- **[Business Introduction](../../../../docs/samples/scenarios/financial/introduction.md)** - Business context & architecture  
- **[Enhanced Features](../../../../docs/samples/scenarios/financial/enhanced-features.md)** - Full ecosystem integration
- **[OpenAI Setup](../../../../docs/samples/scenarios/financial/openai-setup.md)** - AI integration configuration

### **API References**
- **[Core Package](../../src/SdJwt.Net/README.md)** - Fundamental SD-JWT operations
- **[VC Package](../../src/SdJwt.Net.Vc/README.md)** - Verifiable Credentials support
- **[Status List](../../src/SdJwt.Net.StatusList/README.md)** - Credential lifecycle management

## 🎯 **Key Learning Outcomes**

After completing these samples, you will understand:

### **Core Concepts**
- ✅ Selective disclosure principles and implementation
- ✅ Cryptographic verification and key binding
- ✅ Privacy-by-design architecture patterns
- ✅ RFC 9901 compliance and interoperability

### **Standards Integration**  
- ✅ W3C Verifiable Credentials with SD-JWT
- ✅ OpenID4VCI credential issuance workflows
- ✅ OpenID4VP presentation protocols
- ✅ DIF Presentation Exchange intelligent selection
- ✅ Status List credential lifecycle management
- ✅ OpenID Federation trust chains

### **Production Readiness**
- ✅ Multi-package integration patterns
- ✅ Performance optimization techniques
- ✅ Security hardening and validation
- ✅ Cross-platform deployment strategies
- ✅ Real-world scenario implementations

### **Advanced Applications**
- ✅ Privacy-preserving AI integration  
- ✅ Financial services use cases
- ✅ Healthcare credential management
- ✅ Government identity solutions
- ✅ Enterprise SSO implementations

## 🔧 **Technical Features Demonstrated**

### **Cryptographic Operations**
- ECDSA P-256 signature generation and verification
- SHA-256 hash-based claim digests
- Key binding proof construction
- Multi-signature validation

### **Data Formats & Serialization**
- JWT compact serialization  
- JWS JSON serialization (Flattened and General)
- SD-JWT disclosure format
- Base64url encoding/decoding

### **Protocol Compliance**
- RFC 9901 (SD-JWT)
- RFC 9902 (SD-JWT VC)  
- OpenID4VCI 1.0
- OpenID4VP 1.0
- DIF PE v2.0.0
- W3C VC Data Model 2.0
- OAuth 2.0 security best practices

### **Enterprise Features**
- Dependency injection patterns
- Comprehensive logging
- Error handling strategies
- Performance monitoring
- Memory management
- Resource cleanup

## 🤝 **Contributing**

We welcome contributions to improve these samples:

### **Enhancement Ideas**
- **New Industry Scenarios** - Healthcare, education, government examples
- **Advanced Integration Patterns** - Complex multi-issuer workflows  
- **Performance Optimizations** - Benchmarking and optimization examples
- **Security Hardening** - Additional validation and security patterns
- **UI/UX Examples** - Web and mobile integration samples

### **Getting Started with Contributions**
1. **Fork the repository** and create a feature branch
2. **Follow the new organization structure** when adding examples
3. **Include comprehensive documentation** with your examples
4. **Add tests** to verify your examples work correctly  
5. **Submit a pull request** with clear description of changes

### **Code Standards**
- Follow C# naming conventions and best practices
- Include XML documentation for all public methods
- Add comprehensive error handling and validation
- Use dependency injection for testability
- Include performance considerations in documentation

---

## 🎉 **Ready to Explore?**

Start with `dotnet run` and choose your learning path! Each example builds on previous concepts while being self-contained enough to study independently.

**The SD-JWT .NET ecosystem is production-ready and waiting for you to build the future of privacy-preserving digital credentials!** 🚀

---

*Last updated: January 2025 | SD-JWT .NET v2.x.x | Professional Sample Organization*
