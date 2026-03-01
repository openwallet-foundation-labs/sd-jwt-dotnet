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
}
