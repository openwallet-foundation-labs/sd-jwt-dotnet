using FluentAssertions;
using SdJwt.Net.AgentTrust.Core;
using SdJwt.Net.AgentTrust.Policy;
using Xunit;

namespace SdJwt.Net.AgentTrust.Policy.Tests;

public class DefaultPolicyEngineTests
{
    [Fact]
    public async Task EvaluateAsync_WithAllowedRule_ShouldPermit()
    {
        var rules = new PolicyBuilder()
            .Allow("agent://*", "Weather", "Read")
            .Build();

        var engine = new DefaultPolicyEngine(rules);
        var result = await engine.EvaluateAsync(new PolicyRequest
        {
            AgentId = "agent://alpha",
            Tool = "Weather",
            Action = "Read",
            Context = new CapabilityContext { CorrelationId = "c1" }
        });

        result.IsPermitted.Should().BeTrue();
    }

    [Fact]
    public async Task EvaluateAsync_WithDeniedRuleHigherPriority_ShouldDeny()
    {
        var rules = new List<PolicyRule>
        {
            new()
            {
                Name = "allow-all-weather",
                AgentPattern = "agent://*",
                ToolPattern = "Weather",
                ActionPattern = "*",
                Effect = PolicyEffect.Allow,
                Priority = 0
            },
            new()
            {
                Name = "deny-delete",
                AgentPattern = "agent://*",
                ToolPattern = "Weather",
                ActionPattern = "Delete",
                Effect = PolicyEffect.Deny,
                Priority = 10
            }
        };

        var engine = new DefaultPolicyEngine(rules);
        var result = await engine.EvaluateAsync(new PolicyRequest
        {
            AgentId = "agent://alpha",
            Tool = "Weather",
            Action = "Delete"
        });

        result.IsPermitted.Should().BeFalse();
        result.DenialCode.Should().Be("policy_denied");
    }

    [Fact]
    public async Task EvaluateAsync_WithNoMatchingRule_ShouldDeny()
    {
        var rules = new PolicyBuilder()
            .Allow("agent://beta", "Weather", "Read")
            .Build();

        var engine = new DefaultPolicyEngine(rules);
        var result = await engine.EvaluateAsync(new PolicyRequest
        {
            AgentId = "agent://alpha",
            Tool = "Weather",
            Action = "Read"
        });

        result.IsPermitted.Should().BeFalse();
        result.DenialCode.Should().Be("policy_no_match");
    }

    [Fact]
    public async Task EvaluateAsync_WithWildcardAllow_ShouldPermitAllActions()
    {
        var rules = new PolicyBuilder()
            .Allow("agent://alpha", "Calendar", "*")
            .Build();

        var engine = new DefaultPolicyEngine(rules);

        var read = await engine.EvaluateAsync(new PolicyRequest { AgentId = "agent://alpha", Tool = "Calendar", Action = "Read" });
        var write = await engine.EvaluateAsync(new PolicyRequest { AgentId = "agent://alpha", Tool = "Calendar", Action = "Write" });

        read.IsPermitted.Should().BeTrue();
        write.IsPermitted.Should().BeTrue();
    }

    [Fact]
    public async Task EvaluateAsync_WithDenyAll_ShouldDenyEverything()
    {
        var rules = new PolicyBuilder()
            .Deny("*", "*", "*")
            .Build();

        var engine = new DefaultPolicyEngine(rules);
        var result = await engine.EvaluateAsync(new PolicyRequest
        {
            AgentId = "agent://alpha",
            Tool = "AnyTool",
            Action = "Read"
        });

        result.IsPermitted.Should().BeFalse();
    }

    [Fact]
    public async Task EvaluateAsync_WithDelegationDepthExceeded_ShouldDeny()
    {
        var rules = new PolicyBuilder()
            .Allow("agent://alpha", "Weather", "Read")
            .Build();

        var engine = new DefaultPolicyEngine(rules);
        var result = await engine.EvaluateAsync(new PolicyRequest
        {
            AgentId = "agent://alpha",
            Tool = "Weather",
            Action = "Read",
            DelegationChain = new DelegationChain { Depth = 5, MaxDepth = 3 }
        });

        result.IsPermitted.Should().BeFalse();
        result.DenialCode.Should().Be("delegation_depth_exceeded");
    }

    [Fact]
    public async Task EvaluateAsync_WithDelegationActionNotAllowed_ShouldDeny()
    {
        var rules = new PolicyBuilder()
            .Allow("agent://alpha", "Weather", "*")
            .Build();

        var engine = new DefaultPolicyEngine(rules);
        var result = await engine.EvaluateAsync(new PolicyRequest
        {
            AgentId = "agent://alpha",
            Tool = "Weather",
            Action = "Delete",
            DelegationChain = new DelegationChain
            {
                Depth = 1,
                MaxDepth = 3,
                AllowedActions = new List<string> { "Read", "Write" }
            }
        });

        result.IsPermitted.Should().BeFalse();
        result.DenialCode.Should().Be("delegation_action_denied");
    }

    [Fact]
    public async Task EvaluateAsync_WithExactAgentMatch_ShouldPermit()
    {
        var rules = new PolicyBuilder()
            .Allow("agent://specific-agent", "Tool", "Action")
            .Build();

        var engine = new DefaultPolicyEngine(rules);
        var result = await engine.EvaluateAsync(new PolicyRequest
        {
            AgentId = "agent://specific-agent",
            Tool = "Tool",
            Action = "Action"
        });

        result.IsPermitted.Should().BeTrue();
    }

    [Fact]
    public void Constructor_WithNullRules_ShouldThrow()
    {
        var act = () => new DefaultPolicyEngine(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task EvaluateAsync_WithNullRequest_ShouldThrow()
    {
        var engine = new DefaultPolicyEngine(new PolicyBuilder().Allow("agent://*", "*", "*").Build());
        var act = async () => await engine.EvaluateAsync(null!);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task EvaluateAsync_WithAllowRuleAndConstraints_ShouldIncludeConstraints()
    {
        var constraints = new PolicyConstraints
        {
            MaxTokenLifetime = TimeSpan.FromMinutes(5)
        };

        var rules = new List<PolicyRule>
        {
            new()
            {
                Name = "allow-with-limits",
                AgentPattern = "agent://alpha",
                ToolPattern = "Finance",
                ActionPattern = "Query",
                Effect = PolicyEffect.Allow,
                Priority = 0,
                Constraints = constraints
            }
        };

        var engine = new DefaultPolicyEngine(rules);
        var result = await engine.EvaluateAsync(new PolicyRequest
        {
            AgentId = "agent://alpha",
            Tool = "Finance",
            Action = "Query"
        });

        result.IsPermitted.Should().BeTrue();
        result.Constraints.Should().NotBeNull();
        result.Constraints!.MaxTokenLifetime.Should().Be(TimeSpan.FromMinutes(5));
    }
}

public class PolicyDecisionTests
{
    [Fact]
    public void Permit_SetsEffectToPermit()
    {
        var decision = PolicyDecision.Permit();
        decision.IsPermitted.Should().BeTrue();
        decision.Effect.Should().Be(DecisionEffect.Permit);
    }

    [Fact]
    public void Deny_SetsEffectAndReasonCode()
    {
        var decision = PolicyDecision.Deny("reason", "code");
        decision.IsPermitted.Should().BeFalse();
        decision.Effect.Should().Be(DecisionEffect.Deny);
        decision.ReasonCode.Should().Be("code");
        decision.DenialReason.Should().Be("reason");
        decision.DenialCode.Should().Be("code");
    }

    [Fact]
    public void Obligations_DefaultsToEmpty()
    {
        var decision = new PolicyDecision();
        decision.Obligations.Should().BeEmpty();
    }

    [Fact]
    public void PolicyMetadata_CanBeSet()
    {
        var decision = PolicyDecision.Permit();
        decision.DecisionId = "dec-1";
        decision.PolicyId = "pol-1";
        decision.PolicyVersion = "1.0";
        decision.PolicyHash = "abc123";

        decision.DecisionId.Should().Be("dec-1");
        decision.PolicyId.Should().Be("pol-1");
        decision.PolicyVersion.Should().Be("1.0");
        decision.PolicyHash.Should().Be("abc123");
    }
}

public class PolicyRequestTests
{
    [Fact]
    public void Attributes_DefaultsToEmpty()
    {
        var request = new PolicyRequest();
        request.Attributes.Should().BeEmpty();
    }

    [Fact]
    public void SpecFields_CanBeSet()
    {
        var request = new PolicyRequest
        {
            AgentId = "agent-1",
            UserId = "user-1",
            TenantId = "tenant-1",
            ToolId = "tool-1",
            Tool = "Weather",
            Action = "Read",
            DataClassification = "confidential"
        };

        request.UserId.Should().Be("user-1");
        request.TenantId.Should().Be("tenant-1");
        request.ToolId.Should().Be("tool-1");
        request.DataClassification.Should().Be("confidential");
    }
}

public class DecisionEffectTests
{
    [Fact]
    public void AllValues_AreDefined()
    {
        Enum.GetValues(typeof(DecisionEffect)).Length.Should().Be(4);
    }
}
