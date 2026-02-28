using FluentAssertions;
using SdJwt.Net.Mdoc.Cose;
using SdJwt.Net.Mdoc.Issuer;
using SdJwt.Net.Mdoc.Namespaces;
using Xunit;

namespace SdJwt.Net.Mdoc.Tests.Issuer;

/// <summary>
/// Tests for MdocIssuerBuilder fluent API.
/// </summary>
public class MdocIssuerBuilderTests : TestBase
{
    [Fact]
    public async Task Build_WithMinimalConfiguration_CreatesMdoc()
    {
        // Arrange
        var deviceCoseKey = CoseKey.FromECDsa(DeviceKey);

        // Act
        var document = await new MdocIssuerBuilder()
            .WithDocType(MdlDocType)
            .WithIssuerKey(IssuerSigningKey)
            .WithDeviceKey(deviceCoseKey)
            .AddClaim(MdlNamespace, "family_name", "Doe")
            .WithValidity(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddYears(5))
            .BuildAsync(new DefaultCoseCryptoProvider());

        // Assert
        document.Should().NotBeNull();
        document.DocType.Should().Be(MdlDocType);
    }

    [Fact]
    public async Task Build_WithMultipleClaims_IncludesAllClaims()
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
            .AddClaim(MdlNamespace, "birth_date", "1990-01-15")
            .WithValidity(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddYears(5))
            .BuildAsync(new DefaultCoseCryptoProvider());

        // Assert
        document.IssuerSigned.NameSpaces[MdlNamespace].Should().HaveCount(3);
    }

    [Fact]
    public async Task Build_WithMdlDataElement_UsesCorrectIdentifier()
    {
        // Arrange
        var deviceCoseKey = CoseKey.FromECDsa(DeviceKey);

        // Act
        var document = await new MdocIssuerBuilder()
            .WithDocType(MdlDocType)
            .WithIssuerKey(IssuerSigningKey)
            .WithDeviceKey(deviceCoseKey)
            .AddMdlElement(MdlDataElement.FamilyName, "Doe")
            .AddMdlElement(MdlDataElement.GivenName, "John")
            .AddMdlElement(MdlDataElement.AgeOver18, true)
            .WithValidity(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddYears(5))
            .BuildAsync(new DefaultCoseCryptoProvider());

        // Assert
        var elements = document.IssuerSigned.NameSpaces[MdlNamespace];
        elements.Should().Contain(e => e.ElementIdentifier == "family_name");
        elements.Should().Contain(e => e.ElementIdentifier == "given_name");
        elements.Should().Contain(e => e.ElementIdentifier == "age_over_18");
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
    public async Task Build_WithES384Algorithm_UsesCorrectAlgorithm()
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
    }

    [Fact]
    public void WithDocType_ChainsMethods()
    {
        // Arrange
        var builder = new MdocIssuerBuilder();

        // Act
        var result = builder.WithDocType(MdlDocType);

        // Assert
        result.Should().BeSameAs(builder);
    }

    [Fact]
    public void AddClaim_ChainsMethods()
    {
        // Arrange
        var builder = new MdocIssuerBuilder();

        // Act
        var result = builder.AddClaim(MdlNamespace, "test", "value");

        // Assert
        result.Should().BeSameAs(builder);
    }

    [Fact]
    public async Task Build_WithMultipleNamespaces_IncludesBothNamespaces()
    {
        // Arrange
        var deviceCoseKey = CoseKey.FromECDsa(DeviceKey);
        var customNamespace = "com.example.custom";

        // Act
        var document = await new MdocIssuerBuilder()
            .WithDocType(MdlDocType)
            .WithIssuerKey(IssuerSigningKey)
            .WithDeviceKey(deviceCoseKey)
            .AddClaim(MdlNamespace, "family_name", "Doe")
            .AddClaim(customNamespace, "custom_field", "custom_value")
            .WithValidity(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddYears(5))
            .BuildAsync(new DefaultCoseCryptoProvider());

        // Assert
        document.IssuerSigned.NameSpaces.Should().ContainKey(MdlNamespace);
        document.IssuerSigned.NameSpaces.Should().ContainKey(customNamespace);
    }

    [Fact]
    public void WithIssuerKey_WithCoseKey_ChainsMethods()
    {
        // Arrange
        var builder = new MdocIssuerBuilder();
        var coseKey = CoseKey.FromECDsa(IssuerSigningKey);

        // Act
        var result = builder.WithIssuerKey(coseKey);

        // Assert
        result.Should().BeSameAs(builder);
    }

    [Fact]
    public void WithAlgorithm_ChainsMethods()
    {
        // Arrange
        var builder = new MdocIssuerBuilder();

        // Act
        var result = builder.WithAlgorithm(CoseAlgorithm.ES256);

        // Assert
        result.Should().BeSameAs(builder);
    }

    [Fact]
    public void WithDeviceKey_ChainsMethods()
    {
        // Arrange
        var builder = new MdocIssuerBuilder();
        var deviceCoseKey = CoseKey.FromECDsa(DeviceKey);

        // Act
        var result = builder.WithDeviceKey(deviceCoseKey);

        // Assert
        result.Should().BeSameAs(builder);
    }

    [Fact]
    public void WithValidity_ChainsMethods()
    {
        // Arrange
        var builder = new MdocIssuerBuilder();

        // Act
        var result = builder.WithValidity(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddYears(1));

        // Assert
        result.Should().BeSameAs(builder);
    }

    [Fact]
    public void AddMdlElement_ChainsMethods()
    {
        // Arrange
        var builder = new MdocIssuerBuilder();

        // Act
        var result = builder.AddMdlElement(MdlDataElement.FamilyName, "Doe");

        // Assert
        result.Should().BeSameAs(builder);
    }
}
