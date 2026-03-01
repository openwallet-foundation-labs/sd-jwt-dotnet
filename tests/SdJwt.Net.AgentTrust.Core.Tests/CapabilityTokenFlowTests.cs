using FluentAssertions;
using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.AgentTrust.Core;
using System.Text;
using Xunit;

namespace SdJwt.Net.AgentTrust.Core.Tests;

public class CapabilityTokenFlowTests
{
    [Fact]
    public async Task MintAndVerify_WithValidInput_ShouldSucceed()
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("0123456789ABCDEF0123456789ABCDEF"));
        var nonceStore = new MemoryNonceStore();
        var issuer = new CapabilityTokenIssuer(key, SecurityAlgorithms.HmacSha256, nonceStore);
        var verifier = new CapabilityTokenVerifier(nonceStore);

        var minted = issuer.Mint(new CapabilityTokenOptions
        {
            Issuer = "agent://alpha",
            Audience = "tool://weather",
            Capability = new CapabilityClaim { Tool = "Weather", Action = "Read", Purpose = "forecast" },
            Context = new CapabilityContext { CorrelationId = "corr-1" }
        });

        var result = await verifier.VerifyAsync(minted.Token, new CapabilityVerificationOptions
        {
            ExpectedAudience = "tool://weather",
            TrustedIssuers = new Dictionary<string, SecurityKey> { ["agent://alpha"] = key }
        });

        result.IsValid.Should().BeTrue(result.Error);
        result.Capability.Should().NotBeNull();
        result.Capability!.Tool.Should().Be("Weather");
        result.Context!.CorrelationId.Should().Be("corr-1");
    }

    [Fact]
    public async Task VerifyAsync_WithReplayToken_ShouldFail()
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("0123456789ABCDEF0123456789ABCDEF"));
        var nonceStore = new MemoryNonceStore();
        var issuer = new CapabilityTokenIssuer(key, SecurityAlgorithms.HmacSha256, nonceStore);
        var verifier = new CapabilityTokenVerifier(nonceStore);

        var minted = issuer.Mint(new CapabilityTokenOptions
        {
            Issuer = "agent://alpha",
            Audience = "tool://weather",
            Capability = new CapabilityClaim { Tool = "Weather", Action = "Read" },
            Context = new CapabilityContext { CorrelationId = "corr-2" }
        });

        var first = await verifier.VerifyAsync(minted.Token, new CapabilityVerificationOptions
        {
            ExpectedAudience = "tool://weather",
            TrustedIssuers = new Dictionary<string, SecurityKey> { ["agent://alpha"] = key }
        });
        var second = await verifier.VerifyAsync(minted.Token, new CapabilityVerificationOptions
        {
            ExpectedAudience = "tool://weather",
            TrustedIssuers = new Dictionary<string, SecurityKey> { ["agent://alpha"] = key }
        });

        first.IsValid.Should().BeTrue(first.Error);
        second.IsValid.Should().BeFalse();
        second.ErrorCode.Should().Be("replay_detected");
    }
}
