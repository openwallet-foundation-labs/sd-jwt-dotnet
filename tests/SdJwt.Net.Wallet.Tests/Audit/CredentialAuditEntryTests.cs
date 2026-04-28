using FluentAssertions;
using SdJwt.Net.Wallet.Audit;
using Xunit;

namespace SdJwt.Net.Wallet.Tests.Audit;

/// <summary>
/// Unit tests for <see cref="CredentialAuditEntry"/> and <see cref="CredentialOperation"/>.
/// </summary>
public class CredentialAuditEntryTests
{
    [Fact]
    public void CredentialAuditEntry_InheritsFromTransactionLog()
    {
        // Assert
        typeof(CredentialAuditEntry).Should().BeDerivedFrom<TransactionLog>();
    }

    [Fact]
    public void CredentialAuditEntry_DefaultValues_AreCorrect()
    {
        // Act
        var entry = new CredentialAuditEntry();

        // Assert
        entry.CredentialId.Should().BeNull();
        entry.CounterpartyId.Should().BeNull();
        entry.DisclosedClaims.Should().BeNull();
        entry.CredentialOperation.Should().Be(CredentialOperation.Issuance);
    }

    [Fact]
    public void CredentialAuditEntry_SetProperties_ReturnsCorrectValues()
    {
        // Arrange & Act
        var entry = new CredentialAuditEntry
        {
            Type = TransactionType.Presentation,
            Status = TransactionStatus.Completed,
            Operation = "PresentCredential",
            CredentialId = "cred-789",
            CredentialOperation = CredentialOperation.Presentation,
            CounterpartyId = "https://verifier.example.com",
            DisclosedClaims = new[] { "email", "name" }
        };

        // Assert
        entry.CredentialId.Should().Be("cred-789");
        entry.CredentialOperation.Should().Be(CredentialOperation.Presentation);
        entry.CounterpartyId.Should().Be("https://verifier.example.com");
        entry.DisclosedClaims.Should().HaveCount(2);
        entry.DisclosedClaims.Should().Contain("email");
    }

    [Theory]
    [InlineData(CredentialOperation.Issuance)]
    [InlineData(CredentialOperation.Presentation)]
    [InlineData(CredentialOperation.StatusCheck)]
    [InlineData(CredentialOperation.Deletion)]
    [InlineData(CredentialOperation.KeyRotation)]
    public void CredentialOperation_AllValues_AreDefined(CredentialOperation operation)
    {
        // Assert
        Enum.IsDefined(typeof(CredentialOperation), operation).Should().BeTrue();
    }
}
