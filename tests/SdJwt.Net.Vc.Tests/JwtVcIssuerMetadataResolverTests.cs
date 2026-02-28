using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;
using SdJwt.Net.Vc.Metadata;
using Xunit;

namespace SdJwt.Net.Vc.Tests;

/// <summary>
/// Unit tests for JwtVcIssuerMetadataResolver.
/// </summary>
public class JwtVcIssuerMetadataResolverTests : TestBase
{
    [Fact]
    public void JwtVcIssuerMetadataResolver_ThrowsOnNullHttpClient()
    {
        Assert.Throws<ArgumentNullException>(() => new JwtVcIssuerMetadataResolver(null!));
    }

    [Fact]
    public async Task JwtVcIssuerMetadataResolver_ThrowsOnNonHttpsIssuer()
    {
        using var httpClient = VcMetadataTestHelpers.CreateHttpClient(new Dictionary<string, HttpResponseMessage>());
        var resolver = new JwtVcIssuerMetadataResolver(httpClient);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            resolver.ResolveAsync("http://issuer.example.com"));
    }

    [Fact]
    public async Task JwtVcIssuerMetadataResolver_ThrowsOnIssuerWithQuery()
    {
        using var httpClient = VcMetadataTestHelpers.CreateHttpClient(new Dictionary<string, HttpResponseMessage>());
        var resolver = new JwtVcIssuerMetadataResolver(httpClient);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            resolver.ResolveAsync("https://issuer.example.com?param=value"));
    }

    [Fact]
    public async Task JwtVcIssuerMetadataResolver_ThrowsOnIssuerWithFragment()
    {
        using var httpClient = VcMetadataTestHelpers.CreateHttpClient(new Dictionary<string, HttpResponseMessage>());
        var resolver = new JwtVcIssuerMetadataResolver(httpClient);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            resolver.ResolveAsync("https://issuer.example.com#fragment"));
    }

    [Fact]
    public async Task JwtVcIssuerMetadataResolver_ThrowsOnLocalhostWhenNotAllowed()
    {
        using var httpClient = VcMetadataTestHelpers.CreateHttpClient(new Dictionary<string, HttpResponseMessage>());
        var resolver = new JwtVcIssuerMetadataResolver(httpClient, new JwtVcIssuerMetadataResolverOptions
        {
            AllowPrivateHosts = false
        });

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            resolver.ResolveAsync("https://localhost"));
    }

    [Fact]
    public async Task JwtVcIssuerMetadataResolver_ThrowsOnPrivateIPv4()
    {
        using var httpClient = VcMetadataTestHelpers.CreateHttpClient(new Dictionary<string, HttpResponseMessage>());
        var resolver = new JwtVcIssuerMetadataResolver(httpClient, new JwtVcIssuerMetadataResolverOptions
        {
            AllowPrivateHosts = false
        });

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            resolver.ResolveAsync("https://192.168.1.1"));
    }

    [Fact]
    public async Task JwtVcIssuerMetadataResolver_ThrowsOn10Network()
    {
        using var httpClient = VcMetadataTestHelpers.CreateHttpClient(new Dictionary<string, HttpResponseMessage>());
        var resolver = new JwtVcIssuerMetadataResolver(httpClient, new JwtVcIssuerMetadataResolverOptions
        {
            AllowPrivateHosts = false
        });

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            resolver.ResolveAsync("https://10.0.0.1"));
    }

    [Fact]
    public async Task JwtVcIssuerMetadataResolver_ThrowsOn172Network()
    {
        using var httpClient = VcMetadataTestHelpers.CreateHttpClient(new Dictionary<string, HttpResponseMessage>());
        var resolver = new JwtVcIssuerMetadataResolver(httpClient, new JwtVcIssuerMetadataResolverOptions
        {
            AllowPrivateHosts = false
        });

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            resolver.ResolveAsync("https://172.16.0.1"));
    }

    [Fact]
    public async Task JwtVcIssuerMetadataResolver_ThrowsOnLoopback127()
    {
        using var httpClient = VcMetadataTestHelpers.CreateHttpClient(new Dictionary<string, HttpResponseMessage>());
        var resolver = new JwtVcIssuerMetadataResolver(httpClient, new JwtVcIssuerMetadataResolverOptions
        {
            AllowPrivateHosts = false
        });

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            resolver.ResolveAsync("https://127.0.0.1"));
    }

    [Fact]
    public async Task JwtVcIssuerMetadataResolver_ThrowsOnNon200Response()
    {
        var issuer = "https://issuer.example.com";
        using var httpClient = VcMetadataTestHelpers.CreateHttpClient(new Dictionary<string, HttpResponseMessage>
        {
            ["https://issuer.example.com/.well-known/jwt-vc-issuer"] = new HttpResponseMessage(HttpStatusCode.NotFound)
        });
        var resolver = new JwtVcIssuerMetadataResolver(httpClient);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            resolver.ResolveAsync(issuer));
    }

    [Fact]
    public async Task JwtVcIssuerMetadataResolver_ThrowsOnNonJsonContentType()
    {
        var issuer = "https://issuer.example.com";
        using var httpClient = VcMetadataTestHelpers.CreateHttpClient(new Dictionary<string, HttpResponseMessage>
        {
            ["https://issuer.example.com/.well-known/jwt-vc-issuer"] = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{}", Encoding.UTF8, "text/html")
            }
        });
        var resolver = new JwtVcIssuerMetadataResolver(httpClient);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            resolver.ResolveAsync(issuer));
    }

    [Fact]
    public async Task JwtVcIssuerMetadataResolver_ThrowsOnOversizedResponse()
    {
        var issuer = "https://issuer.example.com";
        var largeContent = new string('x', 600 * 1024);
        using var httpClient = VcMetadataTestHelpers.CreateHttpClient(new Dictionary<string, HttpResponseMessage>
        {
            ["https://issuer.example.com/.well-known/jwt-vc-issuer"] = VcMetadataTestHelpers.CreateJsonResponse(largeContent)
        });
        var resolver = new JwtVcIssuerMetadataResolver(httpClient, new JwtVcIssuerMetadataResolverOptions
        {
            MaxResponseBytes = 512 * 1024
        });

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            resolver.ResolveAsync(issuer));
    }

    [Fact]
    public async Task JwtVcIssuerMetadataResolver_ThrowsOnMissingIssuerProperty()
    {
        var issuer = "https://issuer.example.com";
        var metadataJson = """{"jwks_uri": "https://issuer.example.com/jwks.json"}""";
        using var httpClient = VcMetadataTestHelpers.CreateHttpClient(new Dictionary<string, HttpResponseMessage>
        {
            ["https://issuer.example.com/.well-known/jwt-vc-issuer"] = VcMetadataTestHelpers.CreateJsonResponse(metadataJson)
        });
        var resolver = new JwtVcIssuerMetadataResolver(httpClient);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            resolver.ResolveAsync(issuer));
    }

    [Fact]
    public async Task JwtVcIssuerMetadataResolver_ThrowsOnIssuerMismatch()
    {
        var issuer = "https://issuer.example.com";
        var metadataJson = """
        {
            "issuer": "https://other.example.com",
            "jwks_uri": "https://issuer.example.com/jwks.json"
        }
        """;
        using var httpClient = VcMetadataTestHelpers.CreateHttpClient(new Dictionary<string, HttpResponseMessage>
        {
            ["https://issuer.example.com/.well-known/jwt-vc-issuer"] = VcMetadataTestHelpers.CreateJsonResponse(metadataJson)
        });
        var resolver = new JwtVcIssuerMetadataResolver(httpClient);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            resolver.ResolveAsync(issuer));
    }

    [Fact]
    public async Task JwtVcIssuerMetadataResolver_ThrowsOnBothJwksAndJwksUri()
    {
        var issuer = "https://issuer.example.com";
        var metadataJson = """
        {
            "issuer": "https://issuer.example.com",
            "jwks_uri": "https://issuer.example.com/jwks.json",
            "jwks": { "keys": [ { "kty": "EC", "crv": "P-256", "x": "x", "y": "y" } ] }
        }
        """;
        using var httpClient = VcMetadataTestHelpers.CreateHttpClient(new Dictionary<string, HttpResponseMessage>
        {
            ["https://issuer.example.com/.well-known/jwt-vc-issuer"] = VcMetadataTestHelpers.CreateJsonResponse(metadataJson)
        });
        var resolver = new JwtVcIssuerMetadataResolver(httpClient);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            resolver.ResolveAsync(issuer));
    }

    [Fact]
    public async Task JwtVcIssuerMetadataResolver_ThrowsOnNeitherJwksNorJwksUri()
    {
        var issuer = "https://issuer.example.com";
        var metadataJson = """{"issuer": "https://issuer.example.com"}""";
        using var httpClient = VcMetadataTestHelpers.CreateHttpClient(new Dictionary<string, HttpResponseMessage>
        {
            ["https://issuer.example.com/.well-known/jwt-vc-issuer"] = VcMetadataTestHelpers.CreateJsonResponse(metadataJson)
        });
        var resolver = new JwtVcIssuerMetadataResolver(httpClient);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            resolver.ResolveAsync(issuer));
    }

    [Fact]
    public async Task JwtVcIssuerMetadataResolver_ThrowsOnNonHttpsJwksUri()
    {
        var issuer = "https://issuer.example.com";
        var metadataJson = """
        {
            "issuer": "https://issuer.example.com",
            "jwks_uri": "http://issuer.example.com/jwks.json"
        }
        """;
        using var httpClient = VcMetadataTestHelpers.CreateHttpClient(new Dictionary<string, HttpResponseMessage>
        {
            ["https://issuer.example.com/.well-known/jwt-vc-issuer"] = VcMetadataTestHelpers.CreateJsonResponse(metadataJson)
        });
        var resolver = new JwtVcIssuerMetadataResolver(httpClient);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            resolver.ResolveAsync(issuer));
    }

    [Fact]
    public async Task JwtVcIssuerMetadataResolver_ThrowsOnEmptyJwks()
    {
        var issuer = "https://issuer.example.com";
        var metadataJson = """
        {
            "issuer": "https://issuer.example.com",
            "jwks": { "keys": [] }
        }
        """;
        using var httpClient = VcMetadataTestHelpers.CreateHttpClient(new Dictionary<string, HttpResponseMessage>
        {
            ["https://issuer.example.com/.well-known/jwt-vc-issuer"] = VcMetadataTestHelpers.CreateJsonResponse(metadataJson)
        });
        var resolver = new JwtVcIssuerMetadataResolver(httpClient);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            resolver.ResolveAsync(issuer));
    }

    [Fact]
    public async Task JwtVcIssuerMetadataResolver_BuildsWellKnownUriWithPath()
    {
        var issuer = "https://issuer.example.com/tenants/test";
        var metadataJson = """
        {
            "issuer": "https://issuer.example.com/tenants/test",
            "jwks": { "keys": [ { "kty": "EC", "crv": "P-256", "x": "test", "y": "test" } ] }
        }
        """;
        using var httpClient = VcMetadataTestHelpers.CreateHttpClient(new Dictionary<string, HttpResponseMessage>
        {
            ["https://issuer.example.com/.well-known/jwt-vc-issuer/tenants/test"] = VcMetadataTestHelpers.CreateJsonResponse(metadataJson)
        });
        var resolver = new JwtVcIssuerMetadataResolver(httpClient);

        var result = await resolver.ResolveAsync(issuer);

        Assert.Equal(issuer, result.Metadata.Issuer);
    }

    [Fact]
    public async Task JwtVcIssuerMetadataResolver_ReturnsValidMetadata()
    {
        var issuer = "https://issuer.example.com";
        var issuerJwk = JsonWebKeyConverter.ConvertFromSecurityKey(IssuerSigningKey);
        var metadataJson = JsonSerializer.Serialize(new
        {
            issuer,
            jwks = new
            {
                keys = new[] { issuerJwk }
            }
        });
        using var httpClient = VcMetadataTestHelpers.CreateHttpClient(new Dictionary<string, HttpResponseMessage>
        {
            ["https://issuer.example.com/.well-known/jwt-vc-issuer"] = VcMetadataTestHelpers.CreateJsonResponse(metadataJson)
        });
        var resolver = new JwtVcIssuerMetadataResolver(httpClient);

        var result = await resolver.ResolveAsync(issuer);

        Assert.Equal(issuer, result.Metadata.Issuer);
        Assert.NotNull(result.Metadata.Jwks);
        Assert.NotEmpty(result.RawJson);
        Assert.Equal(new Uri("https://issuer.example.com/.well-known/jwt-vc-issuer"), result.SourceUri);
    }
}
