using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SdJwt.Net.AgentTrust.Core;
using SdJwt.Net.AgentTrust.Policy;

namespace SdJwt.Net.AgentTrust.Maf;

/// <summary>
/// Extensions for MAF integration.
/// </summary>
public static class AgentTrustExtensions
{
    /// <summary>
    /// Adds the agent trust middleware to the pipeline.
    /// The middleware evaluates policy and mints capability tokens for each function call.
    /// </summary>
    /// <param name="builder">The agent builder.</param>
    /// <param name="issuer">The capability token issuer.</param>
    /// <param name="policyEngine">The policy engine.</param>
    /// <param name="receiptWriter">The receipt writer.</param>
    /// <param name="configure">A delegate to configure middleware options.</param>
    /// <returns>The builder for chaining.</returns>
    public static IAgentBuilder UseAgentTrust(
        this IAgentBuilder builder,
        CapabilityTokenIssuer issuer,
        IPolicyEngine policyEngine,
        IReceiptWriter receiptWriter,
        Action<AgentTrustMiddlewareOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(issuer);
        ArgumentNullException.ThrowIfNull(policyEngine);
        ArgumentNullException.ThrowIfNull(receiptWriter);
        ArgumentNullException.ThrowIfNull(configure);

        var options = new AgentTrustMiddlewareOptions
        {
            AgentId = "agent://default"
        };
        configure(options);

        var middleware = new AgentTrustMiddleware(issuer, policyEngine, receiptWriter, options);

        builder.Use(async (context, next) =>
        {
            await middleware.InvokeAsync(context, next);
        });

        return builder;
    }

    /// <summary>
    /// Adds the agent trust middleware to the pipeline using services resolved from the provided service provider.
    /// </summary>
    /// <param name="builder">The agent builder.</param>
    /// <param name="services">The service provider used to resolve dependencies.</param>
    /// <param name="configure">A delegate to configure middleware options.</param>
    /// <returns>The builder for chaining.</returns>
    public static IAgentBuilder UseAgentTrust(
        this IAgentBuilder builder,
        IServiceProvider services,
        Action<AgentTrustMiddlewareOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        var issuer = services.GetRequiredService<CapabilityTokenIssuer>();
        var policyEngine = services.GetRequiredService<IPolicyEngine>();
        var receiptWriter = services.GetRequiredService<IReceiptWriter>();

        return builder.UseAgentTrust(issuer, policyEngine, receiptWriter, configure);
    }

    /// <summary>
    /// Registers core agent trust services into an <see cref="IServiceCollection"/>.
    /// Uses the types specified in <paramref name="configure"/> to register implementations.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">A delegate to configure service registration options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddAgentTrust(
        this IServiceCollection services,
        Action<AgentTrustServiceOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        var options = new AgentTrustServiceOptions();
        configure(options);

        services.AddSingleton(typeof(INonceStore), options.NonceStoreType);
        services.AddSingleton(typeof(IReceiptWriter), options.ReceiptWriterType);
        services.AddSingleton(typeof(IPolicyEngine), options.PolicyEngineType);
        services.AddSingleton<ILoggerFactory>(Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance);

        return services;
    }
}
