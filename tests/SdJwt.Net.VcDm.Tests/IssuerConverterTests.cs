using FluentAssertions;
using SdJwt.Net.VcDm.Models;
using SdJwt.Net.VcDm.Serialization;
using System.Text.Json;
using Xunit;

namespace SdJwt.Net.VcDm.Tests;

public class IssuerConverterTests
{
    [Fact]
    public void Serialize_SimpleUrl_WritesString()
    {
        var issuer = new Issuer("https://example.com");
        var json = JsonSerializer.Serialize(issuer, VcDmSerializerOptions.Default);
        json.Should().Be("\"https://example.com\"");
    }

    [Fact]
    public void Serialize_IssuerWithName_WritesObject()
    {
        var issuer = new Issuer("https://example.com") { Name = "Example Corp" };
        var json = JsonSerializer.Serialize(issuer, VcDmSerializerOptions.Default);
        json.Should().Contain("\"id\":\"https://example.com\"");
        json.Should().Contain("\"name\":\"Example Corp\"");
    }

    [Fact]
    public void Deserialize_StringIssuer_IsSimpleUrl()
    {
        var json = "\"https://example.com\"";
        var issuer = JsonSerializer.Deserialize<Issuer>(json, VcDmSerializerOptions.Default);
        issuer!.Id.Should().Be("https://example.com");
        issuer.IsSimpleUrl.Should().BeTrue();
    }

    [Fact]
    public void Deserialize_ObjectIssuer_HasAllFields()
    {
        var json = """{"id":"https://example.com","name":"Acme","description":"Acme Corp"}""";
        var issuer = JsonSerializer.Deserialize<Issuer>(json, VcDmSerializerOptions.Default);
        issuer!.Id.Should().Be("https://example.com");
        issuer.Name.Should().Be("Acme");
        issuer.Description.Should().Be("Acme Corp");
        issuer.IsSimpleUrl.Should().BeFalse();
    }

    [Fact]
    public void Deserialize_ObjectIssuerWithExtension_CapturesAdditionalProperties()
    {
        var json = """{"id":"https://example.com","name":"Corp","logo":"https://logo.example.com"}""";
        var issuer = JsonSerializer.Deserialize<Issuer>(json, VcDmSerializerOptions.Default);
        issuer!.AdditionalProperties.Should().ContainKey("logo");
    }

    [Fact]
    public void ImplicitConversion_FromString_CreatesIssuer()
    {
        Issuer issuer = "https://example.com";
        issuer.Id.Should().Be("https://example.com");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_NullOrEmptyId_Throws(string? id)
    {
        var act = () => new Issuer(id!);
        act.Should().Throw<ArgumentException>();
    }
}
