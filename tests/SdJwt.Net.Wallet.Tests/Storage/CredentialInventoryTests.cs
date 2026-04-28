using FluentAssertions;
using SdJwt.Net.Wallet.Core;
using SdJwt.Net.Wallet.Storage;
using Xunit;

namespace SdJwt.Net.Wallet.Tests.Storage;

/// <summary>
/// Unit tests for <see cref="ICredentialInventory.FindMatchingAsync"/> on
/// <see cref="InMemoryCredentialStore"/>.
/// </summary>
public class CredentialInventoryTests
{
    private readonly InMemoryCredentialStore _store;

    public CredentialInventoryTests()
    {
        _store = new InMemoryCredentialStore();
    }

    [Fact]
    public void InMemoryCredentialStore_ImplementsICredentialInventory()
    {
        // Assert
        _store.Should().BeAssignableTo<ICredentialInventory>();
    }

    [Fact]
    public async Task FindMatchingAsync_WithNullDescriptorId_ThrowsArgumentNullException()
    {
        // Act
        var act = () => _store.FindMatchingAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task FindMatchingAsync_WithNoCredentials_ReturnsEmptyList()
    {
        // Act
        var result = await _store.FindMatchingAsync("desc-1");

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task FindMatchingAsync_ByType_ReturnsMatchingCredentials()
    {
        // Arrange
        await _store.StoreAsync(new StoredCredential
        {
            Format = "vc+sd-jwt",
            RawCredential = "raw1",
            Type = "DriverLicense",
            Issuer = "https://issuer.example.com"
        });
        await _store.StoreAsync(new StoredCredential
        {
            Format = "vc+sd-jwt",
            RawCredential = "raw2",
            Type = "Passport",
            Issuer = "https://issuer.example.com"
        });

        // Act
        var result = await _store.FindMatchingAsync(
            "desc-1",
            requiredTypes: new[] { "DriverLicense" });

        // Assert
        result.Should().HaveCount(1);
        result[0].Type.Should().Be("DriverLicense");
    }

    [Fact]
    public async Task FindMatchingAsync_ByFormat_ReturnsMatchingCredentials()
    {
        // Arrange
        await _store.StoreAsync(new StoredCredential
        {
            Format = "vc+sd-jwt",
            RawCredential = "raw1",
            Type = "DriverLicense"
        });
        await _store.StoreAsync(new StoredCredential
        {
            Format = "mso_mdoc",
            RawCredential = "raw2",
            Type = "DriverLicense"
        });

        // Act
        var result = await _store.FindMatchingAsync(
            "desc-1",
            requiredFormat: "mso_mdoc");

        // Assert
        result.Should().HaveCount(1);
        result[0].Format.Should().Be("mso_mdoc");
    }

    [Fact]
    public async Task FindMatchingAsync_ExcludesExpiredCredentials()
    {
        // Arrange
        await _store.StoreAsync(new StoredCredential
        {
            Format = "vc+sd-jwt",
            RawCredential = "raw1",
            Type = "DriverLicense",
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(-1)
        });
        await _store.StoreAsync(new StoredCredential
        {
            Format = "vc+sd-jwt",
            RawCredential = "raw2",
            Type = "DriverLicense",
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(1)
        });

        // Act
        var result = await _store.FindMatchingAsync("desc-1");

        // Assert
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task FindMatchingAsync_ExcludesRevokedCredentials()
    {
        // Arrange
        await _store.StoreAsync(new StoredCredential
        {
            Format = "vc+sd-jwt",
            RawCredential = "raw1",
            Type = "DriverLicense",
            Status = CredentialStatusType.Revoked
        });
        await _store.StoreAsync(new StoredCredential
        {
            Format = "vc+sd-jwt",
            RawCredential = "raw2",
            Type = "DriverLicense",
            Status = CredentialStatusType.Valid
        });

        // Act
        var result = await _store.FindMatchingAsync("desc-1");

        // Assert
        result.Should().HaveCount(1);
        result[0].Status.Should().Be(CredentialStatusType.Valid);
    }

    [Fact]
    public async Task FindMatchingAsync_WithTypeAndFormat_FiltersOnBoth()
    {
        // Arrange
        await _store.StoreAsync(new StoredCredential
        {
            Format = "vc+sd-jwt",
            RawCredential = "raw1",
            Type = "DriverLicense"
        });
        await _store.StoreAsync(new StoredCredential
        {
            Format = "mso_mdoc",
            RawCredential = "raw2",
            Type = "DriverLicense"
        });
        await _store.StoreAsync(new StoredCredential
        {
            Format = "vc+sd-jwt",
            RawCredential = "raw3",
            Type = "Passport"
        });

        // Act
        var result = await _store.FindMatchingAsync(
            "desc-1",
            requiredTypes: new[] { "DriverLicense" },
            requiredFormat: "vc+sd-jwt");

        // Assert
        result.Should().HaveCount(1);
        result[0].Type.Should().Be("DriverLicense");
        result[0].Format.Should().Be("vc+sd-jwt");
    }

    [Fact]
    public async Task FindMatchingAsync_CaseInsensitiveTypeMatch()
    {
        // Arrange
        await _store.StoreAsync(new StoredCredential
        {
            Format = "vc+sd-jwt",
            RawCredential = "raw1",
            Type = "DriverLicense"
        });

        // Act
        var result = await _store.FindMatchingAsync(
            "desc-1",
            requiredTypes: new[] { "driverlicense" });

        // Assert
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task FindMatchingAsync_IncludesCredentialsWithNoExpiry()
    {
        // Arrange
        await _store.StoreAsync(new StoredCredential
        {
            Format = "vc+sd-jwt",
            RawCredential = "raw1",
            Type = "DriverLicense",
            ExpiresAt = null
        });

        // Act
        var result = await _store.FindMatchingAsync("desc-1");

        // Assert
        result.Should().HaveCount(1);
    }
}
