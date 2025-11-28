using SdJwt.Net.Internal;
using Xunit;

namespace SdJwt.Net.Core.Tests;

public class NetStandardCompatibilityTests
{
    [Fact]
    public void SdJwtUtils_NetStandardCompatibility_WorksCorrectly()
    {
        // Arrange
        var algorithm = "SHA-256";
        var disclosure = "test_disclosure_for_netstandard";

        // Act - This should work on all target frameworks including .NET Standard 2.1
        var digest = SdJwtUtils.CreateDigest(algorithm, disclosure);
        var isApproved = SdJwtUtils.IsApprovedHashAlgorithm(algorithm);
        var salt = SdJwtUtils.GenerateSalt();

        // Assert
        Assert.NotNull(digest);
        Assert.NotEmpty(digest);
        Assert.True(isApproved);
        Assert.NotNull(salt);
        Assert.NotEmpty(salt);
    }

    [Theory]
    [InlineData("SHA-256")]
    [InlineData("SHA-384")]
    [InlineData("SHA-512")]
    public void CreateDigest_AllSupportedAlgorithms_WorkOnAllFrameworks(string algorithm)
    {
        // Arrange
        var disclosure = $"test_disclosure_{algorithm}";

        // Act
        var digest = SdJwtUtils.CreateDigest(algorithm, disclosure);

        // Assert
        Assert.NotNull(digest);
        Assert.NotEmpty(digest);
        
        // Verify consistency
        var digest2 = SdJwtUtils.CreateDigest(algorithm, disclosure);
        Assert.Equal(digest, digest2);
    }
}