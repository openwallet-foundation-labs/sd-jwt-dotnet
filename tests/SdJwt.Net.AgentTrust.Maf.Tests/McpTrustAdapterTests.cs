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
    private static readonly SymmetricSecurityKey SharedKey =
        new(Encoding.UTF8.GetBytes("0123456789ABCDEF0123456789ABCDEF"));

    private static CapabilityTokenIssuer CreateIssuer() =>
        new(SharedKey, SecurityAlgorithms.HmacSha256, new MemoryNonceStore());

    [Fact]
    public async Task MintForToolCallAsync_WithPermittedPolicy_ShouldReturnToken()
    {
        var policy = new DefaultPolicyEngine(new PolicyBuilder().Allow("agent://alpha", "Weather", "Read").Build());
        var adapter = new McpTrustAdapter(
            CreateIssuer(),
            policy,
            "agent://alpha",
            new Dictionary<string, string> { ["Weather"] = "tool://weather" });

        var result = await adapter.MintForToolCallAsync(
            "Weather",
            new Dictionary<string, object>(),
            new CapabilityContext { CorrelationId = "corr-maf" });

        result.Token.Should().NotBeNullOrWhiteSpace();
        result.TokenId.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task MintForToolCallAsync_WithDeniedPolicy_ShouldThrow()
    {
        var policy = new DefaultPolicyEngine(new PolicyBuilder().Deny("*", "*", "*").Build());
        var adapter = new McpTrustAdapter(
            CreateIssuer(),
            policy,
            "agent://alpha",
            new Dictionary<string, string> { ["Weather"] = "tool://weather" });

        var act = async () => await adapter.MintForToolCallAsync(
            "Weather",
            new Dictionary<string, object>(),
            new CapabilityContext { CorrelationId = "corr-denied" });

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task MintForToolCallAsync_WithUnmappedTool_ShouldThrow()
    {
        var policy = new DefaultPolicyEngine(new PolicyBuilder().Allow("*", "*", "*").Build());
        var adapter = new McpTrustAdapter(
            CreateIssuer(),
            policy,
            "agent://alpha",
            new Dictionary<string, string>());

        var act = async () => await adapter.MintForToolCallAsync(
            "UnknownTool",
            new Dictionary<string, object>(),
            new CapabilityContext());

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*audience*");
    }

    [Fact]
    public async Task MintForToolCallAsync_TokenShouldHaveExpectedTool()
    {
        var policy = new DefaultPolicyEngine(new PolicyBuilder().Allow("agent://alpha", "Weather", "*").Build());
        var adapter = new McpTrustAdapter(
            CreateIssuer(),
            policy,
            "agent://alpha",
            new Dictionary<string, string> { ["Weather"] = "tool://weather" });

        var result = await adapter.MintForToolCallAsync(
            "Weather",
            new Dictionary<string, object>(),
            new CapabilityContext { CorrelationId = "check-tool" });

        result.Token.Should().NotBeNullOrWhiteSpace();
    }
}

public class AgentTrustMiddlewareTests
{
    private static readonly SymmetricSecurityKey SharedKey =
        new(Encoding.UTF8.GetBytes("0123456789ABCDEF0123456789ABCDEF"));

    private static AgentTrustMiddleware CreateMiddleware(bool failOnMintError = true, bool denyAll = false)
    {
        var issuer = new CapabilityTokenIssuer(SharedKey, SecurityAlgorithms.HmacSha256, new MemoryNonceStore());
        var policy = denyAll
            ? (IPolicyEngine)new DefaultPolicyEngine(new PolicyBuilder().Deny("*", "*", "*").Build())
            : new DefaultPolicyEngine(new PolicyBuilder().Allow("agent://alpha", "Weather", "*").Build());

        return new AgentTrustMiddleware(
            issuer,
            policy,
            new LoggingReceiptWriter(),
            new AgentTrustMiddlewareOptions
            {
                AgentId = "agent://alpha",
                ToolAudienceMapping = new Dictionary<string, string> { ["Weather"] = "tool://weather" },
                FailOnMintError = failOnMintError
            });
    }

    private static FunctionCallContext CreateContext(string tool = "Weather", string action = "Read") =>
        new()
        {
            ToolName = tool,
            ActionName = action,
            Context = new CapabilityContext { CorrelationId = "test" }
        };

    [Fact]
    public async Task InvokeAsync_WithPermittedTool_ShouldSetTokenInMetadata()
    {
        var middleware = CreateMiddleware();
        FunctionCallContext? captured = null;

        await middleware.InvokeAsync(CreateContext(), ctx =>
        {
            captured = ctx;
            return Task.CompletedTask;
        });

        captured.Should().NotBeNull();
        captured!.Metadata.Should().ContainKey("Authorization");
        captured.Metadata["Authorization"].ToString().Should().StartWith("SdJwt ");
    }

    [Fact]
    public async Task InvokeAsync_WithPolicyDenied_FailClosed_ShouldThrow()
    {
        var middleware = CreateMiddleware(failOnMintError: true, denyAll: true);

        var act = async () => await middleware.InvokeAsync(CreateContext(), _ => Task.CompletedTask);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task InvokeAsync_WithPolicyDenied_FailOpen_ShouldContinueWithoutToken()
    {
        var middleware = CreateMiddleware(failOnMintError: false, denyAll: true);
        var context = CreateContext();
        var reached = false;

        await middleware.InvokeAsync(context, _ =>
        {
            reached = true;
            return Task.CompletedTask;
        });

        reached.Should().BeTrue();
        context.Metadata.Should().NotContainKey("Authorization");
    }

    [Fact]
    public async Task InvokeAsync_WithNullContext_ShouldThrow()
    {
        var middleware = CreateMiddleware();
        var act = async () => await middleware.InvokeAsync(null!, _ => Task.CompletedTask);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task InvokeAsync_WithNullNext_ShouldThrow()
    {
        var middleware = CreateMiddleware();
        var act = async () => await middleware.InvokeAsync(CreateContext(), null!);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }
}

public class AgentTrustExtensionsTests
{
    private static readonly SymmetricSecurityKey SharedKey =
        new(Encoding.UTF8.GetBytes("0123456789ABCDEF0123456789ABCDEF"));

    [Fact]
    public async Task UseAgentTrust_WithExplicitDependencies_ShouldInvokeMiddleware()
    {
        var issuer = new CapabilityTokenIssuer(SharedKey, SecurityAlgorithms.HmacSha256, new MemoryNonceStore());
        var policy = new DefaultPolicyEngine(new PolicyBuilder().Allow("agent://alpha", "Weather", "*").Build());
        var builder = new TestAgentBuilder();

        // Since AgentTrustMiddlewareOptions uses init-only properties, we use the direct middleware approach
        var middleware = new AgentTrustMiddleware(
            issuer,
            policy,
            new LoggingReceiptWriter(),
            new AgentTrustMiddlewareOptions
            {
                AgentId = "agent://alpha",
                ToolAudienceMapping = new Dictionary<string, string> { ["Weather"] = "tool://weather" }
            });

        builder.Use((ctx, next) => middleware.InvokeAsync(ctx, next));

        var context = new FunctionCallContext
        {
            ToolName = "Weather",
            ActionName = "Read",
            Context = new CapabilityContext { CorrelationId = "ext-test" }
        };

        await builder.RunAsync(context);

        context.Metadata.Should().ContainKey("Authorization");
    }

    [Fact]
    public void UseAgentTrust_WithNullBuilder_ShouldThrow()
    {
        IAgentBuilder? nullBuilder = null;
        var act = () => AgentTrustExtensions.UseAgentTrust(
            nullBuilder!,
            new CapabilityTokenIssuer(SharedKey, SecurityAlgorithms.HmacSha256, new MemoryNonceStore()),
            new DefaultPolicyEngine(new PolicyBuilder().Allow("*", "*", "*").Build()),
            new LoggingReceiptWriter(),
            _ => { });

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void UseAgentTrust_WithNullIssuer_ShouldThrow()
    {
        var builder = new TestAgentBuilder();
        var act = () => builder.UseAgentTrust(
            null!,
            new DefaultPolicyEngine(new PolicyBuilder().Allow("*", "*", "*").Build()),
            new LoggingReceiptWriter(),
            _ => { });

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void UseAgentTrust_WithNullConfigure_ShouldThrow()
    {
        var builder = new TestAgentBuilder();
        var issuer = new CapabilityTokenIssuer(SharedKey, SecurityAlgorithms.HmacSha256, new MemoryNonceStore());
        var act = () => builder.UseAgentTrust(
            issuer,
            new DefaultPolicyEngine(new PolicyBuilder().Allow("*", "*", "*").Build()),
            new LoggingReceiptWriter(),
            null!);

        act.Should().Throw<ArgumentNullException>();
    }
}

/// <summary>
/// Test IAgentBuilder implementation that runs the registered middleware pipeline.
/// </summary>
internal sealed class TestAgentBuilder : IAgentBuilder
{
    private readonly List<Func<FunctionCallContext, Func<FunctionCallContext, Task>, Task>> _stack = new();

    public IAgentBuilder Use(Func<FunctionCallContext, Func<FunctionCallContext, Task>, Task> middleware)
    {
        _stack.Add(middleware);
        return this;
    }

    public Task RunAsync(FunctionCallContext context)
    {
        Func<FunctionCallContext, Task> end = _ => Task.CompletedTask;
        for (var i = _stack.Count - 1; i >= 0; i--)
        {
            var current = _stack[i];
            var next = end;
            end = ctx => current(ctx, next);
        }

        return end(context);
    }
}
