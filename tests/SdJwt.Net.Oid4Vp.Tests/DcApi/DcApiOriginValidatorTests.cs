using FluentAssertions;
using SdJwt.Net.Oid4Vp.DcApi;
using Xunit;

namespace SdJwt.Net.Oid4Vp.Tests.DcApi;

/// <summary>
/// Tests for DcApiOriginValidator following TDD methodology.
/// </summary>
public class DcApiOriginValidatorTests
{
    private readonly DcApiOriginValidator _validator;

    public DcApiOriginValidatorTests()
    {
        _validator = new DcApiOriginValidator();
    }

    #region ValidateOrigin Tests

    [Fact]
    public void ValidateOrigin_WithMatchingOrigins_ReturnsTrue()
    {
        // Arrange
        var responseOrigin = "https://verifier.example.com";
        var expectedClientId = "https://verifier.example.com";

        // Act
        var result = _validator.ValidateOrigin(responseOrigin, expectedClientId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ValidateOrigin_WithDifferentOrigins_ReturnsFalse()
    {
        // Arrange
        var responseOrigin = "https://malicious.example.com";
        var expectedClientId = "https://verifier.example.com";

        // Act
        var result = _validator.ValidateOrigin(responseOrigin, expectedClientId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ValidateOrigin_WithDifferentPorts_ReturnsFalse()
    {
        // Arrange
        var responseOrigin = "https://verifier.example.com:8443";
        var expectedClientId = "https://verifier.example.com";

        // Act
        var result = _validator.ValidateOrigin(responseOrigin, expectedClientId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ValidateOrigin_WithDifferentSchemes_ReturnsFalse()
    {
        // Arrange
        var responseOrigin = "http://verifier.example.com";
        var expectedClientId = "https://verifier.example.com";

        // Act
        var result = _validator.ValidateOrigin(responseOrigin, expectedClientId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ValidateOrigin_WithPathInClientId_IgnoresPath()
    {
        // Arrange
        var responseOrigin = "https://verifier.example.com";
        var expectedClientId = "https://verifier.example.com/callback";

        // Act
        var result = _validator.ValidateOrigin(responseOrigin, expectedClientId);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData(null, "https://verifier.example.com")]
    [InlineData("https://verifier.example.com", null)]
    [InlineData("", "https://verifier.example.com")]
    [InlineData("https://verifier.example.com", "")]
    public void ValidateOrigin_WithNullOrEmpty_ReturnsFalse(string? responseOrigin, string? expectedClientId)
    {
        // Act
        var result = _validator.ValidateOrigin(responseOrigin!, expectedClientId!);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region ExtractOrigin Tests

    [Fact]
    public void ExtractOrigin_FromFullUrl_ReturnsOriginOnly()
    {
        // Arrange
        var url = "https://verifier.example.com/path/to/endpoint?query=value";

        // Act
        var origin = DcApiOriginValidator.ExtractOrigin(url);

        // Assert
        origin.Should().Be("https://verifier.example.com");
    }

    [Fact]
    public void ExtractOrigin_WithPort_IncludesPort()
    {
        // Arrange
        var url = "https://verifier.example.com:8443/path";

        // Act
        var origin = DcApiOriginValidator.ExtractOrigin(url);

        // Assert
        origin.Should().Be("https://verifier.example.com:8443");
    }

    [Fact]
    public void ExtractOrigin_WithDefaultHttpsPort_ExcludesPort()
    {
        // Arrange
        var url = "https://verifier.example.com:443/path";

        // Act
        var origin = DcApiOriginValidator.ExtractOrigin(url);

        // Assert
        origin.Should().Be("https://verifier.example.com");
    }

    [Fact]
    public void ExtractOrigin_WithDefaultHttpPort_ExcludesPort()
    {
        // Arrange
        var url = "http://verifier.example.com:80/path";

        // Act
        var origin = DcApiOriginValidator.ExtractOrigin(url);

        // Assert
        origin.Should().Be("http://verifier.example.com");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not-a-url")]
    public void ExtractOrigin_WithInvalidUrl_ThrowsArgumentException(string? url)
    {
        // Act
        var act = () => DcApiOriginValidator.ExtractOrigin(url!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    #endregion

    #region Case Sensitivity Tests

    [Fact]
    public void ValidateOrigin_WithDifferentCase_IsCaseInsensitive()
    {
        // Arrange - origins should be case-insensitive per RFC 3986
        var responseOrigin = "https://VERIFIER.EXAMPLE.COM";
        var expectedClientId = "https://verifier.example.com";

        // Act
        var result = _validator.ValidateOrigin(responseOrigin, expectedClientId);

        // Assert
        result.Should().BeTrue();
    }

    #endregion
}
