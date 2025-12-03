using SdJwt.Net.Utils;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json;
using Xunit;

namespace SdJwt.Net.Tests;

/// <summary>
/// Comprehensive tests for SdJwtParser to reach 95% coverage
/// </summary>
public class SdParserComprehensiveTests
{
    [Fact]
    public void ParseIssuance_WithMultipleDisclosures_ParsesAll()
    {
        // Arrange
        const string sdJwt = "eyJhbGciOiJIUzI1NiJ9.eyJfc2QiOlsiYSJdfQ.sig";
        const string d1 = "WyJzMSIsImMxIiwidjEiXQ";
        const string d2 = "WyJzMiIsImMyIiwidjIiXQ";
        const string d3 = "WyJzMyIsImMzIiwidjMiXQ";
        var issuance = $"{sdJwt}~{d1}~{d2}~{d3}";

        // Act
        var parsed = SdJwtParser.ParseIssuance(issuance);

        // Assert
        Assert.Equal(3, parsed.Disclosures.Count);
    }

    [Fact]
    public void ParseIssuance_WithTrailingSeparators_HandlesGracefully()
    {
        // Arrange
        const string sdJwt = "eyJhbGciOiJIUzI1NiJ9.eyJfc2QiOlsiYSJdfQ.sig";
        const string disclosure = "WyJzYWx0IiwibmFtZSIsInZhbHVlIl0";
        var issuance = $"{sdJwt}~{disclosure}~~~";

        // Act
        var parsed = SdJwtParser.ParseIssuance(issuance);

        // Assert
        Assert.Single(parsed.Disclosures);
    }

    [Fact]
    public void ParsePresentation_WithMultipleDisclosuresAndKbJwt_ParsesCorrectly()
    {
        // Arrange
        const string sdJwt = "eyJhbGciOiJIUzI1NiJ9.eyJfc2QiOlsiYSJdfQ.sig";
        const string d1 = "WyJzMSIsImMxIiwidjEiXQ";
        const string d2 = "WyJzMiIsImMyIiwidjIiXQ";
        const string kbJwt = "eyJhbGciOiJIUzI1NiIsInR5cCI6ImtiK2p3dCJ9.eyJub25jZSI6IjEyMyJ9.sig";
        var presentation = $"{sdJwt}~{d1}~{d2}~{kbJwt}";

        // Act
        var parsed = SdJwtParser.ParsePresentation(presentation);

        // Assert
        Assert.Equal(2, parsed.Disclosures.Count);
        Assert.NotNull(parsed.RawKeyBindingJwt);
    }

    [Fact]
    public void ParsePresentation_WithOnlyKbJwt_ParsesCorrectly()
    {
        // Arrange
        const string sdJwt = "eyJhbGciOiJIUzI1NiJ9.eyJfc2QiOlsiYSJdfQ.sig";
        const string kbJwt = "eyJhbGciOiJIUzI1NiIsInR5cCI6ImtiK2p3dCJ9.eyJub25jZSI6IjEyMyJ9.sig";
        var presentation = $"{sdJwt}~{kbJwt}";

        // Act
        var parsed = SdJwtParser.ParsePresentation(presentation);

        // Assert
        Assert.Empty(parsed.Disclosures);
        Assert.NotNull(parsed.RawKeyBindingJwt);
    }

    [Fact]
    public void ParseJson_WithNullValue_ReturnsNull()
    {
        // Arrange
        const string json = "null";

        // Act
        var result = SdJwtParser.ParseJson<object>(json);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void ParseJson_WithComplexObject_DeserializesCorrectly()
    {
        // Arrange
        const string json = "{\"user\":{\"name\":\"test\",\"age\":30},\"roles\":[\"admin\",\"user\"]}";

        // Act
        var result = SdJwtParser.ParseJson<Dictionary<string, JsonElement>>(json);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.ContainsKey("user"));
        Assert.True(result.ContainsKey("roles"));
    }

    [Fact]
    public void ParseIssuance_WithVeryLongDisclosure_HandlesCorrectly()
    {
        // Arrange
        var longValue = new string('a', 10000);
        var disclosure = Base64UrlEncoder.Encode($"[\"salt\",\"claim\",\"{longValue}\"]");
        var issuance = $"eyJhbGciOiJIUzI1NiJ9.e30.sig~{disclosure}";

        // Act
        var parsed = SdJwtParser.ParseIssuance(issuance);

        // Assert
        Assert.Single(parsed.Disclosures);
    }

    [Fact]
    public void ParsePresentation_WithEmptyStringBetweenSeparators_HandlesGracefully()
    {
        // Arrange
        const string sdJwt = "eyJhbGciOiJIUzI1NiJ9.eyJfc2QiOlsiYSJdfQ.sig";
        const string disclosure = "WyJzYWx0IiwibmFtZSIsInZhbHVlIl0";
        var presentation = $"{sdJwt}~~{disclosure}~~";

        // Act
        var parsed = SdJwtParser.ParsePresentation(presentation);

        // Assert
        Assert.Single(parsed.Disclosures);
    }

}
