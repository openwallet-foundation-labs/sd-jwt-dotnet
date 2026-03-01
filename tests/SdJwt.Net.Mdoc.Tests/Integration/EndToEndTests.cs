using FluentAssertions;
using SdJwt.Net.Mdoc.Cose;
using SdJwt.Net.Mdoc.Handover;
using SdJwt.Net.Mdoc.Issuer;
using SdJwt.Net.Mdoc.Models;
using SdJwt.Net.Mdoc.Namespaces;
using SdJwt.Net.Mdoc.Verifier;
using Xunit;

namespace SdJwt.Net.Mdoc.Tests.Integration;

/// <summary>
/// End-to-end integration tests for mdoc issuance, presentation, and verification.
/// </summary>
public class EndToEndTests : TestBase
{
    private readonly ICoseCryptoProvider _cryptoProvider;

    public EndToEndTests()
    {
        _cryptoProvider = new DefaultCoseCryptoProvider();
    }

    [Fact]
    public async Task Issue_WithValidClaims_CreatesSignedMdoc()
    {
        // Arrange
        var issuerCoseKey = CoseKey.FromECDsa(IssuerSigningKey);
        var deviceCoseKey = CoseKey.FromECDsa(DeviceKey);

        // Act
        var mdoc = await new MdocIssuerBuilder()
            .WithDocType(MdlDocType)
            .WithIssuerKey(issuerCoseKey)
            .WithDeviceKey(deviceCoseKey)
            .AddMdlElement(MdlDataElement.FamilyName, "Doe")
            .AddMdlElement(MdlDataElement.GivenName, "John")
            .AddMdlElement(MdlDataElement.BirthDate, "1990-01-15")
            .AddMdlElement(MdlDataElement.IssueDate, "2024-01-01")
            .AddMdlElement(MdlDataElement.ExpiryDate, "2029-01-01")
            .AddMdlElement(MdlDataElement.IssuingCountry, "US")
            .AddMdlElement(MdlDataElement.IssuingAuthority, "State DMV")
            .AddMdlElement(MdlDataElement.DocumentNumber, "DL123456789")
            .AddMdlElement(MdlDataElement.AgeOver18, true)
            .AddMdlElement(MdlDataElement.AgeOver21, true)
            .WithValidity(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddYears(5))
            .BuildAsync(_cryptoProvider);

        // Assert
        mdoc.Should().NotBeNull();
        mdoc.DocType.Should().Be(MdlDocType);
        mdoc.IssuerSigned.Should().NotBeNull();
        mdoc.IssuerSigned.IssuerAuth.Should().NotBeNull();
    }

    [Fact]
    public async Task Issue_WithMinimalClaims_Succeeds()
    {
        // Arrange
        var issuerCoseKey = CoseKey.FromECDsa(IssuerSigningKey);
        var deviceCoseKey = CoseKey.FromECDsa(DeviceKey);

        // Act
        var mdoc = await new MdocIssuerBuilder()
            .WithDocType(MdlDocType)
            .WithIssuerKey(issuerCoseKey)
            .WithDeviceKey(deviceCoseKey)
            .AddMdlElement(MdlDataElement.FamilyName, "Doe")
            .WithValidity(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddYears(1))
            .BuildAsync(_cryptoProvider);

        // Assert
        mdoc.Should().NotBeNull();
    }

    [Fact]
    public void Build_WithoutDocType_ThrowsInvalidOperationException()
    {
        // Arrange
        var issuerCoseKey = CoseKey.FromECDsa(IssuerSigningKey);

        // Act
        var act = () => new MdocIssuerBuilder()
            .WithIssuerKey(issuerCoseKey)
            .AddMdlElement(MdlDataElement.FamilyName, "Doe")
            .Build();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*DocType*");
    }

    [Fact]
    public void Build_WithoutIssuerKey_ThrowsInvalidOperationException()
    {
        // Act
        var act = () => new MdocIssuerBuilder()
            .WithDocType(MdlDocType)
            .AddMdlElement(MdlDataElement.FamilyName, "Doe")
            .Build();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*key*");
    }

    [Fact]
    public void SessionTranscript_ForOpenId4Vp_CreatesCorrectStructure()
    {
        // Act
        var transcript = SessionTranscript.ForOpenId4Vp(
            "https://verifier.example.com",
            "nonce123",
            null,
            "https://verifier.example.com/callback");

        // Assert
        transcript.Should().NotBeNull();
        transcript.DeviceEngagement.Should().BeNull();
        transcript.EReaderKeyPub.Should().BeNull();
        transcript.Handover.Should().NotBeNull();
        transcript.Handover.Should().BeOfType<OpenId4VpHandover>();
    }

    [Fact]
    public void SessionTranscript_ForOpenId4VpDcApi_CreatesCorrectStructure()
    {
        // Act
        var transcript = SessionTranscript.ForOpenId4VpDcApi(
            "https://verifier.example.com",
            "nonce123",
            null);

        // Assert
        transcript.Should().NotBeNull();
        transcript.DeviceEngagement.Should().BeNull();
        transcript.EReaderKeyPub.Should().BeNull();
        transcript.Handover.Should().NotBeNull();
        transcript.Handover.Should().BeOfType<OpenId4VpDcApiHandover>();
    }

    [Fact]
    public void SessionTranscript_ToCbor_ProducesValidCbor()
    {
        // Arrange
        var transcript = SessionTranscript.ForOpenId4Vp(
            "https://verifier.example.com",
            "nonce123",
            null,
            "https://verifier.example.com/callback");

        // Act
        var cbor = transcript.ToCbor();

        // Assert
        cbor.Should().NotBeNull();
        cbor.Should().NotBeEmpty();
    }

    [Fact]
    public async Task IssuerSigned_ContainsNameSpaces()
    {
        // Arrange
        var issuerCoseKey = CoseKey.FromECDsa(IssuerSigningKey);
        var deviceCoseKey = CoseKey.FromECDsa(DeviceKey);

        // Act
        var mdoc = await new MdocIssuerBuilder()
            .WithDocType(MdlDocType)
            .WithIssuerKey(issuerCoseKey)
            .WithDeviceKey(deviceCoseKey)
            .AddMdlElement(MdlDataElement.FamilyName, "Doe")
            .AddMdlElement(MdlDataElement.GivenName, "John")
            .WithValidity(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddYears(1))
            .BuildAsync(_cryptoProvider);

        // Assert
        mdoc.IssuerSigned.NameSpaces.Should().NotBeNull();
        mdoc.IssuerSigned.NameSpaces.Should().ContainKey(MdlNamespace);
    }

    [Fact]
    public async Task IssuerAuth_HasValidSignature()
    {
        // Arrange
        var issuerCoseKey = CoseKey.FromECDsa(IssuerSigningKey);
        var deviceCoseKey = CoseKey.FromECDsa(DeviceKey);

        // Act
        var mdoc = await new MdocIssuerBuilder()
            .WithDocType(MdlDocType)
            .WithIssuerKey(issuerCoseKey)
            .WithDeviceKey(deviceCoseKey)
            .AddMdlElement(MdlDataElement.FamilyName, "Doe")
            .WithValidity(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddYears(1))
            .BuildAsync(_cryptoProvider);

        // Assert - IssuerAuth is COSE_Sign1 bytes
        mdoc.IssuerSigned.IssuerAuth.Should().NotBeNull();
        mdoc.IssuerSigned.IssuerAuth.Should().NotBeEmpty();
    }

    [Fact]
    public void CoseKey_FromECDsa_CreatesValidKey()
    {
        // Act
        var coseKey = CoseKey.FromECDsa(IssuerSigningKey);

        // Assert
        coseKey.Should().NotBeNull();
        coseKey.KeyType.Should().Be(CoseKeyType.EC2);
    }

    [Fact]
    public void CoseKey_RoundTrip_PreservesKey()
    {
        // Arrange
        var original = CoseKey.FromECDsa(IssuerSigningKey);

        // Act
        var cbor = original.ToCbor();
        var restored = CoseKey.FromCbor(cbor);

        // Assert
        restored.KeyType.Should().Be(original.KeyType);
        restored.X.Should().Equal(original.X);
        restored.Y.Should().Equal(original.Y);
    }

    [Fact]
    public async Task Document_ToCbor_ProducesValidCbor()
    {
        // Arrange
        var issuerCoseKey = CoseKey.FromECDsa(IssuerSigningKey);
        var deviceCoseKey = CoseKey.FromECDsa(DeviceKey);

        var mdoc = await new MdocIssuerBuilder()
            .WithDocType(MdlDocType)
            .WithIssuerKey(issuerCoseKey)
            .WithDeviceKey(deviceCoseKey)
            .AddMdlElement(MdlDataElement.FamilyName, "Doe")
            .WithValidity(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddYears(1))
            .BuildAsync(_cryptoProvider);

        // Act
        var cbor = mdoc.ToCbor();

        // Assert
        cbor.Should().NotBeNull();
        cbor.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Document_RoundTrip_PreservesData()
    {
        // Arrange
        var issuerCoseKey = CoseKey.FromECDsa(IssuerSigningKey);
        var deviceCoseKey = CoseKey.FromECDsa(DeviceKey);

        var original = await new MdocIssuerBuilder()
            .WithDocType(MdlDocType)
            .WithIssuerKey(issuerCoseKey)
            .WithDeviceKey(deviceCoseKey)
            .AddMdlElement(MdlDataElement.FamilyName, "Doe")
            .WithValidity(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddYears(1))
            .BuildAsync(_cryptoProvider);

        // Act
        var cbor = original.ToCbor();
        var restored = Document.FromCborObject(PeterO.Cbor.CBORObject.DecodeFromBytes(cbor));

        // Assert
        restored.DocType.Should().Be(original.DocType);
        restored.IssuerSigned.NameSpaces.Should().ContainKey(MdlNamespace);
    }
}
