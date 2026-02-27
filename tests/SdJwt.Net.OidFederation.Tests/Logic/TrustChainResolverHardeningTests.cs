using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Moq.Protected;
using SdJwt.Net.OidFederation.Logic;
using SdJwt.Net.OidFederation.Models;
using System.Net;
using System.Reflection;
using Xunit;

namespace SdJwt.Net.OidFederation.Tests.Logic;

/// <summary>
/// Tests for resolver hardening behaviors (cache/DoS controls and metadata policy processing).
/// </summary>
public class TrustChainResolverHardeningTests
{
    [Fact]
    public async Task ResolveAsync_WithCachingEnabled_ShouldReuseCachedEntityConfiguration()
    {
        // Arrange
        var requestCount = 0;
        var handler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri!.AbsoluteUri.Contains("/.well-known/openid-federation", StringComparison.Ordinal)),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() =>
            {
                requestCount++;
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("invalid-jwt")
                };
            });

        var resolver = CreateResolver(handler, new TrustChainResolverOptions
        {
            EnableCaching = true,
            CacheDurationMinutes = 30
        });

        // Act
        var first = await resolver.ResolveAsync("https://wallet.example.com");
        var second = await resolver.ResolveAsync("https://wallet.example.com");

        // Assert
        first.IsValid.Should().BeFalse();
        second.IsValid.Should().BeFalse();
        requestCount.Should().Be(1);
    }

    [Fact]
    public async Task ResolveAsync_WithOversizedEntityConfiguration_ShouldFailEarly()
    {
        // Arrange
        var requestCount = 0;
        var oversizedContent = new string('x', 256);
        var handler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri!.AbsoluteUri.Contains("/.well-known/openid-federation", StringComparison.Ordinal)),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() =>
            {
                requestCount++;
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(oversizedContent)
                };
            });

        var resolver = CreateResolver(handler, new TrustChainResolverOptions
        {
            MaxResponseSizeBytes = 64
        });

        // Act
        var result = await resolver.ResolveAsync("https://wallet.example.com");

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Failed to fetch entity configuration");
        requestCount.Should().Be(1);
    }

    [Fact]
    public void ApplyMetadataPolicies_WithValueOperator_ShouldMutateMetadata()
    {
        // Arrange
        var resolver = CreateResolver(new Mock<HttpMessageHandler>(MockBehavior.Loose), new TrustChainResolverOptions());
        var baseMetadata = new EntityMetadata
        {
            OpenIdCredentialIssuer = new Dictionary<string, object>(StringComparer.Ordinal)
        };

        var statement = new EntityStatement
        {
            Issuer = "https://ta.example.com",
            MetadataPolicy = new MetadataPolicy
            {
                OpenIdCredentialIssuer = new MetadataPolicyRules
                {
                    FieldPolicies = new Dictionary<string, object>(StringComparer.Ordinal)
                    {
                        ["credential_issuer"] = new Dictionary<string, object>(StringComparer.Ordinal)
                        {
                            [PolicyOperators.Value] = "https://issuer.example.com"
                        }
                    }
                }
            }
        };

        // Act
        var result = InvokeApplyMetadataPolicies(resolver, baseMetadata, new[] { statement });

        // Assert
        var issuerMetadata = result!.OpenIdCredentialIssuer.Should().BeOfType<Dictionary<string, object>>().Subject;
        issuerMetadata.Should().ContainKey("credential_issuer");
        issuerMetadata["credential_issuer"].Should().Be("https://issuer.example.com");
    }

    [Fact]
    public void ApplyMetadataPolicies_WithOneOfViolation_ShouldThrow()
    {
        // Arrange
        var resolver = CreateResolver(new Mock<HttpMessageHandler>(MockBehavior.Loose), new TrustChainResolverOptions());
        var baseMetadata = new EntityMetadata
        {
            OpenIdCredentialIssuer = new Dictionary<string, object>(StringComparer.Ordinal)
            {
                ["credential_signing_alg_values_supported"] = "ES256"
            }
        };

        var statement = new EntityStatement
        {
            Issuer = "https://ta.example.com",
            MetadataPolicy = new MetadataPolicy
            {
                OpenIdCredentialIssuer = new MetadataPolicyRules
                {
                    FieldPolicies = new Dictionary<string, object>(StringComparer.Ordinal)
                    {
                        ["credential_signing_alg_values_supported"] = new Dictionary<string, object>(StringComparer.Ordinal)
                        {
                            [PolicyOperators.OneOf] = new object[] { "PS256", "EdDSA" }
                        }
                    }
                }
            }
        };

        // Act
        var act = () => InvokeApplyMetadataPolicies(resolver, baseMetadata, new[] { statement });

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*one_of check failed*");
    }

    private static TrustChainResolver CreateResolver(Mock<HttpMessageHandler> handler, TrustChainResolverOptions options)
    {
        var httpClient = new HttpClient(handler.Object);
        var trustAnchors = new Dictionary<string, SecurityKey>
        {
            ["https://ta.example.com"] = new SymmetricSecurityKey(new byte[32])
        };

        return new TrustChainResolver(
            httpClient,
            trustAnchors,
            options,
            NullLogger<TrustChainResolver>.Instance);
    }

    private static EntityMetadata? InvokeApplyMetadataPolicies(
        TrustChainResolver resolver,
        EntityMetadata baseMetadata,
        IReadOnlyList<EntityStatement> chain)
    {
        var method = typeof(TrustChainResolver).GetMethod(
            "ApplyMetadataPolicies",
            BindingFlags.NonPublic | BindingFlags.Instance);

        method.Should().NotBeNull("metadata policy application must remain available in resolver internals");

        try
        {
            return method!.Invoke(resolver, new object?[] { baseMetadata, chain, new List<string>() }) as EntityMetadata;
        }
        catch (TargetInvocationException tie) when (tie.InnerException != null)
        {
            throw tie.InnerException;
        }
    }
}
