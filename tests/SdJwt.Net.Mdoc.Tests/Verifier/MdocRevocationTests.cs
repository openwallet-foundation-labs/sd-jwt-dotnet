using FluentAssertions;
using SdJwt.Net.Mdoc.Verifier;
using Xunit;

namespace SdJwt.Net.Mdoc.Tests.Verifier;

/// <summary>
/// Tests for mdoc MSO revocation checking support.
/// </summary>
public class MdocRevocationTests
{
    [Fact]
    public void MdocVerificationOptions_VerifyRevocation_DefaultsFalse()
    {
        // Arrange & Act
        var options = new MdocVerificationOptions();

        // Assert
        options.VerifyRevocation.Should().BeFalse();
        options.RevocationProvider.Should().BeNull();
    }

    [Fact]
    public void MdocVerificationOptions_VerifyRevocation_CanBeEnabled()
    {
        // Arrange
        var provider = new TestRevocationProvider(isRevoked: false);

        // Act
        var options = new MdocVerificationOptions
        {
            VerifyRevocation = true,
            RevocationProvider = provider
        };

        // Assert
        options.VerifyRevocation.Should().BeTrue();
        options.RevocationProvider.Should().NotBeNull();
    }

    [Fact]
    public void MdocVerificationResult_RevocationCheckPassed_DefaultsTrue()
    {
        // Arrange & Act
        var result = new MdocVerificationResult();

        // Assert
        result.RevocationCheckPassed.Should().BeTrue();
    }

    [Fact]
    public void MdocVerificationResult_Success_HasRevocationCheckPassedTrue()
    {
        // Arrange & Act
        var result = MdocVerificationResult.Success();

        // Assert
        result.RevocationCheckPassed.Should().BeTrue();
    }

    [Fact]
    public async Task IMdocRevocationProvider_IsRevokedAsync_ReturnsFalseForValidCredential()
    {
        // Arrange
        var provider = new TestRevocationProvider(isRevoked: false);

        // Act
        var result = await provider.IsRevokedAsync(Array.Empty<byte>(), "org.iso.18013.5.1.mDL");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IMdocRevocationProvider_IsRevokedAsync_ReturnsTrueForRevokedCredential()
    {
        // Arrange
        var provider = new TestRevocationProvider(isRevoked: true);

        // Act
        var result = await provider.IsRevokedAsync(Array.Empty<byte>(), "org.iso.18013.5.1.mDL");

        // Assert
        result.Should().BeTrue();
    }

    private sealed class TestRevocationProvider : IMdocRevocationProvider
    {
        private readonly bool _isRevoked;

        public TestRevocationProvider(bool isRevoked)
        {
            _isRevoked = isRevoked;
        }

        public Task<bool> IsRevokedAsync(byte[] issuerCertificate, string docType, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_isRevoked);
        }
    }
}
