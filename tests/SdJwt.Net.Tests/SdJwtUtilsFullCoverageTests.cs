using SdJwt.Net.Internal;
using Xunit;

namespace SdJwt.Net.Tests;

/// <summary>
/// Tests to achieve 100% coverage for SdJwtUtils
/// </summary>
public class SdJwtUtilsFullCoverageTests
{
    [Fact]
    public void CreateDigest_WithMD5Algorithm_ThrowsNotSupportedException()
    {
        // Arrange
        var encodedDisclosure = "WyJzYWx0IiwibmFtZSIsInZhbHVlIl0";

        // Act & Assert
        var ex = Assert.Throws<NotSupportedException>(() =>
            SdJwtUtils.CreateDigest("MD5", encodedDisclosure));
        Assert.Contains("cryptographically weak", ex.Message);
    }

    [Fact]
    public void CreateDigest_WithSHA1Algorithm_ThrowsNotSupportedException()
    {
        // Arrange
        var encodedDisclosure = "WyJzYWx0IiwibmFtZSIsInZhbHVlIl0";

        // Act & Assert
        var ex = Assert.Throws<NotSupportedException>(() =>
            SdJwtUtils.CreateDigest("SHA-1", encodedDisclosure));
        Assert.Contains("cryptographically weak", ex.Message);
    }

    [Fact]
    public void CreateDigest_WithSHA1Variant_ThrowsNotSupportedException()
    {
        // Arrange
        var encodedDisclosure = "WyJzYWx0IiwibmFtZSIsInZhbHVlIl0";

        // Act & Assert
        var ex = Assert.Throws<NotSupportedException>(() =>
            SdJwtUtils.CreateDigest("SHA1", encodedDisclosure));
        Assert.Contains("cryptographically weak", ex.Message);
    }

    [Fact]
    public void CreateDigest_WithUnsupportedAlgorithm_ThrowsNotSupportedException()
    {
        // Arrange
        var encodedDisclosure = "WyJzYWx0IiwibmFtZSIsInZhbHVlIl0";

        // Act & Assert
        var ex = Assert.Throws<NotSupportedException>(() =>
            SdJwtUtils.CreateDigest("SHA-224", encodedDisclosure));
        Assert.Contains("not approved for SD-JWT use", ex.Message);
    }

    [Fact]
    public void CreateDigest_WithNullAlgorithm_ThrowsArgumentNullException()
    {
        // Arrange
        var encodedDisclosure = "WyJzYWx0IiwibmFtZSIsInZhbHVlIl0";

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            SdJwtUtils.CreateDigest(null!, encodedDisclosure));
    }

    [Fact]
    public void CreateDigest_WithEmptyAlgorithm_ThrowsArgumentException()
    {
        // Arrange
        var encodedDisclosure = "WyJzYWx0IiwibmFtZSIsInZhbHVlIl0";

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            SdJwtUtils.CreateDigest("", encodedDisclosure));
    }

    [Fact]
    public void CreateDigest_WithWhitespaceAlgorithm_ThrowsArgumentException()
    {
        // Arrange
        var encodedDisclosure = "WyJzYWx0IiwibmFtZSIsInZhbHVlIl0";

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            SdJwtUtils.CreateDigest("   ", encodedDisclosure));
    }

    [Fact]
    public void CreateDigest_WithNullDisclosure_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            SdJwtUtils.CreateDigest("SHA-256", null!));
    }

    [Fact]
    public void CreateDigest_WithEmptyDisclosure_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            SdJwtUtils.CreateDigest("SHA-256", ""));
    }

    [Fact]
    public void CreateDigest_WithWhitespaceDisclosure_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            SdJwtUtils.CreateDigest("SHA-256", "   "));
    }

    [Fact]
    public void CreateDigest_WithSHA384_CreatesCorrectDigest()
    {
        // Arrange
        var encodedDisclosure = "WyJzYWx0IiwibmFtZSIsInZhbHVlIl0";

        // Act
        var digest = SdJwtUtils.CreateDigest("SHA-384", encodedDisclosure);

        // Assert
        Assert.NotNull(digest);
        Assert.NotEmpty(digest);
    }

    [Fact]
    public void CreateDigest_WithSHA512_CreatesCorrectDigest()
    {
        // Arrange
        var encodedDisclosure = "WyJzYWx0IiwibmFtZSIsInZhbHVlIl0";

        // Act
        var digest = SdJwtUtils.CreateDigest("SHA-512", encodedDisclosure);

        // Assert
        Assert.NotNull(digest);
        Assert.NotEmpty(digest);
    }

    [Fact]
    public void IsApprovedHashAlgorithm_WithNull_ReturnsFalse()
    {
        Assert.False(SdJwtUtils.IsApprovedHashAlgorithm(null));
    }

    [Fact]
    public void IsApprovedHashAlgorithm_WithEmpty_ReturnsFalse()
    {
        Assert.False(SdJwtUtils.IsApprovedHashAlgorithm(""));
    }

    [Fact]
    public void IsApprovedHashAlgorithm_WithWhitespace_ReturnsFalse()
    {
        Assert.False(SdJwtUtils.IsApprovedHashAlgorithm("   "));
    }

    [Fact]
    public void IsApprovedHashAlgorithm_WithSHA256_ReturnsTrue()
    {
        Assert.True(SdJwtUtils.IsApprovedHashAlgorithm("SHA-256"));
    }

    [Fact]
    public void IsApprovedHashAlgorithm_WithSHA384_ReturnsTrue()
    {
        Assert.True(SdJwtUtils.IsApprovedHashAlgorithm("SHA-384"));
    }

    [Fact]
    public void IsApprovedHashAlgorithm_WithSHA512_ReturnsTrue()
    {
        Assert.True(SdJwtUtils.IsApprovedHashAlgorithm("SHA-512"));
    }

    [Fact]
    public void IsApprovedHashAlgorithm_WithMD5_ReturnsFalse()
    {
        Assert.False(SdJwtUtils.IsApprovedHashAlgorithm("MD5"));
    }

    [Fact]
    public void GenerateSalt_GeneratesUniqueValues()
    {
        // Act
        var salt1 = SdJwtUtils.GenerateSalt();
        var salt2 = SdJwtUtils.GenerateSalt();

        // Assert
        Assert.NotEqual(salt1, salt2);
    }
}
