using FluentAssertions;
using SdJwt.Net.Wallet.Core;
using SdJwt.Net.Wallet.Status;
using Xunit;

namespace SdJwt.Net.Wallet.Tests.Status;

/// <summary>
/// Unit tests for <see cref="StatusListDocumentStatusResolver"/>.
/// </summary>
public class StatusListDocumentStatusResolverTests
{
    [Fact]
    public void Constructor_WithNullIssuerKeyResolver_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new StatusListDocumentStatusResolver(null!, httpClient: new HttpClient());

        // Assert
        act.Should().Throw<ArgumentNullException>().WithParameterName("issuerKeyResolver");
    }

    [Fact]
    public async Task ResolveStatusAsync_WithNullCredential_ThrowsArgumentNullException()
    {
        // Arrange
        using var resolver = new StatusListDocumentStatusResolver(
            _ => throw new NotSupportedException());

        // Act
        var act = () => resolver.ResolveStatusAsync(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task ResolveStatusAsync_WithEmptyRawCredential_ReturnsValid()
    {
        // Arrange
        using var resolver = new StatusListDocumentStatusResolver(
            _ => throw new NotSupportedException());
        var credential = new StoredCredential
        {
            Id = "cred-1",
            Format = "vc+sd-jwt",
            RawCredential = string.Empty
        };

        // Act
        var result = await resolver.ResolveStatusAsync(credential);

        // Assert
        result.Status.Should().Be(DocumentStatus.Valid);
        result.Reason.Should().Contain("No status claim");
    }

    [Fact]
    public async Task ResolveStatusAsync_WithNoStatusClaim_ReturnsValid()
    {
        // Arrange - create a minimal JWT without a "status" claim
        using var resolver = new StatusListDocumentStatusResolver(
            _ => throw new NotSupportedException());
        var credential = new StoredCredential
        {
            Id = "cred-2",
            Format = "vc+sd-jwt",
            RawCredential = CreateMinimalJwt()
        };

        // Act
        var result = await resolver.ResolveStatusAsync(credential);

        // Assert
        result.Status.Should().Be(DocumentStatus.Valid);
        result.Reason.Should().Contain("No status claim");
    }

    [Fact]
    public async Task ResolveStatusAsync_WithInvalidJwt_ReturnsValid()
    {
        // Arrange
        using var resolver = new StatusListDocumentStatusResolver(
            _ => throw new NotSupportedException());
        var credential = new StoredCredential
        {
            Id = "cred-3",
            Format = "vc+sd-jwt",
            RawCredential = "not-a-jwt"
        };

        // Act
        var result = await resolver.ResolveStatusAsync(credential);

        // Assert
        result.Status.Should().Be(DocumentStatus.Valid);
    }

    [Fact]
    public void Dispose_WithOwnedVerifier_DoesNotThrow()
    {
        // Arrange
        var resolver = new StatusListDocumentStatusResolver(
            _ => throw new NotSupportedException());

        // Act & Assert
        resolver.Invoking(r => r.Dispose()).Should().NotThrow();
    }

    /// <summary>
    /// Creates a minimal unsigned JWT for testing (header.payload.signature).
    /// The payload has no "status" claim.
    /// </summary>
    private static string CreateMinimalJwt()
    {
        var header = Base64UrlEncode("{\"alg\":\"none\",\"typ\":\"JWT\"}");
        var payload = Base64UrlEncode("{\"sub\":\"test\",\"iss\":\"https://issuer.example.com\",\"iat\":1700000000}");
        return $"{header}.{payload}.";
    }

    private static string Base64UrlEncode(string json)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(json);
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}
