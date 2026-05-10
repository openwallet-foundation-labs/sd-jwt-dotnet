using FluentAssertions;
using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.AgentTrust.Core;
using System.Text;
using Xunit;

namespace SdJwt.Net.AgentTrust.Core.Tests;

/// <summary>
/// Tests for enterprise-grade verification using <see cref="AgentTrustVerificationContext"/>.
/// </summary>
public class EnterpriseVerificationTests
{
    private static readonly SymmetricSecurityKey DefaultKey =
        new(Encoding.UTF8.GetBytes("0123456789ABCDEF0123456789ABCDEF"));

    private static (CapabilityTokenIssuer Issuer, CapabilityTokenVerifier Verifier) CreatePair()
    {
        var nonceStore = new MemoryNonceStore();
        return (new CapabilityTokenIssuer(DefaultKey, SecurityAlgorithms.HmacSha256, nonceStore),
                new CapabilityTokenVerifier(nonceStore));
    }

    [Fact]
    public async Task VerifyAsync_WithValidContext_ShouldSucceed()
    {
        var (issuer, verifier) = CreatePair();

        var minted = issuer.Mint(new CapabilityTokenOptions
        {
            Issuer = "agent://alpha",
            Audience = "tool://weather",
            Capability = new CapabilityClaim { Tool = "Weather", Action = "Read" },
            Context = new CapabilityContext { CorrelationId = "corr-ctx-1", TenantId = "tenant-1" }
        });

        var context = new AgentTrustVerificationContext
        {
            ExpectedAudience = "tool://weather",
            TrustedIssuers = new Dictionary<string, SecurityKey> { ["agent://alpha"] = DefaultKey },
            SecurityMode = AgentTrustSecurityMode.Demo,
            AllowedAlgorithms = new[] { SecurityAlgorithms.HmacSha256 },
            EnforceReplayPrevention = false
        };

        var result = await verifier.VerifyAsync(minted.Token, context);

        result.IsValid.Should().BeTrue(result.Error);
        result.SecurityMode.Should().Be(AgentTrustSecurityMode.Demo);
    }

    [Fact]
    public async Task VerifyAsync_WithToolMismatch_ShouldFail()
    {
        var (issuer, verifier) = CreatePair();

        var minted = issuer.Mint(new CapabilityTokenOptions
        {
            Issuer = "agent://alpha",
            Audience = "tool://weather",
            Capability = new CapabilityClaim { Tool = "Weather", Action = "Read" },
            Context = new CapabilityContext { CorrelationId = "corr-tool-mm" }
        });

        var context = new AgentTrustVerificationContext
        {
            ExpectedAudience = "tool://weather",
            TrustedIssuers = new Dictionary<string, SecurityKey> { ["agent://alpha"] = DefaultKey },
            ExpectedToolId = "Calendar",
            AllowedAlgorithms = new[] { SecurityAlgorithms.HmacSha256 },
            EnforceReplayPrevention = false
        };

        var result = await verifier.VerifyAsync(minted.Token, context);

        result.IsValid.Should().BeFalse();
        result.ErrorCode.Should().Be("tool_mismatch");
    }

    [Fact]
    public async Task VerifyAsync_WithActionMismatch_ShouldFail()
    {
        var (issuer, verifier) = CreatePair();

        var minted = issuer.Mint(new CapabilityTokenOptions
        {
            Issuer = "agent://alpha",
            Audience = "tool://weather",
            Capability = new CapabilityClaim { Tool = "Weather", Action = "Read" },
            Context = new CapabilityContext { CorrelationId = "corr-act-mm" }
        });

        var context = new AgentTrustVerificationContext
        {
            ExpectedAudience = "tool://weather",
            TrustedIssuers = new Dictionary<string, SecurityKey> { ["agent://alpha"] = DefaultKey },
            ExpectedAction = "Write",
            AllowedAlgorithms = new[] { SecurityAlgorithms.HmacSha256 },
            EnforceReplayPrevention = false
        };

        var result = await verifier.VerifyAsync(minted.Token, context);

        result.IsValid.Should().BeFalse();
        result.ErrorCode.Should().Be("action_mismatch");
    }

    [Fact]
    public async Task VerifyAsync_WithTenantMismatch_ShouldFail()
    {
        var (issuer, verifier) = CreatePair();

        var minted = issuer.Mint(new CapabilityTokenOptions
        {
            Issuer = "agent://alpha",
            Audience = "tool://weather",
            Capability = new CapabilityClaim { Tool = "Weather", Action = "Read" },
            Context = new CapabilityContext { CorrelationId = "corr-tenant-mm", TenantId = "tenant-A" }
        });

        var context = new AgentTrustVerificationContext
        {
            ExpectedAudience = "tool://weather",
            TrustedIssuers = new Dictionary<string, SecurityKey> { ["agent://alpha"] = DefaultKey },
            ExpectedTenantId = "tenant-B",
            AllowedAlgorithms = new[] { SecurityAlgorithms.HmacSha256 },
            EnforceReplayPrevention = false
        };

        var result = await verifier.VerifyAsync(minted.Token, context);

        result.IsValid.Should().BeFalse();
        result.ErrorCode.Should().Be("tenant_mismatch");
    }

    [Fact]
    public async Task VerifyAsync_WithAlgorithmNotAllowed_ShouldFail()
    {
        var (issuer, verifier) = CreatePair();

        var minted = issuer.Mint(new CapabilityTokenOptions
        {
            Issuer = "agent://alpha",
            Audience = "tool://weather",
            Capability = new CapabilityClaim { Tool = "Weather", Action = "Read" },
            Context = new CapabilityContext { CorrelationId = "corr-alg" }
        });

        var context = new AgentTrustVerificationContext
        {
            ExpectedAudience = "tool://weather",
            TrustedIssuers = new Dictionary<string, SecurityKey> { ["agent://alpha"] = DefaultKey },
            AllowedAlgorithms = new List<string> { SecurityAlgorithms.EcdsaSha256 },
            EnforceReplayPrevention = false
        };

        var result = await verifier.VerifyAsync(minted.Token, context);

        result.IsValid.Should().BeFalse();
        result.ErrorCode.Should().Be("algorithm_not_allowed");
    }

    [Fact]
    public async Task VerifyAsync_WithExcessiveLifetime_ShouldFail()
    {
        var (issuer, verifier) = CreatePair();

        var minted = issuer.Mint(new CapabilityTokenOptions
        {
            Issuer = "agent://alpha",
            Audience = "tool://weather",
            Capability = new CapabilityClaim { Tool = "Weather", Action = "Read" },
            Context = new CapabilityContext { CorrelationId = "corr-lt" },
            Lifetime = TimeSpan.FromHours(2) // exceeds max
        });

        var context = new AgentTrustVerificationContext
        {
            ExpectedAudience = "tool://weather",
            TrustedIssuers = new Dictionary<string, SecurityKey> { ["agent://alpha"] = DefaultKey },
            MaxTokenLifetime = TimeSpan.FromMinutes(5),
            EnforceReplayPrevention = false
        };

        var result = await verifier.VerifyAsync(minted.Token, context);

        result.IsValid.Should().BeFalse();
        result.ErrorCode.Should().Be("excessive_lifetime");
    }

    [Fact]
    public async Task VerifyAsync_WithManifestHashRequired_NoHash_ShouldFail()
    {
        var (issuer, verifier) = CreatePair();

        var minted = issuer.Mint(new CapabilityTokenOptions
        {
            Issuer = "agent://alpha",
            Audience = "tool://weather",
            Capability = new CapabilityClaim { Tool = "Weather", Action = "Read" },
            Context = new CapabilityContext { CorrelationId = "corr-manifest" }
        });

        var context = new AgentTrustVerificationContext
        {
            ExpectedAudience = "tool://weather",
            TrustedIssuers = new Dictionary<string, SecurityKey> { ["agent://alpha"] = DefaultKey },
            RequireToolManifestBinding = true,
            AllowedAlgorithms = new[] { SecurityAlgorithms.HmacSha256 },
            EnforceReplayPrevention = false
        };

        var result = await verifier.VerifyAsync(minted.Token, context);

        result.IsValid.Should().BeFalse();
        result.ErrorCode.Should().Be("missing_manifest_hash");
    }

    [Fact]
    public async Task VerifyAsync_WithToolIdMatch_ShouldSucceed()
    {
        var (issuer, verifier) = CreatePair();

        var minted = issuer.Mint(new CapabilityTokenOptions
        {
            Issuer = "agent://alpha",
            Audience = "tool://weather",
            Capability = new CapabilityClaim { Tool = "Weather", ToolId = "weather-v1", Action = "Read" },
            Context = new CapabilityContext { CorrelationId = "corr-toolid" }
        });

        var context = new AgentTrustVerificationContext
        {
            ExpectedAudience = "tool://weather",
            TrustedIssuers = new Dictionary<string, SecurityKey> { ["agent://alpha"] = DefaultKey },
            ExpectedToolId = "weather-v1",
            AllowedAlgorithms = new[] { SecurityAlgorithms.HmacSha256 },
            EnforceReplayPrevention = false
        };

        var result = await verifier.VerifyAsync(minted.Token, context);

        result.IsValid.Should().BeTrue(result.Error);
    }
}
