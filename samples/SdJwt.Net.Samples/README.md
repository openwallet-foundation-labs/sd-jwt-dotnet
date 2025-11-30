# SD-JWT .NET Samples - Comprehensive Developer Guide

This sample project provides **comprehensive demonstrations** of the entire SD-JWT .NET ecosystem, showcasing all packages and their integration capabilities. These samples are designed for developers who want to understand the full potential of selective disclosure and verifiable credentials in .NET applications.

## 🚀 Quick Start

```bash
cd samples/SdJwt.Net.Samples
dotnet run
```

Select from the interactive menu to explore specific features or run the complete demonstration.

## 📋 Sample Categories

### 🔧 Core Features
Essential SD-JWT functionality based on RFC 9901.

| Sample | Description | Key Features |
|--------|-------------|--------------|
| **Core SD-JWT** | Basic selective disclosure | Issuance, holder presentations, verification |
| **JSON Serialization** | JWS JSON serialization formats | Flattened/General JSON, round-trip conversion |

### 🆔 Verifiable Credentials
Standards-compliant verifiable credential implementations.

| Sample | Description | Key Features |
|--------|-------------|--------------|
| **Verifiable Credentials** | SD-JWT VC implementation | Medical licenses, education, employment |
| **Status Lists** | Revocation & suspension | BitArray status, multi-bit states, performance |

### 🔗 Protocol Integration
OpenID standards implementation for credential ecosystems.

| Sample | Description | Key Features |
|--------|-------------|--------------|
| **OpenID4VCI** | Credential issuance protocol | Pre-authorized flows, batch issuance, deferred |
| **OpenID4VP** | Presentation verification | Cross-device flows, complex requirements |
| **OpenID Federation** | Trust management | Entity configuration, trust chains, policies |
| **Presentation Exchange** | DIF credential selection | Complex requirements, intelligent selection |

### 🏗️ Advanced Features
Enterprise-grade capabilities and integration patterns.

| Sample | Description | Key Features |
|--------|-------------|--------------|
| **Comprehensive Integration** | Multi-package workflows | Nested disclosure, multi-credentials, status integration |
| **Cross-Platform Features** | .NET compatibility | Framework optimization, performance, deployment |
| **Security Features** | Security best practices | Attack prevention, privacy protection, threat mitigation |

### 🌍 Real-World Scenarios
Complete end-to-end workflows demonstrating practical applications.

| Sample | Description | Key Features |
|--------|-------------|--------------|
| **Real-World Scenarios** | Industry use cases | University-to-bank loans, job applications, healthcare |
| **Financial Co-Pilot** | AI-powered financial advisor | OpenAI integration, privacy-preserving AI, GPT-5 support |

## 🤖 AI-Powered Financial Co-Pilot

**NEW**: Experience the future of privacy-preserving AI with our Financial Co-Pilot scenario!

### Key Features
- **GPT-5 Support**: Latest OpenAI models for sophisticated financial reasoning
- **Privacy-Preserving**: Selective disclosure protects sensitive data (TFN, addresses)
- **Session Memory**: Maintains context within conversation, clears after session
- **Real-Time Advice**: Personalized financial guidance with verified data
- **Australian Focus**: Superannuation, tax implications, retirement planning

### Quick Start
```bash
# Optional: Enable real AI responses
export OPENAI_API_KEY="your-api-key-here"
export OPENAI_MODEL="gpt-5-turbo"  # Recommended

# Run the demo
dotnet run
# Select "F" for Financial Co-Pilot
```

See [Financial Co-Pilot Documentation](./Scenarios/Financial/README.md) for detailed setup and capabilities.

## 🔍 Detailed Sample Descriptions

### Core SD-JWT Features
**File:** `Examples/CoreSdJwtExample.cs`

Demonstrates the fundamental SD-JWT capabilities:
- **Selective Disclosure**: Make specific claims selectively disclosable
- **Key Binding**: Cryptographically bind credentials to holder keys
- **Nested Structures**: Complex claim structures with granular disclosure
- **Performance**: High-throughput issuance and verification
- **Security**: RFC 9901 compliance and algorithm validation

```csharp
// Example: Issue credential with selective disclosure
var options = new SdIssuanceOptions
{
    DisclosureStructure = new
    {
        given_name = true,      // Can be disclosed
        email = true,           // Can be disclosed
        address = new           // Nested disclosure
        {
            city = true,
            state = true
        }
    }
};

var credential = issuer.Issue(claims, options, holderJwk);
```

### JSON Serialization
**File:** `Examples/JsonSerializationExample.cs`

Advanced serialization formats beyond compact representation:
- **Flattened JSON**: Single signature with JSON structure
- **General JSON**: Multiple signatures support
- **Round-trip Conversion**: Lossless format conversion
- **Format Validation**: Ensure serialization integrity

### Verifiable Credentials (SD-JWT VC)
**File:** `Examples/VerifiableCredentialsExample.cs`

Real-world credential scenarios:
- **Medical Licenses**: Professional healthcare credentials
- **University Degrees**: Academic achievement verification
- **Employment Records**: Job position and salary verification
- **Context-Specific Disclosure**: Different presentations for different audiences

### Status Lists & Revocation
**File:** `Examples/StatusListExample.cs`

Comprehensive revocation and status management:
- **BitArray Status Lists**: Efficient storage for millions of credentials
- **Multi-bit States**: Support for revoked, suspended, under investigation
- **Performance Optimization**: High-throughput status checking
- **HTTP Integration**: Real-world status endpoint simulation

### OpenID4VCI Protocol
**File:** `Examples/OpenId4VciExample.cs`

Complete credential issuance workflows:
- **Pre-authorized Code Flow**: University degree issuance
- **Authorization Code Flow**: Government ID issuance
- **Batch Issuance**: Corporate onboarding packages
- **Deferred Issuance**: Manual approval workflows
- **QR Code Integration**: Mobile wallet workflows

### OpenID4VP Protocol
**File:** `Examples/OpenId4VpExample.cs`

Presentation and verification patterns:
- **Employment Verification**: Bank loan applications
- **Age Verification**: Privacy-preserving age proof
- **Cross-device Flows**: QR code to mobile presentation
- **Complex Requirements**: Multi-credential presentations

### Comprehensive Integration
**File:** `Examples/ComprehensiveIntegrationExample.cs`

**Advanced integration patterns that developers often need:**

#### 1. Advanced Selective Disclosure
- **Deep Nesting**: Complex claim structures with granular control
- **Context-Aware Presentations**: Different disclosure patterns for different scenarios
- **Privacy Optimization**: Minimal disclosure principles

#### 2. Multi-Credential Workflows
- **Orchestrated Issuance**: Multiple issuers, single holder
- **Complex Presentations**: Requirements spanning multiple credentials
- **Cross-Issuer Verification**: Unified verification across trust domains

#### 3. Status-Integrated Credentials
- **Real-time Status Checking**: Integration with revocation systems
- **Performance Optimization**: Efficient status validation
- **Workflow Integration**: Status checking in presentation flows

#### 4. Advanced Key Binding
- **Holder Identity Verification**: Cryptographic proof of possession
- **Multi-Holder Scenarios**: Same credential type, different holders
- **Replay Attack Prevention**: Timestamp and nonce validation

#### 5. Performance Optimizations
- **Batch Operations**: High-throughput issuance and verification
- **Memory Efficiency**: Large-scale credential management
- **Concurrent Processing**: Parallel verification workflows

### Cross-Platform Features
**File:** `Examples/CrossPlatformFeaturesExample.cs`

Platform compatibility and optimization:
- **Algorithm Support**: Testing cryptographic capabilities across platforms
- **Performance Characteristics**: Framework-specific optimizations
- **Deployment Scenarios**: Windows, Linux, containers, cloud
- **Version Compatibility**: .NET 8, .NET 9, .NET Standard 2.1

### Security Features
**File:** `Examples/SecurityFeaturesExample.cs`

**Comprehensive security implementation guide:**

#### 1. Cryptographic Algorithm Security
- **Approved Algorithms**: SHA-2 family enforcement
- **Blocked Weak Algorithms**: MD5, SHA-1 rejection
- **Signature Algorithm Testing**: ECDSA strength validation

#### 2. Attack Prevention
- **Signature Tampering**: Detection and prevention
- **Replay Attacks**: Nonce and timestamp validation
- **Timing Attacks**: Constant-time operation patterns
- **Disclosure Tampering**: Hash integrity protection

#### 3. Privacy Protection
- **Minimal Disclosure**: Principle of least privilege
- **Context-Specific Sharing**: Audience-appropriate information
- **Zero-Knowledge Patterns**: Identity proof without data exposure

#### 4. Key Management
- **Secure Generation**: Cryptographically strong keys
- **Key Rotation**: Backward compatibility with old credentials
- **Key Validation**: Property and usage verification

#### 5. Threat Mitigation
- **Security Checklist**: Implementation best practices
- **Vulnerability Prevention**: Common attack vectors
- **Operational Security**: Production deployment considerations

### Real-World Scenarios
**File:** `Scenarios/RealWorldScenariosExample.cs`

**Complete end-to-end workflows:**

#### 1. University to Bank Loan
A graduate applies for a home loan, demonstrating:
- **Multi-Issuer Ecosystem**: University + employer credentials
- **Privacy-Preserving Verification**: Minimal disclosure for loan approval
- **Trust Chain**: Cryptographic verification across institutions

#### 2. Job Application with Background Verification
Defense contractor position requiring:
- **Multi-Credential Requirements**: Education, employment, security clearance
- **Complex Verification Logic**: Multiple criteria satisfaction
- **High-Security Context**: Government contractor requirements

### Financial Co-Pilot Scenario
**File:** `Scenarios/FinancialCoPilotScenario.cs`

**AI-powered privacy-preserving financial advisor:**

#### Architecture Features
- **OpenAI Integration**: Real GPT-4/GPT-5 responses for financial advice
- **Session-Based Memory**: Maintains context within conversation session
- **Progressive Disclosure**: Only required data disclosed per query
- **Privacy Protection**: TFN, full names, addresses never sent to AI
- **Cryptographic Verification**: All data verified before AI processing

#### Conversation Flow
1. **Contribution Strategy**: "Should I salary sacrifice?"
2. **Growth Simulation**: "If I add $200 per fortnight, what happens?"  
3. **Retirement Planning**: "What if I retire at 60 instead of 65?"
4. **Summary Generation**: "Send me the summary"

#### Technical Implementation
- **Intent Router**: Determines required data fields per query
- **Wallet Simulator**: Creates selective presentations
- **Presentation Verifier**: Cryptographically validates data
- **OpenAI Engine**: Generates personalized advice with session context

#### Documentation
- **[Financial Co-Pilot Overview](./Scenarios/Financial/README.md)** - Complete demo guide
- **[Business Context & Architecture](./Scenarios/Financial/FINANCIAL_COPILOT_INTRODUCTION.md)** - Comprehensive analysis
- **[OpenAI Setup](./Scenarios/OPENAI_SETUP.md)** - AI integration configuration

## 🛠️ Development Patterns

### Basic Usage Pattern
```csharp
// 1. Create issuer
var issuer = new SdIssuer(signingKey, SecurityAlgorithms.EcdsaSha256);

// 2. Define selective disclosure
var options = new SdIssuanceOptions
{
    DisclosureStructure = new { sensitive_claim = true }
};

// 3. Issue credential
var credential = issuer.Issue(claims, options, holderJwk);

// 4. Create presentation
var holder = new SdJwtHolder(credential.Issuance);
var presentation = holder.CreatePresentation(
    disclosure => disclosure.ClaimName == "sensitive_claim",
    keyBindingJwt, holderPrivateKey, algorithm);

// 5. Verify presentation
var verifier = new SdVerifier(keyResolver);
var result = await verifier.VerifyAsync(presentation, validationParams);
```

### Advanced Integration Pattern
```csharp
// Multi-package integration
var vcIssuer = new SdJwtVcIssuer(issuerKey, algorithm);
var statusManager = new StatusListManager(statusKey, algorithm);
var vcVerifier = new SdJwtVcVerifier(multiIssuerKeyResolver);

// Create status-aware credential
var vcPayload = new SdJwtVcPayload
{
    // ... credential data
    Status = new { status_list = statusReference }
};

var credential = vcIssuer.Issue(vct, vcPayload, sdOptions, holderJwk);

// Verify with status checking
var result = await vcVerifier.VerifyAsync(
    presentation, validationParams, kbParams, expectedVct);
```

## 🏗️ Architecture Patterns

### Enterprise Integration
```csharp
public class CredentialService
{
    private readonly SdIssuer _issuer;
    private readonly StatusListManager _statusManager;
    private readonly IKeyResolver _keyResolver;
    
    public async Task<CredentialResult> IssueCredential(
        CredentialRequest request)
    {
        // 1. Validate request
        // 2. Create selective disclosure options
        // 3. Issue credential with status reference
        // 4. Store for revocation management
        // 5. Return to holder
    }
    
    public async Task<VerificationResult> VerifyPresentation(
        PresentationRequest request)
    {
        // 1. Resolve issuer keys
        // 2. Verify signature and structure
        // 3. Check status lists
        // 4. Validate key binding
        // 5. Return verification result
    }
}
```

### Multi-Tenant Architecture
```csharp
public class TenantCredentialManager
{
    public async Task<Credential> IssueTenantCredential(
        string tenantId, CredentialData data)
    {
        var tenantKey = await _keyManager.GetTenantKey(tenantId);
        var tenantOptions = await _configService.GetTenantOptions(tenantId);
        
        return _issuer.Issue(data, tenantOptions, holderJwk);
    }
}
```

## 🔧 Configuration Examples

### Dependency Injection Setup
```csharp
services.AddSingleton<SdIssuer>(sp =>
{
    var key = sp.GetRequiredService<ECDsaSecurityKey>();
    return new SdIssuer(key, SecurityAlgorithms.EcdsaSha256);
});

services.AddSingleton<SdJwtVcIssuer>(sp =>
{
    var key = sp.GetRequiredService<ECDsaSecurityKey>();
    return new SdJwtVcIssuer(key, SecurityAlgorithms.EcdsaSha256);
});

services.AddSingleton<StatusListManager>(sp =>
{
    var key = sp.GetRequiredService<ECDsaSecurityKey>();
    return new StatusListManager(key, SecurityAlgorithms.EcdsaSha256);
});
```

### Production Configuration
```csharp
services.Configure<SdJwtOptions>(options =>
{
    options.DefaultHashAlgorithm = "SHA-256";
    options.AllowWeakAlgorithms = false;
    options.KeyRotationInterval = TimeSpan.FromDays(365);
    options.StatusListCacheDuration = TimeSpan.FromHours(1);
});
```

## 📊 Performance Benchmarks

The samples include performance demonstrations showing:

| Operation | Throughput | Latency |
|-----------|------------|---------|
| Credential Issuance | 1,000+ ops/sec | < 1ms |
| Presentation Creation | 2,000+ ops/sec | < 0.5ms |
| Verification | 1,500+ ops/sec | < 0.7ms |
| Status List Check | 10,000+ ops/sec | < 0.1ms |
| AI Advice Generation* | 50+ ops/sec | < 2s |

*With OpenAI API integration

## 🔒 Security Features

### Algorithm Security
- ✅ **SHA-256, SHA-384, SHA-512**: Approved and secure
- ❌ **MD5, SHA-1**: Blocked for security
- ✅ **ECDSA P-256/P-384/P-521**: Recommended signatures
- ✅ **RSA 2048+**: Supported for compatibility

### Attack Prevention
- **Signature Tampering**: Cryptographic detection
- **Replay Attacks**: Nonce and timestamp validation
- **Timing Attacks**: Constant-time operations
- **Data Leakage**: Minimal disclosure enforcement

### Privacy Protection
- **Selective Disclosure**: Granular claim control
- **Key Binding**: Holder authentication
- **Zero-Knowledge Patterns**: Identity without data
- **Context Awareness**: Audience-specific sharing

## 🌐 Deployment Scenarios

### Cloud Platforms
- **Azure**: App Service, Functions, Kubernetes
- **AWS**: Lambda, ECS, EKS
- **Google Cloud**: Cloud Run, GKE

### Container Deployment
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "SdJwt.Net.Samples.dll"]
```

### Kubernetes
```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: sdjwt-samples
spec:
  replicas: 3
  selector:
    matchLabels:
      app: sdjwt-samples
  template:
    metadata:
      labels:
        app: sdjwt-samples
    spec:
      containers:
      - name: sdjwt
        image: your-registry/sdjwt-samples:latest
        ports:
        - containerPort: 8080
```

## 📚 Learning Path

### Beginner
1. **Core SD-JWT Example**: Basic concepts
2. **Verifiable Credentials**: Real-world credentials
3. **JSON Serialization**: Alternative formats

### Intermediate
4. **Status Lists**: Revocation management
5. **OpenID4VCI**: Issuance protocols
6. **OpenID4VP**: Presentation protocols

### Advanced
7. **Comprehensive Integration**: Multi-package workflows
8. **Security Features**: Production security
9. **Cross-Platform Features**: Deployment optimization

### Expert
10. **Real-World Scenarios**: Complete ecosystems
11. **Financial Co-Pilot**: AI-powered privacy-preserving applications
12. **Custom Integration**: Your specific use cases

## 🤝 Contributing

To add new samples or enhance existing ones:

1. **Follow the Pattern**: Use the established sample structure
2. **Comprehensive Comments**: Explain each step clearly
3. **Error Handling**: Demonstrate proper error patterns
4. **Performance Considerations**: Include timing where relevant
5. **Security Best Practices**: Show secure implementations

### Sample Template
```csharp
/// <summary>
/// Demonstrates [specific feature] with [specific focus]
/// Shows [key capabilities] and [important considerations]
/// </summary>
public class NewFeatureExample
{
    public static async Task RunExample(IServiceProvider services)
    {
        // 1. Setup and explanation
        // 2. Demonstrate core functionality
        // 3. Show variations and options
        // 4. Include error handling
        // 5. Performance considerations
        // 6. Security implications
        // 7. Best practices summary
    }
}
```

## 📄 License

This sample code is provided under the same license as the SD-JWT .NET library (Apache 2.0).

---

**Ready to build with SD-JWT .NET?** Start with the Core example and work through the samples that match your use case. Each sample is self-contained but builds upon concepts from previous examples.

For production deployments, pay special attention to the **Security Features** and **Cross-Platform Features** examples, which cover enterprise-grade implementation considerations.

For AI-powered applications, explore the **Financial Co-Pilot** scenario to see how SD-JWT enables privacy-preserving AI with real-world financial advice capabilities.
