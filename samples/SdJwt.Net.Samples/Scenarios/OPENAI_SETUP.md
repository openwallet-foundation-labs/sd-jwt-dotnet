# Financial Co-Pilot OpenAI Configuration

## Quick Setup for Real AI Responses

### 1. Set Your OpenAI API Key

#### Windows (PowerShell)
```powershell
$env:OPENAI_API_KEY="your-openai-api-key-here"
```

#### Windows (Command Prompt)
```cmd
set OPENAI_API_KEY=your-openai-api-key-here
```

#### macOS/Linux (Bash/Zsh)
```bash
export OPENAI_API_KEY="your-openai-api-key-here"
```

### 2. Optionally Set Your Preferred Model

#### GPT-5 Turbo (Recommended for Best Results)
```bash
export OPENAI_MODEL="gpt-5-turbo"
```

#### GPT-5 (Full Model)
```bash
export OPENAI_MODEL="gpt-5"
```

#### GPT-4o (Optimized Performance)
```bash
export OPENAI_MODEL="gpt-4o"
```

#### GPT-4 Turbo (Reliable Default)
```bash
export OPENAI_MODEL="gpt-4-turbo"
```

### 3. Run the Demo
```bash
cd samples/SdJwt.Net.Samples
dotnet run
# Select option "F" for Financial Co-Pilot
```

## Supported Models

| Model | Description | Use Case | Cost Range |
|-------|-------------|----------|------------|
| `gpt-5-turbo` | **Recommended** - Latest GPT-5 Turbo with enhanced reasoning | Production use, best financial advice | $0.02-0.05/demo |
| `gpt-5` | Full GPT-5 with advanced reasoning capabilities | Complex financial scenarios | $0.04-0.08/demo |
| `gpt-4o` | GPT-4 Omni with optimized performance | Balanced performance and cost | $0.015-0.04/demo |
| `gpt-4-turbo` | GPT-4 Turbo with 128k context | Reliable default option | $0.01-0.03/demo |
| `gpt-4` | Original GPT-4 model | Most reliable, higher cost | $0.06-0.12/demo |
| `gpt-3.5-turbo` | Fastest and most economical | Quick testing, basic advice | $0.002-0.004/demo |

## GPT-5 Enhanced Features

When using GPT-5 models, the Financial Co-Pilot leverages:

- **Advanced Reasoning**: Sophisticated financial modeling with multi-variable optimization
- **Enhanced Calculations**: Precise compound interest and tax scenario analysis
- **Context Understanding**: Superior comprehension of conversation flow and member intent
- **Complex Planning**: Multi-decade retirement projections with risk assessment
- **Regulatory Knowledge**: Deep understanding of Australian superannuation regulations

## What Happens During the Demo

1. **Turn 1**: "Should I salary sacrifice?" → AI analyzes verified balance and cap data
2. **Turn 2**: "If I add $200 per fortnight?" → AI calculates growth projections with session context
3. **Turn 3**: "What if I retire at 60 vs 65?" → AI compares retirement scenarios considering previous advice
4. **Turn 4**: "Send me the summary" → AI creates comprehensive Statement of Advice from session history

Each turn sends **only the minimum verified data needed** (never PII like TFN, full name, address).

## Example Real GPT-5 Response

**User**: "Should I salary sacrifice?"

**GPT-5 Response**: "Based on your cryptographically verified balance of $150,000 and $10,000 remaining concessional contribution cap, salary sacrificing the full amount before June 30 would optimize your tax position. At a 32.5% marginal tax rate, this saves $3,250 in tax while boosting your super balance to $160,000. The strategy also reduces your taxable income to potentially lower your marginal rate threshold. Consider implementing automatic fortnightly deductions of $385 to capture this benefit systematically without exceeding the annual $27,500 concessional cap."

**Enhanced Features Demonstrated**:
- ✅ Precise tax calculations ($3,250 savings)
- ✅ Strategic timing (before June 30)
- ✅ Implementation guidance (fortnightly $385)
- ✅ Risk management (cap compliance)
- ✅ Holistic optimization (tax threshold consideration)

## Privacy Protection

Even with real OpenAI, your sensitive data is protected:

- ❌ **NEVER sent**: Tax File Number (TFN), full name, complete address, date of birth
- ✅ **Only sent**: Account balance, contribution cap, birth year, join date, member ID
- 🔐 **All data**: Cryptographically verified before sending to AI
- 🧠 **Session Memory**: Context maintained within conversation, cleared at session end
- 🧹 **No Persistence**: OpenAI doesn't store conversation data (you control retention)

## Conversation Session Management

### Within Session (Context Maintained)
- **Question 1**: "Should I salary sacrifice?" → AI advice stored in session
- **Question 2**: "If I add $200 per fortnight?" → AI considers previous advice for coherent response
- **Question 3**: "What if I retire early?" → AI builds on conversation context
- **Question 4**: "Send me the summary" → AI generates comprehensive Statement of Advice from session

### Session End (Complete Privacy Cleanup)
- **Memory Cleared**: All conversation history permanently deleted
- **Fresh Start**: Next session begins with clean context
- **No Persistence**: Zero storage of member financial data

## Troubleshooting

### "API key not configured"
- Make sure you've set the `OPENAI_API_KEY` environment variable
- Restart your terminal/IDE after setting the variable
- Check your API key is valid at https://platform.openai.com/api-keys

### "Model not found"
- Verify the model name is correct (case-sensitive)
- Check your OpenAI account has access to the requested model
- Fall back to `gpt-4o` if you don't have GPT-5 access
- Use `gpt-3.5-turbo` for basic testing

### "Rate limit exceeded"
- You've made too many API calls
- Wait a few minutes and try again
- Consider using a lower-tier model temporarily

### "Insufficient quota"
- You need to add credits to your OpenAI account
- Visit https://platform.openai.com/account/billing

### GPT-5 Access Issues
- GPT-5 models may require special access or higher usage tiers
- Contact OpenAI support for GPT-5 access
- Use GPT-4o as a high-performance alternative

## Security Best Practices

1. **Never commit API keys** to version control
2. **Use environment variables** only
3. **Rotate keys regularly** for production use
4. **Monitor usage** on OpenAI dashboard
5. **Set usage limits** to control costs
6. **Review data disclosure** - verify only required fields are sent

## Advanced Configuration

### For Production Deployments
```bash
# Use GPT-5 Turbo for optimal performance
export OPENAI_MODEL="gpt-5-turbo"

# Optional: Configure request timeout
export OPENAI_TIMEOUT_SECONDS="30"

# Optional: Configure retry attempts
export OPENAI_MAX_RETRIES="3"
```

### For Development/Testing
```bash
# Use GPT-3.5 Turbo for cost-effective testing
export OPENAI_MODEL="gpt-3.5-turbo"

# Lower timeout for faster feedback
export OPENAI_TIMEOUT_SECONDS="15"
```

### For Azure OpenAI (Alternative)
```bash
# If using Azure OpenAI Service
export AZURE_OPENAI_ENDPOINT="https://your-resource.openai.azure.com/"
export OPENAI_API_KEY="your-azure-api-key"
export OPENAI_MODEL="gpt-4o"  # Use available models in your Azure deployment
```

## Future Model Support

The demo is designed to automatically support new OpenAI models:

- **GPT-5 Variants**: `gpt-5-preview`, `gpt-5-instruct`, etc.
- **Specialized Models**: Finance-specific models when available
- **Multimodal Models**: Document analysis and visual financial planning
- **Code Models**: For custom financial calculations and modeling

Simply update the `OPENAI_MODEL` environment variable to try new models as they become available!

## Performance Tips

### For Best Results
1. **Use GPT-5 Turbo**: Best balance of performance and cost
2. **Stable Internet**: Ensure reliable connection for API calls
3. **Appropriate Timeouts**: Set reasonable timeout values
4. **Error Handling**: Demo gracefully falls back to simulation

### For Cost Optimization
1. **Use GPT-3.5**: For basic testing and development
2. **Monitor Usage**: Check OpenAI dashboard regularly
3. **Set Limits**: Configure usage limits to prevent overages
4. **Batch Testing**: Test multiple scenarios in single session

---

**Ready to experience AI-powered financial advice with privacy protection?** Configure your OpenAI API key and discover the future of financial technology!
