using Microsoft.Extensions.DependencyInjection;

namespace SdJwt.Net.AgentTrust.Policy.Opa;

/// <summary>
/// DI extensions for registering OPA policy engine.
/// </summary>
public static class OpaServiceExtensions
{
    /// <summary>
    /// Registers the OPA HTTP policy engine as the <see cref="IPolicyEngine"/> implementation.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Configuration action for <see cref="OpaOptions"/>.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddAgentTrustOpaPolicy(
        this IServiceCollection services,
        Action<OpaOptions> configure)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        if (configure == null)
        {
            throw new ArgumentNullException(nameof(configure));
        }

        services.Configure(configure);
        services.AddHttpClient("AgentTrustOpa");
        services.AddSingleton<IPolicyEngine, OpaHttpPolicyEngine>();

        return services;
    }
}
