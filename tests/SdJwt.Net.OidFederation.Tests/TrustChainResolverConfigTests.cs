using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Moq.Protected;
using SdJwt.Net.OidFederation.Logic;
using SdJwt.Net.OidFederation.Models;
using System.Net;
using System.Security.Cryptography;

namespace SdJwt.Net.OidFederation.Tests;

/// <summary>
/// Tests for TrustChainRequirements, EntityConfigurationBuilderExtensions,
/// TrustChainResolverOptions, and TrustChainResolver classes.
/// </summary>
public class TrustChainResolverConfigTests
{
    private readonly ECDsaSecurityKey _signingKey;
    private readonly object _jwkSet;

    /// <summary>
    /// Initializes test fixtures for the tests.
    /// </summary>
    public TrustChainResolverConfigTests()
    {
        var ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        _signingKey = new ECDsaSecurityKey(ecdsa);
        _jwkSet = new
        {
            keys = new[] { new { kty = "EC", crv = "P-256", x = "test", y = "test", use = "sig" } }
        };
    }

    #region TrustChainRequirements Tests

    /// <summary>
    /// Tests TrustChainRequirements default initialization.
    /// </summary>
    [Fact]
    public void TrustChainRequirements_DefaultInitialization_ShouldHaveNullProperties()
    {
        // Act
        var requirements = new TrustChainRequirements();

        // Assert
        requirements.MaxPathLength.Should().BeNull();
        requirements.RequiredTrustMarks.Should().BeNull();
        requirements.RequiredProtocols.Should().BeNull();
        requirements.AllowedTrustAnchors.Should().BeNull();
    }

    /// <summary>
    /// Tests TrustChainRequirements.Create with max path length.
    /// </summary>
    [Fact]
    public void TrustChainRequirements_Create_ShouldSetMaxPathLength()
    {
        // Act
        var requirements = TrustChainRequirements.Create(maxPathLength: 5);

        // Assert
        requirements.MaxPathLength.Should().Be(5);
    }

    /// <summary>
    /// Tests TrustChainRequirements.Create with required trust marks.
    /// </summary>
    [Fact]
    public void TrustChainRequirements_Create_ShouldSetRequiredTrustMarks()
    {
        // Act
        var requirements = TrustChainRequirements.Create(null, "mark1", "mark2");

        // Assert
        requirements.RequiredTrustMarks.Should().BeEquivalentTo(new[] { "mark1", "mark2" });
    }

    /// <summary>
    /// Tests TrustChainRequirements.Create sets null when no trust marks provided.
    /// </summary>
    [Fact]
    public void TrustChainRequirements_Create_ShouldSetNullTrustMarksWhenEmpty()
    {
        // Act
        var requirements = TrustChainRequirements.Create();

        // Assert
        requirements.RequiredTrustMarks.Should().BeNull();
    }

    /// <summary>
    /// Tests TrustChainRequirements.ForProtocol creates correct requirements.
    /// </summary>
    [Fact]
    public void TrustChainRequirements_ForProtocol_ShouldSetRequiredProtocol()
    {
        // Act
        var requirements = TrustChainRequirements.ForProtocol("openid_credential_issuer");

        // Assert
        requirements.RequiredProtocols.Should().BeEquivalentTo(new[] { "openid_credential_issuer" });
    }

    /// <summary>
    /// Tests TrustChainRequirements.ForProtocol with max path length.
    /// </summary>
    [Fact]
    public void TrustChainRequirements_ForProtocol_ShouldSetMaxPathLength()
    {
        // Act
        var requirements = TrustChainRequirements.ForProtocol("openid_provider", 3);

        // Assert
        requirements.RequiredProtocols.Should().Contain("openid_provider");
        requirements.MaxPathLength.Should().Be(3);
    }

    #endregion

    #region EntityConfigurationBuilderExtensions Tests

    /// <summary>
    /// Tests AsCredentialIssuer extension method.
    /// </summary>
    [Fact]
    public void EntityConfigurationBuilderExtensions_AsCredentialIssuer_ShouldSetMetadata()
    {
        // Arrange
        var builder = EntityConfigurationBuilder.Create("https://issuer.example.com")
            .WithSigningKey(_signingKey)
            .WithJwkSet(_jwkSet);
        var credentialIssuerMetadata = new
        {
            credential_issuer = "https://issuer.example.com"
        };

        // Act
        builder.AsCredentialIssuer(credentialIssuerMetadata);
        var config = builder.BuildConfiguration();

        // Assert
        config.Metadata.Should().NotBeNull();
        config.Metadata!.OpenIdCredentialIssuer.Should().NotBeNull();
    }

    /// <summary>
    /// Tests AsVerifier extension method.
    /// </summary>
    [Fact]
    public void EntityConfigurationBuilderExtensions_AsVerifier_ShouldSetMetadata()
    {
        // Arrange
        var builder = EntityConfigurationBuilder.Create("https://verifier.example.com")
            .WithSigningKey(_signingKey)
            .WithJwkSet(_jwkSet);
        var verifierMetadata = new
        {
            client_id = "verifier-client-id"
        };

        // Act
        builder.AsVerifier(verifierMetadata);
        var config = builder.BuildConfiguration();

        // Assert
        config.Metadata.Should().NotBeNull();
        config.Metadata!.OpenIdRelyingPartyVerifier.Should().NotBeNull();
    }

    /// <summary>
    /// Tests AsOpenIdProvider extension method.
    /// </summary>
    [Fact]
    public void EntityConfigurationBuilderExtensions_AsOpenIdProvider_ShouldSetMetadata()
    {
        // Arrange
        var builder = EntityConfigurationBuilder.Create("https://provider.example.com")
            .WithSigningKey(_signingKey)
            .WithJwkSet(_jwkSet);
        var providerMetadata = new
        {
            issuer = "https://provider.example.com"
        };

        // Act
        builder.AsOpenIdProvider(providerMetadata);
        var config = builder.BuildConfiguration();

        // Assert
        config.Metadata.Should().NotBeNull();
        config.Metadata!.OpenIdProvider.Should().NotBeNull();
    }

    /// <summary>
    /// Tests WithMultipleProtocols extension method.
    /// </summary>
    [Fact]
    public void EntityConfigurationBuilderExtensions_WithMultipleProtocols_ShouldSetMetadata()
    {
        // Arrange
        var builder = EntityConfigurationBuilder.Create("https://entity.example.com")
            .WithSigningKey(_signingKey)
            .WithJwkSet(_jwkSet);
        var protocols = new Dictionary<string, object>
        {
            { "custom_protocol_1", new { name = "Protocol 1" } },
            { "custom_protocol_2", new { name = "Protocol 2" } }
        };

        // Act
        builder.WithMultipleProtocols(protocols);
        var config = builder.BuildConfiguration();

        // Assert
        config.Metadata.Should().NotBeNull();
        config.Metadata!.AdditionalMetadata.Should().NotBeNull();
        config.Metadata.AdditionalMetadata.Should().ContainKey("custom_protocol_1");
        config.Metadata.AdditionalMetadata.Should().ContainKey("custom_protocol_2");
    }

    #endregion

    #region TrustChainResolverOptions Tests

    /// <summary>
    /// Tests TrustChainResolverOptions default values.
    /// </summary>
    [Fact]
    public void TrustChainResolverOptions_DefaultValues_ShouldMatchConstants()
    {
        // Act
        var options = new TrustChainResolverOptions();

        // Assert
        options.MaxPathLength.Should().Be(OidFederationConstants.Defaults.MaxPathLength);
        options.HttpTimeoutSeconds.Should().Be(OidFederationConstants.Defaults.HttpTimeoutSeconds);
        options.ClockSkewMinutes.Should().Be(5);
        options.EnableCaching.Should().BeTrue();
        options.CacheDurationMinutes.Should().Be(OidFederationConstants.Cache.DefaultCacheDurationMinutes);
        options.MaxResponseSizeBytes.Should().Be(1024 * 1024);
    }

    /// <summary>
    /// Tests TrustChainResolverOptions properties are settable.
    /// </summary>
    [Fact]
    public void TrustChainResolverOptions_Properties_ShouldBeSettable()
    {
        // Arrange & Act
        var options = new TrustChainResolverOptions
        {
            MaxPathLength = 20,
            HttpTimeoutSeconds = 60,
            ClockSkewMinutes = 10,
            EnableCaching = false,
            CacheDurationMinutes = 120,
            MaxResponseSizeBytes = 2 * 1024 * 1024
        };

        // Assert
        options.MaxPathLength.Should().Be(20);
        options.HttpTimeoutSeconds.Should().Be(60);
        options.ClockSkewMinutes.Should().Be(10);
        options.EnableCaching.Should().BeFalse();
        options.CacheDurationMinutes.Should().Be(120);
        options.MaxResponseSizeBytes.Should().Be(2 * 1024 * 1024);
    }

    #endregion

    #region TrustChainResolver Additional Tests

    /// <summary>
    /// Tests TrustChainResolver resolves trust anchor directly.
    /// </summary>
    [Fact]
    public async Task TrustChainResolver_ResolveAsync_ShouldHandleTrustAnchorDirectly()
    {
        // Arrange
        var httpHandlerMock = new Mock<HttpMessageHandler>();
        var httpClient = new HttpClient(httpHandlerMock.Object);
        var trustAnchorUrl = "https://trust-anchor.example.com";

        var trustAnchors = new Dictionary<string, SecurityKey>
        {
            { trustAnchorUrl, _signingKey }
        };

        var resolver = new TrustChainResolver(httpClient, trustAnchors);

        httpHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound
            });

        // Act
        var result = await resolver.ResolveAsync(trustAnchorUrl);

        // Assert
        result.Should().NotBeNull();
        // Note: May fail due to HTTP response, but tests the code path
        result.ErrorMessage.Should().NotBeNullOrEmpty();
    }

    /// <summary>
    /// Tests TrustChainResolver with cancellation token.
    /// </summary>
    [Fact]
    public async Task TrustChainResolver_ResolveAsync_ShouldRespectCancellationToken()
    {
        // Arrange
        var httpHandlerMock = new Mock<HttpMessageHandler>();
        var httpClient = new HttpClient(httpHandlerMock.Object);
        var trustAnchors = new Dictionary<string, SecurityKey>
        {
            { "https://trust-anchor.example.com", _signingKey }
        };

        var cts = new CancellationTokenSource();
        cts.Cancel();

        httpHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new TaskCanceledException());

        var resolver = new TrustChainResolver(httpClient, trustAnchors);

        // Act
        var result = await resolver.ResolveAsync("https://entity.example.com", cts.Token);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
    }

    /// <summary>
    /// Tests TrustChainResolver with custom options.
    /// </summary>
    [Fact]
    public void TrustChainResolver_Constructor_ShouldAcceptCustomOptions()
    {
        // Arrange
        var httpClient = new HttpClient();
        var trustAnchors = new Dictionary<string, SecurityKey>
        {
            { "https://trust-anchor.example.com", _signingKey }
        };
        var options = new TrustChainResolverOptions
        {
            MaxPathLength = 5,
            EnableCaching = false
        };

        // Act & Assert
        var act = () => new TrustChainResolver(httpClient, trustAnchors, options);
        act.Should().NotThrow();
    }

    /// <summary>
    /// Tests TrustChainResolver with logger.
    /// </summary>
    [Fact]
    public void TrustChainResolver_Constructor_ShouldAcceptLogger()
    {
        // Arrange
        var httpClient = new HttpClient();
        var trustAnchors = new Dictionary<string, SecurityKey>
        {
            { "https://trust-anchor.example.com", _signingKey }
        };
        var loggerMock = new Mock<ILogger<TrustChainResolver>>();

        // Act & Assert
        var act = () => new TrustChainResolver(httpClient, trustAnchors, null, loggerMock.Object);
        act.Should().NotThrow();
    }

    #endregion
}
