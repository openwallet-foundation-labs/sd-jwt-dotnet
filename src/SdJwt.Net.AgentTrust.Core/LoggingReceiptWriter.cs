using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace SdJwt.Net.AgentTrust.Core;

/// <summary>
/// Receipt writer based on ILogger.
/// </summary>
public class LoggingReceiptWriter : IReceiptWriter
{
    private readonly ILogger<LoggingReceiptWriter> _logger;

    /// <summary>
    /// Initializes a new writer.
    /// </summary>
    public LoggingReceiptWriter(ILogger<LoggingReceiptWriter>? logger = null)
    {
        _logger = logger ?? NullLogger<LoggingReceiptWriter>.Instance;
    }

    /// <inheritdoc/>
    public Task WriteAsync(AuditReceipt receipt, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "AgentTrust receipt TokenId={TokenId} Decision={Decision} Tool={Tool} Action={Action} CorrelationId={CorrelationId} DenyReason={DenyReason}",
            receipt.TokenId,
            receipt.Decision,
            receipt.Tool,
            receipt.Action,
            receipt.CorrelationId,
            receipt.DenyReason);

        return Task.CompletedTask;
    }
}

