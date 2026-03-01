using Microsoft.Extensions.DependencyInjection;
using SdJwt.Net.AgentTrust.Core;
using SdJwt.Net.AgentTrust.Policy;

namespace SdJwt.Net.AgentTrust.Maf;

/// <summary>
/// Extensions for MAF integration.
/// </summary>
public static class AgentTrustExtensions
{
    /// <summary>
    /// Adds middleware to an agent builder.
    /// </summary>
    public static IAgentBuilder UseAgentTrust(
        this IAgentBuilder builder,
        Action<AgentTrustMiddlewareOptions> configure)
    {
        var options = new AgentTrustMiddlewareOptions
        {
            AgentId = "agent://default"
        };
        configure(options);

        builder.Use(async (context, next) =>
        {
            await next(context);
        });

        return builder;
    }

    /// <summary>
    /// Adds core services.
    /// </summary>
    public static IServiceCollection AddAgentTrust(
        this IServiceCollection services,
        Action<AgentTrustServiceOptions> configure)
    {
        var options = new AgentTrustServiceOptions();
        configure(options);

        services.AddSingleton<INonceStore, MemoryNonceStore>();
        services.AddSingleton<IReceiptWriter, LoggingReceiptWriter>();
        services.AddSingleton<IPolicyEngine>(_ => new DefaultPolicyEngine(new PolicyBuilder().Deny("*", "*", "*").Build()));

        return services;
    }
}
