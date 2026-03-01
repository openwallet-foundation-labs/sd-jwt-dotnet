using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using SdJwt.Net.AgentTrust.Core;
using SdJwt.Net.AgentTrust.Policy;

namespace SdJwt.Net.AgentTrust.AspNetCore;

/// <summary>
/// Service and pipeline extensions for agent trust middleware.
/// </summary>
public static class AgentTrustAspNetCoreExtensions
{
    /// <summary>
    /// Adds middleware to request pipeline.
    /// </summary>
    public static IApplicationBuilder UseAgentTrustVerification(this IApplicationBuilder app)
    {
        return app.UseMiddleware<AgentTrustVerificationMiddleware>();
    }

    /// <summary>
    /// Adds verification services.
    /// </summary>
    public static IServiceCollection AddAgentTrustVerification(
        this IServiceCollection services,
        Action<AgentTrustVerificationOptions> configure)
    {
        services.Configure(configure);
        services.AddSingleton<INonceStore, MemoryNonceStore>();
        services.AddSingleton<CapabilityTokenVerifier>();
        services.AddSingleton<IReceiptWriter, LoggingReceiptWriter>();
        services.AddSingleton<IPolicyEngine>(_ => new DefaultPolicyEngine(new PolicyBuilder().Deny("*", "*", "*").Build()));
        return services;
    }

    /// <summary>
    /// Adds an authorization policy for a required capability.
    /// </summary>
    public static AuthorizationBuilder AddAgentTrustPolicy(
        this AuthorizationBuilder builder,
        string policyName,
        string tool,
        string action)
    {
        builder.AddPolicy(policyName, policy =>
        {
            policy.RequireAssertion(context =>
            {
                var httpContext = context.Resource as Microsoft.AspNetCore.Http.HttpContext;
                var capability = httpContext?.GetVerifiedCapability();
                return capability != null &&
                       string.Equals(capability.Tool, tool, StringComparison.OrdinalIgnoreCase) &&
                       string.Equals(capability.Action, action, StringComparison.OrdinalIgnoreCase);
            });
        });

        return builder;
    }
}
