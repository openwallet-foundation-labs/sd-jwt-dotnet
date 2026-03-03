using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SdJwt.Net.Oid4Vci.AspNetCore.Services;

/// <summary>
/// Background service that periodically sweeps expired tokens from
/// <see cref="InMemoryAccessTokenService"/> to prevent unbounded memory growth on long-running servers.
/// Registered automatically when <c>UseInMemoryServices()</c> is called.
/// </summary>
internal sealed class InMemoryTokenCleanupService : BackgroundService
{
    private readonly InMemoryAccessTokenService _tokenService;
    private readonly ILogger<InMemoryTokenCleanupService> _logger;
    private readonly TimeSpan _interval;

    public InMemoryTokenCleanupService(
        InMemoryAccessTokenService tokenService,
        ILogger<InMemoryTokenCleanupService> logger,
        TimeSpan interval)
    {
        _tokenService = tokenService;
        _logger = logger;
        _interval = interval;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogDebug("OID4VCI token cleanup service started. Interval={IntervalSeconds}s", _interval.TotalSeconds);

        using var timer = new PeriodicTimer(_interval);
        while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false))
        {
            try
            {
                var removed = _tokenService.PurgeExpired();
                if (removed > 0)
                {
                    _logger.LogDebug("Purged {Count} expired access token(s).", removed);
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Error during OID4VCI token cleanup sweep.");
            }
        }

        _logger.LogDebug("OID4VCI token cleanup service stopped.");
    }
}
