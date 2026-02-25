using SdJwt.Net.Internal;
using System.Text.Json;
using Xunit;

namespace SdJwt.Net.Tests;

public class SdJwtUtilsFinalTests
{
    [Fact]
    public void ConvertJsonElement_WithUndefinedKind_ThrowsInvalidOperationException()
    {
        // Arrange
        JsonElement undefinedElement = default; // ValueKind is Undefined

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() =>
            SdJwtUtils.ConvertJsonElement(undefinedElement));

        Assert.Contains("Unsupported JsonValueKind", ex.Message);
    }

    [Fact]
    public void IsApprovedHashAlgorithm_WithNullOrEmpty_ReturnsFalse()
    {
        Assert.False(SdJwtUtils.IsApprovedHashAlgorithm(null));
        Assert.False(SdJwtUtils.IsApprovedHashAlgorithm(""));
        Assert.False(SdJwtUtils.IsApprovedHashAlgorithm("   "));
    }

    [Fact]
    public void CreateDigest_WithNullOrEmptyAlgorithm_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentNullException>(() => SdJwtUtils.CreateDigest(null!, "input"));
        Assert.Throws<ArgumentException>(() => SdJwtUtils.CreateDigest("", "input"));
    }

    [Fact]
    public void CreateDigest_WithNullOrEmptyInput_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentNullException>(() => SdJwtUtils.CreateDigest("sha-256", null!));
        Assert.Throws<ArgumentException>(() => SdJwtUtils.CreateDigest("sha-256", ""));
    }
}
