using FluentAssertions;
using SdJwt.Net.Mdoc.Cose;
using SdJwt.Net.Mdoc.Issuer;
using SdJwt.Net.Mdoc.Namespaces;
using Xunit;

namespace SdJwt.Net.Mdoc.Tests.Issuer;

/// <summary>
/// Tests for MdocIssuer credential issuance.
/// </summary>
public class MdocIssuerTests : TestBase
{
    [Fact]
    public async Task IssueAsync_WithValidConfiguration_CreatesSignedMdoc()
    {
        // Arrange
        var deviceCoseKey = CoseKey.FromECDsa(DeviceKey);

        // Act
        var document = await new MdocIssuerBuilder()
            .WithDocType(MdlDocType)
            .WithIssuerKey(IssuerSigningKey)
            .WithDeviceKey(deviceCoseKey)
            .AddClaim(MdlNamespace, "family_name", "Doe")
            .AddClaim(MdlNamespace, "given_name", "John")
            .WithValidity(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddYears(5))
            .BuildAsync(new DefaultCoseCryptoProvider());

        // Assert
        document.Should().NotBeNull();
        document.DocType.Should().Be(MdlDocType);
        document.IssuerSigned.Should().NotBeNull();
        document.IssuerSigned.IssuerAuth.Should().NotBeNullOrEmpty();
        document.IssuerSigned.NameSpaces.Should().ContainKey(MdlNamespace);
    }

    [Fact]
    public void Issue_WithValidConfiguration_CreatesSignedMdoc()
    {
        // Arrange
        var deviceCoseKey = CoseKey.FromECDsa(DeviceKey);

        var issuer = new MdocIssuerBuilder()
            .WithDocType(MdlDocType)
            .WithIssuerKey(IssuerSigningKey)
            .WithDeviceKey(deviceCoseKey)
            .AddClaim(MdlNamespace, "family_name", "Doe")
            .WithValidity(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddYears(5))
            .Build();

        // Act
        var issuerSigned = issuer.Issue();

        // Assert
        issuerSigned.Should().NotBeNull();
        issuerSigned.IssuerAuth.Should().NotBeNullOrEmpty();
        issuerSigned.NameSpaces.Should().ContainKey(MdlNamespace);
    }

    [Fact]
    public void Build_WithoutIssuerKey_ThrowsInvalidOperationException()
    {
        // Arrange
        var deviceCoseKey = CoseKey.FromECDsa(DeviceKey);

        // Act
        var act = () => new MdocIssuerBuilder()
            .WithDocType(MdlDocType)
            .WithDeviceKey(deviceCoseKey)
            .AddClaim(MdlNamespace, "family_name", "Doe")
            .WithValidity(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddYears(5))
            .Build();

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Build_WithoutDocType_ThrowsInvalidOperationException()
    {
        // Arrange
        var deviceCoseKey = CoseKey.FromECDsa(DeviceKey);

        // Act
        var act = () => new MdocIssuerBuilder()
            .WithIssuerKey(IssuerSigningKey)
            .WithDeviceKey(deviceCoseKey)
            .AddClaim(MdlNamespace, "family_name", "Doe")
            .WithValidity(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddYears(5))
            .Build();

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public async Task IssueAsync_WithDataElements_ComputesDigests()
    {
        // Arrange
        var deviceCoseKey = CoseKey.FromECDsa(DeviceKey);

        // Act
        var document = await new MdocIssuerBuilder()
            .WithDocType(MdlDocType)
            .WithIssuerKey(IssuerSigningKey)
            .WithDeviceKey(deviceCoseKey)
            .AddClaim(MdlNamespace, "family_name", "Doe")
            .AddClaim(MdlNamespace, "given_name", "John")
            .WithValidity(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddYears(5))
            .BuildAsync(new DefaultCoseCryptoProvider());

        // Assert
        document.IssuerSigned.NameSpaces[MdlNamespace].Should().HaveCount(2);
        document.IssuerSigned.NameSpaces[MdlNamespace]
            .All(item => item.Random != null && item.Random.Length > 0)
            .Should().BeTrue();
    }

    [Fact]
    public async Task IssueAsync_WithES384Algorithm_CreatesDocument()
    {
        // Arrange
        var deviceCoseKey = CoseKey.FromECDsa(DeviceKey);

        // Act
        var document = await new MdocIssuerBuilder()
            .WithDocType(MdlDocType)
            .WithIssuerKey(IssuerSigningKeyEs384)
            .WithDeviceKey(deviceCoseKey)
            .WithAlgorithm(CoseAlgorithm.ES384)
            .AddClaim(MdlNamespace, "family_name", "Doe")
            .WithValidity(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddYears(5))
            .BuildAsync(new DefaultCoseCryptoProvider());

        // Assert
        document.Should().NotBeNull();
        document.IssuerSigned.IssuerAuth.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void CreateBuilder_ReturnsNewBuilder()
    {
        // Act
        var builder = MdocIssuer.CreateBuilder();

        // Assert
        builder.Should().NotBeNull();
        builder.Should().BeOfType<MdocIssuerBuilder>();
    }
}
