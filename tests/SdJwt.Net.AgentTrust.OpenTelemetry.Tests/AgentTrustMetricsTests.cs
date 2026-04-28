using System.Diagnostics.Metrics;
using FluentAssertions;
using SdJwt.Net.AgentTrust.Core;
using SdJwt.Net.AgentTrust.OpenTelemetry;
using Xunit;

namespace SdJwt.Net.AgentTrust.OpenTelemetry.Tests;

public class AgentTrustMetricsTests
{
    [Fact]
    public void RecordMint_WithToolAndAction_DoesNotThrow()
    {
        var act = () => AgentTrustMetrics.RecordMint("Weather", "Read");
        act.Should().NotThrow();
    }

    [Fact]
    public void RecordMint_WithDuration_DoesNotThrow()
    {
        var act = () => AgentTrustMetrics.RecordMint("Calendar", "Write", 42.5);
        act.Should().NotThrow();
    }

    [Fact]
    public void RecordVerify_ValidTrue_DoesNotThrow()
    {
        var act = () => AgentTrustMetrics.RecordVerify("Weather", "Read", valid: true);
        act.Should().NotThrow();
    }

    [Fact]
    public void RecordVerify_ValidFalse_DoesNotThrow()
    {
        var act = () => AgentTrustMetrics.RecordVerify("Weather", "Read", valid: false);
        act.Should().NotThrow();
    }

    [Fact]
    public void RecordVerify_WithDuration_DoesNotThrow()
    {
        var act = () => AgentTrustMetrics.RecordVerify("Weather", "Read", valid: true, durationMs: 10.0);
        act.Should().NotThrow();
    }

    [Fact]
    public void RecordPolicyEvaluation_Permitted_DoesNotThrow()
    {
        var act = () => AgentTrustMetrics.RecordPolicyEvaluation(permitted: true, "Weather", "Read");
        act.Should().NotThrow();
    }

    [Fact]
    public void RecordPolicyEvaluation_Denied_DoesNotThrow()
    {
        var act = () => AgentTrustMetrics.RecordPolicyEvaluation(permitted: false, "Weather", "Read");
        act.Should().NotThrow();
    }

    [Fact]
    public void MeterName_ShouldBeCorrect()
    {
        AgentTrustMetrics.MeterName.Should().Be("SdJwt.Net.AgentTrust");
    }
}

public class TelemetryReceiptWriterTests
{
    [Fact]
    public async Task WriteAsync_WithAllowReceipt_RecordsMetricsAndDelegates()
    {
        var innerWriter = new TestReceiptWriter();
        var writer = new TelemetryReceiptWriter(innerWriter);

        var receipt = new AuditReceipt
        {
            TokenId = "tok-1",
            Timestamp = DateTimeOffset.UtcNow,
            Decision = ReceiptDecision.Allow,
            Tool = "Weather",
            Action = "Read",
            CorrelationId = "corr-1"
        };

        await writer.WriteAsync(receipt);

        innerWriter.WrittenReceipts.Should().ContainSingle();
        innerWriter.WrittenReceipts[0].TokenId.Should().Be("tok-1");
    }

    [Fact]
    public async Task WriteAsync_WithDenyReceipt_RecordsMetricsAndDelegates()
    {
        var innerWriter = new TestReceiptWriter();
        var writer = new TelemetryReceiptWriter(innerWriter);

        var receipt = new AuditReceipt
        {
            TokenId = "tok-2",
            Timestamp = DateTimeOffset.UtcNow,
            Decision = ReceiptDecision.Deny,
            Tool = "Calendar",
            Action = "Write",
            CorrelationId = "corr-2",
            DenyReason = "Policy denied"
        };

        await writer.WriteAsync(receipt);

        innerWriter.WrittenReceipts.Should().ContainSingle();
        innerWriter.WrittenReceipts[0].Decision.Should().Be(ReceiptDecision.Deny);
    }

    [Fact]
    public async Task WriteAsync_WithNoInner_DoesNotThrow()
    {
        var writer = new TelemetryReceiptWriter();

        var receipt = new AuditReceipt
        {
            TokenId = "tok-3",
            Timestamp = DateTimeOffset.UtcNow,
            Decision = ReceiptDecision.Allow,
            Tool = "Weather",
            Action = "Read",
            CorrelationId = "corr-3"
        };

        var act = async () => await writer.WriteAsync(receipt);
        await act.Should().NotThrowAsync();
    }

    private class TestReceiptWriter : IReceiptWriter
    {
        public List<AuditReceipt> WrittenReceipts { get; } = new();

        public Task WriteAsync(AuditReceipt receipt, CancellationToken cancellationToken = default)
        {
            WrittenReceipts.Add(receipt);
            return Task.CompletedTask;
        }
    }
}
