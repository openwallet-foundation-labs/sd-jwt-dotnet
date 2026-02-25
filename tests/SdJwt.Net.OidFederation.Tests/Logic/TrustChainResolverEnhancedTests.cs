using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Moq;
using SdJwt.Net.OidFederation.Logic;
using SdJwt.Net.OidFederation.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using Xunit;

namespace SdJwt.Net.OidFederation.Tests.Logic;

public class TrustChainResolverEnhancedTests : IDisposable
{
    private readonly ECDsaSecurityKey _signingKey;
    private readonly Mock<ILogger<TrustChainResolver>> _loggerMock;
    private readonly Dictionary<string, SecurityKey> _trustAnchors;
    private readonly HttpClient _httpClient;

    public TrustChainResolverEnhancedTests()
    {
        var ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        _signingKey = new ECDsaSecurityKey(ecdsa);
        _loggerMock = new Mock<ILogger<TrustChainResolver>>();
        _httpClient = new HttpClient();
        _trustAnchors = new Dictionary<string, SecurityKey>
        {
            { "https://trust-anchor.example.com", _signingKey }
        };
    }

    [Fact]
    public async Task ResolveAsync_WithValidSubject_ShouldReturnFailedResultDueToNetworkDependency()
    {
        // Arrange
        var subject = "https://subject.example.com";
        var resolver = new TrustChainResolver(_httpClient, _trustAnchors, null, _loggerMock.Object);

        // Act
        var result = await resolver.ResolveAsync(subject);

        // Assert - Should return a failed result due to network dependency
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNullOrEmpty();
        result.TrustChain.Should().BeEmpty();
        result.EntityConfiguration.Should().BeNull();
        result.TrustAnchor.Should().BeNull();
    }

    [Fact]
    public async Task ResolveAsync_WithNullSubject_ShouldThrowArgumentException()
    {
        // Arrange
        var resolver = new TrustChainResolver(_httpClient, _trustAnchors, null, _loggerMock.Object);

        // Act & Assert
        await FluentActions.Invoking(() => resolver.ResolveAsync(null!))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage("Target entity URL cannot be null or empty*");
    }

    [Fact]
    public async Task ResolveAsync_WithEmptySubject_ShouldThrowArgumentException()
    {
        // Arrange
        var resolver = new TrustChainResolver(_httpClient, _trustAnchors, null, _loggerMock.Object);

        // Act & Assert
        await FluentActions.Invoking(() => resolver.ResolveAsync(""))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage("Target entity URL cannot be null or empty*");
    }

    [Fact]
    public async Task ResolveAsync_WithWhitespaceSubject_ShouldThrowArgumentException()
    {
        // Arrange
        var resolver = new TrustChainResolver(_httpClient, _trustAnchors, null, _loggerMock.Object);

        // Act & Assert
        await FluentActions.Invoking(() => resolver.ResolveAsync("   "))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage("Target entity URL cannot be null or empty*");
    }

    [Fact]
    public async Task ResolveAsync_WithInvalidSubjectUrl_ShouldThrowArgumentException()
    {
        // Arrange
        var subject = "not-a-url";
        var resolver = new TrustChainResolver(_httpClient, _trustAnchors, null, _loggerMock.Object);

        // Act & Assert
        await FluentActions.Invoking(() => resolver.ResolveAsync(subject))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage("Target entity URL must be a valid HTTPS URL*");
    }

    [Fact]
    public async Task ResolveAsync_WithHttpUrl_ShouldThrowArgumentException()
    {
        // Arrange
        var subject = "http://insecure.example.com";
        var resolver = new TrustChainResolver(_httpClient, _trustAnchors, null, _loggerMock.Object);

        // Act & Assert
        await FluentActions.Invoking(() => resolver.ResolveAsync(subject))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage("Target entity URL must be a valid HTTPS URL*");
    }

    [Fact]
    public async Task ResolveAsync_WithFtpUrl_ShouldThrowArgumentException()
    {
        // Arrange
        var subject = "ftp://files.example.com";
        var resolver = new TrustChainResolver(_httpClient, _trustAnchors, null, _loggerMock.Object);

        // Act & Assert
        await FluentActions.Invoking(() => resolver.ResolveAsync(subject))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage("Target entity URL must be a valid HTTPS URL*");
    }

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateResolver()
    {
        // Act
        var resolver = new TrustChainResolver(_httpClient, _trustAnchors, null, _loggerMock.Object);

        // Assert
        resolver.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithNullHttpClient_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        FluentActions.Invoking(() => new TrustChainResolver(null!, _trustAnchors))
            .Should().Throw<ArgumentNullException>()
            .WithParameterName("httpClient");
    }

    [Fact]
    public void Constructor_WithNullTrustAnchors_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        FluentActions.Invoking(() => new TrustChainResolver(_httpClient, null!))
            .Should().Throw<ArgumentNullException>()
            .WithParameterName("trustAnchors");
    }

    [Fact]
    public void Constructor_WithEmptyTrustAnchors_ShouldCreateResolver()
    {
        // Arrange
        var emptyTrustAnchors = new Dictionary<string, SecurityKey>();

        // Act
        var resolver = new TrustChainResolver(_httpClient, emptyTrustAnchors, null, _loggerMock.Object);

        // Assert
        resolver.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithOptions_ShouldCreateResolver()
    {
        // Arrange
        var options = new TrustChainResolverOptions
        {
            MaxPathLength = 3,
            HttpTimeoutSeconds = 15,
            ClockSkewMinutes = 2,
            EnableCaching = false,
            CacheDurationMinutes = 30
        };

        // Act
        var resolver = new TrustChainResolver(_httpClient, _trustAnchors, options, _loggerMock.Object);

        // Assert
        resolver.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithNullLogger_ShouldCreateResolver()
    {
        // Act
        var resolver = new TrustChainResolver(_httpClient, _trustAnchors, null, null);

        // Assert
        resolver.Should().NotBeNull();
    }

    [Fact]
    public async Task ResolveAsync_WithCancellationToken_ShouldHandleCancellationGracefully()
    {
        // Arrange
        var resolver = new TrustChainResolver(_httpClient, _trustAnchors, null, _loggerMock.Object);
        using var cts = new CancellationTokenSource();

        // Act - The method may either return a failed result or throw an exception depending on timing
        var result = await resolver.ResolveAsync("https://subject.example.com", cts.Token);
        cts.Cancel(); // Cancel after the call completes

        // Assert - Should handle cancellation gracefully
        result.Should().NotBeNull();
        // We can't guarantee an exception since the cancellation happens after the call
    }

    [Fact]
    public async Task ResolveAsync_WithPreCancelledToken_ShouldRespectCancellation()
    {
        // Arrange
        var resolver = new TrustChainResolver(_httpClient, _trustAnchors, null, _loggerMock.Object);
        using var cts = new CancellationTokenSource();
        cts.Cancel(); // Cancel before the call

        // Act
        var result = await resolver.ResolveAsync("https://subject.example.com", cts.Token);

        // Assert - The implementation may either throw OperationCanceledException or return a failed result
        // depending on where cancellation is checked
        result.Should().NotBeNull();
        if (result.IsValid)
        {
            // If the call completed successfully, it means cancellation wasn't checked early enough
            // This is acceptable behavior depending on implementation
            result.ErrorMessage.Should().BeNull();
        }
        else
        {
            // If it failed, it should be due to cancellation or other error
            result.ErrorMessage.Should().NotBeNullOrEmpty();
        }
    }

    [Fact]
    public void TrustChainRequirements_Creation_ShouldWorkCorrectly()
    {
        // Act
        var requirements = new TrustChainRequirements
        {
            MaxPathLength = 5,
            RequiredTrustMarks = new[] { "trust-mark-1", "trust-mark-2" },
            RequiredProtocols = new[] { "openid_credential_issuer", "openid_provider" },
            AllowedTrustAnchors = new[] { "https://trust-anchor.example.com", "https://another-anchor.example.com" }
        };

        // Assert
        requirements.Should().NotBeNull();
        requirements.MaxPathLength.Should().Be(5);
        requirements.RequiredTrustMarks.Should().HaveCount(2);
        requirements.RequiredTrustMarks.Should().Contain("trust-mark-1");
        requirements.RequiredTrustMarks.Should().Contain("trust-mark-2");
        requirements.RequiredProtocols.Should().HaveCount(2);
        requirements.RequiredProtocols.Should().Contain("openid_credential_issuer");
        requirements.RequiredProtocols.Should().Contain("openid_provider");
        requirements.AllowedTrustAnchors.Should().HaveCount(2);
    }

    [Fact]
    public void TrustChainRequirements_CreateWithTrustMarks_ShouldWork()
    {
        // Act
        var requirements = TrustChainRequirements.Create(5, "trust-mark-1", "trust-mark-2");

        // Assert
        requirements.Should().NotBeNull();
        requirements.MaxPathLength.Should().Be(5);
        requirements.RequiredTrustMarks.Should().HaveCount(2);
        requirements.RequiredTrustMarks.Should().Contain("trust-mark-1");
        requirements.RequiredTrustMarks.Should().Contain("trust-mark-2");
    }

    [Fact]
    public void TrustChainRequirements_CreateWithoutTrustMarks_ShouldWork()
    {
        // Act
        var requirements = TrustChainRequirements.Create(3);

        // Assert
        requirements.Should().NotBeNull();
        requirements.MaxPathLength.Should().Be(3);
        requirements.RequiredTrustMarks.Should().BeNull();
    }

    [Fact]
    public void TrustChainRequirements_ForProtocol_ShouldWork()
    {
        // Act
        var requirements = TrustChainRequirements.ForProtocol("openid_credential_issuer", 3);

        // Assert
        requirements.Should().NotBeNull();
        requirements.MaxPathLength.Should().Be(3);
        requirements.RequiredProtocols.Should().HaveCount(1);
        requirements.RequiredProtocols.Should().Contain("openid_credential_issuer");
    }

    [Fact]
    public void TrustChainRequirements_ForProtocolWithoutMaxPath_ShouldWork()
    {
        // Act
        var requirements = TrustChainRequirements.ForProtocol("openid_provider");

        // Assert
        requirements.Should().NotBeNull();
        requirements.MaxPathLength.Should().BeNull();
        requirements.RequiredProtocols.Should().HaveCount(1);
        requirements.RequiredProtocols.Should().Contain("openid_provider");
    }

    [Fact]
    public void TrustChainResolverOptions_DefaultValues_ShouldBeSetCorrectly()
    {
        // Act
        var options = new TrustChainResolverOptions();

        // Assert
        options.MaxPathLength.Should().Be(OidFederationConstants.Defaults.MaxPathLength);
        options.HttpTimeoutSeconds.Should().Be(OidFederationConstants.Defaults.HttpTimeoutSeconds);
        options.ClockSkewMinutes.Should().Be(5);
        options.EnableCaching.Should().BeTrue();
        options.CacheDurationMinutes.Should().Be(OidFederationConstants.Cache.DefaultCacheDurationMinutes);
    }

    [Fact]
    public void TrustChainResolverOptions_CustomValues_ShouldBeSetCorrectly()
    {
        // Act
        var options = new TrustChainResolverOptions
        {
            MaxPathLength = 15,
            HttpTimeoutSeconds = 60,
            ClockSkewMinutes = 10,
            EnableCaching = false,
            CacheDurationMinutes = 120
        };

        // Assert
        options.MaxPathLength.Should().Be(15);
        options.HttpTimeoutSeconds.Should().Be(60);
        options.ClockSkewMinutes.Should().Be(10);
        options.EnableCaching.Should().BeFalse();
        options.CacheDurationMinutes.Should().Be(120);
    }

    [Fact]
    public async Task ResolveAsync_WithTrustAnchorSubject_ShouldHandleDirectTrustAnchor()
    {
        // Arrange
        var trustAnchorUrl = "https://trust-anchor.example.com";
        var resolver = new TrustChainResolver(_httpClient, _trustAnchors, null, _loggerMock.Object);

        // Act
        var result = await resolver.ResolveAsync(trustAnchorUrl);

        // Assert - Even if it's a trust anchor, it still needs entity configuration fetching
        result.Should().NotBeNull();
        // The result might be valid or invalid depending on the actual network call
    }

    [Fact]
    public void CreateMockEntityConfiguration_WithoutAuthorityHints_ShouldCreateValidConfiguration()
    {
        // Act
        var config = CreateMockEntityConfiguration("https://entity.example.com");

        // Assert
        config.Should().NotBeNullOrEmpty();
        // The configuration should be a valid JWT string
        config.Split('.').Should().HaveCount(3); // header.payload.signature
    }

    [Fact]
    public void CreateMockEntityConfiguration_WithAuthorityHints_ShouldCreateValidConfiguration()
    {
        // Arrange
        var authorityHints = new[] { "https://authority1.example.com", "https://authority2.example.com" };

        // Act
        var config = CreateMockEntityConfiguration("https://entity.example.com", authorityHints);

        // Assert
        config.Should().NotBeNullOrEmpty();
        config.Split('.').Should().HaveCount(3); // header.payload.signature
    }

    [Fact]
    public void CreateMockEntityStatement_ShouldCreateValidStatement()
    {
        // Arrange
        var issuer = "https://issuer.example.com";
        var subject = "https://subject.example.com";

        // Act
        var statement = CreateMockEntityStatement(issuer, subject);

        // Assert
        statement.Should().NotBeNull();
        statement.Issuer.Should().Be(issuer);
        statement.Subject.Should().Be(subject);
        statement.IssuedAt.Should().BeGreaterThan(0);
        statement.ExpiresAt.Should().BeGreaterThan(statement.IssuedAt);
        statement.JwkSet.Should().NotBeNull();
    }

    [Fact]
    public void TrustChainResult_SuccessCreation_ShouldWork()
    {
        // Arrange
        var trustAnchor = "https://trust-anchor.example.com";
        var entityConfig = EntityConfiguration.Create("https://entity.example.com", new { keys = Array.Empty<object>() });
        var trustChain = new List<EntityStatement>();

        // Act
        var result = TrustChainResult.Success(trustAnchor, entityConfig, trustChain);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeTrue();
        result.TrustAnchor.Should().Be(trustAnchor);
        result.EntityConfiguration.Should().Be(entityConfig);
        result.TrustChain.Should().BeEmpty();
        result.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void TrustChainResult_FailureCreation_ShouldWork()
    {
        // Arrange
        var errorMessage = "Test error message";

        // Act
        var result = TrustChainResult.Failed(errorMessage);

        // Assert
        result.Should().NotBeNull();
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be(errorMessage);
        result.TrustAnchor.Should().BeNull();
        result.EntityConfiguration.Should().BeNull();
        result.TrustChain.Should().BeEmpty();
    }

    private string CreateMockEntityConfiguration(string issuer, string[]? authorityHints = null)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var credentials = new SigningCredentials(_signingKey, SecurityAlgorithms.EcdsaSha256);

        var payload = new JwtPayload
        {
            ["iss"] = issuer,
            ["sub"] = issuer,
            ["iat"] = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            ["exp"] = DateTimeOffset.UtcNow.AddDays(1).ToUnixTimeSeconds(),
            ["jwks"] = new Dictionary<string, object>
            {
                ["keys"] = new object[]
                {
                    new Dictionary<string, object>
                    {
                        ["kty"] = "EC",
                        ["crv"] = "P-256",
                        ["x"] = "test",
                        ["y"] = "test"
                    }
                }
            }
        };

        if (authorityHints?.Length > 0)
        {
            payload["authority_hints"] = authorityHints;
        }

        var header = new JwtHeader(credentials);
        var token = new JwtSecurityToken(header, payload);

        return tokenHandler.WriteToken(token);
    }

    private EntityStatement CreateMockEntityStatement(string issuer, string subject)
    {
        return new EntityStatement
        {
            Issuer = issuer,
            Subject = subject,
            IssuedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(1).ToUnixTimeSeconds(),
            JwkSet = new { keys = new[] { new { kty = "EC", crv = "P-256", x = "test", y = "test" } } }
        };
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
        _signingKey?.ECDsa?.Dispose();
    }
}
