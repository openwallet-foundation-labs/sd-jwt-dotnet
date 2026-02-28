using FluentAssertions;
using SdJwt.Net.Mdoc.Cose;
using Xunit;

namespace SdJwt.Net.Mdoc.Tests.Cose;

/// <summary>
/// Tests for COSE algorithm identifiers.
/// </summary>
public class CoseAlgorithmTests
{
    [Fact]
    public void ES256_HasCorrectValue()
    {
        // Assert
        ((int)CoseAlgorithm.ES256).Should().Be(-7);
    }

    [Fact]
    public void ES384_HasCorrectValue()
    {
        // Assert
        ((int)CoseAlgorithm.ES384).Should().Be(-35);
    }

    [Fact]
    public void ES512_HasCorrectValue()
    {
        // Assert
        ((int)CoseAlgorithm.ES512).Should().Be(-36);
    }

    [Fact]
    public void EdDSA_HasCorrectValue()
    {
        // Assert
        ((int)CoseAlgorithm.EdDSA).Should().Be(-8);
    }

    [Theory]
    [InlineData(CoseAlgorithm.ES256, "SHA-256")]
    [InlineData(CoseAlgorithm.ES384, "SHA-384")]
    [InlineData(CoseAlgorithm.ES512, "SHA-512")]
    public void GetDigestAlgorithm_ReturnsCorrectValue(CoseAlgorithm algorithm, string expectedDigest)
    {
        // Act
        var result = algorithm.GetDigestAlgorithm();

        // Assert
        result.Should().Be(expectedDigest);
    }

    [Fact]
    public void IsSupportedForHaip_WithES256_ReturnsTrue()
    {
        // Act
        var result = CoseAlgorithm.ES256.IsSupportedForHaip();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsSupportedForHaip_WithES384_ReturnsTrue()
    {
        // Act
        var result = CoseAlgorithm.ES384.IsSupportedForHaip();

        // Assert
        result.Should().BeTrue();
    }
}
