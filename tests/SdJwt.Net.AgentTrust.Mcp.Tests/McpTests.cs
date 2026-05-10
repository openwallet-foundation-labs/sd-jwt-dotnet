using FluentAssertions;
using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.AgentTrust.Core;
using SdJwt.Net.AgentTrust.Mcp;
using SdJwt.Net.AgentTrust.Policy;
using System.Text;
using Xunit;

namespace SdJwt.Net.AgentTrust.Mcp.Tests;

public class McpToolTrustManifestTests
{
    [Fact]
    public void Constructor_WithDefaults_HasExpectedValues()
    {
        var manifest = new McpToolTrustManifest();

        manifest.Name.Should().BeEmpty();
        manifest.Audience.Should().BeEmpty();
        manifest.Actions.Should().BeEmpty();
        manifest.RequiresCapabilityToken.Should().BeTrue();
        manifest.MaxTokenLifetime.Should().BeNull();
    }

    [Fact]
    public void Properties_ShouldBeConfigurable()
    {
        var manifest = new McpToolTrustManifest
        {
            Name = "Weather",
            Audience = "tool://weather",
            Actions = new[] { "Read", "Write" },
            RequiresCapabilityToken = false,
            MaxTokenLifetime = TimeSpan.FromMinutes(5)
        };

        manifest.Name.Should().Be("Weather");
        manifest.Audience.Should().Be("tool://weather");
        manifest.Actions.Should().HaveCount(2);
        manifest.RequiresCapabilityToken.Should().BeFalse();
        manifest.MaxTokenLifetime.Should().Be(TimeSpan.FromMinutes(5));
    }
}

public class McpToolCallTests
{
    [Fact]
    public void Constructor_WithDefaults_HasExpectedValues()
    {
        var call = new McpToolCall();

        call.ToolName.Should().BeEmpty();
        call.Action.Should().Be("Read");
        call.Arguments.Should().BeNull();
        call.Context.Should().BeNull();
    }
}

public class McpTrustResultTests
{
    [Fact]
    public void Success_CreatesSuccessResult()
    {
        var result = McpTrustResult.Success("token123", "tid-1");

        result.IsSuccess.Should().BeTrue();
        result.Token.Should().Be("token123");
        result.TokenId.Should().Be("tid-1");
        result.Error.Should().BeNull();
    }

    [Fact]
    public void Denied_CreatesDeniedResult()
    {
        var result = McpTrustResult.Denied("Not authorized");

        result.IsSuccess.Should().BeFalse();
        result.Token.Should().BeNull();
        result.Error.Should().Be("Not authorized");
    }

    [Fact]
    public void Failed_CreatesFailedResult()
    {
        var result = McpTrustResult.Failed("Connection error");

        result.IsSuccess.Should().BeFalse();
        result.Token.Should().BeNull();
        result.Error.Should().Be("Connection error");
    }
}

public class McpClientTrustOptionsTests
{
    [Fact]
    public void Constructor_WithDefaults_HasExpectedValues()
    {
        var options = new McpClientTrustOptions();

        options.AgentId.Should().BeEmpty();
        options.ToolAudienceMapping.Should().BeEmpty();
        options.DefaultTokenLifetime.Should().Be(TimeSpan.FromSeconds(60));
        options.TokenHeaderName.Should().Be("X-Agent-Trust-Token");
    }
}

public class McpServerTrustOptionsTests
{
    [Fact]
    public void Constructor_WithDefaults_HasExpectedValues()
    {
        var options = new McpServerTrustOptions();

        options.Audience.Should().BeEmpty();
        options.TrustedIssuers.Should().BeEmpty();
    }
}

public class McpServerTrustGuardTests
{
    private static readonly SymmetricSecurityKey DefaultKey =
        new(Encoding.UTF8.GetBytes("0123456789ABCDEF0123456789ABCDEF"));

    [Fact]
    public async Task VerifyToolCallAsync_WithEmptyToolName_ReturnsFailure()
    {
        var nonceStore = new MemoryNonceStore();
        var verifier = new CapabilityTokenVerifier(nonceStore);
        var policyEngine = new AlwaysPermitPolicyEngine();
        var options = new McpServerTrustOptions
        {
            Audience = "tool://weather",
            TrustedIssuers = new Dictionary<string, SecurityKey>
            {
                ["agent://alpha"] = DefaultKey
            }
        };
        var guard = new McpServerTrustGuard(verifier, policyEngine, options);

        var result = await guard.VerifyToolCallAsync("", "some-token");

        result.IsValid.Should().BeFalse();
        result.Error.Should().Contain("Tool name is required");
    }

    [Fact]
    public async Task VerifyToolCallAsync_WithEmptyToken_ReturnsFailure()
    {
        var nonceStore = new MemoryNonceStore();
        var verifier = new CapabilityTokenVerifier(nonceStore);
        var policyEngine = new AlwaysPermitPolicyEngine();
        var options = new McpServerTrustOptions
        {
            Audience = "tool://weather",
            TrustedIssuers = new Dictionary<string, SecurityKey>
            {
                ["agent://alpha"] = DefaultKey
            }
        };
        var guard = new McpServerTrustGuard(verifier, policyEngine, options);

        var result = await guard.VerifyToolCallAsync("Weather", "");

        result.IsValid.Should().BeFalse();
        result.Error.Should().Contain("Token is required");
    }

    [Fact]
    public async Task VerifyToolCallAsync_WithInvalidToken_ReturnsFailure()
    {
        var nonceStore = new MemoryNonceStore();
        var verifier = new CapabilityTokenVerifier(nonceStore);
        var policyEngine = new AlwaysPermitPolicyEngine();
        var options = new McpServerTrustOptions
        {
            Audience = "tool://weather",
            TrustedIssuers = new Dictionary<string, SecurityKey>
            {
                ["agent://alpha"] = DefaultKey
            }
        };
        var guard = new McpServerTrustGuard(verifier, policyEngine, options);

        var result = await guard.VerifyToolCallAsync("Weather", "not.a.valid.jwt");

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task VerifyToolCallAsync_WithValidTokenAndMatchingTool_ReturnsSuccess()
    {
        var nonceStore = new MemoryNonceStore();
        var issuer = new CapabilityTokenIssuer(DefaultKey, SecurityAlgorithms.HmacSha256, nonceStore);
        var verifier = new CapabilityTokenVerifier(nonceStore);
        var policyEngine = new AlwaysPermitPolicyEngine();
        var options = new McpServerTrustOptions
        {
            Audience = "tool://weather",
            TrustedIssuers = new Dictionary<string, SecurityKey>
            {
                ["agent://alpha"] = DefaultKey
            },
            AllowedAlgorithms = new[] { SecurityAlgorithms.HmacSha256 }
        };
        var guard = new McpServerTrustGuard(verifier, policyEngine, options);

        var minted = issuer.Mint(new CapabilityTokenOptions
        {
            Issuer = "agent://alpha",
            Audience = "tool://weather",
            Capability = new CapabilityClaim { Tool = "Weather", Action = "Read" },
            Context = new CapabilityContext { CorrelationId = "corr-guard-1" }
        });

        var result = await guard.VerifyToolCallAsync("Weather", minted.Token);

        result.IsValid.Should().BeTrue(result.Error);
    }

    [Fact]
    public async Task VerifyToolCallAsync_WithToolMismatch_ReturnsFailure()
    {
        var nonceStore = new MemoryNonceStore();
        var issuer = new CapabilityTokenIssuer(DefaultKey, SecurityAlgorithms.HmacSha256, nonceStore);
        var verifier = new CapabilityTokenVerifier(nonceStore);
        var policyEngine = new AlwaysPermitPolicyEngine();
        var options = new McpServerTrustOptions
        {
            Audience = "tool://weather",
            TrustedIssuers = new Dictionary<string, SecurityKey>
            {
                ["agent://alpha"] = DefaultKey
            }
        };
        var guard = new McpServerTrustGuard(verifier, policyEngine, options);

        var minted = issuer.Mint(new CapabilityTokenOptions
        {
            Issuer = "agent://alpha",
            Audience = "tool://weather",
            Capability = new CapabilityClaim { Tool = "Calendar", Action = "Read" },
            Context = new CapabilityContext { CorrelationId = "corr-guard-mismatch" }
        });

        var result = await guard.VerifyToolCallAsync("Weather", minted.Token);

        result.IsValid.Should().BeFalse();
        result.Error.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task VerifyToolCallAsync_WithPolicyDenial_ReturnsFailure()
    {
        var nonceStore = new MemoryNonceStore();
        var issuer = new CapabilityTokenIssuer(DefaultKey, SecurityAlgorithms.HmacSha256, nonceStore);
        var verifier = new CapabilityTokenVerifier(nonceStore);
        var policyEngine = new AlwaysDenyPolicyEngine();
        var options = new McpServerTrustOptions
        {
            Audience = "tool://weather",
            TrustedIssuers = new Dictionary<string, SecurityKey>
            {
                ["agent://alpha"] = DefaultKey
            }
        };
        var guard = new McpServerTrustGuard(verifier, policyEngine, options);

        var minted = issuer.Mint(new CapabilityTokenOptions
        {
            Issuer = "agent://alpha",
            Audience = "tool://weather",
            Capability = new CapabilityClaim { Tool = "Weather", Action = "Read" },
            Context = new CapabilityContext { CorrelationId = "corr-guard-deny" }
        });

        var result = await guard.VerifyToolCallAsync("Weather", minted.Token);

        result.IsValid.Should().BeFalse();
    }

    private class AlwaysPermitPolicyEngine : IPolicyEngine
    {
        public Task<PolicyDecision> EvaluateAsync(PolicyRequest request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(PolicyDecision.Permit());
        }
    }

    private class AlwaysDenyPolicyEngine : IPolicyEngine
    {
        public Task<PolicyDecision> EvaluateAsync(PolicyRequest request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(PolicyDecision.Deny("Denied by test policy.", "test_denied"));
        }
    }
}
