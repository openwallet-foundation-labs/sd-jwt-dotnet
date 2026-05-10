using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.AgentTrust.AspNetCore;
using SdJwt.Net.AgentTrust.Core;
using SdJwt.Net.AgentTrust.Policy;
using System.Text;
using Xunit;

namespace SdJwt.Net.AgentTrust.AspNetCore.Tests;

public class AgentTrustVerificationMiddlewareTests
{
    private static readonly SymmetricSecurityKey SharedKey =
        new(Encoding.UTF8.GetBytes("0123456789ABCDEF0123456789ABCDEF"));

    private static CapabilityTokenResult MintToken(
        string issuer = "agent://alpha",
        string audience = "tool://weather",
        string tool = "Weather",
        string action = "Read",
        TimeSpan? lifetime = null)
    {
        var nonceStore = new MemoryNonceStore();
        var tokenIssuer = new CapabilityTokenIssuer(SharedKey, SecurityAlgorithms.HmacSha256, nonceStore);
        var opts = new CapabilityTokenOptions
        {
            Issuer = issuer,
            Audience = audience,
            Capability = new CapabilityClaim { Tool = tool, Action = action },
            Context = new CapabilityContext { CorrelationId = "corr-test" }
        };
        if (lifetime.HasValue)
        {
            opts = opts with
            {
                Lifetime = lifetime.Value
            };
        }

        return tokenIssuer.Mint(opts);
    }

    private static AgentTrustVerificationMiddleware CreateMiddleware(
        CapabilityTokenVerifier verifier,
        string allowedTool = "Weather",
        string allowedAction = "Read",
        string audience = "tool://weather",
        string issuer = "agent://alpha")
    {
        return new AgentTrustVerificationMiddleware(
            _ => Task.CompletedTask,
            verifier,
            new DefaultPolicyEngine(new PolicyBuilder().Allow("*", allowedTool, allowedAction).Build()),
            new LoggingReceiptWriter(),
            Options.Create(new AgentTrustVerificationOptions
            {
                Audience = audience,
                TrustedIssuers = new Dictionary<string, SecurityKey> { [issuer] = SharedKey }
            }));
    }

    [Fact]
    public async Task InvokeAsync_WithValidToken_ShouldSetCapabilityInHttpContext()
    {
        var nonceStore = new MemoryNonceStore();
        var minted = MintToken();
        var verifier = new CapabilityTokenVerifier(nonceStore);

        // Re-mint using shared nonceStore
        var issuer = new CapabilityTokenIssuer(SharedKey, SecurityAlgorithms.HmacSha256, nonceStore);
        var token = issuer.Mint(new CapabilityTokenOptions
        {
            Issuer = "agent://alpha",
            Audience = "tool://weather",
            Capability = new CapabilityClaim { Tool = "Weather", Action = "Read" },
            Context = new CapabilityContext { CorrelationId = "corr-asp" }
        }).Token;

        var context = new DefaultHttpContext();
        context.Request.Path = "/api/weather";
        context.Request.Headers.Authorization = $"Bearer {token}";

        var middleware = new AgentTrustVerificationMiddleware(
            _ => Task.CompletedTask,
            new CapabilityTokenVerifier(nonceStore),
            new DefaultPolicyEngine(new PolicyBuilder().Allow("*", "Weather", "Read").Build()),
            new LoggingReceiptWriter(),
            Options.Create(new AgentTrustVerificationOptions
            {
                Audience = "tool://weather",
                TrustedIssuers = new Dictionary<string, SecurityKey> { ["agent://alpha"] = SharedKey },
                AllowedAlgorithms = new[] { SecurityAlgorithms.HmacSha256 }
            }));

        await middleware.InvokeAsync(context);

        context.GetVerifiedCapability().Should().NotBeNull();
        context.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
    }

    [Fact]
    public async Task InvokeAsync_WithNoAuthorizationHeader_ShouldReturn401()
    {
        var nonceStore = new MemoryNonceStore();
        var verifier = new CapabilityTokenVerifier(nonceStore);
        var middleware = CreateMiddleware(verifier);

        var context = new DefaultHttpContext();
        context.Request.Path = "/api/weather";

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
        context.GetVerifiedCapability().Should().BeNull();
    }

    [Fact]
    public async Task InvokeAsync_WithWrongScheme_ShouldReturn401()
    {
        var nonceStore = new MemoryNonceStore();
        var tokenIssuer = new CapabilityTokenIssuer(SharedKey, SecurityAlgorithms.HmacSha256, nonceStore);
        var token = tokenIssuer.Mint(new CapabilityTokenOptions
        {
            Issuer = "agent://alpha",
            Audience = "tool://weather",
            Capability = new CapabilityClaim { Tool = "Weather", Action = "Read" },
            Context = new CapabilityContext { CorrelationId = "corr-wrong-scheme" }
        }).Token;

        var verifier = new CapabilityTokenVerifier(nonceStore);
        var middleware = CreateMiddleware(verifier);

        var context = new DefaultHttpContext();
        context.Request.Headers.Authorization = $"Basic {token}";

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
    }

    [Fact]
    public async Task InvokeAsync_WithInvalidToken_ShouldReturn403()
    {
        var nonceStore = new MemoryNonceStore();
        var verifier = new CapabilityTokenVerifier(nonceStore);
        var middleware = CreateMiddleware(verifier);

        var context = new DefaultHttpContext();
        context.Request.Headers.Authorization = "Bearer not.a.valid.token";

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
    }

    [Fact]
    public async Task InvokeAsync_WithPolicyDenied_ShouldReturn403()
    {
        var nonceStore = new MemoryNonceStore();
        var tokenIssuer = new CapabilityTokenIssuer(SharedKey, SecurityAlgorithms.HmacSha256, nonceStore);
        var token = tokenIssuer.Mint(new CapabilityTokenOptions
        {
            Issuer = "agent://alpha",
            Audience = "tool://weather",
            Capability = new CapabilityClaim { Tool = "Weather", Action = "Delete" },
            Context = new CapabilityContext { CorrelationId = "corr-policy-deny" }
        }).Token;

        var middleware = new AgentTrustVerificationMiddleware(
            _ => Task.CompletedTask,
            new CapabilityTokenVerifier(nonceStore),
            new DefaultPolicyEngine(new PolicyBuilder().Allow("*", "Weather", "Read").Build()),
            new LoggingReceiptWriter(),
            Options.Create(new AgentTrustVerificationOptions
            {
                Audience = "tool://weather",
                TrustedIssuers = new Dictionary<string, SecurityKey> { ["agent://alpha"] = SharedKey }
            }));

        var context = new DefaultHttpContext();
        context.Request.Headers.Authorization = $"SdJwt {token}";

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
    }

    [Fact]
    public async Task InvokeAsync_WithExcludedPath_ShouldBypass()
    {
        var nonceStore = new MemoryNonceStore();
        var verifier = new CapabilityTokenVerifier(nonceStore);

        var middleware = new AgentTrustVerificationMiddleware(
            _ => Task.CompletedTask,
            verifier,
            new DefaultPolicyEngine(new PolicyBuilder().Allow("*", "*", "*").Build()),
            new LoggingReceiptWriter(),
            Options.Create(new AgentTrustVerificationOptions
            {
                Audience = "tool://weather",
                TrustedIssuers = new Dictionary<string, SecurityKey> { ["agent://alpha"] = SharedKey },
                ExcludedPaths = new List<string> { "/health" }
            }));

        var context = new DefaultHttpContext();
        context.Request.Path = "/health";

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
    }
}
