using FluentAssertions;
using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.AgentTrust.Core;
using SdJwt.Net.AgentTrust.Maf;
using SdJwt.Net.AgentTrust.Policy;
using System.Text;
using Xunit;

namespace SdJwt.Net.AgentTrust.Maf.Tests;

public class McpTrustAdapterTests
{
    [Fact]
    public async Task MintForToolCallAsync_WithPermittedPolicy_ShouldReturnToken()
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("0123456789ABCDEF0123456789ABCDEF"));
        var issuer = new CapabilityTokenIssuer(key, SecurityAlgorithms.HmacSha256, new MemoryNonceStore());
        var policy = new DefaultPolicyEngine(new PolicyBuilder().Allow("agent://alpha", "Weather", "Read").Build());
        var adapter = new McpTrustAdapter(issuer, policy, "agent://alpha", new Dictionary<string, string> { ["Weather"] = "tool://weather" });

        var result = await adapter.MintForToolCallAsync("Weather", new Dictionary<string, object>(), new CapabilityContext { CorrelationId = "corr-maf" });

        result.Token.Should().NotBeNullOrWhiteSpace();
        result.TokenId.Should().NotBeNullOrWhiteSpace();
    }
}
