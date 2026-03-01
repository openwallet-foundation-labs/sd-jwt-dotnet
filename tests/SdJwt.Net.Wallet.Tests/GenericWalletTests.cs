using FluentAssertions;
using Moq;
using SdJwt.Net.Wallet.Core;
using SdJwt.Net.Wallet.Formats;
using SdJwt.Net.Wallet.Storage;
using Xunit;

namespace SdJwt.Net.Wallet.Tests;

/// <summary>
/// Unit tests for GenericWallet.
/// </summary>
public class GenericWalletTests
{
    private readonly Mock<ICredentialStore> _storeMock;
    private readonly Mock<IKeyManager> _keyManagerMock;
    private readonly GenericWallet _wallet;

    public GenericWalletTests()
    {
        _storeMock = new Mock<ICredentialStore>();
        _keyManagerMock = new Mock<IKeyManager>();
        _wallet = new GenericWallet(
            _storeMock.Object,
            _keyManagerMock.Object,
            null,
            new WalletOptions { ValidateOnAdd = false }
        );
    }

    [Fact]
    public void Constructor_WithNullStore_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new GenericWallet(null!, _keyManagerMock.Object));
    }

    [Fact]
    public void Constructor_WithNullKeyManager_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new GenericWallet(_storeMock.Object, null!));
    }

    [Fact]
    public void WalletId_WithOptions_ReturnsConfiguredValue()
    {
        // Arrange
        var wallet = new GenericWallet(
            _storeMock.Object,
            _keyManagerMock.Object,
            null,
            new WalletOptions { WalletId = "test-wallet" }
        );

        // Assert
        wallet.WalletId.Should().Be("test-wallet");
    }

    [Fact]
    public void WalletId_WithoutOptions_ReturnsDefault()
    {
        // Assert
        _wallet.WalletId.Should().Be("default");
    }

    [Fact]
    public void DisplayName_WithOptions_ReturnsConfiguredValue()
    {
        // Arrange
        var wallet = new GenericWallet(
            _storeMock.Object,
            _keyManagerMock.Object,
            null,
            new WalletOptions { DisplayName = "My Wallet" }
        );

        // Assert
        wallet.DisplayName.Should().Be("My Wallet");
    }

    [Fact]
    public void DisplayName_WithoutOptions_ReturnsDefault()
    {
        // Assert
        _wallet.DisplayName.Should().Be("Generic Wallet");
    }

    [Fact]
    public void RegisterFormatPlugin_WithValidPlugin_Succeeds()
    {
        // Arrange
        var pluginMock = new Mock<ICredentialFormatPlugin>();
        pluginMock.Setup(p => p.FormatId).Returns("test-format");

        // Act
        _wallet.RegisterFormatPlugin(pluginMock.Object);

        // Assert
        var retrieved = _wallet.GetFormatPluginById("test-format");
        retrieved.Should().BeSameAs(pluginMock.Object);
    }

    [Fact]
    public void RegisterFormatPlugin_WithNullPlugin_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            _wallet.RegisterFormatPlugin(null!));
    }

    [Fact]
    public void GetFormatPlugin_WithMatchingCredential_ReturnsPlugin()
    {
        // Arrange
        var pluginMock = new Mock<ICredentialFormatPlugin>();
        pluginMock.Setup(p => p.FormatId).Returns("vc+sd-jwt");
        pluginMock.Setup(p => p.CanHandle(It.IsAny<string>())).Returns(true);

        _wallet.RegisterFormatPlugin(pluginMock.Object);

        // Act
        var result = _wallet.GetFormatPlugin("test.credential.data");

        // Assert
        result.Should().BeSameAs(pluginMock.Object);
    }

    [Fact]
    public void GetFormatPlugin_WithNoMatchingCredential_ReturnsNull()
    {
        // Act
        var result = _wallet.GetFormatPlugin("unknown.credential.data");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetFormatPluginById_WithExistingFormat_ReturnsPlugin()
    {
        // Arrange
        var pluginMock = new Mock<ICredentialFormatPlugin>();
        pluginMock.Setup(p => p.FormatId).Returns("test-format");

        _wallet.RegisterFormatPlugin(pluginMock.Object);

        // Act
        var result = _wallet.GetFormatPluginById("test-format");

        // Assert
        result.Should().BeSameAs(pluginMock.Object);
    }

    [Fact]
    public void GetFormatPluginById_WithNonExistingFormat_ReturnsNull()
    {
        // Act
        var result = _wallet.GetFormatPluginById("non-existing");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task StoreCredentialAsync_WithValidCredential_StoresAndReturns()
    {
        // Arrange
        var parsed = new ParsedCredential
        {
            Format = "vc+sd-jwt",
            Type = "DriverLicense",
            Issuer = "https://issuer.example.com",
            RawCredential = "test.credential.data",
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(30)
        };

        _storeMock.Setup(s => s.StoreAsync(It.IsAny<StoredCredential>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("stored-id-123");

        // Act
        var result = await _wallet.StoreCredentialAsync(parsed, "key-123", "doc-456");

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be("stored-id-123");
        result.Format.Should().Be("vc+sd-jwt");
        result.Type.Should().Be("DriverLicense");
        result.HolderKeyId.Should().Be("key-123");
        result.DocumentId.Should().Be("doc-456");

        _storeMock.Verify(s => s.StoreAsync(It.IsAny<StoredCredential>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task StoreCredentialAsync_WithNullCredential_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _wallet.StoreCredentialAsync(null!));
    }

    [Fact]
    public async Task GetCredentialAsync_WithExistingId_ReturnsCredential()
    {
        // Arrange
        var stored = new StoredCredential { Id = "test-id", Format = "vc+sd-jwt" };
        _storeMock.Setup(s => s.GetAsync("test-id", It.IsAny<CancellationToken>()))
            .ReturnsAsync(stored);

        // Act
        var result = await _wallet.GetCredentialAsync("test-id");

        // Assert
        result.Should().BeSameAs(stored);
    }

    [Fact]
    public async Task GetCredentialAsync_WithNonExistingId_ReturnsNull()
    {
        // Arrange
        _storeMock.Setup(s => s.GetAsync("non-existing", It.IsAny<CancellationToken>()))
            .ReturnsAsync((StoredCredential?)null);

        // Act
        var result = await _wallet.GetCredentialAsync("non-existing");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task FindMatchingCredentialsAsync_WithFilter_QueriesStore()
    {
        // Arrange
        var expected = new List<StoredCredential>
        {
            new() { Id = "1", Type = "DriverLicense" },
            new() { Id = "2", Type = "DriverLicense" }
        };

        _storeMock.Setup(s => s.QueryAsync(It.IsAny<CredentialQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var filter = new CredentialFilter
        {
            CredentialType = "DriverLicense",
            Issuer = null,
            IncludeExpired = false
        };

        // Act
        var results = await _wallet.FindMatchingCredentialsAsync(filter);

        // Assert
        results.Should().HaveCount(2);
        _storeMock.Verify(s => s.QueryAsync(
            It.Is<CredentialQuery>(q =>
                q.Types != null &&
                q.Types.Count == 1 &&
                q.Types[0] == "DriverLicense"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ListCredentialsAsync_WithNoFilter_ListsAll()
    {
        // Arrange
        var expected = new List<StoredCredential>
        {
            new() { Id = "1" },
            new() { Id = "2" }
        };

        _storeMock.Setup(s => s.ListAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        // Act
        var results = await _wallet.ListCredentialsAsync();

        // Assert
        results.Should().HaveCount(2);
        _storeMock.Verify(s => s.ListAllAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ListCredentialsAsync_WithFilter_QueriesStore()
    {
        // Arrange
        var expected = new List<StoredCredential>();
        _storeMock.Setup(s => s.QueryAsync(It.IsAny<CredentialQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var filter = new CredentialFilter { Format = "mso_mdoc" };

        // Act
        await _wallet.ListCredentialsAsync(filter);

        // Assert
        _storeMock.Verify(s => s.QueryAsync(
            It.Is<CredentialQuery>(q => q.FormatId == "mso_mdoc"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteCredentialAsync_CallsStore()
    {
        // Arrange
        _storeMock.Setup(s => s.DeleteAsync("test-id", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _wallet.DeleteCredentialAsync("test-id");

        // Assert
        result.Should().BeTrue();
        _storeMock.Verify(s => s.DeleteAsync("test-id", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RecordUsageAsync_WithExistingCredential_UpdatesUsageCount()
    {
        // Arrange
        var stored = new StoredCredential { Id = "test-id", UsageCount = 5 };
        _storeMock.Setup(s => s.GetAsync("test-id", It.IsAny<CancellationToken>()))
            .ReturnsAsync(stored);
        _storeMock.Setup(s => s.UpdateAsync(It.IsAny<StoredCredential>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        await _wallet.RecordUsageAsync("test-id");

        // Assert
        _storeMock.Verify(s => s.UpdateAsync(
            It.Is<StoredCredential>(c => c.UsageCount == 6),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RecordUsageAsync_WithNonExistingCredential_ThrowsInvalidOperationException()
    {
        // Arrange
        _storeMock.Setup(s => s.GetAsync("non-existing", It.IsAny<CancellationToken>()))
            .ReturnsAsync((StoredCredential?)null);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _wallet.RecordUsageAsync("non-existing"));

        ex.Message.Should().Contain("not found");
    }

    [Fact]
    public async Task CheckStatusAsync_WithExpiredCredential_ReturnsExpired()
    {
        // Arrange
        var stored = new StoredCredential
        {
            Id = "test-id",
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(-1),
            Status = CredentialStatusType.Valid
        };
        _storeMock.Setup(s => s.GetAsync("test-id", It.IsAny<CancellationToken>()))
            .ReturnsAsync(stored);

        // Act
        var result = await _wallet.CheckStatusAsync("test-id");

        // Assert
        result.Should().Be(CredentialStatusType.Expired);
    }

    [Fact]
    public async Task CheckStatusAsync_WithValidCredential_ReturnsStoredStatus()
    {
        // Arrange
        var stored = new StoredCredential
        {
            Id = "test-id",
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(30),
            Status = CredentialStatusType.Valid
        };
        _storeMock.Setup(s => s.GetAsync("test-id", It.IsAny<CancellationToken>()))
            .ReturnsAsync(stored);

        // Act
        var result = await _wallet.CheckStatusAsync("test-id");

        // Assert
        result.Should().Be(CredentialStatusType.Valid);
    }

    [Fact]
    public async Task CheckStatusAsync_WithNonExistingCredential_ThrowsInvalidOperationException()
    {
        // Arrange
        _storeMock.Setup(s => s.GetAsync("non-existing", It.IsAny<CancellationToken>()))
            .ReturnsAsync((StoredCredential?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _wallet.CheckStatusAsync("non-existing"));
    }

    [Fact]
    public async Task ValidateCredentialAsync_WithNonExistingCredential_ReturnsInvalid()
    {
        // Arrange
        _storeMock.Setup(s => s.GetAsync("non-existing", It.IsAny<CancellationToken>()))
            .ReturnsAsync((StoredCredential?)null);

        // Act
        var result = await _wallet.ValidateCredentialAsync("non-existing");

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("not found"));
    }

    [Fact]
    public async Task ValidateCredentialAsync_WithNoPlugin_ReturnsInvalid()
    {
        // Arrange
        var stored = new StoredCredential { Id = "test-id", Format = "unknown-format" };
        _storeMock.Setup(s => s.GetAsync("test-id", It.IsAny<CancellationToken>()))
            .ReturnsAsync(stored);

        // Act
        var result = await _wallet.ValidateCredentialAsync("test-id");

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.Contains("No format plugin"));
    }

    [Fact]
    public async Task BindKeyAsync_WithExistingCredentialAndKey_ReturnsTrue()
    {
        // Arrange
        var stored = new StoredCredential { Id = "test-id", Format = "vc+sd-jwt" };
        _storeMock.Setup(s => s.GetAsync("test-id", It.IsAny<CancellationToken>()))
            .ReturnsAsync(stored);
        _storeMock.Setup(s => s.UpdateAsync(It.IsAny<StoredCredential>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _keyManagerMock.Setup(k => k.KeyExistsAsync("key-123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _wallet.BindKeyAsync("test-id", "key-123");

        // Assert
        result.Should().BeTrue();
        _storeMock.Verify(s => s.UpdateAsync(
            It.Is<StoredCredential>(c => c.BoundKeyId == "key-123"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task BindKeyAsync_WithNonExistingCredential_ReturnsFalse()
    {
        // Arrange
        _storeMock.Setup(s => s.GetAsync("non-existing", It.IsAny<CancellationToken>()))
            .ReturnsAsync((StoredCredential?)null);

        // Act
        var result = await _wallet.BindKeyAsync("non-existing", "key-123");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task BindKeyAsync_WithNonExistingKey_ThrowsInvalidOperationException()
    {
        // Arrange
        var stored = new StoredCredential { Id = "test-id" };
        _storeMock.Setup(s => s.GetAsync("test-id", It.IsAny<CancellationToken>()))
            .ReturnsAsync(stored);
        _keyManagerMock.Setup(k => k.KeyExistsAsync("non-existing-key", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _wallet.BindKeyAsync("test-id", "non-existing-key"));

        ex.Message.Should().Contain("Key");
        ex.Message.Should().Contain("not found");
    }

    [Fact]
    public async Task GetCredentialCountAsync_CallsStore()
    {
        // Arrange
        _storeMock.Setup(s => s.CountAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(5);

        // Act
        var result = await _wallet.GetCredentialCountAsync();

        // Assert
        result.Should().Be(5);
    }

    [Fact]
    public async Task CreatePresentationAsync_WithNonExistingCredential_ThrowsInvalidOperationException()
    {
        // Arrange
        _storeMock.Setup(s => s.GetAsync("non-existing", It.IsAny<CancellationToken>()))
            .ReturnsAsync((StoredCredential?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _wallet.CreatePresentationAsync("non-existing", new[] { "$.name" }, "audience", "nonce"));
    }

    [Fact]
    public async Task CreatePresentationAsync_WithoutBoundKey_ThrowsInvalidOperationException()
    {
        // Arrange
        var stored = new StoredCredential { Id = "test-id", BoundKeyId = null };
        _storeMock.Setup(s => s.GetAsync("test-id", It.IsAny<CancellationToken>()))
            .ReturnsAsync(stored);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _wallet.CreatePresentationAsync("test-id", new[] { "$.name" }, "audience", "nonce"));

        ex.Message.Should().Contain("bound key");
    }
}

/// <summary>
/// Unit tests for WalletOptions.
/// </summary>
public class WalletOptionsTests
{
    [Fact]
    public void Constructor_WithDefaults_InitializesCorrectly()
    {
        // Act
        var options = new WalletOptions();

        // Assert
        options.WalletId.Should().BeNull();
        options.DisplayName.Should().BeNull();
        options.ValidateOnAdd.Should().BeTrue();
        options.AutoCheckStatus.Should().BeFalse();
        options.DefaultKeyOptions.Should().BeNull();
    }
}
