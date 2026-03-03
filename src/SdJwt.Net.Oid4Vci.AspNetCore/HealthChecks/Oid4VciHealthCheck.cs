using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using SdJwt.Net.Oid4Vci.AspNetCore.Options;

namespace SdJwt.Net.Oid4Vci.AspNetCore.HealthChecks;

/// <summary>
/// Health check that verifies the OID4VCI issuer is minimally configured and the
/// required services are wired correctly.
/// </summary>
public sealed class Oid4VciHealthCheck : IHealthCheck
{
    private readonly CredentialIssuerOptions _options;
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of <see cref="Oid4VciHealthCheck"/>.
    /// </summary>
    public Oid4VciHealthCheck(
        IOptions<CredentialIssuerOptions> options,
        IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(serviceProvider);

        _options = options.Value;
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc/>
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var issues = new List<string>();

        if (string.IsNullOrWhiteSpace(_options.IssuerUrl))
        {
            issues.Add("IssuerUrl is not configured.");
        }

        if (_options.AccessTokenLifetimeSeconds <= 0)
        {
            issues.Add("AccessTokenLifetimeSeconds must be greater than zero.");
        }

        if (_options.CredentialConfigurationsSupported.Count == 0)
        {
            issues.Add("No CredentialConfigurationsSupported are registered.");
        }

        if (issues.Count > 0)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy(
                description: string.Join("; ", issues)));
        }

        var data = new Dictionary<string, object>
        {
            ["issuerUrl"] = _options.IssuerUrl,
            ["credentialConfigurations"] = _options.CredentialConfigurationsSupported.Count,
            ["accessTokenLifetimeSeconds"] = _options.AccessTokenLifetimeSeconds
        };

        return Task.FromResult(HealthCheckResult.Healthy(data: data));
    }
}
