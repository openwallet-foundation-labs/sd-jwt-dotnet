using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SdJwt.Net.Oid4Vci.AspNetCore.HealthChecks;
using SdJwt.Net.Oid4Vci.AspNetCore.Options;
using SdJwt.Net.Oid4Vci.AspNetCore.Services;

namespace SdJwt.Net.Oid4Vci.AspNetCore.Extensions;

/// <summary>
/// Extension methods for registering OID4VCI issuer services in the ASP.NET Core DI container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the core OID4VCI issuer services and configuration, with startup validation.
    /// </summary>
    /// <example>
    /// <code>
    /// builder.Services.AddOid4VciIssuer(options =>
    /// {
    ///     options.IssuerUrl = "https://issuer.example.com";
    ///     options.AccessTokenLifetimeSeconds = 300;
    /// }).UseInMemoryServices();
    /// </code>
    /// </example>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">A delegate to configure <see cref="CredentialIssuerOptions"/>.</param>
    /// <returns>An <see cref="IOid4VciBuilder"/> for further configuration.</returns>
    public static IOid4VciBuilder AddOid4VciIssuer(
        this IServiceCollection services,
        Action<CredentialIssuerOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        services.Configure(configure);

        // Validate options at startup — throws OptionsValidationException if invalid
        services.AddSingleton<IValidateOptions<CredentialIssuerOptions>, CredentialIssuerOptionsValidator>();

        // Require IMemoryCache for metadata caching
        services.TryAddSingleton<IMemoryCache>(sp => new MemoryCache(new MemoryCacheOptions()));

        return new Oid4VciBuilder(services);
    }

    /// <summary>
    /// Registers the OID4VCI health check under the tag <c>"oid4vci"</c>.
    /// Requires <c>AddHealthChecks()</c> to be called first.
    /// </summary>
    public static IHealthChecksBuilder AddOid4VciHealthChecks(this IHealthChecksBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.AddCheck<Oid4VciHealthCheck>("oid4vci", tags: ["oid4vci", "ready"]);
    }
}

/// <summary>
/// Builder interface for further configuration of OID4VCI services.
/// </summary>
public interface IOid4VciBuilder
{
    /// <summary>
    /// Registers in-memory implementations suitable for development and single-node scenarios.
    /// Also registers the background token cleanup service.
    /// </summary>
    IOid4VciBuilder UseInMemoryServices();

    /// <summary>
    /// Registers <c>IDistributedCache</c>-backed implementations suitable for
    /// multi-node production deployments. Requires a distributed cache to be registered
    /// separately (e.g. <c>AddStackExchangeRedisCache</c>).
    /// </summary>
    IOid4VciBuilder UseDistributedCacheServices();

    /// <summary>Registers a custom <see cref="IAccessTokenService"/> implementation.</summary>
    IOid4VciBuilder UseAccessTokenService<T>() where T : class, IAccessTokenService;

    /// <summary>Registers a custom <see cref="IDeferredCredentialStore"/> implementation.</summary>
    IOid4VciBuilder UseDeferredCredentialStore<T>() where T : class, IDeferredCredentialStore;

    /// <summary>Registers a custom <see cref="ICredentialIssuer"/> implementation.</summary>
    IOid4VciBuilder UseCredentialIssuer<T>() where T : class, ICredentialIssuer;
}

internal sealed class Oid4VciBuilder : IOid4VciBuilder
{
    private readonly IServiceCollection _services;

    internal Oid4VciBuilder(IServiceCollection services)
    {
        _services = services;
    }

    public IOid4VciBuilder UseInMemoryServices()
    {
        _services.AddSingleton<InMemoryAccessTokenService>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<CredentialIssuerOptions>>().Value;
            var logger = sp.GetRequiredService<ILogger<InMemoryAccessTokenService>>();
            return new InMemoryAccessTokenService(logger, options.AccessTokenLifetimeSeconds, options.CNonceLifetimeSeconds);
        });

        // Register interface pointing to the singleton so both DI keys resolve the same instance
        _services.AddSingleton<IAccessTokenService>(sp =>
            sp.GetRequiredService<InMemoryAccessTokenService>());

        // Background cleanup — uses the same singleton instance
        _services.AddHostedService(sp =>
        {
            var svc = sp.GetRequiredService<InMemoryAccessTokenService>();
            var options = sp.GetRequiredService<IOptions<CredentialIssuerOptions>>().Value;
            var logger = sp.GetRequiredService<ILogger<InMemoryTokenCleanupService>>();
            return new InMemoryTokenCleanupService(svc, logger, TimeSpan.FromSeconds(options.TokenCleanupIntervalSeconds));
        });

        _services.AddSingleton<IDeferredCredentialStore>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<InMemoryDeferredCredentialStore>>();
            return new InMemoryDeferredCredentialStore(logger);
        });

        return this;
    }

    public IOid4VciBuilder UseDistributedCacheServices()
    {
        _services.AddSingleton<IAccessTokenService>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<CredentialIssuerOptions>>().Value;
            var cache = sp.GetRequiredService<Microsoft.Extensions.Caching.Distributed.IDistributedCache>();
            var logger = sp.GetRequiredService<ILogger<DistributedCacheAccessTokenService>>();
            return new DistributedCacheAccessTokenService(cache, logger, options.AccessTokenLifetimeSeconds, options.CNonceLifetimeSeconds);
        });

        _services.AddSingleton<IDeferredCredentialStore>(sp =>
        {
            var cache = sp.GetRequiredService<Microsoft.Extensions.Caching.Distributed.IDistributedCache>();
            var logger = sp.GetRequiredService<ILogger<DistributedCacheDeferredCredentialStore>>();
            return new DistributedCacheDeferredCredentialStore(cache, logger);
        });

        return this;
    }

    public IOid4VciBuilder UseAccessTokenService<T>() where T : class, IAccessTokenService
    {
        _services.AddSingleton<IAccessTokenService, T>();
        return this;
    }

    public IOid4VciBuilder UseDeferredCredentialStore<T>() where T : class, IDeferredCredentialStore
    {
        _services.AddSingleton<IDeferredCredentialStore, T>();
        return this;
    }

    public IOid4VciBuilder UseCredentialIssuer<T>() where T : class, ICredentialIssuer
    {
        _services.AddSingleton<ICredentialIssuer, T>();
        return this;
    }
}
