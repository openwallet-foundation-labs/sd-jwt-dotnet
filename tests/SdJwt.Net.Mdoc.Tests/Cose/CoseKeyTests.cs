using System.Security.Cryptography;
using FluentAssertions;
using SdJwt.Net.Mdoc.Cose;
using Xunit;

namespace SdJwt.Net.Mdoc.Tests.Cose;

/// <summary>
/// Tests for COSE_Key representation.
/// </summary>
public class CoseKeyTests : IDisposable
{
    private readonly ECDsa _ecKeyP256;
    private readonly ECDsa _ecKeyP384;

    public CoseKeyTests()
    {
        _ecKeyP256 = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        _ecKeyP384 = ECDsa.Create(ECCurve.NamedCurves.nistP384);
    }

    [Fact]
    public void FromECDsa_WithP256_CreatesValidCoseKey()
    {
        // Act
        var coseKey = CoseKey.FromECDsa(_ecKeyP256);

        // Assert
        coseKey.Should().NotBeNull();
        coseKey.KeyType.Should().Be(CoseKeyType.EC2);
        coseKey.Curve.Should().Be(CoseCurve.P256);
    }

    [Fact]
    public void FromECDsa_WithP384_CreatesValidCoseKey()
    {
        // Act
        var coseKey = CoseKey.FromECDsa(_ecKeyP384);

        // Assert
        coseKey.Should().NotBeNull();
        coseKey.KeyType.Should().Be(CoseKeyType.EC2);
        coseKey.Curve.Should().Be(CoseCurve.P384);
    }

    [Fact]
    public void FromECDsa_WithNull_ThrowsArgumentNullException()
    {
        // Act
        var act = () => CoseKey.FromECDsa(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ToCbor_ReturnsValidCborBytes()
    {
        // Arrange
        var coseKey = CoseKey.FromECDsa(_ecKeyP256);

        // Act
        var cborBytes = coseKey.ToCbor();

        // Assert
        cborBytes.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void FromCbor_WithValidData_CreatesCoseKey()
    {
        // Arrange
        var originalKey = CoseKey.FromECDsa(_ecKeyP256);
        var cborBytes = originalKey.ToCbor();

        // Act
        var restoredKey = CoseKey.FromCbor(cborBytes);

        // Assert
        restoredKey.Should().NotBeNull();
        restoredKey.KeyType.Should().Be(originalKey.KeyType);
        restoredKey.Curve.Should().Be(originalKey.Curve);
    }

    [Fact]
    public void HasPrivateKey_WithPrivateKey_ReturnsTrue()
    {
        // Arrange
        var coseKey = CoseKey.FromECDsa(_ecKeyP256);

        // Act & Assert
        coseKey.HasPrivateKey.Should().BeTrue();
    }

    [Fact]
    public void GetPublicKey_ReturnsOnlyPublicPortion()
    {
        // Arrange
        var coseKey = CoseKey.FromECDsa(_ecKeyP256);

        // Act
        var publicKey = coseKey.GetPublicKey();

        // Assert
        publicKey.HasPrivateKey.Should().BeFalse();
        publicKey.X.Should().BeEquivalentTo(coseKey.X);
        publicKey.Y.Should().BeEquivalentTo(coseKey.Y);
    }

    [Fact]
    public void ToECDsa_ReturnsEquivalentKey()
    {
        // Arrange
        var coseKey = CoseKey.FromECDsa(_ecKeyP256);

        // Act
        using var ecDsa = coseKey.ToECDsa();

        // Assert
        ecDsa.Should().NotBeNull();
        var exportedParams = ecDsa.ExportParameters(false);
        exportedParams.Q.X.Should().BeEquivalentTo(coseKey.X);
        exportedParams.Q.Y.Should().BeEquivalentTo(coseKey.Y);
    }

    public void Dispose()
    {
        _ecKeyP256.Dispose();
        _ecKeyP384.Dispose();
        GC.SuppressFinalize(this);
    }
}
