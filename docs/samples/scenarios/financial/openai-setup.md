# OpenAI Setup Guide for Financial Co-Pilot (Updated 2025)

This guide provides comprehensive instructions for setting up OpenAI integration with the Financial Co-Pilot scenario, enabling real AI-powered financial advice generation with the latest models and optimizations.

## Table of Contents

- [Overview](#overview)
- [OpenAI Platform Setup](#openai-platform-setup)
- [Azure OpenAI Setup](#azure-openai-setup)
- [Environment Configuration](#environment-configuration)
- [Model Selection (2025 Update)](#model-selection-2025-update)
- [Cost Optimization](#cost-optimization)
- [Error Handling](#error-handling)
- [Troubleshooting](#troubleshooting)

## Overview

The Financial Co-Pilot can operate in two modes:

1. **Simulated Mode** (Default): High-quality simulated responses demonstrating all privacy-preserving patterns
2. **AI-Powered Mode**: Real OpenAI integration with latest models for genuine financial advice

Both modes demonstrate the complete SD-JWT integration patterns with all 6 packages, but AI-powered mode provides authentic, sophisticated financial guidance using the latest AI capabilities.

## OpenAI Platform Setup

### 1. Create OpenAI Account

1. Visit [OpenAI Platform](https://platform.openai.com/)
2. Sign up for an account or log in
3. Complete account verification (may require phone verification)
4. Set up billing (required for API access)
5. Consider upgrading to higher usage tiers for better rate limits

### 2. Generate API Key

1. Navigate to [API Keys](https://platform.openai.com/api-keys)
2. Click "Create new secret key"
3. Give it a descriptive name (e.g., "SD-JWT-Financial-Co-Pilot-2025")
4. **Set permissions**: Select appropriate scope (recommended: All or Custom with model access)
5. Copy the key immediately (it won't be shown again)
6. Store it securely using environment variables or key management systems

### 3. Understanding Usage and Pricing (2025 Rates)

#### Current Pricing Overview

| Model | Input Cost | Output Cost | Context | Best Use Case |
|-------|------------|-------------|---------|---------------|
| **GPT-4o** | $2.50/1M tokens | $10.00/1M tokens | 128K | **Recommended** - Production balance |
| **GPT-4-turbo** | $10.00/1M tokens | $30.00/1M tokens | 128K | High-quality analysis |
| **GPT-4o-mini** | $0.15/1M tokens | $0.60/1M tokens | 128K | Development/testing |
| **o1-preview** | $15.00/1M tokens | $60.00/1M tokens | 128K | Complex reasoning |
| **o1-mini** | $3.00/1M tokens | $12.00/1M tokens | 128K | Efficient reasoning |

#### Financial Co-Pilot Token Usage

**Typical conversation (4 turns) breakdown**:

```
Enhanced Financial Co-Pilot Session:
- System Prompt: ~1,200 tokens (includes financial context)
- User Queries: ~250 tokens/turn = 1,000 tokens
- AI Responses: ~800 tokens/turn = 3,200 tokens  
- Session context: ~600 tokens
- Total per session: ~6,000 tokens

Cost Estimates (per conversation):
- GPT-4o: ~$0.045 (Recommended for production)
- GPT-4-turbo: ~$0.16 (Premium analysis)
- GPT-4o-mini: ~$0.005 (Development)
- o1-preview: ~$0.45 (Complex financial modeling)
```

### 4. Set Usage Limits and Monitoring

1. Go to [Usage Limits](https://platform.openai.com/account/limits)
2. Set monthly spending limits:
   - **Development**: $10-25/month
   - **Production**: $100-500/month based on usage
   - **Enterprise**: Custom limits
3. Enable email notifications for usage alerts
4. Set up monitoring dashboards for cost tracking

## Azure OpenAI Setup

### 1. Create Azure OpenAI Resource

1. Sign in to [Azure Portal](https://portal.azure.com/)
2. Create new resource → Search "OpenAI" → Select "Azure OpenAI"
3. Configure:
   - **Subscription**: Your Azure subscription
   - **Resource Group**: Create new or use existing
   - **Region**: Choose supported region (East US, West Europe, Sweden Central, etc.)
   - **Name**: Unique name for your resource
   - **Pricing Tier**: Standard S0

### 2. Deploy Models (2025 Models)

1. Go to your Azure OpenAI resource
2. Navigate to "Model deployments" or use Azure OpenAI Studio
3. Click "Create new deployment"
4. Configure recommended models:

#### Primary Production Model

```
Model: gpt-4o
Deployment Name: financial-copilot-gpt4o
Version: Latest available
Capacity: 50 TPM (tokens per minute)
```

#### Alternative/Backup Model  

```
Model: gpt-4-turbo
Deployment Name: financial-copilot-gpt4-turbo
Version: Latest available
Capacity: 30 TPM
```

#### Development Model

```
Model: gpt-4o-mini
Deployment Name: financial-copilot-mini
Version: Latest available
Capacity: 100 TPM
```

### 3. Get Connection Information

1. Go to "Keys and Endpoint" in your Azure OpenAI resource
2. Copy:
   - **Endpoint**: e.g., `https://your-resource.openai.azure.com/`
   - **Key**: Primary or secondary key
   - **API Version**: Use latest (e.g., `2024-08-01-preview`)

## Environment Configuration

### Windows (PowerShell)

#### OpenAI Configuration (2025)

```powershell
# Primary recommendation: GPT-4o
$env:OPENAI_API_KEY = "sk-your-openai-api-key-here"
$env:OPENAI_MODEL = "gpt-4o"

# Alternative models for different scenarios
# For complex financial modeling
$env:OPENAI_MODEL = "o1-preview"

# For development and testing
$env:OPENAI_MODEL = "gpt-4o-mini"

# For high-quality traditional analysis
$env:OPENAI_MODEL = "gpt-4-turbo"

# Advanced configuration
$env:OPENAI_TEMPERATURE = "0.1"  # Very deterministic for financial advice
$env:OPENAI_MAX_TOKENS = "1500"  # Detailed responses
$env:OPENAI_TOP_P = "0.9"

# Verify configuration
Write-Host "OpenAI API Key: $($env:OPENAI_API_KEY.Substring(0,10))..."
Write-Host "Model: $env:OPENAI_MODEL"
Write-Host "Temperature: $env:OPENAI_TEMPERATURE"
```

#### Azure OpenAI Configuration (2025)

```powershell
# Set Azure OpenAI endpoint and key
$env:AZURE_OPENAI_ENDPOINT = "https://your-resource.openai.azure.com/"
$env:OPENAI_API_KEY = "your-azure-openai-key"

# Use deployment name (not model name) 
$env:OPENAI_MODEL = "financial-copilot-gpt4o"

# Azure API version (latest)
$env:AZURE_OPENAI_API_VERSION = "2024-08-01-preview"

# Verify configuration
Write-Host "Azure Endpoint: $env:AZURE_OPENAI_ENDPOINT"
Write-Host "Deployment: $env:OPENAI_MODEL"
Write-Host "API Version: $env:AZURE_OPENAI_API_VERSION"
```

### Linux/macOS (Bash)

#### OpenAI Configuration

```bash
# Primary recommendation for production
export OPENAI_API_KEY="sk-your-openai-api-key-here"
export OPENAI_MODEL="gpt-4o"

# Alternative configurations
# For complex reasoning
# export OPENAI_MODEL="o1-preview"

# For development
# export OPENAI_MODEL="gpt-4o-mini"

# For premium analysis
# export OPENAI_MODEL="gpt-4-turbo"

# Advanced settings
export OPENAI_TEMPERATURE="0.1"
export OPENAI_MAX_TOKENS="1500"
export OPENAI_TOP_P="0.9"

# Add to ~/.bashrc or ~/.zshrc for persistence
echo 'export OPENAI_API_KEY="sk-your-key"' >> ~/.bashrc
echo 'export OPENAI_MODEL="gpt-4o"' >> ~/.bashrc
echo 'export OPENAI_TEMPERATURE="0.1"' >> ~/.bashrc
```

#### Azure OpenAI Configuration

```bash
# Azure OpenAI setup
export AZURE_OPENAI_ENDPOINT="https://your-resource.openai.azure.com/"
export OPENAI_API_KEY="your-azure-openai-key"
export OPENAI_MODEL="financial-copilot-gpt4o"
export AZURE_OPENAI_API_VERSION="2024-08-01-preview"

# Add to profile for persistence
echo 'export AZURE_OPENAI_ENDPOINT="https://your-resource.openai.azure.com/"' >> ~/.bashrc
echo 'export OPENAI_API_KEY="your-azure-key"' >> ~/.bashrc
echo 'export OPENAI_MODEL="financial-copilot-gpt4o"' >> ~/.bashrc
```

### Docker Configuration (2025)

```dockerfile
# Enhanced Dockerfile for Financial Co-Pilot
FROM mcr.microsoft.com/dotnet/runtime:9.0

# OpenAI Configuration
ENV OPENAI_API_KEY=""
ENV OPENAI_MODEL="gpt-4o"
ENV OPENAI_TEMPERATURE="0.1"
ENV OPENAI_MAX_TOKENS="1500"

# Or for Azure OpenAI
ENV AZURE_OPENAI_ENDPOINT=""
ENV OPENAI_API_KEY=""
ENV OPENAI_MODEL="financial-copilot-gpt4o"
ENV AZURE_OPENAI_API_VERSION="2024-08-01-preview"

# Enhanced features configuration
ENV ENHANCED_LOGGING="true"
ENV PRIVACY_AUDIT_LEVEL="comprehensive"
ENV STATUS_VALIDATION_ENABLED="true"
ENV FEDERATION_VALIDATION_ENABLED="true"
```

```bash
# Run with comprehensive environment variables
docker run \
  -e OPENAI_API_KEY="sk-your-key" \
  -e OPENAI_MODEL="gpt-4o" \
  -e OPENAI_TEMPERATURE="0.1" \
  -e ENHANCED_LOGGING="true" \
  -e PRIVACY_AUDIT_LEVEL="comprehensive" \
  your-financial-copilot-app
```

## Model Selection (2025 Update)

### Recommended Models by Use Case

#### Production Deployment (Recommended)

```bash
# Best overall balance of capability, speed, and cost
export OPENAI_MODEL="gpt-4o"
export OPENAI_TEMPERATURE="0.1"
export OPENAI_MAX_TOKENS="1500"

# For Azure OpenAI
export OPENAI_MODEL="financial-copilot-gpt4o"
```

**Why GPT-4o for Financial Co-Pilot**:

- Excellent reasoning capabilities for financial analysis
- Good balance of speed and quality
- Cost-effective for production workloads
- Strong performance on mathematical calculations
- Reliable for financial advice generation

#### Complex Financial Analysis

```bash
# When you need advanced reasoning for complex scenarios
export OPENAI_MODEL="o1-preview"
export OPENAI_TEMPERATURE="0.05"  # Very deterministic
export OPENAI_MAX_TOKENS="2000"
```

**Use o1-preview for**:

- Complex retirement modeling
- Multi-variable optimization
- Advanced tax scenario analysis
- Comprehensive financial planning
- Risk assessment with multiple factors

#### Development and Testing

```bash
# Cost-effective for development and testing
export OPENAI_MODEL="gpt-4o-mini"
export OPENAI_TEMPERATURE="0.2"
export OPENAI_MAX_TOKENS="1000"
```

**Use GPT-4o-mini for**:

- Feature development and testing
- Automated testing scenarios
- Proof of concepts
- Learning and experimentation

#### Premium Analysis

```bash
# Traditional high-quality model
export OPENAI_MODEL="gpt-4-turbo"
export OPENAI_TEMPERATURE="0.1"
export OPENAI_MAX_TOKENS="1500"
```

### Model Capabilities Matrix (2025)

| Model | Financial Accuracy | Cost Efficiency | Speed | Reasoning | Best Use Case |
|-------|-------------------|-----------------|-------|-----------|---------------|
| **gpt-4o** | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐ | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐ | **Production (Recommended)** |
| **o1-preview** | ⭐⭐⭐⭐⭐ | ⭐⭐ | ⭐⭐⭐ | ⭐⭐⭐⭐⭐ | Complex Analysis |
| **gpt-4-turbo** | ⭐⭐⭐⭐⭐ | ⭐⭐⭐ | ⭐⭐⭐⭐ | ⭐⭐⭐⭐ | Premium Analysis |
| **gpt-4o-mini** | ⭐⭐⭐⭐ | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐⭐ | ⭐⭐⭐ | Development/Testing |
| **o1-mini** | ⭐⭐⭐⭐ | ⭐⭐⭐⭐ | ⭐⭐⭐⭐ | ⭐⭐⭐⭐⭐ | Efficient Reasoning |

## Cost Optimization

### 1. Model Selection Strategy

```bash
# Development and testing phase
export OPENAI_MODEL="gpt-4o-mini"

# Production deployment 
export OPENAI_MODEL="gpt-4o"

# Complex analysis when needed
export OPENAI_MODEL="o1-preview"

# Budget-conscious production
export OPENAI_MODEL="gpt-4o-mini"
```

### 2. Usage Monitoring and Control

Set up comprehensive monitoring:

```bash
# Enable usage tracking
export FINANCIAL_COPILOT_TRACK_USAGE="true"
export FINANCIAL_COPILOT_USAGE_LOG="/path/to/usage.log"

# Set budget controls
export FINANCIAL_COPILOT_DAILY_BUDGET="10.00"
export FINANCIAL_COPILOT_MONTHLY_BUDGET="200.00"

# Enable cost alerts
export FINANCIAL_COPILOT_COST_ALERTS="true"
export FINANCIAL_COPILOT_ALERT_THRESHOLD="0.80"  # 80% of budget
```

### 3. Advanced Optimization Techniques

The Financial Co-Pilot includes built-in cost optimization:

- **Intelligent Model Selection**: Automatic model selection based on query complexity
- **Smart Caching**: Cache similar financial advice to reduce API calls
- **Context Optimization**: Minimal context sent while maintaining conversation quality
- **Session Management**: Efficient conversation memory to reduce token usage
- **Batch Processing**: Group related queries when possible

```bash
# Enable all optimization features
export FINANCIAL_COPILOT_ENABLE_CACHING="true"
export FINANCIAL_COPILOT_ENABLE_CONTEXT_OPTIMIZATION="true"
export FINANCIAL_COPILOT_ENABLE_MODEL_SELECTION="true"
export FINANCIAL_COPILOT_CACHE_DURATION="3600"  # 1 hour
```

## Error Handling

### Common Configuration Issues

#### 1. Invalid API Key

```
Error: Invalid API key provided
Solution: Verify API key is correct and active
Check: OpenAI Platform → API Keys → Verify key is not revoked
```

#### 2. Model Access Issues

```
Error: Model 'gpt-4o' not found or not accessible
Solution: 
- Verify your account has access to the model
- Check if you need to upgrade your OpenAI plan
- Use gpt-4o-mini as alternative for testing
```

#### 3. Rate Limiting

```
Error: Rate limit exceeded
Solution:
- Implement exponential backoff
- Upgrade to higher tier plan for better limits
- Use request queuing for high-volume scenarios
```

#### 4. Azure Deployment Issues

```
Error: Deployment 'financial-copilot-gpt4o' not found
Solution:
- Verify deployment name matches exactly
- Check deployment is in "Succeeded" state
- Ensure sufficient quota allocated
```

### Graceful Fallback Strategy

The Financial Co-Pilot automatically handles errors with intelligent fallback:

```csharp
// Built-in fallback logic (2025 update)
if (await IsModelAvailable("gpt-4o"))
    return await GenerateAdvice("gpt-4o", query);
else if (await IsModelAvailable("gpt-4-turbo"))
    return await GenerateAdvice("gpt-4-turbo", query);
else if (await IsModelAvailable("gpt-4o-mini"))
    return await GenerateAdvice("gpt-4o-mini", query);
else
    return GenerateSimulatedAdvice(query); // High-quality simulation
```

## Troubleshooting

### Testing Your Configuration

#### 1. Quick Configuration Test

```bash
# Run the samples to test configuration
cd samples/SdJwt.Net.Samples
dotnet run

# Look for successful initialization:
# "AI Provider: OpenAI gpt-4o (Real AI responses enabled)"
# "Enhanced Financial Co-Pilot initialized with 6 packages"
```

#### 2. Manual API Verification

**PowerShell (2025 Update):**

```powershell
# Test OpenAI connectivity with latest API
$headers = @{
    "Authorization" = "Bearer $env:OPENAI_API_KEY"
    "Content-Type" = "application/json"
}

$body = @{
    "model" = "$env:OPENAI_MODEL"
    "messages" = @(
        @{
            "role" = "user"
            "content" = "Test financial calculation: If I have $100,000 and add $1,000 monthly at 7% annual return, what will I have in 10 years?"
        }
    )
    "max_tokens" = 150
    "temperature" = 0.1
} | ConvertTo-Json -Depth 10

try {
    $response = Invoke-RestMethod -Uri "https://api.openai.com/v1/chat/completions" -Method Post -Headers $headers -Body $body
    Write-Host "✓ OpenAI connection successful"
    Write-Host "Model: $($response.model)"
    Write-Host "Response: $($response.choices[0].message.content)"
} catch {
    Write-Host "✗ OpenAI connection failed: $($_.Exception.Message)"
}
```

**Bash:**

```bash
# Test OpenAI API with latest endpoints
curl -X POST https://api.openai.com/v1/chat/completions \
  -H "Authorization: Bearer $OPENAI_API_KEY" \
  -H "Content-Type: application/json" \
  -d '{
    "model": "'$OPENAI_MODEL'",
    "messages": [
      {
        "role": "user", 
        "content": "Test: Calculate compound interest on $100k at 7% for 10 years"
      }
    ],
    "max_tokens": 150,
    "temperature": 0.1
  }'

# Success returns JSON with calculation
```

### Advanced Troubleshooting

#### Issue: "Unsupported model version"

**Solution:**

```bash
# Check available models
curl https://api.openai.com/v1/models \
  -H "Authorization: Bearer $OPENAI_API_KEY" | jq '.data[].id' | grep gpt-4

# Update to supported version
export OPENAI_MODEL="gpt-4o-2024-11-20"  # Example versioned model
```

#### Issue: Azure OpenAI deployment problems

**Solution:**

```bash
# Verify Azure deployment status
az cognitiveservices account deployment show \
  --name your-openai-resource \
  --resource-group your-rg \
  --deployment-name financial-copilot-gpt4o

# Check quotas and usage
az cognitiveservices usage list \
  --name your-openai-resource \
  --resource-group your-rg
```

#### Issue: High costs or unexpected usage

**Solution:**

```bash
# Enable detailed logging
export FINANCIAL_COPILOT_LOG_LEVEL="DEBUG"
export FINANCIAL_COPILOT_LOG_TOKENS="true"
export FINANCIAL_COPILOT_LOG_COSTS="true"

# Monitor usage patterns
export FINANCIAL_COPILOT_USAGE_ANALYTICS="true"
```

### Getting Help

1. **OpenAI Platform Issues**:
   - [OpenAI Help Center](https://help.openai.com/)
   - [OpenAI Community Forum](https://community.openai.com/)
   - [OpenAI Status Page](https://status.openai.com/)

2. **Azure OpenAI Issues**:
   - [Azure Support](https://azure.microsoft.com/support/)
   - [Azure OpenAI Documentation](https://docs.microsoft.com/azure/cognitive-services/openai/)

3. **SD-JWT .NET Issues**:
   - [GitHub Issues](https://github.com/thomas-tran/sd-jwt-dotnet/issues)
   - [GitHub Discussions](https://github.com/thomas-tran/sd-jwt-dotnet/discussions)

4. **Financial Co-Pilot Specific**:
   - [Enhanced Features Documentation](./enhanced-features.md)
   - [Financial Co-Pilot README](./README.md)

## Advanced Configuration (2025)

### Production Environment

**Enterprise Configuration:**

```bash
# Production settings for financial services
export OPENAI_MODEL="gpt-4o"
export OPENAI_TEMPERATURE="0.05"  # Very deterministic for financial advice
export OPENAI_MAX_TOKENS="2000"   # Detailed financial analysis
export OPENAI_TOP_P="0.9"
export OPENAI_FREQUENCY_PENALTY="0.1"  # Reduce repetition

# Enhanced Financial Co-Pilot features
export FINANCIAL_COPILOT_ENABLE_MONITORING="true"
export FINANCIAL_COPILOT_LOG_LEVEL="INFO"
export FINANCIAL_COPILOT_PRIVACY_AUDIT="COMPREHENSIVE"
export FINANCIAL_COPILOT_COST_TRACKING="true"
export FINANCIAL_COPILOT_PERFORMANCE_METRICS="true"

# Security and compliance
export FINANCIAL_COPILOT_ENABLE_ENCRYPTION="true"
export FINANCIAL_COPILOT_AUDIT_TRAIL="comprehensive"
export FINANCIAL_COPILOT_COMPLIANCE_MODE="financial_services"
```

### Custom Model Parameters

**Fine-tuned Configuration for Financial Advice:**

```bash
# Financial services optimized parameters
export OPENAI_TEMPERATURE="0.1"        # Low for consistent advice
export OPENAI_MAX_TOKENS="1500"        # Detailed responses
export OPENAI_TOP_P="0.9"             # Focused responses
export OPENAI_FREQUENCY_PENALTY="0.0" # Allow financial term repetition
export OPENAI_PRESENCE_PENALTY="0.1"  # Encourage comprehensive advice

# Advanced timeout and retry settings
export OPENAI_TIMEOUT_SECONDS="30"
export OPENAI_MAX_RETRIES="3"
export OPENAI_RETRY_DELAY="2"
```

### Multi-Model Strategy

**Intelligent Model Selection Based on Query Type:**

```bash
# Enable automatic model selection
export FINANCIAL_COPILOT_ENABLE_SMART_MODELS="true"

# Model assignments by complexity
export FINANCIAL_COPILOT_SIMPLE_MODEL="gpt-4o-mini"      # Basic queries
export FINANCIAL_COPILOT_STANDARD_MODEL="gpt-4o"         # Standard analysis
export FINANCIAL_COPILOT_COMPLEX_MODEL="o1-preview"      # Complex modeling
export FINANCIAL_COPILOT_REASONING_MODEL="o1-mini"       # Logical reasoning
```
