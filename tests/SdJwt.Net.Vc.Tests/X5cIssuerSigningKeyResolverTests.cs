using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using FluentAssertions;
using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.Vc.Metadata;
using Xunit;

namespace SdJwt.Net.Vc.Tests;

/// <summary>
/// Tests for X5cIssuerSigningKeyResolver.
/// </summary>
public class X5cIssuerSigningKeyResolverTests
{
    [Fact]
    public void Constructor_WithNullOptions_UsesDefaults()
    {
        // Act
        var resolver = new X5cIssuerSigningKeyResolver(null);

        // Assert
        resolver.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithOptions_DoesNotThrow()
    {
        // Arrange
        var options = new X5cIssuerSigningKeyResolverOptions
        {
            ValidateCertificateChain = false,
            CheckRevocation = false
        };

        // Act
        var resolver = new X5cIssuerSigningKeyResolver(options);

        // Assert
        resolver.Should().NotBeNull();
    }

    [Fact]
    public async Task ResolveSigningKeyAsync_WithNullToken_ThrowsArgumentNullException()
    {
        // Arrange
        var resolver = new X5cIssuerSigningKeyResolver();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => resolver.ResolveSigningKeyAsync(null!));
    }

    [Fact]
    public async Task ResolveSigningKeyAsync_WithMissingX5cHeader_ThrowsSecurityTokenException()
    {
        // Arrange
        var resolver = new X5cIssuerSigningKeyResolver();
        var ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var key = new ECDsaSecurityKey(ecdsa);
        var header = new JwtHeader(new SigningCredentials(key, SecurityAlgorithms.EcdsaSha256));
        var payload = new JwtPayload("issuer", "audience", null, null, DateTime.UtcNow.AddHours(1));
        var token = new JwtSecurityToken(header, payload);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<SecurityTokenException>(
            () => resolver.ResolveSigningKeyAsync(token));
        ex.Message.Should().Contain("x5c");
    }

    [Fact]
    public async Task ResolveSigningKeyAsync_WithValidX5cHeader_ReturnsX509SecurityKey()
    {
        // Arrange
        var options = new X5cIssuerSigningKeyResolverOptions
        {
            ValidateCertificateChain = false
        };
        var resolver = new X5cIssuerSigningKeyResolver(options);

        using var ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var request = new CertificateRequest(
            "CN=Test", ecdsa, HashAlgorithmName.SHA256);
        using var cert = request.CreateSelfSigned(
            DateTimeOffset.UtcNow.AddMinutes(-5),
            DateTimeOffset.UtcNow.AddHours(1));

        var base64Cert = Convert.ToBase64String(cert.RawData);

        var header = new JwtHeader();
        header["alg"] = "ES256";
        header["x5c"] = new List<object> { base64Cert };
        var payload = new JwtPayload("issuer", "audience", null, null, DateTime.UtcNow.AddHours(1));
        var token = new JwtSecurityToken(header, payload);

        // Act
        var result = await resolver.ResolveSigningKeyAsync(token);

        // Assert
        result.Should().BeOfType<X509SecurityKey>();
    }

    [Fact]
    public void X5cIssuerSigningKeyResolverOptions_DefaultValues_AreCorrect()
    {
        // Act
        var options = new X5cIssuerSigningKeyResolverOptions();

        // Assert
        options.ValidateCertificateChain.Should().BeTrue();
        options.CheckRevocation.Should().BeTrue();
        options.RevocationMode.Should().Be(X509RevocationMode.Online);
        options.ClockSkew.Should().Be(TimeSpan.FromMinutes(5));
        options.ValidationTime.Should().BeNull();
        options.TrustedRoots.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public async Task ResolveSigningKeyAsync_WithInvalidBase64InX5c_ThrowsSecurityTokenException()
    {
        // Arrange
        var options = new X5cIssuerSigningKeyResolverOptions
        {
            ValidateCertificateChain = false
        };
        var resolver = new X5cIssuerSigningKeyResolver(options);

        var header = new JwtHeader();
        header["alg"] = "ES256";
        header["x5c"] = new List<object> { "not-valid-base64!@#$" };
        var payload = new JwtPayload("issuer", "audience", null, null, DateTime.UtcNow.AddHours(1));
        var token = new JwtSecurityToken(header, payload);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<SecurityTokenException>(
            () => resolver.ResolveSigningKeyAsync(token));
        ex.Message.Should().Contain("Base64");
    }
}
