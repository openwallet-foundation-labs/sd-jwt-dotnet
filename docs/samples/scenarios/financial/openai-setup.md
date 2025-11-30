# OpenAI Setup Guide for Financial Co-Pilot

This guide provides comprehensive instructions for setting up OpenAI integration with the Financial Co-Pilot scenario, enabling real AI-powered financial advice generation.

## Table of Contents

- [Overview](#overview)
- [OpenAI Platform Setup](#openai-platform-setup)
- [Azure OpenAI Setup](#azure-openai-setup)
- [Environment Configuration](#environment-configuration)
- [Model Selection](#model-selection)
- [Cost Optimization](#cost-optimization)
- [Error Handling](#error-handling)
- [Troubleshooting](#troubleshooting)

## Overview

The Financial Co-Pilot can operate in two modes:

1. **Simulated Mode** (Default): High-quality simulated responses demonstrating all privacy-preserving patterns
2. **AI-Powered Mode**: Real OpenAI integration with GPT models for genuine financial advice

Both modes demonstrate the complete SD-JWT integration patterns, but AI-powered mode provides authentic, sophisticated financial guidance.

## OpenAI Platform Setup

### 1. Create OpenAI Account

1. Visit [OpenAI Platform](https://platform.openai.com/)
2. Sign up for an account or log in
3. Complete account verification
4. Set up billing (required for API access)

### 2. Generate API Key

1. Navigate to [API Keys](https://platform.openai.com/api-keys)
2. Click "Create new secret key"
3. Give it a descriptive name (e.g., "Financial-Co-Pilot-Demo")
4. Copy the key immediately (it won't be shown again)
5. Store it securely

### 3. Understanding Usage and Billing

#### Pricing Overview (as of 2024)

| Model | Input Cost | Output Cost | Use Case |
|-------|------------|-------------|----------|
| **gpt-4-turbo** | $10.00/1M tokens | $30.00/1M tokens | Production |
| **gpt-4o** | $5.00/1M tokens | $15.00/1M tokens | Balanced |
| **gpt-3.5-turbo** | $0.50/1M tokens | $1.50/1M tokens | Development |

#### Token Usage Estimation

Financial Co-Pilot typical usage per conversation:

```
Average Conversation (4 turns):
- System Prompt: ~800 tokens
- User Queries: ~200 tokens/turn = 800 tokens  
- AI Responses: ~600 tokens/turn = 2400 tokens
- Total per conversation: ~4000 tokens

Cost Examples:
- GPT-4-Turbo: ~$0.08 per conversation
- GPT-4o: ~$0.04 per conversation  
- GPT-3.5-Turbo: ~$0.006 per conversation
```

### 4. Set Usage Limits

1. Go to [Usage Limits](https://platform.openai.com/account/limits)
2. Set monthly spending limits (recommended: start with $10-20)
3. Enable email notifications for usage alerts

## Azure OpenAI Setup

### 1. Create Azure OpenAI Resource

1. Sign in to [Azure Portal](https://portal.azure.com/)
2. Create new resource → Search "OpenAI"
3. Select "Azure OpenAI"
4. Configure:
   - **Subscription**: Your Azure subscription
   - **Resource Group**: Create new or use existing
   - **Region**: Choose supported region (East US, West Europe, etc.)
   - **Name**: Unique name for your resource
   - **Pricing Tier**: Standard S0

### 2. Deploy Models

1. Go to your Azure OpenAI resource
2. Navigate to "Model deployments" 
3. Click "Create new deployment"
4. Configure:
   - **Model**: Select GPT-4-Turbo or GPT-4
   - **Deployment Name**: e.g., "financial-copilot-gpt4"
   - **Version**: Latest available
   - **Capacity**: Start with 10 TPM (tokens per minute)

### 3. Get Connection Information

1. Go to "Keys and Endpoint" in your Azure OpenAI resource
2. Copy:
   - **Endpoint**: e.g., `https://your-resource.openai.azure.com/`
   - **Key**: Primary or secondary key

## Environment Configuration

### Windows (PowerShell)

#### OpenAI Configuration
```powershell
# Set OpenAI API key
$env:OPENAI_API_KEY = "sk-your-openai-api-key-here"

# Set preferred model
$env:OPENAI_MODEL = "gpt-4-turbo"

# For GPT-5 (when available)
$env:OPENAI_MODEL = "gpt-5-turbo"

# Verify configuration
Write-Host "OpenAI API Key: $($env:OPENAI_API_KEY.Substring(0,10))..."
Write-Host "Model: $env:OPENAI_MODEL"
```

#### Azure OpenAI Configuration
```powershell
# Set Azure OpenAI endpoint and key
$env:AZURE_OPENAI_ENDPOINT = "https://your-resource.openai.azure.com/"
$env:OPENAI_API_KEY = "your-azure-openai-key"

# Set deployment name (not model name)
$env:OPENAI_MODEL = "your-gpt4-deployment-name"

# Verify configuration
Write-Host "Azure Endpoint: $env:AZURE_OPENAI_ENDPOINT"
Write-Host "Deployment: $env:OPENAI_MODEL"
```

### Linux/macOS (Bash)

#### OpenAI Configuration
```bash
# Set OpenAI API key
export OPENAI_API_KEY="sk-your-openai-api-key-here"

# Set preferred model
export OPENAI_MODEL="gpt-4-turbo"

# For advanced models
export OPENAI_MODEL="gpt-5-turbo"

# Add to ~/.bashrc or ~/.zshrc for persistence
echo 'export OPENAI_API_KEY="sk-your-key"' >> ~/.bashrc
echo 'export OPENAI_MODEL="gpt-4-turbo"' >> ~/.bashrc
```

#### Azure OpenAI Configuration
```bash
# Set Azure OpenAI endpoint and key
export AZURE_OPENAI_ENDPOINT="https://your-resource.openai.azure.com/"
export OPENAI_API_KEY="your-azure-openai-key"

# Set deployment name
export OPENAI_MODEL="your-gpt4-deployment-name"

# Add to profile for persistence
echo 'export AZURE_OPENAI_ENDPOINT="https://your-resource.openai.azure.com/"' >> ~/.bashrc
echo 'export OPENAI_API_KEY="your-azure-key"' >> ~/.bashrc
echo 'export OPENAI_MODEL="your-deployment-name"' >> ~/.bashrc
```

### Docker Configuration

```dockerfile
# In your Dockerfile
ENV OPENAI_API_KEY=""
ENV OPENAI_MODEL="gpt-4-turbo"

# Or for Azure OpenAI
ENV AZURE_OPENAI_ENDPOINT=""
ENV OPENAI_API_KEY=""
ENV OPENAI_MODEL="your-deployment-name"
```

```bash
# Run with environment variables
docker run -e OPENAI_API_KEY="sk-your-key" \
           -e OPENAI_MODEL="gpt-4-turbo" \
           your-app
```

## Model Selection

### Recommended Models by Use Case

#### Production Deployment
```bash
# Best balance of performance and cost
export OPENAI_MODEL="gpt-4-turbo"

# For Azure OpenAI (use deployment name)
export OPENAI_MODEL="financial-advisor-gpt4-turbo"
```

#### Development and Testing
```bash
# Cost-effective for development
export OPENAI_MODEL="gpt-3.5-turbo"

# Good performance at lower cost
export OPENAI_MODEL="gpt-4o-mini"
```

#### Advanced Financial Analysis
```bash
# Latest and most capable (when available)
export OPENAI_MODEL="gpt-5-turbo"

# Current best for complex reasoning
export OPENAI_MODEL="gpt-4-turbo"
```

### Model Capabilities Matrix

| Model | Financial Accuracy | Cost Efficiency | Speed | Best Use Case |
|-------|-------------------|-----------------|-------|---------------|
| **gpt-5-turbo** | ⭐⭐⭐⭐⭐ | ⭐⭐⭐ | ⭐⭐⭐⭐ | Production (Premium) |
| **gpt-4-turbo** | ⭐⭐⭐⭐⭐ | ⭐⭐⭐ | ⭐⭐⭐⭐ | Production (Standard) |
| **gpt-4o** | ⭐⭐⭐⭐ | ⭐⭐⭐⭐ | ⭐⭐⭐⭐⭐ | Balanced Production |
| **gpt-4** | ⭐⭐⭐⭐ | ⭐⭐ | ⭐⭐⭐ | Legacy Production |
| **gpt-3.5-turbo** | ⭐⭐⭐ | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐⭐ | Development/Testing |

## Cost Optimization

### 1. Model Selection Strategy

```bash
# Development phase
export OPENAI_MODEL="gpt-3.5-turbo"

# Testing phase  
export OPENAI_MODEL="gpt-4o-mini"

# Production phase
export OPENAI_MODEL="gpt-4-turbo"

# Premium production
export OPENAI_MODEL="gpt-5-turbo"
```

### 2. Usage Monitoring

Set up monitoring to track costs:

```bash
# Enable usage tracking
export FINANCIAL_COPILOT_TRACK_USAGE="true"
export FINANCIAL_COPILOT_USAGE_LOG="/path/to/usage.log"

# Set budget alerts
export FINANCIAL_COPILOT_DAILY_BUDGET="5.00"
export FINANCIAL_COPILOT_MONTHLY_BUDGET="100.00"
```

### 3. Optimization Techniques

The Financial Co-Pilot includes several cost optimization features:

- **Smart Caching**: Repeated similar queries use cached responses
- **Context Optimization**: Minimal context sent to AI for each turn
- **Model Selection**: Automatic fallback to cheaper models for simple queries
- **Session Management**: Efficient conversation memory management

## Error Handling

### Common Configuration Issues

#### 1. Invalid API Key
```
Error: Invalid API key provided
Solution: Verify API key is correct and active
```

#### 2. Insufficient Credits
```
Error: You have exceeded your current quota
Solution: Add credits to your OpenAI account or check billing
```

#### 3. Model Not Available
```
Error: Model 'gpt-5-turbo' not found
Solution: Use available model like 'gpt-4-turbo'
```

#### 4. Azure Deployment Not Found
```
Error: Deployment 'your-model' not found
Solution: Use actual deployment name, not model name
```

### Graceful Fallback

The Financial Co-Pilot automatically handles errors:

```csharp
// Built-in fallback logic
if (OpenAI_API_Available)
    return await GenerateRealAdvice(query);
else
    return GenerateSimulatedAdvice(query); // High-quality simulation
```

## Troubleshooting

### Testing Your Configuration

#### 1. Quick Test
```bash
# Run the samples
cd samples/SdJwt.Net.Samples
dotnet run

# Select Financial Co-Pilot option
# You should see: "AI Provider: OpenAI gpt-4-turbo (Real AI responses enabled)"
```

#### 2. Manual Verification

**PowerShell:**
```powershell
# Test OpenAI connectivity
$headers = @{
    "Authorization" = "Bearer $env:OPENAI_API_KEY"
}

try {
    $response = Invoke-RestMethod -Uri "https://api.openai.com/v1/models" -Headers $headers
    Write-Host "✓ OpenAI connection successful"
    Write-Host "Available models: $($response.data.Count)"
} catch {
    Write-Host "✗ OpenAI connection failed: $($_.Exception.Message)"
}
```

**Bash:**
```bash
# Test OpenAI connectivity
curl -H "Authorization: Bearer $OPENAI_API_KEY" \
     https://api.openai.com/v1/models

# Success should return JSON with model list
```

### Common Issues and Solutions

#### Issue: "No API key configured"
**Solution:**
```bash
# Check if environment variable is set
echo $OPENAI_API_KEY  # Should not be empty

# If empty, set it:
export OPENAI_API_KEY="sk-your-key-here"
```

#### Issue: "Model not found"
**Solution:**
```bash
# For OpenAI, use exact model names
export OPENAI_MODEL="gpt-4-turbo"

# For Azure, use your deployment name
export OPENAI_MODEL="your-actual-deployment-name"
```

#### Issue: Azure OpenAI "Resource not found"
**Solution:**
```bash
# Verify endpoint format (must end with /)
export AZURE_OPENAI_ENDPOINT="https://your-resource.openai.azure.com/"

# Verify resource name matches
```

#### Issue: High costs
**Solution:**
```bash
# Switch to more economical model for testing
export OPENAI_MODEL="gpt-3.5-turbo"

# Enable usage tracking
export FINANCIAL_COPILOT_TRACK_USAGE="true"
```

### Getting Help

1. **OpenAI Platform Issues**: [OpenAI Help Center](https://help.openai.com/)
2. **Azure OpenAI Issues**: [Azure Support](https://azure.microsoft.com/support/)
3. **SD-JWT .NET Issues**: [GitHub Issues](https://github.com/openwallet-foundation-labs/sd-jwt-dotnet/issues)
4. **General Questions**: [GitHub Discussions](https://github.com/openwallet-foundation-labs/sd-jwt-dotnet/discussions)

## Advanced Configuration

### Custom Model Parameters

The Financial Co-Pilot allows customization of AI behavior:

```bash
# Advanced configuration
export OPENAI_TEMPERATURE="0.2"        # Lower for more deterministic advice
export OPENAI_MAX_TOKENS="1200"        # Longer responses for detailed advice  
export OPENAI_TOP_P="0.9"             # Nucleus sampling parameter
export OPENAI_FREQUENCY_PENALTY="0.0" # Reduce repetition
```

### Production Environment

For production deployment:

```bash
# Production settings
export OPENAI_MODEL="gpt-4-turbo"
export OPENAI_TEMPERATURE="0.2"
export FINANCIAL_COPILOT_ENABLE_MONITORING="true"
export FINANCIAL_COPILOT_LOG_LEVEL="INFO"
export FINANCIAL_COPILOT_PRIVACY_AUDIT="COMPREHENSIVE"
```

---

**Ready to enable AI-powered financial advice?** Follow this setup guide to integrate real OpenAI capabilities with the Financial Co-Pilot's privacy-preserving architecture.

**Remember**: The Financial Co-Pilot works perfectly without AI integration, providing high-quality simulated responses that demonstrate all SD-JWT patterns. AI integration enhances the experience with genuine financial reasoning capabilities.
