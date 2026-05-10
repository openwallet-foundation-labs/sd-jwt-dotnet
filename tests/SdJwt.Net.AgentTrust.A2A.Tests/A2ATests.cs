using FluentAssertions;
using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.AgentTrust.A2A;
using SdJwt.Net.AgentTrust.Core;
using SdJwt.Net.AgentTrust.Policy;
using System.Text;
using Xunit;

namespace SdJwt.Net.AgentTrust.A2A.Tests;

public class AgentCardTests
{
    [Fact]
    public void Constructor_WithDefaults_HasExpectedValues()
    {
        var card = new AgentCard();

        card.AgentId.Should().BeEmpty();
        card.DisplayName.Should().BeEmpty();
        card.Description.Should().BeNull();
        card.Version.Should().BeNull();
        card.Capabilities.Should().BeEmpty();
        card.TrustEndpoint.Should().BeNull();
        card.JwksUri.Should().BeNull();
        card.MaxDelegationDepth.Should().Be(3);
        card.SupportsDelegation.Should().BeTrue();
        card.TenantId.Should().BeNull();
    }

    [Fact]
    public void Properties_ShouldBeConfigurable()
    {
        var card = new AgentCard
        {
            AgentId = "agent://alpha",
            DisplayName = "Alpha Agent",
            Description = "Test agent",
            Version = "1.0",
            Capabilities = new[] { "Weather.Read", "Calendar.Write" },
            TrustEndpoint = "https://alpha.example.com/.well-known/agent-trust",
            JwksUri = "https://alpha.example.com/.well-known/jwks",
            MaxDelegationDepth = 5,
            SupportsDelegation = false,
            TenantId = "tenant-1"
        };

        card.AgentId.Should().Be("agent://alpha");
        card.DisplayName.Should().Be("Alpha Agent");
        card.Capabilities.Should().HaveCount(2);
        card.MaxDelegationDepth.Should().Be(5);
        card.SupportsDelegation.Should().BeFalse();
    }
}

public class DelegationChainValidationResultTests
{
    [Fact]
    public void Valid_CreatesValidResult()
    {
        var result = DelegationChainValidationResult.Valid(2, "agent://root");

        result.IsValid.Should().BeTrue();
        result.Depth.Should().Be(2);
        result.RootIssuer.Should().Be("agent://root");
        result.Error.Should().BeNull();
        result.ErrorCode.Should().BeNull();
    }

    [Fact]
    public void Invalid_CreatesInvalidResult()
    {
        var result = DelegationChainValidationResult.Invalid("Chain too deep", "delegation_depth_exceeded");

        result.IsValid.Should().BeFalse();
        result.Error.Should().Be("Chain too deep");
        result.ErrorCode.Should().Be("delegation_depth_exceeded");
    }
}

public class DelegationChainValidatorTests
{
    private static readonly SymmetricSecurityKey DefaultKey =
        new(Encoding.UTF8.GetBytes("0123456789ABCDEF0123456789ABCDEF"));

    [Fact]
    public async Task ValidateChainAsync_WithEmptyChain_ReturnsInvalid()
    {
        var nonceStore = new MemoryNonceStore();
        var verifier = new CapabilityTokenVerifier(nonceStore);
        var validator = new DelegationChainValidator(verifier, maxDepth: 3);

        var result = await validator.ValidateChainAsync(
            Array.Empty<string>(),
            new Dictionary<string, SecurityKey> { ["agent://alpha"] = DefaultKey },
            "tool://weather");

        result.IsValid.Should().BeFalse();
        result.ErrorCode.Should().Be("empty_chain");
    }

    [Fact]
    public async Task ValidateChainAsync_WithNullTokens_ReturnsInvalid()
    {
        var nonceStore = new MemoryNonceStore();
        var verifier = new CapabilityTokenVerifier(nonceStore);
        var validator = new DelegationChainValidator(verifier, maxDepth: 3);

        var result = await validator.ValidateChainAsync(
            null!,
            new Dictionary<string, SecurityKey> { ["agent://alpha"] = DefaultKey },
            "tool://weather");

        result.IsValid.Should().BeFalse();
        result.ErrorCode.Should().Be("empty_chain");
    }

    [Fact]
    public async Task ValidateChainAsync_ExceedingMaxDepth_ReturnsInvalid()
    {
        var nonceStore = new MemoryNonceStore();
        var verifier = new CapabilityTokenVerifier(nonceStore);
        var validator = new DelegationChainValidator(verifier, maxDepth: 2);

        var tokens = new List<string> { "tok1", "tok2", "tok3" };

        var result = await validator.ValidateChainAsync(
            tokens,
            new Dictionary<string, SecurityKey> { ["agent://alpha"] = DefaultKey },
            "tool://weather");

        result.IsValid.Should().BeFalse();
        result.ErrorCode.Should().Be("delegation_depth_exceeded");
    }

    [Fact]
    public async Task ValidateChainAsync_WithSingleValidToken_ReturnsValid()
    {
        var nonceStore = new MemoryNonceStore();
        var issuer = new CapabilityTokenIssuer(DefaultKey, SecurityAlgorithms.HmacSha256, nonceStore);
        var verifier = new CapabilityTokenVerifier(nonceStore);
        var validator = new DelegationChainValidator(verifier, maxDepth: 3);

        var minted = issuer.Mint(new CapabilityTokenOptions
        {
            Issuer = "agent://alpha",
            Audience = "tool://weather",
            Capability = new CapabilityClaim { Tool = "Weather", Action = "Read" },
            Context = new CapabilityContext { CorrelationId = "corr-chain-1" }
        });

        var result = await validator.ValidateChainAsync(
            new[] { minted.Token },
            new Dictionary<string, SecurityKey> { ["agent://alpha"] = DefaultKey },
            "tool://weather");

        result.IsValid.Should().BeTrue(result.Error);
        result.Depth.Should().Be(1);
        result.RootIssuer.Should().Be("agent://alpha");
    }

    [Fact]
    public async Task ValidateChainAsync_WithInvalidToken_ReturnsInvalid()
    {
        var nonceStore = new MemoryNonceStore();
        var verifier = new CapabilityTokenVerifier(nonceStore);
        var validator = new DelegationChainValidator(verifier, maxDepth: 3);

        var result = await validator.ValidateChainAsync(
            new[] { "not.a.valid.jwt" },
            new Dictionary<string, SecurityKey> { ["agent://alpha"] = DefaultKey },
            "tool://weather");

        result.IsValid.Should().BeFalse();
        result.ErrorCode.Should().NotBeNullOrWhiteSpace();
    }
}

public class A2ADelegationIssuerTests
{
    private static readonly SymmetricSecurityKey DefaultKey =
        new(Encoding.UTF8.GetBytes("0123456789ABCDEF0123456789ABCDEF"));

    [Fact]
    public async Task DelegateAsync_WithValidRequest_ReturnsToken()
    {
        var nonceStore = new MemoryNonceStore();
        var issuer = new CapabilityTokenIssuer(DefaultKey, SecurityAlgorithms.HmacSha256, nonceStore);
        var policyEngine = new AlwaysPermitPolicyEngine();
        var delegationIssuer = new A2ADelegationIssuer(issuer, policyEngine);

        var result = await delegationIssuer.DelegateAsync(new A2ADelegationOptions
        {
            Issuer = "agent://alpha",
            Audience = "agent://beta",
            Capability = new CapabilityClaim { Tool = "Weather", Action = "Read" },
            Context = new CapabilityContext { CorrelationId = "corr-delegate-1" },
            Delegation = new DelegationChain { DelegatedBy = "agent://alpha", Depth = 0, MaxDepth = 3 }
        });

        result.Token.Should().NotBeNullOrWhiteSpace();
        result.TokenId.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task DelegateAsync_AtMaxDepth_ThrowsInvalidOperationException()
    {
        var nonceStore = new MemoryNonceStore();
        var issuer = new CapabilityTokenIssuer(DefaultKey, SecurityAlgorithms.HmacSha256, nonceStore);
        var policyEngine = new AlwaysPermitPolicyEngine();
        var delegationIssuer = new A2ADelegationIssuer(issuer, policyEngine);

        var act = async () => await delegationIssuer.DelegateAsync(new A2ADelegationOptions
        {
            Issuer = "agent://alpha",
            Audience = "agent://beta",
            Capability = new CapabilityClaim { Tool = "Weather", Action = "Read" },
            Context = new CapabilityContext { CorrelationId = "corr-delegate-max" },
            Delegation = new DelegationChain { DelegatedBy = "agent://alpha", Depth = 3, MaxDepth = 3 }
        });

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*maximum*");
    }

    [Fact]
    public async Task DelegateAsync_WithPolicyDenial_ThrowsInvalidOperationException()
    {
        var nonceStore = new MemoryNonceStore();
        var issuer = new CapabilityTokenIssuer(DefaultKey, SecurityAlgorithms.HmacSha256, nonceStore);
        var policyEngine = new AlwaysDenyPolicyEngine();
        var delegationIssuer = new A2ADelegationIssuer(issuer, policyEngine);

        var act = async () => await delegationIssuer.DelegateAsync(new A2ADelegationOptions
        {
            Issuer = "agent://alpha",
            Audience = "agent://beta",
            Capability = new CapabilityClaim { Tool = "Weather", Action = "Read" },
            Context = new CapabilityContext { CorrelationId = "corr-delegate-deny" },
            Delegation = new DelegationChain { DelegatedBy = "agent://alpha", Depth = 0, MaxDepth = 3 }
        });

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*denied*");
    }

    [Fact]
    public async Task DelegateAsync_WithNullOptions_ThrowsArgumentNullException()
    {
        var nonceStore = new MemoryNonceStore();
        var issuer = new CapabilityTokenIssuer(DefaultKey, SecurityAlgorithms.HmacSha256, nonceStore);
        var policyEngine = new AlwaysPermitPolicyEngine();
        var delegationIssuer = new A2ADelegationIssuer(issuer, policyEngine);

        var act = async () => await delegationIssuer.DelegateAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
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
            return Task.FromResult(PolicyDecision.Deny("Denied by test.", "test_denied"));
        }
    }
}

public class A2ADelegationOptionsTests
{
    [Fact]
    public void Defaults_ShouldHaveExpectedValues()
    {
        var options = new A2ADelegationOptions();

        options.Issuer.Should().BeEmpty();
        options.Audience.Should().BeEmpty();
        options.Lifetime.Should().Be(TimeSpan.FromSeconds(60));
    }
}

public class AttenuationValidatorTests
{
    [Fact]
    public void Validate_WithIdenticalCapabilities_ReturnsValid()
    {
        var parent = new CapabilityTokenOptions
        {
            Issuer = "agent://root",
            Audience = "agent://child",
            Capability = new CapabilityClaim { Tool = "Weather", Action = "Read", Resource = "us-west" },
            Lifetime = TimeSpan.FromMinutes(10),
            Context = new CapabilityContext { TenantId = "t1" }
        };

        var child = new CapabilityTokenOptions
        {
            Issuer = "agent://child",
            Audience = "agent://grandchild",
            Capability = new CapabilityClaim { Tool = "Weather", Action = "Read", Resource = "us-west" },
            Lifetime = TimeSpan.FromMinutes(5),
            Context = new CapabilityContext { TenantId = "t1" },
            Delegation = new DelegationBinding { Depth = 1, MaxDepth = 3, RootIssuer = "agent://root" }
        };

        var result = AttenuationValidator.Validate(parent, child);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithExpandedLifetime_ReturnsInvalid()
    {
        var parent = new CapabilityTokenOptions
        {
            Issuer = "agent://root",
            Audience = "agent://child",
            Capability = new CapabilityClaim { Tool = "Weather", Action = "Read" },
            Lifetime = TimeSpan.FromMinutes(5)
        };

        var child = new CapabilityTokenOptions
        {
            Issuer = "agent://child",
            Audience = "agent://grandchild",
            Capability = new CapabilityClaim { Tool = "Weather", Action = "Read" },
            Lifetime = TimeSpan.FromMinutes(10)
        };

        var result = AttenuationValidator.Validate(parent, child);
        result.IsValid.Should().BeFalse();
        result.Violations.Should().Contain(v => v.Contains("lifetime"));
    }

    [Fact]
    public void Validate_WithDifferentTool_ReturnsInvalid()
    {
        var parent = new CapabilityTokenOptions
        {
            Issuer = "agent://root",
            Audience = "agent://child",
            Capability = new CapabilityClaim { Tool = "Weather", Action = "Read" },
            Lifetime = TimeSpan.FromMinutes(5)
        };

        var child = new CapabilityTokenOptions
        {
            Issuer = "agent://child",
            Audience = "agent://grandchild",
            Capability = new CapabilityClaim { Tool = "Calendar", Action = "Read" },
            Lifetime = TimeSpan.FromMinutes(5)
        };

        var result = AttenuationValidator.Validate(parent, child);
        result.IsValid.Should().BeFalse();
        result.Violations.Should().Contain(v => v.Contains("tool"));
    }

    [Fact]
    public void Validate_WithDifferentTenant_ReturnsInvalid()
    {
        var parent = new CapabilityTokenOptions
        {
            Issuer = "agent://root",
            Audience = "agent://child",
            Capability = new CapabilityClaim { Tool = "Weather", Action = "Read" },
            Lifetime = TimeSpan.FromMinutes(5),
            Context = new CapabilityContext { TenantId = "t1" }
        };

        var child = new CapabilityTokenOptions
        {
            Issuer = "agent://child",
            Audience = "agent://grandchild",
            Capability = new CapabilityClaim { Tool = "Weather", Action = "Read" },
            Lifetime = TimeSpan.FromMinutes(5),
            Context = new CapabilityContext { TenantId = "t2" }
        };

        var result = AttenuationValidator.Validate(parent, child);
        result.IsValid.Should().BeFalse();
        result.Violations.Should().Contain(v => v.Contains("tenant"));
    }

    [Fact]
    public void Validate_WithWrongDelegationDepth_ReturnsInvalid()
    {
        var parent = new CapabilityTokenOptions
        {
            Issuer = "agent://root",
            Audience = "agent://child",
            Capability = new CapabilityClaim { Tool = "Weather", Action = "Read" },
            Lifetime = TimeSpan.FromMinutes(5),
            Delegation = new DelegationBinding { Depth = 1, MaxDepth = 3, RootIssuer = "agent://root" }
        };

        var child = new CapabilityTokenOptions
        {
            Issuer = "agent://child",
            Audience = "agent://grandchild",
            Capability = new CapabilityClaim { Tool = "Weather", Action = "Read" },
            Lifetime = TimeSpan.FromMinutes(5),
            Delegation = new DelegationBinding { Depth = 5, MaxDepth = 3, RootIssuer = "agent://root" }
        };

        var result = AttenuationValidator.Validate(parent, child);
        result.IsValid.Should().BeFalse();
        result.Violations.Should().Contain(v => v.Contains("depth"));
    }

    [Fact]
    public void Validate_WithNullParent_ThrowsArgumentNullException()
    {
        var child = new CapabilityTokenOptions { Issuer = "i", Audience = "a" };
        var act = () => AttenuationValidator.Validate(null!, child);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Validate_WithNullChild_ThrowsArgumentNullException()
    {
        var parent = new CapabilityTokenOptions { Issuer = "i", Audience = "a" };
        var act = () => AttenuationValidator.Validate(parent, null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Validate_FirstDelegation_MustHaveDepth1()
    {
        var parent = new CapabilityTokenOptions
        {
            Issuer = "agent://root",
            Audience = "agent://child",
            Capability = new CapabilityClaim { Tool = "Weather", Action = "Read" },
            Lifetime = TimeSpan.FromMinutes(5)
        };

        var child = new CapabilityTokenOptions
        {
            Issuer = "agent://child",
            Audience = "agent://grandchild",
            Capability = new CapabilityClaim { Tool = "Weather", Action = "Read" },
            Lifetime = TimeSpan.FromMinutes(5),
            Delegation = new DelegationBinding { Depth = 1, MaxDepth = 3, RootIssuer = "agent://root" }
        };

        var result = AttenuationValidator.Validate(parent, child);
        result.IsValid.Should().BeTrue();
    }
}

public class AttenuationValidationResultTests
{
    [Fact]
    public void Valid_ReturnsValidResult()
    {
        var result = AttenuationValidationResult.Valid();
        result.IsValid.Should().BeTrue();
        result.Violations.Should().BeEmpty();
    }

    [Fact]
    public void Invalid_ReturnsViolations()
    {
        var violations = new List<string> { "v1", "v2" };
        var result = AttenuationValidationResult.Invalid(violations);
        result.IsValid.Should().BeFalse();
        result.Violations.Should().HaveCount(2);
    }
}
