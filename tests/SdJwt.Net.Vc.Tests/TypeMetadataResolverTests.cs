using System.Net;
using System.Security.Cryptography;
using System.Text;
using SdJwt.Net.Vc.Metadata;
using Xunit;

namespace SdJwt.Net.Vc.Tests;

/// <summary>
/// Unit tests for TypeMetadataResolver.
/// </summary>
public class TypeMetadataResolverTests : TestBase
{
    [Fact]
    public void TypeMetadataResolver_ThrowsOnNullHttpClient()
    {
        Assert.Throws<ArgumentNullException>(() => new TypeMetadataResolver(null!));
    }

    [Fact]
    public async Task TypeMetadataResolver_ThrowsOnNullVct()
    {
        using var httpClient = VcMetadataTestHelpers.CreateHttpClient(new Dictionary<string, HttpResponseMessage>());
        var resolver = new TypeMetadataResolver(httpClient);

        await Assert.ThrowsAsync<ArgumentException>(() => resolver.ResolveAsync(null!));
    }

    [Fact]
    public async Task TypeMetadataResolver_ThrowsOnEmptyVct()
    {
        using var httpClient = VcMetadataTestHelpers.CreateHttpClient(new Dictionary<string, HttpResponseMessage>());
        var resolver = new TypeMetadataResolver(httpClient);

        await Assert.ThrowsAsync<ArgumentException>(() => resolver.ResolveAsync(""));
    }

    [Fact]
    public async Task TypeMetadataResolver_ThrowsOnNonHttpsVct()
    {
        using var httpClient = VcMetadataTestHelpers.CreateHttpClient(new Dictionary<string, HttpResponseMessage>());
        var resolver = new TypeMetadataResolver(httpClient);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            resolver.ResolveAsync("http://types.example.com/pid"));
    }

    [Fact]
    public async Task TypeMetadataResolver_ThrowsOnNon200Response()
    {
        var vct = "https://types.example.com/pid";
        using var httpClient = VcMetadataTestHelpers.CreateHttpClient(new Dictionary<string, HttpResponseMessage>
        {
            [vct] = new HttpResponseMessage(HttpStatusCode.NotFound)
        });
        var resolver = new TypeMetadataResolver(httpClient);

        await Assert.ThrowsAsync<InvalidOperationException>(() => resolver.ResolveAsync(vct));
    }

    [Fact]
    public async Task TypeMetadataResolver_ThrowsOnNonJsonContentType()
    {
        var vct = "https://types.example.com/pid";
        using var httpClient = VcMetadataTestHelpers.CreateHttpClient(new Dictionary<string, HttpResponseMessage>
        {
            [vct] = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{}", Encoding.UTF8, "text/plain")
            }
        });
        var resolver = new TypeMetadataResolver(httpClient);

        await Assert.ThrowsAsync<InvalidOperationException>(() => resolver.ResolveAsync(vct));
    }

    [Fact]
    public async Task TypeMetadataResolver_ThrowsOnOversizedResponse()
    {
        var vct = "https://types.example.com/pid";
        var largeContent = new string('x', 600 * 1024);
        using var httpClient = VcMetadataTestHelpers.CreateHttpClient(new Dictionary<string, HttpResponseMessage>
        {
            [vct] = VcMetadataTestHelpers.CreateJsonResponse(largeContent)
        });
        var resolver = new TypeMetadataResolver(httpClient, new TypeMetadataResolverOptions
        {
            MaxResponseBytes = 512 * 1024
        });

        await Assert.ThrowsAsync<InvalidOperationException>(() => resolver.ResolveAsync(vct));
    }

    [Fact]
    public async Task TypeMetadataResolver_ThrowsOnMissingVctProperty()
    {
        var vct = "https://types.example.com/pid";
        using var httpClient = VcMetadataTestHelpers.CreateHttpClient(new Dictionary<string, HttpResponseMessage>
        {
            [vct] = VcMetadataTestHelpers.CreateJsonResponse("""{"name": "Test"}""")
        });
        var resolver = new TypeMetadataResolver(httpClient);

        await Assert.ThrowsAsync<InvalidOperationException>(() => resolver.ResolveAsync(vct));
    }

    [Fact]
    public async Task TypeMetadataResolver_ThrowsOnVctMismatch()
    {
        var vct = "https://types.example.com/pid";
        using var httpClient = VcMetadataTestHelpers.CreateHttpClient(new Dictionary<string, HttpResponseMessage>
        {
            [vct] = VcMetadataTestHelpers.CreateJsonResponse("""{"vct": "https://other.example.com/vct"}""")
        });
        var resolver = new TypeMetadataResolver(httpClient);

        await Assert.ThrowsAsync<InvalidOperationException>(() => resolver.ResolveAsync(vct));
    }

    [Fact]
    public async Task TypeMetadataResolver_ValidatesClaimPathMustBeNonEmpty()
    {
        var vct = "https://types.example.com/pid";
        var metadataJson = $$"""
        {
            "vct": "{{vct}}",
            "claims": [
                { "path": [] }
            ]
        }
        """;
        using var httpClient = VcMetadataTestHelpers.CreateHttpClient(new Dictionary<string, HttpResponseMessage>
        {
            [vct] = VcMetadataTestHelpers.CreateJsonResponse(metadataJson)
        });
        var resolver = new TypeMetadataResolver(httpClient);

        await Assert.ThrowsAsync<InvalidOperationException>(() => resolver.ResolveAsync(vct));
    }

    [Fact]
    public async Task TypeMetadataResolver_ValidatesSelectiveDisclosureValues()
    {
        var vct = "https://types.example.com/pid";
        var metadataJson = $$"""
        {
            "vct": "{{vct}}",
            "claims": [
                { "path": ["age"], "sd": "invalid-value" }
            ]
        }
        """;
        using var httpClient = VcMetadataTestHelpers.CreateHttpClient(new Dictionary<string, HttpResponseMessage>
        {
            [vct] = VcMetadataTestHelpers.CreateJsonResponse(metadataJson)
        });
        var resolver = new TypeMetadataResolver(httpClient);

        await Assert.ThrowsAsync<InvalidOperationException>(() => resolver.ResolveAsync(vct));
    }

    [Fact]
    public async Task TypeMetadataResolver_ResolvesMinimalTypeMetadata()
    {
        var vct = "https://types.example.com/pid";
        var metadataJson = $$"""
        {
            "vct": "{{vct}}",
            "name": "Personal Identity",
            "description": "A credential type for personal identity verification"
        }
        """;
        using var httpClient = VcMetadataTestHelpers.CreateHttpClient(new Dictionary<string, HttpResponseMessage>
        {
            [vct] = VcMetadataTestHelpers.CreateJsonResponse(metadataJson)
        });
        var resolver = new TypeMetadataResolver(httpClient, new TypeMetadataResolverOptions
        {
            ValidateDisplayMetadata = false
        });

        var result = await resolver.ResolveAsync(vct);
        Assert.Equal(vct, result.Metadata.Vct);
        Assert.Equal("Personal Identity", result.Metadata.Name);
    }

    [Fact]
    public async Task TypeMetadataResolver_ValidatesSvgIdPattern()
    {
        var vct = "https://types.example.com/pid";
        var metadataJson = $$"""
        {
            "vct": "{{vct}}",
            "claims": [
                { "path": ["name"], "svg_id": "123invalid" }
            ]
        }
        """;
        using var httpClient = VcMetadataTestHelpers.CreateHttpClient(new Dictionary<string, HttpResponseMessage>
        {
            [vct] = VcMetadataTestHelpers.CreateJsonResponse(metadataJson)
        });
        var resolver = new TypeMetadataResolver(httpClient);

        await Assert.ThrowsAsync<InvalidOperationException>(() => resolver.ResolveAsync(vct));
    }

    [Fact]
    public async Task TypeMetadataResolver_ValidatesSvgIdUniqueness()
    {
        var vct = "https://types.example.com/pid";
        var metadataJson = $$"""
        {
            "vct": "{{vct}}",
            "claims": [
                { "path": ["field1"], "svg_id": "myId" },
                { "path": ["field2"], "svg_id": "myId" }
            ]
        }
        """;
        using var httpClient = VcMetadataTestHelpers.CreateHttpClient(new Dictionary<string, HttpResponseMessage>
        {
            [vct] = VcMetadataTestHelpers.CreateJsonResponse(metadataJson)
        });
        var resolver = new TypeMetadataResolver(httpClient);

        await Assert.ThrowsAsync<InvalidOperationException>(() => resolver.ResolveAsync(vct));
    }

    [Fact]
    public async Task TypeMetadataResolver_ValidatesDisplayLocaleFormat()
    {
        var vct = "https://types.example.com/pid";
        var metadataJson = $$"""
        {
            "vct": "{{vct}}",
            "display": [
                { "locale": "invalid!!!", "name": "Test" }
            ]
        }
        """;
        using var httpClient = VcMetadataTestHelpers.CreateHttpClient(new Dictionary<string, HttpResponseMessage>
        {
            [vct] = VcMetadataTestHelpers.CreateJsonResponse(metadataJson)
        });
        var resolver = new TypeMetadataResolver(httpClient);

        await Assert.ThrowsAsync<InvalidOperationException>(() => resolver.ResolveAsync(vct));
    }

    [Fact]
    public async Task TypeMetadataResolver_ValidatesDisplayLocaleDuplicates()
    {
        var vct = "https://types.example.com/pid";
        var metadataJson = $$"""
        {
            "vct": "{{vct}}",
            "display": [
                { "locale": "en-US", "name": "Test 1" },
                { "locale": "en-US", "name": "Test 2" }
            ]
        }
        """;
        using var httpClient = VcMetadataTestHelpers.CreateHttpClient(new Dictionary<string, HttpResponseMessage>
        {
            [vct] = VcMetadataTestHelpers.CreateJsonResponse(metadataJson)
        });
        var resolver = new TypeMetadataResolver(httpClient);

        await Assert.ThrowsAsync<InvalidOperationException>(() => resolver.ResolveAsync(vct));
    }

    [Fact]
    public async Task TypeMetadataResolver_RequiresNameInTypeDisplay()
    {
        var vct = "https://types.example.com/pid";
        var metadataJson = $$"""
        {
            "vct": "{{vct}}",
            "display": [
                { "locale": "en-US" }
            ]
        }
        """;
        using var httpClient = VcMetadataTestHelpers.CreateHttpClient(new Dictionary<string, HttpResponseMessage>
        {
            [vct] = VcMetadataTestHelpers.CreateJsonResponse(metadataJson)
        });
        var resolver = new TypeMetadataResolver(httpClient);

        await Assert.ThrowsAsync<InvalidOperationException>(() => resolver.ResolveAsync(vct));
    }

    [Fact]
    public async Task TypeMetadataResolver_RequiresLabelInClaimDisplay()
    {
        var vct = "https://types.example.com/pid";
        var metadataJson = $$"""
        {
            "vct": "{{vct}}",
            "claims": [
                {
                    "path": ["name"],
                    "display": [
                        { "locale": "en-US" }
                    ]
                }
            ]
        }
        """;
        using var httpClient = VcMetadataTestHelpers.CreateHttpClient(new Dictionary<string, HttpResponseMessage>
        {
            [vct] = VcMetadataTestHelpers.CreateJsonResponse(metadataJson)
        });
        var resolver = new TypeMetadataResolver(httpClient);

        await Assert.ThrowsAsync<InvalidOperationException>(() => resolver.ResolveAsync(vct));
    }

    [Fact]
    public async Task TypeMetadataResolver_ValidatesTextLengthLimit()
    {
        var vct = "https://types.example.com/pid";
        var longName = new string('a', 600);
        var metadataJson = $$"""
        {
            "vct": "{{vct}}",
            "display": [
                { "locale": "en-US", "name": "{{longName}}" }
            ]
        }
        """;
        using var httpClient = VcMetadataTestHelpers.CreateHttpClient(new Dictionary<string, HttpResponseMessage>
        {
            [vct] = VcMetadataTestHelpers.CreateJsonResponse(metadataJson)
        });
        var resolver = new TypeMetadataResolver(httpClient, new TypeMetadataResolverOptions
        {
            MaxDisplayTextLength = 512
        });

        await Assert.ThrowsAsync<InvalidOperationException>(() => resolver.ResolveAsync(vct));
    }

    [Fact]
    public async Task TypeMetadataResolver_ValidatesColorFormat()
    {
        var vct = "https://types.example.com/pid";
        var metadataJson = $$"""
        {
            "vct": "{{vct}}",
            "display": [
                {
                    "locale": "en-US",
                    "name": "Test",
                    "rendering": {
                        "simple": {
                            "background_color": "red"
                        }
                    }
                }
            ]
        }
        """;
        using var httpClient = VcMetadataTestHelpers.CreateHttpClient(new Dictionary<string, HttpResponseMessage>
        {
            [vct] = VcMetadataTestHelpers.CreateJsonResponse(metadataJson)
        });
        var resolver = new TypeMetadataResolver(httpClient);

        await Assert.ThrowsAsync<InvalidOperationException>(() => resolver.ResolveAsync(vct));
    }

    [Fact]
    public async Task TypeMetadataResolver_AcceptsValidShortHexColor()
    {
        var vct = "https://types.example.com/pid";
        var metadataJson = $$"""
        {
            "vct": "{{vct}}",
            "display": [
                {
                    "locale": "en-US",
                    "name": "Test",
                    "rendering": {
                        "simple": {
                            "background_color": "#FFF"
                        }
                    }
                }
            ]
        }
        """;
        using var httpClient = VcMetadataTestHelpers.CreateHttpClient(new Dictionary<string, HttpResponseMessage>
        {
            [vct] = VcMetadataTestHelpers.CreateJsonResponse(metadataJson)
        });
        var resolver = new TypeMetadataResolver(httpClient);

        var result = await resolver.ResolveAsync(vct);
        Assert.Equal(vct, result.Metadata.Vct);
    }

    [Fact]
    public async Task TypeMetadataResolver_AcceptsValidLongHexColor()
    {
        var vct = "https://types.example.com/pid";
        var metadataJson = $$"""
        {
            "vct": "{{vct}}",
            "display": [
                {
                    "locale": "en-US",
                    "name": "Test",
                    "rendering": {
                        "simple": {
                            "text_color": "#112233"
                        }
                    }
                }
            ]
        }
        """;
        using var httpClient = VcMetadataTestHelpers.CreateHttpClient(new Dictionary<string, HttpResponseMessage>
        {
            [vct] = VcMetadataTestHelpers.CreateJsonResponse(metadataJson)
        });
        var resolver = new TypeMetadataResolver(httpClient);

        var result = await resolver.ResolveAsync(vct);
        Assert.Equal(vct, result.Metadata.Vct);
    }

    [Fact]
    public async Task TypeMetadataResolver_ValidatesExtensionDepthLimit()
    {
        var baseVct = "https://types.example.com/level0";
        var responses = new Dictionary<string, HttpResponseMessage>();

        // Create a chain of 10 extensions
        for (int i = 0; i < 10; i++)
        {
            var currentVct = $"https://types.example.com/level{i}";
            var extendsVct = $"https://types.example.com/level{i + 1}";
            var json = $$"""{"vct": "{{currentVct}}", "extends": "{{extendsVct}}"}""";
            responses[currentVct] = VcMetadataTestHelpers.CreateJsonResponse(json);
        }
        responses["https://types.example.com/level10"] = VcMetadataTestHelpers.CreateJsonResponse("""{"vct": "https://types.example.com/level10"}""");

        using var httpClient = VcMetadataTestHelpers.CreateHttpClient(responses);
        var resolver = new TypeMetadataResolver(httpClient, new TypeMetadataResolverOptions
        {
            MaxExtensionDepth = 5,
            ValidateDisplayMetadata = false
        });

        await Assert.ThrowsAsync<InvalidOperationException>(() => resolver.ResolveAsync(baseVct));
    }

    [Fact]
    public async Task TypeMetadataResolver_ThrowsOnNonHttpsExtends()
    {
        var vct = "https://types.example.com/child";
        var metadataJson = $$"""
        {
            "vct": "{{vct}}",
            "extends": "http://insecure.example.com/parent"
        }
        """;
        using var httpClient = VcMetadataTestHelpers.CreateHttpClient(new Dictionary<string, HttpResponseMessage>
        {
            [vct] = VcMetadataTestHelpers.CreateJsonResponse(metadataJson)
        });
        var resolver = new TypeMetadataResolver(httpClient);

        await Assert.ThrowsAsync<InvalidOperationException>(() => resolver.ResolveAsync(vct));
    }

    [Fact]
    public async Task TypeMetadataResolver_ThrowsOnSelfExtension()
    {
        var vct = "https://types.example.com/self";
        var metadataJson = $$"""{"vct": "{{vct}}", "extends": "{{vct}}"}""";
        using var httpClient = VcMetadataTestHelpers.CreateHttpClient(new Dictionary<string, HttpResponseMessage>
        {
            [vct] = VcMetadataTestHelpers.CreateJsonResponse(metadataJson)
        });
        var resolver = new TypeMetadataResolver(httpClient);

        await Assert.ThrowsAsync<InvalidOperationException>(() => resolver.ResolveAsync(vct));
    }

    [Fact]
    public async Task TypeMetadataResolver_ValidatesExtendsIntegrity()
    {
        var child = "https://types.example.com/child";
        var parent = "https://types.example.com/parent";
        var parentJson = $$"""{"vct": "{{parent}}"}""";
        var correctIntegrity = $"sha-256-{Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(parentJson)))}";

        var childJson = $$"""
        {
            "vct": "{{child}}",
            "extends": "{{parent}}",
            "extends#integrity": "sha-256-AAAAAAAAAAAAAAAAAAAAAA=="
        }
        """;

        using var httpClient = VcMetadataTestHelpers.CreateHttpClient(new Dictionary<string, HttpResponseMessage>
        {
            [child] = VcMetadataTestHelpers.CreateJsonResponse(childJson),
            [parent] = VcMetadataTestHelpers.CreateJsonResponse(parentJson)
        });
        var resolver = new TypeMetadataResolver(httpClient, new TypeMetadataResolverOptions
        {
            ValidateDisplayMetadata = false
        });

        await Assert.ThrowsAsync<InvalidOperationException>(() => resolver.ResolveAsync(child));
    }

    [Fact]
    public async Task TypeMetadataResolver_UsesLocalCache()
    {
        var vct = "urn:local:test:pid";
        var localJson = $$"""{"vct": "{{vct}}"}""";

        var options = new TypeMetadataResolverOptions { ValidateDisplayMetadata = false };
        options.LocalTypeMetadataByVct[vct] = localJson;

        using var httpClient = VcMetadataTestHelpers.CreateHttpClient(new Dictionary<string, HttpResponseMessage>());
        var resolver = new TypeMetadataResolver(httpClient, options);

        var result = await resolver.ResolveAsync(vct);
        Assert.Equal(vct, result.Metadata.Vct);
    }

    [Fact]
    public async Task TypeMetadataResolver_ValidatesSvgOrientationValues()
    {
        var vct = "https://types.example.com/pid";
        var svg = "<svg xmlns='http://www.w3.org/2000/svg'><text>hello</text></svg>";
        var svgBytes = Encoding.UTF8.GetBytes(svg);
        var dataUri = $"data:image/svg+xml;base64,{Convert.ToBase64String(svgBytes)}";
        var metadataJson = $$"""
        {
            "vct": "{{vct}}",
            "display": [
                {
                    "locale": "en-US",
                    "name": "Test",
                    "rendering": {
                        "svg_templates": [
                            {
                                "uri": "{{dataUri}}",
                                "properties": { "orientation": "invalid" }
                            }
                        ]
                    }
                }
            ]
        }
        """;
        using var httpClient = VcMetadataTestHelpers.CreateHttpClient(new Dictionary<string, HttpResponseMessage>
        {
            [vct] = VcMetadataTestHelpers.CreateJsonResponse(metadataJson)
        });
        var resolver = new TypeMetadataResolver(httpClient);

        await Assert.ThrowsAsync<InvalidOperationException>(() => resolver.ResolveAsync(vct));
    }

    [Fact]
    public async Task TypeMetadataResolver_ValidatesSvgColorSchemeValues()
    {
        var vct = "https://types.example.com/pid";
        var svg = "<svg xmlns='http://www.w3.org/2000/svg'><text>hello</text></svg>";
        var svgBytes = Encoding.UTF8.GetBytes(svg);
        var dataUri = $"data:image/svg+xml;base64,{Convert.ToBase64String(svgBytes)}";
        var metadataJson = $$"""
        {
            "vct": "{{vct}}",
            "display": [
                {
                    "locale": "en-US",
                    "name": "Test",
                    "rendering": {
                        "svg_templates": [
                            {
                                "uri": "{{dataUri}}",
                                "properties": { "color_scheme": "invalid" }
                            }
                        ]
                    }
                }
            ]
        }
        """;
        using var httpClient = VcMetadataTestHelpers.CreateHttpClient(new Dictionary<string, HttpResponseMessage>
        {
            [vct] = VcMetadataTestHelpers.CreateJsonResponse(metadataJson)
        });
        var resolver = new TypeMetadataResolver(httpClient);

        await Assert.ThrowsAsync<InvalidOperationException>(() => resolver.ResolveAsync(vct));
    }

    [Fact]
    public async Task TypeMetadataResolver_ValidatesSvgContrastValues()
    {
        var vct = "https://types.example.com/pid";
        var svg = "<svg xmlns='http://www.w3.org/2000/svg'><text>hello</text></svg>";
        var svgBytes = Encoding.UTF8.GetBytes(svg);
        var dataUri = $"data:image/svg+xml;base64,{Convert.ToBase64String(svgBytes)}";
        var metadataJson = $$"""
        {
            "vct": "{{vct}}",
            "display": [
                {
                    "locale": "en-US",
                    "name": "Test",
                    "rendering": {
                        "svg_templates": [
                            {
                                "uri": "{{dataUri}}",
                                "properties": { "contrast": "invalid" }
                            }
                        ]
                    }
                }
            ]
        }
        """;
        using var httpClient = VcMetadataTestHelpers.CreateHttpClient(new Dictionary<string, HttpResponseMessage>
        {
            [vct] = VcMetadataTestHelpers.CreateJsonResponse(metadataJson)
        });
        var resolver = new TypeMetadataResolver(httpClient);

        await Assert.ThrowsAsync<InvalidOperationException>(() => resolver.ResolveAsync(vct));
    }

    [Fact]
    public async Task TypeMetadataResolver_AcceptsValidSvgProperties()
    {
        var vct = "https://types.example.com/pid";
        var svg = "<svg xmlns='http://www.w3.org/2000/svg'><text>hello</text></svg>";
        var svgBytes = Encoding.UTF8.GetBytes(svg);
        var dataUri = $"data:image/svg+xml;base64,{Convert.ToBase64String(svgBytes)}";
        var metadataJson = $$"""
        {
            "vct": "{{vct}}",
            "display": [
                {
                    "locale": "en-US",
                    "name": "Test",
                    "rendering": {
                        "svg_templates": [
                            {
                                "uri": "{{dataUri}}",
                                "properties": {
                                    "orientation": "portrait",
                                    "color_scheme": "dark",
                                    "contrast": "high"
                                }
                            }
                        ]
                    }
                }
            ]
        }
        """;
        using var httpClient = VcMetadataTestHelpers.CreateHttpClient(new Dictionary<string, HttpResponseMessage>
        {
            [vct] = VcMetadataTestHelpers.CreateJsonResponse(metadataJson)
        });
        var resolver = new TypeMetadataResolver(httpClient);

        var result = await resolver.ResolveAsync(vct);
        Assert.Equal(vct, result.Metadata.Vct);
    }

    [Fact]
    public async Task TypeMetadataResolver_RequiresIntegrityForRemoteResources()
    {
        var vct = "https://types.example.com/pid";
        var metadataJson = $$"""
        {
            "vct": "{{vct}}",
            "display": [
                {
                    "locale": "en-US",
                    "name": "Test",
                    "rendering": {
                        "svg_templates": [
                            { "uri": "https://assets.example.com/card.svg" }
                        ]
                    }
                }
            ]
        }
        """;
        using var httpClient = VcMetadataTestHelpers.CreateHttpClient(new Dictionary<string, HttpResponseMessage>
        {
            [vct] = VcMetadataTestHelpers.CreateJsonResponse(metadataJson)
        });
        var resolver = new TypeMetadataResolver(httpClient, new TypeMetadataResolverOptions
        {
            RequireIntegrityForRemoteRenderingResources = true
        });

        await Assert.ThrowsAsync<InvalidOperationException>(() => resolver.ResolveAsync(vct));
    }

    [Fact]
    public async Task TypeMetadataResolver_SkipsDisplayValidationWhenDisabled()
    {
        var vct = "https://types.example.com/pid";
        var metadataJson = $$"""
        {
            "vct": "{{vct}}",
            "display": [
                { "locale": "invalid!@#$", "name": "" }
            ]
        }
        """;
        using var httpClient = VcMetadataTestHelpers.CreateHttpClient(new Dictionary<string, HttpResponseMessage>
        {
            [vct] = VcMetadataTestHelpers.CreateJsonResponse(metadataJson)
        });
        var resolver = new TypeMetadataResolver(httpClient, new TypeMetadataResolverOptions
        {
            ValidateDisplayMetadata = false
        });

        var result = await resolver.ResolveAsync(vct);
        Assert.Equal(vct, result.Metadata.Vct);
    }
}
