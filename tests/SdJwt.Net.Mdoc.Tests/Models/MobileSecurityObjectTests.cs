using FluentAssertions;
using SdJwt.Net.Mdoc.Models;
using Xunit;

namespace SdJwt.Net.Mdoc.Tests.Models;

/// <summary>
/// Tests for Mobile Security Object (MSO) per ISO 18013-5 Section 9.1.2.4.
/// </summary>
public class MobileSecurityObjectTests
{
    [Fact]
    public void Constructor_CreatesDefaultValues()
    {
        // Act
        var mso = new MobileSecurityObject();

        // Assert
        mso.Version.Should().Be("1.0");
        mso.DigestAlgorithm.Should().Be("SHA-256");
        mso.ValueDigests.Should().NotBeNull();
        mso.ValueDigests.Should().BeEmpty();
    }

    [Fact]
    public void Version_SetValue_UpdatesCorrectly()
    {
        // Arrange
        var mso = new MobileSecurityObject();

        // Act
        mso.Version = "1.0";

        // Assert
        mso.Version.Should().Be("1.0");
    }

    [Fact]
    public void DigestAlgorithm_WithSHA256_IsValid()
    {
        // Arrange
        var mso = new MobileSecurityObject { DigestAlgorithm = "SHA-256" };

        // Assert
        mso.DigestAlgorithm.Should().Be("SHA-256");
    }

    [Fact]
    public void DigestAlgorithm_WithSHA384_IsValid()
    {
        // Arrange
        var mso = new MobileSecurityObject { DigestAlgorithm = "SHA-384" };

        // Assert
        mso.DigestAlgorithm.Should().Be("SHA-384");
    }

    [Fact]
    public void DigestAlgorithm_WithSHA512_IsValid()
    {
        // Arrange
        var mso = new MobileSecurityObject { DigestAlgorithm = "SHA-512" };

        // Assert
        mso.DigestAlgorithm.Should().Be("SHA-512");
    }

    [Fact]
    public void DocType_SetValue_UpdatesCorrectly()
    {
        // Arrange
        var mso = new MobileSecurityObject();

        // Act
        mso.DocType = "org.iso.18013.5.1.mDL";

        // Assert
        mso.DocType.Should().Be("org.iso.18013.5.1.mDL");
    }

    [Fact]
    public void ValueDigests_AddNamespace_StoresCorrectly()
    {
        // Arrange
        var mso = new MobileSecurityObject();
        var digests = new DigestIdMapping
        {
            Digests = new Dictionary<int, byte[]>
            {
                [0] = new byte[] { 0x01, 0x02, 0x03 },
                [1] = new byte[] { 0x04, 0x05, 0x06 }
            }
        };

        // Act
        mso.ValueDigests["org.iso.18013.5.1"] = digests;

        // Assert
        mso.ValueDigests.Should().ContainKey("org.iso.18013.5.1");
        mso.ValueDigests["org.iso.18013.5.1"].Digests.Should().HaveCount(2);
    }

    [Fact]
    public void ValidityInfo_SetValues_StoredCorrectly()
    {
        // Arrange
        var mso = new MobileSecurityObject();
        var now = DateTimeOffset.UtcNow;

        // Act
        mso.ValidityInfo = new ValidityInfo
        {
            Signed = now,
            ValidFrom = now,
            ValidUntil = now.AddYears(5)
        };

        // Assert
        mso.ValidityInfo.Signed.Should().Be(now);
        mso.ValidityInfo.ValidFrom.Should().Be(now);
        mso.ValidityInfo.ValidUntil.Should().Be(now.AddYears(5));
    }

    [Fact]
    public void ToCbor_ReturnsValidCborBytes()
    {
        // Arrange
        var mso = new MobileSecurityObject
        {
            Version = "1.0",
            DigestAlgorithm = "SHA-256",
            DocType = "org.iso.18013.5.1.mDL",
            ValidityInfo = new ValidityInfo
            {
                Signed = DateTimeOffset.UtcNow,
                ValidFrom = DateTimeOffset.UtcNow,
                ValidUntil = DateTimeOffset.UtcNow.AddYears(5)
            }
        };

        // Act
        var cborBytes = mso.ToCbor();

        // Assert
        cborBytes.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void FromCbor_WithValidData_RestoresMso()
    {
        // Arrange
        var original = new MobileSecurityObject
        {
            Version = "1.0",
            DigestAlgorithm = "SHA-256",
            DocType = "org.iso.18013.5.1.mDL",
            ValidityInfo = new ValidityInfo
            {
                Signed = new DateTimeOffset(2024, 6, 15, 12, 0, 0, TimeSpan.Zero),
                ValidFrom = new DateTimeOffset(2024, 6, 15, 12, 0, 0, TimeSpan.Zero),
                ValidUntil = new DateTimeOffset(2029, 6, 15, 12, 0, 0, TimeSpan.Zero)
            }
        };

        var cborBytes = original.ToCbor();

        // Act
        var restored = MobileSecurityObject.FromCbor(cborBytes);

        // Assert
        restored.Version.Should().Be(original.Version);
        restored.DigestAlgorithm.Should().Be(original.DigestAlgorithm);
        restored.DocType.Should().Be(original.DocType);
    }
}
