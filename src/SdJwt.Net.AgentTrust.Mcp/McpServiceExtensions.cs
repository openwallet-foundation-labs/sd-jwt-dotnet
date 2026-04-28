using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.AgentTrust.Core;
using SdJwt.Net.AgentTrust.Policy;

namespace SdJwt.Net.AgentTrust.Mcp;

/// <summary>
/// Extension methods for registering MCP trust services.
/// </summary>
public static class McpServiceExtensions
{
    /// <summary>
    /// Registers MCP client trust interceptor services.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <param name="configureOptions">Options configuration delegate.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddMcpClientTrust(
        this IServiceCollection services,
        Action<McpClientTrustOptions> configureOptions)
    {
        var options = new McpClientTrustOptions();
        configureOptions(options);
        services.AddSingleton(options);
        services.AddSingleton<McpClientTrustInterceptor>();
        return services;
    }

    /// <summary>
    /// Registers MCP server trust guard services.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <param name="configureOptions">Options configuration delegate.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddMcpServerTrust(
        this IServiceCollection services,
        Action<McpServerTrustOptions> configureOptions)
    {
        var options = new McpServerTrustOptions();
        configureOptions(options);
        services.AddSingleton(options);
        services.AddSingleton<McpServerTrustGuard>();
        return services;
    }
}
