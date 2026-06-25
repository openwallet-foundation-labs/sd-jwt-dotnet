using System.Security.Cryptography;
using FluentAssertions;
using SdJwt.Net.Mdoc.Cose;
using Xunit;

namespace SdJwt.Net.Mdoc.Tests.Cose;

/// <summary>
/// Tests for COSE_Sign1 structure (RFC 8152).
/// </summary>
public class CoseSign1Tests : IDisposable
{
    private readonly ECDsa _signingKey;
    private readonly ICoseCryptoProvider _cryptoProvider;

    public CoseSign1Tests()
    {
        _signingKey = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        _cryptoProvider = new DefaultCoseCryptoProvider();
    }

    [Fact]
    public async Task Sign_WithValidPayload_ReturnsSignature()
    {
        // Arrange
        var payload = new byte[] { 0x01, 0x02, 0x03 };
        var coseKey = CoseKey.FromECDsa(_signingKey);

        // Act
        var coseSign1 = await CoseSign1.CreateAsync(
            payload,
            coseKey,
            CoseAlgorithm.ES256,
            _cryptoProvider);

        // Assert
        coseSign1.Should().NotBeNull();
        coseSign1.Payload.Should().BeEquivalentTo(payload);
        coseSign1.Signature.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Verify_WithValidSignature_ReturnsTrue()
    {
        // Arrange
        var payload = new byte[] { 0x01, 0x02, 0x03 };
        var coseKey = CoseKey.FromECDsa(_signingKey);

        var coseSign1 = await CoseSign1.CreateAsync(
            payload,
            coseKey,
            CoseAlgorithm.ES256,
            _cryptoProvider);

        // Act
        var result = await coseSign1.VerifyAsync(coseKey.GetPublicKey(), _cryptoProvider);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task Verify_WithWrongKey_ReturnsFalse()
    {
        // Arrange
        var payload = new byte[] { 0x01, 0x02, 0x03 };
        var coseKey = CoseKey.FromECDsa(_signingKey);

        var coseSign1 = await CoseSign1.CreateAsync(
            payload,
            coseKey,
            CoseAlgorithm.ES256,
            _cryptoProvider);

        using var differentKey = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var wrongKey = CoseKey.FromECDsa(differentKey);

        // Act
        var result = await coseSign1.VerifyAsync(wrongKey.GetPublicKey(), _cryptoProvider);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ToCbor_ReturnsValidCborEncoding()
    {
        // Arrange
        var payload = new byte[] { 0x01, 0x02, 0x03 };
        var coseKey = CoseKey.FromECDsa(_signingKey);

        var coseSign1 = await CoseSign1.CreateAsync(
            payload,
            coseKey,
            CoseAlgorithm.ES256,
            _cryptoProvider);

        // Act
        var cborBytes = coseSign1.ToCbor();

        // Assert
        cborBytes.Should().NotBeNullOrEmpty();
        // COSE_Sign1 is tagged with 18
        cborBytes[0].Should().Be(0xD2); // Tag 18 in CBOR
    }

    [Fact]
    public async Task FromCbor_WithValidData_RestoresCoseSign1()
    {
        // Arrange
        var payload = new byte[] { 0x01, 0x02, 0x03 };
        var coseKey = CoseKey.FromECDsa(_signingKey);

        var original = await CoseSign1.CreateAsync(
            payload,
            coseKey,
            CoseAlgorithm.ES256,
            _cryptoProvider);

        var cborBytes = original.ToCbor();

        // Act
        var restored = CoseSign1.FromCbor(cborBytes);

        // Assert
        restored.Should().NotBeNull();
        restored.Payload.Should().BeEquivalentTo(original.Payload);
        restored.Signature.Should().BeEquivalentTo(original.Signature);
    }

    [Fact]
    public async Task Create_WithExternalAad_IncludesInSignature()
    {
        // Arrange
        var payload = new byte[] { 0x01, 0x02, 0x03 };
        var externalAad = new byte[] { 0xAA, 0xBB, 0xCC };
        var coseKey = CoseKey.FromECDsa(_signingKey);

        // Act
        var coseSign1 = await CoseSign1.CreateAsync(
            payload,
            coseKey,
            CoseAlgorithm.ES256,
            _cryptoProvider,
            externalAad: externalAad);

        // Assert - Signature should verify only with same AAD
        var result = await coseSign1.VerifyAsync(coseKey.GetPublicKey(), _cryptoProvider, externalAad);
        result.Should().BeTrue();

        // Different AAD should fail verification
        var resultWithWrongAad = await coseSign1.VerifyAsync(
            coseKey.GetPublicKey(),
            _cryptoProvider,
            new byte[] { 0x11, 0x22 });
        resultWithWrongAad.Should().BeFalse();
    }

    [Fact]
    public void Create_WithNullPayload_ThrowsArgumentNullException()
    {
        // Act
        var act = async () => await CoseSign1.CreateAsync(
            null!,
            CoseKey.FromECDsa(_signingKey),
            CoseAlgorithm.ES256,
            _cryptoProvider);

        // Assert
        act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public void Create_WithNullKey_ThrowsArgumentNullException()
    {
        // Act
        var act = async () => await CoseSign1.CreateAsync(
            new byte[] { 0x01 },
            null!,
            CoseAlgorithm.ES256,
            _cryptoProvider);

        // Assert
        act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public void FromCbor_WithX5ChainAsCborArray_ParsesAsByteArrayArray()
    {
        // Arrange: use Sign() to produce COSE_Sign1 bytes where x5chain is a CBOR array of bstrs
        var certA = new byte[] { 0x30, 0x01, 0x02 };
        var certB = new byte[] { 0x30, 0x04, 0x05 };

        var coseSign1 = new CoseSign1(CoseAlgorithm.ES256, new byte[] { 0xAA });
        coseSign1.UnprotectedHeaders["x5chain"] = new byte[][] { certA, certB };

        var rawBytes = _cryptoProvider.Sign(coseSign1, _signingKey);

        // Act
        var parsed = CoseSign1.FromCbor(rawBytes);

        // Assert
        var chain = parsed.UnprotectedHeaders["x5chain"].Should().BeOfType<byte[][]>().Subject;
        chain.Should().HaveCount(2);
        chain[0].Should().BeEquivalentTo(certA);
        chain[1].Should().BeEquivalentTo(certB);
    }

    [Fact]
    public async Task ToCbor_WithX5ChainArray_SerializesAsCborArray()
    {
        // Arrange: set byte[][] on a created CoseSign1, serialize via ToCbor(), parse back
        var certA = new byte[] { 0x30, 0x01, 0x02 };
        var certB = new byte[] { 0x30, 0x04, 0x05 };

        var coseKey = CoseKey.FromECDsa(_signingKey);
        var coseSign1 = await CoseSign1.CreateAsync(
            new byte[] { 0xAA }, coseKey, CoseAlgorithm.ES256, _cryptoProvider);
        coseSign1.UnprotectedHeaders["x5chain"] = new byte[][] { certA, certB };

        // Act
        var cborBytes = coseSign1.ToCbor();
        var parsed = CoseSign1.FromCbor(cborBytes);

        // Assert
        var chain = parsed.UnprotectedHeaders["x5chain"].Should().BeOfType<byte[][]>().Subject;
        chain.Should().HaveCount(2);
        chain[0].Should().BeEquivalentTo(certA);
        chain[1].Should().BeEquivalentTo(certB);
    }

    public void Dispose()
    {
        _signingKey.Dispose();
        GC.SuppressFinalize(this);
    }
}
