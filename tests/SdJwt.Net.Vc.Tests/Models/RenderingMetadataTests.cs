using SdJwt.Net.Vc.Models;
using System.Text.Json;
using Xunit;

namespace SdJwt.Net.Vc.Tests.Models;

public class RenderingMetadataTests
{
    [Fact]
    public void RenderingMetadata_Serialization_RoundTrip()
    {
        var metadata = new RenderingMetadata
        {
            Simple = new SimpleRenderingMetadata
            {
                Logo = new LogoMetadata
                {
                    Uri = "https://example.com/logo.png",
                    AltText = "Example Logo"
                },
                BackgroundColor = "#ffffff",
                TextColor = "#000000"
            },
            SvgTemplates = [
                        new SvgTemplate {
                                        Uri = "https://example.com/template.svg",
                                        Properties = new SvgTemplateProperties {
                                                Orientation = "landscape",
                                                ColorScheme = "light",
                                                Contrast = "high"
                                        }
                                }
                ]
        };

        var json = JsonSerializer.Serialize(metadata);
        var deserialized = JsonSerializer.Deserialize<RenderingMetadata>(json);

        Assert.NotNull(deserialized);

        Assert.NotNull(deserialized.Simple);
        Assert.NotNull(deserialized.Simple.Logo);
        Assert.Equal("https://example.com/logo.png", deserialized.Simple.Logo.Uri);
        Assert.Equal("Example Logo", deserialized.Simple.Logo.AltText);
        Assert.Equal("#ffffff", deserialized.Simple.BackgroundColor);
        Assert.Equal("#000000", deserialized.Simple.TextColor);

        Assert.NotNull(deserialized.SvgTemplates);
        Assert.Single(deserialized.SvgTemplates);
        var template = deserialized.SvgTemplates[0];
        Assert.Equal("https://example.com/template.svg", template.Uri);
        Assert.NotNull(template.Properties);
        Assert.Equal("landscape", template.Properties.Orientation);
        Assert.Equal("light", template.Properties.ColorScheme);
        Assert.Equal("high", template.Properties.Contrast);
    }

    [Fact]
    public void LogoMetadata_Serialization_WithIntegrity()
    {
        var logo = new LogoMetadata
        {
            Uri = "https://example.com/logo.png",
            UriIntegrity = "sha256-abcdef",
            AltText = "Logo"
        };

        var json = JsonSerializer.Serialize(logo);
        var deserialized = JsonSerializer.Deserialize<LogoMetadata>(json);

        Assert.NotNull(deserialized);
        Assert.Equal("sha256-abcdef", deserialized.UriIntegrity);
        Assert.Contains("\"uri#integrity\"", json);
    }

    [Fact]
    public void SvgTemplate_Serialization_WithIntegrity()
    {
        var template = new SvgTemplate
        {
            Uri = "https://example.com/bg.svg",
            UriIntegrity = "sha256-123456"
        };

        var json = JsonSerializer.Serialize(template);
        var deserialized = JsonSerializer.Deserialize<SvgTemplate>(json);

        Assert.NotNull(deserialized);
        Assert.Equal("sha256-123456", deserialized.UriIntegrity);
        Assert.Contains("\"uri#integrity\"", json);
    }
}
