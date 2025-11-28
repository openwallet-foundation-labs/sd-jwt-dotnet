using SdJwt.Net.Internal;
using Xunit;

namespace SdJwt.Net.Core.Tests;

public class SdJwtUtilsSecurityTests
{
    [Theory]
    [InlineData("MD5")]
    [InlineData("SHA-1")]
    [InlineData("SHA1")]
    [InlineData("md5")]
    [InlineData("sha-1")]
    [InlineData("sha1")]
    public void CreateDigest_WithWeakAlgorithm_ThrowsNotSupportedException(string weakAlgorithm)
    {
        // Arrange
        var disclosure = "test_disclosure";

        // Act & Assert
        var exception = Assert.Throws<NotSupportedException>(() => 
            SdJwtUtils.CreateDigest(weakAlgorithm, disclosure));
        
        Assert.Contains("cryptographically weak", exception.Message);
    }

    [Theory]
    [InlineData("SHA-256")]
    [InlineData("SHA-384")]
    [InlineData("SHA-512")]
    [InlineData("sha-256")]
    [InlineData("sha-384")]
    [InlineData("sha-512")]
    public void CreateDigest_WithApprovedAlgorithm_Succeeds(string approvedAlgorithm)
    {
        // Arrange
        var disclosure = "test_disclosure";

        // Act
        var digest = SdJwtUtils.CreateDigest(approvedAlgorithm, disclosure);

        // Assert
        Assert.NotNull(digest);
        Assert.NotEmpty(digest);
    }

    [Theory]
    [InlineData("BLAKE2B")]
    [InlineData("RIPEMD160")]
    [InlineData("UNKNOWN_ALGO")]
    [InlineData("SHA3-256")] // SHA3 not currently supported
    public void CreateDigest_WithUnapprovedAlgorithm_ThrowsNotSupportedException(string unapprovedAlgorithm)
    {
        // Arrange
        var disclosure = "test_disclosure";

        // Act & Assert
        var exception = Assert.Throws<NotSupportedException>(() => 
            SdJwtUtils.CreateDigest(unapprovedAlgorithm, disclosure));
        
        Assert.Contains("not approved for SD-JWT use", exception.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateDigest_WithInvalidHashAlgorithm_ThrowsArgumentException(string? invalidAlgorithm)
    {
        // Arrange
        var disclosure = "test_disclosure";

        // Act & Assert
        Assert.ThrowsAny<ArgumentException>(() => 
            SdJwtUtils.CreateDigest(invalidAlgorithm!, disclosure));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateDigest_WithInvalidDisclosure_ThrowsArgumentException(string? invalidDisclosure)
    {
        // Arrange
        var algorithm = "SHA-256";

        // Act & Assert
        Assert.ThrowsAny<ArgumentException>(() => 
            SdJwtUtils.CreateDigest(algorithm, invalidDisclosure!));
    }

    [Theory]
    [InlineData("SHA-256", true)]
    [InlineData("SHA-384", true)]
    [InlineData("SHA-512", true)]
    [InlineData("sha-256", true)]
    [InlineData("MD5", false)]
    [InlineData("SHA-1", false)]
    [InlineData("BLAKE2B", false)]
    [InlineData("SHA3-256", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsApprovedHashAlgorithm_ValidatesCorrectly(string? algorithm, bool expected)
    {
        // Act
        var result = SdJwtUtils.IsApprovedHashAlgorithm(algorithm);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void CreateDigest_ProducesConsistentResults()
    {
        // Arrange
        var disclosure = "test_disclosure_for_consistency";
        var algorithm = "SHA-256";

        // Act
        var digest1 = SdJwtUtils.CreateDigest(algorithm, disclosure);
        var digest2 = SdJwtUtils.CreateDigest(algorithm, disclosure);

        // Assert
        Assert.Equal(digest1, digest2);
    }

    [Fact]
    public void CreateDigest_ProducesDifferentResultsForDifferentInputs()
    {
        // Arrange
        var disclosure1 = "test_disclosure_1";
        var disclosure2 = "test_disclosure_2";
        var algorithm = "SHA-256";

        // Act
        var digest1 = SdJwtUtils.CreateDigest(algorithm, disclosure1);
        var digest2 = SdJwtUtils.CreateDigest(algorithm, disclosure2);

        // Assert
        Assert.NotEqual(digest1, digest2);
    }
}