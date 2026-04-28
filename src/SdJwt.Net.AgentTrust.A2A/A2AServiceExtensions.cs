using Microsoft.Extensions.DependencyInjection;

namespace SdJwt.Net.AgentTrust.A2A;

/// <summary>
/// Extension methods for registering A2A agent trust services.
/// </summary>
public static class A2AServiceExtensions
{
    /// <summary>
    /// Registers A2A delegation issuer and chain validator services.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <param name="maxDelegationDepth">Maximum delegation chain depth (default 3).</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddAgentTrustA2A(
        this IServiceCollection services,
        int maxDelegationDepth = 3)
    {
        services.AddSingleton(sp => new DelegationChainValidator(
            sp.GetRequiredService<Core.CapabilityTokenVerifier>(),
            maxDelegationDepth));
        services.AddSingleton<A2ADelegationIssuer>();
        return services;
    }
}
