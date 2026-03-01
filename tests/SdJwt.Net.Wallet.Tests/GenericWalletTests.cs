using FluentAssertions;
using Moq;
using SdJwt.Net.Wallet.Attestation;
using SdJwt.Net.Wallet.Audit;
using SdJwt.Net.Wallet.Core;
using SdJwt.Net.Wallet.Formats;
using SdJwt.Net.Wallet.Protocols;
using SdJwt.Net.Wallet.Sessions;
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

    [Fact]
    public async Task ProcessCredentialOfferAsync_WithoutOid4VciAdapter_ThrowsInvalidOperationException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _wallet.ProcessCredentialOfferAsync("openid-credential-offer://example"));
    }

    [Fact]
    public async Task ProcessCredentialOfferAsync_WithOid4VciAdapter_ParsesOffer()
    {
        // Arrange
        var adapterMock = new Mock<IOid4VciAdapter>();
        var expected = new CredentialOfferInfo
        {
            CredentialIssuer = "https://issuer.example.com",
            CredentialConfigurationIds = ["pid"]
        };
        adapterMock.Setup(a => a.ParseOfferAsync("openid-credential-offer://example", It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var wallet = new GenericWallet(
            _storeMock.Object,
            _keyManagerMock.Object,
            null,
            new WalletOptions { Oid4VciAdapter = adapterMock.Object });

        // Act
        var result = await wallet.ProcessCredentialOfferAsync("openid-credential-offer://example");

        // Assert
        result.Should().BeSameAs(expected);
    }

    [Fact]
    public async Task AcceptCredentialOfferAsync_WithOid4VciAdapter_RequestsAndStoresCredentials()
    {
        // Arrange
        var adapterMock = new Mock<IOid4VciAdapter>();
        var offer = new CredentialOfferInfo
        {
            CredentialIssuer = "https://issuer.example.com",
            CredentialConfigurationIds = ["pid"],
            PreAuthorizedCode = new PreAuthorizedCodeGrant { Code = "pre-auth-code" }
        };

        var metadata = new Dictionary<string, object>
        {
            ["token_endpoint"] = "https://issuer.example.com/token",
            ["credential_endpoint"] = "https://issuer.example.com/credential"
        };

        adapterMock.Setup(a => a.ResolveIssuerMetadataAsync("https://issuer.example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(metadata);
        adapterMock.Setup(a => a.ExchangeTokenAsync(
                "https://issuer.example.com/token",
                It.IsAny<TokenExchangeOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TokenResult { IsSuccessful = true, AccessToken = "access-token", CNonce = "nonce-1" });
        adapterMock.Setup(a => a.RequestCredentialAsync(
                "https://issuer.example.com/credential",
                It.IsAny<CredentialRequestOptions>(),
                It.IsAny<IKeyManager>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new IssuanceResult
            {
                IsSuccessful = true,
                Credentials =
                [
                    new StoredCredential
                    {
                        Format = "vc+sd-jwt",
                        RawCredential = "raw-cred",
                        Type = "pid"
                    }
                ]
            });

        _keyManagerMock.Setup(k => k.GenerateKeyAsync(It.IsAny<KeyGenerationOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new KeyInfo { KeyId = "generated-key-1", Algorithm = "ES256", CreatedAt = DateTimeOffset.UtcNow });
        _storeMock.Setup(s => s.StoreAsync(It.IsAny<StoredCredential>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("stored-credential-1");

        var wallet = new GenericWallet(
            _storeMock.Object,
            _keyManagerMock.Object,
            null,
            new WalletOptions { Oid4VciAdapter = adapterMock.Object });

        // Act
        var result = await wallet.AcceptCredentialOfferAsync(offer);

        // Assert
        result.IsSuccessful.Should().BeTrue();
        result.Credentials.Should().ContainSingle();
        result.Credentials[0].Id.Should().Be("stored-credential-1");
        _storeMock.Verify(s => s.StoreAsync(It.IsAny<StoredCredential>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessPresentationRequestAsync_WithoutOid4VpAdapter_ThrowsInvalidOperationException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _wallet.ProcessPresentationRequestAsync("openid4vp://example"));
    }

    [Fact]
    public async Task ProcessPresentationRequestAsync_WithOid4VpAdapter_ParsesRequest()
    {
        // Arrange
        var adapterMock = new Mock<IOid4VpAdapter>();
        var expected = new PresentationRequestInfo
        {
            RequestId = "req-1",
            ClientId = "verifier-1",
            ResponseUri = "https://verifier.example.com/response",
            Nonce = "nonce-1"
        };
        adapterMock.Setup(a => a.ParseRequestAsync("openid4vp://example", It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var wallet = new GenericWallet(
            _storeMock.Object,
            _keyManagerMock.Object,
            null,
            new WalletOptions { Oid4VpAdapter = adapterMock.Object });

        // Act
        var result = await wallet.ProcessPresentationRequestAsync("openid4vp://example");

        // Assert
        result.Should().BeSameAs(expected);
    }

    [Fact]
    public async Task CreateAndSubmitPresentationAsync_WithOid4VpAdapter_FindsMatchesAndSubmits()
    {
        // Arrange
        var adapterMock = new Mock<IOid4VpAdapter>();
        var request = new PresentationRequestInfo
        {
            RequestId = "req-1",
            ClientId = "verifier-1",
            ResponseUri = "https://verifier.example.com/response",
            Nonce = "nonce-1"
        };
        var availableCredentials = new List<StoredCredential>
        {
            new() { Id = "cred-1", Format = "vc+sd-jwt", RawCredential = "raw-1", Type = "pid" }
        };
        var matches = new List<CredentialMatch>
        {
            new()
            {
                InputDescriptorId = "id-1",
                Credential = availableCredentials[0],
                SatisfiesRequirements = true
            }
        };

        _storeMock.Setup(s => s.ListAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(availableCredentials);
        adapterMock.Setup(a => a.FindMatchingCredentialsAsync(
                request,
                availableCredentials,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(matches);
        adapterMock.Setup(a => a.SubmitPresentationAsync(
                request,
                It.IsAny<PresentationSubmissionOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PresentationSubmissionResult { IsSuccessful = true });

        var wallet = new GenericWallet(
            _storeMock.Object,
            _keyManagerMock.Object,
            null,
            new WalletOptions { Oid4VpAdapter = adapterMock.Object });

        // Act
        var result = await wallet.CreateAndSubmitPresentationAsync(request);

        // Assert
        result.IsSuccessful.Should().BeTrue();
        adapterMock.Verify(a => a.SubmitPresentationAsync(
            request,
            It.Is<PresentationSubmissionOptions>(o => o.Matches.Count == 1 && o.KeyManager == _keyManagerMock.Object),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task BuildAuthorizationUrlAsync_WithoutOid4VciAdapter_ThrowsInvalidOperationException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _wallet.BuildAuthorizationUrlAsync("https://issuer.example.com/auth", "wallet-client", "https://wallet.example.com/callback"));
    }

    [Fact]
    public async Task BuildAuthorizationUrlAsync_WithOid4VciAdapter_DelegatesToAdapter()
    {
        // Arrange
        var adapterMock = new Mock<IOid4VciAdapter>();
        adapterMock.Setup(a => a.BuildAuthorizationUrlAsync(
                "https://issuer.example.com/auth",
                "wallet-client",
                "https://wallet.example.com/callback",
                "openid",
                null,
                "state-123",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync("https://issuer.example.com/auth?state=state-123");

        var wallet = new GenericWallet(
            _storeMock.Object,
            _keyManagerMock.Object,
            null,
            new WalletOptions { Oid4VciAdapter = adapterMock.Object });

        // Act
        var result = await wallet.BuildAuthorizationUrlAsync(
            "https://issuer.example.com/auth",
            "wallet-client",
            "https://wallet.example.com/callback",
            "openid",
            null,
            "state-123");

        // Assert
        result.Should().Be("https://issuer.example.com/auth?state=state-123");
    }

    [Fact]
    public async Task PollDeferredCredentialAsync_WithoutOid4VciAdapter_ThrowsInvalidOperationException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _wallet.PollDeferredCredentialAsync("https://issuer.example.com/deferred", "tx-123", "token"));
    }

    [Fact]
    public async Task PollDeferredCredentialAsync_WithSuccessfulIssuance_StoresIssuedCredentials()
    {
        // Arrange
        var adapterMock = new Mock<IOid4VciAdapter>();
        adapterMock.Setup(a => a.PollDeferredCredentialAsync(
                "https://issuer.example.com/deferred",
                "tx-123",
                "token",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new IssuanceResult
            {
                IsSuccessful = true,
                Credentials =
                [
                    new StoredCredential
                    {
                        Format = "vc+sd-jwt",
                        Type = "pid",
                        RawCredential = "raw-deferred"
                    }
                ]
            });

        _storeMock.Setup(s => s.StoreAsync(It.IsAny<StoredCredential>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("stored-deferred-1");

        var wallet = new GenericWallet(
            _storeMock.Object,
            _keyManagerMock.Object,
            null,
            new WalletOptions { Oid4VciAdapter = adapterMock.Object });

        // Act
        var result = await wallet.PollDeferredCredentialAsync(
            "https://issuer.example.com/deferred",
            "tx-123",
            "token");

        // Assert
        result.IsSuccessful.Should().BeTrue();
        result.Credentials.Should().ContainSingle();
        result.Credentials[0].Id.Should().Be("stored-deferred-1");
        _storeMock.Verify(s => s.StoreAsync(It.IsAny<StoredCredential>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ResolvePresentationRequestUriAsync_WithoutOid4VpAdapter_ThrowsInvalidOperationException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _wallet.ResolvePresentationRequestUriAsync("https://verifier.example.com/request.jwt"));
    }

    [Fact]
    public async Task ResolvePresentationRequestUriAsync_WithOid4VpAdapter_DelegatesToAdapter()
    {
        // Arrange
        var adapterMock = new Mock<IOid4VpAdapter>();
        var expected = new PresentationRequestInfo
        {
            RequestId = "req-resolved",
            ClientId = "verifier",
            ResponseUri = "https://verifier.example.com/response",
            Nonce = "nonce-1"
        };

        adapterMock.Setup(a => a.ResolveRequestUriAsync(
                "https://verifier.example.com/request.jwt",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var wallet = new GenericWallet(
            _storeMock.Object,
            _keyManagerMock.Object,
            null,
            new WalletOptions { Oid4VpAdapter = adapterMock.Object });

        // Act
        var result = await wallet.ResolvePresentationRequestUriAsync("https://verifier.example.com/request.jwt");

        // Assert
        result.Should().BeSameAs(expected);
    }

    [Fact]
    public async Task GetCredentialsCountAsync_WithDocumentId_CountsValidCredentialsOnly()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        _storeMock.Setup(s => s.ListAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<StoredCredential>
            {
                new() { Id = "1", DocumentId = "doc-1", ExpiresAt = now.AddDays(2), Status = CredentialStatusType.Valid },
                new() { Id = "2", DocumentId = "doc-1", ExpiresAt = now.AddDays(-1), Status = CredentialStatusType.Valid },
                new() { Id = "3", DocumentId = "doc-2", ExpiresAt = now.AddDays(2), Status = CredentialStatusType.Valid },
                new() { Id = "4", DocumentId = "doc-1", ExpiresAt = now.AddDays(2), Status = CredentialStatusType.Revoked }
            });

        // Act
        var count = await _wallet.GetCredentialsCountAsync("doc-1");

        // Assert
        count.Should().Be(1);
    }

    [Fact]
    public async Task FindAvailableCredentialAsync_WithRotateUse_SelectsLeastUsedValidCredential()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;
        _storeMock.Setup(s => s.ListAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<StoredCredential>
            {
                new() { Id = "a", DocumentId = "doc-1", UsageCount = 3, ExpiresAt = now.AddDays(1), Status = CredentialStatusType.Valid },
                new() { Id = "b", DocumentId = "doc-1", UsageCount = 1, ExpiresAt = now.AddDays(1), Status = CredentialStatusType.Valid },
                new() { Id = "c", DocumentId = "doc-1", UsageCount = 2, ExpiresAt = now.AddDays(1), Status = CredentialStatusType.Valid }
            });

        // Act
        var result = await _wallet.FindAvailableCredentialAsync("doc-1", CredentialPolicy.RotateUse);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be("b");
    }

    [Fact]
    public async Task ConsumeCredentialAsync_WithOneTimeUse_DeletesCredential()
    {
        // Arrange
        var credential = new StoredCredential { Id = "cred-1" };
        _storeMock.Setup(s => s.GetAsync("cred-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(credential);
        _storeMock.Setup(s => s.DeleteAsync("cred-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        await _wallet.ConsumeCredentialAsync("cred-1", CredentialPolicy.OneTimeUse);

        // Assert
        _storeMock.Verify(s => s.DeleteAsync("cred-1", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ConsumeCredentialAsync_WithRotateUse_IncrementsUsage()
    {
        // Arrange
        var credential = new StoredCredential { Id = "cred-1", UsageCount = 2 };
        _storeMock.Setup(s => s.GetAsync("cred-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(credential);
        _storeMock.Setup(s => s.UpdateAsync(It.IsAny<StoredCredential>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        await _wallet.ConsumeCredentialAsync("cred-1", CredentialPolicy.RotateUse);

        // Assert
        _storeMock.Verify(s => s.UpdateAsync(
            It.Is<StoredCredential>(c => c.Id == "cred-1" && c.UsageCount == 3),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GenerateWalletAttestationAsync_WithoutProvider_ThrowsInvalidOperationException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _wallet.GenerateWalletAttestationAsync("key-1"));
    }

    [Fact]
    public async Task GenerateWalletAttestationAsync_WithProvider_UsesKeyInfo()
    {
        // Arrange
        var providerMock = new Mock<IWalletAttestationsProvider>();
        _keyManagerMock.Setup(k => k.GetKeyInfoAsync("key-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new KeyInfo { KeyId = "key-1", Algorithm = "ES256", CreatedAt = DateTimeOffset.UtcNow });
        providerMock.Setup(p => p.GetWalletAttestationAsync(It.Is<KeyInfo>(k => k.KeyId == "key-1"), It.IsAny<CancellationToken>()))
            .ReturnsAsync("wia-token");

        var wallet = new GenericWallet(
            _storeMock.Object,
            _keyManagerMock.Object,
            null,
            new WalletOptions { WalletAttestationsProvider = providerMock.Object });

        // Act
        var token = await wallet.GenerateWalletAttestationAsync("key-1");

        // Assert
        token.Should().Be("wia-token");
    }

    [Fact]
    public async Task GenerateKeyAttestationAsync_WithProvider_UsesAllResolvedKeys()
    {
        // Arrange
        var providerMock = new Mock<IWalletAttestationsProvider>();
        _keyManagerMock.Setup(k => k.GetKeyInfoAsync("key-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new KeyInfo { KeyId = "key-1", Algorithm = "ES256", CreatedAt = DateTimeOffset.UtcNow });
        _keyManagerMock.Setup(k => k.GetKeyInfoAsync("key-2", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new KeyInfo { KeyId = "key-2", Algorithm = "ES256", CreatedAt = DateTimeOffset.UtcNow });
        providerMock.Setup(p => p.GetKeyAttestationAsync(
                It.Is<IReadOnlyList<KeyInfo>>(keys => keys.Count == 2),
                "nonce-1",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync("wua-token");

        var wallet = new GenericWallet(
            _storeMock.Object,
            _keyManagerMock.Object,
            null,
            new WalletOptions { WalletAttestationsProvider = providerMock.Object });

        // Act
        var token = await wallet.GenerateKeyAttestationAsync(new[] { "key-1", "key-2" }, "nonce-1");

        // Assert
        token.Should().Be("wua-token");
    }

    [Fact]
    public async Task AcceptCredentialOfferAsync_WithTransactionLogger_LogsIssuanceTransaction()
    {
        // Arrange
        var adapterMock = new Mock<IOid4VciAdapter>();
        var loggerMock = new Mock<ITransactionLogger>();
        var offer = new CredentialOfferInfo
        {
            CredentialIssuer = "https://issuer.example.com",
            CredentialConfigurationIds = ["pid"],
            PreAuthorizedCode = new PreAuthorizedCodeGrant { Code = "pre-auth-code" }
        };

        var metadata = new Dictionary<string, object>
        {
            ["token_endpoint"] = "https://issuer.example.com/token",
            ["credential_endpoint"] = "https://issuer.example.com/credential"
        };

        adapterMock.Setup(a => a.ResolveIssuerMetadataAsync("https://issuer.example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(metadata);
        adapterMock.Setup(a => a.ExchangeTokenAsync(
                "https://issuer.example.com/token",
                It.IsAny<TokenExchangeOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TokenResult { IsSuccessful = true, AccessToken = "access-token" });
        adapterMock.Setup(a => a.RequestCredentialAsync(
                "https://issuer.example.com/credential",
                It.IsAny<CredentialRequestOptions>(),
                It.IsAny<IKeyManager>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new IssuanceResult
            {
                IsSuccessful = true,
                Credentials = [new StoredCredential { Format = "vc+sd-jwt", RawCredential = "raw", Type = "pid" }]
            });

        _keyManagerMock.Setup(k => k.GenerateKeyAsync(It.IsAny<KeyGenerationOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new KeyInfo { KeyId = "generated-key-1", Algorithm = "ES256", CreatedAt = DateTimeOffset.UtcNow });
        _storeMock.Setup(s => s.StoreAsync(It.IsAny<StoredCredential>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("stored-1");
        loggerMock.Setup(l => l.LogAsync(It.IsAny<TransactionLog>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var wallet = new GenericWallet(
            _storeMock.Object,
            _keyManagerMock.Object,
            null,
            new WalletOptions
            {
                Oid4VciAdapter = adapterMock.Object,
                TransactionLogger = loggerMock.Object
            });

        // Act
        await wallet.AcceptCredentialOfferAsync(offer);

        // Assert
        loggerMock.Verify(l => l.LogAsync(
            It.Is<TransactionLog>(t => t.Type == TransactionType.Issuance),
            It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task CreateAndSubmitPresentationAsync_WithTransactionLogger_LogsPresentationTransaction()
    {
        // Arrange
        var adapterMock = new Mock<IOid4VpAdapter>();
        var loggerMock = new Mock<ITransactionLogger>();
        var request = new PresentationRequestInfo
        {
            RequestId = "req-1",
            ClientId = "verifier-1",
            ResponseUri = "https://verifier.example.com/response",
            Nonce = "nonce-1"
        };
        var availableCredentials = new List<StoredCredential>
        {
            new() { Id = "cred-1", Format = "vc+sd-jwt", RawCredential = "raw-1", Type = "pid" }
        };
        var matches = new List<CredentialMatch>
        {
            new() { InputDescriptorId = "id-1", Credential = availableCredentials[0], SatisfiesRequirements = true }
        };

        _storeMock.Setup(s => s.ListAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(availableCredentials);
        adapterMock.Setup(a => a.FindMatchingCredentialsAsync(request, availableCredentials, It.IsAny<CancellationToken>()))
            .ReturnsAsync(matches);
        adapterMock.Setup(a => a.SubmitPresentationAsync(request, It.IsAny<PresentationSubmissionOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PresentationSubmissionResult { IsSuccessful = true });
        loggerMock.Setup(l => l.LogAsync(It.IsAny<TransactionLog>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var wallet = new GenericWallet(
            _storeMock.Object,
            _keyManagerMock.Object,
            null,
            new WalletOptions
            {
                Oid4VpAdapter = adapterMock.Object,
                TransactionLogger = loggerMock.Object
            });

        // Act
        await wallet.CreateAndSubmitPresentationAsync(request);

        // Assert
        loggerMock.Verify(l => l.LogAsync(
            It.Is<TransactionLog>(t => t.Type == TransactionType.Presentation),
            It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task CreateRemotePresentationSession_UsesWalletFlow()
    {
        // Arrange
        var adapterMock = new Mock<IOid4VpAdapter>();
        var request = new PresentationRequestInfo
        {
            RequestId = "req-1",
            ClientId = "verifier-1",
            ResponseUri = "https://verifier.example.com/response",
            Nonce = "nonce-1"
        };

        _storeMock.Setup(s => s.ListAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<StoredCredential>());
        adapterMock.Setup(a => a.ParseRequestAsync("openid4vp://example", It.IsAny<CancellationToken>()))
            .ReturnsAsync(request);
        adapterMock.Setup(a => a.FindMatchingCredentialsAsync(
                request,
                It.IsAny<IReadOnlyList<StoredCredential>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<CredentialMatch>());
        adapterMock.Setup(a => a.SubmitPresentationAsync(
                request,
                It.IsAny<PresentationSubmissionOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PresentationSubmissionResult { IsSuccessful = true });

        var wallet = new GenericWallet(
            _storeMock.Object,
            _keyManagerMock.Object,
            null,
            new WalletOptions { Oid4VpAdapter = adapterMock.Object });

        // Act
        var session = wallet.CreateRemotePresentationSession();
        var receivedRequest = await session.ReceiveRequestAsync("openid4vp://example");
        var result = await session.SendResponseAsync(receivedRequest);

        // Assert
        session.FlowType.Should().Be(PresentationFlowType.Remote);
        result.IsSuccessful.Should().BeTrue();
        session.IsActive.Should().BeTrue();
    }

    [Fact]
    public void CreateProximityPresentationSession_ReturnsProximitySession()
    {
        // Act
        var session = _wallet.CreateProximityPresentationSession();

        // Assert
        session.FlowType.Should().Be(PresentationFlowType.Proximity);
        session.IsActive.Should().BeFalse();
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
        options.Oid4VciAdapter.Should().BeNull();
        options.Oid4VpAdapter.Should().BeNull();
        options.WalletAttestationsProvider.Should().BeNull();
        options.TransactionLogger.Should().BeNull();
    }
}
