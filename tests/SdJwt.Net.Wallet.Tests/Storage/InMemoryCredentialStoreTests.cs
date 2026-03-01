using FluentAssertions;
using SdJwt.Net.Wallet.Core;
using SdJwt.Net.Wallet.Storage;
using Xunit;

namespace SdJwt.Net.Wallet.Tests.Storage;

/// <summary>
/// Unit tests for InMemoryCredentialStore.
/// </summary>
public class InMemoryCredentialStoreTests
{
    private readonly InMemoryCredentialStore _store;

    public InMemoryCredentialStoreTests()
    {
        _store = new InMemoryCredentialStore();
    }

    [Fact]
    public async Task StoreAsync_WithValidCredential_ReturnsId()
    {
        // Arrange
        var credential = new StoredCredential
        {
            Format = "vc+sd-jwt",
            RawCredential = "test.credential.data",
            Type = "DriverLicense",
            Issuer = "https://issuer.example.com"
        };

        // Act
        var id = await _store.StoreAsync(credential);

        // Assert
        id.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task StoreAsync_WithProvidedId_UsesProvidedId()
    {
        // Arrange
        var credential = new StoredCredential
        {
            Id = "custom-id-123",
            Format = "vc+sd-jwt",
            RawCredential = "test.credential.data"
        };

        // Act
        var id = await _store.StoreAsync(credential);

        // Assert
        id.Should().Be("custom-id-123");
    }

    [Fact]
    public async Task StoreAsync_WithNullCredential_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _store.StoreAsync(null!));
    }

    [Fact]
    public async Task GetAsync_WithExistingId_ReturnsCredential()
    {
        // Arrange
        var credential = new StoredCredential
        {
            Format = "vc+sd-jwt",
            RawCredential = "test.credential.data",
            Type = "DriverLicense"
        };
        var id = await _store.StoreAsync(credential);

        // Act
        var retrieved = await _store.GetAsync(id);

        // Assert
        retrieved.Should().NotBeNull();
        retrieved!.Type.Should().Be("DriverLicense");
        retrieved.Format.Should().Be("vc+sd-jwt");
    }

    [Fact]
    public async Task GetAsync_WithNonExistingId_ReturnsNull()
    {
        // Act
        var result = await _store.GetAsync("non-existing-id");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAsync_WithNullId_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _store.GetAsync(null!));
    }

    [Fact]
    public async Task UpdateAsync_WithExistingCredential_ReturnsTrue()
    {
        // Arrange
        var credential = new StoredCredential
        {
            Format = "vc+sd-jwt",
            RawCredential = "test.credential.data",
            Type = "DriverLicense"
        };
        var id = await _store.StoreAsync(credential);

        var updated = credential with
        {
            Id = id,
            Type = "UpdatedDriverLicense"
        };

        // Act
        var result = await _store.UpdateAsync(updated);

        // Assert
        result.Should().BeTrue();

        var retrieved = await _store.GetAsync(id);
        retrieved!.Type.Should().Be("UpdatedDriverLicense");
    }

    [Fact]
    public async Task UpdateAsync_WithNonExistingCredential_ReturnsFalse()
    {
        // Arrange
        var credential = new StoredCredential
        {
            Id = "non-existing-id",
            Format = "vc+sd-jwt",
            RawCredential = "test.credential.data"
        };

        // Act
        var result = await _store.UpdateAsync(credential);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_WithNullCredential_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _store.UpdateAsync(null!));
    }

    [Fact]
    public async Task UpdateAsync_WithEmptyId_ThrowsArgumentException()
    {
        // Arrange
        var credential = new StoredCredential
        {
            Id = "",
            Format = "vc+sd-jwt"
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _store.UpdateAsync(credential));
    }

    [Fact]
    public async Task DeleteAsync_WithExistingId_ReturnsTrue()
    {
        // Arrange
        var credential = new StoredCredential
        {
            Format = "vc+sd-jwt",
            RawCredential = "test.credential.data"
        };
        var id = await _store.StoreAsync(credential);

        // Act
        var result = await _store.DeleteAsync(id);

        // Assert
        result.Should().BeTrue();

        var retrieved = await _store.GetAsync(id);
        retrieved.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistingId_ReturnsFalse()
    {
        // Act
        var result = await _store.DeleteAsync("non-existing-id");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_WithNullId_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _store.DeleteAsync(null!));
    }

    [Fact]
    public async Task ListAllAsync_ReturnsAllCredentials()
    {
        // Arrange
        await _store.StoreAsync(new StoredCredential { Format = "vc+sd-jwt", Type = "Credential1" });
        await _store.StoreAsync(new StoredCredential { Format = "vc+sd-jwt", Type = "Credential2" });
        await _store.StoreAsync(new StoredCredential { Format = "mso_mdoc", Type = "Credential3" });

        // Act
        var results = await _store.ListAllAsync();

        // Assert
        results.Should().HaveCount(3);
    }

    [Fact]
    public async Task ListAllAsync_WhenEmpty_ReturnsEmptyList()
    {
        // Act
        var results = await _store.ListAllAsync();

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public async Task CountAsync_ReturnsCorrectCount()
    {
        // Arrange
        await _store.StoreAsync(new StoredCredential { Format = "vc+sd-jwt" });
        await _store.StoreAsync(new StoredCredential { Format = "vc+sd-jwt" });

        // Act
        var count = await _store.CountAsync();

        // Assert
        count.Should().Be(2);
    }

    [Fact]
    public async Task ClearAsync_RemovesAllCredentials()
    {
        // Arrange
        await _store.StoreAsync(new StoredCredential { Format = "vc+sd-jwt" });
        await _store.StoreAsync(new StoredCredential { Format = "vc+sd-jwt" });

        // Act
        await _store.ClearAsync();

        // Assert
        var count = await _store.CountAsync();
        count.Should().Be(0);
    }

    [Fact]
    public async Task ExistsAsync_WithExistingId_ReturnsTrue()
    {
        // Arrange
        var credential = new StoredCredential { Format = "vc+sd-jwt" };
        var id = await _store.StoreAsync(credential);

        // Act
        var exists = await _store.ExistsAsync(id);

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_WithNonExistingId_ReturnsFalse()
    {
        // Act
        var exists = await _store.ExistsAsync("non-existing-id");

        // Assert
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task QueryAsync_FilterByType_ReturnsMatchingCredentials()
    {
        // Arrange
        await _store.StoreAsync(new StoredCredential
        {
            Format = "vc+sd-jwt",
            Type = "DriverLicense",
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(30)
        });
        await _store.StoreAsync(new StoredCredential
        {
            Format = "vc+sd-jwt",
            Type = "Passport",
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(30)
        });
        await _store.StoreAsync(new StoredCredential
        {
            Format = "vc+sd-jwt",
            Type = "DriverLicense",
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(30)
        });

        var query = new CredentialQuery
        {
            Types = new[] { "DriverLicense" },
            OnlyValid = true
        };

        // Act
        var results = await _store.QueryAsync(query);

        // Assert
        results.Should().HaveCount(2);
        results.Should().AllSatisfy(c => c.Type.Should().Be("DriverLicense"));
    }

    [Fact]
    public async Task QueryAsync_FilterByIssuer_ReturnsMatchingCredentials()
    {
        // Arrange
        await _store.StoreAsync(new StoredCredential
        {
            Format = "vc+sd-jwt",
            Issuer = "https://issuer1.example.com",
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(30)
        });
        await _store.StoreAsync(new StoredCredential
        {
            Format = "vc+sd-jwt",
            Issuer = "https://issuer2.example.com",
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(30)
        });

        var query = new CredentialQuery
        {
            Issuers = new[] { "https://issuer1.example.com" },
            OnlyValid = true
        };

        // Act
        var results = await _store.QueryAsync(query);

        // Assert
        results.Should().HaveCount(1);
        results[0].Issuer.Should().Be("https://issuer1.example.com");
    }

    [Fact]
    public async Task QueryAsync_FilterByFormat_ReturnsMatchingCredentials()
    {
        // Arrange
        await _store.StoreAsync(new StoredCredential
        {
            Format = "vc+sd-jwt",
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(30)
        });
        await _store.StoreAsync(new StoredCredential
        {
            Format = "mso_mdoc",
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(30)
        });

        var query = new CredentialQuery
        {
            FormatId = "mso_mdoc",
            OnlyValid = true
        };

        // Act
        var results = await _store.QueryAsync(query);

        // Assert
        results.Should().HaveCount(1);
        results[0].Format.Should().Be("mso_mdoc");
    }

    [Fact]
    public async Task QueryAsync_OnlyValid_ExcludesExpiredCredentials()
    {
        // Arrange
        await _store.StoreAsync(new StoredCredential
        {
            Format = "vc+sd-jwt",
            Type = "Valid",
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(30)
        });
        await _store.StoreAsync(new StoredCredential
        {
            Format = "vc+sd-jwt",
            Type = "Expired",
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(-1)
        });

        var query = new CredentialQuery { OnlyValid = true };

        // Act
        var results = await _store.QueryAsync(query);

        // Assert
        results.Should().HaveCount(1);
        results[0].Type.Should().Be("Valid");
    }

    [Fact]
    public async Task QueryAsync_OnlyValid_ExcludesRevokedCredentials()
    {
        // Arrange
        await _store.StoreAsync(new StoredCredential
        {
            Format = "vc+sd-jwt",
            Type = "Valid",
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(30),
            Status = CredentialStatusType.Valid
        });
        await _store.StoreAsync(new StoredCredential
        {
            Format = "vc+sd-jwt",
            Type = "Revoked",
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(30),
            Status = CredentialStatusType.Revoked
        });

        var query = new CredentialQuery { OnlyValid = true };

        // Act
        var results = await _store.QueryAsync(query);

        // Assert
        results.Should().HaveCount(1);
        results[0].Type.Should().Be("Valid");
    }

    [Fact]
    public async Task QueryAsync_HasKeyBinding_FiltersCorrectly()
    {
        // Arrange
        await _store.StoreAsync(new StoredCredential
        {
            Format = "vc+sd-jwt",
            Type = "WithKey",
            HolderKeyId = "key-123",
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(30)
        });
        await _store.StoreAsync(new StoredCredential
        {
            Format = "vc+sd-jwt",
            Type = "WithoutKey",
            HolderKeyId = null,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(30)
        });

        var queryWithKey = new CredentialQuery { HasKeyBinding = true, OnlyValid = true };
        var queryWithoutKey = new CredentialQuery { HasKeyBinding = false, OnlyValid = true };

        // Act
        var withKeyResults = await _store.QueryAsync(queryWithKey);
        var withoutKeyResults = await _store.QueryAsync(queryWithoutKey);

        // Assert
        withKeyResults.Should().HaveCount(1);
        withKeyResults[0].Type.Should().Be("WithKey");

        withoutKeyResults.Should().HaveCount(1);
        withoutKeyResults[0].Type.Should().Be("WithoutKey");
    }

    [Fact]
    public async Task QueryAsync_WithLimit_LimitsResults()
    {
        // Arrange
        await _store.StoreAsync(new StoredCredential { Format = "vc+sd-jwt", ExpiresAt = DateTimeOffset.UtcNow.AddDays(30) });
        await _store.StoreAsync(new StoredCredential { Format = "vc+sd-jwt", ExpiresAt = DateTimeOffset.UtcNow.AddDays(30) });
        await _store.StoreAsync(new StoredCredential { Format = "vc+sd-jwt", ExpiresAt = DateTimeOffset.UtcNow.AddDays(30) });

        var query = new CredentialQuery { Limit = 2, OnlyValid = true };

        // Act
        var results = await _store.QueryAsync(query);

        // Assert
        results.Should().HaveCount(2);
    }

    [Fact]
    public async Task QueryAsync_Null_ReturnsAllValidCredentials()
    {
        // Arrange
        await _store.StoreAsync(new StoredCredential
        {
            Format = "vc+sd-jwt",
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(30)
        });

        // Act
        var results = await _store.QueryAsync(null);

        // Assert
        results.Should().HaveCount(1);
    }
}
