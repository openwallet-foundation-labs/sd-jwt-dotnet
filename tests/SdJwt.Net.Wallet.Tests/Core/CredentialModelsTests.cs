using FluentAssertions;
using SdJwt.Net.Wallet.Core;
using Xunit;

namespace SdJwt.Net.Wallet.Tests.Core;

/// <summary>
/// Unit tests for StoredCredential record.
/// </summary>
public class StoredCredentialTests
{
    [Fact]
    public void Constructor_WithDefaults_InitializesCorrectly()
    {
        // Act
        var credential = new StoredCredential();

        // Assert
        credential.Id.Should().Be(string.Empty);
        credential.Format.Should().Be(string.Empty);
        credential.RawCredential.Should().Be(string.Empty);
        credential.Status.Should().Be(CredentialStatusType.Valid);
        credential.Policy.Should().Be(CredentialPolicy.Default);
    }

    [Fact]
    public void Type_IsAliasForCredentialType()
    {
        // Arrange
        var credential = new StoredCredential
        {
            CredentialType = "TestType"
        };

        // Assert
        credential.Type.Should().Be("TestType");

        // Act - Update via alias
        credential.Type = "UpdatedType";

        // Assert
        credential.CredentialType.Should().Be("UpdatedType");
    }

    [Fact]
    public void BoundKeyId_IsAliasForHolderKeyId()
    {
        // Arrange
        var credential = new StoredCredential
        {
            HolderKeyId = "key-123"
        };

        // Assert
        credential.BoundKeyId.Should().Be("key-123");

        // Act - Update via alias
        credential.BoundKeyId = "key-456";

        // Assert
        credential.HolderKeyId.Should().Be("key-456");
    }

    [Fact]
    public void WithExpression_CreatesNewInstanceWithChanges()
    {
        // Arrange
        var original = new StoredCredential
        {
            Id = "id-1",
            Format = "vc+sd-jwt",
            Type = "DriverLicense",
            Issuer = "https://issuer.example.com"
        };

        // Act
        var updated = original with
        {
            Type = "UpdatedLicense"
        };

        // Assert
        updated.Should().NotBeSameAs(original);
        updated.Id.Should().Be("id-1");
        updated.Type.Should().Be("UpdatedLicense");
        original.Type.Should().Be("DriverLicense");
    }

    [Fact]
    public void StoredAt_DefaultsToUtcNow()
    {
        // Arrange
        var before = DateTimeOffset.UtcNow;

        // Act
        var credential = new StoredCredential();

        var after = DateTimeOffset.UtcNow;

        // Assert
        credential.StoredAt.Should().BeOnOrAfter(before);
        credential.StoredAt.Should().BeOnOrBefore(after);
    }

    [Fact]
    public void Metadata_StartsAsNull()
    {
        // Act
        var credential = new StoredCredential();

        // Assert
        credential.Metadata.Should().BeNull();
    }

    [Fact]
    public void Metadata_CanStoreArbitraryData()
    {
        // Arrange
        var credential = new StoredCredential
        {
            Metadata = new Dictionary<string, object>()
        };

        // Act
        credential.Metadata["customKey"] = "customValue";
        credential.Metadata["numericKey"] = 42;

        // Assert
        credential.Metadata.Should().HaveCount(2);
        credential.Metadata["customKey"].Should().Be("customValue");
        credential.Metadata["numericKey"].Should().Be(42);
    }
}

/// <summary>
/// Unit tests for ParsedCredential.
/// </summary>
public class ParsedCredentialTests
{
    [Fact]
    public void Constructor_WithDefaults_InitializesCorrectly()
    {
        // Act
        var parsed = new ParsedCredential();

        // Assert
        parsed.FormatId.Should().Be(string.Empty);
        parsed.RawCredential.Should().Be(string.Empty);
        parsed.Issuer.Should().BeNull();
        parsed.Type.Should().BeNull();
        parsed.Claims.Should().BeNull();
        parsed.Disclosures.Should().BeNull();
        parsed.Metadata.Should().BeNull();
    }

    [Fact]
    public void Claims_CanBePopulated()
    {
        // Arrange
        var parsed = new ParsedCredential
        {
            Claims = new Dictionary<string, object>()
        };

        // Act
        parsed.Claims["name"] = "John Doe";
        parsed.Claims["age"] = 30;

        // Assert
        parsed.Claims.Should().HaveCount(2);
    }
}

/// <summary>
/// Unit tests for DisclosureInfo.
/// </summary>
public class DisclosureInfoTests
{
    [Fact]
    public void Constructor_WithDefaults_InitializesCorrectly()
    {
        // Act
        var info = new DisclosureInfo();

        // Assert
        info.Path.Should().Be(string.Empty);
        info.Salt.Should().BeNull();
        info.IsSelected.Should().BeFalse();
    }

    [Fact]
    public void AllProperties_CanBeSet()
    {
        // Arrange & Act
        var info = new DisclosureInfo
        {
            Path = "$.name",
            Salt = "abc123",
            Value = "John Doe",
            IsSelected = false
        };

        // Assert
        info.Path.Should().Be("$.name");
        info.Salt.Should().Be("abc123");
        info.Value.Should().Be("John Doe");
        info.IsSelected.Should().BeFalse();
    }
}

/// <summary>
/// Unit tests for ValidationResult.
/// </summary>
public class ValidationResultTests
{
    [Fact]
    public void Constructor_WithDefaults_InitializesCorrectly()
    {
        // Act
        var result = new ValidationResult();

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().BeNull();
    }

    [Fact]
    public void WithErrors_CanAddErrors()
    {
        // Arrange
        var result = new ValidationResult
        {
            IsValid = false,
            Errors = new List<string>()
        };

        // Act
        result.Errors.Add("Test error");

        // Assert
        result.Errors.Should().ContainSingle();
    }
}

/// <summary>
/// Unit tests for CredentialStatusType enum.
/// </summary>
public class CredentialStatusTypeTests
{
    [Theory]
    [InlineData(CredentialStatusType.Valid, "Valid")]
    [InlineData(CredentialStatusType.Expired, "Expired")]
    [InlineData(CredentialStatusType.Revoked, "Revoked")]
    [InlineData(CredentialStatusType.Suspended, "Suspended")]
    [InlineData(CredentialStatusType.Unknown, "Unknown")]
    public void ToString_ReturnsExpectedValue(CredentialStatusType status, string expected)
    {
        // Act & Assert
        status.ToString().Should().Be(expected);
    }

    [Fact]
    public void ValidIsDefault_WhenExplicitlySet()
    {
        // Arrange
        var credential = new StoredCredential
        {
            Status = CredentialStatusType.Valid
        };

        // Assert
        credential.Status.Should().Be(CredentialStatusType.Valid);
    }
}

/// <summary>
/// Unit tests for CredentialPolicy enum.
/// </summary>
public class CredentialPolicyTests
{
    [Theory]
    [InlineData(CredentialPolicy.Default, "Default")]
    [InlineData(CredentialPolicy.OneTimeUse, "OneTimeUse")]
    [InlineData(CredentialPolicy.RotateUse, "RotateUse")]
    public void ToString_ReturnsExpectedValue(CredentialPolicy policy, string expected)
    {
        // Act & Assert
        policy.ToString().Should().Be(expected);
    }
}
