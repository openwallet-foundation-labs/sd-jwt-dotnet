using System.Security.Cryptography;
using System.Text;
using SdJwt.Net.Vc.Metadata;
using Xunit;

namespace SdJwt.Net.Vc.Tests;

/// <summary>
/// Unit tests for IntegrityMetadataValidator.
/// </summary>
public class IntegrityMetadataValidatorTests
{
    [Fact]
    public void IntegrityMetadataValidator_ValidateString_ThrowsOnNullContent()
    {
        Assert.Throws<ArgumentNullException>(() =>
            IntegrityMetadataValidator.Validate((string)null!, "sha-256-test"));
    }

    [Fact]
    public void IntegrityMetadataValidator_ValidateString_ThrowsOnNullIntegrity()
    {
        Assert.Throws<ArgumentException>(() =>
            IntegrityMetadataValidator.Validate("content", null!));
    }

    [Fact]
    public void IntegrityMetadataValidator_ValidateString_ThrowsOnEmptyIntegrity()
    {
        Assert.Throws<ArgumentException>(() =>
            IntegrityMetadataValidator.Validate("content", ""));
    }

    [Fact]
    public void IntegrityMetadataValidator_ValidateString_ThrowsOnWhitespaceIntegrity()
    {
        Assert.Throws<ArgumentException>(() =>
            IntegrityMetadataValidator.Validate("content", "   "));
    }

    [Fact]
    public void IntegrityMetadataValidator_ValidateBytes_ThrowsOnNullContent()
    {
        Assert.Throws<ArgumentNullException>(() =>
            IntegrityMetadataValidator.Validate((byte[])null!, "sha-256-test"));
    }

    [Fact]
    public void IntegrityMetadataValidator_ValidateBytes_ThrowsOnNullIntegrity()
    {
        Assert.Throws<ArgumentException>(() =>
            IntegrityMetadataValidator.Validate(new byte[] { 1, 2, 3 }, null!));
    }

    [Fact]
    public void IntegrityMetadataValidator_ValidatesSha256WithDash()
    {
        const string content = "test content";
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(content));
        var integrity = $"sha-256-{Convert.ToBase64String(hash)}";

        Assert.True(IntegrityMetadataValidator.Validate(content, integrity));
    }

    [Fact]
    public void IntegrityMetadataValidator_ValidatesSha256WithoutDash()
    {
        const string content = "test content";
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(content));
        var integrity = $"sha256-{Convert.ToBase64String(hash)}";

        Assert.True(IntegrityMetadataValidator.Validate(content, integrity));
    }

    [Fact]
    public void IntegrityMetadataValidator_ValidatesSha384WithDash()
    {
        const string content = "test content for sha384";
        var hash = SHA384.HashData(Encoding.UTF8.GetBytes(content));
        var integrity = $"sha-384-{Convert.ToBase64String(hash)}";

        Assert.True(IntegrityMetadataValidator.Validate(content, integrity));
    }

    [Fact]
    public void IntegrityMetadataValidator_ValidatesSha384WithoutDash()
    {
        const string content = "test content for sha384";
        var hash = SHA384.HashData(Encoding.UTF8.GetBytes(content));
        var integrity = $"sha384-{Convert.ToBase64String(hash)}";

        Assert.True(IntegrityMetadataValidator.Validate(content, integrity));
    }

    [Fact]
    public void IntegrityMetadataValidator_ValidatesSha512WithDash()
    {
        const string content = "test content for sha512";
        var hash = SHA512.HashData(Encoding.UTF8.GetBytes(content));
        var integrity = $"sha-512-{Convert.ToBase64String(hash)}";

        Assert.True(IntegrityMetadataValidator.Validate(content, integrity));
    }

    [Fact]
    public void IntegrityMetadataValidator_ValidatesSha512WithoutDash()
    {
        const string content = "test content for sha512";
        var hash = SHA512.HashData(Encoding.UTF8.GetBytes(content));
        var integrity = $"sha512-{Convert.ToBase64String(hash)}";

        Assert.True(IntegrityMetadataValidator.Validate(content, integrity));
    }

    [Fact]
    public void IntegrityMetadataValidator_ReturnsFalseForMismatchedDigest()
    {
        const string content = "test content";
        var integrity = "sha-256-AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=";

        Assert.False(IntegrityMetadataValidator.Validate(content, integrity));
    }

    [Fact]
    public void IntegrityMetadataValidator_ReturnsFalseForInvalidBase64()
    {
        const string content = "test content";
        var integrity = "sha-256-not-valid-base64!!!";

        Assert.False(IntegrityMetadataValidator.Validate(content, integrity));
    }

    [Fact]
    public void IntegrityMetadataValidator_ReturnsFalseForUnknownAlgorithm()
    {
        const string content = "test content";
        var integrity = "md5-AAAAAAAAAAAAAAAAAAAAAA==";

        Assert.False(IntegrityMetadataValidator.Validate(content, integrity));
    }

    [Fact]
    public void IntegrityMetadataValidator_SkipsInvalidTokensAndSucceedsWithValidOne()
    {
        const string content = "test content";
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(content));
        var validIntegrity = $"sha-256-{Convert.ToBase64String(hash)}";
        var multipleIntegrity = $"invalid-token {validIntegrity} another-invalid";

        Assert.True(IntegrityMetadataValidator.Validate(content, multipleIntegrity));
    }

    [Fact]
    public void IntegrityMetadataValidator_ReturnsFalseWhenAllTokensInvalid()
    {
        const string content = "test content";
        var integrity = "invalid-token another-invalid md5-test";

        Assert.False(IntegrityMetadataValidator.Validate(content, integrity));
    }

    [Fact]
    public void IntegrityMetadataValidator_HandlesHashLengthMismatch()
    {
        const string content = "test content";
        // SHA-256 hash but wrong length
        var integrity = "sha-256-AA==";

        Assert.False(IntegrityMetadataValidator.Validate(content, integrity));
    }

    [Fact]
    public void IntegrityMetadataValidator_IsCaseInsensitiveForAlgorithm()
    {
        const string content = "test content";
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(content));
        var integrity = $"SHA-256-{Convert.ToBase64String(hash)}";

        Assert.True(IntegrityMetadataValidator.Validate(content, integrity));
    }

    [Fact]
    public void IntegrityMetadataValidator_ValidatesWithBytes()
    {
        var content = new byte[] { 1, 2, 3, 4, 5 };
        var hash = SHA256.HashData(content);
        var integrity = $"sha-256-{Convert.ToBase64String(hash)}";

        Assert.True(IntegrityMetadataValidator.Validate(content, integrity));
    }
}
