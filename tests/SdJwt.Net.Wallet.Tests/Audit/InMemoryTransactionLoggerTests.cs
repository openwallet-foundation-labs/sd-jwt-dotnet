using FluentAssertions;
using SdJwt.Net.Wallet.Audit;
using Xunit;

namespace SdJwt.Net.Wallet.Tests.Audit;

/// <summary>
/// Unit tests for <see cref="InMemoryTransactionLogger"/>.
/// </summary>
public class InMemoryTransactionLoggerTests
{
    private readonly InMemoryTransactionLogger _logger;

    public InMemoryTransactionLoggerTests()
    {
        _logger = new InMemoryTransactionLogger();
    }

    [Fact]
    public async Task LogAsync_WithTransactionLog_StoresEntry()
    {
        // Arrange
        var entry = new TransactionLog
        {
            Type = TransactionType.Issuance,
            Status = TransactionStatus.Completed,
            Operation = "AcceptCredentialOffer"
        };

        // Act
        await _logger.LogAsync(entry);

        // Assert
        _logger.GetEntries().Should().HaveCount(1);
        _logger.GetEntries()[0].Operation.Should().Be("AcceptCredentialOffer");
    }

    [Fact]
    public async Task LogAsync_WithNullEntry_ThrowsArgumentNullException()
    {
        // Act
        var act = () => _logger.LogAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task LogAsync_WithCredentialAuditEntry_StoresAsAuditEntry()
    {
        // Arrange
        var entry = new CredentialAuditEntry
        {
            Type = TransactionType.Presentation,
            Status = TransactionStatus.Completed,
            Operation = "PresentCredential",
            CredentialId = "cred-123",
            CredentialOperation = CredentialOperation.Presentation,
            CounterpartyId = "https://verifier.example.com",
            DisclosedClaims = new[] { "given_name", "family_name" }
        };

        // Act
        await _logger.LogAsync(entry);

        // Assert
        _logger.GetEntries().Should().HaveCount(1);
        _logger.GetAuditEntries().Should().HaveCount(1);
        _logger.GetAuditEntries()[0].CredentialId.Should().Be("cred-123");
        _logger.GetAuditEntries()[0].DisclosedClaims.Should().Contain("given_name");
    }

    [Fact]
    public async Task GetAuditEntries_WithMixedEntries_ReturnsOnlyAuditEntries()
    {
        // Arrange
        var plainEntry = new TransactionLog
        {
            Type = TransactionType.Issuance,
            Status = TransactionStatus.Completed,
            Operation = "GenerateKey"
        };
        var auditEntry = new CredentialAuditEntry
        {
            Type = TransactionType.Presentation,
            Status = TransactionStatus.Completed,
            Operation = "PresentCredential",
            CredentialId = "cred-456",
            CredentialOperation = CredentialOperation.Presentation
        };

        // Act
        await _logger.LogAsync(plainEntry);
        await _logger.LogAsync(auditEntry);

        // Assert
        _logger.GetEntries().Should().HaveCount(2);
        _logger.GetAuditEntries().Should().HaveCount(1);
    }

    [Fact]
    public async Task Clear_RemovesAllEntries()
    {
        // Arrange
        await _logger.LogAsync(new TransactionLog { Operation = "op1" });
        await _logger.LogAsync(new TransactionLog { Operation = "op2" });
        _logger.GetEntries().Should().HaveCount(2);

        // Act
        _logger.Clear();

        // Assert
        _logger.GetEntries().Should().BeEmpty();
    }

    [Fact]
    public async Task LogAsync_MultipleConcurrentWrites_AllStored()
    {
        // Arrange
        var tasks = Enumerable.Range(0, 100).Select(i =>
            _logger.LogAsync(new TransactionLog { Operation = $"op-{i}" }));

        // Act
        await Task.WhenAll(tasks);

        // Assert
        _logger.GetEntries().Should().HaveCount(100);
    }
}
