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
    [Fact]
    public async Task InvokeAsync_WithValidToken_ShouldSetCapabilityInHttpContext()
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("0123456789ABCDEF0123456789ABCDEF"));
        var nonce = new MemoryNonceStore();
        var issuer = new CapabilityTokenIssuer(key, SecurityAlgorithms.HmacSha256, nonce);
        var token = issuer.Mint(new CapabilityTokenOptions
        {
            Issuer = "agent://alpha",
            Audience = "tool://weather",
            Capability = new CapabilityClaim { Tool = "Weather", Action = "Read" },
            Context = new CapabilityContext { CorrelationId = "corr-asp" }
        }).Token;

        var context = new DefaultHttpContext();
        context.Request.Path = "/api/weather";
        context.Request.Headers.Authorization = $"SdJwt {token}";

        var middleware = new AgentTrustVerificationMiddleware(
            _ => Task.CompletedTask,
            new CapabilityTokenVerifier(nonce),
            new DefaultPolicyEngine(new PolicyBuilder().Allow("*", "Weather", "Read").Build()),
            new LoggingReceiptWriter(),
            Options.Create(new AgentTrustVerificationOptions
            {
                Audience = "tool://weather",
                TrustedIssuers = new Dictionary<string, SecurityKey> { ["agent://alpha"] = key }
            }));

        await middleware.InvokeAsync(context);

        context.GetVerifiedCapability().Should().NotBeNull();
        context.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
    }
}
