using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.Vc.Metadata;
using Xunit;
using VcModels = SdJwt.Net.Vc.Models;

namespace SdJwt.Net.Vc.Tests;

/// <summary>
/// Unit tests for JwtVcIssuerSigningKeyResolver.
/// </summary>
public class JwtVcIssuerSigningKeyResolverTests : TestBase
{
    [Fact]
    public void JwtVcIssuerSigningKeyResolver_ThrowsOnNullMetadataResolver()
    {
        using var httpClient = new HttpClient();
        Assert.Throws<ArgumentNullException>(() =>
            new JwtVcIssuerSigningKeyResolver(null!, httpClient));
    }

    [Fact]
    public void JwtVcIssuerSigningKeyResolver_ThrowsOnNullHttpClient()
    {
        var mockResolver = new VcMetadataTestHelpers.MockJwtVcIssuerMetadataResolver();
        Assert.Throws<ArgumentNullException>(() =>
            new JwtVcIssuerSigningKeyResolver(mockResolver, null!));
    }

    [Fact]
    public async Task JwtVcIssuerSigningKeyResolver_ThrowsOnNullToken()
    {
        var mockResolver = new VcMetadataTestHelpers.MockJwtVcIssuerMetadataResolver();
        using var httpClient = new HttpClient();
        var keyResolver = new JwtVcIssuerSigningKeyResolver(mockResolver, httpClient);

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            keyResolver.ResolveSigningKeyAsync(null!));
    }

    [Fact]
    public async Task JwtVcIssuerSigningKeyResolver_ThrowsOnMissingIssuerClaim()
    {
        var mockResolver = new VcMetadataTestHelpers.MockJwtVcIssuerMetadataResolver();
        using var httpClient = new HttpClient();
        var keyResolver = new JwtVcIssuerSigningKeyResolver(mockResolver, httpClient);

        var handler = new JwtSecurityTokenHandler();
        var header = new JwtHeader(new SigningCredentials(IssuerSigningKey, IssuerSigningAlgorithm));
        var payload = new JwtPayload();
        var token = new JwtSecurityToken(header, payload);

        await Assert.ThrowsAsync<SecurityTokenException>(() =>
            keyResolver.ResolveSigningKeyAsync(token));
    }

    [Fact]
    public async Task JwtVcIssuerSigningKeyResolver_ResolvesKeyFromInlineJwks()
    {
        var issuer = TrustedIssuer;
        var issuerJwk = JsonWebKeyConverter.ConvertFromSecurityKey(IssuerSigningKey);
        var inlineJwks = new VcModels.JwkSet
        {
            Keys = new[]
            {
                new VcModels.JsonWebKey
                {
                    KeyType = issuerJwk.Kty,
                    KeyId = issuerJwk.KeyId,
                    Curve = issuerJwk.Crv,
                    X = issuerJwk.X,
                    Y = issuerJwk.Y
                }
            }
        };

        var mockResolver = new VcMetadataTestHelpers.MockJwtVcIssuerMetadataResolver
        {
            Result = new JwtVcIssuerMetadataResolutionResult(
                new VcModels.JwtVcIssuerMetadata { Issuer = issuer, Jwks = inlineJwks },
                new Uri($"{issuer}/.well-known/jwt-vc-issuer"),
                "{}")
        };

        using var httpClient = VcMetadataTestHelpers.CreateHttpClient(new Dictionary<string, HttpResponseMessage>());
        var keyResolver = new JwtVcIssuerSigningKeyResolver(mockResolver, httpClient);

        var handler = new JwtSecurityTokenHandler();
        var header = new JwtHeader(new SigningCredentials(IssuerSigningKey, IssuerSigningAlgorithm));
        var payload = new JwtPayload(issuer, null, null, null, null);
        var token = new JwtSecurityToken(header, payload);

        var key = await keyResolver.ResolveSigningKeyAsync(token);

        Assert.NotNull(key);
    }

    [Fact]
    public async Task JwtVcIssuerSigningKeyResolver_ResolvesKeyFromRemoteJwks()
    {
        var issuer = TrustedIssuer;
        var jwksUri = $"{issuer}/jwks.json";
        var issuerJwk = JsonWebKeyConverter.ConvertFromSecurityKey(IssuerSigningKey);
        var jwksJson = JsonSerializer.Serialize(new
        {
            keys = new[] { issuerJwk }
        });

        var mockResolver = new VcMetadataTestHelpers.MockJwtVcIssuerMetadataResolver
        {
            Result = new JwtVcIssuerMetadataResolutionResult(
                new VcModels.JwtVcIssuerMetadata { Issuer = issuer, JwksUri = jwksUri },
                new Uri($"{issuer}/.well-known/jwt-vc-issuer"),
                "{}")
        };

        using var httpClient = VcMetadataTestHelpers.CreateHttpClient(new Dictionary<string, HttpResponseMessage>
        {
            [jwksUri] = VcMetadataTestHelpers.CreateJsonResponse(jwksJson)
        });
        var keyResolver = new JwtVcIssuerSigningKeyResolver(mockResolver, httpClient);

        var handler = new JwtSecurityTokenHandler();
        var header = new JwtHeader(new SigningCredentials(IssuerSigningKey, IssuerSigningAlgorithm));
        var payload = new JwtPayload(issuer, null, null, null, null);
        var token = new JwtSecurityToken(header, payload);

        var key = await keyResolver.ResolveSigningKeyAsync(token);

        Assert.NotNull(key);
    }

    [Fact]
    public async Task JwtVcIssuerSigningKeyResolver_ThrowsOnNonHttpsJwksUri()
    {
        var issuer = TrustedIssuer;
        var mockResolver = new VcMetadataTestHelpers.MockJwtVcIssuerMetadataResolver
        {
            Result = new JwtVcIssuerMetadataResolutionResult(
                new VcModels.JwtVcIssuerMetadata { Issuer = issuer, JwksUri = "http://insecure.com/jwks.json" },
                new Uri($"{issuer}/.well-known/jwt-vc-issuer"),
                "{}")
        };

        using var httpClient = VcMetadataTestHelpers.CreateHttpClient(new Dictionary<string, HttpResponseMessage>());
        var keyResolver = new JwtVcIssuerSigningKeyResolver(mockResolver, httpClient);

        var handler = new JwtSecurityTokenHandler();
        var header = new JwtHeader(new SigningCredentials(IssuerSigningKey, IssuerSigningAlgorithm));
        var payload = new JwtPayload(issuer, null, null, null, null);
        var token = new JwtSecurityToken(header, payload);

        await Assert.ThrowsAsync<SecurityTokenException>(() =>
            keyResolver.ResolveSigningKeyAsync(token));
    }

    [Fact]
    public async Task JwtVcIssuerSigningKeyResolver_ThrowsOnJwksNon200Response()
    {
        var issuer = TrustedIssuer;
        var jwksUri = $"{issuer}/jwks.json";
        var mockResolver = new VcMetadataTestHelpers.MockJwtVcIssuerMetadataResolver
        {
            Result = new JwtVcIssuerMetadataResolutionResult(
                new VcModels.JwtVcIssuerMetadata { Issuer = issuer, JwksUri = jwksUri },
                new Uri($"{issuer}/.well-known/jwt-vc-issuer"),
                "{}")
        };

        using var httpClient = VcMetadataTestHelpers.CreateHttpClient(new Dictionary<string, HttpResponseMessage>
        {
            [jwksUri] = new HttpResponseMessage(HttpStatusCode.InternalServerError)
        });
        var keyResolver = new JwtVcIssuerSigningKeyResolver(mockResolver, httpClient);

        var handler = new JwtSecurityTokenHandler();
        var header = new JwtHeader(new SigningCredentials(IssuerSigningKey, IssuerSigningAlgorithm));
        var payload = new JwtPayload(issuer, null, null, null, null);
        var token = new JwtSecurityToken(header, payload);

        await Assert.ThrowsAsync<SecurityTokenException>(() =>
            keyResolver.ResolveSigningKeyAsync(token));
    }

    [Fact]
    public async Task JwtVcIssuerSigningKeyResolver_ThrowsOnJwksNonJsonContentType()
    {
        var issuer = TrustedIssuer;
        var jwksUri = $"{issuer}/jwks.json";
        var mockResolver = new VcMetadataTestHelpers.MockJwtVcIssuerMetadataResolver
        {
            Result = new JwtVcIssuerMetadataResolutionResult(
                new VcModels.JwtVcIssuerMetadata { Issuer = issuer, JwksUri = jwksUri },
                new Uri($"{issuer}/.well-known/jwt-vc-issuer"),
                "{}")
        };

        using var httpClient = VcMetadataTestHelpers.CreateHttpClient(new Dictionary<string, HttpResponseMessage>
        {
            [jwksUri] = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{}", Encoding.UTF8, "text/plain")
            }
        });
        var keyResolver = new JwtVcIssuerSigningKeyResolver(mockResolver, httpClient);

        var handler = new JwtSecurityTokenHandler();
        var header = new JwtHeader(new SigningCredentials(IssuerSigningKey, IssuerSigningAlgorithm));
        var payload = new JwtPayload(issuer, null, null, null, null);
        var token = new JwtSecurityToken(header, payload);

        await Assert.ThrowsAsync<SecurityTokenException>(() =>
            keyResolver.ResolveSigningKeyAsync(token));
    }

    [Fact]
    public async Task JwtVcIssuerSigningKeyResolver_ThrowsOnOversizedJwks()
    {
        var issuer = TrustedIssuer;
        var jwksUri = $"{issuer}/jwks.json";
        var largeJwks = new string('x', 600 * 1024);
        var mockResolver = new VcMetadataTestHelpers.MockJwtVcIssuerMetadataResolver
        {
            Result = new JwtVcIssuerMetadataResolutionResult(
                new VcModels.JwtVcIssuerMetadata { Issuer = issuer, JwksUri = jwksUri },
                new Uri($"{issuer}/.well-known/jwt-vc-issuer"),
                "{}")
        };

        using var httpClient = VcMetadataTestHelpers.CreateHttpClient(new Dictionary<string, HttpResponseMessage>
        {
            [jwksUri] = VcMetadataTestHelpers.CreateJsonResponse(largeJwks)
        });
        var keyResolver = new JwtVcIssuerSigningKeyResolver(mockResolver, httpClient,
            new JwtVcIssuerSigningKeyResolverOptions { MaxJwksResponseBytes = 512 * 1024 });

        var handler = new JwtSecurityTokenHandler();
        var header = new JwtHeader(new SigningCredentials(IssuerSigningKey, IssuerSigningAlgorithm));
        var payload = new JwtPayload(issuer, null, null, null, null);
        var token = new JwtSecurityToken(header, payload);

        await Assert.ThrowsAsync<SecurityTokenException>(() =>
            keyResolver.ResolveSigningKeyAsync(token));
    }

    [Fact]
    public async Task JwtVcIssuerSigningKeyResolver_ThrowsOnEmptyJwks()
    {
        var issuer = TrustedIssuer;
        var jwksUri = $"{issuer}/jwks.json";
        var mockResolver = new VcMetadataTestHelpers.MockJwtVcIssuerMetadataResolver
        {
            Result = new JwtVcIssuerMetadataResolutionResult(
                new VcModels.JwtVcIssuerMetadata { Issuer = issuer, JwksUri = jwksUri },
                new Uri($"{issuer}/.well-known/jwt-vc-issuer"),
                "{}")
        };

        using var httpClient = VcMetadataTestHelpers.CreateHttpClient(new Dictionary<string, HttpResponseMessage>
        {
            [jwksUri] = VcMetadataTestHelpers.CreateJsonResponse("""{"keys": []}""")
        });
        var keyResolver = new JwtVcIssuerSigningKeyResolver(mockResolver, httpClient);

        var handler = new JwtSecurityTokenHandler();
        var header = new JwtHeader(new SigningCredentials(IssuerSigningKey, IssuerSigningAlgorithm));
        var payload = new JwtPayload(issuer, null, null, null, null);
        var token = new JwtSecurityToken(header, payload);

        await Assert.ThrowsAsync<SecurityTokenException>(() =>
            keyResolver.ResolveSigningKeyAsync(token));
    }

    [Fact]
    public async Task JwtVcIssuerSigningKeyResolver_ThrowsOnNoMatchingKeyByKid()
    {
        var issuer = TrustedIssuer;
        var issuerJwk = JsonWebKeyConverter.ConvertFromSecurityKey(IssuerSigningKey);
        var inlineJwks = new VcModels.JwkSet
        {
            Keys = new[]
            {
                new VcModels.JsonWebKey
                {
                    KeyType = issuerJwk.Kty,
                    KeyId = "different-kid",
                    Curve = issuerJwk.Crv,
                    X = issuerJwk.X,
                    Y = issuerJwk.Y
                }
            }
        };

        var mockResolver = new VcMetadataTestHelpers.MockJwtVcIssuerMetadataResolver
        {
            Result = new JwtVcIssuerMetadataResolutionResult(
                new VcModels.JwtVcIssuerMetadata { Issuer = issuer, Jwks = inlineJwks },
                new Uri($"{issuer}/.well-known/jwt-vc-issuer"),
                "{}")
        };

        using var httpClient = VcMetadataTestHelpers.CreateHttpClient(new Dictionary<string, HttpResponseMessage>());
        var keyResolver = new JwtVcIssuerSigningKeyResolver(mockResolver, httpClient);

        var handler = new JwtSecurityTokenHandler();
        var header = new JwtHeader(new SigningCredentials(IssuerSigningKey, IssuerSigningAlgorithm));
        var payload = new JwtPayload(issuer, null, null, null, null);
        var token = new JwtSecurityToken(header, payload);

        await Assert.ThrowsAsync<SecurityTokenException>(() =>
            keyResolver.ResolveSigningKeyAsync(token));
    }

    [Fact]
    public async Task JwtVcIssuerSigningKeyResolver_ThrowsOnMultipleMatchingKeys()
    {
        var issuer = TrustedIssuer;
        var issuerJwk = JsonWebKeyConverter.ConvertFromSecurityKey(IssuerSigningKey);
        var inlineJwks = new VcModels.JwkSet
        {
            Keys = new[]
            {
                new VcModels.JsonWebKey { KeyType = issuerJwk.Kty, Curve = issuerJwk.Crv, X = issuerJwk.X, Y = issuerJwk.Y },
                new VcModels.JsonWebKey { KeyType = issuerJwk.Kty, Curve = issuerJwk.Crv, X = issuerJwk.X, Y = issuerJwk.Y }
            }
        };

        var mockResolver = new VcMetadataTestHelpers.MockJwtVcIssuerMetadataResolver
        {
            Result = new JwtVcIssuerMetadataResolutionResult(
                new VcModels.JwtVcIssuerMetadata { Issuer = issuer, Jwks = inlineJwks },
                new Uri($"{issuer}/.well-known/jwt-vc-issuer"),
                "{}")
        };

        using var httpClient = VcMetadataTestHelpers.CreateHttpClient(new Dictionary<string, HttpResponseMessage>());
        var keyResolver = new JwtVcIssuerSigningKeyResolver(mockResolver, httpClient);

        // Create token without kid to trigger multiple matches
        var header = new JwtHeader
        {
            { "alg", IssuerSigningAlgorithm },
            { "typ", "dc+sd-jwt" }
        };
        var payload = new JwtPayload(issuer, null, null, null, null);
        var token = new JwtSecurityToken(header, payload);

        await Assert.ThrowsAsync<SecurityTokenException>(() =>
            keyResolver.ResolveSigningKeyAsync(token));
    }

    [Fact]
    public async Task JwtVcIssuerSigningKeyResolver_FiltersKeysByUse()
    {
        var issuer = TrustedIssuer;
        var issuerJwk = JsonWebKeyConverter.ConvertFromSecurityKey(IssuerSigningKey);
        var inlineJwks = new VcModels.JwkSet
        {
            Keys = new[]
            {
                new VcModels.JsonWebKey
                {
                    KeyType = issuerJwk.Kty,
                    KeyId = issuerJwk.KeyId,
                    Curve = issuerJwk.Crv,
                    X = issuerJwk.X,
                    Y = issuerJwk.Y,
                    Use = "enc" // Encryption key, should be filtered out
                }
            }
        };

        var mockResolver = new VcMetadataTestHelpers.MockJwtVcIssuerMetadataResolver
        {
            Result = new JwtVcIssuerMetadataResolutionResult(
                new VcModels.JwtVcIssuerMetadata { Issuer = issuer, Jwks = inlineJwks },
                new Uri($"{issuer}/.well-known/jwt-vc-issuer"),
                "{}")
        };

        using var httpClient = VcMetadataTestHelpers.CreateHttpClient(new Dictionary<string, HttpResponseMessage>());
        var keyResolver = new JwtVcIssuerSigningKeyResolver(mockResolver, httpClient);

        var handler = new JwtSecurityTokenHandler();
        var header = new JwtHeader(new SigningCredentials(IssuerSigningKey, IssuerSigningAlgorithm));
        var payload = new JwtPayload(issuer, null, null, null, null);
        var token = new JwtSecurityToken(header, payload);

        await Assert.ThrowsAsync<SecurityTokenException>(() =>
            keyResolver.ResolveSigningKeyAsync(token));
    }

    [Fact]
    public async Task JwtVcIssuerSigningKeyResolver_FiltersKeysByKeyOps()
    {
        var issuer = TrustedIssuer;
        var issuerJwk = JsonWebKeyConverter.ConvertFromSecurityKey(IssuerSigningKey);
        var inlineJwks = new VcModels.JwkSet
        {
            Keys = new[]
            {
                new VcModels.JsonWebKey
                {
                    KeyType = issuerJwk.Kty,
                    KeyId = issuerJwk.KeyId,
                    Curve = issuerJwk.Crv,
                    X = issuerJwk.X,
                    Y = issuerJwk.Y,
                    KeyOperations = new[] { "encrypt" } // No verify op
                }
            }
        };

        var mockResolver = new VcMetadataTestHelpers.MockJwtVcIssuerMetadataResolver
        {
            Result = new JwtVcIssuerMetadataResolutionResult(
                new VcModels.JwtVcIssuerMetadata { Issuer = issuer, Jwks = inlineJwks },
                new Uri($"{issuer}/.well-known/jwt-vc-issuer"),
                "{}")
        };

        using var httpClient = VcMetadataTestHelpers.CreateHttpClient(new Dictionary<string, HttpResponseMessage>());
        var keyResolver = new JwtVcIssuerSigningKeyResolver(mockResolver, httpClient);

        var handler = new JwtSecurityTokenHandler();
        var header = new JwtHeader(new SigningCredentials(IssuerSigningKey, IssuerSigningAlgorithm));
        var payload = new JwtPayload(issuer, null, null, null, null);
        var token = new JwtSecurityToken(header, payload);

        await Assert.ThrowsAsync<SecurityTokenException>(() =>
            keyResolver.ResolveSigningKeyAsync(token));
    }

    [Fact]
    public async Task JwtVcIssuerSigningKeyResolver_SelectsKeyByExactAlgMatch()
    {
        var issuer = TrustedIssuer;
        var issuerJwk = JsonWebKeyConverter.ConvertFromSecurityKey(IssuerSigningKey);
        var inlineJwks = new VcModels.JwkSet
        {
            Keys = new[]
            {
                new VcModels.JsonWebKey
                {
                    KeyType = issuerJwk.Kty,
                    Curve = issuerJwk.Crv,
                    X = issuerJwk.X,
                    Y = issuerJwk.Y,
                    Algorithm = "ES384" // Wrong algorithm
                },
                new VcModels.JsonWebKey
                {
                    KeyType = issuerJwk.Kty,
                    KeyId = issuerJwk.KeyId,
                    Curve = issuerJwk.Crv,
                    X = issuerJwk.X,
                    Y = issuerJwk.Y,
                    Algorithm = IssuerSigningAlgorithm // Correct algorithm
                }
            }
        };

        var mockResolver = new VcMetadataTestHelpers.MockJwtVcIssuerMetadataResolver
        {
            Result = new JwtVcIssuerMetadataResolutionResult(
                new VcModels.JwtVcIssuerMetadata { Issuer = issuer, Jwks = inlineJwks },
                new Uri($"{issuer}/.well-known/jwt-vc-issuer"),
                "{}")
        };

        using var httpClient = VcMetadataTestHelpers.CreateHttpClient(new Dictionary<string, HttpResponseMessage>());
        var keyResolver = new JwtVcIssuerSigningKeyResolver(mockResolver, httpClient);

        var handler = new JwtSecurityTokenHandler();
        var header = new JwtHeader(new SigningCredentials(IssuerSigningKey, IssuerSigningAlgorithm));
        var payload = new JwtPayload(issuer, null, null, null, null);
        var token = new JwtSecurityToken(header, payload);

        var key = await keyResolver.ResolveSigningKeyAsync(token);

        Assert.NotNull(key);
    }
}
