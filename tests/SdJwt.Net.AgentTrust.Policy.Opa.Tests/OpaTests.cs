using FluentAssertions;
using SdJwt.Net.AgentTrust.Policy;
using SdJwt.Net.AgentTrust.Policy.Opa;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace SdJwt.Net.AgentTrust.Policy.Opa.Tests;

public class OpaOptionsTests
{
    [Fact]
    public void Defaults_ShouldHaveExpectedValues()
    {
        var options = new OpaOptions();

        options.BaseUrl.Should().Be("http://localhost:8181");
        options.PolicyPath.Should().Be("/v1/data/agenttrust/allow");
        options.Timeout.Should().Be(TimeSpan.FromSeconds(5));
        options.DenyOnError.Should().BeTrue();
    }

    [Fact]
    public void Properties_ShouldBeConfigurable()
    {
        var options = new OpaOptions
        {
            BaseUrl = "http://opa:8181",
            PolicyPath = "/v1/data/custom/policy",
            Timeout = TimeSpan.FromSeconds(10),
            DenyOnError = false
        };

        options.BaseUrl.Should().Be("http://opa:8181");
        options.PolicyPath.Should().Be("/v1/data/custom/policy");
        options.Timeout.Should().Be(TimeSpan.FromSeconds(10));
        options.DenyOnError.Should().BeFalse();
    }
}

public class OpaServiceExtensionsTests
{
    [Fact]
    public void AddAgentTrustOpaPolicy_RegistersRequiredServices()
    {
        var services = new ServiceCollection();
        services.AddAgentTrustOpaPolicy(opt =>
        {
            opt.BaseUrl = "http://opa:8181";
        });

        var provider = services.BuildServiceProvider();

        var options = provider.GetRequiredService<IOptions<OpaOptions>>();
        options.Value.BaseUrl.Should().Be("http://opa:8181");

        var engine = provider.GetService<IPolicyEngine>();
        engine.Should().NotBeNull();
        engine.Should().BeOfType<OpaHttpPolicyEngine>();
    }
}

public class OpaHttpPolicyEngineTests
{
    [Fact]
    public async Task EvaluateAsync_WithNullRequest_ThrowsArgumentNullException()
    {
        var services = new ServiceCollection();
        services.AddAgentTrustOpaPolicy(opt =>
        {
            opt.BaseUrl = "http://opa:8181";
        });
        var provider = services.BuildServiceProvider();
        var engine = provider.GetRequiredService<IPolicyEngine>();

        var act = async () => await engine.EvaluateAsync(null!);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task EvaluateAsync_WithUnreachableOpa_DenyOnErrorTrue_ReturnsDeny()
    {
        var services = new ServiceCollection();
        services.AddAgentTrustOpaPolicy(opt =>
        {
            // Use a non-routable address to force connection failure
            opt.BaseUrl = "http://192.0.2.1:1";
            opt.Timeout = TimeSpan.FromMilliseconds(500);
            opt.DenyOnError = true;
        });
        var provider = services.BuildServiceProvider();
        var engine = provider.GetRequiredService<IPolicyEngine>();

        var result = await engine.EvaluateAsync(new PolicyRequest
        {
            AgentId = "agent://test",
            Tool = "Weather",
            Action = "Read"
        });

        result.IsPermitted.Should().BeFalse();
        result.DenialCode.Should().Be("opa_unreachable");
    }

    [Fact]
    public async Task EvaluateAsync_WithUnreachableOpa_DenyOnErrorFalse_ReturnsPermit()
    {
        var services = new ServiceCollection();
        services.AddAgentTrustOpaPolicy(opt =>
        {
            opt.BaseUrl = "http://192.0.2.1:1";
            opt.Timeout = TimeSpan.FromMilliseconds(500);
            opt.DenyOnError = false;
        });
        var provider = services.BuildServiceProvider();
        var engine = provider.GetRequiredService<IPolicyEngine>();

        var result = await engine.EvaluateAsync(new PolicyRequest
        {
            AgentId = "agent://test",
            Tool = "Weather",
            Action = "Read"
        });

        result.IsPermitted.Should().BeTrue();
    }
}
